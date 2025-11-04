using System;
using System.Collections.Generic;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;

namespace LoaderCore
{
    using LogModule;
    using ProberErrorCode;
    using ReadSemicsOCRStates;

    public partial class ReadSemicsOCR : ILoaderProcessModule, IOCRRemotableProcessModule
    {
        public ReadSemicsOCRState StateObj { get; set; }

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

                retval = mypa != null &&
                mypa.UseOCR is ISemicsOCRModule &&
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
                retval = StateObj.GetOCRDoneState();
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
                retval = StateObj.GetOCRDoneState();
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
            throw new NotImplementedException();
        }

        internal ReadSemicsOCRData Data { get; set; }
    }


    public class ReadSemicsOCRData
    {
        public SemicsOCRDevice UseOCRDevice { get; set; }
        
        public List<ReadOCRResult> ReadOCRResults { get; set; }

        public bool EnableOcrLightRemoteMode { get; set; }

        public string OcrTextByUserInput { get; set; }
    }
}
