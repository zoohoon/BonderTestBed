using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AlarmViewDialog
{
    /// <summary>
    /// WaitMessageDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WaitMessageDialog : Window, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Stopwatch _stopwatch;
        private System.Windows.Threading.DispatcherTimer _timer;

        public WaitMessageDialog()
        {
            InitializeComponent();

            _stopwatch = new Stopwatch();
            DataContext = this;

            Loaded += (s, e) =>
            {
                _stopwatch.Start();
                _timer.Start();
            };

            Closed += (s, e) =>
            {
                _stopwatch.Stop();
                _timer.Stop();
            };

            _timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            DisableParentWindow();
        }

        protected override void OnClosed(EventArgs e)
        {
            EnableParentWindow();
            base.OnClosed(e);
        }

        private void DisableParentWindow()
        {
            if (Owner != null)
            {
                Owner.IsEnabled = false;
            }
        }

        private void EnableParentWindow()
        {
            if (Owner != null)
            {
                Owner.IsEnabled = true;
            }
        }

        private TimeSpan _ElapsedTime;
        public TimeSpan ElapsedTime
        {
            get
            {
                return _ElapsedTime;
            }
            set
            {
                _ElapsedTime = value;
                RaisePropertyChanged();
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            ElapsedTime = _stopwatch.Elapsed;
        }
    }

    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString(@"hh\:mm\:ss");
            }
            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
