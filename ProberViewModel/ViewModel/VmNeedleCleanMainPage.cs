using System;
using System.Linq;
using System.Threading.Tasks;

namespace NeedleCleanMainPageViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;

    public class RectItem : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region ==>Pad size Property
        private Element<double> _PadSizeLeft
            = new Element<double>();
        public Element<double> PadSizeLeft
        {
            get { return _PadSizeLeft; }
            set
            {
                if (value != _PadSizeLeft)
                {
                    _PadSizeLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadSizeTop
            = new Element<double>();
        public Element<double> PadSizeTop
        {
            get { return _PadSizeTop; }
            set
            {
                if (value != _PadSizeTop)
                {
                    _PadSizeTop = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadSizeWidth
             = new Element<double>();
        public Element<double> PadSizeWidth
        {
            get { return _PadSizeWidth; }
            set
            {
                if (value != _PadSizeWidth)
                {
                    _PadSizeWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadSizeHeight
            = new Element<double>();
        public Element<double> PadSizeHeight
        {
            get { return _PadSizeHeight; }
            set
            {
                if (value != _PadSizeHeight)
                {
                    _PadSizeHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<String> _Text
           = new Element<string>();
        public Element<String> Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _Thickness
            = new Element<int>();
        public Element<int> Thickness
        {
            get { return _Thickness; }
            set
            {
                if (value != _Thickness)
                {
                    _Thickness = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        private VmNeedleCleanMainPage ParentPage;

        public RectItem(double X, double Y, double Width, double Height)
        {
            try
            {
                this.PadSizeLeft.Value = X;
                this.PadSizeTop.Value = Y;
                this.PadSizeWidth.Value = Width;
                this.PadSizeHeight.Value = Height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public RectItem(VmNeedleCleanMainPage parentPage, double X, double Y, double Width, double Height)
        {
            try
            {
                this.ParentPage = parentPage;
                this.PadSizeLeft.Value = X;
                this.PadSizeTop.Value = Y;
                this.PadSizeWidth.Value = Width;
                this.PadSizeHeight.Value = Height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==>MouseDownCommand                   
        private RelayCommand<object> _MouseDownCommand;
        public ICommand MouseDownCommand
        {
            get
            {
                if (null == _MouseDownCommand) _MouseDownCommand = new RelayCommand<object>(MouseDownCommandFunc);
                return _MouseDownCommand;
            }
        }

        private void MouseDownCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.Canvas rec = (System.Windows.Controls.Canvas)param;

                Point point = Mouse.GetPosition(rec);
                VmNeedleCleanMainPage data = (VmNeedleCleanMainPage)rec.DataContext;

                double min_left = double.MaxValue;
                double min_top = double.MaxValue;
                double near_left = 0.0;
                double near_top = 0.0;

                //RectItem rect = data.ParentPage.RectItems.Where(X => X.PadSizeLeft == data.PadSizeLeft).FirstOrDefault();
                for (int i = 0; i < data.RectItems.Count; i++)
                {
                    if (Math.Abs(data.RectItems[i].PadSizeLeft.Value - point.X) < min_left && Math.Abs(data.RectItems[i].PadSizeTop.Value - point.Y) < min_top)
                    {
                        min_left = Math.Abs(data.RectItems[i].PadSizeLeft.Value - point.X);
                        min_top = Math.Abs(data.RectItems[i].PadSizeTop.Value - point.Y);
                        near_left = data.RectItems[i].PadSizeLeft.Value;
                        near_top = data.RectItems[i].PadSizeTop.Value;
                    }

                    for (int j = 0; j < data.RectItems.Count; j++)
                    {
                        if (data.RectItems[j].PadSizeLeft.Value == near_left && data.RectItems[j].PadSizeTop.Value == near_top)
                        {
                            data.RectItems[j].Thickness.Value = 3;
                            data.RectItems[j].Text.Value = "NC " + (j + 1).ToString();
                            data.Rect = data.RectItems[j];
                        }
                        else
                        {
                            data.RectItems[j].Thickness.Value = 0;
                        }
                    }


                    //if (data.RectItems[i].PadSizeLeft.Value == data.Rect.PadSizeLeft.Value && data.RectItems[i].PadSizeTop.Value == data.Rect.PadSizeTop.Value
                    //    && data.RectItems[i].PadSizeHeight.Value == data.Rect.PadSizeHeight.Value && data.RectItems[i].PadSizeWidth.Value == data.Rect.PadSizeWidth.Value)
                    //{
                    //data.RectItems[i].Thickness.Value = 4;
                    //data.RectItems[i].Text.Value = "NC " + (i+1).ToString();
                    //data.Rect = data.RectItems[i];
                    //}

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion        
    }

    public class VmNeedleCleanMainPage : IMainScreenViewModel, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public int ChangeWidthValue = 20;
        public int ChangeHeightValue = 20;

        private bool DragInProgress = false;

        private RectItem _Rect;
        public RectItem Rect
        {
            get { return _Rect; }
            set
            {
                if (value != _Rect)
                {
                    _Rect = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<RectItem> _RectItems
            = new ObservableCollection<RectItem>();
        public ObservableCollection<RectItem> RectItems
        {
            get { return _RectItems; }
            set
            {
                if (value != _RectItems)
                {
                    _RectItems = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Point _LastPoint;
        public Point LastPoint
        {
            get { return _LastPoint; }
            set
            {
                if (value != _LastPoint)
                {
                    _LastPoint = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==>PrevCommand
        private RelayCommand _PrevCommand;
        public ICommand PrevCommand
        {
            get
            {
                if (null == _PrevCommand) _PrevCommand = new RelayCommand(PrevCommandFunc);
                return _PrevCommand;
            }
        }
        private void PrevCommandFunc()
        {
            try
            {
                //RectItems.Remove(this.Rect);
                int index = RectItems.IndexOf(this.Rect);
                index--;
                if (index <= 0)
                {
                    index = 0;
                }

                for (int i = 0; i < RectItems.Count(); i++)
                {

                    if (index == i)
                    {
                        this.Rect = RectItems[index];
                        this.Rect.Thickness.Value = 3;
                    }
                    else
                    {
                        RectItems[i].Thickness.Value = 0;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>NextCommand
        private RelayCommand _NextCommand;
        public ICommand NextCommand
        {
            get
            {
                if (null == _NextCommand) _NextCommand = new RelayCommand(NextCommandFunc);
                return _NextCommand;
            }
        }
        private void NextCommandFunc()
        {
            try
            {
                //RectItems.Remove(this.Rect);
                int index = RectItems.IndexOf(this.Rect);
                index++;
                if (index >= RectItems.Count())
                {
                    index = RectItems.Count() - 1;
                }

                for (int i = 0; i < RectItems.Count(); i++)
                {

                    if (index == i)
                    {
                        this.Rect = RectItems[index];
                        this.Rect.Thickness.Value = 3;
                    }
                    else
                    {
                        RectItems[i].Thickness.Value = 0;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>SizeUpCommand
        private RelayCommand _SizeUpCommand;
        public ICommand SizeUpCommand
        {
            get
            {
                if (null == _SizeUpCommand) _SizeUpCommand = new RelayCommand(SizeUpCommandFunc);
                return _SizeUpCommand;
            }
        }
        private void SizeUpCommandFunc()
        {
            try
            {
                if (Rect.PadSizeHeight.Value == 920)
                    Rect.PadSizeHeight.Value = 920;

                if (Rect.PadSizeTop.Value > 0 && (Rect.PadSizeTop.Value + Rect.PadSizeHeight.Value) < 910)
                {
                    ChangeHeightValue = Math.Abs(ChangeHeightValue);
                    Rect.PadSizeHeight.Value += ChangeHeightValue;
                    Rect.PadSizeTop.Value -= (ChangeHeightValue / 2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>SizeLeftCommand
        private RelayCommand _SizeLeftCommand;
        public ICommand SizeLeftCommand
        {
            get
            {
                if (null == _SizeLeftCommand) _SizeLeftCommand = new RelayCommand(SizeLeftCommandFunc);
                return _SizeLeftCommand;
            }
        }
        private void SizeLeftCommandFunc()
        {
            try
            {
                //this.StageSupervisor().StageModuleState.StageVMove(_XAxis, _MotionVMoveDistance * -1, EnumTrjType.Normal);
                if (Rect.PadSizeWidth.Value == 20)
                    Rect.PadSizeWidth.Value = 20;
                else
                {
                    ChangeWidthValue = -Math.Abs(ChangeWidthValue);
                    Rect.PadSizeWidth.Value += ChangeWidthValue;
                    Rect.PadSizeLeft.Value -= (ChangeWidthValue / 2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>AddCommand
        private RelayCommand _AddCommand;
        public ICommand AddCommand
        {
            get
            {
                if (null == _AddCommand) _AddCommand = new RelayCommand(AddCommandFunc);
                return _AddCommand;
            }
        }
        private void AddCommandFunc()
        {
            try
            {
                //RectItems.Clear();
                //RectItem rect = new RectItem(this, 10, 10, 100, 100);
                //RectItems.Add(rect);
                RectItem rect = new RectItem(this, 30, 30, 100, 100);
                rect.Thickness.Value = 0;
                rect.Text.Value = "NC " + (RectItems.Count() + 1).ToString();
                RectItems.Add(rect);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>SizeRightCommand
        private RelayCommand _SizeRightCommand;
        public ICommand SizeRightCommand
        {
            get
            {
                if (null == _SizeRightCommand) _SizeRightCommand = new RelayCommand(SizeRightCommandFunc);
                return _SizeRightCommand;
            }
        }
        private void SizeRightCommandFunc()
        {
            try
            {
                if (Rect.PadSizeWidth.Value >= 880)
                    Rect.PadSizeWidth.Value = 880;

                if (Rect.PadSizeLeft.Value > 0 && (Rect.PadSizeLeft.Value + Rect.PadSizeWidth.Value) < 870)
                {
                    ChangeWidthValue = Math.Abs(ChangeWidthValue);
                    Rect.PadSizeWidth.Value += ChangeWidthValue;
                    Rect.PadSizeLeft.Value -= (ChangeWidthValue / 2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>SizeDownCommand
        private RelayCommand _SizeDownCommand;
        public ICommand SizeDownCommand
        {
            get
            {
                if (null == _SizeDownCommand) _SizeDownCommand = new RelayCommand(SizeDownCommandFunc);
                return _SizeDownCommand;
            }
        }
        private void SizeDownCommandFunc()
        {
            try
            {
                //this.StageSupervisor().StageModuleState.StageVMove(_XAxis, _MotionVMoveDistance * -1, EnumTrjType.Normal);
                if (Rect.PadSizeHeight.Value == 20)
                    Rect.PadSizeHeight.Value = 20;
                else
                {
                    ChangeHeightValue = -Math.Abs(ChangeHeightValue);
                    Rect.PadSizeHeight.Value += ChangeHeightValue;
                    Rect.PadSizeTop.Value -= (ChangeHeightValue / 2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>DelCommand
        private RelayCommand _DelCommand;
        public ICommand DelCommand
        {
            get
            {
                if (null == _DelCommand) _DelCommand = new RelayCommand(DelCommandFunc);
                return _DelCommand;
            }
        }
        private void DelCommandFunc()
        {
            RectItems.Remove(this.Rect);
        }
        #endregion

        #region ==>PosUpCommand
        private RelayCommand _PosUpCommand;
        public ICommand PosUpCommand
        {
            get
            {
                if (null == _PosUpCommand) _PosUpCommand = new RelayCommand(PosUpCommandFunc);
                return _PosUpCommand;
            }
        }
        private void PosUpCommandFunc()
        {
            try
            {
                if (Rect.PadSizeTop.Value > 0)
                {
                    Rect.PadSizeTop.Value = Rect.PadSizeTop.Value - 10;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>PosLeftCommand
        private RelayCommand _PosLeftCommand;
        public ICommand PosLeftCommand
        {
            get
            {
                if (null == _PosLeftCommand) _PosLeftCommand = new RelayCommand(PosLeftCommandFunc);
                return _PosLeftCommand;
            }
        }
        private void PosLeftCommandFunc()
        {
            try
            {
                //this.StageSupervisor().StageModuleState.StageVMove(_XAxis, _MotionVMoveDistance * -1, EnumTrjType.Normal); 
                if (Rect.PadSizeLeft.Value > 0)
                {
                    Rect.PadSizeLeft.Value = Rect.PadSizeLeft.Value - 10;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>PosRightCommand
        private RelayCommand _PosRightCommand;
        public ICommand PosRightCommand
        {
            get
            {
                if (null == _PosRightCommand) _PosRightCommand = new RelayCommand(PosRightCommandFunc);
                return _PosRightCommand;
            }
        }
        private void PosRightCommandFunc()
        {
            try
            {
                if (Rect.PadSizeLeft.Value >= 0 && (Rect.PadSizeLeft.Value + Rect.PadSizeWidth.Value) < 870)
                {
                    Rect.PadSizeLeft.Value = Rect.PadSizeLeft.Value + 10;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>PosDownCommand
        private RelayCommand _PosDownCommand;
        public ICommand PosDownCommand
        {
            get
            {
                if (null == _PosDownCommand) _PosDownCommand = new RelayCommand(PosDownCommandFunc);
                return _PosDownCommand;
            }
        }
        private void PosDownCommandFunc()
        {
            try
            {
                if (Rect.PadSizeTop.Value >= 0 && (Rect.PadSizeTop.Value + Rect.PadSizeHeight.Value) < 910)
                {
                    Rect.PadSizeTop.Value = Rect.PadSizeTop.Value + 10;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>MouseDownCommand
        private RelayCommand<object> _MouseDownCommand;
        public ICommand MouseDownCommand
        {
            get
            {
                if (null == _MouseDownCommand) _MouseDownCommand = new RelayCommand<object>(MouseDownCommandFunc);
                return _MouseDownCommand;
            }
        }
        private void MouseDownCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.Canvas rec = (System.Windows.Controls.Canvas)param;
                LastPoint = Mouse.GetPosition(rec);
                DragInProgress = true;


                Point point = Mouse.GetPosition(rec);

                double offset_x = point.X - LastPoint.X;
                double offset_y = point.Y - LastPoint.Y;

                double new_x = Rect.PadSizeLeft.Value;
                double new_y = Rect.PadSizeTop.Value;
                double new_width = Rect.PadSizeWidth.Value;
                double new_height = Rect.PadSizeHeight.Value;

                //this.NeedleCleaner().NCHeightProfilingModule.GetPZErrorComp();

                if ((new_width > 0) && (new_height > 0) && (new_x + offset_x + Rect.PadSizeWidth.Value) < 870
                     && (new_y + offset_y + Rect.PadSizeHeight.Value) < 910 && new_x + offset_x > 0 && new_y + offset_y > 0)
                {
                    Rect.PadSizeLeft.Value = new_x + offset_x;
                    Rect.PadSizeTop.Value = new_y + offset_y;
                    LastPoint = point;
                }

                //LoggerManager.Debug("SDSDFSDF");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==>MouseMoveCommand
        private RelayCommand<object> _MouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get
            {
                if (null == _MouseMoveCommand) _MouseMoveCommand = new RelayCommand<object>(MouseMoveCommandFunc);
                return _MouseMoveCommand;
            }
        }
        [PreventLogging]
        private void MouseMoveCommandFunc(object param)
        {
            try
            {
                if (DragInProgress == true)
                {
                    System.Windows.Controls.Canvas rec = (System.Windows.Controls.Canvas)param;
                    //VmNeedleCleanMainPage data = (VmNeedleCleanMainPage)rec.DataContext;
                    Point point = Mouse.GetPosition(rec);

                    double offset_x = point.X - LastPoint.X;
                    double offset_y = point.Y - LastPoint.Y;

                    double new_x = Rect.PadSizeLeft.Value;
                    double new_y = Rect.PadSizeTop.Value;
                    double new_width = Rect.PadSizeWidth.Value;
                    double new_height = Rect.PadSizeHeight.Value;

                    if ((new_width > 0) && (new_height > 0) && (new_x + offset_x + Rect.PadSizeWidth.Value) < 870
                         && (new_y + offset_y + Rect.PadSizeHeight.Value) < 910 && new_x + offset_x > 0 && new_y + offset_y > 0)
                    {
                        Rect.PadSizeLeft.Value = new_x + offset_x;
                        Rect.PadSizeTop.Value = new_y + offset_y;
                        LastPoint = point;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==>MouseUpCommand
        private RelayCommand<object> _MouseUpCommand;
        public ICommand MouseUpCommand
        {
            get
            {
                if (null == _MouseUpCommand) _MouseUpCommand = new RelayCommand<object>(MouseUpCommandFunc);
                return _MouseUpCommand;
            }
        }

        private void MouseUpCommandFunc(object param)
        {
            DragInProgress = false;
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("ef6cdfcc-7ee8-437a-a38a-1abebec3d664");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Rect = new RectItem(this, 10, 10, 100, 100);
                RectItems.Add(Rect);
                Rect.PadSizeWidth.Value = 200;
                Rect.PadSizeHeight.Value = 200;
                Rect.PadSizeLeft.Value = (880 / 2) - (Rect.PadSizeWidth.Value / 2);
                Rect.PadSizeTop.Value = (920 / 2) - (Rect.PadSizeHeight.Value / 2);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
          
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
    }
}
