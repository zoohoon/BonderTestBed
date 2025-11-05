using System;
using System.Linq;
using System.Threading.Tasks;

namespace CognexOCRManualDialog
{
    using Autofac;
    using LoaderBase;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LoaderParameters;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using VirtualKeyboardControl;

    public class VmGPCognexOCRManualMainPage : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
        #region ==> IsBtnEnabled
        private bool _IsBtnEnabled;
        public bool IsBtnEnabled
        {
            get { return _IsBtnEnabled; }
            set
            {
                if (value != _IsBtnEnabled)
                {
                    _IsBtnEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> cheksumResult
        private bool _cheksumResult;
        public bool cheksumResult
        {
            get { return _cheksumResult; }
            set
            {
                if (value != _cheksumResult)
                {
                    _cheksumResult = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> LotInfo
        private ActiveLotInfo _LotInfo;
        public ActiveLotInfo LotInfo
        {
            get { return _LotInfo; }
            set
            {
                if (value != _LotInfo)
                {
                    _LotInfo = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> CheckIntegrity
        private bool _CheckIntegrity;
        public bool CheckIntegrity
        {
            get { return _CheckIntegrity; }
            set
            {
                if (value != _CheckIntegrity)
                {
                    _CheckIntegrity = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

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

        private String _LastOCR;
        public String LastOCR
        {
            get { return _LastOCR; }
            set
            {
                if (value != _LastOCR)
                {
                    _LastOCR = value;
                    if (_LastOCR == String.Empty)
                        OCRInputBoxBrush = Brushes.LightGray;

                    CurTextLength = _LastOCR.Length;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        private string _TargetInfo;

        public string TargetInfo
        {
            get { return _TargetInfo; }
            set { _TargetInfo = value; }
        }

        private string _CurrModule;

        public string CurrModule
        {
            get { return _CurrModule; }
            set { _CurrModule = value; }
        }

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
        private RelayCommand _EnterKeyCommand;
        public ICommand EnterKeyCommand
        {
            get
            {
                if (null == _EnterKeyCommand) _EnterKeyCommand = new RelayCommand(EnterKeyCommandFunc);
                return _EnterKeyCommand;
            }
        }
       
        private void EnterKeyCommandFunc()
        {
            String upperOCRString = OCRString.ToUpper();
            LastOCR = upperOCRString;
            IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, HostIndex + 1);
            if (paModule == null)
            {
                CheckIntegrity = true;
            }
            CheckIntegrity = _CognexProcessManager.CheckIntegrity(paModule.Holder.TransferObject.OCRDevParam, LotInfo, upperOCRString);
            //switch (_CognexProcessManager.CognexProcDevParam.GetManualConfig().CheckSum)
            //{
            //    case "2"://==> SEMI
            //        cheksumResult = SEMIChecksum(upperOCRString);
            //        break;
            //    case "4"://==> IBM
            //        cheksumResult = IMBChecksum(upperOCRString);
            //        break;
            //    default:
            //        cheksumResult = true;
            //        break;
            //}
            cheksumResult = _CognexProcessManager.SEMIChecksum(upperOCRString);

            if (cheksumResult == false | CheckIntegrity == false)
            {
                OCRInputBoxBrush = Brushes.Red;
                return;
            }
            _CognexProcessManager.Ocr[HostIndex] = upperOCRString;
            _CognexProcessManager.LastOcrResult[HostIndex] = upperOCRString;
            _CognexProcessManager.SetManualOCRState(HostIndex, true);

            _Win.Close();
        }
        #endregion

        #region ==> ExitCommand
        private RelayCommand _ExitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new RelayCommand(ExitCommandFunc);
                return _ExitCommand;
            }
        }

        private void ExitCommandFunc()
        {

            _Win.Close();
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
        private IModuleManager _ModuleManager;
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
            int idx = LightModeList.IndexOf(SelectedLight);
            idx++;

            if (idx > LightModeList.Count - 1)
                return;

            SelectedLight = LightModeList[idx];
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
            int idx = LightModeList.IndexOf(SelectedLight);
            idx--;

            if (idx < 0)
                return;

            SelectedLight = LightModeList[idx];
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
            if (LightIntensity + _ChangeSize > _MaxLightIntensity)
                return;

            LightIntensity += _ChangeSize;

            UpdateLight();
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
            if (LightIntensity - _ChangeSize < 0)
                return;

            LightIntensity -= _ChangeSize;

            UpdateLight();
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

        }
        private bool UpdateImage()
        {
            IsBtnEnabled = true;
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
            _ManualConfig = new OCRConfig();
        }


        private String _CognexIP;
        private OCRConfig _ManualConfig;
        public Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager;
        private ILoaderModule _LoaderModule;
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
                // _Container = this.LoaderController().GetLoaderContainer();
            }
            _CognexProcessManager = _Container.Resolve<ICognexProcessManager>();
            _ModuleManager = _Container.Resolve<IModuleManager>();
            _LoaderModule = _Container.Resolve<ILoaderModule>();

            OCRString = String.Empty;
            OCRInputBoxBrush = Brushes.LightGray;
            _CognexIP =
                _CognexProcessManager.CognexProcSysParam.GetIPOrNull_Index(0);

            Initialized = true;

            MaxTextLength = 15;
            IsBtnEnabled = true;
            
            retval = EventCodeEnum.NONE;

            return retval;
        }
        private int HostIndex = 0;
        public EventCodeEnum InitModule(int hostIndex)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized)
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    return EventCodeEnum.DUPLICATE_INVOCATION;
                }
                HostIndex = hostIndex;
                _CognexProcessManager = _Container.Resolve<ICognexProcessManager>();
                _ModuleManager = _Container.Resolve<IModuleManager>();
                _LoaderModule = _Container.Resolve<ILoaderModule>();

               
                OCRInputBoxBrush = Brushes.LightGray;
                _CognexIP =
                    _CognexProcessManager.CognexProcSysParam.GetIPOrNull_Index(hostIndex);

                Initialized = true;
                _CognexProcessManager.SetManualOCRState(hostIndex, false);
                MaxTextLength = 15;
                IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, HostIndex + 1);
                IARMModule arm1 = _ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, 1);
                IARMModule arm2 = _ModuleManager.FindModule<IARMModule>(ModuleTypeEnum.ARM, 2);
                OCRPosition = new OCRAccessParam();
                ICognexOCRModule cognexOCRModule = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, HostIndex + 1);
                if (paModule.Holder.TransferObject != null)
                {                    
                    OCRPosition.VPos.Value = paModule.Holder.TransferObject.OCRAngle.Value;                    
                    TargetInfo = $"  Origin:{_LoaderModule.SlotToFoupConvert(paModule.Holder.TransferObject.OriginHolder)}";
                }
                if (arm1.Holder.TransferObject != null)
                {                    
                    OCRPosition.VPos.Value = arm1.Holder.TransferObject.OCRAngle.Value;
                }
                if (arm2.Holder.TransferObject != null)
                {                   
                    OCRPosition.VPos.Value = arm2.Holder.TransferObject.OCRAngle.Value;
                }
                CurrModule = $"OCR{cognexOCRModule.ID.Index}";

                try
                {
                    int foupNum = 0;
                    var PaObject = paModule.Holder.TransferObject;
                    if (PaObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        int slotNum = PaObject.OriginHolder.Index % 25;
                        int offset = 0;
                        if (slotNum == 0)
                        {
                            slotNum = 25;
                            offset = -1;
                        }
                        foupNum = ((PaObject.OriginHolder.Index + offset) / 25) + 1;
                        LotInfo = _LoaderModule.LoaderMaster.ActiveLotInfos[foupNum - 1];
                    }
                    LastOCR = _CognexProcessManager.LastOcrResult[HostIndex];
                    CheckIntegrity = _CognexProcessManager.CheckIntegrity(paModule.Holder.TransferObject.OCRDevParam, LotInfo, LastOCR);
                    cheksumResult = _CognexProcessManager.SEMIChecksum(LastOCR);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    OCRString = String.Empty;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            retval = EventCodeEnum.NONE;

            return retval;
        }
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
                IsBtnEnabled = true;
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


        #region ==> RelAngle
        private double _RelAngle;
        public double RelAngle
        {
            get { return _RelAngle; }
            set
            {
                if (value != _RelAngle)
                {
                    _RelAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> FlatJogCRCommand
        private AsyncCommand _PAResetCommand;
        public ICommand PAResetCommand
        {
            get
            {
                if (null == _PAResetCommand) _PAResetCommand = new AsyncCommand(PAResetCommandFunc);
                return _PAResetCommand;
            }
        }
        private Task PAResetCommandFunc()
        {
            IsBtnEnabled = false;
            try
            {
                _LoaderModule.PAManager.PAModules[HostIndex].UpdateState();
                var ret = _LoaderModule.PAManager.PAModules[HostIndex].ModuleInit();
                if (ret == EventCodeEnum.NONE)
                {
                    _LoaderModule.PAManager.PAModules[HostIndex].ModuleReset();
                }
                _LoaderModule.PAManager.PAModules[HostIndex].UpdateState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsBtnEnabled = true;
            }
            return Task.CompletedTask;
        }
        #endregion
        #region ==> FlatJogCRCommand
        private AsyncCommand _FlatJogCRCommand;
        public ICommand FlatJogCRCommand
        {
            get
            {
                if (null == _FlatJogCRCommand) _FlatJogCRCommand = new AsyncCommand(FlatJogCRCommandFunc);
                return _FlatJogCRCommand;
            }
        }
        private Task FlatJogCRCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}
            try
            {
                double value = RelAngle * -1;

                OCRPosition.VPos.Value += value;

                if (OCRPosition.VPos.Value < 0)
                {
                    OCRPosition.VPos.Value = 360 - OCRPosition.VPos.Value;
                }
                else if (OCRPosition.VPos.Value >= 360)
                {
                    OCRPosition.VPos.Value = OCRPosition.VPos.Value % 360;
                }
                AngleMove(OCRPosition);

                UpdateImage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion
        private void AngleMove(OCRAccessParam accparam)
        {
            try
            {
                IsBtnEnabled = false;
                IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, HostIndex + 1);
                if (paModule == null)
                {
                    return;
                }

                ICognexOCRModule cognexOCRModule = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, HostIndex + 1);
                if (cognexOCRModule == null)
                {
                    return;
                }

                accparam.Position.U.Value = 0;
                accparam.Position.W.Value = 0; 

                SubchuckMotionParam subchuckMotionParam = null;
                if (paModule.Holder.TransferObject != null)
                {
                    double angleoffset = 0;
                    double OCRReadXPos = 0;
                    double OCRReadYPos = 0;
                    subchuckMotionParam = cognexOCRModule.GetSubchuckMotionParam(paModule.Holder.TransferObject.Size.Value);
                    if (subchuckMotionParam != null)
                    {
                        angleoffset = subchuckMotionParam.SubchuckAngle_Offset.Value;
                        OCRReadXPos = subchuckMotionParam.SubchuckXCoord.Value;
                        OCRReadYPos = subchuckMotionParam.SubchuckYCoord.Value;
                        LoggerManager.Debug($"[AngleMove] Host Index: {HostIndex + 1}, Wafer Size: {paModule.Holder.TransferObject.Size.Value}," +
                        $" OCR Angle: {accparam.VPos.Value}, OCR Position (X, Y, Angle Offset): ({OCRReadXPos}, {OCRReadYPos}, {angleoffset})");
                    }

                    var oCRangle = accparam.VPos.Value + angleoffset;
                    if (oCRangle < 0)
                    {
                        oCRangle = 360 + oCRangle;
                    }
                    else if (oCRangle >= 360)
                    {
                        oCRangle = oCRangle % 360;
                    }

                    var ret = _LoaderModule.GetLoaderCommands().PAMove(paModule, oCRangle);
                    if (ret == EventCodeEnum.NONE)
                    {
                        _LoaderModule.GetLoaderCommands().PAMove(paModule, OCRReadXPos, OCRReadYPos, 0);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #region ==> FlatJogCCRCommand
        private AsyncCommand _FlatJogCCRCommand;
        public ICommand FlatJogCCRCommand
        {
            get
            {
                if (null == _FlatJogCCRCommand) _FlatJogCCRCommand = new AsyncCommand(FlatJogCCRCommandFunc);
                return _FlatJogCCRCommand;
            }
        }
        private Task FlatJogCCRCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}
            try
            {
                double value = RelAngle;

                OCRPosition.VPos.Value += value;
                if (OCRPosition.VPos.Value > 360)
                {
                    OCRPosition.VPos.Value = OCRPosition.VPos.Value - 360;
                }
                else if (OCRPosition.VPos.Value >= 360)
                {
                    OCRPosition.VPos.Value = OCRPosition.VPos.Value % 360;
                }
                AngleMove(OCRPosition);
                UpdateImage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> CurAngleMoveCommand
        private AsyncCommand _CurAngleMoveCommand;
        public ICommand CurAngleMoveCommand
        {
            get
            {
                if (null == _CurAngleMoveCommand) _CurAngleMoveCommand = new AsyncCommand(CurAngleMoveCommandFunc);
                return _CurAngleMoveCommand;
            }
        }
        private Task CurAngleMoveCommandFunc()
        {
            try
            {
                var accparam = GetOCRAccessParam();
                if (accparam == null)
                {
                    return Task.CompletedTask; ;
                }
                AngleMove(accparam);
                UpdateImage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> OCRPosition
        private OCRAccessParam _OCRPosition;
        public OCRAccessParam OCRPosition
        {
            get { return _OCRPosition; }
            set
            {
                if (value != _OCRPosition)
                {
                    _OCRPosition = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> AngleTextBoxClickCommand
        private RelayCommand _AngleTextBoxClickCommand;
        public ICommand AngleTextBoxClickCommand
        {
            get
            {
                if (null == _AngleTextBoxClickCommand) _AngleTextBoxClickCommand = new RelayCommand(AngleTextBoxClickCommandFunc);
                return _AngleTextBoxClickCommand;
            }
        }
        private void AngleTextBoxClickCommandFunc()
        {
            String angleString = VirtualKeyboard.Show(RelAngle.ToString(), KB_TYPE.DECIMAL);

            if (String.IsNullOrEmpty(angleString))
                return;
            double angle = 0;
            if (Double.TryParse(angleString, out angle))
            {

                RelAngle = angle % 365;
            }
        }
        #endregion

        private OCRAccessParam GetOCRAccessParam()
        {

            //==> TODO : Single Stage OCR ViewModel도 이렇게 바꿔야 함.
            ICognexOCRModule cognexOCRModule = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, HostIndex + 1);
            if (cognexOCRModule == null)
            {
                return null;
            }

            IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, HostIndex + 1);
            if (paModule == null)
            {
                return null;
            }
            if (paModule.Holder.TransferObject != null)
            {
                var temp = cognexOCRModule.GetAccessParam(
                    paModule.Holder.TransferObject.Type.Value,
                    paModule.Holder.TransferObject.Size.Value);
                OCRPosition.VPos.Value = temp.VPos.Value;
            }
            else
            {
            }
            return OCRPosition;
        }
    }
}
