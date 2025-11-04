using LogModule;
using System;

namespace ProberInterfaces.Param
{
    public class RegisteImageBufferParam
    {
        private ImageBuffer _ImageBuffer;

        public ImageBuffer ImageBuffer
        {
            get { return _ImageBuffer; }
            set { _ImageBuffer = value; }
        }


        private EnumProberCam _CamType;

        public EnumProberCam CamType
        {
            get { return _CamType; }
            set { _CamType = value; }
        }


        private int _LocationX;

        public int LocationX
        {
            get { return _LocationX; }
            set { _LocationX = value; }
        }

        private int _LocationY;

        public int LocationY
        {
            get { return _LocationY; }
            set { _LocationY = value; }
        }

        private int _Width;

        public int Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private int _Height;

        public int Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        private double _Rotangle;

        public double Rotangle
        {
            get { return _Rotangle; }
            set { _Rotangle = value; }
        }


        private string _PatternPath;
        public string PatternPath
        {
            get { return _PatternPath; }
            set { _PatternPath = value; }
        }

        private bool _IsregistMask;

        public bool IsregistMask
        {
            get { return _IsregistMask; }
            set { _IsregistMask = value; }
        }


        public RegisteImageBufferParam()
        {

        }
        public RegisteImageBufferParam(int locationx, int locationy, int width, int height)
        {
            try
            {
                this.LocationX = locationx;
                this.LocationY = locationy;
                this.Width = width;
                this.Height = height;
                this.Rotangle = 0.0;
                PatternPath = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public RegisteImageBufferParam(int locationx, int locationy, int width, int height, string patternpath)
        {
            try
            {
                this.LocationX = locationx;
                this.LocationY = locationy;
                this.Width = width;
                this.Height = height;
                this.Rotangle = 0.0;
                PatternPath = patternpath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public RegisteImageBufferParam(EnumProberCam camtype, int locationx, int locationy, int width, int height, string patternpath)
        {
            try
            {
                CamType = camtype;
                this.LocationX = locationx;
                this.LocationY = locationy;
                this.Width = width;
                this.Height = height;
                this.Rotangle = 0.0;
                PatternPath = patternpath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public RegisteImageBufferParam(EnumProberCam camtype, int locationx, int locationy,
            int width, int height, string patternpath, bool isregistmask)
        {
            try
            {
                CamType = camtype;
                this.LocationX = locationx;
                this.LocationY = locationy;
                this.Width = width;
                this.Height = height;
                this.Rotangle = 0.0;
                PatternPath = patternpath;
                IsregistMask = isregistmask;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public RegisteImageBufferParam(int locationx, int locationy, int width, int height, double rotangle, string patternpath)
        {
            try
            {
                this.LocationX = locationx;
                this.LocationY = locationy;
                this.Width = width;
                this.Height = height;
                this.Rotangle = rotangle;
                PatternPath = patternpath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
