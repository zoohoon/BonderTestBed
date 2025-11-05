namespace GPLoaderRouter.RobotCommands
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using SequenceRunner;
    using SequenceRunnerBehaviors;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Buffer_Put : GPLoaderRobotCommand
    {
        public Buffer_Put()
        {
            CommandName = EnumRobotCommand.BUFF_PUT;
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
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Target_BeforePut() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LT_LW() });
                        CommandSequence.Add(25, new List<SequenceBehavior>() { new Vac_On_Target(),
                                                                               new CheckVac_Target_BeforePut() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePut(),
                                                                               new Vac_On_Target() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LT() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new CheckVac_Target_AfterPut() });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPut(),
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LT_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(), 
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePut(), 
                                                                               new Vac_On_Target() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LT() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(), 
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Target_BeforePut() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PutPos_LT_LW() });
                        CommandSequence.Add(25, new List<SequenceBehavior>() { new Vac_On_Target(), 
                                                                               new CheckVac_Target_BeforePut() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm(), 
                                                                               new Vac_Off_Arm() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePut(), 
                                                                               new Vac_On_Target() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LT() });
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new CheckVac_Target_AfterPut() });
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new Vac_On_Arm(),
                                                                               new CheckVac_Arm_AfterPut(), 
                                                                               new Vac_Off_Arm() });
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
