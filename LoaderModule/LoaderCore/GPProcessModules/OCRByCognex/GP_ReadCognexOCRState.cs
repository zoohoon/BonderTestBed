using Autofac;
using CognexOCRManualDialog;
using LoaderBase;
using LoaderBase.AttachModules.ModuleInterfaces;
using LogModule;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Event;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoaderCore.GP_ReadCognexOCRStates
{
    public abstract class GP_ReadCognexOCRState : LoaderProcStateBase
    {
        public GP_ReadCognexOCR Module { get; set; }

        public GP_ReadCognexOCRState(GP_ReadCognexOCR module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_ReadCognexOCRState stateObj)
        {
            try
            {

                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state transition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected ICognexProcessManager CognexProcessManager => Module.Container.Resolve<ICognexProcessManager>();

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();


        protected IPreAlignModule PA => Module.Param.Curr as IPreAlignModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_ReadCognexOCRState
    {
        public IdleState(GP_ReadCognexOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }
    
    public class RunningState : GP_ReadCognexOCRState
    {
        public RunningState(GP_ReadCognexOCR module) : base(module)
        {
            Loader.LoaderMaster.LoaderLogManager.UpdateLogUploadListForLoader(EnumUploadLogType.LoaderOCR);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                String ocr = String.Empty;
                double ocrScore = 0;
                bool isOCRFail = false;
                bool isNeedOCRWait = false;
                EventCodeEnum retValOcr = EventCodeEnum.NONE;

                int slotNum = 0;
                int foupNum = 0;
                slotNum = PA.Holder.TransferObject.OriginHolder.Index % 25;
                int offset = 0;
                if (slotNum == 0)
                {
                    slotNum = 25;
                    offset = -1;
                }
                foupNum = ((PA.Holder.TransferObject.OriginHolder.Index + offset) / 25) + 1;

                double dstNotchAngle = 0;
                if (PA.Holder.Status == EnumSubsStatus.EXIST)
                {
                    dstNotchAngle = PA.Holder.TransferObject.NotchAngle.Value - 90;

                    dstNotchAngle = dstNotchAngle % 360;

                    if (dstNotchAngle < 0)
                    {
                        dstNotchAngle += 360;
                    }
                }


                Action<bool> SkipWafer = (showDialog) =>
                {
                    PA.Holder.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                    PA.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                    
                    if (showDialog)
                    {
                        var ret = (this).MetroDialogManager().ShowMessageDialog("OCR ABORT", $"Wafer({Loader.SlotToFoupConvert(PA.Holder.TransferObject.OriginHolder)}) is changed to the skipped state and returns to the origin position.", EnumMessageStyle.Affirmative).Result;
                    }

                    LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.ERROR, $"ABORT, OCR Index: {PA.ID.Index}, Origin Location: {Loader.SlotToFoupConvert(PA.Holder.TransferObject.OriginHolder)}, Wafer State: Skipped");
                };


                ///[1-1][ ArmToPreAlign 이후 ProcModule에서 OCR 시도하는 동작을 대기한다.] - 만약 PA가 Running중이 아니면 1번에 해당하는 동작을 하지 않고 바로 2번으로넘어간다.                 
                if (Module.Param.TransferObject.IsOCRDone() == false)// READ Mode와 Manual Mode가 들어올수있다.
                {

                    if (Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort == true)
                    {
                        isNeedOCRWait = false;
                    }
                    else if (Module.Param.TransferObject.OCRMode.Value == OCRModeEnum.READ || Module.Param.TransferObject.OCRMode.Value == OCRModeEnum.MANUAL)
                    {
                        if (this.Loader.LoaderMaster.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.RUNNING
                           || this.Loader.LoaderMaster.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.ABORT)
                        {
                            if (Module.Param.TransferObject.WaferType.Value == EnumWaferType.STANDARD || (Module.Param.TransferObject.WaferType.Value == EnumWaferType.TCW)
                              && (Module.Param.TransferObject.OCRMode.Value == OCRModeEnum.READ || Module.Param.TransferObject.OCRMode.Value == OCRModeEnum.MANUAL)
                                && Module.Param.TransferObject.OCRReadState != OCRReadStateEnum.DONE
                                && Module.Param.TransferObject.OCRReadState != OCRReadStateEnum.ABORT)
                            {
                                isNeedOCRWait = true;
                            }
                        }
                        else
                        {
                            isNeedOCRWait = false;
                        }
                    }
                    else
                    {
                        isNeedOCRWait = false;
                    }



                    if (isNeedOCRWait)
                    {                                                
                        retValOcr = this.GetLoaderCommands().WaitForOCR((IPreAlignModule)Module.Param.Curr, out ocr, out ocrScore);//OCR Angle + Centering + OCR Read 
                        if (retValOcr != EventCodeEnum.NONE || Module.Param.TransferObject.OCRReadState == OCRReadStateEnum.FAILED)
                        {
                            if (Module.Param.TransferObject.OCRReadState == OCRReadStateEnum.FAILED)
                            {
                                //OCR Config를 못찾아서 실패함. 
                                isOCRFail = true;
                            }
                            else if (Module.Param.TransferObject.IsOCRDone())
                            {
                                //do nothing
                            }
                            else
                            {
                                //PreAlign 실패함.
                                //Module.Param.TransferObject.SetOCRState(Module.Param.TransferObject.OCR.Value, 0, OCRReadStateEnum.ABORT);// OCR 시도 전 PreAlign Fail 이 여기로 오기 때문에 이 웨이퍼는 아직 OCRReadState가 NONE상태, ABORT로 만들어서 다시 시도하지 않도록 해야함.
                                Module.Param.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                LoggerManager.Debug($"GP_ReadCognexOCRState(): Failed. " +
                                                                 $"WaferType: {Module.Param.TransferObject.WaferType.Value}\n" +
                                                                 $"OCRReadState must be Done, Cur: {Module.Param.TransferObject.OCRReadState}\n" +
                                                                 $"PreAlignState must be Done, Cur: {Module.Param.TransferObject.PreAlignState}\n" +
                                                                 $"OcrMode: {Module.Param.TransferObject.OCRMode.Value}"
                                                                 );
                                Loader.ResonOfError = $"{Module.Param.TransferObject.WaferType} Wafer(OriginHolder: FoupNumber:{foupNum}, SlotNumber:{slotNum}) PreAlign Failed.";
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }
                            LoggerManager.Debug("[WaitForOCR TimeOut =120s]");
                        }                        
                        else if (retValOcr == EventCodeEnum.NONE)
                        {
                            if (PA.Holder.TransferObject.NotchAngle.Value == PA.Holder.TransferObject.OCRAngle.Value)
                            {
                                //OCR을 실제로 시도 했고 Ocr Angle과 Loading Angle이 같을 경우 PreAlign이 완료되었다고 판단한다. 
                                PA.Holder.TransferObject.SetPreAlignDone(PA.ID);
                            }
                            else
                            {
                                PA.Holder.TransferObject.CleanPreAlignState(reason: $"Notch Angle({PA.Holder.TransferObject.NotchAngle.Value}) and Ocr Angle({PA.Holder.TransferObject.OCRAngle.Value}) is not same");
                            }


                        }
                        else
                        {
                            // 고려되지 않은 상황
                            LoggerManager.Debug($"GP_ReadCognexOCRState(): Failed. " +
                                                                $"WaferType: {Module.Param.TransferObject.WaferType.Value}\n" +
                                                                $"OCRReadState must be Done, Cur: {Module.Param.TransferObject.OCRReadState}\n" +
                                                                $"PreAlignState must be Done, Cur: {Module.Param.TransferObject.PreAlignState}\n" +
                                                                $"OcrMode: {Module.Param.TransferObject.OCRMode.Value}"
                                                                );
                            Loader.ResonOfError = $"{Module.Param.TransferObject.WaferType} Wafer(OriginHolder: FoupNumber:{foupNum}, SlotNumber:{slotNum}) OcrReadState and PreAlign State is Invalid.";
                            StateTransition(new SystemErrorState(Module));
                            return;
                        }

                    }



                    ///[1-2][ OCR Fail일 경우 Retry를 해서 OcrReadState를 완료로 만든다. Done/Abort ]
                    if (isOCRFail == true)
                    {
                        //Fail 인 경우 PAPutAsync에서 LoadingAngle로 맞춰주지 않고 ManualInput 창을 띄우기 위해서 무조건 PreAlignState를 Clear해준다. 
                        PA.Holder.TransferObject.CleanPreAlignState(reason: $"PA Fail:({retValOcr}).");

                        bool isEnableOCRTable = false;
                        if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            isEnableOCRTable = Loader.OCRConfig.Mode == OCR_OperateMode.NG_GO;
                        }


                        if (isEnableOCRTable)
                        {
                            if (Loader.LoaderMaster.ActiveLotInfos[foupNum - 1] != null && Loader.LoaderMaster.ActiveLotInfos[foupNum - 1].WaferIDInfo.Count >= slotNum)
                            {
                                if (Loader.LoaderMaster.ActiveLotInfos[foupNum - 1].WaferIDInfo[slotNum - 1].WaferID != "" && Loader.LoaderMaster.ActiveLotInfos[foupNum - 1].WaferIDInfo[slotNum - 1].WaferID != null)
                                {
                                    string waferID = Loader.LoaderMaster.ActiveLotInfos[foupNum - 1].WaferIDInfo[slotNum - 1].WaferID;
                                    Module.Param.TransferObject.SetOCRState(waferID, 0, OCRReadStateEnum.DONE);
                                    LoggerManager.Debug($"[OCR NG_GO Enable] State={OCRReadStateEnum.DONE}, ID: {waferID} , FoupNumber{foupNum}, SlotNumber{slotNum}");
                                    LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {waferID}, Score: ---, OCR Index: {PA.ID.Index}, Origin Location: {Loader.SlotToFoupConvert(PA.Holder.TransferObject.OriginHolder)} [NG_GO]");
                                }
                                else
                                {
                                    Module.Param.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                                    Module.Param.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                    LoggerManager.Debug($"[OCR NG_GO Error] Data is Empty. FoupNumber{foupNum}, SlotNumber{slotNum}");
                                }
                            }
                            else
                            {
                                Module.Param.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                                Module.Param.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                LoggerManager.Debug($"[OCR NG_GO Error] Data is Empty. FoupNumber{foupNum}, SlotNumber{slotNum}");
                            }
                        }
                        else
                        {
                            this.GetLoaderCommands().LoaderBuzzer(true);
                            if (this.Loader.OCRConfig.SkipWaferOcrFail)
                            {
                                SkipWafer(false);
                            }
                            else
                            {
                                CognexManualInput.Show(this.GetLoaderContainer(), PA.ID.Index - 1);
                                ICognexProcessManager cognexProcessManager = this.Loader.Container.Resolve<ICognexProcessManager>();
                                if (cognexProcessManager.GetManualOCRState(PA.ID.Index - 1))
                                {
                                    ocr = this.Loader.Container.Resolve<ICognexProcessManager>().Ocr[PA.ID.Index - 1];
                                    ocrScore = this.Loader.Container.Resolve<ICognexProcessManager>().OcrScore[PA.ID.Index - 1];

                                    PA.Holder.TransferObject.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);

                                    LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ManualDone, ID: {ocr}, Score: {0}, OCR Index: {PA.ID.Index}, Origin Location: {Loader.SlotToFoupConvert(PA.Holder.TransferObject.OriginHolder)}");

                                    LoggerManager.Debug($"[OCR Result Data] State=Manual_DONE, ID:{ocr}, Score: {0}  OCR Index:{PA.ID.Index} , Origin Location:{Loader.SlotToFoupConvert(PA.Holder.TransferObject.OriginHolder)}");
                                }
                                else
                                {
                                    this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, PA.ID.Index);
                                    PA.Holder.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                                    SkipWafer(true);
                                }
                            }

                        }
                    }


                    ///[1-3][ OCRReadState가 확정된 후에 동작 ]
                    if (PA.Holder.TransferObject.IsOCRDone())
                    {
                        Loader.LoaderMaster.OcrReadStateRisingEvent(PA.Holder.TransferObject, PA.ID);
                        if (PA.Holder.TransferObject.OCRReadState == OCRReadStateEnum.DONE &&
                            this.Loader.LoaderMaster.GetIsWaitForWaferIdConfirm())
                        {
                            PA.Holder.TransferObject.WFWaitFlag = true;
                            bool isvalid = this.Loader.LoaderMaster.WaferIdConfirm(PA.Holder.TransferObject, ocr);
                            if (!isvalid)
                            {
                                PA.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                //PA.Holder.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                            }
                        }

                        if (isOCRFail)//Fail이 아닌경우 PAPutAsync에서 PreAlign을 완료하기 때문에 현재 ProbModule에서는 Fail일때만 PreAlign해주면 된다.
                        {
                            ///[2-1][ Loading Angle 로 맞추는 동작 ] - Ocr Read Fail 일 경우 PreAlign 이 안되어있는 상태이고 Ocr Read Success 인경우 Ocr Angle과 LoadingAngle이 다를 경우 PreAlign 해야한다.                            
                            EventCodeEnum PAerrorCode = EventCodeEnum.UNDEFINED;
                            PAerrorCode = this.Loader.PAManager.PAModules[PA.ID.Index - 1].DoPreAlign(dstNotchAngle);
                            if (PAerrorCode != EventCodeEnum.NONE)
                            {
                                PA.Holder.TransferObject.WaferState = EnumWaferState.SKIPPED;
                                if (PAerrorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                              PAerrorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                {
                                    this.NotifyManager().Notify(PAerrorCode, PA.ID.Index);
                                }
                                LoggerManager.Debug($"GP_ReadCognexOCRState(): Failed. " +
                                                                 $"WaferType: {Module.Param.TransferObject.WaferType.Value}\n" +
                                                                 $"OCRReadState must be Done, Cur: {Module.Param.TransferObject.OCRReadState}\n" +
                                                                 $"PreAlignState must be Done, Cur: {Module.Param.TransferObject.PreAlignState}\n" +
                                                                 $"OcrMode: {Module.Param.TransferObject.OCRMode.Value}"
                                                                 );
                                Loader.ResonOfError = $"{Module.Param.TransferObject.WaferType} Wafer(OriginHolder: FoupNumber:{foupNum}, SlotNumber:{slotNum}) PreAlign Failed.";
                                StateTransition(new SystemErrorState(Module));
                                return;
                            }
                        }

                    }                    

                }
                



                ///[3-1][ PA Pick 하는 동작 ] - 다음 ProcModule에서 Pick을 할거기 때문에 여기에서 PAPutAsync의 PreAlign을 끝날때까지 기다려줘야한다.
                int waitForPreAlign = this.GetLoaderCommands().WaitForPA((IPreAlignModule)Module.Param.Curr);
                if (waitForPreAlign == 0 &&                    
                    Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort == false)
                {
                    PA.Holder.TransferObject.SetPreAlignDone(PA.ID);
                }
                else
                {
                    PA.Holder.TransferObject.CleanPreAlignState(reason: $"PreAlign Fail:({retValOcr}). waitForPreAlign:{waitForPreAlign},  PAAlignAbort:{Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort}");
                }

                ///[4-1][ 현재 ProcModule에서 해야하는 일을 다했는지 평가하는 동작 ]
                if (Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.DONE)
                {
                    StateTransition(new DoneState(Module));                    
                }
                else
                {                                       
                    Loader.ResonOfError = $"GP_ReadCognexOCRState(): Ocr or PreAlignState is Invalid. " +
                                                                $"WaferInfo: {Loader.SlotToFoupConvert(Module.Param.TransferObject.OriginHolder)}\n" +
                                                                $"OCRReadState must be Done, Cur: {Module.Param.TransferObject.OCRReadState}\n" +
                                                                $"PreAlignState must be Done, Cur: {Module.Param.TransferObject.PreAlignState}\n" +
                                                                $"OcrMode: {Module.Param.TransferObject.OCRMode.Value}";
                    LoggerManager.Debug(Loader.ResonOfError);
                    StateTransition(new SystemErrorState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                StateTransition(new SystemErrorState(Module));
            }
            
            return;
        }
    }
    public class DoneState : GP_ReadCognexOCRState
    {
        public DoneState(GP_ReadCognexOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_ReadCognexOCRState
    {
        public SystemErrorState(GP_ReadCognexOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
