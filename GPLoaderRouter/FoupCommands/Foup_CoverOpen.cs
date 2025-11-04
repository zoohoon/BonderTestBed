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

    public class Foup_CoverOpen : GPLoaderCSTCommand
    {
        public Foup_CoverOpen(SubstrateSizeEnum waferSize)
        {
            CommandName = EnumCSTCtrl.COVEROPEN;
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
                        CommandSequence.Add(0, new List<SequenceBehavior> { new CheckHomed(), new InitCoverOpeningState() });
                        CommandSequence.Add(10, new List<SequenceBehavior> { new CheckCoverUnlock(), new CheckCstVacOn(), new CstMappingOff() });
                        CommandSequence.Add(15, new List<SequenceBehavior> { new CheckCstMappingOff() });
                        CommandSequence.Add(20, new List<SequenceBehavior> { new CheckCoverOpen(), new CoverOpenOn() });
                        CommandSequence.Add(27, new List<SequenceBehavior> { new CheckCoverOpen() });
                        CommandSequence.Add(30, new List<SequenceBehavior> { new CheckNoWaferOut(), new MoveUpPos(), new CoverOpenOn() });
                        CommandSequence.Add(35, new List<SequenceBehavior> { new CheckCoverOpen() });
                        CommandSequence.Add(100, new List<SequenceBehavior> { new InitCoverOpenedState() });
                    }
                    else
                    {
                        //not performed cover open.
                    }
                       
                }
                else if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior> { new CheckHomed(), new InitCoverOpeningState() });
                        CommandSequence.Add(10, new List<SequenceBehavior> { new CheckCoverNotExist(), new CstVacOff(), new CstMappingOff() });
                        CommandSequence.Add(12, new List<SequenceBehavior> { });
                        CommandSequence.Add(13, new List<SequenceBehavior> { new CheckCoverNotExist() });
                        CommandSequence.Add(15, new List<SequenceBehavior> { new CheckCstMappingOff() });
                        CommandSequence.Add(20, new List<SequenceBehavior> { new CheckCoverOpen(), new CoverOpenOn() });
                        CommandSequence.Add(27, new List<SequenceBehavior> { new CheckCoverOpen() });
                        CommandSequence.Add(30, new List<SequenceBehavior> { new CheckNoWaferOut(), new MoveUpPos(), new CoverOpenOn() });
                        CommandSequence.Add(35, new List<SequenceBehavior> { new CheckCoverOpen() });
                        CommandSequence.Add(100, new List<SequenceBehavior> { new InitCoverOpenedState() });
                    }
                    else
                    {
                        //not performed cover open.
                    }

                }
                else if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior> { new InitCoverOpeningState() });
                        CommandSequence.Add(10, new List<SequenceBehavior> { new CheckCstVacOn(), new CstMappingOff() });
                        CommandSequence.Add(15, new List<SequenceBehavior> { new CheckCstMappingOff() });
                        CommandSequence.Add(20, new List<SequenceBehavior> { new CheckCoverOpen(), new CoverOpenOn() });
                        CommandSequence.Add(27, new List<SequenceBehavior> { new CheckCoverOpen() });
                        CommandSequence.Add(30, new List<SequenceBehavior> { new CheckNoWaferOut(), new MoveUpPos(), new CoverOpenOn() });
                        CommandSequence.Add(35, new List<SequenceBehavior> { new CheckCoverOpen() });
                        CommandSequence.Add(100, new List<SequenceBehavior> { new InitCoverOpenedState() });
                    }
                    else
                    {
                        //not performed cover open.
                    }

                    //6,8인치는 command 받았을 때 무조건 cover opened로 처리.
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
