using System;
using System.Collections.Generic;
using System.Linq;
using ProberInterfaces;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ProbingSequenceObject;
using ProberErrorCode;
using System.Runtime.CompilerServices;

using LogModule;
using RetestObject;
using ProberInterfaces.Retest;
using ProberInterfaces.BinData;

namespace ProbingSequenceManager
{


    public class ProbingSequenceModule : IProbingSequenceModule, IHasDevParameterizable
    {
        public bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public IProbingSequenceParameter ProbingSeqParameter { get; set; }

        private ObservableCollection<TransitionInfo> _TransitionInfo
            = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum GetNextSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = ProbingSequenceState.GetNextSequence(ref MI);

            return retVal;
        }

        public EventCodeEnum GetFirstSequence(ref MachineIndex MI)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MI = new MachineIndex();

                if (ProbingSeqParameter.ProbingSeq.Value.Count > 0)
                {
                    MI = ProbingSeqParameter.ProbingSeq.Value[0];

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum GetSequenceExistCurWafer()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MachineIndex MI = new MachineIndex();
                LotModeEnum lotModeEnum = this.LotOPModule().LotInfo.LotMode.Value;

                if (lotModeEnum == LotModeEnum.CP1 || lotModeEnum == LotModeEnum.CONTINUEPROBING)
                {
                    retVal = this.ProbingSequenceModule().GetFirstSequence(ref MI);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        retVal = EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR;
                    }
                }
                else if (lotModeEnum == LotModeEnum.MPP)
                {
                    for (int i = 0; i < this.ProbingSequenceModule().ProbingSequenceCount; i++)
                    {
                        MI = this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[i];

                        RetestDeviceParam retestDeviceParam = this.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                        if (retestDeviceParam.Retest_MPP.Mode.Value == ReTestMode.ALL)
                        {
                            if (this.ProbingModule().NeedRetest(MI.XIndex, MI.YIndex) == true)
                            {
                                retVal = EventCodeEnum.NONE;
                                break;
                            }
                            else
                            {
                                retVal = EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR;
                            }
                        }
                        else
                        {
                            if (this.ProbingModule().NeedRetestbyBIN(RetestTypeEnum.RETESTFORCP2, MI.XIndex, MI.YIndex) == true)
                            {
                                retVal = EventCodeEnum.NONE;
                                break;
                            }
                            else
                            {
                                retVal = EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum SetProbingInnerState(LotModeEnum mode)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                switch (mode)
                {
                    case LotModeEnum.CP1:
                        ProbingSequenceInnerState = new CircuitProbe1State(this);
                        break;
                    case LotModeEnum.MPP:
                        ProbingSequenceInnerState = new MultiPassProbeState(this);
                        break;
                    //case LotModeEnum.PASSDIEPROBING:
                    //    ProbingSequenceInnerState = new PassDieProbeState(this);
                    //    break;
                    case LotModeEnum.CONTINUEPROBING:
                        ProbingSequenceInnerState = new CircuitProbe1State(this);
                        break;
                    default:
                        ProbingSequenceInnerState = new UndefinedState(this);
                        break;
                }

                if (ProbingSequenceInnerState.GetState() == LotModeEnum.UNDEFINED)
                {
                    retval = EventCodeEnum.UNDEFINED;
                }
                else
                {
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        public int ProbingSequenceCount
        {
            get { return ProbingSeqParameter?.ProbingSeq?.Value?.Count ?? 0; }
        }

        private int _ProbingSequenceRemainCount;
        public int ProbingSequenceRemainCount
        {
            get { return _ProbingSequenceRemainCount; }
            set
            {
                if (value != _ProbingSequenceRemainCount)
                {
                    _ProbingSequenceRemainCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ProbingCP1OnlineRetestRemainCount;
        public int ProbingCP1OnlineRetestRemainCount
        {
            get { return _ProbingCP1OnlineRetestRemainCount; }
            set
            {
                if (value != _ProbingCP1OnlineRetestRemainCount)
                {
                    _ProbingCP1OnlineRetestRemainCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _ProbingMPPRetestRemainCount;
        public int ProbingMPPRetestRemainCount
        {
            get { return _ProbingMPPRetestRemainCount; }
            set
            {
                if (value != _ProbingMPPRetestRemainCount)
                {
                    _ProbingMPPRetestRemainCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<RetestComponent> _OnlineRetestList;
        public List<RetestComponent> OnlineRetestList
        {
            get { return _OnlineRetestList; }
            set
            {
                if (value != _OnlineRetestList)
                {
                    _OnlineRetestList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<MachineIndex> _RemainProbingSequence;
        public List<MachineIndex> RemainProbingSequence
        {
            get { return _RemainProbingSequence; }
            set
            {
                if (value != _RemainProbingSequence)
                {
                    _RemainProbingSequence = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<MachineIndex> _CP1OnlineRetestSeq;
        public List<MachineIndex> CP1OnlineRetestSeq
        {
            get { return _CP1OnlineRetestSeq; }
            set
            {
                if (value != _CP1OnlineRetestSeq)
                {
                    _CP1OnlineRetestSeq = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<MachineIndex> _MPPRetestSeq;
        public List<MachineIndex> MPPRetestSeq
        {
            get { return _MPPRetestSeq; }
            set
            {
                if (value != _MPPRetestSeq)
                {
                    _MPPRetestSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<MachineIndex> _ContinueRetestSeq;
        public List<MachineIndex> ContinueRetestSeq
        {
            get { return _ContinueRetestSeq; }
            set
            {
                if (value != _ContinueRetestSeq)
                {
                    _ContinueRetestSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbingSequenceState _ProbingSequenceState;
        public ProbingSequenceState ProbingSequenceState
        {
            get { return _ProbingSequenceState; }
            set
            {
                _ProbingSequenceState = value;
            }
        }

        private ProbingSequenceInnerStateBase _ProbingSequenceInnerState;
        public ProbingSequenceInnerStateBase ProbingSequenceInnerState
        {
            get { return _ProbingSequenceInnerState; }
            set
            {
                _ProbingSequenceInnerState = value;
            }
        }

        public ProbingSequenceStateEnum GetProbingSequenceState()
        {
            return ProbingSequenceState.GetState();
        }
        //public EventCodeEnum ResetState()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = ResetProbingSequence();

        //        //if (ProbingSequenceCount > 0)
        //        //{
        //        //    ProbingSequenceState = new ProbingSequenceSRState(this);
        //        //}
        //        //else
        //        //{
        //        //    ProbingSequenceState = new ProbingSequenceNMSRState(this);
        //        //}
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}

        private ModuleStateBase _ModuleState;

        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }


        private IWaferObject Wafer { get; set; }

        public void ProbingSequenceStateTransition(ProbingSequenceState state)
        {
            try
            {
                ProbingSequenceState = state;

                if (state.GetType() == typeof(ProbingSequenceSRState))
                {

                }
                else if (state.GetType() == typeof(ProbingSequenceNMSRState))
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }

        public EventCodeEnum ResetProbingSequence()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Wafer = this.GetParam_Wafer();

                if (ProbingSeqParameter.ProbingSeq.Value != null)
                {
                    _ProbingSequenceRemainCount = ProbingSequenceCount;
                    CP1OnlineRetestSeq = new List<MachineIndex>();
                    MPPRetestSeq = new List<MachineIndex>();
                    OnlineRetestList = new List<RetestComponent>();

                    RetestDeviceParam retestDeviceParam = this.RetestModule().RetestModuleDevParam_IParam as RetestDeviceParam;

                    LotModeEnum lotModeEnum = this.LotOPModule().LotInfo.LotMode.Value;

                    if (lotModeEnum == LotModeEnum.CP1)
                    {
                        _ProbingSequenceRemainCount = ProbingSequenceCount;
                        _ProbingMPPRetestRemainCount = 0;

                        if (retestDeviceParam != null)
                        {
                            foreach (var item in retestDeviceParam.Retest_Online_List)
                            {
                                if (item.Enable.Value == true && this.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.CP1)
                                {
                                    _ProbingCP1OnlineRetestRemainCount = ProbingSequenceCount;
                                    OnlineRetestList.Add(item);
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[ProbingSequenceModule], ResetProbingSequence() : LotMode = {lotModeEnum}, RetestDeviceParam param is null.");
                        }
                    }
                    else if (lotModeEnum == LotModeEnum.MPP)
                    {
                        _ProbingSequenceRemainCount = 0;
                        _ProbingCP1OnlineRetestRemainCount = 0;
                        _ProbingMPPRetestRemainCount = ProbingSequenceCount;
                    }
                    else if (lotModeEnum == LotModeEnum.CONTINUEPROBING)
                    {
                        _ProbingSequenceRemainCount = ProbingSequenceCount;
                        _ProbingMPPRetestRemainCount = 0;

                        if (retestDeviceParam != null)
                        {
                            foreach (var item in retestDeviceParam.Retest_Online_List)
                            {
                                if (item.Enable.Value == true && this.LotOPModule().LotInfo.LotMode.Value == LotModeEnum.CP1)
                                {
                                    _ProbingCP1OnlineRetestRemainCount = ProbingSequenceCount;
                                    OnlineRetestList.Add(item);
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[ProbingSequenceModule], ResetProbingSequence() : LotMode = {lotModeEnum}, RetestDeviceParam param is null.");
                        }
                    }
                    else
                    {
                        //Error
                    }
                }

                if (ProbingSequenceCount > 0)
                {
                    ProbingSequenceState = new ProbingSequenceSRState(this);
                }
                else
                {
                    ProbingSequenceState = new ProbingSequenceNMSRState(this);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ProbingSequenceTransfer(int idx)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (ProbingSeqParameter.ProbingSeq.Value != null)
            {
                _ProbingSequenceRemainCount++;

                //if (ProbingSequenceCount <= _ProbingSequenceRemainCount + idx)
                //{
                //    _ProbingSequenceRemainCount = ProbingSequenceCount;
                //}
                //else if (0 >= _ProbingSequenceRemainCount + idx)
                //{
                //    _ProbingSequenceRemainCount = 0;
                //}
                //else
                //{
                //    _ProbingSequenceRemainCount += idx;
                //}
            }

            return retval;
        }

        public EventCodeEnum UpdateProbingStateSequence()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Wafer = this.GetParam_Wafer();

                ResetProbingSequence();

                if (ProbingSequenceCount > 0)
                {
                    ProbingSequenceState = new ProbingSequenceSRState(this);
                }
                else
                {
                    ProbingSequenceState = new ProbingSequenceNMSRState(this);
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {


                    ResetProbingSequence();

                    if (ProbingSequenceCount > 0)
                    {
                        ProbingSequenceState = new ProbingSequenceSRState(this);
                    }
                    else
                    {
                        ProbingSequenceState = new ProbingSequenceNMSRState(this);
                    }

                    Initialized = true;
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

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<int> BinCodes { get; set; }

        public EnumProbingJobResult ValidateJobResult(List<DeviceObject> testeddevices)
        {
            EnumProbingJobResult jobResult = EnumProbingJobResult.UNKNOWN;
            try
            {

                if (testeddevices.Count > 0)
                {
                    jobResult = EnumProbingJobResult.PROB_PROGRESSING;
                }
                else
                {
                    jobResult = EnumProbingJobResult.PROB_NOT_PERFORMED;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return jobResult;
        }

        public List<MachineIndex> MakeProbingSequence(int SeqDir)
        {
            var wafer = this.GetParam_Wafer();
            var card = this.GetParam_ProbeCard();

            try
            {
                if (SeqDir == 1)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq(card, true, true, 1, false, false);
                }
                else if (SeqDir == 2)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq(card, false, true, 1, false, false);
                }
                else if (SeqDir == 3)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq(card, false, false, 1, false, false);
                }
                else if (SeqDir == 4)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq(card, true, false, 1, false, false);
                }
                else if (SeqDir == 5)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq_hor(card, true, false, 1, false);
                }
                else if (SeqDir == 6)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq_hor(card, true, true, 1, false);
                }
                else if (SeqDir == 7)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq_hor(card, false, true, 1, false);
                }
                else if (SeqDir == 8)
                {
                    ProbingSeqParameter.ProbingSeq.Value = wafer.makeMultiProbeSeq_hor(card, false, false, 1, false);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, $"LoadProbingSequence(SeqDir = {SeqDir})");
                LoggerManager.Exception(err);
            }

            return ProbingSeqParameter.ProbingSeq.Value;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(ProbingSeqParameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ProbingSeqParameter = tmpParam as ProbingSeqParameters;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(ProbingSeqParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetProbingSequence(List<MachineIndex> seq)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbingSeqParameter.ProbingSeq.Value = seq;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ObservableCollection<IDeviceObject> GetNextProbingSeq(int curseqindex)
        {
            ObservableCollection<IDeviceObject> devices = null;
            try
            {
                if (curseqindex + 1 >= 0 & curseqindex + 1 < this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value.Count)
                {
                    //ObservableCollection<IDeviceObject> dutdies = new ObservableCollection<IDeviceObject>();
                    //long currSeqRefDieXIndex =
                    //    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[curseqindex+1].XIndex;
                    //long currSeqRefDieYIndex =
                    //    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[curseqindex+1].YIndex;
                    ////UnderDutDies.Clear();
                    //int dutIndex = 0;
                    //foreach (var dut in this.StageSupervisor().ProbeCardInfo.DutList)
                    //{
                    //    var die = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(
                    //   d => d.DieIndexM.XIndex == currSeqRefDieXIndex + dut.MacIndex.XIndex
                    //   & d.DieIndexM.YIndex == currSeqRefDieYIndex + dut.MacIndex.YIndex);

                    //    if (die != null)
                    //    {
                    //        die.CurTestHistory.DutIndex.Value = dutIndex;
                    //        devices.Add(die);

                    //        dutIndex++;
                    //    }


                    //}
                    devices = GetUnderDutDices(this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[curseqindex + 1]);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return devices;
        }
        public ObservableCollection<IDeviceObject> GetPreProbingSeq(int curseqindex)
        {
            ObservableCollection<IDeviceObject> devices = null;
            try
            {
                if (curseqindex - 1 >= 0 & curseqindex - 1 < this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value.Count)
                {
                    //ObservableCollection<IDeviceObject> dutdies = new ObservableCollection<IDeviceObject>();
                    //long currSeqRefDieXIndex =
                    //    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[curseqindex-1].XIndex;
                    //long currSeqRefDieYIndex =
                    //    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[curseqindex-1].YIndex;
                    //UnderDutDies.Clear();
                    //int dutIndex = 0;
                    //foreach (var dut in this.StageSupervisor().ProbeCardInfo.DutList)
                    //{
                    //    var die = Wafer.GetSubsInfo().Devices.ToList<DeviceObject>().Find(
                    //   d => d.DieIndexM.XIndex == currSeqRefDieXIndex + dut.MacIndex.XIndex
                    //   & d.DieIndexM.YIndex == currSeqRefDieYIndex + dut.MacIndex.YIndex);

                    //    if (die != null)
                    //    {
                    //        die.CurTestHistory.DutIndex.Value = dutIndex;
                    //        devices.Add(die);

                    //        dutIndex++;
                    //    }


                    //}
                    devices = GetUnderDutDices(this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.Value[curseqindex - 1]);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return devices;
        }


        public ObservableCollection<IDeviceObject> GetUnderDutDices(MachineIndex mCoord)
        {

            ObservableCollection<IDeviceObject> dev = new ObservableCollection<IDeviceObject>();

            var cardinfo = this.GetParam_ProbeCard();
            Wafer = this.GetParam_Wafer();
            try
            {
                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                    {
                        IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));
                        IDeviceObject devobj = Wafer.GetDevices().Find(x => x.DieIndexM.Equals(retindex));
                        if (devobj != null)
                        {
                            dev.Add(devobj);
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                        }
                        else
                        {
                            devobj = new DeviceObject();
                            devobj.DieIndexM.XIndex = retindex.XIndex;
                            devobj.DieIndexM.YIndex = retindex.YIndex;
                            dev.Add(devobj);
                            dev[dev.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[dutIndex].DutNumber;
                        }
                    }
                    if (dev.Count() > 0)
                    {

                        System.Collections.ObjectModel.ObservableCollection<IDeviceObject> dutdevs = new ObservableCollection<IDeviceObject>();
                        for (int devIndex = 0; devIndex < dev.Count; devIndex++)
                        {
                            if (dev[devIndex] != null)
                                dutdevs.Add(dev[devIndex]);
                        }
                        dev = dutdevs;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return dev;
        }
    }
}
