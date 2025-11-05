using System;

namespace LoaderBase
{
    /// <summary>
    /// 
    /// </summary>
    public struct Vector2
    {
        /// <summary>
        /// 
        /// </summary>
        public static double Tolerance = 1e-9;

        /// <summary>
        /// 
        /// </summary>
        public double x;

        /// <summary>
        /// 
        /// </summary>
        public double y;

        /// <summary>
        /// 
        /// </summary>
        public double X { get { return x; } set { x = value; } }

        /// <summary>
        /// 
        /// </summary>
        public double Y { get { return y; } set { y = value; } }

        /// <summary>
        /// 
        /// </summary>
        public static readonly Vector2 Zero = new Vector2(0, 0);

        /// <summary>
        /// 
        /// </summary>
        public double Length => Math.Sqrt((x * x) + (y * y));

        /// <summary>
        /// 
        /// </summary>
        public Vector2 NormalizedCopy
        {
            get
            {
                var vec = this;
                vec.Normalize();
                return vec;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public Vector2(double val)
        {
            x = val; y = val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2(double x, double y)
        {
            this.x = x; this.y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("X:{0}, Y:{1}", x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool PositionEquals(Vector2 other)
        {
            return
                Math.Abs(x - other.x) < Tolerance &&
                Math.Abs(y - other.y) < Tolerance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Vector2 == false)
                return false;

            return PositionEquals((Vector2)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Vector2 operator +(Vector2 l, Vector2 r)
        {
            return new Vector2(l.X + r.X, l.Y + r.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Vector2 operator -(Vector2 l, Vector2 r)
        {
            return new Vector2(l.X - r.X, l.Y - r.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Vector2 operator *(Vector2 l, double val)
        {
            return new Vector2(l.X * val, l.Y * val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Vector2 operator /(Vector2 l, double val)
        {
            return new Vector2(l.X / val, l.Y / val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double Dot(Vector2 other)
        {
            return (x * other.x) + (y * other.y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Normalize()
        {
            double length = this.Length;
            if (Math.Abs(length) > Tolerance)
            {
                double inv = 1.0f / length;
                X *= inv;
                Y *= inv;
            }
            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double Distance(Vector2 other)
        {
            double dx = x - other.x;
            double dy = y - other.y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Radian BetweenAngle(Vector2 other)
        {
            // v1*v2 = x1x2 + y1y2
            // v1*v2 = [v1][v2]con(T)
            double cosAngle = Dot(other);
            return new Radian(Math.Acos(cosAngle));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Vector2 Rotated(Radian angle)
        {
            Vector2 rot = new Vector2();
            rot.x = x * Math.Cos(angle.ValueRadians) - y * Math.Sin(angle.ValueRadians);
            rot.y = y * Math.Cos(angle.ValueRadians) + x * Math.Sin(angle.ValueRadians);
            return rot;
        }
    }
}
