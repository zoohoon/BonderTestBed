using Autofac;
using LoaderBase;
using LoaderCore;
using LoaderParameters;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Communication;
using ProberInterfaces.Enum;
using ProberInterfaces.Foup;
using ProberInterfaces.RFID;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoaderParameterSettingView
{
    public class LoaderParameterViewModel : IMainScreenViewModel
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private Autofac.IContainer _Container => this.GetLoaderContainer();

        

        public LoaderParameterViewModel()
        {
        }

        //private List<string> _SerialPortList;
        //public List<string> SerialPortList
        //{
        //    get { return _SerialPortList; }
        //    set
        //    {
        //        if (value != _SerialPortList)
        //        {
        //            _SerialPortList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool _VisibleflagSingle = false;
        public bool VisibleflagSingle
        {
            get { return _VisibleflagSingle; }
            set
            {
                if (value != _VisibleflagSingle)
                {
                    _VisibleflagSingle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _VisibleflagMultiple = false;
        public bool VisibleflagMultiple
        {
            get { return _VisibleflagMultiple; }
            set
            {
                if (value != _VisibleflagMultiple)
                {
                    _VisibleflagMultiple = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Visibleflag_AddParam = false;
        public bool Visibleflag_AddParam
        {
            get { return _Visibleflag_AddParam; }
            set
            {
                if (value != _Visibleflag_AddParam)
                {
                    _Visibleflag_AddParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Visibleflag_CanUseBuffer = false;
        public bool Visibleflag_CanUseBuffer
        {
            get { return _Visibleflag_CanUseBuffer; }
            set
            {
                if (value != _Visibleflag_CanUseBuffer)
                {
                    _Visibleflag_CanUseBuffer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _RFIDConnectState = "DISCONNECT";
        public string RFIDConnectState
        {
            get { return _RFIDConnectState; }
            set
            {
                if (value != _RFIDConnectState)
                {
                    _RFIDConnectState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderModule _Loader;
        public ILoaderModule Loader
        {
            get { return _Loader; }
            set
            {
                if (value != _Loader)
                {
                    _Loader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFoupOpModule _FoupOP;
        public IFoupOpModule FoupOP
        {
            get { return _FoupOP; }
            set
            {
                if (value != _FoupOP)
                {
                    _FoupOP = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _RFID_CARD_ID;
        public string RFID_CARD_ID
        {
            get { return _RFID_CARD_ID; }
            set 
            { 
                _RFID_CARD_ID = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<SubstrateSizeEnum> _WaferSizeEnums
        = new ObservableCollection<SubstrateSizeEnum>();
        public ObservableCollection<SubstrateSizeEnum> WaferSizeEnums
        {
            get { return _WaferSizeEnums; }
            set
            {
                if (value != _WaferSizeEnums)
                {
                    _WaferSizeEnums = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand<object> _SaveParamCommand;
        public ICommand SaveParamCommand
        {
            get
            {
                if (null == _SaveParamCommand) _SaveParamCommand = new AsyncCommand<object>(SaveParamFunc);
                return _SaveParamCommand;
            }
        }
        
        private async Task SaveParamFunc(object forcedAffirmation)
        {
            try
            {
                var retVal = EnumMessageDialogResult.UNDEFIND;

                bool isAffirmative;
                var isParsed = bool.TryParse(forcedAffirmation.ToString(), out isAffirmative);
                if(isParsed && isAffirmative)
                {
                    retVal = EnumMessageDialogResult.AFFIRMATIVE;
                }
                else
                {
                    retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save", $"Do you want to Loader Parameter Save?", EnumMessageStyle.AffirmativeAndNegative).Result;
                }

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    EventCodeEnum ret = EventCodeEnum.UNDEFINED;

                    await (Loader as LoaderModule).CheckCanUseBufferExistPolish(true);

                    ret = (Loader as LoaderModule).SaveSysParameter();
                    (Loader as LoaderModule).SetModuleEnable();
                    if (ret != EventCodeEnum.NONE)
                    {
                        retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save Failed", $"Save SystemParameter failed", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    else
                    {
                        LoggerManager.Debug("Loader Module SaveSystem");
                        var CSTIDReader = FoupOP.FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CSTIDReader;
                        ret = CSTIDReader.SaveSysParameter();

                        if (ret != EventCodeEnum.NONE)
                        {
                            retVal = this.MetroDialogManager().ShowMessageDialog("Loader CSTIDReader Parameter Save Error", $"Would you like to Parameter Save proceed?", EnumMessageStyle.AffirmativeAndNegative).Result;
                            if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    if (ret != EventCodeEnum.NONE)
                    {
                        retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save Failed", $"Save SystemParameter failed", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    else
                    {
                        CheckCassetteType();
                        ValidateCassetteTypesConsistency();

                        ret = (Loader as LoaderModule).SaveDevParameter();
                        if (ret != EventCodeEnum.NONE)
                        {
                            retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save Failed", $"Save DeviceParameter failed", EnumMessageStyle.Affirmative).Result;
                        }

                        ret = Loader.MotionManager.SaveLoaderAxesObject();
                        if (ret != EventCodeEnum.NONE)
                        {
                            retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save Failed", $"Save Loader Axis failed", EnumMessageStyle.Affirmative).Result;
                        }
                        else
                        {
                            for (int i = 0; i < SystemModuleCount.ModuleCnt.FoupCount; i++)
                            {
                                ret = Loader.LoaderService.UpdateLoaderSystem(i + 1);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    break;
                                }
                                ret = Loader.LoaderService.UpdateCassetteSystem(Loader.GetLoaderCommands().GetDeviceSize(i), i + 1);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    break;
                                }
                            }

                            if (ret == EventCodeEnum.NONE)
                            {
                                retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save Success", $"Loader Parameter Save Succeed", EnumMessageStyle.Affirmative).Result;
                            }
                            else
                            {
                                retVal = this.MetroDialogManager().ShowMessageDialog("Loader Parameter Save Failed", $"Update LoaderSystem failed", EnumMessageStyle.Affirmative).Result;
                            }
                        }
                    }
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _Add13slotParamCommand;
        public ICommand Add13slotParamCommand
        {
            get
            {
                if (null == _Add13slotParamCommand) _Add13slotParamCommand = new AsyncCommand<object>(Add13slotParamCommandFunc);
                return _Add13slotParamCommand;
            }
        }

        private async Task Add13slotParamCommandFunc(object forcedAffirmation)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var size = Loader.GetDefaultWaferSize();
                //Chuck tap
                foreach (var chuckModule in Loader.SystemParameter.ChuckModules)
                {
                    var chuck = chuckModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_25);
                    var chuck_13 = chuckModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_13);
                    if (chuck != null && chuck_13 == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ChuckAccessParam clonedChuck = (ChuckAccessParam)chuck.DeepClone();
                            clonedChuck.CassetteType.Value = CassetteTypeEnum.FOUP_13;
                            chuckModule.AccessParams.Add(clonedChuck);
                        });
                    }
                }
                //PreAlign tap
                foreach (var preAlignModule in Loader.SystemParameter.PreAlignModules)
                {
                    var preAlign = preAlignModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_25);
                    var preAlign_13 = preAlignModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_13);
                    if (preAlign != null && preAlign_13 == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            PreAlignAccessParam clonedpreAlign = (PreAlignAccessParam)preAlign.DeepClone();
                            clonedpreAlign.CassetteType.Value = CassetteTypeEnum.FOUP_13;
                            preAlignModule.AccessParams.Add(clonedpreAlign);
                        });
                    }
                }
                //Cassette tap
                foreach (var cassetteModule in Loader.SystemParameter.CassetteModules)
                {
                    var cassette = cassetteModule.Slot1AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_25);
                    var cassette_13 = cassetteModule.Slot1AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_13);
                    if (cassette != null && cassette_13 == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            CassetteSlot1AccessParam clonedcassette = (CassetteSlot1AccessParam)cassette.DeepClone();
                            clonedcassette.CassetteType.Value = CassetteTypeEnum.FOUP_13;
                            cassetteModule.Slot1AccessParams.Add(clonedcassette);
                        });
                    }
                }
                //Buffer tap
                foreach (var bufferModule in Loader.SystemParameter.BufferModules)
                {
                    //FoupModeStatusEnum.OFFLINE 이 아니라 선택된 tap 번호를 들고와야함
                    var buffer = bufferModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_25);
                    var buffer_13 = bufferModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_13);
                    if (buffer != null && buffer_13 == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            BufferAccessParam clonedbuffer = (BufferAccessParam)buffer.DeepClone();
                            clonedbuffer.CassetteType.Value = CassetteTypeEnum.FOUP_13;
                            bufferModule.AccessParams.Add(clonedbuffer);
                        });
                    }
                }
                //INSPTray tap
                foreach (var inspectionModule in Loader.SystemParameter.InspectionTrayModules)
                {
                    var inspection = inspectionModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_25);
                    var inspection_13 = inspectionModule.AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_13);
                    if (inspection != null && inspection_13 == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            InspectionTrayAccessParam clonedTray = (InspectionTrayAccessParam)inspection.DeepClone();
                            clonedTray.CassetteType.Value = CassetteTypeEnum.FOUP_13;
                            inspectionModule.AccessParams.Add(clonedTray);
                        });
                    }
                }
                //Foup tap
                foreach (var scanSensorModule in Loader.SystemParameter.ScanSensorModules)
                {
                    var scanSensor = scanSensorModule.ScanParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_25);
                    var scanSensor_13 = scanSensorModule.ScanParams.FirstOrDefault(dev => dev.SubstrateSize.Value == size && dev.CassetteType.Value == CassetteTypeEnum.FOUP_13);
                    if (scanSensor != null && scanSensor_13 == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ScanSensorParam clonedscanSensor = (ScanSensorParam)scanSensor.DeepClone();
                            clonedscanSensor.CassetteType.Value = CassetteTypeEnum.FOUP_13;
                            scanSensorModule.ScanParams.Add(clonedscanSensor);
                        });
                    }
                }
                SaveParamFunc(true);
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally 
            {
                if (ret != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Loader Parameter Addition Failed", $"Failed to add loader parameter. ", EnumMessageStyle.Affirmative);
                }
            }
        }

        public void Initialize(Autofac.IContainer container)
        {
            // Loader.MotionManager.LoaderAxes.ProbeAxisProviders[0].Param.IndexSearchingSpeed.Value
            //  Loader.SystemParameter.BufferModules[0].AccessParams[0].PickupIncrement.Value
        }



        readonly Guid _ViewModelGUID = new Guid("4F4C481C-F554-8911-D4D1-19BC4BB5E821");
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

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        public EventCodeEnum InitModule()
        {
            try
            {
                Loader = _Container.Resolve<ILoaderModule>();
                FoupOP = _Container.Resolve<IFoupOpModule>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            foreach (var waferenum in Enum.GetValues(typeof(SubstrateSizeEnum)))
            {
                if (SubstrateSizeEnum.INVALID != (SubstrateSizeEnum)waferenum & SubstrateSizeEnum.UNDEFINED != (SubstrateSizeEnum)waferenum)
                {
                    WaferSizeEnums.Add((SubstrateSizeEnum)waferenum);
                }
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            var retval = EventCodeEnum.NONE;
            try
            {
                VisibleflagMultiple = true;     //Default는 false(visible) => 일단 둘다 안보이게 true(collapsed)로 RaisePropertyChanged();
                VisibleflagSingle = true;
                var rfidModule = FoupOP.FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as IRFIDModule;

                if (rfidModule != null)
                {
                    if (rfidModule.RFIDSysParam.RFIDProtocolType == ProberInterfaces.RFID.EnumRFIDProtocolType.SINGLE)
                    {
                        VisibleflagMultiple = true;     //true가 collapsed
                        VisibleflagSingle = false;      //false가 visible
                    }
                    else if (rfidModule.RFIDSysParam.RFIDProtocolType == ProberInterfaces.RFID.EnumRFIDProtocolType.MULTIPLE)
                    {
                        VisibleflagSingle = true;
                        VisibleflagMultiple = false;
                    }
                }

                CheckCassetteType();
                ValidateCassetteTypesConsistency();

                RFIDConnectState = Loader.GetLoaderCommands().GetRFIDCommState_ForCardID().ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        private void CheckCassetteType()
        {
            try
            {
                var Exist13slotCSTparam = Loader.GetModulesSupportingCassetteType(CassetteTypeEnum.FOUP_13);
                bool Availableflag = FoupOP.FoupControllers[0].CassetteTypeAvailable(CassetteTypeEnum.FOUP_13) == EventCodeEnum.NONE;
                if (Availableflag && Exist13slotCSTparam != EventCodeEnum.NONE)
                {
                    Visibleflag_AddParam = true;
                }
                else
                {
                    Visibleflag_AddParam = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum ValidateCassetteTypesConsistency()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            string combinedLog = "";
            try
            {
                retval = Loader.ValidateCassetteTypesConsistency(out combinedLog);
                if (retval == EventCodeEnum.NONE)
                {
                    Visibleflag_CanUseBuffer = true;
                }
                else
                {
                    Visibleflag_CanUseBuffer = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            var retval = EventCodeEnum.NONE;
            try
            {
                CheckParameterChanged();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        private void CheckParameterChanged()
        {
            var CassetteIDReaderModule = FoupOP.FoupControllers[0].Service.FoupModule.CassetteIDReaderModule;

            var paramChanged = ((Loader as LoaderModule).MotionManager.LoaderAxes as LoaderAxes).IsParamChanged ||
                                (Loader as LoaderModule).SystemParameter.IsParamChanged ||
                                CassetteIDReaderModule.CSTIDReaderParam.IsParamChanged;

            if (paramChanged)
            {
                if (this.MetroDialogManager().
                    ShowMessageDialog("Loader Parameters have been changed.",
                                     $"Do you want to save changes?",
                                    EnumMessageStyle.AffirmativeAndNegative).Result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    SaveParamFunc(true);
                }
                else
                {   // CANCEL
                    // 변경한 파라미터 값을 저장하지 않는 경우,
                    // 기존 파라미터를 다시 로딩하여 변경된 사항을 지운다.
                    if(((Loader as LoaderModule).MotionManager.LoaderAxes as LoaderAxes).IsParamChanged)
                    {
                        if((Loader as LoaderModule).MotionManager is LoaderCore.ProxyModules.RemoteMotionProxy motionManager)
                        {
                            motionManager.LoadSysParameter();
                        }
                    }

                    if ((Loader as LoaderModule).SystemParameter.IsParamChanged)
                    {
                        (Loader as LoaderModule).LoadSysParameter();
                    }

                    if ((Loader as LoaderModule).DeviceParameter.IsParamChanged)
                    {
                        (Loader as LoaderModule).LoadDevParameter();
                    }

                    if (CassetteIDReaderModule.CSTIDReaderParam.IsParamChanged)
                    {
                        CassetteIDReaderModule.CSTIDReader.LoadSysParameter();
                    }

                    Loader = null;
                    FoupOP = null;

                    InitModule();
                }
            }
        }

        private AsyncCommand _RFIDReInitializeCommand;
        public ICommand RFIDReInitializeCommand
        {
            get
            {
                if (null == _RFIDReInitializeCommand) _RFIDReInitializeCommand = new AsyncCommand(RFIDReInitializeCommandFunc);
                return _RFIDReInitializeCommand;
            }
        }
        private async Task RFIDReInitializeCommandFunc()//RFIDProtocolType이 Multiple 일 때 불리는 Command
        {
            try
            {
                var rfidModule = FoupOP.FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as IRFIDModule;
                rfidModule.LoadSysParameter();
                rfidModule.ReInitialize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SelectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get
            {
                if (null == _SelectionChangedCommand) _SelectionChangedCommand = new AsyncCommand<object>(SelectionChangedCommandFunc);
                return _SelectionChangedCommand;
            }
        }

        private async Task SelectionChangedCommandFunc(object param)
        {
            try
            {
                RFIDParamConverter obj = param as RFIDParamConverter;
                if (obj != null)
                {
                    //bool attatch = (bool)obj.IsAttatch;
                    if(obj != null)
                    {
                        Element<int> index = (Element<int>)obj.Index;
                        bool attatch = (bool)obj.IsAttatch;
                        
                        var rfidModule = FoupOP.FoupControllers[index.Value].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as IRFIDModule;

                        rfidModule.ModuleAttached = attatch;
                        rfidModule.ModuleCommType = (EnumCommmunicationType)obj.CommType;
                        rfidModule.StopBitsEnum = (StopBits)obj.StopBits;
                        rfidModule.SerialPort = (string)obj.SerialPort;

                        if (rfidModule.RFIDSysParam.RFIDProtocolType == ProberInterfaces.RFID.EnumRFIDProtocolType.SINGLE)
                        {
                            rfidModule.ReInitialize();
                        }
                        else if (rfidModule.RFIDSysParam.RFIDProtocolType == ProberInterfaces.RFID.EnumRFIDProtocolType.MULTIPLE)
                        {
                            (FoupOP.FoupControllers[0].Service.FoupModule.CassetteIDReaderModule.CSTIDReader as IRFIDModule).ReInitialize();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ReadCardIDTestCommand;
        public ICommand ReadCardIDTestCommand
        {
            get
            {
                if (null == _ReadCardIDTestCommand) _ReadCardIDTestCommand = new AsyncCommand(ReadCardIDTestCommandFunc);
                return _ReadCardIDTestCommand;
            }
        }
        private async Task ReadCardIDTestCommandFunc()
        {
            string tag_id = "";
            RFID_CARD_ID = "";

            try
            {
                if (Loader.GetLoaderCommands().GetCardIDReadDataReady() == true)
                {
                    tag_id = Loader.GetLoaderCommands().GetReceivedCardID();
                    RFID_CARD_ID = tag_id;
                }
                else
                {
                    LoggerManager.Debug($"[RFID_CardID_ReadTest] GetCardIDReadDataReady() is False");
                }

                LoggerManager.Debug($"[RFID_CardID_ReadTest] Card ID : {RFID_CARD_ID}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _RFIDforCardIDReInitializeCommand;
        public ICommand RFIDforCardIDReInitializeCommand
        {
            get
            {
                if (null == _RFIDforCardIDReInitializeCommand) _RFIDforCardIDReInitializeCommand = new AsyncCommand(RFIDforCardIDReInitializeCommandFunc);
                return _RFIDforCardIDReInitializeCommand;
            }
        }
        private async Task RFIDforCardIDReInitializeCommandFunc()
        {
            EventCodeEnum errCode = EventCodeEnum.UNDEFINED;
            RFIDConnectState = "";
            RFID_CARD_ID = "";

            try
            {
                errCode = Loader.GetLoaderCommands().RFIDReInitialize();
                if(errCode == EventCodeEnum.NONE)
                {
                    RFIDConnectState = Loader.GetLoaderCommands().GetRFIDCommState_ForCardID().ToString();
                }
                else
                {
                    LoggerManager.Debug($"RFIDforCardIDReInitializeCommandFunc() Fail. {errCode}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
