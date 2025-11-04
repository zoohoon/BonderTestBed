using LogModule;
using System;

namespace ProberInterfaces.Vision
{
    public class GrabDevPosition
    {
        private long mDevIndex;

        public long DevIndex
        {
            get { return mDevIndex; }
            set { mDevIndex = value; }
        }
        private double mScore;

        public double Score
        {
            get { return mScore; }
            set { mScore = value; }
        }
        private double mArea;

        public double Area
        {
            get { return mArea; }
            set { mArea = value; }
        }

        private double mPosX;

        public double PosX
        {
            get { return mPosX; }
            set { mPosX = value; }

        }

        private double mPosY;

        public double PosY
        {
            get { return mPosY; }
            set { mPosY = value; }
        }

        private double mAngle;

        public double Angle
        {
            get { return mAngle; }
            set { mAngle = value; }
        }

        private double mSizeX;

        public double SizeX
        {
            get { return mSizeX; }
            set { mSizeX = value; }

        }

        private double mSizeY;

        public double SizeY
        {
            get { return mSizeY; }
            set { mSizeY = value; }
        }

        public double CenterWeight { get; set; }
        public double AveragePixelValue { get; set; }
        public double PixelSumValue { get; set; }


        public GrabDevPosition(long index, double score, double area,
                                double posx, double posy, double angle,
                                double sizex = 0, double sizey = 0)
        {
            try
            {
            mArea = area;
            mScore = score;
            mDevIndex = index;
            mPosX = posx;
            mPosY = posy;
            mAngle = angle;

            mSizeX = sizex;
            mSizeY = sizey;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public GrabDevPosition()
        {

        }
    }
}
