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

    public class Foup_CstUnlock : GPLoaderCSTCommand
    {
        public Foup_CstUnlock(SubstrateSizeEnum waferSize)
        {
            CommandName = EnumCSTCtrl.CSTUNLOCK;
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
                CommandSequence = new Dictionary<int, List<SequenceBehavior>> ();
                if (SystemManager.SystemType == SystemTypeEnum.Opera)
                {
                    if (WaferSize == SubstrateSizeEnum.INCH12)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstUnlockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { new UnlockCst() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstUnlock() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstUnlockedState() });
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
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstUnlockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstUnlock() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstUnlockedState() });
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
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstUnlockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new UnlockCst() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstUnlock() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstUnlockedState() });
                    }
                    else if (WaferSize == SubstrateSizeEnum.INCH8)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstUnlockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstLock8Inch(), new UnlockCst8Inch() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstUnlock8Inch() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstUnlockedState() });
                    }
                    else if (WaferSize == SubstrateSizeEnum.INCH8)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstUnlockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstLock8Inch(), new UnlockCst8Inch() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstUnlock8Inch() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstUnlockedState() });
                    }
                    else
                    {
                        //not exist custom size.
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
