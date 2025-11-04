namespace DemoRunDYWindow
{
    using LoaderBase;
    using LoaderParameters.Data;
    using RelayCommandBase;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class LotInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _LotID;
        public string LotID
        {
            get { return _LotID; }
            set
            {
                if (value != _LotID)
                {
                    _LotID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DYModeEnable = true;
        public bool DYModeEnable
        {
            get { return _DYModeEnable; }
            set
            {
                if (value != _DYModeEnable)
                {
                    _DYModeEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ILoaderSupervisor Master { get; set; }
        private bool _DYModeDisable;
        public bool DYModeDisable
        {
            get { return _DYModeDisable; }
            set
            {
                if (value != _DYModeDisable)
                {
                    _DYModeDisable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DynamicFoupStateEnum _DynamicFoupState;
        public DynamicFoupStateEnum DynamicFoupState
        {
            get { return _DynamicFoupState; }
            set
            {
                if (value != _DynamicFoupState)
                {
                    _DynamicFoupState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SlotInfo> _SlotInfos;
        public ObservableCollection<SlotInfo> SlotInfos
        {
            get { return _SlotInfos; }
            set
            {
                if (value != _SlotInfos)
                {
                    _SlotInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _LoadUnloadCommand;
        public ICommand LoadUnloadCommand
        {
            get
            {
                if (null == _LoadUnloadCommand) _LoadUnloadCommand = new RelayCommand(LoadUnloadCommandFunc);
                return _LoadUnloadCommand;
            }
        }

        public void LoadUnloadCommandFunc()
        {
            DynamicFoupState = DynamicFoupStateEnum.LOAD_AND_UNLOAD;
            Master.ActiveLotInfos[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
            Master.Loader.Foups[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
        }

        private RelayCommand _OnlyUnloadCommand;
        public ICommand OnlyUnloadCommand
        {
            get
            {
                if (null == _OnlyUnloadCommand) _OnlyUnloadCommand = new RelayCommand(OnlyUnloadCommandFunc);
                return _OnlyUnloadCommand;
            }
        }

        public void OnlyUnloadCommandFunc()
        {
            DynamicFoupState = DynamicFoupStateEnum.UNLOAD;
            Master.ActiveLotInfos[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
            Master.Loader.Foups[FoupIndex - 1].DynamicFoupState = DynamicFoupState;
        }





        //#Hynix_Merge: 검토 필요 아래 함수 Hynix 코드, V20은 오른쪽 public LotInfo(int foupidx, int stageCount,ILoaderSupervisor loaderMaster)
        public LotInfo(int foupidx, int stageCount, ObservableCollection<string> predefind_ids, ILoaderSupervisor loaderMaster)
        {
            try
            {
                FoupIndex = foupidx;
                SlotInfos = new ObservableCollection<SlotInfo>();
                Master = loaderMaster;
                for (int index = 25; index > 0; index--)
                {
                    //#Hynix_Merge: SlotInfos.Add(new SlotInfo(index, stageCount));
                    SlotInfos.Add(new SlotInfo(index, stageCount, predefind_ids[index - 1]));
                }
            }
            catch (Exception)
            {
                
            }
        }
    }

    public class SlotInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _SlotIndex;
        public int SlotIndex
        {
            get { return _SlotIndex; }
            set
            {
                if (value != _SlotIndex)
                {
                    _SlotIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Predefined_WaferId;
        public string Predefined_WaferId
        {
            get { return _Predefined_WaferId; }
            set
            {
                if (value != _Predefined_WaferId)
                {
                    _Predefined_WaferId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SlotCellInfomation> _SlotCellInfos;
        public ObservableCollection<SlotCellInfomation> SlotCellInfos
        {
            get { return _SlotCellInfos; }
            set
            {
                if (value != _SlotCellInfos)
                {
                    _SlotCellInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //#Hynix_Merge: 검토 필요 아래 함수 Hynix 코드, V20 public SlotInfo(int slotindex, int stageCount)
        public SlotInfo(int slotindex, int stageCount, string predefined_id)
        {
            try
            {
                SlotIndex = slotindex;
                SlotCellInfos = new ObservableCollection<SlotCellInfomation>();
                Predefined_WaferId = predefined_id;
                int stageMaxCount = 12;

                for (int index = 0; index < stageMaxCount; index++)
                {
                    SlotCellInfos.Add(new SlotCellInfomation(index+1));
                }
            }
            catch (Exception)
            {

            }
        }
    }

    public class SlotCellInfomation : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _CellIndex;
        public int CellIndex
        {
            get { return _CellIndex; }
            set
            {
                if (value != _CellIndex)
                {
                    _CellIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnableCell = false;
        public bool EnableCell
        {
            get { return _EnableCell; }
            set
            {
                if (value != _EnableCell)
                {
                    _EnableCell = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _EnableSlot;
        public bool EnableSlot
        {
            get { return _EnableSlot; }
            set
            {
                if (value != _EnableSlot)
                {
                    _EnableSlot = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SlotCellInfomation(int cellNumber)
        {
            CellIndex = cellNumber;
        }


    }

    public class CellInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _CellIndex;
        public int CellIndex
        {
            get { return _CellIndex; }
            set
            {
                if (value != _CellIndex)
                {
                    _CellIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CurDeviceName;
        public string CurDeviceName
        {
            get { return _CurDeviceName; }
            set
            {
                if (value != _CurDeviceName)
                {
                    _CurDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _SetDeviceName;
        public string SetDeviceName
        {
            get { return _SetDeviceName; }
            set
            {
                if (value != _SetDeviceName)
                {
                    _SetDeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaferUnloadFoupNumber;
        public int WaferUnloadFoupNumber
        {
            get { return _WaferUnloadFoupNumber; }
            set
            {
                if (value != _WaferUnloadFoupNumber)
                {
                    _WaferUnloadFoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsForecedDone;
        public bool IsForecedDone
        {
            get { return _IsForecedDone; }
            set
            {
                if (value != _IsForecedDone)
                {
                    _IsForecedDone = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CellInfo(int cellidx)
        {
            CellIndex = cellidx;
        }
    }

}
