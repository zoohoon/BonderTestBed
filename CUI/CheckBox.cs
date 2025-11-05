using AccountModule;
using CUIServices;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

namespace CUI
{
    public class CheckBox : System.Windows.Controls.CheckBox, INotifyPropertyChanged, IFactoryModule, ICUIControl
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
                String log = $"CheckBox GUID:{GUID.ToString()} | OnClick";
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
}
