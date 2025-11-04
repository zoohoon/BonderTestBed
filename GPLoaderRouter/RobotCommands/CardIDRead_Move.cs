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

    public class CardIDRead_Move : GPLoaderRobotCommand
    {
        public CardIDRead_Move()
        {
            CommandName = EnumRobotCommand.CARDID_MOVE;
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
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_CardIDReadingPos_LX_LZ_LW() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new Barcode_Trig_ON() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Barcode_Trig_OFF() });                        // 1초 대기 후 바코드 리딩 센서 Off 다음 Step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new Move_SafePos_Arm() });                         // Arm빼고 움직여야되는데, 5로 보내는 부분이 없음..
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_CardIDReadingPos_LX_LZ_LW() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Barcode_Trig_ON() });                         // 0.3초 대기 후 바코드 리딩 센서 On 다음 Step
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Barcode_Trig_OFF() });                        // 1초 대기 후 바코드 리딩 센서 Off 다음 Step
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_CardIDReadingPos_LX_LZ_LW() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_TargetPos_Arm(),
                                                                               new Barcode_Trig_ON() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Barcode_Trig_OFF() });                        // 1초 대기 후 바코드 리딩 센서 Off 다음 Step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
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
