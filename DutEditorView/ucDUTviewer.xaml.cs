using CUIServices;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;

namespace ucDutViewer
{
    public partial class ucDUTviewer : UserControl, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        public ucDUTviewer()
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

        public bool Lockable { get; set; } = true;
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }
        public bool IsReleaseMode { get; set; }
        public BindingBase IsEnableBindingBase { get; set; }
    }
}
