namespace FocusingSettingDialog
{
    using System;
    using System.Windows;
    using ProberInterfaces.Focus;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberErrorCode;
    using LogModule;

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FocusSettingDialog : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FocusSettingDialog()
        {
            try
            {
                this.FocusDealyTime = FocusingStaticParam.FocusDelayTime;
                this.SaveImageFlag = FocusingStaticParam.SaveImageFlag;
                this.OverlayFocusROIFlag = FocusingStaticParam.OverlayFocusROIFlag;
                this.SaveImageFullPath = FocusingStaticParam.SaveDebugImagePath;

                this.FocusingOffsetX = FocusingStaticParam.FocusingOffsetX;
                this.FocusingOffsetY = FocusingStaticParam.FocusingOffsetY;
                this.FocusingWidth = FocusingStaticParam.FocusingWidth;
                this.FocusingHeight = FocusingStaticParam.FocusingHeight;

                this.PinFocusingOffsetX = FocusingStaticParam.PinFocusingOffsetX;
                this.PinFocusingOffsetY = FocusingStaticParam.PinFocusingOffsetY;
                this.PinFocusingWidth = FocusingStaticParam.PinFocusingWidth;
                this.PinFocusingHeight = FocusingStaticParam.PinFocusingHeight;

                this.SetIdleGrabCount = FocusingStaticParam.SetIdleGrabCount;

                if(FocusingStaticParam.ErrorEventCodeEnum == EventCodeEnum.FOCUS_POS_NEAREDGE)
                {
                    error_FOCUS_POS_NEAREDGE_Checked = true;
                }
                else
                {
                    error_FOCUS_POS_NEAREDGE_Checked = false;
                }

                this.DataContext = this;
                InitializeComponent();
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private int _FocusDealyTime;
        public int FocusDealyTime
        {
            get { return _FocusDealyTime; }
            set
            {
                if (value != _FocusDealyTime)
                {
                    _FocusDealyTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _SaveImageFlag;
        public bool SaveImageFlag
        {
            get { return _SaveImageFlag; }
            set
            {
                if (value != _SaveImageFlag)
                {
                    _SaveImageFlag = value;
                    RaisePropertyChanged();
                    FocusingStaticParam.SaveImageFlag = _SaveImageFlag;
                }
            }
        }

        private bool _OverlayFocusROIFlag;
        public bool OverlayFocusROIFlag
        {
            get { return _OverlayFocusROIFlag; }
            set
            {
                if (value != _OverlayFocusROIFlag)
                {
                    _OverlayFocusROIFlag = value;
                    RaisePropertyChanged();
                    FocusingStaticParam.OverlayFocusROIFlag = _OverlayFocusROIFlag;
                }
            }
        }
        

        private string _SaveImageFullPath;
        public string SaveImageFullPath
        {
            get { return _SaveImageFullPath; }
            set
            {
                if (value != _SaveImageFullPath)
                {
                    _SaveImageFullPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusingOffsetX;
        public double FocusingOffsetX
        {
            get { return _FocusingOffsetX; }
            set
            {
                if (value != _FocusingOffsetX)
                {
                    _FocusingOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinFocusingOffsetX;
        public double PinFocusingOffsetX
        {
            get { return _PinFocusingOffsetX; }
            set
            {
                if (value != _PinFocusingOffsetX)
                {
                    _PinFocusingOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _FocusingOffsetY;
        public double FocusingOffsetY
        {
            get { return _FocusingOffsetY; }
            set
            {
                if (value != _FocusingOffsetY)
                {
                    _FocusingOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinFocusingOffsetY;
        public double PinFocusingOffsetY
        {
            get { return _PinFocusingOffsetY; }
            set
            {
                if (value != _PinFocusingOffsetY)
                {
                    _PinFocusingOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _FocusingWidth;
        public double FocusingWidth
        {
            get { return _FocusingWidth; }
            set
            {
                if (value != _FocusingWidth)
                {
                    _FocusingWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinFocusingWidth;
        public double PinFocusingWidth
        {
            get { return _PinFocusingWidth; }
            set
            {
                if (value != _PinFocusingWidth)
                {
                    _PinFocusingWidth = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _FocusingHeight;
        public double FocusingHeight
        {
            get { return _FocusingHeight; }
            set
            {
                if (value != _FocusingHeight)
                {
                    _FocusingHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinFocusingHeight;
        public double PinFocusingHeight
        {
            get { return _PinFocusingHeight; }
            set
            {
                if (value != _PinFocusingHeight)
                {
                    _PinFocusingHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SetIdleGrabCount;
        public int SetIdleGrabCount
        {
            get { return _SetIdleGrabCount; }
            set
            {
                if (value != _SetIdleGrabCount)
                {
                    _SetIdleGrabCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _error_FOCUS_POS_NEAREDGE_Checked;

        public bool error_FOCUS_POS_NEAREDGE_Checked
        {
            get { return _error_FOCUS_POS_NEAREDGE_Checked; }
            set { _error_FOCUS_POS_NEAREDGE_Checked = value; }
        }


        private void SetDelayTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FocusingStaticParam.FocusDelayTime = this.FocusDealyTime;
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        private void SetPathBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FocusingStaticParam.SaveDebugImagePath = this.SaveImageFullPath;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private void SetROI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FocusingStaticParam.FocusingOffsetX = this.FocusingOffsetX;
                FocusingStaticParam.FocusingOffsetY = this.FocusingOffsetY;
                FocusingStaticParam.FocusingWidth = this.FocusingWidth;
                FocusingStaticParam.FocusingHeight = this.FocusingHeight;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private void SetPinROI_Click(object sender, RoutedEventArgs e)
        {
            FocusingStaticParam.PinFocusingOffsetX = this.PinFocusingOffsetX;
            FocusingStaticParam.PinFocusingOffsetY = this.PinFocusingOffsetY;
            FocusingStaticParam.PinFocusingWidth = this.PinFocusingWidth;
            FocusingStaticParam.PinFocusingHeight = this.PinFocusingHeight;
        }

        private void SetIdleGrabCount_Click(object sender, RoutedEventArgs e)
        {
            FocusingStaticParam.SetIdleGrabCount = this.SetIdleGrabCount;
        }

        private void cb_error_FOCUS_POS_NEAREDGE_Checked(object sender, RoutedEventArgs e)
        {
            FocusingStaticParam.ErrorEventCodeEnum = EventCodeEnum.FOCUS_POS_NEAREDGE;
            LoggerManager.Debug($"FocusingStaticParam.ErrorEventCodeEnum set to {FocusingStaticParam.ErrorEventCodeEnum}");
        }

        private void cb_error_FOCUS_POS_NEAREDGE_Unchecked(object sender, RoutedEventArgs e)
        {
            FocusingStaticParam.ErrorEventCodeEnum = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"FocusingStaticParam.ErrorEventCodeEnum set to {FocusingStaticParam.ErrorEventCodeEnum}");
        }
    }
}
