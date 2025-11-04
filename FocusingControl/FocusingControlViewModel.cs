using System;

namespace FocusingControl
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    public class FocusingControlViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region //=>RaisePropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private FocusParameter _FocusParam;
        public FocusParameter FocusParam
        {
            get { return _FocusParam; }
            set
            {
                if (value != _FocusParam)
                {
                    _FocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusROI_x;
        public double FocusROI_x
        {
            get { return _FocusROI_x; }
            set
            {
                if (value != _FocusROI_x)
                {
                    _FocusROI_x = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusROI_y;
        public double FocusROI_y
        {
            get { return _FocusROI_y; }
            set
            {
                if (value != _FocusROI_y)
                {
                    _FocusROI_y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusROI_Width;
        public double FocusROI_Width
        {
            get { return _FocusROI_Width; }
            set
            {
                if (value != _FocusROI_Width)
                {
                    _FocusROI_Width = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusROI_Height;
        public double FocusROI_Height
        {
            get { return _FocusROI_Height; }
            set
            {
                if (value != _FocusROI_Height)
                {
                    _FocusROI_Height = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<EnumProberCam> _CamTypes;
        public ObservableCollection<EnumProberCam> CamTypes
        {
            get { return _CamTypes; }
            set
            {
                if (value != _CamTypes)
                {
                    _CamTypes = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<EnumAxisConstants> _Axises;
        public ObservableCollection<EnumAxisConstants> Axises
        {
            get { return _Axises; }
            set
            {
                if (value != _Axises)
                {
                    _Axises = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<EnumFocusingType> _FocusTypes;
        public ObservableCollection<EnumFocusingType> FocusTypes
        {
            get { return _FocusTypes; }
            set
            {
                if (value != _FocusTypes)
                {
                    _FocusTypes = value;
                    RaisePropertyChanged();
                }
            }
        }


        public FocusingControlViewModel()
        {
            CamTypes = new ObservableCollection<EnumProberCam>();
            CamTypes.Add(EnumProberCam.WAFER_HIGH_CAM);
            CamTypes.Add(EnumProberCam.WAFER_LOW_CAM);
            CamTypes.Add(EnumProberCam.PIN_HIGH_CAM);
            CamTypes.Add(EnumProberCam.PIN_LOW_CAM);

            Axises = new ObservableCollection<EnumAxisConstants>();
            Axises.Add(EnumAxisConstants.Z);
            Axises.Add(EnumAxisConstants.PZ);

            FocusTypes = new ObservableCollection<EnumFocusingType>();
            FocusTypes.Add(EnumFocusingType.UNDEFINED);
            FocusTypes.Add(EnumFocusingType.WAFER);
            FocusTypes.Add(EnumFocusingType.PIN);
            FocusTypes.Add(EnumFocusingType.MARK);

        }

        public void SetFocusParam(FocusParameter focusparam)
        {
            try
            {
                FocusParam = focusparam;

                FocusROI_x = FocusParam.FocusingROI.Value.X;
                FocusROI_y = FocusParam.FocusingROI.Value.Y;
                FocusROI_Width = FocusParam.FocusingROI.Value.Width;
                FocusROI_Height = FocusParam.FocusingROI.Value.Height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }


        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private RelayCommand<Object> _FocusROITextBoxClickCommand;
        public ICommand FocusROITextBoxClickCommand
        {
            get
            {
                if (null == _FocusROITextBoxClickCommand) _FocusROITextBoxClickCommand = new RelayCommand<Object>(FocusROITextBoxClickCommandFunc);
                return _FocusROITextBoxClickCommand;
            }
        }


        private void FocusROITextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                var tmpstr = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 3);

                string propertyToUpdate = tb.Tag.ToString(); // We use Tag to get the property name (X, Y, Width, Height)

                bool isValid = false;

                var tmpval = Convert.ToDouble(tmpstr);

                switch (propertyToUpdate)
                {
                    case "Width":

                        // Validate Width
                        if (tmpval > 0 && tmpval <= 960)
                        {
                            isValid = true;
                            FocusROI_Width = tmpval;
                        }
                        else
                        {
                            isValid = false;

                            LoggerManager.Error($"[{this.GetType().Name}], FocusROITextBoxClickCommandFunc() : Invalid Width value, Input = {tmpval}");
                        }
                        break;
                    case "Height":

                        // Validate Height
                        if (tmpval > 0 && tmpval <= 960)
                        {
                            isValid = true;
                            FocusROI_Height = tmpval;
                        }
                        else
                        {
                            isValid = false;

                            LoggerManager.Error($"[{this.GetType().Name}], FocusROITextBoxClickCommandFunc() : Invalid Height value, Input = {tmpval}");
                        }
                        break;
                }

                if(isValid)
                {
                    tb.Text = tmpstr;
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                    UpdateFocusingROI();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void UpdateFocusingROI()
        {

            try
            {
                // TODO : 하드 코딩, Loader에서는 값을 얻어올 수 없다.
                //var focusCam = this.VisionManager().GetCam(FocusParam.FocusingCam.Value);
                //var cam_width = focusCam.GetGrabSizeWidth();
                //var cam_height = focusCam.GetGrabSizeHeight();

                var cam_width = 960;
                var cam_height = 960;

                FocusROI_x = (cam_width - FocusROI_Width) / 2.0;
                FocusROI_y = (cam_height - FocusROI_Height) / 2.0;

                FocusParam.FocusingROI.Value = new Rect(FocusROI_x, FocusROI_y, FocusROI_Width, FocusROI_Height);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
