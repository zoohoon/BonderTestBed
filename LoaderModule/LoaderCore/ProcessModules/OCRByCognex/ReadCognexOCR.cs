using System;
using LoaderBase;

namespace LoaderCore
{
    using LoaderParameters;
    using LogModule;
    using ProberErrorCode;
    using ReadCognexOCRStates;

    public class ReadCognexOCR : ILoaderProcessModule, IOCRRemotableProcessModule
    {
        public ReadCognexOCRState StateObj { get; set; }

        public OCRProcParam Param { get; private set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {
                this.Container = container;

                this.Param = param as OCRProcParam;
                this.StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(ILoaderProcessParam param)
        {
            bool retval = false;

            try
            {
                var mypa = param as OCRProcParam;

                retval =
                    mypa != null &&
                    mypa.UseOCR is ICognexOCRModule &&
                    mypa.UseARM is IARMModule;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public LoaderProcStateEnum State => StateObj.State;

        public ReasonOfSuspendedEnum ReasonOfSuspended => StateObj.ReasonOfSuspended;

        public void Execute()
        {
            try
            {
                StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Awake()
        {
            try
            {
                StateObj.Resume();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SelfRecovery()
        {
            try
            {
                StateObj.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            imgBuf = new byte[0];
            w = 0;
            h = 0;

            try
            {
                retval = StateObj.GetOCRImage(out imgBuf, out w, out h);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ChangeLight(int channelMapIdx, ushort intensity)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.ChangeLight(channelMapIdx, intensity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetOcrID(string ocrID)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.SetOcrID(ocrID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum OCRRemoteEnd()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.OCRRemoteEnd();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.GetOCRState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum OCRRetry()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.OCRRetry();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum OCRFail()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.OCRFail();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum OCRAbort()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = StateObj.OCRAbort();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        internal ReadCognexOCRData Data { get; set; }
    }

    public class ReadCognexOCRData
    {
        public string OcrTextByUserInput { get; set; }
    }
}
