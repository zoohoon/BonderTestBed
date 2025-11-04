using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using RelayCommandBase;
using System.Windows.Input;
using LogModule;
using VirtualKeyboardControl;
using System.Windows.Controls;

namespace GpibSettingVM
{
    public class GpibSettingViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("C3FF1695-274B-4075-8C0F-19172FC56506");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private IGPIB _GPIB;
        public IGPIB GPIB
        {
            get { return _GPIB; }
            set { _GPIB = value; }
        }

        private EnumGpibEnable _GPIBEnable;
        public EnumGpibEnable GPIBEnable
        {
            get { return _GPIBEnable; }
            set
            {
                if (value != _GPIBEnable)
                {
                    _GPIBEnable = value;

                    ChangedGPIBEnable();

                    RaisePropertyChanged();
                }
            }
        }


        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    GPIB = this.GPIB();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void ChangedGPIBEnable()
        {
            try
            {
                if (GPIBEnable == EnumGpibEnable.DISABLE)
                {
                    this.SequenceEngineManager().StopSequence(this.GPIB() as ISequenceEngineService);
                }
                else if (GPIBEnable == EnumGpibEnable.ENABLE)
                {
                    this.SequenceEngineManager().RunSequence(this.GPIB() as ISequenceEngineService, "GPIB");
                }

                this.GPIB().SetGPIBEnable(GPIBEnable);
                this.GPIB().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            GPIBEnable = GPIB.GetGPIBEnable();

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = GPIB.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public EventCodeEnum CheckParameterToSave()
        {
            return EventCodeEnum.NONE;
        }

        private RelayCommand _ReInitializeCommand;
        public ICommand ReInitializeCommand
        {
            get
            {
                if (null == _ReInitializeCommand) _ReInitializeCommand = new RelayCommand(ReInitializeFunc);
                return _ReInitializeCommand;
            }
        }
        private void ReInitializeFunc()
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                retVal = GPIB.ReInitializeAndConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //

        private RelayCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new RelayCommand<Object>(FuncDecimalTextBoxClickCommand);
                return _DecimalTextBoxClickCommand;
            }
        }

        private void FuncDecimalTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                int oldvalue = Convert.ToInt32(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);

                int newvalue = Convert.ToInt32(tb.Text);

                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                if (oldvalue != newvalue)
                {
                    SaveParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _StringTextBoxClickCommand;
        public ICommand StringTextBoxClickCommand
        {
            get
            {
                if (null == _StringTextBoxClickCommand) _StringTextBoxClickCommand = new RelayCommand<Object>(FuncStringTextBoxClickCommand);
                return _StringTextBoxClickCommand;
            }
        }

        private void FuncStringTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                string oldvalue = Convert.ToString(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.ALPHABET);

                string newvalue = Convert.ToString(tb.Text);

                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                if (oldvalue != newvalue)
                {
                    SaveParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _SelectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get
            {
                if (null == _SelectionChangedCommand) _SelectionChangedCommand = new RelayCommand<object>(SelectionChangedCommandFunc);
                return _SelectionChangedCommand;
            }
        }

        private void SelectionChangedCommandFunc(object param)
        {
            try
            {
                string selectedtypeStr = string.Empty;

                ComboBox combobox = (param as ComboBox);

                Type EnumType = null;

                if ((combobox).ItemsSource != null)
                {
                    // Enum type

                    //var enumerator = (combobox).ItemsSource.GetEnumerator();

                    //if(enumerator.MoveNext() == true)
                    //{
                    //    EnumType = enumerator.Current.GetType();
                    //}
                    EnumType = combobox.SelectedItem.GetType();

                    selectedtypeStr = EnumType.ToString();
                }
                else
                {
                    // Boolean type
                    selectedtypeStr = (combobox).SelectedValue?.GetType().ToString();
                }

                string changedvalStr = (combobox).SelectedItem.ToString();

                if (EnumType != null)
                {
                    //if (EnumType == typeof(GROUPING_METHOD))
                    //{
                    //    // Grouping Method�� ���� ��.
                    //    // Grouping Done �÷��׸� ������.

                    //    foreach (var TableTemplateInfo in this.PMIInfo.PadTableTemplateInfo)
                    //    {
                    //        TableTemplateInfo.GroupingDone.Value = false;
                    //    }
                    //}
                }

                EventCodeEnum retval = SaveParameter();

                LoggerManager.Debug($"[GpibSettingViewModel] SelectionChangedCommandFunc() : Enum Name : {selectedtypeStr}, Changed Value : {changedvalStr}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
