using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using LogModule;

namespace MainWindowWidgetControl
{
    /// <summary>
    /// MainWindowWidget.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindowWidget : MetroWindow, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }
        #endregion
        public Window MainHandle;

        public MainWindowWidget()
        {
            InitializeComponent();
        }

        private void Button_Show(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Hide();
                MainHandle.Show();
                (MainHandle.DataContext as IViewModelManager)?.UpdateCurMainViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                /*
                 * Ctrl + c를 누른 상태로 [X] 버튼을 클릭하는 경우에만 Widget Unlock이 동작함.
                if (e.Key == Key.Escape)
                {
                    this.Hide();
                    //(((MainHandle.DataContext as IViewModelManager)?.MainScreenView as UserControl)?.DataContext as IMainScreenViewModel)?.PageSwitched();
                    ((MainHandle.DataContext as IViewModelManager)?.MainScreenView as UserControl).DataContext = this.DataContext;
                    this.DataContext = null;
                    MainHandle.Show();
                    (MainHandle.DataContext as IViewModelManager)?.UpdateCurMainViewModel();
                }
                */
                //var data = this.DataContext;
                //this.DataContext = null;
                //this.DataContext = data;
                //(((MainHandle.DataContext as IViewModelManager)?.MainScreenView as UserControl)?.DataContext as IMainScreenViewModel)?.PageSwitched();


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public bool IsWindowOpen<T>(string name = "") where T : Window
        //{
        //    return string.IsNullOrEmpty(name)
        //       ? Application.Current.Windows.OfType<T>().Any()
        //       : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        //}

        public bool IsOpen(Window window)
        {
            return Application.Current.Windows.Cast<Window>().Any(x => x == window);
        }

        private async void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                //bool isopen = IsOpen(MainHandle);

                e.Cancel = true;

                this.Hide();

                if (MainHandle.Visibility == Visibility.Hidden)
                {
                    //(((MainHandle.DataContext as IViewModelManager)?.MainScreenView as UserControl)?.DataContext as IMainScreenViewModel)?.PageSwitched();
                    ((MainHandle.DataContext as IViewModelManager)?.MainScreenView as UserControl).DataContext = this.DataContext;
                    this.DataContext = null;

                    MainHandle.Show();
                    (MainHandle.DataContext as IViewModelManager)?.UpdateCurMainViewModel();
                }

                bool checkUnlock = await (MainHandle.DataContext as IViewModelManager).CheckUnlockWidget();
                if(checkUnlock == false)
                {
                    Visibility v = Visibility.Hidden;

                    Application.Current.Dispatcher.Invoke
                    (
                        () =>

                        {
                            v = (System.Windows.Application.Current.MainWindow).Visibility;

                            if (v == Visibility.Visible)
                            {
                                (MainHandle.DataContext as IViewModelManager)?.ViewModelManager().ViewTransitionAsync(new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"));
                                (System.Windows.Application.Current.MainWindow).Hide();

                                (MainHandle.DataContext as IViewModelManager)?.ViewModelManager().UpdateWidget();
                                (MainHandle.DataContext as IViewModelManager)?.ViewModelManager().MainWindowWidget.Show();
                            }
                        }
                    );
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
