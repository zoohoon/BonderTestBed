using Autofac;
using LoaderBase;
using LoaderBase.Communication;
using LoaderCore;
using LoaderMapView;
using LoaderParameters;
using LoaderParameters.Data;
using LoaderRecoveryControl;
using LogModule;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using RelayCommandBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MultiManualContactTransferDiaglog
{
    public class MultiManualContactTransferVM : INotifyPropertyChanged, ILoaderMapConvert, IFactoryModule, IForcedLoaderMapConvert
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private Autofac.IContainer LoaderContainer = null;
        private IGPLoader _GPLoader = null;

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
        private object _TransferSource;
        public object TransferSource
        {
            get { return _TransferSource; }
            set
            {
                if (value != _TransferSource)
                {
                    //if (value is true || value is null)
                    //{
                    //    TargetEnable = false;
                    //}
                    //else
                    //{
                    //    TargetEnable = true;
                    //}
                    _TransferSource = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        public ObservableCollection<FoupObject> Foups
        {
            get { return _Foups; }
            set
            {
                if (value != _Foups)
                {
                    _Foups = value;
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

                    _TransferTarget = value;
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
        private string _WaferOrigin;
        public string WaferOrigin
        {
            get { return _WaferOrigin; }
            set
            {
                if (value != _WaferOrigin)
                {

                    _WaferOrigin = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _WaferID;
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {

                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _CardID;
        public string CardID
        {
            get { return _CardID; }
            set
            {
                if (value != _CardID)
                {

                    _CardID = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {

                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumSubsStatus _CardStatus;
        public EnumSubsStatus CardStatus
        {
            get { return _CardStatus; }
            set
            {
                if (value != _CardStatus)
                {

                    _CardStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private FoupObject _SelectedFoup;
        public FoupObject SelectedFoup
        {
            get { return _SelectedFoup; }
            set
            {
                if (value != _SelectedFoup)
                {
                    if (value is null)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = false;
                        FoupUnloadEnable = false;
                    }
                    else if (value.State == FoupStateEnum.LOAD)
                    {
                        FoupScanEnable = true;
                        FoupLoadEnable = false;
                        FoupUnloadEnable = true;
                    }
                    else if (value.State == FoupStateEnum.UNLOAD)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = true;
                        FoupUnloadEnable = false;
                    }
                    else if (value.State == FoupStateEnum.ERROR)
                    {
                        FoupScanEnable = false;
                        FoupLoadEnable = true;
                        FoupUnloadEnable = true;
                    }

                    _SelectedFoup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FoupLoadEnable = false;
        public bool FoupLoadEnable
        {
            get { return _FoupLoadEnable; }
            set
            {
                if (value != _FoupLoadEnable)
                {
                    _FoupLoadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _FoupUnloadEnable = false;
        public bool FoupUnloadEnable
        {
            get { return _FoupUnloadEnable; }
            set
            {
                if (value != _FoupUnloadEnable)
                {
                    _FoupUnloadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _FoupScanEnable = false;
        public bool FoupScanEnable
        {
            get { return _FoupScanEnable; }
            set
            {
                if (value != _FoupScanEnable)
                {
                    _FoupScanEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsEnableWaferAlign = false;
        public bool IsEnableWaferAlign
        {
            get { return _IsEnableWaferAlign; }
            set
            {
                if (value != _IsEnableWaferAlign)
                {
                    _IsEnableWaferAlign = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsEnablePinAlign = false;
        public bool IsEnablePinAlign
        {
            get { return _IsEnablePinAlign; }
            set
            {
                if (value != _IsEnablePinAlign)
                {
                    _IsEnablePinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsCheckWaferAlign = false;
        public bool IsCheckWaferAlign
        {
            get { return _IsCheckWaferAlign; }
            set
            {
                if (value != _IsCheckWaferAlign)
                {
                    _IsCheckWaferAlign = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsCheckPinAlign = false;
        public bool IsCheckPinAlign
        {
            get { return _IsCheckPinAlign; }
            set
            {
                if (value != _IsCheckPinAlign)
                {
                    _IsCheckPinAlign = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsExistWafer;
        public bool IsExistWafer
        {
            get { return _IsExistWafer; }
            set
            {
                if (value != _IsExistWafer)
                {

                    _IsExistWafer = value;
                    if (value == true)
                    {
                        TransferSource = new StageObject(StageIndex);
                        TransferTarget = null;
                    }
                    else
                    {
                        TransferTarget = new StageObject(StageIndex);
                        TransferSource = null;
                    }
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
                    _GPLoader = container.Resolve<IGPLoader>();
                    _LoaderModule = container.Resolve<ILoaderModule>();
                    LoaderMaster = container.Resolve<ILoaderSupervisor>();
                    LoaderCommunicationManager = container.Resolve<ILoaderCommunicationManager>();
                    int foupCnt = _LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Count();

                    for (int i = 0; i < _LoaderModule.GetLoaderInfo().StateMap.CassetteModules.Count(); i++)
                    {
                        Foups.Add(new FoupObject(i));
                        Foups[i].Slots = new ObservableCollection<SlotObject>();
                        int slotCnt = _LoaderModule.GetLoaderInfo().StateMap.CassetteModules[i].SlotModules.Count();

                        for (int j = 0; j < slotCnt; j++)
                        {
                            Foups[i].Slots.Add(new SlotObject(slotCnt - j));
                        }
                    }

                    _LoaderModule.SetLoaderMapConvert(this);
                    _LoaderModule.BroadcastLoaderInfo();

                    var loaderMap = _LoaderModule.GetLoaderInfo().StateMap;
                    if (loaderMap.ChuckModules[idx - 1].WaferStatus == EnumSubsStatus.EXIST)
                    {
                        IsExistWafer = true;
                        TransferSource = new StageObject(StageIndex);
                        TransferTarget = null;
                    }
                    else
                    {
                        IsExistWafer = false;
                        TransferTarget = new StageObject(StageIndex);
                        TransferSource = null;


                    }
                    SetAllDisableSlot();


                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    wd = new MainWindow();
                    wd.DataContext = this;
                    wd.ShowDialog();

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

        private AsyncCommand _TransferSourceClickCommand;
        public ICommand TransferSourceClickCommand
        {
            get
            {
                if (null == _TransferSourceClickCommand) _TransferSourceClickCommand = new AsyncCommand(TransferSourceClickCommandFunc);
                return _TransferSourceClickCommand;
            }
        }

        private Task TransferSourceClickCommandFunc()
        {
            try
            {
                if (IsExistWafer)
                {
                    if (TransferTarget is bool)
                    {
                        TransferTarget = null;
                        SetAllDisableSlot();
                    }
                    else
                    {
                        TransferTarget = true;
                        SetDisableSlot();
                    }
                }
                else
                {
                    if (TransferSource is bool)
                    {
                        TransferSource = null;
                        SetAllDisableSlot();
                    }
                    else
                    {
                        TransferSource = true;
                        SetDisableSlot();
                    }

                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
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

                listView = (ListView)_param[0];
                index = (int)_param[1];
                if (index == -1)
                    return;
                listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(index);
                object item = _param[2];
                listViewItem.IsSelected = false;

                if (IsExistWafer)
                {
                    TransferTarget = item;
                    SetAllDisableSlot();
                }
                else
                {
                    TransferSource = item;
                    int FoupIdx = 0;
                    if (TransferSource is SlotObject)
                    {
                        var slot = TransferSource as SlotObject;
                        if (slot.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            if (slot.WaferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                            {
                                int slotNum = slot.WaferObj.OriginHolder.Index % 25;
                                int offset = 0;
                                if (slotNum == 0)
                                {
                                    slotNum = 25;
                                    offset = -1;
                                }
                                FoupIdx = ((slot.WaferObj.OriginHolder.Index + offset) / 25) + 1;
                            }
                        }

                    }
                    SetAllDisableSlot();

                }

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
                await this.MetroDialogManager().ShowWaitCancelDialogTarget(wd, this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    var loader = _LoaderModule;
                    Map = loader.GetLoaderInfo().StateMap;

                    string sourceId = null;
                    ModuleID targetId = new ModuleID();
                    object transfermodule = null;
                    //Source Info
                    SetTransferInfoToModule(TransferSource, ref transfermodule, true);
                    if (transfermodule == null)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Transfer Warning", $"Source Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    sourceId = (string)transfermodule;
                    //Target Info
                    SetTransferInfoToModule(TransferTarget, ref transfermodule, false);
                    if (transfermodule == null)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Transfer Warning", $"Destination Path is incorrect.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }
                    targetId = (ModuleID)transfermodule;

                    if (sourceId == null || targetId == null)
                        return;

                    TransferObject subObj = Map.GetTransferObjectAll().Where(item => item.ID.Value == sourceId).FirstOrDefault();
                    ModuleInfoBase dstLoc = Map.GetLocationModules().Where(item => item.ID == targetId).FirstOrDefault();

                    /* [ISSD-3418]
                   Origin : 원래 위치 
                   Source: 현재 위치
                   Target : 이동하려는 위치 
                   이렇게 세개의 위치 개념이 있는데, 여기서 

                   1) Origin 과 Source 가 다르고
                   2) Origin 과 Target 이 다른 경우 
                   Pop up을 띄워서 알려주도록 한다.

                   Foup에서 Cell 로 이동할때는 1)조건이 만족되지 않기에 ( Origin 과 Source 랑 같아서) popup 이 안뜰 것이고 
                   Origin 과 다른 위치에서 출발하는 모든 Object 들에 대해서 확인 가능하다.
                    */
                    if (((subObj.OriginHolder.ModuleType != subObj.CurrHolder.ModuleType) || (subObj.OriginHolder.Index != subObj.CurrHolder.Index)) &&
                        ((subObj.OriginHolder.ModuleType != dstLoc.ID.ModuleType) || (subObj.OriginHolder.Index != dstLoc.ID.Index)))
                    {
                        Func<ModuleID, (int, int)> GetSlotInfo = (ModuleID moduleID) =>
                        {
                            ICassetteModule cstModule = loader.ModuleManager.FindModule(ModuleTypeEnum.CST, 1) as ICassetteModule;
                            var slotCount = loader.ModuleManager.FindSlots(cstModule).Count();

                            var slot = moduleID.Index % slotCount;
                            int oldoffset = 0;
                            if (slot == 0)
                            {
                                slot = slotCount;
                                oldoffset = -1;
                            }
                            var foup = ((moduleID.Index + oldoffset) / slotCount) + 1;

                            return (foup, slot);
                        };

                        // Origin
                        string originPosition;
                        if (subObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            var (foup, slot) = GetSlotInfo(subObj.OriginHolder);
                            originPosition = $"FOUP : {foup}, SLOT : {slot}";
                        }
                        else
                        {
                            originPosition = $"{subObj.OriginHolder.ModuleType}, INDEX : {subObj.OriginHolder.Index}";
                        }

                        // Target
                        string targetPosition;
                        if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            var (foup, slot) = GetSlotInfo(dstLoc.ID);
                            targetPosition = $"FOUP : {foup}, SLOT : {slot}";
                        }
                        else
                        {
                            targetPosition = $"{dstLoc.ID.ModuleType}, INDEX : {dstLoc.ID.Index}";
                        }

                        var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Transfer Warning",
                               $"Wafer's Origin position and Target position are different. Do you want Continue? " +
                               $"\n[Origin Position] {originPosition}" +
                               $"\n[Target Position] {targetPosition}"
                               , EnumMessageStyle.AffirmativeAndNegative).Result;

                        if (retVal == EnumMessageDialogResult.NEGATIVE)
                        {
                            return;
                        }
                    }

                    if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK)
                    {
                        if (subObj.WaferType.Value == EnumWaferType.STANDARD)
                        {
                            _LoaderMaster.SetAngleInfo(dstLoc.ID.Index, subObj);
                            LoaderMaster.SetNotchType(dstLoc.ID.Index, subObj);
                        }
                    }
                    // Fixed Tray 또는 Inspection Tray 에서 Chuck으로 보내는 경우, Polish Wafer 설정이 되어 있을 때
                    if ((subObj.CurrPos.ModuleType == ModuleTypeEnum.FIXEDTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK) ||
                         (subObj.CurrPos.ModuleType == ModuleTypeEnum.INSPECTIONTRAY && dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK))
                    {
                        // DeviceManager로부터 Source의 데이터가 Polish Wafer로 설정되어 있는 Holder인지 알아온다.

                        DeviceManagerParameter DMParam = _LoaderMaster.DeviceManager().DeviceManagerParamerer_IParam as DeviceManagerParameter;
                        TransferObject deviceInfo = null;
                        WaferSupplyMappingInfo supplyInfo = null;

                        supplyInfo = DMParam.DeviceMappingInfos.FirstOrDefault(i => i.WaferSupplyInfo.ModuleType == subObj.CurrPos.ModuleType && i.WaferSupplyInfo.ID.Index == subObj.CurrPos.Index);

                        if (supplyInfo != null)
                        {
                            deviceInfo = supplyInfo.DeviceInfo;
                        }

                    }

                    bool canusearm = false;
                    for (int i = 0; i < Map.ARMModules.Count(); i++)
                    {
                        if (Map.ARMModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                        {
                            canusearm = true;
                            break;
                        }
                    }

                    if (canusearm == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Transfer Warning", $"There is already a wafer on the ARM.\n Please Check the ARM.", EnumMessageStyle.Affirmative).Result;
                        return;
                    }


                    bool canusePA = false;
                    for (int i = 0; i < Map.PreAlignModules.Count(); i++)
                    {
                        if (Map.PreAlignModules[i].WaferStatus == EnumSubsStatus.NOT_EXIST)
                        {
                            canusePA = true;
                            break;
                        }
                    }

                    if (canusePA == false)
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Transfer Warning", $"There is already a wafer on the PreAlign.\n Please Check the PreAlign.", EnumMessageStyle.Affirmative).Result; ;
                        return;
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
                        }
                        else if (ccLoad)
                        {
                            stageidx = (TransferTarget as StageObject).Index;
                        }

                        // CC Temp - 추후 파라미터 처리 필요 
                        //if (stageidx != -1)
                        //{
                        //    var tempClient = loaderCommunicationManager.GetTempControllerClient(stageidx);
                        //    if (tempClient != null)
                        //    {
                        //        //stage 온도 확인 : 저온이면 Ambiment 온도로 올린뒤 다시 동작하도록 한다.
                        //        var temp = tempClient.GetTemperature();
                        //        if (temp < 0)
                        //        {
                        //            var msgRet = await this.MetroDialogManager().ShowMessageDialogTarget("Error Message",
                        //                $"Card Change operation is possible only at room temperature." +
                        //                $" A temperature of the selected stage is {temp}℃." +
                        //                $"Press OK to automatically set the temperature to a safe temperature and you can try again when it reaches normal temperature." +
                        //                $"Or direct fixes can be made in the Device Temperature set." +
                        //                $" After the card change, you must change the temperature to its original temperature.",
                        //                EnumMessageStyle.AffirmativeAndNegative);
                        //            if(msgRet == EnumMessageDialogResult.AFFIRMATIVE)
                        //            {
                        //                tempClient.SetAmbientTemp();
                        //            }
                        //            return;
                        //        }
                        //    }
                        //}
                    }
                    //=========================================================


                    SetTransfer(sourceId, targetId);

                    var mapSlicer = new LoaderMapSlicer();
                    var slicedMap = mapSlicer.ManualSlicing(Map);

                    if (slicedMap != null)
                    {
                        bool isError = false;

                        for (int i = 0; i < slicedMap.Count; i++)
                        {
                            _LoaderModule.SetRequest(slicedMap[i]);

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
                                    LoaderRecoveryControlVM.Show(Container, loader.ResonOfError, loader.ErrorDetails);
                                    loader.ResonOfError = "";
                                    loader.ErrorDetails = "";
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
                    }
                    else
                    {
                        var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Transfer Warning", $"This is an incorrect operation.\n Please check again.", EnumMessageStyle.Affirmative).Result;
                    }
                    var loaderMap = _LoaderModule.GetLoaderInfo().StateMap;
                    if (loaderMap.ChuckModules[StageIndex - 1].WaferStatus == EnumSubsStatus.EXIST)
                    {
                        if (IsCheckPinAlign)
                        {
                            // 사용 안됨.
                            _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().DO_ManualPinAlign();
                        }

                        if (IsCheckWaferAlign)
                        {
                            LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>(StageIndex).DoWaferAlign();
                        }
                        IsExistWafer = true;
                        TransferSource = new StageObject(StageIndex);
                        TransferTarget = null;

                    }
                    else
                    {
                        IsExistWafer = false;
                        TransferTarget = new StageObject(StageIndex);
                        TransferSource = null;
                    }
                    SetAllDisableSlot();
                });
                task.Start();
                await task;

                //  UpdateCSTBtnEnable();
                //  UpdateCSTBtnEnable();
                //TransferTarget = null;
                //   isClickedTransferTarget = false;
                //   SetAllObjectEnable();
                //    IsSelected = false;
                //TransferSource = null;
                // isClickedTransferSource = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaogTarget(wd, this.GetHashCode().ToString());
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
                if (subObj.CurrPos.ModuleType == ModuleTypeEnum.CHUCK && stageBusy == false)
                {
                    bool needtorecovery = false;
                    ModuleStateEnum wafertransferstate = ModuleStateEnum.ABORT;
                    var ret = LoaderMaster.GetClient(subObj.CurrPos.Index).CanWaferUnloadRecovery(ref needtorecovery,
                        ref wafertransferstate);
                    if (ret == EventCodeEnum.NONE)
                    {
                        if (needtorecovery && wafertransferstate == ModuleStateEnum.ERROR)
                        {
                            stageBusy = true;
                        }
                    }
                }
            }
            else if (dstLoc.ID.ModuleType == ModuleTypeEnum.CHUCK ||
                dstLoc.ID.ModuleType == ModuleTypeEnum.CC)
            {
                LoaderServiceBase.ILoaderServiceCallback callback = LoaderMaster.GetClient(dstLoc.ID.Index);

                if (callback != null)
                {
                    stageBusy = callback.GetRunState();
                }
                else
                {
                    LoggerManager.Debug($"[LoaderHandlingViewModel], SetTransfer() : COM ERROR");
                    stageBusy = false;
                }
            }

            if (stageBusy == false)
            {
                var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Cell Busy", $"Cell{subObj.CurrPos.Index} is Busy Right Now", EnumMessageStyle.Affirmative).Result;

                if (retVal == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    return;
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
                    if (LoaderMaster.ClientList.ContainsKey(dstLoc.ID.ToString()))
                    {
                        var deviceInfo = LoaderMaster.GetDeviceInfoClient(LoaderMaster.ClientList[dstLoc.ID.ToString()]);
                        if (deviceInfo != null)
                        {
                            subObj.NotchAngle.Value = deviceInfo.NotchAngle.Value;
                        }
                    }
                    else
                    {

                    }
                }
                else if (dstLoc.ID.ModuleType == ModuleTypeEnum.SLOT)
                {
                    subObj.NotchAngle.Value = (this.LoaderModule.ModuleManager.FindModule(dstLoc.ID) as ISlotModule).Cassette.Device.LoadingNotchAngle.Value;
                }

                currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                currHolder.Substrate = null;
                dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                dstHolder.Substrate = subObj;
            }
            else
            {
                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = destinationID;
            }
        }

        private void SetTransferInfoToModule(object param, ref object Id, bool issource)
        {
            try
            {
                var loader = _LoaderModule;
                Map = loader.GetLoaderInfo().StateMap;

                if (param is StageObject)
                {

                    //Move Wafer

                    int chuckindex = Convert.ToInt32(((param as StageObject).Name.Split('#'))[1]);
                    HolderModuleInfo chuckModule = null;
                    if (issource)
                        chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate != null && i.ID.Index == chuckindex);
                    else
                        chuckModule = Map.ChuckModules.FirstOrDefault(i => i.Substrate == null && i.ID.Index == chuckindex);

                    if (chuckModule == null)
                    {
                        Id = null;
                        return;
                    }
                    if (issource)
                        Id = chuckModule.Substrate.ID.Value;
                    else
                        Id = chuckModule.ID;


                }
                else if (param is SlotObject)
                {
                    int slotindex = Convert.ToInt32(((param as SlotObject).Name.Split('#'))[1]);
                    var waferIdx = (((param as SlotObject).FoupNumber) * 25) + slotindex;
                    if (Map.CassetteModules[(param as SlotObject).FoupNumber].FoupState == FoupStateEnum.LOAD && Map.CassetteModules[(param as SlotObject).FoupNumber].ScanState == CassetteScanStateEnum.READ)
                    {
                        var slotModule = Map.CassetteModules[(param as SlotObject).FoupNumber].SlotModules.FirstOrDefault(i => i.ID.Index == waferIdx);
                        if (slotModule == null)
                            return;
                        if (issource)
                            Id = slotModule.Substrate.ID.Value;
                        else
                            Id = slotModule.ID;
                    }
                    else
                    {
                        Id = null;
                        return;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetAllDisableSlot()
        {
            foreach (var foup in Foups)
            {
                foreach (var slot in foup.Slots)
                {

                    slot.IsEnableTransfer = false;

                }
            }
        }
        private void SetAllEnableSlot()
        {
            foreach (var foup in Foups)
            {
                foreach (var slot in foup.Slots)
                {
                    slot.IsEnableTransfer = true;
                }
            }
        }
        private void SetDisableSlot()
        {
            var loaderMap = LoaderModule.GetLoaderInfo().StateMap;

            foreach (var foup in Foups)
            {
                foreach (var slot in foup.Slots)
                {
                    if (foup.State == ProberInterfaces.Foup.FoupStateEnum.LOAD && foup.ScanState == CassetteScanStateEnum.READ)
                    {
                        if (IsExistWafer)
                        {
                            if (slot.WaferStatus == EnumSubsStatus.NOT_EXIST)
                            {
                                slot.IsEnableTransfer = true;
                            }
                            else
                            {
                                slot.IsEnableTransfer = false;
                            }

                        }
                        else
                        {
                            if (slot.WaferStatus == EnumSubsStatus.EXIST)
                            {
                                slot.IsEnableTransfer = true;
                            }
                            else
                            {
                                slot.IsEnableTransfer = false;
                            }
                        }
                    }
                    else
                    {
                        slot.IsEnableTransfer = false;
                    }

                }
            }
        }

        public void UpdateChanged()
        {
            try
            {
                //  UpdateCSTBtnEnable();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"UpdateChanged(): Exception occurred. Err = {err.Message}");
            }
        }
        public void SetWaferID(string id)
        {
            if (WaferStatus != EnumSubsStatus.EXIST)
            {
                WaferID = WaferStatus.ToString();
            }
            else
            {
                WaferID = id;
            }
        }
        public void SetProbeCardID(string id)
        {
            if (CardStatus != EnumSubsStatus.EXIST)
            {
                CardID = CardStatus.ToString();
            }
            else
            {
                CardID = id;
            }
        }
        public void SetWaferOrigin(ModuleID id)
        {
            if (id == null || id.ModuleType == ModuleTypeEnum.UNDEFINED || id.ModuleType == ModuleTypeEnum.INVALID)
            {
                WaferOrigin = "";
            }
            else
            {
                if (id.ModuleType == ModuleTypeEnum.SLOT)
                {
                    int slotNum = id.Index % 25;
                    int offset = 0;
                    if (slotNum == 0)
                    {
                        slotNum = 25;
                        offset = -1;
                    }
                    int foupNum = ((id.Index + offset) / 25) + 1;
                    WaferOrigin = $"Foup#{foupNum } - SLOT#{slotNum}";
                }
                else
                {
                    WaferOrigin = id.ToString();
                }
            }
        }


        public Task LoaderMapConvert(LoaderMap map)
        {
            // TODO : 고쳐야 될 것 같은데.. 난감하네..
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < map.ChuckModules.Count(); i++)
                {
                    //int idx = 0;
                    //if (i < 4)
                    //{
                    //    idx = i + 8;
                    //}
                    //else if (i < 8)
                    //{
                    //    idx = i;
                    //}
                    //else
                    //{
                    //    idx = i - 8;
                    //}

                    //cell.Name = map.ChuckModules[i].ID.ToString();
                    if (map.CCModules[i].Substrate != null)
                    {

                        LoaderCommunicationManager.Cells[i].CardStatus = map.CCModules[i].WaferStatus;
                        LoaderCommunicationManager.Cells[i].Progress = map.CCModules[i].Substrate.OriginHolder.Index;
                        LoaderCommunicationManager.Cells[i].CardObj = map.CCModules[i].Substrate;
                        if (map.CCModules[i].ID.Index == StageIndex)
                        {
                            CardStatus = map.CCModules[i].WaferStatus;
                            SetProbeCardID(map.CCModules[i].Substrate.ProbeCardID.Value);

                        }
                    }
                    else
                    {
                        LoaderCommunicationManager.Cells[i].CardStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                        if (map.CCModules[i].ID.Index == StageIndex)
                        {
                            CardStatus = map.CCModules[i].WaferStatus;
                            SetProbeCardID("");
                        }
                    }

                    if (map.ChuckModules[i].Substrate != null)
                    {
                        if (map.ChuckModules[i].ID.Index == StageIndex)
                        {
                            WaferStatus = map.ChuckModules[i].WaferStatus;
                            SetWaferID(map.ChuckModules[i].Substrate.OCR.Value);
                            SetWaferOrigin(map.ChuckModules[i].Substrate.OriginHolder);
                            IsEnableWaferAlign = false;
                            IsEnablePinAlign = false;
                        }
                        LoaderCommunicationManager.Cells[i].WaferStatus = map.ChuckModules[i].WaferStatus;
                        LoaderCommunicationManager.Cells[i].WaferObj = map.ChuckModules[i].Substrate;

                    }
                    else
                    {
                        if (map.ChuckModules[i].ID.Index == StageIndex)
                        {
                            WaferStatus = map.ChuckModules[i].WaferStatus;
                            SetWaferID("");
                            ModuleID id = new ModuleID(ModuleTypeEnum.UNDEFINED, 0, "");
                            SetWaferOrigin(id);
                            IsEnableWaferAlign = true;
                            if (CardStatus == EnumSubsStatus.EXIST)
                            {
                                IsEnablePinAlign = true;
                            }
                        }
                        LoaderCommunicationManager.Cells[i].WaferStatus = map.ChuckModules[i].WaferStatus;
                        LoaderCommunicationManager.Cells[i].WaferObj = null;

                    }




                }


                for (int i = 0; i < map.CassetteModules.Count(); i++)
                {

                    Foups[i].State = map.CassetteModules[i].FoupState;
                    Foups[i].ScanState = map.CassetteModules[i].ScanState;
                    Foups[i].CassetteType = LoaderModule.Foups[i].CassetteType;

                    for (int j = 0; j < map.CassetteModules[i].SlotModules.Count(); j++)
                    {
                        //var slot = new SlotObject(map.CassetteModules[i].SlotModules.Count() - j);
                        if (map.CassetteModules[i].SlotModules[j].Substrate != null)
                        {
                            if (LoaderMaster.ActiveLotInfos[i].State == LotStateEnum.Running) //foupNumber가 같을때
                            {
                                if (!(LoaderMaster.ActiveLotInfos[i].UsingSlotList.Where(idx => idx == Foups[i].Slots[j].Index).FirstOrDefault() > 0))
                                {
                                    Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                    LoaderModule.Foups[i].Slots[j].WaferState = EnumWaferState.SKIPPED;
                                }
                                else
                                {
                                    Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                    LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                }

                                if (LoaderMaster.ActiveLotInfos[i].UsingPMIList.Where(idx => idx == Foups[i].Slots[j].Index).FirstOrDefault() > 0)
                                {
                                    if (LoaderModule.Foups[i].Slots[j].WaferObj != null)
                                    {
                                        //LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMITriggerEnum.WAFER_INTERVAL;
                                        LoaderModule.Foups[i].Slots[j].WaferObj.PMITirgger = ProberInterfaces.Enum.PMIRemoteTriggerEnum.UNDIFINED;
                                        LoaderModule.Foups[i].Slots[j].WaferObj.DoPMIFlag = true;
                                    }
                                }
                            }
                            else
                            {
                                Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                                LoaderModule.Foups[i].Slots[j].WaferState = map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
                            }
                            Foups[i].Slots[j].WaferStatus = map.CassetteModules[i].SlotModules[j].WaferStatus;
                            Foups[i].Slots[j].WaferObj = map.CassetteModules[i].SlotModules[j].Substrate;
                        }
                        else if (map.CassetteModules[i].SlotModules[j].WaferStatus == EnumSubsStatus.UNDEFINED)
                        {
                            Foups[i].Slots[j].WaferStatus = EnumSubsStatus.NOT_EXIST;
                        }
                        else
                        {
                            Foups[i].Slots[j].WaferStatus = map.CassetteModules[i].SlotModules[j].WaferStatus;
                        }
                        Foups[i].Slots[j].FoupNumber = i;
                        //foup.Slots.Add(slot);
                    }
                }

            }));

            return Task.CompletedTask;
        }


        private AsyncCommand _CSTLoadCommand;
        public ICommand CSTLoadCommand
        {
            get
            {
                if (null == _CSTLoadCommand) _CSTLoadCommand = new AsyncCommand(CSTLoadFunc);
                return _CSTLoadCommand;
            }
        }
        private async Task CSTLoadFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialogTarget(wd, this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    try
                    {
                        if (SelectedFoup == null)
                            return;
                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
                        var modules = this.LoaderModule.ModuleManager;
                        var Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        //  this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
                        LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1].FoupLoad();
                        UpdateCSTBtnEnable();
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaogTarget(wd, this.GetHashCode().ToString());
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        if (SelectedFoup == null)
            //            return;
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
            //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupLoadCommand());
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});
        }

        private AsyncCommand _CSTUnloadCommand;
        public ICommand CSTUnloadCommand
        {
            get
            {
                if (null == _CSTUnloadCommand) _CSTUnloadCommand = new AsyncCommand(CSTUnloadFunc);
                return _CSTUnloadCommand;
            }
        }

        private async Task CSTUnloadFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialogTarget(wd, this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    if (SelectedFoup == null)
                        return;
                    try
                    {
                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
                        var modules = this.LoaderModule.ModuleManager;
                        var Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
                        // this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
                        LoaderMaster.ActiveLotInfos[Cassette.ID.Index - 1].FoupUnLoad();
                        UpdateCSTBtnEnable();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaogTarget(wd, this.GetHashCode().ToString());
            }

            //return Task.Run(() =>
            //{
            //    if (SelectedFoup == null)
            //        return;
            //    try
            //    {
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);
            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        //this.LoaderModule.GetLoaderCommands().CassetteLoad(Cassette);
            //        this.FoupOpModule().FoupControllers[Cassette.ID.Index - 1].Execute(new FoupUnloadCommand());
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});
        }
        public void UpdateCSTBtnEnable()
        {
            Thread.Sleep(100);
            if (SelectedFoup is null)
            {
                FoupScanEnable = false;
                FoupLoadEnable = false;
                FoupUnloadEnable = false;
            }
            else if (SelectedFoup.State == FoupStateEnum.LOAD)
            {
                FoupScanEnable = true;
                FoupLoadEnable = false;
                FoupUnloadEnable = true;
            }
            else if (SelectedFoup.State == FoupStateEnum.UNLOAD)
            {
                FoupScanEnable = false;
                FoupLoadEnable = true;
                FoupUnloadEnable = false;
            }
            else if (SelectedFoup.State == FoupStateEnum.ERROR)
            {
                FoupScanEnable = false;
                FoupLoadEnable = true;
                FoupUnloadEnable = true;
            }
        }

        private AsyncCommand _CSTScanCommand;
        public ICommand CSTScanCommand
        {
            get
            {
                if (null == _CSTScanCommand) _CSTScanCommand = new AsyncCommand(CSTScanFunc);
                return _CSTScanCommand;
            }
        }

        public void AllCstBtnDisable()
        {
            FoupScanEnable = false;
            FoupLoadEnable = false;
            FoupUnloadEnable = false;
        }
        private async Task CSTScanFunc()
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialogTarget(wd, this.GetHashCode().ToString(), "Wait");

                Task task = new Task(() =>
                {
                    try
                    {
                        if (SelectedFoup == null)
                            return;
                        AllCstBtnDisable();
                        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

                        var modules = this.LoaderModule.ModuleManager;
                        var Cassette = this.LoaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
                        LoaderModule.ScanCount = 25;

                        Cassette.SetNoReadScanState();
                        bool scanWaitFlag = false;

                        if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE)
                        {
                            Cassette.Device.AllocateDeviceInfo.OCRDevParam = new OCRDevParameter();
                            var retVal = LoaderModule.DoScanJob(foupindex);
                            if (retVal.Result == EventCodeEnum.NONE)
                            {
                                scanWaitFlag = true;
                            }
                        }
                        while (scanWaitFlag)
                        {
                            if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL)
                            {
                                var retVal = this.MetroDialogManager().ShowMessageDialogTarget(wd, "Error", "Illegal ScanState", EnumMessageStyle.Affirmative).Result;
                                break;
                            }
                            else if (Cassette.ScanState == CassetteScanStateEnum.READ)
                            {
                                break;
                            }
                            Thread.Sleep(10);
                        }
                        UpdateCSTBtnEnable();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaogTarget(wd, this.GetHashCode().ToString());
            }

            //return Task.Run(() =>
            //{
            //    try
            //    {
            //        if (SelectedFoup == null)
            //            return;
            //        AllCstBtnDisable();
            //        int foupindex = Convert.ToInt32((SelectedFoup.Name.Split('#'))[1]);

            //        var modules = this.loaderModule.ModuleManager;
            //        var Cassette = this.loaderModule.ModuleManager.FindModule<ICassetteModule>(ModuleTypeEnum.CST, foupindex);
            //        loaderModule.ScanCount = 25;

            //        Cassette.SetNoReadScanState();
            //        bool scanWaitFlag = false;

            //        if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.NONE)
            //        {
            //            var retVal = loaderModule.DoScanJob(foupindex);
            //            if (retVal.Result == EventCodeEnum.NONE)
            //            {
            //                scanWaitFlag = true;
            //            }
            //        }
            //        while (scanWaitFlag)
            //        {

            //            if (Cassette.ScanState == CassetteScanStateEnum.ILLEGAL || Cassette.ScanState == CassetteScanStateEnum.READ)
            //            {
            //                break;
            //            }
            //            Thread.Sleep(10);
            //        }
            //        UpdateCSTBtnEnable();
            //    }
            //    catch (Exception err)
            //    {
            //        LoggerManager.Error($"PAPickMethod(): Exception occurred. Err = {err.Message}");
            //    }
            //});

        }

    }
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is StageObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Cell);
            }
            else if (value is ArmObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Arm);
            }
            else if (value is SlotObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.FOUP);
            }
            else if (value is PAObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.PA);
            }
            else if (value is BufferObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Buffer);
            }
            else if (value is CardBufferObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.CardHand);
            }
            else if (value is CardTrayObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.CardTray);
            }
            else if (value is CardArmObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.CardHand);
            }
            else if (value is FixedTrayInfoObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Buffer);
            }
            else if (value is InspectionTrayInfoObject)
            {
                return SetIconSoruceBitmap(Properties.Resources.Buffer);
            }
            else if (value is bool)
            {
                return SetIconSoruceBitmap(Properties.Resources.selecteicon);
            }

            return null;
        }
        public BitmapImage SetIconSoruceBitmap(Bitmap bitmap)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                Application.Current.Dispatcher.Invoke(delegate
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        image.BeginInit();
                        ms.Seek(0, SeekOrigin.Begin);
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        image.StreamSource = null;
                    }
                });
                return image;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TransferObjectLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StageObject)
            {
                return (value as StageObject).Name;
            }
            else if (value is ArmObject)
            {
                return (value as ArmObject).Name;
            }
            else if (value is SlotObject)
            {
                string str = "";
                str = $"Foup#{(value as SlotObject).FoupNumber + 1 } - {(value as SlotObject).Name}";
                return str;
            }
            else if (value is FixedTrayInfoObject)
            {
                return (value as FixedTrayInfoObject).Name;
            }
            else if (value is PAObject)
            {
                return (value as PAObject).Name;
            }
            else if (value is BufferObject)
            {
                return (value as BufferObject).Name;
            }
            else if (value is CardBufferObject)
            {
                return (value as CardBufferObject).Name;
            }
            else if (value is CardTrayObject)
            {
                return (value as CardTrayObject).Name;
            }
            else if (value is CardArmObject)
            {
                return (value as CardArmObject).Name;
            }
            else if (value is InspectionTrayInfoObject)
            {
                return (value as InspectionTrayInfoObject).Name;
            }
            else if (value is bool)
            {
                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ListViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
