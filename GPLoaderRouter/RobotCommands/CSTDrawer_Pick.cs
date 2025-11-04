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

    public class CSTDrawer_Pick : GPLoaderRobotCommand
    {
        public CSTDrawer_Pick()
        {
            CommandName = EnumRobotCommand.DRW_PICK;
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
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckVac_Target_BeforePick(),
                                                                               new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new Vac_On_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Vac_Off_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });
                        CommandSequence.Add(69, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckVac_Target_BeforePick(),
                                                                               new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new Vac_On_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(),
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Vac_Off_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });
                        CommandSequence.Add(69, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
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
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckVac_Target_BeforePick(), 
                                                                               new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(), 
                                                                               new Vac_On_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ(), 
                                                                               new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Vac_Off_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });
                        CommandSequence.Add(69, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
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
