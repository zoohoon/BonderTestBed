using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
namespace ProberSimulator
{
    public class BoolAnimator : Viewport3D
    {
        // Create a custom routed event by first registering a RoutedEventID
        // This event uses the bubbling routing strategy
        public static readonly RoutedEvent WaferCamMiddleEvent = EventManager.RegisterRoutedEvent(
            nameof(WaferCamMiddle), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BoolAnimator));

        // Provide CLR accessors for the event
        public event RoutedEventHandler WaferCamMiddle
        {
            add { AddHandler(WaferCamMiddleEvent, value); }
            remove { RemoveHandler(WaferCamMiddleEvent, value); }
        }
        // This method raises the Tap event
        public void RaiseWaferCamMiddleEvent()
        {
            try
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(BoolAnimator.WaferCamMiddleEvent);
                RaiseEvent(newEventArgs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static readonly RoutedEvent WaferCamBackEvent = EventManager.RegisterRoutedEvent(
            nameof(WaferCamBack), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BoolAnimator));
        // Provide CLR accessors for the event
        public event RoutedEventHandler WaferCamBack
        {
            add { AddHandler(WaferCamBackEvent, value); }
            remove { RemoveHandler(WaferCamBackEvent, value); }
        }
        public void RaiseWaferCamBackEvent()
        {
            try
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(BoolAnimator.WaferCamBackEvent);
                RaiseEvent(newEventArgs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static readonly RoutedEvent DrawerOpenEvent = EventManager.RegisterRoutedEvent(
            nameof(DrawerOpen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BoolAnimator));
        // Provide CLR accessors for the event
        public event RoutedEventHandler DrawerOpen
        {
            add { AddHandler(DrawerOpenEvent, value); }
            remove { RemoveHandler(DrawerOpenEvent, value); }
        }
        public void RaiseDrawerOpenEvent()
        {
            try
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(BoolAnimator.DrawerOpenEvent);
                RaiseEvent(newEventArgs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public static readonly RoutedEvent DrawerCloseEvent = EventManager.RegisterRoutedEvent(
            nameof(DrawerClose), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BoolAnimator));
        // Provide CLR accessors for the event
        public event RoutedEventHandler DrawerClose
        {
            add { AddHandler(DrawerCloseEvent, value); }
            remove { RemoveHandler(DrawerCloseEvent, value); }
        }
        public void RaiseDrawerCloseEvent()
        {
            try
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(BoolAnimator.DrawerCloseEvent);
                RaiseEvent(newEventArgs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public bool IsWaferCamMoving
        {
            get { return (bool)this.GetValue(IsWaferCamMovingProperty); }
            set
            {
                this.SetValue(IsWaferCamMovingProperty, value);
            }
        }
        public static readonly DependencyProperty IsWaferCamMovingProperty =
                        DependencyProperty.Register(nameof(IsWaferCamMoving),
                        typeof(bool), typeof(BoolAnimator),
                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsWaferCamMovingChanged)));

        private static void OnIsWaferCamMovingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                BoolAnimator animateControl = sender as BoolAnimator;
                RoutedEventArgs newEventArgs = null;
                if (animateControl.IsWaferCamMoving == true)
                {
                    newEventArgs = new RoutedEventArgs(BoolAnimator.WaferCamBackEvent);
                }
                else
                {
                    newEventArgs = new RoutedEventArgs(BoolAnimator.WaferCamMiddleEvent);
                }
                animateControl.RaiseEvent(newEventArgs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool IsDrawerMoving
        {
            get { return (bool)this.GetValue(IsDrawerMovingProperty); }
            set
            {
                this.SetValue(IsDrawerMovingProperty, value);
            }
        }
        public static readonly DependencyProperty IsDrawerMovingProperty =
                        DependencyProperty.Register(nameof(IsDrawerMoving),
                        typeof(bool), typeof(BoolAnimator),
                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsDrawerMovingChanged)));

        private static void OnIsDrawerMovingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                BoolAnimator animateControl = sender as BoolAnimator;
                RoutedEventArgs newEventArgs = null;
                if (animateControl.IsDrawerMoving == true)
                {
                    newEventArgs = new RoutedEventArgs(BoolAnimator.DrawerCloseEvent);
                }
                else
                {
                    newEventArgs = new RoutedEventArgs(BoolAnimator.DrawerOpenEvent);
                }
                animateControl.RaiseEvent(newEventArgs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }







    }

}
