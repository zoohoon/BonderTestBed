using System;
using System.Threading.Tasks;

namespace NeedleCleanManualPageViewModel
{
    using ProberInterfaces;
    using System.ComponentModel;
    using ProberErrorCode;
    using NeedleCleanerModuleParameter;
    using System.Runtime.CompilerServices;
    using NeedleCleanViewer;
    using SubstrateObjects;
    using System.Windows.Input;
    using RelayCommandBase;
    using LogModule;
    using VirtualKeyboardControl;
    using System.Windows;
    using ProberInterfaces.PnpSetup;
    using MetroDialogInterfaces;
    using ProberInterfaces.State;
    using ProberInterfaces.Command.Internal;
    using System.Threading;
    using ProberInterfaces.Command;

    public class VmNeedleCleanManualPage : IMainScreenViewModel, INotifyPropertyChanged, IFactoryModule, ISetUpState
    {
        readonly Guid _ViewModelGUID = new Guid("370c0268-621f-42b5-b915-0174470b1e94");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IStateModule _NeedleCleanModule;
        public IStateModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set
            {
                if (value != _NeedleCleanModule)
                {
                    _NeedleCleanModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int ViewNUM;
        public int _ViewNUM
        {
            get { return _ViewNUM; }
            set
            {
                if (value != _ViewNUM)
                {
                    _ViewNUM = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ViewNUMx2;
        public bool ViewNUMx2
        {
            get { return _ViewNUMx2; }
            set
            {
                if (value != _ViewNUMx2)
                {
                    _ViewNUMx2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsItDisplayed2RateMagnification;
        public bool IsItDisplayed2RateMagnification
        {
            get { return _IsItDisplayed2RateMagnification; }
            set
            {
                if (value != _IsItDisplayed2RateMagnification)
                {
                    _IsItDisplayed2RateMagnification = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanDeviceParameter _NeedleCleanDevParam;
        public NeedleCleanDeviceParameter NeedleCleanDevParam
        {
            get { return _NeedleCleanDevParam; }
            set
            {
                if (value != _NeedleCleanDevParam)
                {
                    _NeedleCleanDevParam = value;
                    RaisePropertyChanged("");
                }
            }
        }

        public NeedleCleanObject NC
        {
            get { return this.StageSupervisor().NCObject as NeedleCleanObject; }
        }

        private NeedleCleanSystemParameter _NCParam;
        public NeedleCleanSystemParameter NCParam
        {
            get { return (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam; }
            set
            {
                if (value != _NCParam)
                {
                    _NCParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanViewModel _NeedleCleanVM;
        public NeedleCleanViewModel NeedleCleanVM
        {
            get { return _NeedleCleanVM; }
            set
            {
                if (value != _NeedleCleanVM)
                {
                    _NeedleCleanVM = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get { return _Stage3DModel; }
            set
            {
                if (value != _Stage3DModel)
                {
                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }


        //private string _CameraPosition = "-515.765208251643,761.364818848568,488.886585805774";
        //public string CameraPosition
        //{
        //    get { return _CameraPosition; }
        //    set
        //    {
        //        if (value != _CameraPosition)
        //        {
        //            _CameraPosition = value;
        //            RaisePropertyChanged("PadSizeLeft");
        //        }
        //    }
        //}

        //private string _CameraLookDirection = "0.66941852206432,-0.566550936668482,-0.48051938408069";
        //public string CameraLookDirection
        //{
        //    get { return _CameraLookDirection; }
        //    set
        //    {
        //        if (value != _CameraLookDirection)
        //        {
        //            _CameraLookDirection = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private string _CameraUpDirection = "0.460251689958924,0.824026720537669,-0.330376067126373";
        //public string CameraUpDirection
        //{
        //    get { return _CameraUpDirection; }
        //    set
        //    {
        //        if (value != _CameraUpDirection)
        //        {
        //            _CameraUpDirection = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public IStageSupervisor StageSupervisor { get { return this.StageSupervisor(); } }

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
                    NeedleCleanModule = this.NeedleCleaner();

                    var ncDevModule = (IHasDevParameterizable)NeedleCleanModule;
                    var ncSysModule = (IHasSysParameterizable)NeedleCleanModule;

                    if (this.NeedleCleaner() != null)
                    {
                        NeedleCleanDevParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;
                    }

                    //nvm = new NeedleCleanViewModel((ProbeCard)this.StageSupervisor().ProbeCardInfo,
                    //    (NeedleCleanDeviceParameter)module.DevParam,
                    //    (NeedleCleanSystemParameter)module.SysParam,
                    //    (NeedleCleanObject)this.StageSupervisor().NCObject,
                    //    (WaferObject)this.StageSupervisor().WaferObject
                    //    );
                    NeedleCleanVM = new NeedleCleanViewModel();

                    /*this.StageSupervisor().ProbeCardInfo,
                        NeedleCleanDevParam,
                        (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam,
                        (NeedleCleanObject)this.StageSupervisor().NCObject,
                        (WaferObject)this.StageSupervisor().WaferObject); */

                    //ViewModelManager = this.ViewModelManager();                    
                    ViewModelManager = this.ViewModelManager();
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                    ViewNUM = 0;
                    CenterView();
                    IsItDisplayed2RateMagnification = false;
                    PosRefresh();

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

        public IViewModelManager ViewModelManager { get; set; }
        public void Viewx2() // 2x view
        {
            try
            {
                IsItDisplayed2RateMagnification = !IsItDisplayed2RateMagnification;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CWVIEW() //CW
        {
            try
            {
                ViewNUM = ((Enum.GetNames(typeof(CameraViewPoint)).Length) + (--ViewNUM)) % Enum.GetNames(typeof(CameraViewPoint)).Length;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);

                //ViewNUM = ViewNUM - 1;
                //if (ViewNUM < 0)
                //{
                //    ViewNUM = 7;
                //}
                //ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CenterView() //FRONT
        {
            try
            {
                ViewNUM = 0;
                IsItDisplayed2RateMagnification = false;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);

                //ViewNUM = 0;
                //ViewNUMx2 = false;
                //ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CCWView() // CCW
        {
            try
            {
                ViewNUM = Math.Abs(++ViewNUM % Enum.GetNames(typeof(CameraViewPoint)).Length);
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);

                //ViewNUM = ViewNUM + 1;
                //if (ViewNUM > 7)
                //{
                //    ViewNUM = 0;
                //}
                //ViewerCont();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private RelayCommand _X2ViewChangeCommand;
        public RelayCommand X2ViewChangeCommand
        {
            get
            {
                if (null == _X2ViewChangeCommand) _X2ViewChangeCommand = new RelayCommand(Viewx2);
                return _X2ViewChangeCommand;
            }
        }
        private RelayCommand _CWViewChangeCommand;
        public ICommand CWViewChangeCommand
        {
            get
            {
                if (null == _CWViewChangeCommand) _CWViewChangeCommand = new RelayCommand(CWVIEW);
                return _CWViewChangeCommand;
            }
        }


        private RelayCommand _CenterViewChangeCommand;
        public ICommand CenterViewChangeCommand
        {
            get
            {
                if (null == _CenterViewChangeCommand) _CenterViewChangeCommand = new RelayCommand(CenterView);
                return _CenterViewChangeCommand;
            }
        }


        private RelayCommand _CCWViewChangeCommand;
        public ICommand CCWViewChangeCommand
        {
            get
            {
                if (null == _CCWViewChangeCommand) _CCWViewChangeCommand = new RelayCommand(CCWView);
                return _CCWViewChangeCommand;
            }
        }


        //public void ViewerCont()
        //{
        //    try
        //    {
        //        if (ViewNUMx2 == false)
        //        {
        //            if (ViewNUM == 0) // FRONT VIEW
        //            {
        //                ViewModelManager.CamPosition = new Point3D(121.4, 1487.7, 1830.4);
        //                ViewModelManager.CamLookDirection = new Vector3D(0, -0.7, -0.8);
        //                ViewModelManager.CamUpDirection = new Vector3D(0, 0.8, -0.6);
        //            }
        //            else if (ViewNUM == 1) // FOUP
        //            {
        //                ViewModelManager.CamPosition = new Point3D(1419.1, 1487.7, 1298.2);
        //                ViewModelManager.CamLookDirection = new Vector3D(-0.5, -0.6, -0.6);
        //                ViewModelManager.CamUpDirection = new Vector3D(-0.4, 0.8, -0.4);
        //            }
        //            else if (ViewNUM == 2) // LOADER
        //            {
        //                ViewModelManager.CamPosition = new Point3D(1960.4, 1487.7, 4.2);
        //                ViewModelManager.CamLookDirection = new Vector3D(-0.8, -0.6, 0);
        //                ViewModelManager.CamUpDirection = new Vector3D(-0.6, 0.8, 0);
        //            }
        //            else if (ViewNUM == 3) // LOADER BACK
        //            {
        //                ViewModelManager.CamPosition = new Point3D(1535.8, 1487.7, -1175.2);
        //                ViewModelManager.CamLookDirection = new Vector3D(-0.6, -0.6, 0.5);
        //                ViewModelManager.CamUpDirection = new Vector3D(-0.5, 0.8, 0.4);
        //            }
        //            else if (ViewNUM == 4) // BACK
        //            {
        //                ViewModelManager.CamPosition = new Point3D(134.3, 1487.7, -1834.7);
        //                ViewModelManager.CamLookDirection = new Vector3D(0, -0.6, 0.8);
        //                ViewModelManager.CamUpDirection = new Vector3D(0, 0.8, 0.6);
        //            }
        //            else if (ViewNUM == 5) // STAGE BACK
        //            {
        //                ViewModelManager.CamPosition = new Point3D(-1271.8, 1487.7, -1185);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.6, -0.6, 0.5);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.5, 0.8, 0.4);
        //            }
        //            else if (ViewNUM == 6) // STAGE 
        //            {
        //                ViewModelManager.CamPosition = new Point3D(-1704.7, 1487.7, -8.6);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.8, -0.6, 0);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.6, 0.8, 0);
        //            }
        //            else if (ViewNUM == 7) // STAGE 
        //            {
        //                ViewModelManager.CamPosition = new Point3D(-1055, 1487.7, 1397.5);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.5, -0.6, -0.6);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.4, 0.8, -0.5);
        //            }
        //            else
        //            {

        //            }
        //        }
        //        else if (ViewNUMx2 == true)
        //        {
        //            if (ViewNUM == 0) // FRONT VIEW x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(123, 1148, 1412);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.0, -0.6, -0.8);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.0, 0.8, -0.6);
        //            }
        //            else if (ViewNUM == 1) // FOUP x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(1208, 1148, 911);
        //                ViewModelManager.CamLookDirection = new Vector3D(-0.6, -0.6, -0.5);
        //                ViewModelManager.CamUpDirection = new Vector3D(-0.5, 0.8, -0.4);
        //            }
        //            else if (ViewNUM == 2) // LOADER x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(1542, 1148, 3);
        //                ViewModelManager.CamLookDirection = new Vector3D(-0.8, -0.6, 0.0);
        //                ViewModelManager.CamUpDirection = new Vector3D(-0.6, 0.8, 0.0);
        //            }
        //            else if (ViewNUM == 3) // LOADER BACK x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(1041, 1148, -1083);
        //                ViewModelManager.CamLookDirection = new Vector3D(-0.5, -0.6, 0.6);
        //                ViewModelManager.CamUpDirection = new Vector3D(-0.4, 0.8, 0.5);
        //            }
        //            else if (ViewNUM == 4) // BACK x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(133, 1148, -1417);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.0, -0.6, 0.8);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.0, 0.8, 0.6);
        //            }
        //            else if (ViewNUM == 5) // STAGE BACK x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(-1095, 1148, -714);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.7, -0.6, 0.4);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.5, 0.8, 0.3);
        //            }
        //            else if (ViewNUM == 6) // STAGE x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(-1287, 1148, -7);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.8, -0.6, 0.0);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.6, 0.8, 0.0);
        //            }
        //            else if (ViewNUM == 7) // STAGE  x2
        //            {
        //                ViewModelManager.CamPosition = new Point3D(-876, 1148, 994);
        //                ViewModelManager.CamLookDirection = new Vector3D(0.6, -0.6, -0.5);
        //                ViewModelManager.CamUpDirection = new Vector3D(0.4, 0.8, -0.4);
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        private IZoomObject _ZoomObject;

        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set { _ZoomObject = value; }
        }
        public WaferObject Wafer { get { return this.StageSupervisor().WaferObject as WaferObject; } }
        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ZoomObject = Wafer;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //this.SysState().SetSetUpState();
                PosRefresh();
                NeedleCleanVM = new NeedleCleanViewModel(); /* this.StageSupervisor().ProbeCardInfo,
                      NeedleCleanDevParam,
                      (NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam,
                      (NeedleCleanObject)this.StageSupervisor().NCObject,
                      (WaferObject)this.StageSupervisor().WaferObject); */

                //CenterView();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stage3DModel = null;
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                });
                CenterView();
                IsItDisplayed2RateMagnification = false;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        private RelayCommand<object> _SwitchPage;
        public ICommand SwitchPage
        {
            get
            {
                if (null == _SwitchPage) _SwitchPage = new RelayCommand<object>(PageSwitching);
                return _SwitchPage;
            }
        }

        private void PageSwitching(object obj)
        {
            try
            {
                this.ViewModelManager().ViewTransitionAsync(new Guid(obj.ToString()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        #region ==>CleaningCommand        
        private AsyncCommand _CleaningCommand;
        public ICommand CleaningCommand
        {
            get
            {
                if (null == _CleaningCommand) _CleaningCommand = new AsyncCommand(CleaningCommandFunc);
                return _CleaningCommand;
            }
        }

        private async Task CleaningCommandFunc()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            try
            {
                bool checker1 = false;
                bool checker2 = false;

                for (int i = 0; i <= NC.NCSysParam.MaxCleanPadNum.Value - 1; i++)
                {
                    if (NeedleCleanDevParam.SheetDevs[i].Enabled.Value == true)
                    {
                        checker1 = true;
                    }

                    if (NC.NCSysParam.ManualNC.EnableCleaning.Value[i] == true)
                    {
                        checker2 = true;
                    }
                }

                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() != AlignStateEnum.DONE)
                {
                    if (NC.NCSysParam.ManualNC.EnablePinAlignBeforeNC.Value != true)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Pin alignment is not done yet", EnumMessageStyle.Affirmative, "OK");
                        return;
                    }
                }

                // 모두 다 disabled 이라는 경고 메세지 띄워야 함
                if (checker1 == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Could not start cleaning because cleaning function is not enabled in recipe", EnumMessageStyle.Affirmative, "OK");
                    return;
                }

                if (checker2 == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Could not start cleaning because you did not select any cleaning operation yet", EnumMessageStyle.Affirmative, "OK");
                    return;
                }

                if (NC.NCSysParam.ManualNC.EnablePinAlignBeforeNC.Value == true)
                {
                    this.PinAligner().PinAlignSource = PINALIGNSOURCE.NEEDLE_CLEANING;

                    //await Task.Run(() => this.PinAligner().DoPinAlignProcess());

                    // TODO : 동작 잘 되는지, 소스는 맞게 도는지

                    var sendModule = this.NeedleCleaner();
                    var targetModule = this.PinAligner();

                    var injgeted = this.CommandManager().SetCommand<IDOPINALIGN>(this.NeedleCleaner());

                    if (injgeted)
                    {
                        while (true)
                        {
                            if (sendModule.CommandSendSlot.Token.SubjectInfo == targetModule.CommandRecvDoneSlot.Token.SubjectInfo)
                            {
                                if (targetModule.ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                {
                                    if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.DONE)
                                    {
                                        this.PinAligner().CommandRecvDoneSlot.ClearToken();
                                        break;
                                        
                                        if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                                        {
                                            // 성공
                                        }
                                        else
                                        {
                                            // 실패
                                        }
                                    }
                                    else if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ABORTED ||
                                             targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ERROR)
                                    {
                                        // 실패
                                        break;
                                    }
                                    else if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.NOCOMMAND)
                                    {
                                        // 실패
                                        break;
                                    }
                                    else if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.REJECTED)
                                    {
                                        // 실패
                                        break;
                                    }
                                }
                            }

                            Thread.Sleep(30);
                        }
                    }
                }

                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() != AlignStateEnum.DONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Pin alignment is failed", EnumMessageStyle.Affirmative, "OK");
                    return;
                }

                await Task.Run(() => retval = this.NeedleCleaner().DoNeedleCleaningProcess());

                if (retval != EventCodeEnum.NONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Needle cleaning operation is failed", EnumMessageStyle.Affirmative, "OK");
                    return;
                }

                if (NC.NCSysParam.ManualNC.EnablePinAlignAfterNC.Value == true)
                {
                    this.PinAligner().PinAlignSource = PINALIGNSOURCE.NEEDLE_CLEANING;

                    //await Task.Run(() => this.PinAligner().DoPinAlignProcess());

                    var sendModule = this.NeedleCleaner();
                    var targetModule = this.PinAligner();

                    var injgeted = this.CommandManager().SetCommand<IDOPINALIGN>(this.NeedleCleaner());

                    if (injgeted)
                    {
                        while (true)
                        {
                            if (sendModule.CommandSendSlot.Token.SubjectInfo == targetModule.CommandRecvDoneSlot.Token.SubjectInfo)
                            {
                                if (targetModule.ModuleState.GetState() != ModuleStateEnum.RUNNING)
                                {
                                    if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.DONE)
                                    {
                                        this.PinAligner().CommandRecvDoneSlot.ClearToken();
                                        break;

                                        if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                                        {
                                            // 성공
                                        }
                                        else
                                        {
                                            // 실패
                                        }
                                    }
                                    else if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ABORTED ||
                                             targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.ERROR)
                                    {
                                        // 실패
                                        break;
                                    }
                                    else if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.NOCOMMAND)
                                    {
                                        // 실패
                                        break;
                                    }
                                    else if (targetModule.CommandRecvDoneSlot.Token.GetState() == CommandStateEnum.REJECTED)
                                    {
                                        // 실패
                                        break;
                                    }
                                }
                            }

                            Thread.Sleep(30);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. Manual cleaning operation is failed in CleaningCommandFunc()");
                LoggerManager.Exception(err);
            }
            finally
            {

            }
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

        #endregion

        #region ==> TextBoxClickCommand
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
        #endregion
        private async Task PosRefresh()
        {
            try
            {
                IMotionManager Motionmanager = this.MotionManager();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
    }
}
