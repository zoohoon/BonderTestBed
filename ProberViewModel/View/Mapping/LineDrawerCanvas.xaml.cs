using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UcMappingView
{
    public partial class LineDrawerCanvas : UserControl
    {
        public LineDrawerCanvas()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register(nameof(CanvasWidth), typeof(double), typeof(LineDrawerCanvas),
            new PropertyMetadata(0d));

        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register(nameof(CanvasHeight), typeof(double), typeof(LineDrawerCanvas),
            new PropertyMetadata(0d));

        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register(nameof(MousePosition), typeof(Point), typeof(LineDrawerCanvas),
            new PropertyMetadata(default(Point)));

        public Point MousePosition
        {
            get { return (Point)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                CanvasWidth = this.ActualWidth;
                CanvasHeight = this.ActualHeight;
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                MousePosition = e.GetPosition(this);
            }
            catch(Exception err)
            {
                throw;
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
