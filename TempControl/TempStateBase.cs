using LogModule;
using ProberInterfaces.Temperature.TempManager;
using System;

namespace Temperature.Temp
{
    public abstract class TempManagerStateBase : TempManagerState
    {
        private ITempManager _Module;
        public ITempManager Module
        {
            get { return _Module; }
            set { _Module = value; }
        }

        public TempManagerStateBase(ITempManager module)
        {
            this.Module = module;
        }

        public virtual TempManagerStateEnum GetState()
        {
            throw new NotImplementedException();
        }

        public virtual bool Connect(string serialPort, byte UnitIdentifier)
        {
            throw new NotImplementedException();
        }

        public virtual void Disconnect()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual double ReadMV()
        {
            throw new NotImplementedException();
        }

        public virtual double ReadPV()
        {
            throw new NotImplementedException();
        }

        public virtual double ReadSetV()
        {
            throw new NotImplementedException();
        }

        public virtual long ReadStatus()
        {
            throw new NotImplementedException();
        }
        public virtual long Get_OutPut_State()
        {
            return Module.TempModule.Get_OutPut_State();
        }

        public virtual void SetRemote_OFF(object notUsed)
        {
            throw new NotImplementedException();
        }

        public virtual void SetRemote_ON(object notUsed)
        {
            throw new NotImplementedException();
        }

        public virtual void SetSV(double value)
        {
            throw new NotImplementedException();
        }

        public virtual void Set_DT(double value)
        {
            throw new NotImplementedException();
        }

        public virtual void Set_IT(double value)
        {
            throw new NotImplementedException();
        }

        public virtual void Set_Offset(double value)
        {
            throw new NotImplementedException();
        }

        public virtual void Set_OutPut_OFF(object notUsed)
        {
            throw new NotImplementedException();
        }

        public virtual void Set_OutPut_ON(object notUsed)
        {
            throw new NotImplementedException();
        }

        public virtual void Set_PB(double value)
        {
            throw new NotImplementedException();
        }

        public virtual ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteSingleRegister(int data, int value)
        {
            throw new NotImplementedException();
        }
    }

    public class TempManagerDisConnectedState : TempManagerStateBase
    {
        public TempManagerDisConnectedState(ITempManager module) : base(module)
        {

        }

        public override TempManagerStateEnum GetState()
        {
            return TempManagerStateEnum.DISCONNECTED;
        }

        public override bool Connect(string serialPort, byte UnitIdentifier)
        {
            bool retVal = Module.TempModule?.Connect(serialPort, UnitIdentifier) == true ? true : false;
            try
            {

                if (retVal)
                {
                    if (Module.ChangeEnable)
                    {
                        //Module.TempModule.Set_OutPut_ON(null);
                        Module.TempManagerStateTransition(new TempManagerConnWrtEnableState(Module));
                    }
                    else
                        Module.TempManagerStateTransition(new TempManagerConnWrtDisableState(Module));
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                //LoggerManager.Debug($"Temp. controller has been disconnected.");
                LoggerManager.Debug($"Temp : controller has been disconnected.");

                //throw new NotImplementedException();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            throw new NotImplementedException();
        }

        public override double ReadMV()
        {
            throw new NotImplementedException();
        }

        public override double ReadPV()
        {
            throw new NotImplementedException();
        }

        public override double ReadSetV()
        {
            throw new NotImplementedException();
        }

        public override long ReadStatus()
        {
            throw new NotImplementedException();
        }

        public override void SetRemote_OFF(object notUsed)
        {
            throw new NotImplementedException();
        }

        public override void SetRemote_ON(object notUsed)
        {
            //throw new NotImplementedException();
        }

        public override void SetSV(double value)
        {
        }

        public override void Set_DT(double value)
        {
        }

        public override void Set_IT(double value)
        {
        }

        public override void Set_Offset(double value)
        {
        }

        public override void Set_OutPut_OFF(object notUsed)
        {
        }

        public override void Set_OutPut_ON(object notUsed)
        {
        }

        public override void Set_PB(double value)
        {
        }

        public override void WriteSingleRegister(int data, int value)
        {
        }
    }

    public class TempManagerDisConnectingState : TempManagerStateBase
    {
        public TempManagerDisConnectingState(ITempManager module) : base(module)
        {
        }

        public override bool Connect(string serialPort, byte UnitIdentifier)
        {
            return false;
        }

        public override void Disconnect()
        {
            try
            {
                Module.TempModule?.Dispose();
                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Dispose()
        {
            try
            {
                Module.TempModule?.Dispose();
                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override TempManagerStateEnum GetState()
        {
            return TempManagerStateEnum.DISCONNECTING;
        }

        public override ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            return null;
        }

        public override double ReadMV()
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            return -1;
        }

        public override double ReadPV()
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            return -1;
        }

        public override double ReadSetV()
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            return -1;
        }

        public override long ReadStatus()
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            return -1;
        }

        public override void SetRemote_OFF(object notUsed)
        {
            try
            {
                Module.TempModule?.Dispose();
                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetRemote_ON(object notUsed)
        {
            try
            {
                Module.TempModule?.Dispose();
                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetSV(double value)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
        }

        public override void Set_DT(double value)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
        }

        public override void Set_IT(double value)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
        }

        public override void Set_Offset(double value)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
        }

        public override void Set_OutPut_OFF(object notUsed)
        {
            try
            {
                Module.TempModule?.Dispose();
                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Set_OutPut_ON(object notUsed)
        {
            try
            {
                Module.TempModule?.Dispose();
                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Set_PB(double value)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
        }

        public override void WriteSingleRegister(int data, int value)
        {
            Module.TempModule?.Dispose();
            Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
        }
    }

    public class TempManagerConnWrtEnableState : TempManagerStateBase
    {
        public TempManagerConnWrtEnableState(ITempManager module) : base(module)
        {
            Module.TempModule?.SetRemote_ON(null);
        }

        public override bool Connect(string serialPort, byte UnitIdentifier)
        {
            bool retVal = false;
            try
            {
                Module.TempModule?.Dispose();

                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
                retVal = Module.Connect(serialPort, UnitIdentifier);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
                Module.Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Dispose()
        {
            try
            {
                Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
                Module.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override TempManagerStateEnum GetState()
        {
            return TempManagerStateEnum.CONN_WRT_ENABLE;
        }

        public override ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            return Module.TempModule.Get_Cur_Temp(dataGetdelay);
        }

        public override double ReadMV()
        {
            return Module.TempModule.ReadMV();
        }

        public override double ReadPV()
        {
            return Module.TempModule.ReadPV();
        }

        public override double ReadSetV()
        {
            return Module.TempModule.ReadSetV();
        }

        public override long ReadStatus()
        {
            return Module.TempModule.ReadStatus();
        }
        public override long Get_OutPut_State()
        {
            return Module.TempModule.Get_OutPut_State();
        }

        public override void SetRemote_OFF(object notUsed)
        {
            try
            {
                Module.TempManagerStateTransition(new TempManagerConnWrtDisableState(Module));
                Module.TempModule.SetRemote_OFF(notUsed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetRemote_ON(object notUsed)
        {
            Module.TempModule.SetRemote_ON(notUsed);
        }

        public override void SetSV(double value)
        {
            Module.TempModule.SetSV(value);
        }

        public override void Set_DT(double value)
        {
            Module.TempModule.Set_DT(value);
        }

        public override void Set_IT(double value)
        {
            Module.TempModule.Set_IT(value);
        }

        public override void Set_Offset(double value)
        {
            Module.TempModule.Set_Offset(value);
        }

        public override void Set_OutPut_OFF(object notUsed)
        {
            try
            {
                //Module.TempManagerStateTransition(new TempManagerConnWrtDisableState(Module));
                Module.TempModule.Set_OutPut_OFF(notUsed);
                LoggerManager.Debug("[Temp OutPut] Off");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void Set_OutPut_ON(object notUsed)
        {
            try
            {
                Module.TempModule.Set_OutPut_ON(notUsed);
                LoggerManager.Debug("[Temp OutPut] On");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void Set_PB(double value)
        {
            Module.TempModule.Set_PB(value);
        }

        public override void WriteSingleRegister(int data, int value)
        {
            Module.TempModule.WriteSingleRegister(data, value);
        }
    }

    public class TempManagerConnWrtDisableState : TempManagerStateBase
    {
        public TempManagerConnWrtDisableState(ITempManager module) : base(module)
        {
            Module.TempModule.SetRemote_OFF(null);
        }

        public override bool Connect(string serialPort, byte UnitIdentifier)
        {
            bool retVal = false;
            try
            {
                Module.TempModule?.Dispose();

                Module.TempManagerStateTransition(new TempManagerDisConnectedState(Module));
                retVal = Module.Connect(serialPort, UnitIdentifier);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override void Disconnect()
        {
            try
            {
                Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
                Module.Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Dispose()
        {
            try
            {
                Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
                Module.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override TempManagerStateEnum GetState()
        {
            return TempManagerStateEnum.CONN_WRT_DISABLE;
        }

        public override ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            return Module.TempModule.Get_Cur_Temp(dataGetdelay);
        }

        public override double ReadMV()
        {
            return Module.TempModule.ReadMV();
        }

        public override double ReadPV()
        {
            return Module.TempModule.ReadPV();
        }

        public override double ReadSetV()
        {
            return Module.TempModule.ReadSetV();
        }

        public override long ReadStatus()
        {
            return Module.TempModule.ReadStatus();
        }
        public override long Get_OutPut_State()
        {
            return Module.TempModule.Get_OutPut_State();
        }

        public override void SetRemote_OFF(object notUsed)
        {
            Module.TempModule.SetRemote_OFF(notUsed);
        }

        public override void SetRemote_ON(object notUsed)
        {
            try
            {
                Module.TempModule.SetRemote_ON(notUsed);
                Module.TempManagerStateTransition(new TempManagerConnWrtEnableState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetSV(double value)
        {
        }

        public override void Set_DT(double value)
        {
        }

        public override void Set_IT(double value)
        {
        }

        public override void Set_Offset(double value)
        {
        }

        public override void Set_OutPut_OFF(object notUsed)
        {
            try
            {
                Module.TempModule.Set_OutPut_OFF(notUsed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void Set_OutPut_ON(object notUsed)
        {
            try
            {
                Module.TempModule.Set_OutPut_ON(notUsed);
                Module.TempManagerStateTransition(new TempManagerConnWrtEnableState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override void Set_PB(double value)
        {
        }

        public override void WriteSingleRegister(int data, int value)
        {
        }
    }

    public class TempManagerErrorState : TempManagerStateBase
    {
        public TempManagerErrorState(ITempManager module) : base(module)
        {

        }

        public override bool Connect(string serialPort, byte UnitIdentifier)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
            return false;
        }

        public override void Disconnect()
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Dispose()
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override TempManagerStateEnum GetState()
        {
            return TempManagerStateEnum.ERROR;
        }

        public override ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
            return null;
        }

        public override double ReadMV()
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
            return -1;
        }

        public override double ReadPV()
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
            return -1;
        }

        public override double ReadSetV()
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
            return -1;
        }

        public override long ReadStatus()
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
            return -1;
        }

        public override void SetRemote_OFF(object notUsed)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void SetRemote_ON(object notUsed)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void SetSV(double value)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Set_DT(double value)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Set_IT(double value)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Set_Offset(double value)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Set_OutPut_OFF(object notUsed)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Set_OutPut_ON(object notUsed)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void Set_PB(double value)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }

        public override void WriteSingleRegister(int data, int value)
        {
            Module.TempManagerStateTransition(new TempManagerDisConnectingState(Module));
        }
    }
}

//namespace Temporary.TempControl
//{
//    public abstract class TempControlStateBase : TempControlState
//    {
//        protected static Logger Log = NLog.LogManager.GetCurrentClassLogger();

//        private TempController _Module;

//        public TempController Module
//        {
//            get { return _Module; }
//            private set { _Module = value; }
//        }

//        private E5EN _e5en;
//        public E5EN e5en
//        {
//            get { return _e5en; }
//            private set { _e5en = value; }
//        }

//        public TempControlStateBase(TempController module, E5EN e5en)
//        {
//            this.Module = module;
//            this.e5en = e5en;
//        }
//    }

//    public class TempControlDisConnectedState : TempControlStateBase
//    {
//        public TempControlDisConnectedState(TempController module, E5EN e5en) : base(module, e5en)
//        {

//        }

//        public override TempControlStateEnum GetState()
//        {
//            return TempControlStateEnum.DISCONNECTED;
//        }

//        public override bool Connect(string serialPort, byte UnitIdentifier)
//        {
//            bool retVal = e5en?.Connect(serialPort, UnitIdentifier) == true ? true : false;

//            if(retVal)
//            {
//                if(Module.ChangeEnable)
//                    Module.TempControlStateTransition(new TempControlConnWrtEnableState(Module, e5en));
//                else
//                    Module.TempControlStateTransition(new TempControlConnWrtDisableState(Module, e5en));
//            }

//            return retVal;
//        }

//        public override void Disconnect()
//        {
//            LoggerManager.Debug($"Temp. controller has been disconnected.");
//            //throw new NotImplementedException();
//        }

//        public override void Dispose()
//        {
//            throw new NotImplementedException();
//        }

//        public override TempUpdateEventArgs Get_Cur_Temp()
//        {
//            throw new NotImplementedException();
//        }

//        public override long ReadMV()
//        {
//            throw new NotImplementedException();
//        }

//        public override long ReadPV()
//        {
//            throw new NotImplementedException();
//        }

//        public override long ReadSetV()
//        {
//            throw new NotImplementedException();
//        }

//        public override long ReadStatus()
//        {
//            throw new NotImplementedException();
//        }

//        public override void SetRemote_OFF(object notUsed)
//        {
//            throw new NotImplementedException();
//        }

//        public override void SetRemote_ON(object notUsed)
//        {
//            throw new NotImplementedException();
//        }

//        public override void SetSV(double value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_DT(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_IT(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_Offset(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_OutPut_OFF(object notUsed)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_OutPut_ON(object notUsed)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_PB(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void WriteSingleRegister(int data, int value)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class TempControlDisConnectingState : TempControlStateBase
//    {
//        public TempControlDisConnectingState(TempController module, E5EN e5en) : base(module, e5en)
//        {
//        }

//        public override bool Connect(string serialPort, byte UnitIdentifier)
//        {
//            return false;
//        }

//        public override void Disconnect()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Dispose()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override TempControlStateEnum GetState()
//        {
//            return TempControlStateEnum.DISCONNECTING;
//        }

//        public override TempUpdateEventArgs Get_Cur_Temp()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            return null;
//        }

//        public override long ReadMV()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            return -1;
//        }

//        public override long ReadPV()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            return -1;
//        }

//        public override long ReadSetV()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            return -1;
//        }

//        public override long ReadStatus()
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            return -1;
//        }

//        public override void SetRemote_OFF(object notUsed)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void SetRemote_ON(object notUsed)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void SetSV(double value)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Set_DT(string value)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Set_IT(string value)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Set_Offset(string value)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Set_OutPut_OFF(object notUsed)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Set_OutPut_ON(object notUsed)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void Set_PB(string value)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }

//        public override void WriteSingleRegister(int data, int value)
//        {
//            e5en?.Dispose();
//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//        }
//    }

//    public class TempControlConnWrtEnableState : TempControlStateBase
//    {
//        public TempControlConnWrtEnableState(TempController module, E5EN e5en) : base(module, e5en)
//        {
//            e5en.SetRemote_ON(null);
//        }

//        public override bool Connect(string serialPort, byte UnitIdentifier)
//        {
//            bool retVal = false;
//            e5en?.Dispose();

//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            retVal = Module.Connect(serialPort, UnitIdentifier);

//            return retVal;
//        }

//        public override void Disconnect()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            Module.Disconnect();
//        }

//        public override void Dispose()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            Module.Dispose();
//        }

//        public override TempControlStateEnum GetState()
//        {
//            return TempControlStateEnum.CONN_WRT_ENABLE;
//        }

//        public override TempUpdateEventArgs Get_Cur_Temp()
//        {
//            return e5en.Get_Cur_Temp();
//        }

//        public override long ReadMV()
//        {
//            return e5en.ReadMV();
//        }

//        public override long ReadPV()
//        {
//            return e5en.ReadPV();
//        }

//        public override long ReadSetV()
//        {
//            return e5en.ReadSetV();
//        }

//        public override long ReadStatus()
//        {
//            return e5en.ReadStatus();
//        }

//        public override void SetRemote_OFF(object notUsed)
//        {
//            e5en.SetRemote_OFF(notUsed);
//            Module.TempControlStateTransition(new TempControlConnWrtDisableState(Module, e5en));
//        }

//        public override void SetRemote_ON(object notUsed)
//        {
//            e5en.SetRemote_ON(notUsed);
//        }

//        public override void SetSV(double value)
//        {
//            e5en.SetSV(value);
//        }

//        public override void Set_DT(string value)
//        {
//            e5en.Set_DT(value);
//        }

//        public override void Set_IT(string value)
//        {
//            e5en.Set_IT(value);
//        }

//        public override void Set_Offset(string value)
//        {
//            e5en.Set_Offset(value);
//        }

//        public override void Set_OutPut_OFF(object notUsed)
//        {
//            e5en.Set_OutPut_OFF(notUsed);
//        }

//        public override void Set_OutPut_ON(object notUsed)
//        {
//            e5en.Set_OutPut_ON(notUsed);
//        }

//        public override void Set_PB(string value)
//        {
//            e5en.Set_PB(value);
//        }

//        public override void WriteSingleRegister(int data, int value)
//        {
//            e5en.WriteSingleRegister(data, value);
//        }
//    }

//    public class TempControlConnWrtDisableState : TempControlStateBase
//    {
//        public TempControlConnWrtDisableState(TempController module, E5EN e5en) : base(module, e5en)
//        {
//            e5en.SetRemote_OFF(null);
//        }

//        public override bool Connect(string serialPort, byte UnitIdentifier)
//        {
//            bool retVal = false;
//            e5en?.Dispose();

//            Module.TempControlStateTransition(new TempControlDisConnectedState(Module, e5en));
//            retVal = Module.Connect(serialPort, UnitIdentifier);

//            return retVal;
//        }

//        public override void Disconnect()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            Module.Disconnect();
//        }

//        public override void Dispose()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            Module.Dispose();
//        }

//        public override TempControlStateEnum GetState()
//        {
//            return TempControlStateEnum.CONN_WRT_DISABLE;
//        }

//        public override TempUpdateEventArgs Get_Cur_Temp()
//        {
//            return e5en.Get_Cur_Temp();
//        }

//        public override long ReadMV()
//        {
//            return e5en.ReadMV();
//        }

//        public override long ReadPV()
//        {
//            return e5en.ReadPV();
//        }

//        public override long ReadSetV()
//        {
//            return e5en.ReadSetV();
//        }

//        public override long ReadStatus()
//        {
//            return e5en.ReadStatus();
//        }

//        public override void SetRemote_OFF(object notUsed)
//        {
//            e5en.SetRemote_OFF(notUsed);
//        }

//        public override void SetRemote_ON(object notUsed)
//        {
//            e5en.SetRemote_ON(notUsed);
//            Module.TempControlStateTransition(new TempControlConnWrtEnableState(Module, e5en));
//        }

//        public override void SetSV(double value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_DT(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_IT(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_Offset(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Set_OutPut_OFF(object notUsed)
//        {
//            e5en.Set_OutPut_OFF(notUsed);
//        }

//        public override void Set_OutPut_ON(object notUsed)
//        {
//            e5en.Set_OutPut_ON(notUsed);
//        }

//        public override void Set_PB(string value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void WriteSingleRegister(int data, int value)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class TempControlErrorState : TempControlStateBase
//    {
//        public TempControlErrorState(TempController module, E5EN e5en) : base(module, e5en)
//        {

//        }

//        public override bool Connect(string serialPort, byte UnitIdentifier)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            return false;
//        }

//        public override void Disconnect()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Dispose()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override TempControlStateEnum GetState()
//        {
//            return TempControlStateEnum.ERROR;
//        }

//        public override TempUpdateEventArgs Get_Cur_Temp()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            return null;
//        }

//        public override long ReadMV()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            return -1;
//        }

//        public override long ReadPV()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            return -1;
//        }

//        public override long ReadSetV()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            return -1;
//        }

//        public override long ReadStatus()
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//            return -1;
//        }

//        public override void SetRemote_OFF(object notUsed)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void SetRemote_ON(object notUsed)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void SetSV(double value)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Set_DT(string value)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Set_IT(string value)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Set_Offset(string value)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Set_OutPut_OFF(object notUsed)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Set_OutPut_ON(object notUsed)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void Set_PB(string value)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }

//        public override void WriteSingleRegister(int data, int value)
//        {
//            Module.TempControlStateTransition(new TempControlDisConnectingState(Module, e5en));
//        }
//    }
//}
