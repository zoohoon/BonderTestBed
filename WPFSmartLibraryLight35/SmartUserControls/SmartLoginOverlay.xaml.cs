//	--------------------------------------------------------------------
//		Member of the WPFSmartLibrary
//		For more information see : http://wpfsmartlibrary.codeplex.com/
//		(by DotNetMastermind)
//
//		filename		: SmartLoginOverlay.xaml.cs
//		namespace	: SoftArcs.WPFSmartLibrary.SmartUserControls
//		class(es)	: SmartLoginOverlay
//							
//	--------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using SoftArcs.WPFSmartLibrary.CommonHelper;
using VirtualKeyboardControl;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RelayCommandBase;

namespace SoftArcs.WPFSmartLibrary.SmartUserControls
{
    [TemplatePart(Name = "PART_TextBlockHint", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_RevealButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_SubmitButton", Type = typeof(CUI.Button))]
    public partial class SmartLoginOverlay : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region DependencyProperty - Watermark (type of "String")
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark",
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata("Enter password",
            (dpo, ea) =>
            {
                // ReSharper disable ConvertToLambdaExpression
                ((SmartLoginOverlay)dpo).OnWatermarkChanged((string)ea.OldValue, (string)ea.NewValue);
                // ReSharper restore ConvertToLambdaExpression
            }));
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
        private void OnWatermarkChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue && this.textBlockHint != null)
            {
                this.textBlockHint.Text = newValue;
            }
        }

        #endregion

        #region DependencyProperty - Password (type of "String")
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(SmartLoginOverlay),
            new FrameworkPropertyMetadata(
            default(string))
            {
                BindsTwoWayByDefault = true
            });
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        #endregion

        #region DependencyProperty - UserName (type of "String")
        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            nameof(UserName),
            typeof(string),
            typeof(SmartLoginOverlay),
            new FrameworkPropertyMetadata(
            default(string))
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }
        #endregion

        #region DependencyProperty - UserImageSource (type of "String")
        public static readonly DependencyProperty UserImageSourceProperty = DependencyProperty.Register(
            nameof(UserImageSource),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata(default(string),
            (dpo, ea) =>
            {
                // ReSharper disable ConvertToLambdaExpression
                ((SmartLoginOverlay)dpo).OnUserImageSourceChanged((string)ea.OldValue, (string)ea.NewValue);
                // ReSharper restore ConvertToLambdaExpression
            }));
        public string UserImageSource
        {
            get { return (string)GetValue(UserImageSourceProperty); }
            set { SetValue(UserImageSourceProperty, value); }
        }
        protected virtual void OnUserImageSourceChanged(string oldValue, string newValue)
        {
            if (oldValue == null && newValue == null || oldValue != null && oldValue.Equals(newValue)) return;

            try
            {
                if (newValue != String.Empty)
                {
                    Uri uri = new Uri(newValue, UriKind.RelativeOrAbsolute);

                    if (!uri.IsAbsoluteUri || (uri.IsAbsoluteUri && uri.IsFile))
                    {
                        this.imgUser.Source = new BitmapImage(uri);
                    }
                    else
                    {
                        // TODO => Should refactor the code one day to get the image asynchronously from the web
                        Debug.WriteLine("Loading an image from an URL path could take a long time ... so it will be bypassed");

                        // TEST
                        this.imgUser.Source = new BitmapImage(uri);
                    }
                }
                else
                {
                    this.imgUser.Source = new BitmapImage(new Uri("../CommonImages/UserSilhouette.png", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region DependencyProperty - IsUserOptionAvailable (type of "String")
        public static readonly DependencyProperty IsUserOptionAvailableProperty = DependencyProperty.Register(
            nameof(IsUserOptionAvailable),
            typeof(bool),
            typeof(SmartLoginOverlay),
            new PropertyMetadata(false,
            (dpo, ea) =>
            {
                // ReSharper disable ConvertToLambdaExpression
                ((SmartLoginOverlay)dpo).OnIsUserOptionAvailableChanged((bool)ea.OldValue, (bool)ea.NewValue);
                // ReSharper restore ConvertToLambdaExpression
            }));
        public bool IsUserOptionAvailable
        {
            get { return (bool)GetValue(IsUserOptionAvailableProperty); }
            set { SetValue(IsUserOptionAvailableProperty, value); }
        }

        protected virtual void OnIsUserOptionAvailableChanged(bool oldValue, bool newValue)
        {
            if (oldValue.Equals(newValue))
                return;

            if (newValue == true)
            {
                this.FaultMessagePanel.Margin = new Thickness(12, 40, 12, 5);
            }
            else
            {
                this.FaultMessagePanel.Margin = new Thickness(12, 18, 12, 5);
            }
        }

        #endregion

        #region DependencyProperty - AdditionalUserInfo (type of "String")
        public string AdditionalUserInfo
        {
            get { return (string)GetValue(AdditionalUserInfoProperty); }
            set { SetValue(AdditionalUserInfoProperty, value); }
        }
        public static readonly DependencyProperty AdditionalUserInfoProperty = DependencyProperty.Register(
            nameof(AdditionalUserInfo),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata(default(string)));
        #endregion

        #region DependencyProperty - AdditionalSystemInfo (type of "String")

        public static readonly DependencyProperty AdditionalSystemInfoProperty = DependencyProperty.Register(
            nameof(AdditionalSystemInfo),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata(default(string)));
        public string AdditionalSystemInfo
        {
            get { return (string)GetValue(AdditionalSystemInfoProperty); }
            set { SetValue(AdditionalSystemInfoProperty, value); }
        }
        #endregion

        #region DependencyProperty - SubmitButtonTooltip (type of "String")
        public static readonly DependencyProperty SubmitButtonTooltipProperty = DependencyProperty.Register(
            nameof(SubmitButtonTooltip),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata("Submit"));
        public string SubmitButtonTooltip
        {
            get { return (string)GetValue(SubmitButtonTooltipProperty); }
            set { SetValue(SubmitButtonTooltipProperty, value); }
        }
        #endregion

        #region DependencyProperty - SubmitButtonGUID (type of "Guid")
        public static readonly DependencyProperty SubmitButtonGUIDProperty = DependencyProperty.Register(
            nameof(SubmitButtonGUID),
            typeof(Guid),
            typeof(SmartLoginOverlay),
            new PropertyMetadata(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")));
        public Guid SubmitButtonGUID
        {
            get { return (Guid)GetValue(SubmitButtonGUIDProperty); }
            set { SetValue(SubmitButtonGUIDProperty, value); }
        }
        #endregion

        #region DependencyProperty - DisappearAnimation (type of "DisappearAnimationType")
        public static readonly DependencyProperty DisappearAnimationProperty = DependencyProperty.Register(
            nameof(DisappearAnimation),
            typeof(DisappearAnimationType),
            typeof(SmartLoginOverlay),
            new PropertyMetadata(DisappearAnimationType.MoveAndFadeOutToRight));
        public DisappearAnimationType DisappearAnimation
        {
            get { return (DisappearAnimationType)GetValue(DisappearAnimationProperty); }
            set { SetValue(DisappearAnimationProperty, value); }
        }
        #endregion

        #region DependencyProperty - CapsLockInfo (type of "String")
        public static readonly DependencyProperty CapsLockInfoProperty = DependencyProperty.Register(
            nameof(CapsLockInfo),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata("Caps Lock is active"));
        public string CapsLockInfo
        {
            get { return (string)GetValue(CapsLockInfoProperty); }
            set { SetValue(CapsLockInfoProperty, value); }
        }
        #endregion

        #region DependencyProperty - NoCredentialsInfo (type of "String")
        public static readonly DependencyProperty NoCredentialsInfoProperty = DependencyProperty.Register(
            nameof(NoCredentialsInfo),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata("Enter your credentials and try again."));
        public string NoCredentialsInfo
        {
            get { return (string)GetValue(NoCredentialsInfoProperty); }
            set { SetValue(NoCredentialsInfoProperty, value); }
        }
        #endregion

        #region DependencyProperty - WrongCredentialsInfo (type of "String")
        //public static readonly DependencyProperty WrongCredentialsInfoProperty = DependencyProperty.Register(
        //    nameof(WrongCredentialsInfo),
        //    typeof(string),
        //    typeof(SmartLoginOverlay),
        //    new PropertyMetadata("The password is incorrect. Make sure that you use the password for your account. You can reset the password at any time under 'myaccount.credentialserver.com/reset'."));

        public static readonly DependencyProperty WrongCredentialsInfoProperty = DependencyProperty.Register(
            nameof(WrongCredentialsInfo),
            typeof(string),
            typeof(SmartLoginOverlay),
            new PropertyMetadata("The password is incorrect."));

        public string WrongCredentialsInfo
        {
            get { return (string)GetValue(WrongCredentialsInfoProperty); }
            set { SetValue(WrongCredentialsInfoProperty, value); }
        }
        #endregion

        #region ==> UserNameLeftClickCommand
        private RelayCommand<object> _UserNameLeftClickCommand;
        public ICommand UserNameLeftClickCommand
        {
            get
            {
                if (null == _UserNameLeftClickCommand) _UserNameLeftClickCommand = new RelayCommand<object>(UserNameLeftClickCommandFunc);
                return _UserNameLeftClickCommand;
            }
        }
        private void UserNameLeftClickCommandFunc(object e)
        {
            string retVal = null;

            Window Owner = Application.Current.MainWindow;

            //int left = (int)((Owner.ActualWidth / 2) - (647 / 2));
            //int top = (int)((Owner.ActualHeight) - (400));
            //int left = (int)((Owner.ActualWidth / 2));
            //int top = (int)((Owner.ActualHeight)/2);

            retVal = VirtualKeyboard.Show(locationtype:WindowLocationType.BOTTOM);

            if (retVal != null)
            {
                this.UserName = retVal;
            }
        }
        #endregion

        #region ==> PasswordLeftClickCommand
        private RelayCommand<object> _PasswordLeftClickCommand;
        public ICommand PasswordLeftClickCommand
        {
            get
            {
                if (null == _PasswordLeftClickCommand) _PasswordLeftClickCommand = new RelayCommand<object>(PasswordLeftClickCommandFunc);
                return _PasswordLeftClickCommand;
            }
        }
        private void PasswordLeftClickCommandFunc(object e)
        {
            string retVal = null;

            Window Owner = Application.Current.MainWindow;

            //int left = (int)((Owner.ActualWidth / 2) - (647 / 2));
            //int top = (int)((Owner.ActualHeight) - (400));

            retVal = VirtualKeyboard.Show(kbType:KB_TYPE.PASSWORD, locationtype: WindowLocationType.BOTTOM);

            if (retVal != null)
            {
                Password = retVal;

                SubmitButton_Click(this, null);
            }
        }
        #endregion

        private TextBlock textBlockHint;
        private Brush savedBackgroundBrush;
        private DependencyObject visualRoot;
        public SmartLoginOverlay()
        {
            InitializeComponent();
        }
        private void SmartLoginOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.InitializeBaseClass(this);

                this.visualRoot = SmartVisualTreeHelper.FindAncestor<Window>(this);

                // The initialization of the "Dependency Properties" in the "Loaded Event" is very important for
                // the properly visualisation of the control in "Design Mode" (when it is loaded)
                this.SetFullSpan(this.FullSpan);

                this.Lock();
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public override void OnApplyTemplate()
        {
            // ReSharper disable JoinDeclarationAndInitializer
            try
            {
                object dpo;
                // ReSharper restore JoinDeclarationAndInitializer

                // This invoke is very important, because i have realized that it is not guaranteed that the PasswordBox
                // Styles and Templates are fully applied at this moment (maybe because they are within a Style ?!)
                PasswordBoxControl.ApplyTemplate();

                // Find the TextBlock in the template, so we are able to apply the watermark text
                dpo = PasswordBoxControl.Template.FindName("PART_TextBlockHint", PasswordBoxControl);
                if (dpo is TextBlock)
                {
                    textBlockHint = dpo as TextBlock;
                    textBlockHint.Text = Watermark;
                }

                // Connect the LostMouseCapture event, so we can set the focus to the PasswordBox if neccessary
                dpo = PasswordBoxControl.Template.FindName("PART_RevealButton", PasswordBoxControl);
                if (dpo is Button)
                {
                    (dpo as Button).LostMouseCapture += RevealButton_LostMouseCapture;
                }

                // Connect the Click event, so we can handle a mouseclick on the submit button
                dpo = PasswordBoxControl.Template.FindName("PART_SubmitButton", PasswordBoxControl);

                if (dpo is CUI.Button)
                {
                    //Button submitButton = dpo as Button;
                    CUI.Button submitButton = dpo as CUI.Button;

                    submitButton.ToolTip = this.SubmitButtonTooltip;
                    submitButton.Click += SubmitButton_Click;
                    submitButton.Visibility = false;
                    this.SubmitButtonGUID = submitButton.GUID;
                }

                base.OnApplyTemplate();
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void PasswordBoxControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return || e.Key == Key.Enter)
                {
                    this.SubmitButton_Click(sender, new RoutedEventArgs());
                    return;
                }

                this.lblCapsLockInfo.Visibility = Console.CapsLock ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception err)
            {
                throw;
            }
        }
        private void PasswordBoxControl_OnGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.lblCapsLockInfo.Visibility = Console.CapsLock ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception err)
            {
                throw;
            }
        }
        private void PasswordBoxControl_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.lblCapsLockInfo.Visibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                throw;
            }
        }
        void RevealButton_LostMouseCapture(object sender, MouseEventArgs e)
        {
            try
            {
                this.PasswordBoxControl.Focus();
            }
            catch (Exception err)
            {
                throw;
            }
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Password))
                {
                    ShowNoCredentialsMessage();
                    return;
                }

                if (Command != null && Command.CanExecute(CommandParameter))
                {
                    Command.Execute(CommandParameter);
                    return;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void btnOK_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.PasswordBoxControl.Visibility = Visibility.Visible;
                this.FaultMessagePanel.Visibility = Visibility.Hidden;

                this.PasswordBoxControl.Focus();
            }
            catch (Exception err)
            {
                throw;
            }
        }


        protected void assimilateBackground()
        {
            try
            {
                if (this.savedBackgroundBrush != null)
                {
                    this.Background = this.savedBackgroundBrush.Clone();
                    return;
                }

                if (this.savedBackgroundBrush == null && this.Background != null)
                {
                    this.savedBackgroundBrush = this.Background.Clone();
                    return;
                }

                if (this.visualRoot is Window)
                {
                    Window window = this.visualRoot as Window;

                    this.savedBackgroundBrush = window.Background.Clone();
                    this.Background = this.savedBackgroundBrush;
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        private void setAvailabilityOfParentChilds(bool isEnabled)
        {
            try
            {
                var panel = this.ParentElement as Panel;
                if (panel != null)
                {
                    foreach (var child in panel.Children)
                    {
                        if (!child.Equals(this))
                        {
                            // ReSharper disable PossibleNullReferenceException
                            (child as UIElement).IsEnabled = isEnabled;
                            // ReSharper restore PossibleNullReferenceException
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public void Unlock()
        {
            try
            {
                Storyboard storyboard;
                bool simultaneous = false;

                switch (DisappearAnimation)
                {
                    case DisappearAnimationType.FadeOut:
                        storyboard = null;
                        break;
                    case DisappearAnimationType.MoveAndFadeOutToRight:
                        storyboard = this.TryFindResource("MoveOutToRightStoryboard") as Storyboard;
                        break;
                    case DisappearAnimationType.MoveAndFadeOutToTop:
                        storyboard = this.TryFindResource("MoveOutToTopStoryboard") as Storyboard;
                        break;
                    case DisappearAnimationType.MoveAndFadeOutToRightSimultaneous:
                        storyboard = this.TryFindResource("MoveOutToRightSimultaneousStoryboard") as Storyboard;
                        simultaneous = true;
                        break;
                    case DisappearAnimationType.MoveAndFadeOutToTopSimultaneous:
                        storyboard = this.TryFindResource("MoveOutToTopSimultaneousStoryboard") as Storyboard;
                        simultaneous = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (storyboard != null) storyboard.Begin();

                this.setAvailabilityOfParentChilds(true);

                if (!simultaneous)
                {
                    storyboard = this.TryFindResource("FadeOutStoryboard") as Storyboard;

                    if (storyboard != null)
                    {
                        storyboard.Completed += (os, ea) =>
                        {
                            this.Visibility = Visibility.Hidden;
                            var panel = this.ParentElement as Panel;

                            if (panel != null)
                            {
                                panel.Opacity = 0.0;
                                DoubleAnimation doubleAnimation =
                                                        new DoubleAnimation()
                                                        {
                                                            From = 0.0,
                                                            To = 1.0,
                                                            Duration = new Duration(((KeyTime)this.TryFindResource("FadeInDurationKeyTime")).TimeSpan)
                                                        };
                                panel.BeginAnimation(OpacityProperty, doubleAnimation);
                            }
                        };
                    }
                }
                else
                {
                    storyboard = this.TryFindResource("FadeOutSimultaneousStoryboard") as Storyboard;
                }

                if (storyboard != null)
                {
                    storyboard.Begin();
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public void Lock()
        {
            try
            {
                this.BeginAnimation(OpacityProperty, null);
                this.BeginAnimation(BackgroundProperty, null);
                this.BeginAnimation(VisibilityProperty, null);
                this.VisualRootTranslateTransform.BeginAnimation(TranslateTransform.XProperty, null);
                this.VisualRootTranslateTransform.BeginAnimation(TranslateTransform.YProperty, null);

                this.LayoutRoot.BeginAnimation(OpacityProperty, null);
                this.LayoutRoot.BeginAnimation(VisibilityProperty, null);
                this.LayoutRootTranslateTransform.BeginAnimation(TranslateTransform.XProperty, null);
                this.LayoutRootTranslateTransform.BeginAnimation(TranslateTransform.YProperty, null);

                this.assimilateBackground();

                //this.setAvailabilityOfParentChilds( false );

                this.Visibility = Visibility.Visible;
                this.Password = String.Empty;

                if (this.IsUserOptionAvailable)
                {
                    //this.tbUserName.Focus();
                }
                else
                {
                    //this.PasswordBoxControl.Focus();
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public void ShowNoCredentialsMessage()
        {
            try
            {
                this.PasswordBoxControl.Visibility = Visibility.Collapsed;
                this.FaultMessagePanel.Visibility = Visibility.Visible;
                this.tblNoCredentialsMessage.Visibility = Visibility.Visible;
                this.tblWrongCredentialsMessage.Visibility = Visibility.Collapsed;

                this.btnOK.Focus();
            }
            catch (Exception err)
            {
                throw;
            }
        }
        public void ShowWrongCredentialsMessage()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.PasswordBoxControl.Visibility = Visibility.Collapsed;
                    this.FaultMessagePanel.Visibility = Visibility.Visible;
                    this.tblNoCredentialsMessage.Visibility = Visibility.Collapsed;
                    this.tblWrongCredentialsMessage.Visibility = Visibility.Visible;

                    this.btnOK.Focus();
                }));
            }
            catch (Exception err)
            {
                throw;
            }
        }
    }
    public enum DisappearAnimationType
    {
        FadeOut,
        MoveAndFadeOutToRight,
        MoveAndFadeOutToTop,
        MoveAndFadeOutToRightSimultaneous,
        MoveAndFadeOutToTopSimultaneous
    }
}
