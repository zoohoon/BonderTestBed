using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LogModule;

namespace NeedleCleanPadModule.UC
{
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces;
    using RelayCommandBase;
    using SubstrateObjects;
    using System.ComponentModel;
    using VirtualKeyboardControl;

    /// <summary>
    /// UcNeedleCleanPad.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcNeedleCleanPad : CustomDialog, IFactoryModule, INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Error { get; set; }

        public string this[string columnName] => throw new NotImplementedException();

        public NeedleCleanObject NC { get { return this.StageSupervisor().NCObject as NeedleCleanObject; } }

        private float _PadWidth;
        public float PadWidth
        {
            get { return _PadWidth; }
            set
            {
                if (value != _PadWidth)
                {
                    _PadWidth = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private float _PadHeight;
        public float PadHeight
        {
            get { return _PadHeight; }
            set
            {
                if (value != _PadHeight)
                {
                    _PadHeight = value;
                    RaisePropertyChanged("");
                }
            }
        }



        private MetroWindow MetroWindow;
        public UcNeedleCleanPad()
        {
            try
            {
                this.DataContext = this;

                PadWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
                PadHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;

                InitializeComponent();
                MetroWindow = this.MetroDialogManager().GetMetroWindow() as MetroWindow;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await MetroWindow.HideMetroDialogAsync(this);

                NC.NCSysParam.NeedleCleanPadWidth.Value = PadWidth;
                NC.NCSysParam.NeedleCleanPadHeight.Value = PadHeight;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private async void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await MetroWindow.HideMetroDialogAsync(this);

                PadWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
                PadHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }


        private void TextBoxClickCommandFunc(Object param)
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
    }
}
