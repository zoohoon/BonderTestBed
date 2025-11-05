using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinAlignSettingViewModelPnP
{
    using Autofac;
    using LogModule;
    using MetroDialogInterfaces;
    using ProbeCardObject;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using System.Collections.ObjectModel;

    public class PinAlignSettingViewModelPnP : IMainScreenViewModel, IParamScrollingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        private readonly Guid _ViewModelGUID = new Guid("86664d7c-c444-4228-a3ea-e4c52b8557e6");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;

        #region ==> RecipeEditorParamEdit
        private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        {
            get { return _RecipeEditorParamEdit; }
            set
            {
                if (value != _RecipeEditorParamEdit)
                {
                    _RecipeEditorParamEdit = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ObservableCollection<string> _List = new ObservableCollection<string>();
        public ObservableCollection<string> List
        {
            get { return _List; }
            set
            {
                if (value != _List)
                {
                    _List = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SelectedItem;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (value != _SelectedItem)
                {
                    _SelectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        // Combo Box 에 보여줄 items List Name    
        // ex) PinAligner.PinAlignInfo.PinAlignParam.PinAlignSettignParameter[0].CardCenterTolerenceX
        private String _EnumTypeName;
        public String EnumTypeName
        {
            get
            {
                _EnumTypeName = "PinAlignSettignParameter";
                return _EnumTypeName;
            }
            set { _EnumTypeName = value; }
        }

        private Dictionary<string, string> _DictionaryResult = new Dictionary<string, string>();
        //private List<IElement> DeviceParamElementList => this.ParamManager().GetDevElementList();
        private List<IElement> DeviceParamElementList => GetParamManager().GetDevElementList();
        public IParamManager GetParamManager()
        {
            if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                return this.ParamManager();
            else // Remote 
                return this.GetLoaderContainer().Resolve<ILoaderParamManager>();
        }

        private PinAlignDevParameters PinAlignParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

        private int _Index;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

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

            return retval;
        }
        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Enum.GetNames(typeof(PINALIGNSOURCE)).Length > 0)
                {
                    for (int i = 0; i < Enum.GetNames(typeof(PINALIGNSOURCE)).Length; i++)
                    {
                        string sourcestr = Enum.GetName(typeof(PINALIGNSOURCE), i);

                        if (sourcestr != "UNDEFINED")
                        {
                            List.Add(Enum.GetName(typeof(PINALIGNSOURCE), i));
                        }
                    }

                    List.Add("All");
                }

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
                SelectedItem = List[List.Count() - 1].ToString();
                SelectedItemAndCategoryFiltering(EnumTypeName, SelectedItem, 10025001);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
        //public EventCodeEnum RollBackParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        //public bool HasParameterToSave()
        //{
        //    return true;
        //}

        #region //..Command 
        private AsyncCommand<CUI.Button> _PinAlignSetupCommand;
        public ICommand PinAlignSetupCommand
        {
            get
            {
                if (null == _PinAlignSetupCommand) _PinAlignSetupCommand = new AsyncCommand<CUI.Button>(FuncPinAlignSetup);
                return _PinAlignSetupCommand;
            }
        }
        private async Task FuncPinAlignSetup(object cuiparam)
        {
            try
            {
                CUI.Button param = cuiparam as CUI.Button;
                EnumMessageDialogResult mresult = EnumMessageDialogResult.UNDEFIND;
                EventCodeEnum retVal = EventCodeEnum.NONE;

                //if(SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                //{
                //    ILoaderCommunicationManager _LoaderCommunicationManager = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
                //    IStageObject selectedstage = _LoaderCommunicationManager.SelectedStage;

                //    if (selectedstage != null)
                //    {
                //        _LoaderCommunicationManager.GetWaferObject(selectedstage);
                //    }
                //}

                if (this.StageSupervisor().WaferObject.GetSubsInfo() != null)
                {
                    //if (this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count() < 1)
                    if (this.StageSupervisor().DutPadInfosCount() < 1)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Operation Fail", "Pad data is not ready yet. \nPlease finish pad registration firstly.", EnumMessageStyle.Affirmative, "OK");
                        return;
                    }

                    //retVal = this.StageSupervisor().ProbeCardInfo.CheckPinPadParameterValidity();
                    retVal = this.StageSupervisor().CheckPinPadParameterValidity();

                    if (retVal != EventCodeEnum.NONE)
                    {
                        if (retVal == EventCodeEnum.PTPA_WRONG_DUT_NUMBER)
                        {
                            // 더트 번호가 0이하인 것이 존재함. 즉 더트 등록이 이상함.
                           await  this.MetroDialogManager().ShowMessageDialog("Operation Fail",
                            "DUT data has wrong DUT number. \nPlease register DUT again...", EnumMessageStyle.Affirmative, "OK");
                            return;
                        }
                        else if (retVal == EventCodeEnum.PTPA_WRONG_PAD_NUMBER)
                        {
                            // 패드 번호가 0이하인 것이 존재함. 즉 패드 등록이 이상함.
                           await  this.MetroDialogManager().ShowMessageDialog("Operation Fail",
                            "Pad data has wrong pad number. \nPlease register pad again...", EnumMessageStyle.Affirmative, "OK");
                            return;
                        }
                        else
                        {
                            mresult = await this.MetroDialogManager().ShowMessageDialog("Warning",
                                "Pad data is different with pin data, pin data will be initialized from pad data. \n \nPress [ OK ] to continue...", EnumMessageStyle.AffirmativeAndNegative, "OK", "Cancel");

                            if (mresult == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                //this.StageSupervisor().ProbeCardInfo.GetPinDataFromPads();
                                this.StageSupervisor().GetPinDataFromPads();

                                //PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange = false;   // Reg 다시 해야 함
                            }
                            else
                                return;
                        }
                    }

                    //this.PinAligner().IsRecoveryStarted = false;        // 리커버리 상태 해제. 처음부터 다시 셋업.

                    Guid viewguid = new Guid();
                    List<Guid> pnpsteps = new List<Guid>();

                    PROBECARD_TYPE cardtype = this.StageSupervisor().GetProbeCardType();

                    if (cardtype == PROBECARD_TYPE.Cantilever_Standard)
                    {
                        if (this.TemplateManager() != null & this.PinAligner() != null)
                            this.TemplateManager().CheckTemplate(this.PinAligner(), true, 0);
                    }
                    else if (cardtype == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                    {
                        if (this.TemplateManager() != null & this.PinAligner() != null)
                            this.TemplateManager().CheckTemplate(this.PinAligner(), true, 1);
                    }
                    else if (cardtype == PROBECARD_TYPE.VerticalType)
                    {
                        if (this.TemplateManager() != null & this.PinAligner() != null)
                            this.TemplateManager().CheckTemplate(this.PinAligner(), true, 2);
                    }

                    //if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.Cantilever_Standard)
                    //{
                    //    if(this.TemplateManager() != null & this.PinAligner() != null)
                    //        this.TemplateManager().CheckTemplate(this.PinAligner(), true, 0);
                    //}
                    //else if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                    //{
                    //    if (this.TemplateManager() != null & this.PinAligner() != null)
                    //        this.TemplateManager().CheckTemplate(this.PinAligner(), true, 1);
                    //}

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        this.PnPManager().GetCuiBtnParam(this.PinAligner(), param.GUID, out viewguid, out pnpsteps);

                        if (pnpsteps.Count != 0)
                        {
                            this.PnPManager().SetNavListToGUIDs(this.PinAligner(), pnpsteps);
                            await this.ViewModelManager().ViewTransitionAsync(viewguid);
                        }
                        this.PinAligner().IsRecoveryStarted = false;
                    }
                    else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                        {
                            this.PnPManager().GetCuiBtnParam(this.PinAligner(), param.GUID, out viewguid, out pnpsteps);

                            if (pnpsteps.Count != 0)
                            {
                                this.PnPManager().SetNavListToGUIDs(this.PinAligner(), pnpsteps);
                                await this.ViewModelManager().ViewTransitionAsync(viewguid);
                            }
                        }
                        else
                        {
                            await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                            await this.PnPManager().SettingRemotePNP("PinAlign", "IPinAligner", new Guid("29cc6805-9418-4fb5-813c-fa1101820b3c"));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"FuncPinAlignSetup(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum CheckParameterToSave()
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
            return retVal;
        }

        public EventCodeEnum UpProc()
        {
            RecipeEditorParamEdit.PrevPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            RecipeEditorParamEdit.NextPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        private RelayCommand _DropDownClosedCommand;
        public ICommand DropDownClosedCommand
        {
            get
            {
                if (null == _DropDownClosedCommand) _DropDownClosedCommand = new RelayCommand(DropDownClosedCommandFunc);
                return _DropDownClosedCommand;
            }
        }

        private void DropDownClosedCommandFunc()
        {
            try
            {
                if (SelectedItem != null)
                {
                    _Index = List.IndexOf(SelectedItem);
                    SelectedItemAndCategoryFiltering(EnumTypeName, SelectedItem, 10025001);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private void SelectedItemAndCategoryFiltering(string enumType, string selectItem, int categoryID)
        {
            try
            {
                string path = "";
                string lastSplit = "";


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                });
                RecipeEditorParamEdit.HardCategoryFiltering(categoryID);

                List<IElement> data = new List<IElement>();
                IEnumerable<IElement> filter;

                for (int i = 0; i < List.Count; i++)
                {
                    filter = DeviceParamElementList.Where(item => item.PropertyPath.Contains(enumType)).AsEnumerable();
                    data = filter.Where(item => item.PropertyPath.Contains(i.ToString())).ToList();
                    RecipeEditorParamEdit.AddDataNameInfo(List[i], data);
                }

                if (selectItem == "All")
                    return;


                filter = DeviceParamElementList.Where(item => item.PropertyPath.Contains(enumType)).AsEnumerable();
                data = filter.Where(item => item.PropertyPath.Contains(_Index.ToString())).ToList();

                RecipeEditorParamEdit.HardElementFiltering(data);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
