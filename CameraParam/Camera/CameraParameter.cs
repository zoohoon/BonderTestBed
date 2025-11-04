using System;
using System.Collections.Generic;


namespace VisionParams.Camera
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;


    [Serializable]
    public class CameraParameter : ICameraParameter, IParamNode      // Vision system parameter definitions
    {
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        private Element<int> _DigiNumber
            = new Element<int>();
        // [XmlAttribute("DigiNumber")]
        public Element<int> DigiNumber
        {
            get { return _DigiNumber; }
            set { _DigiNumber = value; }
        }

        private Element<int> _ChannelNumber
    = new Element<int>();
        //  [XmlAttribute("ChannelNumber")]
        public Element<int> ChannelNumber
        {
            get { return _ChannelNumber; }
            set { _ChannelNumber = value; }
        }


        private Element<string> _ChannelDesc
            = new Element<string>();
        //  [XmlAttribute("ChannelDesc")]
        public Element<string> ChannelDesc
        {
            get { return _ChannelDesc; }
            set { _ChannelDesc = value; }
        }

        private Element<EnumProberCam> _ChannelType
             = new Element<EnumProberCam>();
        // [XmlAttribute("ChannelType")]

        public Element<EnumProberCam> ChannelType
        {
            get { return _ChannelType; }
            set { _ChannelType = value; }
        }


        //[XmlAttribute("ChannelType")]
        //public EnumProberCam ChannelType { get; set; }

        private Element<FlipEnum> _VerticalFlip
             = new Element<FlipEnum>();
        // [XmlAttribute("VerticalFlip")]
        public Element<FlipEnum> VerticalFlip
        {
            get { return _VerticalFlip; }
            set { _VerticalFlip = value; }
        }

        private Element<FlipEnum> _HorizontalFlip
             = new Element<FlipEnum>();
        // [XmlAttribute("HorizontalFlip")]
        public Element<FlipEnum> HorizontalFlip
        {
            get { return _HorizontalFlip; }
            set { _HorizontalFlip = value; }
        }

        private Element<double> mRatioX = new Element<double>();
        public Element<double> RatioX
        {
            get { return mRatioX; }
            set { mRatioX = value; }
        }

        private Element<double> mRatioY = new Element<double>();

        public Element<double> RatioY
        {
            get { return mRatioY; }
            set { mRatioY = value; }
        }

        //private Element<double> mCamAngle = new Element<double>("CamAngle");

        //public Element<double> CamAngle
        //{
        //    get { return mCamAngle; }
        //    set { mCamAngle = value; }
        //}

        private Element<int> mGrabSizeX = new Element<int>();

        public Element<int> GrabSizeX
        {
            get { return mGrabSizeX; }
            set { mGrabSizeX = value; }
        }

        private Element<int> mGrabSizeY = new Element<int>();

        public Element<int> GrabSizeY
        {
            get { return mGrabSizeY; }
            set { mGrabSizeY = value; }
        }
        private Element<int> _mBand = new Element<int>();

        public Element<int> Band
        {
            get { return _mBand; }
            set { _mBand = value; }
        }

        private Element<int> _mColorDept = new Element<int>();

        public Element<int> ColorDept
        {
            get { return _mColorDept; }
            set { _mColorDept = value; }
        }

        private Element<double> _Rotate = new Element<double>();

        public Element<double> Rotate
        {
            get { return _Rotate; }
            set { _Rotate = value; }
        }

        //private Element<bool> _DoubleGrabEnable = new Element<bool>() { Value = false };

        //public Element<bool> DoubleGrabEnable
        //{
        //    get { return _DoubleGrabEnable; }
        //    set { _DoubleGrabEnable = value; }
        //}
        //private Element<int> mPixelDirectionX = new Element<int>("PixelDirectionX");

        //public Element<int> PixelDirectionX
        //{
        //    get { return mPixelDirectionX; }
        //    set { mPixelDirectionX = value; }
        //}

        //private Element<int> mPixelDirectionY = new Element<int>("PixelDirectionY");

        //public Element<int> PixelDirectionY
        //{
        //    get { return mPixelDirectionY; }
        //    set { mPixelDirectionY = value; }
        //}

        private Element<ObservableCollection<LightChannelType>> _LightsChannels
             = new Element<ObservableCollection<LightChannelType>>();

        public Element<ObservableCollection<LightChannelType>> LightsChannels
        {
            get { return _LightsChannels; }
            set { _LightsChannels = value; }
        }

        public CameraParameter() { }

        public CameraParameter(EnumProberCam chntype,
            EnumGrabberRaft grabberType = EnumGrabberRaft.UNDIFIND,
            int diginum = 0, int channelnum = 0)
        {
            try
            {
                DigiNumber.Value = diginum;
                ChannelNumber.Value = channelnum;
                ChannelDesc.Value = "Unknown channel";
                ChannelType.Value = chntype;

                VerticalFlip.Value = FlipEnum.NONE;
                HorizontalFlip.Value = FlipEnum.NONE;



                if (grabberType == EnumGrabberRaft.MILMORPHIS || grabberType == EnumGrabberRaft.EMULGRABBER)
                {
                    GrabSizeX.Value = 480;
                    GrabSizeY.Value = 480;
                    Band.Value = 1;
                    ColorDept.Value = 8;

                    switch (chntype)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 2));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 1));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.WAFER_HIGH_CAM, 0);
                            VerticalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 0.78;
                            RatioY.Value = 0.78;
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 0));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 3));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.WAFER_LOW_CAM, 1);
                            VerticalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 7.85;
                            RatioY.Value = 7.85;
                            break;
                        case EnumProberCam.PIN_HIGH_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 5));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 4));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PIN_HIGH_CAM, 2);
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 0.585;
                            RatioY.Value = 0.585;
                            break;
                        case EnumProberCam.PIN_LOW_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 7));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 6));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PIN_LOW_CAM, 3);
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 4.1;
                            RatioY.Value = 4.1;
                            break;
                        case EnumProberCam.PACL6_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 8));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PACL6_CAM, 4);
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            break;
                        case EnumProberCam.PACL8_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 9));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PACL8_CAM, 5);
                            HorizontalFlip.Value = FlipEnum.NONE;
                            break;
                        case EnumProberCam.PACL12_CAM:
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PACL12_CAM, 6);
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            break;
                        case EnumProberCam.ARM_6_CAM:
                            //_CameraChannel = new CameraChannelType(EnumProberCam.ARM_6_CAM, 7);
                            break;
                        case EnumProberCam.ARM_8_12_CAM:
                            //_CameraChannel = new CameraChannelType(EnumProberCam.ARM_8_12_CAM, 8);
                            break;
                        case EnumProberCam.OCR1_CAM:
                            //_CameraChannel = new CameraChannelType(EnumProberCam.OCR1_CAM, 9);
                            break;
                        case EnumProberCam.OCR2_CAM:
                            //_CameraChannel = new CameraChannelType(EnumProberCam.OCR2_CAM, 10);
                            break;
                        case EnumProberCam.INVALID:
                        case EnumProberCam.UNDEFINED:
                        default:
                            break;
                    }

                }
                else if (grabberType == EnumGrabberRaft.MILGIGE || grabberType == EnumGrabberRaft.GIGE_EMULGRABBER)
                {
                    GrabSizeX.Value = 960;
                    GrabSizeY.Value = 960; //960
                    Band.Value = 1;
                    ColorDept.Value = 8;
                    switch (chntype)
                    {
                        case EnumProberCam.WAFER_HIGH_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 0));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 1));
                            VerticalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 0.539;
                            RatioY.Value = 0.541;
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 2));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 3));
                            VerticalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 5.47;
                            RatioY.Value = 5.47;
                            break;
                        case EnumProberCam.PIN_HIGH_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 6));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 5));
                            HorizontalFlip.Value = FlipEnum.NONE;
                            RatioX.Value = 0.415;
                            RatioY.Value = 0.415;
                            break;
                        case EnumProberCam.PIN_LOW_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 7));
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 4));

                            HorizontalFlip.Value = FlipEnum.FLIP;
                            RatioX.Value = 2.7;
                            RatioY.Value = 2.7;
                            break;
                        case EnumProberCam.PACL6_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 8));
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            break;
                        case EnumProberCam.PACL8_CAM:
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 9));
                            HorizontalFlip.Value = FlipEnum.NONE;
                            break;
                        case EnumProberCam.PACL12_CAM:
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            break;
                        case EnumProberCam.ARM_6_CAM:
                            break;
                        case EnumProberCam.ARM_8_12_CAM:
                            break;
                        case EnumProberCam.OCR1_CAM:
                            break;
                        case EnumProberCam.OCR2_CAM:
                            break;
                        case EnumProberCam.INVALID:
                        case EnumProberCam.UNDEFINED:
                        default:
                            break;
                    }
                }
                else if (grabberType == EnumGrabberRaft.TIS)
                {

                    Band.Value = 3;
                    ColorDept.Value = 8;
                    RatioX.Value = 41.66;
                    RatioY.Value = 41.66;
                    switch (chntype)
                    {
                        case EnumProberCam.INVALID:
                            break;
                        case EnumProberCam.UNDEFINED:
                            break;
                        case EnumProberCam.WAFER_HIGH_CAM:
                            break;
                        case EnumProberCam.WAFER_LOW_CAM:
                            break;
                        case EnumProberCam.PIN_HIGH_CAM:
                            break;
                        case EnumProberCam.PIN_LOW_CAM:
                            break;
                        case EnumProberCam.PACL6_CAM:
                            GrabSizeX.Value = 744;
                            GrabSizeY.Value = 480; //960
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 8));
                            HorizontalFlip.Value = FlipEnum.FLIP;
                            break;
                        case EnumProberCam.PACL8_CAM:
                            GrabSizeX.Value = 744;
                            GrabSizeY.Value = 480; //960
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 9));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PACL8_CAM, 5);
                            HorizontalFlip.Value = FlipEnum.NONE;
                            break;
                        case EnumProberCam.PACL12_CAM:
                            GrabSizeX.Value = 744;
                            GrabSizeY.Value = 480; //960
                            LightsChannels.Value = new ObservableCollection<LightChannelType>();
                            LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 9));
                            //_CameraChannel = new CameraChannelType(EnumProberCam.PACL8_CAM, 5);
                            HorizontalFlip.Value = FlipEnum.NONE;
                            break;
                        case EnumProberCam.ARM_6_CAM:
                            break;
                        case EnumProberCam.ARM_8_12_CAM:
                            break;
                        case EnumProberCam.OCR1_CAM:
                            break;
                        case EnumProberCam.OCR2_CAM:
                            break;
                        case EnumProberCam.MAP_1_CAM:
                            break;
                        case EnumProberCam.MAP_2_CAM:
                            break;
                        case EnumProberCam.MAP_3_CAM:
                            break;
                        case EnumProberCam.MAP_4_CAM:
                            break;
                        case EnumProberCam.MAP_5_CAM:
                            break;
                        case EnumProberCam.MAP_6_CAM:
                            break;
                        case EnumProberCam.MAP_7_CAM:
                            break;
                        case EnumProberCam.MAP_8_CAM:
                            break;
                        case EnumProberCam.MAP_REF_CAM:
                            break;
                        case EnumProberCam.CAM_LAST:
                            break;
                        default:
                            break;
                    }


                }

                if (LightsChannels.Value == null)
                {
                    LightsChannels.Value = new ObservableCollection<LightChannelType>();
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.COAXIAL, 0));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.OBLIQUE, 1));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.AUX, 2));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.EXTERNAL1, 3));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.EXTERNAL2, 4));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.EXTERNAL3, 5));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.EXTERNAL4, 6));
                    LightsChannels.Value.Add(new LightChannelType(EnumLightType.EXTERNAL5, 7));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
    }
}
