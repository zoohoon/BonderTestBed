namespace ProberInterfaces.Data
{
    using System.Collections.Generic;

    public interface IStageMoveLockStatus
    {
        List<ReasonOfStageMoveLock> LastStageMoveLockReasonList { get; set; }
    }
}
