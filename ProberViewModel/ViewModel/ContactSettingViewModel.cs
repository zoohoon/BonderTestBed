using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProbingModule;
using RelayCommandBase;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using SoakingParameters;
using LoaderBase.Communication;
using VirtualKeyboardControl;
using Autofac;
using ProberInterfaces.Proxies;
using System.Windows;
using LoaderBase.FactoryModules.ViewModelModule;

namespace ContactSettingVM
{
    public class ContactSettingViewModel : INotifyPropertyChanged, IMainScreenViewModel
    {
        public Guid _ViewModelGUID = new Guid("7A33ACE3-1AA4-4AD2-9719-BEBE14EF2092");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RasiePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public bool Initialized { get; set; } = false;
        public IProbingModule ProbingModule { get; private set; }
        public ILoaderCommunicationManager _LoaderCommunicationManager = null;
        //private bool _ContactBeforeSoakEnable;
        //public bool ContactBeforeSoakEnable
        //{
        //    get { return _ContactBeforeSoakEnable; }
        //    set
        //    {
        //        if (value != _ContactBeforeSoakEnable)
        //        {
        //            _ContactBeforeSoakEnable = value;
        //            RasiePropertyChanged();
        //        }
        //    }
        //}

        //private bool _AutoZupEnable;
        //public bool AutoZupEnable
        //{
        //    get { return _AutoZupEnable; }
        //    set
        //    {
        //        if (value != _AutoZupEnable)
        //        {
        //            _AutoZupEnable = value;
        //            RasiePropertyChanged();
        //        }
        //    }
        //}


        #region //v22 이후 ItemControl이나 ListView로 변경 필요

        private IElement _ZClearence;
        public IElement ZClearence
        {
            get { return _ZClearence; }
            set
            {
                if (value != _ZClearence)
                {
                    _ZClearence = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _Overdrive;
        public IElement Overdrive
        {
            get { return _Overdrive; }
            set
            {
                if (value != _Overdrive)
                {
                    _Overdrive = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _OverdriveUpperLimit;// TODO: Element 아님.주의 필요
        public IElement OverdriveUpperLimit
        {
            get { return _OverdriveUpperLimit; }
            set
            {
                if (value != _OverdriveUpperLimit)
                {
                    _OverdriveUpperLimit = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _OverdriveLowLimit;// TODO: Element 아님.주의 필요
        public IElement OverdriveLowLimit
        {
            get { return _OverdriveLowLimit; }
            set
            {
                if (value != _OverdriveLowLimit)
                {
                    _OverdriveLowLimit = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _ProbingXYOffSetUpperLimit;
        public IElement ProbingXYOffSetUpperLimit
        {
            get { return _ProbingXYOffSetUpperLimit; }
            set
            {
                if (value != _ProbingXYOffSetUpperLimit)
                {
                    _ProbingXYOffSetUpperLimit = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _ProbingXYOffSetLowLimit;
        public IElement ProbingXYOffSetLowLimit
        {
            get { return _ProbingXYOffSetLowLimit; }
            set
            {
                if (value != _ProbingXYOffSetLowLimit)
                {
                    _ProbingXYOffSetLowLimit = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _BeforeZupSoakingClearanceZ;// TODO: Element 아님.주의 필요
        public IElement BeforeZupSoakingClearanceZ
        {
            get { return _BeforeZupSoakingClearanceZ; }
            set
            {
                if (value != _BeforeZupSoakingClearanceZ)
                {
                    _BeforeZupSoakingClearanceZ = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _BeforeZupSoakingTime;// TODO: Element 아님.주의 필요
        public IElement BeforeZupSoakingTime
        {
            get { return _BeforeZupSoakingTime; }
            set
            {
                if (value != _BeforeZupSoakingTime)
                {
                    _BeforeZupSoakingTime = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _PinPadMatchedTimeOut;// TODO: Element 아님.주의 필요
        public IElement PinPadMatchedTimeOut
        {
            get { return _PinPadMatchedTimeOut; }
            set
            {
                if (value != _PinPadMatchedTimeOut)
                {
                    _PinPadMatchedTimeOut = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactCount;
        public IElement MultipleContactCount
        {
            get { return _MultipleContactCount; }
            set
            {
                if (value != _MultipleContactCount)
                {
                    _MultipleContactCount = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactBackOD;
        public IElement MultipleContactBackOD
        {
            get { return _MultipleContactBackOD; }
            set
            {
                if (value != _MultipleContactBackOD)
                {
                    _MultipleContactBackOD = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactSpeed;
        public IElement MultipleContactSpeed
        {
            get { return _MultipleContactSpeed; }
            set
            {
                if (value != _MultipleContactSpeed)
                {
                    _MultipleContactSpeed = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactAccel;
        public IElement MultipleContactAccel
        {
            get { return _MultipleContactAccel; }
            set
            {
                if (value != _MultipleContactAccel)
                {
                    _MultipleContactAccel = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactOD;
        public IElement MultipleContactOD
        {
            get { return _MultipleContactOD; }
            set
            {
                if (value != _MultipleContactOD)
                {
                    _MultipleContactOD = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactDelayTime;
        public IElement MultipleContactDelayTime
        {
            get { return _MultipleContactDelayTime; }
            set
            {
                if (value != _MultipleContactDelayTime)
                {
                    _MultipleContactDelayTime = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IElement _MultipleContactBackODOption;
        public IElement MultipleContactBackODOption
        {
            get { return _MultipleContactBackODOption; }
            set
            {
                if (value != _MultipleContactBackODOption)
                {
                    _MultipleContactBackODOption = value;
                    RasiePropertyChanged();
                }
            }
        }


        private IElement _BeforeZupSoakingEnable; 
        public IElement BeforeZupSoakingEnable
        {
            get { return _BeforeZupSoakingEnable; }
            set
            {
                if (value != _BeforeZupSoakingEnable)
                {
                    _BeforeZupSoakingEnable = value;
                    RasiePropertyChanged();
                }
            }
        }

        #endregion
        private IParam _ProbingDevCopyParam;
        public IParam ProbingDevCopyParam
        {
            get { return _ProbingDevCopyParam; }
            set
            {
                if (_ProbingDevCopyParam != value)
                {
                    _ProbingDevCopyParam = value;
                    RasiePropertyChanged();
                }
            }
        }

        private IParam _ProbingSysCopyParam;
        public IParam ProbingSysCopyParam
        {
            get { return _ProbingSysCopyParam; }
            set
            {
                if (_ProbingSysCopyParam != value)
                {
                    _ProbingSysCopyParam = value;
                    RasiePropertyChanged();
                }
            }
        }

        private bool _BeforeZupSoakEnableCtrl = true;
        public bool BeforeZupSoakEnableCtrl 
        {
            get
            {
                return _BeforeZupSoakEnableCtrl;
            }
            set
            {
                if (_BeforeZupSoakEnableCtrl != value)
                {
                    _BeforeZupSoakEnableCtrl = value;
                    RasiePropertyChanged();
                }
            }
        }

        public double UserXShiftValue { get; set; }
        public double UserYShiftValue { get; set; }
        public double SystemXShiftValue { get; set; }
        public double SystemYShiftValue { get; set; }

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

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ProbingModule = this.ProbingModule();

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

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }


        private List<IElement> _DevElements = new List<IElement>();
        public List<IElement> DevElements
        {
            get { return _DevElements; }
            set
            {
                if (value != _DevElements)
                {
                    _DevElements = value;
                    RasiePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {                
                var statusSoakingConfigParam = GetStatusSoakingConfig();
                if (statusSoakingConfigParam != null)
                {                    
                    BeforeZupSoakEnableCtrl = !statusSoakingConfigParam.ShowStatusSoakingSettingPage.Value;
                }



                ProbingDevCopyParam = ProbingModule.GetProbingDevIParam();

                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                    {
                        //1. List<ElementPack> 
                        List<ElementPack> list = new List<ElementPack>();

                        //2. Dictionary = {prepertyPath: {Value}}
                        ILoaderCommunicationManager loadercomm = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                        IRemoteMediumProxy RemoteMediumProxy = loadercomm.GetProxy<IRemoteMediumProxy>();
                        GetUserSystemMarkShiftValue(RemoteMediumProxy);

                        List<IElement> ProbingDevParams = new List<IElement>();
                        //3. Overdrive = Dictionary.where( prepertyPath.Contain("Overdrive")));
                        var bulkElem = loadercomm.GetProxy<IParamManagerProxy>().GetDevParamElementsBulk("Probing.ProbingModuleDevParam");//serialize
                                                                                                                                          //List<IElement> ProbingDevElemList =  loadercomm.GetProxy<IParamManagerProxy>().GetElementList(ProbingDevParams, ref bulkElem);//DeserializeFromByte
                        List<IElement> ProbingDevElemList = this.ParamManager().GetElementList(ProbingDevParams, ref bulkElem);//DeserializeFromByte
                        Overdrive = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.OverDrive").FirstOrDefault();
                        ZClearence = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.ZClearence").FirstOrDefault();
                        OverdriveUpperLimit = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.OverdriveUpperLimit").FirstOrDefault();
                        OverdriveLowLimit = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.OverdriveLowLimit").FirstOrDefault();
                        ProbingXYOffSetUpperLimit = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.ProbingXYOffSetUpperLimit").FirstOrDefault();
                        ProbingXYOffSetLowLimit = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.ProbingXYOffSetLowLimit").FirstOrDefault();
                        BeforeZupSoakingClearanceZ = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.BeforeZupSoakingClearanceZ").FirstOrDefault();
                        BeforeZupSoakingTime = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.BeforeZupSoakingTime").FirstOrDefault();
                        BeforeZupSoakingEnable = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.BeforeZupSoakingEnable").FirstOrDefault();
                        PinPadMatchedTimeOut = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.PinPadMatchedTimeOut").FirstOrDefault();
                        MultipleContactCount = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactCount").FirstOrDefault();
                        MultipleContactBackOD = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactBackOD").FirstOrDefault();
                        MultipleContactSpeed = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactSpeed").FirstOrDefault();
                        MultipleContactAccel = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactAccel").FirstOrDefault();
                        MultipleContactOD = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactOD").FirstOrDefault();
                        MultipleContactDelayTime = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactDelayTime").FirstOrDefault();// 없음.
                        MultipleContactBackODOption = ProbingDevElemList.Where(w => w.PropertyPath == "Probing.ProbingModuleDevParam.MultipleContactBackODOption").FirstOrDefault();
                    }
                    else if(AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                    {
                        GetUserSystemMarkShiftValue(null); //셀이라 null로 넘김
                        SetProbingParametersFromDevParam();
                    }
                }
                else // 싱글
                {
                    GetUserSystemMarkShiftValue(null);
                    SetProbingParametersFromDevParam();
                }

                //if ((ProbingDevCopyParam as ProbingModuleDevParam).BeforeZupSoakingEnable.Value)
                //{
                //    ContactBeforeSoakEnable = true;
                //}
                //else
                //{
                //    ContactBeforeSoakEnable = false;
                //}

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                if (ProbingDevCopyParam != null)
                {
                    //ProbingModule.SetProbingDevParam(ProbingDevCopyParam);
                    ProbingModule.SaveDevParameter();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        public EventCodeEnum RollBackParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ProbingDevCopyParam = ProbingModule.ProbingModuleDevParam_IParam.Copy();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        private void SetProbingParametersFromDevParam()
        {
            try
            {
                ProbingModuleDevParam probingDevParam = ProbingDevCopyParam as ProbingModuleDevParam;
                if (probingDevParam != null)
                {
                    Overdrive = probingDevParam.OverDrive;
                    ZClearence = probingDevParam.ZClearence;
                    OverdriveUpperLimit = probingDevParam.OverdriveUpperLimit;
                    OverdriveLowLimit = probingDevParam.OverdriveLowLimit;
                    ProbingXYOffSetUpperLimit = probingDevParam.ProbingXYOffSetUpperLimit;
                    ProbingXYOffSetLowLimit = probingDevParam.ProbingXYOffSetLowLimit;
                    BeforeZupSoakingClearanceZ = probingDevParam.BeforeZupSoakingClearanceZ;
                    BeforeZupSoakingTime = probingDevParam.BeforeZupSoakingTime;
                    BeforeZupSoakingEnable = probingDevParam.BeforeZupSoakingEnable;
                    PinPadMatchedTimeOut = probingDevParam.PinPadMatchedTimeOut;
                    MultipleContactCount = probingDevParam.MultipleContactCount;
                    MultipleContactBackOD = probingDevParam.MultipleContactBackOD;
                    MultipleContactSpeed = probingDevParam.MultipleContactSpeed;
                    MultipleContactAccel = probingDevParam.MultipleContactAccel;
                    MultipleContactOD = probingDevParam.MultipleContactOD;
                    MultipleContactDelayTime = probingDevParam.MultipleContactDelayTime;
                    MultipleContactBackODOption = probingDevParam.MultipleContactBackODOption;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool HasParameterToSave()
        {
            bool retVal = false;

            try
            {
                var devCopyProperties = ProbingDevCopyParam.GetType().GetProperties();
                var devProperties = ProbingModule.ProbingModuleDevParam_IParam.GetType().GetProperties();

                foreach (var propertyInfo in devCopyProperties)
                {
                    retVal = CheckParamNeedsSave(propertyInfo, devProperties, ProbingModule.ProbingModuleDevParam_IParam);

                    if (retVal == true)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private bool CheckParamNeedsSave(System.Reflection.PropertyInfo propertyInfo,
                                        System.Reflection.PropertyInfo[] originPropertyInfo,
                                        IParam originParam)
        {
            bool retVal = false;

            try
            {
                object value = null;

                value = propertyInfo.GetValue(ProbingDevCopyParam);

                var originValue = originPropertyInfo.First(info => info.Name == propertyInfo.Name)?.GetValue(originParam);

                if (value is IElement)
                {
                    IElement copyElement = value as IElement;
                    IElement originElement = originValue as IElement;

                    if (!copyElement.GetValue().Equals(originElement.GetValue()))
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum CheckParameterToSave()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                ret = CheckOverdriveRangeProcessing();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        private EventCodeEnum CheckOverdriveRangeProcessing()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IManualContact ManualContact = this.ManualContactModule();

                ProbingModuleSysParam probingSysParam = this.ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                ProbingModuleDevParam probingDevParam = ProbingDevCopyParam as ProbingModuleDevParam;

                bool isOverdriveRange = true;
                string errorReason = string.Empty;

                isOverdriveRange = CheckOverdriveRange(ref errorReason);

                if (isOverdriveRange == false)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", $"OverDrive Value : {errorReason}", EnumMessageStyle.Affirmative);

                    retVal = EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;

            bool CheckOverdriveRange(ref string errorReasonStr)
            {
                bool retIsOverdriveRange = true;
                ProbingModuleSysParam probingSysParam = this.ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                ProbingModuleDevParam probingDevParam = ProbingDevCopyParam as ProbingModuleDevParam;

                try
                {

                    if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.ALL_CONTACT)
                    {
                        if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                            probingDevParam.AllContactToOverdriveLimitRange.Value < probingDevParam.OverDrive.Value)
                        {
                            retIsOverdriveRange = false;
                            errorReasonStr += $"AllContactToOverdriveLimit is Enable state.\n" +
                                $"Range : {probingDevParam.AllContactToOverdriveLimitRange.Value}\n" +
                                $"OverDrive : {probingDevParam.OverDrive.Value}";
                        }
                    }
                    else if (probingSysParam.OverDriveStartPosition.Value == OverDriveStartPositionType.FIRST_CONTACT)
                    {
                        if (probingDevParam.IsEnableFirstContactToOverdriveLimit.Value == true &&
                            probingDevParam.FirstContactToOverdriveLimitRange.Value < probingDevParam.OverDrive.Value)
                        {
                            retIsOverdriveRange = false;
                            errorReasonStr += $"FirstContactToOverdriveLimit is Enable state.\n" +
                                $"Range : {probingDevParam.FirstContactToOverdriveLimitRange.Value}\n" +
                                $"OverDrive : {probingDevParam.OverDrive.Value}";
                        }
                        if (probingDevParam.IsEnableAllContactToOverdriveLimit.Value == true &&
                            probingDevParam.AllContactToOverdriveLimitRange.Value < probingDevParam.OverDrive.Value)
                        {
                            retIsOverdriveRange = false;
                            errorReasonStr += $"AllContactToOverdriveLimit is Enable state.\n" +
                                $"Range : {probingDevParam.AllContactToOverdriveLimitRange.Value}\n" +
                                $"OverDrive : {probingDevParam.OverDrive.Value}";
                        }
                    }
                    else
                    {
                        retIsOverdriveRange = true;
                    }

                    if (probingDevParam.OverDrive.Value > probingDevParam.OverdriveUpperLimit.Value)
                    {
                        retIsOverdriveRange = false;
                        errorReasonStr += $"Positive SW Limit occurred.\n" +
                 $"Overdrive Limit Value => {probingDevParam.OverdriveUpperLimit.Value}\n" +
                 $"Overdrive Value => {probingDevParam.OverDrive.Value}\n";

                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                return retIsOverdriveRange;
            }
        }

        

        private async Task<EventCodeEnum> UpdateValue(string propertypath, string newVal)
        {
            string details = "";
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    var loadercomm = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                    retVal = loadercomm.GetProxy<IParamManagerProxy>().SetValue(propertypath, newVal);
                    if(retVal == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                    {
                        retVal = EventCodeEnum.NONE;

                    }

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            this.ParamManager().MetroDialogManager().ShowMessageDialog("Param Validation Failed", $"Cannot be set due to {retVal} error code. \n details:", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }));
                    }
                }
                else
                {
                    retVal = this.ParamManager().SetValue(propertypath, newVal);
                    if (retVal == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                    {
                        retVal = EventCodeEnum.NONE;

                    }

                    if (retVal != EventCodeEnum.NONE)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            this.ParamManager().MetroDialogManager().ShowMessageDialog("Param Validation Failed", $"Cannot be set due to {retVal} error code. \n details:", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        }));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private async Task GetUserSystemMarkShiftValue(IRemoteMediumProxy RemoteMediumProxy)
        {
            try
            {
                if (RemoteMediumProxy != null)
                {
                    var info = await RemoteMediumProxy.GetUserSystemMarkShiftValue();

                    if (info != null)
                    {
                        UserXShiftValue = info.UserProbeMarkXShift;
                        UserYShiftValue = info.UserProbeMarkYShift;
                        SystemXShiftValue = info.SystemMarkXShift;
                        SystemYShiftValue = info.SystemMarkYShift;
                    }
                    else
                    {
                        LoggerManager.Debug($"[ContactSettingViewModel] Failed to get values ​​UserMarkShiftValue, SystemMarkShiftValue.");
                    }
                }
                else
                {
                    ProbingModuleSysParam probingSysParam = this.ProbingModule.ProbingModuleSysParam_IParam as ProbingModuleSysParam;
                    UserXShiftValue = probingSysParam.UserProbeMarkShift.Value.X.Value;
                    UserYShiftValue = probingSysParam.UserProbeMarkShift.Value.Y.Value;
                    SystemXShiftValue = probingSysParam.ProbeMarkShift.Value.X.Value;
                    SystemYShiftValue = probingSysParam.ProbeMarkShift.Value.Y.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new AsyncCommand<Object>(FuncTextBoxClickCommand);
                return _TextBoxClickCommand;
            }
        }

        private async Task FuncTextBoxClickCommand(Object param)
        {
            try
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                object[] _params = param as object[];
                string newVal = "";
                
                System.Windows.Controls.TextBox tb = null;
                if (_params.Count() > 0)
                {
                    tb = (System.Windows.Controls.TextBox)_params[0];
                    if (_params[0] is System.Windows.Controls.TextBox)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            newVal = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 10);

                            if (_params[1] is string)
                            {
                                // TODO : 
                                string errorReason = null;
                                bool ret = VerifyParameterRange(tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).ParentBinding.Path.Path, Convert.ToDouble(newVal), ref errorReason);

                                if (ret == false)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Error", $"Error Setting Values: {errorReason}", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Apply..");

                                    string propertypath = _params[1].ToString();

                                    Task<EventCodeEnum> task = UpdateValue(propertypath, newVal);

                                    if (task.Result == EventCodeEnum.NONE)
                                    {
                                        tb.Text = newVal.ToString();
                                        tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                                    }
                                }

                            }
                        }));
                    }
                }


                bool VerifyParameterRange(string path, double newvalue, ref string errorReasonStr)
                {
                    bool retval = true;
                    if (path == "Overdrive.Value")
                    {
                        if (newvalue > Convert.ToDouble(OverdriveUpperLimit.GetValue()))
                        {
                            retval = false;
                            errorReasonStr += $"Positive SW Limit occurred.\n" +
                            $"Overdrive Upper Limit Value => {Convert.ToDouble(OverdriveUpperLimit.GetValue())}\n" +
                            $"Overdrive Value => {newvalue}\n";
                        }

                        if (newvalue < Convert.ToDouble(OverdriveLowLimit.GetValue()))
                        {
                            retval = false;
                            errorReasonStr += $"Negative SW Limit occurred.\n" +
                            $"Overdrive Low Limit Value => {Convert.ToDouble(OverdriveLowLimit.GetValue())}\n" +
                            $"Overdrive Value => {newvalue}\n";
                        }
                    }
                    else if (path == "OverdriveUpperLimit.Value")
                    {
                        if (Convert.ToDouble(Overdrive.GetValue()) > newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Positive SW Limit occurred.\n" +
                                $"Overdrive Upper Limit Value => {newvalue}\n" +
                                $"Overdrive Value => {Convert.ToDouble(Overdrive.GetValue())}\n";
                        }

                        if (Convert.ToDouble(OverdriveLowLimit.GetValue()) > newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Positive SW Limit occurred.\n" +
                                $"Overdrive Upper Limit Value => {newvalue}\n" +
                                $"Overdrive Low Limit Value => {Convert.ToDouble(OverdriveLowLimit.GetValue())}\n";
                        }
                    }
                    else if (path == "OverdriveLowLimit.Value")
                    {
                        if (Convert.ToDouble(Overdrive.GetValue()) < newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Negative SW Limit occurred.\n" +
                                $"Overdrive Low Limit Value => {newvalue}\n" +
                                $"Overdrive Value => {Convert.ToDouble(Overdrive.GetValue())}\n";
                        }

                        if (Convert.ToDouble(OverdriveUpperLimit.GetValue()) < newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Negative SW Limit occurred.\n" +
                                $"Overdrive Low Limit Value => {newvalue}\n" +
                                $"Overdrive Upper Limit Value => {Convert.ToDouble(OverdriveUpperLimit.GetValue())}\n";
                        }
                    }
                    else if (path == "ProbingXYOffSetUpperLimit.Value")
                    {
                        double resultX = 0.0;
                        double resultY = 0.0;

                        resultX = UserXShiftValue + SystemXShiftValue;
                        resultY = UserYShiftValue + SystemYShiftValue;

                        if (Convert.ToDouble(ProbingXYOffSetLowLimit.GetValue()) > newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Positive SW Limit occurred.\n" +
                                $"Probing OffSet XY Low Limit Value => {Convert.ToDouble(ProbingXYOffSetLowLimit.GetValue())}\n";
                        }

                        if (resultX > newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"\n" + $"Adding UserProbeMark Xshift: {UserXShiftValue}, ProbeMark Xshift: {SystemXShiftValue} gives {resultX}, but the new value {newvalue} is smaller.\n";
                        }

                        if (resultY > newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"\n" + $"Adding UserProbeMark Yshift: {UserYShiftValue}, ProbeMark Yshift: {SystemYShiftValue} gives {resultY}, but the new value {newvalue} is smaller.\n";
                        }

                    }
                    else if (path == "ProbingXYOffSetLowLimit.Value")
                    {
                        double resultX = 0.0;
                        double resultY = 0.0;

                        resultX = UserXShiftValue + SystemXShiftValue;
                        resultY = UserYShiftValue + SystemYShiftValue;

                        if (Convert.ToDouble(ProbingXYOffSetUpperLimit.GetValue()) < newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"Negative SW Limit occurred.\n" +
                                $"Probing OffSet XY Upper Limit Value => {Convert.ToDouble(ProbingXYOffSetUpperLimit.GetValue())}\n";
                        }

                        if (resultX < newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"\n" + $"Adding UserProbeMarkXshift: {UserXShiftValue}, ProbeMarkXshift: {SystemXShiftValue} gives {resultX}, but the new value {newvalue} is larger.\n";
                        }

                        if (resultY < newvalue)
                        {
                            retval = false;
                            errorReasonStr += $"\n" + $"Adding UserProbeMark Yshift: {UserYShiftValue}, ProbeMark Yshift: {SystemYShiftValue} gives {resultY}, but the new value {newvalue} is larger.\n";
                        }

                    }
                    else if (path == "ZClearence.Value")
                    {
                        if (newvalue > Convert.ToDouble(Overdrive.GetValue()))
                        {
                            retval = false;
                            errorReasonStr += $"Positive SW Limit occurred.\n" +
                                $"Z clearence Value => {newvalue}\n" +
                                $"Overdrive Value => {Convert.ToDouble(Overdrive.GetValue())}\n";
                        }
                        else
                            retval = true;
                    }
                    #region MultipleContact value set validation
                    // v22에선 Element로 되어 있고 element Lowlimit, upperlimit값으로 validation 이미 하고 있음
                    // v22 merge 시에는 빠져도 되는 코드
                    // => v22 에 Element Min/Max Validation 기능이 아직 없어 체리픽 진행 함. - 231129 by randy
                    else if (path == "MultipleContactCount.Value")
                    {
                        if (newvalue > 10 || newvalue < 0)
                        {
                            errorReasonStr += $"You can set the value between 0 and 10.";
                            retval = false;
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    else if (path == "MultipleContactBackOD.Value")
                    {
                        if (newvalue > 0 || newvalue < -500)
                        {
                            errorReasonStr += $"You can set the value between -500 and 0.";
                            retval = false;
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    else if (path == "MultipleContactSpeed.Value")
                    {
                        if (newvalue > 260000 || newvalue < 10)
                        {
                            errorReasonStr += $"You can set the value between 10 and 260000.";
                            retval = false;
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    else if (path == "MultipleContactAccel.Value")
                    {
                        if (newvalue > 2600000 || newvalue < 10)
                        {
                            errorReasonStr += $"You can set the value between 10 and 2600000.";
                            retval = false;
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    else if (path == "MultipleContactOD.Value")
                    {
                        if (newvalue > 30 || newvalue < -30)
                        {
                            errorReasonStr += $"You can set the value between -30 and 30.";
                            retval = false;
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    else if (path == "MultipleContactDelayTime.Value")
                    {
                        if (newvalue > 5000 || newvalue < 0)
                        {
                            errorReasonStr += $"You can set the value between 0 and 5000.";
                            retval = false;
                        }
                        else
                        {
                            retval = true;
                        }
                    }
                    #endregion
                    LoggerManager.Debug($"VerifyParameter() in {GetType().Name} result: {retval} [{path}: {newvalue}]");

                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).ConfigureAwait(false);
            }
        }

        private AsyncCommand<Object> _ContanctBeforeSoakEnableCommand;
        public ICommand ContanctBeforeSoakEnableCommand
        {
            get
            {
                if (null == _ContanctBeforeSoakEnableCommand) _ContanctBeforeSoakEnableCommand = new AsyncCommand<Object>(ContanctBeforeSoakEnableCommandFunc);
                return _ContanctBeforeSoakEnableCommand;
            }
        }

        private async Task ContanctBeforeSoakEnableCommandFunc(Object param)
        {
            try
            {

                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Apply..");

                string propertypath = param as string;

                Task<EventCodeEnum> task = UpdateValue(propertypath, "True");
                await task;

                if(task.Result == EventCodeEnum.NONE)
                {
                    BeforeZupSoakingEnable.SetValue(true);
                }

                //(ProbingDevCopyParam as ProbingModuleDevParam).BeforeZupSoakingEnable.Value = false;
                //this.ProbingModule().SetProbingDevParam(ProbingDevCopyParam);
                this.ProbingModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).ConfigureAwait(false);
            }
        }

        private AsyncCommand<Object> _ContanctBeforeSoakDisableCommand;
        public ICommand ContanctBeforeSoakDisableCommand
        {
            get
            {
                if (null == _ContanctBeforeSoakDisableCommand) _ContanctBeforeSoakDisableCommand = new AsyncCommand<Object>(ContanctBeforeSoakDisableCommandFunc);
                return _ContanctBeforeSoakDisableCommand;
            }
        }

        private async Task ContanctBeforeSoakDisableCommandFunc(Object param)
        {
            try
            {

                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Apply..");

                string propertypath = param as string;

                Task<EventCodeEnum> task = UpdateValue(propertypath, "False");
                await task;
                if (task.Result == EventCodeEnum.NONE)
                {
                    BeforeZupSoakingEnable.SetValue(false);
                }
                //(ProbingDevCopyParam as ProbingModuleDevParam).BeforeZupSoakingEnable.Value = false;
                //this.ProbingModule().SetProbingDevParam(ProbingDevCopyParam);
                this.ProbingModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).ConfigureAwait(false);
            }
        }

        private StatusSoakingConfig GetStatusSoakingConfig()
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (_LoaderCommunicationManager == null)
                    {
                        _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                    }

                    if ((_LoaderCommunicationManager == null) || (_LoaderCommunicationManager.SelectedStage == null))
                    {
                        return null;
                    }
                }

                var statusSoakingParamByte = this.SoakingModule().GetStatusSoakingConfigParam();
                var statusSoakingParamObj = SerializeManager.ByteToObject(statusSoakingParamByte);
                if (null == statusSoakingParamObj)
                {
                    return null;
                }

                return statusSoakingParamObj as StatusSoakingConfig;
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
            }

            return null;
        }

        private AsyncCommand<Object> _BackODOptionCommand;
        public ICommand BackODOptionCommand
        {
            get
            {
                if (null == _BackODOptionCommand) _BackODOptionCommand = new AsyncCommand<Object>(BackODOptionCommandFunc);
                return _BackODOptionCommand;
            }
        }

        private async Task BackODOptionCommandFunc(Object param)
        {
            try
            {
                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Apply..");
                //(ProbingDevCopyParam as ProbingModuleDevParam).MultipleContactBackODOption.Value = (MultipleContactBackODOptionEnum)param;
                

                Task<EventCodeEnum> task = UpdateValue(MultipleContactBackODOption.PropertyPath, param.ToString());
                await task;

                if (task.Result == EventCodeEnum.NONE)
                {
                    MultipleContactBackODOption.SetValue(param);
                }

                //(ProbingDevCopyParam as ProbingModuleDevParam).BeforeZupSoakingEnable.Value = false;
                //this.ProbingModule().SetProbingDevParam(ProbingDevCopyParam);
                this.ProbingModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString()).ConfigureAwait(false);
            }

        
        }
    }
}
