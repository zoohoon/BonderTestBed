using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

using Autofac;
using ProberInterfaces;
using LoaderBase;
using LoaderParameters;
using ProberErrorCode;
using LoaderServiceBase;
using LogModule;
using System.Reflection;
using LoaderBase.Communication;
using ProberInterfaces.LoaderController;
using LoaderBase.LoaderLog;
using ProberInterfaces.CardChange;
using MetroDialogInterfaces;
using System.Diagnostics;
using LoaderBase.LoaderResultMapUpDown;
using ProberInterfaces.ODTP;
using ProberInterfaces.Param;
using ProberInterfaces.Monitoring;
using ProberInterfaces.Enum;

namespace LoaderCore.GPLoaderService
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class GPWCFLoaderService : IGPLoaderService, IFactoryModule
    {
        public Autofac.IContainer cont;
        private ILoaderSupervisor LoaderMaster => cont.Resolve<ILoaderSupervisor>();
        private ILoaderCommunicationManager _LoaderCommunicationManager => cont.Resolve<ILoaderCommunicationManager>();
        private ILoaderLogManagerModule LoaderLogmanager => cont.Resolve<ILoaderLogManagerModule>();
        private ILoaderModule LoaderModule => cont.Resolve<ILoaderModule>();
        private ILoaderResultMapUpDownMng LoaderResultMapUpDownMng => cont.Resolve<ILoaderResultMapUpDownMng>();

        private ILoaderODTPManager LoaderODTPManager => cont.Resolve<ILoaderODTPManager>();
        //private Guid _LoaderStageSummaryVMGuid { get; set; } = new Guid("6e199680-a422-4882-841d-cd4628a8c009");

        public EventCodeEnum AbortRequest()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum AwakeProcessModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.AwakeProcessModule();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum AbortProcessModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.AbortProcessModule();
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum ClearRequestData()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method executed");
            return EventCodeEnum.NONE;
        }

        public void IsServiceAvailable()
        {
            //return true;
        }

        public EventCodeEnum Connect(string chuckID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                OperationContext.Current.Channel.Faulted += Channel_Faulted;
                var loaderController = OperationContext.Current.GetCallbackChannel<ILoaderServiceCallback>();
                int chuckIdx = -1;
                if (chuckID.Length >= 5)
                {
                    int.TryParse(chuckID.Substring(5), out chuckIdx);
                }
                retVal = LoaderMaster.Connect(chuckID, loaderController);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Connect() - Failed GPLService Host Callback Channel Connect (Exception : {err}");
            }
            return retVal;
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (LoaderMaster.ClientList.ContainsValue((ILoaderServiceCallback)sender))
                {
                    foreach (var keyvaluepair in LoaderMaster.ClientList)
                    {
                        if (ReferenceEquals(keyvaluepair.Value, (ILoaderServiceCallback)sender))
                        {
                            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
                            if ((keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Faulted || (keyvaluepair.Value as ICommunicationObject).State == CommunicationState.Closed)
                            {
                                LoggerManager.Debug($"GPLService Host Callback Channel faulted. Sender = {sender}, cell index = {keyvaluepair.Key}");
                                //LoaderMaster.ClientList에 포함된 callback 객체는 임의로 삭제 하지 않는다.. reconnect 로직에서 해당 map에 있으나 연결이 끊어진 경우 reconnect 대상으로 보고 있다.
                                //Clinetlist에서의 삭제는 명시적인 disconnect에서만 제거 하도록 한다.
                            }
                            else
                            {
                                LoggerManager.Debug($"Ignore GPLService Host Callback Channel faulted. Sender = {sender}, Already Reconnected");
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Disconnect(int chuckID = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var client = LoaderMaster.GetClient(chuckID);
                if (client != null)
                {
                    LoaderMaster.DisConnectClient(client);
                    LoaderMaster.RemoveClientAtList(chuckID);
                    LoggerManager.Debug($"GPWCFLoaderService Host Callback Channel close. cell index = {chuckID}");
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Debug($"GPWCFLoaderService Host Callback Channel client #[{chuckID}] Already Close.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //ClientList
            //LoaderMaster.Disconnect();
            return retVal;
        }

        public EventCodeEnum WTR_NotifyTransferObject(TransferObject transferobj)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderMaster.Loader.WaferTransferRemoteService.NotifyTransferObject(transferobj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InquireJob()
        {
            return EventCodeEnum.NONE;
        }
        public LoaderDeviceParameter GetDeviceParam()
        {
            LoaderDeviceParameter retVal = null;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderInfo GetLoaderInfo()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            LoaderInfo retVal = null;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderSystemParameter GetSystemParam()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            LoaderSystemParameter retVal = null;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Initialize(string rootParamPath)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        public EventCodeEnum RECOVERY_MotionInit()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum RECOVERY_ResetWaferLocation()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum RetractAll()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.SaveDeviceParam(deviceParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.SaveSystemParam(systemParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        public ResponseResult SetRequest(LoaderMap dstMap)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            ResponseResult retVal = new ResponseResult();

            try
            {
                //retVal = Loader.SetRequest(dstMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum WTR_ChuckDownMove(int option = 0)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.ChuckDownMove(option);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_ChuckUpMove(int option = 0)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.ChuckUpMove(option);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WTR_Wafer_MoveLoadingPosition()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.Wafer_MoveLoadingPosition();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WTR_MonitorForARMVacuum(bool value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.MonitorForVacuum(value);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum GetWaferLoadObject(out TransferObject loadedObject)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadedObject = new TransferObject();
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.GetWaferLoadObject(out loadedObject);
                //retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadedObject = new TransferObject();
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.NotifyLoadedToThreeLeg(out loadedObject);
                //retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange = true)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.NotifyUnloadedFromThreeLeg(waferState, cellIdx, isWaferStateChange);
                // retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.NotifyWaferTransferResult(isSucceed);
                //retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_PickUpMove()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.PickUpMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_PlaceDownMove()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.PlaceDownMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_RetractARM()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.RetractARM();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SafePosW()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SafePosW();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SelfRecoveryRetractARM()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SelfRecoveryRetractARM();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SelfRecoveryTransferToPreAlign()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SelfRecoveryTransferToPreAlign();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);
                // retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.Notifyhandlerholdwafer(ishandlerhold);
                // retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_WaitForARMVacuum(bool value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.WaitForVacuum(value);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_WriteARMVacuum(bool value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.WriteVacuum(value);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public SubstrateSizeEnum WTR_GetTransferWaferSize()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            SubstrateSizeEnum retVal = SubstrateSizeEnum.UNDEFINED;
            try
            {
                retVal = LoaderMaster.Loader.WaferTransferRemoteService.GetTransferWaferSize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderServiceTypeEnum GetServiceType()
        {
            return LoaderServiceTypeEnum.WCF;
        }

        public void SetContainer(Autofac.IContainer container)
        {
            cont = container;
        }


        public EventCodeEnum CTR_NotifyCardTransferResult(bool isSucceed)
        {
            return LoaderMaster.Loader.CardTransferRemoteService.NotifyCardTransferResult(isSucceed);
        }
        public EventCodeEnum CTR_NotifyCardDocking()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.NotifyCardDocking();
        }
        public EventCodeEnum CTR_CardChangePick()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.CardChangePick();
        }

        public EventCodeEnum CTR_CardChangePut(out TransferObject transObj)
        {
            return LoaderMaster.Loader.CardTransferRemoteService.CardChangePut(out transObj);
        }
        public EventCodeEnum CTR_SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState)
        {
            return LoaderMaster.Loader.CardTransferRemoteService.SetTransferAfterCardChangePutError(out transObj,  waferState);
        }
        public EventCodeEnum CTR_CardChangeCarrierPick()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.CardChangeCarrierPick();
        }
        public EventCodeEnum CTR_OriginCarrierPut()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.OriginCarrierPut();
        }
        public EventCodeEnum CTR_OriginCardPut()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.OriginCardPut();
        }
        public EventCodeEnum CTR_CardChangeCarrierPut()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.CardChangeCarrierPut();
        }
        public EventCodeEnum CTR_OriginCarrierPick()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.OriginCarrierPick();
        }
        public EventCodeEnum CTR_Card_MoveLoadingPosition()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.Card_MoveLoadingPosition();
        }
        public EventCodeEnum CTR_CardTransferDone(bool isSucceed)
        {
            return LoaderMaster.Loader.CardTransferRemoteService.CardTransferDone(isSucceed);
        }
        public void SetStageState(int cellIdx, ModuleStateEnum StageState, bool isBuzzerOn)
        {
            LoaderMaster.SetStageState(cellIdx, StageState, isBuzzerOn);
        }

        public EventCodeEnum ResponseSystemInit(EventCodeEnum errorCode)
        {
            return LoaderMaster.ResponseSystemInit(errorCode);
        }

        public EventCodeEnum ResponseCardRecovery(EventCodeEnum errorCode)
        {
            return LoaderMaster.ResponseCardRecovery(errorCode);
        }

        public string CTR_GetProbeCardID()
        {
            return LoaderMaster.Loader.CardTransferRemoteService.GetProbeCardID();
        }

        public EventCodeEnum CTR_UserCardIDInput(out string UserCardIDInput) 
        {
            return LoaderMaster.Loader.CardTransferRemoteService.GetUserCardIDInput(out UserCardIDInput);
        }

        public string CTR_GetProbeCardIDLastTwoWord()
        {
            return LoaderMaster.CardIDLastTwoWord;
        }

        public EventCodeEnum NotifyStageSystemError(int cellindex)
        {
            return LoaderMaster.NotifyStageSystemError(cellindex);
        }
        public EventCodeEnum NotifyClearStageSystemError(int cellindex)
        {
            return LoaderMaster.NotifyClearStageSystemError(cellindex);
        }


        public void NotifyReasonOfError(string errmsg)
        {
            LoaderMaster.Loader.ResonOfError = errmsg;
        }



        public void SetTitleMessage(int cellno, string message, string foreground = "", string background = "")
        {
            LoaderMaster.SetTitleMessage(cellno, message, foreground, background);
        }
        object lockObj = new object();
        public void SetActionLogMessage(string message, int idx, ModuleLogType ModuleType, StateLogType State)
        {
            lock (lockObj)
            {
                LoaderMaster.SetActionLogMessage(message, idx, ModuleType, State);
            }
        }
        public void SetParamLogMessage(string message, int idx)
        {
            lock (lockObj)
            {
                LoaderMaster.SetParamLogMessage(message, idx);
            }
        }

        public void SetDeviceName(int cellno, string deviceName)
        {
            _LoaderCommunicationManager.SetDeviceName(cellno, deviceName);
        }
        public void SetDeviceLoadResult(int cellno, bool result)
        {
            _LoaderCommunicationManager.SetDeviceLoadResult(cellno, result);
        }
        public byte[] GetBytesFoupObjects()
        {
            byte[] retval = null;

            try
            {
                retval = _LoaderCommunicationManager.GetBytesFoupObjects();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum SetRecoveryMode(int cellIdx,bool isRecovery)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                _LoaderCommunicationManager.SetRecoveryMode(cellIdx, isRecovery);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetCardState(int index, EnumWaferState CardState)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var CCModules = this.LoaderMaster.Loader.ModuleManager.FindModules<ICCModule>();
                var CCModule = CCModules.FirstOrDefault(i => i.ID.Index == index);
                if (CCModule.Holder.TransferObject != null)
                {
                    CCModule.Holder.TransferObject.WaferState = CardState;
                    this.LoaderMaster.Loader.BroadcastLoaderInfo();
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void UpdateSoakingInfo(SoakingInfo soakinfo)
        {
            // TODO : 각 셀별 데이터로 변경. 
            // ChuckIndex에 적절한 데이터를 사용하는지 TEST 필요 

            try
            {
                var stage = _LoaderCommunicationManager.GetStage(soakinfo.ChuckIndex);

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        if (stage.StageInfo.LotData != null)
                        {
                            stage.StageInfo.LotData.SoakingType = soakinfo.SoakingType;
                            stage.StageInfo.LotData.SoakingZClearance = soakinfo.ZClearance.ToString();
                            stage.StageInfo.LotData.SoakingRemainTime = Math.Abs(soakinfo.RemainTime).ToString();
                            stage.StageInfo.LotData.StopSoakBtnEnable = soakinfo.StopSoakBtnEnable;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateLotVerifyInfo(int foupindex, int cellindex, bool flag)
        {
            try
            {
                LoggerManager.Debug($"GPWCFLoaderService.UpdateLotVerifyInfo() flag: {flag}");
                
                if(foupindex <= 0)
                {
                    foupindex = 1;
                }

                var foup = LoaderModule.Foups.FirstOrDefault(x => x.Index == foupindex);

                if(foup != null)
                {
                    if(foup.LotSettings != null)
                    {
                        var lotsetting = foup.LotSettings.FirstOrDefault(x => x.Index == cellindex);

                        if(lotsetting != null)
                        {
                            lotsetting.IsVerified = flag;

                            if (flag == false) 
                            {
                                lotsetting.Clear(LoaderModule.GetUseLotProcessingVerify());
                                SetReasonOfError("Lot Parameter Validation Fail", $"Invalid (stage {cellindex}) information assigned to foup {foupindex}. \nPlease set the lot information and assign it.");
                            }
                            var vm = this.ViewModelManager().GetViewModelFromViewGuid(this.ViewModelManager().HomeViewGuid);

                            if (vm != null)
                            {
                                //(vm as ILoaderStageSummaryViewModel_GOP).TriggerForStartConfirm = !((vm as ILoaderStageSummaryViewModel_GOP).TriggerForStartConfirm);
                                (vm as ILoaderStageSummaryViewModel_GOP).TriggerForStartConfirm = !((vm as ILoaderStageSummaryViewModel_GOP).TriggerForStartConfirm);
                            }
                            else
                            {
                                LoggerManager.Error($"[{this.GetType().Name}], UpdateLotVerifyInfo() : flag: {flag}, vm is null.");
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], UpdateLotVerifyInfo() : flag: {flag}, lotsetting is null.");
                        }
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], UpdateLotVerifyInfo() : foup is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void UpdateDownloadMapResult(int cellindex, bool flag)
        {
            try
            {
                LoggerManager.Debug($"GPWCFLoaderService.UpdateDownloadMapResult() flag: {flag}");
                var stage = _LoaderCommunicationManager.GetStage(cellindex);

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        stage.StageInfo.IsMapDownloaded = flag;

                        // ViewModel이 갖고 있는 변수 변경을 통해, 컨버터 트리거...

                        //var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderStageSummaryVMGuid);
                        var vm = this.ViewModelManager().GetViewModelFromGuid(this.ViewModelManager().HomeViewGuid);

                        if (vm != null)
                        {
                            (vm as ILoaderStageSummaryViewModel_GOP).TriggerForStartConfirm = !((vm as ILoaderStageSummaryViewModel_GOP).TriggerForStartConfirm);
                        }

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateLogUploadList(int cellindex, EnumUploadLogType type)
        {
            try
            {
                LoaderMaster.LoaderLogManager.UpdateLogUploadListForStage(cellindex, type);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateLotModeEnum(int cellindex, LotModeEnum mode)
        {
            try
            {
                LoggerManager.Debug($"GPWCFLoaderService.UpdateLotModeEnum() Mode : {mode}");
                var stage = _LoaderCommunicationManager.GetStage(cellindex);

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        if (stage.StageInfo.LotData.LotMode != mode)
                        {
                            stage.StageInfo.LotData.LotMode = mode;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateTesterConnectedStatus(int cellindex, bool flag)
        {
            try
            {
                var stage = _LoaderCommunicationManager.GetStage(cellindex);

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        stage.StageInfo.IsTesterConnected = flag;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void UpdateLotDataInfo(int cellindex, StageLotDataEnum type, string val)
        {
            try
            {
                LoggerManager.Debug($"UpdateLotDataInfo(). Cell : [{cellindex}]");
                var stage = _LoaderCommunicationManager.GetStage(cellindex);

                if (stage != null)
                {
                    if (stage.StageInfo != null)
                    {
                        if (stage.StageInfo.LotData != null)
                        {
                            switch (type)
                            {
                                case StageLotDataEnum.WAFERLOADINGTIME:
                                    stage.StageInfo.LotData.WaferLoadingTime = val;
                                    break;
                                case StageLotDataEnum.FOUPNUMBER:
                                    stage.StageInfo.LotData.FoupNumber = val;
                                    break;
                                case StageLotDataEnum.SLOTNUMBER:
                                    stage.StageInfo.LotData.SlotNumber = val;
                                    break;
                                case StageLotDataEnum.WAFERCOUNT:
                                    stage.StageInfo.LotData.WaferCount = val;
                                    break;
                                case StageLotDataEnum.PROCESSEDWAFERCOUNTUNTILBEFORECARDCHANGE:
                                    stage.StageInfo.LotData.ProcessedWaferCountUntilBeforeCardChange = val;
                                    break;
                                case StageLotDataEnum.TOUCHDOWNCOUNTUNTILBEFORECARDCHANGE:
                                    stage.StageInfo.LotData.TouchDownCountUntilBeforeCardChange = val;
                                    break;
                                case StageLotDataEnum.SETTEMP:
                                    stage.StageInfo.LotData.SetTemp = val;
                                    break;
                                case StageLotDataEnum.DEVIATION:
                                    stage.StageInfo.LotData.Deviation = val;
                                    break;
                                case StageLotDataEnum.LOTSTATE:
                                    stage.StageInfo.LotData.LotState = val;
                                    break;
                                case StageLotDataEnum.WAFERALIGNSTATE:
                                    stage.StageInfo.LotData.WaferAlignState = val;
                                    break;
                                case StageLotDataEnum.PADCOUNT:
                                    stage.StageInfo.LotData.PadCount = val;
                                    break;
                                case StageLotDataEnum.PINALIGNSTATE:
                                    stage.StageInfo.LotData.PinAlignState = val;
                                    break;
                                case StageLotDataEnum.MARKALIGNSTATE:
                                    stage.StageInfo.LotData.MarkAlignState = val;
                                    break;
                                case StageLotDataEnum.PROBINGSTATE:
                                    stage.StageInfo.LotData.ProbingState = val;
                                    break;
                                case StageLotDataEnum.PROBINGOD:
                                    stage.StageInfo.LotData.ProbingOD = val;
                                    break;
                                case StageLotDataEnum.CLEARANCE:
                                    stage.StageInfo.LotData.Clearance = val;
                                    break;
                                case StageLotDataEnum.STAGEMOVESTATE:
                                    stage.StageInfo.LotData.StageMoveState = val;
                                    break;
                                case StageLotDataEnum.LOTSTARTTIME:
                                    stage.StageInfo.LotData.LotStartTime = val;
                                    break;
                                case StageLotDataEnum.LOTENDTIME:
                                    stage.StageInfo.LotData.LotEndTime = val;
                                    break;
                                case StageLotDataEnum.SOAKING:
                                    stage.StageInfo.LotData.SoakingState = val;
                                    break;
                                case StageLotDataEnum.LOTNAME:
                                    stage.StageInfo.LotData.LotID = val;
                                    break;
                                case StageLotDataEnum.LOTMODE:
                                    LotModeEnum lotmode = (LotModeEnum)Enum.Parse(typeof(LotModeEnum), val);
                                    stage.StageInfo.LotData.LotMode = lotmode;
                                    break;
                                case StageLotDataEnum.TEMPSTATE:
                                    stage.StageInfo.LotData.TempState = val;
                                    break;
                                default:
                                    break;
                            }
                            LoggerManager.Debug($"UpdateLotDataInfo(). StageLotDataEnum: {type.ToString()}, Value = {val}");
                            stage.StageInfo.LotData.DataCollect();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateStageMove(StageMoveInfo info)
        {
            try
            {
                LoggerManager.Debug($"UpdateStageMove(). Start");
                var vm = this.ViewModelManager().GetViewModelFromViewGuid(this.ViewModelManager().HomeViewGuid);

                if (vm != null)
                {
                    if (_LoaderCommunicationManager.SelectedStageIndex == info.ChuckIndex)
                    {
                        (vm as ILoaderStageSummaryViewModel).StageMoveState = info.StageMove;
                        LoggerManager.Debug($"UpdateStageMove(). StageMove : [{info.StageMove}]");
                    }
                }
                else
                {
                    LoggerManager.Debug($"UpdateStageMove() : vm is  null.");
                    LoggerManager.Debug($"UpdateStageMove() : {this.ViewModelManager().HomeViewGuid}");
                }
                LoggerManager.Debug($"UpdateStageMove(). End");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum PINImageUploadLoaderToServer(int cellindex, byte[] images)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                ret = LoaderLogmanager.PINImageUploadLoaderToServer(cellindex, images);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public EventCodeEnum PMIImageUploadStageToLoader(int cellindex, byte[] data, string filename)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                ret = LoaderLogmanager.PMIImageUploadStageToLoader(cellindex, data, filename);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public Task<EventCodeEnum> PMIImageUploadLoaderToServer(int cellindex)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderLogmanager.PMIImageUploadLoaderToServer(cellindex);

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult(ret);
        }

        public EventCodeEnum LogUpload(int cellindex, EnumUploadLogType logtype)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                ret = LoaderLogmanager.CellLogUploadToServer(cellindex, logtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public void UploadRecentLogs(int cellindex)
        {
            try
            {
                LoaderLogmanager.UploadRecentLogs(cellindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public EventCodeEnum ODTPUpload(int stageindex, string filename)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderODTPManager.CellODTPUploadToServer(stageindex, filename.ToLower());
                ret = EventCodeEnum.NONE;
            }
            catch (Exception)
            {

            }

            return ret;
        }
        public EventCodeEnum ResultMapUpload(int stageindex, string filename)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderResultMapUpDownMng.CellResultMapUploadToServer(stageindex, filename);
                ret = EventCodeEnum.NONE;
            }
            catch (Exception)
            {

            }

            return ret;
        }
        public EventCodeEnum ResultMapDownload(int stageindex, string filename)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderResultMapUpDownMng.ServerResultMapDownloadToCell(stageindex, filename);
                ret = EventCodeEnum.NONE;
            }
            catch (Exception)
            {

            }

            return ret;
        }

        public EventCodeEnum UploadCardPatternImages(byte[] data, string filename, string devicename, string cardid)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderLogmanager.UploadCardPatternImages(data, filename, devicename, cardid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

        public EnumWaferType GetActiveLotWaferType(string lotid)
        {
            EnumWaferType ret = EnumWaferType.UNDEFINED;

            try
            {
                if(lotid == null || lotid == "")
                {
                    ret = EnumWaferType.UNDEFINED;
                    LoggerManager.Error($"[GPWCFLoaderService].GetActiveLotWaferType(): Invalid.");
                }
                else
                {
                    var foupnum = LoaderMaster.ActiveLotInfos.Where(l => l.LotID == lotid).FirstOrDefault().FoupNumber;
                    
                    ret = LoaderMaster.Loader.ModuleManager
                         .GetTransferObjectAll().Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                                            (((w.OriginHolder.Index- 1) / 25) + 1) == foupnum)
                         .FirstOrDefault()?.WaferType?.Value ?? EnumWaferType.INVALID;
                    LoggerManager.Debug($"[GPWCFLoaderService].GetActiveLotWaferType(): {ret}");
                }
               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        public List<CardImageBuffer> DownloadCardPatternImages(string devicename, int downimgcnt, string cardid)
        {
            List<CardImageBuffer> ret = new List<CardImageBuffer>();

            try
            {
                ret = LoaderLogmanager.DownloadCardPatternImages(devicename, downimgcnt, cardid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        public EventCodeEnum UploadProbeCardInfo(ProberCardListParameter probeCard)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderLogmanager.UploadProbeCardInfo(probeCard);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }
        public ProberCardListParameter DownloadProbeCardInfo(string cardID)
        {
            ProberCardListParameter retVal = null;
            try
            {
                retVal = LoaderLogmanager.DownloadProbeCardInfo(cardID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetProbingStart(int cellIdx, bool isStart)
        {
            try
            {
                LoaderMaster.SetProbingStart(cellIdx, isStart);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetTransferError(int cellIdx, bool isError)
        {
            try
            {
                LoaderMaster.SetTransferError(cellIdx, isError);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetReasonOfError(string title, string errorMsg, string recoveryBeh = "", int cellIdx = 0)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoaderMaster.Loader.RecoveryCellIdx = cellIdx;
                LoaderMaster.Loader.RecoveryBehavior = recoveryBeh;

                Task.Run(() =>
                {
                    var ret = this.MetroDialogManager().ShowMessageDialog(title, errorMsg, EnumMessageStyle.Affirmative);
                });

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetStopBeforeProbingFlag(int stageidx, bool flag)
        {
            try
            {
                var stage = _LoaderCommunicationManager.GetStage(stageidx);
                if(stage != null)
                {
                    stage.StopBeforeProbing = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStopAfterProbingFlag(int stageidx, bool flag)
        {
            try
            {
                var stage = _LoaderCommunicationManager.GetStage(stageidx);
                if (stage != null)
                {
                    stage.StopAfterProbing = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetOnceStopBeforeProbingFlag(int stageidx, bool flag)
        {
            try
            {
                var stage = _LoaderCommunicationManager.GetStage(stageidx);
                if (stage != null)
                {
                    stage.OnceStopBeforeProbing = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetOnceStopAfterProbingFlag(int stageidx, bool flag)
        {
            try
            {
                var stage = _LoaderCommunicationManager.GetStage(stageidx);
                if (stage != null)
                {
                    stage.OnceStopAfterProbing = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum WriteWaitHandle(short value)
        {
            return LoaderMaster.WriteWaitHandle(value);
        }
        public void SetStageLock(int stageIndex, StageLockMode mode)
        {
            try
            {
                LoaderMaster.SetStageLock(stageIndex, mode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetForcedDoneMode(int stageIndex, EnumModuleForcedState forcedDoneMode)
        {
            try
            {
                LoaderMaster.SetForcedDoneMode(stageIndex, forcedDoneMode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetStopBeforeProbingFlag(int stageIdx)
        {
            bool retVal = false;
            retVal = LoaderMaster.GetStopBeforeProbingFlag(stageIdx);
            return retVal;
        }
        public bool GetStopAfterProbingFlag(int stageIdx)
        {
            bool retVal = false;
            retVal = LoaderMaster.GetStopAfterProbingFlag(stageIdx);
            return retVal;
        }

        public EventCodeEnum WaitForHandle(short handle, long timeout = 60000)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            try
            {

                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();
                bool commandDone = false;
                bool runFlag = true;
                do
                {
                    if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                    {
                        return  EventCodeEnum.NONE;
                    }
                    var handleState = ReadWaitHandle();
                    if (handleState == handle)
                    {
                        commandDone = true;
                    }
                    if (commandDone == true)
                    {
                        if (handleState == handle)
                        {
                            runFlag = false;
                            errorCode = EventCodeEnum.NONE;
                        }
                    }
                    if (timeout != 0)
                    {
                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                        {
                            runFlag = false;
                            LoggerManager.Debug($"WaitForHandle(): Timeout occurred. Target state = {handle}, Curr. State = {handleState}, Timeout = {timeout}");
                            errorCode = EventCodeEnum.LOADER_ROBOTCMD_TIMEOUT;
                        }
                    }
                } while (runFlag == true);


            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForHandle(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            finally
            {
                elapsedStopWatch.Stop();
            }
            return errorCode;
        }
        public int ReadWaitHandle()
        {
            return LoaderMaster.ReadWaitHandle();
        }
        /// <summary>
        /// PGV Card Change State를 가져오는 함수
        /// </summary>
        /// <returns>CardChangeSupervisorState의 현재 State를 리턴</returns>
        public ModuleStateEnum GetPGVCardChangeState()
        {
            ICardChangeSupervisor cardChangeSupervisor = LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
            return cardChangeSupervisor.ModuleState.GetState();
        }

        public bool IsActiveCCAllocatedState(int stageNumber)
        {
            ICardChangeSupervisor cardChangeSupervisor = LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
            return cardChangeSupervisor.IsAllocatedStage(stageNumber);
        }


        public void SetTCW_Mode(int stageIndex, TCW_Mode mode)
        {
            LoaderMaster.SetTCW_Mode(stageIndex, mode);
        }

        public EventCodeEnum IsShutterClose(int cellIdx)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal= LoaderMaster.IsShutterClose(cellIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsShutterClose(): Exception occurred. Err = {err.Message}");
                throw;
            }
            finally
            {
            }
            return retVal;
        }

        public void SetMonitoringBehavior(byte[] monitoringBehaviors, int stageIdx)
        {
            try
            {
                object obj = this.ByteArrayToObjectSync(monitoringBehaviors);

                List<IMonitoringBehavior> monitoringBehaviorsList = obj as List<IMonitoringBehavior>;

                LoaderMaster.SetMonitoringBehavior(monitoringBehaviorsList, stageIdx);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangeTabIndex(TabControlEnum tabEnum)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromViewGuid(this.ViewModelManager().HomeViewGuid);

                if (vm != null)
                {
                    (vm as ILoaderStageSummaryViewModel).ChangeTabIndex(tabEnum);
                }
                else
                {
                    LoggerManager.Debug($"ChangeTabIndex() : vm is  null.");
                    LoggerManager.Debug($"ChangeTabIndex() : {this.ViewModelManager().HomeViewGuid}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum GetLoaderEmergency()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.LoaderMaster.GetLoaderEmergency();
                LoggerManager.Debug($"GPWCFLoaderService.GetLoaderEmergency() {retVal}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderEmergency(): Exception occurred. Err = {err.Message}");
            }

            return retVal;
        }

        public void SetTransferAbort()
        {
            try
            {
                this.LoaderMaster.Loader.SetTransferAbort();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
