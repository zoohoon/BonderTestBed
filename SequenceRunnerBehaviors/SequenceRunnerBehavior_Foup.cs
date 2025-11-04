using CylType;
using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.SequenceRunner;
using SequenceRunner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SequenceRunnerBehaviors
{
    [Serializable]
    public class InitDPOutMovingState : SequenceBehavior
    {
        public InitDPOutMovingState()
        {
            try
            {
                this.description = nameof(InitDPOutMovingState);
                this.SequenceDescription = "Init DPOut Moving State";
                //this.InputPorts
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitDPOutMoveDoneState : SequenceBehavior
    {
        public InitDPOutMoveDoneState()
        {
            try
            {
                this.description = nameof(InitDPOutMoveDoneState);
                this.SequenceDescription = "Init DPOut Move Done State";
                //this.InputPorts
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }



    [Serializable]
    public class InitDPInMovingState : SequenceBehavior
    {
        public InitDPInMovingState()
        {
            try
            {
                this.description = nameof(InitDPInMovingState);
                this.SequenceDescription = "Init DPIn Moving State";
                //this.InputPorts
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CheckCstExist : SequenceBehavior
    {
        public CheckCstExist()
        {
            try
            {
                this.description = nameof(CheckCstExist);
                this.SequenceDescription = "Check Cassette Exist On Docking Port";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstExist8Inch : SequenceBehavior
    {
        public CheckCstExist8Inch()
        {
            try
            {
                this.description = nameof(CheckCstExist8Inch);
                this.SequenceDescription = "Check Cassette(8Inch) Exist On Docking Port";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckHomed : SequenceBehavior
    {
        public CheckHomed()
        {
            try
            {
                this.description = nameof(CheckHomed);
                this.SequenceDescription = "Check Homing Completed";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CoverLockOn : SequenceBehavior
    {
        public CoverLockOn()
        {
            try
            {
                this.description = nameof(CoverLockOn);
                this.SequenceDescription = "Cover Lock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstCoverLock : SequenceBehavior
    {
        public CheckCstCoverLock()
        {
            try
            {
                this.description = nameof(CheckCstCoverLock);
                this.SequenceDescription = "Check Cassette Cover Locked";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CstLoadOn : SequenceBehavior
    {
        public CstLoadOn()
        {
            try
            {
                this.description = nameof(CstLoadOn);
                this.SequenceDescription = "Cassette Load";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CstUnloadOn : SequenceBehavior
    {
        public CstUnloadOn()
        {
            try
            {
                this.description = nameof(CstUnloadOn);
                this.SequenceDescription = "Cassette Unload";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CheckCstLoadOn : SequenceBehavior
    {
        public CheckCstLoadOn()
        {
            try
            {
                this.description = nameof(CheckCstLoadOn);
                this.SequenceDescription = "Check Cassette Load Sensor Is On";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstLoadOff : SequenceBehavior
    {
        public CheckCstLoadOff()
        {
            try
            {
                this.description = nameof(CheckCstLoadOff);
                this.SequenceDescription = "Check Cassette Load Sensor Is Off";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstUnloadOn : SequenceBehavior
    {
        public CheckCstUnloadOn()
        {
            try
            {
                this.description = nameof(CheckCstUnloadOn);
                this.SequenceDescription = "Check Cassette Unload Sensor Is On";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class InitDPInMoveDoneState : SequenceBehavior
    {
        public InitDPInMoveDoneState()
        {
            try
            {
                this.description = nameof(InitDPInMoveDoneState);
                this.SequenceDescription = "Init DPIn Move Done State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class InitCoverUnlockingState : SequenceBehavior
    {
        public InitCoverUnlockingState()
        {
            try
            {
                this.description = nameof(InitCoverUnlockingState);
                this.SequenceDescription = "Init Cover Unlocking State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstNotExist : SequenceBehavior
    {
        public CheckCstNotExist()
        {
            try
            {
                this.description = nameof(CheckCstNotExist);
                this.SequenceDescription = "Check Cassette Not Exist On Docking Port";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCoverNotExist : SequenceBehavior
    {
        public CheckCoverNotExist()
        {
            try
            {
                this.description = nameof(CheckCoverNotExist);
                this.SequenceDescription = "Check Cassette Cover Not Exist On FoupDoor";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CstMappingOff : SequenceBehavior
    {
        public CstMappingOff()
        {
            try
            {
                this.description = nameof(CstMappingOff);
                this.SequenceDescription = "Cassette Mapping Sensor Off";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstMappingOff : SequenceBehavior
    {
        public CheckCstMappingOff()
        {
            try
            {
                this.description = nameof(CheckCstMappingOff);
                this.SequenceDescription = "Check Cassette Mapping Holded";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckDownPos : SequenceBehavior
    {
        public CheckDownPos()
        {
            try
            {
                this.description = nameof(CheckDownPos);
                this.SequenceDescription = "Check FoupDoor DownPos";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCoverOpen : SequenceBehavior
    {
        public CheckCoverOpen()
        {
            try
            {
                this.description = nameof(CheckCoverOpen);
                this.SequenceDescription = "Check FoupDoor Open";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }



    [Serializable]
    public class MoveDownPos : SequenceBehavior
    {
        public MoveDownPos()
        {
            try
            {
                this.description = nameof(MoveDownPos);
                this.SequenceDescription = "Move DownPos";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }



    [Serializable]
    public class CheckUpPos : SequenceBehavior
    {
        public CheckUpPos()
        {
            try
            {
                this.description = nameof(CheckUpPos);
                this.SequenceDescription = "Check FoupDoor UpPos";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class MoveUpPos : SequenceBehavior
    {
        public MoveUpPos()
        {
            try
            {
                this.description = nameof(MoveUpPos);
                this.SequenceDescription = "Move UpPos";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CoverCloseOn : SequenceBehavior
    {
        public CoverCloseOn()
        {
            try
            {
                this.description = nameof(CoverCloseOn);
                this.SequenceDescription = "Cover Close";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckNoWaferOut : SequenceBehavior
    {
        public CheckNoWaferOut()
        {
            try
            {
                this.description = nameof(CheckNoWaferOut);
                this.SequenceDescription = "Check There Is No Wafer Out";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCoverClose : SequenceBehavior
    {
        public CheckCoverClose()
        {
            try
            {
                this.description = nameof(CheckCoverClose);
                this.SequenceDescription = "Check FoupDoor Close";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstLock : SequenceBehavior
    {
        public CheckCstLock()
        {
            try
            {
                this.description = nameof(CheckCstLock);
                this.SequenceDescription = "Check Cassette Clamp Lock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstLock8Inch : SequenceBehavior
    {
        public CheckCstLock8Inch()
        {
            try
            {
                this.description = nameof(CheckCstLock8Inch);
                this.SequenceDescription = "Check Cassette Clamp Lock(8Inch)";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstLock6Inch : SequenceBehavior
    {
        public CheckCstLock6Inch()
        {
            try
            {
                this.description = nameof(CheckCstLock6Inch);
                this.SequenceDescription = "Check Cassette Clamp Lock(6Inch)";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstUnlock : SequenceBehavior
    {
        public CheckCstUnlock()
        {
            try
            {
                this.description = nameof(CheckCstUnlock);
                this.SequenceDescription = "Check Cassette Clamp Unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CheckCstUnlock8Inch : SequenceBehavior
    {
        public CheckCstUnlock8Inch()
        {
            try
            {
                this.description = nameof(CheckCstUnlock8Inch);
                this.SequenceDescription = "Check Cassette Clamp Unlock(8Inch)";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CstVacOn : SequenceBehavior
    {
        public CstVacOn()
        {
            try
            {
                this.description = nameof(CstVacOn);
                this.SequenceDescription = "Cassette Vacuum On";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCoverVac : SequenceBehavior
    {
        public CheckCoverVac()
        {
            try
            {
                this.description = nameof(CheckCoverVac);
                this.SequenceDescription = "Check Cassette Cover Vacuum Detected";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CheckCoverUnlock : SequenceBehavior
    {
        public CheckCoverUnlock()
        {
            try
            {
                this.description = nameof(CheckCoverUnlock);
                this.SequenceDescription = "Check Cassette Unlocked";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CstVacOff : SequenceBehavior
    {
        public CstVacOff()
        {
            try
            {
                this.description = nameof(CstVacOff);
                this.SequenceDescription = "Cassette Cover Vacuum Off";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCoverLock : SequenceBehavior
    {
        public CheckCoverLock()
        {
            try
            {
                this.description = nameof(CheckCoverLock);
                this.SequenceDescription = "Check Cover Lock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class CheckCstVacOff : SequenceBehavior
    {
        public CheckCstVacOff()
        {
            try
            {
                this.description = nameof(CheckCstVacOff);
                this.SequenceDescription = "Check Cassette Vacuum Not Detected";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckCstVacOn : SequenceBehavior
    {
        public CheckCstVacOn()
        {
            try
            {
                this.description = nameof(CheckCstVacOn);
                this.SequenceDescription = "Check Cassette Vacuum Detected";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCoverUnlockedState : SequenceBehavior
    {
        public InitCoverUnlockedState()
        {
            try
            {
                this.description = nameof(InitCoverUnlockedState);
                this.SequenceDescription = "Init Cover Unlocked State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCoverClosingState : SequenceBehavior
    {
        public InitCoverClosingState()
        {
            try
            {
                this.description = nameof(InitCoverClosingState);
                this.SequenceDescription = "Init Cover Unlocking State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CoverOpenOn : SequenceBehavior
    {
        public CoverOpenOn()
        {
            try
            {
                this.description = nameof(CoverOpenOn);
                this.SequenceDescription = "FoupDoor Open";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCoverClosedState : SequenceBehavior
    {
        public InitCoverClosedState()
        {
            try
            {
                this.description = nameof(InitCoverClosedState);
                this.SequenceDescription = "Init Cover Closed State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCoverLockingState : SequenceBehavior
    {
        public InitCoverLockingState()
        {
            try
            {
                this.description = nameof(InitCoverLockingState);
                this.SequenceDescription = "Init Cover Locking State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CoverUnlockOn : SequenceBehavior
    {
        public CoverUnlockOn()
        {
            try
            {
                this.description = nameof(CoverUnlockOn);
                this.SequenceDescription = "Cover Unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCoverLockedState : SequenceBehavior
    {
        public InitCoverLockedState()
        {
            try
            {
                this.description = nameof(InitCoverLockedState);
                this.SequenceDescription = "Init Cover Locked State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCstLockingState : SequenceBehavior
    {
        public InitCstLockingState()
        {
            try
            {
                this.description = nameof(InitCstLockingState);
                this.SequenceDescription = "Init Cassette Locking State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class LockCst : SequenceBehavior
    {
        public LockCst()
        {
            try
            {
                this.description = nameof(LockCst);
                this.SequenceDescription = "Clamp Lock Cassette";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class LockCst8Inch : SequenceBehavior
    {
        public LockCst8Inch()
        {
            try
            {
                this.description = nameof(LockCst8Inch);
                this.SequenceDescription = "Clamp Lock(8Inch) Cassette";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class UnlockCst : SequenceBehavior
    {
        public UnlockCst()
        {
            try
            {
                this.description = nameof(UnlockCst);
                this.SequenceDescription = "Clamp Unlock Cassette";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class UnlockCst8Inch : SequenceBehavior
    {
        public UnlockCst8Inch()
        {
            try
            {
                this.description = nameof(UnlockCst8Inch);
                this.SequenceDescription = "Clamp Unlock(8Inch) Cassette";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCstLockedState : SequenceBehavior
    {
        public InitCstLockedState()
        {
            try
            {
                this.description = nameof(InitCstLockedState);
                this.SequenceDescription = "Init Cassette Locked State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class InitCstUnlockingState : SequenceBehavior
    {
        public InitCstUnlockingState()
        {
            try
            {
                this.description = nameof(InitCstUnlockingState);
                this.SequenceDescription = "Init Cassette Unlocking State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class InitCstUnlockedState : SequenceBehavior
    {
        public InitCstUnlockedState()
        {
            try
            {
                this.description = nameof(InitCstUnlockedState);
                this.SequenceDescription = "Init Cassette Unlocked State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    [Serializable]
    public class InitCoverOpeningState : SequenceBehavior
    {
        public InitCoverOpeningState()
        {
            try
            {
                this.description = nameof(InitCoverOpeningState);
                this.SequenceDescription = "Init Cover Opening State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class InitCoverOpenedState : SequenceBehavior
    {
        public InitCoverOpenedState()
        {
            try
            {
                this.description = nameof(InitCoverOpenedState);
                this.SequenceDescription = "Init Cover Opened State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class DoneState : SequenceBehavior
    {
        public DoneState()
        {
            try
            {
                this.description = nameof(DoneState);
                this.SequenceDescription = "Done State";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
}
