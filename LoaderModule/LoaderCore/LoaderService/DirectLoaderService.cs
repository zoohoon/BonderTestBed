using System;

using Autofac;
using ProberInterfaces;
using LoaderBase;
using LoaderParameters;
using ProberErrorCode;
using ProberInterfaces.Foup;
using LoaderServiceBase;
using LogModule;
using LoaderParameters.Data;

namespace LoaderCore
{
    public class DirectLoaderService : IDirectLoaderService, IFactoryModule
    {
        private ILoaderServiceCallback callback;
        public Autofac.IContainer loaderContainer;
        private ILoaderModule Loader => loaderContainer.Resolve<ILoaderModule>();

        #region => Direct Extension Methods
        public void SetLoaderContainer(Autofac.IContainer loaderContainer)
        {
            try
            {
                this.loaderContainer = loaderContainer;

                Extensions_IModule.SetLoaderContainer(null, this.loaderContainer);

                this.Loader.SetLoaderContainer(loaderContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Autofac.IContainer GetLoaderContainer()
        {
            return loaderContainer;
        }
        public void Set(Autofac.IContainer stageContainer, ILoaderServiceCallback callback)
        {
            try
            {
                this.Loader.SetStageContainer(stageContainer);
                this.callback = callback;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Deinitialize()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Deinitialize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region => Init Methods
        public EventCodeEnum Connect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Connect(callback);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum CallbackConnect()
        {
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

        public EventCodeEnum Disconnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Disconnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Initialize(string rootParamPath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.Initialize(LoaderServiceTypeEnum.DynamicLinking, rootParamPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region => Loader Work Methods
        public LoaderInfo GetLoaderInfo()
        {
            LoaderInfo retVal = null;

            try
            {
                retVal = Loader.GetLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public string GetResonOfError()
        {
            string error = Loader.ResonOfError;
            Loader.ResonOfError = null;
            return error;
        }

        public bool IsFoupAccessed(int cassetteNumber)
        {
            bool retVal = false;

            try
            {
                retVal = Loader.IsFoupAccessed(cassetteNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool SetNoReadScanState(int cassetteNumber)
        {
            bool retVal = false;

            try
            {
                retVal = Loader.SetNoReadScanState(cassetteNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ResponseResult SetRequest(LoaderMap dstMap)
        {
            ResponseResult retVal = new ResponseResult();

            try
            {
                retVal = Loader.SetRequest(dstMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum AwakeProcessModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.AwakeProcessModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum AbortRequest()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.AbortRequest();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ClearRequestData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.ClearRequestData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void SetPause()
        {
            try
            {
                Loader.SetPause();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetResume()
        {
            try
            {
                Loader.SetResume();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SelfRecovery()
        {
            try
            {
                Loader.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region => Motion Methods
        public EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.SystemInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.MOTION_JogRelMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.MOTION_JogAbsMove(axis, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region => Setting Param Methods
        public LoaderSystemParameter GetSystemParam()
        {
            LoaderSystemParameter retVal = null;

            try
            {
                retVal = Loader.SystemParameter.Clone<LoaderSystemParameter>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderDeviceParameter GetDeviceParam()
        {
            LoaderDeviceParameter retVal = null;

            try
            {
                retVal = Loader.DeviceParameter.Clone<LoaderDeviceParameter>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.UpdateSystemParam(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.SaveSystemParam(systemParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.UpdateDeviceParam(deviceParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        //public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam=null)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = Loader.SaveDeviceParam(deviceParam);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}
        public EventCodeEnum GetWaferLoadObject(out TransferObject loadobj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadobj = new TransferObject();
            try
            {
                retVal = Loader.WaferTransferRemoteService.GetWaferLoadObject(out loadobj);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.MoveToModuleForSetup(module, skipuaxis, slot, index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum RetractAll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.RetractAll();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region => Notify Foup State
        public void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo)
        {
            try
            {
                Loader.FOUP_RaiseFoupStateChanged(foupInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void FOUP_RaiseWaferOutDetected(int cassetteNumber)
        {
            try
            {
                Loader.FOUP_RaiseWaferOutDetected(cassetteNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region => WaferTransfer Remote Methods
        public EventCodeEnum WTR_ChuckUpMove(int option=0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.ChuckUpMove(option);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum WTR_Wafer_MoveLoadingPosition()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.Wafer_MoveLoadingPosition();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_ChuckDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.ChuckDownMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_WriteARMVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.WriteVacuum(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_MonitorForARMVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.MonitorForVacuum(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_WaitForARMVacuum(bool value)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.WaitForVacuum(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_RetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.RetractARM();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public bool WTR_IsLoadWafer()
        {
            bool retVal = false;
            try
            {
                retVal = Loader.WaferTransferRemoteService.IsLoadWafer();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WTR_SafePosW()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.SafePosW();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.Notifyhandlerholdwafer(ishandlerhold);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        
        public EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = Loader.WaferTransferRemoteService.NotifyLoadedToThreeLeg(out loadedObject);

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool isWaferStateChange=false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.NotifyUnloadedFromThreeLeg(waferState,cellIdx, isWaferStateChange);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_PickUpMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.PickUpMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_PlaceDownMove()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.PlaceDownMove();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.NotifyWaferTransferResult(isSucceed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SelfRecoveryRetractARM()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.SelfRecoveryRetractARM();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SelfRecoveryTransferToPreAlign()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.WaferTransferRemoteService.SelfRecoveryTransferToPreAlign();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public double GetArmUpOffset()
        {
            return Loader.WaferTransferRemoteService.GetCurrArmUpOffset();
        }

        #endregion

        #region => OCR Remote Methods
        public EventCodeEnum OFR_GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = Loader.OCRRemoteService.GetOCRImage(out imgBuf, out w, out h);
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OFR_ChangeLight(int channelMapIdx, ushort intensity)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.ChangeLight(channelMapIdx, intensity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OFR_SetOcrID(string ocrID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.SetOcrID(ocrID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OFR_OCRRemoteEnd()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.OCRRemoteEnd();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum OFR_GetOCRState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.GetOCRState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OFR_OCRRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.OCRRetry();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum OFR_OCRFail()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.OCRFail();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OFR_OCRAbort()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.OCRRemoteService.OCRAbort();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        
        #endregion

        #region => Recovery Methods
        public EventCodeEnum RECOVERY_MotionInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.RECOVERY_MotionInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RECOVERY_ResetWaferLocation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = Loader.RECOVERY_ResetWaferLocation();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        #endregion

        #region => TestFlag
        public void SetTestCenteringFlag(bool centeringtestflag)
        {
            try
            {
                Loader.SetTestCenteringFlag(centeringtestflag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public void SetLoaderTestOption(LoaderTestOption option)
        {
            Loader.LoaderOption = option;
        }

        public LoaderServiceTypeEnum GetServiceType()
        {
            return LoaderServiceTypeEnum.DynamicLinking;
        }

        public void SetContainer(IContainer container)
        {
            loaderContainer = container;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void SetCallBack(ILoaderServiceCallback loadercontroller)
        {
            callback = loadercontroller;
        }

        public EventCodeEnum UpdateLoaderSystem(int foupIndex)
        {
            throw new NotImplementedException();
        }

        public string GetProbeCardID()
        {
            return Loader.GetLoaderInfo().ModuleInfo.ProbeCardID ;
        }

        public EventCodeEnum ResultMapUpload(int v)
        {
            LoggerManager.Debug($"ResultMaoUpload(): Method excuted");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ResultMapDownload(int v, string s)
        {
            LoggerManager.Debug($"ResultMaoUpload(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum UpdateCassetteSystem(SubstrateSizeEnum WaferSize, int foupIndex)
        {
            throw new NotImplementedException();
        }
    }
}
