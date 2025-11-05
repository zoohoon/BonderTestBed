namespace ProberInterfaces.Monitoring
{
    using ProberErrorCode;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public interface IMonitoringBehavior : IFactoryModule
    {
        EventCodeEnum InitModule();
        EventCodeEnum ErrorOccurred(EventCodeEnum eventCode);
        void ClearError();
        EventCodeEnum Monitoring();
        bool SystemErrorType { get; set; }
        bool IsError { get; }
        bool PauseOnError { get; }
        bool ImmediatePauseOnError { get; }
        string Name { get; set; }
        string ErrorDescription { get; set; }
        EventCodeEnum ErrorCode { get; set; }
        Element<string> BehaviorClassName { get; set; }
        bool CanManualRecovery { get; }
        void ManualRecovery();
        string RecoveryDescription { get; }
        List<string> PreCheckRecoveryBehaviors { get; }
        bool ShowMessageDialog { get; }
    }
}
