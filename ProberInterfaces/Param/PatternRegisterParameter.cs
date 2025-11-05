using LogModule;
using System;

namespace ProberInterfaces.Param
{
    public class PatternRegiserParameter
    {
        private ImageBuffer _GrabBuffer;

        public ImageBuffer GrabBuffer
        {
            get { return _GrabBuffer; }
            set { _GrabBuffer = value; }
        }

        private int _ImageControlWidth;

        public int ImageControlWidth
        {
            get { return _ImageControlWidth; }
            set { _ImageControlWidth = value; }
        }

        private int _ImageControlHeight;

        public int ImageControlHeight
        {
            get { return _ImageControlHeight; }
            set { _ImageControlHeight = value; }
        }

        private int _PatternLocationX;

        public int PatternLocationX
        {
            get { return _PatternLocationX; }
            set { _PatternLocationX = value; }
        }

        private int _PatternLocationY;

        public int PatternLocationY
        {
            get { return _PatternLocationY; }
            set { _PatternLocationY = value; }
        }

        private int _PatternWidth;

        public int PatternWdith
        {
            get { return _PatternWidth; }
            set { _PatternWidth = value; }
        }

        private int _PatternHeight;

        public int PatternHeight
        {
            get { return _PatternHeight; }
            set { _PatternHeight = value; }
        }
        private int _RotationAngle;

        public int RotaionAngle
        {
            get { return _RotationAngle; }
            set { _RotationAngle = value; }
        }

        private string _PatternPath;

        public string PatternPath
        {
            get { return _PatternPath; }
            set { _PatternPath = value; }
        }

        public PatternRegiserParameter()
        {

        }
        public PatternRegiserParameter(ImageBuffer ib,
           int patternlocationx, int patternlocationy, int patternwdith, int patternheigh,
           int rotaionangle, string patternpath)
        {
            try
            {
            GrabBuffer = ib;

            PatternLocationX = patternlocationx;
            PatternLocationY = patternlocationy;
            PatternWdith = patternwdith;
            PatternHeight = patternheigh;
            RotaionAngle = rotaionangle;
            PatternPath = patternpath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternRegiserParameter(int imagecontrolwidth, int imagecontrolheight,
            int patternlocationx, int patternlocationy, int patternwdith, int patternheigh,
            int rotaionangle,string patternpath)
        {
            try
            {
            ImageControlWidth = imagecontrolwidth;
            ImageControlHeight = imagecontrolheight;
            PatternLocationX = patternlocationx;
            PatternLocationY = patternlocationy;
            PatternWdith = patternwdith;
            PatternHeight = patternheigh;
            RotaionAngle = rotaionangle;
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
