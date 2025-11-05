using LogModule;
using System;
using System.Collections.Generic;

namespace SharpDXRender.RenderObjectPack
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDX.Mathematics.Interop;
    using System.Windows;
    using System.Runtime.Serialization;
    using WinPoint = System.Windows.Point;

    [DataContract]
    public class RenderGeometry : RenderObject
    {
        private List<Point> GeometryPoint;
        //private Geometry geo;
        [DataMember]
        private bool Fill;
        [DataMember]
        private List<WindowsPoint> WindowsGeometryPoint { get; set; }

        public RenderGeometry(float centerX, float centerY, float width, float height, String color, String strokeColor = DefaultColor, float strokeWidth = 1f)
            : base(centerX, centerY, width, height, color, strokeWidth, strokeColor)
        {
        }

        public RenderGeometry()
        {
        }

        public void SetFill(bool flag)
        {
            Fill = flag;
        }

        public void SetGeometryPoint(List<Point> pts)
        {
            GeometryPoint = pts;
            WindowsGeometryPoint = new List<WindowsPoint>();
            foreach (var point in GeometryPoint)
            {
                WindowsGeometryPoint.Add(new WindowsPoint(point.X, point.Y));
            }
        }

        public override void Draw(RenderTarget target, ResourceCache resCache)
        {
            Draw(target, resCache, Color, StrokeWidth);
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, float strokeWidth)
        {
            Draw(target, resCache, Color, strokeWidth);
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, String color, float strokeWidth)
        {
            Geometry geo = CreateGeometry(target);

            //==> draw Geometry
            if (strokeWidth > 0)
            {
                if (Fill == true)
                {
                    target.FillGeometry(geo, resCache[color] as Brush);
                }
                else
                {
                    target.DrawGeometry(geo, resCache[color] as Brush, strokeWidth);
                }
            }


            //string FullPath = null;

            //System.Windows.Media.Geometry g = System.Windows.Media.Geometry.Parse(FullPath);

            ////==> draw ellipse
            //if (strokeWidth > 0)
            //    target.DrawGeometry(geo, resCache["Black"] as Brush, strokeWidth);
        }
        private Geometry CreateGeometry(RenderTarget target)
        {
            PathGeometry geo = new PathGeometry(target.Factory);
            try
            {

                List<RawVector2> m_path = new List<RawVector2>();

                if (GeometryPoint == null & WindowsGeometryPoint != null)
                {
                    GeometryPoint = new List<WinPoint>();
                    foreach (var point in WindowsGeometryPoint)
                    {
                        GeometryPoint.Add(new WinPoint(point.X, point.Y));
                    }
                }

                foreach (var item in GeometryPoint)
                {
                    RawVector2 tmp = new RawVector2();

                    tmp.X = (float)item.X;
                    tmp.Y = (float)item.Y;

                    m_path.Add(tmp);
                }

                using (GeometrySink Geo_Sink = geo.Open())
                {
                    int count = m_path.Count;

                    //// create the path
                    //Geo_Sink.BeginFigure(m_path[0].GetVector2(), FigureBegin.Filled);
                    //for (int i = 1; i < count; i++)
                    //{
                    //    Geo_Sink.AddLine(m_path[i].GetVector2());
                    //}

                    // create the path
                    Geo_Sink.BeginFigure(m_path[0], FigureBegin.Filled);
                    for (int i = 1; i < count; i++)
                    {
                        Geo_Sink.AddLine(m_path[i]);
                    }

                    Geo_Sink.EndFigure(FigureEnd.Open);
                    Geo_Sink.Close();
                }

                //// check if we use other color
                //if (m_useDefaultColor || m_lineColorBrush == null)
                //{
                //    target.DrawGeometry(geo, brush, m_lineWidth);
                //}
                //else
                //{
                //    target.DrawGeometry(geo, m_lineColorBrush, m_lineWidth);
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return geo;
        }

        //public void Render2D(RenderTarget renderTarget, Brush brush)
        //{
        //    #region Update Color Brush
        //    // if the brush is dirty renew it
        //    if (m_isLineColorBrushDirty && !m_useDefaultColor)
        //    {
        //        // clean the color brush
        //        if (m_lineColorBrush != null)
        //            m_lineColorBrush.Dispose();

        //        // allocate a new one
        //        m_lineColorBrush = new SolidColorBrush(renderTarget, m_lineColor);

        //        // clean the flag
        //        m_isLineColorBrushDirty = false;
        //    }
        //    #endregion

        //    if (m_path.Count > 0)
        //    {
        //        // Update geometry
        //        if (isGeometryDirty)
        //        {
        //            if (m_sharpGeometry != null)
        //                m_sharpGeometry.Dispose();

        //            m_sharpGeometry = new PathGeometry(renderTarget.Factory);

        //            using (GeometrySink Geo_Sink = m_sharpGeometry.Open())
        //            {
        //                int count = m_path.Count;

        //                // create the path
        //                Geo_Sink.BeginFigure(m_path[0].GetVector2(), FigureBegin.Filled);
        //                for (int i = 1; i < count; i++)
        //                {
        //                    Geo_Sink.AddLine(m_path[i].GetVector2());
        //                }
        //                Geo_Sink.EndFigure(FigureEnd.Open);
        //                Geo_Sink.Close();
        //            }

        //            isGeometryDirty = false;
        //        }

        //        // check if we use other color
        //        if (m_useDefaultColor || m_lineColorBrush == null)
        //        {
        //            renderTarget.DrawGeometry(m_sharpGeometry, brush, m_lineWidth);
        //        }
        //        else
        //        {
        //            renderTarget.DrawGeometry(m_sharpGeometry, m_lineColorBrush, m_lineWidth);
        //        }
        //    }
        //}

        //public void RenderScatterGeometry(RenderTarget renderTarget)
        //{
        //    double[] x = curve.X;
        //    double[] y = curve.Y;
        //    int length = x.Length;
        //    double xScale, xOffset, yScale, yOffset;
        //    xScale = graphToCanvas.Matrix.M11;
        //    xOffset = graphToCanvas.Matrix.OffsetX - this.xOffsetMarker;
        //    yScale = graphToCanvas.Matrix.M22;
        //    yOffset = graphToCanvas.Matrix.OffsetY - this.yOffsetMarker;
        //    bool[] include = curve.includeMarker;
        //    StrokeStyleProperties properties = new StrokeStyleProperties();
        //    properties.LineJoin = LineJoin.MiterOrBevel;
        //    StrokeStyle strokeStyle = new StrokeStyle(renderTarget.Factory, properties);
        //    for (int i = 0; i < length; ++i)
        //    {
        //        if (include[i])
        //        {
        //            renderTarget.Transform = (Matrix3x2)Matrix.Translation((float)(x[i] * xScale + xOffset), (float)(y[i] * yScale + yOffset), 0);
        //            renderTarget.FillGeometry(Geometry, FillBrush);
        //            renderTarget.DrawGeometry(Geometry, Brush, (float)StrokeThickness, strokeStyle);
        //        }
        //    }
        //    renderTarget.Transform = Matrix3x2.Identity;
        //}
    }
}
