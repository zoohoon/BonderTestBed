using ProberInterfaces;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;

namespace UcChillerScreen
{
    public partial class UcHBChiller : UserControl, IMainScreenView
    {
        public UcHBChiller()
        {
            InitializeComponent();
        }

        private Autofac.IContainer Container { get; set; }

        readonly Guid _ViewGUID = new Guid("F1D9D686-D18A-4844-A3B6-068DFE2859E9");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        #region << PropertyChanged >>
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion
    }

    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            string retVal = "Gray";
            try
            {
            bool IsReverse = false;

            if (parameter != null)
            {
                if(parameter.ToString() == "REVERSE")
                {
                    IsReverse = true;
                }
            }

            if ((bool)value ^ IsReverse)
            {
                retVal = "Green";
            }
            else
            {
                retVal = "Gray";
            }

            }
            catch (Exception err)
            {
                 throw;
            }
            return retVal;
        }
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
