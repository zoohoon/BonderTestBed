using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChillerConnSettingDialog
{
    using LogModule;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Animation;
    using UcAnimationScrollViewer;

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    //public partial class ChillerConnSettingView : CustomDialog
    public partial class ChillerConnSettingView : UserControl , INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public ChillerConnSettingView()
        {
            PreviewKeyDown += UserControl_PreviewKeyDown;

            InitializeComponent();
        }

        private Visibility _ChillerErrorOptionSettingVisibiliy = Visibility.Hidden;
        public Visibility ChillerErrorOptionSettingVisibiliy
        {
            get { return _ChillerErrorOptionSettingVisibiliy; }
            set
            {
                if (value != _ChillerErrorOptionSettingVisibiliy)
                {
                    _ChillerErrorOptionSettingVisibiliy = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                     Keyboard.IsKeyDown(Key.S))
                {
                    if(ChillerErrorOptionSettingVisibiliy == Visibility.Hidden)
                    {
                        ChillerErrorOptionSettingVisibiliy = Visibility.Visible;
                    }
                    else
                    {
                        ChillerErrorOptionSettingVisibiliy = Visibility.Hidden;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ExitWindowCommand;
        public ICommand ExitWindowCommand
        {
            get
            {
                if (null == _ExitWindowCommand) _ExitWindowCommand = new RelayCommand(ExitWindowCommandFunc);
                return _ExitWindowCommand;
            }
        }
        private void ExitWindowCommandFunc()
        {
            try
            {
                Window window = Window.GetWindow(this);
                if (window != null)
                    window.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CategoryUpBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset - ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));

                storyboard.Begin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void CategoryDwBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DoubleAnimation verticalAnimation = new DoubleAnimation();

                verticalAnimation.From = svViewer.VerticalOffset;
                verticalAnimation.To = svViewer.VerticalOffset + ((svViewer.ActualHeight / 3) * 2);
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);

                Storyboard.SetTarget(verticalAnimation, svViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimationScrollViewer.CurrentVerticalOffsetProperty));
                storyboard.Begin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
