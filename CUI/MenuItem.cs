using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CUI
{
    using ProberInterfaces;
    using System.Windows;
    using CUIServices;
    using System.Windows.Data;

    public class MenuItem : System.Windows.Controls.MenuItem, INotifyPropertyChanged, IFactoryModule, ICUIControl
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



        //private bool _Lockable;
        //public bool Lockable
        //{
        //    get { return _Lockable; }
        //    set
        //    {
        //        if (value != _Lockable)
        //        {
        //            _Lockable = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public static readonly DependencyProperty LockableProperty =
            DependencyProperty.Register("Lockable",
                                        typeof(bool), typeof(MenuItem),
                                        new FrameworkPropertyMetadata((bool)true));
        public bool Lockable
        {
            get { return (bool)this.GetValue(LockableProperty); }
            set { this.SetValue(LockableProperty, value); }
        }

        public static readonly DependencyProperty InnerLockableProperty =
    DependencyProperty.Register("InnerLockable",
                                typeof(bool), typeof(MenuItem),
                                new FrameworkPropertyMetadata((bool)true));
        public bool InnerLockable
        {
            get { return (bool)this.GetValue(InnerLockableProperty); }
            set { this.SetValue(InnerLockableProperty, value); }
        }



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

    }
}
