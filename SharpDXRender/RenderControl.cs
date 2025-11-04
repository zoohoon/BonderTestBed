using System;
using LogModule;

namespace SharpDXRender
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDX.Mathematics.Interop;
    using System.Windows;

    //using ProberInterfaces;
    //using ProberInterfaces.PinAlign.ProbeCardData;
    //using ProberInterfaces.Param;

    using WinPoint = System.Windows.Point;

    public class RenderControl : D2dControl
    {
        #region ==> DEP RenderLayerData
        public static readonly DependencyProperty RenderDataProperty =
            DependencyProperty.Register(nameof(RenderLayerData), typeof(RenderLayer), typeof(RenderControl), new FrameworkPropertyMetadata(null));
        public RenderLayer RenderLayerData
        {
            get { return (RenderLayer)this.GetValue(RenderDataProperty); }
            set { this.SetValue(RenderDataProperty, value); }
        }
        #endregion

        private RawColor4 BackgorundColor { get; set; }
        public RenderControl()
        {
            try
            {
                InitEvents();
                InitColorPallette();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void InitEvents()
        {
            try
            {
            this.MouseDown += MapViewControl_MouseDown;
            this.MouseWheel += MapViewControl_MouseWheel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        //==> 색깔 추가
        private void InitColorPallette()
        {
            try
            {
            Func<String, int, int, int, int, bool> addColor = (colorName, r, g, b, a) =>
            {
                RawColor4 color = new RawColor4(
                    r / 255f,
                    g / 255f,
                    b / 255f,
                    a / 255f);
                resCache.Add(colorName, t => new SolidColorBrush(t, color));
                return true;
            };

            addColor("Transparent", 0, 0, 0, 0);
            addColor("Black", 0, 0, 0, 255);
            addColor("White", 255, 255, 255, 255);
            addColor("Red", 255, 0, 0, 255);
            addColor("Green", 0, 255, 0, 255);
            addColor("Blue", 0, 0, 255, 255);
            addColor("Cyan", 0, 255, 255, 255);
            addColor("DutColor", 100, 100, 100, 255);
            addColor("PinColor", 170, 170, 170, 255);

            addColor("RoyalBlue", 65, 105, 255, 255);
            addColor("DarkOrange", 255, 140, 0, 255);
            addColor("Violet", 238, 130, 238, 255);
            addColor("Purple", 160, 32, 240, 255);
            addColor("Yellow", 255, 255, 0, 255);
            addColor("SpringGreen", 0, 255, 127, 255);
            addColor("Gray", 128, 128, 128, 255);

            int OpaqueValue = 64;

            addColor("OpaqueBlack", 0, 0, 0, OpaqueValue);
            addColor("OpaqueWhite", 255, 255, 255, OpaqueValue);
            addColor("OpaqueRed", 255, 0, 0, OpaqueValue);
            addColor("OpaqueGreen", 0, 255, 0, OpaqueValue);
            addColor("OpaqueBlue", 0, 0, 255, OpaqueValue);
            addColor("OpaqueYellow", 255, 255, 0, OpaqueValue);
            addColor("OpaqueCyan", 0, 255, 255, OpaqueValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        #region ==> Event handlers
        private void MapViewControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            try
            {
            if (RenderLayerData == null)
                return;
            if (RenderLayerData.MouseWheelEventEnable == false)
                return;
            //if (e.Delta > 0)
            //    RenderData.ZoomIn();
            //if (e.Delta < 0)
            //    RenderData.ZoomOut();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void MapViewControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
            if (RenderLayerData == null)
                return;

            if (RenderLayerData.MouseDownEventEnable == false)
                return;

            RenderLayerData.MoveMatrixPivot(e.GetPosition((IInputElement)sender));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        #endregion

        #region ==> Render API
        public override void Render(RenderTarget target)
        {
            try
            {
            //==> Binding 과 Render 간의 시점 문제로 Check 해야함
            if (RenderLayerData == null)
                return;

            InitRender(target);
            ClearBackground(target);

            if (RenderLayerData.CallRenderEnable == true)
            {
                RenderLayerData.Render(target, resCache);
                //RenderMinimap(target);
                RenderLayerData.RenderHud(target, resCache);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        //==> 화면 Clear
        private void ClearBackground(RenderTarget target)
        {
            try
            {
            //target.Clear(new RawColor4(50 / 255f, 50 / 255f, 50 / 255f, 1));
            target.Clear(BackgorundColor);
            target.Clear(new RawColor4(0.196078435f, 0.196078435f, 0.196078435f, 0));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        //==> Render시 처음 한번은 호출 되어야 하는 초기화 과정
        private void InitRender(RenderTarget target)
        {
            try
            {
            if (RenderLayerData.OnceInit)
            {
                return;
            }

            BackgorundColor = RenderLayerData.BackgroundColor;

            RenderLayerData.OnceInit = true;

            RenderLayerData.InitRender(target, resCache);
            if (RenderLayerData.MouseDownHandler != null)
                this.MouseDown += RenderLayerData.MouseDownHandler;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        #region ==> RENDER : Minimap
        //private float _RedutionValue = 3.5f;
        //private float _MinimapMoveX;
        //private float _MinimapMoveY;
        //private void RenderMinimap(RenderTarget target)
        //{
        //    float minMapVisibleZoomLevel = 4;
        //    if (RenderData.ZoomLevel < minMapVisibleZoomLevel)
        //        return;

        //    AntialiasMode antialiasMode = target.AntialiasMode;
        //    target.AntialiasMode = AntialiasMode.Aliased;

        //    _RedutionValue = 3f;
        //    _MinimapMoveX = 85 * _RedutionValue;
        //    _MinimapMoveY = -90 * _RedutionValue;

        //    //==> get minimap background matrixt
        //    Matrix3x2 minimapBackgroundMatrix = MinimapTranslation() * MinimapScaling(1);

        //    //==> get minimap Background Scope Rect
        //    WinPoint minimapLTScopePos = MatrixConverter.ApplyMatrixPoint(minimapBackgroundMatrix, (float)_DutRenderLayout.Edge.Left, (float)_DutRenderLayout.Edge.Top);
        //    WinPoint minimapRBScopePos = MatrixConverter.ApplyMatrixPoint(minimapBackgroundMatrix, (float)_DutRenderLayout.Edge.Right, (float)_DutRenderLayout.Edge.Bottom);
        //    Rect minimapEdge = new Rect(minimapLTScopePos, minimapRBScopePos);

        //    //==> Render Object Edge
        //    float renderLeftEdge = _DutRenderLayout.RenderObjectList.Min(render => (float)render.RB.X);
        //    float renderTopEdge = _DutRenderLayout.RenderObjectList.Min(render => (float)render.RB.Y);
        //    float renderRightEdge = _DutRenderLayout.RenderObjectList.Max(render => (float)render.LT.X);
        //    float renderBottomEdge = _DutRenderLayout.RenderObjectList.Max(render => (float)render.LT.Y);
        //    Rect renderEdge = new Rect(new WinPoint(renderLeftEdge, renderTopEdge), new WinPoint(renderRightEdge, renderBottomEdge));
        //    //DebugRectPrint(target, renderEdge);

        //    //==> get edge object list
        //    List<RenderObject> edgeObjectList = _DutRenderLayout.RenderObjectList
        //        .Where(render => renderEdge.Contains(new Rect(render.LT, render.RB)) == false)
        //        .ToList();

        //    //==> get render object matrix
        //    Matrix3x2 matrix = MinimapTranslation() * MinimapRotation() * MinimapScaling(1);
        //    Matrix3x2 minimapObjectMatrix = matrix;
        //    for (int figureZoom = 1; figureZoom < _MaxZoomLevel; figureZoom++)
        //    {
        //        matrix = MinimapTranslation() * MinimapRotation() * MinimapScaling(figureZoom);
        //        bool allInMinimap = true;
        //        foreach (RenderObject render in edgeObjectList)
        //        {
        //            WinPoint renderLTScopePos = MatrixConverter.ApplyMatrixPoint(matrix, render.LT);
        //            WinPoint renderRTScopePos = MatrixConverter.ApplyMatrixPoint(matrix, render.RT);
        //            WinPoint renderLBScopePos = MatrixConverter.ApplyMatrixPoint(matrix, render.LB);
        //            WinPoint renderRBScopePos = MatrixConverter.ApplyMatrixPoint(matrix, render.RB);
        //            if (minimapEdge.Contains(renderLTScopePos) == false ||
        //                minimapEdge.Contains(renderRTScopePos) == false ||
        //                minimapEdge.Contains(renderLBScopePos) == false ||
        //                minimapEdge.Contains(renderRBScopePos) == false)
        //            {
        //                allInMinimap = false;
        //                break;
        //            }
        //        }

        //        if (allInMinimap == false)
        //            break;

        //        minimapObjectMatrix = matrix;
        //    }

        //    RawMatrix3x2 currentTransform = target.Transform;
        //    //==> Minimap Background Draw
        //    target.Transform = minimapBackgroundMatrix;

        //    RawRectangleF rect = new RawRectangleF(
        //        (float)_DutRenderLayout.Edge.Left,
        //        (float)_DutRenderLayout.Edge.Top,
        //        (float)_DutRenderLayout.Edge.Right,
        //        (float)_DutRenderLayout.Edge.Bottom);

        //    target.FillRectangle(rect, resCache["White"] as Brush);

        //    //==> Minimap object Draw
        //    target.Transform = minimapObjectMatrix;

        //    Rect backEdge = _DutRenderLayout.Edge;
        //    _DutRenderLayout.Edge = minimapEdge;
        //    _DutRenderLayout.Render(target, resCache);
        //    _DutRenderLayout.Edge = backEdge;

        //    target.Transform = currentTransform;
        //    target.AntialiasMode = antialiasMode;
        //}
        //private Matrix3x2 MinimapTranslation()
        //{
        //    return Matrix3x2.Translation(_MinimapMoveX, _MinimapMoveY);
        //}
        //private Matrix3x2 MinimapRotation()
        //{
        //    return Matrix3x2.Rotation(Angle, new RawVector2((float)_ScopeCenterPos.X + _MinimapMoveX, (float)_ScopeCenterPos.Y + _MinimapMoveY));
        //}
        //private Matrix3x2 MinimapScaling(float zoom)
        //{
        //    return Matrix3x2.Scaling(zoom / _RedutionValue, zoom / _RedutionValue, new Vector2((float)_ScopeCenterPos.X + _MinimapMoveX, (float)_ScopeCenterPos.Y + _MinimapMoveY));
        //}
        #endregion
        #endregion

        #region ==> Debug Function
        private void DebugRectPrint(RenderTarget target, float left, float top, float right, float bottom)
        {
            try
            {
            RawRectangleF rect = new RawRectangleF(left, top, right, bottom);
            //==> draw rectangle
            target.DrawRectangle(rect, resCache["Red"] as Brush);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void DebugRectPrint(RenderTarget target, WinPoint leftTop, WinPoint rightBottom)
        {
            try
            {
            RawRectangleF rect = new RawRectangleF((float)leftTop.X, (float)leftTop.Y, (float)rightBottom.X, (float)rightBottom.Y);
            //==> draw rectangle
            target.DrawRectangle(rect, resCache["Red"] as Brush);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void DebugRectPrint(RenderTarget target, Rect rectangle)
        {
            try
            {
            RawRectangleF rect = new RawRectangleF((float)rectangle.Left, (float)rectangle.Top, (float)rectangle.Right, (float)rectangle.Bottom);
            //==> draw rectangle
            target.DrawRectangle(rect, resCache["Green"] as Brush);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        #endregion
    }
}
