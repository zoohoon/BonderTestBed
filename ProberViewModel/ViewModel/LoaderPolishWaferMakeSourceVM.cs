using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderCore;
using LoaderMapView;
using LoaderParameters;
using LoaderParameters.Data;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ControlClass.ViewModel.Polish_Wafer;
using ProberInterfaces.Loader;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace LoaderPolishWaferMakeSourceViewModelModule
{
    public class PWColorPreset
    {
        public List<string> ColorNamelist;
        //public List<Brush> Brushlist;

        public KnownColor[] allColors;

        public PWColorPreset()
        {
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            allColors = new KnownColor[colorsArray.Length];

            Array.Copy(colorsArray, allColors, colorsArray.Length);

            //Brushlist = new List<Brush>();

            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(255, 0, 0)));
            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(0, 255, 0)));
            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(0, 0, 255)));
            //Brushlist.Add(new SolidColorBrush(Color.FromRgb(255, 255, 0)));

            ColorNamelist = new List<string>();

            AddColor("Red");
            AddColor("Green");
            AddColor("Blue");
            AddColor("Chocolate");
            AddColor("Yellow");
            AddColor("DarkOrchid");
            AddColor("DodgerBlue");
            AddColor("HotPink");
            AddColor("LemonChiffon");
        }

        private void AddColor(string inputname)
        {
            var check = allColors.FirstOrDefault(x => x.ToString() == inputname);

            string ColorName = string.Empty;

            if (check != null)
            {
                if (ColorName != inputname)
                {
                    ColorNamelist.Add(inputname);
                }
            }
        }
    }

    public class LoaderPolishWaferMakeSourceVM : IPolishWaferMakeSourceVM
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

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        private readonly Guid _ViewModelGUID = new Guid("4022d034-d082-4828-8c71-fb3c6dd94153");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        private int MaximumSourceCount = 0;

        private IPolishWaferSourceInformation _SelectedSourceInfo;
        public IPolishWaferSourceInformation SelectedSourceInfo
        {
            get { return _SelectedSourceInfo; }
            set
            {
                if (value != _SelectedSourceInfo)
                {
                    _SelectedSourceInfo = value;
                    if (_SelectedSourceInfo != null) 
                    {
                        SelectedSourceInfoSize = _SelectedSourceInfo.Size.Value;
                    }
                    RaisePropertyChanged();
                }   
            }
        }

        private SubstrateSizeEnum _SelectedSourceInfoSize;
        public SubstrateSizeEnum SelectedSourceInfoSize
        {
            get { return _SelectedSourceInfoSize; }
            set
            {
                if (value != _SelectedSourceInfoSize)
                {
                    _SelectedSourceInfoSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PWColorPreset _ColorPreset = new PWColorPreset();
        public PWColorPreset ColorPreset
        {
            get { return _ColorPreset; }
            set
            {
                if (value != _ColorPreset)
                {
                    _ColorPreset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITrasnferObjectSet _TrasnferObjectSet;
        public ITrasnferObjectSet TrasnferObjectSet
        {
            get { return _TrasnferObjectSet; }
            set
            {
                if (value != _TrasnferObjectSet)
                {
                    _TrasnferObjectSet = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<IPolishWaferSourceInformation> _PolishWaferSourceInformations;
        public AsyncObservableCollection<IPolishWaferSourceInformation> PolishWaferSourceInformations
        {
            get { return _PolishWaferSourceInformations; }
            set
            {
                if (value != _PolishWaferSourceInformations)
                {
                    _PolishWaferSourceInformations = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CanChange;
        public bool CanChange
        {
            get { return _CanChange; }
            set
            {
                if (value != _CanChange)
                {
                    _CanChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ModuleInfoEnable = false;
        public bool ModuleInfoEnable
        {
            get { return _ModuleInfoEnable; }
            set
            {
                if (value != _ModuleInfoEnable)
                {
                    _ModuleInfoEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            MaximumSourceCount = ColorPreset.ColorNamelist.Count;

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

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

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                // DeviceManager가 갖고 있는 PolishWafer Source 데이터 가져오기.
                PolishWaferSourceInformations = this.DeviceManager().GetPolishWaferSources();

                if ((PolishWaferSourceInformations != null) && (PolishWaferSourceInformations.Count > 0))
                {
                    SelectedSourceInfo = PolishWaferSourceInformations[0];
                }

                TrasnferObjectSet = this.DeviceManager().TransferObjectInfos;

                this.DeviceManager().UpdateFixedTrayCanUseBuffer();

                CanChangeValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                var PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameters.PolishWaferParameter;
                foreach (var intervalparam in PolishWaferParam?.PolishWaferIntervalParameters)
                {
                    foreach (var cleaningparam in intervalparam?.CleaningParameters)
                    {
                        var pwInfo = this.DeviceManager().GetPolishWaferSources().Where(x => x.DefineName.Value == cleaningparam.WaferDefineType.Value).FirstOrDefault();
                        if (pwInfo != null)
                        {
                            if (cleaningparam.Thickness.Value != pwInfo.Thickness.Value)
                            {
                                cleaningparam.Thickness.Value = pwInfo.Thickness.Value;
                            }
                        }
                    }
                }

                byte[] param = null;
                param = SerializerUtil.SerializeManager.SerializeToByte(PolishWaferParam, typeof(PolishWaferParameters.PolishWaferParameter));

                if (param != null)
                {
                    _RemoteMediumProxy.PolishWaferRecipeSettingVM_SetPolishWaferIParam(param);
                }
                else
                {
                    LoggerManager.Error($"ParamSynchronization() Failed");
                }
                this.DeviceManager().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private AsyncCommand _AddSourceCommand;
        public IAsyncCommand AddSourceCommand
        {
            get
            {
                if (null == _AddSourceCommand)
                    _AddSourceCommand = new AsyncCommand(AddSourceCommandFunc);
                return _AddSourceCommand;
            }
        }

        private async Task AddSourceCommandFunc()
        {
            try
            {
                int SourceCountBeforeAdd = PolishWaferSourceInformations.Count;

                if (PolishWaferSourceInformations.Count >= MaximumSourceCount)
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Information]", "Can not add more data.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;

                    result = await this.MetroDialogManager().ShowSingleInputDialog("Name : ", "Add");

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        string devName = string.Empty;

                        Task task = new Task(async() =>
                        {
                            devName = this.MetroDialogManager().GetSingleInputData();

                            if (!string.IsNullOrEmpty(devName))
                            {
                                IPolishWaferSourceInformation alreadyHasDevDir = null;

                                bool isProcessChangeDevice = false;

                                if (PolishWaferSourceInformations.Count > 0)
                                {
                                    alreadyHasDevDir = PolishWaferSourceInformations.Where(x => x.DefineName.Value == devName).FirstOrDefault();
                                }

                                if (alreadyHasDevDir != null)
                                {
                                    isProcessChangeDevice = false;

                                    // TODO: 중복된 이름으로 Add 할 수 없다는 내용을 사용자에게 전달해야 함.
                                }
                                else
                                {
                                    isProcessChangeDevice = true;
                                }

                                if (isProcessChangeDevice)
                                {
                                    PolishWaferInformation tmpinfo = new PolishWaferInformation();
                                    tmpinfo.DefineName.Value = devName;

                                    tmpinfo.IdentificationColorBrush.Value = GetNotAssignedColor();

                                    tmpinfo.Thickness.Value = 1000;
                                    tmpinfo.Size.Value = SubstrateSizeEnum.INCH12;
                                    tmpinfo.NotchType.Value = WaferNotchTypeEnum.NOTCH;

                                    await PolishWaferSourceInformations.AddAsync(tmpinfo);
                                    SelectedSourceInfo = PolishWaferSourceInformations.Last();
                                }
                            }
                        });
                        task.Start();
                        await task;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetNotAssignedColor()
        {
            string retval = string.Empty;

            try
            {
                foreach (var color in ColorPreset.ColorNamelist)
                {
                    var source = PolishWaferSourceInformations.FirstOrDefault(x => x.IdentificationColorBrush.Value == color);

                    if (source == null)
                    {
                        retval = color;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private AsyncCommand _RemoveSourceCommand;
        public IAsyncCommand RemoveSourceCommand
        {
            get
            {
                if (null == _RemoveSourceCommand)
                    _RemoveSourceCommand = new AsyncCommand(RemoveSourceCommandFunc);
                return _RemoveSourceCommand;
            }
        }

        private async Task RemoveSourceCommandFunc()
        {
            try
            {
                if (SelectedSourceInfo != null)
                {
                    EnumMessageDialogResult result = EnumMessageDialogResult.NEGATIVE;

                    result = await this.MetroDialogManager().ShowMessageDialog("[Information]", "Are you sure you want to remove the data?", EnumMessageStyle.AffirmativeAndNegative);

                    if (result == EnumMessageDialogResult.AFFIRMATIVE)  // OK
                    {
                        Task task = new Task(async() =>
                        {
                            UpdateCleaningParameters(SelectedSourceInfo.DefineName.Value);

                            TrasnferObjectSet.RemoveAssignedWaferType(SelectedSourceInfo);

                            //Collection Change Event
                            await PolishWaferSourceInformations.RemoveAsync(SelectedSourceInfo);
;
                            if ((PolishWaferSourceInformations != null) && (PolishWaferSourceInformations.Count > 0))
                            {
                                SelectedSourceInfo = PolishWaferSourceInformations[0];
                            }

                            TrasnferObjectSet.UpdateAssignedWaferTypeColor(PolishWaferSourceInformations);

                        });
                        task.Start();
                        await task;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AssignCommand;
        public IAsyncCommand AssignCommand
        {
            get
            {
                if (null == _AssignCommand) _AssignCommand = new AsyncCommand(AssignCommandFunc);
                return _AssignCommand;
            }
        }

        private async Task AssignCommandFunc()
        {
            try
            {
                if (SelectedSourceInfo != null)
                {
                    if (ValidateWaferChangeData(out string ErrorMessage, true)) 
                    {
                        EventCodeEnum retVal = TrasnferObjectSet.AssignWaferType(EnumWaferType.POLISH, SelectedSourceInfo);
                        if (retVal == EventCodeEnum.POLISHWAFER_ASSIGN_FAIL)
                        {
                            await this.MetroDialogManager().ShowMessageDialog("[Warning]", "Polish Wafer cannot be assigned because selected tray is for buffers.", EnumMessageStyle.Affirmative);
                            TrasnferObjectSet.UnSelectAll();
                        }
                        else
                        {
                            TrasnferObjectSet.UnSelectAll();
                            //this.DeviceManager().SaveSysParameter();
                        }
                    }
                    else 
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Error Message]", $"{ErrorMessage}", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    LoggerManager.Debug($"Selected source is not exist.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private bool ValidateWaferChangeData(out string ErrorMessage, bool isassign = false, bool isremove = false)
        {
            bool retval = true;
            SubstrateSizeEnum wafersize = SubstrateSizeEnum.UNDEFINED;
            ErrorMessage = "";
            try
            {
                ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                var map = loaderModule.GetLoaderInfo().StateMap;


                var PolishWaferParam = this.PolishWaferModule().GetPolishWaferIParam() as PolishWaferParameters.PolishWaferParameter;
                if (PolishWaferParam != null)
                {
                    bool isExistnotmatchedinterval = false;
                    if (PolishWaferParam.PolishWaferIntervalParameters != null && PolishWaferParam.PolishWaferIntervalParameters.Count > 0)
                    {
                        var cellinfo = loaderModule.LoaderMaster.GetDeviceInfoClient(loaderModule.LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index));
                        foreach (var interval in PolishWaferParam.PolishWaferIntervalParameters)
                        {
                            if (interval.CleaningParameters != null && interval.CleaningParameters.Count > 0)
                            {
                                if (interval.CleaningParameters.Any(cleaning => cleaning.WaferDefineType.Value == SelectedSourceInfo.DefineName.Value))
                                {
                                    if (SelectedSourceInfoSize != cellinfo.Size.Value)
                                    {
                                        retval = false;
                                       
                                        ErrorMessage = $"The selected stage{this._LoaderCommunicationManager.SelectedStage.Index} uses that polish wafer type {SelectedSourceInfo.DefineName.Value} If you want to change the size, delete the scenario using that type in the stage's polish wafer interval setting.";
                                        return retval;
                                    }
                                }
                            }
                        }
                    }
                }


                // 현재 선택된 PW의 이름과 동일한 웨이퍼가 Fixed Tray 또는 Inspection가 아닌 위치에 존재할수도 있음 
                // 모두 wafer size 가 assign 하려는 웨이퍼 사이즈와 같아야함
                // 아니라면 Error 발생해야 함
                //[1] 이미 같은 DefineName 으로 source 존재. CurrHolder 가 tray가 아닌 웨이퍼들이 Size 가 다른게 있는지 확인하기 위함.
                List<IWaferSupplyMappingInfo> AssignTrays = TrasnferObjectSet.GetDefineNameModulesList(SelectedSourceInfo.DefineName.Value);
                if (AssignTrays != null) 
                {
                    foreach (var tray in AssignTrays)
                    {
                        //GetTransferObjectAll 일때 DefineName 가 DEFAULTDEVNAME 이 되는 버그가 있어서 x.Size.Value 조건 못걸었음.
                        if (tray is FixedTrayObject)
                        {
                            var ft = tray as FixedTrayObject;
                            var pws = map.GetTransferObjectAll().FindAll(x => x.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY && x.OriginHolder == ft.WaferSupplyInfo.ID);
                            if (pws != null && pws.Count > 0)
                            {
                                if (ft.DeviceInfo.Size.Value != SelectedSourceInfoSize) 
                                {
                                    retval = false;
                                    ErrorMessage = $"The assigned wafer size ({SelectedSourceInfo.DefineName.Value}) information does not match the intended wafer size change.";
                                    return retval;
                                }
                            }
                        }
                        else if (tray is InspectionTrayObject)
                        {
                            var ft = tray as InspectionTrayObject;
                            var pws = map.GetTransferObjectAll().FindAll(x => x.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && x.OriginHolder == ft.WaferSupplyInfo.ID);
                            if (pws != null && pws.Count > 0)
                            {
                                if (ft.DeviceInfo.Size.Value != SelectedSourceInfoSize)
                                {
                                    retval = false;
                                    ErrorMessage = $"The assigned wafer size ({SelectedSourceInfo.DefineName.Value}) information does not match the intended wafer size change.";
                                    return retval;
                                }
                            }
                        }
                    }
                }

                List<IWaferSupplyMappingInfo> SelectedTrays = TrasnferObjectSet.GetSelectedModulesList();
                if (SelectedTrays == null)
                {
                    retval = true;
                    return retval;
                }

                var nonTrayModules = map.GetTransferObjectAll().FindAll(x => x.PolishWaferInfo != null && x.CurrHolder.ModuleType != ModuleTypeEnum.FIXEDTRAY && x.CurrHolder.ModuleType != ModuleTypeEnum.INSPECTIONTRAY);
                if (isremove || isassign)
                {
                    foreach (var item in SelectedTrays)
                    {
                        if (item is FixedTrayObject)
                        {
                            var ft = item as FixedTrayObject;
                            if (nonTrayModules != null && nonTrayModules.Count > 0)
                            {
                                var ret = nonTrayModules.Where(h => h.OriginHolder.Index == ft.WaferSupplyInfo.ID.Index && h.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY).FirstOrDefault();
                                if (ret != null)
                                {
                                    retval = false;
                                    ErrorMessage += $"The Wafer exists in {ret.CurrHolder.Label} on {ret.OriginHolder.Label}\n";
                                }
                            }
                        }
                        else if (item is InspectionTrayObject)
                        {
                            var ft = item as InspectionTrayObject;
                            if (nonTrayModules != null && nonTrayModules.Count > 0)
                            {
                                var ret = nonTrayModules.Where(h => h.OriginHolder.Index == ft.WaferSupplyInfo.ID.Index && h.OriginHolder.ModuleType == ModuleTypeEnum.INSPECTIONTRAY).FirstOrDefault();
                                if (ret != null)
                                {
                                    retval = false;
                                    ErrorMessage += $"The Wafer exists in {ret.CurrHolder.Label} on {ret.OriginHolder.Label}\n";
                                }
                            }
                        }
                    }
                }

                if (isassign)
                {
                    //[2] 설정하려는 source 의 wafer size 가 이미 Wafer 가 존재하는 모듈의 wafer Size .
                    ErrorMessage += $"The assigned wafer size({ SelectedSourceInfo.DefineName.Value})\n";
                    foreach (var item in SelectedTrays)
                    {
                        if (item is FixedTrayObject)
                        {
                            var ft = item as FixedTrayObject;
                            var waferobj = map.GetHolderModuleAll().FirstOrDefault(h => h.ID.Index == ft.WaferSupplyInfo.ID.Index && h.ID.ModuleType == ModuleTypeEnum.FIXEDTRAY && h.WaferStatus == EnumSubsStatus.EXIST);
                            if (waferobj != null)
                            {
                                IAttachedModule module = loaderModule.ModuleManager.FindModule(ft.WaferSupplyInfo.ID);
                                if (module is IFixedTrayModule ownable)
                                {
                                    if (ownable.Device.AllocateDeviceInfo.Size.Value != SelectedSourceInfoSize)
                                    {
                                        retval = false;
                                        wafersize = ownable.Device.AllocateDeviceInfo.Size.Value;
                                        ErrorMessage += $"FixedTray{ft.WaferSupplyInfo.ID.Index} Size :{wafersize}\n";
                                    }
                                }
                            }   
                        }
                        else if (item is InspectionTrayObject)
                        {
                            var ft = item as InspectionTrayObject;
                            var waferobj = map.GetHolderModuleAll().FirstOrDefault(h => h.ID.Index == ft.WaferSupplyInfo.ID.Index && h.ID.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && h.WaferStatus == EnumSubsStatus.EXIST);
                            if (waferobj != null)
                            {
                                IAttachedModule module = loaderModule.ModuleManager.FindModule(ft.WaferSupplyInfo.ID);
                                if (module is IInspectionTrayModule ownable)
                                {
                                    if (ownable.Device.AllocateDeviceInfo.Size.Value != SelectedSourceInfoSize)
                                    {
                                        retval = false;
                                        wafersize = ownable.Device.AllocateDeviceInfo.Size.Value;
                                        ErrorMessage += $"InspectionTray{ft.WaferSupplyInfo.ID.Index} Size :{wafersize}\n";
                                    }
                                }
                                //if (waferobj.Substrate.Size.Value != SelectedSourceInfoSize)
                                //{
                                //    retval = false;
                                //    wafersize = waferobj.Substrate.Size.Value;
                                //    ErrorMessage += $"InspectionTray{ft.WaferSupplyInfo.ID.Index} Size :{waferobj.Substrate.Size.Value}\n";
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = false;
                ErrorMessage = "An error occurred while validating wafer change data.";
            }
            finally 
            {
                if (retval == false) 
                {
                    SelectedSourceInfoSize = SelectedSourceInfo.Size.Value;
                }
                else
                {
                    SelectedSourceInfo.Size.Value = SelectedSourceInfoSize;
                }
            }
            return retval;
        }

        private AsyncCommand _RemoveCommand;
        public IAsyncCommand RemoveCommand
        {
            get
            {
                if (null == _RemoveCommand) _RemoveCommand = new AsyncCommand(RemoveCommandFunc);
                return _RemoveCommand;
            }
        }

        private async Task RemoveCommandFunc()
        {
            try
            {
                if (TrasnferObjectSet != null)
                {
                    if (ValidateWaferChangeData(out string ErrorMessage, isremove: true))
                    {
                        TrasnferObjectSet.AssignWaferType(EnumWaferType.UNDEFINED, SelectedSourceInfo);
                        TrasnferObjectSet.UnSelectAll();
                        //this.DeviceManager().SaveSysParameter();
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Error Message]", $"{ErrorMessage}", EnumMessageStyle.Affirmative);
                        TrasnferObjectSet.UnSelectAll();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SelectedSourceInfoChangedCommand;
        public IAsyncCommand SelectedSourceInfoChangedCommand
        {
            get
            {
                if (null == _SelectedSourceInfoChangedCommand) _SelectedSourceInfoChangedCommand = new AsyncCommand<object>(SelectedSourceInfoChangedCommandFunc);
                return _SelectedSourceInfoChangedCommand;
            }
        }

        private async Task SelectedSourceInfoChangedCommandFunc(object param)
        {
            try
            {
                CanChangeValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SelectedObjectCommand;
        public IAsyncCommand SelectedObjectCommand
        {
            get
            {
                if (null == _SelectedObjectCommand) _SelectedObjectCommand = new AsyncCommand<object>(SelectedObjectCommandFunc);
                return _SelectedObjectCommand;
            }
        }

        private async Task SelectedObjectCommandFunc(object param)
        {
            try
            {
                IList<object> paramlist = (IList<object>)param;

                if (paramlist.Count > 0)
                {
                    if (paramlist[0] is string)
                    {
                        if (((string)paramlist[0]).Equals("SlotListView"))
                        {
                            //SlotObject;
                            ListView lv = (ListView)paramlist[1];
                        }
                        else if (((string)paramlist[0]).Equals("FoupListView"))
                        {
                            //FoupObject
                            ListView lv = (ListView)paramlist[1];
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        // StageObject
                    }


                    if (ModuleInfoEnable) 
                    {
                        int selectedcount = paramlist.Count;

                        ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();
                        if (paramlist[selectedcount - 1] is LoaderParameters.Data.FixedTrayObject)
                        {
                            var TrayObject = paramlist[selectedcount - 1] as LoaderParameters.Data.FixedTrayObject;
                            if (TrayObject != null) 
                            {
                                ProberViewModel.ModuleInfoVM.Show(ModuleTypeEnum.FIXEDTRAY, TrayObject, loaderModule.LoaderMaster);
                            }
                        }
                        else if (paramlist[selectedcount - 1] is LoaderParameters.Data.InspectionTrayObject)
                        {
                            var TrayObject = paramlist[selectedcount - 1] as LoaderParameters.Data.InspectionTrayObject;
                            if (TrayObject != null)
                            {
                                ProberViewModel.ModuleInfoVM.Show(ModuleTypeEnum.INSPECTIONTRAY, TrayObject, loaderModule.LoaderMaster);
                            }
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CanChangeValue()
        {
            try
            {
                // 현재 선택된 PW의 이름과 동일한 웨이퍼가 Fixed Tray 또는 Inspection가 아닌 위치에 존재하는지 확인
                if (SelectedSourceInfo != null) 
                {
                    ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();

                    var map = loaderModule.GetLoaderInfo().StateMap;
                    var pws = map.GetTransferObjectAll().FindAll(x => x.PolishWaferInfo != null && x.PolishWaferInfo.DefineName.Value == SelectedSourceInfo.DefineName.Value && x.CurrHolder.ModuleType != ModuleTypeEnum.FIXEDTRAY && x.CurrHolder.ModuleType != ModuleTypeEnum.INSPECTIONTRAY);

                    if (pws != null && pws.Count > 0)
                    {
                        CanChange = false;
                        //this.MetroDialogManager().ShowMessageDialog("[Information]", "The value cannot be changed until all polish wafers are returned to their original positions.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        CanChange = true;
                    }
                }
                else
                {
                    CanChange = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _NotchAngleTextBoxClickCommand;
        public ICommand NotchAngleTextBoxClickCommand
        {
            get
            {
                if (null == _NotchAngleTextBoxClickCommand) _NotchAngleTextBoxClickCommand = new RelayCommand<Object>(FuncNotchAngleTextBoxClickCommand);
                return _NotchAngleTextBoxClickCommand;
            }
        }

        private void FuncNotchAngleTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                if (SelectedSourceInfo != null)
                {
                    if (ValidateWaferChangeData(out string ErrorMessage))
                    {
                        TrasnferObjectSet.UpdateInfo(SelectedSourceInfo);
                        ChangeNotchAngle(SelectedSourceInfo);
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Error Message]", $"{ErrorMessage}", EnumMessageStyle.Affirmative);
                        TrasnferObjectSet.UnSelectAll();
                    }
                }

                //this.DeviceManager().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ChangeNotchAngle(IPolishWaferSourceInformation pwinfo)
        {
            try
            {
                ILoaderModule loaderModule = this.GetLoaderContainer().Resolve<ILoaderModule>();

                DeviceManagerParameter DMParam = this.DeviceManager()?.DeviceManagerParamerer_IParam as DeviceManagerParameter;

                if (DMParam != null)
                {
                    var polishWaferCompatibleHolders = DMParam.DeviceMappingInfos.Where(x => (x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.FIXEDTRAY || x.WaferSupplyInfo.ModuleType == ModuleTypeEnum.INSPECTIONTRAY) &&
                                                                                              x.DeviceInfo.WaferType.Value == EnumWaferType.POLISH);

                    foreach (var item in polishWaferCompatibleHolders)
                    {
                        IAttachedModule module = loaderModule.ModuleManager.FindModule(item.WaferSupplyInfo.ID);

                        if (module is IWaferOwnable ownable)
                        {
                            TransferObject tmp = ownable.Holder.TransferObject;

                            // 웨이퍼 존재
                            if (tmp != null)
                            {
                                // 이미 할당되어 있는 경우
                                if (tmp.PolishWaferInfo != null && tmp.PolishWaferInfo.DefineName.Value == item.DeviceInfo.PolishWaferInfo.DefineName.Value)
                                {
                                    // Angle 업데이트
                                    tmp.PolishWaferInfo.CurrentAngle.Value = item.DeviceInfo.PolishWaferInfo.CurrentAngle.Value;

                                    LoggerManager.Debug($"[{this.GetType().Name}], ChangeNotchAngle() : ID = {module.ID}, The value of CurrentAngle has been set to {tmp.PolishWaferInfo.CurrentAngle.Value} as the NotchAngle has been modified.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _DecimalTextBoxClickCommand;
        public ICommand DecimalTextBoxClickCommand
        {
            get
            {
                if (null == _DecimalTextBoxClickCommand) _DecimalTextBoxClickCommand = new RelayCommand<Object>(FuncDecimalTextBoxClickCommand);
                return _DecimalTextBoxClickCommand;
            }
        }

        private void FuncDecimalTextBoxClickCommand(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                if (SelectedSourceInfo != null)
                {
                    if (ValidateWaferChangeData(out string ErrorMessage))
                    {
                        TrasnferObjectSet.UpdateInfo(SelectedSourceInfo);
                        ChangeNotchAngle(SelectedSourceInfo);
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Error Message]", $"{ErrorMessage}", EnumMessageStyle.Affirmative);
                        TrasnferObjectSet.UnSelectAll();
                    }
                }

                this.DeviceManager().SaveSysParameter();
                //ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ChangedSizeCommand;
        public ICommand ChangedSizeCommand
        {
            get
            {
                if (null == _ChangedSizeCommand) _ChangedSizeCommand = new AsyncCommand<object>(ChangedSizeCommandFunc);
                return _ChangedSizeCommand;
            }
        }

        private Task ChangedSizeCommandFunc(object obj)
        {
            try
            {
                if (SelectedSourceInfo != null)
                {
                    if (ValidateWaferChangeData(out string ErrorMessage))
                    {
                        TrasnferObjectSet.UpdateInfo(SelectedSourceInfo);
                        ChangeNotchAngle(SelectedSourceInfo);
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Error Message]", $"{ErrorMessage}", EnumMessageStyle.Affirmative);
                        TrasnferObjectSet.UnSelectAll();
                    }
                    //this.DeviceManager().SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        private AsyncCommand<object> _ChangedNotchTypeCommand;
        public ICommand ChangedNotchTypeCommand
        {
            get
            {
                if (null == _ChangedNotchTypeCommand) _ChangedNotchTypeCommand = new AsyncCommand<object>(ChangedNotchTypeCommandFunc);
                return _ChangedNotchTypeCommand;
            }
        }

        private Task ChangedNotchTypeCommandFunc(object obj)
        {
            try
            {
                if (SelectedSourceInfo != null)
                {
                    if (ValidateWaferChangeData(out string ErrorMessage))
                    {
                        TrasnferObjectSet.UpdateInfo(SelectedSourceInfo);
                        ChangeNotchAngle(SelectedSourceInfo);
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("[Error Message]", $"{ErrorMessage}", EnumMessageStyle.Affirmative);
                        TrasnferObjectSet.UnSelectAll();
                    }
                    //this.DeviceManager().SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        public void UpdateCleaningParameters(string sourcename)
        {
            try
            {
                _RemoteMediumProxy.UpdateCleaningParameters(SelectedSourceInfo.DefineName.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task AddSourceRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task RemoveSourceRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task AssignRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task RemoveRemoteCommand()
        {
            throw new NotImplementedException();
        }

        public Task SelectedObjectRemoteCommand(object param)
        {
            throw new NotImplementedException();
        }
    }
}
