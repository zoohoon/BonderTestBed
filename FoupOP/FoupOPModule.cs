using ProberInterfaces;
using SequenceService;
using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using Autofac;
using SubstrateObjects;
using ProberErrorCode;
using ProberInterfaces.Foup;
using LogModule;
using System.Runtime.CompilerServices;
using ProberInterfaces.Enum;
using RFID;
using ProberInterfaces.Communication.RFID;
using NotifyEventModule;
using System.Threading;
using ProberInterfaces.Event;
using ProberInterfaces.RFID;
using ProberInterfaces.CassetteIDReader;
using System.Threading.Tasks;

namespace FoupOP
{
    public class FoupOpModule : SequenceServiceBase, IFoupOpModule, INotifyPropertyChanged, IHasDevParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private class WaferInfo : IWaferInfo
        {
            public EnumWaferSize WaferSize { get; set; }
            public double Notchangle { get; set; }
            public double WaferThickness { get; set; }
            public OCRTypeEnum OCRType { get; set; }
            public OCRDirectionEnum OCRDirection { get; set; }
            public OCRModeEnum OCRMode { get; set; }
        }
        IFoupCylinderType FoupCylinderType { get; }

        private ObservableCollection<IFoupController> _FoupControllers;

        public ObservableCollection<IFoupController> FoupControllers
        {
            get { return _FoupControllers; }
            set { _FoupControllers = value; RaisePropertyChanged(); }
        }

        private IRFIDAdapter _RFIDAdapter;
        public IRFIDAdapter RFIDAdapter
        {
            get { return _RFIDAdapter; }
            set
            {
                if (value != _RFIDAdapter)
                {
                    _RFIDAdapter = value;
                }
            }
        }


        public bool Initialized { get; set; } = false;

        private IParam _FoupManagerSysParam_IParam;
        public IParam FoupManagerSysParam_IParam
        {
            get { return _FoupManagerSysParam_IParam; }
            set
            {
                if (value != _FoupManagerSysParam_IParam)
                {
                    _FoupManagerSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _FoupManagerDevParam_IParam;
        public IParam FoupManagerDevParam_IParam
        {
            get { return _FoupManagerDevParam_IParam; }
            set
            {
                if (value != _FoupManagerDevParam_IParam)
                {
                    _FoupManagerDevParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IParam _CassetteConfigurationParam_IParam;
        public IParam CassetteConfigurationParam_IParam
        {
            get { return _CassetteConfigurationParam_IParam; }
            set
            {
                if (value != _CassetteConfigurationParam_IParam)
                {
                    _CassetteConfigurationParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        public FoupManagerSystemParameter FoupManagerSysParam;

        public FoupManagerDeviceParameter FoupManagerDevParam;

        public CassetteConfigurationParameter CassetteConfigurationParam;

        public void DeInitModule()
        {
            try
            {
                foreach (var controller in FoupControllers)
                {
                    controller.DeInit();
                }

                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


        }
        public EventCodeEnum InitProcedures()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
               int foupNumber = 1;

                foreach (var foupSysParam in FoupManagerSysParam.FoupModules)
                {

                    retVal = FoupControllers[foupNumber - 1].InitProcedures();
                    foupNumber++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum FoupInitState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                int foupNumber = 1;
                
                foreach (var foupSysParam in FoupManagerSysParam.FoupModules)
                {

                    retVal = FoupControllers[foupNumber - 1].FoupStateInit();
                    foupNumber++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (Initialized == false)
                    {
                        _FoupControllers = new ObservableCollection<IFoupController>();
                      


                        int foupNumber = 1;

                        foreach (var foupSysParam in FoupManagerSysParam.FoupModules)
                        {
                            IFoupController controller = new FoupController.FoupController();

                            _FoupControllers.Add(controller);

                            var foupDevParam = FoupManagerDevParam.FoupModules[foupNumber - 1];

                            controller.InitController(foupNumber, foupSysParam, foupDevParam, CassetteConfigurationParam);
                            controller.FoupModuleInfo.FoupModeStatus = foupSysParam.FoupModeStatus.Value;
                            foupNumber++;
                        }

                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupOPModule_OK);

                        IStageSupervisor stageSupervisor = this.StageSupervisor();

                        if (stageSupervisor != null)
                        {
                            stageSupervisor.WaferObject.ChangedWaferObjectEvent += OnChangedWaferObjectFunc;
                            stageSupervisor.WaferObject.CallWaferobjectChangedEvent();

                            //stageSupervisor.ChangedWaferObjectEvent += OnChangedWaferObjectFunc;
                            //stageSupervisor.CallWaferobjectChangedEvent();
                        }

                        var rfidModule = (FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as RFIDModule);
                        
                        if(rfidModule != null)
                        {
                            rfidModule.LoadSysParameter();
                            if (rfidModule.RFIDSysParam?.RFIDProtocolType == EnumRFIDProtocolType.MULTIPLE)
                            {
                                RFIDAdapter = new RFIDAdapter();
                                RFIDAdapter.InitModule();
                            }
                        }

                        Task task = new Task(() =>
                        {
                            FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CheckConnectState();
                        });
                        task.Start();

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

                    LoggerManager.Debug($"{err.ToString()}. FoupOPModule - InitModule() : Error occured.");

                    retval = EventCodeEnum.UNKNOWN_EXCEPTION;

                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupOPModule_Failure, retval);
                }

                //TODO : Load System
                //FoupManagerSysParam = new FoupManagerSystemParameter();
                //FoupManagerSysParam.FoupModules = new ObservableCollection<FoupSystemParam>();
                //FoupManagerSysParam.FoupModules.Add(new FoupSystemParam()
                //{
                //    FoupType = FoupTypeEnum.TOP,
                //});

                //TODO : Load Device
                //FoupManagerDevParam = new FoupManagerDeviceParameter();
                //FoupManagerDevParam.FoupModules = new ObservableCollection<FoupDeviceParam>();
                //FoupManagerDevParam.FoupModules.Add(new FoupDeviceParam()
                //{
                //    SubstrateType = SubstrateTypeEnum.Wafer,
                //    SubstrateSize = SubstrateSizeEnum.INCH12,
                //});

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        private void OnChangedWaferObjectFunc(object sender, EventArgs e)
        {
            try
            {
                EventCodeEnum SaveResult = EventCodeEnum.UNDEFINED;
                if (e is WaferObjectEventArgs)
                {
                    EnumWaferSize waferSize = EnumWaferSize.UNDEFINED;
                    waferSize = GetWaferSizeFromWaferObjEventArg(e as WaferObjectEventArgs);

                    IWaferInfo waferInfo = new WaferInfo() { WaferSize = waferSize };

                    switch (waferInfo.WaferSize)
                    {
                        case EnumWaferSize.INVALID:
                            break;
                        case EnumWaferSize.UNDEFINED:
                            break;
                        case EnumWaferSize.INCH6:
                            FoupManagerDevParam.FoupModules[0].SubstrateSize.Value = SubstrateSizeEnum.INCH6;
                            break;
                        case EnumWaferSize.INCH8:
                            FoupManagerDevParam.FoupModules[0].SubstrateSize.Value = SubstrateSizeEnum.INCH8;
                            break;
                        case EnumWaferSize.INCH12:
                            FoupManagerDevParam.FoupModules[0].SubstrateSize.Value = SubstrateSizeEnum.INCH12;
                            break;
                        default:
                            break;
                    }

                    SaveResult = SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EnumWaferSize GetWaferSizeFromWaferObjEventArg(WaferObjectEventArgs waferObjArgs)
        {
            EnumWaferSize retWaferSize = EnumWaferSize.UNDEFINED;
            try
            {
                WaferObject waferObj = waferObjArgs?.WaferObject as WaferObject;
                retWaferSize = waferObj?.GetPhysInfo()?.WaferSizeEnum ?? EnumWaferSize.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retWaferSize;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public IFoupController GetFoupController(int cassetteNumber)
        {
            IFoupController controller = null;
            try
            {
                if((cassetteNumber - 1) >= 0 && (cassetteNumber) <= _FoupControllers.Count)
                {
                    controller = _FoupControllers[cassetteNumber - 1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return controller;
        }

        public override ModuleStateEnum SequenceRun()
        {
            foreach (var foupController in FoupControllers)
            {
                foupController.RecoveryIfErrorOccurred();
            }

            return ModuleStateEnum.IDLE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new FoupManagerSystemParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(FoupManagerSystemParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    FoupManagerSysParam_IParam = tmpParam;
                    FoupManagerSysParam = FoupManagerSysParam_IParam as FoupManagerSystemParameter;
                }

                tmpParam = null;
                tmpParam = new CassetteConfigurationParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CassetteConfigurationParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CassetteConfigurationParam_IParam = tmpParam;
                    CassetteConfigurationParam = tmpParam as CassetteConfigurationParameter;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupOPModule - LoadSysParameter() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupOPModule_Failure, RetVal);
                return RetVal;

            }


            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = this.SaveParameter(FoupManagerSysParam);
                RetVal = this.SaveParameter(CassetteConfigurationParam);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new FoupManagerDeviceParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(FoupManagerDeviceParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    FoupManagerDevParam_IParam = tmpParam;
                    FoupManagerDevParam = FoupManagerDevParam_IParam as FoupManagerDeviceParameter;
                }
                //DevParam = FoupManagerDevParam;
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    if (FoupControllers != null)
                    {
                        FoupControllers[0].GetFoupService().SetDevice(FoupManagerDevParam.FoupModules[0]);
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. FoupOPModule - LoadDevParameter() : Error occured.");
                RetVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.FoupOPModule_Failure, RetVal);

                return RetVal;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //
                RetVal = this.SaveParameter(FoupManagerDevParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public FoupIOMappings GetFoupIOMap(int cassetteNumber)
        {
            return FoupControllers[cassetteNumber - 1].GetFoupIOMap();
        }

        public void SetFoupModeStatus(int foupindex , FoupModeStatusEnum statusEnum)
        {
            try
            {
                if(FoupManagerSysParam.FoupModules.Count != 0 && FoupManagerSysParam.FoupModules.Count >= foupindex && foupindex != 0)
                {
                    var preState = FoupControllers[foupindex - 1].FoupModuleInfo.FoupModeStatus;
                    FoupManagerSysParam.FoupModules[foupindex - 1].FoupModeStatus.Value = statusEnum;
                    FoupControllers[foupindex - 1].FoupModuleInfo.FoupModeStatus = statusEnum;
                    if(preState != statusEnum)
                    {
                        var pivinfo = new PIVInfo() { FoupNumber = foupindex };
                        if (statusEnum == FoupModeStatusEnum.ONLINE)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(FoupOnlineEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                        }
                        else if(statusEnum == FoupModeStatusEnum.OFFLINE)
                        {
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(FoupOfflineEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void TempSetFoupModeStatus(int foupindex, bool entry)
        {
            try
            {
                if (FoupManagerSysParam.FoupModules.Count != 0 && FoupManagerSysParam.FoupModules.Count >= foupindex && foupindex != 0)
                {
                    if (entry == true) //모두 체크
                    {
                        var preState = FoupControllers[foupindex - 1].FoupModuleInfo.FoupModeStatus;
                        //LoggerManager.Debug($"Entry Foup Recovery Page: FoupIndex = {foupindex}, FoupMode = {preState}");
                        if (preState == FoupModeStatusEnum.ONLINE)
                        {
                            FoupManagerSysParam.FoupModules[foupindex - 1].FoupModeStatus.Value = FoupModeStatusEnum.OFFLINE;
                            FoupControllers[foupindex - 1].FoupModuleInfo.FoupModeStatus = FoupModeStatusEnum.OFFLINE;
                            FoupControllers[foupindex - 1].FoupModuleInfo.IsChangedFoupMode = true;

                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(FoupOfflineEvent).FullName, new ProbeEventArgs(this, semaphore));
                            semaphore.Wait();

                            LoggerManager.Debug($"==Foup#{foupindex} Mode== Entry Foup Recovery Page: Change from FoupMode ONLINE to OFFLINE");
                        }
                        else
                        {
                            LoggerManager.Debug($"==Foup#{foupindex} Mode== Entry Foup Recovery Page: Cur FoupMode OFFLINE");
                            FoupControllers[foupindex - 1].FoupModuleInfo.IsChangedFoupMode = false;
                        }
                    }
                    else //바꾼 foup만 체크
                    {
                        //page 나감. 복구
                        FoupManagerSysParam.FoupModules[foupindex - 1].FoupModeStatus.Value = FoupModeStatusEnum.ONLINE;
                        FoupControllers[foupindex - 1].FoupModuleInfo.FoupModeStatus = FoupModeStatusEnum.ONLINE;
                        FoupControllers[foupindex - 1].FoupModuleInfo.IsChangedFoupMode = false;

                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(FoupOnlineEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();

                        LoggerManager.Debug($"==Foup#{foupindex} Mode== Exit Foup Recovery Page: FoupIndex = {foupindex}, FoupMode is changed to ONLINE");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
