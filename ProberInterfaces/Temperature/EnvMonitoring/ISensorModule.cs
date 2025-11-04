using ProberErrorCode;
using ProberInterfaces.Communication;
using ProberInterfaces.EnvControl.Enum;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Temperature.EnvMonitoring
{
    public delegate void SensorTempChangedEvent(int idx, double temp);
    public delegate void SensorStatusChangedEvent(int idx, GEMSensorStatusEnum status);
    public delegate void SensorRequestDataEvent(byte[] buffer, int offset, int count);
    public interface ISensorModule : IFactoryModule, IModule
    {
        event SensorTempChangedEvent TempChangedEvent;
        event SensorStatusChangedEvent StatusChangedEvent;
        event SensorRequestDataEvent RequestDataEvent;
        bool SensorEnable { get; set; }
        int SensorIndex { get; set; }
        string SensorAlias { get; set; }
        SensorStatusEnum SensorStatus { get; set; }
        GEMSensorStatusEnum GEMSensorStatus { get; set; }
        bool bDisconnectNotifyDone { get; set; }
        bool bAlarmNotifyDone { get; set; }
        Boolean bRcvDone { get; set; }
        List<SensorStatusEnum> ErrorReasons { get; set; }
        SensorInfo SensorInfo { get; set; }
        SensorSysParameter SensorSysParameter { get; set; }
        IProtocolModule ProtocolModule { get; }
        ModuleStateEnum Execute();      
        void UpdateSensorModule();
        EventCodeEnum FillInputBuff(byte[] receiveData);
        void UpdateSensorInfo(ISensorInfo sensorInfo);        
    }
}
