using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanPadSequenceAdvanceSetup.ViewModel
{
    using LogModule;
    using RelayCommandBase;
    using VirtualKeyboardControl;
    using NeedleCleanerModuleParameter;
    using SubstrateObjects;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using ProberErrorCode;
    using SerializerUtil;

    public class CleanPadSequenceAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IDataErrorInfo, ILoaderFactoryModule, IPnpAdvanceSetupViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IDataErrorInfo
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
        #endregion

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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleCleanObject NC { get { return (NeedleCleanObject)this.StageSupervisor().NCObject; } }
        #endregion

        #region //.. Command & Method

        public void SettingData(NeedleCleanDeviceParameter param)
        {
            try
            {
                NeedleCleanerParam = (NeedleCleanDeviceParameter)param;
                CleaningDistance = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value;
                CleaningCount = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _CmdOKClick;
        public ICommand CmdOKClick
        {
            get
            {
                if (null == _CmdOKClick) _CmdOKClick = new AsyncCommand(CmdOKClickFunc);
                return _CmdOKClick;
            }
        }
        private async Task CmdOKClickFunc()
        {
            try
            {
                if (!ParamValidationFlag)
                    return;

                NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value = CleaningDistance;
                NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value = CleaningCount;
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private AsyncCommand _CmdCancelClick;
        public ICommand CmdCancelClick
        {
            get
            {
                if (null == _CmdCancelClick) _CmdCancelClick = new AsyncCommand(CmdCancelClickFunc);
                return _CmdCancelClick;
            }
        }
        private async Task CmdCancelClickFunc()
        {
            try
            {
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
                CleaningDistance = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningDistance.Value;
                CleaningCount = NeedleCleanerParam.SheetDevs[NC.NCSheetVMDef.Index].CleaningCount.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        #endregion

        #region //.. IPnpAdvanceSetupViewModel Method

        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    foreach (var param in datas)
                    {
                        object target;
                        SerializeManager.DeserializeFromByte(param, out target, typeof(NeedleCleanDeviceParameter));
                        if (target != null)
                        {
                            SettingData(target as NeedleCleanDeviceParameter);
                            break;
                        }
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
                if (NeedleCleanerParam != null)
                    parameters.Add(SerializeManager.SerializeToByte(NeedleCleanerParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion


        #region //..ILoaderFactoryModule
        public InitPriorityEnum InitPriority { get; set; }

        public EventCodeEnum InitModule(global::Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            return;
        }
        #endregion
        public void Init()
        {
            return;
        }

    }
}
