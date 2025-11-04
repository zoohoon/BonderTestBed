using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderOperateViewModelModule
{
    using Autofac;
    using LoaderBase;
    using LoaderCore;
    using LoaderMapView;
    using LoaderParameters;
    using LoaderServiceBase;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Foup;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Input;

    public class LoaderOperateViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Creator & Init
        public LoaderOperateViewModel()
        {

        }
        #endregion

        #region //..IMainScreenViewModel 

        readonly Guid _ViewModelGUID = new Guid("8EC173F8-E897-F025-7292-F3DA3892C366");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
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


        private List<string> _CassetteList = new List<string>();
        public List<string> CassetteList
        {
            get { return _CassetteList; }
            set
            {
                if (value != _CassetteList)
                {
                    _CassetteList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _ArmList = new List<string>();
        public List<string> ArmList
        {
            get { return _ArmList; }
            set
            {
                if (value != _ArmList)
                {
                    _ArmList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _BufferList = new List<string>();
        public List<string> BufferList
        {
            get { return _BufferList; }
            set
            {
                if (value != _BufferList)
                {
                    _BufferList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _FixedTrayList = new List<string>();
        public List<string> FixedTrayList
        {
            get { return _FixedTrayList; }
            set
            {
                if (value != _FixedTrayList)
                {
                    _FixedTrayList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _InspectionTrayList = new List<string>();
        public List<string> InspectionTrayList
        {
            get { return _InspectionTrayList; }
            set
            {
                if (value != _InspectionTrayList)
                {
                    _InspectionTrayList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _SlotList = new List<string>();
        public List<string> SlotList
        {
            get { return _SlotList; }
            set
            {
                if (value != _SlotList)
                {
                    _SlotList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _StageList = new List<string>();
        public List<string> StageList
        {
            get { return _StageList; }
            set
            {
                if (value != _StageList)
                {
                    _StageList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _CardTrayList = new List<string>();
        public List<string> CardTrayList
        {
            get { return _CardTrayList; }
            set
            {
                if (value != _CardTrayList)
                {
                    _CardTrayList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _CardBufferList = new List<string>();
        public List<string> CardBufferList
        {
            get { return _CardBufferList; }
            set
            {
                if (value != _CardBufferList)
                {
                    _CardBufferList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<SubstrateSizeEnum> _WaferSizeList = new List<SubstrateSizeEnum>();
        public List<SubstrateSizeEnum> WaferSizeList
        {
            get { return _WaferSizeList; }
            set
            {
                if (value != _WaferSizeList)
                {
                    _WaferSizeList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<string> _PAList = new List<string>();
        public List<string> PAList
        {
            get { return _PAList; }
            set
            {
                if (value != _PAList)
                {
                    _PAList = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventCodeEnum InitModule()
        {
            _LoaderModule = _Container.Resolve<ILoaderModule>();

            LX = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LX);
            LZM = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LZM);
            LZS = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LZS);
            LZ = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LZM);
            LW = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LW);
            LT = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LB);
            LUU = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LUU);
            LUD = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LUD);
            LCC = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.LCC);
            FC1 = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.FC1);
            FC2 = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.FC2);
            FC3 = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.FC3);
            FC4 = LoaderModule.MotionManager.GetAxis(EnumAxisConstants.FC4);
            LXStep = 1000;
            LZStep = 1000;
            LWStep = 1000;
            LBStep = 1000;
            LUDStep = 1000;
            LUUStep = 1000;
            LCCStep = 1000;
            FC1Step = 1000;
            FC2Step = 1000;
            FC3Step = 1000;

            try
            {
                GPIOs = new ObservableCollection<IOPortDescripter<bool>>();

                var inputs = LoaderModule.IOManager.IOMappings.RemoteInputs;
                var outputs = LoaderModule.IOManager.IOMappings.RemoteOutputs;
                var inputType = inputs.GetType();
                var props = inputType.GetProperties();

                foreach (var item in props)
                {
                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
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


                for (int i = 1; i <= SystemModuleCount.ModuleCnt.FoupCount; i++)
                {
                    CassetteList.Add($"Cassette {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.ArmCount; i++)
                {
                    if (i == 1)
                    {
                        ArmList.Add($"Arm LUD");
                    }
                    else if (i == 2)
                    {
                        ArmList.Add($"Arm LUU");
                    }
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.CardArmCount; i++)
                {
                    ArmList.Add($"Arm LCC");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.BufferCount; i++)
                {
                    BufferList.Add($"Buffer {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.FixedTrayCount; i++)
                {
                    FixedTrayList.Add($"FixedTray {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.INSPCount; i++)
                {
                    InspectionTrayList.Add($"InspTray {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.SlotCount; i++)
                {
                    SlotList.Add($"Slot {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.StageCount; i++)
                {
                    StageList.Add($"Stage {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.CardTrayCount; i++)
                {
                    CardTrayList.Add($"CardTray {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.CardBufferCount; i++)
                {
                    CardBufferList.Add($"CardBuffer {i}");
                }
                for (int i = 1; i <= SystemModuleCount.ModuleCnt.PACount; i++)
                {
                    PAList.Add($"PA {i}");
                }

                foreach (var waferenum in Enum.GetValues(typeof(SubstrateSizeEnum)))
                {
                    if (SubstrateSizeEnum.CUSTOM != (SubstrateSizeEnum)waferenum &&
                        SubstrateSizeEnum.INVALID != (SubstrateSizeEnum)waferenum &&
                        SubstrateSizeEnum.UNDEFINED != (SubstrateSizeEnum)waferenum)
                    {
                        WaferSizeList.Add((SubstrateSizeEnum)waferenum);
                    }
                    
                    if ((SubstrateSizeEnum)waferenum == LoaderModule.GetDefaultWaferSize())
                    {
                        SelectedWaferSizeIndex = (int)LoaderModule.GetDefaultWaferSize() - 1;
                    }
                }

                this.GPIO_INPUT_ARMs = new ObservableCollection<IOPortDescripter<bool>>();
                if (inputs.DIARM1VAC != null)
                {
                    inputs.DIARM1VAC.Alias.Value = "LUD_VAC";
                    this.GPIO_INPUT_ARMs.Add(inputs.DIARM1VAC);
                }
                if (inputs.DIARM2VAC != null)
                {
                    inputs.DIARM2VAC.Alias.Value = "LUU_VAC";
                    this.GPIO_INPUT_ARMs.Add(inputs.DIARM2VAC);
                }
                if (inputs.DICCARMVAC != null)
                {
                    inputs.DICCARMVAC.Alias.Value = "LCC_VAC";
                    this.GPIO_INPUT_ARMs.Add(inputs.DICCARMVAC);
                }

                this.GPIO_INPUT_BUFFERs = new ObservableCollection<IOPortDescripter<bool>>();
                if (inputs.DIBUFVACS != null)
                {
                    for (int i = 0; i < inputs.DIBUFVACS.Count; i++)
                    {
                        inputs.DIBUFVACS[i].Alias.Value = $"BUFFER_VAC_{i + 1}";
                        this.GPIO_INPUT_BUFFERs.Add(inputs.DIBUFVACS[i]);
                    }
                }
                this.GPIO_INPUT_FIXEDs = new ObservableCollection<IOPortDescripter<bool>>();
                if (inputs.DIWaferOnInSPs != null)
                {
                    for (int i = 0; i < inputs.DIWaferOnInSPs.Count; i++)
                    {
                        inputs.DIWaferOnInSPs[i].Alias.Value = $"INSP_EXIST_{i + 1}";

                        this.GPIO_INPUT_FIXEDs.Add(inputs.DIWaferOnInSPs[i]);
                    }
                }
                if (inputs.DI6inchWaferOnInSPs != null)
                {
                    for (int i = 0; i < inputs.DI6inchWaferOnInSPs.Count; i++)
                    {
                        inputs.DI6inchWaferOnInSPs[i].Alias.Value = $"INSP_6INCH_EXIST_{i + 1}";

                        this.GPIO_INPUT_FIXEDs.Add(inputs.DI6inchWaferOnInSPs[i]);
                    }
                }
                if (inputs.DI8inchWaferOnInSPs != null)
                {
                    for (int i = 0; i < inputs.DI8inchWaferOnInSPs.Count; i++)
                    {
                        inputs.DI8inchWaferOnInSPs[i].Alias.Value = $"INSP_8INCH_EXIST_{i + 1}";

                        this.GPIO_INPUT_FIXEDs.Add(inputs.DI8inchWaferOnInSPs[i]);
                    }
                }
                if (inputs.DIOpenInSPs != null)
                {
                    for (int i = 0; i < inputs.DIOpenInSPs.Count; i++)
                    {
                        inputs.DIOpenInSPs[i].Alias.Value = $"INSP_OPEN_{i + 1}";

                        this.GPIO_INPUT_FIXEDs.Add(inputs.DIOpenInSPs[i]);
                    }
                }
                if (inputs.DIFixTrays != null)
                {
                    for (int i = 0; i < inputs.DIFixTrays.Count; i++)
                    {
                        inputs.DIFixTrays[i].Alias.Value = $"FIXED_EXIST_{i + 1}";
                        this.GPIO_INPUT_FIXEDs.Add(inputs.DIFixTrays[i]);
                    }
                }
                if (inputs.DI6inchWaferOnFixTs != null)
                {
                    for (int i = 0; i < inputs.DI6inchWaferOnFixTs.Count; i++)
                    {
                        inputs.DI6inchWaferOnFixTs[i].Alias.Value = $"FIXED_6INCH_EXIST_{i + 1}";
                        this.GPIO_INPUT_FIXEDs.Add(inputs.DI6inchWaferOnFixTs[i]);
                    }
                }
                if (inputs.DI8inchWaferOnFixTs != null)
                {
                    for (int i = 0; i < inputs.DI8inchWaferOnFixTs.Count; i++)
                    {
                        inputs.DI8inchWaferOnFixTs[i].Alias.Value = $"FIXED_8INCH_EXIST_{i + 1}";
                        this.GPIO_INPUT_FIXEDs.Add(inputs.DI8inchWaferOnFixTs[i]);
                    }
                }

                this.GPIO_INPUT_CARDBUFs = new ObservableCollection<IOPortDescripter<bool>>();


                if (inputs.DICardBuffs != null)
                {
                    for (int i = 0; i < inputs.DICardBuffs.Count; i++)
                    {
                        int CardTrayExistIdx = (i - this.LoaderModule.SystemParameter.CardBufferModules.Count);
                        int CardBuffOpenIdx = (i - this.LoaderModule.SystemParameter.CardBufferModules.Count - this.LoaderModule.SystemParameter.CardBufferTrayModules.Count);
                        if (i < this.LoaderModule.SystemParameter.CardBufferModules.Count)
                        {
                            inputs.DICardBuffs[i].Alias.Value = $"C_BUFFER_EXIST_{i + 1}";
                        }
                        else if (CardTrayExistIdx >= 0 && CardTrayExistIdx < this.LoaderModule.SystemParameter.CardBufferTrayModules.Count)
                        {
                            inputs.DICardBuffs[i].Alias.Value = $"C_TRAY_EXIST_{CardTrayExistIdx + 1}";
                        }
                        else if (CardBuffOpenIdx >= 0 && CardBuffOpenIdx < this.LoaderModule.SystemParameter.CardBufferTrayModules.Count)
                        {
                            inputs.DICardBuffs[i].Alias.Value = $"C_TRAY_OPEN_{CardBuffOpenIdx + 1}";
                        }

                        this.GPIO_INPUT_CARDBUFs.Add(inputs.DICardBuffs[i]);
                    }
                }
                this.GPIO_INPUT_ETCs = new ObservableCollection<IOPortDescripter<bool>>();
                if (inputs.DIMainAirs != null)
                {
                    for (int i = 0; i < inputs.DIMainAirs.Count; i++)
                    {
                        string alias = "";
                        if (i == 0)
                        {
                            alias = "MAIN_AIR_STAGE";
                        }
                        else if (i == 1)
                        {
                            alias = "MAIN_VAC_STAGE";
                        }
                        else if (i == 2)
                        {
                            alias = "MAIN_AIR_LOADER";
                        }
                        else if (i == 3)
                        {
                            alias = "MAIN_VAC_LOADER";
                        }
                        inputs.DIMainAirs[i].Alias.Value = alias;
                        this.GPIO_INPUT_ETCs.Add(inputs.DIMainAirs[i]);
                    }
                }
                if (inputs.DIRightDoorClose != null)
                {
                    inputs.DIRightDoorClose.Alias.Value = "R_DOOR_CLOSE";
                    this.GPIO_INPUT_ETCs.Add(inputs.DIRightDoorClose);
                }
                if (inputs.DILeftDoorClose != null)
                {
                    inputs.DILeftDoorClose.Alias.Value = "L_DOOR_CLOSE";
                    this.GPIO_INPUT_ETCs.Add(inputs.DILeftDoorClose);
                }

                this.GPIO_OUTPUT_ARMs = new ObservableCollection<IOPortDescripter<bool>>();
                if (outputs.DOARM1Vac != null)
                {
                    outputs.DOARM1Vac.Alias.Value = "LUD_VAC";
                    this.GPIO_OUTPUT_ARMs.Add(outputs.DOARM1Vac);
                }
                if (outputs.DOARMVac2 != null)
                {
                    outputs.DOARMVac2.Alias.Value = "LUU_VAC";
                    this.GPIO_OUTPUT_ARMs.Add(outputs.DOARMVac2);
                }
                if (outputs.DOCCArmVac != null)
                {
                    outputs.DOCCArmVac.Alias.Value = "LCC_VAC";
                    this.GPIO_OUTPUT_ARMs.Add(outputs.DOCCArmVac);
                }
                if (outputs.DOCCArmVac_Break != null)
                {
                    outputs.DOCCArmVac_Break.Alias.Value = "LCC_VAC_Break";
                    this.GPIO_OUTPUT_ARMs.Add(outputs.DOCCArmVac_Break);
                }
                this.GPIO_OUTPUT_BUFFERs = new ObservableCollection<IOPortDescripter<bool>>();

                if (outputs.DOBuffVacs != null)
                {
                    for (int i = 0; i < outputs.DOBuffVacs.Count; i++)
                    {
                        outputs.DOBuffVacs[i].Alias.Value = $"BUFFER_VAC_{i + 1}";
                        this.GPIO_OUTPUT_BUFFERs.Add(outputs.DOBuffVacs[i]);
                    }
                }

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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        private Autofac.IContainer _Container => this.GetLoaderContainer();
        public void Initialize(Autofac.IContainer container)
        {


        }

        #region //..Property

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

        //  private ILoaderModule loaderModule => _Container.Resolve<ILoaderModule>();
        private ILoaderSupervisor loaderMaster => _Container.Resolve<ILoaderSupervisor>();
        #region //Properties
        public ILoaderServiceCallback LoaderCallback { get; set; }
        public ILoaderService LoaderService { get; set; }
        public IGPLoaderService GPLoaderService { get; set; }
        public ILoaderService DAL { get; set; }
        private IGPLoader GPLoader
        {
            get { return _Container.Resolve<IGPLoader>(); }
        }
        public IViewModelManager ViewModelManager
        {
            get { return _Container.Resolve<IViewModelManager>(); }
        }

        private string _PipeName = "Service Host: Not initialized.";
        public string PipeName
        {
            get { return _PipeName; }
            set
            {
                if (value != _PipeName)
                {
                    _PipeName = value;
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

        #region // GP Loader axes
        #region ==> LZ
        private AxisObject _LZ;
        public AxisObject LZ
        {
            get { return _LZ; }
            set
            {
                if (value != _LZ)
                {
                    _LZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LW
        private ProbeAxisObject _LW;
        public ProbeAxisObject LW
        {
            get { return _LW; }
            set
            {
                if (value != _LW)
                {
                    _LW = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        private ProbeAxisObject _LX;
        public ProbeAxisObject LX
        {
            get { return _LX; }
            set
            {
                if (value != _LX)
                {
                    _LX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _LZM;
        public AxisObject LZM
        {
            get { return _LZM; }
            set
            {
                if (value != _LZM)
                {
                    _LZM = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _LZS;
        public AxisObject LZS
        {
            get { return _LZS; }
            set
            {
                if (value != _LZS)
                {
                    _LZS = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private ProbeAxisObject _LZ;
        //public ProbeAxisObject LZ
        //{
        //    get { return _LZ; }
        //    set
        //    {
        //        if (value != _LZ)
        //        {
        //            _LZ = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private ProbeAxisObject _LW;
        //public ProbeAxisObject LW
        //{
        //    get { return _LW; }
        //    set
        //    {
        //        if (value != _LW)
        //        {
        //            _LW = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private ProbeAxisObject _LT;
        public ProbeAxisObject LT
        {
            get { return _LT; }
            set
            {
                if (value != _LT)
                {
                    _LT = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _LUU;
        public ProbeAxisObject LUU
        {
            get { return _LUU; }
            set
            {
                if (value != _LUU)
                {
                    _LUU = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _LUD;
        public ProbeAxisObject LUD
        {
            get { return _LUD; }
            set
            {
                if (value != _LUD)
                {
                    _LUD = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _LCC;
        public ProbeAxisObject LCC
        {
            get { return _LCC; }
            set
            {
                if (value != _LCC)
                {
                    _LCC = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _FC1;
        public ProbeAxisObject FC1
        {
            get { return _FC1; }
            set
            {
                if (value != _FC1)
                {
                    _FC1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _FC2;
        public ProbeAxisObject FC2
        {
            get { return _FC2; }
            set
            {
                if (value != _FC2)
                {
                    _FC2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _FC3;
        public ProbeAxisObject FC3
        {
            get { return _FC3; }
            set
            {
                if (value != _FC3)
                {
                    _FC3 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _FC4;
        public ProbeAxisObject FC4
        {
            get { return _FC4; }
            set
            {
                if (value != _FC4)
                {
                    _FC4 = value;
                    RaisePropertyChanged();
                }
            }
        }
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
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_INPUT_ARMs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_INPUT_ARMs
        {
            get { return _GPIO_INPUT_ARMs; }
            set
            {
                if (value != _GPIO_INPUT_ARMs)
                {
                    _GPIO_INPUT_ARMs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_OUTPUT_ARMs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_OUTPUT_ARMs
        {
            get { return _GPIO_OUTPUT_ARMs; }
            set
            {
                if (value != _GPIO_OUTPUT_ARMs)
                {
                    _GPIO_OUTPUT_ARMs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_INPUT_BUFFERs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_INPUT_BUFFERs
        {
            get { return _GPIO_INPUT_BUFFERs; }
            set
            {
                if (value != _GPIO_INPUT_BUFFERs)
                {
                    _GPIO_INPUT_BUFFERs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_OUTPUT_BUFFERs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_OUTPUT_BUFFERs
        {
            get { return _GPIO_OUTPUT_BUFFERs; }
            set
            {
                if (value != _GPIO_OUTPUT_BUFFERs)
                {
                    _GPIO_OUTPUT_BUFFERs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _GPIO_INPUT_FIXEDs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_INPUT_FIXEDs
        {
            get { return _GPIO_INPUT_FIXEDs; }
            set
            {
                if (value != _GPIO_INPUT_FIXEDs)
                {
                    _GPIO_INPUT_FIXEDs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_OUTPUT_FIXEDs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_OUTPUT_FIXEDs
        {
            get { return _GPIO_OUTPUT_FIXEDs; }
            set
            {
                if (value != _GPIO_OUTPUT_FIXEDs)
                {
                    _GPIO_OUTPUT_FIXEDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _GPIO_INPUT_CARDBUFs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_INPUT_CARDBUFs
        {
            get { return _GPIO_INPUT_CARDBUFs; }
            set
            {
                if (value != _GPIO_INPUT_CARDBUFs)
                {
                    _GPIO_INPUT_CARDBUFs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_OUTPUT_CARDBUFs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_OUTPUT_CARDBUFs
        {
            get { return _GPIO_OUTPUT_CARDBUFs; }
            set
            {
                if (value != _GPIO_OUTPUT_CARDBUFs)
                {
                    _GPIO_OUTPUT_CARDBUFs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _GPIO_INPUT_ETCs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_INPUT_ETCs
        {
            get { return _GPIO_INPUT_ETCs; }
            set
            {
                if (value != _GPIO_INPUT_ETCs)
                {
                    _GPIO_INPUT_ETCs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _GPIO_OUTPUT_ETCs;
        public ObservableCollection<IOPortDescripter<bool>> GPIO_OUTPUT_ETCs
        {
            get { return _GPIO_OUTPUT_ETCs; }
            set
            {
                if (value != _GPIO_OUTPUT_ETCs)
                {
                    _GPIO_OUTPUT_ETCs = value;
                    RaisePropertyChanged();
                }
            }
        }




        private bool _isPickForced;
        public bool isPickForced
        {
            get { return _isPickForced; }
            set
            {
                if (value != _isPickForced)
                {
                    _isPickForced = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isPutForced;
        public bool isPutForced
        {
            get { return _isPutForced; }
            set
            {
                if (value != _isPutForced)
                {
                    _isPutForced = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _LockIO;
        public bool LockIO
        {
            get { return _LockIO; }
            set
            {
                if (value != _LockIO)
                {
                    _LockIO = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _JogStep;
        public int JogStep
        {
            get { return _JogStep; }
            set
            {
                if (value != _JogStep)
                {
                    _JogStep = value;
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
        #endregion
        private int _SelectedCassetteIndex;
        public int SelectedCassetteIndex
        {
            get { return _SelectedCassetteIndex; }
            set
            {
                if (value != _SelectedCassetteIndex)
                {
                    _SelectedCassetteIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedFixedTrayIndex;
        public int SelectedFixedTrayIndex
        {
            get { return _SelectedFixedTrayIndex; }
            set
            {
                if (value != _SelectedFixedTrayIndex)
                {
                    _SelectedFixedTrayIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedInspectionTrayIndex;
        public int SelectedInspectionTrayIndex
        {
            get { return _SelectedInspectionTrayIndex; }
            set
            {
                if (value != _SelectedInspectionTrayIndex)
                {
                    _SelectedInspectionTrayIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedCardTrayIndex;
        public int SelectedCardTrayIndex
        {
            get { return _SelectedCardTrayIndex; }
            set
            {
                if (value != _SelectedCardTrayIndex)
                {
                    _SelectedCardTrayIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedCardBufferIndex;
        public int SelectedCardBufferIndex
        {
            get { return _SelectedCardBufferIndex; }
            set
            {
                if (value != _SelectedCardBufferIndex)
                {
                    _SelectedCardBufferIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedArmIndex;
        public int SelectedArmIndex
        {
            get { return _SelectedArmIndex; }
            set
            {
                if (value != _SelectedArmIndex)
                {
                    _SelectedArmIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedBufferIndex;
        public int SelectedBufferIndex
        {
            get { return _SelectedBufferIndex; }
            set
            {
                if (value != _SelectedBufferIndex)
                {
                    _SelectedBufferIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedSlotIndex;
        public int SelectedSlotIndex
        {
            get { return _SelectedSlotIndex; }
            set
            {
                if (value != _SelectedSlotIndex)
                {
                    _SelectedSlotIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedChuckIndex;
        public int SelectedChuckIndex
        {
            get { return _SelectedChuckIndex; }
            set
            {
                if (value != _SelectedChuckIndex)
                {
                    _SelectedChuckIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedWaferSizeIndex;
        public int SelectedWaferSizeIndex
        {
            get { return _SelectedWaferSizeIndex; }
            set
            {
                if (value != _SelectedWaferSizeIndex)
                {
                    _SelectedWaferSizeIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SelectedPAIndex;
        public int SelectedPAIndex
        {
            get { return _SelectedPAIndex; }
            set
            {
                if (value != _SelectedPAIndex)
                {
                    _SelectedPAIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region // Jog steps


        private double _LXStep;
        public double LXStep
        {
            get { return _LXStep; }
            set
            {
                if (value != _LXStep)
                {
                    _LXStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _LZStep;
        public double LZStep
        {
            get { return _LZStep; }
            set
            {
                if (value != _LZStep)
                {
                    _LZStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _LBStep;
        public double LBStep
        {
            get { return _LBStep; }
            set
            {
                if (value != _LBStep)
                {
                    _LBStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _LWStep;
        public double LWStep
        {
            get { return _LWStep; }
            set
            {
                if (value != _LWStep)
                {
                    _LWStep = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _LUDStep;
        public double LUDStep
        {
            get { return _LUDStep; }
            set
            {
                if (value != _LUDStep)
                {
                    _LUDStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _LUUStep;
        public double LUUStep
        {
            get { return _LUUStep; }
            set
            {
                if (value != _LUUStep)
                {
                    _LUUStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _LCCStep;
        public double LCCStep
        {
            get { return _LCCStep; }
            set
            {
                if (value != _LCCStep)
                {
                    _LCCStep = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FC1Step;
        public double FC1Step
        {
            get { return _FC1Step; }
            set
            {
                if (value != _FC1Step)
                {
                    _FC1Step = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FC2Step;
        public double FC2Step
        {
            get { return _FC2Step; }
            set
            {
                if (value != _FC2Step)
                {
                    _FC2Step = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FC3Step;
        public double FC3Step
        {
            get { return _FC3Step; }
            set
            {
                if (value != _FC3Step)
                {
                    _FC3Step = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ObservableCollection<ModuleTypeEnum> _MoveSteps = new ObservableCollection<ModuleTypeEnum>();
        public ObservableCollection<ModuleTypeEnum> MoveSteps
        {
            get { return _MoveSteps; }
            set
            {
                if (value != _MoveSteps)
                {
                    _MoveSteps = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion
        #endregion

        #region //..Command & Command Mehtod
        #region Robot commands
        private AsyncCommand _CstPickCommand;
        public ICommand CstPickCommand
        {
            get
            {
                if (null == _CstPickCommand) _CstPickCommand = new AsyncCommand(CstPickMethod);
                return _CstPickCommand;
            }
        }
        private AsyncCommand _CSTPutCommand;
        public ICommand CSTPutCommand
        {
            get
            {
                if (null == _CSTPutCommand) _CSTPutCommand = new AsyncCommand(CSTPutMethod);
                return _CSTPutCommand;
            }
        }

        private async Task CSTPutMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Cassette Put", $"Do you want to Cassette{SelectedCassetteIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Cassette Put Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var slots = this.LoaderModule.ModuleManager.FindSlots(modules.FindModule<ICassetteModule>(ModuleTypeEnum.CST, SelectedCassetteIndex + 1));
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(arm.Holder);
                    if (result == EventCodeEnum.NONE) 
                    {
                        var slot = slots.Find(s => s.ID.Index == (SelectedCassetteIndex * slots.Count) + SelectedSlotIndex + 1);
                        this.LoaderModule.GetLoaderCommands().CassettePut(arm, slot);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CSTPutMethod(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;

        }

        private AsyncCommand _PAPickCommand;
        public ICommand PAPickCommand
        {
            get
            {
                if (null == _PAPickCommand) _PAPickCommand = new AsyncCommand(PAPickMethod);
                return _PAPickCommand;
            }
        }

        private async Task PAPickMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("PA Pick", $"Do you want to PA{SelectedPAIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"PA Pick Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var pa = this.LoaderModule.ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, SelectedPAIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(pa.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        bool alignmentActive = true;
                        if (pa.Holder.TransferObject != null)
                        {
                            if (pa.Holder.TransferObject.PreAlignState == PreAlignStateEnum.SKIP &&
                                pa.Holder.Status == EnumSubsStatus.UNKNOWN)
                            {
                                alignmentActive = false;
                                LoggerManager.Error($"PAPickMethod(): The alignment was skipped because the state of PA is {pa.Holder.Status}. ");
                            }
                        }

                        if (isPickForced || alignmentActive == false)
                        {
                            this.LoaderModule.GetLoaderCommands().PAForcedPick(pa, arm);
                        }
                        else
                        {
                            this.LoaderModule.GetLoaderCommands().PAPick(pa, arm);

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

        private AsyncCommand _PAPutCommand;
        public ICommand PAPutCommand
        {
            get
            {
                if (null == _PAPutCommand) _PAPutCommand = new AsyncCommand(PAPutMethod);
                return _PAPutCommand;
            }
        }

        private async Task PAPutMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("PA Put", $"Do you want to PA{SelectedPAIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"PA Put Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var pa = this.LoaderModule.ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, SelectedPAIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(arm.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        if (isPutForced)
                        {
                            this.LoaderModule.GetLoaderCommands().PAPut_NotVac(arm, pa);
                        }
                        else
                        {
                            this.LoaderModule.GetLoaderCommands().PAPut(arm, pa);
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"PAPutMethod(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;
        }

        private AsyncCommand _BufferPickCommand;
        public ICommand BufferPickCommand
        {
            get
            {
                if (null == _BufferPickCommand) _BufferPickCommand = new AsyncCommand(BufferPickMethod);
                return _BufferPickCommand;
            }
        }

        private async Task BufferPickMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Buffer Pick", $"Do you want to Buffer{SelectedBufferIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Buffer Pick Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;

                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var buffer = this.LoaderModule.ModuleManager.FindModule<IBufferModule>(ModuleTypeEnum.BUFFER, SelectedBufferIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(buffer.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().BufferPick(buffer, arm);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"BufferPickMethod(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;
        }

        private AsyncCommand _BUfferPutCommand;
        public ICommand BUfferPutCommand
        {
            get
            {
                if (null == _BUfferPutCommand) _BUfferPutCommand = new AsyncCommand(BUfferPutMethod);
                return _BUfferPutCommand;
            }
        }

        private async Task BUfferPutMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Buffer Put", $"Do you want to Buffer{SelectedBufferIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Buffer Put Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var buffer = this.LoaderModule.ModuleManager.FindModule<IBufferModule>(ModuleTypeEnum.BUFFER, SelectedBufferIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(arm.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().BufferPut(arm, buffer);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"BUfferPutMethod(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;
        }

        private AsyncCommand _StagePickCommand;
        public ICommand StagePickCommand
        {
            get
            {
                if (null == _StagePickCommand) _StagePickCommand = new AsyncCommand(StagePickMethod);
                return _StagePickCommand;
            }
        }

        private async Task StagePickMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Stage Pick", $"Do you want to Stage{SelectedChuckIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Stage Pick Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var chuck = this.LoaderModule.ModuleManager.FindModule<IChuckModule>(ModuleTypeEnum.CHUCK, SelectedChuckIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(chuck.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().ChuckPick(chuck, arm);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"StagePickMethod(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;
        }

        private AsyncCommand _StagePutCommand;
        public ICommand StagePutCommand
        {
            get
            {
                if (null == _StagePutCommand) _StagePutCommand = new AsyncCommand(StagePutMethod);
                return _StagePutCommand;
            }
        }

        private async Task StagePutMethod()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Stage Put", $"Do you want to Stage{SelectedChuckIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Stage Put Message Cancel");
                return;
            }

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var chuck = this.LoaderModule.ModuleManager.FindModule<IChuckModule>(ModuleTypeEnum.CHUCK, SelectedChuckIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(arm.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().ChuckPut(arm, chuck);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"StagePutMethod(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;
        }

        private AsyncCommand _INSPPickCommand;
        public ICommand INSPPickCommand
        {
            get
            {
                if (null == _INSPPickCommand) _INSPPickCommand = new AsyncCommand(INSPPickFunc);
                return _INSPPickCommand;
            }
        }
        private AsyncCommand _INSPPutCommand;
        public ICommand INSPPutCommand
        {
            get
            {
                if (null == _INSPPutCommand) _INSPPutCommand = new AsyncCommand(INSPPutFunc);
                return _INSPPutCommand;
            }
        }
        private async Task INSPPickFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("InspectionTray Pick", $"Do you want to InspectionTray{SelectedInspectionTrayIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"InspectionTray Pick Message Cancel");
                return;
            }

            var loader = LoaderModule;
            await Task.Run(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var INSPTray = this.LoaderModule.ModuleManager.FindModule<IInspectionTrayModule>(ModuleTypeEnum.INSPECTIONTRAY, SelectedInspectionTrayIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(INSPTray.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().DRWPick(INSPTray, arm);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"INSPPickFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private async Task INSPPutFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("InspectionTray Put", $"Do you want to InspectionTray{SelectedInspectionTrayIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"InspectionTray Put Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;
            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var INSPTray = this.LoaderModule.ModuleManager.FindModule<IInspectionTrayModule>(ModuleTypeEnum.INSPECTIONTRAY, SelectedInspectionTrayIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(arm.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().DRWPut(arm, INSPTray);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"INSPutFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }



        private RelayCommand _AxisEnableCommand;
        public ICommand AxisEnableCommand
        {
            get
            {
                if (null == _AxisEnableCommand) _AxisEnableCommand = new RelayCommand(AxisEnableMethod);
                return _AxisEnableCommand;
            }
        }

        private void AxisEnableMethod()
        {
            if (GPLoader.AxisEnabled == true)
            {
                this.LoaderModule.GetLoaderCommands().DisableAxis();
            }
            else
            {
                this.LoaderModule.GetLoaderCommands().EnableAxis();
            }


        }
        private RelayCommand _AxisDisableCommand;
        public ICommand AxisDisableCommand
        {
            get
            {
                if (null == _AxisDisableCommand) _AxisDisableCommand = new RelayCommand(AxisDisableMethod);
                return _AxisDisableCommand;
            }
        }

        private void AxisDisableMethod()
        {
            this.LoaderModule.GetLoaderCommands().DisableAxis();
        }

        #endregion

        #region Motion commands
        private AsyncCommand<EnumAxisConstants> _JogStepPosCommand;
        public ICommand JogStepPosCommand
        {
            get
            {
                if (null == _JogStepPosCommand) _JogStepPosCommand = new AsyncCommand<EnumAxisConstants>(JogStepPosMethod);
                return _JogStepPosCommand;
            }
        }

        private async Task JogStepPosMethod(EnumAxisConstants axis)
        {
            try
            {
                double dist = 0;
                switch (axis)
                {
                    case EnumAxisConstants.LX:
                        dist = LXStep;
                        break;
                    case EnumAxisConstants.LZM:
                        dist = LZStep;
                        break;
                    case EnumAxisConstants.LZS:
                        dist = LZStep;
                        break;
                    case EnumAxisConstants.LW:
                        dist = LWStep;
                        break;
                    case EnumAxisConstants.LB:
                        dist = LBStep;
                        break;
                    case EnumAxisConstants.LUD:
                        dist = LUDStep;
                        break;
                    case EnumAxisConstants.LUU:
                        dist = LUUStep;
                        break;
                    case EnumAxisConstants.LCC:
                        dist = LCCStep;
                        break;
                    case EnumAxisConstants.FC1:
                        dist = FC1Step;
                        break;
                    case EnumAxisConstants.FC2:
                        dist = FC1Step;
                        break;
                    case EnumAxisConstants.FC3:
                        dist = FC1Step;
                        break;
                    case EnumAxisConstants.FC4:
                        dist = FC1Step;
                        break;
                    default:
                        break;
                }


                Task task = new Task(() =>
                {
                    GPLoader.JogMove(LoaderModule.MotionManager.GetAxis(axis), dist);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<EnumAxisConstants> _JogStepNegCommand;
        public ICommand JogStepNegCommand
        {
            get
            {
                if (null == _JogStepNegCommand) _JogStepNegCommand = new AsyncCommand<EnumAxisConstants>(JogStepNegMethod);
                return _JogStepNegCommand;
            }
        }

        private async Task JogStepNegMethod(EnumAxisConstants axis)
        {
            try
            {
                double dist = 0; ;
                switch (axis)
                {
                    case EnumAxisConstants.LX:
                        dist = LXStep;
                        break;
                    case EnumAxisConstants.LZM:
                        dist = LZStep;
                        break;
                    case EnumAxisConstants.LZS:
                        dist = LZStep;
                        break;
                    case EnumAxisConstants.LW:
                        dist = LWStep;
                        break;
                    case EnumAxisConstants.LB:
                        dist = LBStep;
                        break;
                    case EnumAxisConstants.LUD:
                        dist = LUDStep;
                        break;
                    case EnumAxisConstants.LUU:
                        dist = LUUStep;
                        break;
                    case EnumAxisConstants.LCC:
                        dist = LCCStep;
                        break;
                    case EnumAxisConstants.FC1:
                        dist = FC1Step;
                        break;
                    case EnumAxisConstants.FC2:
                        dist = FC1Step;
                        break;
                    case EnumAxisConstants.FC3:
                        dist = FC1Step;
                        break;
                    case EnumAxisConstants.FC4:
                        dist = FC1Step;
                        break;
                    default:
                        break;
                }

                Task task = new Task(() =>
                {
                    GPLoader.JogMove(LoaderModule.MotionManager.GetAxis(axis), dist * -1d);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _HomingSysCommand;
        public ICommand HomingSysCommand
        {
            get
            {
                if (null == _HomingSysCommand) _HomingSysCommand = new AsyncCommand(HomingSysMethod);
                return _HomingSysCommand;
            }
        }

        private async Task HomingSysMethod()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Task task = new Task(() =>
                {
                    ret = ((IGPLoaderCommands)GPLoader).HomingRobot();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"HomingSysMethod(): Retry Init System & Homing Start");
                        EventCodeEnum updateloadersystem_result = LoaderModule.LoaderService.UpdateLoaderSystem(-1);
                        EventCodeEnum initrobot_result = ((IGPLoaderCommands)GPLoader).InitRobot();

                        if (updateloadersystem_result == EventCodeEnum.NONE && initrobot_result == EventCodeEnum.NONE)
                        {
                            ret = ((IGPLoaderCommands)GPLoader).HomingRobot();
                        }
                        else
                        {
                            ret = EventCodeEnum.MOTION_LOADER_INIT_ERROR;
                            LoggerManager.Debug($"HomingSysMethod(): Retry Homing , UpdateLoaderSystem or InitRobot failrue.");
                        }
                    }

                    if (ret == EventCodeEnum.NONE)
                    {
                        this.GEMModule().ClearAlarmOnly();
                    }

                    ((IGPLoaderCommands)GPLoader).HomingResultAlarm(ret);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                ret = EventCodeEnum.EXCEPTION;
            }
        }

        private AsyncCommand _InitSysCommand;
        public ICommand InitSysCommand
        {
            get
            {
                if (null == _InitSysCommand) _InitSysCommand = new AsyncCommand(InitSysMethod);
                return _InitSysCommand;
            }
        }

        private async Task InitSysMethod()
        {
            try
            {
                Task task = new Task(() =>
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                    {
                        LoaderModule.LoaderService.UpdateLoaderSystem(i + 1);
                        LoaderModule.LoaderService.UpdateCassetteSystem(LoaderModule.DeviceSize, i + 1);
                    }
                    ((IGPLoaderCommands)GPLoader).InitRobot();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"InitSysMethod(): Error occurred. Err = {err.Message}");
            }

        }
        #endregion

        bool stopFlag = false;
        private AsyncCommand _StopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (null == _StopCommand) _StopCommand = new AsyncCommand(StopMethod);
                return _StopCommand;
            }
        }

        private async Task StopMethod()
        {
            stopFlag = true;
        }

        private AsyncCommand _TransferCommand;
        public ICommand TransferCommand
        {
            get
            {
                if (null == _TransferCommand) _TransferCommand = new AsyncCommand(TransferMethod);
                return _TransferCommand;
            }
        }

        private async Task TransferMethod()
        {
            try
            {
                FOUPTestRun();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task FOUPTestRun()
        {
            await Task.Run(() =>
            {
                stopFlag = false;
                try
                {
                    int iterCount = 0;
                    while (stopFlag == false)
                    {

                        var modules = this.LoaderModule.ModuleManager;
                        var Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, SelectedCassetteIndex + 1);
                        try
                        {
                            for (int index = 0; index < LoaderModule.FoupScanFlag.Count; index++)
                            {
                                if (LoaderModule.FoupScanFlag[index].ScanFlag == false)
                                {
                                    stopFlag = true;
                                    break;
                                }

                                Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, index + 1);
                                if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].FoupModuleInfo.State != FoupStateEnum.ERROR
                                    & LoaderModule.FoupScanFlag[index].ScanFlag == true)
                                {
                                    this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                                }

                                //Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 2);
                                //if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].FoupModuleInfo.State != FoupStateEnum.ERROR
                                //    & LoaderModule.ScanFlag[1] == true)
                                //{
                                //    this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                                //}
                                //Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 3);
                                //if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].FoupModuleInfo.State != FoupStateEnum.ERROR
                                //    & LoaderModule.ScanFlag[2] == true)
                                //{
                                //    this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                                //}

                                if (this.LoaderModule.FoupScanFlag[index].ScanFlag)
                                {
                                    this.LoaderModule.DoScanJob(index + 1);
                                }
                                //if (this.LoaderModule.ScanFlag[1])
                                //{
                                //    this.LoaderModule.DoScanJob(2);
                                //}
                                //if (this.LoaderModule.ScanFlag[2])
                                //{
                                //    this.LoaderModule.DoScanJob(3);
                                //}

                                var dstCstInfo = this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == index + 1).FirstOrDefault();
                                while (dstCstInfo.ScanState != CassetteScanStateEnum.READ & this.LoaderModule.FoupScanFlag[index].ScanFlag == true)
                                {
                                    dstCstInfo = this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == index + 1).FirstOrDefault();
                                }
                                //dstCstInfo = this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == 2).FirstOrDefault();
                                //while (dstCstInfo.ScanState != CassetteScanStateEnum.READ & this.LoaderModule.ScanFlag[1] == true)
                                //{
                                //    dstCstInfo = this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == 2).FirstOrDefault();
                                //}
                                //dstCstInfo = this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == 3).FirstOrDefault();
                                //while (dstCstInfo.ScanState != CassetteScanStateEnum.READ & this.LoaderModule.ScanFlag[2] == true)
                                //{
                                //    dstCstInfo = this.LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Where(i => i.ID.Index == 3).FirstOrDefault();
                                //}

                                Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, index + 1);
                                if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].FoupModuleInfo.State != FoupStateEnum.ERROR
                                    & LoaderModule.FoupScanFlag[index].ScanFlag == true)
                                {
                                    this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                                }
                                //Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 2);
                                //if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].FoupModuleInfo.State != FoupStateEnum.ERROR
                                //    & LoaderModule.ScanFlag[1] == true)
                                //{
                                //    this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                                //}
                                //Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, 3);
                                //if (this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].FoupModuleInfo.State != FoupStateEnum.ERROR
                                //    & LoaderModule.ScanFlag[2] == true)
                                //{
                                //    this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                                //}
                                iterCount++;
                                LoggerManager.Debug($"FOUPTestRun(): {iterCount} iterations processed.");
                            }

                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"FOUPTestRun(): Exception occurred. Err = {err.Message}");
                        }
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Error($"FOUPTestRun(): Exception occurred. Err = {err.Message}");
                }
            });
        }
        private bool _IsFoupRepeat;
        public bool IsFoupRepeat
        {
            get { return _IsFoupRepeat; }
            set
            {
                if (value != _IsFoupRepeat)
                {
                    _IsFoupRepeat = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand _FoupRepeatTestCommand;
        public ICommand FoupRepeatTestCommand
        {
            get
            {
                if (null == _FoupRepeatTestCommand) _FoupRepeatTestCommand = new AsyncCommand(FoupRepeatTestFunc);
                return _FoupRepeatTestCommand;
            }
        }

        private Task FoupRepeatTestFunc()
        {
            return Task.Run(() =>
            {
                var loader = LoaderModule;

                int cnt = 0;
                try
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    var modules = this.LoaderModule.ModuleManager;
                    var Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, SelectedCassetteIndex + 1);

                    while (IsFoupRepeat)
                    {
                        cnt++;
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        if (SelectedCassetteIndex >= 0 & SelectedCassetteIndex < this.FoupOpModule().FoupControllers.Count)
                        {
                            retVal = this.FoupOpModule().FoupControllers[SelectedCassetteIndex].Execute(new FoupLoadCommand());
                        }
                        if (retVal != EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("[Foup Test Error (Load)]", $"Count{cnt}, Time:" + DateTime.Now, EnumMessageStyle.Affirmative);
                            IsFoupRepeat = false;
                            return;
                        }
                        //de.DelayFor(3000);
                        Thread.Sleep(3000);
                        if (SelectedCassetteIndex >= 0 & SelectedCassetteIndex < this.FoupOpModule().FoupControllers.Count)
                        {
                            bool scanWaitFlag = false;
                            var ret = this.LoaderModule.DoScanJob(SelectedCassetteIndex + 1);
                            if (ret.Result != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[Repeat] CassettePut(): Exception occurred. Err = {retVal}");
                                break;
                            }
                            Thread.Sleep(1000);

                            /////////////////////////////////////////////////////////////////////////////////////////////
                            //remove when merge
                            //retVal = this.LoaderModule.GetLoaderCommands().CassettePick(targetSlot2, arm);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[Repeat] CassettePick(): Exception occurred. Err = {retVal}");
                                break;
                            }
                            Thread.Sleep(1000);
                            //remove when merge
                            //retVal = this.LoaderModule.GetLoaderCommands().PAPut(arm, pa);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("[Foup Test Error (SCAN)]", $"Count{cnt}, Time:" + DateTime.Now, EnumMessageStyle.Affirmative);
                                    IsFoupRepeat = false;
                                    return;
                                }
                                else if (Cassette.ScanState == CassetteScanStateEnum.READ)
                                {
                                    break;
                                }
                                //de.DelayFor(10);
                                Thread.Sleep(10);
                            }
                        }
                        //de.DelayFor(3000);
                        Thread.Sleep(3000);
                        if (SelectedCassetteIndex >= 0 & SelectedCassetteIndex < this.FoupOpModule().FoupControllers.Count)
                        {
                            retVal = this.FoupOpModule().FoupControllers[SelectedCassetteIndex].Execute(new FoupUnloadCommand());
                        }
                        if (retVal != EventCodeEnum.NONE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("[Foup Test Error (UnLoad)]", $"Count{cnt}, Time:" + DateTime.Now, EnumMessageStyle.Affirmative);
                            IsFoupRepeat = false;
                            return;
                        }
                        //de.DelayFor(3000);
                        Thread.Sleep(3000);

                    }
                    //var isUnprocessedWafer = loader.GetLoaderInfo().StateMap.CassetteModules[0].SlotModules.FirstOrDefault(i => i.Substrate.WaferState == EnumWaferState.UNPROCESSED);
                    //if (isUnprocessedWafer==null)
                    //{
                    //    break;
                    //}
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"FoupRepeatTestFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }


        private AsyncCommand _CTLockCommand;
        public ICommand CTLockCommand
        {
            get
            {
                if (null == _CTLockCommand) _CTLockCommand = new AsyncCommand(CTLockunc);
                return _CTLockCommand;
            }
        }

        private Task CTLockunc()
        {
            return Task.Run(() =>
            {
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardTrayLock(true);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CTLockunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private AsyncCommand _CTUnlockCommand;
        public ICommand CTUnlockCommand
        {
            get
            {
                if (null == _CTUnlockCommand) _CTUnlockCommand = new AsyncCommand(CTUnlockFunc);
                return _CTUnlockCommand;
            }
        }

        private Task CTUnlockFunc()
        {
            return Task.Run(() =>
            {
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardTrayLock(false);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CTUnlockFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private AsyncCommand _WaferToPACommand;
        public ICommand WaferToPACommand
        {
            get
            {
                if (null == _WaferToPACommand) _WaferToPACommand = new AsyncCommand(WaferToPATestFunc);
                return _WaferToPACommand;
            }
        }

        private Task WaferToPATestFunc()
        {
            return Task.Run(() =>
            {
                var loader = LoaderModule;

                int cnt = 0;

                try
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    var modules = this.LoaderModule.ModuleManager;
                    var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                    var pa = this.LoaderModule.ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, SelectedCassetteIndex + 1);

                    try
                    {
                        while (IsFoupRepeat)
                        {
                            this.LoaderModule.GetLoaderCommands().PAPut_NotVac(arm, pa);
                            Thread.Sleep(5000);
                            this.LoaderModule.GetLoaderCommands().PAPick_NotVac(pa, arm);
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }

                    //var isUnprocessedWafer = loader.GetLoaderInfo().StateMap.CassetteModules[0].SlotModules.FirstOrDefault(i => i.Substrate.WaferState == EnumWaferState.UNPROCESSED);
                    //if (isUnprocessedWafer==null)
                    //{
                    //    break;
                    //}
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"WaferToPATestFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private async Task CstPickMethod()
        {

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Cassette Pick", $"Do you want to Cassette{SelectedCassetteIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Cassette Pick Message Cancel");
                return;
            }

            await Task.Run(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var slot = this.LoaderModule.ModuleManager.FindSlots(modules.FindModule<ICassetteModule>(ModuleTypeEnum.CST, SelectedCassetteIndex + 1));
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                try
                {
                    var targetSlot = slot.Find(s => s.ID.Index == SelectedSlotIndex + 1 + SelectedCassetteIndex * slot.Count);
                    EventCodeEnum result = this.SetWaferSize(targetSlot.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().CassettePick(targetSlot, arm);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CstPickMethod(): Exception occurred. Err = {err.Message}");
                }
            });
        }


        private AsyncCommand _ChuckPickCommand;
        public ICommand ChuckPickCommand
        {
            get
            {
                if (null == _ChuckPickCommand) _ChuckPickCommand = new AsyncCommand(ChuckPickMethod);
                return _ChuckPickCommand;
            }
        }

        private async Task ChuckPickMethod()
        {
            try
            {
                TransferObject subs = new TransferObject();
                TransferObject dest = new TransferObject();
                subs.CurrHolder = new ModuleID(ModuleTypeEnum.CHUCK, 0, "SLOT0@CST0");
                subs.PreAlignState = PreAlignStateEnum.DONE;
                subs.OCRReadState = OCRReadStateEnum.DONE;
                subs.WaferState = EnumWaferState.PROCESSED;
                dest.CurrHolder = new ModuleID(ModuleTypeEnum.SLOT, 0, "CHUCK0@STAGE0");

                List<ModuleTypeEnum> resultPaths = new List<ModuleTypeEnum>();
                ModulePathGenerator pathGen = new ModulePathGenerator(subs);
                await Task.Run(() =>
                {
                    resultPaths = pathGen.GetPath(dest);

                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MoveSteps.Clear();
                        foreach (var path in resultPaths)
                        {
                            MoveSteps.Add(path);
                        }
                    }));
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _WaferLoadCommand;
        public ICommand WaferLoadCommand
        {
            get
            {
                if (null == _WaferLoadCommand) _WaferLoadCommand = new AsyncCommand(WaferLoadFunc);
                return _WaferLoadCommand;
            }
        }

        public void SetTransfer(string id, ModuleID destinationID)
        {
            try
            {
                TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
                ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == destinationID).FirstOrDefault();
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

                if (dstLoc is HolderModuleInfo)
                {
                    var currHolder = Map.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                    var dstHolder = dstLoc as HolderModuleInfo;

                    subObj.PrevHolder = subObj.CurrHolder;
                    subObj.PrevPos = subObj.CurrPos;

                    subObj.CurrHolder = destinationID;
                    subObj.CurrPos = destinationID;


                    currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                    currHolder.Substrate = null;

                    dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                    dstHolder.Substrate = subObj;
                    TransferObject currSubObj = LoaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
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
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        LoaderMap Map;
        private async Task WaferLoadFunc()
        {
            Task task = new Task(() =>
            {
                var loader = LoaderModule;
                var modules = this.LoaderModule.ModuleManager;
                LoaderMapViewModel vm = LoaderMapViewModel.Instance;

                var slot = this.LoaderModule.ModuleManager.FindModule<ISlotModule>(ModuleTypeEnum.SLOT, SelectedSlotIndex + 1);
                var mapSlicer = new LoaderMapSlicer();
                Map = loader.GetLoaderInfo().StateMap;
                try
                {
                    var waferIdx = SelectedCassetteIndex * 25 + (SelectedSlotIndex + 1);
                    var chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == SelectedChuckIndex + 1);
                    var slotModule = Map.CassetteModules[SelectedCassetteIndex].SlotModules.FirstOrDefault(i => i.ID.Index == waferIdx);


                    SetTransfer(slotModule.Substrate.ID.Value, chuckModule.ID);

                    var slicedMap = mapSlicer.Slicing(Map);

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        LoaderModule.SetRequest(slicedMap[i]);
                        while (true)
                        {
                            if (loader.ModuleState == ModuleStateEnum.DONE)
                            {
                                loader.ClearRequestData();

                                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    vm.LoaderMapConvert(loader.GetLoaderInfo().StateMap);
                                }));
                                break;
                            }
                            Thread.Sleep(10);
                        }


                        Thread.Sleep(33);
                    }
                    Thread.Sleep(33);
                    //var isUnprocessedWafer = loader.GetLoaderInfo().StateMap.CassetteModules[0].SlotModules.FirstOrDefault(i => i.Substrate.WaferState == EnumWaferState.UNPROCESSED);
                    //if (isUnprocessedWafer==null)
                    //{
                    //    break;
                    //}
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"WaferLoadFunc(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;

        }

        private AsyncCommand _WaferUnloadCommand;
        public ICommand WaferUnloadCommand
        {
            get
            {
                if (null == _WaferUnloadCommand) _WaferUnloadCommand = new AsyncCommand(WaferUnloadFunc);
                return _WaferUnloadCommand;
            }
        }
        private async Task WaferUnloadFunc()
        {
            Task task = new Task(() =>
            {
                var loader = LoaderModule;
                var modules = this.LoaderModule.ModuleManager;
                LoaderMapViewModel vm = LoaderMapViewModel.Instance;

                var mapSlicer = new LoaderMapSlicer();
                Map = loader.GetLoaderInfo().StateMap;
                try
                {
                    var waferIdx = SelectedCassetteIndex * 25 + (SelectedSlotIndex + 1);
                    var chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == SelectedChuckIndex + 1);
                    var slotModule = Map.CassetteModules[SelectedCassetteIndex].SlotModules.FirstOrDefault(i => i.ID.Index == waferIdx && i.Substrate == null);

                    SetTransfer(chuckModule.Substrate.ID.Value, slotModule.ID);

                    var slicedMap = mapSlicer.Slicing(Map);

                    for (int i = 0; i < slicedMap.Count; i++)
                    {
                        LoaderModule.SetRequest(slicedMap[i]);
                        while (true)
                        {
                            if (loader.ModuleState == ModuleStateEnum.DONE)
                            {
                                loader.ClearRequestData();

                                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    vm.LoaderMapConvert(loader.GetLoaderInfo().StateMap);
                                }));
                                break;
                            }
                            Thread.Sleep(10);
                        }


                        Thread.Sleep(33);
                    }
                    Thread.Sleep(33);
                    //var isUnprocessedWafer = loader.GetLoaderInfo().StateMap.CassetteModules[0].SlotModules.FirstOrDefault(i => i.Substrate.WaferState == EnumWaferState.UNPROCESSED);
                    //if (isUnprocessedWafer==null)
                    //{
                    //    break;
                    //}
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"WaferUnloadFunc(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;

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
                var resultOp = await this.MetroDialogManager().ShowMessageDialog("Cassette Load", $"Do you want to CST{SelectedCassetteIndex + 1} Load?", EnumMessageStyle.AffirmativeAndNegative);
                if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
                {
                    LoggerManager.Debug($"Cassette Load Message Cancel");
                    return;
                }

                this.FoupOpModule().FoupControllers[SelectedCassetteIndex].Execute(new FoupLoadCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CSTLoadFunc(): Exception occurred. Err = {err.Message}");
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
        private async Task CSTUnloadFunc()
        {
            try
            {
                var resultOp = await this.MetroDialogManager().ShowMessageDialog("Cassette UnLoad", $"Do you want to CST{SelectedCassetteIndex + 1} UnLoad?", EnumMessageStyle.AffirmativeAndNegative);
                if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
                {
                    LoggerManager.Debug($"Cassette UnLoad Message Cancel");
                    return;
                }

                this.FoupOpModule().FoupControllers[SelectedCassetteIndex].Execute(new FoupUnloadCommand());
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CSTUnloadFunc(): Exception occurred. Err = {err.Message}");
            }
        }

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
                var resultOp = await this.MetroDialogManager().ShowMessageDialog("Cassette Scan", $"Do you want to CST{SelectedCassetteIndex + 1} Scan?", EnumMessageStyle.AffirmativeAndNegative);
                if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
                {
                    LoggerManager.Debug($"Cassette Scan Message Cancel");
                    return;
                }

                this.LoaderModule.DoScanJob(SelectedCassetteIndex + 1);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CSTScanFunc(): Exception occurred. Err = {err.Message}");
            }
        }

        private AsyncCommand _FixedTrayPickCommand;
        public ICommand FixedTrayPickCommand
        {
            get
            {
                if (null == _FixedTrayPickCommand) _FixedTrayPickCommand = new AsyncCommand(FixedTrayPickFunc);
                return _FixedTrayPickCommand;
            }
        }
        private AsyncCommand _FixedTrayPutCommand;
        public ICommand FixedTrayPutCommand
        {
            get
            {
                if (null == _FixedTrayPutCommand) _FixedTrayPutCommand = new AsyncCommand(FixedTrayPutFunc);
                return _FixedTrayPutCommand;
            }
        }
        private async Task FixedTrayPickFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("FixedTray Pick", $"Do you want to FixedTray{SelectedFixedTrayIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"FixedTray Pick Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;
            await Task.Run(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var fixedTray = this.LoaderModule.ModuleManager.FindModule<IFixedTrayModule>(ModuleTypeEnum.FIXEDTRAY, SelectedFixedTrayIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(fixedTray.Holder);
                    if (result == EventCodeEnum.NONE) 
                    {
                        this.LoaderModule.GetLoaderCommands().FixedTrayPick(fixedTray, arm);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"FixedTrayPickFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private async Task FixedTrayPutFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("FixedTray Put", $"Do you want to FixedTray{SelectedFixedTrayIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"FixedTray Put Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            Task task = new Task(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var fixedTray = this.LoaderModule.ModuleManager.FindModule<IFixedTrayModule>(ModuleTypeEnum.FIXEDTRAY, SelectedFixedTrayIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                try
                {
                    EventCodeEnum result = this.SetWaferSize(arm.Holder);
                    if (result == EventCodeEnum.NONE)
                    {
                        this.LoaderModule.GetLoaderCommands().FixedTrayPut(arm, fixedTray);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"FixedTrayPutFunc(): Exception occurred. Err = {err.Message}");
                }
            });
            task.Start();
            await task;
        }




        private AsyncCommand _CardTrayPickCommand;
        public ICommand CardTrayPickCommand
        {
            get
            {
                if (null == _CardTrayPickCommand) _CardTrayPickCommand = new AsyncCommand(CardTrayPickFunc);
                return _CardTrayPickCommand;
            }
        }
        private AsyncCommand _CardTrayPutCommand;
        public ICommand CardTrayPutCommand
        {
            get
            {
                if (null == _CardTrayPutCommand) _CardTrayPutCommand = new AsyncCommand(CardTrayPutFunc);
                return _CardTrayPutCommand;
            }
        }
        private async Task CardTrayPickFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("CardTray Pick", $"Do you want to CardTray{SelectedCardTrayIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"CardTray Pick Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardfixedTray = this.LoaderModule.ModuleManager.FindModule<ICardBufferTrayModule>(ModuleTypeEnum.CARDTRAY, SelectedCardTrayIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardTrayPick(CardfixedTray, arm);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardTrayPickFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private async Task CardTrayPutFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("CardTray Put", $"Do you want to CardTray{SelectedCardTrayIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"CardTray Put Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardfixedTray = this.LoaderModule.ModuleManager.FindModule<ICardBufferTrayModule>(ModuleTypeEnum.CARDTRAY, SelectedCardTrayIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardTrayPut(arm, CardfixedTray);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardTrayPutFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }




        private AsyncCommand _CardBufferPickCommand;
        public ICommand CardBufferPickCommand
        {
            get
            {
                if (null == _CardBufferPickCommand) _CardBufferPickCommand = new AsyncCommand(CardBufferPickFunc);
                return _CardBufferPickCommand;
            }
        }
        private AsyncCommand _CardBufferPutCommand;
        public ICommand CardBufferPutCommand
        {
            get
            {
                if (null == _CardBufferPutCommand) _CardBufferPutCommand = new AsyncCommand(CardBufferPutFunc);
                return _CardBufferPutCommand;
            }
        }
        private async Task CardBufferPickFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("CardBuffer Pick", $"Do you want to CardBuffer{SelectedCardBufferIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"CardBuffer Pick Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardBuffer = this.LoaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, SelectedCardBufferIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardBufferPick(CardBuffer, arm);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardBufferPickFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private async Task CardBufferPutFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("CardBuffer Put", $"Do you want to CardBuffer{SelectedCardBufferIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"CardBuffer Put Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardBuffer = this.LoaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, SelectedCardBufferIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardBufferPut(arm, CardBuffer);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardBufferPutFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }



        private AsyncCommand _CardChangePickCommand;
        public ICommand CardChangePickCommand
        {
            get
            {
                if (null == _CardChangePickCommand) _CardChangePickCommand = new AsyncCommand(CardChangePickFunc);
                return _CardChangePickCommand;
            }
        }
        private AsyncCommand _CardChangePutCommand;
        public ICommand CardChangePutCommand
        {
            get
            {
                if (null == _CardChangePutCommand) _CardChangePutCommand = new AsyncCommand(CardChangePutFunc);
                return _CardChangePutCommand;
            }
        }
        private async Task CardChangePickFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Card Change Pick", $"Do you want to Card Change{SelectedChuckIndex + 1} Pick?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Card Change Pick Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardChange = this.LoaderModule.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, SelectedChuckIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardChangerPick(CardChange, arm);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardChangePickFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private async Task CardChangePutFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Card Change Put", $"Do you want to Card Change{SelectedChuckIndex + 1} Put?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Card Change Put Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardChange = this.LoaderModule.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, SelectedChuckIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardChangerPut(arm, CardChange);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardChangePutFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private AsyncCommand _WaferLoadMoveCommand;
        public ICommand WaferLoadMoveCommand
        {
            get
            {
                if (null == _WaferLoadMoveCommand) _WaferLoadMoveCommand = new AsyncCommand(WaferLoadMoveFunc);
                return _WaferLoadMoveCommand;
            }
        }
        private async Task WaferLoadMoveFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Wafer Load Pos Move", $"Do you want to move to wafer loading postion. Target Chuck{SelectedChuckIndex + 1}?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Wafer Load Pos Move Message Cancel");
                return;
            }

            var loader = LoaderModule;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, SelectedArmIndex + 1);
                var chuck = this.LoaderModule.ModuleManager.FindModule<IChuckModule>(ModuleTypeEnum.CHUCK, SelectedChuckIndex + 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().ChuckMoveLoadingPosition(chuck, arm);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"WaferLoadMoveFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }
        private AsyncCommand _CardLoadMoveCommand;
        public ICommand CardLoadMoveCommand
        {
            get
            {
                if (null == _CardLoadMoveCommand) _CardLoadMoveCommand = new AsyncCommand(CardLoadMoveFunc);
                return _CardLoadMoveCommand;
            }
        }
        private async Task CardLoadMoveFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Card Load Pos Move", $"Do you want to move to card loading postion. Target Chuck{SelectedChuckIndex + 1}?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Card Load Pos Move Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {

                var modules = this.LoaderModule.ModuleManager;
                var CardChange = this.LoaderModule.ModuleManager.FindModule<ICCModule>(ModuleTypeEnum.CC, SelectedChuckIndex + 1);
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardMoveLoadingPosition(CardChange, arm);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"CardLoadMoveFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }

        private AsyncCommand _BCDReadMoveCommand;
        public ICommand BCDReadMoveCommand
        {
            get
            {
                if (null == _BCDReadMoveCommand) _BCDReadMoveCommand = new AsyncCommand(BCDReadMoveCommandFunc);
                return _BCDReadMoveCommand;
            }
        }
        private async Task BCDReadMoveCommandFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("Barcode Read Pos Move", $"Do you want to move to barcode reading postion?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"Barcode Read Pos Move Message Cancel");
                return;
            }

            var loader = LoaderModule;
            var scanjob = loader.GetLoaderInfo().StateMap;

            await Task.Run(() =>
            {
                var modules = this.LoaderModule.ModuleManager;
                var arm = this.LoaderModule.ModuleManager.FindModule<ICardARMModule>(ModuleTypeEnum.CARDARM, 1);
                try
                {
                    this.LoaderModule.GetLoaderCommands().CardIDMovePosition(arm.Holder);
                }
                catch (Exception err)
                {
                    LoggerManager.Error($"BCDReadMoveCommandFunc(): Exception occurred. Err = {err.Message}");
                }
            });
        }


        private AsyncCommand _PAResetCommand;
        public ICommand PAResetCommand
        {
            get
            {
                if (null == _PAResetCommand) _PAResetCommand = new AsyncCommand(PAResetFunc);
                return _PAResetCommand;
            }
        }
        private async Task PAResetFunc()
        {
            var resultOp = await this.MetroDialogManager().ShowMessageDialog("PA Reset", $"Do you want to PA Reset?", EnumMessageStyle.AffirmativeAndNegative);
            if (resultOp != EnumMessageDialogResult.AFFIRMATIVE)
            {
                LoggerManager.Debug($"PA Reset Message Cancel");
                return;
            }

            try
            {
                int PACount = SystemModuleCount.ModuleCnt.PACount;
                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                for (int i = 0; i < PACount; i++)
                {
                    LoaderModule.PAManager.PAModules[i].UpdateState();
                    if (LoaderModule.PAManager.PAModules[i].PAStatus != ProberInterfaces.PreAligner.EnumPAStatus.Error)
                    {
                        LoggerManager.Debug($"PAResetFunc(): PAModule [{i}] PA State (OriginDone) : {LoaderModule.PAManager.PAModules[i].State.OriginDone} , PA Status {LoaderModule.PAManager.PAModules[i].PAStatus}");
                        ret = LoaderModule.PAManager.PAModules[i].ModuleInit();
                        if (ret == EventCodeEnum.NONE)
                        {
                            ret = LoaderModule.PAManager.PAModules[i].ModuleReset();
                            if (ret == EventCodeEnum.NONE)
                            {
                                // IsSubstrateExist 호출하면 Pa Vac 을 정리 해줌 ( 있으면 켜고 없으면 끄고)
                                var waferExist = LoaderModule.PAManager.PAModules[i].IsSubstrateExist(out bool isExist);
                            }
                        }
                    }
                    LoaderModule.PAManager.PAModules[i].UpdateState();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum SetWaferSize(WaferHolder Source)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (Source.TransferObject != null)
                {
                    if (Source.Status == EnumSubsStatus.EXIST || Source.Status == EnumSubsStatus.UNKNOWN)
                    {
                        SubstrateSizeEnum Size = Source.TransferObject.Size.Value;
                        EnumWaferType type = Source.TransferObject.WaferType.Value;
                        if (WaferSizeList.Contains(Size))
                        {
                            if (type == EnumWaferType.STANDARD || type == EnumWaferType.POLISH || type == EnumWaferType.TCW)
                            {
                                if (Size != WaferSizeList[SelectedWaferSizeIndex] &&// 여기서 에러 발생.
                                    Size != SubstrateSizeEnum.UNDEFINED &&
                                    Size != SubstrateSizeEnum.INVALID) // manual operation 화면에서의 동작에서 6/8/12 inch 처럼 명확하지 않은 웨이퍼를 리커버리 못하게 막아 버리면 복구는 어떻게 하지???
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error Message", $"The wafer size in the module({Size}) and the size information of the wafer({WaferSizeList[SelectedWaferSizeIndex]}) you want to operate are different.", EnumMessageStyle.Affirmative);

                                    return retval;
                                }
                                else
                                {
                                    LoggerManager.Debug($"SetWaferSize(): [{Source.TransferObject.CurrHolder}] Wafer Size {Size}");
                                    retval = this.LoaderModule.SetTransferWaferSize(Source.TransferObject, Source.Status);
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"SetWaferSize(): [{Source.TransferObject.CurrHolder}] Wafer Type {type}");
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"SetWaferSize(): [{Source.TransferObject.CurrHolder}] Wafer Size {Size}");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"SetWaferSize(): [{Source.TransferObject.CurrHolder}] Wafer Status {Source.Status}");
                    }
                }

                if (retval != EventCodeEnum.NONE)
                {
                    var transferSource = new TransferObject();
                    transferSource.WaferType.Value = EnumWaferType.STANDARD;
                    transferSource.Size.Value = WaferSizeList[SelectedWaferSizeIndex];//12inch 로 set 해줌. 

                    retval = this.LoaderModule.SetTransferWaferSize(transferSource, EnumSubsStatus.EXIST);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }


        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }
        #endregion
    }
}
