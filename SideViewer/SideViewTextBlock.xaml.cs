using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UcSideViewer
{
    /// <summary>
    /// SideViewTextBlock.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SideViewTextBlock : UserControl
    {
        #region ==> Dependancy Properties
        public static readonly DependencyProperty SideTextContentsProperty =
        DependencyProperty.Register(nameof(SideTextContents), typeof(String), typeof(SideViewTextBlock), new FrameworkPropertyMetadata(null));
        public String SideTextContents
        {
            get { return (String)GetValue(SideTextContentsProperty); }
            set { SetValue(SideTextContentsProperty, value); }
        }

        public static readonly DependencyProperty SideTextFontSizeProperty =
        DependencyProperty.Register(nameof(SideTextFontSize), typeof(double), typeof(SideViewTextBlock), new PropertyMetadata((double)1));
        public double SideTextFontSize
        {
            get { return (double)GetValue(SideTextFontSizeProperty); }
            set { SetValue(SideTextFontSizeProperty, value); }
        }

        public static readonly DependencyProperty SideTextFontColorProperty =
        DependencyProperty.Register(nameof(SideTextFontColor), typeof(Brush), typeof(SideViewTextBlock), new FrameworkPropertyMetadata(null));
        public Brush SideTextFontColor
        {
            get { return (Brush)GetValue(SideTextFontColorProperty); }
            set { SetValue(SideTextFontColorProperty, value); }
        }

        public static readonly DependencyProperty SideTextBackgroundProperty =
        DependencyProperty.Register(nameof(SideTextBackground), typeof(Brush), typeof(SideViewTextBlock), new FrameworkPropertyMetadata(null));
        public Brush SideTextBackground
        {
            get { return (Brush)GetValue(SideTextBackgroundProperty); }
            set { SetValue(SideTextBackgroundProperty, value); }
        }


        //public static readonly DependencyProperty SideTextVisibilityProperty =
        //DependencyProperty.Register(nameof(SideTextVisibility), typeof(Visibility), typeof(SideViewTextBlock), new FrameworkPropertyMetadata(null));
        //public Visibility SideTextVisibility
        //{
        //    get { return (Visibility)GetValue(SideTextVisibilityProperty); }
        //    set { SetValue(SideTextVisibilityProperty, value); }
        //}
        #endregion

        public SideViewTextBlock()
        {
            InitializeComponent();

            try
            {
                #region ==> Bindings
                Binding bindSideTextContents = new Binding();
                bindSideTextContents.Path = new PropertyPath("SideTextContents");
                bindSideTextContents.Source = this;
                BindingOperations.SetBinding(textblock, TextBlock.TextProperty, bindSideTextContents);

                //SideTextFontSize = 1;
                Binding bindSideTextFontSize = new Binding();
                bindSideTextFontSize.Path = new PropertyPath("SideTextFontSize");
                bindSideTextFontSize.Source = this;
                BindingOperations.SetBinding(textblock, TextBlock.FontSizeProperty, bindSideTextFontSize);

                Binding bindSideTextFontColor = new Binding();
                bindSideTextFontColor.Path = new PropertyPath("SideTextFontColor");
                bindSideTextFontColor.Source = this;
                BindingOperations.SetBinding(textblock, TextBlock.ForegroundProperty, bindSideTextFontColor);

                Binding bindSideTextBackground = new Binding();
                bindSideTextBackground.Path = new PropertyPath("SideTextBackground");
                bindSideTextBackground.Source = this;
                BindingOperations.SetBinding(textblock, TextBlock.BackgroundProperty, bindSideTextBackground);

                //Binding bindSideTextVisibility = new Binding();
                //bindSideTextVisibility.Path = new PropertyPath("SideTextVisibility");
                //bindSideTextVisibility.Source = this;
                //BindingOperations.SetBinding(textblock, TextBlock.VisibilityProperty, bindSideTextVisibility);
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[SideViewTextBlock] [SideViewTextBlock()] : {err}");
            }
        }
    }
}
