using FoupModules.DockingPort;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoupModules.Template.MINI
{
    public class MINIFoupDockingPort : FoupDockingPortBase, ITemplateModule
    {
        private MINIFoupDockingPortStateBase _State;

        public MINIFoupDockingPortStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }
        #region // ITemplateModule implementation.

        public MINIFoupDockingPort()
        {

        }
        public bool Initialized { get; set; } = false;
        public void DeInitModule()
        {

        }
        public EventCodeEnum InitModule()
        {
            //EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            //ret = StateInit();
            //if (ret != EventCodeEnum.NONE)
            //{
            //    LoggerManager.Debug($"GPFoupCover12Inch.InitModule(): Init. error. Ret = {ret}");
            //}
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion
        public MINIFoupDockingPort(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(MINIFoupDockingPortStateBase state)
        {
            try
            {
                _State = state;

                this.EnumState = _State.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
        public override EventCodeEnum In()
        {
            return State.In();
        }

        public override EventCodeEnum Out()
        {
            return State.Out();
        }

        public override DockingPortStateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;
            try
            {
                State = new MINIFoupDockingPortNAState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                State = new MINIFoupDockingPortStateError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. MINIFoupDockingPort - StateInit() : Error occured.");
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Foup8InchTilt_Failure, retVal);
                return retVal;
            }
            return retVal;
        }

        public int SendMessage(String obj, String param)
        {
            int retVal = 0;
            try
            {
                IPAddress rIP, nodeIP;
                IPAddress.TryParse("192.168.1.178", out rIP);  // P-MAS IP
                IPAddress.TryParse("192.168.1.49", out nodeIP);  // P-MAS IP
                string input = obj + "=" + param + ";\n\r";

                int port = 5001;
                //UDP Socket 생성
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPEndPoint endPoint = new IPEndPoint(nodeIP, port);

                //바인드
                socket.Connect(endPoint);

                //데이터 입력

                //인코딩(byte[])
                byte[] sBuffer = Encoding.ASCII.GetBytes(input);
                //byte[] rbuffer;

                //보내기
                retVal = socket.Send(sBuffer, 0, sBuffer.Length, SocketFlags.None);
                //sBuffer = Encoding.ASCII.GetBytes("OL[15]");
                // retVal = socket.Send(sBuffer, 0, sBuffer.Length, SocketFlags.None);
                // rbuffer = ReadBySize(socket);

                socket.Close();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    public abstract class MINIFoupDockingPortStateBase
    {
        public MINIFoupDockingPortStateBase(MINIFoupDockingPort owner)
        {
            this.Owner = owner;
        }

        private MINIFoupDockingPort _Owner;
        public MINIFoupDockingPort Owner
        {
            get { return _Owner; }
            set
            {
                if (value != _Owner)
                {
                    _Owner = value;
                }
            }
        }

        protected IFoupModule FoupModule => Owner.Module;

        protected IFoupIOStates IO => Owner.Module.IOManager;

        public abstract DockingPortStateEnum GetState();

        public virtual EventCodeEnum In() { return InFunc(); }

        public virtual EventCodeEnum Out() { return OutFunc(); }

        public EventCodeEnum InFunc()
        {
            Owner.StateTransition(new MINIFoupDockingPortStateIn(Owner));
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum OutFunc()
        {
            Owner.StateTransition(new MINIFoupDockingPortStateOut(Owner));
            return EventCodeEnum.NONE;
        }
    }

    public class MINIFoupDockingPortStateIn : MINIFoupDockingPortStateBase
    {
        public MINIFoupDockingPortStateIn(MINIFoupDockingPort owner) : base(owner)
        {
        }
        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.IN;
        }

        public override EventCodeEnum In()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = OutFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupDockingPortStateOut : MINIFoupDockingPortStateBase
    {
        public MINIFoupDockingPortStateOut(MINIFoupDockingPort owner) : base(owner)
        {
        }

        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.OUT;
        }

        public override EventCodeEnum In()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = InFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = OutFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupDockingPortStateError : MINIFoupDockingPortStateBase
    {
        public MINIFoupDockingPortStateError(MINIFoupDockingPort owner) : base(owner)
        {
        }

        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.ERROR;
        }

        public override EventCodeEnum In()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = InFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = OutFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupDockingPortNAState : MINIFoupDockingPortStateBase
    {
        public MINIFoupDockingPortNAState(MINIFoupDockingPort owner) : base(owner)
        {
        }

        public override DockingPortStateEnum GetState()
        {
            return DockingPortStateEnum.IDLE;
        }

        public override EventCodeEnum In()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = InFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = OutFunc();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
