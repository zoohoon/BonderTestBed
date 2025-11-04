using LogModule;
using System;

namespace ProberInterfaces
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using ProberInterfaces.Vision;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [Serializable()]
    [DataContract]
    public class ImageBuffer : INotifyPropertyChanged
    {
        public delegate void ImageReadyDelegate(ImageBuffer image);
        public delegate void DrawDisplay(ImageBuffer image, ICamera cam = null);
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private byte[] _Buffer;
        [DataMember]
        public byte[] Buffer
        {
            get { return _Buffer; }
            set { _Buffer = value; }
        }

        private int _SizeX;
        [DataMember]
        public int SizeX
        {
            get { return _SizeX; }
            set { _SizeX = value; }
        }

        private int _SizeY;
        [DataMember]
        public int SizeY
        {
            get { return _SizeY; }
            set { _SizeY = value; }
        }

        private int _Band;
        [DataMember]
        public int Band
        {
            get { return _Band; }
            set { _Band = value; }
        }

        private int _ColorDept;
        [DataMember]
        public int ColorDept
        {
            get { return _ColorDept; }
            set { _ColorDept = value; }
        }

        private int _GrayLevelValue;
        [DataMember]
        public int GrayLevelValue
        {
            get { return _GrayLevelValue; }
            set
            {
                if (value != _GrayLevelValue)
                {
                    _GrayLevelValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _FocusLevelValue;
        public int FocusLevelValue
        {
            get { return _FocusLevelValue; }
            set
            {
                if (_FocusLevelValue != value)
                {
                    _FocusLevelValue = value;
                    RaisePropertyChanged();

                }
            }
        }

        private int _FiliterdFocusValue;
        public int FiliterdFocusValue
        {
            get { return _FiliterdFocusValue; }
            set
            {
                if (value != _FiliterdFocusValue)
                {
                    _FiliterdFocusValue = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _ArithmeticFocusValue;
        public int ArithmeticFocusValue
        {
            get { return _ArithmeticFocusValue; }
            set
            {
                if (value != _ArithmeticFocusValue)
                {
                    _ArithmeticFocusValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _CapturedTime;
        public DateTime CapturedTime
        {
            get { return _CapturedTime; }
            set
            {
                if (value != _CapturedTime)
                {
                    _CapturedTime = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _ZHeight;
        [DataMember]
        public double ZHeight
        {
            get { return _ZHeight; }
            set
            {
                if (_ZHeight != value)
                {
                    _ZHeight = value;
                    RaisePropertyChanged();

                }
            }
        }

        private EnumProberCam _CamType;
        [DataMember]
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private MachineCoordinate _MachineCoordinates;
        [DataMember]
        public MachineCoordinate MachineCoordinates
        {
            get { return _MachineCoordinates; }
            set
            {
                if (value != _MachineCoordinates)
                {
                    _MachineCoordinates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CatCoordinates;
        [DataMember]
        public CatCoordinates CatCoordinates
        {
            get { return _CatCoordinates; }
            set
            {
                if (value != _CatCoordinates)
                {
                    _CatCoordinates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _MachineIdx;
        [DataMember]
        public MachineIndex MachineIdx
        {
            get { return _MachineIdx; }
            set
            {
                if (value != _MachineIdx)
                {
                    _MachineIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _UserIdx;
        [DataMember]
        public UserIndex UserIdx
        {
            get { return _UserIdx; }
            set
            {
                if (value != _UserIdx)
                {
                    _UserIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> mRatioX = new Element<double>();
        [DataMember]
        public Element<double> RatioX
        {
            get { return mRatioX; }
            set { mRatioX = value; }
        }

        private Element<double> mRatioY = new Element<double>();
        [DataMember]
        public Element<double> RatioY
        {
            get { return mRatioY; }
            set { mRatioY = value; }
        }
        private EventCodeEnum _ErrorCode;

        public EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set { _ErrorCode = value; }
        }

        private bool _UpdateOverlayFlag;
        [DataMember]
        public bool UpdateOverlayFlag
        {
            get { return _UpdateOverlayFlag; }
            set
            {
                if (_UpdateOverlayFlag != value)
                {
                    _UpdateOverlayFlag = value;
                }

            }
        }


        #region //..Overlay
        private ObservableCollection<IDrawable> _DrawOverlayContexts
             = new ObservableCollection<IDrawable>();
        [DataMember]
        public ObservableCollection<IDrawable> DrawOverlayContexts
        {
            get { return _DrawOverlayContexts; }
            set { _DrawOverlayContexts = value; }
        }

        #endregion

        public ImageBuffer()
        {

        }
        public ImageBuffer(EnumProberCam camtype)
        {
            _CamType = camtype;
        }
        public ImageBuffer(byte[] buffer, int sizex, int sizey, int colordept)
        {
            try
            {
                _Buffer = buffer;
                _SizeX = sizex;
                _SizeY = sizey;
                _Band = 1;
                _ColorDept = colordept;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ImageBuffer(byte[] buffer, int sizex, int sizey, int datatype, int colordept)
        {
            try
            {
                _Buffer = buffer;
                _SizeX = sizex;
                _SizeY = sizey;
                _Band = datatype;
                _ColorDept = colordept;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public ImageBuffer(int sizex, int sizey, int band, int colordept)
        {
            try
            {
                _Buffer = new byte[sizex * sizey * band];

                _SizeX = sizex;
                _SizeY = sizey;
                _Band = band;
                _ColorDept = colordept;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public ImageBuffer(byte[] buffer, int sizex, int sizey, int band, int colordept, EnumProberCam camtype)
        {
            try
            {
                _Buffer = buffer;
                _SizeX = sizex;
                _SizeY = sizey;
                _Band = band;
                _ColorDept = colordept;
                _CamType = camtype;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ImageBuffer(ImageBuffer image)
        {
            try
            {
                this.Buffer = image.Buffer;
                this.SizeX = image.SizeX;
                this.SizeY = image.SizeY;
                this.Band = image.Band;
                this.ColorDept = image.ColorDept;
                this.ColorDept = image.ColorDept;
                this.RatioX = image.RatioX;
                this.RatioX = image.RatioX;
                this.MachineCoordinates = image.MachineCoordinates;
                this.CatCoordinates = image.CatCoordinates;
                this.ZHeight = image.ZHeight;
                this.RatioX = image.RatioX;
                this.RatioY = image.RatioY;
                this.CamType = image.CamType;
                this.CapturedTime = image.CapturedTime;

                image.ErrorCode = this.ErrorCode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ImageCopyTo(ImageBuffer image)
        {
            try
            {
                image.Buffer = (byte[])this.Buffer.Clone();
                image.SizeX = this.SizeX;
                image.SizeY = this.SizeY;
                image.Band = this.Band;
                image.ColorDept = this.ColorDept;
                image.ColorDept = this.ColorDept;
                image.RatioX = this.RatioX;
                image.RatioY = this.RatioY;
                image.MachineCoordinates = this.MachineCoordinates;
                image.CatCoordinates = this.CatCoordinates;
                image.ZHeight = this.ZHeight;
                image.RatioX = this.RatioX;
                image.RatioY = this.RatioY;
                image.CamType = this.CamType;
                image.CapturedTime = this.CapturedTime;

                image.ErrorCode = this.ErrorCode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearBuffer()
        {
            this.Buffer = new byte[Buffer.Length];
        }
        public void CopyTo(ImageBuffer image)
        {
            try
            {
                if (image != null)
                {
                    if (this.Buffer != null)
                    {
                        if (image.Buffer == null || (Buffer.Length != image.Buffer.Length))
                        {
                            image.Buffer = new byte[Buffer.Length];
                        }

                        Array.Copy(this.Buffer, image.Buffer, Buffer.Length);
                    }

                    image.SizeX = this.SizeX;
                    image.SizeY = this.SizeY;
                    image.Band = this.Band;
                    image.ColorDept = this.ColorDept;
                    image.RatioX = this.RatioX;
                    image.RatioY = this.RatioY;
                    image.MachineCoordinates = this.MachineCoordinates;
                    image.CatCoordinates = this.CatCoordinates;
                    image.ZHeight = this.ZHeight;
                    image.RatioX = this.RatioX;
                    image.RatioY = this.RatioY;
                    image.CamType = this.CamType;
                    image.FocusLevelValue = this.FocusLevelValue;
                    image.CapturedTime = this.CapturedTime;

                    image.ErrorCode = this.ErrorCode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
