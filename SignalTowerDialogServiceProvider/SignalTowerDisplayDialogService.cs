using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.DialogControl;
using ProberInterfaces.SignalTower;
using RelayCommandBase;
using SignalTowerModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SignalTowerDialogServiceProvider
{
    public class SignalTowerDisplayDialogService : INotifyPropertyChanged, ISignalTowerDisplayDialogService
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }        
        //private SignalTowerDisplayDialog SignalTowerDisplayDialog;

        public ISignalTowerManager SignalTowerManager { get; private set; }        
        public ISignalTowerController SignalTowerController { get; private set; }

        public bool Initialized { get; set; } = false;        

        public SignalTowerDisplayDialogService()
        {
            try
            {           
                SignalTowerManager = this.SignalTowerManager();                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }        

        private RelayCommand<object> _cmdOnEnqueueButtonClick;
        public ICommand cmdOnEnqueueButtonClick
        {
            get
            {
                if (null == _cmdOnEnqueueButtonClick) _cmdOnEnqueueButtonClick = new RelayCommand<object>(OnEnqueueButtonClick);
                return _cmdOnEnqueueButtonClick;
            }
        }

        private void OnEnqueueButtonClick(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                var value = paramArr[0];
                SignalTowerUnitBase unitbase = (SignalTowerUnitBase)value;
                //unitbase.OnQueue()
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _cmdOnDequeueButtonClick;
        public ICommand cmdOnDequeueButtonClick
        {
            get
            {
                if (null == _cmdOnDequeueButtonClick) _cmdOnDequeueButtonClick = new RelayCommand<object>(OnDequeueButtonClick);
                return _cmdOnDequeueButtonClick;
            }
        }

        private void OnDequeueButtonClick(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                var value = paramArr[0];
                SignalTowerUnitBase unitbase = (SignalTowerUnitBase)value;
                //SignalTowerController.OnQueue(unitbase);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _cmdBlinkEnqueueButtonClick;
        public ICommand cmdBlinkEnqueueButtonClick
        {
            get
            {
                if (null == _cmdBlinkEnqueueButtonClick) _cmdBlinkEnqueueButtonClick = new RelayCommand<object>(BlinkEnqueueButtonClick);
                return _cmdBlinkEnqueueButtonClick;
            }
        }

        private void BlinkEnqueueButtonClick(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                var value = paramArr[0];
                SignalTowerUnitBase unitbase = (SignalTowerUnitBase)value;
                //SignalTowerController.EnBlinkQueue(unitbase);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _cmdBlinkDequeueButtonClick;
        public ICommand cmdBlinkDequeueButtonClick
        {
            get
            {
                if (null == _cmdBlinkDequeueButtonClick) _cmdBlinkDequeueButtonClick = new RelayCommand<object>(BlinkDequeueButtonClick);
                return _cmdBlinkDequeueButtonClick;
            }
        }

        private void BlinkDequeueButtonClick(object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                var value = paramArr[0];
                SignalTowerUnitBase unitbase = (SignalTowerUnitBase)value;
                //SignalTowerController.DeBlinkQueue(unitbase);


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }    
}
