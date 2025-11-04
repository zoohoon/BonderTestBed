using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.SequenceRunner;
using SequenceRunner;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SequenceRunnerBehaviors
{
    [Serializable]
    public class FrontDoorLock : SequenceBehavior
    {
        public FrontDoorLock()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.FrontDoorLock;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = new FrontDoorUnlock();

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

                IIOManager ioManager = this.IOManager();
                Type _type = null;
                PropertyInfo _propertyInfo = null;

                ///////////////////////////////////
                _type = ioManager.IO.Outputs.GetType();
                _propertyInfo = _type.GetProperty("DOFDOOR_LOCK");
                IOPortDescripter<bool> DOFDOOR_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Outputs);
                OutputPorts.Add(DOFDOOR_LOCK);

                _propertyInfo = _type.GetProperty("DOFDOOR_UNLOCK");
                IOPortDescripter<bool> DOFDOOR_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Outputs);
                OutputPorts.Add(DOFDOOR_UNLOCK);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            IIOManager ioManager = this.IOManager();
            try
            {
                IOPortDescripter<bool> DOFDOOR_LOCK = OutputPorts.ToList().Find(io => io.Key.Value.Equals("DOFDOOR_LOCK"));
                IOPortDescripter<bool> DOFDOOR_UNLOCK = OutputPorts.ToList().Find(io => io.Key.Value.Equals("DOFDOOR_UNLOCK"));

                ioManager.IOServ.WriteBit(DOFDOOR_LOCK, true);
                ioManager.IOServ.WriteBit(DOFDOOR_UNLOCK, false);

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class FrontDoorUnlock : SequenceBehavior
    {
        public FrontDoorUnlock()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.FrontDoorUnlock;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = new FrontDoorLock();

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

                IIOManager ioManager = this.IOManager();
                Type _type = null;
                PropertyInfo _propertyInfo = null;

                ///////////////////////////////////
                _type = ioManager.IO.Outputs.GetType();
                _propertyInfo = _type.GetProperty("DOFDOOR_LOCK");
                IOPortDescripter<bool> DOFDOOR_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Outputs);
                OutputPorts.Add(DOFDOOR_LOCK);

                _propertyInfo = _type.GetProperty("DOFDOOR_UNLOCK");
                IOPortDescripter<bool> DOFDOOR_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Outputs);
                OutputPorts.Add(DOFDOOR_UNLOCK);

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            IIOManager ioManager = this.IOManager();
            try
            {
                IOPortDescripter<bool> DOFDOOR_LOCK = OutputPorts.ToList().Find(io => io.Key.Value.Equals("DOFDOOR_LOCK"));
                IOPortDescripter<bool> DOFDOOR_UNLOCK = OutputPorts.ToList().Find(io => io.Key.Value.Equals("DOFDOOR_UNLOCK"));

                ioManager.IOServ.WriteBit(DOFDOOR_LOCK, false);
                ioManager.IOServ.WriteBit(DOFDOOR_UNLOCK, true);

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class WaitForFrontDoorOpen : SequenceBehavior
    {
        public WaitForFrontDoorOpen()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.WaitForFrontDoorOpen;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

                IIOManager ioManager = this.IOManager();
                Type _type = ioManager.IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIFRONTDOOROPEN");
                IOPortDescripter<bool> DIFRONTDOOROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Inputs);
                InputPorts.Add(DIFRONTDOOROPEN);

                _propertyInfo = _type.GetProperty("DIFRONTDOORCLOSE");
                IOPortDescripter<bool> DIFRONTDOORCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Inputs);
                InputPorts.Add(DIFRONTDOORCLOSE);

                ///////////////////////////////////

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            IIOManager ioManager = this.IOManager();
            try
            {
                //1. FrontDoor가 Open 되었는지 확인
                //2. 되었으면 통과.
                //3. 안되었으면 다시 Dialog확인
                //3-1. ok는 체크.
                //3-2. cancel는 취소.

                IOPortDescripter<bool> DIFRONTDOOROPEN = InputPorts.ToList().Find(io => io.Key.Value.Equals("DIFRONTDOOROPEN"));
                IOPortDescripter<bool> DIFRONTDOORCLOSE = InputPorts.ToList().Find(io => io.Key.Value.Equals("DIFRONTDOORCLOSE"));

                if(DIFRONTDOOROPEN != null && DIFRONTDOORCLOSE != null)
                {
                    bool isFrontdoorOpen = false;
                    bool isFrontdoorClose = false;
                    bool bRun = true;
                    do
                    {
                        isFrontdoorOpen = DIFRONTDOOROPEN.Value;
                        isFrontdoorClose = DIFRONTDOORCLOSE.Value;

                        if (isFrontdoorOpen && !isFrontdoorClose)
                        {
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            bRun = false;
                        }
                        else
                        {

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", "Check opened the front door of stage.", EnumMessageStyle.AffirmativeAndNegative, "YES", "NO");

                            if (result == EnumMessageDialogResult.NEGATIVE)
                            {
                                retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                                bRun = false;
                            }
                        }
                    } while (bRun);
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class WaitForFrontDoorClose : SequenceBehavior
    {
        public WaitForFrontDoorClose()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.WaitForFrontDoorClose;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

                IIOManager ioManager = this.IOManager();
                Type _type = ioManager.IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIFRONTDOOROPEN");
                IOPortDescripter<bool> DIFRONTDOOROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Inputs);
                InputPorts.Add(DIFRONTDOOROPEN);

                _propertyInfo = _type.GetProperty("DIFRONTDOORCLOSE");
                IOPortDescripter<bool> DIFRONTDOORCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Inputs);
                InputPorts.Add(DIFRONTDOORCLOSE);

                ///////////////////////////////////

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            IIOManager ioManager = this.IOManager();
            try
            {
                //1. FrontDoor가 Close 되었는지 확인
                //2. 되었으면 통과.
                //3. 안되었으면 다시 Dialog확인
                //3-1. ok는 체크.
                //3-2. cancel는 취소.

                IOPortDescripter<bool> DIFRONTDOOROPEN = InputPorts.ToList().Find(io => io.Key.Value.Equals("DIFRONTDOOROPEN"));
                IOPortDescripter<bool> DIFRONTDOORCLOSE = InputPorts.ToList().Find(io => io.Key.Value.Equals("DIFRONTDOORCLOSE"));

                if (DIFRONTDOOROPEN != null && DIFRONTDOORCLOSE != null)
                {
                    bool isFrontdoorOpen = false;
                    bool isFrontdoorClose = false;
                    bool bRun = true;
                    do
                    {
                        isFrontdoorOpen = DIFRONTDOOROPEN.Value;
                        isFrontdoorClose = DIFRONTDOORCLOSE.Value;

                        if (!isFrontdoorOpen && isFrontdoorClose)
                        {
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            bRun = false;
                        }
                        else
                        {
                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", "Check closed the front door of stage.", EnumMessageStyle.AffirmativeAndNegative, "YES", "NO");

                            if (result == EnumMessageDialogResult.NEGATIVE)
                            {
                                retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                                bRun = false;
                            }
                        }
                    } while (bRun);
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class NCPadVacOn : SequenceBehavior
    {
        public NCPadVacOn()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.NCPadVacOn;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;
                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

                IIOManager ioManager = this.IOManager();
                Type _type = ioManager.IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLEANUNITVAC");
                IOPortDescripter<bool> DICLEANUNITVAC = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Inputs);
                InputPorts.Add(DICLEANUNITVAC);

                ///////////////////////////////////

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            IIOManager ioManager = this.IOManager();
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONEEDLECLEANAIRON, true);
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class NCPadVacOff : SequenceBehavior
    {
        public NCPadVacOff()
        {
        }

        public override string ToString()
        {
            return Properties.Resources.NCPadVacOff;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;
                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

                IIOManager ioManager = this.IOManager();
                Type _type = ioManager.IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLEANUNITVAC");
                IOPortDescripter<bool> DICLEANUNITVAC = (IOPortDescripter<bool>)_propertyInfo.GetValue(ioManager.IO.Inputs);
                InputPorts.Add(DICLEANUNITVAC);

                ///////////////////////////////////

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            IIOManager ioManager = this.IOManager();
            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DONEEDLECLEANAIRON, false);
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
}
