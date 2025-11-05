namespace GPLoaderRouter.RobotCommands
{
    using LogModule;
    using ProberInterfaces;
    using SequenceRunner;
    using SequenceRunnerBehaviors;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Stage_Put : GPLoaderRobotCommand
    {
        public Stage_Put()
        {
            CommandName = EnumRobotCommand.STAGE_PUT;
            CreateCommandSequence();
        }

        private EnumRobotCommand _CommandName;
        public override EnumRobotCommand CommandName
        {
            get { return _CommandName; }
            set { _CommandName = value; }
        }

        private Dictionary<int, List<SequenceBehavior>> _CommandSequence;
        public override Dictionary<int, List<SequenceBehavior>> CommandSequence
        {
            get { return _CommandSequence; }
            set { _CommandSequence = value; }
        }

        public override void CreateCommandSequence()
        {
            try
            {
                switch (SystemManager.SystemType)
                {
                    case SystemTypeEnum.None:
                        break;
                    case SystemTypeEnum.Opera:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new Vac_Off_Arm(),
                                                                               new CheckVac_Arm_BeforePut() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });                 // LCC관련 안쓰임
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(62, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPut(),
                                                                               new Vac_Off_Arm() });
                        //AutoRecovery
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });           // Normal Wafer - LZ에 Pickupoffset만큼 더 올린 값
                        CommandSequence.Add(22, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });           // Thin Wafer - LZ에 Pickupoffset 적용 x
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(32, new List<SequenceBehavior>() { new Move_Up_LZ_UsingBernoulli() });       // Thin Wafer
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new Vac_Off_Arm(),
                                                                               new CheckVac_Arm_BeforePut() });
                        CommandSequence.Add(38, new List<SequenceBehavior>() { new No_Error_Sequence() });                 // 150ms 대기하는 step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });                 // LCC관련 안쓰임
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(62, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPut(),
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new No_Error_Sequence() { isRecoverySeq = true } });                 // Recovery - 제자리 Move
                        CommandSequence.Add(66, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });             // Recovery - 제자리 Move
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });       // Recovery - 제자리 Move
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new Vac_Off_Arm(),
                                                                               new CheckVac_Arm_BeforePut() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });                 // LCC관련 안쓰임
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(62, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPut(),
                                                                               new Vac_Off_Arm() });
                        //AutoRecovery
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
