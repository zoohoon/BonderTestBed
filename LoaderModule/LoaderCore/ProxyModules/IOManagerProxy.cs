using System;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LoaderParameters;
using LogModule;

namespace LoaderCore
{
    public class IOManagerProxy : IIOManagerProxy
    {
        public bool Initialized { get; set; } = false;

        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;

        public IContainer Container { get; set; }

        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();

        public IIOManager IOStates { get; set; }
        public IIOMappingsParameter IOMappings => IOStates.IO;

        private IGPLoader _Remote;
        public IGPLoader Remote { get => GetRemote(); }
        public IGPLoader GetRemote()
        {
            if (_Remote == null)
                GetModule<IGPLoader>(out _Remote);

            return _Remote;
        }

        private void GetModule<T>(out T module)
        {
            if (Container != null)
            {
                if (Container.IsRegistered<T>() == true)
                {
                    module = Container.Resolve<T>();
                }
                else
                {
                    module = default(T);
                }
            }
            else
            {
                module = default(T);
            }
        }

        public void DeInitModule()
        {
            try
            {
                if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                {
                    //No WORKS.
                }
                else
                {
                    IOStates.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule(IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.Container = container;

                    if (Loader.ServiceType == LoaderServiceTypeEnum.DynamicLinking)
                    {
                        IOStates = Loader.StageContainer.Resolve<IIOManager>();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

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
            }

            return retval;
        }

        //public bool IsEmulMode(IOPortDescripter )
        //{
        //    IOStates.IOServ.IOList[]
        //}
        
        public IOPortDescripter<bool> GetIOPortDescripter(string ioName)
        {
            try
            {
                var inputs = IOStates.IO.Inputs;
                var inputType = inputs.GetType();
                var prop = inputType.GetProperty(ioName);
                if (prop != null)
                {
                    var iodesc = prop.GetValue(inputs) as IOPortDescripter<bool>;
                    return iodesc;
                }

                var outputs = IOStates.IO.Outputs;
                var outputType = outputs.GetType();
                prop = outputType.GetProperty(ioName);
                if (prop != null)
                {
                    var iodesc = prop.GetValue(outputs) as IOPortDescripter<bool>;
                    return iodesc;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }
        public EventCodeEnum ReadIO(IOPortDescripter<bool> iodesc, out bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var ioRet = IOStates.IOServ.ReadBit(iodesc, out value);

            try
            {
                retVal = ioRet == IORet.NO_ERR ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaitForIO(IOPortDescripter<bool> iodesc, bool value, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ioRet = (IORet)IOStates.IOServ.WaitForIO(iodesc, value, timeout);
                retVal = ioRet == IORet.NO_ERR ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WriteIO(IOPortDescripter<bool> iodesc, bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ioRet = IOStates.IOServ.WriteBit(iodesc, value);
                retVal = ioRet == IORet.NO_ERR ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum MonitorForIO(IOPortDescripter<bool> iodesc, bool value, long maintainTime = 500, long timeout = 1000, bool writelog = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var ioRet = (IORet)IOStates.IOServ.MonitorForIO(iodesc, value, maintainTime, timeout, writelog);
                retVal = ioRet == IORet.NO_ERR ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            throw new NotImplementedException();
        }
    }

}
