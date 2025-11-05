using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autofac;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LogModule;
using RelayCommandBase;

namespace LoaderCore.ProxyModules
{
    public class RemoteIOManagerProxy : IFactoryModule, IIOManager, INotifyPropertyChanged
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
        }
        public IIOManagerProxy IOProxy { get; set; }
        public EventCodeEnum InitIOStates()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitService()
        {
            return;
        }

        public RemoteIOManagerProxy()
        {
            IOProxy = this.GetContainer().Resolve<IIOManagerProxy>();
            IOServ = new IOServiceProxy(IOProxy);
        }
        public IIOService IOServ { get; set; }
        public IIOMappingsParameter IO => IOProxy.IOMappings;
        public bool Initialized { get => true; set => throw new NotImplementedException(); }
    }
    public class IOServiceProxy : IFactoryModule, IIOService
    {
        public IIOManagerProxy IOProxy { get; private set; }

        public ObservableCollection<InputChannel> Inputs { get; set; }
        public ObservableCollection<OutputChannel> Outputs { get; set; }
        public ObservableCollection<AnalogInputChannel> AnalogInputs { get; set; }
        public bool Initialized { get; private set; }

        public IOServiceProxy()
        {
            IOProxy = this.GetContainer().Resolve<IIOManagerProxy>();
            Inputs = new ObservableCollection<InputChannel>();
            Outputs = new ObservableCollection<OutputChannel>();
            AnalogInputs = new ObservableCollection<AnalogInputChannel>();
            Initialized = true;
        }

        public IOServiceProxy(IIOManagerProxy manager)
        {
            IOProxy = manager;
            Inputs = new ObservableCollection<InputChannel>();
            Outputs = new ObservableCollection<OutputChannel>();
            Initialized = true;
        }
        public List<IIOBase> IOList { get; private set; }

        public int DeInitIO()
        {
            return 1;
        }
        public void DeInitModule()
        {
        }
        public int InitIOService()
        {
            IOList = new List<IIOBase>();
            return 1;
        }

        public EventCodeEnum InitModule(int ctrlNum, string devConfigParam, string ecatIOConfigParam)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }
        private RelayCommand<IOPortDescripter<bool>> _BitOutputEnableCommand;

        public ICommand BitOutputEnableCommand
        {
            get
            {
                if (null == _BitOutputEnableCommand) _BitOutputEnableCommand
                        = new RelayCommand<IOPortDescripter<bool>>(BitOutputEnable);
                return _BitOutputEnableCommand;
            }
        }
        private RelayCommand<IOPortDescripter<bool>> _BitOutputDisableCommand;
        public ICommand BitOutputDisableCommand
        {
            get
            {
                if (null == _BitOutputDisableCommand) _BitOutputDisableCommand
                        = new RelayCommand<IOPortDescripter<bool>>(BitOutputDisable);
                return _BitOutputDisableCommand;
            }
        }

        public void BitOutputEnable(IOPortDescripter<bool> ioport)
        {
            LoggerManager.Debug($"BitOutputEnable(): Not Available method for IO service proxy.");
        }
        public void BitOutputDisable(IOPortDescripter<bool> ioport)
        {
            LoggerManager.Debug($"BitOutputDisable(): Not Available method for IO service proxy.");

        }
        public int MonitorForIO(IOPortDescripter<bool> io, bool level, long maintainTime = 0, long timeout = 0, bool writelog = true)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            int retVal = -1;
            errorCode = IOProxy.MonitorForIO(io, level, maintainTime, timeout, writelog);
            if (errorCode == EventCodeEnum.NONE)
            {
                retVal = 1;
            }
            else if (errorCode == EventCodeEnum.IO_TIMEOUT_ERROR)
            {
                retVal = -2;
            }
            else
            {
                retVal = -1;
            }
            return retVal;
        }
        public IORet ReadBit(IOPortDescripter<bool> io, out bool value)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            IORet retVal = IORet.UNKNOWN;
            errorCode = IOProxy.ReadIO(io, out value);
            if (errorCode == EventCodeEnum.NONE)
            {
                retVal = IORet.NO_ERR;
            }
            else if (errorCode == EventCodeEnum.IO_TIMEOUT_ERROR)
            {
                retVal = IORet.ErrorDeviceIoTimeOut;
            }
            else
            {
                retVal = IORet.ErrorUndefined;
            }
            return retVal;
        }
        public int WaitForIO(IOPortDescripter<bool> io, bool level, long timeout = 0)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            int retVal = -1;
            errorCode = IOProxy.WaitForIO(io, level, timeout);
            if (errorCode == EventCodeEnum.NONE)
            {
                retVal = 1;
            }
            else if (errorCode == EventCodeEnum.IO_TIMEOUT_ERROR)
            {
                retVal = -2;
            }
            else
            {
                retVal = -1;
            }
            return retVal;
        }

        public IORet WriteBit(IOPortDescripter<bool> io, bool value)
        {
            IORet retVal = IORet.ERROR;
           var errorCode = IOProxy.WriteIO(io, value);
            if (errorCode == EventCodeEnum.NONE)
            {
                retVal = IORet.NO_ERR;
            }
            return retVal;
        }
    }
}
