using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using LoaderParameters;
using System.Runtime.CompilerServices;
using LoaderBase.AttachModules.ModuleInterfaces;
using LogModule;
using System.Windows;
using ProberInterfaces;
using ProberInterfaces.Enum;

namespace LoaderCore.ReadCognexOCRStates
{
    public abstract class ReadCognexOCRState : LoaderProcStateBase
    {
        public ReadCognexOCR Module { get; set; }

        public ReadCognexOCRState(ReadCognexOCR context)
        {
            this.Module = context;
        }

        protected void StateTransition(ReadCognexOCRState stateObj)
        {
            Module.StateObj = stateObj;
            LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state tranition : {State}");
        }

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected ICognexOCRModule OCR => Module.Param.UseOCR as ICognexOCRModule;
        protected ICognexProcessManager CognexProcessManager => Module.Container.Resolve<ICognexProcessManager>();

        protected IARMModule UseARM => Module.Param.UseARM;

        protected bool IsManualMode()
        {
            bool retval = false;

            try
            {
                retval = Module.Param.DestPos == OCR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region => OCR Remote Methods
        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

                retval = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            imgBuf = new byte[0];
            w = 0;
            h = 0;

            try
            {
                String firstCognexHost = CognexProcessManager.CognexProcSysParam.GetIPOrNull(CognexProcessManager.CognexProcDevParam.CognexHostList[0].ModuleName);

                CognexProcessManager.DO_AcquireConfig(firstCognexHost);
                if (CognexProcessManager.DO_RI(firstCognexHost) == false)
                    return EventCodeEnum.UNDEFINED;

                if (CognexProcessManager.CognexCommandManager.CognexRICommand.Status != "1")
                    return EventCodeEnum.UNDEFINED;

                imgBuf = CognexProcessManager.CognexCommandManager.CognexRICommand.ByteData;
                Size size = CognexProcessManager.CognexCommandManager.CognexRICommand.GetImageSize();
                w = Convert.ToInt32(size.Width);
                h = Convert.ToInt32(size.Height);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //imgBuf = new byte[0];
            //w = 0;
            //h = 0;

            //try
            //{
            //    retval = RaiseInvalidState();
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}

            return retval;
        }

        public virtual EventCodeEnum ChangeLight(int channelMapIdx, ushort intensity)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                String firstCognexHost = CognexProcessManager.CognexProcSysParam.GetIPOrNull(CognexProcessManager.CognexProcDevParam.CognexHostList[0].ModuleName);
                CognexProcessManager.DO_SetConfigLightMode(firstCognexHost, channelMapIdx.ToString());

                CognexProcessManager.DO_SetConfigLightPower(firstCognexHost, intensity.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //try
            //{
            //    retval = RaiseInvalidState();
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}

            return retval;
        }

        public virtual EventCodeEnum SetOcrID(string ocrID)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //retval = RaiseInvalidState();
                Module.Data.OcrTextByUserInput = ocrID;
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum OCRRemoteEnd()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //retval = RaiseInvalidState();
                if (string.IsNullOrEmpty(Module.Data.OcrTextByUserInput))
                {
                    StateTransition(new ReadingState(Module));
                }
                else
                {
                    if (IsManualMode() == false)
                    {
                        //TODO : Clear HW Device
                    }

                    //TODO : Clear HW Device

                    Module.Param.TransferObject.SetOCRState(Module.Data.OcrTextByUserInput, 100, OCRReadStateEnum.DONE);
                    if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, Module.Data.OcrTextByUserInput);
                    }
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new DoneState(Module));
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum OCRRetry()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum OCRFail()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum OCRAbort()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        

        #endregion
    }

    public class IdleState : ReadCognexOCRState
    {
        public IdleState(ReadCognexOCR context) : base(context) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                //TODO :
                Module.Data = new ReadCognexOCRData();
                StateTransition(new ReadingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class ReadingState : ReadCognexOCRState
    {
        public ReadingState(ReadCognexOCR context) : base(context) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            #region ==> Test Code
            if (Loader.LoaderOption.OptionFlag)
            {
                //String firstCognexHost = CognexProcessManager.CognexProcSysParam.CognexHostList[0].IP.Value;
                //Stopwatch stw = new Stopwatch();
                //stw.Start();

                //String ocrString = CognexProcessManager.GetOCRString(firstCognexHost);
                //String ocrScoreStr = CognexProcessManager.GetOCRScore(firstCognexHost);

                //String ellapseTime = stw.ElapsedMilliseconds.ToString();
                //stw.Stop();

                //Loader.BroadcastLoaderInfo();
                //StateTransition(new DoneState(Module));
                //double ocrScore;
                //double.TryParse(ocrScoreStr, out ocrScore);
                //UseARM.Holder.TransferObject.SetOCRState(ocrString, ocrScore, OCRReadStateEnum.DONE);

                ////string filePath = @"C:\ProberSystem\OCRTEST.txt";
                ////// Create a file to write to.
                ////using (StreamWriter sw = new StreamWriter(filePath, append:true))
                ////{
                ////    sw.WriteLine($"{DateTime.Now.ToString()}    String : {ocrString}, Score : {ocrScoreStr }, Ellapse Time : {ellapseTime}");
                ////}

                //if (ocrScore < 300)
                //{
                //    CognexProcessManager.SaveOCRImage(firstCognexHost);
                //}

                //return;
            }
            #endregion

            //EnumCognexMode cognexMode = CognexProcessManager.CognexProcDevParam.Mode.Value;

            //LoggerManager.Debug($"Read Cognex OCR");
            //LoggerManager.Debug($"[LOADER] {Module.GetType().Name} Mode={cognexMode}");

            //if (cognexMode == EnumCognexMode.MANUAL)
            if (Module.Param.TransferObject.OCRMode.Value == OCRModeEnum.MANUAL)
            {
                StateTransition(new WaitForOCRRemoteState(Module));//==> Suspend 상태로 빠짐
                return;
            }
            if (Module.Param.TransferObject.OCRMode.Value == OCRModeEnum.NONE)
            {
                UseARM.Holder.TransferObject.SetOCRState(String.Empty, 0, OCRReadStateEnum.DONE);
                return;
            }

            //==> SEMIAUTO, AUTO
            String firstCognexHostIP = CognexProcessManager.CognexProcSysParam.GetIPOrNull(CognexProcessManager.CognexProcDevParam.CognexHostList[0].ModuleName);
            String firstCognexHostConfig = CognexProcessManager.CognexProcDevParam.CognexHostList[0].ConfigName;
            CognexConfig config = CognexProcessManager.CognexProcDevParam.ConfigList.FirstOrDefault(item => item.Name == firstCognexHostConfig);

            if (config == null)
            {
                StateTransition(new WaitForOCRRemoteState(Module));
                return;
            }
            String ocrString = String.Empty;
            String ocrScoreStr = String.Empty;
            switch (config.RetryOption)
            {
                case "1":
                case "2":
                    //==> Adjust 수행
                    ocrString = CognexProcessManager.GetOCRString(firstCognexHostIP, true, out ocrScoreStr);
                    break;
                case "0":
                default:
                    //==> Adjust 안함
                    ocrString = CognexProcessManager.GetOCRString(firstCognexHostIP, false, out ocrScoreStr);
                    break;
            }


            double ocrScore;
            double.TryParse(ocrScoreStr, out ocrScore);

            LoggerManager.Debug($"[OCR] String : {ocrString}, Score : {ocrScoreStr}");


            int ocrScoreCutline = config.OCRCutLineScore;

            LoggerManager.Debug($"[OCR] Cut line : {ocrScoreCutline}");

            if (ocrScore < ocrScoreCutline)//==> Fail
            {
                CognexProcessManager.SaveOCRImage(firstCognexHostIP);
                StateTransition(new WaitForOCRRemoteState(Module));
                return;
            }
            //==> Success
            UseARM.Holder.TransferObject.SetOCRState(ocrString, ocrScore, OCRReadStateEnum.DONE);
            if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
            {
                Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, ocrString);
            }
            Loader.BroadcastLoaderInfo();
            StateTransition(new DoneState(Module));


            //String firstCognexHostIP = CognexProcessManager.CognexProcSysParam.CognexHostList[0].IP.Value;

            //String ocr;
            //double ocrScore;
            //EventCodeEnum result = CognexProcessManager.GetOCRString(0, out ocr, out ocrScore);

            //if (result == EventCodeEnum.COGNEX_IS_MANUAL)
            //{
            //    StateTransition(new WaitForOCRRemoteState(Module));//==> Suspend 상태로 빠짐
            //    return;
            //}
            //else if (result == EventCodeEnum.COGNEX_CONFIG_INVALID)
            //{
            //    StateTransition(new WaitForOCRRemoteState(Module));
            //    return;
            //}
            //else if (result == EventCodeEnum.COGNEX_SCORE_IS_UNDER_CUTLINE)
            //{
            //    CognexProcessManager.SaveOCRImage(firstCognexHostIP);
            //    StateTransition(new WaitForOCRRemoteState(Module));
            //    return;
            //}

            ////==> Success
            //UseARM.Holder.TransferObject.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
            //if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
            //{
            //    Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, ocr);
            //}
            //Loader.BroadcastLoaderInfo();
            //StateTransition(new DoneState(Module));
        }

        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.COGNEX_READINGSTATE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class AnalyzeState : ReadCognexOCRState
    {
        public AnalyzeState(ReadCognexOCR context) : base(context) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
        }
        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class WaitForOCRRemoteState : ReadCognexOCRState
    {
        public WaitForOCRRemoteState(ReadCognexOCR context) : base(context) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.OCR_FAILED;

        public override void Execute() { /*NoWORKS*/ }

        public override void Resume()
        {
            try
            {
                StateTransition(new RemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class RemotingState : ReadCognexOCRState
    {
        public RemotingState(ReadCognexOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;
        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.OCR_REMOTING;
        public override void Execute() { /*NoWORKS*/ }
        public override void Resume() { /*NoWORKS*/ }

        public override EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //No WORKS.
            imgBuf = new byte[0];
            w = 0;
            h = 0;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum ChangeLight(int lightChannel, ushort intensity)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //No WORKS.

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum SetOcrID(string ocrID)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Data.OcrTextByUserInput = ocrID;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum OCRRemoteEnd()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (string.IsNullOrEmpty(Module.Data.OcrTextByUserInput))
                {
                    StateTransition(new ReadingState(Module));
                }
                else
                {
                    if (IsManualMode() == false)
                    {
                        //TODO : Clear HW Device
                    }

                    //TODO : Clear HW Device

                    Module.Param.TransferObject.SetOCRState(Module.Data.OcrTextByUserInput, 100, OCRReadStateEnum.DONE);
                    if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, Module.Data.OcrTextByUserInput);
                    }
                    //==> Done 상태로 만들고 현재 LoaderInfo 갱신
                    StateTransition(new DoneState(Module));
                    Loader.BroadcastLoaderInfo();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum OCRAbort()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Param.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                Module.Param.TransferObject.WaferState = EnumWaferState.SKIPPED;

                StateTransition(new AbortState(Module));
                Loader.BroadcastLoaderInfo();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum OCRRetry()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StateTransition(new ReadingState(Module));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum OCRFail()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StateTransition(new SystemErrorState(Module));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class AbortState : ReadCognexOCRState
    {
        public AbortState(ReadCognexOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;
        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.OCR_ABORT;


        public override void Execute() { /*NoWORKS*/ }
        public override void Resume() { /*NoWORKS*/ }
        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.COGNEX_ABORT;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class DoneState : ReadCognexOCRState
    {
        public DoneState(ReadCognexOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
        public override void Resume() { /*NoWORKS*/ }
        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.COGNEX_DONESTATE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class SystemErrorState : ReadCognexOCRState
    {
        public SystemErrorState(ReadCognexOCR module) : base(module) { }
        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;
        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.OCR_FAILED;
        public override void Execute() { /*NoWORKS*/ }
        public override void SelfRecovery() { /*NoWORKS*/ }
        public override EventCodeEnum GetOCRState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
