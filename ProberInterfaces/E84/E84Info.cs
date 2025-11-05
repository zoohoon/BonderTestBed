using ProberInterfaces.E84.ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.E84
{
    public class E84Info : INotifyPropertyChanged
    {
        #region // ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IE84Controller _E84Controller;
        public IE84Controller E84Controller
        {
            get { return _E84Controller; }
            set
            {
                if (value != _E84Controller)
                {
                    _E84Controller = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84OPModuleTypeEnum _E84OPModuleType;
        public E84OPModuleTypeEnum E84OPModuleType
        {
            get { return _E84OPModuleType; }
            set
            {
                if (value != _E84OPModuleType)
                {
                    _E84OPModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }


        public E84Info(int foupindex, IE84Controller controller, E84OPModuleTypeEnum optype)
        {
            this.FoupIndex = foupindex;
            this.E84Controller = controller;
            this.E84OPModuleType = optype;
        }

        public E84Info(int foupindex, IE84Controller controller, bool isconnected)
        {
            this.FoupIndex = foupindex;
            this.E84Controller = controller;
            this.IsConnected = isconnected;
        }
    }
}
