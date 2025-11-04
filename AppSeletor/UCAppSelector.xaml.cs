using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;

namespace AppSelector
{
    using CUIServices;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Interaction logic for UCAppSelector.xaml
    /// </summary>
    public partial class UCAppSelector : UserControl, ICUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public UCAppSelector()
        {
            InitializeComponent();

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

        public bool Lockable { get; set; } = true;
        public bool InnerLockable { get; set; } 
        public List<int> AvoidLockHashCodes { get; set; }

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

    }
}
