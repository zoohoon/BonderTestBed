using CUIServices;
using LogModule;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CUIContextMenu
{
    public partial class UserContextMenu : System.Windows.Controls.MenuItem, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public UserContextMenu()
        {
            try
            {
                InitializeComponent();

                this.DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private bool _IsEnable;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                if (value != _IsEnable)
                {
                    _IsEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _ParentControl;
        public object ParentControl
        {
            get { return _ParentControl; }
            set
            {
                if (value != _ParentControl)
                {
                    _ParentControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                MaskingItem content = (this.Parent as ContextMenu)?.DataContext as MaskingItem;
                if (content != null)
                {
                    ParentControl = content;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.MenuItem menuItem = e.OriginalSource as System.Windows.Controls.MenuItem;

                if (menuItem != null)
                {
                    if (!string.IsNullOrEmpty(menuItem.Header.ToString()))
                    {
                        int selectMaskingValue = int.Parse(menuItem.Header.ToString());
                        int setMaskingResult = 0;
                        ICUIControl cuiCon = (ParentControl as MaskingItem)?.SourceUI as ICUIControl;
                        setMaskingResult = CUIService.SetMaskingLevel(cuiCon.GUID, selectMaskingValue);

                        //if (setMaskingResult != selectMaskingValue)
                        //{
                        //    LoggerManager.Debug($"Don't save CUI Masking Value. {cuiCon.GUID}");
                        //}

                        cuiCon.MaskingLevel = selectMaskingValue;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush retBrush = new SolidColorBrush(Colors.Gray);
            try
            {
                int? one = values[0] as int?;
                int? two = values[1] as int?;
                if (one != null && two != null)
                {
                    if (one == two)
                    {
                        retBrush = new SolidColorBrush(new Color() { R = 95, G = 75, B = 139, A = 255 });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
