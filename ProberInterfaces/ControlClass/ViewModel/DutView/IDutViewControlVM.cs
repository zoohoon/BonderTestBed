using RelayCommandBase;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    [ServiceContract]
    public interface IDutViewControlVM : INotifyPropertyChanged
    {
        IStageSupervisor    StageSupervisor         { get; }
        IMotionManager      MotionManager           { get; }
        IProbeCard          ProbeCard               { get; }
        IWaferObject        WaferObject             { get; }
        IVisionManager      VisionManager           { get; }
        EnumProberCam       CamType                 { get; }
        double              ZoomLevel               { get; }
        bool?               AddCheckBoxIsChecked    { get; }
        bool?               EnableDragMap           { get; }
        bool?               ShowCurrentPos          { get; }
        bool?               ShowGrid                { get; }
        bool?               ShowPad                 { get; }
        bool?               ShowPin                 { get; }
        bool?               ShowSelectedDut         { get; }
        bool                IsEnableMoving          { get; }
        double              CurXPos                 { get; }
        double              CurYPos                 { get; }
        IAsyncCommand DutAddMouseDownCommand { get; }
        Task DutAddbyMouseDown();
    }
}