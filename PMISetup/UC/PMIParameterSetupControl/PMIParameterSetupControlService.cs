using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogModule;
using PMIModuleParameter;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using RelayCommandBase;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VirtualKeyboardControl;
using SerializerUtil;
using SubstrateObjects;
using System.Windows.Controls;

namespace PMISetup.UC
{

    public class PMIParameterSetupControlService : INotifyPropertyChanged, IFactoryModule, IPnpAdvanceSetupViewModel
    {
        #region == > PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PMIParameterSetupControl DialogControl { get; set; }
        //private bool bChanged = false;
        //private PMIModuleDevParam ModuleParam;
        private IParam BackupParam { get; set; }

        private PMIInfo _PMIInfo;
        public PMIInfo PMIInfo
        {
            get { return _PMIInfo; }
            set
            {
                if (value != _PMIInfo)
                {
                    _PMIInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMIModuleDevParam _PMIDevParm;
        public PMIModuleDevParam PMIDevParm
        {
            get { return _PMIDevParm; }
            set
            {
                if (value != _PMIDevParm)
                {
                    _PMIDevParm = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _GPVisibility;
        public Visibility GPVisibility
        {
            get { return _GPVisibility; }
            set
            {
                if (value != _GPVisibility)
                {
                    _GPVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private IParam OriginalDevParam
        //{
        //    get { return this.PMIModule().PMIModuleDevParam_IParam; }
        //}

        //private PMIParameterSettingModule SettingModule;

        //private PMIModuleDevParam _PMIDevParm;
        //public PMIModuleDevParam PMIDevParm
        //{
        //    get { return this.PMIModule().GetSubsInfo().GetPMIInfo(); }
        //}

        public PMIParameterSetupControlService()
        {
            try
            {
                DialogControl = new PMIParameterSetupControl();
                DialogControl.DataContext = this;

                // Stage : Multi or Loader side

                if ((SystemManager.SysteMode == SystemModeEnum.Multiple) || (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote))
                {
                    GPVisibility = Visibility.Collapsed;
                }
                else
                {
                    GPVisibility = Visibility.Visible;
                }

                //if (this.PMIModule() != null)
                //{
                //    PMIDevParm = this.PMIModule().GetPMIDevIParam() as PMIModuleDevParam;
                //}
                //PMIDevParm = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region //..Command 

        private AsyncCommand _CmdMaunalInputContolExitClick;
        public ICommand CmdMaunalInputContolExitClick
        {
            get
            {
                if (null == _CmdMaunalInputContolExitClick) _CmdMaunalInputContolExitClick = new AsyncCommand(MaunalInputContolExitClick);
                return _CmdMaunalInputContolExitClick;
            }
        }

        private async Task MaunalInputContolExitClick()
        {
            try
            {
                //retval = this.PMIModule().SaveDevParameter();
                //retval = this.SaveParameter(PMIDevParm);
                //if(retval != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Debug($"Parameter save is failed.");
                //}

                await this.PnPManager().ClosePnpAdavanceSetupWindow();
                //await HiddenDialogControl();


                //if (bChanged == true)
                //{
                //    EnumMessageDialogResult ret = await this.MetroDialogManager().ShowMessageDialogv(
                //                                "Information",
                //                                "Parameter has changed. Do you want to save it?"
                //                                + Environment.NewLine + "Ok         : Save & Exit"
                //                                + Environment.NewLine + "Cancel     : Cancel Exit"
                //                                + Environment.NewLine + "Just Exit  : Exit without Save",
                //                                EnumMessageStyle.AffirmativeAndNegativeAndSingleAuxiliary
                //                                , "Just Exit");

                //    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                //    {
                //        // save & Exit
                //        Task<EventCodeEnum> saveTask = MaunalInputContolSave();

                //        if (saveTask.Result == EventCodeEnum.NONE)
                //        {
                //            bChanged = false;
                //            await HiddenDialogControl();
                //        }
                //        else
                //        {
                //            throw new NotImplementedException("Save Failed");
                //        }
                //    }
                //    else if (ret == EnumMessageDialogResult.NEGATIVE)
                //    {
                //        // cancel
                //        //..Do nothing..
                //    }
                //    else if (ret == EnumMessageDialogResult.FirstAuxiliary)
                //    {
                //        // Exit without Save
                //        bChanged = false;
                //        await HiddenDialogControl();
                //    }
                //}
                //else
                //{
                //    bChanged = false;
                //    await HiddenDialogControl();
                //}
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
                    if (EnumType == typeof(GROUPING_METHOD))
                    {
                        // Grouping Method가 변경 됨.
                        // Grouping Done 플래그를 깨놓자.

                        foreach (var TableTemplateInfo in this.PMIInfo.PadTableTemplateInfo)
                        {
                            TableTemplateInfo.GroupingDone.Value = false;
                        }
                    }
                }

                LoggerManager.Debug($"[PMIParameterSetupControlService] SelectionChangedCommandFunc() : Enum Name : {selectedtypeStr}, Changed Value : {changedvalStr}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private RelayCommand<Object> _TextBoxClickCommand;

        //public ICommand TextBoxClickCommand
        //{
        //    get
        //    {
        //        if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
        //        return _TextBoxClickCommand;
        //    }
        //}

        //private void TextBoxClickCommandFunc(Object param)
        //{
        //    try
        //    {
        //        LoggerManager.Debug($"TextBoxClickCommandFunc() : Clicked.");

        //        Object[] paramArr = param as Object[];

        //        int intValue = 0;
        //        long lngValue = 0;
        //        double dblValue = 0.0;
        //        short shtValue = 0;
        //        ushort ushtValue = 0;
        //        int iLowerLimit = 0;
        //        int iUpperLimit = 100;
        //        long lLowerLimit = 0;
        //        long lUpperLimit = 100;
        //        double dLowerLimit = 0.0;
        //        double dUpperLimit = 100.0;
        //        short sLowerLimit = 0;
        //        short sUpperLimit = 100;
        //        ushort usLowerLimit = 0;
        //        ushort usUpperLimit = 100;
        //        int readMaskingLevel = 99;
        //        string tbValue = "";
        //        System.Type type = null;
        //        //System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();

        //        if (paramArr[1] == null)
        //        {
        //            throw new System.ArgumentException("Parameter Cannot Be Null");
        //        }

        //        System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)paramArr[0];
        //        tbValue = tb.Text;

        //        if (paramArr[1] is Element<double>)
        //        {
        //            Element<double> eParam = paramArr[1] as Element<double>;

        //            dLowerLimit = eParam.LowerLimit;
        //            dUpperLimit = eParam.UpperLimit;
        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(double);
        //            tbValue = (eParam.Value).ToString();
        //        }
        //        else if (paramArr[1] is Element<int>)
        //        {
        //            Element<int> eParam = paramArr[1] as Element<int>;

        //            iLowerLimit = Convert.ToInt32(eParam.LowerLimit);
        //            iUpperLimit = Convert.ToInt32(eParam.UpperLimit);
        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(int);
        //            tbValue = (eParam.Value).ToString();
        //        }
        //        else if (paramArr[1] is Element<string>)
        //        {
        //            Element<string> eParam = paramArr[1] as Element<string>;

        //            iLowerLimit = Convert.ToInt32(eParam.LowerLimit);
        //            iUpperLimit = Convert.ToInt32(eParam.UpperLimit);

        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(string);
        //            tbValue = (eParam.Value).ToString();
        //        }
        //        else if (paramArr[1] is Element<long>)
        //        {
        //            Element<long> eParam = paramArr[1] as Element<long>;

        //            lLowerLimit = Convert.ToInt64(eParam.LowerLimit);
        //            lUpperLimit = Convert.ToInt64(eParam.UpperLimit);

        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(long);
        //            tbValue = (eParam.Value).ToString();
        //        }
        //        else if (paramArr[1] is Element<double>)
        //        {
        //            Element<double> eParam = paramArr[1] as Element<double>;

        //            dLowerLimit = eParam.LowerLimit;
        //            dUpperLimit = eParam.UpperLimit;
        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(double);
        //            tbValue = (eParam.Value).ToString();
        //        }
        //        else if (paramArr[1] is Element<short>)
        //        {
        //            Element<short> eParam = paramArr[1] as Element<short>;

        //            sLowerLimit = Convert.ToInt16(eParam.LowerLimit);
        //            sUpperLimit = Convert.ToInt16(eParam.UpperLimit);
        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(short);
        //            tbValue = (eParam.Value).ToString();
        //        }
        //        else if (paramArr[1] is Element<ushort>)
        //        {
        //            Element<ushort> eParam = paramArr[1] as Element<ushort>;

        //            usLowerLimit = Convert.ToUInt16(eParam.LowerLimit);
        //            usUpperLimit = Convert.ToUInt16(eParam.UpperLimit);

        //            readMaskingLevel = eParam.ReadMaskingLevel;
        //            type = typeof(ushort);
        //            tbValue = (eParam.Value).ToString();
        //        }

        //        if (type == null)
        //        {
        //            throw new System.ArgumentException("Parameter Type Cannot Be Null");
        //        }
        //        else if (type == typeof(int))
        //        {
        //            int.TryParse(tbValue, out intValue);
        //            tb.Text = VirtualKeyboard.Show(intValue, iLowerLimit, iUpperLimit).ToString();
        //        }
        //        else if (type == typeof(double))
        //        {
        //            double.TryParse(tbValue, out dblValue);
        //            tb.Text = VirtualKeyboard.Show(dblValue, dLowerLimit, dUpperLimit).ToString();
        //        }
        //        else if (type == typeof(string))
        //        {
        //            tb.Text = VirtualKeyboard.Show(tbValue, KB_TYPE.DECIMAL | KB_TYPE.FLOAT | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL, iLowerLimit, iUpperLimit);
        //        }
        //        else if (type == typeof(long))
        //        {
        //            long.TryParse(tbValue, out lngValue);
        //            tb.Text = VirtualKeyboard.Show(lngValue, lLowerLimit, lUpperLimit).ToString();
        //        }
        //        else if (type == typeof(short))
        //        {
        //            short.TryParse(tbValue, out shtValue);
        //            tb.Text = VirtualKeyboard.Show(shtValue, sLowerLimit, sUpperLimit).ToString();
        //        }
        //        else if (type == typeof(ushort))
        //        {
        //            ushort.TryParse(tbValue, out ushtValue);
        //            tb.Text = VirtualKeyboard.Show(ushtValue, usLowerLimit, usUpperLimit).ToString();
        //        }

        //        // Update source data
        //        tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        //        //bChanged = true;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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
                string backuptext = tb.Text;
                var bindingExpression = tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                    var bindingPath = bindingExpression.ParentBinding.Path.Path;
                    // Check if the binding path matches the expected one
                    if (bindingPath == "PMIDevParm.PausePercentPerDie.Value")
                    {
                        // Handle specific binding case
                        if (Convert.ToInt32(tb.Text) < (int)PMIDevParm.PausePercentPerDie.LowerLimit || Convert.ToInt32(tb.Text) > (int)PMIDevParm.PausePercentPerDie.UpperLimit)
                        {
                            LoggerManager.Debug($"[PMIParameterSetupControlService] FuncDecimalTextBoxClickCommand() : Set Value = {tb.Text}");
                            tb.Text = backuptext;
                        }
                    }

                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
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
                if (null == _StringTextBoxClickCommand) _StringTextBoxClickCommand = new RelayCommand<Object>(StringTextBoxClickCommandFunc);
                return _StringTextBoxClickCommand;
            }
        }
        private void StringTextBoxClickCommandFunc(Object param)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
            tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.ALPHABET);
            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        }

        //private RelayCommand<Object> _RadioButtonClickCommand;

        //public ICommand RadioButtonClickCommand
        //{
        //    get
        //    {
        //        if (null == _RadioButtonClickCommand) _RadioButtonClickCommand = new RelayCommand<Object>(RadioButtonClickCommandFunc);
        //        return _RadioButtonClickCommand;
        //    }
        //}

        //private void RadioButtonClickCommandFunc(Object param)
        //{
        //    try
        //    {
        //        //LoggerManager.Debug($"TextBoxClickCommandFunc() : Clicked.");
        //        //bChanged = true;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        #endregion

        #region //.. IPnpAdvanceSetupViewModel Method
        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    if (datas.Count == 2)
                    {
                        for (int i = 0; i < datas.Count; i++)
                        {
                            if (i == 0)
                            {
                                object target;
                                SerializeManager.DeserializeFromByte(datas[i], out target, typeof(PMIModuleDevParam));
                                if (target != null)
                                {
                                    PMIDevParm = (PMIModuleDevParam)target;
                                    //deserialize하기 때문에 low, upper 적용 안됨
                                    PMIDevParm.PausePercentPerDie.LowerLimit = 0;
                                    PMIDevParm.PausePercentPerDie.UpperLimit = 99;
                                }
                            }
                            else if (i == 1)
                            {
                                object target;
                                SerializeManager.DeserializeFromByte(datas[i], out target, typeof(PMIInfo));
                                if (target != null)
                                {
                                    PMIInfo = (PMIInfo)target;
                                }
                            }
                        }

                        //foreach (var param in datas)
                        //{
                        //    object target;
                        //    SerializeManager.DeserializeFromByte(param, out target, typeof(PMIModuleDevParam));
                        //    if (target != null)
                        //    {
                        //        PMIDevParm = (PMIModuleDevParam)target;
                        //        //this.PMIModule().PMIModuleDevParam_IParam = (PMIModuleDevParam)target;
                        //        break;
                        //    }
                        //}
                    }
                    else
                    {
                        LoggerManager.Error("[PMIParameterSetupControlService] SetParameters() : Data count is wrong.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();
            try
            {
                if (PMIDevParm != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(PMIDevParm));
                }

                if (PMIInfo != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(PMIInfo));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion


        public async Task ShowDialogControl()
        {
            try
            {
                //PMIDevParm = this.PMIModule().PMIModuleDevParam_IParam.Copy() as PMIModuleDevParam;

                await this.MetroDialogManager().ShowWindow(DialogControl, true);

                LoggerManager.Debug($"PMIParameterSetupControlService ShowDialogControl();");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. ShowDialogControl() : Error occured.");
            }
        }

        private async Task HiddenDialogControl()
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(DialogControl);

                LoggerManager.Debug($"PMIParameterSetupControlService HiddenDialogControl();");
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. HiddenDialogControl() : Error occured.");
            }

        }

        public void Init()
        {
            return;
        }
    }
}
