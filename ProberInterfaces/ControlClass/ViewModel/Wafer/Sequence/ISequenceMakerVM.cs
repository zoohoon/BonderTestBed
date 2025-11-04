using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence
{
    using ProberErrorCode;
    using RelayCommandBase;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Input;

    [DataContract]
    public class SequenceMakerDataDescription
    {
        private int _SequenceCount;
        [DataMember]
        public int SequenceCount
        {
            get { return _SequenceCount; }
            set { _SequenceCount = value; }
        }

        private int _CurrentSeqNumber;
        [DataMember]
        public int CurrentSeqNumber
        {
            get { return _CurrentSeqNumber; }
            set { _CurrentSeqNumber = value; }
        }


        private bool _AutoAddSeqEnable;
        [DataMember]
        public bool AutoAddSeqEnable
        {
            get { return _AutoAddSeqEnable; }
            set { _AutoAddSeqEnable = value; }
        }

        private Point _MXYIndex;
        [DataMember]
        public Point MXYIndex
        {
            get { return _MXYIndex; }
            set { _MXYIndex = value; }
        }
    }

    public interface ISequenceMakerVM
    {
        SequenceMakerDataDescription GetSequenceMakerInfo();
        List<DeviceObject> GetUnderDutDevices();
        Task<EventCodeEnum> GetUnderDutDices(MachineIndex mCoord);
        void UpdateDeviceObject(List<ExistSeqs> list);
        bool IsCallbackEntry { get; set; }
        bool AutoAddSeqEnable { get; set; }
        Point MXYIndex { get; set; }
        ICommand MoveToPrevSeqCommand { get; }
        ICommand MoveToNextSeqCommand { get; }
        ICommand InsertSeqCommand { get; }
        ICommand DeleteSeqCommand { get; }
        ICommand MapMoveCommand { get; }
        IAsyncCommand AutoMakeSeqCommand { get; }
        IAsyncCommand DeleteAllSeqCommand { get; }
        //IAsyncCommand<object> SeqNumberSeletedCommand { get; }

        IAsyncCommand SeqNumberSeletedCommand { get; }

        Task<EventCodeEnum> PageSwitched(object parameter = null);
        Task<EventCodeEnum> Cleanup(object parameter = null);

        Task SeqNumberSeletedRemote(object param);
        Task AutoMakeSeq();
        Task DeleteAllSeq();
        Task SetMXYIndex(object newVal);
        Task MoveToNextSeq();
        Task MoveToPrevSeq();
        Task InsertSeq();
        Task DeleteSeq();
        Task MapMove(object param);
    }
}
