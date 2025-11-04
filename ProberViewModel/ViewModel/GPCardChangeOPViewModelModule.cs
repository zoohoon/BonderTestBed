using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ProberErrorCode;
using System.Collections.ObjectModel;
using ProberInterfaces.CardChange;
using RelayCommandBase;
using System.Windows.Input;
using SequenceRunner;
using System.Diagnostics;
using CylType;
using LogModule;
using ProberInterfaces.Param;
using System.ServiceModel;
using ProberInterfaces.ViewModel;
using MetroDialogInterfaces;
using CardChange;
using ProberInterfaces.SequenceRunner;
using SerializerUtil;
using LoaderController.GPController;

namespace GPCardChangeOPViewModelModule
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GPCardChangeOPViewModelModule : IMainScreenViewModel, ICardChangeOPVM
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region ==> ETC...
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        readonly Guid _ViewModelGUID = new Guid("fe7caf28-719b-43bd-8926-f97a6f1faed0");
        public bool Initialized { get; set; } = false;
        #endregion


        #region IMainScreenViewModel
        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }
        public void DeInitModule()
        {
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            //==> INPUT
            {
                if (this.IOManager() != null)
                {
                    //==> Card Lock Sol이 닫혀 있는지, Lock(DITPLATE_PCLATCH_SENSOR_LOCK = 1, DITPLATE_PCLATCH_SENSOR_UNLOCK = 0), Unlock(DITPLATE_PCLATCH_SENSOR_LOCK = 0, DITPLATE_PCLATCH_SENSOR_UNLOCK = 1)
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK);
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK);
                    //==> 상판쪽 Card Vacu이 들어와 있는지, On = 1, Off = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR);
                    //==> 상판쪽 Tester Vacu이 들어와 있는지, On = 1, Off = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR);
                    //==> 상판이 들어와 있는지, Reverse, Lock = 1, Unlock = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR);
                    //==> Tester가 상판쪽에 Docking 되었는지, Reverse, Exist = 1, NOT Exist = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR);
                    //==> Chuck쪽 pod 실린더 상태, UP = 1, DOWN = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);
                    //==> Chuck쪽 Card 유무 반사경 상태, Exist = 1, NOT Exist = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR);
                    //==> Chuck쪽 Card 흡착 상태, Exist = 1, NOT Exist = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR);
                    //==> Tester-Prober 고정 상태, 고정 = 1, 해제 = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_LOCK);
                    //==> Tester-Prober 고정 해제 상태, 해제 = 1, 고정 = 0
                    CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_UNLOCK);
                }
            }

            //==> OUTPUT
            {
                if (this.IOManager() != null)
                {
                    //==> VV : 파기, 해제시 진공 끄고 파기를 추가로 해야함.
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE);
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOPOGOCARD_VACU);
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE);
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU);

                    //==> Chuck쪽 Pod Up/Down 실린더 : Up(DOUPMODULE_DOWN = 0, DOUPMODULE_UP = 1), Down(DOUPMODULE_DOWN = 1, DOUPMODULE_UP = 0)
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOUPMODULE_DOWN);
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOUPMODULE_UP);

                    //==> 척쪽 Card Pod Vacu 조절, On = 1, Off = 0
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOUPMODULE_VACU);

                    //==> Card Lock Sol 조절, Lock = 1, Unlock = 0
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK);

                    //==> Chuck Vacuum
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOCHUCKAIRON_0);
                    CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOCHUCKAIRON_1);
                }
            }
            UIEnable = true;

            ChuckThetaValue = 0;

            if (this.CardChangeModule() != null)
            {
                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeParam == null)
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    Debugger.Break();
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.INITVIEWMODEL_EXCEPTION);
                }


                CardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                CardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public void WaferCamFoldCommandFunc()
        {
            StageCylinderType.MoveWaferCam.Retract();
        }
        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            this.StageSupervisor().StageModuleState.CCZCLEARED();
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                this.StageSupervisor().StageModuleState.ZCLEARED();
                WaferCamFoldCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        #endregion

        #region ====> IO

        #region ==> CardChangeInputs
        private ObservableCollection<IOPortDescripter<bool>> _CardChangeInputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> CardChangeInputs
        {
            get { return _CardChangeInputs; }
            set
            {
                if (value != _CardChangeInputs)
                {
                    _CardChangeInputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardChangeOutputs
        private ObservableCollection<IOPortDescripter<bool>> _CardChangeOutputs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> CardChangeOutputs
        {
            get { return _CardChangeOutputs; }
            set
            {
                if (value != _CardChangeOutputs)
                {
                    _CardChangeOutputs = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #endregion

        #region ====> Function

        #region ==> SmallRaiseChuckCommand
        private AsyncCommand _SmallRaiseChuckCommand;
        public ICommand SmallRaiseChuckCommand
        {
            get
            {
                if (null == _SmallRaiseChuckCommand) _SmallRaiseChuckCommand = new AsyncCommand(SmallRaiseChuckCommandFunc);
                return _SmallRaiseChuckCommand;
            }
        }
        public async Task SmallRaiseChuckCommandFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double apos = 0;
                this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                double pos = Math.Abs(100.0);
                double absPos = pos + apos;

                if (absPos > -4500)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $"Dangerous Position", EnumMessageStyle.Affirmative);

                    return;
                }

                if (absPos < zaxis.Param.PosSWLimit.Value)
                {
                    ret = this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                    LoggerManager.Debug($"Current Z Pos : {absPos}");
                }
                else
                {
                    //Sw limit
                    ret = EventCodeEnum.MOTION_DANGEROUS_POS;
                }

                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                if (ret == EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $" Check to chuck up ERR {ret.ToString()}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

        }
        #endregion

        #region ==> SmallDropChuckCommand
        private AsyncCommand _SmallDropChuckCommand;
        public ICommand SmallDropChuckCommand
        {
            get
            {
                if (null == _SmallDropChuckCommand) _SmallDropChuckCommand = new AsyncCommand(SmallDropChuckCommandFunc);
                return _SmallDropChuckCommand;
            }
        }
        public async Task SmallDropChuckCommandFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double apos = 0;
                this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                double pos = Math.Abs(100.0);
                pos *= -1;
                double absPos = pos + apos;

                if (absPos > zaxis.Param.NegSWLimit.Value)
                {
                    ret = this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                    LoggerManager.Debug($"Current Z Pos : {absPos}");
                }
                else
                {
                    ret = EventCodeEnum.MOTION_DANGEROUS_POS;
                }

                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                if (ret == EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
                else
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $" Check to chuck down sequence  ERR:{ret.ToString()}", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region ==> BigRaiseChuckCommand
        private RelayCommand _BigRaiseChuckCommand;
        public ICommand BigRaiseChuckCommand
        {
            get
            {
                if (null == _BigRaiseChuckCommand) _BigRaiseChuckCommand = new RelayCommand(BigRaiseChuckCommandFunc);
                return _BigRaiseChuckCommand;
            }
        }
        public void BigRaiseChuckCommandFunc()
        {
            if (StageCylinderType.MoveWaferCam.State != CylinderStateEnum.RETRACT)
            {
                return;
            }

            ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            this.MotionManager().AbsMove(zaxis, -20500, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);

            return;
        }
        #endregion

        #region ==> BigDropChuckCommand
        private RelayCommand _BigDropChuckCommand;
        public ICommand BigDropChuckCommand
        {
            get
            {
                if (null == _BigDropChuckCommand) _BigDropChuckCommand = new RelayCommand(BigDropChuckCommandFunc);
                return _BigDropChuckCommand;
            }
        }
        public void BigDropChuckCommandFunc()
        {
            ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            double apos = 0;
            this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
            double pos = Math.Abs(1500.0);
            pos *= -1;
            double absPos = pos + apos;
            if (absPos > zaxis.Param.NegSWLimit.Value)
            {
                LoggerManager.Debug($"Before up Z Pos : {apos}");
                this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
            }
            else
            {
                //Sw limit
            }
        }
        #endregion

        #region ==> TestRunCommand
        private AsyncCommand _TestRunCommand;
        public ICommand TestRunCommand
        {
            get
            {
                if (null == _TestRunCommand) _TestRunCommand = new AsyncCommand(TestRunCommandFunc);
                return _TestRunCommand;
            }
        }
        public async Task TestRunCommandFunc()
        {
            if (_TestRun)
                return;

            _TestRun = true;

            UIEnable = false;

            for (int i = 0; i < 10; i++)
            {
                if (_TestRun == false)
                {
                    break;
                }

                await DockCardCommandFunc();
                System.Threading.Thread.Sleep(1000);

                if (_TestRun == false)
                {
                    Debugger.Break();
                    break;
                }

                await UnDockCardCommandFunc();
                System.Threading.Thread.Sleep(1000);
            }

            UIEnable = true;
        }
        #endregion

        #region ==> TestStopCommand
        private AsyncCommand _TestStopCommand;
        public ICommand TestStopCommand
        {
            get
            {
                if (null == _TestStopCommand) _TestStopCommand = new AsyncCommand(TestStopCommandFunc);
                return _TestStopCommand;
            }
        }
        public async Task TestStopCommandFunc()
        {
            _TestRun = false;
        }
        #endregion

        #region ==> SetCardChangeStateCommand
        private RelayCommand _SetCardChangeStateCommand;
        public ICommand SetCardChangeStateCommand
        {
            get
            {
                if (null == _SetCardChangeStateCommand) _SetCardChangeStateCommand = new RelayCommand(SetCardChangeStateCommandFunc);
                return _SetCardChangeStateCommand;
            }
        }
        public void SetCardChangeStateCommandFunc()
        {
            //this.StageSupervisor().StageModuleState.LockCCState();
        }
        #endregion

        #region ==> UnSetCardChangeStateCommand
        private RelayCommand _UnSetCardChangeStateCommand;
        public ICommand UnSetCardChangeStateCommand
        {
            get
            {
                if (null == _UnSetCardChangeStateCommand) _UnSetCardChangeStateCommand = new RelayCommand(UnSetCardChangeStateCommandFunc);
                return _UnSetCardChangeStateCommand;
            }
        }
        public void UnSetCardChangeStateCommandFunc()
        {
            // this.StageSupervisor().StageModuleState.UnLockCCState();
        }
        #endregion

        #region ==> SimulateContactCommand
        private AsyncCommand _SimulateContactCommand;
        public ICommand SimulateContactCommand
        {
            get
            {
                if (null == _SimulateContactCommand) _SimulateContactCommand = new AsyncCommand(SimulateContactCommandFunc);
                return _SimulateContactCommand;
            }
        }
        public async Task SimulateContactCommandFunc()
        {
            //==> Align 수행
            var beh = new GP_CardAlign();

            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }



            //==> Big Raise 수행
            if (StageCylinderType.MoveWaferCam.State != CylinderStateEnum.RETRACT)
            {
                return;
            }

            ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            this.MotionManager().AbsMove(zaxis, -30500, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
        }
        #endregion

        #region ==> ChuckAirOnCommand
        private AsyncCommand _ChuckAirOnCommand;
        public ICommand ChuckAirOnCommand
        {
            get
            {
                if (null == _ChuckAirOnCommand) _ChuckAirOnCommand = new AsyncCommand(ChuckAirOnCommandFunc);
                return _ChuckAirOnCommand;
            }
        }
        public async Task ChuckAirOnCommandFunc()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
            //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
        }
        #endregion

        #region ==> ChuckAirOffCommand
        private AsyncCommand _ChuckAirOffCommand;
        public ICommand ChuckAirOffCommand
        {
            get
            {
                if (null == _ChuckAirOffCommand) _ChuckAirOffCommand = new AsyncCommand(ChuckAirOffCommandFunc);
                return _ChuckAirOffCommand;
            }
        }
        public async Task ChuckAirOffCommandFunc()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
        }
        #endregion

        #region ==> ForceCardVacuumOffCommand
        private AsyncCommand _ForceCardVacuumOffCommand;
        public ICommand ForceCardVacuumOffCommand
        {
            get
            {
                if (null == _ForceCardVacuumOffCommand) _ForceCardVacuumOffCommand = new AsyncCommand(ForceCardVacuumOffCommandFunc);
                return _ForceCardVacuumOffCommand;
            }
        }
        public async Task ForceCardVacuumOffCommandFunc()
        {
            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
            {
                Debugger.Break();
                return;
            }

            SequenceBehavior beh = new GP_CheckTopPlateSolIsLock();
            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                Debugger.Break();
                return;
            }

            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);

            System.Threading.Thread.Sleep(1000);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
            System.Threading.Thread.Sleep(7000);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);

            if (this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR.Value)
            {
                Debugger.Break();
                return;
            }
        }
        #endregion

        #endregion

        #region ====> Command

        #region ==> SafetyDownChuckCommand
        private AsyncCommand _SafetyDownChuckCommand;
        public IAsyncCommand SafetyDownChuckCommand
        {
            get
            {
                if (null == _SafetyDownChuckCommand) _SafetyDownChuckCommand = new AsyncCommand(SafetyDownChuckCommandFunc);
                return _SafetyDownChuckCommand;
            }
        }
        public async Task SafetyDownChuckCommandFunc()
        {
            var beh = new GP_DropChuckSafety();
            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
            //var result = await beh.Run();
            //if (result.ErrorCode != EventCodeEnum.NONE)
            //{
            //    Debugger.Break();
            //}
        }
        #endregion

        #region ==> SafetyDownCardCommand
        private AsyncCommand _SafetyDownCardCommand;
        public IAsyncCommand SafetyDownCardCommand
        {
            get
            {
                if (null == _SafetyDownCardCommand) _SafetyDownCardCommand = new AsyncCommand(SafetyDownCardCommandFunc);
                return _SafetyDownCardCommand;
            }
        }
        public async Task SafetyDownCardCommandFunc()
        {
            var beh = new GP_DropPCardSafety();
            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }

        }
        #endregion

        #region ==> MoveToLoaderCommand
        private AsyncCommand _MoveToLoaderCommand;
        public IAsyncCommand MoveToLoaderCommand
        {
            get
            {
                if (null == _MoveToLoaderCommand) _MoveToLoaderCommand = new AsyncCommand(MoveToLoaderCommandFunc);
                return _MoveToLoaderCommand;
            }
        }
        public async Task MoveToLoaderCommandFunc()
        {
            try
            {
                var beh = new GP_MoveChuckToLoader();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

            }
        }
        public async Task MoveToLoaderRemoteCommand()
        {
            try
            {
                var beh = new GP_MoveChuckToLoader();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

            }
        }
        #endregion

        #region ==> MoveToCenterCommand
        private AsyncCommand _MoveToCenterCommand;
        public IAsyncCommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new AsyncCommand(MoveToCenterCommandFunc);
                return _MoveToCenterCommand;
            }
        }
        private async Task MoveToCenterCommandFunc()
        {
            try
            {
                await Task.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                    this.MotionManager().StageMove(0, 0, zaxis.Param.ClearedPosition.Value, 0);
                });
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

            }
        }
        #endregion

        #region ==> MoveToBackCommand
        private AsyncCommand _MoveToBackCommand;
        public IAsyncCommand MoveToBackCommand
        {
            get
            {
                if (null == _MoveToBackCommand) _MoveToBackCommand = new AsyncCommand(MoveToBackCommandFunc);
                return _MoveToBackCommand;
            }
        }
        private async Task MoveToBackCommandFunc()
        {
            try
            {
                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.StageSupervisor().StageModuleState.ZCLEARED();

                    this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value - 1000, zaxis.Param.HomeOffset.Value);
                });
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

            }
        }
        #endregion

        #region ==> MoveToFrontCommand
        private AsyncCommand _MoveToFrontCommand;
        public IAsyncCommand MoveToFrontCommand
        {
            get
            {
                if (null == _MoveToFrontCommand) _MoveToFrontCommand = new AsyncCommand(MoveToFrontCommandFunc);
                return _MoveToFrontCommand;
            }
        }
        private async Task MoveToFrontCommandFunc()
        {
            try
            {
                await Task.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    this.StageSupervisor().StageModuleState.ZCLEARED();


                    this.MotionManager().StageMove(0, yaxis.Param.NegSWLimit.Value + 1000, zaxis.Param.HomeOffset.Value);
                });
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

            }
        }
        #endregion

        #endregion

        #region ==> RaisePodAfterMoveCardAlignPosCommand
        private AsyncCommand _RaisePodAfterMoveCardAlignPosCommand;
        public IAsyncCommand RaisePodAfterMoveCardAlignPosCommand
        {
            get
            {
                if (null == _RaisePodAfterMoveCardAlignPosCommand) _RaisePodAfterMoveCardAlignPosCommand = new AsyncCommand(RaisePodAfterMoveCardAlignPosCommandFunc);
                return _RaisePodAfterMoveCardAlignPosCommand;
            }
        }
        public async Task RaisePodAfterMoveCardAlignPosCommandFunc()
        {
            try
            {
                var beh = new GP_RaisePCardPodAfterMoveCardAlignPos();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    if(ret == EventCodeEnum.GP_CardChange_CARD_ALIGN_FAIL)
                    {
                        await this.MetroDialogManager().ShowMessageDialog($"Fail", $"After proceeding with the alignment, raise the Card Pod. AlignState : {CardChangeSysParam.CardAlignState}", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog($"Fail", $"You can raise the card pod after moving the card alignment position.", EnumMessageStyle.Affirmative);
                    }
                    
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"Check to Raise pod Sequence Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region ==> RaisePodCommand
        private AsyncCommand _RaisePodCommand;
        public IAsyncCommand RaisePodCommand
        {
            get
            {
                if (null == _RaisePodCommand) _RaisePodCommand = new AsyncCommand(RaisePodCommandFunc);
                return _RaisePodCommand;
            }
        }
        public async Task RaisePodCommandFunc()
        {
            try
            {
                var beh = new GP_RaisePCardPod();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Exception Error", $"Check to Raise pod Sequence Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        public async Task RaisePodRemoteCommand()
        {
            await RaisePodCommandFunc();
        }
        #endregion

        #region ==> DropPodCommand
        private AsyncCommand _DropPodCommand;
        public IAsyncCommand DropPodCommand
        {
            get
            {
                if (null == _DropPodCommand) _DropPodCommand = new AsyncCommand(DropPodCommandFunc);
                return _DropPodCommand;
            }
        }
        public async Task DropPodCommandFunc()
        {
            try
            {
                var beh = new GP_DropPCardPod();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to Drop Pod Sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        #endregion

        #region ==> DropPodCommand
        private AsyncCommand _ForcedDropPodCommand;
        public IAsyncCommand ForcedDropPodCommand
        {
            get
            {
                if (null == _ForcedDropPodCommand) _ForcedDropPodCommand = new AsyncCommand(ForcedDropPodCommandFunc);
                return _ForcedDropPodCommand;
            }
        }
        public async Task<EventCodeEnum> ForcedDropPodCommandFunc()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var beh = new GP_ForcedDropPCardPod();
                retVal = await this.CardChangeModule().BehaviorRun(beh);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #endregion

        #region ==> TopPlateSolLockCommand
        private AsyncCommand _TopPlateSolLockCommand;
        public IAsyncCommand TopPlateSolLockCommand
        {
            get
            {
                if (null == _TopPlateSolLockCommand) _TopPlateSolLockCommand = new AsyncCommand(TopPlateSolLockCommandFunc);
                return _TopPlateSolLockCommand;
            }
        }
        public async Task TopPlateSolLockCommandFunc()
        {
            try
            {
                var beh = new GP_TopPlateSolLock();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to top plate sol lock sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        #endregion

        #region ==> TopPlateSolUnLockCommand
        private AsyncCommand _TopPlateSolUnLockCommand;
        public IAsyncCommand TopPlateSolUnLockCommand
        {
            get
            {
                if (null == _TopPlateSolUnLockCommand) _TopPlateSolUnLockCommand = new AsyncCommand(TopPlateSolUnLockCommandFunc);
                return _TopPlateSolUnLockCommand;
            }
        }
        public async Task TopPlateSolUnLockCommandFunc()
        {
            try
            {
                var beh = new GP_TopPlateSolUnLock();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to top plate sol unlock sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        #endregion

        #region ==> PCardPodVacuumOffCommand
        private AsyncCommand _PCardPodVacuumOffCommand;
        public IAsyncCommand PCardPodVacuumOffCommand
        {
            get
            {
                if (null == _PCardPodVacuumOffCommand) _PCardPodVacuumOffCommand = new AsyncCommand(PCardPodVacuumOffCommandFunc);
                return _PCardPodVacuumOffCommand;
            }
        }
        public async Task PCardPodVacuumOffCommandFunc()
        {
            try
            {
                var beh = new GP_PCardPodVacuumOff();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to card pod vac off sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        #endregion

        #region ==> PCardPodVacuumOnCommand
        private AsyncCommand _PCardPodVacuumOnCommand;
        public IAsyncCommand PCardPodVacuumOnCommand
        {
            get
            {
                if (null == _PCardPodVacuumOnCommand) _PCardPodVacuumOnCommand = new AsyncCommand(PCardPodVacuumOnCommandFunc);
                return _PCardPodVacuumOnCommand;
            }
        }
        public async Task PCardPodVacuumOnCommandFunc()
        {
            try
            {
                var beh = new GP_PCardPodVacuumOn();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to card pod vac on sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        #endregion

        #region ==> PogoPCardContactVacuumOffCommand
        private AsyncCommand _FogoPCardContactVacuumOffCommand;
        public IAsyncCommand FogoPCardContactVacuumOffCommand
        {
            get
            {
                if (null == _FogoPCardContactVacuumOffCommand) _FogoPCardContactVacuumOffCommand = new AsyncCommand(FogoPCardContactVacuumOffCommandFunc);
                return _FogoPCardContactVacuumOffCommand;
            }
        }
        public async Task FogoPCardContactVacuumOffCommandFunc()
        {
            var beh = new GP_PogoPCardContactVacuumOff();
            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
        }

        #endregion

        #region ==> FogoPCardContactVacuumOnCommand
        private AsyncCommand _FogoPCardContactVacuumOnCommand;
        public IAsyncCommand FogoPCardContactVacuumOnCommand
        {
            get
            {
                if (null == _FogoPCardContactVacuumOnCommand) _FogoPCardContactVacuumOnCommand = new AsyncCommand(FogoPCardContactVacuumOnCommandFunc);
                return _FogoPCardContactVacuumOnCommand;
            }
        }
        public async Task FogoPCardContactVacuumOnCommandFunc()
        {
            var beh = new GP_PogoPCardContactVacuumOn_Manual();
            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
              //  await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region ==> UpPlateTesterCOfftactVacuumOffCommand
        private AsyncCommand _UpPlateTesterCOfftactVacuumOffCommand;
        public IAsyncCommand UpPlateTesterCOfftactVacuumOffCommand
        {
            get
            {
                if (null == _UpPlateTesterCOfftactVacuumOffCommand) _UpPlateTesterCOfftactVacuumOffCommand = new AsyncCommand(UpPlateTesterCOfftactVacuumOffCommandFunc);
                return _UpPlateTesterCOfftactVacuumOffCommand;
            }
        }
        public async Task UpPlateTesterCOfftactVacuumOffCommandFunc()
        {
            try
            {
                var beh = new GP_UpPlateTesterContactVacuumOff();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Tester Vacuum Off", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to up plate tester contact vac off sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region ==> UpPlateTesterContactVacuumOnCommand
        private AsyncCommand _UpPlateTesterContactVacuumOnCommand;
        public IAsyncCommand UpPlateTesterContactVacuumOnCommand
        {
            get
            {
                if (null == _UpPlateTesterContactVacuumOnCommand) _UpPlateTesterContactVacuumOnCommand = new AsyncCommand(UpPlateTesterContactVacuumOnCommandFunc);
                return _UpPlateTesterContactVacuumOnCommand;
            }
        }
        public async Task UpPlateTesterContactVacuumOnCommandFunc()
        {
            try
            {
                var beh = new GP_UpPlateTesterContactVacuumOn();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Tester Vacuum On", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to up plate tester contact vac on sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region ==> UpPlateTesterPurgeAirOffCommand
        private AsyncCommand _UpPlateTesterPurgeAirOffCommand;
        public IAsyncCommand UpPlateTesterPurgeAirOffCommand
        {
            get
            {
                if (null == _UpPlateTesterPurgeAirOffCommand) _UpPlateTesterPurgeAirOffCommand = new AsyncCommand(UpPlateTesterPurgeAirOffCommandFunc);
                return _UpPlateTesterPurgeAirOffCommand;
            }
        }
        public async Task UpPlateTesterPurgeAirOffCommandFunc()
        {
            try
            {
                var beh = new GP_UpPlateTesterPurgeAirOff();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                //else
                //{
                //    await this.MetroDialogManager().ShowMessageDialog("Success", "Tester Purge Air Off", EnumMessageStyle.Affirmative);
                //}
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to up plate tester purge air off sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region ==> UpPlateTesterPurgeAirOnCommand
        private AsyncCommand _UpPlateTesterPurgeAirOnCommand;
        public IAsyncCommand UpPlateTesterPurgeAirOnCommand
        {
            get
            {
                if (null == _UpPlateTesterPurgeAirOnCommand) _UpPlateTesterPurgeAirOnCommand = new AsyncCommand(UpPlateTesterPurgeAirOnCommandFunc);
                return _UpPlateTesterPurgeAirOnCommand;
            }
        }
        public async Task UpPlateTesterPurgeAirOnCommandFunc()
        {
            try
            {
                var beh = new GP_UpPlateTesterPurgeAirOn();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                //else
                //{
                //    await this.MetroDialogManager().ShowMessageDialog("Success", "Tester Purge Air On", EnumMessageStyle.Affirmative);
                //}
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to up plate tester purge air on sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region ==> DockCardCommand
        private AsyncCommand _DockCardCommand;
        public IAsyncCommand DockCardCommand
        {
            get
            {
                if (null == _DockCardCommand) _DockCardCommand = new AsyncCommand(DockCardCommandFunc);
                return _DockCardCommand;
            }
        }

        private ISequenceBehavior GetDockPCardTopPlate()
        {
            ISequenceBehavior beh = null;
            switch (this.StageSupervisor().CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.NONE:
                    break;
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_DockPCardTopPlate();
                    break;
                case EnumCardChangeType.CARRIER:
                    beh =  new GOP_DockPCardTopPlate();
                    break;
                default:
                    break;
            }
            return beh;
        }

        private ISequenceBehavior GetUnDockPCardTopPlate()
        {
            ISequenceBehavior beh = null;
            switch (this.StageSupervisor().CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.NONE:
                    break;
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_UndockPCardTopPlate();
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_UndockPCardTopPlate();
                    break;
                default:
                    break;
            }
            return beh;
        }

        public async Task DockCardCommandFunc()
        {
            try
            {
                var beh = GetDockPCardTopPlate();


                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                await this.MetroDialogManager().ShowMessageDialog("Check to dock card sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region ==> UnDockCardCommand
        private AsyncCommand _UnDockCardCommand;
        public IAsyncCommand UnDockCardCommand
        {
            get
            {
                if (null == _UnDockCardCommand) _UnDockCardCommand = new AsyncCommand(UnDockCardCommandFunc);
                return _UnDockCardCommand;
            }
        }
        public async Task UnDockCardCommandFunc()
        {
            try
            {
                //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var beh = GetUnDockPCardTopPlate();

                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to undock card sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region ==> MachinToPinCommand
        private AsyncCommand _MachinToPinCommand;
        public IAsyncCommand MachinToPinCommand
        {
            get
            {
                if (null == _MachinToPinCommand) _MachinToPinCommand = new AsyncCommand(MachinToPinCommandFunc);
                return _MachinToPinCommand;
            }
        }
        public async Task MachinToPinCommandFunc()
        {
            ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);

            double curXPos = 0;
            double curYPos = 0;
            this.MotionManager().GetActualPos(xAxis.AxisType.Value, ref curXPos);
            this.MotionManager().GetActualPos(yAxis.AxisType.Value, ref curYPos);

            PinCoordinate machinToPinPos = this.CoordinateManager().PinLowPinConvert.Convert(new MachineCoordinate(curXPos, curYPos));

            if (machinToPinPos.X.Value < xAxis.Param.PosSWLimit.Value &&
                machinToPinPos.Y.Value < yAxis.Param.PosSWLimit.Value &&
                machinToPinPos.X.Value > xAxis.Param.NegSWLimit.Value &&
                machinToPinPos.Y.Value > yAxis.Param.NegSWLimit.Value)
            {
                //this.MotionManager().AbsMove(xAxis, machinToPinPos.X.Value, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                //this.MotionManager().AbsMove(yAxis, machinToPinPos.Y.Value, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
            }
        }
        #endregion

        #region ==> PinToMachinCommand
        private AsyncCommand _PinToMachinCommand;
        public IAsyncCommand PinToMachinCommand
        {
            get
            {
                if (null == _PinToMachinCommand) _PinToMachinCommand = new AsyncCommand(PinToMachinCommandFunc);
                return _PinToMachinCommand;
            }
        }
        public async Task PinToMachinCommandFunc()
        {
            ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
            ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);

            //==> 현재 Machin 좌표
            var pinCoordinate = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
            MachineCoordinate pinToMachinePos = this.CoordinateManager().PinLowPinConvert.ConvertBack(pinCoordinate);

            if (pinToMachinePos.X.Value < xAxis.Param.PosSWLimit.Value &&
                pinToMachinePos.Y.Value < yAxis.Param.PosSWLimit.Value &&
                pinToMachinePos.X.Value > xAxis.Param.NegSWLimit.Value &&
                pinToMachinePos.Y.Value > yAxis.Param.NegSWLimit.Value)
            {
                //this.MotionManager().AbsMove(xAxis, machinToPinPos.X.Value, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                //this.MotionManager().AbsMove(yAxis, machinToPinPos.Y.Value, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
            }
        }
        #endregion

        #region ==> CardAlignCommand
        private AsyncCommand _CardAlignCommand;
        public IAsyncCommand CardAlignCommand
        {
            get
            {
                if (null == _CardAlignCommand) _CardAlignCommand = new AsyncCommand(CardAlignCommandFunc);
                return _CardAlignCommand;
            }
        }
        public async Task CardAlignCommandFunc()
        {
            var beh = new GP_CardAlign();

            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }


        }
        #endregion

        #region ==> SetAlignPosCommand
        private AsyncCommand _SetAlignPosCommand;
        public IAsyncCommand SetAlignPosCommand
        {
            get
            {
                if (null == _SetAlignPosCommand) _SetAlignPosCommand = new AsyncCommand(SetAlignPosCommandFunc);
                return _SetAlignPosCommand;
            }
        }
        public async Task SetAlignPosCommandFunc()
        {
            var beh = new GP_SetAlignPos();

            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region ==> CardStuckRecoveryCommand
        private AsyncCommand _CardStuckRecoveryCommand;
        public IAsyncCommand CardStuckRecoveryCommand
        {
            get
            {
                if (null == _CardStuckRecoveryCommand) _CardStuckRecoveryCommand = new AsyncCommand(CardStuckRecoveryCommandFunc);
                return _CardStuckRecoveryCommand;
            }
        }
        public async Task CardStuckRecoveryCommandFunc()
        {
           
            SequenceBehavior beh = null;
            if (this.StageSupervisor().CardChangeModule().GetCCType() == EnumCardChangeType.DIRECT_CARD)
            {
                beh = new GP_PCardSutckRecovery();
            }
            else if (this.StageSupervisor().CardChangeModule().GetCCType() == EnumCardChangeType.CARRIER)
            {
                beh = new GOP_PCardSutckRecovery();
            }

            if(beh != null)
            {
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            
        }
        #endregion

        #region ==> ReadyToGetCardCommand
        private AsyncCommand _ReadyToGetCardCommand;
        public IAsyncCommand ReadyToGetCardCommand
        {
            get
            {
                if (null == _ReadyToGetCardCommand) _ReadyToGetCardCommand = new AsyncCommand(ReadyToGetCardCommandFunc);
                return _ReadyToGetCardCommand;
            }
        }
        public async Task ReadyToGetCardCommandFunc()
        {
            var beh = new GP_ReadyToGetCard();

            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
        }
        #endregion


        #region ==> ClearCardChangeCommand
        private AsyncCommand _ClearCardChangeCommand;
        public IAsyncCommand ClearCardChangeCommand
        {
            get
            {
                if (null == _ClearCardChangeCommand) _ClearCardChangeCommand = new AsyncCommand(ClearCardChangeCommandFunc);
                return _ClearCardChangeCommand;
            }
        }
        public async Task ClearCardChangeCommandFunc()
        {
            var beh = new GP_ClearCardChange();

            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region==> MoveToZCleared
        private RelayCommand _MoveToZClearedCommand;
        public ICommand MoveToZClearedCommand
        {
            get
            {
                if (null == _MoveToZClearedCommand) _MoveToZClearedCommand = new RelayCommand(MoveToZClearedCommandFunc);
                return _MoveToZClearedCommand;
            }
        }
        public void MoveToZClearedCommandFunc()
        {
            this.StageSupervisor().StageModuleState.CCZCLEARED();
        }

        #endregion

        #region ====> ETC...

        #region ==> UIEnable
        private bool _UIEnable;
        public bool UIEnable
        {
            get { return _UIEnable; }
            set
            {
                if (value != _UIEnable)
                {
                    _UIEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> 
        private ICardChangeSysParam _CardChangeSysParam;
        public ICardChangeSysParam CardChangeSysParam
        {
            get { return _CardChangeSysParam; }
            set
            {
                if (value != _CardChangeSysParam)
                {
                    _CardChangeSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICardChangeDevParam _CardChangeDevParam;
        public ICardChangeDevParam CardChangeDevParam
        {
            get { return _CardChangeDevParam; }
            set
            {
                if (value != _CardChangeDevParam)
                {
                    _CardChangeDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion


        #endregion

        #region ==> CardContactCorrectionX
        private double _CardContactCorrectionX;
        public double CardContactCorrectionX
        {
            get { return _CardContactCorrectionX; }
            set
            {
                if (value != _CardContactCorrectionX)
                {
                    _CardContactCorrectionX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardContactOffsetSettingZ
        private double _CardContactOffsetSettingZ;
        public double CardContactOffsetSettingZ
        {
            get { return _CardContactOffsetSettingZ; }
            set
            {
                if (value != _CardContactOffsetSettingZ)
                {
                    _CardContactOffsetSettingZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardContactOffsetSettingX
        private double _CardContactOffsetSettingX;
        public double CardContactOffsetSettingX
        {
            get { return _CardContactOffsetSettingX; }
            set
            {
                if (value != _CardContactOffsetSettingX)
                {
                    _CardContactOffsetSettingX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardContactOffsetSettingY
        private double _CardContactOffsetSettingY;
        public double CardContactOffsetSettingY
        {
            get { return _CardContactOffsetSettingY; }
            set
            {
                if (value != _CardContactOffsetSettingY)
                {
                    _CardContactOffsetSettingY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardUndockContactOffsetSettingZ
        private double _CardUndockContactOffsetSettingZ;
        public double CardUndockContactOffsetSettingZ
        {
            get { return _CardUndockContactOffsetSettingZ; }
            set
            {
                if (value != _CardUndockContactOffsetSettingZ)
                {
                    _CardUndockContactOffsetSettingZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardFocusRange
        private double _FocusRange;
        public double FocusRange
        {
            get { return _FocusRange; }
            set
            {
                if (value != _FocusRange)
                {
                    _FocusRange = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CardAlignRetry
        private int _CardAlignRetryCount;
        public int CardAlignRetryCount
        {
            get { return _CardAlignRetryCount; }
            set
            {
                if (value != _CardAlignRetryCount)
                {
                    _CardAlignRetryCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CardContactSettingZCommand
        private RelayCommand _CardContactSettingZCommand;
        public ICommand CardContactSettingZCommand
        {
            get
            {
                if (null == _CardContactSettingZCommand) _CardContactSettingZCommand = new RelayCommand(CardContactSettingZCommandFunc);
                return _CardContactSettingZCommand;
            }
        }
        public void CardContactSettingZCommandFunc()
        {

            CardChangeSysParam.GP_ContactCorrectionZ = CardContactOffsetSettingZ;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion
        #region ==> CardUndockContactSettingZCommand
        private RelayCommand _CardUndockContactSettingZCommand;
        public ICommand CardUndockContactSettingZCommand
        {
            get
            {
                if (null == _CardUndockContactSettingZCommand) _CardUndockContactSettingZCommand = new RelayCommand(CardUndockContactSettingZCommandFunc);
                return _CardUndockContactSettingZCommand;
            }
        }
        public void CardUndockContactSettingZCommandFunc()
        {

            CardChangeSysParam.GP_Undock_ContactCorrectionZ = CardUndockContactOffsetSettingZ;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion
        #region ==> ChuckThetaValue
        private double _ChuckThetaValue;
        public double ChuckThetaValue
        {
            get { return _ChuckThetaValue; }
            set
            {
                if (value != _ChuckThetaValue)
                {
                    _ChuckThetaValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region CardFocusRangeSettingZCommand
        private AsyncCommand<double> _CardFocusRangeSettingCommand;
        public ICommand CardFocusRangeSettingCommand
        {
            get
            {
                if (null == _CardFocusRangeSettingCommand) _CardFocusRangeSettingCommand = new AsyncCommand<double>(CardFocusRangeSettingCommandFunc);
                return _CardFocusRangeSettingCommand;
            }
        }
        public async Task CardFocusRangeSettingCommandFunc(double rangevalue)
        {
            (CardChangeSysParam as CardChangeSysParam).FocusParam.FocusRange.Value = rangevalue;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion

        #region SetAlignRetryCountCommand
        private AsyncCommand<int> _SetAlignRetryCountCommand;
        public ICommand SetAlignRetryCountCommand
        {
            get
            {
                if (null == _SetAlignRetryCountCommand) _SetAlignRetryCountCommand = new AsyncCommand<int>(SetAlignRetryCountCommandFunc);
                return _SetAlignRetryCountCommand;
            }
        }
        public async Task SetAlignRetryCountCommandFunc(int retrycount)
        {
            CardChangeSysParam.PatternMatchingRetryCount = retrycount;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion

        #region SetAlignRetryCountCommand
        private AsyncCommand<bool> _SetWaitForCardPermitEnableCommand;
        public ICommand SetWaitForCardPermitEnableCommand
        {
            get
            {
                if (null == _SetWaitForCardPermitEnableCommand) _SetWaitForCardPermitEnableCommand = new AsyncCommand<bool>(SetWaitForCardPermitEnableFunc);
                return _SetWaitForCardPermitEnableCommand;
            }
        }
        public async Task SetWaitForCardPermitEnableFunc(bool toggle)
        {            

            this.CardChangeModule().SetWaitForCardPermitEnable(toggle);
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion

        private AsyncCommand<int> _ZifToggleCommand;
        public ICommand ZifToggleCommand
        {
            get
            {
                if (null == _ZifToggleCommand) _ZifToggleCommand = new AsyncCommand<int>(ZifToggleCommandFunc);
                return _ZifToggleCommand;
            }
        }
        public async Task ZifToggleCommandFunc()
        {
            var beh = new Request_ZIFCommandLowActive();
            var ret = await this.CardChangeModule().BehaviorRun(beh);
            if (ret != EventCodeEnum.NONE)
            {
                await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
            }
        }

        public byte[] GetDockSequences()
        {
            byte[] retval = null;
            try
            {
                byte[] serializeData = ObjectSerialize.Serialize(this.CardChangeModule().CCDockBehaviors.ISequenceBehaviorCollection);
                retval = serializeData;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public byte[] GetUnDockSequences()
        {
            byte[] retval = null;
            try
            {
                byte[] serializeData = ObjectSerialize.Serialize(this.CardChangeModule().CCUnDockBehaviors.ISequenceBehaviorCollection);
                retval = serializeData;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        public int GetCurBehaviorIdx()
        {
            int retval = 0;
            try
            {
                retval = this.CardChangeModule().CurBehaviorIdx;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }
        //#region CardFocusRangeSettingZCommand
        //private RelayCommand<double> _CardFocusRangeSettingCommand;
        //public ICommand CardFocusRangeSettingCommand
        //{
        //    get
        //    {
        //        if (null == _CardFocusRangeSettingCommand) _CardFocusRangeSettingCommand = new RelayCommand<double>(CardFocusRangeSettingCommandFunc);
        //        return _CardFocusRangeSettingCommand;
        //    }
        //}
        //private void CardFocusRangeSettingCommandFunc(double range)
        //{
        //    if (range <= 2000)
        //    {
        //        (CardChangeSysParam as CardChangeSysParam).FocusParam.FocusRange.Value = range;
        //        var ret = this.CardChangeModule().SaveSysParameter();
        //        if (ret != EventCodeEnum.NONE)
        //        {
        //            LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
        //            return;
        //        }
        //    }
        //}
        //#endregion
        //#region SetAlignRetryCountCommand
        //private RelayCommand<int> _SetAlignRetryCountCommand;
        //public ICommand SetAlignRetryCountCommand
        //{
        //    get
        //    {
        //        if (null == _SetAlignRetryCountCommand) _SetAlignRetryCountCommand = new RelayCommand<int>(SetAlignRetryCountCommandFunc);
        //        return _SetAlignRetryCountCommand;
        //    }
        //}
        //private void SetAlignRetryCountCommandFunc(int retrycount)
        //{
        //    CardChangeSysParam.PatternMatchingRetryCount = retrycount;
        //    var ret = this.CardChangeModule().SaveSysParameter();
        //    if (ret != EventCodeEnum.NONE)
        //    {
        //        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
        //        return;
        //    }
        //}
        //#endregion



        #region SetDistanceOffsetCommand
        private AsyncCommand<double> _SetDistanceOffsetCommand;
        public ICommand SetDistanceOffsetCommand
        {
            get
            {
                if (null == _SetDistanceOffsetCommand) _SetDistanceOffsetCommand = new AsyncCommand<double>(SetDistanceOffsetCommandFunc);
                return _SetDistanceOffsetCommand;
            }
        }
        public async Task SetDistanceOffsetCommandFunc(double distanceOffset)
        {
            CardChangeSysParam.DistanceOffset = distanceOffset;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion

        #region SetCardTopFromChuckPlaneSettingCommand
        private AsyncCommand<double> _SetCardTopFromChuckPlaneSettingCommand;
        public ICommand SetCardTopFromChuckPlaneSettingCommand
        {
            get
            {
                if (null == _SetCardTopFromChuckPlaneSettingCommand) _SetCardTopFromChuckPlaneSettingCommand = new AsyncCommand<double>(SetCardTopFromChuckPlaneSettingCommandFunc);
                return _SetCardTopFromChuckPlaneSettingCommand;
            }
        }
        public async Task SetCardTopFromChuckPlaneSettingCommandFunc(double distance)
        {
            CardChangeSysParam.CardTopFromChuckPlane.Value = distance;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                return;
            }
        }
        #endregion
        private bool _TestRun = false;


        #region ==> RotateChuckCommand
        private RelayCommand _RotateChuckCommand;
        public ICommand RotateChuckCommand
        {
            get
            {
                if (null == _RotateChuckCommand) _RotateChuckCommand = new RelayCommand(RotateChuckCommandFunc);
                return _RotateChuckCommand;
            }
        }
        public void RotateChuckCommandFunc()
        {
            this.MotionManager().AbsMove(EnumAxisConstants.C, ChuckThetaValue * 10000);

            ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
            if (cardChangeParam == null)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                Debugger.Break();
                return;
            }

            cardChangeParam.GP_LoadingPosT = ChuckThetaValue;
            var ret = this.CardChangeModule().SaveSysParameter();
            if (ret != EventCodeEnum.NONE)
            {
                Debugger.Break();
                return;
            }
        }
        #endregion

        #region ==> GetVac,IO,threeleg
        public CardChangeVacuumAndIOStatus GetCardChangeVacAndIOStatus()
        {
            CardChangeVacuumAndIOStatus vacstatus = new CardChangeVacuumAndIOStatus();
            bool DICARDCHANGEVACU;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
            //var upmoduleVacResult = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false);
            //if (upmoduleVacResult == 0)
            //{
            //    DICARDCHANGEVACU = false;       // Normal state
            //}
            //else
            //{
            //    DICARDCHANGEVACU = true;
            //}

            bool DIFOGOCARDVACU;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out DIFOGOCARDVACU);

            /*
             * 예전 OPERA 장비의 경우 밑에 3가지 Input을 다 봄. (DITPLATEIN_SENSOR, DITESTER_DOCKING_SENSOR, DIPOGOTESTER_VACU_SENSOR)
             * 현재 OPERA 장비의 경우 2가지 Input을 봄. (DITESTER_DOCKING_SENSOR, DIPOGOTESTER_VACU_SENSOR)
             * 현재 MD 장비의 경우 1가지 Input을 봄. (DIPOGOTESTER_VACU_SENSOR)
             * 안보는 IO의 경우는 EMUL처리 되어있음. EMUL일 경우 true로 처리하여 TesterPogoTouched에 대한 표기를 장비 구분없이 나타낼 수 있음. 
             */
            bool ditplatein_sensor;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
            if(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR.IOOveride.Value == EnumIOOverride.EMUL)
            {
                ditplatein_sensor = true;
            }

            bool ditester_docking_sensor;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);
            if (this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR.IOOveride.Value == EnumIOOverride.EMUL)
            {
                ditester_docking_sensor = true;
            }

            bool dipogotester_vacu_sensor;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
            if (this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR.IOOveride.Value == EnumIOOverride.EMUL)
            {
                dipogotester_vacu_sensor = true;
            }

            if (ditplatein_sensor == true && ditester_docking_sensor == true && dipogotester_vacu_sensor == true)
            {
                vacstatus.TesterPogoTouched = true;
            }
            else if (this.IOManager().IO.Inputs.DITPLATEIN_SENSOR.IOOveride.Value == EnumIOOverride.EMUL &&
                this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR.IOOveride.Value == EnumIOOverride.EMUL &&
                dipogotester_vacu_sensor == true)
            {
                vacstatus.TesterPogoTouched = true;
            }
            else
            {
                vacstatus.TesterPogoTouched = false;
            }

            bool isCamMiddle = false;
            bool isCamRear = false;

            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERCAMMIDDLE, out isCamMiddle);
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERCAMREAR, out isCamRear);

            if (isCamMiddle == true && isCamRear == false)
            {
                vacstatus.IsCamExtended = true;
            }
            else
            {
                vacstatus.IsCamExtended = false;
            }

            bool diCardLatchLock = false;
            IORet latchlock = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out diCardLatchLock);
            if (this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.IOOveride.Value == EnumIOOverride.EMUL)
            {
                if (latchlock == IORet.NO_ERR)
                {
                    diCardLatchLock = false;
                }
                else
                {
                    diCardLatchLock = true;
                }
            }
            //waitIOResult = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true);
            //if (waitIOResult == 0)
            //{
            //    diCardLatchLock = false;     // Normal state
            //}
            //else
            //{
            //    diCardLatchLock = true;
            //}



            bool diCardLatchUnLock = false;
            IORet latchunlock = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out diCardLatchUnLock);
            if (this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.IOOveride.Value == EnumIOOverride.EMUL)
            {
                if (latchunlock == IORet.NO_ERR)
                {
                    diCardLatchUnLock = true;
                }
                else
                {
                    diCardLatchUnLock = false;
                }
            }
            //waitIOResult = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false);
            //if (waitIOResult == 0)
            //{
            //    diCardLatchUnLock = true;       // Normal state
            //}
            //else
            //{
            //    diCardLatchUnLock = false;
            //}



            bool dileft_up_module;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out dileft_up_module);
            bool diright_up_module;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diright_up_module);
            bool dicardexistOnCardPod;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out dicardexistOnCardPod);
            bool diTester_mb_lock;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_MBLOCK, out diTester_mb_lock);
            bool diTester_pb_unlock;
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_PBUNLOCK, out diTester_pb_unlock);

            bool isthreelegup = false;
            this.StageSupervisor().MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);

            bool isthreelegdn = false;
            this.StageSupervisor().MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdn);

            vacstatus.CardOnPogoPod = DICARDCHANGEVACU;
            vacstatus.CardPogoTouched = DIFOGOCARDVACU;
            vacstatus.IsLeftUpModuleUp = dileft_up_module;
            vacstatus.IsRightUpModuleUp = diright_up_module;
            vacstatus.IsCardExistOnCardPod = dicardexistOnCardPod;
            vacstatus.IsTesterMotherBoardConnected = diTester_mb_lock;
            vacstatus.IsTesterPCBUnlocked = diTester_pb_unlock;

            vacstatus.IsThreeLegDown = isthreelegdn;
            vacstatus.IsThreeLegUp = isthreelegup;

            vacstatus.IsCardLatchLock = diCardLatchLock;
            vacstatus.IsCardLatchUnLock = diCardLatchUnLock;

            return vacstatus;
        }
        #endregion

        public async Task PogoVacuumOnCommandFunc()
        {
            try
            {
                var beh = new GP_PogoPCardContactVacuumOn_Manual();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to up plate tester contact vac on sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.ProgressDialogService().CloseDialg();
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        public async Task PogoVacuumOffCommandFunc()
        {
            try
            {
                var beh = new GP_PogoPCardContactVacuumOff();
                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                await this.MetroDialogManager().ShowMessageDialog("Check to up plate tester contact vac on sequence", $"Err:{err.Message}", EnumMessageStyle.Affirmative);
            }
            finally
            {
                //this.ProgressDialogService().CloseDialg();
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #region ==> GetCCSysParam

        public CardChangeSysparamData GetCardChangeSysParam()
        {
            CardChangeSysparamData sysparam = new CardChangeSysparamData();

            try
            {
                //sysparam.GP_CardContactPosZ = this.CardChangeSysParam.GP_CardContactPosZ;
                sysparam.GP_ContactCorrectionZ = this.CardChangeSysParam.GP_ContactCorrectionZ;
                sysparam.GP_ContactCorrectionX = this.CardChangeSysParam.GP_ContactCorrectionX;
                sysparam.GP_ContactCorrectionY = this.CardChangeSysParam.GP_ContactCorrectionY;
                sysparam.GP_CardFocusRange = (this.CardChangeSysParam as CardChangeSysParam).FocusParam.FocusRange.Value;
                sysparam.GP_CardAlignRetryCount = this.CardChangeSysParam.PatternMatchingRetryCount;
                sysparam.GP_UndockCorrectionZ = this.CardChangeSysParam.GP_Undock_ContactCorrectionZ;
                sysparam.GP_DistanceOffset = this.CardChangeSysParam.DistanceOffset;

                if (CardChangeSysParam.PinBaseFiducialMarkInfos.Count > 1)
                {
                    sysparam.GP_CardCenterOffsetX1 = this.CardChangeSysParam.PinBaseFiducialMarkInfos[0].CardCenterOffset.GetX();
                    sysparam.GP_CardCenterOffsetY1 = this.CardChangeSysParam.PinBaseFiducialMarkInfos[0].CardCenterOffset.GetY();
                    sysparam.GP_CardCenterOffsetX2 = this.CardChangeSysParam.PinBaseFiducialMarkInfos[1].CardCenterOffset.GetX();
                    sysparam.GP_CardCenterOffsetY2 = this.CardChangeSysParam.PinBaseFiducialMarkInfos[1].CardCenterOffset.GetY();
                }
                sysparam.GP_CardPodCenterX = this.CardChangeSysParam.CardPodCenterX.Value;
                sysparam.GP_CardPodCenterY = this.CardChangeSysParam.CardPodCenterY.Value;

                sysparam.GP_CardLoadZLimit = this.CardChangeSysParam.DD_CardDockZMaxValue.Value;
                sysparam.GP_CardLoadZInterval = this.CardChangeSysParam.DD_CardDockZInterval.Value;
                sysparam.GP_CardUnloadZOffset = this.CardChangeSysParam.DD_CardUndockZOffeset.Value;

                if (this.CardChangeSysParam.CardTransferPos.Value == null)
                {
                    this.CardChangeSysParam.CardTransferPos.Value = new CatCoordinates();
                }
                sysparam.GP_CardAlignPosX = this.CardChangeSysParam.CardTransferPos.Value.GetX() + this.CardChangeSysParam.CardTransferOffsetX.Value;
                sysparam.GP_CardAlignPosY = this.CardChangeSysParam.CardTransferPos.Value.GetY() + this.CardChangeSysParam.CardTransferOffsetY.Value;
                sysparam.GP_CardAlignPosT = this.CardChangeSysParam.CardTransferPos.Value.GetT() + this.CardChangeSysParam.CardTransferOffsetT.Value;

                sysparam.GP_DockMatchedPosX = this.CardChangeSysParam.GP_Undock_CardContactPosX;
                sysparam.GP_DockMatchedPosY = this.CardChangeSysParam.GP_Undock_CardContactPosY;
                sysparam.GP_DockMatchedPosZ = this.CardChangeSysParam.GP_Undock_CardContactPosZ;
                sysparam.GP_DockMatchedPosT = this.CardChangeSysParam.GP_Undock_CardContactPosT;
                sysparam.ProberCardList = this.CardChangeSysParam.ProberCardList;
                sysparam.CardTopFromChuckPlane = this.CardChangeSysParam.CardTopFromChuckPlane.Value;
                sysparam.PogoAlignPoint = this.CardChangeSysParam.PogoAlignPoint.Value;
                sysparam.WaitForCardPermitEnable = this.CardChangeSysParam.WaitForCardPermitEnable.Value;
                sysparam.CardChangeType = this.CardChangeSysParam.CardChangeType.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return sysparam;
        }
        #endregion

        #region ==> GetCCDevParam

        public async Task<CardChangeDevparamData> GetCardChangeDevParam()
        {
            CardChangeDevparamData devparam = new CardChangeDevparamData();

            try
            {
                devparam.GP_CardContactPosZ = this.CardChangeDevParam.GP_CardContactPosZ;
                //devparam.GP_ContactCorrectionZ = this.CardChangeDevParam.GP_ContactCorrectionZ;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return devparam;
        }
        #endregion
        #region ==> SetContactOffsetZValueCommand
        private AsyncCommand<double> _SetContactOffsetZValueCommand;
        public ICommand SetContactOffsetZValueCommand
        {
            get
            {
                if (null == _SetContactOffsetZValueCommand) _SetContactOffsetZValueCommand = new AsyncCommand<double>(SetContactOffsetZValueFunc);
                return _SetContactOffsetZValueCommand;
            }
        }
        public async Task SetContactOffsetZValueFunc(double offsetz)
        {
            try
            {
                if (offsetz != null)
                {
                    CardContactOffsetSettingZ = offsetz;
                    CardChangeSysParam.GP_ContactCorrectionZ = offsetz;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
        #region ==> SetContactOffsetXValueCommand
        private AsyncCommand<double> _SetContactOffsetXValueCommand;
        public ICommand SetContactOffsetXValueCommand
        {
            get
            {
                if (null == _SetContactOffsetXValueCommand) _SetContactOffsetXValueCommand = new AsyncCommand<double>(SetContactOffsetXValueFunc);
                return _SetContactOffsetXValueCommand;
            }
        }
        public async Task SetContactOffsetXValueFunc(double offsetx)
        {
            try
            {
                if (offsetx != null)
                {
                    CardContactOffsetSettingX = offsetx;
                    CardChangeSysParam.GP_ContactCorrectionX = offsetx;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
        #region ==> SetContactOffsetYValueCommand
        private AsyncCommand<double> _SetContactOffsetYValueCommand;
        public ICommand SetContactOffsetYValueCommand
        {
            get
            {
                if (null == _SetContactOffsetYValueCommand) _SetContactOffsetYValueCommand = new AsyncCommand<double>(SetContactOffsetYValueFunc);
                return _SetContactOffsetYValueCommand;
            }
        }
        public async Task SetContactOffsetYValueFunc(double offsety)
        {
            try
            {
                if (offsety != null)
                {
                    CardContactOffsetSettingY = offsety;
                    CardChangeSysParam.GP_ContactCorrectionY = offsety;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
        #region ==> SetUndockContactOffsetZValueCommand

        private AsyncCommand<double> _SetUndockContactOffsetZValueCommand;
        public ICommand SetUndockContactOffsetZValueCommand
        {
            get
            {
                if (null == _SetUndockContactOffsetZValueCommand) _SetUndockContactOffsetZValueCommand = new AsyncCommand<double>(SetUndockContactOffsetZValueCommandFunc);
                return _SetUndockContactOffsetZValueCommand;
            }
        }
        public async Task SetUndockContactOffsetZValueCommandFunc(double offsetz)
        {
            try
            {
                if (offsetz != null)
                {
                    CardUndockContactOffsetSettingZ = offsetz;
                    CardChangeSysParam.GP_Undock_ContactCorrectionZ = offsetz;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region SetFocusRange

        private AsyncCommand<double> _SetFocusRangeValueCommand;
        public ICommand SetFocusRangeValueCommand
        {
            get
            {
                if (null == _SetFocusRangeValueCommand) _SetFocusRangeValueCommand = new AsyncCommand<double>(SetFocusRangeValueCommandFunc);
                return _SetFocusRangeValueCommand;
            }
        }
        public async Task SetFocusRangeValueCommandFunc(double range)
        {
            try
            {
                if (range != null)
                {
                    FocusRange = range;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region SetCardAlignRetry

        private RelayCommand<int> _SetAlignRetryValueCommand;
        public ICommand SetAlignRetryValueCommand
        {
            get
            {
                if (null == _SetAlignRetryValueCommand) _SetAlignRetryValueCommand = new RelayCommand<int>(SetAlignRetryValueCommandFunc);
                return _SetAlignRetryValueCommand;
            }
        }
        public void SetAlignRetryValueCommandFunc(int retrycnt)
        {
            try
            {
                if (retrycnt != null)
                {
                    CardAlignRetryCount = retrycnt;
                    CardChangeSysParam.PatternMatchingRetryCount = CardAlignRetryCount;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    LoggerManager.Debug($"SetAlignRetryValueCommandFunc(): CardChangeSysParam.PatternMatchingRetryCount has been updated, Value = {CardChangeSysParam.PatternMatchingRetryCount} ");
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> CleanUpCommand
        private AsyncCommand _CleanUpCommand;
        public IAsyncCommand CleanUpCommand
        {
            get
            {
                if (null == _CleanUpCommand) _CleanUpCommand = new AsyncCommand(CleanUpCommandFunc);
                return _CleanUpCommand;
            }
        }
        public async Task CleanUpCommandFunc()
        {
            try
            {
                await Cleanup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> PageSwitchCommand
        private AsyncCommand _PageSwitchCommand;
        public IAsyncCommand PageSwitchCommand
        {
            get
            {
                if (null == _PageSwitchCommand) _PageSwitchCommand = new AsyncCommand(PageSwitchCommandFunc);
                return _PageSwitchCommand;
            }
        }
        public async Task PageSwitchCommandFunc()
        {
            try
            {
                await PageSwitched();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> SetCCParam //Drax
        public async Task SetCardCenterOffsetX1CommandFunc(double value)
        {
            try
            {
                string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                if (String.IsNullOrEmpty(probeCardID))
                {
                    probeCardID = "DefaultCard";
                }

                ProberCardListParameter probeCard = CardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                if (CardChangeSysParam != null && probeCard.FiducialMarInfos.Count > 1)
                {
                    probeCard.FiducialMarInfos[0].CardCenterOffset.X.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                    this.LoaderController().UploadProbeCardInfo(probeCard);
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error"); 
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardCenterOffsetX2CommandFunc(double value)
        {
            try
            {
                string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                if (String.IsNullOrEmpty(probeCardID))
                {
                    probeCardID = "DefaultCard";
                }

                ProberCardListParameter probeCard = CardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                if (CardChangeSysParam != null && probeCard.FiducialMarInfos.Count > 1)
                {
                    probeCard.FiducialMarInfos[1].CardCenterOffset.X.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                    this.LoaderController().UploadProbeCardInfo(probeCard);
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardCenterOffsetY1CommandFunc(double value)
        {
            try
            {
                string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                if (String.IsNullOrEmpty(probeCardID))
                {
                    probeCardID = "DefaultCard";
                }

                ProberCardListParameter probeCard = CardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                if (CardChangeSysParam != null && probeCard.FiducialMarInfos.Count > 1)
                {
                    probeCard.FiducialMarInfos[0].CardCenterOffset.Y.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                    this.LoaderController().UploadProbeCardInfo(probeCard);
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardCenterOffsetY2CommandFunc(double value)
        {
            try
            {
                string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                if (String.IsNullOrEmpty(probeCardID))
                {
                    probeCardID = "DefaultCard";
                }

                ProberCardListParameter probeCard = CardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                if (CardChangeSysParam != null && probeCard.FiducialMarInfos.Count > 1)
                {
                    probeCard.FiducialMarInfos[1].CardCenterOffset.Y.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                    this.LoaderController().UploadProbeCardInfo(probeCard);
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task SetCardPodCenterXCommandFunc(double value)
        {
            try
            {
                if (CardChangeSysParam != null)
                {
                    CardChangeSysParam.CardPodCenterX.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardPodCenterYCommandFunc(double value)
        {
            try
            {
                if (CardChangeSysParam != null)
                {
                    CardChangeSysParam.CardPodCenterY.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardLoadZLimitCommandFunc(double value)
        {
            try
            {
                if (CardChangeSysParam != null)
                {
                    CardChangeSysParam.DD_CardDockZMaxValue.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardLoadZIntervalCommandFunc(double value)
        {
            try
            {
                if (CardChangeSysParam != null)
                {
                    CardChangeSysParam.DD_CardDockZInterval.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetCardUnloadZOffsetCommandFunc(double value)
        {
            try
            {
                if (CardChangeSysParam != null)
                {
                    CardChangeSysParam.DD_CardUndockZOffeset.Value = value;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
