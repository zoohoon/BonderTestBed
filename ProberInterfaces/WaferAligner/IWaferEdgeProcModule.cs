using System.Collections.Generic;
using System.Windows;

namespace ProberInterfaces.WaferAligner
{
    public interface IWaferEdgeProcModule
    {
        void DoClearRecoveryData();
    }

    public class WaferEdgeBlobData
    {
        public double Area { get; set; }
        public double AspectRatio { get; set; }
        public List<Point> Coordinates { get; set; }
        public double DistanceToDiagonal { get; set; }
        public double ShapeSimilarity { get; set; }
        public System.Windows.Point Gravity { get; set; }

        public WaferEdgeBlobData(double area, double aspectRatio, List<Point> coordinates, double distanceToDiagonal, double shapeSimilarity, Point gravity)
        {
            Area = area;
            Coordinates = coordinates;
            AspectRatio = aspectRatio;
            DistanceToDiagonal = distanceToDiagonal;
            ShapeSimilarity = shapeSimilarity;
            Gravity = gravity;
        }
    }
}
