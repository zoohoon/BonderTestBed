using ProberErrorCode;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace ProberInterfaces
{
    public enum EnumMovingDirection
    {
        NONE = 0x0000,
        RIGHT = 0x0101,
        LEFT = 0x0102,
        UP = 0x0201,
        DOWN = 0x0202,
        DIRECTION_DISTINGUISH = 0x0100
    }

    public enum EnumDirectionSign
    {
        PLUS = 1,
        MINUS = 2
    }

    public enum EnumDirectionAxis
    {
        X = 1,
        Y = 2
    }

    public enum EnumStartContactPosition
    {
        NONE = 0,
        FIRST_CONTACT = 1,
        ALL_CONTACT = 2
    }
    [ServiceContract]
    public interface IManualContact : IFactoryModule, IModule
    {
        System.Windows.Point MXYIndex { get; set; }
        double OverDrive { get; }
        double SelectedContactPosition { get; }
        bool IsZUpState { get; set; }
        object ViewTarget { get; set; }
        double CPC_Z0 { get; }
        double CPC_Z1 { get; set; }
        double CPC_Z2 { get; set; }
        //bool AlawaysMoveToTeachDie { get; set; }
        MachinePosition MachinePosition { get; set; }

        [OperationContract]
        EventCodeEnum MoveToWannaZIntervalPlus(double wantToMoveZInterval);
        [OperationContract]
        EventCodeEnum MoveToWannaZIntervalMinus(double wantToMoveZInterval);
        [OperationContract]
        Task<bool> FirstContactSet();
        Task<bool> AllContactSet();
        void IncreaseY();
        void DecreaseY();
        void IncreaseX();
        void DecreaseX();
        void ChangeToZUpState();
        Task ChangeToZDownState();
        void ZoomIn();
        void ZoomOut();
        void ZUpMode();
        EventCodeEnum ZUpMode(long xIndex, long yIndex, double OD);
        void ZDownMode(bool needZCleared = false);
        void MachinePositionUpdate();
        void ChangeOverDrive(string OverDriveValue);
        void OverDriveValueUp();
        void OverDriveValueDown();
        void SetContactStartPosition();
        void ResetContactStartPosition();
        void InitSelectedContactPosition();
        void GetOverDriveFromProbingModule();
        ICoordinateManager CoordinateManager { get; set; }
        Visibility CPC_Visibility { get; set; }
        bool IsMovingStage { get; set; }
        bool IsCallbackEntry { get; set; }
        void SetIndex(EnumMovingDirection xdir, EnumMovingDirection ydir);
        void ManualContactZDownStateTransition();

        void ChangeCPC_Z1(string CPC_Z1);
        void ChangeCPC_Z2(string CPC_Z2);
    }
}
