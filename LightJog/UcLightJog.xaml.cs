using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LightJog
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using CUIServices;
    using LogModule;
    using ProberInterfaces;
    /// <summary>
    /// Interaction logic for UcLightJog.xaml
    /// </summary>
    public partial class UcLightJog : UserControl, ICUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Guid GUID { get; set; } = new Guid("C399DE62-C35E-8CA5-67B2-926140E66910");
        private int _MaskingLevel;
        public int MaskingLevel
        {
            get
            {
                _MaskingLevel = CUIService.GetMaskingLevel(this.GUID);
                return _MaskingLevel;
            }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool Lockable { get; set; } = true;
        public bool InnerLockable { get; set; } = false;
        public List<int> AvoidLockHashCodes { get; set; }

        private bool _IsReleaseMode;
        public bool IsReleaseMode
        {
            get { return _IsReleaseMode; }
            set
            {
                if (value != _IsReleaseMode)
                {
                    _IsReleaseMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BindingBase _IsEnableBindingBase;
        public BindingBase IsEnableBindingBase
        {
            get { return _IsEnableBindingBase; }
            set
            {
                if (value != _IsEnableBindingBase)
                {
                    _IsEnableBindingBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        public UcLightJog()
        {
            try
            {
                InitializeComponent();
                //this.DataContext = new LightJogViewModel(
                //    container: ModuleFactory.ModuleResolver.ConfigureDependencies(),
                //    maxLightValue: 255,
                //    minLightValue: 0,
                //    initLightValue: 100,
                //    initCameraType: ProberInterfaces.EnumProberCam.PIN_LOW_CAM);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class LightValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if(values[0] != DependencyProperty.UnsetValue & 
                    values[1] != DependencyProperty.UnsetValue &
                    values[2] != DependencyProperty.UnsetValue)
                {
                    ICamera cam = values[1] as ICamera;
                    return cam.GetLight((EnumLightType)values[2]).ToString();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
