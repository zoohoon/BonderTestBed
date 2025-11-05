using System;
using LogModule;

namespace BinEditorControl
{
    using System.Windows.Input;
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Windows;
    using System.Runtime.CompilerServices;

    public class BinItemViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> BinTitle
        private String _BinTitle;
        public String BinTitle
        {
            get { return _BinTitle; }
            set
            {
                if (value != _BinTitle)
                {
                    _BinTitle = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> BinContent
        private String _BinContent;
        public String BinContent
        {
            get { return _BinContent; }
            set
            {
                if (value != _BinContent)
                {
                    _BinContent = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> BinContentColor
        private Brush _BinContentColor;
        public Brush BinContentColor
        {
            get { return _BinContentColor; }
            set
            {
                if (value != _BinContentColor)
                {
                    _BinContentColor = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ContentDownCommand
        public ICommand ContentDownCommand { get; set; }
        #endregion

        #region ==> RecordVisibility
        private Visibility _RecordVisibility;
        public Visibility RecordVisibility
        {
            get { return _RecordVisibility; }
            set
            {
                if (value != _RecordVisibility)
                {
                    _RecordVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public BinDataFormat BinData { get; set; }

        public BinItemViewModel()
        {
            try
            {
                _BinTitle = "BIN";
                _BinContent = String.Empty;
                _BinContentColor = Brushes.LightGray;
                _RecordVisibility = Visibility.Visible;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
