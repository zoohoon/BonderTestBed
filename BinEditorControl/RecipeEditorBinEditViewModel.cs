using System;
using System.Collections.Generic;
using System.Linq;

namespace BinEditorControl
{
    using ColorPaletteControl;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using VirtualKeyboardControl;
    using LogModule;

    public delegate void PropertyControlDel(BinItemViewModel binItem);
    public class RecipeEditorBinEditViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> BinItemVMList
        public ObservableCollection<BinItemViewModel> _BinItemVMList = new ObservableCollection<BinItemViewModel>();
        public ObservableCollection<BinItemViewModel> BinItemVMList
        {
            get { return _BinItemVMList; }
        }
        #endregion

        //==> Bin Number Box 옆 Bin 항목
        #region ==> TemplateBinItem
        private BinItemViewModel _TemplateBinItem;
        public BinItemViewModel TemplateBinItem
        {
            get { return _TemplateBinItem; }
            set
            {
                if (value != _TemplateBinItem)
                {
                    _TemplateBinItem = value;
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
        private void PrevPageCommandFunc()
        {
            try
            {
                CurPageIndex--;
                if (CurPageIndex < 1)
                {
                    CurPageIndex = 1;
                    return;
                }

                UpdateBinPage();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        #endregion

        #region ==> BigPrevPageCommand
        private RelayCommand _BigPrevPageCommand;
        public ICommand BigPrevPageCommand
        {
            get
            {
                if (null == _BigPrevPageCommand) _BigPrevPageCommand = new RelayCommand(BigPrevPageCommandFunc);
                return _BigPrevPageCommand;
            }
        }
        private void BigPrevPageCommandFunc()
        {
            try
            {
                CurPageIndex -= 10;
                if (CurPageIndex < 1)
                {
                    CurPageIndex = 1;
                    return;
                }

                UpdateBinPage();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
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
            try
            {
                CurPageIndex++;
                if (CurPageIndex > TotalPageCount)
                {
                    CurPageIndex = TotalPageCount;
                    return;
                }

                UpdateBinPage();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        #endregion

        #region ==> BigNextPageCommand
        private RelayCommand _BigNextPageCommand;
        public ICommand BigNextPageCommand
        {
            get
            {
                if (null == _BigNextPageCommand) _BigNextPageCommand = new RelayCommand(BigNextPageCommandFunc);
                return _BigNextPageCommand;
            }
        }
        private void BigNextPageCommandFunc()
        {
            try
            {
                CurPageIndex += 10;

                if (CurPageIndex > TotalPageCount)
                {
                    CurPageIndex = TotalPageCount;
                    return;
                }

                UpdateBinPage();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        #endregion

        #region ==> RestoreCommand
        private RelayCommand _RestoreCommand;
        public ICommand RestoreCommand
        {
            get
            {
                if (null == _RestoreCommand) _RestoreCommand = new RelayCommand(RestoreCommandFunc);
                return _RestoreCommand;
            }
        }
        private void RestoreCommandFunc()
        {

        }
        #endregion

        #region ==> BinNumBoxClickCommand
        private RelayCommand _BinNumBoxClickCommand;
        public ICommand BinNumBoxClickCommand
        {
            get
            {
                if (null == _BinNumBoxClickCommand) _BinNumBoxClickCommand = new RelayCommand(BinNumBoxClickCommandFunc);
                return _BinNumBoxClickCommand;
            }
        }
        private void BinNumBoxClickCommandFunc()
        {
            try
            {
                BinNumBoxText = VirtualKeyboard.Show(BinNumBoxText, KB_TYPE.DECIMAL, 0, 5);

                if (BinNum == -1)//==> BinNumBoxText 정수형으로 제대로 Casting 되었는지 확인
                    return;

                //==> Firter 된 Bin 데이터를 기준으로 몇번째 데이터인지 확인
                int itemIdx = 0;
                foreach (var binItem in _FilteredBinDataSource)
                {
                    if (binItem.DataID == BinNum)
                    {
                        TemplateBinItem.BinData = binItem;
                        break;
                    }
                    itemIdx++;
                }

                //==> 검색한 Bin Item이 있는 Page 계산
                if (itemIdx == _FilteredBinDataSource.Count)
                    return;

                CurPageIndex = (itemIdx / _BinItemCountPerPage) + 1;

                //==> Bin Item이 있는 Page 출력
                UpdateBinPage();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        #endregion

        #region ==> BinNumBoxText
        private String _BinNumBoxText;
        public String BinNumBoxText
        {
            get { return _BinNumBoxText; }
            set
            {
                if (value != _BinNumBoxText)
                {
                    _BinNumBoxText = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> BinNum
        private int BinNum
        {
            get
            {
                int binNum = 0;
                if (int.TryParse(BinNumBoxText, out binNum) == false)
                    return -1;

                return binNum;
            }
        }
        #endregion

        #region ==> SearchOptionItemList
        public ObservableCollection<SearchOption> _SearchOptionItemList = new ObservableCollection<SearchOption>();
        public ObservableCollection<SearchOption> SearchOptionItemList
        {
            get { return _SearchOptionItemList; }
        }
        #endregion

        private List<BinDataFormat> _BinDataSource;
        private List<BinDataFormat> _FilteredBinDataSource = null;
        private PropertyControlDel _ShowBinItemHandler;
        private PropertyControlDel _ClickBinItemHandler;
        private readonly int _BinItemCountPerPage = 50;//==> 수정 시 주의 : XAML(UcRecipeEditorBinEdit.xaml)의 BinItemVMList 개수와 같아야 한다.
        private SearchOption _SearchOptionMenu;
        public RecipeEditorBinEditViewModel()
        {
            try
            {
                //==> Test Data
                List<BinDataFormat> binDataSource = new List<BinDataFormat>();
                for (int i = 0; i < 10000; i++)
                {
                    BinDataFormat data = new BinDataFormat();
                    data.DataID = i;
                    data.Color = i % 15;
                    data.DataValue = i + " : Description String is So Long to Print";
                    binDataSource.Add(data);
                }

                PropertyControlDel showBinItemAction1 = (BinItemViewModel binItem) =>
                {
                    binItem.BinTitle = "BIN" + binItem.BinData.DataID;
                    binItem.BinContent = binItem.BinData.DataValue.ToString();
                    binItem.BinContentColor = Brushes.DarkCyan;
                    binItem.RecordVisibility = Visibility.Visible;
                };
                PropertyControlDel showBinItemAction2 = (BinItemViewModel binItem) =>
                {
                    binItem.BinTitle = "BIN" + binItem.BinData.DataID;
                    binItem.BinContentColor = ColorPalette.ConvertToBrush(binItem.BinData.Color);
                    binItem.RecordVisibility = Visibility.Visible;
                };
                PropertyControlDel clickBinItemAction1 = (BinItemViewModel binItem) =>
                {
                    binItem.BinData.DataValue = VirtualKeyboard.Show(binItem.BinContent);
                    binItem.BinContent = binItem.BinData.DataValue.ToString();
                };
                PropertyControlDel clickBinItemAction2 = (BinItemViewModel binItem) =>
                {
                    Brush selectColor = ColorPalette.Show();
                    binItem.BinData.Color = ColorPalette.ConvertToInt(selectColor);
                    binItem.BinContentColor = selectColor;
                };

                SearchOption aaa = new SearchOption("aaa", item => item.DataID == 0);
                SearchOption bbb = new SearchOption("bbb", item => item.DataID % 2 == 1);
                SearchOption ccc = new SearchOption("Normal");
                List<SearchOption> searchOptionList = new List<SearchOption>();
                searchOptionList.Add(aaa);
                searchOptionList.Add(bbb);
                searchOptionList.Add(ccc);

                //==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>==>


                //==> Init Parameter
                _BinDataSource = binDataSource;

                _ShowBinItemHandler = showBinItemAction1;
                _ClickBinItemHandler = clickBinItemAction1;

                //==> Item Click Relay Command
                RelayCommand<BinItemViewModel> relayCommand =
                    new RelayCommand<BinItemViewModel>((BinItemViewModel binItem) => { _ClickBinItemHandler(binItem); });

                //==> Set Bin Item View Model
                for (int i = 0; i < _BinItemCountPerPage; i++)
                {
                    BinItemViewModel binItemVM = new BinItemViewModel();

                    //==> Set Bin Item Click Event Handler
                    binItemVM.ContentDownCommand = relayCommand;
                    BinItemVMList.Add(binItemVM);
                }

                //==> Set Template Bin Item
                _TemplateBinItem = BinItemVMList[0];

                _SearchOptionMenu = new SearchOption("Option");
                _SearchOptionMenu.InitRecipeEdit(this);

                if (searchOptionList != null)
                {
                    foreach (var item in searchOptionList)
                    {
                        item.InitRecipeEdit(this);
                        _SearchOptionMenu.SearchOptionItemList.Add(item);
                    }

                }
                _SearchOptionItemList.Add(_SearchOptionMenu);

                FilteringBinData(null, "Option");//==> Filtering 후 UpdateBinPage 호출
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        public void FilteringBinData(Func<BinDataFormat, bool> binSearcFilter, String filterName)
        {
            try
            {
                //==> Bin Item을 Filter 해서 List에 저장
                if (binSearcFilter == null)
                    _FilteredBinDataSource = _BinDataSource.ToList();
                else
                    _FilteredBinDataSource = _BinDataSource.Where(binSearcFilter).ToList();

                CurPageIndex = 1;//==> 현재 page 갱신
                TotalPageCount = GetTotalPageCount(_FilteredBinDataSource.Count, _BinItemCountPerPage);
                UpdateBinPage();//==> Bin Page 갱신
                _SearchOptionMenu.HeadTitle = filterName;//==> Filter menu UI 갱신
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        //==> CurPageIndex 값에 맞게 Page Screen UI를 Update 함
        public void UpdateBinPage()
        {
            try
            {
                int startRecord = (CurPageIndex - 1) * _BinItemCountPerPage;
                int endRecord = startRecord + _BinItemCountPerPage;

                if (endRecord > _FilteredBinDataSource.Count)
                    endRecord = _FilteredBinDataSource.Count;

                int recordIdx = startRecord;
                for (int i = 0; i < _BinItemCountPerPage; i++)
                {
                    if (recordIdx < endRecord)
                    {
                        if (_FilteredBinDataSource[recordIdx].DataID == BinNum)
                            TemplateBinItem = BinItemVMList[i];//==> Template Bin 을 설정

                        BinItemVMList[i].BinData = _FilteredBinDataSource[recordIdx];
                        _ShowBinItemHandler(BinItemVMList[i]);
                    }
                    else
                        BinItemVMList[i].RecordVisibility = Visibility.Collapsed;
                    recordIdx++;
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private int GetTotalPageCount(int recordCount, int perPageCount)
        {
            int pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(recordCount) / perPageCount));
            try
            {

                if (pageCount < 1)
                    pageCount = 1;

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return pageCount;
        }
    }

    public class SearchOption : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> HeadTitle
        private String _HeadTitle;
        public String HeadTitle
        {
            get { return _HeadTitle; }
            set
            {
                if (value != _HeadTitle)
                {
                    _HeadTitle = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MenuCommand
        private RelayCommand<Object> _MenuCommand;
        public ICommand MenuCommand
        {
            get
            {
                if (null == _MenuCommand) _MenuCommand = new RelayCommand<Object>(MenuCommandFunc);
                return _MenuCommand;
            }
        }
        private void MenuCommandFunc(Object param)
        {
            _RecipeEditor.FilteringBinData(_Filter, _HeadTitle);
        }
        #endregion

        #region ==> SearchOptionItemList
        public ObservableCollection<SearchOption> _SearchOptionItemList = new ObservableCollection<SearchOption>();
        public ObservableCollection<SearchOption> SearchOptionItemList
        {
            get { return _SearchOptionItemList; }
        }
        #endregion

        public Func<BinDataFormat, bool> _Filter;

        RecipeEditorBinEditViewModel _RecipeEditor;

        public SearchOption(String headTitle, Func<BinDataFormat, bool> filter = null)
        {
            try
            {
                _Filter = filter;
                _HeadTitle = headTitle;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        public void InitRecipeEdit(RecipeEditorBinEditViewModel recipeEditor)
        {
            _RecipeEditor = recipeEditor;
        }
    }
    public class BinDataFormat
    {
        public int DataID { get; set; }
        public int Color { get; set; }//==> .mdb에는 업는 Column이다, mdb에서는 한 레코드에 15, 10, 10, 13 같이 넣었다.
        public Object DataValue { get; set; }
    }
}
