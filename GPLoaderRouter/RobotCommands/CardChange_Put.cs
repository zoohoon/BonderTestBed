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

    public class CardChange_Put : GPLoaderRobotCommand
    {
        public CardChange_Put()
        {
            CommandName = EnumRobotCommand.CC_PUT;
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
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ(), 
                                                                               new LCC_Holder_Open(),
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(42, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(43, new List<SequenceBehavior>() { new Move_Down_LZ(),          //40 중복
                                                                               new LCC_Holder_Open() });
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPut() });
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(), 
                                                                               new LCC_Holder_Close() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true} });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true} });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ() });
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() });             // 45로 가지 않음.
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPut() });
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.6초 대기 Step
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Down_LZ() });                        // 이미 같은 위치
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(32, new List<SequenceBehavior>() { new Wait_Handle_Sequence(2) });              //Ready For FiducialMarkAlign 
                        CommandSequence.Add(34, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new Wait_Handle_Sequence(6) });             //Wait For FiducialMarkAlign 
                        CommandSequence.Add(38, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new No_Error_Sequence(),                          // LZ Move지만 현재위치 그대로임, 셀이 와서 카드를 건내줌 (헷갈릴 수 있기때문에 No_Error_Sequence라고 해둠)
                                                                               new LCC_Holder_Open(),
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(43, new List<SequenceBehavior>() { new Wait_Handle_Sequence(8) });
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPut() });
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(), 
                                                                               new LCC_Holder_Close() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new Wait_Handle_Sequence(92) { isRecoverySeq = true } });
                        CommandSequence.Add(90, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true} });
                        CommandSequence.Add(100, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
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
