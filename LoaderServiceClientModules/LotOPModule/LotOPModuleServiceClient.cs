using System;
using System.Collections.Generic;

namespace LoaderServiceClientModules
{
    using Autofac;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using LogModule;
    using ProberInterfaces.Proxies;
    using ProberInterfaces.State;
    using System.Windows;
    using SubstrateObjects;

    public class LotOPModuleServiceClient : ILotOPModule, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region // Property

        //.. Lot Screen Binding Properties
        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }


        private object _ViewTarget;
        public object ViewTarget
        {
            get { return _ViewTarget; }
            set
            {
                if (value != _ViewTarget)
                {

                    PreViewTarget = _ViewTarget;
                    _ViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }



        private Visibility _MiniVisible;
        public Visibility MiniVisible
        {
            get { return _MiniVisible; }
            set
            {
                if (value != _MiniVisible)
                {
                    _MiniVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        public object PreViewTarget { get; set; }


        private Visibility _SwitchVisiability = Visibility.Hidden;
        public Visibility SwitchVisiability
        {
            get { return _SwitchVisiability; }
            set
            {
                if (value != _SwitchVisiability)
                {
                    _SwitchVisiability = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        public int PauseRequest(object caller)
        {
            throw new NotImplementedException();
        }

        public int ResumeRequest(object caller)
        {
            throw new NotImplementedException();
        }

        public int StartRequest(object caller)
        {
            throw new NotImplementedException();
        }

        public void InitLotScreen()
        {
        }

        public void VisionScreenToLotScreen()
        {
        }

        public void MapScreenToLotScreen()
        {
        }

        public void NCToLotScreen()
        {
            throw new NotImplementedException();
        }

        public void LoaderScreenToLotScreen()
        {
            throw new NotImplementedException();
        }

        public void ChangePreMainViewTarget()
        {
            throw new NotImplementedException();
        }

        public void HiddenLoaderScreenToLotScreen()
        {
            throw new NotImplementedException();
        }

        public void SetLotViewDisplayChannel()
        {
            throw new NotImplementedException();
        }

        public void ViewSwip()
        {
            throw new NotImplementedException();
        }

        public void ChangeMainViewUserTarget(object target)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveAppItems()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitData()
        {
            throw new NotImplementedException();
        }

        public void ViewTargetUpdate()
        {
            throw new NotImplementedException();
        }

        public void UpdateWafer(IWaferObject waferObject)
        {
            throw new NotImplementedException();
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Execute()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            throw new NotImplementedException();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {

        }

        public EventCodeEnum InitModule()
        {
            //DisplayPort = new DisplayPort() { GUID = new Guid("31E6CCB2-5B75-91D0-22DC-4F57DD485F00") };
            //(this.ViewModelManager() as ILoaderViewModelManager).RegisteDisplayPort(DisplayPort);
            return EventCodeEnum.NONE;
        }

        public bool IsLotReady(out string msg)
        {
            bool retval = true;
            try
            {
                msg = "";
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public void SetDeviceName(string devicename)
        {
            ILotOPModuleProxy proxy = LoaderCommunicationManager.GetProxy<ILotOPModuleProxy>();

            if (proxy != null)
            {
                proxy.SetDeviceName(devicename);
            }
        }
        #endregion


        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }
        public EnumCommunicationState CommunicationState
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    return EnumCommunicationState.CONNECTED;
                }
                else
                {
                    return EnumCommunicationState.UNAVAILABLE;
                }
            }
            set { }
        }


        public IParam AppItems_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ILotInfo LotInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISystemInfo SystemInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDeviceInfo DeviceInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ReasonOfStopOption ReasonOfStopOption { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ILotDeviceParam LotDeviceParam => throw new NotImplementedException();

        public bool IsLastWafer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsNeedLotEnd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool ModuleStopFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool LotStartFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool LotEndFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<IStateModule> RunList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IInnerState InnerState => throw new NotImplementedException();

        public ReasonOfError ReasonOfError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState => throw new NotImplementedException();

        public ObservableCollection<TransitionInfo> TransitionInfo => throw new NotImplementedException();

        public EnumModuleForcedState ForcedDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ErrorEndStateEnum ErrorEndState { get; set; }
        public EventCodeInfo PauseSourceEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public LotOPStateEnum LotStateEnum => throw new NotImplementedException();
        public int UnloadFoupNumber { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool TransferReservationAboutPolishWafer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool GetErrorEndFlag() { return false; }
        public void SetErrorEndFalg(bool flag) { }
        public void SetErrorState()
        {
        }

        public int GetLotPauseTimeoutAlarm() { return 0; }
        public void SetLotPauseTimeoutAlarm(int time) { }

        public bool IsLotAbortedByUser()
        {
            throw new NotImplementedException();
        }

        public void UpdateLotName(string lotname)
        {
            throw new NotImplementedException();
        }

        public void UpdateWaferID(string id)
        {
            throw new NotImplementedException();
        }
        public bool IsCanPerformLotStart()
        {
            return false;
        }

        public bool IsCanPerformLotEnd(int foupidx, string lotID, string cstHashCode, bool isCheckHashCode)
        {
            return false;
        }
        public void ValidateCancelLot(bool iscellend, int foupNumber, string lotID, string cstHashCode)
        {
            throw new NotImplementedException();
        }
    }
}
