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

    public class CardBuffer_Put : GPLoaderRobotCommand
    {
        public CardBuffer_Put()
        {
            CommandName = EnumRobotCommand.CARDBUFF_PUT;
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
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckVac_Target_BeforePut(),
                                                                               new Move_TargetPos_Arm()});
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ(),
                                                                               new LCC_Holder_Open(),
                                                                               new Vac_Off_Arm()});
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPut() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(),
                                                                               new LCC_Holder_Close() });
                        //Auto Recovery
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true} });
                        CommandSequence.Add(68, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                              new CheckVac_Target_BeforePut(), 
                                                                              new Vac_Off_Arm() });
                        CommandSequence.Add(8, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LX_LZ_LW() });
                        CommandSequence.Add(24, new List<SequenceBehavior>() { new Check_Close_CardTray() });
                        CommandSequence.Add(25, new List<SequenceBehavior>() { new Open_CardTray() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePut(),
                                                                               new Move_TargetPos_Arm() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Vac_Off_Arm(),
                                                                               new Move_Down_LZ() });
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(52, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPut() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Down_LZ() });
                        CommandSequence.Add(58, new List<SequenceBehavior>() { new CheckVac_Target_AfterPut() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new Close_CardTray(),
                                                                               new CheckVac_Target_AfterPut() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckVac_Target_BeforePut(),
                                                                               new Move_TargetPos_Arm()});
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ(),
                                                                               new LCC_Holder_Open(),
                                                                               new Vac_Off_Arm()});
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() });
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });
                        CommandSequence.Add(57, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPut() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(), 
                                                                               new LCC_Holder_Close() });
                        //Auto Recovery
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true } });
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
