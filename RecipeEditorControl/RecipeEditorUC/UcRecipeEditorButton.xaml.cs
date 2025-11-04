using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RecipeEditorControl.RecipeEditorUC
{
    using System.Windows.Media.Effects;

    /// <summary>
    /// UcRecipeEditorButton.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcRecipeEditorButton : UserControl
    {
        #region ==> DEP BtnBackground
        public static readonly DependencyProperty BtnBackgroundProperty =
            DependencyProperty.Register(nameof(BtnBackground)
                , typeof(Brush),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public Brush BtnBackground
        {
            get { return (Brush)this.GetValue(BtnBackgroundProperty); }
            set { this.SetValue(BtnBackgroundProperty, value); }
        }
        #endregion

        #region ==> DEP BtnStroke
        public static readonly DependencyProperty BtnStrokeProperty =
            DependencyProperty.Register(nameof(BtnStroke)
                , typeof(Brush),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public Brush BtnStroke
        {
            get { return (Brush)this.GetValue(BtnStrokeProperty); }
            set { this.SetValue(BtnStrokeProperty, value); }
        }
        #endregion

        #region ==> DEP TextFontSize
        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(nameof(TextFontSize)
                , typeof(double),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public double TextFontSize
        {
            get { return (double)this.GetValue(TextFontSizeProperty); }
            set { this.SetValue(TextFontSizeProperty, value); }
        }
        #endregion

        #region ==> DEP BtnCaption
        public static readonly DependencyProperty BtnCaptionProperty =
            DependencyProperty.Register(nameof(BtnCaption)
                , typeof(String),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public String BtnCaption
        {
            get { return (String)this.GetValue(BtnCaptionProperty); }
            set { this.SetValue(BtnCaptionProperty, value); }
        }
        #endregion

        #region ==> DEP BtnCommand
        public static readonly DependencyProperty BtnCommandProperty =
            DependencyProperty.Register(nameof(BtnCommand)
                , typeof(ICommand),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public ICommand BtnCommand
        {
            get { return (ICommand)this.GetValue(BtnCommandProperty); }
            set { this.SetValue(BtnCommandProperty, value); }
        }
        #endregion

        #region ==> DEP ShadowDepth
        public static readonly DependencyProperty ShadowDepthProperty =
            DependencyProperty.Register(nameof(ShadowDepth)
                , typeof(double),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public double ShadowDepth
        {
            get { return (double)this.GetValue(ShadowDepthProperty); }
            set { this.SetValue(ShadowDepthProperty, value); }
        }
        #endregion

        #region ==> DEP BtnCommandParameter
        public static readonly DependencyProperty BtnCommandParameterProperty =
            DependencyProperty.Register(nameof(BtnCommandParameter)
                , typeof(Object),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public Object BtnCommandParameter
        {
            get { return (Object)this.GetValue(BtnCommandParameterProperty); }
            set { this.SetValue(BtnCommandParameterProperty, value); }
        }
        #endregion

        #region ==> DEP BtnForeground
        public static readonly DependencyProperty BtnForegroundProperty =
            DependencyProperty.Register(nameof(BtnForeground)
                , typeof(Brush),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public Brush BtnForeground
        {
            get { return (Brush)this.GetValue(BtnForegroundProperty); }
            set { this.SetValue(BtnForegroundProperty, value); }
        }
        #endregion

        #region ==> DEP BtnHorizontalAlignment
        public static readonly DependencyProperty BtnHorizontalAlignmentProperty =
            DependencyProperty.Register(nameof(BtnHorizontalAlignment)
                , typeof(HorizontalAlignment),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public HorizontalAlignment BtnHorizontalAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(BtnHorizontalAlignmentProperty); }
            set { this.SetValue(BtnHorizontalAlignmentProperty, value); }
        }
        #endregion

        #region ==> DEP BtnTextWrap
        public static readonly DependencyProperty BtnTextWrapProperty =
            DependencyProperty.Register(nameof(BtnTextWrap)
                , typeof(TextWrapping),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public TextWrapping BtnTextWrap
        {
            get { return (TextWrapping)this.GetValue(BtnTextWrapProperty); }
            set { this.SetValue(BtnTextWrapProperty, value); }
        }
        #endregion

        #region ==> DEP BtnContent
        public static readonly DependencyProperty BtnContentProperty =
            DependencyProperty.Register(nameof(BtnContent)
                , typeof(RecipeEditorUCViewModel),
                typeof(UcRecipeEditorButton),
                new FrameworkPropertyMetadata(null));
        public RecipeEditorUCViewModel BtnContent
        {
            get { return (RecipeEditorUCViewModel)this.GetValue(BtnContentProperty); }
            set { this.SetValue(BtnContentProperty, value); }
        }
        #endregion

        public UcRecipeEditorButton()
        {
            InitializeComponent();

            BtnBackground = Brushes.Black;
            BtnStroke = Brushes.White;
            TextFontSize = 16;
            ShadowDepth = 4;
            BtnForeground = Brushes.White;
            BtnHorizontalAlignment = HorizontalAlignment.Center;
            BtnTextWrap = TextWrapping.NoWrap;

            Binding bindBtnBackground = new Binding();
            bindBtnBackground.Path = new PropertyPath(nameof(BtnBackground));
            bindBtnBackground.Source = this;
            BindingOperations.SetBinding(editBtn, Border.BackgroundProperty, bindBtnBackground);

            Binding bindBtnStroke = new Binding();
            bindBtnStroke.Path = new PropertyPath(nameof(BtnStroke));
            bindBtnStroke.Source = this;
            BindingOperations.SetBinding(editBtn, Border.BorderBrushProperty, bindBtnStroke);

            Binding bindTextFontSize = new Binding();
            bindTextFontSize.Path = new PropertyPath(nameof(TextFontSize));
            bindTextFontSize.Source = this;
            BindingOperations.SetBinding(btnText, TextBlock.FontSizeProperty, bindTextFontSize);

            Binding bindBtnCaption = new Binding();
            bindBtnCaption.Path = new PropertyPath(nameof(BtnCaption));
            bindBtnCaption.Source = this;
            BindingOperations.SetBinding(btnText, TextBlock.TextProperty, bindBtnCaption);

            Binding bindBtnShadowDepth = new Binding();
            bindBtnShadowDepth.Path = new PropertyPath(nameof(ShadowDepth));
            bindBtnShadowDepth.Source = this;
            BindingOperations.SetBinding(btnImageEffect, DropShadowBitmapEffect.ShadowDepthProperty, bindBtnShadowDepth);

            Binding bindBtnForeground = new Binding();
            bindBtnForeground.Path = new PropertyPath(nameof(BtnForeground));
            bindBtnForeground.Source = this;
            BindingOperations.SetBinding(btnText, TextBlock.ForegroundProperty, bindBtnForeground);

            Binding bindHorizontalAlignment = new Binding();
            bindHorizontalAlignment.Path = new PropertyPath(nameof(BtnHorizontalAlignment));
            bindHorizontalAlignment.Source = this;
            BindingOperations.SetBinding(btnText, TextBlock.HorizontalAlignmentProperty, bindHorizontalAlignment);

            Binding bindBtnTextWrap = new Binding();
            bindBtnTextWrap.Path = new PropertyPath(nameof(BtnTextWrap));
            bindBtnTextWrap.Source = this;
            BindingOperations.SetBinding(btnText, TextBlock.TextWrappingProperty, bindBtnTextWrap);

            Binding bindBtnContent = new Binding();
            bindBtnContent.Path = new PropertyPath(nameof(BtnContent));
            bindBtnContent.Source = this;
            BindingOperations.SetBinding(btnContent, ContentControl.ContentProperty, bindBtnContent);

        }

        private void editBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (BtnCommand == null)
                return;

            editBtn.Opacity = 1;
        }

        private void editBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (BtnCommand == null)
                return;

            editBtn.Opacity = 1;
        }

        private void editBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BtnCommand == null)
                return;

            editBtn.Opacity = 0.5;
            BtnCommand.Execute(BtnCommandParameter);
        }
    }
}
