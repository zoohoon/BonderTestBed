using System;

namespace PolishWaferDevMainPageViewModel.ManualSetting
{
    using LogModule;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using VirtualKeyboardControl;
    using ProberInterfaces;
    public class ManualSettingViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        private int _TotalSeqCount = 0;
        public int TotalSeqCount
        {
            get { return _TotalSeqCount; }
            set
            {
                if (value != _TotalSeqCount)
                {
                    _TotalSeqCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurSeq = 0;
        public int CurSeq
        {
            get { return _CurSeq; }
            set
            {
                if (value != _CurSeq)
                {
                    _CurSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ContactMoveLength = 0.0;
        public double ContactMoveLength
        {
            get { return _ContactMoveLength; }
            set
            {
                if (value != _ContactMoveLength)
                {
                    _ContactMoveLength = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Command & Command Method

        private RelayCommand<Object> _InputContactLengthCommand;
        public ICommand InputContactLengthCommand
        {
            get
            {
                if (null == _InputContactLengthCommand) _InputContactLengthCommand = new RelayCommand<Object>(InputContactLengthCommandFunc);
                return _InputContactLengthCommand;
            }
        }

        private void InputContactLengthCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();  
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion
    }
}
