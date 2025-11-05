using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanPadAdvanceSetup.ViewModel
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using SubstrateObjects;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using VirtualKeyboardControl;
    using System.Windows.Input;
    using SerializerUtil;

    public class CleanPadAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IDataErrorInfo , ILoaderFactoryModule , IPnpAdvanceSetupViewModel
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
        public string Error { get; set; }

        public string this[string columnName] => throw new NotImplementedException();
        #endregion

        #region //..Property
        private NeedleCleanObject _NC;
        public NeedleCleanObject NC
        {
            get { return _NC; }
            set
            {
                if (value != _NC)
                {
                    _NC = value;
                    RaisePropertyChanged();
                }
            }
        }
        //public NeedleCleanObject NC { get { return this.StageSupervisor().NCObject as NeedleCleanObject; } }
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
        #endregion

        #region //..Command & Method
        public CleanPadAdvanceSetupViewModel()
        {
            try
            {
                //PadWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
                //PadHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SettingData()
        {
            PadWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
            PadHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;
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
                NC = (NeedleCleanObject)this.StageSupervisor().NeedleCleaner().GetParam_NcObject();
                var lastPadWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
                var lastPadHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;
                NC.NCSysParam.NeedleCleanPadWidth.Value = PadWidth;
                NC.NCSysParam.NeedleCleanPadHeight.Value = PadHeight;

                var changedWidRatio = PadWidth / lastPadWidth;
                var changedHeiRatio = PadHeight / lastPadHeight;
                
                for (int i = 0; i < NC.NCSheetVMDefs.Count; i++)
                {
                    var lastVal = NC.NCSysParam.SheetDefs[i].Range.Value.X.Value;
                    NC.NCSysParam.SheetDefs[i].Range.Value.X.Value = lastVal * changedWidRatio;
                    lastVal = NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value;
                    NC.NCSysParam.SheetDefs[i].Range.Value.Y.Value = lastVal * changedHeiRatio;
                }

                NC.InitCleanPadRender();
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

                PadWidth = NC.NCSysParam.NeedleCleanPadWidth.Value;
                PadHeight = NC.NCSysParam.NeedleCleanPadHeight.Value;
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
                        SerializeManager.DeserializeFromByte(param, out target, typeof(NeedleCleanObject));
                        if (target != null)
                        {
                            NC = (target as NeedleCleanObject);
                            SettingData();
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
                if (NC != null)
                    parameters.Add(SerializeManager.SerializeToByte(NC));
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
