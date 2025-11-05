using System.Collections.Generic;
using System.ComponentModel;
using ProberErrorCode;

namespace ProberInterfaces
{
    public interface ISequenceEngineManager : IFactoryModule, INotifyPropertyChanged, IModule
    {
        List<ISequenceEngineService> Services { get; set; }

        object GetLockObject();
        bool GetRunState(bool CheckStage = true, bool condiserStatusSoaking = false, bool isWaferTransfer = false);
        bool GetRunStateChangeMode();
        bool GetRunState(IStateModule excludeModule);
         bool GetMovingState();

        EventCodeEnum RunSequences();
        bool isMovingState();
        bool GetEndReadyState();
        EventCodeEnum StopSequence(ISequenceEngineService service);
        EventCodeEnum RunSequence(ISequenceEngineService service, string threadname);

        bool IsDoPolish();
    }
}
