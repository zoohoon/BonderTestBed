using ProberErrorCode;
using ProberInterfaces.Monitoring;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    public interface IMonitoringManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        IParam MonitoringSystemParam_IParam { get; set; }
        IIOService IOService { get; set; }
        bool IsSystemError { get; }
        bool IsStageSystemError { get; }
        bool IsLoaderSystemError { get; }
        bool IsMachineInitDone { get; }

        bool IsMachinInitOn { get; set; }
        //EventCodeEnum CheckEMGButton();
        //EventCodeEnum CheckMainAir();
        //EventCodeEnum CheckMainVacuum();
        void RunCheck();
        void StopCheck();
        void DEBUG_Check();
        Object GetHWPartCheckList();
        EventCodeEnum StageEmergencyStop();
        EventCodeEnum LoaderEmergencyStop();
        Task<EventCodeEnum> MachineMonitoring();
        EventCodeEnum RecievedFromLoaderEMG(EnumLoaderEmergency emgtype);

        bool SkipCheckChuckVacuumFlag { get; set; }

        List<IMonitoringBehavior> MonitoringBehaviorList { get; set; }
    }
}
