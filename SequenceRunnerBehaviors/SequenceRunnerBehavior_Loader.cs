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
    /// <summary>
    /// WaferOut seonsor, Target 갯수 체크 등등.
    /// </summary>
    [Serializable]
    public class PreCheck : SequenceBehavior
    {
        public PreCheck()
        {
            try
            {
                this.description = nameof(PreCheck);
                this.SequenceDescription = "PreCheck";
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

    /// <summary>
    /// Pick 하기 전 Target 베쿰 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Target_BeforePick : SequenceBehavior
    {
        public CheckVac_Target_BeforePick()
        {
            try
            {
                this.description = nameof(CheckVac_Target_BeforePick);
                this.SequenceDescription = "CheckVac Target BeforePick";
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

    /// <summary>
    /// Pick 이후 Target 베큠 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Target_AfterPick : SequenceBehavior
    {
        public CheckVac_Target_AfterPick()
        {
            try
            {
                this.description = nameof(CheckVac_Target_AfterPick);
                this.SequenceDescription = "CheckVac Target AfterPick";
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

    /// <summary>
    /// Put 하기 전 Target 베쿰 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Target_BeforePut : SequenceBehavior
    {
        public CheckVac_Target_BeforePut()
        {
            try
            {
                this.description = nameof(CheckVac_Target_BeforePut);
                this.SequenceDescription = "CheckVac Target BeforePut";
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

    /// <summary>
    /// Put 이후 Target 베큠 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Target_AfterPut : SequenceBehavior
    {
        public CheckVac_Target_AfterPut()
        {
            try
            {
                this.description = nameof(CheckVac_Target_AfterPut);
                this.SequenceDescription = "CheckVac Target AfterPut";
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

    /// <summary>
    /// Arm을 안전한 위치로 접어 넣는다.
    /// </summary>
    [Serializable]
    public class Move_SafePos_Arm : SequenceBehavior
    {
        public Move_SafePos_Arm()
        {
            try
            {
                this.description = nameof(Move_SafePos_Arm);
                this.SequenceDescription = "MoveSafePos Arm";
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

    /// <summary>
    /// Target Pos로 Arm을 뻗는다.
    /// </summary>
    [Serializable]
    public class Move_TargetPos_Arm : SequenceBehavior
    {
        public Move_TargetPos_Arm()
        {
            try
            {
                this.description = nameof(Move_TargetPos_Arm);
                this.SequenceDescription = "MoveTargetPos Arm";
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

    /// <summary>
    /// Pick 하기 전 Arm 베쿰 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Arm_BeforePick : SequenceBehavior
    {
        public CheckVac_Arm_BeforePick()
        {
            try
            {
                this.description = nameof(CheckVac_Arm_BeforePick);
                this.SequenceDescription = "CheckVac Arm BeforePick";
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

    /// <summary>
    /// Pick 이후 Arm 베쿰 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Arm_AfterPick : SequenceBehavior
    {
        public CheckVac_Arm_AfterPick()
        {
            try
            {
                this.description = nameof(CheckVac_Arm_AfterPick);
                this.SequenceDescription = "CheckVac Arm AfterPick";
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

    /// <summary>
    /// Put 하기 전 Arm 베큠 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Arm_BeforePut : SequenceBehavior
    {
        public CheckVac_Arm_BeforePut()
        {
            try
            {
                this.description = nameof(CheckVac_Arm_BeforePut);
                this.SequenceDescription = "CheckVac Arm BeforePut";
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

    /// <summary>
    /// Put 이후 Arm 베큠 체크
    /// </summary>
    [Serializable]
    public class CheckVac_Arm_AfterPut : SequenceBehavior
    {
        public CheckVac_Arm_AfterPut()
        {
            try
            {
                this.description = nameof(CheckVac_Arm_AfterPut);
                this.SequenceDescription = "CheckVac Arm AfterPut";
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

    /// <summary>
    /// Foup Homing 상태 체크
    /// </summary>
    [Serializable]
    public class Check_Foup_HomingState : SequenceBehavior
    {
        public Check_Foup_HomingState()
        {
            try
            {
                this.description = nameof(Check_Foup_HomingState);
                this.SequenceDescription = "Check Foup HomingState";
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

    /// <summary>
    /// Pick Pos로 이동한다. (LX, LZ, LW축)
    /// </summary>
    [Serializable]
    public class Move_PickPos_LX_LZ_LW : SequenceBehavior
    {
        public Move_PickPos_LX_LZ_LW()
        {
            try
            {
                this.description = nameof(Move_PickPos_LX_LZ_LW);
                this.SequenceDescription = "Move PickPos LX_LZ_LW";
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

    /// <summary>
    /// Put Pos로 이동한다. (LX, LZ, LW축)
    /// </summary>
    [Serializable]
    public class Move_PutPos_LX_LZ_LW : SequenceBehavior
    {
        public Move_PutPos_LX_LZ_LW()
        {
            try
            {
                this.description = nameof(Move_PutPos_LX_LZ_LW);
                this.SequenceDescription = "Move PutPos LX_LZ_LW";
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

    /// <summary>
    /// CardID Reading Pos로 이동한다. (LX, LZ, LW축)
    /// </summary>
    [Serializable]
    public class Move_CardIDReadingPos_LX_LZ_LW : SequenceBehavior
    {
        public Move_CardIDReadingPos_LX_LZ_LW()
        {
            try
            {
                this.description = nameof(Move_CardIDReadingPos_LX_LZ_LW);
                this.SequenceDescription = "Move CardIDReadingPos LX_LZ_LW";
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

    /// <summary>
    /// Pick pos로 이동한다. (LT, LW)
    /// </summary>
    [Serializable]
    public class Move_PickPos_LT_LW : SequenceBehavior
    {
        public Move_PickPos_LT_LW()
        {
            try
            {
                this.description = nameof(Move_PickPos_LT_LW);
                this.SequenceDescription = "Move PickPos LT_LW";
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

    /// <summary>
    /// Put pos로 이동한다. (LT, LW)
    /// </summary>
    [Serializable]
    public class Move_PutPos_LT_LW : SequenceBehavior
    {
        public Move_PutPos_LT_LW()
        {
            try
            {
                this.description = nameof(Move_PutPos_LT_LW);
                this.SequenceDescription = "Move PutPos LT_LW";
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

    /// <summary>
    /// Arm을 뒤로 뺀다.
    /// </summary>
    [Serializable]
    public class Move_BackPos_Arm : SequenceBehavior
    {
        public Move_BackPos_Arm()
        {
            try
            {
                this.description = nameof(Move_BackPos_Arm);
                this.SequenceDescription = "Move BackPos Arm";
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

    /// <summary>
    /// Pick or Put 동작의 끝
    /// </summary>
    [Serializable]
    public class PickPut_Done : SequenceBehavior
    {
        public PickPut_Done()
        {
            try
            {
                this.description = nameof(PickPut_Done);
                this.SequenceDescription = "PickPut Done";
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

    /// <summary>
    /// LZ축을 올린다.
    /// </summary>
    [Serializable]
    public class Move_Up_LZ : SequenceBehavior
    {
        public Move_Up_LZ()
        {
            try
            {
                this.description = nameof(Move_Up_LZ);
                this.SequenceDescription = "Move Up LZ";
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

    /// <summary>
    /// LZ축을 내린다.
    /// </summary>
    [Serializable]
    public class Move_Down_LZ : SequenceBehavior
    {
        public Move_Down_LZ()
        {
            try
            {
                this.description = nameof(Move_Down_LZ);
                this.SequenceDescription = "Move Down LZ";
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

    /// <summary>
    /// LZ축을 올린다.(Bernoulli)
    /// </summary>
    [Serializable]
    public class Move_Up_LZ_UsingBernoulli : SequenceBehavior
    {
        public Move_Up_LZ_UsingBernoulli()
        {
            try
            {
                this.description = nameof(Move_Up_LZ_UsingBernoulli);
                this.SequenceDescription = "Move Up LZ UsingBernoulli";
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

    /// <summary>
    /// LZ축을 내린다.(Bernoulli)
    /// </summary>
    [Serializable]
    public class Move_Down_LZ_UsingBernoulli: SequenceBehavior
    {
        public Move_Down_LZ_UsingBernoulli()
        {
            try
            {
                this.description = nameof(Move_Down_LZ_UsingBernoulli);
                this.SequenceDescription = "Move Down LZ UsingBernoulli";
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

    /// <summary>
    /// LT축을 올린다.(버퍼축)
    /// </summary>
    [Serializable]
    public class Move_Up_LT : SequenceBehavior
    {
        public Move_Up_LT()
        {
            try
            {
                this.description = nameof(Move_Up_LT);
                this.SequenceDescription = "Move Up LT";
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

    /// <summary>
    /// LT축을 내린다.(버퍼축)
    /// </summary>
    [Serializable]
    public class Move_Down_LT : SequenceBehavior
    {
        public Move_Down_LT()
        {
            try
            {
                this.description = nameof(Move_Down_LT);
                this.SequenceDescription = "Move Down LT";
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

    /// <summary>
    /// Error가 나지 않는 특정 동작에만 있는 Step (ex. Timer로 대기 하는 시퀀스)
    /// </summary>
    [Serializable]
    public class No_Error_Sequence : SequenceBehavior
    {
        public No_Error_Sequence()
        {
            try
            {
                this.description = nameof(No_Error_Sequence);
                this.SequenceDescription = "No Error Sequence";
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

    /// <summary>
    /// 바코드 트리거 On
    /// </summary>
    [Serializable]
    public class Barcode_Trig_ON : SequenceBehavior
    {
        public Barcode_Trig_ON()
        {
            try
            {
                this.description = nameof(Barcode_Trig_ON);
                this.SequenceDescription = "Barcode Trig ON";
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

    /// <summary>
    /// 바코드 트리거 Off
    /// </summary>
    [Serializable]
    public class Barcode_Trig_OFF : SequenceBehavior
    {
        public Barcode_Trig_OFF()
        {
            try
            {
                this.description = nameof(Barcode_Trig_OFF);
                this.SequenceDescription = "Barcode Trig OFF";
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

    /// <summary>
    /// 카드트레이가 닫혀 있나 체크
    /// </summary>
    [Serializable]
    public class Check_Close_CardTray : SequenceBehavior
    {
        public Check_Close_CardTray()
        {
            try
            {
                this.description = nameof(Check_Close_CardTray);
                this.SequenceDescription = "Check_Close_CardTray";
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

    /// <summary>
    /// 카드트레이가 열기 (for MPT)
    /// </summary>
    [Serializable]
    public class Open_CardTray : SequenceBehavior
    {
        public Open_CardTray()
        {
            try
            {
                this.description = nameof(Open_CardTray);
                this.SequenceDescription = "Open CardTray";
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

    /// <summary>
    /// 카드트레이가 닫기 (for MPT)
    /// </summary>
    [Serializable]
    public class Close_CardTray : SequenceBehavior
    {
        public Close_CardTray()
        {
            try
            {
                this.description = nameof(Close_CardTray);
                this.SequenceDescription = "Close CardTray";
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

    /// <summary>
    /// Arm Vac On
    /// </summary>
    [Serializable]
    public class Vac_On_Arm : SequenceBehavior
    {
        public Vac_On_Arm()
        {
            try
            {
                this.description = nameof(Vac_On_Arm);
                this.SequenceDescription = "Vac On Arm";
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

    /// <summary>
    /// Arm Vac Off
    /// </summary>
    [Serializable]
    public class Vac_Off_Arm : SequenceBehavior
    {
        public Vac_Off_Arm()
        {
            try
            {
                this.description = nameof(Vac_Off_Arm);
                this.SequenceDescription = "Vac Off Arm";
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

    /// <summary>
    /// Target Vac On (Buffer, Inspection Tray...)
    /// </summary>
    [Serializable]
    public class Vac_On_Target : SequenceBehavior
    {
        public Vac_On_Target()
        {
            try
            {
                this.description = nameof(Vac_On_Target);
                this.SequenceDescription = "Vac On Target";
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

    /// <summary>
    /// Target Vac Off (Buffer, Inspection Tray...)
    /// </summary>
    [Serializable]
    public class Vac_Off_Target : SequenceBehavior
    {
        public Vac_Off_Target()
        {
            try
            {
                this.description = nameof(Vac_Off_Target);
                this.SequenceDescription = "Vac_Off_Target";
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

    /// <summary>
    /// LCC Holder Open
    /// </summary>
    [Serializable]
    public class LCC_Holder_Open : SequenceBehavior
    {
        public LCC_Holder_Open()
        {
            try
            {
                this.description = nameof(LCC_Holder_Open);
                this.SequenceDescription = "LCC Holder Open";
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

    /// <summary>
    /// LCC Holder Close
    /// </summary>
    [Serializable]
    public class LCC_Holder_Close : SequenceBehavior
    {
        public LCC_Holder_Close()
        {
            try
            {
                this.description = nameof(LCC_Holder_Close);
                this.SequenceDescription = "LCC Holder Close";
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

    /// <summary>
    /// LCC Holder Closed Check
    /// </summary>
    [Serializable]
    public class LCC_Holder_Closed_Check : SequenceBehavior
    {
        public LCC_Holder_Closed_Check()
        {
            try
            {
                this.description = nameof(LCC_Holder_Closed_Check);
                this.SequenceDescription = "LCC Holder Closed Check";
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

    /// <summary>
    /// LCC Holder Opened Check
    /// </summary>
    [Serializable]
    public class LCC_Holder_Opened_Check : SequenceBehavior
    {
        public LCC_Holder_Opened_Check()
        {
            try
            {
                this.description = nameof(LCC_Holder_Opened_Check);
                this.SequenceDescription = "LCC Holder Opened Check";
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

    /// <summary>
    /// Wait Handle From Maestro
    /// </summary>
    [Serializable]
    public class Wait_Handle_Sequence : SequenceBehavior
    {
        public Wait_Handle_Sequence(int waitHandle)
        {
            try
            {
                this.description = nameof(Wait_Handle_Sequence);
                this.SequenceDescription = $"Wait Handle Sequence + ({waitHandle})";
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
}

