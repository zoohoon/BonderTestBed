using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserCoordAdvanceSetup.ViewModel
{
    using LogModule;
    using ProberInterfaces;
    using RelayCommandBase;
    using SerializerUtil;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    public class UserCoordStandardAdvanceSetupViewModel : IFactoryModule, INotifyPropertyChanged, IDataErrorInfo, IPnpAdvanceSetupViewModel
    {
        #region //..RaisePropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..IDataErrorInfo
        public string Error { get { return string.Empty; } }
        //private bool ParamValidationFlag = true;



        public string this[string columnName]
        {
            get
            {
                //if (columnName == "UserCoordX" && this.UserCoordX < 0 )
                //{
                //    ParamValidationFlag = false;
                //    return Properties.Resources.NegativeErrorMessage;

                //}
                //else if (columnName == "UserCoordY" && this.UserCoordY <0)
                //{
                //    ParamValidationFlag = false;
                //    return Properties.Resources.NegativeErrorMessage;
                //}
                //else
                //    ParamValidationFlag = true;

                return null;
            }
        }
        #endregion

        #region //..Property



        private long _UserCoordX;
        public long UserCoordX
        {
            get { return _UserCoordX; }
            set
            {
                if (value != _UserCoordX)
                {
                    _UserCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _UserCoordY;
        public long UserCoordY
        {
            get { return _UserCoordY; }
            set
            {
                if (value != _UserCoordY)
                {
                    _UserCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _RefCoordX;
        public long RefCoordX
        {
            get { return _RefCoordX; }
            set
            {
                if (value != _RefCoordX)
                {
                    _RefCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _RefCoordY;
        public long RefCoordY
        {
            get { return _RefCoordY; }
            set
            {
                if (value != _RefCoordY)
                {
                    _RefCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _CendieX;
        public long CendieX
        {
            get { return _CendieX; }
            set
            {
                if (value != _CendieX)
                {
                    _CendieX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CendieY;
        public long CendieY
        {
            get { return _CendieY; }
            set
            {
                if (value != _CendieY)
                {
                    _CendieY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CurMachineCoordX;
        public long CurMachineCoordX
        {
            get { return _CurMachineCoordX; }
            set
            {
                if (value != _CurMachineCoordX)
                {
                    _CurMachineCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CurMachineCoordY;
        public long CurMachineCoordY
        {
            get { return _CurMachineCoordY; }
            set
            {
                if (value != _CurMachineCoordY)
                {
                    _CurMachineCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }


        private long _MachineCoordX;
        public long MachineCoordX
        {
            get { return _MachineCoordX; }
            set
            {
                if (value != _MachineCoordX)
                {
                    _MachineCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MachineCoordY;
        public long MachineCoordY
        {
            get { return _MachineCoordY; }
            set
            {
                if (value != _MachineCoordY)
                {
                    _MachineCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<TargetDieWrapper> _targetUs;
        public ObservableCollection<TargetDieWrapper> TargetUs
        {
            get { return _targetUs; }
            set
            {
                if (value != _targetUs)
                {
                    _targetUs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TargetDieWrapper _selectedTargetU;
        public TargetDieWrapper SelectedTargetU
        {
            get { return _selectedTargetU; }
            set
            {
                if (value != _selectedTargetU)
                {
                    _selectedTargetU = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _tabIndex;
        public int TabIndex
        {
            get { return _tabIndex; }
            set
            {
                if (value != _tabIndex)
                {
                    _tabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        
        public WaferObject Wafer { get { return this.StageSupervisor().WaferObject as WaferObject; } }

        #endregion

        #region //..Command & Method

        public void SettingData()
        {
            try
            {
                TabIndex = 0;

                MachineCoordX = Wafer.GetPhysInfo().OrgM.XIndex.Value;
                MachineCoordY = Wafer.GetPhysInfo().OrgM.YIndex.Value;

                UserCoordX = Wafer.GetPhysInfo().OrgU.XIndex.Value;
                UserCoordY = Wafer.GetPhysInfo().OrgU.YIndex.Value;

                RefCoordX = Wafer.GetPhysInfo().RefU.XIndex.Value;
                RefCoordY = Wafer.GetPhysInfo().RefU.YIndex.Value;

                CendieX = Wafer.GetPhysInfo().CenU.XIndex.Value;
                CendieY = Wafer.GetPhysInfo().CenU.YIndex.Value;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (TargetUs == null)
                    {
                        TargetUs = new ObservableCollection<TargetDieWrapper>();
                    }
                    else
                    {
                        TargetUs.Clear();
                    }

                    foreach (var targetdie in Wafer.GetPhysInfo().TargetUs)
                    {
                        TargetDieWrapper tmp = new TargetDieWrapper();

                        tmp.TargetDie.XIndex.Value = targetdie.XIndex.Value;
                        tmp.TargetDie.YIndex.Value = targetdie.YIndex.Value;

                        SetTargetDieCommand(tmp);

                        TargetUs.Add(tmp);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _StandardCmdOKClick;
        public ICommand StandardCmdOKClick
        {
            get
            {
                if (null == _StandardCmdOKClick) _StandardCmdOKClick = new AsyncCommand(StandardCmdOKClickFunc);
                return _StandardCmdOKClick;
            }
        }
        private async Task StandardCmdOKClickFunc()
        {
            try
            {
                Wafer.GetPhysInfo().OrgU.XIndex.Value = UserCoordX;
                Wafer.GetPhysInfo().OrgU.YIndex.Value = UserCoordY;

                Wafer.GetPhysInfo().OrgM.XIndex.Value = Wafer.CurrentMXIndex;
                Wafer.GetPhysInfo().OrgM.YIndex.Value = Wafer.CurrentMYIndex;

                var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                Wafer.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;
                Wafer.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;
                //Wafer.GetPhysInfo().CenU.XIndex.Value = CendieX;
                //Wafer.GetPhysInfo().CenU.YIndex.Value = CendieY;

                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand _AdvancedCmdOKClick;
        public ICommand AdvancedCmdOKClick
        {
            get
            {
                if (null == _AdvancedCmdOKClick) _AdvancedCmdOKClick = new AsyncCommand(AdvancedCmdOKClickFunc);
                return _AdvancedCmdOKClick;
            }
        }
        private async Task AdvancedCmdOKClickFunc()
        {
            try
            {
                Wafer.GetPhysInfo().CenU.XIndex.Value = CendieX;
                Wafer.GetPhysInfo().CenU.YIndex.Value = CendieY;

                this.CoordinateManager().UpdateCenM();

                Wafer.GetPhysInfo().RefU.XIndex.Value = RefCoordX;
                Wafer.GetPhysInfo().RefU.YIndex.Value = RefCoordY;

                Wafer.GetPhysInfo().TargetUs.Clear();

                foreach (var item in TargetUs)
                {
                    ElemUserIndex tmp = new ElemUserIndex();

                    tmp.XIndex.Value = item.TargetDie.XIndex.Value;
                    tmp.YIndex.Value = item.TargetDie.YIndex.Value;

                    Wafer.GetPhysInfo().TargetUs.Add(tmp);
                }

                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetTargetDieCommand(TargetDieWrapper data)
        {
            try
            {
                //data.TextBoxClickCommand = new RelayCommand<Object>(o => TargetDieTextBoxClickCommandFunc(data));

                data.TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void TargetDieTextBoxClickCommandFunc(Object param)
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

                //UserCoordX = Wafer.GetPhysInfo().OrgU.XIndex.Value;
                //UserCoordY = Wafer.GetPhysInfo().OrgU.YIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<Object> _AddTargetDieCommand;
        public ICommand AddTargetDieCommand
        {
            get
            {
                if (null == _AddTargetDieCommand) _AddTargetDieCommand = new RelayCommand<Object>(AddTargetDieCommandFunc);
                return _AddTargetDieCommand;
            }
        }


        private void AddTargetDieCommandFunc(Object param)
        {
            try
            {
                if (this.TargetUs == null)
                {
                    this.TargetUs = new ObservableCollection<TargetDieWrapper>();
                }

                TargetDieWrapper tmp = new TargetDieWrapper();

                SetTargetDieCommand(tmp);
                this.TargetUs.Add(tmp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DeleteTargetDieCommand;
        public ICommand DeleteTargetDieCommand
        {
            get
            {
                if (null == _DeleteTargetDieCommand) _DeleteTargetDieCommand = new RelayCommand<Object>(DeleteTargetDieCommandFunc);
                return _DeleteTargetDieCommand;
            }
        }


        private void DeleteTargetDieCommandFunc(Object param)
        {
            try
            {
                if (this.SelectedTargetU != null)
                {
                    this.TargetUs.Remove(this.SelectedTargetU);
                }
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
            }
        }

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
                        SerializeManager.DeserializeFromByte(param, out target, typeof(PhysicalInfo));
                        if (target != null)
                        {
                            //(this.StageSupervisor().WaferObject as WaferObject).CopyForm((WaferObject)target);
                            this.StageSupervisor().WaferObject.SetPhysInfo((IPhysicalInfo)target);
                            break;
                        }
                    }
                }
                SettingData();
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
                parameters.Add(SerializeManager.SerializeToByte(Wafer.GetPhysInfo()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        #endregion

        #endregion

        public void Init()
        {
            return;
        }
    }

    public class TargetDieWrapper : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ElemUserIndex _targetDie;
        public ElemUserIndex TargetDie
        {
            get { return _targetDie; }
            set
            {
                if (value != _targetDie)
                {
                    _targetDie = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand TextBoxClickCommand { get; set; }

        public TargetDieWrapper()
        {
            this.TargetDie = new ElemUserIndex();
        }
    }
}
