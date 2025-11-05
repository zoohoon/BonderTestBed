using System;

using ProberErrorCode;
using LoaderBase;
using ProberInterfaces;
using Autofac;
using LogModule;

namespace LoaderCore
{
    public class OCRRemoteService : IOCRRemoteService
    {
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL1;

        public IContainer Container { get; set; }

        public IOCRRemotableProcessModule ProcModule { get; set; }

        public EventCodeEnum InitModule(IContainer container)
        {
            try
            {
                this.Container = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {

        }

        public void Activate(IOCRRemotableProcessModule procModule)
        {
            try
            {
                this.ProcModule = procModule;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void Deactivate()
        {
            try
            {
                this.ProcModule = null;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            imgBuf = null;
            w = 0;
            h = 0;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.GetOCRImage(out imgBuf, out w, out h);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ChangeLight(int channelMapIdx, ushort intensity)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.ChangeLight(channelMapIdx, intensity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetOcrID(string ocrID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.SetOcrID(ocrID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OCRRemoteEnd()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.OCRRemoteEnd();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum GetOCRState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.GetOCRState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OCRRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.OCRRetry();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OCRFail()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.OCRFail();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OCRAbort()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ProcModule == null)
                return retVal;
            try
            {
                retVal = ProcModule.OCRAbort();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
