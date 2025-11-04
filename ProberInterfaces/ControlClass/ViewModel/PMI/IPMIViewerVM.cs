using ProberErrorCode;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProberInterfaces.ControlClass.ViewModel.PMI
{
    public interface IPMIViewerVM : IMainScreenViewModel
    {
        int GetTotalImageCount();

        Task<EventCodeEnum> LoadImage();

        PMIImageInformationPack GetImageFileData(int index);

        void UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status);

        ObservableCollection<PMIWaferInfo> GetWaferlist();

        void ChangedWaferListItem(PMIWaferInfo pmiwaferinfo);
        void WaferListClear();
    }

    [DataContract]
    public class PMILogParser : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Date;
        public string Date
        {
            get { return _Date; }
            set
            {
                if (value != _Date)
                {
                    _Date = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PMILogSet> _PMILogDatas = new List<PMILogSet>();
        public List<PMILogSet> PMILogDatas
        {
            get { return _PMILogDatas; }
            set
            {
                if (value != _PMILogDatas)
                {
                    _PMILogDatas = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class PMILogSet
    {
        private DateTime _StartTime;
        public DateTime StartTime
        {
            get { return _StartTime; }
            set
            {
                if (value != _StartTime)
                {
                    _StartTime = value;
                }
            }
        }

        private DateTime _EndTime;
        public DateTime EndTime
        {
            get { return _EndTime; }
            set
            {
                if (value != _EndTime)
                {
                    _EndTime = value;
                }
            }
        }

        private int _GroupNo;
        public int GroupNo
        {
            get { return _GroupNo; }
            set
            {
                if (value != _GroupNo)
                {
                    _GroupNo = value;
                }
            }
        }

        private ObservableCollection<SimplificatedPMIPadResult> _PMIPadResult;
        public ObservableCollection<SimplificatedPMIPadResult> PMIPadResult
        {
            get { return _PMIPadResult; }
            set
            {
                if (value != _PMIPadResult)
                {
                    _PMIPadResult = value;
                }
            }
        }
    }

    [DataContract]
    public class PMIImageInformationPack
    {
        private byte[] _CompressedImageData;
        [DataMember]
        public byte[] CompressedImageData
        {
            get { return _CompressedImageData; }
            set { _CompressedImageData = value; }
        }

        private PMIImageInformation _PMIImageInfo;
        [DataMember]
        public PMIImageInformation PMIImageInfo
        {
            get { return _PMIImageInfo; }
            set { _PMIImageInfo = value; }
        }
    }

    [DataContract]
    public class PMIImageInformation : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _FullPath;
        [DataMember]
        public string FullPath
        {
            get { return _FullPath; }
            set
            {
                if (value != _FullPath)
                {
                    _FullPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Image _InterestImage;
        public Image InterestImage
        {
            get { return _InterestImage; }
            set
            {
                if (value != _InterestImage)
                {
                    _InterestImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<Image> _CroppedImagelist = new ObservableCollection<Image>();
        //public ObservableCollection<Image> CroppedImagelist
        //{
        //    get { return _CroppedImagelist; }
        //    set
        //    {
        //        if (value != _CroppedImagelist)
        //        {
        //            _CroppedImagelist = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Image _OriginalImage;
        public Image OriginalImage
        {
            get { return _OriginalImage; }
            set
            {
                if (value != _OriginalImage)
                {
                    _OriginalImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _DateAndTime;
        [DataMember]
        public DateTime DateAndTime
        {
            get { return _DateAndTime; }
            set
            {
                if (value != _DateAndTime)
                {
                    _DateAndTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Date;
        [DataMember]
        public string Date
        {
            get { return _Date; }
            set
            {
                if (value != _Date)
                {
                    _Date = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Time;
        [DataMember]
        public string Time
        {
            get { return _Time; }
            set
            {
                if (value != _Time)
                {
                    _Time = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferID;
        [DataMember]
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaferMapIndex;
        [DataMember]
        public int WaferMapIndex
        {
            get { return _WaferMapIndex; }
            set
            {
                if (value != _WaferMapIndex)
                {
                    _WaferMapIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _TableIndex;
        [DataMember]
        public int TableIndex
        {
            get { return _TableIndex; }
            set
            {
                if (value != _TableIndex)
                {
                    _TableIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _DutNumber;
        [DataMember]
        public int DutNumber
        {
            get { return _DutNumber; }
            set
            {
                if (value != _DutNumber)
                {
                    _DutNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _UI;
        [DataMember]
        public UserIndex UI
        {
            get { return _UI; }
            set
            {
                if (value != _UI)
                {
                    _UI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _MI;
        [DataMember]
        public MachineIndex MI
        {
            get { return _MI; }
            set
            {
                if (value != _MI)
                {
                    _MI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GroupIndex;
        [DataMember]
        public int GroupIndex
        {
            get { return _GroupIndex; }
            set
            {
                if (value != _GroupIndex)
                {
                    _GroupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _GroupingMode;
        [DataMember]
        public string GroupingMode
        {
            get { return _GroupingMode; }
            set
            {
                if (value != _GroupingMode)
                {
                    _GroupingMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Status;
        [DataMember]
        public string Status
        {
            get { return _Status; }
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SimplificatedPMIPadResult> _PadResult = new ObservableCollection<SimplificatedPMIPadResult>();
        [DataMember]
        public ObservableCollection<SimplificatedPMIPadResult> PadResult
        {
            get { return _PadResult; }
            set
            {
                if (value != _PadResult)
                {
                    _PadResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedPadIndex = 0;
        [DataMember]
        public int SelectedPadIndex
        {
            get { return _SelectedPadIndex; }
            set
            {
                if (value != _SelectedPadIndex)
                {
                    _SelectedPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SimplificatedPMIPadResult _SelectedPadResult;
        [DataMember]
        public SimplificatedPMIPadResult SelectedPadResult
        {
            get { return _SelectedPadResult; }
            set
            {
                if (value != _SelectedPadResult)
                {
                    _SelectedPadResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedMarkIndex = 0;
        [DataMember]
        public int SelectedMarkIndex
        {
            get { return _SelectedMarkIndex; }
            set
            {
                if (value != _SelectedMarkIndex)
                {
                    _SelectedMarkIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SimplificatedPMIMarkResult _SelectedMarkResult;
        [DataMember]
        public SimplificatedPMIMarkResult SelectedMarkResult
        {
            get { return _SelectedMarkResult; }
            set
            {
                if (value != _SelectedMarkResult)
                {
                    _SelectedMarkResult = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [DataContract]
    public class PMIWaferInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _WaferID = string.Empty;
        [DataMember]
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _NumberOfImages = 0;
        //[DataMember]
        //public int NumberOfImages
        //{
        //    get { return _NumberOfImages; }
        //    set
        //    {
        //        if (value != _NumberOfImages)
        //        {
        //            _NumberOfImages = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
    }

    [DataContract]
    public class SimplificatedPMIPadResult : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _PadIndex;
        public int PadIndex
        {
            get { return _PadIndex; }
            set
            {
                if (value != _PadIndex)
                {
                    _PadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _GroupIndex;
        public int GroupIndex
        {
            get { return _GroupIndex; }
            set
            {
                if (value != _GroupIndex)
                {
                    _GroupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PadStatusCodeEnum> _PadStatus;
        [DataMember]
        public ObservableCollection<PadStatusCodeEnum> PadStatus
        {
            get { return _PadStatus; }
            set
            {
                if (value != _PadStatus)
                {
                    _PadStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SimplificatedPMIMarkResult> _MarkResults = new ObservableCollection<SimplificatedPMIMarkResult>();
        [DataMember]
        public ObservableCollection<SimplificatedPMIMarkResult> MarkResults
        {
            get { return _MarkResults; }
            set { _MarkResults = value; }
        }

        public void CopyTo(SimplificatedPMIPadResult Target)
        {
            Target.PadStatus = this.PadStatus;

            foreach (var markresult in MarkResults)
            {
                Target.MarkResults.Add(markresult);
            }
        }
    }

    [DataContract]
    public class SimplificatedPMIMarkResult : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<MarkStatusCodeEnum> _Status = new ObservableCollection<MarkStatusCodeEnum>();
        [DataMember]
        public ObservableCollection<MarkStatusCodeEnum> Status
        {
            get { return _Status; }
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ScrubSizeX;
        [DataMember]
        public double ScrubSizeX
        {
            get { return _ScrubSizeX; }
            set
            {
                if (value != _ScrubSizeX)
                {
                    _ScrubSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ScrubSizeY;
        [DataMember]
        public double ScrubSizeY
        {
            get { return _ScrubSizeY; }
            set
            {
                if (value != _ScrubSizeY)
                {
                    _ScrubSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ScrubCenterX;
        [DataMember]
        public double ScrubCenterX
        {
            get { return _ScrubCenterX; }
            set
            {
                if (value != _ScrubCenterX)
                {
                    _ScrubCenterX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ScrubCenterY;
        [DataMember]
        public double ScrubCenterY
        {
            get { return _ScrubCenterY; }
            set
            {
                if (value != _ScrubCenterY)
                {
                    _ScrubCenterY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ScrubArea;
        [DataMember]
        public double ScrubArea
        {
            get { return _ScrubArea; }
            set
            {
                if (value != _ScrubArea)
                {
                    _ScrubArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MarkProximity _MarkProximity;
        [DataMember]
        public MarkProximity MarkProximity
        {
            get { return _MarkProximity; }
            set
            {
                if (value != _MarkProximity)
                {
                    _MarkProximity = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void CopyTo(SimplificatedPMIMarkResult Target)
        {
        }
    }
}
