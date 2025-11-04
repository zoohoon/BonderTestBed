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

    public class CardBuffer_Pick : GPLoaderRobotCommand
    {
        public CardBuffer_Pick()
        {
            CommandName = EnumRobotCommand.CARDBUFF_PICK;
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
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckVac_Target_BeforePick(),
                                                                               new Move_TargetPos_Arm(),
                                                                               new LCC_Holder_Open() });                     // Arm 뻗기 전에 카드암센서 먼저 체크, Latch 열어.
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.5초 대기 Step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ(),
                                                                               new LCC_Holder_Close() });                    // Z 내림
                        CommandSequence.Add(41, new List<SequenceBehavior>() { new LCC_Holder_Closed_Check() });             // Latch 닫음 (따닥따닥하는거임)
                        CommandSequence.Add(42, new List<SequenceBehavior>() { new LCC_Holder_Open(),
                                                                               new LCC_Holder_Opened_Check() });             // Latch열고 열렸나 확인 (따닥따닥하는거임)
                        CommandSequence.Add(43, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.2초 대기 Step
                        CommandSequence.Add(44, new List<SequenceBehavior>() { new Move_Down_LZ(),
                                                                               new LCC_Holder_Close(),
                                                                               new LCC_Holder_Closed_Check() });             // LZ 제자리 Move, Latch 다시 닫음 (따닥따닥하는거임)
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Closed_Check(),
                                                                               new Vac_On_Arm() });                          // Latch 닫혔나 확인, LCC Vac On
                        CommandSequence.Add(46, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.5초 대기 Step
                        CommandSequence.Add(52, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick(),
                                                                               new LCC_Holder_Close() });                    // LCC Vac 확인
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.5초 대기 Step
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });                          // LZ 올림
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(),                       // 뒤로 빠짐
                                                                               new CheckVac_Arm_AfterPick() });              // LCC 베큠 확인
                        //Auto Recovery
                        CommandSequence.Add(64, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true} });                   // (Recovery) - LZ 내리기
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new LCC_Holder_Open() { isRecoverySeq = true } });               // (Recovery) - LCC Vac Off
                        CommandSequence.Add(66, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() { isRecoverySeq = true } });       // (Recovery) - Latch 체크
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() { isRecoverySeq = true } });        // (Recovery) - Arm Vac 체크
                        CommandSequence.Add(68, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true } });                    // (Recovery) - LZ 올리기
                        CommandSequence.Add(69, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true} });               // (Recovery) - Arm 뒤로 빼기
                        CommandSequence.Add(70, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.GOP:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Target_BeforePick() });
                        CommandSequence.Add(8, new List<SequenceBehavior>() { new No_Error_Sequence() });                      // 1초 대기 Step
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(24, new List<SequenceBehavior>() { new Check_Close_CardTray() });                // 트레이나 버퍼 위 Object도 체크
                        CommandSequence.Add(25, new List<SequenceBehavior>() { new Open_CardTray() });                      // for MPT
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new Move_TargetPos_Arm() });                  // Arm 뻗기 전에 카드암센서 먼저 체크
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.5초 대기 Step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Up_LZ() });
                        CommandSequence.Add(41, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.2초 대기 Step
                        CommandSequence.Add(50, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() });              // gInt_IsCardHolderOn = 1 (Only Carrier), 2 (Carrier + Holder)에 따라 다른 Step 이동
                        CommandSequence.Add(51, new List<SequenceBehavior>() { new Vac_Off_Arm() });                     // (Only Carrier) CardArmVac Off -> Blow On -> (대기) -> Blow Off
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });                     // 0.5초 대기 Step
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });                          // 중복 코드인듯.. 40에서 이미 올렸음.
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm() });
                        //Auto Recovery
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new Vac_Off_Arm() { isRecoverySeq = true} });                    // (Recovery) - Arm Vac 끄기
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() { isRecoverySeq = true} });               // (Recovery) - Arm Vac 체크
                        CommandSequence.Add(68, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });               // (Recovery) - LZ 내리기
                        CommandSequence.Add(69, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true} });               // (Recovery) - Arm 뒤로 빼기

                        CommandSequence.Add(70, new List<SequenceBehavior>() { new CheckVac_Target_AfterPick() });
                        CommandSequence.Add(80, new List<SequenceBehavior>() { new PickPut_Done() });
                        break;
                    case SystemTypeEnum.DRAX:
                        CommandSequence = new Dictionary<int, List<SequenceBehavior>>();
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new PreCheck() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckVac_Arm_BeforePick() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new Move_SafePos_Arm() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new Move_PickPos_LX_LZ_LW() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckVac_Target_BeforePick(), 
                                                                               new Move_TargetPos_Arm(),
                                                                               new LCC_Holder_Open() });                     // Arm 뻗기 전에 카드암센서 먼저 체크, Latch 열어.
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.5초 대기 Step
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new Move_Down_LZ(), 
                                                                               new LCC_Holder_Close() });                    // Z 내림
                        CommandSequence.Add(41, new List<SequenceBehavior>() { new LCC_Holder_Closed_Check() });             // Latch 닫음 (따닥따닥하는거임)
                        CommandSequence.Add(42, new List<SequenceBehavior>() { new LCC_Holder_Open(), 
                                                                               new LCC_Holder_Opened_Check() });             // Latch열고 열렸나 확인 (따닥따닥하는거임)
                        CommandSequence.Add(43, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.2초 대기 Step
                        CommandSequence.Add(44, new List<SequenceBehavior>() { new Move_Down_LZ(), 
                                                                               new LCC_Holder_Close(), 
                                                                               new LCC_Holder_Closed_Check() });             // LZ 제자리 Move, Latch 다시 닫음 (따닥따닥하는거임)
                        CommandSequence.Add(45, new List<SequenceBehavior>() { new LCC_Holder_Closed_Check(), 
                                                                               new Vac_On_Arm() });                          // Latch 닫혔나 확인, LCC Vac On
                        CommandSequence.Add(46, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.5초 대기 Step
                        CommandSequence.Add(52, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick(), 
                                                                               new LCC_Holder_Close() });                    // LCC Vac 확인
                        CommandSequence.Add(53, new List<SequenceBehavior>() { new No_Error_Sequence() });                   // 0.5초 대기 Step
                        CommandSequence.Add(55, new List<SequenceBehavior>() { new Move_Up_LZ() });                          // LZ 올림
                        CommandSequence.Add(60, new List<SequenceBehavior>() { new Move_BackPos_Arm(),                       // 뒤로 빠짐
                                                                               new CheckVac_Arm_AfterPick() });              // LCC 베큠 확인
                        //Auto Recovery
                        CommandSequence.Add(64, new List<SequenceBehavior>() { new Move_Down_LZ() { isRecoverySeq = true } });                  // (Recovery) - LZ 내리기
                        CommandSequence.Add(65, new List<SequenceBehavior>() { new LCC_Holder_Open() { isRecoverySeq = true } });               // (Recovery) - LCC Vac Off
                        CommandSequence.Add(66, new List<SequenceBehavior>() { new LCC_Holder_Opened_Check() { isRecoverySeq = true } });       // (Recovery) - Latch 체크
                        CommandSequence.Add(67, new List<SequenceBehavior>() { new CheckVac_Arm_AfterPick() { isRecoverySeq = true } });        // (Recovery) - Arm Vac 체크
                        CommandSequence.Add(68, new List<SequenceBehavior>() { new Move_Up_LZ() { isRecoverySeq = true } });                    // (Recovery) - LZ 올리기
                        CommandSequence.Add(69, new List<SequenceBehavior>() { new Move_BackPos_Arm() { isRecoverySeq = true } });              // (Recovery) - Arm 뒤로 빼기
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
