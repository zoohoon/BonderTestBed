using System;

namespace SharpDXRender.RenderObjectPack
{
    using DXControlBase;
    using LogModule;
    using SharpDX.Direct2D1;
    using SharpDX.Mathematics.Interop;
    using System.Runtime.Serialization;
    using WinPoint = System.Windows.Point;

    [DataContract]
    public class RenderEllipse : RenderObject
    {
        public RenderEllipse(float centerX, float centerY, float width, float height, String color, float strokeWidth = 1f, String strokeColor = DefaultColor)
            : base(centerX, centerY, width, height, color, strokeWidth, strokeColor)
        {
        }

        public override void Draw(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                Draw(target, resCache, Color, StrokeWidth);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, float strokeWidth)
        {
            try
            {
                Draw(target, resCache, Color, strokeWidth);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, String color, float strokeWidth)
        {
            try
            {
                Ellipse elip = CreateElip();

                //==> draw ellipse
                if (strokeWidth > 0 && StrokeColor != DefaultColor)
                    target.DrawEllipse(elip, resCache[StrokeColor] as Brush, strokeWidth);

                if (color != DefaultColor)
                    target.FillEllipse(elip, resCache[color] as Brush);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private Ellipse CreateElip()
        {
            try
            {
                WinPoint drawCenterPoint
    = new WinPoint(DrawBasePos.X + CenterX, DrawBasePos.Y + CenterY);

                float radiousX = Width / 2;
                float radiousY = Height / 2;

                //==> make ellipse
                RawVector2 elipCenter = new RawVector2(
                    (float)drawCenterPoint.X,
                    (float)drawCenterPoint.Y);

                Ellipse elip = new Ellipse(elipCenter, radiousX, radiousY);

                return elip;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
