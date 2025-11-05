using ProberErrorCode;

namespace ProberInterfaces
{
    public interface IInspection : IFactoryModule, IModule
    {
        //ObservableCollection<IStateModule> ModuleForInspectionCollection { get; }

        //System.Windows.Point MXYIndex { get; set; }

        void IncreaseY();
        void DecreaseY();
        void IncreaseX();
        void DecreaseX();
        //void ZoomIn();
        //void ZoomOut();
        void MachinePositionUpdate();
        void NextDut(ICamera camera);
        void PreDut(ICamera camera);
        void NextPad(ICamera camera);
        void PrePad(ICamera camera);
        void DutStartPoint();
        void SetFrom();
        void Apply();
        void Clear();
        //void ManualSetIndex();
        bool ManualSetIndexToggle { get; set; }
        //int ManualSetIndexX { get; set; }
        //int ManualSetIndexY { get; set; }
        int DUTCount { get; set; }
        int PADCount { get; set; }
        int ViewPadIndex { get; set; }
        int ViewDutIndex { get; set; }
        int XDutStartIndexPoint { get; set; }
        int YDutStartIndexPoint { get; set; }
        double XSetFromCoord { get; set; }
        double YSetFromCoord { get; set; }
        int SavePadImages(ICamera camera);
    }
}
