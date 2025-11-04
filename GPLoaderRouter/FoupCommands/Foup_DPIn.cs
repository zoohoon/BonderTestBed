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

    public class Foup_DPIn : GPLoaderCSTCommand
    {
        public Foup_DPIn(SubstrateSizeEnum waferSize)
        {
            CommandName = EnumCSTCtrl.DPIN;
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
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitDPInMovingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new CoverLockOn() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstCoverLock() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CstLoadOn() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckCstLoadOn() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new InitDPInMoveDoneState() });
                    }
                    else
                    {
                        //not exist inch 6,8 model.
                    }


                }
                else if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitDPInMovingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new CoverLockOn() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstCoverLock() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CstLoadOn() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckCstLoadOn() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new InitDPInMoveDoneState() });
                    }
                    else
                    {
                        //not exist inch 6,8 model.
                    }


                }
                else if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitDPInMovingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new CoverLockOn() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstCoverLock() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CstLoadOn() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckCstLoadOn() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new InitDPInMoveDoneState() });
                    }
                    else if (WaferSize == SubstrateSizeEnum.INCH8)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitDPInMovingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist8Inch(), new CoverLockOn() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstCoverLock() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CstLoadOn() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckCstLoadOn() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new InitDPInMoveDoneState() });

                    }
                    else if (WaferSize == SubstrateSizeEnum.INCH6)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitDPInMovingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist8Inch(), new CoverLockOn() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstCoverLock() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CstLoadOn() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckCstLoadOn() });
                        CommandSequence.Add(40, new List<SequenceBehavior>() { new InitDPInMoveDoneState() });

                    }
                    else
                    {
                        //not exist custom size
                    }
                }
                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
