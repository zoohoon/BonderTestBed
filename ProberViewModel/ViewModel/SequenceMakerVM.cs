
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProberViewModel
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using RelayCommandBase;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using AutoSequenceMakerControl;
    using System.Runtime.CompilerServices;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
    using MetroDialogInterfaces;

    public class SequenceMakerVM : IMainScreenViewModel, INotifyPropertyChanged, ISequenceMakerVM
    {
        readonly Guid _ViewModelGUID = new Guid("C3BC83A1-C6CA-4BB4-0DE2-34DF6EA06DF2");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler MXYIndexPropertyChanged;
        protected void RaiseMXYIndexPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (MXYIndexPropertyChanged != null)
                MXYIndexPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Initialized { get; set; } = false;

        private IProberStation _Prober;
        public IProberStation Prober
        {
            get { return _Prober; }
            set
            {
                if (value != _Prober)
                {
                    _Prober = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region //..Binding Property

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

        public IStageSupervisor StageSupervisor { get { return this.StageSupervisor(); } }

        public async Task SetMXYIndex(object newVal)
        {
            try
            {
                if (newVal != null)
                {
                    Point value = (Point)newVal;

                    if ((value.X != _MXYIndex.X) || (value.Y != _MXYIndex.Y))
                    {
                        var retVal = await GetUnderDutDices(new MachineIndex((long)value.X, (long)value.Y));

                        MXYIndex = value;
                        RaisePropertyChanged(MXYIndex.ToString());

                        await AddCurSeq(false, (long)_MXYIndex.X, (long)_MXYIndex.Y);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                }
            }
        }

        private MachineIndex _CurrSeqRefDieIndex = new MachineIndex();
        public MachineIndex CurrSeqRefDieIndex
        {
            get { return _CurrSeqRefDieIndex; }
            set
            {
                if (value != _CurrSeqRefDieIndex)
                {
                    _CurrSeqRefDieIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<IDeviceObject> _Devices;
        public List<IDeviceObject> Devices
        {
            get { return _Devices; }
            set
            {
                if (value != _Devices)
                {
                    _Devices = value;
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
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AutoAddSeqEnable;
        public bool AutoAddSeqEnable
        {
            get { return _AutoAddSeqEnable; }
            set
            {
                if (value != _AutoAddSeqEnable)
                {
                    _AutoAddSeqEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AutoSequenceContorl _AutoSeqControl;
        #endregion

        private async Task AddCurSeq(bool autoflag = true, long xindex = -1, long yindex = -1)
        {
            try
            {
                if (AutoAddSeqEnable)
                {
                    await InsertSeq(autoflag, xindex, yindex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task InitUnderDutDiesEmptySequence()
        {
            try
            {
                List<IDut> duts = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.ToList();

                long xMinIndex = 0;
                long xMaxIndex = 0;
                long yMinIndex = 0;
                long yMaxIndex = 0;

                int dutWidthCount = 0;
                int dutHeightCount = 0;

                long teachdutXIndex = 0;
                long teachdutYIndex = 0;

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

                int waferMapXLength = wafermap.GetUpperBound(0) + 1;
                int waferMapYLength = wafermap.GetUpperBound(1) + 1;

                for (int index = 0; index < waferMapYLength; index++)
                {
                    DieTypeEnum dietype = (DieTypeEnum)wafermap[WaferObject.GetPhysInfo().CenM.XIndex.Value - (dutWidthCount / 2), index];

                    if (dietype == DieTypeEnum.TEST_DIE)
                    {
                        vertestdiecount++;
                    }
                }

                for (int index = 0; index < waferMapXLength; index++)
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
                    long offsety = (yMinIndex - yMaxIndex + 1) / 2;
                    _TeachDieYIndex = _TeachDieYIndex + offsety;
                }

                var die = StageSupervisor.WaferObject.GetDevices().Find(d => d.DieIndexM.XIndex == _TeachDieXIndex && d.DieIndexM.YIndex == _TeachDieYIndex);

                await SetMXYIndex(new Point(_TeachDieXIndex, _TeachDieYIndex));

                List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();

                List<IDeviceObject> deviceObjects = new List<IDeviceObject>();

                foreach (var dev in Sequence)
                {
                    var retrievedObjects = await AddExistSeqForDev(dev, devs);
                    deviceObjects.AddRange(retrievedObjects);
                }

                UpdateDeviceObject(deviceObjects, true);

                if (Sequence.Count == 0)
                {
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
        private async Task InitUnderDutDiesWithSequence()
        {
            try
            {
                await GetUnderDutDices(new MachineIndex(Sequence[0].XIndex, Sequence[0].YIndex));

                if (UnderDutDies.Count == 0)
                {
                    Sequence.Clear();
                    await InitUnderDutDiesEmptySequence();
                    CurrentSeqNumber = 0;
                }
                else
                {
                    CurrentSeqNumber = 1;
                    MXYIndex = new Point(UnderDutDies[0].DieIndexM.XIndex, UnderDutDies[0].DieIndexM.YIndex);
                }

                SequenceCount = Sequence.Count;

                List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();
                List<IDeviceObject> deviceObjects = new List<IDeviceObject>();

                foreach (var dev in Sequence)
                {
                    var retrievedObjects = await AddExistSeqForDev(dev, devs);
                    deviceObjects.AddRange(retrievedObjects);
                }

                UpdateDeviceObject(deviceObjects, true);

                if (Sequence.Count == 0)
                {
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
        private async Task InitUnderDutDies()
        {
            try
            {
                if (Sequence.Count == 0)
                {
                    await InitUnderDutDiesEmptySequence();
                }
                else
                {
                    await InitUnderDutDiesWithSequence();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public SequenceMakerVM()
        {
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

        private AsyncCommand _InsertSeqCommand;
        public ICommand InsertSeqCommand
        {
            get
            {
                if (null == _InsertSeqCommand) _InsertSeqCommand = new AsyncCommand(InsertSeq);
                return _InsertSeqCommand;
            }
        }
        public async Task InsertSeq()
        {
            try
            {
                int index = Sequence.FindIndex(dev => dev.XIndex == MXYIndex.X && dev.YIndex == MXYIndex.Y);
                List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();

                if (index != -1)
                {
                    Sequence.Insert(index, new MachineIndex((long)MXYIndex.X, (long)MXYIndex.Y));

                    var retrievedObjects = await AddExistSeqForDev(Sequence[index], devs);
                    UpdateDeviceObject(retrievedObjects, true);

                    CurrentSeqNumber = index + 1;
                    SequenceCount = Sequence.Count;
                }
                else
                {
                    Sequence.Add(new MachineIndex((long)MXYIndex.X, (long)MXYIndex.Y));

                    var retrievedObjects = await AddExistSeqForDev(Sequence[Sequence.Count - 1], devs);
                    UpdateDeviceObject(retrievedObjects, true);

                    SequenceCount = Sequence.Count;
                    CurrentSeqNumber = Sequence.Count;
                }

                if (Sequence.Count != 0)
                {
                    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task InsertSeq(bool autoflag = true, long xindex = -1, long yindex = -1)
        {
            try
            {
                List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();

                if (autoflag)
                {
                    int index = Sequence.FindIndex(dev => dev.XIndex == MXYIndex.X && dev.YIndex == MXYIndex.Y);

                    if (index != -1)
                    {
                        Sequence.Insert(index, new MachineIndex((long)MXYIndex.X, (long)MXYIndex.Y));
                        CurrentSeqNumber = index;

                        var retrievedObjects = await AddExistSeqForDev(Sequence[index], devs);
                        UpdateDeviceObject(retrievedObjects, true);
                    }
                    else
                    {
                        Sequence.Add(new MachineIndex((long)MXYIndex.X, (long)MXYIndex.Y));

                        var retrievedObjects = await AddExistSeqForDev(Sequence[Sequence.Count - 1], devs);
                        UpdateDeviceObject(retrievedObjects, true);

                        SequenceCount = Sequence.Count;
                        CurrentSeqNumber = Sequence.Count;
                    }
                }
                else
                {
                    int index = Sequence.FindIndex(dev => dev.XIndex == xindex && dev.YIndex == yindex);

                    if (index != -1)
                    {
                        Sequence.Insert(index, new MachineIndex(xindex, yindex));
                        CurrentSeqNumber = index;

                        var retrievedObjects = await AddExistSeqForDev(Sequence[index], devs);
                        UpdateDeviceObject(retrievedObjects, true);
                    }
                    else
                    {
                        Sequence.Add(new MachineIndex(xindex, yindex));

                        var retrievedObjects = await AddExistSeqForDev(Sequence[Sequence.Count - 1], devs);
                        UpdateDeviceObject(retrievedObjects, true);

                        SequenceCount = Sequence.Count;
                        CurrentSeqNumber = Sequence.Count;
                    }
                }

                if (Sequence.Count != 0)
                {
                    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand _DeleteSeqCommand;
        public ICommand DeleteSeqCommand
        {
            get
            {
                if (null == _DeleteSeqCommand)
                {
                    _DeleteSeqCommand = new AsyncCommand(DeleteSeq);
                }

                return _DeleteSeqCommand;
            }
        }
        public async Task DeleteSeq()
        {
            try
            {
                if (Sequence.Count != 0)
                {
                    bool enableflag = AutoAddSeqEnable;

                    if (enableflag)
                    {
                        AutoAddSeqEnable = false;
                    }

                    MachineIndex idx = Sequence[CurrentSeqNumber - 1];

                    Sequence.Remove(Sequence[CurrentSeqNumber - 1]);

                    SequenceCount = Sequence.Count;

                    if (CurrentSeqNumber >= 2)
                    {
                        //이전의 Seq가있는경우
                        MoveToPrevSeq();
                    }
                    else if (CurrentSeqNumber <= Sequence.Count)
                    {
                        await GetUnderDutDices(Sequence[CurrentSeqNumber - 1]);
                    }
                    else
                    {
                        CurrentSeqNumber = 0;
                    }

                    List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();

                    var retrievedObjects = RemoveExistSeqForDev(idx, devs);
                    UpdateDeviceObject(retrievedObjects, false);

                    if (enableflag)
                    {
                        AutoAddSeqEnable = true;
                    }
                }

                if (Sequence.Count != 0)
                {
                    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _DeleteAllSeqCommand;
        public IAsyncCommand DeleteAllSeqCommand
        {
            get
            {
                if (null == _DeleteAllSeqCommand)
                {
                    _DeleteAllSeqCommand = new AsyncCommand(DeleteAllSeq);
                }

                return _DeleteAllSeqCommand;
            }
        }
        public async Task DeleteAllSeq()
        {
            try
            {

                EnumMessageDialogResult mret = await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.WarningMessageTitle, Properties.Resources.DeleteAllSeqMessage, EnumMessageStyle.AffirmativeAndNegative);

                if (mret == EnumMessageDialogResult.NEGATIVE)
                {
                    return;
                }

                if (Sequence != null && Sequence.Count > 0)
                {
                    if (AutoAddSeqEnable)
                    {
                        AutoAddSeqEnable = false;
                    }

                    List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();
                    List<IDeviceObject> deviceObjects = new List<IDeviceObject>();

                    foreach (var seq in Sequence)
                    {
                        var retrievedObjects = RemoveExistSeqForDev(seq, devs);
                        deviceObjects.AddRange(retrievedObjects);
                    }
                    UpdateDeviceObject(deviceObjects, false);

                    List<MachineIndex> defaultseq = new List<MachineIndex>();
                    Sequence.Clear();

                    SequenceCount = 0;

                    UnderDutDies = new ObservableCollection<IDeviceObject>();

                    CurrentSeqNumber = 0;

                    InitUnderDutDies();
                }

                if (Sequence.Count != 0)
                {
                    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _AutoMakeSeqCommand;
        public IAsyncCommand AutoMakeSeqCommand
        {
            get
            {
                if (null == _AutoMakeSeqCommand)
                {
                    _AutoMakeSeqCommand = new AsyncCommand(AutoMakeSeq);
                }

                return _AutoMakeSeqCommand;
            }
        }
        public async Task AutoMakeSeq()
        {
            await this.MetroDialogManager().ShowWindow(_AutoSeqControl);

            if (Sequence.Count != 0)
            {
                this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
            }
        }

        private AsyncCommand _MoveToNextSeqCommand;
        public ICommand MoveToNextSeqCommand
        {
            get
            {
                if (null == _MoveToNextSeqCommand)
                {
                    _MoveToNextSeqCommand = new AsyncCommand(MoveToNextSeq);
                }

                return _MoveToNextSeqCommand;
            }
        }

        private AsyncCommand _MoveToPrevSeqCommand;
        public ICommand MoveToPrevSeqCommand
        {
            get
            {
                if (null == _MoveToPrevSeqCommand)
                {
                    _MoveToPrevSeqCommand = new AsyncCommand(MoveToPrevSeq);
                }

                return _MoveToPrevSeqCommand;
            }
        }

        public async Task MoveToPrevSeq()
        {
            try
            {
                if (Sequence.Count != 0 & CurrentSeqNumber - 1 > 0)
                {
                    await GetUnderDutDices(Sequence[CurrentSeqNumber - 2]);

                    if (UnderDutDies != null)
                    {
                        CurrentSeqNumber--;
                    }
                }
                else if (CurrentSeqNumber == 1)
                {
                    await GetUnderDutDices(Sequence[0]);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task MoveToNextSeq()
        {
            try
            {
                if (Sequence.Count != 0 & CurrentSeqNumber + 1 <= Sequence.Count)
                {
                    await GetUnderDutDices(Sequence[CurrentSeqNumber]);

                    if (UnderDutDies != null)
                    {
                        CurrentSeqNumber++;
                    }
                }
                else if (CurrentSeqNumber == 1)
                {
                    await GetUnderDutDices(Sequence[0]);
                }
                else if (Sequence.Count != 0 & CurrentSeqNumber == Sequence.Count)
                {
                    await GetUnderDutDices(Sequence[Sequence.Count - 1]);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _MapMoveCommand;
        public ICommand MapMoveCommand
        {
            get
            {
                if (null == _MapMoveCommand)
                {
                    _MapMoveCommand = new AsyncCommand<object>(MapMove, false);
                }

                return _MapMoveCommand;
            }
        }

        public async Task MapMove(object parameter)
        {
            try
            {
                int direction = Convert.ToInt32(parameter);

                int increment = 1;
                if(this.VisionManager().GetDispHorFlip()== DispFlipEnum.FLIP && this.VisionManager().GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    increment = -1;
                }

                switch (direction)
                {
                    case 1: //LeftUpper
                        await SetMXYIndex(new Point(MXYIndex.X - increment, MXYIndex.Y + increment));
                        break;
                    case 2: //Upper
                        await SetMXYIndex(new Point(MXYIndex.X, MXYIndex.Y + increment));
                        break;
                    case 3: //RightUpper
                        await SetMXYIndex(new Point(MXYIndex.X + increment, MXYIndex.Y + increment));
                        break;
                    case 4: //Left
                        await SetMXYIndex(new Point(MXYIndex.X - increment, MXYIndex.Y));
                        break;
                    case 5: //Right
                        await SetMXYIndex(new Point(MXYIndex.X + increment, MXYIndex.Y));
                        break;
                    case 6: //LeftLower
                        await SetMXYIndex(new Point(MXYIndex.X - increment, MXYIndex.Y - increment));
                        break;
                    case 7: //Lower
                        await SetMXYIndex(new Point(MXYIndex.X, MXYIndex.Y - increment));
                        break;
                    case 8: //RightLower
                        await SetMXYIndex(new Point(MXYIndex.X + increment, MXYIndex.Y - increment));
                        break;

                    default:
                        break;
                }

                this.LoaderRemoteMediator()?.GetServiceCallBack()?.SequenceMakerVM_SetMXYIndex(_MXYIndex);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ChangeTestDieCommand;
        public ICommand ChangeTestDieCommand
        {
            get
            {
                if (null == _ChangeTestDieCommand)
                {
                    _ChangeTestDieCommand = new RelayCommand(ChangeTestDie);
                }

                return _ChangeTestDieCommand;
            }
        }

        private void ChangeTestDie()
        {
            try
            {
                Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y).State.Value = DieStateEnum.TESTED;

                Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y).CurTestHistory.BinCode.Value = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ChangeMarkDieCommand;
        public ICommand ChangeMarkDieCommand
        {
            get
            {
                if (null == _ChangeMarkDieCommand)
                {
                    _ChangeMarkDieCommand = new RelayCommand(ChangeMarkDie);
                }

                return _ChangeMarkDieCommand;
            }
        }

        private void ChangeMarkDie()
        {
            try
            {
                Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y).State.Value = DieStateEnum.MARK;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ChangeNotExistDieCommand;
        public ICommand ChangeNotExistDieCommand
        {
            get
            {
                if (null == _ChangeNotExistDieCommand)
                {
                    _ChangeNotExistDieCommand = new RelayCommand(ChangeNotExistDie);
                }

                return _ChangeNotExistDieCommand;
            }
        }

        private void ChangeNotExistDie()
        {
            try
            {
                Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y).State.Value = DieStateEnum.NOT_EXIST;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _ChangeSkipDieCommand;
        public ICommand ChangeSkipDieCommand
        {
            get
            {
                if (null == _ChangeSkipDieCommand)
                {
                    _ChangeSkipDieCommand = new RelayCommand(ChangeSkipDie);
                }

                return _ChangeSkipDieCommand;
            }
        }

        private void ChangeSkipDie()
        {
            try
            {
                Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y).State.Value = DieStateEnum.SKIPPED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _ChangePassDieCommand;
        public ICommand ChangePassDieCommand
        {
            get
            {
                if (null == _ChangePassDieCommand)
                {
                    _ChangePassDieCommand = new RelayCommand(ChangePassDie);
                }

                return _ChangePassDieCommand;
            }
        }


        private void ChangePassDie()
        {
            try
            {
                Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y).State.Value = DieStateEnum.NORMAL;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _ChangeFailDieCommand;
        public ICommand ChangeFailDieCommand
        {
            get
            {
                if (null == _ChangeFailDieCommand)
                {
                    _ChangeFailDieCommand = new RelayCommand(ChangeFailDie);
                }

                return _ChangeFailDieCommand;
            }
        }


        private void ChangeFailDie()
        {
            try
            {
                IDeviceObject deviceObject = Devices.Find(item => item.DieIndexM.XIndex == MXYIndex.X && item.DieIndexM.YIndex == MXYIndex.Y);

                if (deviceObject != null)
                {
                    deviceObject.State.Value = DieStateEnum.TESTED;

                    if (deviceObject.CurTestHistory != null)
                    {
                        deviceObject.CurTestHistory.BinCode.Value = -1;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _MapZoomPlusCommand;
        public ICommand MapZoomPlusCommand
        {
            get
            {
                if (null == _MapZoomPlusCommand)
                {
                    _MapZoomPlusCommand = new RelayCommand(MapZoomPlus);
                }

                return _MapZoomPlusCommand;
            }
        }
        private void MapZoomPlus()
        {
            try
            {
                ZoomObject.ZoomLevel--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _MapZoomMinusCommand;
        public ICommand MapZoomMinusCommand
        {
            get
            {
                if (null == _MapZoomMinusCommand)
                {
                    _MapZoomMinusCommand = new RelayCommand(MapZoomMinus);
                }

                return _MapZoomMinusCommand;
            }
        }

        private void MapZoomMinus()
        {
            try
            {
                ZoomObject.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ZoomObject = StageSupervisor.WaferObject;

                _AutoSeqControl = new AutoSequenceContorl();
                _AutoSeqControl.DataContext = this;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ZoomObject = StageSupervisor.WaferObject;

                CurrentSeqNumber = 0;

                if (ProbingSeqModule.ProbingSeqParameter.ProbingSeq.DoneState == ProberInterfaces.State.ElementStateEnum.NEEDSETUP)
                {
                    ProbingSeqModule.ProbingSeqParameter.ProbingSeq.Value.Clear();
                }

                Devices = (StageSupervisor.WaferObject as WaferObject).GetDevices();
                Sequence = Extensions_IParam.Copy<List<MachineIndex>>(ProbingSeqModule.ProbingSeqParameter.ProbingSeq.Value);
                MXYIndex = new Point();

                await InitUnderDutDies();

                this.GetParam_Wafer().MapViewCurIndexVisiablity = true;
                this.GetParam_Wafer().MapViewStageSyncEnable = false;

                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += SetMXYIndex;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= SetMXYIndex;
                }

                retVal = ProbingSeqModule.SetProbingSequence(Sequence);

                retVal = (ProbingSeqModule as IHasDevParameterizable).SaveDevParameter();

                retVal = ProbingSeqModule.ResetProbingSequence();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        #region //..AutoSeqControlProperty

        private int _SeqNumber;
        public int SeqNumber
        {
            get { return _SeqNumber; }
            set
            {
                if (value != _SeqNumber)
                {
                    _SeqNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _AutoSeqSummary;
        public string AutoSeqSummary
        {
            get { return _AutoSeqSummary; }
            set
            {
                if (value != _AutoSeqSummary)
                {
                    _AutoSeqSummary = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region //..AutoSeqControlCommand & Function

        private bool _IsCallbackEntry;
        public bool IsCallbackEntry
        {
            get { return _IsCallbackEntry; }
            set
            {
                if (value != _IsCallbackEntry)
                {
                    _IsCallbackEntry = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand<object> _SeqNumberSeletedCommand;
        public IAsyncCommand SeqNumberSeletedCommand
        {
            get
            {
                if (null == _SeqNumberSeletedCommand) _SeqNumberSeletedCommand = new AsyncCommand<object>(SeqNumberSeleted);
                return _SeqNumberSeletedCommand;
            }
        }

        private async Task SeqNumberSeleted(object parameter)
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(_AutoSeqControl);

                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                SeqNumber = Convert.ToInt32(parameter);

                Sequence = this.ProbingSequenceModule().MakeProbingSequence(SeqNumber);

                if (Sequence != null)
                {
                    AutoSeqSummary = "Sucess Make ProbingSequence";
                }
                else
                {
                    AutoSeqSummary = "Fail Make ProbingSequence";
                }

                CurrentSeqNumber = 1;

                await GetUnderDutDices(new MachineIndex(Sequence[0].XIndex, Sequence[0].YIndex));

                List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();
                List<IDeviceObject> deviceObjects = new List<IDeviceObject>();

                foreach (var seq in Sequence)
                {
                    var retrievedObjects = await AddExistSeqForDev(seq, devs);
                    deviceObjects.AddRange(retrievedObjects);
                }

                UpdateDeviceObject(deviceObjects, true);

                if (Sequence == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.MakeSequenceFailMessage, EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        public async Task SeqNumberSeletedRemote(object parameter)
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(_AutoSeqControl);

                List<IDeviceObject> devs = StageSupervisor.WaferObject.GetDevices();
                List<IDeviceObject> deviceObjects;

                if (Sequence != null && Sequence.Count > 0)
                {
                    if (AutoAddSeqEnable)
                    {
                        AutoAddSeqEnable = false;
                    }
                    
                    deviceObjects = new List<IDeviceObject>();

                    foreach (var seq in Sequence)
                    {
                        var retrievedObjects = RemoveExistSeqForDev(seq, devs);
                        deviceObjects.AddRange(retrievedObjects);
                    }

                    UpdateDeviceObject(deviceObjects, false);

                    List<MachineIndex> defaultseq = new List<MachineIndex>();
                    Sequence.Clear();

                    SequenceCount = 0;
                    UnderDutDies = new ObservableCollection<IDeviceObject>();

                    CurrentSeqNumber = 0;

                    InitUnderDutDies();
                }

                if (Sequence.Count != 0)
                {
                    this.ProbingSequenceModule().ProbingSeqParameter.ProbingSeq.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
                }

                SeqNumber = Convert.ToInt32(parameter);

                Sequence = this.ProbingSequenceModule().MakeProbingSequence(SeqNumber);

                if (Sequence != null)
                {
                    AutoSeqSummary = "Sucess Make ProbingSequence";
                }
                else
                {
                    AutoSeqSummary = "Fail Make ProbingSequence";
                }

                CurrentSeqNumber = 1;

                await GetUnderDutDices(new MachineIndex(Sequence[0].XIndex, Sequence[0].YIndex));

                deviceObjects = new List<IDeviceObject>();

                foreach (var seq in Sequence)
                {
                    var retrievedObjects = await AddExistSeqForDev(seq, devs);
                    deviceObjects.AddRange(retrievedObjects);
                }

                UpdateDeviceObject(deviceObjects, true);

                if (Sequence == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog(Properties.Resources.ErrorMessageTitle, Properties.Resources.MakeSequenceFailMessage, EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        public async Task<EventCodeEnum> GetUnderDutDices(MachineIndex mCoord)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                List<IDeviceObject> dev = new List<IDeviceObject>();

                var cardinfo = this.GetParam_ProbeCard();

                // Convert GetDevices() result to dictionary
                var devicesDict = StageSupervisor.WaferObject.GetDevices().ToDictionary(x => new IndexKey(x.DieIndexM.XIndex, x.DieIndexM.YIndex), x => x);

                Task<EventCodeEnum> task = new Task<EventCodeEnum>(() =>
                {
                    if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                    {
                        for (int dutIndex = 0; dutIndex < cardinfo.ProbeCardDevObjectRef.DutList.Count; dutIndex++)
                        {
                            IndexCoord retindex = mCoord.Add(cardinfo.GetRefOffset(dutIndex));
                            IndexKey key = new IndexKey(retindex.XIndex, retindex.YIndex);
                            IDeviceObject devobj;

                            //IDeviceObject devobj = StageSupervisor.WaferObject.GetDevices().Find(x => x.DieIndexM.Equals(retindex));

                            if (devicesDict.TryGetValue(key, out devobj))
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

                        var dutdevs = new ObservableCollection<IDeviceObject>(dev.Where(device => device != null).ToList());

                        UnderDutDies = dutdevs;

                        if (dev.Count != 0)
                        {
                            // TODO : Check, 의미 X
                            //foreach (var ddie in dev)
                            //{
                            //    IndexKey key = new IndexKey(ddie.DieIndexM.XIndex, ddie.DieIndexM.YIndex);
                            //    IDeviceObject waferDeviceObj;

                            //    if (!devicesDict.TryGetValue(key, out waferDeviceObj) || waferDeviceObj.State.Value == DieStateEnum.NOT_EXIST)
                            //    {
                            //        RetVal = EventCodeEnum.NODATA;
                            //        break;
                            //    }

                            //    
                            //    RetVal = EventCodeEnum.NONE;
                            //}

                            RetVal = EventCodeEnum.NONE;
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

        private async Task<List<IDeviceObject>> AddExistSeqForDev(MachineIndex mCoord, List<IDeviceObject> devs)
        {
            List<IDeviceObject> devlist = new List<IDeviceObject>();

            try
            {
                var cardinfo = this.GetParam_ProbeCard();

                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    var devsDictionary = devs.ToDictionary(x => new IndexKey(x.DieIndexM.XIndex, x.DieIndexM.YIndex), x => x);

                    foreach (var item in cardinfo.ProbeCardDevObjectRef.DutList.Select((value, index) => new { value, index }))
                    {
                        var i = item.index;
                        var dut = item.value;
                        var a = cardinfo.GetRefOffset(i);

                        IndexKey key = new IndexKey(a.XIndex + mCoord.XIndex, a.YIndex + mCoord.YIndex);

                        if (devsDictionary.TryGetValue(key, out IDeviceObject devobj))
                        {
                            devobj.DutNumber = dut.DutNumber;
                            devlist.Add(devobj);
                        }
                    }

                    //UpdateDeviceObject(devlist, true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return devlist;
        }

        private List<IDeviceObject> RemoveExistSeqForDev(MachineIndex mCoord, List<IDeviceObject> devs)
        {
            List<IDeviceObject> devlist = new List<IDeviceObject>();

            try
            {
                var cardinfo = this.GetParam_ProbeCard();

                if ((cardinfo != null) && (cardinfo.ProbeCardDevObjectRef.DutList.Count > 0))
                {
                    var devsDictionary = devs.ToDictionary(x => new IndexKey(x.DieIndexM.XIndex, x.DieIndexM.YIndex), x => x);

                    foreach (var item in cardinfo.ProbeCardDevObjectRef.DutList.Select((value, index) => new { value, index }))
                    {
                        var i = item.index;
                        var dut = item.value;
                        var a = cardinfo.GetRefOffset(i);

                        IndexKey key = new IndexKey(a.XIndex + mCoord.XIndex, a.YIndex + mCoord.YIndex);

                        if (devsDictionary.TryGetValue(key, out IDeviceObject devobj))
                        {
                            devlist.Add(devobj);
                            devlist[devlist.Count() - 1].DutNumber = cardinfo.ProbeCardDevObjectRef.DutList[i].DutNumber;
                        }
                    }

                    //UpdateDeviceObject(devlist, false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return devlist;
        }

        public byte[] GetUnderDuts()
        {
            byte[] retval = null;

            try
            {
                retval = this.ObjectToByteArray(this.UnderDutDies);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public SequenceMakerDataDescription GetSequenceMakerInfo()
        {
            SequenceMakerDataDescription info = new SequenceMakerDataDescription();

            try
            {
                //info.UnderDutDies = this.ObjectToByteArray(this.UnderDutDies);
                info.SequenceCount = this.SequenceCount;
                info.CurrentSeqNumber = this.CurrentSeqNumber;
                info.AutoAddSeqEnable = this.AutoAddSeqEnable;
                info.MXYIndex = this.MXYIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return info;
        }

        public List<DeviceObject> GetUnderDutDevices()
        {
            List<DeviceObject> retval = null;

            try
            {
                List<IDeviceObject> deviceObjects = new List<IDeviceObject>(UnderDutDies);
                retval = deviceObjects.ConvertAll(dev => (DeviceObject)dev);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void UpdateDeviceObject(List<IDeviceObject> devlist, bool IsAdd)
        {
            try
            {
                if (devlist.Count > 0)
                {
                    List<IDeviceObject> updatelist = devlist.ToList();

                    if (IsAdd)
                    {
                        // AddExistSeqForDev
                        updatelist.ForEach(devobj => devobj.ExistSeq.Add(true));
                    }
                    else
                    {
                        List<IDeviceObject> modifiedList = new List<IDeviceObject>();

                        // RemoveExistSeqForDev
                        foreach (IDeviceObject devobj in updatelist)
                        {
                            if (devobj.ExistSeq.Count != 0)
                            {
                                devobj.ExistSeq.RemoveAt(0);

                                modifiedList.Add(devobj);
                            }
                        }

                        updatelist = modifiedList; // Replace updatelist with the modified list.
                    }

                    // Common
                    if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null && (updatelist.Count > 0))
                    {
                        List<ExistSeqs> tmp = updatelist.Distinct().Select(item => new ExistSeqs { ExistSeq = item.ExistSeq, XIndex = item.DieIndex.XIndex, YIndex = item.DieIndex.YIndex }).ToList();
                        UpdateDeviceObject(tmp);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void UpdateDeviceObject(List<ExistSeqs> list)
        {
            try
            {
                this.LoaderRemoteMediator().GetServiceCallBack()?.SequenceMakerVM_UpdateDeviceObject(list);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
