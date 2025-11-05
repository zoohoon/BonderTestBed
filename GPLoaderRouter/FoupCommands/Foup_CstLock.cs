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

    public class Foup_CstLock : GPLoaderCSTCommand
    {
        public Foup_CstLock(SubstrateSizeEnum waferSize)
        {
            CommandName = EnumCSTCtrl.CSTLOCK;
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
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstLockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new LockCst() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstLock() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstLockedState() });
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
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstLockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new LockCst() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstLock() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstLockedState() });
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
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstLockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist(), new LockCst() });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstLock() });
                        CommandSequence.Add(30, new List<SequenceBehavior>() { new InitCstLockedState() });

                    }
                    else if (WaferSize == SubstrateSizeEnum.INCH8)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstLockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist8Inch(), new LockCst8Inch() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstLock8Inch() });
                        CommandSequence.Add(31, new List<SequenceBehavior>() { new InitCstLockedState() });
                    }
                    else if (WaferSize == SubstrateSizeEnum.INCH6)
                    {
                        CommandSequence.Add(0, new List<SequenceBehavior>() { new InitCstLockingState() });
                        CommandSequence.Add(10, new List<SequenceBehavior>() { new CheckCstExist8Inch(), new LockCst8Inch() });
                        CommandSequence.Add(15, new List<SequenceBehavior>() { });
                        CommandSequence.Add(20, new List<SequenceBehavior>() { new CheckCstLock6Inch() });
                        CommandSequence.Add(22, new List<SequenceBehavior>() { new CheckCstLock6Inch() });
                        CommandSequence.Add(32, new List<SequenceBehavior>() { new InitCstLockedState() });
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
