using System;
using System.Collections.Generic;
using LogModule;

namespace SharpDXRender.RenderObjectPack
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using System.Windows;
    using System.Runtime.Serialization;
    using WinPoint = System.Windows.Point;

    [DataContract]
    public class RenderContainer
    {
        public WinPoint DrawBasePos { get; set; }

        private Rect _Edge;

        public Rect Edge
        {
            get { return _Edge; }
            set { _Edge = value; WindowEdge = new WindowRect(_Edge.X, _Edge.Y, _Edge.Width, _Edge.Height); }
        }

        [DataMember]
        public WindowRect WindowEdge { get; set; }
        [DataMember]
        public List<RenderObject> RenderObjectList { get; set; }
        [DataMember]
        public List<RenderObject> DisplayedRenderList { get; set; }

        public RenderContainer(Rect edge)
        {
            try
            {
                Edge = edge;
                RenderObjectList = new List<RenderObject>();
                DisplayedRenderList = new List<RenderObject>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Draw(RenderTarget target, ResourceCache resCache, float strokeWidth = 0)
        {
            try
            {
                int renderCount = 0;
                List<RenderObject> displayedRenderList = new List<RenderObject>();

                foreach (RenderObject render in RenderObjectList)
                {
                    render.UpdateVertex();

                    if (CheckRenderEdgeClash(target, render) == false)
                        continue;
                    if (render.IsVisible == false)
                        continue;

                    render.DrawBasePos = DrawBasePos;

                    if (strokeWidth > 0)
                    {
                        renderCount += 2;

                        render.Draw(target, resCache, strokeWidth);
                    }
                    else
                    {
                        renderCount++;
                        render.Draw(target, resCache);
                    }

                    displayedRenderList.Add(render);
                }

                DisplayedRenderList = displayedRenderList;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[RenderContainer] [Draw()] : {err}");
            }
        }
        public bool CheckRenderEdgeClash(RenderTarget target, RenderObject render)
        {
            try
            {
                WinPoint lt = MatrixConverter.ApplyMatrixPoint(target.Transform, render.LT);
                WinPoint rt = MatrixConverter.ApplyMatrixPoint(target.Transform, render.RT);
                WinPoint lb = MatrixConverter.ApplyMatrixPoint(target.Transform, render.LB);
                WinPoint rb = MatrixConverter.ApplyMatrixPoint(target.Transform, render.RB);

                double xSum = lt.X + rt.X + lb.X + rb.X;
                double ySum = lt.Y + rt.Y + lb.Y + rb.Y;
                WinPoint centerPt = new WinPoint(xSum / 4, ySum / 4);

                double widht1 = Math.Abs(lt.X - rb.X);
                double widht2 = Math.Abs(rt.X - lb.X);

                double height1 = Math.Abs(lt.Y - rb.Y);
                double height2 = Math.Abs(rt.Y - lb.Y);

                double width = widht1 > widht2 ? widht1 : widht2;
                double height = height1 > height2 ? height1 : height2;
                double halfWidth = width / 2;
                double halfHeight = height / 2;

                //==> SharpDX 화면에 render가 출력 되는지 확인
                return Edge.IntersectsWith(new Rect(
                    centerPt.X - halfWidth,
                    centerPt.Y - halfHeight,
                    width,
                    height));
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[RenderContainer] [CheckRenderEdgeClash()] : {err}");
                return true;
            }
        }
    }
}
