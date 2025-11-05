using LogModule;
using System;

namespace SharpDXRender.RenderObjectPack
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDX.Mathematics.Interop;
    using System.Runtime.Serialization;
    using WinPoint = System.Windows.Point;

    [DataContract]
    public class RenderRectangle : RenderObject
    {
        public RenderRectangle(float left, float top, float width, float height, String color, float strokeWidth = 1f, String strokeColor = DefaultColor)
            : base(left + (width / 2), top + (height / 2), width, height, color, strokeWidth, strokeColor)
        {
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
            try
            {
                RawRectangleF rect = CreateRectangle();

                //==> Draw Edge
                if (strokeWidth > 0 && StrokeColor != DefaultColor)
                    target.DrawRectangle(rect, resCache[StrokeColor] as Brush, strokeWidth);

                //==> Fill Rectangle
                if (color != DefaultColor)
                    target.FillRectangle(rect, resCache[color] as Brush);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RawRectangleF CreateRectangle()
        {
            try
            {
                float radiousX = Width / 2;
                float radiousY = Height / 2;

                WinPoint drawStartPoint =
                    new WinPoint(DrawBasePos.X + CenterX - radiousX, DrawBasePos.Y + CenterY - radiousY);

                WinPoint drawEndPoint =
                    new WinPoint(drawStartPoint.X + Width, drawStartPoint.Y + Height);

                //==> make rect
                RawRectangleF rect = new RawRectangleF(
                    (float)drawStartPoint.X,
                    (float)drawStartPoint.Y,
                    (float)drawEndPoint.X,
                    (float)drawEndPoint.Y);
                return rect;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
