using System;
using System.Collections.Generic;

namespace ProberInterfaces.Temperature.DryAir
{
    using ProberErrorCode;
    using System.ServiceModel;
    public interface IDryAirModule : IFactoryModule,
                               IDisposable, IModule,
                               IHasSysParameterizable
    {
        IDryAirController Processor { get; }
        double DryAirActivableHighTemp { get; set; }
        EventCodeEnum InitConnect();
        List<IOPortDescripter<bool>> GetInputPorts();
        List<IOPortDescripter<bool>> GetOutputPorts();
        EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1);
        bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1);
        int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageIndex = -1);
        byte[] GetDryAirParam(int stageIndex = -1);
        EventCodeEnum Execute();
    }
    //[ServiceContract]
    //public interface IDryAirProcessor
    //{

    //}

    [ServiceContract]
    public interface IDryAirController :  IFactoryModule
    {
        [OperationContract]
        EventCodeEnum InitModule();

        [OperationContract]
        byte[] GetDryAirParam(int stageindex = -1);
        [OperationContract]
        EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1);
        [OperationContract]
        bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1);
        [OperationContract]
        int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1);
    }

    [ServiceContract]
    public interface IDryAirService : IDryAirController
    {
        [OperationContract]
        void InitService();
    }

   

}
