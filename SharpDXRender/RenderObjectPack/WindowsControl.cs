using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace SharpDXRender
{
    using System.Runtime.Serialization;
    [DataContract]
    public class WindowsPoint : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        private double _X;
        [DataMember]
        public double X
        {
            get { return _X; }
            set
            {
                if (value != _X)
                {
                    _X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Y;
        [DataMember]
        public double Y
        {
            get { return _Y; }
            set
            {
                if (value != _Y)
                {
                    _Y = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region //..Creator
        public WindowsPoint()
        {

        }

        public WindowsPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
        #endregion

    }

    [DataContract]
    public class WindowSize : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property
        private double _Width;
        [DataMember]
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Height;
        [DataMember]
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Creator
        public WindowSize()
        {

        }

        public WindowSize(double width, double height)
        {
            Width = width;
            Height = height;
        }
        #endregion
    }

    [DataContract]
    public class WindowRect : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        private double _X;
        [DataMember]
        public double X
        {
            get { return _X; }
            set
            {
                if (value != _X)
                {
                    _X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Y;
        [DataMember]
        public double Y
        {
            get { return _Y; }
            set
            {
                if (value != _Y)
                {
                    _Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Width;
        [DataMember]
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Height;
        [DataMember]
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region //..Creator
        public WindowRect(double x, double y , double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height; 
        }
        #endregion

    }
}
