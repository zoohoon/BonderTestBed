using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberInterfaces.Foup;
using ProberErrorCode;
using LogModule;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoaderCore
{
    internal class CassetteModule : AttachedModuleBase, ICassetteModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private CassetteScanStateBase ScanStateObj;

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.CST;

        public CassetteDefinition Definition { get; set; }

        public string HashCode { get; set; } = "";
        public string FoupID { get; set; }
        private object lockobj = new object();
        public CassetteDevice Device { get; set; }

        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override void DeInitModule()
        {

        }

        public EventCodeEnum SetDefinition(CassetteDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                ScanStateTransition(new CassetteNoReadState(this));

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetDevice(CassetteDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Device = device;

                if (string.IsNullOrEmpty(device.Label.Value) == false)
                    this.ID = ModuleID.Create(ID.ModuleType, ID.Index, device.Label.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SetNoReadScanState();

                    Initialized = false;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public CassetteSlot1AccessParam GetSlot1AccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            CassetteSlot1AccessParam param = null;

            try
            {
                param = Definition.Slot1AccessParams
               .Where(
               item =>
               item.SubstrateType.Value == type &&
               item.SubstrateSize.Value == size
               ).FirstOrDefault();

                if (param == null)
                {
                    LoggerManager.Debug($"[CassetteModule], GetSlot1AccessParam(), Parameter is not matched. Type = {type}, Size = {size}");

                    foreach (var item in Definition.Slot1AccessParams.Select((value, index) => new { Value = value, Index = index }))
                    {
                        CassetteSlot1AccessParam currentValue = item.Value;
                        int currentIndex = item.Index;

                        LoggerManager.Error($"[CassetteModule], GetSlot1AccessParam(), CassetteSlot1AccessParam[{currentIndex}], Type : {currentValue.SubstrateType}, Size = {currentValue.SubstrateSize}");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return param;
        }

        public void ScanStateTransition(CassetteScanStateBase stateObj)
        {
            try
            {
                ScanStateObj = stateObj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public CassetteScanStateEnum ScanState => ScanStateObj.ScanState;

        public FoupStateEnum FoupState { get; set; }
        public FoupCoverStateEnum FoupCoverState { get; set; }        

        public LotModeEnum LotMode { get; set; }

        public void SetNoReadScanState()
        {
            try
            {
                ScanStateObj.SetNoReadScanState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetReadingScanState()
        {
            try
            {
                ScanStateObj.SetReadingScanState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic)
        {
            try
            {
                ScanStateObj.SetScanResult(scanRelDic);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetIllegalScanState()
        {
            try
            {
                ScanStateObj.SetIllegalScanState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetReservedScanState()
        {
            try
            {
                ScanStateObj.SetReservedScanState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsInFoup(int foupNumber)
        {
            bool retval = false;

            try
            {
                retval = ID.Index == foupNumber;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetFoupState(FoupStateEnum state)
        {
            try
            {
                FoupStateEnum preFoupState = FoupState;
                FoupState = state;
                LoggerManager.Debug($"[CassetteModule] Index : {ID},Pre Foup State : {preFoupState}, Cur Foup State : {FoupState}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //if(state==FoupStateEnum.LOAD)
            //{
            //    if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
            //    {
            //        if (HashCode != "" && HashCode != null)
            //            return;
            //    }
            //}
            //else
            //{
            //    HashCode = "";
            //}
        }
        public void SetFoupCoverState(FoupCoverStateEnum state)
        {
            try
            {
                FoupCoverStateEnum preFoupState = FoupCoverState;
                FoupCoverState = state;
                LoggerManager.Debug($"[CassetteModule] Index : {ID},Pre Foup cover State : {preFoupState}, Cur Foup cover State : {FoupCoverState}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// Cassette Module 의 Hash Code Data 를 생성 , 삭제 하는 함수 
        /// isAttached : true 인 경우 새로운 Hash Code 를 생성하고, false 인 경우 Hash Code 데이터를 없앤다. 
        /// </summary>
        public void SetHashCode(bool isAttached)
        {
            try
            {
                if (isAttached)
                {
                    Object obj = new Object();
                    HashCode = obj.GetHashCode().ToString();
                    LoggerManager.Debug($"[CassetteModule] Create Foup Number : {ID}, HashCode {HashCode}");
                }
                else
                {
                    HashCode = "";
                    LoggerManager.Debug($"[CassetteModule] Delete Foup Number : {ID}, HashCode empty");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCarrierId(string carrierid)
        {
            FoupID = carrierid;
            LoggerManager.Debug($"SetFoupState(): FoupNum:{ID.Index}, carrierid:{carrierid}");
        }        
        
        public object GetLockObj()
        {
             
            try
            {
                if (lockobj == null)
                {
                    lockobj = new object();
                }                    
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lockobj;
        }
    }

    internal abstract class CassetteScanStateBase
    {
        public CassetteModule Module { get; private set; }

        public CassetteScanStateBase(CassetteModule cassette)
        {
            this.Module = cassette;
        }

        public abstract CassetteScanStateEnum ScanState { get; }

        public abstract void SetNoReadScanState();

        public abstract void SetReadingScanState();

        public abstract void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic);

        public abstract void SetIllegalScanState();
        public abstract void SetReservedScanState();

    }

    internal class CassetteNoReadState : CassetteScanStateBase
    {
        public CassetteNoReadState(CassetteModule module) : base(module) { }

        public override CassetteScanStateEnum ScanState => CassetteScanStateEnum.NONE;

        public override void SetIllegalScanState()
        {
            //TODO : invalid call
        }

        public override void SetReadingScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                foreach (var slot in slots)
                {
                    slot.Holder.SetUndefined();
                }

                Module.ScanStateTransition(new CassetteReadingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetNoReadScanState()
        {
            //TODO : invalid call
        }

        public override void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic)
        {
            //TODO : invalid call
            throw new Exception("Debug test");
        }
        public override void SetReservedScanState()
        {
            Module.ScanStateTransition(new CassetteReservedState(Module));
        }
    }

    internal class CassetteReadingState : CassetteScanStateBase
    {
        public CassetteReadingState(CassetteModule module) : base(module) 
        {
            try
            {
                // TODO : LOT 도중 같은 Cassette 면 데이터 날리지 않도록 추가 수정 필요
                // Slot 정보가 달려졌을 때는, 추가된 Slot 에 대해서는 Scan 후 처리 필요.
                // LOT 상황에서 LOT 도중 Cassette 를 Scan 했을 때는 추가된 Slot 은 SKIPPED 처리 해야함.
                // LOT 아니면 무조건 날려도됨
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                int i = 0;
                foreach (var slot in slots)
                {
                    slot.Holder.SetUndefined();
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferStatus = slot.Holder.Status;
                        Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferObj = slot.Holder.TransferObject;
                    }
                    i++;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
           
        }

        public override CassetteScanStateEnum ScanState => CassetteScanStateEnum.READING;

        public override void SetIllegalScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                foreach (var slot in slots)
                {
                    slot.Holder.SetUnknown();
                }

                Module.ScanStateTransition(new CassetteIllegalState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetReadingScanState()
        {
            //TODO : invalid call
        }

        public override void SetNoReadScanState()
        {
            //TODO : invalid call
        }

        public override void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic)
        {
            try
            {
                int errorCount = 0;
                int slotDetectNum = -1;
                int slotDetectCnt = 0;
                for (int i = 0; i < scanRelDic.Count; i++)
                {
                    if (scanRelDic.ElementAt(i).Value == SlotScanStateEnum.NOT_DETECTED)
                    {
                        scanRelDic.ElementAt(i).Key.Holder.SetUnload();
                        Module.Loader.PrevSlotInfo[i].Holder.SetUnload();
                    }
                    else if (scanRelDic.ElementAt(i).Value == SlotScanStateEnum.DETECTED)
                    {
                        slotDetectCnt++;
                        slotDetectNum = i + 1;
                        if (Module.Loader.PrevSlotInfo[i].Holder.Status == EnumSubsStatus.EXIST)
                        {
                            //  Module.Loader.PrevSlotInfo[i].Holder.TransferObject.WaferState
                            //scanRelDic.ElementAt(i).Key.Holder = Module.Loader.PrevSlotInfo[i].Holder;
                        }
                        else
                        {
                            scanRelDic.ElementAt(i).Key.Holder.SetUnload();
                            scanRelDic.ElementAt(i).Key.Holder.SetAllocate();
                            //if(i==2) //TODO TEST REMOVE!!!Lloyd
                            //{
                            //    scanRelDic.ElementAt(i).Key.Holder.TransferObject.WaferType.Value = EnumWaferType.POLISH;
                            //    scanRelDic.ElementAt(i).Key.Holder.TransferObject.PolishWaferInfo = new PolishWaferInformation();
                            //    scanRelDic.ElementAt(i).Key.Holder.TransferObject.PolishWaferInfo.DefineName.Value = "POLISH1";
                            //}
                        }
                        //이전에 스캔 데이터 비교해서 넣어주기.
                    }
                    else if (scanRelDic.ElementAt(i).Value == SlotScanStateEnum.UNKNOWN)
                    {
                        slotDetectCnt++;
                        slotDetectNum = i + 1;
                        scanRelDic.ElementAt(i).Key.Holder.SetUnknown();
                        errorCount++;
                    }
                    else
                    {
                        scanRelDic.ElementAt(i).Key.Holder.SetUndefined();
                        errorCount++;
                    }

                    if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        Module.Loader.Foups[Module.ID.Index - 1].Slots[scanRelDic.Count - 1 - i].WaferStatus = scanRelDic.ElementAt(i).Key.Holder.Status;
                        Module.Loader.Foups[Module.ID.Index - 1].Slots[scanRelDic.Count - 1 - i].WaferObj = scanRelDic.ElementAt(i).Key.Holder.TransferObject;
                    }
                }





                //foreach (var item in scanRelDic)
                //{
                //    if (item.Value == SlotScanStateEnum.NOT_DETECTED)
                //    {
                //        item.Key.Holder.SetUnload();
                //    }
                //    else if (item.Value == SlotScanStateEnum.DETECTED)
                //    {
                //        item.Key.Holder.SetAllocate();
                //        Module.Loader
                //        //이전에 스캔 데이터 비교해서 넣어주기.
                //    }
                //    else
                //    {
                //        item.Key.Holder.SetUndefined();
                //        errorCount++;
                //    }
                //}
                
                if (Module.Loader.LoaderOption.OptionFlag)
                {
                    if (slotDetectCnt == 1)
                    {
                        if (Module.Loader.LoaderOption.IsScanValidate(slotDetectNum))
                        {
                            //string dirPath = @"C:\ProberSystem\SCANTEST.txt";
                            //if (Directory.Exists(Path.GetDirectoryName(dirPath)) == false)
                            //{
                            //    Directory.CreateDirectory(Path.GetDirectoryName(dirPath));
                            //}
                            //if (!File.Exists(dirPath))
                            //{
                            //    // Create a file to write to.
                            //    using (StreamWriter sw = File.CreateText(dirPath))
                            //    {
                            //        sw.WriteLine("----SCANTEST----");
                            //    }
                            //}
                            int dectcnt = scanRelDic.Count(item => item.Value == SlotScanStateEnum.DETECTED);
                            if (dectcnt == 1)
                            {
                                //using (StreamWriter sw = File.AppendText(dirPath))
                                //{
                                //    sw.WriteLine(slotDetectNum + "Slot DETECT [" + DateTime.Now + "]");
                                //}
                                scanRelDic.ElementAt(slotDetectNum - 1).Key.Holder.SetAllocate();
                                Module.Loader.ServiceCallback.CSTInfoChanged(Module.Loader.GetLoaderInfo());
                                Module.ScanStateTransition(new CassetteReadState(Module));
                            }
                            else
                            {
                               
                                //using (StreamWriter sw = File.AppendText(dirPath))
                                //{
                                //    sw.WriteLine(slotDetectNum+ "Slot UNKWON [" + DateTime.Now + "]");
                                //}
                                scanRelDic.ElementAt(slotDetectNum - 1).Key.Holder.SetAllocate();
                                Module.Loader.ServiceCallback.CSTInfoChanged(Module.Loader.GetLoaderInfo());
                                Module.ScanStateTransition(new CassetteReadState(Module));
                            }
                        }
                        else
                        {
                            Module.Loader.NotifyManager.Notify(EventCodeEnum.FOUP_SCAN_NOTDETECT, Module.ID.Index);
                            LoggerManager.Debug("[LOADER TEST OPTION ERROR] SlotNumber NotMatched SlotDectNum=" + slotDetectNum + ",OptionSlotNumber="+Module.Loader.LoaderOption.ScanSlotNum);
                            Module.ScanStateTransition(new CassetteIllegalState(Module));
                        }
                    }
                    else
                    {
                        Module.Loader.NotifyManager.Notify(EventCodeEnum.FOUP_SCAN_WAFEROUT, Module.ID.Index);
                        LoggerManager.Debug("[LOADER TEST OPTION ERROR] Scan Detect Count More than 1  SCAN DETECT COUNT =" + slotDetectCnt + "");
                        Module.ScanStateTransition(new CassetteIllegalState(Module));
                    }
                }
                else if (errorCount == 0)
                {
                    if (Module.Loader.ServiceCallback != null)
                    {
                        Module.Loader.ServiceCallback.CSTInfoChanged(Module.Loader.GetLoaderInfo());
                    }
                    Module.ScanStateTransition(new CassetteReadState(Module));
                }
                else
                {
                    Module.Loader.NotifyManager.Notify(EventCodeEnum.FOUP_SCAN_WAFEROUT, Module.ID.Index);
                    Module.ScanStateTransition(new CassetteIllegalState(Module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetReservedScanState()
        {
            //TODO : invalid call
        }
    }

    internal class CassetteReadState : CassetteScanStateBase
    {
        public CassetteReadState(CassetteModule module) : base(module) { }

        public override CassetteScanStateEnum ScanState => CassetteScanStateEnum.READ;

        public override void SetIllegalScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                foreach (var slot in slots)
                {
                    slot.Holder.SetUnknown();
                }

                Module.ScanStateTransition(new CassetteIllegalState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetReadingScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                foreach (var slot in slots)
                {
                    slot.Holder.SetUnknown();
                }

                Module.ScanStateTransition(new CassetteReadingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetNoReadScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                int i = 0;
                foreach (var slot in slots)
                {
                    slot.Holder.SetUndefined();
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferStatus = slot.Holder.Status;
                        Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferObj = slot.Holder.TransferObject;
                    }
                    i++;
                }

                Module.ScanStateTransition(new CassetteNoReadState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic)
        {
            //TODO : invalid call
        }
        public override void SetReservedScanState()
        {
            //TODO : invalid call
        }
    }

    internal class CassetteIllegalState : CassetteScanStateBase
    {
        public CassetteIllegalState(CassetteModule module) : base(module) { }

        public override CassetteScanStateEnum ScanState => CassetteScanStateEnum.ILLEGAL;

        public override void SetIllegalScanState()
        {

        }

        public override void SetReadingScanState()
        {
            try
            {
                Module.ScanStateTransition(new CassetteReadingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetNoReadScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                int i = 0;
                foreach (var slot in slots)
                {
                    slot.Holder.SetUndefined();
                    Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferStatus = slot.Holder.Status;
                    Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferObj = slot.Holder.TransferObject;
                    i++;
                }

                Module.ScanStateTransition(new CassetteNoReadState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic)
        {
            //TODO : invalid call
        }
        public override void SetReservedScanState()
        {
            Module.ScanStateTransition(new CassetteReservedState(Module));
        }
    }

    internal class CassetteReservedState : CassetteScanStateBase
    {
        public CassetteReservedState(CassetteModule module) : base(module) { }

        public override CassetteScanStateEnum ScanState => CassetteScanStateEnum.RESERVED;

        public override void SetIllegalScanState()
        {
            try
            {
                Module.ScanStateTransition(new CassetteIllegalState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetReadingScanState()
        {
            try
            {
                Module.ScanStateTransition(new CassetteReadingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        

        public override void SetNoReadScanState()
        {
            try
            {
                var slots = Module.Loader.ModuleManager.FindSlots(Module);
                int i = 0;
                foreach (var slot in slots)
                {
                    slot.Holder.SetUndefined();
                    Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferStatus = slot.Holder.Status;
                    Module.Loader.Foups[Module.ID.Index - 1].Slots[slots.Count - 1 - i].WaferObj = slot.Holder.TransferObject;
                    i++;
                }

                Module.ScanStateTransition(new CassetteNoReadState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void SetScanResult(Dictionary<ISlotModule, SlotScanStateEnum> scanRelDic)
        {
            //TODO : invalid call
        }
        public override void SetReservedScanState()
        {
            //TODO : invalid call
        }
    }
}
