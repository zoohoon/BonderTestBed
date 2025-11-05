using System;
using System.Linq;
using System.Runtime.CompilerServices;

using ProberInterfaces;
using System.Runtime.Serialization;
using LogModule;

namespace LoaderParameters
{
    /// <summary>
    /// LoaderMapEditor의 상태를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public enum LoaderMapEditorStateEnum
    {
        /// <summary>
        /// LoaderMap을 변경할 수 있는 상태
        /// </summary>
        [EnumMember]
        READY,
        /// <summary>
        /// LoaderMap에 스캔 명령이 적용 된 상태
        /// </summary>
        [EnumMember]
        SCAN,
        /// <summary>
        /// LoaderMap에 이송 명령이 적용 된 상태
        /// </summary>
        [EnumMember]
        TRANSFERS,
        /// <summary>
        /// LoaderMap에 OCR 명령이 적용 된 상태
        /// </summary>
        [EnumMember]
        OCR,
        /// <summary>
        /// LoaderMap이 유효하지 않은 상태
        /// </summary>
        [EnumMember]
        ERROR,
    }

    /// <summary>
    /// LoaderMap을 조작하기 위한 도구를 정의합니다.
    /// </summary>
    [DataContract]
    [Serializable]
    public class LoaderMapEditor
    {
        /// <summary>
        /// LoaderMapEditor의 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="schedulingMap">LoaderMap</param>
        public LoaderMapEditor(LoaderMap schedulingMap)
        {
            try
            {
                this.EditMap = schedulingMap?.Clone() as LoaderMap;

                this.EditorState = new LoaderMapEditorReadyState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        /// <summary>
        /// 변경된 LoaderMap을 가져옵니다.
        /// </summary>
        public LoaderMap EditMap { get; private set; }

        /// <summary>
        /// LoaderMapEditor의 상태오브젝트를 가져옵니다.
        /// </summary>
        public LoaderMapEditorStateBase EditorState { get; private set; }

        /// <summary>
        /// LoaderMapEditor의 오류 사유를 가져옵니다.
        /// </summary>
        public string ReasonOfError { get; internal set; }

        /// <summary>
        /// Error 상태의 Editor 인스턴스를 가져옵니다.
        /// </summary>
        public static LoaderMapEditor ERROR_EDITOR
        {
            get
            {
                LoaderMapEditor editor = new LoaderMapEditor(null);
                editor.EditorState = new LoaderMapEditorErrorState(editor);
                return editor;
            }
        }

        internal void StateTransition(LoaderMapEditorStateBase stateObj)
        {
            EditorState = stateObj;
        }
    }

    /// <summary>
    /// LoaderMapEditor의 상태를 정의합니다.
    /// </summary>
    public abstract class LoaderMapEditorStateBase
    {
        /// <summary>
        /// LoaderMapEditor를 가져오거나 설정합니다.
        /// </summary>
        public LoaderMapEditor Editor { get; set; }

        /// <summary>
        /// LoaderMapEditor의 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="editor">LoaderMapEditor</param>
        public LoaderMapEditorStateBase(LoaderMapEditor editor) { this.Editor = editor; }

        /// <summary>
        /// LoaderMapEditor의 상태 값을 가져옵니다.
        /// </summary>
        public abstract LoaderMapEditorStateEnum State { get; }

        #region State Methods
        ///// <summary>
        ///// LoaderMap에 NotchFinding을 설정합니다.
        ///// </summary>
        ///// <param name="PreAlign">대상 PreALign 아이디</param>
        //public virtual void SetPreAlignFindingNotch(ModuleID PreAlign)
        //{
        //    RaiseInvalidCall();
        //}
        /// <summary>
        /// LoaderMap에 스캔명령을 설정합니다.
        /// </summary>
        /// <param name="cassetteID">대상 카세트 아이디</param>
        public virtual void SetScanCassette(ModuleID cassetteID)
        {
            RaiseInvalidCall();
        }

        /// <summary>
        /// LoaderMap에 이송 명령을 설정합니다.
        /// </summary>
        /// <param name="id">Transfer Object ID</param>
        /// <param name="destinationID">목적지 ID</param>
        /// <param name="notchOption">Notch Option</param>
        public virtual void SetTransfer(string id, ModuleID destinationID, LoadNotchAngleOption notchOption = null)
        {
            RaiseInvalidCall();
        }

        /// <summary>
        /// OCR위치에서 PreAlign위치로 이송 명령을 설정합니다.
        /// </summary>
        /// <param name="id">Transfer Object ID</param>
        /// <param name="preAlignID">PreAlign ID</param>
        /// <param name="processOption">OCR Process Option</param>
        public virtual void SetOcrToPreAlign(string id, ModuleID preAlignID, OCRPerformOption processOption = null)
        {
            RaiseInvalidCall();
        }

        /// <summary>
        /// OCR 명령을 설정합니다.
        /// </summary>
        /// <param name="id">Transfer Object ID</param>
        /// <param name="OcrID">OCR ID</param>
        /// <param name="processOption">OCR Process Option</param>
        /// <param name="deviceOption">Override OCR Device Option</param>
        public virtual void SetOCR(string id, ModuleID OcrID, OCRPerformOption processOption = null, OCRDeviceOption deviceOption = null)
        {
            RaiseInvalidCall();
        }
        #endregion

        internal void RaiseInvalidCall([CallerMemberName]string callerName = "")
        {
            try
            {
                Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.{callerName}() :Invalid state. currState={State}";
                Editor.StateTransition(new LoaderMapEditorErrorState(Editor));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Protected Methods
        internal LoaderMap EditMap => Editor.EditMap;

        internal bool SetScanCassetteFunc(ModuleID cassetteID)
        {
            var cassette = EditMap.CassetteModules.Where(item => item.ID == cassetteID).FirstOrDefault();

            try
            {
                if (cassette == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetScanCassetteFunc() : Can not found cassette. cassetteID={cassetteID}";
                    return false;
                }
                cassette.ScanState = CassetteScanStateEnum.READING;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return true;
        }

        internal bool SetTransferFunc(string transferObjID, ModuleID destination, LoadNotchAngleOption notchOption)
        {
            TransferObject subObj = EditMap.GetTransferObjectAll().Where(item => item.ID.Value == transferObjID).FirstOrDefault();
            ModuleInfoBase dstLoc = EditMap.GetLocationModules().Where(item => item.ID == destination).FirstOrDefault();

            try
            {
                if (subObj == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Can not found substrate object. substrateID={transferObjID}";
                    return false;
                }

                if (dstLoc == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Can not found destination Module. destination={destination}";
                    return false;
                }

                if (subObj.CurrPos == destination)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Same position. destination={destination}";
                    return false;
                }

                if (dstLoc is HolderModuleInfo)
                {
                    var currHolder = EditMap.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                    var dstHolder = dstLoc as HolderModuleInfo;

                    subObj.PrevHolder = subObj.CurrHolder;
                    subObj.PrevPos = subObj.CurrPos;

                    subObj.CurrHolder = destination;
                    subObj.CurrPos = destination;
                    subObj.SetReservationState(ReservationStateEnum.RESERVATION);

                    if (notchOption != null)
                        subObj.OverrideLoadNotchAngleOption = notchOption;

                    currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                    currHolder.Substrate = null;

                    dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                    dstHolder.Substrate = subObj;
                }
                else
                {
                    subObj.PrevPos = subObj.CurrPos;
                    subObj.CurrPos = destination;
                }

                subObj.ReservationTime = DateTime.UtcNow;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return true;
        }

        internal bool SetOcrToPreAlignFunc(string transferObjID, ModuleID destination, OCRPerformOption ocrProcOption)
        {
            var subObj = EditMap.GetTransferObjectAll().Where(item => item.ID.Value == transferObjID).FirstOrDefault();
            var dstLoc = EditMap.GetLocationModules().Where(item => item.ID == destination).FirstOrDefault();

            try
            {

                if (subObj == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Can not found substrate object. substrateID={transferObjID}";
                    return false;
                }

                if (dstLoc == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Can not found destination Module. destination={destination}";
                    return false;
                }

                if (subObj.CurrPos == destination)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Same position. destination={destination}";
                    return false;
                }

                if (dstLoc is HolderModuleInfo)
                {
                    var currHolder = EditMap.GetHolderModuleAll().Where(item => item.ID == subObj.CurrHolder).FirstOrDefault();
                    var dstHolder = dstLoc as HolderModuleInfo;

                    subObj.PrevHolder = subObj.CurrHolder;
                    subObj.PrevPos = subObj.CurrPos;

                    subObj.CurrHolder = destination;
                    subObj.CurrPos = destination;

                    if (ocrProcOption != null)
                        subObj.OverrideOCROption = ocrProcOption;

                    currHolder.WaferStatus = EnumSubsStatus.NOT_EXIST;
                    currHolder.Substrate = null;

                    dstHolder.WaferStatus = EnumSubsStatus.EXIST;
                    dstHolder.Substrate = subObj;
                }
                else
                {
                    subObj.PrevPos = subObj.CurrPos;
                    subObj.CurrPos = destination;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return true;
        }

        internal bool SetOCRFunc(string transferObjID, ModuleID OcrID, OCRPerformOption processOption, OCRDeviceOption deviceOption)
        {
            TransferObject subObj = EditMap.GetTransferObjectAll().Where(item => item.ID.Value == transferObjID).FirstOrDefault();
            ModuleInfoBase OCR = EditMap.GetLocationModules().Where(item => item.ID == OcrID).FirstOrDefault();

            try
            {

                if (subObj == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Can not found substrate object. substrateID={transferObjID}";
                    return false;
                }

                if (OCR == null)
                {
                    Editor.ReasonOfError = $"[LOADER] LoaderMapEditorState.SetTransferFunc() : Can not found ocr module. OcrID={OcrID}";
                    return false;
                }

                subObj.PrevPos = subObj.CurrPos;
                subObj.CurrPos = OcrID;

                if (processOption != null)
                    subObj.OverrideOCROption = processOption;

                if (deviceOption != null)
                    subObj.OverrideOCRDeviceOption = deviceOption;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return true;
        }
        #endregion
    }

    internal class LoaderMapEditorReadyState : LoaderMapEditorStateBase
    {
        public LoaderMapEditorReadyState(LoaderMapEditor editor) : base(editor) { }

        public override LoaderMapEditorStateEnum State => LoaderMapEditorStateEnum.READY;
        //public override void SetPreAlignFindingNotch(ModuleID PreAlign)
        //{

        //}

        public override void SetScanCassette(ModuleID cassetteID)
        {
            try
            {
                bool isSucceed = SetScanCassetteFunc(cassetteID);
                try
                {
                    if (isSucceed)
                        Editor.StateTransition(new LoaderMapEditorScanCassetteState(Editor));
                    else
                        Editor.StateTransition(new LoaderMapEditorErrorState(Editor));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetTransfer(string id, ModuleID holderID, LoadNotchAngleOption notchOption)
        {
            try
            {
                bool isSucceed = SetTransferFunc(id, holderID, notchOption);
                try
                {
                    if (isSucceed)
                        Editor.StateTransition(new LoaderMapEditorTransfersState(Editor));
                    else
                        Editor.StateTransition(new LoaderMapEditorErrorState(Editor));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetOcrToPreAlign(string id, ModuleID holderID, OCRPerformOption processOption = null)
        {
            try
            {
                bool isSucceed = SetOcrToPreAlignFunc(id, holderID, processOption);
                try
                {
                    if (isSucceed)
                        Editor.StateTransition(new LoaderMapEditorTransfersState(Editor));
                    else
                        Editor.StateTransition(new LoaderMapEditorErrorState(Editor));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetOCR(string id, ModuleID OcrID, OCRPerformOption processOption, OCRDeviceOption deviceOption)
        {
            try
            {
                bool isSucceed = SetOCRFunc(id, OcrID, processOption, deviceOption);
                try
                {
                    if (isSucceed)
                        Editor.StateTransition(new LoaderMapEditorOCRState(Editor));
                    else
                        Editor.StateTransition(new LoaderMapEditorErrorState(Editor));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    internal class LoaderMapEditorScanCassetteState : LoaderMapEditorStateBase
    {
        public LoaderMapEditorScanCassetteState(LoaderMapEditor editor) : base(editor) { }

        public override LoaderMapEditorStateEnum State => LoaderMapEditorStateEnum.SCAN;
    }

    internal class LoaderMapEditorTransfersState : LoaderMapEditorStateBase
    {
        public LoaderMapEditorTransfersState(LoaderMapEditor editor) : base(editor) { }

        public override LoaderMapEditorStateEnum State => LoaderMapEditorStateEnum.TRANSFERS;

        public override void SetTransfer(string id, ModuleID holderID, LoadNotchAngleOption notchOption)
        {
            try
            {
                bool isSucceed = SetTransferFunc(id, holderID, notchOption);
                try
                {
                    if (isSucceed)
                        Editor.StateTransition(new LoaderMapEditorTransfersState(Editor));
                    else
                        Editor.StateTransition(new LoaderMapEditorErrorState(Editor));
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    internal class LoaderMapEditorOCRState : LoaderMapEditorStateBase
    {
        public LoaderMapEditorOCRState(LoaderMapEditor editor) : base(editor) { }

        public override LoaderMapEditorStateEnum State => LoaderMapEditorStateEnum.OCR;
    }

    internal class LoaderMapEditorErrorState : LoaderMapEditorStateBase
    {
        public LoaderMapEditorErrorState(LoaderMapEditor editor) : base(editor) { }

        public override LoaderMapEditorStateEnum State => LoaderMapEditorStateEnum.ERROR;
    }
}
