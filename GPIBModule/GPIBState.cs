using System;
using System.Text;

namespace GPIBModule
{
    using LogModule;
    using MetroDialogInterfaces;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.BinData;
    using ProberInterfaces.State;
    using RequestInterface;
    using System.Threading;

    public abstract class GPIBState : IFactoryModule, IInnerState
    {
        public abstract GPIBStateEnum GetState();
        public abstract ModuleStateEnum GetModuleState();
        public abstract EventCodeEnum Execute();

        public virtual EventCodeEnum DisConnect()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual string Read()
        {
            return "Not Read - DisConnected State";
        }
        public virtual EventCodeEnum Connect()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum Pause()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum End()
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum Abort()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum ClearState()
        {
            // TODO:
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum Resume()
        {
            return EventCodeEnum.NONE;
        }
        public virtual EventCodeEnum WriteSTB(int? command)
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual EventCodeEnum WriteString(string query_command)
        {
            return EventCodeEnum.UNDEFINED;
        }
        public virtual BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            return null;
        }
    }

    public abstract class GPIBStateBase : GPIBState
    {
        private GPIB _Module;
        public GPIB Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public GPIBStateBase(GPIB module)
        {
            Module = module;
        }

        private bool _ActFlag;
        public bool ActFlag
        {
            get { return _ActFlag; }
            set { _ActFlag = value; }
        }

        private EventCodeEnum _ErrCode;
        public EventCodeEnum ErrCode
        {
            get { return _ErrCode; }
            set { _ErrCode = value; }
        }
    }

    public class GPIBNotConnectedState : GPIBStateBase
    {
        public GPIBNotConnectedState(GPIB module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override GPIBStateEnum GetState()
        {
            return GPIBStateEnum.NOTCONNECTED;
        }

        public override EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //int? connectResult = null;

                //connectResult = Module.GpibManager?.Connect(Module.GPIBSysParam.BoardIndex.Value);

                //if (connectResult == 0)
                //{
                //    Module.InnerStateTransition(new GPIBConnectedState(Module));
                //    retVal = EventCodeEnum.NONE;
                //}

                retVal = Module.TesterDriver_Connect(Module.GPIBSysParam.BoardIndex.Value);

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.GpibCommlog(Convert.ToInt64(GpibStatusFlags.NONE), Convert.ToInt32(GpibCommunicationActionType.CONN), "GPIB connection succeeded");

                    Module.InnerStateTransition(new GPIBConnectedState(Module));
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.GpibCommlog(Convert.ToInt64(GpibStatusFlags.NONE), Convert.ToInt32(GpibCommunicationActionType.CONN), "GPIB connection failed");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public override EventCodeEnum Resume()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[{Module.GetType().Name}] | [{this.GetType().Name}.Resume()] : Resume GPIB connection.");
                Connect();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }

    public class GPIBConnectedState : GPIBStateBase
    {
        public GPIBConnectedState(GPIB module) : base(module)
        {
        }

        public override GPIBStateEnum GetState()
        {
            return GPIBStateEnum.RUNNING;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public SemaphoreSlim semaphore
        {
            get { return _semaphore; }
            set { _semaphore = value; }
        }

        object executeObj = new object();
        bool endDetected = false;
        public override EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //string str = null;
            bool recvCondition = false;
            int ibsta = 0;

            try
            {
                //Module.GpibManager.Ibsta = 0;

                Module.Ibsta = 0;

                lock (executeObj)
                {
                    semaphore.Wait(timeout: System.TimeSpan.FromSeconds(1));

                    try
                    {
                        ibsta = Module.TesterDriver_GetState();
                        recvCondition = RecvContition(ibsta);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                    finally
                    {
                        semaphore.Release();
                    }

                    if (recvCondition == true)
                    {
                        LoggerManager.Debug($"RecvContition(): {Convert.ToString(ibsta, 2)}");
                        endDetected = false;
                        retVal = OnRecvProcessing();
                    }
                    else
                    {
                        if ((ibsta & (int)GpibStatusFlags.END) == (int)GpibStatusFlags.END)
                        {
                            if(endDetected == false)
                            {
                                LoggerManager.Debug($"RecvContition(): {Convert.ToString(ibsta, 2)}");
                                endDetected = true;
                            }
                            
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                LoggerManager.Debug($"[GPIB],[DisConnect],Try to Disconnect.(Exception).");
                this.DisConnect();
            }

            return retVal;

            EventCodeEnum OnRecvProcessing()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                string recv_data = null;
                string commandName = "";
                string argu = "";
                CommunicationRequestSet findreqset = null;
                try
                {
                    recv_data = Read();

                    LoggerManager.Debug($"[GPIB Command - {commandName}],'{recv_data}',Command Start.");
                    if (Module.ProcessReadObject(recv_data, out commandName, out argu, out findreqset) == EventCodeEnum.NONE)
                    {                        
                        LoggerManager.Debug($"[GPIB Command - {commandName}], OnlyCommand:{commandName} Argument: {argu}");

                        var result = findreqset.Request.GetRequestResult()?.ToString();
                        LoggerManager.Debug($"[GPIB Command - {findreqset.Request.GetType().Name}],'{recv_data}',End.");

                        //query.
                        if (!string.IsNullOrEmpty(result))
                        {
                            if (Module.GPIBSysParam_IParam.ExistPrefixInRetVal.Value == true)
                            {
                                result = commandName + result;
                            }
                            //command 날려야해.
                            WriteString(result);
                        }
                        //action.
                        else
                        {
                            LoggerManager.Debug($"[GPIB Command - requestResult is null or empty.");
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[GPIB Command - {commandName}], OnlyCommand:{commandName} Argument: {argu}, ");
                    }

                    ////////////////////////////////
                    // Todo : Test Code
                    //ModuleManager.LISTEN_FLAG = true;//test
                    ////////////////////////////////

                    RetVal = EventCodeEnum.NONE;
                }
                catch (TimeoutException timeoutException)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Debug($"[GPIB],[OnRecvProcessing],Timeout Occurred.,{timeoutException.Message}");
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    Module.InnerStateTransition(new GPIBErrorState(Module));
                    //LoggerManager.Debug($"[GPIB] [OnRecvProcessing] WriteString Fail. : {e.Message}");
                    LoggerManager.Exception(err);
                }

                return RetVal;
            }
        }

     


        private bool RecvContition(int Ibsta)
        {
            bool retVal = false;

            try
            {
                retVal = (Ibsta & (int)GpibStatusFlags.LACS) == (int)GpibStatusFlags.LACS;
                retVal = retVal && ((Ibsta & (int)GpibStatusFlags.REM) == (int)GpibStatusFlags.REM);
                retVal = retVal && ((Ibsta & (int)GpibStatusFlags.ATN) != (int)GpibStatusFlags.ATN);
                retVal = retVal && ((Ibsta & (int)GpibStatusFlags.ERR) != (int)GpibStatusFlags.ERR);
                retVal = retVal && ((Ibsta & (int)GpibStatusFlags.END) != (int)GpibStatusFlags.END);

                retVal = retVal && Module.LISTEN_FLAG;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum WriteSTB(int? command)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                LoggerManager.Debug(command.ToString());

                //Module.GpibManager?.WriteSTB(command);

                semaphore.Wait(timeout: System.TimeSpan.FromSeconds(1));

                Module.TesterComDriver.WriteSTB(command);
                LoggerManager.GpibCommlog(Convert.ToInt64(GpibStatusFlags.NONE), Convert.ToInt32(GpibCommunicationActionType.STB), command?.ToString());

                command = null;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.GpibErrorlog($"[GPIB],[WriteSTB],Fail WriteSTB.,{command} Command");
                Module.InnerStateTransition(new GPIBErrorState(Module));
                LoggerManager.Exception(err);
            }
            finally
            {
                Module.LISTEN_FLAG = true;
                semaphore.Release();
            }

            return retVal;
        }

        private string GpibStaMsg(int ibsta)
        {
            StringBuilder sb = new StringBuilder();
            string result = "";
            try
            {
                if ((ibsta & (int)GpibStatusFlags.ERR) == (int)GpibStatusFlags.ERR) sb.Append("ERR");       /* Error detected                   */
                if ((ibsta & (int)GpibStatusFlags.TIMO) == (int)GpibStatusFlags.TIMO) sb.Append(" TIMO");     /* Timeout                          */
                if ((ibsta & (int)GpibStatusFlags.END) == (int)GpibStatusFlags.END) sb.Append(" END");      /* EOI or EOS detected              */
                if ((ibsta & (int)GpibStatusFlags.SRQI) == (int)GpibStatusFlags.SRQI) sb.Append(" SRQI");     /* SRQ detected by CIC              */
                if ((ibsta & (int)GpibStatusFlags.RQS) == (int)GpibStatusFlags.RQS) sb.Append(" RQS");      /* Device needs service             */
                if ((ibsta & (int)GpibStatusFlags.CMPL) == (int)GpibStatusFlags.CMPL) sb.Append(" CMPL");     /* I/O completed                    */
                if ((ibsta & (int)GpibStatusFlags.LOK) == (int)GpibStatusFlags.LOK) sb.Append(" LOK");      /* Local lockout state              */
                if ((ibsta & (int)GpibStatusFlags.REM) == (int)GpibStatusFlags.REM) sb.Append(" REM");      /* Remote state                     */
                if ((ibsta & (int)GpibStatusFlags.CIC) == (int)GpibStatusFlags.CIC) sb.Append(" CIC");      /* Controller-in-Charge             */
                if ((ibsta & (int)GpibStatusFlags.ATN) == (int)GpibStatusFlags.ATN) sb.Append(" ATN");      /* Attention asserted               */
                if ((ibsta & (int)GpibStatusFlags.TACS) == (int)GpibStatusFlags.TACS) sb.Append(" TACS");     /* Talker active                    */
                if ((ibsta & (int)GpibStatusFlags.LACS) == (int)GpibStatusFlags.LACS) sb.Append(" LACS");     /* Listener active                  */
                if ((ibsta & (int)GpibStatusFlags.DTAS) == (int)GpibStatusFlags.DTAS) sb.Append(" DTAS");     /* Device trigger state             */
                if ((ibsta & (int)GpibStatusFlags.DCAS) == (int)GpibStatusFlags.DCAS) sb.Append(" DCAS");     /* Device clear state               */

                result = sb.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return result;
        }

        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new GPIBPauseState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum DisConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                lock (executeObj)
                {
                    Module.TesterDriver_DisConnect();
                    Module.InnerStateTransition(new GPIBNotConnectedState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override string Read()
        {
            string retVal = string.Empty;

            try
            {
                //retVal = Module.GpibManager?.Read();

                retVal = Module.TesterDriver_Read();
                if (retVal != null)
                {
                    LoggerManager.Debug($"[GPIB],[Read],'{retVal.Replace("\r", "").Replace("\n", "")}'");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum WriteString(string query_command)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                //Module.GpibManager?.WriteString(query_command);

                //OnRecvProcessing 랑 같은 스레드에 있어 lock걸면안됨.
                Module.TesterDriver_WriteString(query_command);

                LoggerManager.Debug($"[GPIB],[Write],{query_command}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            BinAnalysisDataArray BinAnalysisData = null;
            try
            {
                GPIBSysParam gpibSysParam = Module.GPIBSysParam;

                if (!string.IsNullOrEmpty(binCode) && 0 < binCode.Length)
                {
                    if (
                          ((gpibSysParam.EnumGpibComType.Value & EnumGpibCommType.TEL) == EnumGpibCommType.TEL)
                      || ((gpibSysParam.EnumGpibComType.Value & EnumGpibCommType.TSK) == EnumGpibCommType.TSK)
                      || ((gpibSysParam.EnumGpibComType.Value & EnumGpibCommType.TSK_SPECIAL) == EnumGpibCommType.TSK_SPECIAL)
                      )
                    {
                        bool IsSuccessAnalysis = false;

                        // TODO : "M", 1 => 하드코딩
                        string preFix = binCode.Substring(0, 1);
                        IStageSupervisor StageSupervisor = this.StageSupervisor();

                        IsSuccessAnalysis = Module.BinAnalyzerManager.GetTestResultAnalysis(preFix, binCode, StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count, ref BinAnalysisData);

                        if (IsSuccessAnalysis == true)
                        {
                            this.EventManager().RaisingEvent(typeof(CalculatePfNYieldEvent).FullName);
                        }
                        else
                        {
                            //Fail Analysis TestResult.
                            LoggerManager.Error($"[GPIB - AnalyzeBin] Received Wrong Bin Code From Tester. " +
                                $"Tester Result : {binCode}, " +
                                $"Dut Count : {StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count}");

                            this.MetroDialogManager().ShowMessageDialog("Received Wrong Bin Code From Tester", "Received Wrong Tester Data From Tester.", EnumMessageStyle.Affirmative);

                            // TODO : 에러를 내야 하는가?
                            //Module.InnerStateTransition(new GPIBErrorState(Module) { ErrCode = EventCodeEnum.RECIVE_WRONG_RESULT_DATA });
                            //this.Pause();
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return BinAnalysisData;
        }
    }

    public class GPIBPauseState : GPIBStateBase
    {
        public GPIBPauseState(GPIB module) : base(module)
        {
        }

        public override GPIBStateEnum GetState()
        {
            return GPIBStateEnum.PAUSED;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.PAUSED;
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }
    }

    public class GPIBErrorState : GPIBStateBase
    {
        public GPIBErrorState(GPIB module) : base(module)
        {
        }

        public override GPIBStateEnum GetState()
        {
            return GPIBStateEnum.ERROR;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public override EventCodeEnum DisConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                retVal = Module.TesterDriver_DisConnect();

                if (retVal == EventCodeEnum.NONE)
                {
                    Module.InnerStateTransition(new GPIBNotConnectedState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }
    }
}

