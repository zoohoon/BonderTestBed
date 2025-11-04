using AccountModule;
using CUIServices;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CUI
{
    public class ComboBox : System.Windows.Controls.ComboBox, INotifyPropertyChanged, IFactoryModule, ICUIControl
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region ==> ICUIControl
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
        #endregion

        #region ==> ICUIControl

        protected override void OnDropDownOpened(EventArgs e)
        {
            try
            {
                LoggerManager.Debug($"ComboBox GUID:{GUID.ToString()} | OnClick");
                base.OnDropDownOpened(e);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void OnClickEventRaise(EventArgs e)
        {
            try
            {
                OnDropDownOpened(e);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion
        #region // Override base.IsEnabled property with dependency property.

        public new bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static new readonly DependencyProperty IsEnabledProperty =
           DependencyProperty.Register("IsEnabled", typeof(bool), typeof(CUI.ComboBox),
           new PropertyMetadata(true, OnIsEnabledChanged));

        public static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as CUI.ComboBox;
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
            DependencyProperty.Register("IsPrivilegeEnabled", typeof(bool), typeof(CUI.ComboBox),
            new PropertyMetadata(true, OnIsPrivilegeEnabledChanged));

        public static void OnIsPrivilegeEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as CUI.ComboBox;
            if (control != null)
            {
                control.UpdateIsEnabled();
            }
        }

        public void UpdateIsEnabled()
        {
            var enabled = IsPrivilegeEnabled && IsEnabled;
            SetCurrentValue(System.Windows.Controls.ComboBox.IsEnabledProperty, enabled);
        }
        #endregion

    }
}
