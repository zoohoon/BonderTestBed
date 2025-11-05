using ProberInterfaces.Foup;
using System;
using ProberErrorCode;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProberInterfaces;

namespace FoupModules.FoupOpener
{
    public abstract class FoupOpenerBase : IFoupCassetteOpener, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public FoupOpenerBase(IFoupModule module)
        {
            this.Module = module;
        }
        public FoupOpenerBase()
        {
        }
        public IFoupModule Module { get; set; }
        public IFoupIOStates FoupIOManager => Module.IOManager;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Inputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != _Inputs)
                {
                    _Inputs = value;
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _Outputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != _Outputs)
                {
                    _Outputs = value;
                }
            }
        }
        private FoupCassetteOpenerStateEnum _EnumState;
        public FoupCassetteOpenerStateEnum EnumState
        {
            get { return _EnumState; }
            set
            {
                _EnumState = value;
                RaisePropertyChanged();
            }
        }

        public abstract FoupCassetteOpenerStateEnum GetState();
        public abstract EventCodeEnum StateInit();
        public abstract EventCodeEnum CheckState();
        public abstract EventCodeEnum Lock();
        public abstract EventCodeEnum Unlock();

    }
}
