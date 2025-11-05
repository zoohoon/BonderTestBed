using System;

namespace LoaderBase.AttachModules.ModuleInterfaces
{
    [Serializable]
    public class CognexConfig
    {
        public String Name { get; set; }
        public int index { get; set; }
        /*
         * 0 : Normal
         * 1 : Mirrored horizontally
         * 2 : Flipped vertically
         * 3 : Rotated 180 degrees
         */
        public String Direction { get; set; }
        /*
         * 1  : BC, BC 412
         * 2  : BC, IBM 412
         * 3  : Internal Use Only
         * 4  : Chars, SEMI
         * 5  : Chars, IBM
         * 6  : Chars, Triple
         * 7  : OCR-A
         * 11 : SEMI m1.15
         */
        public String Mark { get; set; }
        /*
         * 0 : Virtual
         * 1 : SEMI (Not use)
         * 2 : SEMI with Virtual
         * 3 : BC 412 with Virtual (Not use)
         * 4 : IBM 412 with Virtual
         */
        public String CheckSum { get; set; }
        /*
         * 0 : Not Adjust
         * 1 : Adjust
         * 2 : Adjust & Save
         */
        public String RetryOption { get; set; }
        public String FieldString { get; set; }
        public int OCRCutLineScore { get; set; }
        public double RegionY { get; set; }
        public double RegionX { get; set; }
        public double RegionHeight { get; set; }
        public double RegionWidth { get; set; }
        public double CharY { get; set; }
        public double CharX { get; set; }
        public double CharHeight { get; set; }
        public double CharWidth { get; set; }
        public double UOffset { get; set; }
        public double WOffset { get; set; }
        public double AngleOffset { get; set; }
        public double OCRAngle { get; set; }
        public String Light { get; set; }
        public int LightIntensity { get; set; }
        public CognexConfig()
        {
            Name = String.Empty;
            Direction = "0";
            Mark = "4";
            CheckSum = "2";
            RetryOption = "2";
            FieldString = "************";
            OCRCutLineScore = 200;
            RegionY = 123.0;
            RegionX = 75.0;
            RegionHeight = 165.0;
            RegionWidth = 580.0;
            CharY = 170.5;
            CharX = 527.0;
            CharHeight = 39.0;
            CharWidth = 20.0;
            Light = "6";
            LightIntensity = 4;
            UOffset = 0;
            WOffset = 0;
            AngleOffset = 0;
        }
    }
}
