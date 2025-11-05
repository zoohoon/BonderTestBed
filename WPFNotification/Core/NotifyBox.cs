using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using WPFNotification.Core.Configuration;
using WPFNotification.Core.Interactivity;

namespace WPFNotification.Core
{
    public static class NotifyBox
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        #region Fields
        /// <summary>
        /// Max number of notifications window
        /// </summary>
        private const int MAX_NOTIFICATIONS = 1;

        /// <summary>
        /// Number of notification windows
        /// </summary>
        private static int notificationWindowsCount;

        /// <summary>
        /// The margin between notification windows.
        /// </summary>
        private const double Margin = 5;

        /// <summary>
        /// list of notifications window.
        /// </summary>
        private static List<WindowInfo> notificationWindows;

        /// <summary>
        /// buffer list of notifications window.
        /// </summary>
        private static List<WindowInfo> notificationsBuffer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialises static members of the <see cref="NotifyBox"/> class.
        /// </summary>
        static NotifyBox()
        {
            try
            {
                notificationWindows = new List<WindowInfo>();
                notificationsBuffer = new List<WindowInfo>();
                notificationWindowsCount = 0;
                _timer.Tick += DispatcherTimer_Tick;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region Public Static Methods
        /// <summary>
        /// Shows the specified notification.
        /// </summary>
        /// <param name="content">The notification content.</param>
        /// <param name="configuration">The notification configuration object.</param>
        public static void Show(object content, NotificationConfiguration configuration)
        {
            try
            {
                DataTemplate notificationTemplate = (DataTemplate)Application.Current.Resources[configuration.TemplateName];

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Window window = new Window()
                    {
                        Title = "",
                        Width = configuration.Width.Value,
                        Height = configuration.Height.Value,
                        Content = content,
                        ShowActivated = false,
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        ShowInTaskbar = false,
                        Topmost = true,
                        Background = Brushes.Transparent,
                        UseLayoutRounding = true,
                        ContentTemplate = notificationTemplate
                    };
                    Show(window, configuration.DisplayDuration, configuration.NotificationFlowDirection);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static void ShowToast(object content, NotificationConfiguration configuration)
        {
            try
            {
                DataTemplate notificationTemplate = (DataTemplate)Application.Current.Resources[configuration.TemplateName];
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Window window = new Window()
                    {
                        Title = "",
                        Width = configuration.Width.Value,
                        Height = configuration.Height.Value,
                        Content = content,
                        ShowActivated = false,
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        ShowInTaskbar = false,
                        Topmost = true,
                        Background = Brushes.Transparent,
                        UseLayoutRounding = true,
                        ContentTemplate = notificationTemplate
                    };

                    ShowToast(window, configuration.DisplayDuration, configuration.NotificationFlowDirection);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// Shows the specified notification.
        /// </summary>
        /// <param name="content">The notification content.</param>
        public static void Show(object content)
        {
            Show(content, NotificationConfiguration.DefaultConfiguration);
        }

        private static DispatcherTimer _timer = new DispatcherTimer();

        private static void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (_currentWindowInfo != null && _currentWindowInfo.Window.IsVisible == true)
                OnTimerElapsed(_currentWindowInfo);
            _currentWindowInfo.Window.Close();
        }

        private static WindowInfo _currentWindowInfo = null;
        private static void StartWindowCloseTimer(WindowInfo windowInfo)
        {
            _timer.Stop();
            _timer.Interval = windowInfo.DisplayDuration;
            _currentWindowInfo = windowInfo;
            _timer.Tick += DispatcherTimer_Tick;
            windowInfo.Window.Show();
            windowInfo.Window.Visibility = Visibility.Visible;
            _timer.Start();
        }

        /// <summary>
        /// Shows the specified window as a notification.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="displayDuration">The display duration.</param>
        public static void Show(Window window, TimeSpan displayDuration, NotificationFlowDirection notificationFlowDirection)
        {
            try
            {
                BehaviorCollection behaviors = Interaction.GetBehaviors(window);

                behaviors.Add(new FadeBehavior());
                behaviors.Add(new SlideBehavior());

                SetWindowDirection(window, notificationFlowDirection);

                notificationWindowsCount += 1;

                WindowInfo windowInfo = new WindowInfo()
                {
                    ID = notificationWindowsCount,
                    DisplayDuration = displayDuration,
                    Window = window
                };

                windowInfo.Window.Closed += Window_Closed;

                if (notificationWindows.Count + 1 > MAX_NOTIFICATIONS)
                {
                    notificationsBuffer.Add(windowInfo);
                }
                else
                {
                    //StartWindowCloseTimer(windowInfo);
                    window.Show();

                    bool isopening = true;
                    while (isopening)
                    {
                        foreach (Window openWin in System.Windows.Application.Current.Windows)
                        {
                            if (openWin.GetType() == window.GetType())
                            {
                                isopening = false;
                                break;
                            }
                        }

                    }


                    notificationWindows.Add(windowInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public static void ShowToast(Window window, TimeSpan displayDuration, NotificationFlowDirection notificationFlowDirection)
        {
            try
            {
                BehaviorCollection behaviors = Interaction.GetBehaviors(window);

                behaviors.Add(new FadeBehavior());
                behaviors.Add(new SlideBehavior());

                SetWindowDirection(window, notificationFlowDirection);

                notificationWindowsCount += 1;

                WindowInfo windowInfo = new WindowInfo()
                {
                    ID = notificationWindowsCount,
                    DisplayDuration = displayDuration,
                    Window = window
                };

                windowInfo.Window.Closed += Window_Closed;

                if (notificationWindows.Count + 1 > MAX_NOTIFICATIONS)
                {
                    notificationsBuffer.Add(windowInfo);
                }
                else
                {
                    //windowInfo.Window.Show();
                    StartWindowCloseTimer(windowInfo);

                    //window.Show();
                    //while (isopening)
                    //{
                    //    //foreach (Window openWin in System.Windows.Application.Current.Windows)
                    //    //{
                    //    //    if (openWin.GetType() == window.GetType())
                    //    //    {
                    //    //        isopening = false;
                    //    //        break;
                    //    //    }
                    //    //}

                    //}


                    notificationWindows.Add(windowInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// Remove all notifications from notification list and buffer list.
        /// </summary>
        public static void ClearNotifications()
        {
            try
            {
                if (notificationWindows.Count > 0)
                {
                    foreach (var window in notificationWindows)
                    {
                        BehaviorCollection behaviors = Interaction.GetBehaviors(window.Window);
                        FadeBehavior fadeBehavior = behaviors.OfType<FadeBehavior>().First();
                        SlideBehavior slideBehavior = behaviors.OfType<SlideBehavior>().First();

                        fadeBehavior.FadeOut();
                        slideBehavior.SlideOut();

                        EventHandler eventHandler = null;

                        eventHandler = (sender2, e2) =>
                        {
                            fadeBehavior.FadeOutCompleted -= eventHandler;
                            notificationWindows.Remove(window);

                            try
                            {
                                window.Window.Close();

                                if (notificationsBuffer != null && notificationsBuffer.Count > 0)
                                {
                                    var BufferWindowInfo = notificationsBuffer.First();
                                    StartWindowCloseTimer(BufferWindowInfo);

                                    notificationWindows.Add(BufferWindowInfo);
                                    BufferWindowInfo.Window.Show();
                                    notificationsBuffer.Remove(BufferWindowInfo);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        };
                        fadeBehavior.FadeOutCompleted += eventHandler;
                    }
                }

                notificationWindows.Clear();
                notificationsBuffer.Clear();

                notificationWindowsCount = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        #endregion

        #region Private Static Methods

        /// <summary>
        /// Called when the timer has elapsed. Removes any stale notifications.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private static void OnTimerElapsed(WindowInfo windowInfo)
        {
            try
            {
                if (notificationWindows.Count > 0 && !notificationWindows.Any(i => i.ID == windowInfo.ID))
                {
                    return;
                }
                DateTime now = DateTime.Now;

                if (windowInfo.Window.IsMouseOver)
                {
                    StartWindowCloseTimer(windowInfo);                   
                }
                else
                {
                    BehaviorCollection behaviors = Interaction.GetBehaviors(windowInfo.Window);
                    FadeBehavior fadeBehavior = behaviors.OfType<FadeBehavior>().First();
                    SlideBehavior slideBehavior = behaviors.OfType<SlideBehavior>().First();

                    fadeBehavior.FadeOut();
                    slideBehavior.SlideOut();

                    EventHandler eventHandler = null;

                    eventHandler = (sender2, e2) =>
                    {
                        fadeBehavior.FadeOutCompleted -= eventHandler;
                        notificationWindows.Remove(windowInfo);
                        windowInfo.Window.Close();

                        if (notificationsBuffer != null && notificationsBuffer.Count > 0)
                        {
                            var BufferWindowInfo = notificationsBuffer.First();
                            StartWindowCloseTimer(BufferWindowInfo);

                            notificationWindows.Add(BufferWindowInfo);
                            BufferWindowInfo.Window.Show();
                            notificationsBuffer.Remove(BufferWindowInfo);
                        }
                    };
                    fadeBehavior.FadeOutCompleted += eventHandler;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
        }

        /// <summary>
        /// Called when the window is about to close. 
        /// Remove the notification window from notification windows list and add one from the buffer list.
        /// </summary>

        static void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                var window = (Window)sender;
                if (notificationWindows.Count > 0 && notificationWindows.First().Window == window)
                {
                    WindowInfo windowInfo = notificationWindows.First();
                    notificationWindows.Remove(windowInfo);
                    if (notificationsBuffer != null && notificationsBuffer.Count > 0)
                    {
                        var BufferWindowInfo = notificationsBuffer.First();
                        StartWindowCloseTimer(BufferWindowInfo);

                        notificationWindows.Add(BufferWindowInfo);
                        BufferWindowInfo.Window.Show();
                        notificationsBuffer.Remove(BufferWindowInfo);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// Display the notification window in specified direction of the screen
        /// </summary>
        /// <param name="window"> The window object</param>
        /// <param name="notificationFlowDirection"> Direction in which new notifications will appear.</param>
        private static void SetWindowDirection(Window window, NotificationFlowDirection notificationFlowDirection)
        {
            try
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                var transform = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformFromDevice;
                var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                Window Owner = Application.Current.MainWindow;

                switch (notificationFlowDirection)
                {
                    case NotificationFlowDirection.RightBottom:
                        window.Left = corner.X - window.Width - window.Margin.Right - Margin;
                        window.Top = corner.Y - window.Height - window.Margin.Top;
                        break;
                    case NotificationFlowDirection.LeftBottom:
                        window.Left = 0;
                        window.Top = corner.Y - window.Height - window.Margin.Top;
                        break;
                    case NotificationFlowDirection.LeftUp:
                        window.Left = 0;
                        window.Top = 0;
                        break;
                    case NotificationFlowDirection.RightUp:
                        window.Left = corner.X - window.Width - window.Margin.Right - Margin;
                        window.Top = 0;
                        break;
                    case NotificationFlowDirection.Custom:
                        window.Left = Owner.Left + (Owner.Width / 2) - (window.Width / 2);
                        window.Top = Owner.Top;
                        break;
                    default:
                        window.Left = corner.X - window.Width - window.Margin.Right - Margin;
                        window.Top = corner.Y - window.Height - window.Margin.Top;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Window metadata.
        /// </summary>
        private sealed class WindowInfo
        {
            public int ID { get; set; }
            /// <summary>
            /// Gets or sets the display duration.
            /// </summary>
            /// <value>
            /// The display duration.
            /// </value>
            public TimeSpan DisplayDuration { get; set; }

            /// <summary>
            /// Gets or sets the window.
            /// </summary>
            /// <value>
            /// The window.
            /// </value>
            public Window Window { get; set; }
        }

        #endregion
    }
}
