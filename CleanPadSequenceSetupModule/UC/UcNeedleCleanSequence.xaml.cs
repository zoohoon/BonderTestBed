using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CleanPadSequenceSetupModule.UC
{
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using VirtualKeyboardControl;
    using ProberInterfaces.NeedleClean;
    using NeedleCleanerModuleParameter;
    using SubstrateObjects;

    /// <summary>
    /// UcNeedleCleanSequence.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcNeedleCleanSequence : CustomDialog, IFactoryModule, INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return string.Empty; } }
        private bool ParamValidationFlag = true;
        public string this[string columnName]
        {
            get
            {
                if (columnName == "CleaningDistance" && (this.CleaningDistance <= 0))
                {
                    ParamValidationFlag = false;
                    return "Please enter it as a positive number.";
                }
                else
                    ParamValidationFlag = true;
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #region ==>Property       

        private long _CleaningDistance;
        public long CleaningDistance
        {
            get { return _CleaningDistance; }
            set
            {
                if (value != _CleaningDistance)
                {
                    _CleaningDistance = value;
                    NotifyPropertyChanged("CleaningDistance");
                }
            }
        }


        private int _CleaningCount;
        public int CleaningCount
        {
            get { return _CleaningCount; }
            set
            {
                if (value != _CleaningCount)
                {
                    _CleaningCount = value;
                    NotifyPropertyChanged("CleaningCount");
                }
            }
        }

        private NeedleCleanDeviceParameter _NeedleCleanerParam;
        public NeedleCleanDeviceParameter NeedleCleanerParam
        {
            get { return _NeedleCleanerParam; }
            set
            {
                if (value != _NeedleCleanerParam)
                {
                    _NeedleCleanerParam = value;
                    NotifyPropertyChanged("NeedleCleanerParam");
                }
            }
        }

        public NeedleCleanObject NC { get { return (NeedleCleanObject)this.StageSupervisor().NCObject; } }
        #endregion

        public UcNeedleCleanSequence(IStateModule param)
        {
            try
            {
                this.DataContext = this;

                var ncModule = (IHasDevParameterizable)param;
                //NeedleCleanerParam = (NeedleCleanDeviceParameter)ncModule.DevParam;
                NeedleCleanerParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;

                CleaningDistance = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value;
                CleaningCount = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value;

                InitializeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public UcNeedleCleanSequence(NeedleCleanDeviceParameter param)
        {
            try
            {

                this.DataContext = this;
                NeedleCleanerParam = (NeedleCleanDeviceParameter)param;

                CleaningDistance = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value;
                CleaningCount = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value;

                InitializeComponent();
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
                if (!ParamValidationFlag)
                    return;

                NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value = CleaningDistance;
                NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value = CleaningCount;

                await this.MetroDialogManager().CloseWindow(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(this);

                CleaningDistance = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value;
                CleaningCount = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
