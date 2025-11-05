using LogModule;
using System;

namespace SharpDXRender.RenderObjectPack
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using System.Runtime.Serialization;
    using WinPoint = System.Windows.Point;

    [DataContract]
    public abstract class RenderObject
    {
        public const String DefaultColor = "Transparent";
        [DataMember]
        public float CenterX { get; set; }
        [DataMember]
        public float CenterY { get; set; }
        [DataMember]
        public float Width { get; set; }
        [DataMember]
        public float Height { get; set; }

        //==> Rendering 하는 물체의 외각 꼭지점 좌표, Window에 Render 여부 판단시 사용
        private WinPoint _LT;

        public WinPoint LT
        {
            get { return _LT; }
            set { _LT = value; WLT = new WindowsPoint(_LT.X,_LT.Y); }
        }

        private WinPoint _RT;

        public WinPoint RT
        {
            get { return _RT; }
            set { _RT = value; WRT = new WindowsPoint(_RT.X, _RT.Y); }
        }

        private WinPoint _LB;

        public WinPoint LB
        {
            get { return _LB; }
            set { _LB = value; WLB = new WindowsPoint(_LB.X, _LB.Y); }
        }

        private WinPoint _RB;

        public WinPoint RB
        {
            get { return _RB; }
            set { _RB = value; WRB = new WindowsPoint(_RB.X, _RB.Y); }
        }

        //==> Render Object를 그리는 좌표의 기준, Window 좌표
        private WinPoint _DrawBasePos;

        public WinPoint DrawBasePos
        {
            get { return _DrawBasePos; }
            set { _DrawBasePos = value; WDrawBasePos = new WindowsPoint(_DrawBasePos.X, _DrawBasePos.Y); }
        }

        [DataMember]
        public String Color { get; set; }
        [DataMember]
        public String StrokeColor { get; set; }
        [DataMember]
        public bool IsVisible { get; set; }
        [DataMember]
        public float StrokeWidth { get; set; }

        //==> WCF
        [DataMember]
        public WindowsPoint WLT { get; set; }
        [DataMember]
        public WindowsPoint WRT { get; set; }
        [DataMember]
        public WindowsPoint WLB { get; set; }
        [DataMember]
        public WindowsPoint WRB { get; set; }
        [DataMember]
        public WindowsPoint WDrawBasePos { get; set; }
       

        public RenderObject()
        {

        }
        public RenderObject(float centerX, float centerY, float width, float height, String color, float strokeWidth = 1f, String strokeColor = "Black")
        {
            try
            {
            CenterX = centerX;
            CenterY = centerY;
            Width = width;
            Height = height;
            Color = color;
            IsVisible = true;
            StrokeWidth = strokeWidth;
            StrokeColor = strokeColor;

            float halfWidth = Width / 2;
            float halfHeight = Height / 2;
            LT = new WinPoint(CenterX - halfWidth, CenterY - halfHeight);
            RT = new WinPoint(CenterX + halfWidth, CenterY - halfHeight);
            LB = new WinPoint(CenterX - halfWidth, CenterY + halfHeight);
            RB = new WinPoint(CenterX + halfWidth, CenterY + halfHeight);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        //==> Render의 인자들은 실시간으로 바뀌는 값들이라서 매번 파라미터로 받아와야 한다.
        public abstract void Draw(RenderTarget target, ResourceCache resCache);
        public abstract void Draw(RenderTarget target, ResourceCache resCache, float strokeWidth);
        public abstract void Draw(RenderTarget target, ResourceCache resCache, String color, float strokeWidth);
        public void UpdateVertex()
        {
            try
            {
            float halfWidth = Width / 2;
            float halfHeight = Height / 2;

            LT = new WinPoint(DrawBasePos.X + CenterX - halfWidth, DrawBasePos.Y + CenterY - halfHeight);
            RT = new WinPoint(DrawBasePos.X + CenterX + halfWidth, DrawBasePos.Y + CenterY - halfHeight);
            LB = new WinPoint(DrawBasePos.X + CenterX - halfWidth, DrawBasePos.Y + CenterY + halfHeight);
            RB = new WinPoint(DrawBasePos.X + CenterX + halfWidth, DrawBasePos.Y + CenterY + halfHeight);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public WinPoint GetScopePostion(RenderTarget target)
        {
            float renderPosX = (float)(DrawBasePos.X + CenterX);
            float renderPosY = (float)(DrawBasePos.Y + CenterY);

            return MatrixConverter.ApplyMatrixPoint(target.Transform, renderPosX, renderPosY);
        }
    }
}
