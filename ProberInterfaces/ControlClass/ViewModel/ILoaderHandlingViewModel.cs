namespace ProberInterfaces.ControlClass.ViewModel
{
    using ProberInterfaces.Enum;
    using System;
    using System.Threading.Tasks;

    public interface ILoaderHandlingViewModel
    {
        Task RepeatTransfer(string cardID, int pinAlignInterval, int repeatCnt, bool skipDock, EnumRepeatedTransferMode mode, int delayTime);
        Task CancelTansferFunc();
        bool GetTransferDoneState();
        int GetTransferCurrentCount();
        TimeSpan GetRTElapsedTimeTotal();
        TimeSpan GetRTElapsedTimeLasted();
        object getSourceObject();
        object getTargetObject();
    }
}
