using ProberInterfaces;
using System;

namespace SoakingParameters
{
    [Serializable]
    public class StatusSoakingInfo
    {
        public SoakingStateEnum SoakingState { get; set; } = SoakingStateEnum.PREPARE;
        public long ChillingTime { get; set; } = 0;         // Unit : Second
        public DateTime InfoUpdateTime { get; set; } = default;
    }
}
