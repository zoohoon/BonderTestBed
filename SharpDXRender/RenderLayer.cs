using LogModule;
using System;
using System.Collections.Generic;

namespace SharpDXRender
{
    using DXControlBase;
    using SharpDXRender.RenderObjectPack;
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.Mathematics.Interop;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using WinPoint = System.Windows.Point;
    using WinSize = System.Windows.Size;

    public interface IHasRenderLayer
    {
        RenderLayer SharpDXLayer { get; set; }
    }
    //==> Rendering

    public abstract class RenderLayer : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //==> Layer에 MouseDown Event 발생시 호출되는 핸들러 함수
        private MouseButtonEventHandler _MouseDownHandler;
        public MouseButtonEventHandler MouseDownHandler
        {
            get { return _MouseDownHandler; }
            set
            {
                if (value != _MouseDownHandler)
                {
                    _MouseDownHandler = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Layer에 MouseDown한 좌표
        private WinPoint _MouseDownPos;
        public WinPoint MouseDownPos
        {
            get { return _MouseDownPos; }
            set
            {
                if (value != _MouseDownPos)
                {
                    _MouseDownPos = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> MouseDown 할때 마다 Layer가 이동된 좌표
        private SharingPoint _MouseMoveAmount;
        public SharingPoint MouseMoveAmount
        {
            get { return _MouseMoveAmount; }
            set
            {
                if (value != _MouseMoveAmount)
                {
                    _MouseMoveAmount = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Layer 현재 Zoom Level
        private float _ZoomLevel;
        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Layer의 최대 Zoom Level
        private float _MaxZoomLevel;
        public float MaxZoomLevel
        {
            get { return _MaxZoomLevel; }
            set
            {
                if (value != _MaxZoomLevel)
                {
                    _MaxZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Layer의 최소 Zoom Level
        private float _MinZoomLevel;
        public float MinZoomLevel
        {
            get { return _MinZoomLevel; }
            set
            {
                if (value != _MinZoomLevel)
                {
                    _MinZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Zoom 증가/감소 량
        private float _ZoomSize;
        public float ZoomSize
        {
            get { return _ZoomSize; }
            set
            {
                if (value != _ZoomSize)
                {
                    _ZoomSize = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Layer의 Degree, Angle 각도 변환용
        private float _Degree;
        public float Degree
        {
            get { return _Degree; }
            set
            {
                if (value != _Degree)
                {
                    _Degree = value;
                    RaisePropertyChanged();
                }
            }
        }
        //==> Layer의 각도
        public float Angle
        {
            get { return (float)((Math.PI * Degree) / 180); }
        }

        public RawColor4 BackgroundColor { get; set; }

        private WinPoint _LayerCenterPos;
        public WinPoint LayerCenterPos
        {
            get { return _LayerCenterPos; }
            set
            {
                if (value != _LayerCenterPos)
                {
                    _LayerCenterPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> Layer의 크기, Window 크기와 같게 해주어야 함
        private WinSize _LayerSize;
        public WinSize LayerSize
        {
            get { return _LayerSize; }
            set
            {
                if (value != _LayerSize)
                {
                    _LayerSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        //==> SharpDX Render시 수행하는 초기화를 한번만 수행 시키기 위해 필요
        private bool _OnceInit;
        public bool OnceInit
        {
            get { return _OnceInit; }
            set
            {
                if (value != _OnceInit)
                {
                    _OnceInit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MouseDownEventEnable;
        public bool MouseDownEventEnable
        {
            get { return _MouseDownEventEnable; }
            set
            {
                if (value != _MouseDownEventEnable)
                {
                    _MouseDownEventEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MouseWheelEventEnable;
        public bool MouseWheelEventEnable
        {
            get { return _MouseWheelEventEnable; }
            set
            {
                if (value != _MouseWheelEventEnable)
                {
                    _MouseWheelEventEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool? _CallRenderEnable;
        public bool? CallRenderEnable
        {
            get { return _CallRenderEnable; }
            set
            {
                if (value != _CallRenderEnable)
                {
                    _CallRenderEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RenderLayer(WinSize layerSize, float r, float g, float b, float a)
        {
            try
            {
                //==> Render Object Setting
                LayerSize = layerSize;
                BackgroundColor = new RawColor4(r, g, b, a);
                InitLayer();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //public RenderLayer()
        //{
        //    BackgroundColor = new RawColor4(50 / 255f, 50 / 255f, 50 / 255f, 1);
        //    InitLayer();
        //}
        public RenderLayer(WinSize layerSize)
        {
            try
            {
                //==> Render Object Setting
                LayerSize = layerSize;
                BackgroundColor = new RawColor4(50 / 255f, 50 / 255f, 50 / 255f, 1);
                InitLayer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void InitLayer()
        {
            try
            {
                LayerCenterPos = new WinPoint(LayerSize.Width / 2f, LayerSize.Height / 2f);
                MouseDownPos = new WinPoint();
                MouseMoveAmount = new SharingPoint();
                MouseDownEventEnable = true;
                MouseWheelEventEnable = true;
                CallRenderEnable = true;

                ZoomSetting(1, 20, 1);

                //Init();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public void ChangedLayerSize(WinSize layerSize)
        //{
        //    LayerSize = layerSize;
        //    LayerCenterPos = new WinPoint(LayerSize.Width / 2f, LayerSize.Height / 2f);
        //    Init();
        //}

        //public void UpdateRenderLayer(WinSize layerSize)
        //{
        //    LayerSize = layerSize;
        //    LayerCenterPos = new WinPoint(LayerSize.Width / 2f, LayerSize.Height / 2f);
        //    Init();
        //}

        public abstract void Init();

        #region ==> Render
        public void Render(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                RawMatrix3x2 currentTransform = target.Transform;
                target.Transform = LayerTranslation() * LayerRotation() * LayerScaling();

                AntialiasMode antialiasMode = target.AntialiasMode;
                target.AntialiasMode = AntialiasMode.Aliased;

                RenderCore(target, resCache);

                target.AntialiasMode = antialiasMode;
                target.Transform = currentTransform;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected abstract void RenderCore(RenderTarget target, ResourceCache resCache);
        public abstract void InitRender(RenderTarget target, ResourceCache resCache);
        #endregion

        #region ==> Render Hud
        public void RenderHud(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                AntialiasMode antialiasMode = target.AntialiasMode;
                target.AntialiasMode = AntialiasMode.Aliased;

                RenderHudCore(target, resCache);

                target.AntialiasMode = antialiasMode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected abstract void RenderHudCore(RenderTarget target, ResourceCache resCache);
        #endregion

        #region ==> Matrix3X2
        //==> 각 Layer는 Matrix를 재 정의해서 사용 할 수 있다.
        protected virtual Matrix3x2 LayerTranslation()
        {
            return Matrix3x2.Translation((float)(MouseMoveAmount.X), (float)(MouseMoveAmount.Y));
        }
        //==> Layer의 회전
        protected virtual Matrix3x2 LayerRotation()
        {
            return Matrix3x2.Rotation(Angle, new RawVector2((float)(LayerCenterPos.X + MouseMoveAmount.X), (float)(LayerCenterPos.Y + MouseMoveAmount.Y)));
        }
        //==> Layer의 확대/축소
        protected virtual Matrix3x2 LayerScaling()
        {
            return Matrix3x2.Scaling(ZoomLevel, ZoomLevel, new Vector2((float)LayerCenterPos.X, (float)LayerCenterPos.Y));
        }
        #endregion

        public void ZoomIn()
        {
            try
            {
                if (MinZoomLevel <= ZoomLevel && ZoomLevel < MaxZoomLevel)
                    ZoomLevel += ZoomSize;
                else
                    ZoomLevel = MaxZoomLevel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void ZoomOut()
        {
            try
            {
                if (ZoomLevel > MinZoomLevel)
                    ZoomLevel -= ZoomSize;
                else
                    ZoomLevel = MinZoomLevel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void ZoomSetting(int startZoomLevel, int zoomCount, int zoomSize)
        {
            try
            {
                ZoomLevel = startZoomLevel * zoomSize;
                MinZoomLevel = 1 * zoomSize;
                MaxZoomLevel = zoomCount * zoomSize;
                ZoomSize = zoomSize;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void LeftRotate(float degree)
        {
            Degree -= degree;
        }
        public void RightRotate(float degree)
        {
            Degree += degree;
        }
        public void MoveMatrixPivot(WinPoint point)
        {
            try
            {
                MouseDownPos = point;

                double centerFromMouseX = LayerCenterPos.X - MouseDownPos.X;
                double centerFromMouseY = LayerCenterPos.Y - MouseDownPos.Y;

                centerFromMouseX /= ZoomLevel;
                centerFromMouseY /= ZoomLevel;

                MouseMoveAmount.X += centerFromMouseX;
                MouseMoveAmount.Y += centerFromMouseY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void MoveMatrixPivot(double relMoveX, double relMoveY)
        {
            try
            {
                MouseMoveAmount.X += relMoveX;
                MouseMoveAmount.Y += relMoveY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public abstract List<RenderContainer> GetRenderContainers();
    }
    public class SharingPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public SharingPoint()
        {
            try
            {
                X = 0;
                Y = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public SharingPoint(double x, double y)
        {
            try
            {
                X = x;
                Y = y;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }


}
