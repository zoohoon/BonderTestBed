using System;
using System.Collections.Generic;

using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using System.Runtime.CompilerServices;

namespace LoaderBase
{
    public enum LoaderProcStateEnum
    {
        IDLE,
        RUNNING,
        SUSPENDED,
        DONE,
        SYSTEM_ERROR,
        ALARM_ERROR,
        ABORT
    }
    public enum LoaderProcModuleEnum
    {
        UNDIFINED,
        ARM_TO_BUFFER,
        ARM_TO_STAGE,
        ARM_TO_FIXEDTRAY,
        ARM_TO_INSPTRAY,
        ARM_TO_PREALIGN,
        ARM_TO_SLOT,
        BUFFER_TO_ARM,
        CARDARM_TO_CARDBUFFER,
        CARDARM_TO_STAGE,
        CARDARM_TO_CARDTRAY,
        CARDBUFFER_TO_CARDARM,
        STAGE_TO_CARDARM,
        CARDTRAY_TO_CARDARM,
        STAGE_TO_ARM,
        FIXEDTRAY_TO_ARM,
        INSPTRAY_TO_ARM,
        OCR,
        OCR_TO_PREALIGN,
        PREALIGN,
        PREALIGN_TO_ARM,
        PREALIGN_TO_OCR,
        SENSORSCANCASSETTE,
        SLOT_TO_ARM,
        CLOSE_FOUP_COVER
    }
   
    public interface ILoaderProcessModulePakage
    {
        List<ILoaderProcessModule> ProcModuleList { get; set; }
    }
    public interface ILoaderProcessModule
    {
        LoaderProcStateEnum State { get; }

        ReasonOfSuspendedEnum ReasonOfSuspended { get; }
        bool CanExecute(ILoaderProcessParam param);

        void Init(Autofac.IContainer container, ILoaderProcessParam param);

        void Execute();

        void Awake();

        void SelfRecovery();
    }

    public abstract class LoaderProcStateBase : IFactoryModule, ILoaderSubModule
    {
        public abstract LoaderProcStateEnum State { get; }

        public virtual ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.NONE;

        public virtual void Execute()
        {
            ThrowInvalidState();
        }

        public virtual void Resume()
        {
            ThrowInvalidState();
        }

        public virtual void SelfRecovery()
        {
            ThrowInvalidState();
        }

        private void ThrowInvalidState([CallerMemberName]string memberName = "")
        {
            string msg = $"[LOADER] {GetType().Name}.{State}.{memberName}() : Raise invalid state error occurred.";
            throw new Exception(msg);
        }
    }

    /// <summary>
    /// IWaferTransferRemotableProcessModule 을 정의합니다.
    /// </summary>
    public interface IWaferTransferRemotableProcessModule
    {
        /// <summary>
        /// Chuck Up 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ChuckUpMove(int option = 0);

        /// <summary>
        /// Chuck Down 위치로 이동합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum ChuckDownMove(int option=0);
        EventCodeEnum Wafer_MoveLoadingPosition();
        /// <summary>
        /// 현재 사용중인 ARM의 Vacuum을 입력된 value로 출력합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WriteVacuum(bool value);

        /// <summary>
        /// 현재 사용중인 ARM의 Vacuum의 상태가 value와 일치하는지 확인합니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum MonitorForVacuum(bool value);

        /// <summary>
        /// 현재 사용중인 ARM의 Vacuum상태가 value가 되기를 기다립니다.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WaitForVacuum(bool value);

        /// <summary>
        /// 현재 사용중인 ARM을 Retract합니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum RetractARM();



        EventCodeEnum SafePosW();
        /// <summary>
        /// 현재 사용되는 모듈들의 Status를 Unknown상태로 변경합니다.
        /// </summary>
        /// <param name="isARMUnknown">ARM의 상태가 Unknwon인지 여부</param>
        /// <param name="isChuckUnknown">Chuck의 상태가 Unknown인지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown);
        EventCodeEnum Notifyhandlerholdwafer(bool ishandlerhold);
        
        /// <summary>
        /// TransferObject 가 ThreeLeg에 Load되었음을 알립니다.
        /// </summary>
        /// <param name="loadedObject">Loader에서 관리되는 TransferObject의 정보</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum NotifyLoadedToThreeLeg(out TransferObject loadedObject);

        EventCodeEnum NotifyTransferObject(TransferObject transferobj);

        /// <summary>
        /// TransferObject가 ARM으로 Unload되었음을 알립니다.
        /// </summary>
        /// <param name="waferState">Stage에서 처리된 Object의 Processing State</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx,bool isWaferStateChange = true);

        /// <summary>
        /// Pickup Move를 수행합니다. (option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PickUpMove();

        /// <summary>
        /// Placedown Move를 수행합니다.(option)
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum PlaceDownMove();

        /// <summary>
        /// WaferTransfer작업이 종료되었음을 알립니다.
        /// </summary>
        /// <param name="isSucceed">성공했는지 여부</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum WaferTransferEnd(bool isSucceed);


        //EventCodeEnum SetResonOfError(string errorMsg);
        /// <summary>
        /// ARM을 Retract하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SelfRecoveryRetractARM();

        /// <summary>
        /// ARM에 Wafer이 존재한다고 가정하고 PreAlign으로 이송하는 Self Recovery를 수행합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SelfRecoveryTransferToPreAlign();

        double GetCurrArmUpOffset();
        EventCodeEnum GetWaferLoadedObject(out TransferObject loadedobj);

    }


    public interface ICardTransferRemotableProcessModule
    {
        EventCodeEnum CardTransferEnd(bool isSucceed);


        EventCodeEnum CardChangePick();
        EventCodeEnum CardChangePut(out TransferObject transObj);
        EventCodeEnum SetTransferAfterCardChangePutError(out TransferObject transObj, EnumWaferState waferState);
        String GetProbeCardID();
        EventCodeEnum GetUserCardIDInput(out string userCardIdInput);

        EventCodeEnum CardChangeCarrierPick();
        EventCodeEnum OriginCarrierPut();
        EventCodeEnum OriginCardPut();
        EventCodeEnum CardChangeCarrierPut();
        EventCodeEnum OriginCarrierPick();
        EventCodeEnum CardTransferDone(bool isSucceed);

        EventCodeEnum Card_MoveLoadingPosition();

        EventCodeEnum CardDockingDone();

    }
    /// <summary>
    /// IOCRRemotableProcessModule 를 정의합니다.
    /// </summary>
    public interface IOCRRemotableProcessModule
    {
        /// <summary>
        /// OCR Image를 가져옵니다.
        /// </summary>
        /// <param name="imgBuf">image buffer</param>
        /// <param name="w">image width</param>
        /// <param name="h">image height</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h);

        /// <summary>
        /// OCR의 Light를 조정합니다.
        /// </summary>
        /// <param name="channel">light channel</param>
        /// <param name="intensity">light intensity</param>
        /// <returns></returns>
        EventCodeEnum ChangeLight(int channel, ushort intensity);

        /// <summary>
        /// 사용자가 입력한 OCR을 적용합니다.
        /// </summary>
        /// <param name="inputOCR">사용자가 입력한 문자열</param>
        /// <returns>ErrorCode</returns>
        EventCodeEnum SetOcrID(string inputOCR);

        /// <summary>
        /// OCR Remote를 종료합니다.
        /// </summary>
        /// <returns>ErrorCode</returns>
        EventCodeEnum OCRRemoteEnd();
        EventCodeEnum GetOCRState();
        EventCodeEnum OCRRetry();
        EventCodeEnum OCRFail();

        EventCodeEnum OCRAbort();
    }
}
