
using System;
using System.Diagnostics;

namespace LoaderBase
{
    /// <summary>
    /// Degree
    /// </summary>
    [Serializable]
    public struct Degree : IEquatable<Degree>, IComparable<Degree>
    {
        /// <summary>
        /// DegToRad
        /// </summary>
        public const double DegToRad = (Math.PI / 180.0);

        /// <summary>
        /// Value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// ValueRadians
        /// </summary>
        public double ValueRadians => Value * DegToRad;

        /// <summary>
        /// ValueDegrees
        /// </summary>
        public double ValueDegrees => Value;

        /// <summary>
        /// Degree
        /// </summary>
        /// <param name="r"></param>
        public Degree(Radian r)
        {
            Value = r.ValueDegrees;
        }

        /// <summary>
        /// Degree
        /// </summary>
        /// <param name="d"></param>
        public Degree(double d)
        {
            Value = d;
        }

        /// <summary>
        /// ZERO
        /// </summary>
        public static readonly Degree ZERO = new Degree(0);

        /// <summary>
        /// Degree
        /// </summary>
        /// <param name="r"></param>
        public static implicit operator Degree(Radian r)
        {
            Degree result = new Degree(r.ValueDegrees);
            return result;
        }

        /// <summary>
        /// Degree
        /// </summary>
        /// <param name="f"></param>
        public static implicit operator Degree(double f)
        {
            Degree result = new Degree(f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Degree operator +(Degree l, Radian r)
        {
            Degree result = new Degree(l.Value + r.ValueDegrees);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Degree operator +(Degree l, Degree d)
        {
            Degree result = new Degree(l.Value + d.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Degree operator -(Degree l, Radian r)
        {
            Degree result = new Degree(l.Value - r.ValueDegrees);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Degree operator -(Degree l, Degree d)
        {
            Degree result = new Degree(l.Value - d.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Degree operator -(Degree d)
        {
            Degree result = new Degree(-d.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Degree operator *(Degree l, Degree f)
        {
            Degree result = new Degree(l.Value * f.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Degree operator *(double f, Degree r)
        {
            Degree result = new Degree(r.Value * f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Degree operator *(Degree l, double f)
        {
            Degree result = new Degree(l.Value * f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Degree operator /(double f, Degree r)
        {
            Degree result = new Degree(f / r.Value);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Degree operator /(Degree l, double f)
        {
            Degree result = new Degree(l.Value / f);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool operator <(Degree l, Degree d)
        {
            return l.Value < d.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool operator <=(Degree l, Degree d)
        {
            return l.Value <= d.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool operator ==(Degree l, Degree d)
        {
            return l.Value == d.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool operator !=(Degree l, Degree d)
        {
            return l.Value != d.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool operator >=(Degree l, Degree d)
        {
            return l.Value >= d.Value;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool operator >(Degree l, Degree d)
        {
            return l.Value > d.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Degree other)
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
        public bool Equals(ref Degree other)
        {
            return Value == other.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Degree other)
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
            if (!(obj is Degree))
                return false;


            return Equals((Degree)obj);
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
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <returns></returns>
        public Degree Normalized(double lowerLimit = -180.0, double upperLimit = 180.0)
        {
            //return new Degree(this.Value);

            double round = upperLimit - lowerLimit;
            Debug.Assert(round == 360.0, 
                "The range of angles is 360.", 
                "lowerLimit = {0}, upperLimit = {1}, round = {2}", 
                lowerLimit, upperLimit, round);

            double degree = this.Value % round;

            if (degree < lowerLimit)
                return new Degree(degree + round);

            if (degree < upperLimit)
                return new Degree(degree);

            return new Degree(degree - round);
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
