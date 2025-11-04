using System;


namespace LoaderBase
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct Radian : IEquatable<Radian>, IComparable<Radian>
    {
        private const double RadToDeg = (180.0 / Math.PI);

        /// <summary>
        /// 
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double ValueRadians => Value;
        
        /// <summary>
        /// 
        /// </summary>
        public double ValueDegrees => Value * RadToDeg;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public Radian(Degree d)
        {
            Value = d.ValueRadians;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        public Radian(double r)
        {
            Value = r;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator Radian(Degree d)
        {
            Radian result = new Radian(d.ValueRadians);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator Radian(double f)
        {
            Radian result = new Radian(f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Radian operator +(Radian l, Degree d)
        {
            Radian result = new Radian((double)((double)l.Value + (double)d.ValueRadians));
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Radian operator +(Radian l, Radian r)
        {
            Radian result = new Radian(l.Value + r.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Radian operator -(Radian l, Degree d)
        {
            Radian result = new Radian(l.Value - d.ValueRadians);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Radian operator -(Radian l, Radian r)
        {
            Radian result = new Radian(l.Value - r.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Radian operator -(Radian r)
        {
            Radian result = new Radian(-r.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Radian operator *(Radian l, Radian f)
        {
            Radian result = new Radian(l.Value * f.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Radian operator *(double f, Radian r)
        {
            Radian result = new Radian(r.Value * f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Radian operator *(Radian l, double f)
        {
            Radian result = new Radian(l.Value * f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Radian operator /(double f, Radian r)
        {
            Radian result = new Radian(f / r.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Radian operator /(Radian l, double f)
        {
            Radian result = new Radian(l.Value / f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator <(Radian l, Radian r)
        {
            return l.Value < r.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator <=(Radian l, Radian r)
        {
            return l.Value <= r.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator ==(Radian l, Radian r)
        {
            return l.Value == r.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator !=(Radian l, Radian r)
        {
            return l.Value != r.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator >=(Radian l, Radian r)
        {
            return l.Value >= r.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator >(Radian l, Radian r)
        {
            return l.Value > r.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Radian other)
        {
            if (Value < other.Value) return -1;
            if (Value > other.Value) return 1;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ref Radian other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Radian other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Radian))
                return false;

            return Equals((Radian)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
