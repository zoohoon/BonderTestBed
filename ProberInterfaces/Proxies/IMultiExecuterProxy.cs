using System.Collections.Generic;
using System.ServiceModel;

namespace ProberInterfaces
{
    public interface IMultiExecuterProxy
    {
        CommunicationState State { get; }

        void StartCell(int cellNum);
        void StartCellStageList(List<int>stageindex);
        void ExitCell(int cellNum);
        bool GetCellConnectedInfo(int cellNum);
        bool GetCellAccessible(int cellNum);
        bool InitService();
        bool IsOpened();
        void DeInitService();

        void DisConnectLauncher();
        EnumCellProcessState GetCellState(int cellNum);

        bool GetDiskInfo();

     


    }
}
