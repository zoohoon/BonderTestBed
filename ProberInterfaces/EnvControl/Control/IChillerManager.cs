using ProberErrorCode;
using ProberInterfaces.Temperature.Chiller;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface IChillerManager : IChillerService , IModule
    {
        new EventCodeEnum InitModule();
        EventCodeEnum InitConnect();
        /// <summary>
        /// Single 에서는 ChillerModule 사용
        /// </summary>
        IChillerModule GetChillerModule(int stageIndex= -1, int chillerIndex = -1);
        EnumChillerModuleMode GetMode(int stageindex = -1);
        EventCodeEnum SetEMGSTOP();
        List<IChillerModule> GetChillerModules();
        int GetChillerIndex(int stageindex);
        //IChillerAdapter GetChillerAdapter();
        string GetErrorMessage(double errornumber);
        long GetCommReadDelayTime();
        long GetCommWriteDelayTime();
        int GetPingTimeOut();
        bool CanRunningLot();
    }

    public interface IChillerAdapter : IChillerComm
    {
        void SetCommUnitID(byte unitID);
        int SubModuleIndex { get; set; }
    }

}
