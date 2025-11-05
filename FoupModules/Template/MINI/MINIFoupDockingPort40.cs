using FoupModules.DockingPort40;
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
    public class MINIFoupDockingPort40 : DockingPort40Base, ITemplateModule
    {
        private MINIFoupDockingPort40StateBase _State;

        public MINIFoupDockingPort40StateBase State
        {
            get { return _State; }
            set { _State = value; }
        }
        #region // ITemplateModule implementation.

        public MINIFoupDockingPort40()
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
        public MINIFoupDockingPort40(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(MINIFoupDockingPort40StateBase state)
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

        public override EventCodeEnum In()
        {
            return State.In40();
        }

        public override EventCodeEnum Out()
        {
            return State.Out40();
        }

        public override DockingPort40StateEnum GetState()
        {
            return State.GetState();
        }

        public override EventCodeEnum StateInit()
        {
            EventCodeEnum retVal;
            try
            {
                State = new MINIFoupDockingPort40NAState(this);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                State = new MINIFoupDockingPort40StateError(this);
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. MINIFoupDockingPort40 - StateInit() : Error occured.");
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
    public abstract class MINIFoupDockingPort40StateBase
    {
        public MINIFoupDockingPort40StateBase(MINIFoupDockingPort40 owner)
        {
            this.Owner = owner;
        }

        private MINIFoupDockingPort40 _Owner;
        public MINIFoupDockingPort40 Owner
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

        public abstract DockingPort40StateEnum GetState();

        public virtual EventCodeEnum In40() { return In40Func(); }

        public virtual EventCodeEnum Out40() { return Out40Func(); }

        public EventCodeEnum In40Func()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = EventCodeEnum.NONE;
                Owner.StateTransition(new MINIFoupDockingPort40StateIn(Owner));
            }

            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new MINIFoupDockingPort40StateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum Out40Func()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = EventCodeEnum.NONE;
                Owner.StateTransition(new MINIFoupDockingPort40StateOut(Owner));
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new MINIFoupDockingPort40StateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class MINIFoupDockingPort40StateIn : MINIFoupDockingPort40StateBase
    {
        public MINIFoupDockingPort40StateIn(MINIFoupDockingPort40 owner) : base(owner)
        {
        }

        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.IN;
        }

        public override EventCodeEnum In40()
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

        public override EventCodeEnum Out40()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Out40Func();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupDockingPort40StateOut : MINIFoupDockingPort40StateBase
    {
        public MINIFoupDockingPort40StateOut(MINIFoupDockingPort40 owner) : base(owner)
        {
        }

        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.OUT;
        }

        public override EventCodeEnum In40()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = In40Func();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out40()
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
    }

    public class MINIFoupDockingPort40StateError : MINIFoupDockingPort40StateBase
    {
        public MINIFoupDockingPort40StateError(MINIFoupDockingPort40 owner) : base(owner)
        {
        }

        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.ERROR;
        }

        public override EventCodeEnum In40()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = In40Func();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out40()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Out40Func();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }

    public class MINIFoupDockingPort40NAState : MINIFoupDockingPort40StateBase
    {
        public MINIFoupDockingPort40NAState(MINIFoupDockingPort40 owner) : base(owner)
        {
        }

        public override DockingPort40StateEnum GetState()
        {
            return DockingPort40StateEnum.IDLE;
        }

        public override EventCodeEnum In40()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = In40Func();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.FOUP_ERROR;
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum Out40()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Out40Func();
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
