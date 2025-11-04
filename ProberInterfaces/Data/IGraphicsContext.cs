using System.Collections.ObjectModel;

namespace ProberInterfaces
{
    public interface IGraphicsContext : IFactoryModule
    {
        ObservableCollection<GraphicsParam> Graphicsparam { get; set; }
    }

    public class GraphicsParam
    {
        public double X;
        public double Y;
        public double SizeX;
        public double SizeY;
        public EnumPadShapeType Shape;
        
    }
}
