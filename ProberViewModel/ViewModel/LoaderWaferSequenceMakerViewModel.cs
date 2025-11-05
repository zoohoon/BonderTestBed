using Autofac;
using AutoSequenceMakerControl;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoaderWaferSequenceMakerViewModelModule
{
    public class MIndexEventArgs
    {
        public MachineIndex MacIndex { get; set; }
        public MIndexEventArgs(MachineIndex index)
        {
            MacIndex = index;
        }
    }

    public class LoaderWaferSequenceMakerViewModel : IMainScreenViewModel, INotifyPropertyChanged, ISequenceMakerVM
    {
        readonly Guid _ViewModelGUID = new Guid("33026329-59c0-42ca-8c48-b7fcffe4e821");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();
        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

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

        private bool _AutoAddSeqEnable;
        public bool AutoAddSeqEnable
        {
            get { return _AutoAddSeqEnable; }
            set
            {
                if (value != _AutoAddSeqEnable)
                {
                    _AutoAddSeqEnable = value;

                    _RemoteMediumProxy.SequenceMakerVM_ChangeAutoAddSeqEnable(_AutoAddSeqEnable);
                    RaisePropertyChanged();
                }
            }
        }

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

        private Point _MXYIndex;
        public Point MXYIndex
        {
            get { return _MXYIndex; }
            set
            {
                if (value != _MXYIndex)
                {
                    if (IsCallbackEntry == false)
                    {
                        _MXYIndex = value;
                    }
                    else
                    {
                        _MXYIndex = value;
                        IsCallbackEntry = false;
                    }

                    RaisePropertyChanged();
                }
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
                await _RemoteMediumProxy.SequenceMakerVM_MoveToPrevSeqCommand();
                await UpdateSequenceMakerInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        public async Task MoveToNextSeq()
        {
            try
            {
                await _RemoteMediumProxy.SequenceMakerVM_MoveToNextSeqCommand();
                await UpdateSequenceMakerInfo();
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
                if (null == _InsertSeqCommand)
                {
                    _InsertSeqCommand = new AsyncCommand(InsertSeq);
                }

                return _InsertSeqCommand;
            }
        }

        public async Task InsertSeq()
        {
            try
            {
                await _RemoteMediumProxy.SequenceMakerVM_InsertSeqCommand();
                await UpdateSequenceMakerInfo();
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

                await _RemoteMediumProxy.SequenceMakerVM_DeleteSeqCommand();
                await UpdateSequenceMakerInfo();
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
                await _RemoteMediumProxy.SequenceMakerVM_MapMoveCommand(parameter);
                await UpdateSequenceMakerInfo();
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

        private AutoSequenceContorl _AutoSeqControl;
        private async Task AutoMakeSeq()
        {
            try
            {
                await this.MetroDialogManager().ShowWindow(_AutoSeqControl);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SeqNumberSeletedCommand;
        public IAsyncCommand SeqNumberSeletedCommand
        {
            get
            {
                if (null == _SeqNumberSeletedCommand)
                {
                    _SeqNumberSeletedCommand = new AsyncCommand<object>(SeqNumberSeleted);
                }

                return _SeqNumberSeletedCommand;
            }
        }

        private async Task SeqNumberSeleted(object parameter)
        {
            try
            {
                await _RemoteMediumProxy.SequenceMakerVM_SeqNumberSeletedCommand(parameter);
                await UpdateSequenceMakerInfo();
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

        private async Task DeleteAllSeq()
        {
            try
            {
                await _RemoteMediumProxy.SequenceMakerVM_DeleteAllSeqCommand();
                await UpdateSequenceMakerInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
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

        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= SetMXYIndex;
                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += _LoaderCommunicationManager.UpdateMapIndex;

                await _RemoteMediumProxy.SequenceMakerVM_Cleanup();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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

        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //StageSupervisor : MapviewControl의 Wafer의 바인딩
                StageSupervisor = this.StageSupervisor();

                StageSupervisor.WaferObject.GetSubsInfo().DIEs = await _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>().GetConcreteDIEs();
                CoordinateManager = this.CoordinateManager();

                await _RemoteMediumProxy.SequenceMakerVM_PageSwitched();

                await UpdateSequenceMakerInfo();

                this.GetParam_Wafer().MapViewCurIndexVisiablity = true;
                this.GetParam_Wafer().MapViewStageSyncEnable = false;

                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate -= _LoaderCommunicationManager.UpdateMapIndex;
                this.StageSupervisor().WaferObject.ChangeMapIndexDelegate += SetMXYIndex;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task UpdateSequenceMakerInfo()
        {
            try
            {
                SequenceMakerDataDescription info = null;

                Task task = new Task(() =>
                {
                    info = _RemoteMediumProxy.GetSequenceMakerInfo();
                });
                task.Start();
                await task;

                if (info != null)
                {
                    var underdutdies = GetUnderDutDevices();

                    this.UnderDutDies = new ObservableCollection<IDeviceObject>(underdutdies);

                    SequenceCount = info.SequenceCount;
                    CurrentSeqNumber = info.CurrentSeqNumber;
                    AutoAddSeqEnable = info.AutoAddSeqEnable;

                    MXYIndex = info.MXYIndex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public List<DeviceObject> GetUnderDutDevices()
        {
            List<DeviceObject> retval = null;

            try
            {
                retval = _RemoteMediumProxy.GetUnderDutDIEs();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> GetUnderDutDices(MachineIndex mCoord)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = await _RemoteMediumProxy.SequenceMakerVM_GetUnderDutDices(mCoord);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void UpdateDeviceObject(List<ExistSeqs> list)
        {
            // 받은 List를 이용하여 WaferObject가 갖고 있는 DIEs에 있는 정보를 업데이트 하자. 
            // 그래야 시퀀스를 MapViewControl에서 그릴 수 있다.

            IDeviceObject[,] deviceObjects = null;

            var supervisor = this.StageSupervisor();
            if (supervisor != null)
            {
                var waferObject = supervisor.WaferObject;
                if (waferObject != null)
                {
                    var subsInfo = waferObject.GetSubsInfo();
                    if (subsInfo != null)
                    {
                        deviceObjects = subsInfo.DIEs;
                    }
                }
            }

            if (deviceObjects != null)
            {
                int xlimit = deviceObjects.GetUpperBound(0);
                int ylimit = deviceObjects.GetUpperBound(1);

                // Using parallel processing for thread-safe operations
                Parallel.ForEach(list, item =>
                {
                    if ((item.XIndex >= 0) && (item.XIndex <= xlimit) &&
                        (item.YIndex >= 0) && (item.YIndex <= ylimit))
                    {
                        // Direct access to field if possible and allowed
                        unchecked
                        {
                            deviceObjects[item.XIndex, item.YIndex].ExistSeq = item.ExistSeq;
                        }
                    }
                });
            }
        }

        public async Task SetMXYIndex(object newVal)
        {
            try
            {
                await _RemoteMediumProxy.SequenceMakerVM_SetMXYIndex((Point)newVal);
                await UpdateSequenceMakerInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        

        public SequenceMakerDataDescription GetSequenceMakerInfo()
        {
            throw new NotImplementedException();
        }

        public Task SeqNumberSeletedRemote(object param)
        {
            throw new NotImplementedException();
        }

        Task ISequenceMakerVM.AutoMakeSeq()
        {
            throw new NotImplementedException();
        }

        Task ISequenceMakerVM.DeleteAllSeq()
        {
            throw new NotImplementedException();
        }
    }
}
