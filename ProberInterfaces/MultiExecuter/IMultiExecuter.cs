using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    [DataContract]
    public enum EnumCellProcessState
    {
        [EnumMember]
        UNKNOWN = -1,
        [EnumMember]
        Booting = 0,
        [EnumMember]
        Running = 1,

    }

  


    public class CellInfo
    {
        int CellNumber { get; set; }
    }



    [ServiceContract]
    public interface IMultiExecuterCallback //To Client
    {
        [OperationContract]
        void CellLoaded();
        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void MessageSend(int cellnum, string pc_name, string drivename);
        [OperationContract]
        void DisConnect(int cellnum);
        [OperationContract]
        void InfoSend(int cellnum, string pc_name, string drivename, string usagespace, string availablespace, string totalspace, string percent);
    }


    //HOST
    [ServiceContract(CallbackContract = typeof(IMultiExecuterCallback))]
    public interface IMultiExecuter 
    {
        [OperationContract]
        bool GetCellConnectedInfo(int cellNum);

        [OperationContract]
        bool GetCellAccessible(int cellNum);

        //[OperationContract]
        //void InitService();
        //[OperationContract]
        //List<CellInfo> GetAccessCellInfos();

        [OperationContract(IsOneWay = true)]
        void StartAllCells();
        [OperationContract(IsOneWay =true)]
        void StartCell(int cellNum);
        [OperationContract(IsOneWay = true)]
        void StartCellStageList(List<int>stageindex);
        [OperationContract]
        void ExitCell(int cellNum);
        [OperationContract]
        Task InitService();


        [OperationContract]
        EnumCellProcessState GetCellState(int cellNum);

        [OperationContract]
        void DisConnectLoader();

        [OperationContract]
        Task GetDiskInfo();
    }

}
