using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using RequestCore.Query.TCPIP;
using RequestInterface;
using System;

namespace Command.TCPIP
{
    [Serializable]
    public abstract class TCPIPCommandBase : ProbeCommand
    {
    }

    [Serializable]
    public class TCPIPResponseParam : ProbeCommandParameter
    {
        public RequestBase response { get; set; }
    }

    [Serializable]
    public class TCPIP_RESPONSE : TCPIPCommandBase, ITCPIP_RESPONSE
    {
        public override bool Execute()
        {
            bool RetVal = false;
            try
            {
                ITCPIP TCPIPModule = this.TCPIPModule();

                TCPIPResponseParam param = this.Parameter as TCPIPResponseParam;

                if (param != null)
                {
                    string response = param.response.GetRequestResult().ToString();
                    
                    TCPIPModule.WriteSTB(response);
                    RetVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
    }

    [Serializable]
    public class TCPIP_ACTION : TCPIPCommandBase, ITCPIP_ACTION
    {
        public override bool Execute()
        {
            bool RetVal = false;

            try
            {
                TCPIPResponseParam param = this.Parameter as TCPIPResponseParam;
                if (param != null)
                {
                    param.response.DoRun();
                    RetVal = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
    }

    [Serializable]
    public class ProberConnectionStart : TCPIPQueryData
    {
        public ProberConnectionStart()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
    [Serializable]
    public class ProberCardIDReadDone : TCPIPQueryData
    {
        public ProberCardIDReadDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class FoupAllocated : TCPIPQueryData
    {
        public FoupAllocated()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ProberStatus : TCPIPQueryData
    {
        public ProberStatus()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class LotStart : TCPIPQueryData
    {
        public LotStart()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ChipStart : TCPIPQueryData
    {
        public ChipStart()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class PMIStart : TCPIPQueryData
    {
        public PMIStart()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class PMIEnd : TCPIPQueryData
    {
        public PMIEnd()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ZupDone : TCPIPQueryData
    {
        public ZupDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ZdownDone : TCPIPQueryData
    {
        public ZdownDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    

    [Serializable]
    public class MoveToNextIndexDone : TCPIPQueryData
    {
        public MoveToNextIndexDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class LotEndDone : TCPIPQueryData
    {
        public LotEndDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class UnloadWaferDone : TCPIPQueryData
    {
        public UnloadWaferDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class DWPassDone : TCPIPQueryData
    {
        public DWPassDone()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class ProberErrorOccurred : TCPIPQueryData
    {
        public ProberErrorOccurred()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = GetResponse();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    [Serializable]
    public class TCPIPDummyQueryData : TCPIPQueryData
    {
        public string DummyData { get; set; }

        public TCPIPDummyQueryData()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.Result = MakeTCPIPResponseResult();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private string MakeTCPIPResponseResult()
        {
            string retval = string.Empty;

            try
            {
                retval = DummyData;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    [Serializable]
    public class TCPIPDummyActionData : TCPIPQueryData
    {
        public string EventFullName { get; set; }

        public Func<string> EventConditionFunc;

        // TODO : Action 수행 후, 발행해야하는 이벤트 
        public TCPIPDummyActionData()
        {
        }

        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if(EventConditionFunc == null)
                {
                    if (string.IsNullOrEmpty(EventFullName) == false)
                    {
                        retVal = this.EventManager().RaisingEvent(EventFullName);
                    }
                }
                else
                {
                    string eventname = EventConditionFunc();

                    if (string.IsNullOrEmpty(eventname) == false)
                    {
                        retVal = this.EventManager().RaisingEvent(eventname);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(EventFullName) == false)
                        {
                            retVal = this.EventManager().RaisingEvent(EventFullName);
                        }
                    }
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

