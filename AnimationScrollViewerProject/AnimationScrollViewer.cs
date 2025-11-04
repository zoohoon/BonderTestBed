using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;

namespace UcAnimationScrollViewer
{

    public class AnimationScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty CurrentVerticalOffsetProperty =
            DependencyProperty.Register(nameof(CurrentVerticalOffset), typeof(double), typeof(AnimationScrollViewer),
                new PropertyMetadata(new PropertyChangedCallback(OnVerticalChanged)));

        public static readonly DependencyProperty CurrentHorizontalOffsetProperty =
            DependencyProperty.Register(nameof(CurrentHorizontalOffset), typeof(double), typeof(AnimationScrollViewer),
                new PropertyMetadata(new PropertyChangedCallback(OnHorizontalChanged)));

        public double CurrentVerticalOffset
        {
            get { return (double)GetValue(CurrentVerticalOffsetProperty); }
            set { SetValue(CurrentVerticalOffsetProperty, value); }
        }

        public double CurrentHorizontalOffset
        {
            get { return (double)GetValue(CurrentHorizontalOffsetProperty); }
            set { SetValue(CurrentHorizontalOffsetProperty, value); }
        }

        private static void OnVerticalChanged(DependencyObject property, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                AnimationScrollViewer viewer = property as AnimationScrollViewer;
                viewer.ScrollToVerticalOffset((double)e.NewValue);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static void OnHorizontalChanged(DependencyObject property, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                AnimationScrollViewer viewer = property as AnimationScrollViewer;
                viewer.ScrollToHorizontalOffset((double)e.NewValue);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
