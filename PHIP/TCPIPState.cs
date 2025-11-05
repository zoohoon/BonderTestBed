using System;

namespace TCPIP
{
    using LogModule;
    using MetroDialogInterfaces;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.BinData;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.State;
    using RequestInterface;

    public abstract class TCPIPState : IFactoryModule, IInnerState
    {
        public abstract TCPIPStateEnum GetState();
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
        public virtual EventCodeEnum WriteString(string query_command)
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual EventCodeEnum WriteSTB(string command)
        {
            return EventCodeEnum.UNDEFINED;
        }

        public virtual BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            return null;
        }
    }

    public abstract class TCPIPStateBase : TCPIPState
    {
        private TCPIPModule _Module;
        public TCPIPModule Module
        {
            get { return _Module; }
            private set { _Module = value; }
        }

        public TCPIPStateBase(TCPIPModule module)
        {
            Module = module;
        }
    }

    public class TCPIPNotConnectedState : TCPIPStateBase
    {
        public TCPIPNotConnectedState(TCPIPModule module) : base(module)
        {
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.IDLE;
        }
        public override TCPIPStateEnum GetState()
        {
            return TCPIPStateEnum.NOTCONNECTED;
        }

        public override EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.TesterDriver_Connect(null);

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TCPIP connection succeeded");

                    Module.InnerStateTransition(new TCPIPConnectedState(Module));

                    //retVal = Module.EventManager().RaisingEvent(typeof(TCPIPConnectionStartEvent).FullName);
                    retVal = Module.EventManager().RaisingEvent(typeof(TesterConnectedEvent).FullName);

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"TCPIP connection failed");
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
                LoggerManager.Debug($"[{Module.GetType().Name}] | [{this.GetType().Name}.Resume()] : Resume TCPIP connection.");

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TCPIPConnectedState : TCPIPStateBase
    {
        object executeObj = new object();

        public TCPIPConnectedState(TCPIPModule module) : base(module)
        {
        }

        public override TCPIPStateEnum GetState()
        {
            return TCPIPStateEnum.CONNECTED;
        }

        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.RUNNING;
        }

        public override EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                lock (executeObj)
                {
                    object state = Module.TesterDriver_GetState();
                    EnumTesterDriverState driverstate = (EnumTesterDriverState)state;

                    switch (driverstate)
                    {
                        case EnumTesterDriverState.UNDEFINED:
                            break;
                        case EnumTesterDriverState.DISCONNECT:
                            break;
                        case EnumTesterDriverState.CONNECTED:
                            // 테스터로부터 받을 준비가 됨.
                            //retval = Module.TesterDriver_Receive();
                            break;
                        case EnumTesterDriverState.BEGINRECEIVE:
                            break;
                        case EnumTesterDriverState.BEGINSEND:
                            // 테스터로 커맨드를 보냄.
                            break;
                        case EnumTesterDriverState.RECEIVED:
                            // 테스터로부터 커맨드를 받음.
                            retval = OnRecvProcessing();
                            break;
                        case EnumTesterDriverState.SENDED:
                            break;
                        case EnumTesterDriverState.ERROR:
                            this.Module.ReasonOfError.AddEventCodeInfo(EventCodeEnum.TESTER_RESPONDS_TIMEOUT, "Lost communication\nThe communication connection with the tester was abruptly terminated.", this.GetType().Name);
                            Module.InnerStateTransition(new TCPIPErrorState(Module));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            EventCodeEnum OnRecvProcessing()
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

                string recv_data = null;

                try
                {
                    recv_data = Read();

                    if (recv_data != null)
                    {
                        if (recv_data.Length > 0)
                        {
                            //recv_data = RemoveTerminator(recv_data);
                            if (Module.TCPIPRequestParam != null)
                            {
                                CommunicationRequestSet requestSet = Module.RequestSetList.Find(i => i.Name == recv_data);

                                int cmdLength = 5 > recv_data.Length ? 5 : recv_data.Length;

                                string findString = string.Empty;

                                while (1 <= cmdLength)
                                {
                                    if (requestSet == null)
                                    {
                                        if (cmdLength <= recv_data.Length)
                                        {
                                            findString = recv_data.Substring(0, cmdLength);
                                            requestSet = Module.RequestSetList.Find(i => i.Name == findString);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    cmdLength--;
                                }

                                if (requestSet != null)
                                {
                                    string requestResult = string.Empty;
                                    requestSet.Request.Argument = recv_data;

                                    LoggerManager.Debug($"[TCPIP Command - {requestSet.Request.GetType().Name}],'{recv_data}',Command Start.");
                                    requestResult = requestSet.Request.GetRequestResult()?.ToString();
                                    LoggerManager.Debug($"[TCPIP Command - {requestSet.Request.GetType().Name}],'{recv_data}',End.");

                                    //query.
                                    if (!string.IsNullOrEmpty(requestResult))
                                    {
                                        //command 날려야해.
                                        WriteString(requestResult);
                                    }
                                    //action.
                                    else
                                    {
                                    }
                                }
                            }
                            else 
                            {
                                this.Module.ReasonOfError.AddEventCodeInfo(EventCodeEnum.PARAM_ERROR, "Communication scenario information is invalid. Check the relevant information.\n", this.GetType().Name);
                                Module.InnerStateTransition(new TCPIPErrorState(Module));
                            }
                        }
                    }

                    RetVal = EventCodeEnum.NONE;
                }
                catch (TimeoutException timeoutException)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    LoggerManager.Debug($"[TCPIP],[OnRecvProcessing],Timeout Occurred.,{timeoutException.Message}");
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.UNDEFINED;
                    Module.InnerStateTransition(new TCPIPErrorState(Module));
                    LoggerManager.Exception(err);
                }

                return RetVal;
            }
        }

        //private string RemoveTerminator(string recv_data)
        //{
        //    string terminator = Module.TCPIPSysParam.Terminator.Value;
        //    string retStr = recv_data;

        //    try
        //    {
        //        if (terminator != null && terminator != string.Empty)
        //        {
        //            retStr = retStr.Replace(terminator, "");
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);

        //    }
        //    return retStr;
        //}

        public override BinAnalysisDataArray AnalyzeBin(string binCode)
        {
            BinAnalysisDataArray BinAnalysisData = null;

            try
            {
                if (!string.IsNullOrEmpty(binCode) && 0 < binCode.Length)
                {
                    bool IsSuccessAnalysis = false;
                    
                    // TODO : "BN", 2 => 하드코딩
                    string preFix = binCode.Substring(0, 2);
                    
                    IStageSupervisor StageSupervisor = this.StageSupervisor();

                    IsSuccessAnalysis = Module.BinAnalyzerManager.GetTestResultAnalysis(preFix, binCode, StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count, ref BinAnalysisData);

                    if (IsSuccessAnalysis == true)
                    {
                        this.EventManager().RaisingEvent(typeof(CalculatePfNYieldEvent).FullName);
                    }
                    else
                    {
                        //Fail Analysis TestResult.
                        LoggerManager.Error($"[TCPIPConnectedState], AnalyzeBin() : Recived Wrong Bin Code From Tester. " +
                            $"Tester Result : {binCode}, " +
                            $"Dut Count : {StageSupervisor.ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count}");

                        // TODO : 에러 처리

                        //this.MetroDialogManager().ShowMessageDialog("Recived Wrong Bin Code From Tester", "Recived Wrong Tester Data From Tester.", EnumMessageStyle.Affirmative);
                        //Module.InnerStateTransition(new GPIBErrorState(Module) { ErrCode = EventCodeEnum.RECIVE_WRONG_RESULT_DATA });
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return BinAnalysisData;
        }
        public override EventCodeEnum Pause()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Module.InnerStateTransition(new TCPIPPauseState(Module));
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
                    Module.InnerStateTransition(new TCPIPNotConnectedState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public override EventCodeEnum WriteSTB(string command)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                Module.TesterDriver_WriteSTB(command);
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
                Module.TesterDriver_WriteString(query_command);

                LoggerManager.Debug($"[TCPIP],[Write],{query_command}");
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

                if (retVal != null && retVal != string.Empty)
                {
                    LoggerManager.Debug($"[TCPIP],[Read],'{retVal?.Replace("\r", "").Replace("\n", "")}'");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class TCPIPPauseState : TCPIPStateBase
    {
        public TCPIPPauseState(TCPIPModule module) : base(module)
        {

        }

        public override TCPIPStateEnum GetState()
        {
            return TCPIPStateEnum.PAUSED;
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

    public class TCPIPErrorState : TCPIPStateBase
    {
        public TCPIPErrorState(TCPIPModule module) : base(module)
        {
            EventCodeInfo eventCodeInfo = module.ReasonOfError.GetLastEventCode();
            if (eventCodeInfo != null && eventCodeInfo.Checked == false)
            {
                Module.MetroDialogManager().ShowMessageDialog($"[Communication Error]", $"Code = {eventCodeInfo.EventCode}\n" +
                                                                                 $"Occurred time : {eventCodeInfo.OccurredTime}\n" +
                                                                                 $"Occurred location : {eventCodeInfo.ModuleType}\n" +
                                                                                 $"Reason : {eventCodeInfo.Message}",
                                                                                 EnumMessageStyle.Affirmative);
                module.ReasonOfError.Confirmed();
            }
        }

        public override TCPIPStateEnum GetState()
        {
            return TCPIPStateEnum.ERROR;
        }
        public override ModuleStateEnum GetModuleState()
        {
            return ModuleStateEnum.ERROR;
        }

        public override EventCodeEnum Execute()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public override EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Module.TesterDriver_Connect(null);

                if (retVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TCPIP connection succeeded");

                    Module.InnerStateTransition(new TCPIPConnectedState(Module));

                    //retVal = Module.EventManager().RaisingEvent(typeof(TCPIPConnectionStartEvent).FullName);
                    retVal = Module.EventManager().RaisingEvent(typeof(TesterConnectedEvent).FullName);

                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"TCPIP connection failed");
                }
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
                Module.InnerStateTransition(new TCPIPNotConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}

