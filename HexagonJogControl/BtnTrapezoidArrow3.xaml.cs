using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HexagonJogControl
{
    /// <summary>
    /// BtnTrapezoidArrow3.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BtnTrapezoidArrow3 : UserControl
    {
        #region ==> DEP BtnCaption
        public static readonly DependencyProperty BtnCaptionProperty =
            DependencyProperty.Register(nameof(BtnCaption)
                , typeof(String),
                typeof(BtnTrapezoidArrow3),
                new FrameworkPropertyMetadata("M7,15L12,10L17,15H7Z", null));
        public String BtnCaption
        {
            get { return (String)this.GetValue(BtnCaptionProperty); }
            set { this.SetValue(BtnCaptionProperty, value); }
        }
        #endregion

        #region ==> DEP BtnCaptionWidth
        public static readonly DependencyProperty BtnCaptionWidthProperty =
            DependencyProperty.Register(nameof(BtnCaptionWidth)
                , typeof(float),
                typeof(BtnTrapezoidArrow3),
                new FrameworkPropertyMetadata(36.0f, null));
        public float BtnCaptionWidth
        {
            get { return (float)this.GetValue(BtnCaptionWidthProperty); }
            set { this.SetValue(BtnCaptionWidthProperty, value); }
        }
        #endregion

        #region ==> DEP BtnCaptionAngle
        public static readonly DependencyProperty BtnCaptionAngleProperty =
            DependencyProperty.Register(nameof(BtnCaptionAngle)
                , typeof(float),
                typeof(BtnTrapezoidArrow3),
                new FrameworkPropertyMetadata(45.0f, null));
        public float BtnCaptionAngle
        {
            get { return (float)this.GetValue(BtnCaptionAngleProperty); }
            set { this.SetValue(BtnCaptionAngleProperty, value); }
        }
        #endregion

        #region ==> DEP BtnForeground
        public static readonly DependencyProperty BtnForegroundProperty =
            DependencyProperty.Register(nameof(BtnForeground)
                , typeof(Brush),
                typeof(BtnTrapezoidArrow3),
                new FrameworkPropertyMetadata(Brushes.White, null));
        public Brush BtnForeground
        {
            get { return (Brush)this.GetValue(BtnForegroundProperty); }
            set { this.SetValue(BtnForegroundProperty, value); }
        }
        #endregion

        public BtnTrapezoidArrow3()
        {
            InitializeComponent();

            Binding bindBtnCaption = new Binding();
            bindBtnCaption.Path = new PropertyPath(nameof(BtnCaption));
            bindBtnCaption.Source = this;
            BindingOperations.SetBinding(ContentData, Path.DataProperty, bindBtnCaption);

            Binding bindBtnCaptionWidth = new Binding();
            bindBtnCaptionWidth.Path = new PropertyPath(nameof(BtnCaptionWidth));
            bindBtnCaptionWidth.Source = this;
            BindingOperations.SetBinding(ContentView, Viewbox.WidthProperty, bindBtnCaptionWidth);
            BindingOperations.SetBinding(ContentView, Viewbox.HeightProperty, bindBtnCaptionWidth);

            Binding bindBtnCaptionAngle = new Binding();
            bindBtnCaptionAngle.Path = new PropertyPath(nameof(BtnCaptionAngle));
            bindBtnCaptionAngle.Source = this;
            BindingOperations.SetBinding(ContentRotater, RotateTransform.AngleProperty, bindBtnCaptionAngle);

            Binding bindBtnForeground = new Binding();
            bindBtnForeground.Path = new PropertyPath(nameof(BtnForeground));
            bindBtnForeground.Source = this;
            BindingOperations.SetBinding(ContentData, Path.FillProperty, bindBtnForeground);
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainGrid1.Opacity = 0.5;
            mainGrid2.Opacity = 0.5;
            mainGrid3.Opacity = 0.5;
        }

        private void mainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mainGrid1.Opacity = 1;
            mainGrid2.Opacity = 1;
            mainGrid3.Opacity = 1;
        }

        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            mainGrid1.Opacity = 1;
            mainGrid2.Opacity = 1;
            mainGrid3.Opacity = 1;
        }
    }
}
