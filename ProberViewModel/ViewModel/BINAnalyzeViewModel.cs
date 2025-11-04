using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.PnpSetup;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TCPIPParamObject;

namespace ProberViewModel.ViewModel
{
    public class BINAnalyzeViewModel : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        readonly Guid _ViewModelGUID = new Guid("aeac3349-2b87-451e-8d08-560cfc94dce3");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public bool Initialized { get; set; } = false;

        public Task<EventCodeEnum> InitViewModel()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private IProbingSequenceModule _ProbingSeqModule;
        public IProbingSequenceModule ProbingSeqModule
        {
            get { return _ProbingSeqModule; }
            set
            {
                if (value != _ProbingSeqModule)
                {
                    _ProbingSeqModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CurrentBINData = string.Empty;
        public string CurrentBINData
        {
            get { return _CurrentBINData; }
            set
            {
                if (value != _CurrentBINData)
                {
                    _CurrentBINData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set
            {
                if (value != _CoordinateManager)
                {
                    _CoordinateManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITCPIP _TCPIPModule;
        public ITCPIP TCPIPModule
        {
            get { return _TCPIPModule; }
            set
            {
                if (value != _TCPIPModule)
                {
                    _TCPIPModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Point _MXYIndex;
        public Point MXYIndex
        {
            get { return _MXYIndex; }
            set
            {
                if (value != _MXYIndex)
                {
                    _MXYIndex = value;
                    RaisePropertyChanged();
                    //AddCurSeq(false, (long)_MXYIndex.X, (long)_MXYIndex.Y);
                }
            }
        }

        private IZoomObject _ZoomObject;
        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set
            {
                if (value != _ZoomObject)
                {
                    _ZoomObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IDeviceObject> _UnderDutDies = new ObservableCollection<IDeviceObject>();
        public ObservableCollection<IDeviceObject> UnderDutDies
        {
            get { return _UnderDutDies; }
            set
            {
                if (value != _UnderDutDies)
                {
                    _UnderDutDies = value;

                    SetBINData();

                    RaisePropertyChanged();
                }
            }
        }

        private int _SequenceCount;
        public int SequenceCount
        {
            get { return _SequenceCount; }
            set
            {
                if (value != _SequenceCount)
                {
                    _SequenceCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrentSeqNumber;
        public int CurrentSeqNumber
        {
            get { return _CurrentSeqNumber; }
            set
            {
                if (value != _CurrentSeqNumber)
                {
                    _CurrentSeqNumber = value;
                    RaisePropertyChanged();
                    //UpdateUnderDutDies();
                }
            }
        }
        private List<MachineIndex> _Sequence = new List<MachineIndex>();
        public List<MachineIndex> Sequence
        {
            get { return _Sequence; }
            set
            {
                if (value != _Sequence)
                {
                    _Sequence = value;
                    SequenceCount = _Sequence.Count;
                    RaisePropertyChanged();
                }
            }
        }

        public async Task SetMXYIndex(object newVal)
        {
            try
            {
                if (newVal != null)
                {
                    Point value = (Point)newVal;
                    if ((value.X != _MXYIndex.X) | (value.Y != _MXYIndex.Y))
                    {
                        var retVal = await GetUnderDutDices(new MachineIndex((long)value.X, (long)value.Y));
                        if (retVal == EventCodeEnum.NONE)
                        {
                            MXYIndex = value;
                            RaisePropertyChanged(MXYIndex.ToString());
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private BinType _BinType;
        public BinType BinType
        {
            get { return _BinType; }
            set
            {
                if (value != _BinType)
                {
                    _BinType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _DutCount;
        public int DutCount
        {
            get { return _DutCount; }
            set
            {
                if (value != _DutCount)
                {
                    _DutCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void SetBINData()
        {
            try
            {
                if(UnderDutDies != null && UnderDutDies.Count > 0)
                {
                    // UnderDutDies는 같은 BIN DATA를 갖고 있다.

                    if(UnderDutDies.FirstOrDefault() != null)
                    {
                        if(UnderDutDies.FirstOrDefault().TestHistory != null &&
                            UnderDutDies.FirstOrDefault().TestHistory.Count > 0)
                        {
                            if(UnderDutDies.FirstOrDefault().TestHistory.LastOrDefault().BinData != null)
                            {
                                CurrentBINData = UnderDutDies.FirstOrDefault().TestHistory.LastOrDefault().BinData.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private async Task InitUnderDutDies()
        {
            try
            {
                if (Sequence.Count == 0)
                {
                    List<IDut> duts = new List<IDut>();

                    long xMinIndex = 0;
                    long xMaxIndex = 0;
                    long yMinIndex = 0;
                    long yMaxIndex = 0;

                    int dutWidthCount = 0;
                    int dutHeightCount = 0;

                    long teachdutXIndex = 0;
                    long teachdutYIndex = 0;

                    foreach (var dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        duts.Add(dut);
                    }
                    xMinIndex = duts.Min(dut => dut.MacIndex.XIndex);
                    xMaxIndex = duts.Max(dut => dut.MacIndex.XIndex);
                    yMinIndex = duts.Min(dut => dut.MacIndex.YIndex);
                    yMaxIndex = duts.Max(dut => dut.MacIndex.YIndex);

                    long sdutxindex = duts.Find(dut => dut.DutNumber == 1).MacIndex.XIndex;
                    long sdutyindex = duts.Find(dut => dut.DutNumber == 1).MacIndex.YIndex;

                    xMinIndex = Math.Abs(sdutxindex - xMinIndex);
                    xMaxIndex = Math.Abs(sdutxindex - xMaxIndex);
                    yMinIndex = Math.Abs(sdutyindex - yMinIndex);
                    yMaxIndex = Math.Abs(sdutyindex - yMaxIndex);

                    IWaferObject WaferObject = this.StageSupervisor().WaferObject;

                    double dieXcount = WaferObject.GetPhysInfo().MapCountX.Value;
                    double dieycount = WaferObject.GetPhysInfo().MapCountY.Value;
                    double hortestdiecount = 0;
                    double vertestdiecount = 0;
                    long _TeachDieXIndex;
                    long _TeachDieYIndex;

                    byte[,] wafermap = WaferObject.DevicesConvertByteArray();

                    for (int index = 0; index < wafermap.GetUpperBound(1) + 1; index++)
                    {
                        DieTypeEnum dietype = (DieTypeEnum)wafermap[WaferObject.GetPhysInfo().CenM.XIndex.Value - (dutWidthCount / 2), index];
                        if (dietype == DieTypeEnum.TEST_DIE)
                        {
                            vertestdiecount++;
                        }
                    }

                    for (int index = 0; index < wafermap.GetUpperBound(0) + 1; index++)
                    {
                        DieTypeEnum dietype = (DieTypeEnum)wafermap[index, WaferObject.GetPhysInfo().CenM.YIndex.Value - (dutHeightCount / 2)];
                        if (dietype == DieTypeEnum.TEST_DIE)
                        {
                            hortestdiecount++;
                        }
                    }

                    _TeachDieXIndex = Convert.ToInt64(hortestdiecount / 2);
                    _TeachDieYIndex = Convert.ToInt64(vertestdiecount / 2);

                    if (hortestdiecount - _TeachDieXIndex < xMaxIndex)
                    {
                        //CenterX 로부터 오른쪽 공간이 부족
                        //long offetx = (hortestdiecount - _TeachDieXIndex )- xMaxIndex;
                        //_TeachDieXIndex = _TeachDieXIndex - Math.Abs(offetx);

                        long offsetx = (xMaxIndex - xMinIndex + 1) / 2;
                        _TeachDieXIndex = _TeachDieXIndex - offsetx;
                    }

                    if (_TeachDieXIndex <= xMinIndex)
                    {
                        //CenterX 로부터 왼쪽 공간이 부족

                        long offsetx = (xMaxIndex - xMinIndex + 1) / 2;
                        _TeachDieXIndex = _TeachDieXIndex + offsetx;
                    }

                    if (vertestdiecount - _TeachDieYIndex < yMaxIndex)
                    {
                        //CetnerY로 부터 위 공간이 부족
                        long offsety = (yMinIndex - yMaxIndex + 1) / 2;
                        _TeachDieYIndex = _TeachDieYIndex - offsety;
                    }

                    if (_TeachDieYIndex <= yMinIndex)
                    {
                        //CetnerY로 부터 아래 공간이 부족
                        //long offsety = (_TeachDieYIndex - yMinIndex);
                        //_TeachDieYIndex = _TeachDieYIndex + offsety;
                        long offsety = (yMinIndex - yMaxIndex + 1) / 2;
                        _TeachDieYIndex = _TeachDieYIndex + offsety;
                    }

                    var die = StageSupervisor.WaferObject.GetDevices().Find(
                      d => d.DieIndexM.XIndex == _TeachDieXIndex
                      & d.DieIndexM.YIndex == _TeachDieYIndex);

                    await SetMXYIndex(new Point(_TeachDieXIndex, _TeachDieYIndex));
                    //MXYIndex = new Point(_TeachDieXIndex, _TeachDieYIndex);

                }
                else
                {
                    if (Sequence.Count != 0)
                    {
                        await GetUnderDutDices(new MachineIndex(Sequence[0].XIndex, Sequence[0].YIndex));
                        //MachineIndex mindex = GetFirstDutIndex();
                        //MapIndexX = mindex.XIndex;
                        //MapIndexY = mindex.YIndex;
                        if (UnderDutDies.Count == 0)
                        {
                            Sequence.Clear();
                            await InitUnderDutDies();
                            CurrentSeqNumber = 0;
                        }
                        else
                        {
                            CurrentSeqNumber = 1;
                            MXYIndex = new Point(UnderDutDies[0].DieIndexM.XIndex, UnderDutDies[0].DieIndexM.YIndex);
                        }

                        SequenceCount = Sequence.Count;
                    }
                    else
                    {
                        UnderDutDies = ProbingSeqModule.GetPreProbingSeq(1);
                        SequenceCount = Sequence.Count;
                        CurrentSeqNumber = 1;
                    }
                }

                //foreach (var dev in Sequence)
                //{
                //    await AddExistSeqForDev(dev);
                //}

                if (Sequence.Count == 0)
                {
                    List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();
                    foreach (var d in devs)
                    {
                        d.ExistSeq.Clear();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> GetUnderDutDices(MachineIndex mCoord)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<IDeviceObject> dev = new List<IDeviceObject>();

                var cardinfo = this.GetParam_ProbeCard();

                Task<EventCodeEnum> task = new Task<EventCodeEnum>(() =>
                {
                    if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                    {
                        for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                        {
                            IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));
                            IDeviceObject devobj = StageSupervisor.WaferObject.GetDevices().Find(
                                x => x.DieIndexM.Equals(retindex));
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

                        ObservableCollection<IDeviceObject> dutdevs = new ObservableCollection<IDeviceObject>();
                        if (dev.Count() > 0)
                        {

                            for (int devIndex = 0; devIndex < dev.Count; devIndex++)
                            {
                                if (dev[devIndex] != null)
                                    dutdevs.Add(dev[devIndex]);
                            }
                        }

                        UnderDutDies = dutdevs;

                        if (dev.Count != 0)
                        {
                            foreach (var ddie in dev)
                            {
                                if (this.GetParam_Wafer().GetDevices().Find(devobj => devobj.DieIndexM.Equals(ddie.DieIndexM)) != null)
                                {
                                    if (this.GetParam_Wafer().GetDevices().Find(devobj => devobj.DieIndexM.Equals(ddie.DieIndexM)).State.Value != DieStateEnum.NOT_EXIST)
                                    {
                                        RetVal = EventCodeEnum.NONE;
                                        break;
                                    }
                                    else
                                    {
                                        RetVal = EventCodeEnum.NODATA;
                                    }
                                }
                                else
                                {
                                    RetVal = EventCodeEnum.NODATA;
                                }
                            }
                        }
                        else
                        {
                            RetVal = EventCodeEnum.NODATA;
                        }
                    }
                    return RetVal;
                });
                task.Start();
                RetVal = await task;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.NODATA;
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                ZoomObject = StageSupervisor.WaferObject;

                Sequence = Extensions_IParam.Copy<List<MachineIndex>>(ProbingSeqModule.ProbingSeqParameter.ProbingSeq.Value);

                CurrentSeqNumber = 0;

                MXYIndex = new Point();

                if (Sequence.Count > 0)
                {
                    await SetMXYIndex(new Point(Sequence.First().XIndex, Sequence.First().YIndex));
                }
                else
                {
                    await InitUnderDutDies();
                }

                this.GetParam_Wafer().MapViewCurIndexVisiablity = true;
                this.GetParam_Wafer().MapViewStageSyncEnable = false;

                BinType = (TCPIPModule.TCPIPSysParam_IParam as TCPIPSysParam).EnumBinType.Value;
                DutCount = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.Count();

                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += SetMXYIndex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= SetMXYIndex;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ProbingSeqModule = this.ProbingSequenceModule();
                    CoordinateManager = this.CoordinateManager();
                    StageSupervisor = this.StageSupervisor();
                    TCPIPModule = this.TCPIPModule();

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

        private RelayCommand<Object> _BINDataClearComand;
        public ICommand BINDataClearComand
        {
            get
            {
                if (null == _BINDataClearComand) _BINDataClearComand = new RelayCommand<object>(BINDataClearComandFunc);
                return _BINDataClearComand;
            }
        }

        private void BINDataClearComandFunc(object obj)
        {
            try
            {
                (this.GetParam_Wafer().WaferDevObjectRef as WaferDevObject).InitMap();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        private AsyncCommand<SEQ_DIRECTION> _ChangeProbingSeqIndexCommand;
        public ICommand ChangeProbingSeqIndexCommand
        {
            get
            {
                if (null == _ChangeProbingSeqIndexCommand) _ChangeProbingSeqIndexCommand = new AsyncCommand<SEQ_DIRECTION>(ChangeProbingSeqIndexCommandFunc);
                return _ChangeProbingSeqIndexCommand;
            }
        }

        private async Task ChangeProbingSeqIndexCommandFunc(SEQ_DIRECTION obj)
        {
            bool IsChanged = false;
            MachineIndex ChangedIndex = null;

            try
            {
                if (obj == SEQ_DIRECTION.PREV)
                {
                    if (Sequence.Count != 0 & CurrentSeqNumber - 1 >= 0)
                    {
                        CurrentSeqNumber--;
                        IsChanged = true;
                    }
                    else
                    {
                        CurrentSeqNumber = Sequence.Count - 1;
                        IsChanged = true;
                    }
                }
                else if (obj == SEQ_DIRECTION.NEXT)
                {
                    if (Sequence.Count != 0 & CurrentSeqNumber + 1 < Sequence.Count)
                    {
                        CurrentSeqNumber++;
                        IsChanged = true;
                    }
                    else if (Sequence.Count != 0 & CurrentSeqNumber == Sequence.Count - 1)
                    {
                        CurrentSeqNumber = 0;
                        IsChanged = true;
                        
                    }
                }
                else
                {

                }

                if(IsChanged == true)
                {
                    ChangedIndex = Sequence[CurrentSeqNumber];
                    await GetUnderDutDices(ChangedIndex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
