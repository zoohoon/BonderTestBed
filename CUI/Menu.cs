using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CUI
{
    using LogModule;
    using ProberInterfaces;
    using System.Windows.Input;
    using CUIServices;
    using System.Windows.Data;

    public class Menu : System.Windows.Controls.Menu, INotifyPropertyChanged, IFactoryModule, ICUIControl
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

        private bool _Lockable;
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
                        if (this.ContextMenu.Visibility != System.Windows.Visibility.Visible)
                        {
                            this.ContextMenu.Visibility = System.Windows.Visibility.Visible;
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
    }
}
