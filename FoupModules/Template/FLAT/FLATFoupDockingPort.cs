using CylType;
using ECATIO;
using FoupModules.DockingPort;
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
    public class FLATFoupDockingPort : FoupDockingPortBase, ITemplateModule
    {
        private FLATFoupDockingPortStateBase _State;

        public FLATFoupDockingPortStateBase State
        {
            get { return _State; }
            set { _State = value; }
        }
        #region // ITemplateModule implementation.

        public FLATFoupDockingPort()
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
        public FLATFoupDockingPort(FoupModule module) : base(module)
        {
            StateInit();
        }

        public void StateTransition(FLATFoupDockingPortStateBase state)
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
        public override EventCodeEnum CheckState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
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
                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_CP_IN, out value);
                    int ret1 = IO.ReadBit(IO.IOMap.Inputs.DI_CP_OUT, out value2);

                    if (ret != 0 || ret1 != 0)
                    {
                        StateTransition(new FLATFoupDockingPortStateError(this));
                    }
                    else
                    {
                        if (value == true && value2 == false)
                        {
                            StateTransition(new FLATFoupDockingPortStateIn(this));
                        }
                        else if (value == false && value2 == true)
                        {
                            StateTransition(new FLATFoupDockingPortStateOut(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupDockingPortStateError(this));
                        }
                    }
                }
                else if (Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                {
                    bool IN, OUT;
                    int ret = IO.ReadBit(IO.IOMap.Inputs.DI_CP_IN, out IN);
                    int ret1 = IO.ReadBit(IO.IOMap.Inputs.DI_CP_OUT, out OUT);

                    if (ret != 0 || ret1 != 0)
                    {
                        StateTransition(new FLATFoupDockingPortStateError(this));
                    }
                    else
                    {
                        if (IN == true && OUT == false)
                        {
                            StateTransition(new FLATFoupDockingPortStateIn(this));
                        }
                        else if (IN == false && OUT == true)
                        {
                            StateTransition(new FLATFoupDockingPortStateOut(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupDockingPortStateError(this));
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
                            StateTransition(new FLATFoupDockingPortStateIn(this));
                        }
                        else
                        {
                            StateTransition(new FLATFoupDockingPortStateError(this));
                        }
                    }
                    else if (ret1 == 0 && ret2 == 0)
                    {
                        StateTransition(new FLATFoupDockingPortStateOut(this));
                    }
                    else
                    {
                        StateTransition(new FLATFoupDockingPortStateError(this));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{Module.DeviceParam.SubstrateSize.Value}]");
                    StateTransition(new FLATFoupDockingPortStateError(this));
                }

                if (EnumState == DockingPortStateEnum.ERROR)
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
                StateTransition(new FLATFoupDockingPortStateError(this));
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
    public abstract class FLATFoupDockingPortStateBase
    {
        public FLATFoupDockingPortStateBase(FLATFoupDockingPort owner)
        {
            this.Owner = owner;
        }

        private FLATFoupDockingPort _Owner;
        public FLATFoupDockingPort Owner
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

        public abstract EventCodeEnum In();

        public abstract EventCodeEnum Out();

        public EventCodeEnum InFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPortCylinder = FoupCylinderType.FoupDockingPort;
            //IOPortDescripter<bool> Presence_IO = null;
            //IOPortDescripter<bool> Placement_IO = null;
            IOPortDescripter<bool> CP_40_OUT_IO = null;
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
                        CP_40_OUT_IO = IO.IOMap.Inputs.DI_CP_40_OUT;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;

                        SubstrateSizeValid = true;

                        break;
                    case SubstrateSizeEnum.INCH12:
                        //Presence_IO = IO.IOMap.Inputs.DI_C12IN_PRESENCE1;
                        //Placement_IO = IO.IOMap.Inputs.DI_C12IN_PLACEMENT;
                        CP_40_OUT_IO = IO.IOMap.Inputs.DI_CP_40_OUT;
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

                            Owner.SendMessage("UI[1]", "1");

                            if ((Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH6) ||
                                (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH8)
                                )
                            {
                                ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, true, IO.IOMap.Inputs.DI_CP_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_IN.TimeOut.Value);
                                ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, false, IO.IOMap.Inputs.DI_CP_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_OUT.TimeOut.Value);
                            }
                            else if (Owner.Module.DeviceParam.SubstrateSize.Value == SubstrateSizeEnum.INCH12)
                            {
                                ret1 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_IN, false, IO.IOMap.Inputs.DI_CP_IN.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_IN.TimeOut.Value);
                                ret2 = IO.MonitorForIO(IO.IOMap.Inputs.DI_CP_OUT, true, IO.IOMap.Inputs.DI_CP_OUT.MaintainTime.Value, IO.IOMap.Inputs.DI_CP_OUT.TimeOut.Value);
                            }

                            if (ret1 != 0 && ret2 != 0)
                            {
                                Owner.StateTransition(new FLATFoupDockingPortStateIn(Owner));
                            }
                            else
                            {
                                Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                            }

                            System.Threading.Thread.Sleep(500);
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                            Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                        Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                }
                if (Owner.EnumState == DockingPortStateEnum.ERROR)
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
                Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }

        public EventCodeEnum OutFunc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            var IO = Owner.Module.IOManager;
            FoupCylinderType DockingPortCylinder = FoupCylinderType.FoupDockingPort;
            //IOPortDescripter<bool> Presence_IO = null;
            //IOPortDescripter<bool> Placement_IO = null;
            IOPortDescripter<bool> CP_40_OUT_IO = null;
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
                        CP_40_OUT_IO = IO.IOMap.Inputs.DI_CP_40_OUT;
                        //Level = true;
                        maintaintime = FoupIOGlobalVar.IO_CHECK_MAINTAIN_TIME;
                        timeout = 3000;

                        SubstrateSizeValid = true;

                        break;
                    case SubstrateSizeEnum.INCH12:
                        //Presence_IO = IO.IOMap.Inputs.DI_C12IN_PRESENCE1;
                        //Placement_IO = IO.IOMap.Inputs.DI_C12IN_PLACEMENT;
                        CP_40_OUT_IO = IO.IOMap.Inputs.DI_CP_40_OUT;
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

                            Owner.SendMessage("UI[4]", "1");

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
                                Owner.StateTransition(new FLATFoupDockingPortStateOut(Owner));
                            }
                            else
                            {
                                Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                            }

                            System.Threading.Thread.Sleep(500);
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                            Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[{this.GetType().Name}] IIOBase is abnormal. Input Value = [{iobase}]");
                        Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}] SubstarteSize is abnormal. Input Value = [{FoupModule.DeviceParam.SubstrateSize.Value}]");
                    Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                }
                if (Owner.EnumState == DockingPortStateEnum.ERROR)
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
                Owner.StateTransition(new FLATFoupDockingPortStateError(Owner));
                retval = EventCodeEnum.FOUP_ERROR;
            }

            return retval;
        }
    }

    public class FLATFoupDockingPortStateIn : FLATFoupDockingPortStateBase
    {
        public FLATFoupDockingPortStateIn(FLATFoupDockingPort owner) : base(owner)
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

    public class FLATFoupDockingPortStateOut : FLATFoupDockingPortStateBase
    {
        public FLATFoupDockingPortStateOut(FLATFoupDockingPort owner) : base(owner)
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

    public class FLATFoupDockingPortStateError : FLATFoupDockingPortStateBase
    {
        public FLATFoupDockingPortStateError(FLATFoupDockingPort owner) : base(owner)
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
}
