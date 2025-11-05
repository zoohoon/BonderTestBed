using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ProberErrorCode;
using ProberInterfaces;

namespace WaferSelectSetup
{
    public abstract class WaferSelectSetupBase : IWaferSelectSetup, INotifyPropertyChanged
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<ButtonDescriptor> _WaferSelectBtn = new ObservableCollection<ButtonDescriptor>();
        public ObservableCollection<ButtonDescriptor> WaferSelectBtn
        {
            get { return _WaferSelectBtn; }
            set { _WaferSelectBtn = value; }
        }
        
        private ButtonDescriptor _SelectAllBtn = new ButtonDescriptor();
        public ButtonDescriptor SelectAllBtn
        {
            get { return _SelectAllBtn; }
            set { _SelectAllBtn = value; }
        }

        private ButtonDescriptor _ClearAllBtn = new ButtonDescriptor();
        public ButtonDescriptor ClearAllBtn
        {
            get { return _ClearAllBtn; }
            set { _ClearAllBtn = value; }
        }
    }
}
