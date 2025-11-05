using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AccountModule;

namespace CUI
{
    using LogModule;
    using ProberInterfaces;
    using System.Windows;
    using System.Windows.Input;
    using CUIServices;
    using System.Windows.Data;

    public class Button : System.Windows.Controls.Button, INotifyPropertyChanged, IFactoryModule, ICUIControl
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        #region // Override base.IsEnabled property with dependency property.

        public new bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static new readonly DependencyProperty IsEnabledProperty =
           DependencyProperty.Register("IsEnabled", typeof(bool), typeof(CUI.Button),
           new PropertyMetadata(true, OnIsEnabledChanged));

        public static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as CUI.Button;
            //if (control.Content != null && control.Content.ToString() == "Wafer")
            //{

            //}
            if (control != null)
            {
                control.UpdateIsEnabled();
            }
        }
        
        public bool IsPrivilegeEnabled
        {
            get { return (bool)GetValue(IsPrivilegeEnabledProperty); }
            set { SetValue(IsPrivilegeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsPrivilegeEnabledProperty =
            DependencyProperty.Register("IsPrivilegeEnabled", typeof(bool), typeof(CUI.Button),
            new PropertyMetadata(true, OnIsPrivilegeEnabledChanged));

        public static void OnIsPrivilegeEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as CUI.Button;

            //if (control.Content != null && control.Content.ToString() == "Wafer")
            //{

            //}

            if (control != null)
            {
                control.UpdateIsEnabled();
            }
        }

        public void UpdateIsEnabled()
        {
            var enabled = IsPrivilegeEnabled && IsEnabled;
            //base.IsEnabled = enabled;
            SetCurrentValue(System.Windows.Controls.Button.IsEnabledProperty, enabled);
        }
        #endregion
        private bool _Lockable = true;
        public bool Lockable
        {
            get { return _Lockable; }
            set
            {
                if (value != _Lockable)
                {
                    _Lockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _InnerLockable;
        public bool InnerLockable
        {
            get { return _InnerLockable; }
            set
            {
                if (value != _InnerLockable)
                {
                    _InnerLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        public List<int> AvoidLockHashCodes { get; set; } = new List<int>();

        //public static readonly DependencyProperty AvoidLockHashCodesProperty =
        //    DependencyProperty.Register("AvoidLockHashCodes",
        //                                typeof(List<int>), typeof(Button),
        //                                new FrameworkPropertyMetadata(new List<int>()));
        //public List<int> AvoidLockHashCodes
        //{
        //    get { return (List<int>)this.GetValue(AvoidLockHashCodesProperty); }
        //    set { this.SetValue(AvoidLockHashCodesProperty, value); }
        //}




        //private ObservableCollection<int> _AvoidLockHashCodes;
        //public ObservableCollection<int> AvoidLockHashCodes
        //{
        //    get { return _AvoidLockHashCodes; }
        //    set
        //    {
        //        if (value != _AvoidLockHashCodes)
        //        {
        //            _AvoidLockHashCodes = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}



        private bool _Visibility;
        public new bool Visibility
        {
            get
            {
                _Visibility = CUIService.GetVisibility(this.GUID);
                return _Visibility;
            }
            set
            {
                if (value != _Visibility)
                {

                    _Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            try
            {
                if (this.ViewModelManager().HelpQuestionEnable)
                {
                    if (this.ContextMenu != null)
                    {
                        this.ContextMenu.Visibility = System.Windows.Visibility.Hidden;
                    }

                    CUIService.ShowToolTip(this);
                }
                else
                {
                    if (this.ContextMenu != null)
                    {
                        if (AccountManager.MaskingLevelCollection != null)
                        {
                            if (AccountManager.IsUserLevelAboveThisNum(AccountManager.AdminUserLevel))
                            {
                                if (this.ContextMenu.Visibility != System.Windows.Visibility.Visible)
                                {
                                    this.ContextMenu.Visibility = System.Windows.Visibility.Visible;
                                }
                            }
                            else
                            {
                                this.ContextMenu.Visibility = System.Windows.Visibility.Hidden;
                            }
                        }
                    }
                }

                base.OnMouseRightButtonDown(e);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected override void OnClick()
        {
            try
            {
                String log = $"Button GUID:{GUID.ToString()} | OnClick";
                LoggerManager.Debug(log);

                base.OnClick();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void OnClickEventRaise()
        {
            try
            {
                OnClick();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class RepeatButton : System.Windows.Controls.Primitives.RepeatButton, INotifyPropertyChanged, IFactoryModule, ICUIControl
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        private bool _Lockable = true;
        public bool Lockable
        {
            get { return _Lockable; }
            set
            {
                if (value != _Lockable)
                {
                    _Lockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _InnerLockable;
        public bool InnerLockable
        {
            get { return _InnerLockable; }
            set
            {
                if (value != _InnerLockable)
                {
                    _InnerLockable = value;
                    RaisePropertyChanged();
                }
            }
        }



        public List<int> AvoidLockHashCodes { get; set; } = new List<int>();



        private bool _Visibility;
        public new bool Visibility
        {
            get
            {
                _Visibility = CUIService.GetVisibility(this.GUID);
                return _Visibility;
            }
            set
            {
                if (value != _Visibility)
                {

                    _Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            try
            {
                if (this.ViewModelManager().HelpQuestionEnable)
                {
                    if (this.ContextMenu != null)
                    {
                        this.ContextMenu.Visibility = System.Windows.Visibility.Hidden;
                    }

                    CUIService.ShowToolTip(this);
                }
                else
                {
                    if (this.ContextMenu != null)
                    {
                        if (AccountManager.MaskingLevelCollection != null)
                        {
                            if (AccountManager.IsUserLevelAboveThisNum(AccountManager.AdminUserLevel))
                            {
                                if (this.ContextMenu.Visibility != System.Windows.Visibility.Visible)
                                {
                                    this.ContextMenu.Visibility = System.Windows.Visibility.Visible;
                                }
                            }
                            else
                            {
                                this.ContextMenu.Visibility = System.Windows.Visibility.Hidden;
                            }
                        }
                    }
                }

                base.OnMouseRightButtonDown(e);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected override void OnClick()
        {
            try
            {
                String log = $"Button GUID:{GUID.ToString()} | OnClick";
                LoggerManager.Debug(log);

                base.OnClick();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void OnClickEventRaise()
        {
            OnClick();
        }

        //public void ToggleReleaseMode()
        //{
        //    try
        //    {
        //        IsReleaseMode ^= true;

        //        if (IsReleaseMode == false)
        //        {
        //            if (EnableBinding != null)
        //            {
        //                this.SetBinding(UIElement.IsEnabledProperty, EnableBinding);
        //            }
        //            ApplyMasking();
        //        }
        //        else
        //        {
        //            if (EnableBinding == null)
        //            {
        //                BindingExpression bindingExpression = this.GetBindingExpression(UIElement.IsEnabledProperty);

        //                if (bindingExpression != null)
        //                {
        //                    EnableBinding = bindingExpression.ParentBinding;
        //                }
        //            }

        //            this.IsEnabled = true;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //public void ApplyMasking()
        //{
        //    try
        //    {
        //        if (this.MaskingLevel <= AccountManager.CurrentUserInfo.UserLevel)
        //        {
        //            if (EnableBinding != null)
        //            {
        //                this.SetBinding(Button.IsEnabledProperty, EnableBinding);
        //            }
        //        }
        //        else
        //        {
        //            if (EnableBinding == null)
        //            {
        //                BindingExpression bindingExpression = this.GetBindingExpression(Button.IsEnabledProperty);

        //                if (bindingExpression != null)
        //                {
        //                    EnableBinding = bindingExpression.ParentBinding;
        //                }
        //            }
        //            this.IsEnabled = false;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
    }
}
