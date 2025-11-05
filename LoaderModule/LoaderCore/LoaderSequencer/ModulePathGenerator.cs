using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enum;

namespace LoaderCore
{
    public class ModulePathGenerator : INotifyPropertyChanged, IModulePathGenerator
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private int maxIterationLimit = 10;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private PathStateBase _CurrPathState;
        public PathStateBase CurrPathState
        {
            get { return _CurrPathState; }
            set
            {
                if (value != _CurrPathState)
                {
                    _CurrPathState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _SubsToTransfer;
        public TransferObject SubsToTransfer
        {
            get { return _SubsToTransfer; }
            set
            {
                if (value != _SubsToTransfer)
                {
                    _SubsToTransfer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private TransferObject _StagedTransferObject;
        public TransferObject StagedTransferObject
        {
            get { return _StagedTransferObject; }
            set
            {
                if (value != _StagedTransferObject)
                {
                    _StagedTransferObject = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ModulePathGenerator(TransferObject substotransfer)
        {
            SubsToTransfer = substotransfer;

            switch (SubsToTransfer.CurrHolder.ModuleType)
            {
                case ModuleTypeEnum.UNDEFINED:
                    break;
                case ModuleTypeEnum.CST:
                    CurrPathState = new OnCSTState(this);
                    break;
                case ModuleTypeEnum.SCANSENSOR:
                    break;
                case ModuleTypeEnum.SCANCAM:
                    break;
                case ModuleTypeEnum.SLOT:
                    CurrPathState = new OnSLOTState(this);
                    break;
                case ModuleTypeEnum.ARM:
                    CurrPathState = new OnARMState(this);
                    break;
                case ModuleTypeEnum.PA:
                    CurrPathState = new OnPAState(this);
                    break;
                case ModuleTypeEnum.FIXEDTRAY:
                    CurrPathState = new OnFIXEDTRAYState(this);
                    break;
                case ModuleTypeEnum.INSPECTIONTRAY:
                    CurrPathState = new OnINSPECTIONTRAYState(this);
                    break;
                case ModuleTypeEnum.CHUCK:
                    CurrPathState = new OnCHUCKState(this);
                    break;
                case ModuleTypeEnum.SEMICSOCR:
                    CurrPathState = new OnSEMICSOCRState(this);
                    break;
                case ModuleTypeEnum.COGNEXOCR:
                    CurrPathState = new OnCOGNEXOCRState(this);
                    break;
                case ModuleTypeEnum.BUFFER:
                    CurrPathState = new OnBUFFERState(this);
                    break;
                case ModuleTypeEnum.CARDBUFFER:
                    CurrPathState = new ONCardBufferState(this);
                    break;
                case ModuleTypeEnum.CC:
                    CurrPathState = new OnCCState(this);
                    break;
                case ModuleTypeEnum.CARDARM:
                    CurrPathState = new ONCardArmState(this);
                    break;
                case ModuleTypeEnum.CARDTRAY:
                    CurrPathState = new ONCardTrayState(this);
                    break;
                default:
                    break;
            }
        }
        private void StateTransition(ModuleTypeEnum module)
        {

            switch (module)
            {
                case ModuleTypeEnum.UNDEFINED:
                    break;
                case ModuleTypeEnum.CST:
                    CurrPathState = new OnCSTState(this);
                    break;
                case ModuleTypeEnum.SCANSENSOR:
                    break;
                case ModuleTypeEnum.SCANCAM:
                    break;
                case ModuleTypeEnum.SLOT:
                    CurrPathState = new OnSLOTState(this);
                    break;
                case ModuleTypeEnum.ARM:
                    CurrPathState = new OnARMState(this);
                    break;
                case ModuleTypeEnum.PA:
                    CurrPathState = new OnPAState(this);
                    break;
                case ModuleTypeEnum.FIXEDTRAY:
                    CurrPathState = new OnFIXEDTRAYState(this);
                    break;
                case ModuleTypeEnum.INSPECTIONTRAY:
                    CurrPathState = new OnINSPECTIONTRAYState(this);
                    break;
                case ModuleTypeEnum.CHUCK:
                    CurrPathState = new OnCHUCKState(this);
                    break;
                case ModuleTypeEnum.SEMICSOCR:
                    CurrPathState = new OnSEMICSOCRState(this);
                    break;
                case ModuleTypeEnum.COGNEXOCR:
                    CurrPathState = new OnCOGNEXOCRState(this);
                    break;
                case ModuleTypeEnum.BUFFER:
                    CurrPathState = new OnBUFFERState(this);
                    break;
                case ModuleTypeEnum.CARDBUFFER:
                    CurrPathState = new ONCardBufferState(this);
                    break;
                case ModuleTypeEnum.CC:
                    CurrPathState = new OnCCState(this);
                    break;
                case ModuleTypeEnum.CARDARM:
                    CurrPathState = new ONCardArmState(this);
                    break;
                case ModuleTypeEnum.CARDTRAY:
                    CurrPathState = new ONCardTrayState(this);
                    break;
                default:
                    break;
            }
        }
        public List<ModuleTypeEnum> GetPath(TransferObject dst)
        {
            string sOrigin = string.Empty;
            if (SubsToTransfer.CurrHolder.Label != SubsToTransfer.OriginHolder.Label && dst.CurrHolder.Label != SubsToTransfer.OriginHolder.Label)
            {
                sOrigin = $"(org:{SubsToTransfer.OriginHolder.Label})";
            }
            LoggerManager.LoaderMapLog($"Init Path {SubsToTransfer.CurrHolder.ModuleType}{SubsToTransfer.CurrHolder.Index}{sOrigin} to {dst.CurrHolder.ModuleType}{dst.CurrHolder.Index} Start");

            List<ModuleTypeEnum> moduleSteps = new List<ModuleTypeEnum>();
            List<TransferObject> transferObjects = new List<TransferObject>();
            TransferObject interModuleTO;
            try
            {
                interModuleTO = (TransferObject)SubsToTransfer.Clone();
                StagedTransferObject = (TransferObject)SubsToTransfer.Clone();
                moduleSteps.Add(interModuleTO.CurrHolder.ModuleType);

                bool condition = true;

                while (condition)
                {
                    var iterModuleType = CurrPathState.MoveTo(dst);
                    if (iterModuleType != ModuleTypeEnum.INVALID)
                    {
                        interModuleTO.PrevHolder = interModuleTO.CurrHolder;
                        interModuleTO.CurrHolder = new ModuleID() { ModuleType = iterModuleType };
                        interModuleTO.OCRReadState = StagedTransferObject.OCRReadState;
                        interModuleTO.PreAlignState = StagedTransferObject.PreAlignState;
                        transferObjects.Add((TransferObject)interModuleTO.Clone());
                        StagedTransferObject = (TransferObject)transferObjects.Last().Clone();
                        moduleSteps.Add(iterModuleType);
                        StateTransition(iterModuleType);
                    }
                    if (moduleSteps.Count > maxIterationLimit)
                    {
                        throw new SystemException($"Path generation failed. Too much iterations. Interation = {moduleSteps.Count}, Source = {SubsToTransfer.CurrHolder.ModuleType}, Destination = {dst.CurrHolder.ModuleType}.");
                    }

                    if (interModuleTO.CurrHolder.ModuleType == dst.CurrHolder.ModuleType)
                    {
                        if (dst.CurrHolder.ModuleType == ModuleTypeEnum.ARM && interModuleTO.OCRReadState == OCRReadStateEnum.NONE)
                        {

                        }
                        else
                        {
                            condition = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error occurred while retreive path to destination({dst.CurrHolder.Label}), msg={err.Message}", isLoaderMap: true);

            }
            int pathCount = 0;
            LoggerManager.Debug($"Path to {dst.CurrHolder.ModuleType} from {SubsToTransfer.CurrHolder.ModuleType}");
            foreach (var item in moduleSteps)
            {
                pathCount++;
                LoggerManager.Debug($"Path[#{pathCount}]: {item}", isLoaderMap: true);
            }

            LoggerManager.LoaderMapLog($"Init Path {SubsToTransfer.CurrHolder.ModuleType}{SubsToTransfer.CurrHolder.Index}{sOrigin} to {dst.CurrHolder.ModuleType}{dst.CurrHolder.Index} End");
            return moduleSteps;
        }
        #region // On Module States
        public class OnCSTState : PathStateBase
        {
            public OnCSTState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.CST;
            }
        }
        public class OnSLOTState : PathStateBase
        {
            public OnSLOTState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.SLOT;
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;

                nextModule = ModuleTypeEnum.ARM;
                return nextModule;
            }
        }
        public class OnARMState : PathStateBase
        {
            public OnARMState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                if (Module.SubsToTransfer.NotchAngle.Value != dest.NotchAngle.Value)
                {
                    Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.DONE;
                    nextModule = ModuleTypeEnum.PA;
                }
                else if (Module.StagedTransferObject.PreAlignState == PreAlignStateEnum.DONE && Module.StagedTransferObject.OCRReadState == OCRReadStateEnum.ABORT)
                {
                    nextModule = dest.CurrHolder.ModuleType;
                }
                //PW Load시 buffer 사용 하지 않도록
                else if (Module.StagedTransferObject.PreAlignState == PreAlignStateEnum.DONE &&
                    Module.StagedTransferObject.WaferType.Value == EnumWaferType.POLISH)
                {
                    nextModule = dest.CurrHolder.ModuleType;
                }
                else if ((Module.StagedTransferObject.PreAlignState != PreAlignStateEnum.DONE |
                    Module.StagedTransferObject.OCRReadState != OCRReadStateEnum.DONE) &
                    (Module.StagedTransferObject.WaferState == EnumWaferState.UNPROCESSED |
                    Module.StagedTransferObject.WaferState == EnumWaferState.READY | Module.StagedTransferObject.WaferType.Value != EnumWaferType.STANDARD))
                {
                    Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.DONE;
                    nextModule = ModuleTypeEnum.PA;
                }
                else if (Module.StagedTransferObject.PrevHolder.ModuleType != ModuleTypeEnum.BUFFER &
                    (Module.SubsToTransfer.WaferState != EnumWaferState.PROCESSED
                    && Module.SubsToTransfer.WaferState != EnumWaferState.SKIPPED
                    && Module.SubsToTransfer.WaferState != EnumWaferState.SOAKINGSUSPEND
                    && Module.SubsToTransfer.WaferState != EnumWaferState.SOAKINGDONE))
                {
                    nextModule = ModuleTypeEnum.BUFFER;
                }
                else
                {
                    if (dest.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
                    {
                        if (Module.SubsToTransfer.NotchAngle.Value != dest.NotchAngle.Value
                            | Module.StagedTransferObject.PreAlignState != PreAlignStateEnum.DONE)
                        {
                            Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.DONE;
                            nextModule = ModuleTypeEnum.PA; // Run notch align for unloading
                        }
                        else
                        {
                            nextModule = dest.CurrHolder.ModuleType;
                        }
                    }
                }

                Module.SubsToTransfer.NotchAngle.Value = dest.NotchAngle.Value;

                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.ARM;
            }
        }
        public class OnPAState : PathStateBase
        {
            public OnPAState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.DONE;
                Module.StagedTransferObject.OCRReadState = OCRReadStateEnum.DONE;
                Module.StagedTransferObject.NotchAngle.Value = dest.NotchAngle.Value;
                nextModule = ModuleTypeEnum.ARM;
                //ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                //Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.DONE;
                //Module.StagedTransferObject.NotchAngle.Value = dest.NotchAngle.Value;

                //if (Module.StagedTransferObject.OCRReadState != OCRReadStateEnum.DONE && Module.StagedTransferObject.OCRMode.Value == OCRModeEnum.READ)
                //{
                //    //ocr을 읽어야 하는 경우
                //    if (Module.StagedTransferObject.OCRType.Value == OCRTypeEnum.COGNEX)
                //    {
                //        nextModule = ModuleTypeEnum.COGNEXOCR;
                //    }
                //    else if (Module.StagedTransferObject.OCRType.Value == OCRTypeEnum.SEMICS)
                //    {
                //        nextModule = ModuleTypeEnum.SEMICSOCR;
                //    }
                //}
                //else
                //{
                //    nextModule = ModuleTypeEnum.ARM;
                //}
                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.PA;
            }
        }
        public class OnFIXEDTRAYState : PathStateBase
        {
            public OnFIXEDTRAYState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.FIXEDTRAY;
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.NONE;
                nextModule = ModuleTypeEnum.ARM;
                return nextModule;
            }
        }
        public class OnINSPECTIONTRAYState : PathStateBase
        {
            public OnINSPECTIONTRAYState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.INSPECTIONTRAY;
            }
        }
        public class OnCHUCKState : PathStateBase
        {
            public OnCHUCKState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.CHUCK;
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                Module.SubsToTransfer.WaferState = EnumWaferState.PROCESSED;
                if (Module.StagedTransferObject.WaferType.Value != EnumWaferType.STANDARD)
                {
                    Module.StagedTransferObject.PreAlignState = PreAlignStateEnum.NONE;
                }
                if (dest.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
                {
                    nextModule = ModuleTypeEnum.ARM;
                }
                return nextModule;
            }
        }

        public class OnCCState : PathStateBase
        {
            public OnCCState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                nextModule = ModuleTypeEnum.CARDARM;

                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.CC;
            }
        }

        public class ONCardTrayState : PathStateBase
        {
            public ONCardTrayState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                nextModule = ModuleTypeEnum.CARDARM;

                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.CARDTRAY;
            }
        }
        public class ONCardBufferState : PathStateBase
        {
            public ONCardBufferState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                nextModule = ModuleTypeEnum.CARDARM;

                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.CARDBUFFER;
            }
        }

        public class ONCardArmState : PathStateBase
        {
            public ONCardArmState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;

                nextModule = dest.CurrHolder.ModuleType;
                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.CARDARM;
            }
        }
        public class OnSEMICSOCRState : PathStateBase
        {
            public OnSEMICSOCRState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.SEMICSOCR;
            }
        }
        public class OnCOGNEXOCRState : PathStateBase
        {
            public OnCOGNEXOCRState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum MoveTo(TransferObject dest)
            {
                ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
                Module.StagedTransferObject.OCRReadState = OCRReadStateEnum.DONE;
                nextModule = ModuleTypeEnum.PA;
                return nextModule;
            }
            public override ModuleTypeEnum GetState()
            {

                return ModuleTypeEnum.COGNEXOCR;
            }
        }
        public class OnBUFFERState : PathStateBase
        {
            public OnBUFFERState(ModulePathGenerator module) : base(module)
            {
            }
            public override ModuleTypeEnum GetState()
            {
                return ModuleTypeEnum.BUFFER;
            }
        }

        #endregion
    }
    public abstract class PathStateBase : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private IModulePathGenerator _Module;
        public IModulePathGenerator Module
        {
            get { return _Module; }
            set
            {
                if (value != _Module)
                {
                    _Module = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleID _Holder;
        public ModuleID Holder
        {
            get { return _Holder; }
            set
            {
                if (value != _Holder)
                {
                    _Holder = value;
                    RaisePropertyChanged();
                }
            }
        }
        public virtual ModuleTypeEnum MoveTo(TransferObject dest)
        {
            ModuleTypeEnum nextModule = ModuleTypeEnum.INVALID;
            if (dest.CurrHolder.ModuleType != ModuleTypeEnum.ARM)
            {
                nextModule = ModuleTypeEnum.ARM;
            }
            return nextModule;
        }
        public ModuleTypeEnum GetCurrentHolderType()
        {
            return Holder.ModuleType;
        }
        public PathStateBase(IModulePathGenerator module)
        {
            Module = module;
        }
        public abstract ModuleTypeEnum GetState();
    }

}
