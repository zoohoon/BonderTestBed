using ProberInterfaces;
using SharpDXRender;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DXControlBase;
using SharpDX.Direct2D1;
using System.Windows;
using SharpDXRender.RenderObjectPack;
using WinPoint = System.Windows.Point;
using LogModule;

namespace PathMakerControlViewModel
{
    public class PathMakerRenderLayer : RenderLayer, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public RenderContainer LineRenderContainer { get; set; }

        public RenderContainer PathRenderContainer { get; set; }

        public RenderContainer CursorRenderContainer { get; set; }

        public RenderContainer PathPointRenderContainer { get; set; }

        private RenderEllipse _Cursor;
        public RenderEllipse Cursor
        {
            get { return _Cursor; }
            set
            {
                if (value != _Cursor)
                {
                    _Cursor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private float CursorPosX { get; set; }
        private float CursorPosY { get; set; }

        public float EllipseWidth { get; set; }
        public float EllipseHeight { get; set; }

        public PathMakerRenderLayer(Size layerSize) : base(layerSize)
        {
            try
            {
            LineRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
            PathRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
            CursorRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
            PathPointRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
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
            //RenderRectangle rectObject = new RenderRectangle(0, 0, 50, 50, "Red", 0);
            //RenderContainer.RenderObjectList.Add(rectObject);

            double w, h;

            float stroke = 1f;
            bool sw = false;
            int lineInterval = 16;

            //for (int i = 0; i < LayerSize.Height; i += lineInterval)
            //{
            //    RenderLine line;

            //    line = new RenderLine(new WinPoint(0, i), new WinPoint(LayerSize.Width, i), "OpaqueRed", stroke);

            //    LineRenderContainer.RenderObjectList.Add(line);
            //}

            //for (int i = 0; i < LayerSize.Width; i += lineInterval)
            //{
            //    RenderLine line;

            //    line = new RenderLine(new WinPoint(i, 0), new WinPoint(i, LayerSize.Height), "OpaqueRed", stroke);

            //    LineRenderContainer.RenderObjectList.Add(line);
            //}

            // Horizontal Line
            for (int i = 0; i < LayerSize.Height; i += lineInterval)
            {
                RenderLine line;

                if (sw)
                {
                    line = new RenderLine(new WinPoint(0, i), new WinPoint(LayerSize.Width, i), "OpaqueRed", stroke);
                    sw = false;
                }
                else
                {
                    line = new RenderLine(new WinPoint(0, i), new WinPoint(LayerSize.Width, i), "OpaqueYellow", stroke);
                    sw = true;
                }

                LineRenderContainer.RenderObjectList.Add(line);
            }

            // Vertical Line
            for (int i = 0; i < LayerSize.Width; i += lineInterval)
            {
                RenderLine line;

                if (sw)
                {
                    line = new RenderLine(new WinPoint(i, 0), new WinPoint(i, LayerSize.Height), "OpaqueRed", stroke);
                    sw = false;
                }
                else
                {
                    line = new RenderLine(new WinPoint(i, 0), new WinPoint(i, LayerSize.Height), "OpaqueYellow", stroke);
                    sw = true;
                }

                LineRenderContainer.RenderObjectList.Add(line);
            }

            CursorPosX = 0;
            CursorPosY = 0;
            EllipseWidth = 4;
            EllipseHeight = 4;

            Cursor = new RenderEllipse(CursorPosX, CursorPosY, EllipseWidth, EllipseHeight, "OpaqueBlue", 1);

            CursorRenderContainer.RenderObjectList.Add(Cursor);

            float ellipseWidth;
            float ellipseHeight;

            ellipseWidth = 0.5f;
            ellipseHeight = 0.5f;

            MouseDownEventEnable = false;
            MouseWheelEventEnable = false;
            MouseDownHandler = MainScreen_MouseDown;

            //for (int j = 0; j < LayerSize.Height; j++)
            //{
            //    for (int i = 0; i < LayerSize.Width; i++)
            //    {
            //        RenderEllipse ellipse = new RenderEllipse(i, j, ellipseWidth, ellipseHeight, "OpaqueYellow", 0.5f);

            //        RenderContainer.RenderObjectList.Add(ellipse);
            //    }
            //}

            //float dutRenderLeft = LineRenderContainer.RenderObjectList.Min(render => render.CenterX);
            //float dutRenderTop = LineRenderContainer.RenderObjectList.Min(render => render.CenterY);
            //float dutRenderRight = LineRenderContainer.RenderObjectList.Max(render => render.CenterX);
            //float dutRenderBottom = LineRenderContainer.RenderObjectList.Max(render => render.CenterY);

            //WinPoint drawBasePos = new WinPoint();

            //drawBasePos.X = LayerCenterPos.X + (dutRenderLeft + dutRenderRight) / -2;
            //drawBasePos.Y = LayerCenterPos.Y + (dutRenderTop + dutRenderRight) / -2;

            //LineRenderContainer.DrawBasePos = drawBasePos;
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
            LineRenderContainer.Draw(target, resCache);
            PathRenderContainer.Draw(target, resCache);
            CursorRenderContainer.Draw(target, resCache);
            PathPointRenderContainer.Draw(target, resCache);
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
            LineRenderContainer.Draw(target, resCache);
            PathRenderContainer.Draw(target, resCache);
            CursorRenderContainer.Draw(target, resCache);
            PathPointRenderContainer.Draw(target, resCache);
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

        public void CursorMove(CursorEnum cmd)
        {
            try
            {
            switch (cmd)
            {
                case CursorEnum.TOPLEFT:
                    CursorPosX = CursorPosX - 1;
                    CursorPosY = CursorPosY - 1;
                    break;
                case CursorEnum.UP:
                    CursorPosY = CursorPosY - 1;
                    break;
                case CursorEnum.TOPRIGHT:
                    CursorPosX = CursorPosX + 1;
                    CursorPosY = CursorPosY - 1;
                    break;
                case CursorEnum.LEFT:
                    CursorPosX = CursorPosX - 1;
                    break;
                case CursorEnum.RIGHT:
                    CursorPosX = CursorPosX + 1;
                    break;
                case CursorEnum.BOTTOMLEFT:
                    CursorPosX = CursorPosX - 1;
                    CursorPosY = CursorPosY + 1;
                    break;
                case CursorEnum.DOWN:
                    CursorPosY = CursorPosY + 1;
                    break;
                case CursorEnum.BOTTOMRIGHT:
                    CursorPosX = CursorPosX + 1;
                    CursorPosY = CursorPosY + 1;
                    break;
                default:
                    break;
            }

            Cursor.CenterX = CursorPosX;
            Cursor.CenterY = CursorPosY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void MainScreen_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
            CursorPosX = (float)MouseDownPos.X;
            CursorPosY = (float)MouseDownPos.Y;

            Cursor.CenterX = (float)MouseDownPos.X;
            Cursor.CenterY = (float)MouseDownPos.Y;

            Cursor.Color = "Green";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public override List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = new List<RenderContainer>();
            try
            {
                containers.Add(LineRenderContainer);
                containers.Add(PathRenderContainer);
                containers.Add(CursorRenderContainer);
                containers.Add(PathPointRenderContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return containers;
        }
    }
}
