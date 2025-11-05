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

    public class Stage_Pick : GPLoaderRobotCommand
    {
        public Stage_Pick()
        {
            CommandName = EnumRobotCommand.STAGE_PICK;
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
                        CommandSequence.Add(3, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                              new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick(),
                                                                              new Vac_Off_Arm() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(),
                                                                               new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });                 // LCC관련 안쓰임
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        //AutoRecovey
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(), 
                                                                               new Vac_On_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(), 
                                                                               new CheckVac_Arm_AfterPick() });          // Normal Wafer -> LZ Move 후 Arm Vac 체크 함.
                        CommandSequence.Add(42, new List<SequenceBehavior>() { new Move_Up_LZ_UsingBernoulli() });       // Thin Wafer   -> LZ Move 후 Arm Vac 체크 함.
                        //AutoRecovey (ThinWafer)
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });          // (Recovery - Thin Wafer)
                        CommandSequence.Add(46, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });          // (Recovery - Thin Wafer)
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });               // LCC관련 안쓰임
                        CommandSequence.Add(52, new List<SequenceBehavior>() { new Move_Down_LZ_UsingBernoulli() });     // Thin Wafer
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        //AutoRecovey (NormalWafer)
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new No_Error_Sequence() { isRecoverySeq = true } });             // (Recovery - Normal Wafer) 
                        CommandSequence.Add(66, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });             // (Recovery - Normal Wafer)
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true} });             // (Recovery - Normal Wafer)
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(3, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                              new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick(),
                                                                              new Vac_Off_Arm() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(),
                                                                               new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });                 // LCC관련 안쓰임
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        //AutoRecovey
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new PickPut_Done() });
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
