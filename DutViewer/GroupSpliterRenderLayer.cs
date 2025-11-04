using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DutViewer
{
    using System.Windows;
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDXRender;
    using SharpDXRender.RenderObjectPack;
    using WinPoint = System.Windows.Point;
    using WinSize = System.Windows.Size;

    public class GroupSpliterRenderLayer : RenderLayer
    {
        public bool IsVisible { get; set; }
        public RenderContainer GroupSpliterContainer { get; set; }
        public GroupSpliterRenderLayer(WinSize layerSize)
            : base(layerSize)
        {
            GroupSpliterContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        }
        public override void Init()
        {
            try
            {
            RenderLine line1 =
                new RenderLine(new WinPoint(0, 0), new WinPoint(LayerSize.Width, LayerSize.Width), "Red", 0.5f);
            RenderLine line2 =
                new RenderLine(new WinPoint(LayerSize.Width, 0), new WinPoint(0, LayerSize.Width), "Red", 0.5f);
            GroupSpliterContainer.RenderObjectList.Add(line1);
            GroupSpliterContainer.RenderObjectList.Add(line2);

            float dutRenderLeft = GroupSpliterContainer.RenderObjectList.Min(render => render.CenterX);
            float dutRenderTop = GroupSpliterContainer.RenderObjectList.Min(render => render.CenterY);
            float dutRenderRight = GroupSpliterContainer.RenderObjectList.Max(render => render.CenterX);
            float dutRenderBottom = GroupSpliterContainer.RenderObjectList.Max(render => render.CenterY);

            WinPoint drawBasePos = new WinPoint();
            drawBasePos.X = LayerCenterPos.X + (dutRenderLeft + dutRenderRight) / -2;
            drawBasePos.Y = LayerCenterPos.Y + (dutRenderTop + dutRenderBottom) / -2;
            GroupSpliterContainer.DrawBasePos = drawBasePos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public override void InitRender(RenderTarget target, ResourceCache resCache)
        {
            try
            {
            if (IsVisible == false)
                return;
            GroupSpliterContainer.Draw(target, resCache);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        protected override void RenderCore(RenderTarget target, ResourceCache resCache)
        {
            try
            {
            if (IsVisible == false)
                return;
            GroupSpliterContainer.Draw(target, resCache);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        protected override void RenderHudCore(RenderTarget target, ResourceCache resCache)
        {
        }

        public override List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = new List<RenderContainer>();
            try
            {
                containers.Add(GroupSpliterContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return containers;
        }
    }
}
