using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GeometryHelp
{
    using LogModule;
    using ProberInterfaces.Param;
    using System.Windows;

    public static class PathFigureExtensions
    {
        public static IEnumerable<Point> EnumeratePoints(this PathFigure figure)
        {
            yield return figure.StartPoint;
            foreach (var segment in figure.Segments)
            {
                var lineSegment = segment as LineSegment;
                if (lineSegment != null)
                    yield return lineSegment.Point;
                else
                {
                    var polyLineSegment = segment as PolyLineSegment;
                    if (polyLineSegment == null) continue;
                    foreach (var point in polyLineSegment.Points)
                        yield return point;
                }
            }
        }

        public static IEnumerable<Point> EnumeratePoints(this PathFigure figure, Transform t)
        {
            //Here I am transforming the points using the geometry's transform.
            return EnumeratePoints(figure).Select(p => t.Transform(p));
        }
    }
    public static class GeometryHelper
    {
        public static double GetAngle(PinCoordinate pivot, PinCoordinate point)
        {
            double Degree = 0;
            try
            {
                //==> degree = atan((y2 - cy) / (x2-cx)) - atan((y1 - cy)/(x1-cx)) : 세점사이의 각도 구함
                Degree = Math.Atan2(
                     point.Y.Value - pivot.Y.Value,
                     point.X.Value - pivot.X.Value)
                     * 180 / Math.PI;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetDegree() : Error occured.");
                LoggerManager.Exception(err);

            }
            return Degree;
        }
        public static double GetDistance2D(PinCoordinate FirstPin, PinCoordinate SecondPin)
        {
            double Distance = -1;
            try
            {
                Distance = Math.Sqrt(Math.Pow(FirstPin.GetX() - SecondPin.GetX(), 2) + Math.Pow(FirstPin.GetY() - SecondPin.GetY(), 2));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //LoggerManager.Error($err + "GetDistance2D() : Error occured.");
            }


            return Distance;
        }
        public static double GetDistance2D(double X1, double Y1, double X2, double Y2)
        {
            double Distance = -1;
            try
            {
                Distance = Math.Sqrt(Math.Pow(X1 - X2, 2) + Math.Pow(Y1 - Y2, 2));
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetDistance2D() : Error occured.");
                LoggerManager.Exception(err);

            }
            return Distance;
        }
        public static void GetRotCoordEx(ref PinCoordinate NewPos, PinCoordinate OriPos, PinCoordinate RefPos, double angle)
        {
            double newx = 0.0;
            double newy = 0.0;
            double th = DegreeToRadian(angle);

            try
            {
                NewPos = new PinCoordinate();

                newx = OriPos.X.Value - RefPos.X.Value;
                newy = OriPos.Y.Value - RefPos.Y.Value;

                NewPos.X.Value = newx * Math.Cos(th) - newy * Math.Sin(th) + RefPos.X.Value;
                NewPos.Y.Value = newx * Math.Sin(th) + newy * Math.Cos(th) + RefPos.Y.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public static double DegreeToRadian(double angle)
        {
            double degerr = 0;
            try
            {
                degerr = Math.PI * angle / 180.0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return degerr;
        }
        public static void FindIntersection(
            PinCoordinate p1, PinCoordinate p2, PinCoordinate p3, PinCoordinate p4,
            out bool lines_intersect, out bool segments_intersect,
            out PinCoordinate intersection,
            out PinCoordinate close_p1, out PinCoordinate close_p2)
        {
            lines_intersect = false;
            segments_intersect = false;
            intersection = new PinCoordinate();
            close_p1 = new PinCoordinate();
            close_p2 = new PinCoordinate();

            // Get the segments' parameters.
            double dx12 = p2.X.Value - p1.X.Value;
            double dy12 = p2.Y.Value - p1.Y.Value;
            double dx34 = p4.X.Value - p3.X.Value;
            double dy34 = p4.Y.Value - p3.Y.Value;
            try
            {

                // Solve for t1 and t2
                double denominator = (dy12 * dx34 - dx12 * dy34);

                double t1 =
                    ((p1.X.Value - p3.X.Value) * dy34 + (p3.Y.Value - p1.Y.Value) * dx34)
                        / denominator;
                if (double.IsInfinity(t1))
                {
                    // The lines are parallel (or close enough to it).
                    lines_intersect = false;
                    segments_intersect = false;
                    intersection = new PinCoordinate(float.NaN, float.NaN);
                    close_p1 = new PinCoordinate(float.NaN, float.NaN);
                    close_p2 = new PinCoordinate(float.NaN, float.NaN);
                    return;
                }
                lines_intersect = true;

                double t2 =
                    ((p3.X.Value - p1.X.Value) * dy12 + (p1.Y.Value - p3.Y.Value) * dx12)
                        / -denominator;

                // Find the point of intersection.
                intersection = new PinCoordinate(p1.X.Value + dx12 * t1, p1.Y.Value + dy12 * t1);

                // The segments intersect if t1 and t2 are between 0 and 1.
                segments_intersect =
                    ((t1 >= 0) && (t1 <= 1) &&
                     (t2 >= 0) && (t2 <= 1));

                // Find the closest points on the segments.
                if (t1 < 0)
                {
                    t1 = 0;
                }
                else if (t1 > 1)
                {
                    t1 = 1;
                }

                if (t2 < 0)
                {
                    t2 = 0;
                }
                else if (t2 > 1)
                {
                    t2 = 1;
                }

                close_p1 = new PinCoordinate(p1.X.Value + dx12 * t1, p1.Y.Value + dy12 * t1);
                close_p2 = new PinCoordinate(p3.X.Value + dx34 * t2, p3.Y.Value + dy34 * t2);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"FindIntersection(): Error occurred. Err = {err.Message}");
            }
        }

        public static List<Point> GetIntersectionPoints(PathGeometry FlattenedPath, double[] SegmentLengths)
        {
            List<Point> intersectionPoints = new List<Point>();
            try
            {

                List<Point> pointsOnFlattenedPath = GetPointsOnFlattenedPath(FlattenedPath);

                if (pointsOnFlattenedPath == null || pointsOnFlattenedPath.Count < 2)
                    return intersectionPoints;

                Point currPoint = pointsOnFlattenedPath[0];
                intersectionPoints.Add(currPoint);

                // find point on flattened path that is segment length away from current point

                int flattedPathIndex = 0;

                int segmentIndex = 1;

                while (flattedPathIndex < pointsOnFlattenedPath.Count - 1 &&
                    segmentIndex < SegmentLengths.Length + 1)
                {
                    Point? intersectionPoint = GetIntersectionOfSegmentAndCircle(
                        pointsOnFlattenedPath[flattedPathIndex],
                        pointsOnFlattenedPath[flattedPathIndex + 1], currPoint, SegmentLengths[segmentIndex - 1]);

                    if (intersectionPoint == null)
                        flattedPathIndex++;
                    else
                    {
                        intersectionPoints.Add((Point)intersectionPoint);
                        currPoint = (Point)intersectionPoint;
                        pointsOnFlattenedPath[flattedPathIndex] = currPoint;
                        segmentIndex++;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return intersectionPoints;
        }

        public static List<Point> GetPointsOnFlattenedPath(PathGeometry FlattenedPath, double offsetX = 0, double offsetY = 0)
        {
            List<Point> flattenedPathPoints = new List<Point>();
            try
            {

                // for flattened geometry there should be just one PathFigure in the Figures
                if (FlattenedPath.Figures.Count != 1)
                    return null;

                PathFigure pathFigure = FlattenedPath.Figures[0];

                Point tmp = new Point((pathFigure.StartPoint.X + offsetX), (pathFigure.StartPoint.Y + offsetY));

                flattenedPathPoints.Add(tmp);

                // SegmentsCollection should contain PolyLineSegment and LineSegment
                foreach (PathSegment pathSegment in pathFigure.Segments)
                {
                    if (pathSegment is PolyLineSegment)
                    {
                        PolyLineSegment seg = pathSegment as PolyLineSegment;

                        foreach (Point point in seg.Points)
                        {
                            tmp = new Point((point.X + offsetX), (point.Y + offsetY));

                            flattenedPathPoints.Add(tmp);
                        }
                    }
                    else if (pathSegment is LineSegment)
                    {
                        LineSegment seg = pathSegment as LineSegment;

                        tmp = new Point((seg.Point.X + offsetX), (seg.Point.Y + offsetY));

                        flattenedPathPoints.Add(tmp);
                    }
                    else
                        throw new Exception("GetIntersectionPoint - unexpected path segment type: " + pathSegment.ToString());

                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return (flattenedPathPoints);
        }

        public static PointCollection GetPointCollectionOnFlattenedPath(PathGeometry FlattenedPath, double offsetX = 0, double offsetY = 0)
        {
            PointCollection flattenedPathPoints = new PointCollection();
            try
            {

                // for flattened geometry there should be just one PathFigure in the Figures
                if (FlattenedPath.Figures.Count != 1)
                    return null;

                PathFigure pathFigure = FlattenedPath.Figures[0];

                Point tmp = new Point((pathFigure.StartPoint.X + offsetX), (pathFigure.StartPoint.Y + offsetY));

                flattenedPathPoints.Add(tmp);

                // SegmentsCollection should contain PolyLineSegment and LineSegment
                foreach (PathSegment pathSegment in pathFigure.Segments)
                {
                    if (pathSegment is PolyLineSegment)
                    {
                        PolyLineSegment seg = pathSegment as PolyLineSegment;

                        foreach (Point point in seg.Points)
                        {
                            tmp = new Point((point.X + offsetX), (point.Y + offsetY));

                            flattenedPathPoints.Add(tmp);
                        }
                    }
                    else if (pathSegment is LineSegment)
                    {
                        LineSegment seg = pathSegment as LineSegment;

                        tmp = new Point((seg.Point.X + offsetX), (seg.Point.Y + offsetY));

                        flattenedPathPoints.Add(tmp);
                    }
                    else
                        throw new Exception("GetIntersectionPoint - unexpected path segment type: " + pathSegment.ToString());

                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return (flattenedPathPoints);
        }

        static Point? GetIntersectionOfSegmentAndCircle(Point SegmentPoint1, Point SegmentPoint2,
            Point CircleCenter, double CircleRadius)
        {
            // linear equation for segment: y = mx + b
            double slope = (SegmentPoint2.Y - SegmentPoint1.Y) / (SegmentPoint2.X - SegmentPoint1.X);
            double intercept = SegmentPoint1.Y - (slope * SegmentPoint1.X);

            try
            {
                // special case when segment is vertically oriented
                if (double.IsInfinity(slope))
                {
                    double root = Math.Pow(CircleRadius, 2.0) - Math.Pow(SegmentPoint1.X - CircleCenter.X, 2.0);

                    if (root < 0)
                        return null;


                    // soln 1
                    double SolnX1 = SegmentPoint1.X;
                    double SolnY1 = CircleCenter.Y - Math.Sqrt(root);
                    Point Soln1 = new Point(SolnX1, SolnY1);

                    // have valid result if point is between two segment points
                    if (IsBetween(SolnX1, SegmentPoint1.X, SegmentPoint2.X) &&
                        IsBetween(SolnY1, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                    {
                        // found solution
                        return (Soln1);
                    }

                    // soln 2
                    double SolnX2 = SegmentPoint1.X;
                    double SolnY2 = CircleCenter.Y + Math.Sqrt(root);
                    Point Soln2 = new Point(SolnX2, SolnY2);

                    // have valid result if point is between two segment points
                    if (IsBetween(SolnX2, SegmentPoint1.X, SegmentPoint2.X) &&
                        IsBetween(SolnY2, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                    {
                        // found solution
                        return (Soln2);
                    }
                }
                else
                {
                    // use soln to quadradratic equation to solve intersection of segment and circle:
                    // x = (-b +/ sqrt(b^2-4ac))/(2a)
                    double a = 1 + Math.Pow(slope, 2.0);
                    double b = (-2 * CircleCenter.X) + (2 * (intercept - CircleCenter.Y) * slope);
                    double c = Math.Pow(CircleCenter.X, 2.0) + Math.Pow(intercept - CircleCenter.Y, 2.0) - Math.Pow(CircleRadius, 2.0);

                    // check for no solutions, is sqrt negative?
                    double root = Math.Pow(b, 2.0) - (4 * a * c);

                    if (root < 0)
                        return null;

                    // we might have two solns...

                    // soln 1
                    double SolnX1 = (-b + Math.Sqrt(root)) / (2 * a);
                    double SolnY1 = slope * SolnX1 + intercept;
                    Point Soln1 = new Point(SolnX1, SolnY1);

                    // have valid result if point is between two segment points
                    if (IsBetween(SolnX1, SegmentPoint1.X, SegmentPoint2.X) &&
                        IsBetween(SolnY1, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln1, SegmentPoint1, SegmentPoint2, CircleCenter))
                    {
                        // found solution
                        return (Soln1);
                    }

                    // soln 2
                    double SolnX2 = (-b - Math.Sqrt(root)) / (2 * a);
                    double SolnY2 = slope * SolnX2 + intercept;
                    Point Soln2 = new Point(SolnX2, SolnY2);

                    // have valid result if point is between two segment points
                    if (IsBetween(SolnX2, SegmentPoint1.X, SegmentPoint2.X) &&
                        IsBetween(SolnY2, SegmentPoint1.Y, SegmentPoint2.Y))
                    //if (ValidSoln(Soln2, SegmentPoint1, SegmentPoint2, CircleCenter))
                    {
                        // found solution
                        return (Soln2);
                    }
                }

                // shouldn't get here...but in case
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return null;
        }

        public static PointCollection TransformPoints(PointCollection pc, Transform t)
        {
            PointCollection tp = new PointCollection(pc.Count);
            try
            {
                foreach (Point p in pc)
                    tp.Add(t.Transform(p));
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return tp;
        }
        public static PathGeometry TransformedGeometry(PathGeometry g, Transform t)
        {
            Matrix m = t.Value;

            double scaleX = Math.Sqrt(m.M11 * m.M11 + m.M21 * m.M21);
            double scaleY = (m.M11 * m.M22 - m.M12 * m.M21) / scaleX;

            PathGeometry ng = g.Clone();
            try
            {

                foreach (PathFigure f in ng.Figures)
                {
                    f.StartPoint = t.Transform(f.StartPoint);
                    foreach (PathSegment s in f.Segments)
                    {
                        if (s is LineSegment)
                            (s as LineSegment).Point = t.Transform((s as LineSegment).Point);
                        else if (s is PolyLineSegment)
                            (s as PolyLineSegment).Points = TransformPoints((s as PolyLineSegment).Points, t);
                        else if (s is BezierSegment)
                        {
                            (s as BezierSegment).Point1 = t.Transform((s as BezierSegment).Point1);
                            (s as BezierSegment).Point2 = t.Transform((s as BezierSegment).Point2);
                            (s as BezierSegment).Point3 = t.Transform((s as BezierSegment).Point3);
                        }
                        else if (s is PolyBezierSegment)
                            (s as PolyBezierSegment).Points = TransformPoints((s as PolyBezierSegment).Points, t);
                        else if (s is QuadraticBezierSegment)
                        {
                            (s as QuadraticBezierSegment).Point1 = t.Transform((s as QuadraticBezierSegment).Point1);
                            (s as QuadraticBezierSegment).Point2 = t.Transform((s as QuadraticBezierSegment).Point2);
                        }
                        else if (s is PolyQuadraticBezierSegment)
                            (s as PolyQuadraticBezierSegment).Points = TransformPoints((s as PolyQuadraticBezierSegment).Points, t);
                        else if (s is ArcSegment)
                        {
                            ArcSegment a = s as ArcSegment;
                            a.Point = t.Transform(a.Point);
                            a.Size = new Size(a.Size.Width * scaleX, a.Size.Height * scaleY); // NEVER TRIED
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return ng;
        }

        //private static void SetScaleAndTranslateTransform(object obj, double translateX, double translateY, double scaleWidth, double scaleHeight)
        //{
        //    var geometryDrawing = obj as System.Windows.Media.GeometryDrawing;
        //    if (geometryDrawing != null)
        //    {
        //        geometryDrawing.Transform = new MatrixTransform(scaleWidth, 0, 0, scaleHeight, translateX, translate);
        //        return;
        //    }

        //    var drawingGroup = obj as System.Windows.Media.DrawingGroup;
        //    if (drawingGroup == null) return;

        //    foreach (object objChild in drawingGroup.Children)
        //        SetScaleAndTranslateTransform(objChild, translateX, translateY, scaleWidth, scaleHeight);
        //}
        static double TOL = 0.0000001;
        public static Circle CircleFromPoints(CatCoordinates p1, CatCoordinates p2, CatCoordinates p3)
        {
            
            double offset = Math.Pow(p2.X.Value, 2) + Math.Pow(p2.Y.Value, 2);
            double bc = (Math.Pow(p1.X.Value, 2) + Math.Pow(p1.Y.Value, 2) - offset) / 2.0;
            double cd = (offset - Math.Pow(p3.X.Value, 2) - Math.Pow(p3.Y.Value, 2)) / 2.0;
            double det = (p1.X.Value - p2.X.Value) * (p2.Y.Value - p3.Y.Value) - (p2.X.Value - p3.X.Value) * (p1.Y.Value - p2.Y.Value);

            if (Math.Abs(det) < TOL) { throw new ArithmeticException("CircleFromPoints(): Points are out of tolerence."); }

            double idet = 1 / det;

            double centerx = (bc * (p2.Y.Value - p3.Y.Value) - cd * (p1.Y.Value - p2.Y.Value)) * idet;
            double centery = (cd * (p1.X.Value - p2.X.Value) - bc * (p2.X.Value - p3.X.Value)) * idet;
            double radius =
               Math.Sqrt(Math.Pow(p2.X.Value - centerx, 2) + Math.Pow(p2.Y.Value - centery, 2));

            return new Circle(new Point(centerx, centery), radius);
        }
        public class Circle
        {
            public Point Center { get; set; }
            public double Radius { get; set; }
            public Circle(Point center, double radius)
            {
                this.Center = center; this.Radius = radius;
            }
            public override String ToString()
            {
                return new StringBuilder().Append("Center= ").Append(Center).Append(", r=").Append(Radius).ToString();
            }
        }
        static bool IsBetween(double X, double X1, double X2)
        {
            try
            {
                if (X1 >= X2 && X <= X1 && X >= X2)
                    return true;

                if (X1 <= X2 && X >= X1 && X <= X2)
                    return true;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }

            return false;
        }

        public static double GetDistance2d(double x1, double y1, double x2, double y2)
        {
            double distance2d = double.NaN;
            try
            {
                distance2d = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y2 - y2, 2));

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetDistance2d(): Distance error. ({x1}. {y1}), ({x2}. {y2}). Err = {err.Message}");
            }
            return distance2d;
        }
    }
}
