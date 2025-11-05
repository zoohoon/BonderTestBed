using LogModule;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UcSideViewer
{
    /// <summary>
    /// ExpanderItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExpanderItem : UserControl
    {
        #region ==> Dependancy Properties
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(String), typeof(ExpanderItem), new FrameworkPropertyMetadata(null));
        public String Header
        {
            get { return (String)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(String), typeof(ExpanderItem), new FrameworkPropertyMetadata(null));
        public String Description
        {
            get { return (String)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register(nameof(HeaderFontSize), typeof(double), typeof(ExpanderItem), new PropertyMetadata((double)1));
        public double HeaderFontSize
        {
            get { return (double)GetValue(HeaderFontSizeProperty); }
            set { SetValue(HeaderFontSizeProperty, value); }
        }

        public static readonly DependencyProperty DescriptionFontSizeProperty =
            DependencyProperty.Register(nameof(DescriptionFontSize), typeof(double), typeof(ExpanderItem), new PropertyMetadata((double)1));
        public double DescriptionFontSize
        {
            get { return (double)GetValue(DescriptionFontSizeProperty); }
            set { SetValue(DescriptionFontSizeProperty, value); }
        }

        public static readonly DependencyProperty HeaderColorProperty =
            DependencyProperty.Register(nameof(HeaderColor), typeof(Brush), typeof(ExpanderItem), new FrameworkPropertyMetadata(null));
        public Brush HeaderColor
        {
            get { return (Brush)GetValue(HeaderColorProperty); }
            set { SetValue(HeaderColorProperty, value); }
        }

        public static readonly DependencyProperty ExpanderVisibilityProperty =
            DependencyProperty.Register(nameof(ExpanderVisibility), typeof(Visibility), typeof(ExpanderItem), new FrameworkPropertyMetadata(null));
        public Visibility ExpanderVisibility
        {
            get { return (Visibility)GetValue(ExpanderVisibilityProperty); }
            set { SetValue(ExpanderVisibilityProperty, value); }
        }

        #endregion
        
        public ExpanderItem()
        {
            InitializeComponent();

            try
            {
                #region ==> Bindings
                Binding bindHeader = new Binding();
                bindHeader.Path = new PropertyPath(nameof(Header));
                bindHeader.Source = this;
                BindingOperations.SetBinding(ExpanderHeader, Expander.HeaderProperty, bindHeader);
                
                Binding bindDescription = new Binding();
                bindDescription.Path = new PropertyPath(nameof(Description));
                bindDescription.Source = this;
                BindingOperations.SetBinding(ExpanderContents, TextBlock.TextProperty, bindDescription);

                HeaderFontSize = 1;
                Binding bindHeaderFontSize = new Binding();
                bindHeaderFontSize.Path = new PropertyPath(nameof(HeaderFontSize));
                bindHeaderFontSize.Source = this;
                BindingOperations.SetBinding(ExpanderHeader, Expander.FontSizeProperty, bindHeaderFontSize);

                DescriptionFontSize = 1;
                Binding bindDescriptionFontSize = new Binding();
                bindDescriptionFontSize.Path = new PropertyPath(nameof(DescriptionFontSize));
                bindDescriptionFontSize.Source = this;
                BindingOperations.SetBinding(ExpanderContents, TextBlock.FontSizeProperty, bindDescriptionFontSize);

                Binding bindHeaderColor = new Binding();
                bindHeaderColor.Path = new PropertyPath(nameof(HeaderColor));
                bindHeaderColor.Source = this;
                BindingOperations.SetBinding(ExpanderHeader, Expander.ForegroundProperty, bindHeaderColor);

                Binding bindExpanderVisibility = new Binding();
                bindExpanderVisibility.Path = new PropertyPath(nameof(ExpanderVisibility));
                bindExpanderVisibility.Source = this;
                BindingOperations.SetBinding(ExpanderHeader, Expander.VisibilityProperty, bindExpanderVisibility);
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[ExpanderItem] [ExpanderItem()] : {err}");
            }
        }
    }
}
