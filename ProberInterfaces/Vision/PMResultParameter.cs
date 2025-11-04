using LogModule;
using System;

namespace ProberInterfaces.Vision
{
    public class PMResultParameter
    {
        private double _Xposs;

        public double XPoss
        {
            get { return _Xposs; }
            set { _Xposs = value; }
        }

        private double _Yposs;

        public double YPoss
        {
            get { return _Yposs; }
            set { _Yposs = value; }
        }

        private double _Angle;

        public double Angle
        {
            get { return _Angle; }
            set { _Angle = value; }
        }

        private double _Score;

        public double Score
        {
            get { return _Score; }
            set { _Score = value; }
        }

        public PMResultParameter()
        {

        }
        public PMResultParameter(double xposs, double yposs, double angle, double score)
        {
            try
            {
            XPoss = xposs;
            YPoss = yposs;
            Angle = angle;
            Score = score;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
