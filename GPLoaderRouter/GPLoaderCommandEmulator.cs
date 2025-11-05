using Autofac;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.Enum;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPLoaderRouter
{
    using BCDCLV50x;
    using CognexOCRManualDialog;
    using LoaderBase;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using MetroDialogInterfaces;
    using ProberInterfaces;
    using ProberInterfaces.Foup;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using SystemExceptions.ProberSystemException;
    using NotifyEventModule;
    using ProberInterfaces.Event;
    using ProberInterfaces.RFID;
    using RFID;
    using CardIDManualDialog;

    public class GPLoaderCommandEmulator : IGPLoaderCommands, ICSTControlCommands, IFactoryModule, IGPUtilityBoxCommands, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        private ObservableCollection<bool> _TesterCoolantValveOpened = new ObservableCollection<bool>();
        public ObservableCollection<bool> TesterCoolantValveOpened
        {
            get { return _TesterCoolantValveOpened; }
            private set
            {
                if (value != _TesterCoolantValveOpened)
                {
                    _TesterCoolantValveOpened = value;
                    RaisePropertyChanged();
                }
            }
        }
        ILoaderModule loaderModule
        {
            get
            {
                if (this.GetLoaderContainer() != null)
                {
                    return this.GetLoaderContainer().Resolve<ILoaderModule>();
                }
                else
                {
                    return null;
                }
            }
        }

        object utilBoxLockObject = new object();

        private ObservableCollection<bool> _CoolantInletValveStates;
        public ObservableCollection<bool> CoolantInletValveStates
        {
            get { return _CoolantInletValveStates; }
            set
            {
                if (value != _CoolantInletValveStates)
                {
                    _CoolantInletValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _CoolantOutletValveStates;
        public ObservableCollection<bool> CoolantOutletValveStates
        {
            get { return _CoolantOutletValveStates; }
            set
            {
                if (value != _CoolantOutletValveStates)
                {
                    _CoolantOutletValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _PurgeValveStates;
        public ObservableCollection<bool> PurgeValveStates
        {
            get { return _PurgeValveStates; }
            set
            {
                if (value != _PurgeValveStates)
                {
                    _PurgeValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _DrainValveStates;
        public ObservableCollection<bool> DrainValveStates
        {
            get { return _DrainValveStates; }
            set
            {
                if (value != _DrainValveStates)
                {
                    _DrainValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _DryAirValveStates;
        public ObservableCollection<bool> DryAirValveStates
        {
            get { return _DryAirValveStates; }
            set
            {
                if (value != _DryAirValveStates)
                {
                    _DryAirValveStates = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<int> _CoolantPressures;
        public ObservableCollection<int> CoolantPressures
        {
            get { return _CoolantPressures; }
            set
            {
                if (value != _CoolantPressures)
                {
                    _CoolantPressures = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<bool> _CoolantLeaks;
        public ObservableCollection<bool> CoolantLeaks
        {
            get { return _CoolantLeaks; }
            set
            {
                if (value != _CoolantLeaks)
                {
                    _CoolantLeaks = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GPLoaderSysParam _SysParam;
        public GPLoaderSysParam SysParam
        {
            get { return _SysParam; }
            set { _SysParam = value; }
        }

        private RFIDModule _RFIDReader;
        public RFIDModule RFIDReader
        {
            get { return _RFIDReader; }
            set { _RFIDReader = value; }
        }

        int delaySec = 700;
        private CLVBCDSensor _BCDReader;

        public CLVBCDSensor BCDReader
        {
            get { return _BCDReader; }
            set { _BCDReader = value; }
        }

        public GPLoaderCommandEmulator()
        {
            try
            {
                CoolantPressures = new ObservableCollection<int>();
                CoolantLeaks = new ObservableCollection<bool>();
                CoolantInletValveStates = new ObservableCollection<bool>();
                CoolantOutletValveStates = new ObservableCollection<bool>();
                PurgeValveStates = new ObservableCollection<bool>();
                DrainValveStates = new ObservableCollection<bool>();
                DryAirValveStates = new ObservableCollection<bool>();
                TesterCoolantValveOpened = new ObservableCollection<bool>();
                for (int i = 0; i < GPLoaderDef.StageCount; i++)
                {
                    CoolantPressures.Add(new int());
                    CoolantLeaks.Add(new bool());
                    CoolantInletValveStates.Add(new bool());
                    CoolantOutletValveStates.Add(new bool());
                    PurgeValveStates.Add(new bool());
                    DrainValveStates.Add(new bool());
                    DryAirValveStates.Add(new bool());
                    TesterCoolantValveOpened.Add(new bool());
                }
                // System Parameter 읽기
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
                IParam tmpParam = null;
                tmpParam = new GPLoaderSysParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(GPLoaderSysParam));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    SysParam = tmpParam as GPLoaderSysParam;
                }
                //BCDReader = new CLVBCDSensor();
                //BCDReader.InitComm(4);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public EventCodeEnum BufferPick(IBufferModule Buffer, IARMModule arm)
        {
            LoggerManager.Debug($"BufferPick(): Source = {arm.ID.Label}, Destination = {Buffer.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PAMove(IPreAlignModule pa, double x, double y, double angle)
        {
            LoggerManager.Debug($"PAMove(): Source = {pa.ID.Label}. Position X = {x}, Y = {y}, Angle = {angle}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum PAMove(IPreAlignModule pa, double angle)
        {
            LoggerManager.Debug($"PAMove(): Source = {pa.ID.Label}.  Angle = {angle}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum BufferPut(IARMModule arm, IBufferModule Buffer)
        {
            LoggerManager.Debug($"BufferPut(): Source = {Buffer.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CardBufferPick(ICardBufferModule CCBuffer, ICardARMModule arm, int holderNum = -1)
        {
            LoggerManager.Debug($"CardBufferPick(): Source = {CCBuffer.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum CardBufferPut(ICardARMModule arm, ICardBufferModule CCBuffer, int holderNum = -1)
        {
            LoggerManager.Debug($"CardBufferPut(): Source = {arm.ID.Label}, Destination = {CCBuffer.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum CardTrayPick(ICardBufferTrayModule CCTray, ICardARMModule arm)
        {
            LoggerManager.Debug($"CardTrayPick(): Source = {CCTray.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);


            return EventCodeEnum.NONE;
        }
        public EventCodeEnum CardTrayPut(ICardARMModule arm, ICardBufferTrayModule CCTray)
        {
            LoggerManager.Debug($"CardTrayPut(): Source = {arm.ID.Label}, Destination = {CCTray.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum CardChangerPick(ICCModule CardChanger, ICardARMModule arm, int holderNum = -1)
        {
            LoggerManager.Debug($"CardChangerPick(): Source = {CardChanger.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum CardChangerPut(ICardARMModule arm, ICCModule CardChanger)
        {
            LoggerManager.Debug($"CardChangerPut(): Source = {arm.ID.Label}, Destination = {CardChanger.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum CassetteLoad(ICassetteModule cassetteModule)
        {
            LoggerManager.Debug($"CassetteLoad(): Source = {cassetteModule.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CassettePick(ISlotModule slot, IARMModule arm)
        {
            LoggerManager.Debug($"CassettePick(): Source = {slot.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CassettePut(IARMModule arm, ISlotModule slot)
        {
            LoggerManager.Debug($"CassettePut(): Source = {arm.ID.Label}, Destination = {slot.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CassetteScan(ICassetteModule cassetteModule)
        {
            LoggerManager.Debug($"CassetteScan(): Source = {cassetteModule.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CassetteUnLoad(ICassetteModule cassetteModule)
        {
            LoggerManager.Debug($"CassetteUnLoad(): Source = {cassetteModule.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ChuckPick(IChuckModule Chuck, IARMModule arm, int option = 0)
        {
            LoggerManager.Debug($"ChuckPick(): Source = {Chuck.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ChuckPut(IARMModule arm, IChuckModule Chuck, int option = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"ChuckPut(): Source = {arm.ID.Label}, Destination = {Chuck.ID.Label}");
                System.Threading.Thread.Sleep(delaySec);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        /// <summary>
        /// 로더가 Move 중인지 확인하는 함수. 
        /// true 이면 동작 중이 아님.
        /// </summary>
        /// <returns></returns>
        public bool IsIdleState()
        {
            return true;
        }

        /// <summary>
        /// 로더가 Card 관련 Move 중인지 확인하는 함수. 
        /// true 이면 동작 중이 아님.
        /// </summary>
        /// <returns></returns>
        public bool IsMovingOnCardOwner()
        {
            return false;
        }

        public int WaitForPA(IPreAlignModule pa)
        {
            LoggerManager.Debug($"WaitForPA(): Source = {pa.ID.Label}");
            return 0;
        }

        public void DisableAxis()
        {
            LoggerManager.Debug($"DisableAxis(): Disable axes.");

        }

        public void EnableAxis()
        {
            LoggerManager.Debug($"EnableAxis(): Enable axes.");
        }

        public EventCodeEnum FixedTrayPick(IFixedTrayModule FixedTray, IARMModule arm)
        {
            LoggerManager.Debug($"FixedTrayPick(): Source = {FixedTray.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum FixedTrayPut(IARMModule arm, IFixedTrayModule FixedTray)
        {
            LoggerManager.Debug($"FixedTrayPut(): Source = {arm.ID.Label}, Destination = {FixedTray.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum HomingRobot()
        {
            this.loaderModule.ModuleManager.InitAttachModules(true);
            var CstModules = loaderModule.ModuleManager.FindModules<ICassetteModule>();
            foreach (var foup in CstModules)
            {
                if (foup.FoupState == FoupStateEnum.LOAD && foup.ScanState == LoaderParameters.CassetteScanStateEnum.READ)
                {
                    CoverOpen(foup.ID.Index - 1);
                }
            }
            this.MetroDialogManager().ShowMessageDialog("Homing Done", "HomingSystem done!", EnumMessageStyle.Affirmative);
            return EventCodeEnum.NONE;
        }
        public void HomingResultAlarm(EventCodeEnum result)
        {
            try
            {
                if (result == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"HomingResultAlarm(): HomingSystem done!");
                    this.MetroDialogManager().ShowMessageDialog("Homing Done", "HomingSystem done!", EnumMessageStyle.Affirmative);
                }
                else
                {
                    LoaderBuzzer(true);
                    LoggerManager.Debug($"HomingResultAlarm(): HomingSystem failed! Error Code = {result}");
                    this.MetroDialogManager().ShowMessageDialog("Homing Error", "HomingSystem error occurred!\n Please InitSystem Button Click and Homming Again", EnumMessageStyle.Affirmative);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"HomingResultAlarm(): Err = {err}");
            }
        }
        public EventCodeEnum InitRobot()
        {
            this.MetroDialogManager().ShowMessageDialog("InitController Done", "InitInitController Successed", EnumMessageStyle.Affirmative);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum JogMove(ProbeAxisObject axisobj, double dist)
        {
            LoggerManager.Debug($"JogMove(): Axis = {axisobj.Label}, Distance = {dist}");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PAPick(IPreAlignModule pa, IARMModule arm)
        {
            LoggerManager.Debug($"PAPick(): Source = {pa.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PAForcedPick(IPreAlignModule pa, IARMModule arm)
        {
            LoggerManager.Debug($"PAPick(): Source = {pa.ID.Label}, Destination = {arm.ID.Label}");
            System.Threading.Thread.Sleep(delaySec);
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PAPut(IARMModule arm, IPreAlignModule pa)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                string sOrgin = arm.Holder.TransferObject.OriginHolder.Label;
                bool doPA = true;
                //long timeOut = 60000;
                var armIndex = arm.ID.Index;
                //SetRobotCommand(EnumRobotCommand.IDLE);
                //WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                LoggerManager.Debug($"[GPLoaderCommandEmulator] SetRobotCommand(): {EnumRobotCommand.IDLE}");

                WaferNotchTypeEnum notchType = WaferNotchTypeEnum.UNKNOWN;
                SubstrateSizeEnum waferSize = SubstrateSizeEnum.UNDEFINED;
                int pwIDReadResult = -1;
                // Set wafer size and notch type
                if (arm.Holder.TransferObject != null)
                {
                    if (arm.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        waferSize = arm.Holder.TransferObject.PolishWaferInfo.Size.Value;
                        notchType = arm.Holder.TransferObject.PolishWaferInfo.NotchType.Value;
                        LoggerManager.Debug($"PAPut(): Polsih Wafer Size = {waferSize}, NotchType = {notchType}");
                    }
                    else
                    {
                        waferSize = arm.Holder.TransferObject.Size.Value;
                        notchType = arm.Holder.TransferObject.NotchType;
                    }

                    if (waferSize == SubstrateSizeEnum.INCH6)
                    {
                        notchType = WaferNotchTypeEnum.FLAT;
                    }
                }
                else
                {
                    LoggerManager.Debug($"PAPut(): Transfer object is NULL.");
                    return EventCodeEnum.WAFER_SIZE_ERROR;
                }

                LoggerManager.Debug($"PAPut(): Transfer object waferSize: {waferSize}, notch type {notchType}.");
                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].SetDeviceSize(waferSize, notchType);
                errorCode = EventCodeEnum.NONE;
                if (errorCode != EventCodeEnum.NONE)
                {
                    return errorCode;
                }

                bool isControllerInIdle = true;
                if (isControllerInIdle)
                {
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                    errorCode = EventCodeEnum.NONE;
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        return errorCode;
                    }

                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);
                    LoggerManager.Debug($"PAPut(): Put wafer to {pa.ID.Index} Pre-aligner with {arm.ID.Index} Arm. Start.");
                    LoggerManager.Debug($"[GPLoaderCommandEmulator] SetRobotCommand(): {EnumRobotCommand.PA_PUT}");
                    errorCode = EventCodeEnum.NONE;
                    //errorCode = SetRobotCommand(EnumRobotCommand.PA_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    LoggerManager.Debug($"[GPLoaderCommandEmulator] SetRobotCommand(): {EnumRobotState.PA_PUTED}");
                    errorCode = EventCodeEnum.NONE;
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    if (arm.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        arm.Holder.CurrentWaferInfo = arm.Holder.TransferObject;
                        arm.Holder.SetTransfered(pa);
                        this.loaderModule.BroadcastLoaderInfo();
                    }
                    LoggerManager.Debug($"PAPut(): Put wafer to {pa.ID.Index} Pre-aligner with {arm.ID.Index} Arm. Done.");
                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);

                    double dstNotchAngle = 0;
                    if (pa.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                        {
                            dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                            LoggerManager.Debug($"[{this.GetType().Name}], PAPut(): Polsih Wafer Angle = {dstNotchAngle}");
                        }
                        else
                        {
                            dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                            LoggerManager.Debug($"PAPut(): Wafer Angle = {dstNotchAngle}");
                        }

                        dstNotchAngle = dstNotchAngle % 360;

                        if (dstNotchAngle < 0)
                        {
                            dstNotchAngle += 360;
                        }
                    }
                    ICognexProcessManager cognexProcessManager = loaderModule.Container.Resolve<ICognexProcessManager>();
                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.IDLE;

                    IOCRReadable OCR = loaderModule.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, pa.ID.Index) as IOCRReadable;

                    TransferObject transferObj = null;
                    if (pa.Holder.TransferObject != null)
                    {
                        transferObj = pa.Holder.TransferObject;
                    }
                    else if (arm.Holder.TransferObject != null)
                    {
                        transferObj = arm.Holder.TransferObject;
                    }
                    else
                    {
                        transferObj = pa.Holder.TransferObject;
                    }

                    if (transferObj == null)
                    {
                        System.Threading.Thread.Sleep(500);

                        if (pa.Holder.TransferObject != null)
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                        else if (arm.Holder.TransferObject != null)
                        {
                            transferObj = arm.Holder.TransferObject;
                        }
                        else
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                    }

                    if (transferObj == null)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (pa.Holder.TransferObject != null) //transfer Object가 널인 경우 다시한번 체크 해준다.
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                        else if (arm.Holder.TransferObject != null)
                        {
                            transferObj = arm.Holder.TransferObject;
                        }
                        else
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                    }

                    if (transferObj != null)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);

                        int slotNum = 0;
                        int foupNum = 0;
                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            slotNum = transferObj.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            foupNum = ((transferObj.OriginHolder.Index + offset) / 25) + 1;
                        }
                        else
                        {
                            slotNum = transferObj.OriginHolder.Index;
                            foupNum = 0;
                        }


                        ActiveLotInfo transferObjActiveLot = null;
                        if (transferObj.CST_HashCode != null)
                        {
                            //origin이 cassette인 웨이퍼
                            transferObjActiveLot = loaderModule.LoaderMaster.ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            if (transferObjActiveLot == null)
                            {
                                transferObjActiveLot = loaderModule.LoaderMaster.Prev_ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            }
                        }// //origin이 cassette가 아니라 insp이나 fixed에 있던 standardwafer인 경우 null로 반환.



                        if (transferObj.OCRReadState == OCRReadStateEnum.NONE)
                        {
                            if (transferObj.WaferType.Value == EnumWaferType.STANDARD ||
                                  transferObj.WaferType.Value == EnumWaferType.TCW)
                            {
                                switch (transferObj.OCRMode.Value)
                                {
                                    case OCRModeEnum.NONE:
                                        {
                                            doPA = true;// OCR Done 처리를 이 스레드에서 하므로 여기에서 PreAlign 을 해야한다. 
                                            //임의의 값을 자동할당
                                            transferObj.SetOCRState($"", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is NONE");
                                            
                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");

                                            break;
                                        }
                                    case OCRModeEnum.READ:
                                        {
                                            doPA = true;
                                            if (loaderModule.OCRConfig.Enable)
                                            {
                                                foreach (var config in loaderModule.OCRConfig.ConfigList)
                                                {
                                                    transferObj.OCRDevParam.ConfigList.Add(config);
                                                }

                                            }

                                            double OCRAngle = 0;
                                            LoaderParameters.SubchuckMotionParam subchuckMotionParam = OCR.GetSubchuckMotionParam(transferObj.Size.Value);
                                            if (subchuckMotionParam != null)
                                            {
                                                OCRAngle = transferObj.OCRAngle.Value + subchuckMotionParam.SubchuckAngle_Offset.Value;
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value} + Angle Offset{subchuckMotionParam.SubchuckAngle_Offset.Value} ");
                                            }
                                            else
                                            {
                                                OCRAngle = transferObj.OCRAngle.Value;
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value}");
                                            }
                                            errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                            LoggerManager.Debug($"DoPreAlign[OCR Aync] Result:{errorCode.ToString()}");
                                            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                            {
                                                errorCode = EventCodeEnum.NONE;
                                            }
                                            if (errorCode != EventCodeEnum.NONE)
                                            {
                                                loaderModule.PAManager.PAModules[pa.ID.Index - 1].UpdateState();

                                                LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_OCR, StateLogType.RETRY, $"PA Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Start AnglePosition:{ OCRAngle}");
                                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Result:{errorCode.ToString()}");

                                            }
                                            else // DoPreAlign 성공한 경우 
                                            {
                                                if (transferObj.NotchAngle.Value == transferObj.OCRAngle.Value)
                                                {
                                                    transferObj.SetPreAlignDone(pa.ID);
                                                }
                                                else
                                                {
                                                    transferObj.CleanPreAlignState(reason: $"Notch Angle({transferObj.NotchAngle.Value}) and Ocr Angle({transferObj.OCRAngle.Value}) is not same");
                                                }
                                            }


                                            if (errorCode == EventCodeEnum.NONE)
                                            {
                                                LoggerManager.Debug($"[PAPut()] FoupNumber{foupNum}, SlotNumber{slotNum}");

                                                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                                                if (subchuckMotionParam != null)
                                                {
                                                    if (subchuckMotionParam.SubchuckXCoord.Value == 0 && subchuckMotionParam.SubchuckYCoord.Value == 0)
                                                    {
                                                        retVal = EventCodeEnum.NONE;
                                                    }
                                                    else
                                                    {
                                                        retVal = PAMove(pa, subchuckMotionParam.SubchuckXCoord.Value, subchuckMotionParam.SubchuckYCoord.Value, 0);
                                                    }
                                                }
                                                else
                                                {
                                                    retVal = EventCodeEnum.NONE;
                                                }

                                                if (retVal == EventCodeEnum.NONE)
                                                {
                                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.READING);
                                                    retVal = cognexProcessManager.DoOCRStringCatch(pa.ID.Index - 1, false, transferObj.OCRDevParam, transferObjActiveLot);
                                                    Thread.Sleep(5000);

                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                        var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];
                                                        
                                                        LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {ocr}, Score: {ocrScore}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                        LoggerManager.Debug($"[OCR Result Data] State={OCRReadStateEnum.DONE}, ID: { ocr} , Score:{ocrScore} , OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                        transferObj.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                                        cognexProcessManager.Ocr[pa.ID.Index - 1] = "";
                                                        cognexProcessManager.OcrScore[pa.ID.Index - 1] = 0;
                                                    }
                                                    else
                                                    {
                                                        transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);

                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                        LoggerManager.Debug($"DoPreAlign[OCR] All OCRConfig Failed.");

                                                        LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.ERROR, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");


                                                        loaderModule.NotifyManager.Notify(EventCodeEnum.OCR_READ_FAIL, pa.ID.Index);

                                                        cognexProcessManager.SaveOCRImage(pa.ID.Index - 1);
                                                    }
                                                }
                                                else
                                                {
                                                    //transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PAMove Failed. retVal:{retVal}");
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                }
                                            }
                                            else
                                            {
                                                doPA = false;
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                //PreAlign Fail인 경우                                                  
                                                if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                                    errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                                {
                                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. errorCode:{errorCode}");
                                                }
                                                LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. Cannnot try Read OCR.");
                                                //transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT); <-- 아직 OCR 시도를 안한 상태이기 때문에 Abort 이면 안된다.                                                 
                                            }
                                            break;
                                        }
                                    case OCRModeEnum.MANUAL:
                                        {
                                            WaferIDManualDialog.WaferIDManualInput.Show(this.GetLoaderContainer(), transferObj);
                                            if (transferObj.OCR.Value == null || transferObj.OCR.Value == "")
                                            {
                                                errorCode = EventCodeEnum.NODATA;
                                                transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// 이미 Manual Input에 실패했는데 다시 기회를 줄 이유 없음.                                                         
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                LoggerManager.Error($"WaferIDManualInput is Null Error");
                                            }
                                            else
                                            {
                                                transferObj.SetOCRState(transferObj.OCR.Value, 0, OCRReadStateEnum.DONE);
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;

                                                LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");
                                            }
                                            break;
                                        }
                                    case OCRModeEnum.DEBUGGING:
                                        {
                                            Thread.Sleep(1000);
                                            doPA = true;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is DEBUGGING");
                                            transferObj.SetOCRState("DEBUGGING", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            //cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR; // DoOCRStringCatch 을 기다리는 중이 아니므로 할필요 없음.
                                            break;
                                        }
                                    default:
                                        break;
                                }


                                if (transferObj.OCRReadState == OCRReadStateEnum.FAILED)
                                {
                                    try
                                    {
                                        CognexManualInput.Show(this.GetLoaderContainer(), pa.ID.Index - 1);
                                        if (cognexProcessManager.GetManualOCRState(pa.ID.Index - 1))
                                        {
                                            var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                            var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                            pa.Holder.TransferObject.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                            
                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ManualDone, ID: {ocr}, Score: {0}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");

                                            LoggerManager.Debug($"[OCR Result Data] State=Manual_DONE, ID:{ocr}, Score: {0}  OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");
                                        }
                                        else
                                        {
                                            this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                            pa.Holder.TransferObject.SetOCRState("", 0, OCRReadStateEnum.ABORT);
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                    finally
                                    {
                                        doPA = true;
                                        loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID);
                                    }

                                }
                                else if (transferObj.IsOCRDone())
                                {
                                    doPA = true;
                                    loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID);
                                }
                                else
                                {
                                    doPA = false;
                                    LoggerManager.Debug($"Unexpected State.");
                                    //혹시 모를 예외처리
                                }

                            }
                            else if (transferObj.WaferType.Value == EnumWaferType.POLISH)
                            {
                                switch (transferObj.OCRMode.Value)
                                {
                                    case OCRModeEnum.READ:
                                        {
                                            // wafer change rcmd ocrread = enable trigger가 된 경우 READ
                                            doPA = true;

                                            LoggerManager.Debug($"DoPreAlign[OCR] Start AnglePosition:{ transferObj.OCRAngle.Value}");
                                            errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(transferObj.OCRAngle.Value, true);
                                            LoggerManager.Debug($"DoPreAlign[OCR] Result:{errorCode.ToString()}");
                                            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                            {
                                                errorCode = EventCodeEnum.NONE;
                                            }

                                            if (errorCode != EventCodeEnum.NONE)
                                            {
                                                loaderModule.PAManager.PAModules[pa.ID.Index - 1].UpdateState();

                                                LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_OCR, StateLogType.RETRY, $"PA Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                LoggerManager.Debug($"DoPreAlign[OCR retry] Start AnglePosition:{ transferObj.OCRAngle.Value}");
                                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(transferObj.OCRAngle.Value, true);
                                                LoggerManager.Debug($"DoPreAlign[OCR retry] Result:{errorCode.ToString()}");
                                            }
                                            else // DoPreAlign 성공한 경우 
                                            {
                                                if (transferObj.NotchAngle.Value == transferObj.OCRAngle.Value)
                                                {
                                                    transferObj.SetPreAlignDone(pa.ID);
                                                }
                                                else
                                                {
                                                    transferObj.CleanPreAlignState(reason: $"Notch Angle({transferObj.NotchAngle.Value}) and Ocr Angle({transferObj.OCRAngle.Value}) is not same");
                                                }
                                            }

                                            if (errorCode == EventCodeEnum.NONE)
                                            {
                                                LoggerManager.Debug($"[PAPut()] FoupNumber{foupNum}, SlotNumber{slotNum}");

                                                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                                                if (transferObj.Size.Value == SubstrateSizeEnum.INCH6)
                                                {
                                                    //retVal = PAMove(pa, 0, 30000);
                                                    retVal = EventCodeEnum.NONE;
                                                    LoggerManager.Debug($"[PAPut()] PAMove - INCH6");
                                                }
                                                else if (transferObj.Size.Value == SubstrateSizeEnum.INCH8)
                                                {
                                                    //retVal = PAMove(pa, 0, -20000);
                                                    retVal = EventCodeEnum.NONE;
                                                    LoggerManager.Debug($"[PAPut()] PAMove - INCH8");

                                                }
                                                else
                                                {
                                                    retVal = EventCodeEnum.NONE;
                                                }

                                                if (retVal == EventCodeEnum.NONE)
                                                {
                                                    //rcmd로 부터 받은 ocr 값으로 우선 set 한다.
                                                    transferObj.SetOCRState(transferObj.OCR.Value, 0, OCRReadStateEnum.READING);
                                                    if (transferObj.PolishWaferInfo != null)
                                                    {
                                                        LoggerManager.Debug($"OCR Config DefineName = {transferObj.PolishWaferInfo.DefineName}");
                                                    }
                                                    retVal = cognexProcessManager.DoOCRStringCatch(pa.ID.Index - 1, false, transferObj.OCRDevParam, transferObjActiveLot);
                                                    //Thread.Sleep(5000);
                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        //OCR 성공 !!
                                                        var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                        var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                                        {
                                                            //OCR Value "" 인 경우, HOST에서 받은 값이 없다고 간주,
                                                            if (transferObj.OCR.Value != ocr && transferObj.OCR.Value != "")
                                                            {
                                                                //ocr = HOST 에서 받은 값
                                                                ocr = transferObj.OCR.Value;

                                                                //ocr result 2
                                                                pwIDReadResult = 2;
                                                            }
                                                            else
                                                            {
                                                                //ocr = cognex 결과 값
                                                                //ocr result 0 (성공)
                                                                pwIDReadResult = 0;
                                                            }
                                                        }
                                                        else if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                                        {
                                                            //ocr = cognex 결과 값
                                                            //ocr result 0 (성공)
                                                            pwIDReadResult = 0;
                                                        }

                                                        transferObj.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                                        cognexProcessManager.Ocr[pa.ID.Index - 1] = "";
                                                        cognexProcessManager.OcrScore[pa.ID.Index - 1] = 0;

                                                        LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {ocr}, Score: {ocrScore}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                        LoggerManager.Debug($"[OCR Result Data] State={OCRReadStateEnum.DONE}, ID: { ocr} , Score:{ocrScore} , OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                    }
                                                    else
                                                    {
                                                        //OCR 실패 !!
                                                        doPA = false;
                                                        string ocr = "";
                                                        OCRReadStateEnum ocr_result = OCRReadStateEnum.NONE;
                                                        EnumCognexModuleState cognex_state = EnumCognexModuleState.IDLE;
                                                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                                                        {
                                                            //OCR Value "" 인 경우, HOST에서 받은 값이 없다고 간주
                                                            if (transferObj.OCR.Value != "")
                                                            {
                                                                //ocr = HOST 에서 받은 값
                                                                ocr = transferObj.OCR.Value;
                                                                //ocr result 2 (manual 로 사용 하였음)
                                                                pwIDReadResult = 2;
                                                                ocr_result = OCRReadStateEnum.DONE;
                                                                cognex_state = EnumCognexModuleState.READ_OCR;
                                                            }
                                                            else
                                                            {
                                                                ocr = "EMPTY";
                                                                //ocr result 1 (실패)
                                                                pwIDReadResult = 1;
                                                                ocr_result = OCRReadStateEnum.FAILED;
                                                                cognex_state = EnumCognexModuleState.FAIL;
                                                            }
                                                        }
                                                        else if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.FIXEDTRAY)
                                                        {
                                                            if (transferObj.OCR.Value != "")
                                                            {
                                                                //ocr 이미 가지고 있던 값
                                                                ocr = transferObj.OCR.Value;
                                                                //ocr result 2 (manual)
                                                                pwIDReadResult = 2;
                                                                ocr_result = OCRReadStateEnum.DONE;
                                                                cognex_state = EnumCognexModuleState.READ_OCR;
                                                            }
                                                            else
                                                            {
                                                                ocr = "EMPTY";
                                                                //ocr result 1 (실패)
                                                                pwIDReadResult = 1;
                                                                ocr_result = OCRReadStateEnum.FAILED;
                                                                cognex_state = EnumCognexModuleState.FAIL;
                                                            }
                                                        }

                                                        //MANUAL INPUT창을 안띄우는 것이니 ABORT로 한다.
                                                        transferObj.SetOCRState(ocr, 0, ocr_result);
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = cognex_state;
                                                        LoggerManager.Debug($"DoPreAlign[OCR] All OCRConfig Failed.");

                                                        loaderModule.NotifyManager.Notify(EventCodeEnum.OCR_READ_FAIL, pa.ID.Index);

                                                        cognexProcessManager.SaveOCRImage(pa.ID.Index - 1);
                                                    }
                                                }
                                                else
                                                {
                                                    doPA = false;
                                                    //transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PAMove Failed. retVal:{retVal}");
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                }

                                            }
                                            else
                                            {
                                                doPA = false;
                                                cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                //PreAlign Fail인 경우                                                  
                                                if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                                    errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                                {
                                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                                }
                                                else
                                                {
                                                    LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. errorCode:{errorCode}");
                                                }
                                                LoggerManager.Debug($"DoPreAlign[OCR] PreAlign Retry Failed. Cannnot try Read OCR.");
                                                //transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// System Error로 간주해서 손으로 Recovery 해줘야함. PAAlignAbort = true로 바뀐 상태.

                                            }

                                            LoggerManager.Debug($"PAPut() : OCRMode = {transferObj.OCRMode.Value}, pwIDReadResult = {pwIDReadResult}");

                                            break;
                                        }
                                    default:
                                        //polish wafer ocr 하지 않는 경우, 기존과 동일한 코드
                                        doPA = true;
                                        if (transferObj.OCR.Value != "")
                                        {
                                            pwIDReadResult = 2;
                                            //Host에서 받은 값이 있는 경우,
                                            transferObj.SetOCRState(transferObj.OCR.Value, 399, OCRReadStateEnum.DONE);
                                        }
                                        else
                                        {
                                            pwIDReadResult = 3; //ocr을 읽지 않는 mode이기 때문에 1(실패) 대신 3으로 Set.
                                            transferObj.SetOCRState("EMPTY", 399, OCRReadStateEnum.DONE);
                                        }
                                        LoggerManager.Debug($"PAPut() : POLISH Wafer OCRMode = {transferObj.OCRMode.Value}, OCR Result : {transferObj.OCR.Value}");
                                        break;
                                }

                                if (transferObj.OCRReadState == OCRReadStateEnum.FAILED)
                                {
                                    if (loaderModule.LoaderMaster.HostInitiatedWaferChangeInProgress == true)
                                    {
                                        //manual input 창 띄우지 않고 Abort
                                        this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                        pa.Holder.TransferObject.SetOCRState("EMPTY", 0, OCRReadStateEnum.ABORT);

                                    }
                                    else
                                    {
                                        try
                                        {
                                            CognexManualInput.Show(this.GetLoaderContainer(), pa.ID.Index - 1);
                                            if (cognexProcessManager.GetManualOCRState(pa.ID.Index - 1))
                                            {
                                                pwIDReadResult = 2;
                                                var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                                pa.Holder.TransferObject.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);

                                                LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ManualDone, ID: {ocr}, Score: {0}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");

                                                LoggerManager.Debug($"[OCR Result Data] State=Manual_DONE, ID:{ocr}, Score: {0}  OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(pa.Holder.TransferObject.OriginHolder)}");
                                            }
                                            else
                                            {
                                                this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                                pa.Holder.TransferObject.SetOCRState("EMPTY", 0, OCRReadStateEnum.ABORT);
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            this.NotifyManager().Notify(EventCodeEnum.OCR_READ_FAIL_MANUAL, pa.ID.Index);
                                            pa.Holder.TransferObject.SetOCRState("EMPTY", 0, OCRReadStateEnum.ABORT);
                                            LoggerManager.Exception(err);
                                        }
                                    }
                                }


                                doPA = true;
                                loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID, pwIDReadResult);

                            }
                            else//CARD, INVALID, UNDEFINED
                            {
                                doPA = true;
                                LoggerManager.Debug($"PAPut(): OCRReadStateEnum = {transferObj.OCRReadState.ToString()}");
                                if (transferObj.IsOCRDone() == false)
                                {
                                    LoggerManager.Debug("OCR TransferObejct is NULL");
                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);//기존 코드 동일하게 
                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;//기존 코드 동일하게                                     
                                }
                            }
                        }

                        //if (transferObj.IsOCRDone() == false)//예외처리
                        //{
                        //    doPA = false;
                        //    LoggerManager.Debug("OCR TransferObejct is NULL");
                        //    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                        //    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                        //}

                        if (doPA)//Load일때도 타고 Unload일때도 탐.
                        {
                            LoggerManager.Debug($"DoPreAlign Start NotchAngle:{dstNotchAngle}");
                            errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(dstNotchAngle);                            
                            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                            {
                                errorCode = EventCodeEnum.NONE;
                            }
                            if (errorCode == EventCodeEnum.NONE)
                            {
                                //transferObj.SetPreAlignDone(pa.ID);<-- 이걸 여기서 하면 NobufferMode일때 GP_PreAlignState를 가지고 올수 없어서 다음 job이 진행이 안됨.
                            }
                            else if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                            {
                                this.NotifyManager().Notify(errorCode, pa.ID.Index);
                            }
                            else //errorCode != EventCodeEnum.NONE
                            {
                                transferObj.CleanPreAlignState(reason: "PreAlign Failed.");

                            }

                        }

                        if (errorCode == EventCodeEnum.NONE)
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);
                        }
                        else
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN, errorCode.ToString());
                        }
                    }
                    else
                    {
                        errorCode = EventCodeEnum.LOADER_PA_WAF_MISSED;
                        LoggerManager.Debug($"PAPut(): TransferObject Cannot find. Arm:{arm.Holder.Status}, PA:{pa.Holder.Status}");
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                TransferObject transferObject = null;
                if (arm.Holder.TransferObject != null)
                {
                    transferObject = arm.Holder.TransferObject;
                }

                if (pa.Holder.TransferObject != null)
                {
                    transferObject = pa.Holder.TransferObject;
                }

                if (transferObject != null)
                {
                    if (transferObject.IsOCRDone() == false)
                    {
                        transferObject.OCRReadState = OCRReadStateEnum.ABORT;
                    }
                }

                loaderModule.ResonOfError = $"PAPut(): PA Put Error {errorCode}.";
                LoggerManager.Error($"PAPut(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }
        public EventCodeEnum PAPutAync(IARMModule arm, IPreAlignModule pa)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                string sOrgin = arm.Holder.TransferObject.OriginHolder.Label;
                bool doPA = true;
                //long timeOut = 60000;
                var armIndex = arm.ID.Index;
                //SetRobotCommand(EnumRobotCommand.IDLE);
                //WaitForCommandDone(EnumRobotState.IDLE, timeOut);
                LoggerManager.Debug($"[GPLoaderCommandEmulator] SetRobotCommand(): {EnumRobotCommand.IDLE}");

                WaferNotchTypeEnum notchType = WaferNotchTypeEnum.UNKNOWN;
                SubstrateSizeEnum waferSize = SubstrateSizeEnum.UNDEFINED;
                // Set wafer size and notch type

                if (arm.Holder.TransferObject != null)
                {
                    if (arm.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        waferSize = arm.Holder.TransferObject.PolishWaferInfo.Size.Value;
                        notchType = arm.Holder.TransferObject.PolishWaferInfo.NotchType.Value;
                        LoggerManager.Debug($"PAPutAync(): Polsih Wafer Size = {waferSize}");
                    }
                    else
                    {
                        waferSize = arm.Holder.TransferObject.Size.Value;
                        notchType = arm.Holder.TransferObject.NotchType;
                    }

                    if (waferSize == SubstrateSizeEnum.INCH6)
                    {
                        notchType = WaferNotchTypeEnum.FLAT;
                    }
                }
                else
                {
                    LoggerManager.Debug($"PAPutAync(): Transfer object is NULL.");
                    return EventCodeEnum.WAFER_SIZE_ERROR;
                }

                LoggerManager.Debug($"PAPutAync(): Transfer object waferSize: {waferSize}, notch type {notchType}.");
                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].SetDeviceSize(waferSize, notchType);
                errorCode = EventCodeEnum.NONE;
                if (errorCode != EventCodeEnum.NONE)
                {
                    return errorCode;
                }

                bool isControllerInIdle = true;
                if (isControllerInIdle)
                {
                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].ModuleReset();
                    errorCode = EventCodeEnum.NONE;
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        return errorCode;
                    }

                    //CDXOut.nPreAPos = (short)pa.ID.Index;
                    //CDXOut.nArmIndex = (ushort)armIndex;                    

                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);
                    LoggerManager.Debug($"PAPutAync(): Put wafer to {pa.ID.Index} Pre-aligner with {arm.ID.Index} Arm. Start.");
                    LoggerManager.Debug($"[GPLoaderCommandEmulator] SetRobotCommand(): {EnumRobotCommand.PA_PUT}");
                    errorCode = EventCodeEnum.NONE;
                    //errorCode = SetRobotCommand(EnumRobotCommand.PA_PUT);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    LoggerManager.Debug($"Debug...");
                    //errorCode = WaitForCommandDone(EnumRobotState.PA_PUTED, timeOut);
                    LoggerManager.Debug($"[GPLoaderCommandEmulator] SetRobotCommand(): {EnumRobotState.PA_PUTED}");
                    errorCode = EventCodeEnum.NONE;
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT, errorCode.ToString());
                        throw new ProberSystemException(errorCode);
                    }
                    if (arm.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        arm.Holder.CurrentWaferInfo = arm.Holder.TransferObject;
                        arm.Holder.SetTransfered(pa);
                        this.loaderModule.BroadcastLoaderInfo();
                    }
                    LoggerManager.Debug($"PAPutAync(): Put wafer to {pa.ID.Index} Pre-aligner with {arm.ID.Index} Arm. Done.");
                    LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PA_PUT);

                    double dstNotchAngle = 0;
                    if (pa.Holder.Status == EnumSubsStatus.EXIST)
                    {
                        if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                        {
                            dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                            LoggerManager.Debug($"[{this.GetType().Name}], PAPutAsync(): Polsih Wafer Angle = {dstNotchAngle}");
                        }
                        else
                        {
                            dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                            LoggerManager.Debug($"PAPutAsync(): Wafer Angle = {dstNotchAngle}");
                        }

                        dstNotchAngle = dstNotchAngle % 360;

                        if (dstNotchAngle < 0)
                        {
                            dstNotchAngle += 360;
                        }
                    }
                    ICognexProcessManager cognexProcessManager = loaderModule.Container.Resolve<ICognexProcessManager>();
                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.IDLE;

                    IOCRReadable OCR = loaderModule.ModuleManager.FindModule(ModuleTypeEnum.COGNEXOCR, pa.ID.Index) as IOCRReadable;

                    Thread.Sleep(5000); // GP_ReadCognexState를 타도록 만들기 위한 의도된 Sleep. 해당 지연시간이 없으면 에뮬에서 GP_ReadCognexState를타지 않음. 
                    TransferObject transferObj = null;
                    if (pa.Holder.TransferObject != null)
                    {
                        transferObj = pa.Holder.TransferObject;
                    }
                    else if (arm.Holder.TransferObject != null)
                    {
                        transferObj = arm.Holder.TransferObject;
                    }
                    else
                    {
                        transferObj = pa.Holder.TransferObject;
                    }

                    if (transferObj == null)
                    {
                        System.Threading.Thread.Sleep(500);

                        if (pa.Holder.TransferObject != null)
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                        else if (arm.Holder.TransferObject != null)
                        {
                            transferObj = arm.Holder.TransferObject;
                        }
                        else
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                    }

                    if (transferObj == null)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (pa.Holder.TransferObject != null) //transfer Object가 널인 경우 다시한번 체크 해준다.
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                        else if (arm.Holder.TransferObject != null)
                        {
                            transferObj = arm.Holder.TransferObject;
                        }
                        else
                        {
                            transferObj = pa.Holder.TransferObject;
                        }
                    }

                    if (transferObj != null)
                    {
                        int slotNum = 0;
                        int foupNum = 0;
                        if (transferObj.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            slotNum = transferObj.OriginHolder.Index % 25;
                            int offset = 0;
                            if (slotNum == 0)
                            {
                                slotNum = 25;
                                offset = -1;
                            }
                            foupNum = ((transferObj.OriginHolder.Index + offset) / 25) + 1;
                        }
                        else
                        {
                            slotNum = transferObj.OriginHolder.Index;
                            foupNum = 0;
                        }


                        ActiveLotInfo transferObjActiveLot = null;
                        if (transferObj.CST_HashCode != null)
                        {
                            //origin이 cassette인 웨이퍼
                            transferObjActiveLot = loaderModule.LoaderMaster.ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            if (transferObjActiveLot == null)
                            {
                                transferObjActiveLot = loaderModule.LoaderMaster.Prev_ActiveLotInfos.FirstOrDefault(w => w.CST_HashCode == transferObj.CST_HashCode);
                            }
                        }// //origin이 cassette가 아니라 insp이나 fixed에 있던 standardwafer인 경우 null로 반환.



                        if (transferObj.OCRReadState == OCRReadStateEnum.NONE)
                        {
                            if (transferObj.WaferType.Value == EnumWaferType.STANDARD ||
                                  transferObj.WaferType.Value == EnumWaferType.TCW)
                            {
                                switch (transferObj.OCRMode.Value)
                                {
                                    case OCRModeEnum.NONE:
                                        {
                                            doPA = true;// OCR Done 처리를 이 스레드에서 하므로 여기에서 PreAlign 을 해야한다. 
                                            //임의의 값을 자동할당
                                            transferObj.SetOCRState($"", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is NONE");

                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");

                                            break;
                                        }
                                    case OCRModeEnum.READ:
                                        {
                                            break;
                                        }
                                    case OCRModeEnum.MANUAL:
                                        {
                                            break;
                                        }
                                    case OCRModeEnum.DEBUGGING:
                                        {
                                            Thread.Sleep(1000);
                                            doPA = true;
                                            transferObj.CleanPreAlignState(reason: "Ocr Mode is DEBUGGING");
                                            transferObj.SetOCRState("DEBUGGING", 399, OCRReadStateEnum.DONE);
                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                            //cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR; // DoOCRStringCatch 을 기다리는 중이 아니므로 할필요 없음.
                                            break;
                                        }
                                    default:
                                        break;
                                }

                                if (transferObj.OCRReadState == OCRReadStateEnum.FAILED)
                                {
                                    doPA = false;
                                    //Wait가 필요한 모드, 다음 스레드에서 처리.
                                }
                                else if (transferObj.OCRMode.Value == OCRModeEnum.MANUAL || transferObj.OCRMode.Value == OCRModeEnum.READ)
                                {
                                    doPA = true;
                                }
                                else if (transferObj.OCRMode.Value == OCRModeEnum.DEBUGGING || transferObj.OCRMode.Value == OCRModeEnum.NONE)
                                {
                                    doPA = true;
                                    //Wait가 필요하지 않은 모드
                                    loaderModule.LoaderMaster.OcrReadStateRisingEvent(transferObj, pa.ID);
                                }
                                else
                                {
                                    doPA = true;
                                    LoggerManager.Debug($"Unexpected State.");
                                    //혹시 모를 예외처리
                                }

                            }
                            else if (transferObj.WaferType.Value == EnumWaferType.POLISH)
                            {
                                if (transferObj.OCR.Value != "")
                                {
                                    transferObj.SetOCRState(transferObj.OCR.Value, 399, OCRReadStateEnum.DONE);
                                }
                                else
                                {
                                    transferObj.SetOCRState("EMPTY", 399, OCRReadStateEnum.DONE);//TODO: Polish 의 OCR을시도하지 않더라도 ID로 구별해주는게 좋지 않을까? 
                                }
                                doPA = true;

                                if (pa.Holder.TransferObject.WaferType.Value == EnumWaferType.POLISH)
                                {
                                    dstNotchAngle = pa.Holder.TransferObject.PolishWaferInfo.CurrentAngle.Value - 90;

                                    LoggerManager.Debug($"[{this.GetType().Name}], PAPutAsync(): Polsih Wafer Angle = {dstNotchAngle}");
                                }
                                else
                                {
                                    dstNotchAngle = pa.Holder.TransferObject.NotchAngle.Value - 90;
                                }

                                dstNotchAngle = dstNotchAngle % 360;

                                if (dstNotchAngle < 0)
                                {
                                    dstNotchAngle += 360;
                                }
                            }
                            else//CARD, INVALID, UNDEFINED
                            {
                                doPA = true;
                                LoggerManager.Debug($"PAPutAsync(): OCRReadStateEnum = {transferObj.OCRReadState.ToString()}");
                                if (transferObj.IsOCRDone() == false)
                                {
                                    LoggerManager.Debug("OCR TransferObejct is NULL");
                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);//기존 코드 동일하게 
                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;//기존 코드 동일하게                                     
                                }
                            }
                        }



                        Task.Run(() =>
                        {
                            LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.START, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);

                            if (transferObj.OCRReadState == OCRReadStateEnum.NONE)
                            {
                                /// Mode에 따라서 실제 동작
                                if (transferObj.WaferType.Value == EnumWaferType.STANDARD ||
                                    transferObj.WaferType.Value == EnumWaferType.TCW)
                                {
                                    switch (transferObj.OCRMode.Value)
                                    {
                                        case OCRModeEnum.NONE:
                                            {
                                                break;
                                            }
                                        case OCRModeEnum.READ:
                                            {
                                                if (loaderModule.OCRConfig.Enable)
                                                {
                                                    foreach (var config in loaderModule.OCRConfig.ConfigList)
                                                    {
                                                        transferObj.OCRDevParam.ConfigList.Add(config);
                                                    }

                                                }

                                                double OCRAngle = 0;
                                                LoaderParameters.SubchuckMotionParam subchuckMotionParam = OCR.GetSubchuckMotionParam(transferObj.Size.Value);
                                                if (subchuckMotionParam != null)
                                                {
                                                    OCRAngle = transferObj.OCRAngle.Value + subchuckMotionParam.SubchuckAngle_Offset.Value;
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value} + Angle Offset{subchuckMotionParam.SubchuckAngle_Offset.Value} ");
                                                }
                                                else
                                                {
                                                    OCRAngle = transferObj.OCRAngle.Value;
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync] Start AnglePosition:{ transferObj.OCRAngle.Value}");

                                                }

                                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                                LoggerManager.Debug($"DoPreAlign[OCR Aync] Result:{errorCode.ToString()}");
                                                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                                {
                                                    errorCode = EventCodeEnum.NONE;
                                                }
                                                if (errorCode != EventCodeEnum.NONE)
                                                {                                                    
                                                    loaderModule.PAManager.PAModules[pa.ID.Index - 1].UpdateState();

                                                    LoggerManager.ActionLog(ModuleLogType.PREALIGN_TO_OCR, StateLogType.RETRY, $"PA Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Start AnglePosition:{ OCRAngle}");
                                                    errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(OCRAngle, true);
                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync retry] Result:{errorCode.ToString()}");

                                                }
                                                else // DoPreAlign 성공한 경우 
                                                {
                                                    if (transferObj.NotchAngle.Value == transferObj.OCRAngle.Value)
                                                    {
                                                        transferObj.SetPreAlignDone(pa.ID);
                                                    }
                                                    else
                                                    {
                                                        transferObj.CleanPreAlignState(reason: $"Notch Angle({transferObj.NotchAngle.Value}) and Ocr Angle({transferObj.OCRAngle.Value}) is not same");
                                                    }
                                                }


                                                if (errorCode == EventCodeEnum.NONE)
                                                {
                                                    LoggerManager.Debug($"[PAPutAsync()] FoupNumber{foupNum}, SlotNumber{slotNum}");

                                                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                                                    if (subchuckMotionParam != null)
                                                    {
                                                        if (subchuckMotionParam.SubchuckXCoord.Value == 0 && subchuckMotionParam.SubchuckYCoord.Value == 0)
                                                        {
                                                            retVal = EventCodeEnum.NONE;
                                                        }
                                                        else
                                                        {
                                                            retVal = PAMove(pa, subchuckMotionParam.SubchuckXCoord.Value, subchuckMotionParam.SubchuckYCoord.Value, 0);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        retVal = EventCodeEnum.NONE;
                                                    }

                                                    if (retVal == EventCodeEnum.NONE)
                                                    {
                                                        transferObj.SetOCRState("", 0, OCRReadStateEnum.READING);
                                                        retVal = cognexProcessManager.DoOCRStringCatch(pa.ID.Index - 1, false, transferObj.OCRDevParam, transferObjActiveLot);
                                                        Thread.Sleep(5000);

                                                        if (retVal == EventCodeEnum.NONE)
                                                        {
                                                            var ocr = cognexProcessManager.Ocr[pa.ID.Index - 1];
                                                            var ocrScore = cognexProcessManager.OcrScore[pa.ID.Index - 1];

                                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"ID: {ocr}, Score: {ocrScore}, OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                            LoggerManager.Debug($"[OCR Result Data] State={OCRReadStateEnum.DONE}, ID: { ocr} , Score:{ocrScore} , OCR Index:{pa.ID.Index} , Origin Location:{loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");
                                                            transferObj.SetOCRState(ocr, ocrScore, OCRReadStateEnum.DONE);
                                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;
                                                            cognexProcessManager.Ocr[pa.ID.Index - 1] = "";
                                                            cognexProcessManager.OcrScore[pa.ID.Index - 1] = 0;
                                                        }
                                                        else
                                                        {
                                                            doPA = false;
                                                            transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);

                                                            cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                            LoggerManager.Debug($"DoPreAlign[OCR Aync] All OCRConfig Failed.");

                                                            LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.ERROR, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}");

                                                            loaderModule.NotifyManager.Notify(EventCodeEnum.OCR_READ_FAIL, pa.ID.Index);

                                                            cognexProcessManager.SaveOCRImage(pa.ID.Index - 1);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        doPA = false;
                                                        transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);// System Error로 간주해서 손으로 Recovery 해줘야함.
                                                        LoggerManager.Debug($"DoPreAlign[OCR Aync] PAMove Failed. retVal:{retVal}");
                                                        cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                    }



                                                }
                                                else
                                                {
                                                    doPA = false;
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.ABORT;
                                                    //PreAlign Fail인 경우                                                  
                                                    if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                                        errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                                    {
                                                        this.NotifyManager().Notify(errorCode, pa.ID.Index);
                                                    }
                                                    else
                                                    {
                                                        LoggerManager.Debug($"DoPreAlign[OCR Aync] PreAlign Retry Failed. errorCode:{errorCode}");
                                                    }

                                                    LoggerManager.Debug($"DoPreAlign[OCR Aync] PreAlign Retry Failed. Cannnot try Read OCR.");
                                                    //transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT); <-- 아직 OCR 시도를 안한 상태이기 때문에 Abort 이면 안된다.                                                                                
                                                }
                                                break;
                                            }
                                        case OCRModeEnum.MANUAL:
                                            {
                                                WaferIDManualDialog.WaferIDManualInput.Show(this.GetLoaderContainer(), transferObj);
                                                if (transferObj.OCR.Value == null || transferObj.OCR.Value == "")
                                                {
                                                    errorCode = EventCodeEnum.NODATA;
                                                    transferObj.SetOCRState("", 0, OCRReadStateEnum.ABORT);// 이미 Manual Input에 실패했는데 다시 기회를 줄 이유 없음.                                                                                                             
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                                                    LoggerManager.Error($"WaferIDManualInput is Null Error");
                                                }
                                                else
                                                {
                                                    transferObj.SetOCRState(transferObj.OCR.Value, 0, OCRReadStateEnum.DONE);
                                                    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.READ_OCR;

                                                    LoggerManager.ActionLog(ModuleLogType.OCR, StateLogType.DONE, $"OCR Index: {pa.ID.Index}, Origin Location: {loaderModule.SlotToFoupConvert(transferObj.OriginHolder)}, WaferID: {transferObj.OCR.Value}");
                                                }
                                                break;
                                            }
                                        case OCRModeEnum.DEBUGGING:
                                            {
                                                break;
                                            }
                                        default:
                                            break;
                                    }


                                }
                                else
                                {
                                    //Task 이전에 다 처리함.

                                }
                            }
                            else
                            {
                                //이미 ocr을 완료한 상태
                            }

                            //if (transferObj.IsOCRDone() == false)//예외처리
                            //{
                            //    doPA = false;
                            //    LoggerManager.Debug("OCR TransferObejct is NULL");
                            //    transferObj.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                            //    cognexProcessManager.HostRunning[pa.ID.Index - 1] = EnumCognexModuleState.FAIL;
                            //}


                            if (doPA)//Load일때도 타고 Unload일때도 탐.
                            {
                                LoggerManager.Debug($"DoPreAlign Start NotchAngle:{dstNotchAngle}");
                                errorCode = loaderModule.PAManager.PAModules[pa.ID.Index - 1].DoPreAlign(dstNotchAngle);
                                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                                {
                                    errorCode = EventCodeEnum.NONE;
                                }
                                if (errorCode == EventCodeEnum.NONE)
                                {
                                    //transferObj.SetPreAlignDone(pa.ID);<-- 이걸 여기서 하면 NobufferMode일때 GP_PreAlignState를 가지고 올수 없어서 다음 job이 진행이 안됨.
                                }
                                else if (errorCode == EventCodeEnum.LOADER_FIND_NOTCH_FAIL ||
                                    errorCode == EventCodeEnum.MONITORING_PREALIGN_VAC_ERROR)
                                {
                                    this.NotifyManager().Notify(errorCode, pa.ID.Index);

                                }
                                else //errorCode != EventCodeEnum.NONE
                                {
                                    transferObj.CleanPreAlignState(reason: "PreAlign Failed.");
                                }

                            }

                            if (errorCode == EventCodeEnum.NONE)
                            {
                                LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.DONE, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN);
                            }
                            else
                            {
                                LoggerManager.UpdateLoaderMapHolderSubSequence(StateLogType.ERROR, arm.ID.Label, pa.ID.Label, sOrgin, pa.ID.Label, SubSequenceType.PRE_ALIGN, errorCode.ToString());
                            }

                        });
                    }
                    else
                    {
                        errorCode = EventCodeEnum.LOADER_PA_WAF_MISSED;
                        LoggerManager.Debug($"PAPutAsync(): TransferObject Cannot find. Arm:{arm.Holder.Status}, PA:{pa.Holder.Status}");
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                TransferObject transferObject = null;
                if (arm.Holder.TransferObject != null)
                {
                    transferObject = arm.Holder.TransferObject;
                }

                if (pa.Holder.TransferObject != null)
                {
                    transferObject = pa.Holder.TransferObject;
                }

                if (transferObject != null)
                {
                    if (transferObject.IsOCRDone() == false)
                    {
                        transferObject.OCRReadState = OCRReadStateEnum.ABORT;
                    }
                }


                loaderModule.ResonOfError = $"PAPutAync(): PA Put Error {errorCode}.";
                LoggerManager.Error($"PAPutAync(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        private string ProcessOcrString(string ocr, TransferObject transferobj)
        {
            var proc_ocr = ocr.ToString();
            try
            {
                if (proc_ocr.Count() > 0)
                {
                    if (transferobj.OCRDevParam.ConfigList.FirstOrDefault().CheckSum != "0")// CheckSum Disable
                    {
                        if (loaderModule.OCRConfig.RemoveCheckSum)
                        {
                            proc_ocr = proc_ocr.Substring(0, ocr.Length - 2);
                        }
                    }

                    if (loaderModule.OCRConfig.ReplaceDashToDot)
                    {
                        proc_ocr = proc_ocr.Replace("-", ".");
                    }

                    if (proc_ocr != ocr)
                    {
                        LoggerManager.Debug($"Changed proc_ocr:{ocr} => {proc_ocr}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return proc_ocr;
        }

        private bool WaferIdConfirm(TransferObject transferObj, string ocr)
        {
            bool isvalid = false;
            try
            {
                Stopwatch elapsedStopWatch = new Stopwatch();
                bool runflag = true;
                int timeout = loaderModule.LoaderMaster.GetWaitForWaferIdConfirmTimeout();

                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();

                LoggerManager.Debug($"WaferIdConfirm(): Wait Start. timeout:{timeout}");
                int foupNum = (transferObj.OriginHolder.Index - 1) / 25 + 1;
                int slotNum = (transferObj.OriginHolder.Index % 25 == 0) ? 25 : transferObj.OriginHolder.Index % 25;
                string predefinedId = loaderModule.LoaderMaster.GetPreDefindWaferId(foupNum, slotNum);

                do
                {
                    try
                    {
                        var wafer = loaderModule.LoaderMaster.Loader.ModuleManager.GetTransferObjectAll().Where(
                           item => item.OriginHolder.ModuleType == ModuleTypeEnum.SLOT &&
                                   item.OriginHolder.Index == transferObj.OriginHolder.Index
                           ).ToList();

                        if (wafer.Count > 0)
                        {
                            if (wafer.First().WFWaitFlag == false)
                            {
                                isvalid = true;
                                if (predefinedId == ocr)
                                {
                                    LoggerManager.Debug($"WaferIdConfirm(): Matched. ocr:{ocr}, Pre_OCRID:{predefinedId}");
                                }
                                else
                                {
                                    LoggerManager.Debug($"WaferIdConfirm(): UnMatched. ocr:{ocr}, Pre_OCRID:{predefinedId}");
                                }

                                break;
                            }
                            if (loaderModule.LoaderMaster.ActiveLotInfos[foupNum - 1].State == LotStateEnum.Cancel ||
                                   loaderModule.LoaderMaster.ActiveLotInfos[foupNum - 1].State == LotStateEnum.End ||
                                   loaderModule.LoaderMaster.ActiveLotInfos[foupNum - 1].State == LotStateEnum.Done)
                            {
                                isvalid = false;
                                LoggerManager.Debug($"WaferIdConfirm(): break, loaderModule.ModuleState:{loaderModule.ModuleState}");
                                break;
                            }

                        }


                        if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                        {
                            isvalid = false;
                            runflag = false;

                            PIVInfo pivinfo = new PIVInfo(foupnumber: foupNum);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            this.EventManager().RaisingEvent(typeof(OcrConfirmFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();
                            LoggerManager.Debug($"WaferIdConfirm(): command time out {timeout}");
                        }

                        Thread.Sleep(20);
                    }
                    catch (Exception err)
                    {
                        runflag = false;
                        LoggerManager.Exception(err);
                    }

                } while (runflag);

                LoggerManager.Debug($"WaferIdConfirm(): Wait End");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaferIdConfirm(): Exception occurred. Err = {err.Message}");
            }

            return isvalid;
        }


        public EventCodeEnum PARotateTo(IPreAlignModule pa, double notchAngle)
        {
            LoggerManager.Debug($"PARotateTo(): Source = {pa.ID.Label}, Angle = {notchAngle}");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PrealignWafer(IPreAlignModule pa, double notchAngle)
        {
            LoggerManager.Debug($"PrealignWafer(): Source = {pa.ID.Label}, Angle = {notchAngle}");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum OCRYAxisRelMove(int index, double dist)
        {
            LoggerManager.Debug($"OCR Move Up");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum OCRXAxisRelMove(int index, double dist)
        {
            LoggerManager.Debug($"OCR Move Left");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum OCRRotateAngle(int index, double degreee)
        {
            LoggerManager.Debug($"OCR Rotate Angle");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum CheckWaferIsOnPA(int index, out bool isExist)
        {
            LoggerManager.Debug($"Check Wafer is on PA");
            isExist = true;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LockCassette(int index)
        {
            LoggerManager.Debug($"LockCassette(): Lock #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum UnLockCassette(int index)
        {
            LoggerManager.Debug($"UnLockCassette(): UnLock #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DockingPortIn(int index)
        {
            LoggerManager.Debug($"DockingPortIn(): Docking port moved in #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DockingPortOut(int index)
        {
            LoggerManager.Debug($"DockingPortOut(): Docking port moved out #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CoverLock(int index)
        {
            LoggerManager.Debug($"CoverLock(): Cover Lock #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CoverUnLock(int index)
        {
            LoggerManager.Debug($"CoverUnLock(): Cover unlock #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CoverOpen(int index)
        {
            LoggerManager.Debug($"CoverOpen(): Cover open #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CoverClose(int index)
        {
            LoggerManager.Debug($"CoverClose(): Cover close #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Reset(int index)
        {
            LoggerManager.Debug($"Reset(): Reset #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum FOUPReset(int index)
        {
            LoggerManager.Debug($"FOUPReset(): Reset foup #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum WaitForCSTStatus(int index, EnumCSTCtrl cstState, long timeout = 0, EnumCSTCtrl cstCmd = 0)
        {
            LoggerManager.Debug($"WaitForCSTStatus(): Wait for state. #{index} cassette");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum WaitForOCR(IPreAlignModule pa, out string ocr, out double ocrScore)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            ocr = String.Empty;
            ocrScore = 0;
            try
            {
                var cognexprocessmanager = loaderModule.Container.Resolve<ICognexProcessManager>();
                retVal = cognexprocessmanager.WaitForOCR(pa.ID.Index - 1, cognexprocessmanager.CognexProcSysParam.WaitForOcrTimeout_msec);
                ocr = cognexprocessmanager.Ocr[pa.ID.Index - 1];
                ocrScore = cognexprocessmanager.OcrScore[pa.ID.Index - 1];
            }
            catch (Exception err)
            {
                LoggerManager.Error($"WaitForPA(): Exception occurred. Err = {err.Message}");
                throw err;
            }

            return retVal;
        }

        public void RaiseFoupModuleStateChanged(FoupModuleInfo moduleInfo)
        {
            if (this.loaderModule != null)
            {
                this.loaderModule.FOUP_RaiseFoupStateChanged(moduleInfo);
            }
        }

        public EventCodeEnum DRWPick(IInspectionTrayModule drw, IARMModule arm)
        {
            LoggerManager.Debug($"DRWPick(): Pick wafer from DRW#{drw.ID.Index}  with {arm.ID.Index} Arm");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DRWPut(IARMModule arm, IInspectionTrayModule drw)
        {
            LoggerManager.Debug($"DRWPut(): Put wafer to DRW#{drw.ID.Index}  with {arm.ID.Index} Arm");
            return EventCodeEnum.NONE;
        }

        public void LoaderLampSetState(ModuleStateEnum state)
        {
            return;
        }
        public void StageLampSetState(ModuleStateEnum state)
        {
            return;
        }
        public void LoaderBuzzer(bool isBuzzerOn)
        {
            return;
        }



        public EventCodeEnum CardMoveLoadingPosition(ICCModule CardChanger, ICardARMModule arm)
        {
            LoggerManager.Debug($"CardMoveLoadingPosition(): Move CardChagerIdx:#{CardChanger.ID.Index}  with {arm.ID.Index} Arm");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ChuckMoveLoadingPosition(IChuckModule Chuck, IARMModule arm)
        {
            LoggerManager.Debug($"ChuckMoveLoadingPosition(): Move Chuck:#{Chuck.ID.Index}  with {arm.ID.Index} Arm");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum RFIDReInitialize()
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (RFIDReader != null && RFIDReader.CommModule != null)
                {
                    RFIDReader.CommModule.DisConnect();
                }

                errorCode = InitRFIDModule_ForCardID();
                if (errorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[RFID] Init RFIDModule Failed. EventCodeEnum : {errorCode}");
                    return errorCode;
                }

                if (RFIDReader?.CommModule == null)
                {
                    LoggerManager.Debug($"[RFID] RFID ReInitialize Fail, CommModule is null.");
                    return errorCode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RFIDReInitialize(): Exception occurred. Err = {err.Message}");
            }
            return errorCode;
        }

        private EventCodeEnum InitRFIDModule_ForCardID()
        {
            // CardID를 읽기 위한 RFIDModule 추가
            RFIDReader = new RFIDModule(EnumRFIDModuleType.PROBECARD);
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            retval = RFIDReader.LoadSysParameter();
            if (retval != EventCodeEnum.NONE)
            {
                LoggerManager.Error($"RFIDModule LoadSysParameter() Failed. EventCodeEnum : {retval}");
                return retval;
            }

            retval = RFIDReader.InitModule();
            if (retval != EventCodeEnum.NONE)
            {
                LoggerManager.Error($"RFIDModule InitModule() Failed. EventCodeEnum : {retval}");
            }

            return retval;
        }

        public EnumCommunicationState GetRFIDCommState_ForCardID()
        {
            EnumCommunicationState retVal = EnumCommunicationState.UNAVAILABLE;
            try
            {
                if (RFIDReader == null)
                {
                    InitRFIDModule_ForCardID();
                }

                if (RFIDReader?.CommModule != null)
                {
                    retVal = RFIDReader.CommModule.GetCommState();
                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool IsRFIDDataReady()
        {
            bool retVal = false;
            try
            {
                if (RFIDReader == null)
                {
                    EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
                    errorCode = InitRFIDModule_ForCardID();
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[RFID] InitRFIDModule_ForCardID Fail. errorCode : {errorCode}");
                        return false;
                    }
                }
                if (RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.DISCONNECT
                    || RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.UNAVAILABLE)
                {
                    LoggerManager.Debug($"[RFID] CardID RFID CommModule is disconnected.");
                    return false;
                }
                else if(RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.CONNECTED)
                {
                    LoggerManager.Debug($"[RFID] CardID RFID CommModule is connected.");
                    return true;
                }
                else if(RFIDReader?.CommModule?.GetCommState() == EnumCommunicationState.EMUL)
                {
                    LoggerManager.Debug($"[RFID] CardID RFID CommModule is emul.");
                    return true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"IsRFIDDataReady(): Exception occurred. Err = {err.Message}");
                retVal = false;
            }
            return retVal;
        }

        public bool GetCardIDReadDataReady()
        {
            try
            {
                bool bRet = false;
                switch (SysParam.CardIDReaderType?.Value)
                {
                    case EnumCardIDReaderType.BARCODE:
                        bRet = false;//BCDReader.DataReady;
                        break;
                    case EnumCardIDReaderType.RFID:
                        bRet = IsRFIDDataReady();
                        break;
                    case EnumCardIDReaderType.DATETIME:
                        bRet = true;
                        break;
                    case EnumCardIDReaderType.NONE:
                    default:
                        break;
                }

                return bRet;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetCardIDReadDataReady(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }

        public string GetReceivedCardID()
        {
            try
            {
                string RetCardID = "";
                switch (SysParam.CardIDReaderType.Value)
                {
                    case EnumCardIDReaderType.BARCODE:
                        RetCardID = "NoBarcode";//BCDReader.ReceivedBCD;
                        break;
                    case EnumCardIDReaderType.RFID:
                        RetCardID = RFIDReader.RFID_cont_READID();
                        break;
                    case EnumCardIDReaderType.DATETIME:
                        RetCardID = DateTime.Now.ToString("yyyyMMddHHmmss");
                        break;
                    case EnumCardIDReaderType.NONE:
                    default:
                        break;
                }

                return RetCardID;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetReceivedCardID(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }

        private void ClearCardID()
        {
            try
            {
                switch (SysParam.CardIDReaderType.Value)
                {
                    case EnumCardIDReaderType.BARCODE:
                        //BCDReader.Clear();
                        break;
                    case EnumCardIDReaderType.RFID:
                    case EnumCardIDReaderType.DATETIME:
                    case EnumCardIDReaderType.NONE:
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetReceivedCardID(): Exception occurred. Err = {err.Message}");
                throw err;
            }
        }

        public EventCodeEnum CardIDMovePosition(CardHolder holder)
        {
            EventCodeEnum errorCode = EventCodeEnum.NONE;
            LoggerManager.Debug($"CardIDMovePosition(): Move CardIDPos");
            try
            {
                if (BCDReader != null)
                {
                    if (BCDReader.DataReady == true)
                    {
                        if (holder.TransferObject != null)
                        {
                            holder.TransferObject.ProbeCardID.Value = BCDReader.ReceivedBCD;
                        }
                        LoggerManager.Debug($"PC ID: {BCDReader.ReceivedBCD}");

                        if (string.IsNullOrEmpty(holder.TransferObject.ProbeCardID.Value) == false)
                        {
                            loaderModule.LoaderMaster.CardIDLastTwoWord = holder.TransferObject.ProbeCardID.Value.Substring(holder.TransferObject.ProbeCardID.Value.Length - 2, 2);
                            LoggerManager.Debug($"BCD Reading success. CardIDLastTwoWord: {loaderModule.LoaderMaster.CardIDLastTwoWord}");
                        }

                        BCDReader.Clear();
                    }
                }
                else
                {
                    holder.TransferObject.ProbeCardID.Value = loaderModule.LoaderMaster.CardIDFullWord;
                    if (holder.TransferObject.ProbeCardID.Value == null || holder.TransferObject.ProbeCardID.Value == "")
                    {
                        holder.TransferObject.ProbeCardID.Value = $"{holder.TransferObject.OriginHolder.Label}";
                    }

                    CardIDManualInput.Show(this.GetLoaderContainer(), holder.TransferObject);
                    if (holder.TransferObject == null)
                    {
                        errorCode = EventCodeEnum.NODATA;
                        LoggerManager.Error($"CardIDManualInput is Null Error");
                    }
                    else if (holder.TransferObject.ProbeCardID.Value == null || holder.TransferObject.ProbeCardID.Value == "")
                    {
                        errorCode = EventCodeEnum.NODATA;
                        LoggerManager.Error($"CardIDManualInput is Null Error");
                    }
                    else
                    {
                        string Cardid = holder.TransferObject.ProbeCardID.Value;
                        EventCodeEnum cardidvalidatyionresult = EventCodeEnum.NONE;
                        var ccsuper = loaderModule.LoaderMaster.GetLoaderContainer().Resolve<ICardChangeSupervisor>();
                        if (ccsuper != null)
                        {
                            cardidvalidatyionresult = ccsuper.CardinfoValidation(Cardid, out string Msg);
                            if (cardidvalidatyionresult != EventCodeEnum.NONE)
                            {
                                loaderModule.ResonOfError = Msg;
                                errorCode = cardidvalidatyionresult;
                            }
                        }
                        if (cardidvalidatyionresult == EventCodeEnum.NONE)
                        {
                            loaderModule.LoaderMaster.CardIDLastTwoWord = Cardid.Substring(Cardid.Length - 2, 2);
                            LoggerManager.Error($"CardIDManualInput is Show. CardIDLastTwoWord: {loaderModule.LoaderMaster.CardIDLastTwoWord}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                errorCode = EventCodeEnum.UNDEFINED;
                LoggerManager.Error($"CardIDManualInput is Null Error");
            }
            return errorCode;
        }
        public EventCodeEnum SetCardID(CardHolder holder, string cardid)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            try
            {
                if (holder.TransferObject != null)
                {
                    holder.TransferObject.ProbeCardID.Value = cardid;

                    if (string.IsNullOrEmpty(cardid) == false)
                    {
                        loaderModule.LoaderMaster.CardIDLastTwoWord = holder.TransferObject.ProbeCardID.Value.Substring(holder.TransferObject.ProbeCardID.Value.Length - 2, 2);
                        LoggerManager.Debug($"SetCardID. CardIDLastTwoWord: {loaderModule.LoaderMaster.CardIDLastTwoWord}");
                    }

                    errorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"SetCardID(): Exception occurred. Err = {err.Message}");
                throw err;
            }
            return errorCode;
        }

        public EventCodeEnum ResetRobotCommand()
        {
            LoggerManager.Debug($"ResetRobotCommand");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PAPick_NotVac(IPreAlignModule pa, IARMModule arm)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum PAPut_NotVac(IARMModule arm, IPreAlignModule pa)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetCardTrayVac(bool value)
        {
            return EventCodeEnum.NONE; ;
        }

        public EventCodeEnum CardTrayPick(ICardBufferTrayModule CCTray, ICardARMModule arm, int holderNum = -1)
        {
            return EventCodeEnum.NONE; ;
        }

        public EventCodeEnum CardTrayPut(ICardARMModule arm, ICardBufferTrayModule CCTray, int holderNum = -1)
        {
            return EventCodeEnum.NONE; ;
        }

        public SubstrateSizeEnum GetDeviceSize(int index)
        {
            SubstrateSizeEnum slotSize = loaderModule.GetDefaultWaferSize();
            LoggerManager.Debug($"GetDeviceSize(). Wafer size is set to default wafer size {slotSize}");
            return slotSize;
        }

        public EventCodeEnum SetBufferedMove(double xpos, double zpos, double wpos)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LockRobot()
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"LockRobot(): Loader robot Freeze(Emul).");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }
        public EventCodeEnum UnlockRobot()
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {

                LoggerManager.Debug($"LockRobot(): Loader robot release(Emul).");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }

        public EventCodeEnum SetTesterCoolantValve(int index, bool open)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                TesterCoolantValveOpened[index] = open;
                LoggerManager.Debug($"SetTesterCoolantValve(): Index = {index}, Open state = {open}.");
                eventCodeEnum = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }
        public EventCodeEnum GetTesterCoolantValveState(int index, out bool isopened)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            isopened = false;
            try
            {
                isopened = TesterCoolantValveOpened[index];
                LoggerManager.Debug($"GetTesterCoolantValveState(): Index = {index}, Open state = {isopened}.");
                eventCodeEnum = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }
        public EventCodeEnum CardTrayLock(bool locktray)
        {
            EventCodeEnum eventCodeEnum = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"CardTrayLock({locktray}): Loader robot Card Tray Lock/Unlock. Lock = {locktray}");
                eventCodeEnum = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventCodeEnum;
        }

        public EventCodeEnum ValveControl(EnumValveType valveType, int index, bool state)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                lock (utilBoxLockObject)
                {
                    switch (valveType)
                    {
                        case EnumValveType.IN:
                            retval = HandleValve(CoolantInletValveStates, index, state, "Coolant Inlet");
                            break;
                        case EnumValveType.OUT:
                            retval = HandleValve(CoolantOutletValveStates, index, state, "Coolant Outlet");
                            break;
                        case EnumValveType.PURGE:
                            retval = HandleValve(PurgeValveStates, index, state, "Coolant Purge");
                            break;
                        case EnumValveType.DRAIN:
                            retval = HandleValve(DrainValveStates, index, state, "Coolant Drain");
                            break;
                        case EnumValveType.DRYAIR:
                            retval = HandleValve(DryAirValveStates, index, state, "Dry Air");
                            this.GEMModule().GetPIVContainer().SetDryAirValveState(index, DryAirValveStates[index - 1]);
                            break;
                        default:
                            LoggerManager.Error($"Invalid valve type: {valveType}");
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum HandleValve(ObservableCollection<bool> valveStates, int index, bool state, string valveName)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var valveState = state ? "Opened" : "Closed";
                LoggerManager.Debug($"{valveName} Valve #{index} Set to {valveState} state");
                valveStates[index - 1] = state;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IRFIDModule GetRFIDReaderForCard()
        {
            IRFIDModule ret = null;
            try
            {
                if (RFIDReader != null)
                {
                    ret = RFIDReader;
                }
                else
                {
                    LoggerManager.Debug($"GetRFIDReaderForCard() RFIDReader is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
