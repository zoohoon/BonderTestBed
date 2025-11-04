using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace UcCircularShapedJog
{
    using CUIServices;
    using LogModule;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    /// <summary>
    /// JogBtn.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JogBtn : UserControl, ICUIControl, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool _IsReleaseMode;
        public bool IsReleaseMode
        {
            get { return _IsReleaseMode; }
            set
            {
                if (value != _IsReleaseMode)
                {
                    _IsReleaseMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BindingBase _IsEnableBindingBase;
        public BindingBase IsEnableBindingBase
        {
            get { return _IsEnableBindingBase; }
            set
            {
                if (value != _IsEnableBindingBase)
                {
                    _IsEnableBindingBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> DEP IconSource
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource"
                , typeof(ImageSource),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(ImageSource)));
        public ImageSource IconSource
        {
            get { return (ImageSource)this.GetValue(IconSourceProperty); }
            set { this.SetValue(IconSourceProperty, value); }
        }
        #endregion

        #region ==> DEP MiniIconSource
        public static readonly DependencyProperty MiniIconSourceProperty =
            DependencyProperty.Register("MiniIconSource"
                , typeof(ImageSource),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(ImageSource)));
        public ImageSource MiniIconSource
        {
            get { return (ImageSource)this.GetValue(MiniIconSourceProperty); }
            set { this.SetValue(MiniIconSourceProperty, value); }
        }
        #endregion

        #region ==> DEP IconWidth
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth"
                , typeof(double),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(double)));
        public double IconWidth
        {
            get { return (double)this.GetValue(IconWidthProperty); }
            set { this.SetValue(IconWidthProperty, value); }
        }
        #endregion

        #region ==> DEP IconHeight
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight"
                , typeof(double),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(double)));
        public double IconHeight
        {
            get { return (double)this.GetValue(IconHeightProperty); }
            set { this.SetValue(IconHeightProperty, value); }
        }
        #endregion

        #region ==> DEP Softness
        public static readonly DependencyProperty SoftnessProperty =
            DependencyProperty.Register("Softness"
                , typeof(double),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(double)));
        public double Softness
        {
            get { return (double)this.GetValue(SoftnessProperty); }
            set { this.SetValue(SoftnessProperty, value); }
        }
        #endregion

        #region ==> DEP RepeatEnable
        public static readonly DependencyProperty RepeatEnableProperty =
            DependencyProperty.Register("RepeatEnable"
                , typeof(bool),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(bool)));
        public bool RepeatEnable
        {
            get { return (bool)this.GetValue(RepeatEnableProperty); }
            set { this.SetValue(RepeatEnableProperty, value); }
        }
        #endregion

        #region ==> DEP Caption
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption"
                , typeof(String),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(default(String)));
        public String Caption
        {
            get { return (String)this.GetValue(CaptionProperty); }
            set { this.SetValue(CaptionProperty, value); }
        }
        #endregion

        #region ==> DEP IconCaption
        public static readonly DependencyProperty IconCaptionProperty =
            DependencyProperty.Register("IconCaption"
                , typeof(String),
                typeof(JogBtn),
                new FrameworkPropertyMetadata(null));
        public String IconCaption
        {
            get { return (String)this.GetValue(IconCaptionProperty); }
            set { this.SetValue(IconCaptionProperty, value); }
        }
        #endregion

        #region ==> DEP CaptionSize
        public static readonly DependencyProperty CaptionSizeProperty =
            DependencyProperty.Register("CaptionSize"
                , typeof(double),
                typeof(JogBtn),
                new PropertyMetadata((double)1));
        public double CaptionSize
        {
            get { return (double)this.GetValue(CaptionSizeProperty); }
            set { this.SetValue(CaptionSizeProperty, value); }
        }
        #endregion

        public static readonly DependencyProperty LockableProperty =
           DependencyProperty.Register("Lockable",
                                       typeof(bool), typeof(JogBtn),
                                       new FrameworkPropertyMetadata((bool)true));
        public bool Lockable
        {
            get { return (bool)this.GetValue(LockableProperty); }
            set { this.SetValue(LockableProperty, value); }
        }

        public bool InnerLockable { get; set; }

        private List<int> _AvoidLockHashCodes;
        public List<int> AvoidLockHashCodes
        {
            get { return _AvoidLockHashCodes; }
            set
            {
                if (value != _AvoidLockHashCodes)
                {
                    _AvoidLockHashCodes = value;
                    RaisePropertyChanged();
                }
            }
        }




        public Guid GUID { get; set; }

        private int _MaskingLevel;
        public int MaskingLevel
        {
            get
            {
                _MaskingLevel = CUIService.GetMaskingLevel(this.GUID);
                return _MaskingLevel;
            }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public JogBtn()
        {
            try
            {
                InitializeComponent();

                Binding bindImage = new Binding();
                bindImage.Path = new PropertyPath("IconSource");
                bindImage.Source = this;
                BindingOperations.SetBinding(btnImage, Image.SourceProperty, bindImage);

                Binding bindImageWidth = new Binding();
                bindImageWidth.Path = new PropertyPath("IconWidth");
                bindImageWidth.Source = this;
                BindingOperations.SetBinding(btnImage, Image.WidthProperty, bindImageWidth);

                Binding bindImageHeight = new Binding();
                bindImageHeight.Path = new PropertyPath("IconHeight");
                bindImageHeight.Source = this;
                BindingOperations.SetBinding(btnImage, Image.HeightProperty, bindImageHeight);

                Binding bindImageSoftness = new Binding();
                bindImageSoftness.Path = new PropertyPath("Softness");
                bindImageSoftness.Source = this;
                BindingOperations.SetBinding(btnImageEffect, DropShadowBitmapEffect.SoftnessProperty, bindImageSoftness);

                Binding bindBtnText = new Binding();
                bindBtnText.Path = new PropertyPath("Caption");
                bindBtnText.Source = this;
                BindingOperations.SetBinding(captionTextBlock, TextBlock.TextProperty, bindBtnText);

                Binding bindIconCaption = new Binding();
                bindIconCaption.Path = new PropertyPath("IconCaption");
                bindIconCaption.Source = this;
                BindingOperations.SetBinding(IconText, TextBlock.TextProperty, bindIconCaption);

                CaptionSize = 1;
                Binding bindBtnFontSize = new Binding();
                bindBtnFontSize.Path = new PropertyPath("CaptionSize");
                bindBtnFontSize.Source = this;
                BindingOperations.SetBinding(captionTextBlock, TextBlock.FontSizeProperty, bindBtnFontSize);

                bindImage = new Binding();
                bindImage.Path = new PropertyPath("MiniIconSource");
                bindImage.Source = this;
                BindingOperations.SetBinding(btnMiniIcon, Image.SourceProperty, bindImage);

                bindImageWidth = new Binding();
                bindImageWidth.Path = new PropertyPath("IconWidth");
                bindImageWidth.Source = this;
                BindingOperations.SetBinding(btnMiniIcon, Image.WidthProperty, bindImageWidth);

                bindImageHeight = new Binding();
                bindImageHeight.Path = new PropertyPath("IconHeight");
                bindImageHeight.Source = this;
                BindingOperations.SetBinding(btnMiniIcon, Image.HeightProperty, bindImageHeight);

                bindImageSoftness = new Binding();
                bindImageSoftness.Path = new PropertyPath("Softness");
                bindImageSoftness.Source = this;
                BindingOperations.SetBinding(btnMiniIconEffect, DropShadowBitmapEffect.SoftnessProperty, bindImageSoftness);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mainGrid.Opacity = 0.5;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void mainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mainGrid.Opacity = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                mainGrid.Opacity = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }
}
