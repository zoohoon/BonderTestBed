using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceUpDownControl
{
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using VirtualKeyboardControl;
    using System.Windows.Media.Imaging;
    using System.Runtime.CompilerServices;

    public class DeviceExplorerViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> PrevPageCommand
        private RelayCommand _PrevPageCommand;
        public ICommand PrevPageCommand
        {
            get
            {
                if (null == _PrevPageCommand) _PrevPageCommand = new RelayCommand(PrevPageCommandFunc);
                return _PrevPageCommand;
            }
        }

        public void ChangeDeviceCountPerPage(int maximumcount)
        {
            _DeviceCountPerPage = maximumcount;
        }

        private void PrevPageCommandFunc()
        {
            CurPageIndex--;
            if (CurPageIndex < 1)
            {
                CurPageIndex = 1;
                return;
            }

            UpdateDevicePage();
        }
        #endregion

        #region ==> NextPageCommand
        private RelayCommand _NextPageCommand;
        public ICommand NextPageCommand
        {
            get
            {
                if (null == _NextPageCommand) _NextPageCommand = new RelayCommand(NextPageCommandFunc);
                return _NextPageCommand;
            }
        }
        private void NextPageCommandFunc()
        {
            CurPageIndex++;
            if (CurPageIndex > TotalPageCount)
            {
                CurPageIndex = TotalPageCount;
                return;
            }

            UpdateDevicePage();
        }
        #endregion

        #region ==> ParamSearchBoxClickCommand
        private RelayCommand _ParamSearchBoxClickCommand;
        public ICommand ParamSearchBoxClickCommand
        {
            get
            {
                if (null == _ParamSearchBoxClickCommand) _ParamSearchBoxClickCommand = new RelayCommand(ParamSearchBoxClickCommandFunc);
                return _ParamSearchBoxClickCommand;
            }
        }
        private void ParamSearchBoxClickCommandFunc()
        {
            DeviceSearchBoxText = VirtualKeyboard.Show(DeviceSearchBoxText, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET);

            FilteringDevice(DeviceSearchBoxText);
        }
        #endregion

        #region ==> DeviceSearchBoxText
        private String _DeviceSearchBoxText;
        public String DeviceSearchBoxText
        {
            get { return _DeviceSearchBoxText; }
            set
            {
                if (value != _DeviceSearchBoxText)
                {
                    _DeviceSearchBoxText = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedDeviceItemVM
        private DeviceItemViewModel _SelectedDeviceItemVM;
        public DeviceItemViewModel SelectedDeviceItemVM
        {
            get { return _SelectedDeviceItemVM; }
            set
            {
                if (value != _SelectedDeviceItemVM)
                {
                    _SelectedDeviceItemVM = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DeviceIcon
        private BitmapImage _DeviceIcon;
        public BitmapImage DeviceIcon
        {
            get { return _DeviceIcon; }
            set
            {
                if (value != _DeviceIcon)
                {
                    _DeviceIcon = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> TotalPageCount
        private int _TotalPageCount;
        public int TotalPageCount
        {
            get { return _TotalPageCount; }
            set
            {
                if (value != _TotalPageCount)
                {
                    _TotalPageCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CurPageIndex
        private int _CurPageIndex;
        public int CurPageIndex
        {
            get { return _CurPageIndex; }
            set
            {
                if (value != _CurPageIndex)
                {
                    _CurPageIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private int _DeviceCountPerPage = 14;

        private List<DeviceItemViewModel> _DeviceItemData;
        private List<DeviceItemViewModel> _FilteredDeviceItemData;
        public ObservableCollection<DeviceItemViewModel> DeviceItemVMList { get; set; }

        public DeviceExplorerViewModel(String iconUriPath)
        {
            _DeviceItemData = new List<DeviceItemViewModel>();
            _FilteredDeviceItemData = new List<DeviceItemViewModel>();
            DeviceItemVMList = new ObservableCollection<DeviceItemViewModel>();
            _DeviceIcon = new BitmapImage(new Uri(iconUriPath, UriKind.Absolute));
            FilteringDevice(String.Empty);
        }
        public void SetDeviceItemDataSource(List<String> deviceNameList)
        {
            List<DeviceItemViewModel> devItemSource = new List<DeviceItemViewModel>();
            List<String> devNameList = deviceNameList ?? new List<String>();

            foreach (String deviceName in devNameList)
                devItemSource.Add(new DeviceItemViewModel(deviceName));

            _DeviceItemData = devItemSource;
            FilteringDevice(String.Empty);
        }
        public void FilteringDevice(String filterKeyword)
        {
            DeviceSearchBoxText = filterKeyword;

            if (String.IsNullOrEmpty(DeviceSearchBoxText))
                _FilteredDeviceItemData = _DeviceItemData.ToList();
            else
            {
                _FilteredDeviceItemData = (from devItem
                                           in _DeviceItemData
                                           where devItem.DeviceName.ToLower().Contains(filterKeyword)
                                           select devItem).ToList();
            }
            CurPageIndex = 1;//==> 현재 page 갱신
            TotalPageCount = GetTotalPageCount(_FilteredDeviceItemData.Count, _DeviceCountPerPage);
            UpdateDevicePage();
        }

        public void UpdateDevicePage()
        {
            int startRecord = (CurPageIndex - 1) * _DeviceCountPerPage;
            int endRecord = startRecord + _DeviceCountPerPage;

            if (endRecord > _FilteredDeviceItemData.Count)
                endRecord = _FilteredDeviceItemData.Count;

            int recordIdx = startRecord;
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DeviceItemVMList.Clear();
                for (int i = 0; i < _DeviceCountPerPage; i++)
                {
                    if (recordIdx < endRecord)
                    {
                        DeviceItemVMList.Add(_FilteredDeviceItemData[recordIdx]);
                    }
                    else
                    {
                        break;
                    }

                    recordIdx++;
                }
            }));
        }
        private int GetTotalPageCount(int recordCount, int perPageCount)
        {
            int pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(recordCount) / perPageCount));
            if (pageCount < 1)
                pageCount = 1;
            return pageCount;
        }
    }
}
