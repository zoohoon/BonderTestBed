using System;
using System.Windows.Controls;
using System.Windows.Data;
using LogModule;
using ProberInterfaces;

namespace ProberViewModel
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualContactControl : UserControl, IMainScreenView
    {
        public ManualContactControl()
        {
            InitializeComponent();
        }

        readonly Guid _ViewGUID = new Guid("ac247488-2cb3-4250-9cfd-bd6852802a83");
        public Guid ScreenGUID { get { return _ViewGUID; } }
    }

    public class IsProbingStartPosAllContactConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool retVal = false;
            try
            {

            if (value is OverDriveStartPositionType)
            {
                OverDriveStartPositionType bValue = (OverDriveStartPositionType)value;

                if (bValue == OverDriveStartPositionType.ALL_CONTACT)
                {
                    retVal = true;
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            OverDriveStartPositionType retVal = OverDriveStartPositionType.ALL_CONTACT;
            try
            {

            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue == false)
                {
                    retVal = OverDriveStartPositionType.FIRST_CONTACT;
                }
                else
                {
                    retVal = OverDriveStartPositionType.ALL_CONTACT;
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }

    public class IsProbingStartPosFirstContactConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            bool retVal = false;
            try
            {

            if (value is OverDriveStartPositionType)
            {
                OverDriveStartPositionType bValue = (OverDriveStartPositionType)value;

                if (bValue == OverDriveStartPositionType.FIRST_CONTACT)
                {
                    retVal = true;
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            OverDriveStartPositionType retVal = OverDriveStartPositionType.ALL_CONTACT;
            try
            {

            if (value is bool)
            {
                bool bValue = (bool)value;

                if (bValue == false)
                {
                    retVal = OverDriveStartPositionType.ALL_CONTACT;
                }
                else
                {
                    retVal = OverDriveStartPositionType.FIRST_CONTACT;
                }
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
    }
}
