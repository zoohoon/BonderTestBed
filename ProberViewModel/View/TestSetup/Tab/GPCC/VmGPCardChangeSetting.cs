using System;
using System.Threading.Tasks;

namespace TestSetupDialog.Tab.GPCC
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Collections.ObjectModel;
    using ProberInterfaces;
    using RelayCommandBase;
    using ProberInterfaces.SequenceRunner;
    using ProberErrorCode;
    using System.Windows.Input;
    using SequenceRunner;
    using LogModule;
    using ProberInterfaces.CardChange;
    using System.Diagnostics;
    using ProberInterfaces.Param;
    using CylType;
    using MetroDialogInterfaces;
    using System.Threading;

    public class VmGPCardChangeSetting : INotifyPropertyChanged, IFactoryModule, IDisposable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void Dispose()
        {

            if (this.StageSupervisor().StageModuleState.GetState() != StageStateEnum.CARDCHANGE)
            {
                LoggerManager.Debug($"[CardChange IO Controller UI Exit] StageMoveState: {this.StageSupervisor().StageModuleState.GetState()} , Execute Z Cleared ");
                this.StageSupervisor().StageModuleState.ZCLEARED();
            }
            else
            {
                LoggerManager.Debug($"[CardChange IO Controller UI Exit] StageMoveState: {this.StageSupervisor().StageModuleState.GetState()} , Not Execute Z Cleared ");
            }
        }

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
        private RelayCommand _SmallRaiseChuckCommand;
        public ICommand SmallRaiseChuckCommand
        {
            get
            {
                if (null == _SmallRaiseChuckCommand) _SmallRaiseChuckCommand = new RelayCommand(SmallRaiseChuckCommandFunc);
                return _SmallRaiseChuckCommand;
            }
        }
        private void SmallRaiseChuckCommandFunc()
        {
            ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            double apos = 0;
            this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
            double pos = Math.Abs(100.0);
            double absPos = pos + apos;

            if (absPos > -4500)
            {
                return;
            }

            if (absPos < zaxis.Param.PosSWLimit.Value)
            {
                this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                LoggerManager.Debug($"Current Z Pos : {absPos}");
            }
            else
            {
                //Sw limit
            }
        }
        #endregion


        #region ==> RaiseChuckStopByUpmoduleCommand
        private AsyncCommand _RaiseChuckStopByUpmoduleCommand;
        public ICommand RaiseChuckStopByUpmoduleCommand
        {
            get
            {
                if (null == _RaiseChuckStopByUpmoduleCommand) _RaiseChuckStopByUpmoduleCommand = new AsyncCommand(RaiseChuckStopByUpmoduleFunc);
                return _RaiseChuckStopByUpmoduleCommand;
            }
        }
        private async Task RaiseChuckStopByUpmoduleFunc()
        {
            UIEnable = false;
            try
            {

                IBehaviorResult retVal = new BehaviorResult();
                SequenceBehavior command;


                //==> GOP_PCardPodVacuumOn                
                command = new GOP_PCardPodVacuumOn();//=> Prober Card Pod Vacu를 흡착하여 Card 흡착
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return; }

                //==> CC Clearance                 
                command = new GOP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return; }


                //==> MoveToCenter
                command = new GOP_SetCardContactPos();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return; }




                
                
                double zAbsPos = CardChangeParam.CardDockPosZ.Value;

                double bigZUpZPos = zAbsPos - 40000;
                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    throw new Exception();
                }


                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value/10, zAxis.Param.Acceleration.Value/10);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Small Z up, {bigZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    throw new Exception();
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;
                while (runflag)
                {
                    bool resume = true;
                    bool diupmodule_left_sensor;
                    bool diupmodule_right_sensor;
                    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        resume = false;
                        runflag = resume;
                        LoggerManager.Debug("DIUPMODULE_LEFT_SENSOR off");
                        break;
                    }

                    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        resume = false;
                        runflag = resume;
                        LoggerManager.Debug("DIUPMODULE_RIGHT_SENSOR off");
                        break;
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;
                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if(apos >= zAbsPos)
                        {
                            runflag = false;
                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value/10, zaxis.Param.Acceleration.Value/20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        runflag = false;
                    }
                    Thread.Sleep(2);
                }

                //Func<bool> stopFunc = () =>
                //{
                    
                //    bool raiseStop = false;
                //    bool diupmodule_left_sensor;
                //    bool diupmodule_right_sensor;
                //    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                //    if (iret != (int)IORet.NO_ERR)
                //    {
                //        raiseStop = true;
                //        return raiseStop;
                //    }
                    
                //    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                //    if (iret != (int)IORet.NO_ERR)
                //    {
                //        raiseStop = true;
                //        return raiseStop;
                //    }

                //    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                //    double apos = 0;
                //    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                //    double pos = Math.Abs(100.0);
                //    pos *= -1;
                //    double absPos = pos + apos;
                //    if (absPos > zaxis.Param.PosSWLimit.Value)
                //    {
                //        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                //        LoggerManager.Debug($"Current Z Pos : {absPos}");
                //    }
                //    else
                //    {
                //        //Sw limit
                //    }
                //    return raiseStop;
                //};

                //this.StageSupervisor().MotionManager().WaitForAxisMotionDone(zAxis, stopFunc, resumeLevel: false);
                //this.StageSupervisor().MotionManager().WaitForAxisMotionDone(zAxis);
                //(ARM.Definition.AxisType.Value, stopFunc, resumeVal);
                LoggerManager.Debug($"Current Z Pos : {zAxis.Status.Position.Actual}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }            
            UIEnable = true;
        }
        #endregion


        #region ==> SmallDropChuckCommand
        private RelayCommand _SmallDropChuckCommand;
        public ICommand SmallDropChuckCommand
        {
            get
            {
                if (null == _SmallDropChuckCommand) _SmallDropChuckCommand = new RelayCommand(SmallDropChuckCommandFunc);
                return _SmallDropChuckCommand;
            }
        }
        private void SmallDropChuckCommandFunc()
        {
            ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            double apos = 0;
            this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
            double pos = Math.Abs(100.0);
            pos *= -1;
            double absPos = pos + apos;
            if (absPos > zaxis.Param.NegSWLimit.Value)
            {
                this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value, zaxis.Param.Acceleration.Value);
                LoggerManager.Debug($"Current Z Pos : {absPos}");
            }
            else
            {
                //Sw limit
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
        private void BigRaiseChuckCommandFunc()
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
        private void BigDropChuckCommandFunc()
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

        #region ==> THDockCommand
        private AsyncCommand _THDockCommand;
        public ICommand THDockCommand
        {
            get
            {
                if (null == _THDockCommand) _THDockCommand = new AsyncCommand(THDockCommandFunc);
                return _THDockCommand;
            }
        }
        private async Task THDockCommandFunc()
        {
            UIEnable = false;

            ISequenceBehavior beh = null;
            switch (this.CardChangeModule().GetCCType())
            {
                
                case EnumCardChangeType.DIRECT_CARD:
                    break;
                case EnumCardChangeType.CARRIER:                    
                    beh = new GOP_THDockReady();
                    var result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GOP_THDockReady ERROR");
                        Debugger.Break();
                    }

                    beh = new Request_ZIFCommandLowActive();
                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Request_ZIFCommandLowActive ERROR");
                        Debugger.Break();
                    }

                    // Tester - Prober Lock
                    beh = new TesterHeadRotLock();
                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"TesterHeadRotLock ERROR");
                        Debugger.Break();
                    }


                    beh = new GOP_THDockClearedCheck();
                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GOP_THDockClearedCheck ERROR");
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
            UIEnable = true;
        }
        #endregion

        #region ==> THUndockCommand
        private AsyncCommand _THUndockCommand;
        public ICommand THUndockCommand
        {
            get
            {
                if (null == _THUndockCommand) _THUndockCommand = new AsyncCommand(THUndockCommandFunc);
                return _THUndockCommand;
            }
        }
        private async Task THUndockCommandFunc()
        {
            UIEnable = false;

           
            ISequenceBehavior beh = null;
            switch (this.CardChangeModule().GetCCType())
            {

                case EnumCardChangeType.DIRECT_CARD:
                    break;
                case EnumCardChangeType.CARRIER:                    
                    beh = new GOP_THUndockReady();
                    var result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GOP_THUndockReady ERROR");
                        Debugger.Break();
                    }

                    // Tester - Prober unlock
                    beh = new TesterHeadRotUnlock();
                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"TesterHeadRotUnlock ERROR");
                        Debugger.Break();
                    }


                    beh = new Request_ZIFCommandLowActive();
                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"Request_ZIFCommandLowActive ERROR");
                        Debugger.Break();
                    }


                    beh = new GOP_THUndockClearedCheck();
                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GOP_THUndockClearedCheck ERROR");
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
            UIEnable = true;
        }
        #endregion

        #region ==> ZIFLockToggleCommand
        private AsyncCommand _ZIFLockToggleCommand;
        public ICommand ZIFLockToggleCommand
        {
            get
            {
                if (null == _ZIFLockToggleCommand) _ZIFLockToggleCommand = new AsyncCommand(ZIFLockToggleCommandFunc);
                return _ZIFLockToggleCommand;
            }
        }
        private async Task ZIFLockToggleCommandFunc()
        {
            UIEnable = false;

            ISequenceBehavior beh = null;

            var mblock = false;
            var pblock = false;
            var pbunlock = false;

            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_MBLOCK, out mblock);
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_PBUNLOCK, out pbunlock);
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_PBLOCK, out pblock);            

            LoggerManager.Debug($"ZIF Toggle Before: mblock({this.IOManager().IO.Inputs.DITH_MBLOCK.IOOveride.Value}):{mblock} " +
                                                    $"pblock({this.IOManager().IO.Inputs.DITH_PBLOCK.IOOveride.Value}):{pblock} " +
                                                    $"pbunlock({this.IOManager().IO.Inputs.DITH_PBUNLOCK.IOOveride.Value}):{pbunlock}");

            beh = new Request_ZIFCommandLowActive();
            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"Request_ZIFCommandLowActive ERROR");
                Debugger.Break();
            }

            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_MBLOCK, out mblock);
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_PBUNLOCK, out pbunlock);
            this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITH_PBLOCK, out pblock);

            LoggerManager.Debug($"ZIF Toggle After: mblock({this.IOManager().IO.Inputs.DITH_MBLOCK.IOOveride.Value}):{mblock} " +
                                                   $"pblock({this.IOManager().IO.Inputs.DITH_PBLOCK.IOOveride.Value}):{pblock} " +
                                                   $"pbunlock({this.IOManager().IO.Inputs.DITH_PBUNLOCK.IOOveride.Value}):{pbunlock}");


            UIEnable = true;
        }
        #endregion

        #region ==> ManiUnlockCommand
        private AsyncCommand _ManiUnlockCommand;
        public ICommand ManiUnlockCommand
        {
            get
            {
                if (null == _ManiUnlockCommand) _ManiUnlockCommand = new AsyncCommand(ManiUnlockCommandFunc);
                return _ManiUnlockCommand;
            }
        }
        private async Task ManiUnlockCommandFunc()
        {
            UIEnable = false;

            // mani unlock == mani can up/down
            var intret = (int)this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOMANILOCK, false);
            if (intret != 0)
            {
                LoggerManager.Debug($"GOP_THDockClearedCheck ERROR");
                Debugger.Break();
            }
            
            UIEnable = true;
        }
        #endregion

        #region ==> TesterHeadUnlockCommand
        private AsyncCommand _TesterHeadUnlockCommand;
        public ICommand TesterHeadUnlockCommand
        {
            get
            {
                if (null == _TesterHeadUnlockCommand) _TesterHeadUnlockCommand = new AsyncCommand(TesterHeadUnlockCommandFunc);
                return _TesterHeadUnlockCommand;
            }
        }
        private async Task TesterHeadUnlockCommandFunc()
        {
            UIEnable = false;

            try
            {
                ISequenceBehavior beh = null;
                // Tester - Prober unlock
                beh = new TesterHeadRotUnlock();
                var result = await beh.Run();
                if (result.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TesterHeadRotUnlock Ouput ERROR");
                    //Debugger.Break();
                }

                // tester head lock == mani cannot up/down
                result.ErrorCode = this.CardChangeModule().IsTeadLockRequestedState(false);
                if (result.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TesterHeadRotUnlock Check ERROR");
                    //Debugger.Break();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UIEnable = true;
            }

        }
        #endregion

        #region ==> TesterHeadUnlockCommand
        private AsyncCommand _TesterHeadLockCommand;
        public ICommand TesterHeadLockCommand
        {
            get
            {
                if (null == _TesterHeadLockCommand) _TesterHeadLockCommand = new AsyncCommand(TesterHeadLockCommandFunc);
                return _TesterHeadLockCommand;
            }
        }
        private async Task TesterHeadLockCommandFunc()
        {
            UIEnable = false;

            try
            {
                ISequenceBehavior beh = null;
                // Tester - Prober lock
                beh = new TesterHeadRotLock();
                var result = await beh.Run();
                if (result.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TesterHeadLock Output ERROR");
                    //Debugger.Break();
                }

                // tester head lock == mani cannot up/down
                result.ErrorCode = this.CardChangeModule().IsTeadLockRequestedState(false);
                if (result.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"TesterHeadLock Check ERROR");
                    //Debugger.Break();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                UIEnable = true;
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
        private async Task TestRunCommandFunc()
        {
            if (_TestRun)
                return;

            _TestRun = true;

            UIEnable = false;

            for (int i = 0; i < CardDockRepeatCount; i++)
            {
                
                if (_TestRun == false)
                {
                    break;
                }
                LoggerManager.Debug($"[Card Dock/Undock TestRun][{i + 1}/{CardDockRepeatCount}] Start");
                await DockCardCommandFunc();
                 System.Threading.Thread.Sleep(1000);

                if (_TestRun == false)
                {
                    Debugger.Break();
                    break;
                }

                await UnDockCardCommandFunc();
                System.Threading.Thread.Sleep(1000);
                LoggerManager.Debug($"[Card Dock/Undock TestRun][{i + 1}/{CardDockRepeatCount}] End");
            }

            UIEnable = true;
            _TestRun = false;
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
        private async Task TestStopCommandFunc()
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
        private void SetCardChangeStateCommandFunc()
        {
            this.StageSupervisor().StageModuleState.LockCCState();
            this.StageSupervisor().StageModuleState.CCZCLEARED();// to CardChangeState
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
        private void UnSetCardChangeStateCommandFunc()
        {
            this.StageSupervisor().StageModuleState.UnLockCCState();
            this.StageSupervisor().StageModuleState.ZCLEARED(); // to IDLE 
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
        private async Task SimulateContactCommandFunc()
        {           
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {

                case EnumCardChangeType.DIRECT_CARD:
                    //==> Align 수행
                    beh = new GP_CardAlign();

                    result = await beh.Run();
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
                    break;
                default:
                    break;
            }
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
        private async Task ChuckAirOnCommandFunc()
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
        private async Task ChuckAirOffCommandFunc()
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
        private async Task ForceCardVacuumOffCommandFunc()
        {
            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
            {
                Debugger.Break();
                return;
            }

            

            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_CheckTopPlateSolIsLock();
                    result = await beh.Run();
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
                    break;
                default:
                    break;
            }
        }
        #endregion

        #endregion

        #region ====> Command

        #region ==> SafetyDownChuckCommand
        private AsyncCommand _SafetyDownChuckCommand;
        public ICommand SafetyDownChuckCommand
        {
            get
            {
                if (null == _SafetyDownChuckCommand) _SafetyDownChuckCommand = new AsyncCommand(SafetyDownChuckCommandFunc);
                return _SafetyDownChuckCommand;
            }
        }
        private async Task SafetyDownChuckCommandFunc()
        {

            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
               
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_DropChuckSafety();

                    result = await beh.Run();

                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_DropChuckSafety();

                    result = await beh.Run();

                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> SafetyDownCardCommand
        private AsyncCommand _SafetyDownCardCommand;
        public ICommand SafetyDownCardCommand
        {
            get
            {
                if (null == _SafetyDownCardCommand) _SafetyDownCardCommand = new AsyncCommand(SafetyDownCardCommandFunc);
                return _SafetyDownCardCommand;
            }
        }
        private async Task SafetyDownCardCommandFunc()
        {
            var beh = new GP_DropPCardSafety();

            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }
        }
        #endregion




        #region ==> CardDoorOpenCommand
        private AsyncCommand _CardDoorOpenCommand;
        public ICommand CardDoorOpenCommand
        {
            get
            {
                if (null == _CardDoorOpenCommand) _CardDoorOpenCommand = new AsyncCommand(CardDoorOpenCommandFunc);
                return _CardDoorOpenCommand;
            }
        }
        private async Task CardDoorOpenCommandFunc()
        {

            var retVal = this.StageSupervisor().StageModuleState.CardDoorOpen();
            Task.Delay(1000).Wait();
            if (retVal != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }
           
        }
        #endregion


        #region ==> CardDoorCloseCommand
        private AsyncCommand _CardDoorCloseCommand;
        public ICommand CardDoorCloseCommand
        {
            get
            {
                if (null == _CardDoorCloseCommand) _CardDoorCloseCommand = new AsyncCommand(CardDoorCloseCommandFunc);
                return _CardDoorCloseCommand;
            }
        }
        private async Task CardDoorCloseCommandFunc()
        {

            var retVal = this.StageSupervisor().StageModuleState.CardDoorClose();
            Task.Delay(1000).Wait();
            if (retVal != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }

        }
        #endregion
        #region ==> RaisePodCommand
        private AsyncCommand _RaisePodCommand;
        public ICommand RaisePodCommand
        {
            get
            {
                if (null == _RaisePodCommand) _RaisePodCommand = new AsyncCommand(RaisePodCommandFunc);
                return _RaisePodCommand;
            }
        }
        private async Task RaisePodCommandFunc()
        {
            var beh = new GP_RaisePCardPod();

            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }
        }
        #endregion

        #region ==> DropPodCommand
        private AsyncCommand _DropPodCommand;
        public ICommand DropPodCommand
        {
            get
            {
                if (null == _DropPodCommand) _DropPodCommand = new AsyncCommand(DropPodCommandFunc);
                return _DropPodCommand;
            }
        }
        private async Task DropPodCommandFunc()
        {
            var retVal = await this.MetroDialogManager().ShowMessageDialog($"Drop Pod ", "Do you Want to Drop Pod?", EnumMessageStyle.AffirmativeAndNegative);
            if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
            {
                var beh = new GP_DropPCardPod();

                var result = await beh.Run();
                if (result.ErrorCode != EventCodeEnum.NONE)
                {
                    Debugger.Break();
                }
            }
        }
        #endregion

        #region ==> MoveToLoadercommand
        private AsyncCommand _MoveToLoaderCommand;
        public ICommand MoveToLoaderCommand
        {
            get
            {
                if (null == _MoveToLoaderCommand) _MoveToLoaderCommand = new AsyncCommand(MoveToLoaderCommandFunc);
                return _MoveToLoaderCommand;
            }
        }
        private async Task MoveToLoaderCommandFunc()
        {
            try
            {
                
                var beh = new GOP_MoveChuckToLoader();
                var result = await beh.Run();
                if (result.ErrorCode != EventCodeEnum.NONE)
                {
                    Debugger.Break();
                }
               
            }
            catch (Exception error)
            {
                LoggerManager.Error(error.Message);
            }
        }
        #endregion

        #region ==> TopPlateSolLockCommand
        private AsyncCommand _TopPlateSolLockCommand;
        public ICommand TopPlateSolLockCommand
        {
            get
            {
                if (null == _TopPlateSolLockCommand) _TopPlateSolLockCommand = new AsyncCommand(TopPlateSolLockCommandFunc);
                return _TopPlateSolLockCommand;
            }
        }
        private async Task TopPlateSolLockCommandFunc()
        {
            

            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_TopPlateSolLock();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> TopPlateSolUnLockCommand
        private AsyncCommand _TopPlateSolUnLockCommand;
        public ICommand TopPlateSolUnLockCommand
        {
            get
            {
                if (null == _TopPlateSolUnLockCommand) _TopPlateSolUnLockCommand = new AsyncCommand(TopPlateSolUnLockCommandFunc);
                return _TopPlateSolUnLockCommand;
            }
        }
        private async Task TopPlateSolUnLockCommandFunc()
        {            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_TopPlateSolUnLock();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> PCardPodVacuumOffCommand
        private AsyncCommand _PCardPodVacuumOffCommand;
        public ICommand PCardPodVacuumOffCommand
        {
            get
            {
                if (null == _PCardPodVacuumOffCommand) _PCardPodVacuumOffCommand = new AsyncCommand(PCardPodVacuumOffCommandFunc);
                return _PCardPodVacuumOffCommand;
            }
        }
        private async Task PCardPodVacuumOffCommandFunc()
        {
            ISequenceBehavior beh = null;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.NONE:
                    break;
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_PCardPodVacuumOff();
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_PCardPodVacuumOff();
                    break;
                default:
                    break;
            }

            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }
        }
        #endregion

        #region ==> PCardPodVacuumOnCommand
        private AsyncCommand _PCardPodVacuumOnCommand;
        public ICommand PCardPodVacuumOnCommand
        {
            get
            {
                if (null == _PCardPodVacuumOnCommand) _PCardPodVacuumOnCommand = new AsyncCommand(PCardPodVacuumOnCommandFunc);
                return _PCardPodVacuumOnCommand;
            }
        }
        private async Task PCardPodVacuumOnCommandFunc()
        {
            ISequenceBehavior beh = null;
            switch (this.StageSupervisor().CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.NONE:
                    break;
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_PCardPodVacuumOn();
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_PCardPodVacuumOn();
                    break;
                default:
                    break;
            }

            var result = await beh.Run();
            if (result.ErrorCode != EventCodeEnum.NONE)
            {
                Debugger.Break();
            }
        }
        #endregion

        #region ==> FogoPCardContactVacuumOffCommand
        private AsyncCommand _FogoPCardContactVacuumOffCommand;
        public ICommand FogoPCardContactVacuumOffCommand
        {
            get
            {
                if (null == _FogoPCardContactVacuumOffCommand) _FogoPCardContactVacuumOffCommand = new AsyncCommand(FogoPCardContactVacuumOffCommandFunc);
                return _FogoPCardContactVacuumOffCommand;
            }
        }
        private async Task FogoPCardContactVacuumOffCommandFunc()
        {
            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_PogoPCardContactVacuumOff();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> FogoPCardContactVacuumOnCommand
        private AsyncCommand _FogoPCardContactVacuumOnCommand;
        public ICommand FogoPCardContactVacuumOnCommand
        {
            get
            {
                if (null == _FogoPCardContactVacuumOnCommand) _FogoPCardContactVacuumOnCommand = new AsyncCommand(FogoPCardContactVacuumOnCommandFunc);
                return _FogoPCardContactVacuumOnCommand;
            }
        }
        private async Task FogoPCardContactVacuumOnCommandFunc()
        {            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_PogoPCardContactVacuumOn_Manual();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> UpPlateTesterCOfftactVacuumOffCommand
        private AsyncCommand _UpPlateTesterCOfftactVacuumOffCommand;
        public ICommand UpPlateTesterCOfftactVacuumOffCommand
        {
            get
            {
                if (null == _UpPlateTesterCOfftactVacuumOffCommand) _UpPlateTesterCOfftactVacuumOffCommand = new AsyncCommand(UpPlateTesterCOfftactVacuumOffCommandFunc);
                return _UpPlateTesterCOfftactVacuumOffCommand;
            }
        }
        private async Task UpPlateTesterCOfftactVacuumOffCommandFunc()
        {
            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_UpPlateTesterContactVacuumOff();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> UpPlateTesterContactVacuumOnCommand
        private AsyncCommand _UpPlateTesterContactVacuumOnCommand;
        public ICommand UpPlateTesterContactVacuumOnCommand
        {
            get
            {
                if (null == _UpPlateTesterContactVacuumOnCommand) _UpPlateTesterContactVacuumOnCommand = new AsyncCommand(UpPlateTesterContactVacuumOnCommandFunc);
                return _UpPlateTesterContactVacuumOnCommand;
            }
        }
        private async Task UpPlateTesterContactVacuumOnCommandFunc()
        {            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_UpPlateTesterContactVacuumOn();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> DockCardCommand
        private AsyncCommand<bool> _DockCardCommand;
        public ICommand DockCardCommand
        {
            get
            {
                if (null == _DockCardCommand) _DockCardCommand = new AsyncCommand<bool>(DockCardCommandFunc);
                return _DockCardCommand;
            }
        }

        

        private async Task DockCardCommandFunc()
        {
            ISequenceBehavior beh = null;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.NONE:
                    break;
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_DockPCardTopPlate();
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_DockPCardTopPlate();
                    break;
                default:
                    break;
            }
            var result = await this.CardChangeModule().BehaviorRun(beh);
            if (result != EventCodeEnum.NONE)
            {
                _TestRun = false;
                Debugger.Break();
            }

        }
        #endregion

        #region ==> UnDockCardCommand
        private AsyncCommand _UnDockCardCommand;
        public ICommand UnDockCardCommand
        {
            get
            {
                if (null == _UnDockCardCommand) _UnDockCardCommand = new AsyncCommand(UnDockCardCommandFunc);
                return _UnDockCardCommand;
            }
        }

        private SequenceBehavior GetUnDockBehavior()
        {
            switch (this.CardChangeModule().GetCCType())
            {                
                case EnumCardChangeType.DIRECT_CARD:
                    return new GP_UndockPCardTopPlate();
                    break;
                case EnumCardChangeType.CARRIER:
                    return new GOP_UndockPCardTopPlate();
                    break;
                default:
                    return null;
                    break;
            }
        }

        private async Task UnDockCardCommandFunc()
        {
            var beh = GetUnDockBehavior();

            var result = await this.CardChangeModule().BehaviorRun(beh);
            if (result != EventCodeEnum.NONE)
            {
                _TestRun = false;
                Debugger.Break();
            }
        }
        #endregion

        #region ==> MachinToPinCommand
        private AsyncCommand _MachinToPinCommand;
        public ICommand MachinToPinCommand
        {
            get
            {
                if (null == _MachinToPinCommand) _MachinToPinCommand = new AsyncCommand(MachinToPinCommandFunc);
                return _MachinToPinCommand;
            }
        }
        private async Task MachinToPinCommandFunc()
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
        public ICommand PinToMachinCommand
        {
            get
            {
                if (null == _PinToMachinCommand) _PinToMachinCommand = new AsyncCommand(PinToMachinCommandFunc);
                return _PinToMachinCommand;
            }
        }
        private async Task PinToMachinCommandFunc()
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
        public ICommand CardAlignCommand
        {
            get
            {
                if (null == _CardAlignCommand) _CardAlignCommand = new AsyncCommand(CardAlignCommandFunc);
                return _CardAlignCommand;
            }
        }
        private async Task CardAlignCommandFunc()
        {
            ISequenceBehavior beh = null;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.NONE:
                    break;
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_CardAlign();
                    var result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                case EnumCardChangeType.CARRIER:
                    
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> SetAlignPosCommand
        private AsyncCommand _SetAlignPosCommand;
        public ICommand SetAlignPosCommand
        {
            get
            {
                if (null == _SetAlignPosCommand) _SetAlignPosCommand = new AsyncCommand(SetAlignPosCommandFunc);
                return _SetAlignPosCommand;
            }
        }
        private async Task SetAlignPosCommandFunc()
        {

            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_SetAlignPos();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> CardStuckRecoveryCommand
        private AsyncCommand _CardStuckRecoveryCommand;
        public ICommand CardStuckRecoveryCommand
        {
            get
            {
                if (null == _CardStuckRecoveryCommand) _CardStuckRecoveryCommand = new AsyncCommand(CardStuckRecoveryCommandFunc);
                return _CardStuckRecoveryCommand;
            }
        }
        private async Task CardStuckRecoveryCommandFunc()
        {
           
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_PCardSutckRecovery();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_PCardSutckRecovery();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> ReadyToGetCardCommand
        private AsyncCommand _ReadyToGetCardCommand;
        public ICommand ReadyToGetCardCommand
        {
            get
            {
                if (null == _ReadyToGetCardCommand) _ReadyToGetCardCommand = new AsyncCommand(ReadyToGetCardCommandFunc);
                return _ReadyToGetCardCommand;
            }
        }
        private async Task ReadyToGetCardCommandFunc()
        {            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_ReadyToGetCard();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_ReadyToGetCard();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==> ClearCardChangeCommand
        private AsyncCommand _ClearCardChangeCommand;
        public ICommand ClearCardChangeCommand
        {
            get
            {
                if (null == _ClearCardChangeCommand) _ClearCardChangeCommand = new AsyncCommand(ClearCardChangeCommandFunc);
                return _ClearCardChangeCommand;
            }
        }
        private async Task ClearCardChangeCommandFunc()
        {            
            SequenceBehavior beh;
            IBehaviorResult result;
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    beh = new GP_ClearCardChange();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                case EnumCardChangeType.CARRIER:
                    beh = new GOP_ClearCardChange();

                    result = await beh.Run();
                    if (result.ErrorCode != EventCodeEnum.NONE)
                    {
                        Debugger.Break();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion


        #region ==> PogoCardVac On Command
        private AsyncCommand _PogoCardVacOnCommand;
        public ICommand PogoCardVacOnCommand
        {
            get
            {
                if (null == _PogoCardVacOnCommand) _PogoCardVacOnCommand = new AsyncCommand(PogoCardVacOnCommandFunc);
                return _PogoCardVacOnCommand;
            }
        }
        private async Task PogoCardVacOnCommandFunc()
        {
            //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
            //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
        }
        #endregion

        #region ==> PogoCardVac Off Command
        private AsyncCommand _PogoCardVacOffCommand;
        public ICommand PogoCardVacOffCommand
        {
            get
            {
                if (null == _PogoCardVacOffCommand) _PogoCardVacOffCommand = new AsyncCommand(PogoCardVacOffCommandFunc);
                return _PogoCardVacOffCommand;
            }
        }
        private async Task PogoCardVacOffCommandFunc()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
        }
        #endregion

        #region ==> PogoCard Release on Command
        private AsyncCommand _PogoCardReleaseOnCommand;
        public ICommand PogoCardReleaseOnCommand
        {
            get
            {
                if (null == _PogoCardReleaseOnCommand) _PogoCardReleaseOnCommand = new AsyncCommand(PogoCardReleaseOnCommandFunc);
                return _PogoCardReleaseOnCommand;
            }
        }
        private async Task PogoCardReleaseOnCommandFunc()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
        }
        #endregion

        #region ==> PogoCard Release off Command
        private AsyncCommand _PogoCardReleaseOffCommand;
        public ICommand PogoCardReleaseOffCommand
        {
            get
            {
                if (null == _PogoCardReleaseOffCommand) _PogoCardReleaseOffCommand = new AsyncCommand(PogoCardReleaseOffCommandFunc);
                return _PogoCardReleaseOffCommand;
            }
        }
        private async Task PogoCardReleaseOffCommandFunc()
        {
            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
        }
        #endregion

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



        private int _CardDockRepeatCount = 0;
        public int CardDockRepeatCount
        {
            get { return _CardDockRepeatCount; }
            set
            {
                if (value != _CardDockRepeatCount)
                {
                    _CardDockRepeatCount = value;
                    RaisePropertyChanged();
                }
            }
        }






        #region ==> 
        private ICardChangeSysParam _CardChangeParam;
        public ICardChangeSysParam CardChangeParam
        {
            get { return _CardChangeParam; }
            set
            {
                if (value != _CardChangeParam)
                {
                    _CardChangeParam = value;
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
        private void CardContactSettingZCommandFunc()
        {
            switch (this.CardChangeModule().GetCCType())
            {
                case EnumCardChangeType.DIRECT_CARD:
                    CardChangeParam.GP_ContactCorrectionZ = CardContactOffsetSettingZ;
                    CardChangeParam.GP_Undock_ContactCorrectionZ = CardUndockContactOffsetSettingZ;
                    var ret = this.CardChangeModule().SaveSysParameter();
                    if (ret != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
                        return;
                    }
                    break;
                default:
                    break;
            }
           
        }
        #endregion




        //#region ==> CardContactCorrectionY
        //private double _CardContactCorrectionY;
        //public double CardContactCorrectionY
        //{
        //    get { return _CardContactCorrectionY; }
        //    set
        //    {
        //        if (value != _CardContactCorrectionY)
        //        {
        //            _CardContactCorrectionY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        // #endregion

        //#region ==> CorrectionMoveXCommand
        //private RelayCommand _CorrectionMoveXCommand;
        //public ICommand CorrectionMoveXCommand
        //{
        //    get
        //    {
        //        if (null == _CorrectionMoveXCommand) _CorrectionMoveXCommand = new RelayCommand(CorrectionMoveXCommandFunc);
        //        return _CorrectionMoveXCommand;
        //    }
        //}
        //private void CorrectionMoveXCommandFunc()
        //{
        //    double curXPos = 0;
        //    this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);

        //    double absXPos = curXPos + CardContactCorrectionX;

        //    ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
        //    if (absXPos > xAxis.Param.PosSWLimit.Value)
        //    {
        //        LoggerManager.Error($"[FAIL] {this.GetType().Name} : X, SW Limit, {absXPos}");
        //        return;
        //    }
        //    if (absXPos < xAxis.Param.NegSWLimit.Value)
        //    {
        //        LoggerManager.Error($"[FAIL] {this.GetType().Name} : X, SW Limit, {absXPos}");
        //        return;
        //    }

        //    EventCodeEnum result = this.MotionManager().AbsMove(xAxis, absXPos, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
        //    if(result != EventCodeEnum.NONE)
        //    {
        //        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Motion Error, {absXPos}");
        //        return;
        //    }

        //    CardChangeParam.GP_ContactCorrectionX += CardContactCorrectionX;
        //    var ret = this.CardChangeModule().SaveSysParameter();
        //    if (ret != EventCodeEnum.NONE)
        //    {
        //        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
        //        return;
        //    }
        //}
        //#endregion

        //#region ==> CorrectionMoveYCommand
        //private RelayCommand _CorrectionMoveYCommand;
        //public ICommand CorrectionMoveYCommand
        //{
        //    get
        //    {
        //        if (null == _CorrectionMoveYCommand) _CorrectionMoveYCommand = new RelayCommand(CorrectionMoveYCommandFunc);
        //        return _CorrectionMoveYCommand;
        //    }
        //}
        //private void CorrectionMoveYCommandFunc()
        //{
        //    double curYPos = 0;
        //    this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curYPos);

        //    double absYPos = curYPos + CardContactCorrectionY;

        //    ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
        //    if (absYPos > yAxis.Param.PosSWLimit.Value)
        //    {
        //        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Y, SW Limit, {absYPos}");
        //        return;
        //    }
        //    if (absYPos < yAxis.Param.NegSWLimit.Value)
        //    {
        //        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Y, SW Limit, {absYPos}");
        //        return;
        //    }

        //    EventCodeEnum result = this.MotionManager().AbsMove(yAxis, absYPos, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
        //    if (result != EventCodeEnum.NONE)
        //    {
        //        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Motion Error, {absYPos}");
        //        return;
        //    }


        //    CardChangeParam.GP_ContactCorrectionY += CardContactCorrectionY;
        //    var ret = this.CardChangeModule().SaveSysParameter();
        //    if (ret != EventCodeEnum.NONE)
        //    {
        //        LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Save Parameter Error");
        //        return;
        //    }
        //}
        //#endregion

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
        private void RotateChuckCommandFunc()
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

   

        private bool _TestRun = false;
        public VmGPCardChangeSetting()
        {
            //==> INPUT
            {
                //==> mani up , Lock = 0, UnLock = 1 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_UP);
                //==>  mani down Lock = 1, UnLock = 0
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_DOWN);
                //==> tester degree, 0 Degree = UnLock = 1, 180 Degree = Lock = 0;
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITESTER_HEAD_HORI);
                //==> mb - tester connection, Lock = 1, UnLock = 0 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_MBLOCK);
                //==> mb - pcard connection, Lock = 0, Unlock =1 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_PBUNLOCK);
                //==> mb - pcard connection, Lock = 1, Unlock =0 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_PBLOCK);
                //==> tester head lock, Lock = 1, Unlock =0 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_LOCK);
                //==> tester head unlock, unlock = 1, lock =0 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DITH_UNLOCK);
                //==> tester not use clamp, notuse = 1, use =0 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DINO_CLAMP);
                //==> tester head clamp, lock = 1, unlock =0 
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DICLP_LOCK);

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
                //==> card pod 에 달린 touch 센서  Exist = 1, Not exist = 0
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L);
                //==> card pod 에 달린 touch 센서  Exist = 1, Not exist = 0
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R);
                //==> topplate쪽 Exist = 1, Not exist = 0
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE);
                //==> Chuck쪽 Card 흡착 상태, Exist = 1, NOT Exist = 0
                CardChangeInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR);          
                
            }

            //==> OUTPUT
            {
                //==> mani 고정.
                CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOMANILOCK);

                //==> tester head 고정.
                CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOTESTER_HEAD_LOCK);
                //==> tester head 고정 해제.
                CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOTESTER_HEAD_UNLOCK);

                //==> zif command
                CardChangeOutputs.Add(this.IOManager().IO.Outputs.DOZIF_LOCK_REQ);

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



            UIEnable = true;
            ChuckThetaValue = 0;

            ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
            if (cardChangeParam == null)
            {
                LoggerManager.Debug($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                Debugger.Break();
                return;
            }

         
            CardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
        }
    }
}