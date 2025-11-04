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

    public class CardChange_Pick : GPLoaderRobotCommand
    {
        public CardChange_Pick()
        {
            CommandName = EnumRobotCommand.CC_PICK;
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
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(25, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick(), 
                                                                               new LCC_Holder_Close() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new LCC_Holder_Open(),
                                                                               new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new No_Error_Sequence() });                    
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ(),                                
                                                                               new Vac_On_Arm() });
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick(),
                                                                               new LCC_Holder_Close() });
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });
                        CommandSequence.Add(57, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(63, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });
                        //Auto Recovery
                        CommandSequence.Add(64, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true} });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new LCC_Holder_Open() { isRecoverySeq = true} });
                        CommandSequence.Add(66, new List<SequenceBehavior>() { new No_Error_Sequence() { isRecoverySeq = true} });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true} });
                        CommandSequence.Add(68, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true} });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(25, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.5초 대기 Step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(),                                // GOP 는 위로 들어올려 Pick함.
                                                                               new Vac_On_Arm() });                          
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.5초 대기 Step
                        CommandSequence.Add(48, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 캐리어냐 홀더냐에 따라 다른 Step으로 감
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });              // (Carrier + Holoder)
                        CommandSequence.Add(51, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });              // (Only Carrier) Arm Vac Off + Blow On
                        CommandSequence.Add(52, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.5초 대기 Step + Blow Off
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.5초 대기 Step 
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });                          
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });                                   // Done이 마지막이 아니네..
                        //Auto Recovery
                        CommandSequence.Add(75, new List<SequenceBehavior>() { new Vac_Off_Arm() { isRecoverySeq = true } });           // (Recovery) Arm Vac Off + Blow On + 대기 Blow Off
                        CommandSequence.Add(78, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true} });           // (Recovery) 
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });      // (Recovery)
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck(), 
                                                                              new CheckVac_Arm_BeforePick()});
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(), 
                                                                               new LCC_Holder_Open(), 
                                                                               new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Wait_Handle_Sequence(50) });
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Open() });
                        CommandSequence.Add(48, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new Wait_Handle_Sequence(54) });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick(), 
                                                                               new LCC_Holder_Close() });
                        CommandSequence.Add(51, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(52, new List<SequenceBehavior>() { new LCC_Holder_Closed_Check() });
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new Wait_Handle_Sequence(52), 
                                                                               new Vac_On_Arm() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });
                        CommandSequence.Add(56, new List<SequenceBehavior>() { new Wait_Handle_Sequence(52) });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(63, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true} });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new LCC_Holder_Close() { isRecoverySeq = true } });
                        CommandSequence.Add(66, new List<SequenceBehavior>() { new No_Error_Sequence() { isRecoverySeq = true } });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true } });
                        CommandSequence.Add(68, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
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
