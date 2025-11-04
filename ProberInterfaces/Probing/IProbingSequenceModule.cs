using System.Collections.Generic;
using ProberErrorCode;
using System.Collections.ObjectModel;

namespace ProberInterfaces
{
    public interface IProbingSequenceParameter : IDeviceParameterizable, IParamNode
    {
        Element<List<MachineIndex>> ProbingSeq { get; set; }
    }
    
    public interface IProbingSequenceModule : IFactoryModule, IModule
    {
        //ProbingSequenceState ProbingSequenceState { get; }

        /// <summary>
        /// ProbingSeqParameter.ProbingSeq의 Count와 동일.
        /// </summary>
        /// 
        int ProbingSequenceCount { get; }
        int ProbingSequenceRemainCount { get; }
        IProbingSequenceParameter ProbingSeqParameter { get; set; }
        List<MachineIndex> MakeProbingSequence(int SeqDir);
        List<int> BinCodes { get; set; }
        EnumProbingJobResult ValidateJobResult(List<DeviceObject> testeddevices);
        //EventCodeEnum ResetState();
        EventCodeEnum ResetProbingSequence();
        //EventCodeEnum UpdateProbingStateSequence();
        EventCodeEnum ProbingSequenceTransfer(int idx);

        ObservableCollection<IDeviceObject> GetNextProbingSeq(int curseqindex);
        ObservableCollection<IDeviceObject> GetPreProbingSeq(int curseqindex);

        ObservableCollection<IDeviceObject> GetUnderDutDices(MachineIndex mCoord);

        EventCodeEnum SetProbingSequence(List<MachineIndex> seq);

        EventCodeEnum SetProbingInnerState(LotModeEnum mode);
        ProbingSequenceStateEnum GetProbingSequenceState();
        EventCodeEnum GetNextSequence(ref MachineIndex MI);
        EventCodeEnum UpdateProbingStateSequence();
        EventCodeEnum GetFirstSequence(ref MachineIndex MI);
        EventCodeEnum GetSequenceExistCurWafer();
    }

    public enum SEQ_DIRECTION
    {
        PREV = 0,
        NEXT
    }
    public enum ProbingSequenceStateEnum
    {
        IDLE = 0,
        SEQREMAIN,
        NOSEQ,
        ERROR,
        ABORTED,
        DONE,
    }
    public enum EnumProbingInnerState
    {
        CP1Normal = 0,
        CP1Instant = 1,
        CP1FinalInstantRetest = 2,
        CP1OnlineRetestStart = 3,
        CP1OnlineRetest = 4,
        CP1OnlineInstant = 5,
        CP1FinalOnlineInstant = 6,
        CP1Done = 7,

        MPPNormalStart = 8,
        MPPNormal = 9,
        MPPInstant = 10,
        MPPFinalInstant = 11,
        MPPDone = 12,

        ContinueStart = 13,
        
    }
    
}
