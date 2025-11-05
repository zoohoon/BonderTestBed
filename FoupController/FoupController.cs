using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


using ProberInterfaces;
using ProberInterfaces.Foup;
using ProberInterfaces.Utility;
using System.Threading.Tasks;
using ProberErrorCode;
using LogModule;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Windows.Input;
using RelayCommandBase;
using System.IO;
using ProberInterfaces.Communication.BarcodeReader;

namespace FoupController
{
    public class FoupController : IFoupController, IFoupServiceCallback, INotifyPropertyChanged, IParamNode
    {
        public List<object> Nodes { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private const string foupAssemblyName = "FoupModules.dll";

        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public IFoupService Service { get; set; }

        private FoupModuleInfo _FoupModuleInfo;

        public FoupController()
        {
        }

        public FoupModuleInfo FoupModuleInfo
        {
            get { return _FoupModuleInfo; }
            set { _FoupModuleInfo = value; RaisePropertyChanged(); }
        }

        private string _RFID_ID;
        public string RFID_ID
        {
            get { return _RFID_ID; }
            set { _RFID_ID = value; RaisePropertyChanged(); }
        }

        private string _BCD_ID;
        public string BCD_ID
        {
            get { return _BCD_ID; }
            set { _BCD_ID = value; RaisePropertyChanged(); }
        }

        public int FoupNumber { get; set; }
        public EventCodeEnum InitController(int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam)
        {
            EventCodeEnum retVal;
            try
            {
                //retVal = InitFoupService(FoupServiceTypeEnum.Direct, foupNumber, systemParam, deviceParam);
                retVal = InitFoupService(systemParam.ServiceType.Value, foupNumber, systemParam, deviceParam, CassetteConfigurationParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public EventCodeEnum Execute(FoupCommandBase command)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (FoupModuleInfo.Enable)
                {
                    retVal = this.Service.SetCommand(command);
                }
                else
                {
                    LoggerManager.Debug($"Foup #{FoupModuleInfo.FoupNumber} can not Execute because disable state.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void DeInit()
        {
            try
            {
                if (Service is IDirectFoupService)
                {
                    (Service as IDirectFoupService).Deinit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private AsyncCommand _CassetteIDReaderReInitializeCommand;
        public ICommand CassetteIDReaderReInitializeCommand
        {
            get
            {
                if (null == _CassetteIDReaderReInitializeCommand) _CassetteIDReaderReInitializeCommand = new AsyncCommand(CassetteIDReaderReInitializeCommandFunc);
                return _CassetteIDReaderReInitializeCommand;
            }
        }
        private Task CassetteIDReaderReInitializeCommandFunc()
        {
            try
            {
                Service.FoupModule.CassetteIDReaderModule.CSTIDReader.LoadSysParameter();
                Service.FoupModule.CassetteIDReaderModule.CSTIDReader.ReInitialize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.CompletedTask;
        }

        private AsyncCommand _CassetteID_ReadTestCommand;
        public ICommand CassetteID_ReadTestCommand
        {
            get
            {
                if (null == _CassetteID_ReadTestCommand) _CassetteID_ReadTestCommand = new AsyncCommand(CassetteID_ReadTestCommandFunc);
                return _CassetteID_ReadTestCommand;
            }
        }
        private async Task CassetteID_ReadTestCommandFunc()
        {
            try
            {
                string tag_id = null;
                RFID_ID = null; // UI Binding
                BCD_ID = null; // UI Binding

                Task task = new Task(() =>
                {
                    tag_id = Service.FoupModule.Read_CassetteID();

                    if (Service.FoupModule.CassetteIDReaderModule.CSTIDReader is IRFIDModule)
                    {
                        RFID_ID = tag_id;
                    }
                    else if(Service.FoupModule.CassetteIDReaderModule.CSTIDReader is IBarcodeReaderModule)
                    {
                        BCD_ID = tag_id;
                    }
                    LoggerManager.Debug($"[CassetteID_ReadTestCommandFunc] ID : {tag_id}");
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private EventCodeEnum InitFoupService(FoupServiceTypeEnum serviceType, int foupNumber, FoupSystemParam systemParam, FoupDeviceParam deviceParam, CassetteConfigurationParameter CassetteConfigurationParam)
        {
            EventCodeEnum retVal;

            try
            {
                if (serviceType == FoupServiceTypeEnum.Direct)
                {
                    string dllpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, foupAssemblyName);

                    var foupAssembly = Assembly.LoadFrom(dllpath);

                    IEnumerable<IDirectFoupService> foupServices = ReflectionEx.GetAssignableInstances<IDirectFoupService>(foupAssembly);
                    var directFoupService = foupServices.FirstOrDefault();
                    directFoupService.SetCallback(this, serviceType);
                    this.Service = directFoupService;
                    SetLock(false);
                    retVal = EventCodeEnum.NONE;
                }
                else if (serviceType == FoupServiceTypeEnum.WCF)
                {
                    //Hardcoding-------------
                    //var uri = new Uri(@"net.pipe://localhost/services/foup");
                    //var binding = new NetNamedPipeBinding();
                    //var uri = new Uri(@"net.tcp://localhost:7070/services/foup");
                    //var binding = new NetTcpBinding();
                    //var factory = new DuplexChannelFactory<ILoaderService>(callback, binding, new EndpointAddress(uri));
                    //-----------------------

                    string EndpointConfigurationName = "";

                    var factory = new DuplexChannelFactory<IFoupService>(this, EndpointConfigurationName);

                    this.Service = factory.CreateChannel();

                    retVal = EventCodeEnum.NONE;
                }
                else if (serviceType == FoupServiceTypeEnum.EMUL)
                {
                    string dllpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, foupAssemblyName);

                    var foupAssembly = Assembly.LoadFrom(dllpath);

                    IEnumerable<IDirectFoupService> foupServices = ReflectionEx.GetAssignableInstances<IDirectFoupService>(foupAssembly);
                    var foupService = foupServices.FirstOrDefault();
                    foupService.SetCallback(this, serviceType);
                    this.Service = foupService;
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = Service.InitModule(foupNumber, systemParam, deviceParam, CassetteConfigurationParam);
                }

                if (retVal == EventCodeEnum.NONE)
                {
                    retVal = this.Service.Connect();
                    this.FoupModuleInfo = Service.GetFoupModuleInfo();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void RecoveryIfErrorOccurred()
        {
            try
            {
                //TODO : 
                //Foup에러 발생시
                //Loader & Stage 가 Running이 아니면 리커버리 UI를 띠운다.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public FoupModuleInfo GetFoupModuleInfo()
        {
            return Service.GetFoupModuleInfo();
        }

        public void SetLoaderState(ModuleStateEnum state)
        {
            Service.SetLoaderState(state);
        }

        public void SetLotState(ModuleStateEnum state)
        {
            Service.SetLotState(state);
        }

        public void RaiseFoupModuleStateChanged(FoupModuleInfo moduleInfo)
        {
            try
            {
                this.FoupModuleInfo = moduleInfo;

                if (this.LoaderController().ModuleState != null)
                {
                    this.LoaderController().OnFoupModuleStateChanged(moduleInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum MonitorForWaferOutSensor(bool value)
        {
            return Service.MonitorForWaferOutSensor(value);
        }

        public ICylinderManager GetFoupCylinderManager()
        {
            return Service.GetFoupCylinderManager();
        }

        public IFoupProcedureManager GetFoupProcedureManager()
        {
            return Service.GetFoupProcedureManager();
        }

        public void ChangeState(FoupStateEnum state)
        {
            Service.ChangeState(state);
        }

        public EventCodeEnum FoupStateInit()
        {
            return Service.FoupStateInit();
        }
        public EventCodeEnum InitProcedures()
        {
            return Service.InitProcedures();
        }

        public void OnChangedFoupInfoFunc(object sender, EventArgs e)
        {
            try
            {
                var property = e as PropertyChangedEventArgs;
                FoupModuleInfo foupModuleInfo = sender as FoupModuleInfo;
                if (FoupModuleInfo != null)
                {
                    switch (property.PropertyName)
                    {
                        case "AlarmLamp":
                            {
                                if (foupModuleInfo.AlarmLamp == true)
                                {
                                    this.E84Module().SetSignal(FoupModuleInfo.FoupNumber, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, false);
                                }
                                else
                                {
                                    this.E84Module().SetSignal(FoupModuleInfo.FoupNumber, E84OPModuleTypeEnum.FOUP, E84SignalTypeEnum.HO_AVBL, true);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeFoupServiceStatus(GEMFoupStateEnum state)
        {
            try
            {
                Service?.FoupModule?.ChangeFoupServiceStatus(state);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetLock(bool isLock)
        {
            return Service.SetLock(isLock);
        }

        #region IFoupServiceCallback

        public bool IsFoupUsingByLoader()
        {
            bool isUsing;
            try
            {
                isUsing = this.LoaderController().IsFoupUsingByLoader(FoupModuleInfo.FoupNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isUsing;
        }

        public void RaiseWaferOutDetected()
        {
            this.LoaderController().RaiseWaferOutDetected(FoupModuleInfo.FoupNumber);
        }

        public FoupIOMappings GetFoupIOMap()
        {
            return Service.GetFoupIOMap();
        }
        public IFoupService GetFoupService()
        {
            return Service;
        }

        public EventCodeEnum CassetteTypeAvailable(CassetteTypeEnum cassetteType)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = Service.CassetteTypeAvailable(cassetteType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public CassetteTypeEnum GetCassetteType()
        {
            CassetteTypeEnum cassetteTypeEnum = CassetteTypeEnum.UNDEFINED;
            try
            {
                cassetteTypeEnum = Service.GetCassetteType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return cassetteTypeEnum;
        }

        public EventCodeEnum ValidationCassetteAvailable(out string msg, CassetteTypeEnum cassetteType = CassetteTypeEnum.UNDEFINED)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            msg = "";
            try
            {
                ret = Service.ValidationCassetteAvailable(out msg, cassetteType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        #endregion
    }
}
