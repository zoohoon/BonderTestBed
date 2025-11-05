using System;

namespace SharpDXRender.RenderObjectPack
{
    using DXControlBase;
    using LogModule;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;
    using System.Runtime.Serialization;
    using WinPoint = System.Windows.Point;

    [DataContract]
    public class RenderText : RenderObject
    {
        static SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();
        public TextFormat Format { get; set; }
        [DataMember]
        public String Text { get; set; }
        public RenderText(String text, float left, float top, float width, float height, String strokeColor, float strokeWidth = 1f)
            : base(left + (width / 2), top + (height / 2), width, height, DefaultColor, strokeWidth, strokeColor)
        {
            try
            {
                Text = text;
                Format = new TextFormat(fontFactory, "Segoe UI", StrokeWidth);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Draw(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                Draw(target, resCache, StrokeColor, StrokeWidth);
            }
#pragma warning disable 0168
            catch (Exception err)
            {
                throw;
            }
#pragma warning restore 0168
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, float strokeWidth)
        {
            try
            {
                Draw(target, resCache, StrokeColor, strokeWidth);
            }
#pragma warning disable 0168
            catch (Exception err)
            {
                throw;
            }
#pragma warning restore 0168
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, String strokeColor, float strokeWidth)
        {
            try
            {
                //==> make rect
                TextFormat format = new TextFormat(fontFactory, "Segoe UI", strokeWidth);
                RawRectangleF rect = CreateRectangle();

                if (StrokeColor != DefaultColor & Text != null)
                    target.DrawText(Text, format, rect, resCache[strokeColor] as Brush);
                //target.DrawText(Text, 10, format, rect, resCache[color] as Brush, DrawTextOptions.None, MeasuringMode.Natural);

                //RawVector2 origin = new RawVector2();
                //TextLayout textLayout = new TextLayout();

                //target.DrawTextLayout(origin, textLayout, resCache["Black"] as Brush, DrawTextOptions.None);

                //DrawText(string text, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultForegroundBrush);
                //DrawText(string text, int stringLength, TextFormat textFormat, RawRectangleF layoutRect, Brush defaultFillBrush, DrawTextOptions options, MeasuringMode measuringMode);
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
