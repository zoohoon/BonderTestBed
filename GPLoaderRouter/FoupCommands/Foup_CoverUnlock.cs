namespace GPLoaderRouter.FoupCommands
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

    public class Foup_CoverUnlock : GPLoaderCSTCommand
    {
        public Foup_CoverUnlock(SubstrateSizeEnum waferSize)
        {
            CommandName = EnumCSTCtrl.COVERUNLOCK;
            WaferSize = waferSize;
            CreateCommandSequence();
        }

        private EnumCSTCtrl _CommandName;
        public override EnumCSTCtrl CommandName
        {
            get { return _CommandName; }
            set { _CommandName = value; }
        }

        private SubstrateSizeEnum _WaferSize = SubstrateSizeEnum.UNDEFINED;
        public override SubstrateSizeEnum WaferSize
        {
            get { return _WaferSize; }
            set { _WaferSize = value; }
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

                CommandSequence = new Dictionary<int, List<SequenceBehavior>>();

                if (SystemManager.SystemType == SystemTypeEnum.Opera)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0,  new List<SequenceBehavior>() { new CheckHomed(), new CstVacOn() } );
                        CommandSequence.Add(5,  new List<SequenceBehavior>() { new CheckNoWaferOut(), new InitCoverUnlockingState() } );
                        CommandSequence.Add(10,  new List<SequenceBehavior>() { new CheckCoverNotExist(), new CstMappingOff() } );
                        CommandSequence.Add(15,  new List<SequenceBehavior>() { new CheckCstMappingOff() } );
                        CommandSequence.Add(20,  new List<SequenceBehavior>() { new CoverLockOn() } );
                        CommandSequence.Add(25,  new List<SequenceBehavior>() { new CheckCstCoverLock() } );
                        CommandSequence.Add(26,  new List<SequenceBehavior>() { new CheckDownPos(), new CoverOpenOn() } );
                        CommandSequence.Add(27,  new List<SequenceBehavior>() { new CheckCoverOpen() } );
                        CommandSequence.Add(30,  new List<SequenceBehavior>() { new MoveDownPos(), new CoverCloseOn() } );
                        CommandSequence.Add(35,  new List<SequenceBehavior>() { new CheckCoverClose() } );
                        CommandSequence.Add(40,  new List<SequenceBehavior>() { new CheckCstLock() } );
                        CommandSequence.Add(50,  new List<SequenceBehavior>() { new CheckCstLoadOn() } );
                        CommandSequence.Add(60,  new List<SequenceBehavior>() { new CstVacOn() } );
                        CommandSequence.Add(70,  new List<SequenceBehavior>() { new CheckCoverVac(), new CoverUnlockOn() } );
                        CommandSequence.Add(80,  new List<SequenceBehavior>() { new CheckCoverUnlock(), new CstVacOff() } );
                        CommandSequence.Add(100,  new List<SequenceBehavior>() { new InitCoverUnlockedState() } );
                    }
                    else
                    {
                        //not performed cover unlock.
                    }


                }
                else if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0,  new List<SequenceBehavior>() { new CheckHomed(), new CstVacOn() } );
                        CommandSequence.Add(5,  new List<SequenceBehavior>() { new CheckNoWaferOut(), new InitCoverUnlockingState() } );
                        CommandSequence.Add(10,  new List<SequenceBehavior>() { new CheckCoverNotExist(), new CstMappingOff() } );
                        CommandSequence.Add(15,  new List<SequenceBehavior>() { new CheckCstMappingOff() } );
                        CommandSequence.Add(20,  new List<SequenceBehavior>() { new CoverLockOn() } );
                        CommandSequence.Add(25,  new List<SequenceBehavior>() { new CheckCstCoverLock() } );
                        CommandSequence.Add(26,  new List<SequenceBehavior>() { new CheckDownPos(), new CoverOpenOn() } );
                        CommandSequence.Add(27,  new List<SequenceBehavior>() { new CheckCoverOpen() } );
                        CommandSequence.Add(30,  new List<SequenceBehavior>() { new MoveDownPos(), new CoverCloseOn() } );
                        CommandSequence.Add(35,  new List<SequenceBehavior>() { new CheckCoverClose() } );
                        CommandSequence.Add(40,  new List<SequenceBehavior>() { new CheckCstLock() } );
                        CommandSequence.Add(50,  new List<SequenceBehavior>() { new CheckCstLoadOn() } );
                        CommandSequence.Add(60,  new List<SequenceBehavior>() { new CstVacOn() } );
                        CommandSequence.Add(70,  new List<SequenceBehavior>() { new CheckCoverVac(), new CoverUnlockOn() } );
                        CommandSequence.Add(80,  new List<SequenceBehavior>() { new CheckCoverUnlock(), new CstVacOff() } );
                        CommandSequence.Add(100,  new List<SequenceBehavior>() { new InitCoverUnlockedState() } );

                    }
                    else
                    {
                        //not performed cover unlock.
                    }

                }
                else if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0,  new List<SequenceBehavior>() { new CstVacOn() } );
                        CommandSequence.Add(5,  new List<SequenceBehavior>() { new CheckNoWaferOut(), new InitCoverUnlockingState() } );
                        CommandSequence.Add(10,  new List<SequenceBehavior>() { new CheckCoverNotExist(), new CstMappingOff() } );
                        CommandSequence.Add(15,  new List<SequenceBehavior>() { new CheckCstMappingOff() } );
                        CommandSequence.Add(20,  new List<SequenceBehavior>() { new CoverLockOn() } );
                        CommandSequence.Add(25,  new List<SequenceBehavior>() { new CheckCstCoverLock() } );
                        CommandSequence.Add(26,  new List<SequenceBehavior>() { new CheckDownPos(), new CoverOpenOn() } );
                        CommandSequence.Add(27,  new List<SequenceBehavior>() { new CheckCoverOpen() } );
                        CommandSequence.Add(30,  new List<SequenceBehavior>() { new MoveDownPos(), new CoverCloseOn() } );
                        CommandSequence.Add(35,  new List<SequenceBehavior>() { new CheckCoverClose() } );
                        CommandSequence.Add(40,  new List<SequenceBehavior>() { new CheckCstLock() } );
                        CommandSequence.Add(50,  new List<SequenceBehavior>() { new CstVacOn() } );
                        CommandSequence.Add(60,  new List<SequenceBehavior>() { new CheckCoverVac(), new CoverUnlockOn() } );
                        CommandSequence.Add(70,  new List<SequenceBehavior>() { new CheckCoverUnlock(), new CstVacOff() } );
                        CommandSequence.Add(80,  new List<SequenceBehavior>() { new CheckCstVacOff() } );
                        CommandSequence.Add(100,  new List<SequenceBehavior>() { new InitCoverUnlockedState() } );
                    }
                    else
                    {
                        //not performed cover unlock.
                    }

                    //6,8인치는 command 받았을 때 무조건 cover unlocked로 처리.
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
