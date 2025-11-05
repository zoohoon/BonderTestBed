using System;
using System.Collections.Generic;
using System.Linq;

namespace DutViewer
{
    using DXControlBase;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using SharpDX.Direct2D1;
    using SharpDXRender;
    using SharpDXRender.RenderObjectPack;
    using System.Windows;
    using WinPoint = System.Windows.Point;
    using WinSize = System.Windows.Size;

    public class DutRenderLayer : RenderLayer, IFactoryModule
    {
        public RenderContainer DutRenderContainer { get; set; }
        public RenderContainer PinRenderContainer { get; set; }
        public RenderContainer GroupRectContainer { get; set; }
        public Dictionary<RenderObject, IPinData> PinDataDic { get; set; }
        public Dictionary<RenderRectangle, List<RenderEllipse>> PinContainerDic { get; set; }
        public RenderObject RedDot { get; set; }
        public RenderObject InfoText { get; set; }
        public IPinData SelectedPinData { get; set; }
        public RenderObject SelectedPinRender { get; set; }
        public RenderObject SelectedDutRender { get; set; }
        public GroupSpliterRenderLayer GroupSpliterRenderData { get; set; }
        public IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }
        public DutRenderLayer(WinSize layerSize)
            : base(layerSize)
        {
            try
            {
            DutRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
            PinRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
            GroupRectContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));

            PinDataDic = new Dictionary<RenderObject, IPinData>();
            PinContainerDic = new Dictionary<RenderRectangle, List<RenderEllipse>>();

            //==> Red dot Setting
            float redDotSize = 5;
            RedDot = new RenderEllipse(0, 0, redDotSize, redDotSize, "Red");
            RedDot.DrawBasePos = LayerCenterPos;

            //==> Group Spliter Render
            GroupSpliterRenderData = new GroupSpliterRenderLayer(layerSize);
            GroupSpliterRenderData.Init();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public override void Init()
        {
            try
            {
            /*
             * ==> How to get Dut Rectangle Size from real dut size
             * 
             * Dut Size : Dut Rectangle Size == Stage Size : Layer Size
             * 
             * Dut Rectangle Size  * Stage Size  == Dut Size * Layer Size
             * 
             * Dut Rectangle Size  == (Dut Size * Layer Size) / Stage Size  
             * 
             * [Layer Size / Stage Size  ==> Ratio]
             * 
             * Dut Rectangle Size  == Dut Size * Ratio
             */

            //==> Render Object Setting
            float stageWidth = 400000;
            float stageHeight = 400000;
            float ratioX = (float)LayerSize.Width / stageWidth;
            float ratioY = (float)LayerSize.Height / stageHeight;



            float dutRectWidth = (float)(ProbeCard.ProbeCardDevObjectRef.DutSizeX.Value * ratioX);
            float dutRectHeight = (float)(ProbeCard.ProbeCardDevObjectRef.DutSizeY.Value * ratioY);
            float pinSize = (float)(ratioX * 200.0);//==> PinSize 200

            //==> Add Dut
            ProbeCard.CandidateDutList = ProbeCard.ProbeCardDevObjectRef.DutList;
            if (ProbeCard.CandidateDutList == null)
                return;

            foreach (IDut displayDut in ProbeCard.CandidateDutList)
            {
                #region ==> 이상적인 좌표 계산 방식
                //float dutLeft = displayDut.MacIndex.XIndex.Value * dutRectWidth;
                //float dutTop = displayDut.MacIndex.YIndex.Value * dutRectHeight;
                //String dutColor = "dutBrush";
                //if (ProbeCard.isPinGroupProc && displayDut.PinList.Count > 0)
                //    dutColor = "dutwithpinBrush";

                //DutRenderLayout.RenderObjectList.Add(new RectangleObject(dutLeft, dutTop, dutRectWidth, dutRectHeight, dutColor));

                ////==> Add Pin
                //foreach (PinData pin in displayDut.PinList)
                //{
                //    float pinPosX = dutLeft + (float)(pin.RelPos.GetX() * ratioX);
                //    float pinPosY = dutTop + (float)(pin.RelPos.GetY() * ratioY); //원래는 -1 인데 모르겠음
                //    PinRenderLayout.RenderObjectList.Add(new EllipseObject(pinPosX, pinPosY, pinSize, pinSize, "notPerfomedPinBrush"));
                //}
                #endregion

                float dutLeft = displayDut.MacIndex.XIndex * dutRectWidth;
                float dutTop = displayDut.MacIndex.YIndex * dutRectHeight;

                RenderRectangle rectObject = new RenderRectangle(dutLeft, dutTop, dutRectWidth, dutRectHeight, "DutColor", 0);
                DutRenderContainer.RenderObjectList.Add(rectObject);
                //==> Add Pin
                List<RenderEllipse> elipObjectList = new List<RenderEllipse>();
                foreach (IPinData pin in displayDut.PinList)
                {
                    //==> Dut Left Bottom 기준, Pin의 상대 좌표는 Right, Top 방향으로 증가하는 좌표계 사용
                    float pinPosX = dutLeft + (float)(pin.RelPos.GetX() * ratioX);
                    float pinPosY =
                        dutTop +
                        (float)((pin.RelPos.GetY() * ratioY) * -1) +
                        dutRectHeight;

                    RenderEllipse elipObject = new RenderEllipse(pinPosX, pinPosY, pinSize, pinSize, "PinColor", 0.3f);
                    elipObjectList.Add(elipObject);
                    PinRenderContainer.RenderObjectList.Add(elipObject);
                    PinDataDic.Add(elipObject, pin);
                }
                PinContainerDic.Add(rectObject, elipObjectList);
            }

            float dutRenderLeft = DutRenderContainer.RenderObjectList.Min(render => render.CenterX);
            float dutRenderTop = DutRenderContainer.RenderObjectList.Min(render => render.CenterY);
            float dutRenderRight = DutRenderContainer.RenderObjectList.Max(render => render.CenterX);
            float dutRenderBottom = DutRenderContainer.RenderObjectList.Max(render => render.CenterY);

            WinPoint drawBasePos = new WinPoint();
            drawBasePos.X = LayerCenterPos.X + (dutRenderLeft + dutRenderRight) / -2;
            drawBasePos.Y = LayerCenterPos.Y + (dutRenderTop + dutRenderBottom) / -2;
            DutRenderContainer.DrawBasePos = drawBasePos;
            PinRenderContainer.DrawBasePos = drawBasePos;
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
            DutRenderContainer.Draw(target, resCache);
            PinRenderContainer.Draw(target, resCache);
            GroupRectContainer.Draw(target, resCache);
            GroupSpliterRenderData.InitRender(target, resCache);
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
                float minPinZoomVluae = 4;
                float minDutEdgeZoomValue = 1;

                if (ZoomLevel > minDutEdgeZoomValue)
                    DutRenderContainer.Draw(target, resCache, 0.3f);
                else
                    DutRenderContainer.Draw(target, resCache);

                if (ZoomLevel > minPinZoomVluae)
                    PinRenderContainer.Draw(target, resCache);

                GroupRectContainer.Draw(target, resCache);


                UpdateSelect(target, resCache);
                SelectedPinRender?.Draw(target, resCache, 0.5f);
                GroupSpliterRenderData.Render(target, resCache);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        protected override void RenderHudCore(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                RedDot.Draw(target, resCache);
                //==> Red dot Setting

                if (SelectedPinData == null)
                    return;

                if(SelectedPinData.DutMacIndex.Value != null)
                {
                    String infoText = $"Dut : {SelectedPinData.DutMacIndex.Value.XIndex}, {SelectedPinData.DutMacIndex.Value.YIndex}\nPin : {SelectedPinData.RelPos.X.Value}, {SelectedPinData.RelPos.Y.Value}";

                    InfoText = new RenderText(infoText, 0, 0, 150, 50, "White", 15);
                    InfoText.DrawBasePos = new WinPoint(20, 20);
                    InfoText.Draw(target, resCache);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateSelect(RenderTarget target, ResourceCache resCache)
        {
            RenderRectangle closerDutRender = null;
            float minDistance = float.MaxValue;
            foreach (RenderRectangle render in DutRenderContainer.DisplayedRenderList)
            {
                float distance = (float)WinPoint.Subtract(LayerCenterPos, render.GetScopePostion(target)).Length;
                if (distance < minDistance)
                {
                    closerDutRender = render;
                    minDistance = distance;
                }
            }

            if (closerDutRender == null)
                return;

            List<RenderEllipse> pinRenderObjectList = PinContainerDic[closerDutRender];
            RenderEllipse closerPinRender = null;
            minDistance = float.MaxValue;
            foreach (RenderEllipse render in pinRenderObjectList)
            {
                float distance = (float)WinPoint.Subtract(LayerCenterPos, render.GetScopePostion(target)).Length;
                if (distance < minDistance)
                {
                    closerPinRender = render;
                    minDistance = distance;
                }
            }

            if (closerPinRender == null)
                return;

            IPinData pinData;
            PinDataDic.TryGetValue(closerPinRender, out pinData);
            SelectedPinData = pinData;

            SelectedDutRender = closerDutRender;
            SelectedPinRender = closerPinRender;
        }

        public override List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = new List<RenderContainer>();
            try
            {
                containers.Add(DutRenderContainer);
                containers.Add(PinRenderContainer);
                containers.Add(GroupRectContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return containers;
        }
    }
}
