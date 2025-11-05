using Autofac;
using LoaderBase;
using LogModule;
using ProberErrorCode;
using System;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using ProberInterfaces.Enum;
using LoaderBase.AttachModules.ModuleInterfaces;
using CognexOCRManualDialog;
using MetroDialogInterfaces;
using System.Threading;
using NotifyEventModule;
using ProberInterfaces.Event;

namespace LoaderCore.GP_PreAlignToARMStates
{
    public abstract class GP_PreAlignToARMState : LoaderProcStateBase
    {
        public GP_PreAlignToARM Module { get; set; }

        public GP_PreAlignToARMState(GP_PreAlignToARM module)
        {
            this.Module = module;
        }
        protected void StateTransition(GP_PreAlignToARMState stateObj)
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

        ILoaderModule loader;
        protected ILoaderModule Loader
        {
            get
            {
                if (loader == null)
                {
                    loader = Module.Container.Resolve<ILoaderModule>();
                }
                return loader;
            }
        }
        protected IPreAlignModule PA => Module.Param.Curr as IPreAlignModule;

        protected IARMModule ARM => Module.Param.Next as IARMModule;

        protected EventCodeEnum RaiseInvalidState([CallerMemberName] string memberName = "")
        {
            EventCodeEnum retVal;
            //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
            LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

            retVal = EventCodeEnum.LOADER_STATE_INVALID;
            return retVal;
        }

    }

    public class IdleState : GP_PreAlignToARMState
    {
        public IdleState(GP_PreAlignToARM module) : base(module)
        {
            try
            {
                Loader.ProcModuleInfo.ProcModule = LoaderProcModuleEnum.PREALIGN_TO_ARM;
                Loader.ProcModuleInfo.Source = PA.ID;
                Loader.ProcModuleInfo.Destnation = ARM.ID;
                Loader.ProcModuleInfo.Origin = PA.Holder.TransferObject.OriginHolder;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GP_PreAlignToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            StateTransition(new RunningState(Module));
        }
    }
    public class RunningState : GP_PreAlignToARMState
    {
        public RunningState(GP_PreAlignToARM module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_ARM, StateLogType.START, $"OriginHolder: {Loader.SlotToFoupConvert(PA.Holder.TransferObject.OriginHolder)}, Source: {PA}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.PREALIGN_TO_ARM, StateLogType.START, PA.ID.Label, ARM.ID.Label, PA.Holder.TransferObject.OriginHolder.Label);

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
                 
                    if(Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort == true)
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
                            LoggerManager.Debug("[WaitForOCR TimeOut =120s]");
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
                                LoggerManager.Debug($"GP_PreAlignToARMState(): Failed. " +
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
                            LoggerManager.Debug($"GP_PreAlignToARMState(): Failed. " +
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
                                    LoggerManager.Debug($"[OCR NG_GO Enable] State={OCRReadStateEnum.DONE}, ID: { Loader.LoaderMaster.ActiveLotInfos[foupNum - 1].WaferIDInfo[slotNum - 1].WaferID} , FoupNumber{foupNum}, SlotNumber{slotNum}");
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
                                LoggerManager.Debug($"GP_PreAlignToARMState(): Failed. " +
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



                ///[3-1][ PA Pick 하는 동작 ]
                
                int waitForPreAlign = this.GetLoaderCommands().WaitForPA((IPreAlignModule)Module.Param.Curr);
                if (waitForPreAlign == 0 && PA.Holder.TransferObject.PreAlignState != PreAlignStateEnum.SKIP &&
                    Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort == false)
                {
                    // PreAlignState -> Done
                    PA.Holder.TransferObject.SetPreAlignDone(PA.ID);
                }
                else if (PA.Holder.TransferObject.PreAlignState == PreAlignStateEnum.SKIP)
                {
                    // PreAlignState -> Skip
                    LoggerManager.Error($"GP_PreAlignToARMState(): The alignment was skipped because the state of PA is {PA.Holder.Status}.");
                }
                else
                {
                    // PreAlignState -> Nonezton
                    PA.Holder.TransferObject.CleanPreAlignState(reason: $"PreAlign Fail:({retValOcr}). waitForPreAlign:{waitForPreAlign},  PAAlignAbort:{Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort}");
                }

                

                if (Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.DONE || Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.SKIP)
                {
                    var result = this.Loader.SetTransferWaferSize(Module.Param.TransferObject, PA.Holder.Status);
                    if (result != EventCodeEnum.NONE)
                    {
                        // PA의 Wafer obj 에 정보 이상일 수 있음.
                        PA.Holder.SetUnknown();
                        Loader.ResonOfError = $"PA{PA.ID.Index} To ARM{ARM.ID.Index} Transfer failed.";
                    }
                    else 
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, PA.ID.Label, ARM.ID.Label, PA.Holder.TransferObject.OriginHolder.Label, PA.ID.Label, SubSequenceType.PA_PICK);
                        if (Module.Param.TransferObject.PreAlignState != PreAlignStateEnum.SKIP && Loader.PAManager.PAModules[PA.ID.Index - 1].State.PAAlignAbort == false) 
                        {
                            result = this.GetLoaderCommands().PAPick((IPreAlignModule)Module.Param.Curr, Module.Param.UseARM);
                        }
                        else
                        {
                            result = this.GetLoaderCommands().PAForcedPick((IPreAlignModule)Module.Param.Curr, Module.Param.UseARM);
                        }
                        
                        if (result == EventCodeEnum.NONE)
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, PA.ID.Label, ARM.ID.Label, PA.Holder.TransferObject.OriginHolder.Label, PA.ID.Label, SubSequenceType.PA_PICK);
                            PA.Holder.CurrentWaferInfo = PA.Holder.TransferObject;
                            PA.Holder.SetTransfered(ARM);
                        }
                        else
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, PA.ID.Label, ARM.ID.Label, PA.Holder.TransferObject.OriginHolder.Label, PA.ID.Label, SubSequenceType.PA_PICK, result.ToString());
                            var vacuumresult = ARM.MonitorForVacuum(true);
                            if (vacuumresult != EventCodeEnum.NONE)
                            {
                                bool isExist = false; //PA 베큠체크
                                vacuumresult  = Loader.GetLoaderCommands().CheckWaferIsOnPA(PA.ID.Index, out isExist);
                                if (vacuumresult == EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                                {
                                    if (isExist == false)
                                    {
                                        var clonedTransferObject = PA.Holder.TransferObject.Clone() as TransferObject;
                                        ARM.Holder.SetUnknown(clonedTransferObject);
                                        PA.Holder.SetUnknown(clonedTransferObject);
                                    }
                                    else
                                    {
                                        // 기존에 PA 에  Holder.Status 가 이미 EnumSubsStatus.EXIST
                                    }
                                }
                                else
                                {
                                    var clonedTransferObject = PA.Holder.TransferObject.Clone() as TransferObject;
                                    ARM.Holder.SetUnknown(clonedTransferObject);
                                    PA.Holder.SetUnknown(clonedTransferObject);
                                }
                            }
                            else
                            {
                                if (PA.Holder.Status == EnumSubsStatus.EXIST)
                                {
                                    PA.Holder.SetTransfered(ARM);
                                }
                            }

                            Loader.BroadcastLoaderInfo();
                            LoggerManager.Error($"GP_PreAlignToARMState(): Transfer failed. Job result = {result} && Vacuum result {vacuumresult}");
                            Loader.ResonOfError = $"PreAlign{PA.ID.Index} To ARM{ARM.ID.Index} Transfer failed. {Environment.NewLine} Job result = {result}";
                            StateTransition(new SystemErrorState(Module));
                            Loader.ResonOfError = $"GP_PreAlignToARMState(): {result}. " +
                                $"WaferInfo: {Loader.SlotToFoupConvert(Module.Param.TransferObject.OriginHolder)}\n" +
                                $"OCRReadState must be Done, Cur: {Module.Param.TransferObject.OCRReadState}\n" +
                                $"PreAlignState must be Done, Cur: {Module.Param.TransferObject.PreAlignState}\n" +
                                $"OcrMode: {Module.Param.TransferObject.OCRMode.Value}";
                            return;
                        }
                    }
                    Loader.BroadcastLoaderInfo();
                }
                else
                {
                    //Do Nothing
                }

                ///[4-1][ 현재 ProcModule에서 해야하는 일을 다했는지 평가하는 동작 ]
                if (Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.DONE
                    && ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    StateTransition(new DoneState(Module));
                }
                else if (Module.Param.TransferObject.PreAlignState == PreAlignStateEnum.SKIP)
                {
                    // todo ann : 로그 바꾸세요
                    LoggerManager.Error($"GP_PreAlignToARMState(): The alignment was skipped because the state of PA is {ARM.Holder.Status}.");
                    StateTransition(new DoneState(Module));
                }
                else
                {
                    Loader.ResonOfError = $"GP_PreAlignToARMState(): Ocr or PreAlignState is Invalid. " +
                                                                 $"WaferInfo: {Loader.SlotToFoupConvert(Module.Param.TransferObject.OriginHolder)}\n" +
                                                                 $"OCRReadState must be Done, Cur: {Module.Param.TransferObject.OCRReadState}\n" +
                                                                 $"PreAlignState must be Done, Cur: {Module.Param.TransferObject.PreAlignState}\n" +
                                                                 $"OcrMode: {Module.Param.TransferObject.OCRMode.Value}";
                    LoggerManager.Debug(Loader.ResonOfError);

                    StateTransition(new SystemErrorState(Module));
                    return;
                }
            }
            catch (Exception err)
            {
                TransferObject clonedTransferObject = null;
                if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = ARM.Holder.TransferObject.Clone() as TransferObject;
                }
                else if (PA.Holder.Status == EnumSubsStatus.EXIST)
                {
                    clonedTransferObject = PA.Holder.TransferObject.Clone() as TransferObject;
                }

                var result = ARM.MonitorForVacuum(true);
                if (result != EventCodeEnum.NONE)
                {
                    bool isExist = false; //PA 베큠체크
                    result = Loader.GetLoaderCommands().CheckWaferIsOnPA(PA.ID.Index, out isExist);
                    if (result == EventCodeEnum.NONE) // arm에 웨이퍼가 없을 경우
                    {
                        if (isExist == false)
                        {
                            ARM.Holder.SetUnknown(clonedTransferObject);
                            PA.Holder.SetUnknown(clonedTransferObject);
                        }
                        else
                        {
                            if (ARM.Holder.Status == EnumSubsStatus.EXIST)
                            {
                                ARM.Holder.SetTransfered(PA);
                            }
                        }
                    }
                    else
                    {
                        ARM.Holder.SetUnknown(clonedTransferObject);
                        PA.Holder.SetUnknown(clonedTransferObject);
                    }
                }
                else
                {
                    if (PA.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        PA.Holder.SetTransfered(ARM);
                    }
                }

                Loader.BroadcastLoaderInfo();
                LoggerManager.Error($"GP_PreAlignToARMState(): Exception occurred. Err = {err.Message}, Curr. state = {State}");
                StateTransition(new SystemErrorState(Module));
            }
        
        }
    }
    public class DoneState : GP_PreAlignToARMState
    {
        public DoneState(GP_PreAlignToARM module) : base(module)
        {
            LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_ARM, StateLogType.DONE, $"OriginHolder: {Loader.SlotToFoupConvert(ARM.Holder.TransferObject.OriginHolder)}, Source: {PA}, DestinationHolder: {ARM}");
            LoggerManager.UpdateLoaderMapHolder(ModuleLogType.PREALIGN_TO_ARM, StateLogType.DONE, PA.ID.Label, ARM.ID.Label, ARM.Holder.TransferObject.OriginHolder.Label);
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { }

    }
    public class SystemErrorState : GP_PreAlignToARMState
    {
        public SystemErrorState(GP_PreAlignToARM module) : base(module)
        {

            try
            {
                TransferObject transObj = null;
                
                if (ARM.Holder.TransferObject != null)
                {
                    transObj = ARM.Holder.TransferObject;
                }
                else
                {
                    transObj = PA.Holder.TransferObject;
                }
                EventCodeEnum errorcode = EventCodeEnum.LOADER_PREALIGN_TO_ARM_TRANSFER_ERROR;
                this.NotifyManager().Notify(errorcode);
                LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_ARM, StateLogType.ERROR, $"OriginHolder: {Loader.SlotToFoupConvert(transObj.OriginHolder)}, Source: {PA}, DestinationHolder: {ARM}");
                LoggerManager.UpdateLoaderMapHolder(ModuleLogType.PREALIGN_TO_ARM, StateLogType.ERROR, PA.ID.Label, ARM.ID.Label, transObj.OriginHolder.Label, errMsg: errorcode.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
