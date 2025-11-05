using ProberErrorCode;
using ProberInterfaces.LightJog;
using ProberInterfaces.Loader.RemoteDataDescription;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ProberInterfaces.ControlClass.ViewModel
{
    public interface IChuckPlanarityVM : IUseLightJog, IMainScreenViewModel
    {
        IDisplayPort DisplayPort { get; set; }

        //ICamera CurCam { get; set; }

        double MinHeight { get; set; }
        double MaxHeight { get; set; }
        double DiffHeight { get; set; }
        //double SpecHeight { get; set; }
        double ChuckEndPointMargin { get; set; }

        //IAsyncCommand<EnumChuckPosition> ChuckMoveCommand { get; }

        //IAsyncCommand<EnumChuckPosition> ChuckMoveCommand { get; }
        IAsyncCommand ChuckMoveCommand { get; }

        IAsyncCommand MeasureOnePositionCommand { get; }
        IAsyncCommand MeasureAllPositionCommand { get; }

        IAsyncCommand SetAdjustPlanartyCommand { get; }
        ICommand FloatTextBoxClickCommand { get; }

        //void ChangeSpecHeightValue(double value);
        void ChangeMarginValue(double value);

        void ChangeFocusingRange(double value);

        ChuckPlanarityDataDescription GetChuckPlanarityInfo();

        Task WrapperChuckMoveCommand(ChuckPos param);
        Task ChuckMoveCommandFunc(object param);
        Task<EventCodeEnum> MeasureOnePositionCommandFunc();
        Task<EventCodeEnum> MeasureAllPositionCommandFunc();

        void UpdateDeviceChangeInfo(ChuckPlanarityDataDescription info);
        Task<EventCodeEnum> SetAdjustPlanartyFunc();
    }

    [Serializable, DataContract]
    public class BaseThing
    {
        [DataMember]
        public Double Left { get; set; }
        [DataMember] 
        public Double Top { get; set; }
        [DataMember] 
        public Double Height { get; set; }
        [DataMember] 
        public Double Width { get; set; }
    }

    [Serializable, DataContract]
    public class RectangleVM : BaseThing
    {
        [DataMember]
        public Brush color { get; set; }
        [DataMember]
        public Brush Fillcolor { get; set; }
        [DataMember]
        public ICommand command { get; set; }
        [DataMember]
        public object commandparam { get; set; }
    }

    [Serializable, DataContract]
    public class CircleVM : BaseThing
    {
        [DataMember] 
        public Double EllipseHeight { get; set; }
        [DataMember] 
        public Double EllipseWidth { get; set; }
        [DataMember] 
        public Brush color { get; set; }
        [DataMember] 
        public Brush Fillcolor { get; set; }
    }

    [Serializable, DataContract]
    public class LineVM : BaseThing
    {
        [DataMember] 
        public Double X1 { get; set; }
        [DataMember] 
        public Double Y1 { get; set; }
        [DataMember] 
        public Double X2 { get; set; }
        [DataMember] 
        public Double Y2 { get; set; }
        [DataMember] 
        public Brush color { get; set; }
        [DataMember] 
        public Brush Fillcolor { get; set; }
    }

    [Serializable, DataContract]
    public class TextBoxVM : BaseThing, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _TbText;
        [DataMember]
        public string TbText
        {
            get { return _TbText; }
            set
            {
                if (value != _TbText)
                {
                    _TbText = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        [DataMember]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Brush _ForegroundColor;
        //[DataMember]
        //public Brush ForegroundColor
        //{
        //    get { return _ForegroundColor; }
        //    set
        //    {
        //        if (value != _ForegroundColor)
        //        {
        //            _ForegroundColor = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Brush _BackgroundColor;
        //[DataMember]
        //public Brush BackgroundColor
        //{
        //    get { return _BackgroundColor; }
        //    set
        //    {
        //        if (value != _BackgroundColor)
        //        {
        //            _BackgroundColor = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
    }

    [Serializable, DataContract]
    public class ChuckPos : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ChuckPos()
        {

        }
        public ChuckPos(double xpos, double ypos, double zpos)
        {
            this.XPos = xpos;
            this.YPos = ypos;
            this.ZPos = zpos;
        }
        //private EnumChuckPosition _ChuckPosEnum;
        //[DataMember]
        //public EnumChuckPosition ChuckPosEnum
        //{
        //    get { return _ChuckPosEnum; }
        //    set
        //    {
        //        if (value != _ChuckPosEnum)
        //        {
        //            _ChuckPosEnum = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private CircleVM _CanvasCircle;
        //[DataMember]
        //public CircleVM CanvasCircle
        //{
        //    get { return _CanvasCircle; }
        //    set
        //    {
        //        if (value != _CanvasCircle)
        //        {
        //            _CanvasCircle = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private CircleVM _CanvasCircle;
        //[DataMember]
        //public CircleVM CanvasCircle
        //{
        //    get { return _CanvasCircle; }
        //    set
        //    {
        //        if (value != _CanvasCircle)
        //        {
        //            _CanvasCircle = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private TextBoxVM _CanvasTextBox;
        [DataMember]
        public TextBoxVM CanvasTextBox
        {
            get { return _CanvasTextBox; }
            set
            {
                if (value != _CanvasTextBox)
                {
                    _CanvasTextBox = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _XPos;
        [DataMember]
        public double XPos
        {
            get { return _XPos; }
            set
            {
                if (value != _XPos)
                {
                    _XPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YPos;
        [DataMember]
        public double YPos
        {
            get { return _YPos; }
            set
            {
                if (value != _YPos)
                {
                    _YPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ZPos;
        [DataMember]
        public double ZPos
        {
            get { return _ZPos; }
            set
            {
                if (value != _ZPos)
                {
                    _ZPos = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
