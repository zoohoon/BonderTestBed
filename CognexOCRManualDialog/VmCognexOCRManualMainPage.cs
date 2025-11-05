using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognexOCRManualDialog
{
    using Autofac;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LoaderControllerBase;
    using LoaderParameters;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Lamp;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    public class VmCognexOCRManualMainPage : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
        private bool _SetBuzzer;
        public bool SetBuzzer
        {
            get { return _SetBuzzer; }
            set
            {
                if (value != _SetBuzzer)
                {
                    _SetBuzzer = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> OCRString
        private String _OCRString;
        public String OCRString
        {
            get { return _OCRString; }
            set
            {
                if (value != _OCRString)
                {
                    _OCRString = value;
                    if (_OCRString == String.Empty)
                        OCRInputBoxBrush = Brushes.LightGray;

                    CurTextLength = OCRString.Length;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> OCRInputBoxBrush
        private Brush _OCRInputBoxBrush;
        public Brush OCRInputBoxBrush
        {
            get { return _OCRInputBoxBrush; }
            set
            {
                if (value != _OCRInputBoxBrush)
                {
                    _OCRInputBoxBrush = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> EnterKeyCommand
        private AsyncCommand _EnterKeyCommand;
        public ICommand EnterKeyCommand
        {
            get
            {
                if (null == _EnterKeyCommand) _EnterKeyCommand = new AsyncCommand(EnterKeyCommandFunc);
                return _EnterKeyCommand;
            }
        }
        private async Task EnterKeyCommandFunc()
        {
            bool cheksumResult = false;
            String upperOCRString = OCRString.ToUpper();

            switch (_CognexProcessManager.CognexProcDevParam.GetManualConfig().CheckSum)
            {
                case "2"://==> SEMI
                    cheksumResult = SEMIChecksum(upperOCRString);
                    break;
                case "4"://==> IBM
                    cheksumResult = IMBChecksum(upperOCRString);
                    break;
                default:
                    cheksumResult = true;
                    break;
            }

            if (cheksumResult == false)
            {
                OCRInputBoxBrush = Brushes.Red;
                return;
            }

            do
            {
                if (this.LoaderController().OFR_SetOcrID(upperOCRString) == EventCodeEnum.UNDEFINED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("OCR", "[ERROR] set ocr fault", EnumMessageStyle.Affirmative);

                    break;
                }
                if (this.LoaderController().OFR_OCRRemoteEnd() == EventCodeEnum.UNDEFINED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("OCR", "[ERROR] set ocr fault", EnumMessageStyle.Affirmative);
                    break;
                }
            } while (false);

            //_Win.Close();
            await CloseWindow(true);
        }
        #endregion

        #region ==> ExitCommand
        private AsyncCommand _ExitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new AsyncCommand(ExitCommandFunc);
                return _ExitCommand;
            }
        }

        private async Task ExitCommandFunc()
        {
            //_Win.Close();
            await CloseWindow(false);
        }
        #endregion

        #region ==> ConnectDisplayCommand
        private RelayCommand _ConnectDisplayCommand;
        public ICommand ConnectDisplayCommand
        {
            get
            {
                if (null == _ConnectDisplayCommand) _ConnectDisplayCommand = new RelayCommand(ConnectDisplayCommandFunc);
                return _ConnectDisplayCommand;
            }
        }
        private void ConnectDisplayCommandFunc()
        {
            if (_CognexProcessManager.ConnectDisplay(_CognexIP).Result == false)
                return;

        }
        #endregion

        #region ==> LIGHT GROUP

        #region ==> LightModeList
        private ObservableCollection<VmComboBoxItem> _LightModeList;
        public ObservableCollection<VmComboBoxItem> LightModeList
        {
            get { return _LightModeList; }
            set
            {
                if (value != _LightModeList)
                {
                    _LightModeList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedLight
        private VmComboBoxItem _SelectedLight;
        public VmComboBoxItem SelectedLight
        {
            get { return _SelectedLight; }
            set
            {
                if (value != _SelectedLight)
                {
                    _SelectedLight = value;
                    RaisePropertyChanged();

                    if (_SelectedLight == null)
                        return;

                    _CognexProcessManager.DO_SetConfigLightMode(_CognexIP, (String)_SelectedLight.CommandArg);
                    UpdateImage();

                    SaveLightModeConfig();
                }
            }
        }
        #endregion

        #region ==> LightIntensity
        private int _LightIntensity;
        public int LightIntensity
        {
            get { return _LightIntensity; }
            set
            {
                if (value != _LightIntensity)
                {
                    _LightIntensity = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ScrollStopCommand
        private RelayCommand _ScrollStopCommand;
        public ICommand ScrollStopCommand
        {
            get
            {
                if (null == _ScrollStopCommand) _ScrollStopCommand = new RelayCommand(ScrollStopCommandFunc);
                return _ScrollStopCommand;
            }
        }
        private void ScrollStopCommandFunc()
        {
            UpdateLight();
        }
        #endregion

        #region ==> LightNextTypeCommand
        private RelayCommand _LightNextTypeCommand;
        public ICommand LightNextTypeCommand
        {
            get
            {
                if (null == _LightNextTypeCommand) _LightNextTypeCommand = new RelayCommand(LightNextTypeCommandFunc);
                return _LightNextTypeCommand;
            }
        }
        private void LightNextTypeCommandFunc()
        {
            try
            {
                if (LightModeList != null)
                {
                    int idx = LightModeList.IndexOf(SelectedLight);
                    idx++;

                    if (idx > LightModeList.Count - 1)
                        return;

                    SelectedLight = LightModeList[idx];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightPrevTypeCommand
        private RelayCommand _LightPrevTypeCommand;
        public ICommand LightPrevTypeCommand
        {
            get
            {
                if (null == _LightPrevTypeCommand) _LightPrevTypeCommand = new RelayCommand(LightPrevTypeCommandFunc);
                return _LightPrevTypeCommand;
            }
        }
        private void LightPrevTypeCommandFunc()
        {
            try
            {
                if (LightModeList != null)
                {
                    int idx = LightModeList.IndexOf(SelectedLight);
                    idx--;

                    if (idx < 0)
                        return;

                    SelectedLight = LightModeList[idx];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightPowerIncreaseCommand
        private RelayCommand _LightPowerIncreaseCommand;
        public ICommand LightPowerIncreaseCommand
        {
            get
            {
                if (null == _LightPowerIncreaseCommand) _LightPowerIncreaseCommand = new RelayCommand(LightPowerIncreaseCommandFunc);
                return _LightPowerIncreaseCommand;
            }
        }
        private void LightPowerIncreaseCommandFunc()
        {
            try
            {
                if (LightIntensity + _ChangeSize > _MaxLightIntensity)
                    return;

                LightIntensity += _ChangeSize;

                UpdateLight();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightPowerDecreaseCommand
        private RelayCommand _LightPowerDecreaseCommand;
        public ICommand LightPowerDecreaseCommand
        {
            get
            {
                if (null == _LightPowerDecreaseCommand) _LightPowerDecreaseCommand = new RelayCommand(LightPowerDecreaseCommandFunc);
                return _LightPowerDecreaseCommand;
            }
        }
        private void LightPowerDecreaseCommandFunc()
        {
            try
            {
                if (LightIntensity - _ChangeSize < 0)
                    return;

                LightIntensity -= _ChangeSize;

                UpdateLight();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> BuzzerOff
        private RelayCommand _TurnOffBuzzer;
        public ICommand TurnOffBuzzer
        {
            get
            {
                if (null == _TurnOffBuzzer) _TurnOffBuzzer = new RelayCommand(TurnOffBuzzerFunc);
                return _TurnOffBuzzer;
            }
        }
        private void TurnOffBuzzerFunc()
        {
            try
            {
                SetBuzzer = false;

                this.LampManager().WrappingSetBuzzerStatus(LampStatusEnum.Off);

                //using (LampBarrier lampBarrier = new LampBarrier(
                //    LampStatusEnum.Off,
                //    LampStatusEnum.BlinkOn,
                //    LampStatusEnum.Off,
                //    LampStatusEnum.Off,
                //    AlarmPriority.Warning, sender: "Cognex Manual"))
                //{
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        private Object lockObject = new Object();
        private readonly int _ChangeSize = 1;
        private readonly int _MaxLightIntensity = 255;
        private void SetupLight()
        {
            try
            {
                if (_CognexProcessManager.DO_GetConfigEx(_CognexIP) == false)
                    return;

                LightModeList = new ObservableCollection<VmComboBoxItem>();
                LightModeList.Add(new VmComboBoxItem("Mode0", "0"));
                LightModeList.Add(new VmComboBoxItem("Mode1", "1"));
                LightModeList.Add(new VmComboBoxItem("Mode2", "2"));
                LightModeList.Add(new VmComboBoxItem("Mode3", "3"));
                LightModeList.Add(new VmComboBoxItem("Mode4", "4"));
                LightModeList.Add(new VmComboBoxItem("Mode5", "5"));
                LightModeList.Add(new VmComboBoxItem("Mode6", "6"));
                LightModeList.Add(new VmComboBoxItem("Mode7", "7"));
                LightModeList.Add(new VmComboBoxItem("Mode8", "8"));
                LightModeList.Add(new VmComboBoxItem("Mode9", "9"));
                LightModeList.Add(new VmComboBoxItem("Mode10", "10"));
                LightModeList.Add(new VmComboBoxItem("Mode11", "11"));
                LightModeList.Add(new VmComboBoxItem("Custom", "12"));
                LightModeList.Add(new VmComboBoxItem("External", "13"));
                LightModeList.Add(new VmComboBoxItem("Expansion", "14"));

                UpdateLightUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void UpdateLight()
        {
            _CognexProcessManager.Async_SetConfigLightPower(_CognexIP, LightIntensityConvert.ToString(), UpdateImage);
        }
        private void UpdateLightUI()
        {
            try
            {
                String currentLightMode = _CognexProcessManager.CognexCommandManager.GetConfigEx.LightMode;

                String valueString = _CognexProcessManager.CognexCommandManager.GetConfigEx.LightPower.Split('.')[0];

                int lightIntensity = 0;
                int.TryParse(valueString, out lightIntensity);

                LightIntensity = lightIntensity;

                SelectedLight = LightModeList.FirstOrDefault(item => item.CommandArg.ToString() == currentLightMode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private bool LightPowerChangeCallBack()
        {
            UpdateImage();
            SaveLightIntensityConfig();
            return true;
        }
        private void SaveLightModeConfig()
        {
            try
            {
                if (_ManualConfig == null)
                    return;

                String lightModeValue = (String)_SelectedLight.CommandArg;
                _ManualConfig.Light = lightModeValue;
                _CognexProcessManager.SaveConfig();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SaveLightIntensityConfig()
        {
            try
            {
                if (_ManualConfig == null)
                    return;

                _ManualConfig.LightIntensity = LightIntensity;
                _CognexProcessManager.SaveConfig();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private double LightIntensityConvert
        {
            // _MaxLightIntensity(255) : LightIntensity  = 127 : x
            get { return LightIntensity * 127 / _MaxLightIntensity; }
        }
        #endregion

        #region ==> ImageSource
        private System.Windows.Media.Imaging.BitmapImage _ImageSource;
        public System.Windows.Media.Imaging.BitmapImage ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (value != _ImageSource)
                {
                    _ImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CurTextLength
        private int _CurTextLength;
        public int CurTextLength
        {
            get { return _CurTextLength; }
            set
            {
                if (value != _CurTextLength)
                {
                    _CurTextLength = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MaxTextLength
        private int _MaxTextLength;
        public int MaxTextLength
        {
            get { return _MaxTextLength; }
            set
            {
                if (value != _MaxTextLength)
                {
                    _MaxTextLength = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private void SetupUI()
        {
            SetupLight();
            UpdateImage();
            SetupConfig();
            SetBuzzer = true;
        }
        private bool UpdateImage()
        {
            _CognexProcessManager.DO_AcquireConfig(_CognexIP);
            if (_CognexProcessManager.DO_RI(_CognexIP) == false)
                return false;

            if (_CognexProcessManager.CognexCommandManager.CognexRICommand.Status != "1")
                return false;

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ImageSource = _CognexProcessManager.CognexCommandManager.CognexRICommand.GetBitmapImage();
            }));

            return true;
        }
        private void SetupConfig()
        {
            _ManualConfig = _CognexProcessManager.CognexProcDevParam.GetManualConfig();
        }


        private String _CognexIP;
        private CognexConfig _ManualConfig;
        public Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager;

        private RequestCombination CurrentCombination = null;

        readonly Guid _ViewModelGUID = new Guid("5bd9d105-fec3-45fc-a1bb-0d71e1d011a7");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        private Window _Win;
        public void SetWindow(Window win)
        {
            _Win = win;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized)
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                return EventCodeEnum.DUPLICATE_INVOCATION;
            }

            if (_Container == null)
            {
                _Container = this.LoaderController().GetLoaderContainer();
            }

            // 현재 Lamp 상태 기억
            CurrentCombination = this.LampManager().GetCurrentLampCombo();

            _CognexProcessManager = _Container.Resolve<ICognexProcessManager>();

            OCRString = String.Empty;
            OCRInputBoxBrush = Brushes.LightGray;

            if (Extensions_IParam.ProberRunMode != RunMode.EMUL)
            {
                _CognexIP = _CognexProcessManager.CognexProcSysParam.GetIPOrNull(_CognexProcessManager.CognexProcDevParam.CognexHostList[0].ModuleName);
            }

            Initialized = true;

            MaxTextLength = 15;

            retval = EventCodeEnum.NONE;

            return retval;
        }

        private async Task CloseWindow(bool IsEnter)
        {
            bool IsWindowClosed = false;
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // X 버튼을 통해 진입 시
                if (IsEnter == false)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _Win.Close();
                        IsWindowClosed = true;
                    }));

                    var dlgRel = await this.MetroDialogManager().ShowMessageDialog("[Operation]", $"Do you want to move the wafer to its original location?",
                                                                                    EnumMessageStyle.AffirmativeAndNegative, "YES", "NO");

                    if (dlgRel == EnumMessageDialogResult.AFFIRMATIVE) // YES
                    {
                        var _LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;

                        //var ocrModuleInfo = _LoaderControllerExt
                        //    .LoaderInfo.StateMap.CognexOCRModules
                        //    .Where(item => item.ID.ModuleType == ModuleTypeEnum.COGNEXOCR).FirstOrDefault();

                        //var paModuleInfo = _LoaderControllerExt
                        //    .LoaderInfo.StateMap.PreAlignModules
                        //    .Where(item => item.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();

                        var armModuleInfo = _LoaderControllerExt
                            .LoaderInfo.StateMap.ARMModules
                            .Where(item => item.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();

                        bool IsValid = false;

                        TransferObject target = armModuleInfo.Substrate;
                        ModuleID destPos = armModuleInfo.Substrate.OriginHolder;

                        if (target != null && destPos != null)
                        {
                            if (armModuleInfo.Substrate.CurrPos.ModuleType == ModuleTypeEnum.COGNEXOCR)
                            {
                                var OriginSubstrate = _LoaderControllerExt.LoaderInfo.StateMap.GetSubstrateByHolder(armModuleInfo.Substrate.OriginHolder);

                                if (OriginSubstrate == null)
                                {
                                    if (armModuleInfo.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                    {
                                        if (_LoaderControllerExt.LoaderInfo.StateMap.CassetteModules[0].ScanState == CassetteScanStateEnum.READ)
                                        {
                                            // 전송 가능
                                            IsValid = true;
                                        }
                                    }
                                    else if (armModuleInfo.Substrate.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY)
                                    {
                                        // 전송 가능
                                        IsValid = true;
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"[VmCognexOCRManualMainPage], CloseWindow() : Origin holder = {armModuleInfo.Substrate.OriginHolder.ModuleType}");
                                    }
                                }
                            }
                        }

                        if (IsValid == true)
                        {
                            //string ManualCloseStr = "*********";

                            //this.LoaderController().OFR_SetOcrID(ManualCloseStr);

                            //this.LoaderController().OFR_OCRRemoteEnd();

                            this.LoaderController().OFR_OCRAbort();

                            LoaderMapEditor editor = new LoaderMapEditor(_LoaderControllerExt.LoaderInfo.StateMap);
                            //LoaderMapEditor editor = _LoaderControllerExt.GetLoaderMapEditor();
                            editor.EditorState.SetTransfer(target.ID.Value, destPos);

                            LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                            cmdParam.Editor = editor;
                            
                            // OCR State를 Done으로 바꿨지만, LoaderController State는 아직 
                            bool isInjected = this.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);

                            if (isInjected)
                            {
                                await Task.Run(() =>
                                {
                                    this.LoaderController().WaitForCommandDone();
                                });

                                retval = EventCodeEnum.NONE;
                            }
                            else
                            {
                                LoggerManager.Error($"[VmCognexOCRManualMainPage], CloseWindow() : isInjected : {isInjected}");
                            }
                        }

                        // 창이 닫힌 후, 기억해놓은 Combo를 통해, Lamp 상태 원복

                        if (CurrentCombination != null)
                        {
                            this.LampManager().SetRequestLampCombo(CurrentCombination);
                        }
                        else
                        {
                            LoggerManager.Debug($"[VmCognexOCRManualMainPage] CloseWindow() : CurrentCombination is null.");
                        }
                    }
                    else if (dlgRel == EnumMessageDialogResult.NEGATIVE) // NO
                    {
                    }
                    else
                    {
                        LoggerManager.Error($"[VmCognexOCRManualMainPage], CloseWindow() : dlgRel = {dlgRel}");
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _Win.Close();
                        IsWindowClosed = true;
                    }));

                    // 창이 닫힌 후, 기억해놓은 Combo를 통해, Lamp 상태 원복

                    if (CurrentCombination != null)
                    {
                        this.LampManager().SetRequestLampCombo(CurrentCombination);
                    }
                    else
                    {
                        LoggerManager.Debug($"[VmCognexOCRManualMainPage] CloseWindow() : CurrentCombination is null.");
                    }

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (IsWindowClosed == true && retval == EventCodeEnum.UNDEFINED)
                {
                    CognexManualInput.AsyncShow(this.GetContainer());
                }
            }
        }

        //public EventCodeEnum InitModule(int hostIndex)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    if (Initialized)
        //    {
        //        LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
        //        return EventCodeEnum.DUPLICATE_INVOCATION;
        //    }

        //    if (_Container == null)
        //    {
        //        _Container = this.LoaderController().GetLoaderContainer();
        //    }
        //    _CognexProcessManager = _Container.Resolve<ICognexProcessManager>();

        //    OCRString = String.Empty;
        //    OCRInputBoxBrush = Brushes.LightGray;
        //    _CognexIP =
        //        _CognexProcessManager.CognexProcSysParam.GetIPOrNull(_CognexProcessManager.CognexProcDevParam.CognexHostList[0].ModuleName);

        //    Initialized = true;

        //    MaxTextLength = 15;

        //    retval = EventCodeEnum.NONE;

        //    return retval;
        //}
        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
                Task<EventCodeEnum> t = new Task<EventCodeEnum>(() => { return EventCodeEnum.NONE; });

                if (_CognexProcessManager.IsInit())
                    SetupUI();

                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                UpdateImage();
                Task<EventCodeEnum> t = new Task<EventCodeEnum>(() =>
                {
                    return EventCodeEnum.NONE;
                });

                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            try
            {
                Task<EventCodeEnum> t = new Task<EventCodeEnum>(() =>
                {
                    return EventCodeEnum.NONE;
                });

                return t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void DeInitModule()
        {
        }
        public bool IsWaferExistOnOCR()
        {
            try
            {
                ILoaderControllerExtension loaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
                if(loaderControllerExt != null)
                {
                    var ocrModuleInfo = loaderControllerExt
                    .LoaderInfo.StateMap.CognexOCRModules
                    .Where(item => item.ID.ModuleType == ModuleTypeEnum.COGNEXOCR).FirstOrDefault();

                    var armModuleInfo = loaderControllerExt
                        .LoaderInfo.StateMap.ARMModules
                        .Where(item => item.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();

                    if (ocrModuleInfo == null || armModuleInfo == null)
                    {
                        this.MetroDialogManager().ShowMessageDialog("OCR", "Wafer is not exist on OCR position.", EnumMessageStyle.Affirmative);

                        return false;
                    }
                }
                else
                {
                    LoggerManager.Debug($"LoaderControllerExt is null");
                    return false;
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return true;
        }
        private bool SEMIChecksum(String ocr)
        {
            char[] inputCheckSum = new char[2];
            bool result = false;
            try
            {
                if (ocr.Length > 2)
                {

                    inputCheckSum[0] = ocr[ocr.Length - 2];
                    inputCheckSum[1] = ocr[ocr.Length - 1];

                    StringBuilder stb = new StringBuilder(ocr);
                    stb[ocr.Length - 2] = 'A';
                    stb[ocr.Length - 1] = '0';
                    String strImsiOcrBuf = stb.ToString();

                    int[] chrtmp = new int[18];
                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        chrtmp[i] = strImsiOcrBuf[i];
                    }

                    int seed = 0;
                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        int j = (seed * 8) % 59;
                        j = j + chrtmp[i] - 32;
                        seed = j % 59;
                    }

                    char[] calcCheckSum = new char[2];
                    if (seed == 0)
                    {
                        calcCheckSum[0] = 'A';
                        calcCheckSum[1] = '0';
                    }
                    else
                    {
                        seed = 59 - seed;
                        int j = (seed / 8) & 0x7;
                        int i = seed & 0x7;
                        calcCheckSum[0] = (char)(j + 33 + 32);
                        calcCheckSum[1] = (char)(i + 16 + 32);
                    }

                    String checksum = $"{calcCheckSum[0]}{calcCheckSum[1]}";
                    result = inputCheckSum[0] == calcCheckSum[0] && inputCheckSum[1] == calcCheckSum[1];
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return result;
        }
        private bool IMBChecksum(String ocr)
        {
            bool result = false;
            try
            {
                int fe = 0;
                int fo = 0;
                int cd = 0;

                int[] ibmChar = new int[18];
                for (int i = 0; i < ocr.Length; i++)
                {
                    ibmChar[i] = IBM_Checksum_Char_Val(ocr[i]);
                    if (ibmChar[i] < 0)
                        return false;

                    //==> 번갈아 가며 계산
                    if (i % 2 == 0)//==> 첫 순번인 녀석
                    {
                        fe = fe + ibmChar[i];
                    }
                    else
                    {
                        if (i == 1)//==> CheckSum index
                            fo = fo + 0;
                        else
                            fo = fo + ibmChar[i];
                    }
                }

                //cd = (17 * (fo + (2 * fe))) % 35;

                cd = (17 * (fe + (2 * fo))) % 35;
                if (cd == ibmChar[1])
                {
                    fo = fo + cd;
                    result = (17 * (fe + (2 * fo))) % 35 == 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        private int IBM_Checksum_Char_Val(char chr)
        {
            int ret = -1;
            try
            {
                switch (chr)
                {
                    case '0': ret = 0; break;
                    case '1': ret = 1; break;
                    case '2': ret = 2; break;
                    case '3': ret = 3; break;
                    case '4': ret = 4; break;
                    case '5': ret = 5; break;
                    case '6': ret = 6; break;
                    case '7': ret = 7; break;
                    case '8': ret = 8; break;
                    case '9': ret = 9; break;
                    case 'a': ret = 10; break;
                    case 'b': ret = 11; break;
                    case 'c': ret = 12; break;
                    case 'd': ret = 13; break;
                    case 'e': ret = 14; break;
                    case 'f': ret = 15; break;
                    case 'g': ret = 16; break;
                    case 'h': ret = 17; break;
                    case 'i': ret = 18; break;
                    case 'j': ret = 19; break;
                    case 'k': ret = 20; break;
                    case 'l': ret = 21; break;
                    case 'm': ret = 22; break;
                    case 'n': ret = 23; break;
                    case 'p': ret = 24; break;
                    case 'q': ret = 25; break;
                    case 'r': ret = 26; break;
                    case 's': ret = 27; break;
                    case 't': ret = 28; break;
                    case 'u': ret = 29; break;
                    case 'v': ret = 30; break;
                    case 'w': ret = 31; break;
                    case 'x': ret = 32; break;
                    case 'y': ret = 33; break;
                    case 'z': ret = 34; break;
                    default: ret = -1; break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
