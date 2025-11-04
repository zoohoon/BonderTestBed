using DBManagerModule;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AirBlow;
using ProberInterfaces.AirCooling;
using ProberInterfaces.AutoTilt;
using ProberInterfaces.CardChange;
using ProberInterfaces.Command;
using ProberInterfaces.DialogControl;
using ProberInterfaces.E84.ProberInterfaces;
using ProberInterfaces.Event;
using ProberInterfaces.Foup;
using ProberInterfaces.LoaderController;
using ProberInterfaces.MarkAlign;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.Pad;
using ProberInterfaces.PinAlign;
using ProberInterfaces.PMI;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.PolishWafer;
using ProberInterfaces.Service;
using ProberInterfaces.Temperature;
using ProberInterfaces.Template;
using ProberInterfaces.WaferTransfer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProberInterfaces.Focus;
using System.Runtime.CompilerServices;
using ProberInterfaces.ResultMap;
using ProberInterfaces.Device;
using ProberInterfaces.Communication.Tester;
using ProberInterfaces.Retest;
using ProberInterfaces.ODTP;
using ProberInterfaces.EnvControl;

namespace ProberCore
{
    [Serializable]
    public class ProberStation : INotifyPropertyChanged, IProberStation
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> PROPERTY

        public IFileManager FileManager
        {
            get
            {
                return this.FileManager();
            }
        }


        public IMotionManager MotionManager
        {
            get
            {
                return this.MotionManager();
            }
        }


        public IVisionManager VisionManager
        {
            get
            {
                return this.VisionManager();
            }
        }

        public ILightAdmin LightManager
        {
            get
            {
                return this.LightAdmin();
            }
        }

        public IAutoLightAdvisor AutoLight
        {
            get
            {
                return this.AutoLightAdvisor();
            }
        }

        public IModuleUpdater ModuleUpdater
        {
            get
            {
                return this.ModuleUpdater();
            }
        }

        public ICylinderManager CylinderManager
        {
            get
            {
                return this.CylinderManager();
            }
        }

        //public ITesterComDriver Driver
        //{
        //    get
        //    {
        //        return Container.Resolve<ITesterComDriver>();
        //    }
        //}

        public IGPIB GpibModule
        {
            get
            {
                return this.GPIB();
            }
        }

        public ITCPIP TCPIPModule
        {
            get
            {
                return this.TCPIPModule();
            }
        }

        public ITesterCommunicationManager TesterCommunicationManager
        {
            get
            {
                return this.TesterCommunicationManager();
            }
        }

        public ITempController TempManager
        {
            get
            {
                return this.TempController();
            }
        }

        public ICommandManager CommandManager
        {
            get
            {
                return this.CommandManager();
            }
        }

        public IEventManager EventManager
        {
            get
            {
                return this.EventManager();
            }
        }
        public IE84Module E84Module
        {
            get
            {
                return this.E84Module();
            }
        }
        //public IRFIDModule RFIDModule
        //{
        //    get
        //    {
        //        return this.RFIDModule();
        //    }
        //}

        public IGEMModule GemModule //code for gem
        {
            get
            {
                return this.GEMModule();
            }
        }

        public IStatisticsManager StatisticsManager
        {
            get
            {
                return this.StatisticsManager();
            }
        }

        public IEventExecutor EventExecutor
        {
            get
            {
                return this.EventExecutor();
            }
        }

        public IPMASManager PMASManager
        {
            get
            {
                return this.PMASManager();
            }
        }
        //public ICommandExecutor CommandExecutor
        //{
        //    get
        //    {
        //        return this.CommandExecutor();
        //    }
        //}

        public IInternal Internal
        {
            get
            {
                return this.Internal();
            }
        }

        public INotify Notify
        {
            get
            {
                return this.Notify();
            }
        }

        //public ISequenceEngine Sequencer
        //{
        //    get { return this.SequenceEngine(); }
        //}
        public ISequenceEngineManager SequenceEngineManager
        {
            get { return this.SequenceEngineManager(); }
        }
        //public IModule NC
        //{
        //    get { return this.NCModule(); }
        //}

        public IRetestModule RetestModule
        {
            get
            {
                return this.RetestModule();
            }
        }

        public IProbingModule Probing
        {
            get { return this.ProbingModule(); }
        }
        public IProbingSequenceModule ProbingSequenceModule
        {
            get { return this.ProbingSequenceModule(); }
        }
        //public IModule Soak
        //{
        //    get { return this.SoakModule(); }
        //}
        public IWaferAligner WaferAlign
        {
            get { return this.WaferAligner(); }
        }


        public IPinAligner PinAlign
        {
            get { return this.PinAligner(); }
        }

        public ILotOPModule LotOP
        {
            get { return this.LotOPModule(); }
        }

        public ILoaderOPModule LoaderOP
        {
            get
            {
                return this.LoaderOPModule();
            }
        }

        public ICoordinateManager CoordinateManager
        {
            get { return this.CoordinateManager(); }
        }
        public IIOManager IOState
        {
            get
            {
                return this.IOManager();
            }
        }

        public IMarkAligner MarkAligner
        {
            get
            {
                return this.MarkAligner();
            }
        }
        public IStageSupervisor _StageSuperVisor
        {
            get
            {
                return this.StageSupervisor();
            }
        }
        public IPadRegist PadReg
        {
            get
            {
                return this.PadRegist();
            }
        }
        public INeedleCleanModule NeedleCleaner
        {
            get
            {
                return this.NeedleCleaner();
            }
        }

        public IAutoTiltModule AutoTilt
        {
            get
            {
                return this.AutoTiltModule();
            }
        }


        public ICardChangeModule CardChangeModule
        {
            get
            {
                return this.CardChangeModule();
            }
        }

        public IManualContact ManualContact
        {
            get
            {
                return this.ManualContactModule();
            }
        }

        public ISoakingModule SoakingModule
        {
            get
            {
                return this.SoakingModule();
            }
        }

        public IPolishWaferModule PolishWaferModule
        {
            get
            {
                return this.PolishWaferModule();
            }
        }


        public ITempDisplayDialogService TempDisplayDialogService
        {
            get
            {
                return this.TempDisplayDialogService();
            }
        }


        public IViewModelManager ViewModelManager
        {
            get
            {
                return this.ViewModelManager();
            }

        }
        //public IFoupController FoupController
        //{
        //    get
        //    {
        //        return this.FoupController();
        //    }
        //}

        public ILoaderController LoaderController
        {
            get
            {
                return this.LoaderController();
            }
        }

        public IFoupOpModule FoupOp => this.FoupOpModule();


        public IDisplayPortDialog DisplayPortDialog
        {
            get
            {
                return this.DisplayPortDialog();
            }
        }


        public IInspection InspectionModule
        {
            get
            {
                return this.InspectionModule();
            }
        }


        public IParamManager ParamManager
        {
            get
            {
                return this.ParamManager();
            }
        }
        public IAirBlowChuckCleaningModule AirBlowChuckCleaningModule
        {
            get
            {
                return this.AirBlowChuckCleaningModule();
            }
        }
        public IAirBlowWaferCleaningModule AirBlowWaferCleaningModule
        {
            get
            {
                return this.AirBlowWaferCleaningModule();
            }
        }
        public IAirBlowTempControlModule AirBlowTempControlModule
        {
            get
            {
                return this.AirBlowTempControlModule();
            }
        }
        public IAirCoolingModule AirCoolingModule
        {
            get
            {
                return this.AirCoolingModule();
            }
        }

        public ILampManager LampManager
        {
            get
            {
                return this.LampManager();
            }
        }

        public IWaferTransferModule WaferTransferModule => this.WaferTransferModule();


        public IRemoteServiceModule RemoteServiceModule
        {
            get
            {
                return this.RemoteServiceModule();
            }
        }


        //public IWizardManager WizardManager
        //{
        //    get
        //    {
        //        return this.WizardManager();
        //    }
        //}

        public IMonitoringManager MonitoringManager
        {
            get
            {
                return this.MonitoringManager();
            }
        }

        public IPnpManager PnpManager
        {
            get
            {
                return this.PnPManager();
            }
        }

        public ICameraChannelAdmin CameraChannelManaer
        {
            get
            {
                return this.CameraChannel();
            }
        }

        public ITemplateManager TemplateManager
        {
            get
            {
                return this.TemplateManager();
            }
        }

        public INeedleCleanModule NeedleCleanModule
        {
            get
            {
                return this.NeedleCleaner();
            }
        }

        public IPMIModule PMIModule
        {
            get
            {
                return this.PMIModule();
            }
        }

        public IFocusManager FocusManager
        {
            get
            {
                return this.FocusManager();
            }
        }

        public IGPCardAligner GPCardAligner
        {
            get
            {
                return this.GPCardAligner();
            }
        }

        public IResultMapManager ResultMapManager
        {
            get
            {
                return this.ResultMapManager();
            }
        }

        public IODTPManager ODTPManager
        {
            get
            {
                return this.ODTPManager();
            }
        }

        public INotifyManager NotifyManager
        {
            get
            {
                return this.NotifyManager();
            }
        }

        public IDeviceModule DeviceModule
        {
            get
            {
                return this.DeviceModule();
            }
        }

        public IEnvModule EnvModule
        {
            get
            {
                return this.EnvModule();
            }
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private int _MaskingLevel;
        public int MaskingLevel
        {
            get { return _MaskingLevel; }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    DBManager.Open();
                    var _DevParamModules = new List<IHasDevParameterizable>();
                    var _SysParamModules = new List<IHasSysParameterizable>();

                    _DevParamModules = ReflectionUtil.CrawlingDerrivedParamList<IHasDevParameterizable>(this);
                    _SysParamModules = ReflectionUtil.CrawlingDerrivedParamList<IHasSysParameterizable>(this);

                    //LoadParameter done when program init
                    this.LotOPModule().LotInfo.SetDevLoadResult(true);

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
                throw err;
            }

            return retval;
        }

        public void ChangeScreenToPnp(object setupstep)
        {
        }
        public void ChangeScreenLotOP()
        {
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}

