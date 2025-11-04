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
    public class RenderLine : RenderObject
    {
        //==>
        public WinPoint PT1 { get; set; }
        public WinPoint PT2 { get; set; }
        public RenderLine(WinPoint pt1, WinPoint pt2, String strokColor, float strokeWidth = 1f)
            : base(
                  (float)pt2.X + (float)(pt1.X - pt2.X) / 2,//==> 선의 중심 X
                  (float)pt2.Y + (float)(pt1.Y - pt2.Y) / 2,//==> 선의 중심 Y
                  (float)Math.Abs(pt1.X - pt2.X) / 2,//==> 선의 가로 길이
                  (float)Math.Abs(pt1.Y - pt2.Y) / 2,//==> 선의 세로 길이
                  DefaultColor, strokeWidth, strokColor)//==> 선을 이었을 때 
        {
            try
            {
                PT1 = pt1;
                PT2 = pt2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void Draw(RenderTarget target, ResourceCache resCache)
        {
            Draw(target, resCache, StrokeColor, StrokeWidth);
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, float strokeWidth)
        {
            Draw(target, resCache, StrokeColor, strokeWidth);
        }
        public override void Draw(RenderTarget target, ResourceCache resCache, String strokeColor, float strokeWidth)
        {
            try
            {
                RawVector2 pt1;
                RawVector2 pt2;
                CreateLinePoint(out pt1, out pt2);

                if (StrokeColor != DefaultColor)
                    target.DrawLine(pt1, pt2, resCache[strokeColor] as Brush, strokeWidth);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //==> Window에서 그릴 수 있는 좌표로 변환
        private void CreateLinePoint(out RawVector2 pt1, out RawVector2 pt2)
        {
            try
            {
                pt1 = new RawVector2((float)(DrawBasePos.X + PT1.X), (float)(DrawBasePos.Y + PT1.Y));
                pt2 = new RawVector2((float)(DrawBasePos.X + PT2.X), (float)(DrawBasePos.Y + PT2.Y));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


    }
}
