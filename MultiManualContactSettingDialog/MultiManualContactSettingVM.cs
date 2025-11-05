using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProbingModule;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace MultiManualContactSettingDialog
{
    public class MultiManualContactSettingVM: INotifyPropertyChanged, IFactoryModule
    {        
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        readonly Guid _ScreenGUID = new Guid("460EBE3B-3451-46B3-B70D-4CBFEC3ED6CE");
        public Guid ScreenGUID { get { return _ScreenGUID; } }

        private Autofac.IContainer LoaderContainer = null;
        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private IGPLoader _GPLoader = null;
        public bool Initialized { get; set; } = false;
        private IManualContact _MCM;
        public IManualContact MCM
        {
            get { return _MCM; }
            set
            {
                if (value != _MCM)
                {

                    _MCM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderModule _LoaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _LoaderModule; }
            set
            {
                if (value != _LoaderModule)
                {

                    _LoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }        
        private IProbingModule _ProbingModule;
        public IProbingModule ProbingModule
        {
            get { return _ProbingModule; }
            set
            {
                if (value != _ProbingModule)
                {

                    _ProbingModule = value;
                    RaisePropertyChanged();
                }
            }
        }        

        private double _FirstContactHeight;
        public double FirstContactHeight
        {
            get { return _FirstContactHeight; }
            set
            {
                if (value != _FirstContactHeight)
                {

                    _FirstContactHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AllContactHeight;
        public double AllContactHeight
        {
            get { return _AllContactHeight; }
            set
            {
                if (value != _AllContactHeight)
                {

                    _AllContactHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private OverDriveStartPositionType _OverDriveStartPosition;
        public OverDriveStartPositionType OverDriveStartPosition
        {
            get { return _OverDriveStartPosition; }
            set
            {
                if (value != _OverDriveStartPosition)
                {

                    _OverDriveStartPosition = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IParam _ProbingDevCopyParam;
        public IParam ProbingDevCopyParam
        {
            get { return _ProbingDevCopyParam; }
            set
            {
                if (_ProbingDevCopyParam != value)
                {
                    _ProbingDevCopyParam = value;
                    RaisePropertyChanged();
                }
            }
        }        

        private ILoaderSupervisor _LoaderMaster;
        public ILoaderSupervisor LoaderMaster
        {
            get { return _LoaderMaster; }
            set
            {
                if (value != _LoaderMaster)
                {

                    _LoaderMaster = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ILoaderCommunicationManager _LoaderCommunicationManager;
        public ILoaderCommunicationManager LoaderCommunicationManager
        {
            get { return _LoaderCommunicationManager; }
            set
            {
                if (value != _LoaderCommunicationManager)
                {

                    _LoaderCommunicationManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _StageIndex;
        public int StageIndex
        {
            get { return _StageIndex; }
            set
            {
                if (value != _StageIndex)
                {

                    _StageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        Autofac.IContainer Container = null;
        MainWindow wd;
        public bool Show(Autofac.IContainer container, int idx)
        {
            bool isCheck = false;
            try
            {
                if (LoaderContainer == null)
                {
                    Container = container;
                    StageIndex = idx;
                    LoaderContainer = container;
                    ProbingModule = this.ProbingModule();
                    ProbingDevCopyParam = ProbingModule.GetProbingDevIParam(StageIndex);
                    _GPLoader = container.Resolve<IGPLoader>();
                    _LoaderModule = container.Resolve<ILoaderModule>();
                    LoaderMaster = container.Resolve<ILoaderSupervisor>();
                    LoaderCommunicationManager = container.Resolve<ILoaderCommunicationManager>();
                    int foupCnt = _LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Count();

              

                  
                    var loaderMap = _LoaderModule.GetLoaderInfo().StateMap;
                


                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    wd = new MainWindow();
                    wd.DataContext = this;
                    //wd.ShowDialog();

                });
                if (isCheck)
                {
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ProbingModule.ProbingModuleDevParam_IParam = ProbingDevCopyParam;

                retVal = ProbingModule.SaveDevParameter();
                retVal = ProbingModule.LoadDevParameter();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }       

        private RelayCommand<Object> _OverDriveTextBoxClickCommand;
        public ICommand OverDriveTextBoxClickCommand
        {
            get
            {
                if (null == _OverDriveTextBoxClickCommand) _OverDriveTextBoxClickCommand = new RelayCommand<Object>(FuncOverDriveTextBoxClickCommand);
                return _OverDriveTextBoxClickCommand;
            }
        }

        private void FuncOverDriveTextBoxClickCommand(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                double oldvalue = Convert.ToDouble(tb.Text);
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 10);
                double newvalue = Convert.ToDouble(tb.Text);

                // TODO : 
                string errorReason = null;
                bool ret = VerifyParameterODRange(tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).ParentBinding.Path.Path, ref errorReason);

                if (ret == false)
                {
                    tb.Text = oldvalue.ToString();
                    this.MetroDialogManager().ShowMessageDialog("Error", $"OverDrive Value : {errorReason}", EnumMessageStyle.Affirmative);
                }

                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                bool VerifyParameterODRange(string path, ref string errorReasonStr)
                {
                    ProbingModuleDevParam probingDevParam = ProbingDevCopyParam as ProbingModuleDevParam;
                    bool retval = true;
                    if (path == "ProbingDevCopyParam.OverDrive.Value")
                    {
                        if (newvalue > probingDevParam.OverdriveUpperLimit.Value)
                        {
                            retval = false;
                            if (probingDevParam.OverDrive.Value > probingDevParam.OverdriveUpperLimit.Value)
                            {
                                // 기능 적용 시점 전에 애초에 OverDrive 가 Limit 를 넘게 설정되었거나 누군가 json 파일 건들였을까봐...
                                errorReasonStr += $"Positive SW Limit occurred.\n" +
                                  $"Overdrive Limit Value => {probingDevParam.OverdriveUpperLimit.Value}\n" +
                                  $"Overdrive Value => {newvalue}\n" +
                                  "Change the Overdrive value to the limit.";
                                oldvalue = probingDevParam.OverdriveUpperLimit.Value;
                                probingDevParam.OverDrive.Value = probingDevParam.OverdriveUpperLimit.Value;
                            }
                            else
                            {
                                // 원래값 그대로 유지하고 메시지만 띄워줌
                                errorReasonStr += $"Positive SW Limit occurred.\n" +
                                $"Overdrive Limit Value => {probingDevParam.OverdriveUpperLimit.Value}\n" +
                                $"Overdrive Value => {newvalue}\n";
                            }
                        }
                        else
                            retval = true;
                    }
                    else if (path == "ProbingDevCopyParam.OverdriveUpperLimit.Value")
                    {
                        if (probingDevParam.OverDrive.Value > newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Positive SW Limit occurred.\n" +
                                $"Overdrive Limit Value => {newvalue}\n" +
                                $"Overdrive Value => {probingDevParam.OverDrive.Value}\n";
                        }
                        else
                            retval = true;
                    }

                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<int> _SetOverDriveCommand;
        public ICommand SetOverDriveCommand
        {
            get
            {
                if (null == _SetOverDriveCommand) _SetOverDriveCommand = new RelayCommand<int>(SetOverDrive);
                return _SetOverDriveCommand;
            }
        }

        private void SetOverDrive(int param)
        {
            try
            {
                if(param != 0)
                {
                    // Save를 부르기 전, Stage쪽에 업데이트를 해줘야 함.
                    // TODO : Set은 했지만, Save를 하지 않았을 경우에 대한 생각??? 데이터가 따로 논다??                              
                    this.ProbingModule().SetProbingDevParam(ProbingDevCopyParam, param);
                    this.ProbingModule().SaveDevParameter(ProbingDevCopyParam, param);

                    GetCellInfo(LoaderCommunicationManager.Cells.Where(i => i.Index == param).FirstOrDefault());
                }                                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ZClearanceTextBoxClickCommand;
        public ICommand ZClearanceTextBoxClickCommand
        {
            get
            {
                if (null == _ZClearanceTextBoxClickCommand) _ZClearanceTextBoxClickCommand = new RelayCommand<Object>(FuncZClearanceTextBoxClickCommand);
                return _ZClearanceTextBoxClickCommand;
            }
        }

        private void FuncZClearanceTextBoxClickCommand(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 10);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<int> _SetZClearanceCommand;
        public ICommand SetZClearanceCommand
        {
            get
            {
                if (null == _SetZClearanceCommand) _SetZClearanceCommand = new RelayCommand<int>(SetZClearance);
                return _SetZClearanceCommand;
            }
        }

        private void SetZClearance(int param)
        {
            try
            {
                if(param != 0)
                {
                    // Save를 부르기 전, Stage쪽에 업데이트를 해줘야 함.
                    // TODO : Set은 했지만, Save를 하지 않았을 경우에 대한 생각??? 데이터가 따로 논다??

                    this.ProbingModule().SetProbingDevParam(ProbingDevCopyParam, param);
                    this.ProbingModule().SaveDevParameter(ProbingDevCopyParam, param);
                    GetCellInfo(LoaderCommunicationManager.Cells.Where(i => i.Index == param).FirstOrDefault());
                }
                               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetCellInfo(IStageObject stage)
        {
            try
            {
                if (stage != null)
                {
                    stage.StageInfo.LotData = LoaderMaster.GetStageLotData(stage.Index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GetManualContactInfo(int param)
        {
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(param);
                if (proxy != null)
                {
                    proxy.GetOverDriveFromProbingModule();
                    var info = proxy.GetManualContactInfo();
                    if (info != null)
                    {                                                
                        //this.ManualContactModule().ChangeOverDrive(info.ManualContactModuleOverDrive.ToString());

                        //this.ManualContactModule().ChangeCPC_Z1(info.CPC_Z1.ToString());
                        //this.ManualContactModule().ChangeCPC_Z2(info.CPC_Z2.ToString());

                        this.ProbingModule().FirstContactHeight = info.ProbingModuleFirstContactHeight;
                        this.ProbingModule().AllContactHeight = info.ProbingModuleAllContactHeight;
                        (this.ProbingModule().ProbingModuleSysParam_IParam as ProbingModuleSysParam).OverDriveStartPosition.Value = info.ProbingModuleOverDriveStartPosition;

                        //this.ManualContactModule().MXYIndex = info.ManualContactModuleXYIndex;
                        //this.ManualContactModule().MachinePosition = info.ManualContactModuleMachinePosition;
                        FirstContactHeight = info.ProbingModuleFirstContactHeight;
                        AllContactHeight = info.ProbingModuleAllContactHeight;
                        OverDriveStartPosition = info.ProbingModuleOverDriveStartPosition;

                        this.ManualContactModule().IsZUpState = info.IsZUpState;                 
                    }                    
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _FirstContactSetCommand;
        public IAsyncCommand FirstContactSetCommand
        {
            get
            {
                if (null == _FirstContactSetCommand) _FirstContactSetCommand = new AsyncCommand<int>(FirstContactSet);
                return _FirstContactSetCommand;
            }
        }

        private async Task FirstContactSet(int param)
        {
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(param);

                if (proxy != null)
                {
                    proxy.GetOverDriveFromProbingModule();
                    await proxy.ManualContact_FirstContactSetCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo(param);                        
                    });
                    task.Start();
                    await task;
                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                    GetCellInfo(LoaderCommunicationManager.Cells.Where(i => i.Index == param).FirstOrDefault());
                }                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _AllContactSetCommand;
        public IAsyncCommand AllContactSetCommand
        {
            get
            {
                if (null == _AllContactSetCommand) _AllContactSetCommand = new AsyncCommand<int>(AllContactSet);
                return _AllContactSetCommand;
            }
        }

        private async Task AllContactSet(int param)
        {
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(param);

                if (proxy != null)
                {
                    proxy.GetOverDriveFromProbingModule();
                    await proxy.ManualContact_AllContactSetCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo(param);                        
                    });
                    task.Start();
                    await task;
                    //this.ProbingModule().SetProbingSysParam(ProbingSysCopyParam, param);
                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                    GetCellInfo(LoaderCommunicationManager.Cells.Where(i => i.Index == param).FirstOrDefault());
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<int> _ResetContactStartPositionCommand;
        public IAsyncCommand ResetContactStartPositionCommand
        {
            get
            {
                if (null == _ResetContactStartPositionCommand)
                    _ResetContactStartPositionCommand = new AsyncCommand<int>(ResetContactStartPosition);
                return _ResetContactStartPositionCommand;
            }
        }

        
        

        private async Task ResetContactStartPosition(int param)
        {
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(param);

                if (proxy != null)
                {
                    proxy.GetOverDriveFromProbingModule();
                    await proxy.ManualContact_ResetContactStartPositionCommand();

                    Task task = new Task(() =>
                    {
                        GetManualContactInfo(param);                       
                    });
                    task.Start();
                    await task;
                    //this.ProbingModule().SetProbingSysParam(ProbingSysCopyParam, param);
                    //await Task.Run(() =>
                    //{
                    //    GetManualContactInfo();
                    //});
                    GetCellInfo(LoaderCommunicationManager.Cells.Where(i => i.Index == param).FirstOrDefault());
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Init()
        {
            try
            {
                ProbingModule = this.ProbingModule();                
                ProbingDevCopyParam = this.ProbingModule().GetProbingDevIParam(StageIndex);
                LoaderMaster = _Container.Resolve<ILoaderSupervisor>();
                LoaderCommunicationManager = _Container.Resolve<ILoaderCommunicationManager>();
                GetManualContactInfo(StageIndex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }  
    }
}
