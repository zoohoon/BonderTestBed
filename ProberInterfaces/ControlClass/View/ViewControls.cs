namespace ProberInterfaces
{
    public interface IMapViewControl
    {
        IWaferObject WaferObject { get; set; }
        bool EnalbeClickToMove { get; set; }
        bool IsCrossLineVisible { get; set; }
        ICoordinateManager CoordinateManager { get; set; }
    }

    public interface INeedleCleanView
    {

    }
}