using System;
using System.Linq;
using System.Threading.Tasks;
namespace GPCardChangeOPViewModel
{
    using LogModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase;
    using LoaderCore;
    using LoaderMapView;
    using LoaderParameters;
    using LoaderParameters.Data;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using System.Windows.Input;
    using StageStateEnum = LoaderBase.Communication.StageStateEnum;
    using Autofac;
    using ProberViewModel;
    using LoaderBase.Communication;
    using MetroDialogInterfaces;
    using System.Threading;
    using LoaderRecoveryControl;
    using ProberInterfaces.SequenceRunner;
    using ProberInterfaces.ViewModel;
    using ProberInterfaces.CardChange;
    using System.Collections.Generic;
    using ProberInterfaces.LoaderController;

    public class LoaderGPCardChangeOPViewModel : INotifyPropertyChanged, IMainScreenViewModel, ILoaderMapConvert, IFactoryModule, ILoaderGPCardChangeOPViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region //..Property

        #endregion
        #region IMainScreenViewModel
        readonly Guid _ViewModelGUID = new Guid("95061a72-d013-4dd8-a05d-899e77547ee9");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public bool Initialized { get; set; } = false;
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        private Dictionary<CCSettingOPTabControlEnum, int> TabControlMappingDictionary = new Dictionary<CCSettingOPTabControlEnum, int>();

        private AsyncObservableCollection<IOPortDescripter<bool>> _IOPorts;
        public AsyncObservableCollection<IOPortDescripter<bool>> IOPorts
        {
            get { return _IOPorts; }
            set
            {
                if (value != _IOPorts)
                {
                    _IOPorts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<StageObject> _Cells = new ObservableCollection<StageObject>();
        public ObservableCollection<StageObject> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<CardArmObject> _CardArms = new ObservableCollection<CardArmObject>();
        public ObservableCollection<CardArmObject> CardArms
        {
            get { return _CardArms; }
            set
            {
                if (value != _CardArms)
                {
                    _CardArms = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<CardBufferObject> _CardBuffers = new ObservableCollection<CardBufferObject>();
        public ObservableCollection<CardBufferObject> CardBuffers
        {
            get { return _CardBuffers; }
            set
            {
                if (value != _CardBuffers)
                {
                    _CardBuffers = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<CardTrayObject> _CardTrays = new ObservableCollection<CardTrayObject>();
        public ObservableCollection<CardTrayObject> CardTrays
        {
            get { return _CardTrays; }
            set
            {
                if (value != _CardTrays)
                {
                    _CardTrays = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ISequenceBehaviorGroupItem> _DockSequences = new ObservableCollection<ISequenceBehaviorGroupItem>();
        public ObservableCollection<ISequenceBehaviorGroupItem> DockSequences
        {
            get { return _DockSequences; }
            set
            {
                if (value != _DockSequences)
                {
                    _DockSequences = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _DockSequencesCount;
        public int DockSequencesCount
        {
            get { return _DockSequencesCount; }
            set
            {
                if (value != _DockSequencesCount)
                {
                    _DockSequencesCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _UnDockSequencesCount;
        public int UnDockSequencesCount
        {
            get { return _UnDockSequencesCount; }
            set
            {
                if (value != _UnDockSequencesCount)
                {
                    _UnDockSequencesCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ISequenceBehaviorGroupItem> _UnDockSequences = new ObservableCollection<ISequenceBehaviorGroupItem>();
        public ObservableCollection<ISequenceBehaviorGroupItem> UnDockSequences
        {
            get { return _UnDockSequences; }
            set
            {
                if (value != _UnDockSequences)
                {
                    _UnDockSequences = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Reason = "";
        public string Reason
        {

            get { return _Reason; }
            set
            {
                
                if (_Reason != value)
                {
                    _Reason = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _SourceEnable = true;
        public bool SourceEnable
        {
            get { return _SourceEnable; }
            set
            {
                if (value != _SourceEnable)
                {
                    _SourceEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _TransferEnable = false;
        public bool TransferEnable
        {
            get { return _TransferEnable; }
            set
            {
                if (value != _TransferEnable)
                {
                    _TransferEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _TargetEnable = false;
        public bool TargetEnable
        {
            get { return _TargetEnable; }
            set
            {
                if (value != _TargetEnable)
                {
                    _TargetEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {

            get { return _IsSelected; }
            set
            {
                //if (value != _IsSelected)
                //{
                _IsSelected = value;
                if (_IsSelected == false)
                {
                    RaisePropertyChanged();
                }
                //}
            }
        }
        private bool _ModuleInfoEnable = true;
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
        int stagecnt = 12;
        int cardArmCnt = 1;
        int cardBufferCnt = 4;
        int cardTrayCnt = 9;
        private Autofac.IContainer _Container => this.GetLoaderContainer();

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

        public ILoaderModule loaderModule => _Container.Resolve<ILoaderModule>();
        public ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();

        private void InitData()
        {
            try
            {
                stagecnt = SystemModuleCount.ModuleCnt.StageCount;
                cardArmCnt = SystemModuleCount.ModuleCnt.CardArmCount;
                cardTrayCnt = SystemModuleCount.ModuleCnt.CardTrayCount;
                cardBufferCnt = SystemModuleCount.ModuleCnt.CardBufferCount;

                for (int i = 0; i < stagecnt; i++)
                {
                    Cells.Add(new StageObject(i + 1));
                }
                for (int i = 0; i < cardArmCnt; i++)
                {
                    CardArms.Add(new CardArmObject(i));
                }
                for (int i = 0; i < cardTrayCnt; i++)
                {
                    CardTrays.Add(new CardTrayObject(i));
                }
                for (int i = 0; i < cardBufferCnt; i++)
                {
                    CardBuffers.Add(new CardBufferObject(i));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void Initialize()
        {
            try
            {
                LoaderModule = this.loaderModule;
                loaderModule.SetLoaderMapConvert(this);
                LoaderMapConvert(loaderModule.GetLoaderInfo().StateMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task LoaderMapConvert(LoaderMap map)
        {
            try
            {
                Task task = new Task(() =>
                {
                    for (int i = 0; i < map.ChuckModules.Count(); i++)
                    {
                        if (Cells.Count > i)
                        {
                            if (LoaderMaster.StageStates.Count > i)
                            {
                                Cells[i].StageState = this.LoaderMaster.StageStates[i];
                                //cell.Name = map.ChuckModules[i].ID.ToString();
                                if (map.ChuckModules[i].Substrate != null)
                                {
                                    Cells[i].State = StageStateEnum.Requested;
                                    Cells[i].TargetName = map.ChuckModules[i].Substrate.PrevPos.ToString();
                                    Cells[i].Progress = map.ChuckModules[i].Substrate.OriginHolder.Index;
                                    Cells[i].WaferObj = map.ChuckModules[i].Substrate;
                                }
                                else
                                {
                                    Cells[i].State = StageStateEnum.Not_Request;
                                }
                                Cells[i].WaferStatus = map.ChuckModules[i].WaferStatus;
                                if (map.CCModules[i].Substrate != null)
                                {
                                    Cells[i].CardObj = map.CCModules[i].Substrate;
                                    Cells[i].CardStatus = map.CCModules[i].WaferStatus;
                                    Cells[i].Progress = map.CCModules[i].Substrate.OriginHolder.Index;
                                }
                                Cells[i].CardStatus = map.CCModules[i].WaferStatus;
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] StageCount : {Cells.Count}, [LoaderSystem.json] ChuckModules : {map.ChuckModules.Count()}");
                            break;
                        }
                    }


                    for (int i = 0; i < map.CardArmModule.Count(); i++)
                    {
                        if (CardArms.Count > i)
                        {
                            if (map.CardArmModule[i].Substrate != null)
                            {
                                CardArms[i].CardObj = map.CardArmModule[i].Substrate;
                            }
                            CardArms[i].WaferStatus = map.CardArmModule[i].WaferStatus;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] CardArmCount : {CardArms.Count}, [LoaderSystem.json] CardArmModule : {map.CardArmModule.Count()}");
                            break;
                        }
                    }

                    for (int i = 0; i < map.CardBufferModules.Count(); i++)
                    {
                        if (CardBuffers.Count > i)
                        {

                            if (map.CardBufferModules[i].Substrate != null)
                            {
                                CardBuffers[i].CardObj = map.CardBufferModules[i].Substrate;
                            }

                            var cardbuffer = loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1);
                            if (cardbuffer.CanDistinguishCard())
                            {
                                CardBuffers[i].CardPRESENCEState = cardbuffer.CardPRESENCEState;
                            }
                            else
                            {
                                if (cardbuffer.CardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_DETACH)
                                {
                                    CardBuffers[i].CardPRESENCEState = ProberInterfaces.CardChange.CardPRESENCEStateEnum.CARD_ATTACH;//홀더와 카드가 구분이 불가능할 경우에는 무조건 카드가 있다고 UI 에 표시 
                                }
                            }

                            CardBuffers[i].WaferStatus = map.CardBufferModules[i].WaferStatus;
                        }
                        else
                        {
                            //LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] CardBufferCount : {CardBuffers.Count}, [LoaderSystem.json] CardBufferModules : {map.CardBufferModules.Count()}");
                            break;
                        }
                    }

                    for (int i = 0; i < map.CardTrayModules.Count(); i++)
                    {
                        if (CardTrays.Count > i)
                        {
                            if (map.CardTrayModules[i].Substrate != null)
                            {
                                CardTrays[i].CardObj = map.CardTrayModules[i].Substrate;
                            }
                            CardTrays[i].WaferStatus = map.CardTrayModules[i].WaferStatus;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderMapConvert(): ModuleCount is incorrect. [ModuleCount.json] CardTrayCount : {CardTrays.Count}, [LoaderSystem.json] CardTrayModules : {map.CardTrayModules.Count()}");
                            break;
                        }
                    }
                    int selectedFoupNum = -1;
                    bool isExternalLotStart = false;
                    if (LoaderMaster.Mode == LoaderMasterMode.External)
                    {
                        if (LoaderMaster.SelectedLotInfo != null)
                        {
                            isExternalLotStart = true;
                            selectedFoupNum = LoaderMaster.SelectedLotInfo.FoupNumber;
                        }

                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"LoaderMapConvert: Error occurred. Err = {err.Message}");
            }
        }
        #region //..Transfer
        private object _TransferSource;
        public object TransferSource
        {
            get { return _TransferSource; }
            set
            {
                if (value != _TransferSource)
                {
                    if (value is true || value is null)
                    {
                        TargetEnable = false;
                    }
                    else
                    {
                        TargetEnable = true;
                    }
                    _TransferSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _TransferTarget;
        public object TransferTarget
        {
            get { return _TransferTarget; }
            set
            {
                if (value != _TransferTarget)
                {
                    if (value is true || value is null)
                    {
                        TransferEnable = false;
                    }
                    else
                    {
                        TransferEnable = true;
                    }
                    _TransferTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool isClickedTransferSource { get; set; } = false;
        private bool isClickedTransferTarget { get; set; } = false;
        private bool isSourceCard { get; set; } = false;

        private AsyncCommand _TransferSourceClickCommand;
        public ICommand TransferSourceClickCommand
        {
            get
            {
                if (null == _TransferSourceClickCommand) _TransferSourceClickCommand = new AsyncCommand(TransferSourceClickCommandFunc);
                return _TransferSourceClickCommand;
            }
        }
        bool sourceToggle = true;
        private async Task TransferSourceClickCommandFunc()
        {
            try
            {
                if (!targetToggle)
                {
                    TransferTarget = null;
                    isClickedTransferTarget = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;
                }

                if (sourceToggle)
                {
                    if (!isClickedTransferTarget)
                    {
                        IsSelected = false;
                        TransferSource = true;
                        isClickedTransferSource = true;
                        SetAllObjectEnable();
                        SetNotExistObjectDisable();


                        if (TransferTarget != null)
                        {
                            TransferTarget = null;
                            isClickedTransferTarget = false;
                            //if (IsCardObject(TransferTarget))
                            //{
                            //    isSourceCard = true;
                            //    SetDisalbeDontMoveCard();
                            //}
                            //else
                            //    SetDisableDontMoveWafer();
                        }

                    }
                    sourceToggle = false;
                }
                else
                {

                    TransferSource = null;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    sourceToggle = true;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TransferTargetClickCommand;
        public ICommand TransferTargetClickCommand
        {
            get
            {
                if (null == _TransferTargetClickCommand) _TransferTargetClickCommand = new AsyncCommand(TransferTargetClickCommandFunc);
                return _TransferTargetClickCommand;
            }
        }
        bool targetToggle = true;
        private async Task TransferTargetClickCommandFunc()
        {
            try
            {
                if (!sourceToggle)
                {
                    if (TransferSource is bool)
                    {
                        return;
                    }
                }
                if (targetToggle)
                {
                    if (!isClickedTransferSource)
                    {
                        IsSelected = false;
                        isClickedTransferTarget = true;
                        TransferTarget = true;
                        SetAllObjectEnable();
                        SetExistObjectDisable();

                        if (TransferSource != null)
                        {
                            if (TransferSource is StageObject)
                            {
                                foreach (var cell in Cells)
                                {
                                    cell.IsEnableTransfer = false;
                                }
                            }
                            if (TransferSource is StageObject && (TransferSource as StageObject).CardStatus == EnumSubsStatus.EXIST && (TransferSource as StageObject).WaferStatus == EnumSubsStatus.EXIST)
                            {

                            }
                            else if (IsSourceCardObject(TransferSource))
                            {
                                isSourceCard = true;
                                SetDisalbeDontMoveCard();
                            }
                            else
                                SetDisableDontMoveWafer();

                        }
                    }
                    targetToggle = false;
                }
                else
                {
                    TransferTarget = null;
                    isClickedTransferTarget = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;
                    //if (IsTargetCardObject(TransferTarget))
                    //{
                    //    isSourceCard = true;
                    //}
                    //else
                    //{
                    //    isSourceCard = false;
                    //}
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private CancellationTokenSourcePack transferCancelTransferTokenSource = new CancellationTokenSourcePack();
        private bool transferAbort { get; set; } = false;
        public void TransferAbortAction(object cardChangeObj)
        {
            try
            {
                transferAbort = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _TransferCommand;
        public ICommand TransferCommand
        {
            get
            {
                if (null == _TransferCommand) _TransferCommand = new AsyncCommand(TransferObjectFunc);
                return _TransferCommand;
            }
        }

        LoaderMap Map;
        private async Task TransferObjectFunc()
        {
            try
            {
                //TransferSource = null;
                //TransferTarget = null;
                await Task.Run(async () =>
                {
                    var loader = loaderModule;
                    Map = loader.GetLoaderInfo().StateMap;
                    bool isExecute = true;
                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    object transfermodule = null;
                    //Source Info
                    SetTransferInfoToModule(TransferSource, ref transfermodule, true);
                    sourceId = (string)transfermodule;
                    //Target Info
                    SetTransferInfoToModule(TransferTarget, ref transfermodule, false);
                    targetId = (ModuleID)transfermodule;

                    if (sourceId == null | targetId == null)
                        return;

                    TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == sourceId).FirstOrDefault();
                    ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == targetId).FirstOrDefault();


                    //if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDTRAY)
                    //{
                    //    ICardBufferTrayModule cardTrayModule = loader.ModuleManager.FindModule(subObj.CurrPos) as ICardBufferTrayModule;
                    //    if (cardTrayModule != null)
                    //    {
                    //        if (!cardTrayModule.IsDrawerSensorOn())
                    //        {
                    //            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Card Tray{cardTrayModule.ID.Index} Open Sensor On.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    //            return;
                    //        }
                    //    }
                    //}
                    //else 
                    if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDTRAY)
                    {
                        ICardBufferTrayModule cardTrayModule = loader.ModuleManager.FindModule(dstLoc.ID) as ICardBufferTrayModule;
                        if (!cardTrayModule.IsDrawerSensorOn())
                        {
                            var retVal = (this).MetroDialogManager().ShowMessageDialog("Transfer Warning", $"Card Tray{cardTrayModule.ID.Index} Open Sensor On.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                            return;
                        }
                    }

                    // CC인경우 (Loading prober card, Unloading prober card )저온인지 온도 체크
                    var ccUnload = subObj.CurrPos.ModuleType == ModuleTypeEnum.CC &
                    (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER
                        | dstLoc.ID.ModuleType == ModuleTypeEnum.CARDTRAY);
                    var ccLoad = dstLoc.ID.ModuleType == ModuleTypeEnum.CC &
                    (subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDARM | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDBUFFER
                       | subObj.CurrPos.ModuleType == ModuleTypeEnum.CARDTRAY);
                    if (ccUnload | ccLoad)
                    {

                        int stageidx = -1;
                        if (ccUnload)
                        {
                            stageidx = (TransferSource as StageObject).Index;
                            //#Hynix_Merge: GetRemoteMediumClient -> GetClient로 바꿔야함.
                            //CardChangeVacuumAndIOStatus data = _LoaderCommunicationManager.GetRemoteMediumClient(stageidx).GPCC_OP_GetCCVacuumStatus();
                            var data = new CardChangeVacuumAndIOStatus();// 임시코드 
                            data = _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(stageidx).GPCC_OP_GetCCVacuumStatus();
                            //======

                            //1 카드팟이 올라가있고 카드팟위에 캐리어가 있을경우
                            if (data.IsLeftUpModuleUp==true && data.IsRightUpModuleUp==true&& data.IsCardExistOnCardPod==true)
                            {
                               if( dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM)
                                {
                                   var cardArm= Map.CardArmModule.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                   if(cardArm!=null)
                                    {
                                        if(cardArm.WaferStatus==EnumSubsStatus.NOT_EXIST)
                                        {
                                            //수행 가능
                                            isExecute = true;
                                        }
                                        else
                                        {
                                            var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardArm WaferStatus : {cardArm.WaferStatus}", EnumMessageStyle.Affirmative);
                                            isExecute = false;
                                            //수행 못함
                                        }
                                    }else
                                    {
                                        var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"Card Arm  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                        //못한다.
                                    }
                                }
                               else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER)
                                {
                                    var cardBuffer = Map.CardBufferModules.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardBuffer != null)
                                    {
                                        if (cardBuffer.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                        {
                                            //수행 가능
                                            isExecute = true;
                                        }
                                        else
                                        {
                                            //못한다.
                                            isExecute = false;
                                            var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer WaferStatus : {cardBuffer.WaferStatus}", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                    else
                                    {
                                        //못한다.
                                        var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                    }
                                }

                            }
                            else if(data.IsLeftUpModuleUp == false && data.IsRightUpModuleUp == false && data.IsCardExistOnCardPod == false)
                            {
                                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDARM)
                                {
                                    var cardArm = Map.CardArmModule.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardArm != null)
                                    {
                                        if (cardArm.WaferStatus == EnumSubsStatus.CARRIER)
                                        {
                                            isExecute = true;
                                        }
                                        else if(cardArm.WaferStatus == EnumSubsStatus.NOT_EXIST)//버퍼에 캐리어 있는데 카드 암 선택해서 이동시키려 할때
                                        {
                                            //if(버퍼 어딘가에 캐리어가 있을경우에는 가능하다)
                                            // else 나머지는 불가능.
                                        }
                                        else
                                        {
                                            var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"card Arm  info Null", EnumMessageStyle.Affirmative);
                                            isExecute = false;
                                        }
                                    }
                                    else
                                    {
                                        var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"card Arm  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                    }
                                }
                                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CARDBUFFER)
                                {
                                    var cardBuffer = Map.CardBufferModules.Where(i => i.ID.Index == dstLoc.ID.Index).FirstOrDefault();
                                    if (cardBuffer != null)
                                    {
                                        EnumCardChangeType clientCardType = EnumCardChangeType.UNDEFINED;
                                        clientCardType = loaderModule.LoaderMaster.GetClient(stageidx).GetCardChangeType();

                                        if (clientCardType == EnumCardChangeType.CARRIER)
                                        {
                                            if (cardBuffer.WaferStatus == EnumSubsStatus.CARRIER)
                                            {
                                                isExecute = true;
                                            }
                                            else
                                            {
                                                isExecute = false;
                                                var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer WaferStatus : {cardBuffer.WaferStatus}", EnumMessageStyle.Affirmative);
                                            }
                                        }
                                        else if (clientCardType == EnumCardChangeType.DIRECT_CARD)
                                        {
                                            if (cardBuffer.WaferStatus == EnumSubsStatus.NOT_EXIST)
                                            {
                                                isExecute = true;
                                            }
                                            else
                                            {
                                                isExecute = false;
                                                var msgRet = this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer WaferStatus : {cardBuffer.WaferStatus}", EnumMessageStyle.Affirmative);
                                            }
                                        }
                                        else
                                        {
                                            LoggerManager.Debug($"TransferObjectFunc() Error dstLoc = {dstLoc.ID.ModuleType}, CardChangeModuleType = {this.CardChangeModule().GetCCType()}");
                                        }
                                    }
                                    else
                                    {
                                        var msgRet = await this.MetroDialogManager().ShowMessageDialog("Transfer Error Message", $"cardBuffer  info Null", EnumMessageStyle.Affirmative);
                                        isExecute = false;
                                    }
                                }
                            }
                            else
                            {

                            }
                            
                        }
                        else if (ccLoad)
                        {
                            stageidx = (TransferTarget as StageObject).Index;
                        }

                        // CC Temp - 추후 파라미터 처리 필요 
                        if (isExecute && stageidx != -1)
                        {
                            var tempClient = _LoaderCommunicationManager.GetProxy<ITempControllerProxy>(stageidx);
                            if (tempClient != null)
                            {
                                //stage 온도 확인 : CCActivatableTemp 보다 낮으면 메시지 출력.
                                transferAbort = false;
                                var temp = tempClient.GetTemperature();
                                var ccActemp = tempClient.GetCCActivatableTemp();
                                var client = loaderModule.LoaderMaster.GetClient(stageidx);
                                if (client != null)
                                {
                                    bool needToSetSV = client.NeedToSetCCActivatableTemp();
                                    var checkCCActiveTemp = loaderModule.LoaderMaster.GetClient(stageidx)?.CardChangeIsConditionSatisfied(needToSetTempToken: false);
                                    if (checkCCActiveTemp != EventCodeEnum.NONE || needToSetSV)
                                    {
                                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Error Message",
                                           $"Card Change operation is possible only above then CC Activatable Temperature : {ccActemp}.\n" +
                                           $" A temperature of the selected stage is {Math.Round(temp, 2)}℃.\n" +
                                           $"Do you want to set the operating conditions and wait?",
                                           EnumMessageStyle.AffirmativeAndNegative).Result;

                                        if (msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                                        {
                                            this.MetroDialogManager().SetMessageToWaitCancelDialog($"Waiting for temperature change to {ccActemp}℃.");
                                            this.MetroDialogManager().ShowCancelButtonOfWaitCancelDiaglog(transferCancelTransferTokenSource.TokenSource, "Abort", TransferAbortAction, this, true);

                                            if (needToSetSV)
                                            {
                                                client.SetCCActiveTemp();
                                                needToSetSV = false;
                                            }
                                            

                                            bool canDoAction = false;
                                            while (canDoAction == false)
                                            {
                                                if (transferAbort)
                                                {
                                                    client.RecoveryCCBeforeTemp();
                                                    this.MetroDialogManager().HiddenCancelButtonOfWaitCancelDiaglog();
                                                    return;
                                                }
                                                else
                                                {
                                                    var cardChangeIsConditionSatisfied = loaderModule.LoaderMaster.GetClient(stageidx)?.CardChangeIsConditionSatisfied(needToSetTempToken: needToSetSV) ?? EventCodeEnum.UNDEFINED;
                                                    

                                                    if (cardChangeIsConditionSatisfied == EventCodeEnum.NONE)
                                                    {
                                                        canDoAction = true;
                                                        this.MetroDialogManager().SetMessageToWaitCancelDialog($"Waiting for card change");
                                                        this.MetroDialogManager().HiddenCancelButtonOfWaitCancelDiaglog();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool canExcuteMap = false;
                    var remainJob = loaderModule.LoaderJobViewList.Count(x => x.JobDone == false);
                    if (remainJob == 0)
                    {
                        canExcuteMap = true;
                    }
                    else
                    {
                        var msgRet = this.MetroDialogManager().ShowMessageDialog("Error Message",
                                        $"Loader is Busy\n LoaderJob already exist.",
                                        EnumMessageStyle.Affirmative);
                        return;
                    }
                    //=========================================================
                    if (isExecute)
                    {
                        SetTransfer(sourceId, targetId);
                        var mapSlicer = new LoaderMapSlicer();
                        var slicedMap = mapSlicer.ManualSlicing(Map);

                        bool isError = false;
                        for (int i = 0; i < slicedMap.Count; i++)
                        {
                            loaderModule.SetRequest(slicedMap[i]);
                            while (true)
                            {

                                if (loader.ModuleState == ModuleStateEnum.DONE)
                                {
                                    loader.ClearRequestData();


                                    LoaderMapConvert(loader.GetLoaderInfo().StateMap);

                                    Thread.Sleep(100);




                                    break;
                                }
                                else if (loader.ModuleState == ModuleStateEnum.ERROR)
                                {
                                    LoaderRecoveryControlVM.Show(_Container, loader.ResonOfError, loader.ErrorDetails, loader.RecoveryBehavior, loader.RecoveryCellIdx);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";
                                    loader.RecoveryBehavior = "";
                                    loader.RecoveryCellIdx = -1;
                                    isError = true;
                                    break;
                                }

                                Thread.Sleep(100);
                            }
                            if (isError)
                            {
                                break;
                            }
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(33);
                    }else
                    {

                    }

                });


                TransferTarget = null;
                isClickedTransferTarget = false;
                SetAllObjectEnable();
                IsSelected = false;
                targetToggle = true;
                TransferSource = null;
                isClickedTransferSource = false;
                sourceToggle = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DockingPauseCommand;
        public ICommand DockingPauseCommand
        {
            get
            {
                if (null == _DockingPauseCommand) _DockingPauseCommand = new AsyncCommand(DockPauseFunc);
                return _DockingPauseCommand;
            }
        }
        private async Task DockPauseFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_Docking_PauseState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _DockingStepUpCommand;
        public ICommand DockingStepUpCommand
        {
            get
            {
                if (null == _DockingStepUpCommand) _DockingStepUpCommand = new AsyncCommand(DockStepUpFunc);
                return _DockingStepUpCommand;
            }
        }
        private async Task DockStepUpFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_Docking_StepUpState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _DockingContinueCommand;
        public ICommand DockingContinueCommand
        {
            get
            {
                if (null == _DockingContinueCommand) _DockingContinueCommand = new AsyncCommand(DockContinueFunc);
                return _DockingContinueCommand;
            }
        }
        private async Task DockContinueFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_Docking_ContinueState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DockingAbortCommand;
        public ICommand DockingAbortCommand
        {
            get
            {
                if (null == _DockingAbortCommand) _DockingAbortCommand = new AsyncCommand(DockAbortFunc);
                return _DockingAbortCommand;
            }
        }
        private async Task DockAbortFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_Docking_AbortState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _UnDockingPauseCommand;
        public ICommand UnDockingPauseCommand
        {
            get
            {
                if (null == _UnDockingPauseCommand) _UnDockingPauseCommand = new AsyncCommand(UnDockPauseFunc);
                return _UnDockingPauseCommand;
            }
        }
        private async Task UnDockPauseFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_UnDocking_PauseState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _UnDockingStepUpCommand;
        public ICommand UnDockingStepUpCommand
        {
            get
            {
                if (null == _UnDockingStepUpCommand) _UnDockingStepUpCommand = new AsyncCommand(UnDockStepUpFunc);
                return _UnDockingStepUpCommand;
            }
        }
        private async Task UnDockStepUpFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_UnDocking_StepUpState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _UnDockingContinueCommand;
        public ICommand UnDockingContinueCommand
        {
            get
            {
                if (null == _UnDockingContinueCommand) _UnDockingContinueCommand = new AsyncCommand(UnDockContinueFunc);
                return _UnDockingContinueCommand;
            }
        }
        private async Task UnDockContinueFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_UnDocking_ContinueState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _UnDockingAbortCommand;
        public ICommand UnDockingAbortCommand
        {
            get
            {
                if (null == _UnDockingAbortCommand) _UnDockingAbortCommand = new AsyncCommand(UnDockAbortFunc);
                return _UnDockingAbortCommand;
            }
        }
        private async Task UnDockAbortFunc()
        {
            try
            {
                _RemoteMediumProxy.GPCC_UnDocking_AbortState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetTransferInfoToModule(object param, ref object Id, bool issource)
        {
            try
            {
                var loader = loaderModule;
                Map = loader.GetLoaderInfo().StateMap;

                if (param is StageObject)
                {

                    if (!isSourceCard)
                    {
                        //Move Wafer

                        int chuckindex = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                        HolderModuleInfo chuckModule = null;
                        if (issource)
                            chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == chuckindex);
                        else
                            chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == chuckindex);

                        if (chuckModule == null)
                            return;
                        if (issource)
                            Id = chuckModule.Substrate.ID.Value;
                        else
                            Id = chuckModule.ID;
                    }
                    else
                    {
                        //Move Card
                        int stageindex = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                        HolderModuleInfo chuckModule = null;
                        if (issource)
                            chuckModule = Map.CCModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == stageindex);
                        else
                            chuckModule = Map.CCModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == stageindex);

                        if (chuckModule == null)
                            return;
                        if (issource)
                            Id = chuckModule.Substrate.ID.Value;
                        else
                            Id = chuckModule.ID;
                    }
                }
                else if (param is SlotObject)
                {
                    int slotindex = Convert.ToInt32(((param as SlotObject).Name.Split('#'))[1]);
                    var waferIdx = (((param as SlotObject).FoupNumber) * 25) + slotindex;
                    var slotModule = Map.CassetteModules[(param as SlotObject).FoupNumber].SlotModules.FirstOrDefault(i => i.ID.Index == waferIdx);
                    if (slotModule == null)
                        return;
                    if (issource)
                        Id = slotModule.Substrate.ID.Value;
                    else
                        Id = slotModule.ID;
                }
                else if (param is ArmObject)
                {
                    int armindex = Convert.ToInt32(((param as ArmObject).Name.Split('#'))[1]);
                    HolderModuleInfo armmodule = null;
                    if (issource)
                        armmodule = Map.ARMModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == armindex);
                    else
                        armmodule = Map.ARMModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == armindex);

                    if (armmodule == null)
                        return;
                    if (issource)
                        Id = armmodule.Substrate.ID.Value;
                    else
                        Id = armmodule.ID;
                }
                else if (param is PAObject)
                {
                    int paindex = Convert.ToInt32(((param as PAObject).Name.Split('#'))[1]);
                    HolderModuleInfo pamodule = null;
                    if (issource)
                        pamodule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == paindex);
                    else
                        pamodule = Map.PreAlignModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == paindex);

                    if (pamodule == null)
                        return;
                    if (issource)
                        Id = pamodule.Substrate.ID.Value;
                    else
                        Id = pamodule.ID;
                }
                else if (param is BufferObject)
                {
                    int bufferindex = Convert.ToInt32(((param as BufferObject).Name.Split('#'))[1]);
                    HolderModuleInfo buffermodule = null;
                    if (issource)
                        buffermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == bufferindex);
                    else
                        buffermodule = Map.BufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == bufferindex);

                    if (buffermodule == null)
                        return;
                    if (issource)
                        Id = buffermodule.Substrate.ID.Value;
                    else
                        Id = buffermodule.ID;
                }
                else if (param is CardTrayObject)
                {
                    int cardtaryindex = Convert.ToInt32(((param as CardTrayObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardtraymodule = null;
                    if (issource)
                        cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardtaryindex);
                    else
                        cardtraymodule = Map.CardTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardtaryindex);

                    if (cardtraymodule == null)
                        return;
                    if (issource)
                        Id = cardtraymodule.Substrate.ID.Value;
                    else
                        Id = cardtraymodule.ID;
                }
                else if (param is CardBufferObject)
                {
                    int cardbufindex = Convert.ToInt32(((param as CardBufferObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardtraymodule = null;
                    if (issource)
                        cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardbufindex);
                    else
                        //cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardbufindex);
                    {
                        cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardbufindex);

                        if (cardtraymodule == null)
                        {
                            cardtraymodule = Map.CardBufferModules.FirstOrDefault(i => i.Substrate != null && i.WaferStatus == EnumSubsStatus.CARRIER && i.ID.Index == cardbufindex);
                        }
                    }

                    if (cardtraymodule == null)
                        return;
                    if (issource)
                        Id = cardtraymodule.Substrate.ID.Value;
                    else
                        Id = cardtraymodule.ID;
                }
                else if (param is CardArmObject)
                {
                    int cardarmindex = Convert.ToInt32(((param as CardArmObject).Name.Split('#'))[1]);
                    HolderModuleInfo cardarmmodule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource)
                        cardarmmodule = Map.CardArmModule.FirstOrDefault(i => i.Substrate != null && i.ID.Index == cardarmindex);
                    else
                        cardarmmodule = Map.CardArmModule.FirstOrDefault(i => i.Substrate == null && i.ID.Index == cardarmindex);

                    if (cardarmmodule == null)
                        return;
                    if (issource)
                        Id = cardarmmodule.Substrate.ID.Value;
                    else
                        Id = cardarmmodule.ID;
                }
                else if (param is FixedTrayInfoObject)
                {
                    int fixedtrayindex = Convert.ToInt32(((param as FixedTrayInfoObject).Name.Split('#'))[1]);
                    HolderModuleInfo fixedTrayModule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource)
                        fixedTrayModule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == fixedtrayindex);
                    else
                        fixedTrayModule = Map.FixedTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == fixedtrayindex);

                    if (fixedTrayModule == null)
                        return;
                    if (issource)
                        Id = fixedTrayModule.Substrate.ID.Value;
                    else
                        Id = fixedTrayModule.ID;
                }
                else if (param is InspectionTrayInfoObject)
                {
                    int iNSPtrayindex = Convert.ToInt32(((param as InspectionTrayInfoObject).Name.Split('#'))[1]);
                    HolderModuleInfo iNSPTrayModule = null;
                    //Change Map.CardBufferModules => CardAramModule 
                    if (issource)
                        iNSPTrayModule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == iNSPtrayindex);
                    else
                        iNSPTrayModule = Map.InspectionTrayModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == iNSPtrayindex);

                    if (iNSPTrayModule == null)
                        return;
                    if (issource)
                        Id = iNSPTrayModule.Substrate.ID.Value;
                    else
                        Id = iNSPTrayModule.ID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTransfer(string id, ModuleID destinationID)
        {
            TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == destinationID).FirstOrDefault();
            bool stageBusy = true;
            if (subObj == null)
            {
                Map = null;
            }

            if (dstLoc == null)
            {
                Map = null;
            }

            if (subObj.CurrPos == destinationID)
            {
                Map = null;
            }

            if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK ||
                subObj.CurrPos.ModuleType == ModuleTypeEnum.CC)

            {
                stageBusy = LoaderMaster.GetClient(subObj.CurrPos.Index).GetRunState();
            }
            else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK ||
                dstLoc.ID.ModuleType == ModuleTypeEnum.CC)
            {
                stageBusy = LoaderMaster.GetClient(dstLoc.ID.Index).GetRunState();
            }

            if (stageBusy == false)
            {
                var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{subObj.CurrPos.Index} is Busy Right Now", EnumMessageStyle.Affirmative);

                if (retVal.Result == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    return;
                }
            }
            if (dstLoc is HolderModuleInfo)
            {
                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                {
                    stageBusy = LoaderMaster.GetClient(dstLoc.ID.Index).GetRunState();
                    if (stageBusy == false)
                    {
                        var retVal = (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{subObj.CurrPos.Index} is Busy Right Now", EnumMessageStyle.Affirmative);

                        if (retVal.Result == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            return;
                        }
                    }
                }
            }
            var card = this.LoaderModule.ModuleManager.GetTransferObjectAll().Where(item => item.ID.Value == id).FirstOrDefault();
            if (card != null)
            {
                if(CardChangeType == EnumCardChangeType.CARRIER)
                {
                    card.CardSkip = ProberInterfaces.Enum.CardSkipEnum.NONE;
                }
                else
                {
                    card.CardSkip = ProberInterfaces.Enum.CardSkipEnum.SKIP;
                }
            }
            if (dstLoc is HolderModuleInfo)
            {
                var currHolder = Map.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                var dstHolder = dstLoc as HolderModuleInfo;

                subObj.PrevHolder = subObj.CurrHolder;
                subObj.PrevPos = subObj.CurrPos;

                subObj.CurrHolder = destinationID;
                subObj.CurrPos = destinationID;

                if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                {
                    if (loaderModule.LoaderMaster.ClientList.ContainsKey(dstLoc.ID.ToString()))
                    {
                        var deviceInfo = loaderModule.LoaderMaster.GetDeviceInfoClient(loaderModule.LoaderMaster.ClientList[dstLoc.ID.ToString()]);
                        subObj.NotchAngle.Value = deviceInfo.NotchAngle.Value;
                    }
                    else
                    {

                    }
                }
                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                {
                    subObj.NotchAngle.Value = (this.loaderModule.ModuleManager.FindModule(dstLoc.ID) as ISlotModule).Cassette.Device.LoadingNotchAngle.Value;
                }

                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;
                dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                dstHolder.Substrate = subObj;
                subObj.DstPos = dstHolder.ID;

                TransferObject currSubObj = LoaderModule.ModuleManager.FindTransferObject(subObj.ID.Value);
                if (currSubObj != null)
                {
                    currSubObj.DstPos = dstHolder.ID;
                }
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
            }
        }

        private void SetNotExistObjectDisable()
        {
            int cellidx = 0;
            foreach (var cell in Cells)
            {
                int idx = 0;
                //if (cellidx < 4)
                //{
                //    idx = cellidx + 8;
                //}
                //else if (cellidx < 8)
                //{
                //    idx = cellidx;
                //}
                //else
                //{
                //    idx = cellidx - 8;
                //}

                if (!(this._LoaderCommunicationManager.Cells[cellidx].StageMode == GPCellModeEnum.ONLINE) && this.LoaderMaster.StageStates[cellidx] == ModuleStateEnum.IDLE || this.LoaderMaster.StageStates[cellidx] == ModuleStateEnum.PAUSED)
                {
                    if (cell.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST
                         | cell.CardStatus == EnumSubsStatus.EXIST)
                        cell.IsEnableTransfer = true;
                    else
                        cell.IsEnableTransfer = false;
                }
                else
                {
                    cell.IsEnableTransfer = false;
                }
                cellidx++;
            }
            foreach (var cardbuf in CardBuffers)
            {
                var cardbuffermodule = this.loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, CardBuffers.IndexOf(cardbuf) + 1);
                if (cardbuf.WaferStatus != EnumSubsStatus.EXIST
                    && cardbuf.WaferStatus != EnumSubsStatus.CARRIER)
                {
                    cardbuf.IsEnableTransfer = false;
                }
                else if (cardbuffermodule.CardPRESENCEState == ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                {
                    cardbuf.IsEnableTransfer = false;
                }
            }
            foreach (var cardtary in CardTrays)
            {
                if (cardtary.WaferStatus != EnumSubsStatus.EXIST)
                    cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                if (cardarm.WaferStatus != EnumSubsStatus.EXIST)
                {
                    if(cardarm.WaferStatus == EnumSubsStatus.CARRIER)
                    {
                        cardarm.IsEnableTransfer = true;
                    }
                    else
                    {
                        cardarm.IsEnableTransfer = false;
                    }
                }
               
            }
        }
        private void SetExistObjectDisable()
        {
            int cellidx = 0;
            foreach (var cell in Cells)
            {
                int idx = 0;
                //if (cellidx < 4)
                //{
                //    idx = cellidx + 8;
                //}
                //else if (cellidx < 8)
                //{
                //    idx = cellidx;
                //}
                //else
                //{
                //    idx = cellidx - 8;
                //}

                if (!(this._LoaderCommunicationManager.Cells[cellidx].StageMode == GPCellModeEnum.ONLINE) && this.LoaderMaster.StageStates[cellidx] == ModuleStateEnum.IDLE || this.LoaderMaster.StageStates[cellidx] == ModuleStateEnum.PAUSED)
                {
                    if (cell.WaferStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST || cell.CardStatus == ProberInterfaces.EnumSubsStatus.NOT_EXIST)
                        cell.IsEnableTransfer = true;
                    else
                    {
                        cell.IsEnableTransfer = false;
                    }
                }
                else
                {
                    cell.IsEnableTransfer = false;
                }
                cellidx++;
            }
            foreach (var cardbuf in CardBuffers)
            {
                var cardbuffermodule = this.loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, CardBuffers.IndexOf(cardbuf) + 1);

                if (cardbuf.WaferStatus != EnumSubsStatus.NOT_EXIST
                    && cardbuf.WaferStatus != EnumSubsStatus.CARRIER)
                {
                    cardbuf.IsEnableTransfer = false;
                }
                else if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                {
                    cardbuf.IsEnableTransfer = false;
                }
            }
            foreach (var cardtary in CardTrays)
            {
                if (cardtary.WaferStatus != EnumSubsStatus.NOT_EXIST && cardtary.WaferStatus != EnumSubsStatus.CARRIER)
                    cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                if (cardarm.WaferStatus != EnumSubsStatus.NOT_EXIST)
                    cardarm.IsEnableTransfer = false;
            }
        }

        private void SetAllObjectEnable()
        {
            int cellidx = 0;
            foreach (var cell in Cells)
            {
                int idx = 0;
                //if (cellidx < 4)
                //{
                //    idx = cellidx + 8;
                //}
                //else if (cellidx < 8)
                //{
                //    idx = cellidx;
                //}
                //else
                //{
                //    idx = cellidx - 8;
                //}
                if (!(this._LoaderCommunicationManager.Cells[cellidx].StageMode == GPCellModeEnum.ONLINE) && (this.LoaderMaster.StageStates[cellidx] == ModuleStateEnum.IDLE || this.LoaderMaster.StageStates[cellidx] == ModuleStateEnum.PAUSED))
                {
                    cell.IsEnableTransfer = true;
                }
                else
                {
                    cell.IsEnableTransfer = false;
                }
                cellidx++;
            }
            foreach (var cardbuf in CardBuffers)
            {
                cardbuf.IsEnableTransfer = true;
            }
            foreach (var cardtary in CardTrays)
            {
                cardtary.IsEnableTransfer = true;
            }
            foreach (var cardarm in CardArms)
            {
                cardarm.IsEnableTransfer = true;
            }
        }

        private void SetDisalbeDontMoveCard()
        {
            foreach (var cell in Cells)
            {
                if (cell.CardStatus == EnumSubsStatus.EXIST)
                    cell.IsEnableTransfer = false;
            }

            foreach (var cardbuf in CardBuffers)
            {
                var cardbuffermodule = this.loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, CardBuffers.IndexOf(cardbuf) + 1);
                if (cardbuf.WaferStatus == EnumSubsStatus.EXIST)
                {
                    cardbuf.IsEnableTransfer = false;
                }
                else if (cardbuf.WaferStatus == EnumSubsStatus.CARRIER
                    && (TransferSource is CardBufferObject || TransferSource is CardTrayObject))
                {
                    cardbuf.IsEnableTransfer = false;
                }

                if (cardbuffermodule.CardPRESENCEState != ProberInterfaces.CardChange.CardPRESENCEStateEnum.EMPTY)
                {
                    cardbuf.IsEnableTransfer = false;
                }
            }
            foreach (var cardtary in CardTrays)
            {
                if (cardtary.WaferStatus == EnumSubsStatus.EXIST)
                    cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                if (cardarm.WaferStatus == EnumSubsStatus.EXIST)
                    cardarm.IsEnableTransfer = false;
            }

        }
        private void SetDisableDontMoveWafer()
        {
            foreach (var cardbuf in CardBuffers)
            {
                cardbuf.IsEnableTransfer = false;
            }
            foreach (var cardtary in CardTrays)
            {
                cardtary.IsEnableTransfer = false;
            }
            foreach (var cardarm in CardArms)
            {
                cardarm.IsEnableTransfer = false;
            }
        }
        private bool IsSourceCardObject(object obj)
        {
            bool retVal = false;
            try
            {
                if (obj is CardArmObject | obj is CardBufferObject | obj is CardTrayObject)
                    retVal = true;
                else
                {
                    if (obj is StageObject)
                    {

                        if (TransferSource != null && (TransferSource is CardArmObject | TransferSource is CardBufferObject | TransferSource is CardTrayObject))
                        {
                            retVal = true;
                        }
                        if ((obj as StageObject).CardStatus == EnumSubsStatus.EXIST)
                        {
                            retVal = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private bool IsTargetCardObject(object obj)
        {
            bool retVal = false;
            try
            {
                if (obj is CardArmObject | obj is CardBufferObject | obj is CardTrayObject)
                    retVal = true;
                else
                {
                    if (obj is StageObject)
                    {
                        if (TransferSource != null && (TransferSource is CardArmObject | TransferSource is CardBufferObject | TransferSource is CardTrayObject))
                        {
                            retVal = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void InitCell()
        {
            try
            {
                var loaderSupervisor = _Container.Resolve<ILoaderSupervisor>();
                foreach (var client in loaderSupervisor.ClientList)
                {
                    //client.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _CurBehaviorIdx = 0;
        public int CurBehaviorIdx
        {
            get { return _CurBehaviorIdx; }
            set
            {
                if (value != _CurBehaviorIdx)
                {
                    _CurBehaviorIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SequenceBehaviorStateEnum _DockCurrState = SequenceBehaviorStateEnum.PAUSED;
        public SequenceBehaviorStateEnum DockCurrState
        {
            get { return _DockCurrState; }
            set
            {
                if (value != _DockCurrState)
                {
                    _DockCurrState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SequenceBehaviorStateEnum _UnDockCurrState = SequenceBehaviorStateEnum.PAUSED;
        public SequenceBehaviorStateEnum UnDockCurrState
        {
            get { return _UnDockCurrState; }
            set
            {
                if (value != _UnDockCurrState)
                {
                    _UnDockCurrState = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Loader COmmand
        private RelayCommand<object> _ObjectClickCommand;
        public ICommand ObjectClickCommand
        {
            get
            {
                if (null == _ObjectClickCommand) _ObjectClickCommand = new RelayCommand<object>(ObjectClickCommandFunc);
                return _ObjectClickCommand;
            }
        }

        private void ObjectClickCommandFunc(object param)
        {
            try
            {
                object[] _param = param as object[];
                // 0 : ListView
                // 1 : SelectedIndex
                // 2 : SelectedItem
                ListView listView = null;
                int index = -1;
                ListViewItem listViewItem = null;
                ListView foupListView = null;


                listView = (ListView)_param[0];
                index = (int)_param[1];
                if (index == -1)
                    return;
                listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);
                object item = _param[2];
                listViewItem.IsSelected = false;
                if (isClickedTransferSource)
                {
                    TransferSource = item;
                    isClickedTransferTarget = false;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    sourceToggle = true;
                }
                else if (isClickedTransferTarget)
                {
                    TransferTarget = item;
                    isClickedTransferTarget = false;
                    isClickedTransferSource = false;
                    SetAllObjectEnable();
                    IsSelected = false;
                    targetToggle = true;
                    if (IsTargetCardObject(TransferTarget))
                    {
                        isSourceCard = true;
                    }
                    else
                    {
                        isSourceCard = false;
                    }
                }
                else
                {
                    if (ModuleInfoEnable)
                    {


                        if (item is SlotObject)
                        {
                            var slot = item as SlotObject;
                            if (slot.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.SLOT, item, this.LoaderMaster);
                            }

                        }
                        else if (item is ArmObject)
                        {
                            var arm = item as ArmObject;
                            if (arm.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.ARM, item,this.LoaderMaster);
                            }
                        }
                        else if (item is PAObject)
                        {
                            var PAObj = item as PAObject;
                            if (PAObj.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.PA, item, this.LoaderMaster);
                            }
                        }
                        else if (item is BufferObject)
                        {
                            var BufferObj = item as BufferObject;
                            if (BufferObj.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.BUFFER, item, this.LoaderMaster);
                            }
                        }
                        else if (item is StageObject)
                        {
                            var Chuck = item as StageObject;
                            if (Chuck.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CHUCK, item, this.LoaderMaster);
                            }
                            if (Chuck.CardStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CHUCK, item, this.LoaderMaster);
                            }
                        }
                        else if (item is FixedTrayInfoObject)
                        {
                            var FixedTrayObj = item as FixedTrayInfoObject;
                            if (FixedTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.FIXEDTRAY, item, this.LoaderMaster);
                            }
                        }
                        else if (item is InspectionTrayObject)
                        {
                            var INSPTrayObj = item as InspectionTrayObject;
                            if (INSPTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.INSPECTIONTRAY, item, this.LoaderMaster);
                            }
                        }
                        else if (item is CardTrayObject)
                        {
                            var CardTrayObj = item as CardTrayObject;
                            if (CardTrayObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CARDTRAY, item, this.LoaderMaster);
                            }
                        }
                        else if (item is CardBufferObject)
                        {
                            var CardBufferObj = item as CardBufferObject;
                            if (CardBufferObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CARDBUFFER, item, this.LoaderMaster);
                            }
                        }
                        else if (item is CardArmObject)
                        {
                            var CardArmObj = item as CardArmObject;
                            if (CardArmObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.CARDARM, item, this.LoaderMaster);
                            }
                        }
                        else if (item is InspectionTrayInfoObject)
                        {
                            var INSPObj = item as InspectionTrayInfoObject;
                            if (INSPObj.WaferStatus == ProberInterfaces.EnumSubsStatus.EXIST)
                            {
                                ModuleInfoVM.Show(ModuleTypeEnum.INSPECTIONTRAY, item, this.LoaderMaster);
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
        #endregion

        private void VacUpdateData()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(2000);

                    var vacdata = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                    if (vacdata != null)
                    {
                        CardOnPogoPod = vacdata.CardOnPogoPod;
                        CardPogoTouched = vacdata.CardPogoTouched;
                        TesterPogoTouched = vacdata.TesterPogoTouched;
                        IsTesterMotherBoardConnected = vacdata.IsTesterMotherBoardConnected;
                        IsTesterPCBUnlocked = vacdata.IsTesterPCBUnlocked;

                    }
                    DockSequences = _RemoteMediumProxy.GPCC_OP_GetDockSequence();
                    UnDockSequences = _RemoteMediumProxy.GPCC_OP_GetUnDockSequence();
                    CurBehaviorIdx = _RemoteMediumProxy.GPCC_OP_GetCurBehaviorIdx();
                    if (DockSequences.Count > 0)
                    {
                        if (CurBehaviorIdx >= 0 && CurBehaviorIdx < DockSequencesCount)//정상적일때
                        {
                            DockCurrState = DockSequences[CurBehaviorIdx].StateEnum;
                        }
                        else//마지막일때 고려, 버튼 때문에 한거임
                        {
                            DockCurrState = DockSequences[CurBehaviorIdx - 1].StateEnum;
                        }
                    }

                    if (UnDockSequences.Count > 0)
                    {
                        if (CurBehaviorIdx >= 0 && CurBehaviorIdx < UnDockSequencesCount)//정상적일때
                        {
                            UnDockCurrState = UnDockSequences[CurBehaviorIdx].StateEnum;
                        }
                        else//마지막일때 고려, 버튼 때문에 한거임
                        {
                            UnDockCurrState = UnDockSequences[CurBehaviorIdx - 1].StateEnum;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //System.Timers.Timer _VacuumUpdateTimer;
        //private static int VacUpdateInterValInms = 250;

        //private async void _VacUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{

        //    try
        //    {
        //        if (_RemoteMediumProxy != null)
        //        {
        //            VacUpdateInterValInms = 3000;
        //            _VacuumUpdateTimer.Interval = VacUpdateInterValInms;
        //            await Task.Run(() =>
        //            {
        //                var vacdata = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
        //                if (vacdata != null)
        //                {
        //                    CardOnPogoPod = vacdata.CardOnPogoPod;
        //                    CardPogoTouched = vacdata.CardPogoTouched;
        //                    TesterPogoTouched = vacdata.TesterPogoTouched;

        //                }
        //            });
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        public void DeInitModule()
        {
            try
            {
                if (Initialized == false)
                {
                    Initialized = true;
                }
            }
            catch (Exception err)
            {

                throw err;
            }
        }

        Thread VacUpdateThread;

        public EventCodeEnum InitModule()
        {
            try
            {
                InitData();
                Initialize();
                VacUpdateThread = new Thread(new ThreadStart(VacUpdateData));
                //_VacuumUpdateTimer = new System.Timers.Timer();
                //_VacuumUpdateTimer.Interval = VacUpdateInterValInms;
                //_VacuumUpdateTimer.Elapsed += _VacUpdateTimer_Elapsed;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public async Task<EventCodeEnum> InitViewModel()
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

        private int _SelectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set
            {
                if (value != _SelectedTabIndex)
                {
                    _SelectedTabIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public void UpdateSequence_Dock_EnumState(ObservableCollection<ISequenceBehaviorGroupItem> update_seq)
        //{
        //    try
        //    {
        //        DockSequences = update_seq;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public void UpdateSequence_UnDock_EnumState(ObservableCollection<ISequenceBehaviorGroupItem> update_seq)
        //{
        //    try
        //    {
        //        UnDockSequences = update_seq;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        public void Update_ErrorMessage(string update_MSG)
        {
            try
            {
                Reason = update_MSG;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private IStageObject _SelectedStage;
        public IStageObject SelectedStage
        {
            get { return _SelectedStage; }
            set
            {
                if (value != _SelectedStage)
                {
                    _SelectedStage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            //카드체인지 파람 받아오기  
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (_LoaderCommunicationManager != null)
                {
                    _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
                }

                loaderModule.SetLoaderMapConvert(this);
                loaderModule.BroadcastLoaderInfo();
                if (Extensions_IParam.ProberRunMode == RunMode.DEFAULT)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.CardBufferCount; i++)
                    {
                        var carriervac = loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1).GetDICARRIERVAC();
                        if (carriervac != null)
                        {
                            loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1).RecoveryCardStatus();
                        }
                    }
                }

                if (_RemoteMediumProxy != null)
                {
                    //await Task.Run(() =>
                    //{GPCC_OP_GetGPCcDockUnDockList
                    //ObservableCollection<String> docklist = await _RemoteMediumProxy.GPCC_OP_GetGPCcDockList();
                    //DockSequenceCollection = new AsyncObservableCollection<String>(docklist);

                    await _RemoteMediumProxy.GPCC_OP_PageSwitchCommand();

                    var ccsysParam = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                    ContactCorrectionZ = ccsysParam.GP_ContactCorrectionZ;
                    ContactCorrectionX = ccsysParam.GP_ContactCorrectionX;
                    ContactCorrectionY = ccsysParam.GP_ContactCorrectionY;
                    CardUndockContactOffsetZ = ccsysParam.GP_UndockCorrectionZ;
                    FocusRange = ccsysParam.GP_CardFocusRange;
                    await _RemoteMediumProxy.GPCC_OP_SetFocusRangeValueCommand(FocusRange);
                    CardAlignRetryCount = ccsysParam.GP_CardAlignRetryCount;
                    DistanceOffset = ccsysParam.GP_DistanceOffset;
                    CardTopFromChuckPlane = ccsysParam.CardTopFromChuckPlane;
                    WaitForCardPermitEnable = ccsysParam.WaitForCardPermitEnable;
                    CardChangeType = ccsysParam.CardChangeType;

                    var ccdevParam = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeDevParamData();
                    CardContactPosZ = ccdevParam.GP_CardContactPosZ;

                    await Task.Run(() =>
                    {
                        var vacstatus = _RemoteMediumProxy.GPCC_OP_GetCCVacuumStatus();
                        this.CardOnPogoPod = vacstatus.CardOnPogoPod;
                        this.TesterPogoTouched = vacstatus.TesterPogoTouched;
                        this.CardPogoTouched = vacstatus.CardPogoTouched;
                    });
                    DockSequences = _RemoteMediumProxy.GPCC_OP_GetDockSequence();
                    UnDockSequences = _RemoteMediumProxy.GPCC_OP_GetUnDockSequence();
                    //_VacuumUpdateTimer.Start();


                    if (VacUpdateThread.ThreadState == ThreadState.Unstarted)
                    {
                        VacUpdateThread.Start();
                    }
                    else if (VacUpdateThread.ThreadState == ThreadState.Suspended)
                    {
                        VacUpdateThread.Resume();
                    }
                    //_VacuumUpdateTimer.Start();
                    DockSequencesCount = DockSequences.Count();
                    UnDockSequencesCount = UnDockSequences.Count();
                    SelectedStage = _LoaderCommunicationManager.Cells[_LoaderCommunicationManager.SelectedStageIndex - 1];

                    if (CardChangeType == EnumCardChangeType.CARRIER)
                    {
                        SelectedTabIndex = Convert.ToInt32(CCSettingOPTabControlEnum.DOCKSEUQUENCE);
                    }
                    else
                    {
                        SelectedTabIndex = Convert.ToInt32(CCSettingOPTabControlEnum.PARAMETER);
                    }
                    ret = EventCodeEnum.NONE;
                    //});
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return ret;

            //return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.DEFAULT)
                {
                    for (int i = 0; i < SystemModuleCount.ModuleCnt.CardBufferCount; i++)
                    {
                        var carriervac = loaderModule.ModuleManager.FindModule<ICardBufferModule>(ModuleTypeEnum.CARDBUFFER, i + 1).GetDICARRIERVAC();
                        if (carriervac != null)
                        {
                            LoggerManager.Debug($"[LoaderGPCardChangeOPViewModel] Cleanup(): SetCardTrayVac({carriervac.Value})");
                            if (carriervac.Value == false)
                            {
                                loaderModule.GetLoaderCommands().SetCardTrayVac(false);
                            }
                        }
                    }
                } 

                if (_RemoteMediumProxy != null)
                {
                    await _RemoteMediumProxy.GPCC_OP_CleanUpCommand();
                    //_VacuumUpdateTimer.Stop();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                VacUpdateThread.Suspend();
            }

            return EventCodeEnum.NONE;
            //return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (_RemoteMediumProxy != null)
                {
                    //_VacuumUpdateTimer.Stop();
                    VacUpdateThread.Join();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region GPCCOPViewModel Property


        
        private double _ContactCorrectionZ;
        public double ContactCorrectionZ
        {
            get { return _ContactCorrectionZ; }
            set
            {
                if (value != _ContactCorrectionZ)
                {
                    _ContactCorrectionZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ContactCorrectionX;
        public double ContactCorrectionX
        {
            get { return _ContactCorrectionX; }
            set
            {
                if (value != _ContactCorrectionX)
                {
                    _ContactCorrectionX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ContactCorrectionY;
        public double ContactCorrectionY
        {
            get { return _ContactCorrectionY; }
            set
            {
                if (value != _ContactCorrectionY)
                {
                    _ContactCorrectionY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CardContactPosZ;
        public double CardContactPosZ
        {
            get { return _CardContactPosZ; }
            set
            {
                if (value != _CardContactPosZ)
                {
                    _CardContactPosZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusRange;
        public double FocusRange
        {
            get { return _FocusRange; }
            set
            {
                if (value != _FocusRange)
                {
                    _FocusRange = value;
                    //if (_RemoteMediumProxy != null)
                    //{
                    if (_FocusRange > 2000)
                    {
                        _FocusRange = 2000;
                    }
                    //_RemoteMediumProxy.GPCC_OP_SetFocusRangeValueCommand(_FocusRange);
                    //}
                    RaisePropertyChanged();
                }
            }
        }

        private double _FocusRangeSetting;
        public double FocusRangeSetting
        {
            get { return _FocusRangeSetting; }
            set
            {
                if (value != _FocusRangeSetting)
                {
                    _FocusRangeSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private double _CardTopFromChuckPlane;
        public double CardTopFromChuckPlane
        {
            get { return _CardTopFromChuckPlane; }
            set
            {
                if (value != _CardTopFromChuckPlane)
                {
                    _CardTopFromChuckPlane = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CardTopFromChuckPlaneSetting;
        public double CardTopFromChuckPlaneSetting
        {
            get { return _CardTopFromChuckPlaneSetting; }
            set
            {
                if (value != _CardTopFromChuckPlaneSetting)
                {
                    _CardTopFromChuckPlaneSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CardAlignRetryCountSetting;
        public int CardAlignRetryCountSetting
        {
            get { return _CardAlignRetryCountSetting; }
            set
            {
                if (value != _CardAlignRetryCountSetting)
                {
                    _CardAlignRetryCountSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DistanceOffset;
        public double DistanceOffset
        {
            get { return _DistanceOffset; }
            set
            {
                if (value != _DistanceOffset)
                {
                    _DistanceOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _DistanceOffsetSetting;
        public double DistanceOffsetSetting
        {
            get { return _DistanceOffsetSetting; }
            set
            {
                if (value != _DistanceOffsetSetting)
                {
                    _DistanceOffsetSetting = value;
                    RaisePropertyChanged();
                }
            }
        }

        
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


        #region ==> CardOnPogoPod
        private bool _CardOnPogoPod;
        public bool CardOnPogoPod
        {
            get { return _CardOnPogoPod; }
            set
            {
                if (value != _CardOnPogoPod)
                {
                    _CardOnPogoPod = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _TesterPogoTouched;
        public bool TesterPogoTouched
        {
            get { return _TesterPogoTouched; }
            set
            {
                if (value != _TesterPogoTouched)
                {
                    _TesterPogoTouched = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _CardPogoTouched;
        public bool CardPogoTouched
        {
            get { return _CardPogoTouched; }
            set
            {
                if (value != _CardPogoTouched)
                {
                    _CardPogoTouched = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> DockPosStr
        private string _DockPosStr;
        public string DockPosStr
        {
            get { return _DockPosStr; }
            set
            {
                if (value != _DockPosStr)
                {
                    _DockPosStr = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _IsTesterMotherBoardConnected;
        public bool IsTesterMotherBoardConnected
        {
            get { return _IsTesterMotherBoardConnected; }
            set
            {
                if (value != _IsTesterMotherBoardConnected)
                {
                    _IsTesterMotherBoardConnected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsTesterPCBUnlocked;
        public bool IsTesterPCBUnlocked
        {
            get { return _IsTesterPCBUnlocked; }
            set
            {
                if (value != _IsTesterPCBUnlocked)
                {
                    _IsTesterPCBUnlocked = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _WaitForCardPermitEnable;
        public bool WaitForCardPermitEnable
        {
            get { return _WaitForCardPermitEnable; }
            set
            {
                if (value != _WaitForCardPermitEnable)
                {
                    _WaitForCardPermitEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumCardChangeType _CardChangeType;
        public EnumCardChangeType CardChangeType
        {
            get { return _CardChangeType; }
            set
            {
                if (value != _CardChangeType)
                {
                    _CardChangeType = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region Command
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
        private async Task MoveToLoaderCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_MoveToLoaderCommand();
                    //});
                }

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
        private async Task RaisePodCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_RaisePodCommand();
                    //});
                }

            }
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
        private async Task DropPodCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_DropPodCommand();

                    //});
                }


            }
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
        private async Task TopPlateSolLockCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_TopPlateSolLockCommand();

                    //});
                }


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
        private async Task TopPlateSolUnLockCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_TopPlateSolUnLockCommand();

                    //});
                }


            }
        }
        #endregion

        #region ==> PCardPodVacuumOffCommand
        private AsyncCommand _PCardPodVacuumOffCommand;
        public IAsyncCommand PCardPodVacuumOffCommand
        {
            get
            {
                if (null == _PCardPodVacuumOffCommand)
                {
                    _PCardPodVacuumOffCommand = new AsyncCommand(PCardPodVacuumOffCommandFunc);
                }

                return _PCardPodVacuumOffCommand;
            }
        }
        private async Task PCardPodVacuumOffCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_PCardPodVacuumOffCommand();
                    //});
                }

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
        private async Task PCardPodVacuumOnCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_PCardPodVacuumOnCommand();

                    //});
                }


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
        private async Task UpPlateTesterCOfftactVacuumOffCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_UpPlateTesterCOfftactVacuumOffCommand();

                    //});
                }


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
        private async Task UpPlateTesterContactVacuumOnCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_UpPlateTesterContactVacuumOnCommand();

                    //});
                }


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
        private async Task UpPlateTesterPurgeAirOffCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_OP_UpPlateTesterPurgeAirOffCommand();
                }
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
        private async Task UpPlateTesterPurgeAirOnCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_OP_UpPlateTesterPurgeAirOnCommand();
                }
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
        private async Task DockCardCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_DockCardCommand();

                    //});
                }


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
        private async Task UnDockCardCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_UnDockCardCommand();

                    //});
                }


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
        private async Task CardAlignCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(async() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_CardAlignCommand();

                    //});
                }


            }

        }
        #endregion

        #region ==> CardContactSettingZCommand
        private AsyncCommand _CardContactSettingZCommand;
        public ICommand CardContactSettingZCommand
        {
            get
            {
                if (null == _CardContactSettingZCommand) _CardContactSettingZCommand = new AsyncCommand(CardContactSettingZCommandFunc);
                return _CardContactSettingZCommand;
            }
        }
        private async Task CardContactSettingZCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetContactOffsetZValueCommand(CardContactOffsetSettingZ);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                var devdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeDevParamData();
                //this.CardChangeSysParam.GP_CardContactPosZ = data.GP_CardContactPosZ;
                CardContactPosZ = devdata.GP_CardContactPosZ;
                ContactCorrectionZ = sysdata.GP_ContactCorrectionZ;
                ContactCorrectionX = sysdata.GP_ContactCorrectionX;
                ContactCorrectionY = sysdata.GP_ContactCorrectionY;
            }
            //});
        }
        #endregion

        #region ==> CardContactSettingXCommand
        private AsyncCommand _CardContactSettingXCommand;
        public ICommand CardContactSettingXCommand
        {
            get
            {
                if (null == _CardContactSettingXCommand) _CardContactSettingXCommand = new AsyncCommand(CardContactSettingXCommandFunc);
                return _CardContactSettingXCommand;
            }
        }
        private async Task CardContactSettingXCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetContactOffsetXValueCommand(CardContactOffsetSettingX);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                var devdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeDevParamData();
                //this.CardChangeSysParam.GP_CardContactPosZ = data.GP_CardContactPosZ;
                CardContactPosZ = devdata.GP_CardContactPosZ;
                ContactCorrectionX = sysdata.GP_ContactCorrectionX;
            }
            //});
        }
        #endregion

        #region ==> CardContactSettingYCommand
        private AsyncCommand _CardContactSettingYCommand;
        public ICommand CardContactSettingYCommand
        {
            get
            {
                if (null == _CardContactSettingYCommand) _CardContactSettingYCommand = new AsyncCommand(CardContactSettingYCommandFunc);
                return _CardContactSettingYCommand;
            }
        }
        private async Task CardContactSettingYCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetContactOffsetYValueCommand(CardContactOffsetSettingY);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                var devdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeDevParamData();
                CardContactPosZ = devdata.GP_CardContactPosZ;
                ContactCorrectionY = sysdata.GP_ContactCorrectionY;
                
            }
            //});
        }
        #endregion
        #region ==> CardContactSettingZCommand
        private AsyncCommand<bool> _SetWaitForCardPermitEnableCommand;
        public IAsyncCommand SetWaitForCardPermitEnableCommand
        {
            get
            {
                if (null == _SetWaitForCardPermitEnableCommand) _SetWaitForCardPermitEnableCommand = new AsyncCommand<bool>(SetWaitForCardPermitEnableCommandFunc);
                return _SetWaitForCardPermitEnableCommand;
            }
        }
        private async Task SetWaitForCardPermitEnableCommandFunc(bool enable)
        {
            //await Task.Run(() =>
            //{

            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_SetWaitForCardPermitEnableCommand(enable);             
            }
            var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
            WaitForCardPermitEnable = sysdata.WaitForCardPermitEnable;
            //});
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
        private double _CardUndockContactOffsetZ;
        public double CardUndockContactOffsetZ
        {
            get { return _CardUndockContactOffsetZ; }
            set
            {
                if (value != _CardUndockContactOffsetZ)
                {
                    _CardUndockContactOffsetZ = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> CardUndockContactSettingZCommand
        private AsyncCommand _CardUndockContactSettingZCommand;
        public ICommand CardUndockContactSettingZCommand
        {
            get
            {
                if (null == _CardUndockContactSettingZCommand) _CardUndockContactSettingZCommand = new AsyncCommand(CardUndockContactSettingZCommandFunc);
                return _CardUndockContactSettingZCommand;
            }
        }
        private async Task CardUndockContactSettingZCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetUndockContactOffsetZValueCommand(CardUndockContactOffsetSettingZ);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                var devdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeDevParamData();
                //this.CardChangeSysParam.GP_CardContactPosZ = data.GP_CardContactPosZ;
                CardContactPosZ = devdata.GP_CardContactPosZ;
                CardUndockContactOffsetZ = sysdata.GP_UndockCorrectionZ;
                //ContactCorrectionZ = sysdata.GP_ContactCorrectionZ;
            }
            //});
        }
        #endregion

        #region ==> FocusRangeSettingZCommand
        private AsyncCommand _FocusRangeSettingZCommand;
        public ICommand FocusRangeSettingZCommand
        {
            get
            {
                if (null == _FocusRangeSettingZCommand) _FocusRangeSettingZCommand = new AsyncCommand(FocusRangeSettingZCommandFunc);
                return _FocusRangeSettingZCommand;
            }
        }
        private async Task FocusRangeSettingZCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                if (FocusRangeSetting > 2000)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("FocusRange Setting Failed", $"FocusRange cannot exceed 2000.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await _RemoteMediumProxy.GPCC_OP_CardFocusRangeSettingZCommand(FocusRangeSetting);
                    var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                    FocusRange = sysdata.GP_CardFocusRange;
                    await _RemoteMediumProxy.GPCC_OP_SetFocusRangeValueCommand(FocusRange);
                }
            }
            //});
        }
        #endregion
        #region ==> SetAlignRetryCountCommand
        private AsyncCommand _SetAlignRetryCountCommand;
        public ICommand SetAlignRetryCountCommand
        {
            get
            {
                if (null == _SetAlignRetryCountCommand) _SetAlignRetryCountCommand = new AsyncCommand(SetAlignRetryCountCommandFunc);
                return _SetAlignRetryCountCommand;
            }
        }
        private async Task SetAlignRetryCountCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetAlignRetryCountCommand(CardAlignRetryCountSetting);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                CardAlignRetryCount = sysdata.GP_CardAlignRetryCount;
            }
            //});
        }
        #endregion

        #region ==> SetDistanceOffsetCommand
        private AsyncCommand _SetDistanceOffsetCommand;
        public ICommand SetDistanceOffsetCommand
        {
            get
            {
                if (null == _SetDistanceOffsetCommand) _SetDistanceOffsetCommand = new AsyncCommand(SetDistanceOffsetCommandFunc);
                return _SetDistanceOffsetCommand;
            }
        }
        private async Task SetDistanceOffsetCommandFunc()
        {
            //await Task.Run(() =>
            //{
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetDistanceOffsetCommand(DistanceOffsetSetting);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                DistanceOffset = sysdata.GP_DistanceOffset;
            }
            //});
        }
        #endregion

        #region ==> SetCardTopFromChuckPlaneSetting
        private AsyncCommand _SetCardTopFromChuckPlaneSettingCommand;
        public ICommand SetCardTopFromChuckPlaneSettingCommand
        {
            get
            {
                if (null == _SetCardTopFromChuckPlaneSettingCommand) _SetCardTopFromChuckPlaneSettingCommand = new AsyncCommand(SetCardTopFromChuckPlaneSettingCommandFunc);
                return _SetCardTopFromChuckPlaneSettingCommand;
            }
        }
        private async Task SetCardTopFromChuckPlaneSettingCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                await _RemoteMediumProxy.GPCC_OP_SetCardTopFromChuckPlaneSettingCommand(CardTopFromChuckPlaneSetting);
                var sysdata = await _RemoteMediumProxy.GPCC_OP_GetGPCardChangeSysParamData();
                CardTopFromChuckPlane = sysdata.CardTopFromChuckPlane;
            }
        }
        #endregion

        #region ==> SmallDropChuckCommand
        private AsyncCommand _SmallDropChuckCommand;
        public IAsyncCommand SmallDropChuckCommand
        {
            get
            {
                if (null == _SmallDropChuckCommand) _SmallDropChuckCommand = new AsyncCommand(SmallDropChuckCommandFunc);
                return _SmallDropChuckCommand;
            }
        }
        private async Task SmallDropChuckCommandFunc()
        {
            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_SmallDropChuckCommand();

                    //});
                }


            }
        }
        #endregion

        #region ==> SmallRaiseChuckCommand
        private AsyncCommand _SmallRaiseChuckCommand;
        public IAsyncCommand SmallRaiseChuckCommand
        {
            get
            {
                if (null == _SmallRaiseChuckCommand) _SmallRaiseChuckCommand = new AsyncCommand(SmallRaiseChuckCommandFunc);
                return _SmallRaiseChuckCommand;
            }
        }
        private async Task SmallRaiseChuckCommandFunc()
        {


            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_SmallRaiseChuckCommand();

                    //});
                }


            }
        }
        #endregion
        #region ==> ZIFLockToggleCommand
        private AsyncCommand _ZIFLockToggleCommand;
        public IAsyncCommand ZIFLockToggleCommand
        {
            get
            {
                if (null == _ZIFLockToggleCommand) _ZIFLockToggleCommand = new AsyncCommand(ZIFLockToggleCommandFunc);
                return _ZIFLockToggleCommand;
            }
        }
        private async Task ZIFLockToggleCommandFunc()
        {


            if (_RemoteMediumProxy != null)
            {
                var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();
                if (stageBusy == false)
                {
                    await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                }
                else
                {
                    //await Task.Run(() =>
                    //{
                    await _RemoteMediumProxy.GPCC_OP_ZifToggleCommand();

                    //});
                }


            }
        }
        #endregion

        public void UpdateChanged()
        {
        }
        

        #region ==> PGVClearCommand
        private AsyncCommand _PGVClearCommand;
        public IAsyncCommand PGVClearCommand
        {
            get
            {
                if (null == _PGVClearCommand) _PGVClearCommand = new AsyncCommand(PGVClearCommandFunc);
                return _PGVClearCommand;
            }
        }
        private async Task PGVClearCommandFunc()
        {
            // CardChange 상태를 Clear 하여 IDLE 상태로 변경한다.
            ICardChangeSupervisor cardChangeSupervisor = this.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
            cardChangeSupervisor.Abort();
        }
        #endregion
        #region ==>MoveToCardDockPosCommand

        private AsyncCommand _MoveToCardDockPosCommand;
        public IAsyncCommand MoveToCardDockPosCommand
        {
            get
            {
                if (null == _MoveToCardDockPosCommand) _MoveToCardDockPosCommand = new AsyncCommand(MoveToCardDockPosCommandFunc);
                return _MoveToCardDockPosCommand;
            }
        }
        public async Task MoveToCardDockPosCommandFunc()
        {

            var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();

            var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Move to Dock Position",
                            "Do you want Move to Dock Position?",
                            EnumMessageStyle.AffirmativeAndNegative);

            if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
            {
                if (_RemoteMediumProxy != null)
                {
                    if (stageBusy == false)
                    {
                        await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_OP_CardAlignCommand();

                    }
                }
            }
        }

        #endregion

        #region ==>ForcedDropPod

        private AsyncCommand _ForcedDropPodCommand;
        public IAsyncCommand ForcedDropPodCommand
        {
            get
            {
                if (null == _ForcedDropPodCommand) _ForcedDropPodCommand = new AsyncCommand(ForcedDropPodCommandFunc);
                return _ForcedDropPodCommand;
            }
        }
        public async Task ForcedDropPodCommandFunc()
        {

            var stageBusy = LoaderMaster.GetClient(this._LoaderCommunicationManager.SelectedStage.Index).GetMovingState();

            var dialogRet = await (this).MetroDialogManager().ShowMessageDialog("Forced Card Pod Down",
                            "Please make sure there are no cards on the card pod.\n" +
                            "Are you sure you want to perform the [Drop Pod] action?",
                            EnumMessageStyle.AffirmativeAndNegative);

            if (dialogRet == EnumMessageDialogResult.AFFIRMATIVE)
            {
                if (_RemoteMediumProxy != null)
                {
                    if (stageBusy == false)
                    {
                        await (this).MetroDialogManager().ShowMessageDialog("Cell Busy", $"Cell{this._LoaderCommunicationManager.SelectedStageIndex} is Busy Right Now", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await _RemoteMediumProxy.GPCC_OP_ForcedDropPodCommand();

                    }
                }
            }
        }

        #endregion
    }
}
