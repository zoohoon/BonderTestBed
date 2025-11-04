using CylType;
using ECATIO;
using FoupModules.DockingPort40;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FoupModules.Template.FLAT
{
    public class FLATFoupDockingPort40 : DockingPort40Base, ITemplateModule
    {
        private FLATFoupDockingPort40StateBase _State;

        public FLATFoupDockingPort40StateBase State
        {
            get { return _State; }
            set { _State = value; }
        }
        #region // ITemplateModule implementation.

        public FLATFoupDockingPort40()
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
        public FLATFoupDockingPort40(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(FLATFoupDockingPort40StateBase state)
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
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            var IO = Module.IOManager;

            try
            {
                if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6)
                {
                    bool value, value2;
                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_CP_40_IN, out value);
                    int ret1 = IO.ReadBit(IO.IOMap.Inputs.DI_CP_40_OUT, out value2);

                    if (ret != 0 || ret1 != 0)
                    {
                        StateTransition(new FLATFoupDockingPort40StateError(this));
                    }
                    else
                    {
                        if (value == true && value2 == false)
                        {
                            StateTransition(new FLATFoupDockingPort40StateIn(this));
                        }
                        else if (value == false && value2 == true)
                        {
                            StateTransition(new FLATFoupDockingPort40StateOut(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupDockingPort40StateError(this));
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    bool IN, OUT;
                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_CP_40_IN, out IN);
                    int ret1 = IO.ReadBit(IO.IOMap.Inputs.DI_CP_40_OUT, out OUT);

                    if (ret != 0 || ret1 != 0)
                    {
                        StateTransition(new FLATFoupDockingPort40StateError(this));
                    }
                    else
                    {
                        if (IN == true && OUT == false)
                        {
                            StateTransition(new FLATFoupDockingPort40StateIn(this));
                        }
                        else if (IN == false && OUT == true)
                        {
                            StateTransition(new FLATFoupDockingPort40StateOut(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupDockingPort40StateError(this));
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                {
                    // OUT
                    int ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, false, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);
                    int ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, true, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);

                    if (ret1 == -2 && ret2 == -2)
                    {
                        int ret3 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, true, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);
                        int ret4 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, false, FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME, FoupIOGlobalVar.IO_CHECK_TIME_OUT);

                        // IN
                        if (ret3 == 0 && ret4 == 0)
                        {
                            StateTransition(new FLATFoupDockingPort40StateIn(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupDockingPort40StateError(this));
                        }
                    }
                    else if (ret1 == 0 && ret2 == 0)
                    {
                        StateTransition(new FLATFoupDockingPort40StateOut(this));
                    }
                    else
                    {
                        StateTransition(new FLATFoupDockingPort40StateError(this));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{Module.DeviceParam.SubstrateSize.Value}]");
                    StateTransition(new FLATFoupDockingPort40StateError(this));
                }

                if (EnumState == DockingPort40StateEnum.ERROR)
                {
                    retval = EventCodeEnum.FOUP_ERROR;
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                StateTransition(new FLATFoupDockingPort40StateError(this));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
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
    public abstract class FLATFoupDockingPort40StateBase
    {
        public FLATFoupDockingPort40StateBase(FLATFoupDockingPort40 owner)
        {
            this.Owner = owner;
        }

        private FLATFoupDockingPort40 _Owner;
        public FLATFoupDockingPort40 Owner
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

        public abstract EventCodeEnum In40();

        public abstract EventCodeEnum Out40();

        public EventCodeEnum In40Func()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPort40Cylinder = FoupCylinderType.FoupDockingPort40;

            //IOPortDescripter<bool> Presence_IO = null;
            //IOPortDescripter<bool> Placement_IO = null;
            //IOPortDescripter<bool> FoupCover_UP_IO = null;
            IOPortDescripter<bool> CP_IN_IO = null;

            bool SubstrateSizeValid = false;
            //bool Level = true;
            long maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
            long timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;

            try
            {
                switch (FoupModule.DeviceParam.SubstrateSize.Value)
                {
                    case SubstrateSizeEnum.UNDEFINED:
                        break;
                    case SubstrateSizeEnum.INCH6:
                        //Presence_IO = ;
                        //Placement_IO = ;
                        //Level = true;
                        //maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        //timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        SubstrateSizeValid = true;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        //Presence_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1;
                        //Placement_IO = IO.IOMap.Inputs.DI_C8IN_PLACEMENT;
                        //FoupCover_UP_IO = IO.IOMap.Inputs.DI_FO_UP;
                        CP_IN_IO = IO.IOMap.Inputs.DI_CP_IN;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;

                        SubstrateSizeValid = true;

                        break;
                    case SubstrateSizeEnum.INCH12:
                        //Presence_IO = IO.IOMap.Inputs.DI_C12IN_PRESENCE1;
                        //Placement_IO = IO.IOMap.Inputs.DI_C12IN_PLACEMENT;
                        //FoupCover_UP_IO = IO.IOMap.Inputs.DI_FO_UP;
                        CP_IN_IO = IO.IOMap.Inputs.DI_CP_IN;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;

                        SubstrateSizeValid = true;

                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if (SubstrateSizeValid == true)
                {
                    int devNum = IO.IOServ.Outputs[IO.IOMap.Outputs.DO_CP_CYL_IN_AIR.ChannelIndex.Value].DevIndex;
                    IIOBase iobase = IO.IOServ.IOList[devNum];

                    if (iobase != null)
                    {
                        if (iobase is ECATIOProvider)
                        {
                            int ret1 = -1;
                            int ret2 = -1;

                            Owner.SendMessage("UI[2]", "1");

                            // TODO: 
                            ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, true, IO.IOMap.Inputs.DI_CP_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_IN.TimeOut.Value);
                            ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, false, IO.IOMap.Inputs.DI_CP_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_OUT.TimeOut.Value);

                            if (ret1 != 0 && ret2 != 0)
                            {
                                Owner.StateTransition(new FLATFoupDockingPort40StateIn(Owner));
                            }
                            else
                            {
                                Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                            }

                            System.Threading.Thread.Sleep(500);
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                            Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                        Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                }

                if (Owner.EnumState == DockingPort40StateEnum.ERROR)
                {
                    retval = EventCodeEnum.FOUP_ERROR;
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum Out40Func()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPort40Cylinder = FoupCylinderType.FoupDockingPort40;

            //IOPortDescripter<bool> Presence_IO = null;
            //IOPortDescripter<bool> Placement_IO = null;
            //IOPortDescripter<bool> FoupCover_UP_IO = null;
            IOPortDescripter<bool> CP_IN_IO = null;

            bool SubstrateSizeValid = false;
            //bool Level = true;
            long maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
            long timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;

            try
            {
                switch (FoupModule.DeviceParam.SubstrateSize.Value)
                {
                    case SubstrateSizeEnum.UNDEFINED:
                        break;
                    case SubstrateSizeEnum.INCH6:
                        //Presence_IO = ;
                        //Placement_IO = ;
                        //Level = true;
                        //maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        //timeout = FoupIOGlobalVar.IO_CHECK_TIME_OUT;
                        SubstrateSizeValid = true;
                        break;
                    case SubstrateSizeEnum.INCH8:
                        //Presence_IO = IO.IOMap.Inputs.DI_C6IN_C8IN_PRESENCE1;
                        //Placement_IO = IO.IOMap.Inputs.DI_C8IN_PLACEMENT;
                        //FoupCover_UP_IO = IO.IOMap.Inputs.DI_FO_UP;
                        CP_IN_IO = IO.IOMap.Inputs.DI_CP_IN;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;

                        SubstrateSizeValid = true;

                        break;
                    case SubstrateSizeEnum.INCH12:
                        //Presence_IO = IO.IOMap.Inputs.DI_C12IN_PRESENCE1;
                        //Placement_IO = IO.IOMap.Inputs.DI_C12IN_PLACEMENT;
                        //FoupCover_UP_IO = IO.IOMap.Inputs.DI_FO_UP;
                        CP_IN_IO = IO.IOMap.Inputs.DI_CP_IN;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;

                        SubstrateSizeValid = true;

                        break;
                    case SubstrateSizeEnum.CUSTOM:
                        break;
                    default:
                        break;
                }

                if (SubstrateSizeValid == true)
                {
                    int devNum = IO.IOServ.Outputs[IO.IOMap.Outputs.DO_CP_CYL_OUT_AIR.ChannelIndex.Value].DevIndex;
                    IIOBase iobase = IO.IOServ.IOList[devNum];

                    if (iobase != null)
                    {
                        if (iobase is ECATIOProvider)
                        {
                            int ret1 = -1;
                            int ret2 = -1;

                            Owner.SendMessage("UI[3]", "1");

                            // TODO: 
                            if ((Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                                (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                                )
                            {
                                ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, false, IO.IOMap.Inputs.DI_CP_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_IN.TimeOut.Value);
                                ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, true, IO.IOMap.Inputs.DI_CP_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_OUT.TimeOut.Value);
                            }
                            else if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                            {
                                ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, true, IO.IOMap.Inputs.DI_CP_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_IN.TimeOut.Value);
                                ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, false, IO.IOMap.Inputs.DI_CP_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_OUT.TimeOut.Value);
                            }

                            if (ret1 != 0 && ret2 != 0)
                            {
                                Owner.StateTransition(new FLATFoupDockingPort40StateOut(Owner));
                            }
                            else
                            {
                                Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                            }

                            System.Threading.Thread.Sleep(500);
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                            Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                        Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                }

                if (Owner.EnumState == DockingPort40StateEnum.ERROR)
                {
                    retval = EventCodeEnum.FOUP_ERROR;
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug("Error occurred.");
                LoggerManager.Exception(err);
                Owner.StateTransition(new FLATFoupDockingPort40StateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class FLATFoupDockingPort40StateIn : FLATFoupDockingPort40StateBase
    {
        public FLATFoupDockingPort40StateIn(FLATFoupDockingPort40 owner) : base(owner)
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

    public class FLATFoupDockingPort40StateOut : FLATFoupDockingPort40StateBase
    {
        public FLATFoupDockingPort40StateOut(FLATFoupDockingPort40 owner) : base(owner)
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

    public class FLATFoupDockingPort40StateError : FLATFoupDockingPort40StateBase
    {
        public FLATFoupDockingPort40StateError(FLATFoupDockingPort40 owner) : base(owner)
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
}
