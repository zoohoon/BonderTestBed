using Autofac;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using ProberInterfaces.Command;
using ProberInterfaces.Enum;
using RequestInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoaderServiceClientModules.TCPIPModule
{
    public class TCPIPModuleServiceClient : ITCPIP, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public EnumTCPIPEnable GetTCPIPOnOff()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WriteString(string command)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WriteSTB(string command)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ReInitializeAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                if (proxy != null)
                {
                    retVal = proxy.ReInitializeAndConnect();
                }

                return retVal;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public EventCodeEnum CheckAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                if (proxy != null)
                {
                    retVal = proxy.ReInitializeAndConnect();
                }

                return retVal;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
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

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }

        public string GetModuleMessage()
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
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

        public EventCodeEnum CreateTesterComDriver()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            throw new NotImplementedException();
        }

        public DRDWConnectorBase GetDRDWConnector(int id)
        {
            throw new NotImplementedException();
        }

        public DWDataBase GetDWDataBase(string argument)
        {
            throw new NotImplementedException();
        }

        public bool GetTesterAvailable()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum IsControlAvailableState(out string errorlog)
        {
            errorlog = "";
            return EventCodeEnum.UNDEFINED;
        }

        public List<CommunicationRequestSet> RequestSetList => throw new NotImplementedException();

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

        public bool Initialized => throw new NotImplementedException();

        public EnumCommunicationState CommunicationState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ITesterComDriver TesterComDriver { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IParam TCPIPSysParam_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CardchangeTempInfo CardchangeTempInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
