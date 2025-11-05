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

    public class Foup_CoverClose : GPLoaderCSTCommand
    {
        public Foup_CoverClose(SubstrateSizeEnum waferSize)
        {
            CommandName = EnumCSTCtrl.COVERCLOSE;
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
                        CommandSequence.Add(0,  new List<SequenceBehavior>(){ new CheckHomed(), new CstVacOn() } );
                        CommandSequence.Add(5,  new List<SequenceBehavior>(){ new CheckNoWaferOut(), new InitCoverClosingState() } );
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCoverUnlock(), new CstVacOff(), new CstMappingOff() } );
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstMappingOff() } );
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CoverOpenOn() } );
                        CommandSequence.Add(27, new List<SequenceBehavior>() { new CheckCoverOpen() } );
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new MoveDownPos(), new CheckNoWaferOut(), new CoverCloseOn() } );
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new CheckCoverClose() } );
                        CommandSequence.Add(100,new List<SequenceBehavior>() { new InitCoverClosedState() } );

                    }                   
                    else
                    {
                        // not performed cover close.
                    }
                    
                }
                if (SystemManager.SystemType == SystemTypeEnum.DRAX)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0,   new List<SequenceBehavior>() { new CstVacOn() });
                        CommandSequence.Add(5,   new List<SequenceBehavior>() { new CheckNoWaferOut(), new InitCoverClosingState() });
                        CommandSequence.Add(10,  new List<SequenceBehavior>() { new CheckCoverUnlock(), new CstVacOff(), new CstMappingOff() });
                        CommandSequence.Add(15,  new List<SequenceBehavior>() { new CheckCstMappingOff() });
                        CommandSequence.Add(20,  new List<SequenceBehavior>() { new CoverOpenOn() });
                        CommandSequence.Add(27,  new List<SequenceBehavior>() { new CheckCoverOpen() });
                        CommandSequence.Add(30,  new List<SequenceBehavior>() { new MoveDownPos(), new CheckNoWaferOut(), new CoverCloseOn() });
                        CommandSequence.Add(35,  new List<SequenceBehavior>() { new CheckCoverClose() });
                        CommandSequence.Add(100, new List<SequenceBehavior>() { new InitCoverClosedState() });
                    }
                    else
                    {
                        // not performed cover close.
                    }


                }
                else if (SystemManager.SystemType == SystemTypeEnum.GOP)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new CstVacOn() });
                        CommandSequence.Add(5, new List<SequenceBehavior>() { new CheckNoWaferOut(), new InitCoverClosingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCoverUnlock(), new CstMappingOff() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new CheckCstMappingOff() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CoverOpenOn() });
                        CommandSequence.Add(27, new List<SequenceBehavior>() { new CheckCoverOpen(), new CheckNoWaferOut() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new CheckNoWaferOut(), new MoveDownPos(), new CoverCloseOn() });
                        CommandSequence.Add(35, new List<SequenceBehavior>() { new CoverCloseOn(), new CheckCoverClose() });
                        CommandSequence.Add(100, new List<SequenceBehavior>() { new InitCoverClosedState() });
                        //6,8인치는 command 받았을 때 무조건 cover closed로 처리.
                    }
                    else
                    {
                        // not performed cover close.
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
