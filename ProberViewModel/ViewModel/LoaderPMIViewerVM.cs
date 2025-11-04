using LogModule;
using ProberErrorCode;
using ProberInterfaces;
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
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using ProberInterfaces.PMI;
using ProberInterfaces.ControlClass.ViewModel.PMI;
using LoaderBase.Communication;
using Autofac;
using MetroDialogInterfaces;

namespace PMIViewerViewModel
{
    public class LoaderPMIViewerVM : IPMIViewerVM
    {
        readonly Guid _ViewModelGUID = new Guid("e761a17f-f052-453e-bba5-40d58f58a053");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        public bool Initialized { get; set; } = false;


        private ICoordinateManager _CoordManager;
        public ICoordinateManager CoordManager
        {
            get { return _CoordManager; }
            set
            {
                if (value != _CoordManager)
                {
                    _CoordManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CurrentCursorX;
        public long CurrentCursorX
        {
            get { return _CurrentCursorX; }
            set
            {
                if (value != _CurrentCursorX)
                {
                    _CurrentCursorX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _CurrentCursorY;
        public long CurrentCursorY
        {
            get { return _CurrentCursorY; }
            set
            {
                if (value != _CurrentCursorY)
                {
                    _CurrentCursorY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ImageInfoGroupboxVisibility;
        public Visibility ImageInfoGroupboxVisibility
        {
            get { return _ImageInfoGroupboxVisibility; }
            set
            {
                if (value != _ImageInfoGroupboxVisibility)
                {
                    _ImageInfoGroupboxVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }



        private ICamera _CurCam;
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private string _SaveBasePath;
        //public string SaveBasePath
        //{
        //    get { return _SaveBasePath; }
        //    set
        //    {
        //        if (value != _SaveBasePath)
        //        {
        //            _SaveBasePath = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private DateTime _StartDate = DateTime.Now.Date;
        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                if (value != _StartDate)
                {
                    _StartDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _EndDate = DateTime.Now.Date;
        public DateTime EndDate
        {
            get { return _EndDate; }
            set
            {
                if (value != _EndDate)
                {
                    _EndDate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PadStatusResultEnum _StatusFilter;
        public PadStatusResultEnum StatusFilter
        {
            get { return _StatusFilter; }
            set
            {
                if (value != _StatusFilter)
                {
                    _StatusFilter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _MainViewTarget;
        public object MainViewTarget
        {
            get { return _MainViewTarget; }
            set
            {
                if (value != _MainViewTarget)
                {
                    _MainViewTarget = value;
                    if (_MainViewTarget == ZoomObject)
                        MainViewZoomVisible = Visibility.Visible;
                    else
                        MainViewZoomVisible = Visibility.Hidden;

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

        private Visibility _MainViewZoomVisible;
        public Visibility MainViewZoomVisible
        {
            get { return _MainViewZoomVisible; }
            set
            {
                if (value != _MainViewZoomVisible)
                {
                    _MainViewZoomVisible = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PMIImageInformation> _PMIFileDelimiterlist = new ObservableCollection<PMIImageInformation>();
        public ObservableCollection<PMIImageInformation> PMIFileDelimiterlist
        {
            get { return _PMIFileDelimiterlist; }
            set
            {
                if (value != _PMIFileDelimiterlist)
                {
                    _PMIFileDelimiterlist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PMIImageInformation> _FilteredPMIImageInfolist = new ObservableCollection<PMIImageInformation>();
        public ObservableCollection<PMIImageInformation> FilteredPMIImageInfolist
        {
            get { return _FilteredPMIImageInfolist; }
            set
            {
                if (value != _FilteredPMIImageInfolist)
                {
                    _FilteredPMIImageInfolist = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private ObservableCollection<PMIImageInformation> _FilteredPMIFileDelimiterlist = new ObservableCollection<PMIImageInformation>();
        //public ObservableCollection<PMIImageInformation> FilteredPMIFileDelimiterlist
        //{
        //    get { return _FilteredPMIFileDelimiterlist; }
        //    set
        //    {
        //        if (value != _FilteredPMIFileDelimiterlist)
        //        {
        //            _FilteredPMIFileDelimiterlist = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<PMIWaferInfo> _PMIWaferInfos = new ObservableCollection<PMIWaferInfo>();
        public ObservableCollection<PMIWaferInfo> PMIWaferInfos
        {
            get { return _PMIWaferInfos; }
            set
            {
                if (value != _PMIWaferInfos)
                {
                    _PMIWaferInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedWaferIndex;
        public int SelectedWaferIndex
        {
            get { return _SelectedWaferIndex; }
            set
            {
                if (value != _SelectedWaferIndex)
                {
                    _SelectedWaferIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMIImageInformation _SelectedImageItem;
        public PMIImageInformation SelectedImageItem
        {
            get { return _SelectedImageItem; }
            set
            {
                if (value != _SelectedImageItem)
                {
                    _SelectedImageItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMIWaferInfo _SelectedWaferInfo;
        public PMIWaferInfo SelectedWaferInfo
        {
            get { return _SelectedWaferInfo; }
            set
            {
                if (value != _SelectedWaferInfo)
                {
                    _SelectedWaferInfo = value;

                    ChangedWaferListItem(_SelectedWaferInfo);

                    RaisePropertyChanged();
                }
            }
        }

        private int _SelectedImageItemIndex = -1;
        public int SelectedImageItemIndex
        {
            get { return _SelectedImageItemIndex; }
            set
            {
                if (value != _SelectedImageItemIndex)
                {
                    _SelectedImageItemIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _DelimiterCount;
        //public int DelimiterCount
        //{
        //    get { return _DelimiterCount; }
        //    set
        //    {
        //        if (value != _DelimiterCount)
        //        {
        //            _DelimiterCount = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double _MapOpacity;
        public double MapOpacity
        {
            get { return _MapOpacity; }
            set
            {
                if (value != _MapOpacity)
                {
                    _MapOpacity = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ActiveTabItemIndex;
        public int ActiveTabItemIndex
        {
            get { return _ActiveTabItemIndex; }
            set
            {
                if (value != _ActiveTabItemIndex)
                {
                    _ActiveTabItemIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncObservableCollection<HighlightDieComponent> _HighlightDies = new AsyncObservableCollection<HighlightDieComponent>();
        public AsyncObservableCollection<HighlightDieComponent> HighlightDies
        {
            get { return _HighlightDies; }
            set
            {
                if (value != _HighlightDies)
                {
                    _HighlightDies = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Command

        //private RelayCommand<object> _StatusFilterChangedCommand;
        //public ICommand StatusFilterChangedCommand
        //{
        //    get
        //    {
        //        if (null == _StatusFilterChangedCommand) _StatusFilterChangedCommand = new RelayCommand<object>(StatusFilterChangedCommandFunc);
        //        return _StatusFilterChangedCommand;
        //    }
        //}

        //private void StatusFilterChangedCommandFunc(object param)
        //{
        //    try
        //    {
        //        // 이미 데이터가 로드되어 있는 경우
        //        if (PMIFileDelimiterlist.Count > 0)
        //        {
        //            System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                FilteredPMIFileDelimiterlist.Clear();

        //                foreach (var item in PMIFileDelimiterlist)
        //                {
        //                    if (item.Status == StatusFilter.ToString().ToUpper())
        //                    {
        //                        // Status가 동일한 경우
        //                        FilteredPMIFileDelimiterlist.Add(item);
        //                    }
        //                }

        //                if (FilteredPMIFileDelimiterlist.Count > 0)
        //                {
        //                    SelectedDelimiteritem = FilteredPMIFileDelimiterlist[0];
        //                    SelectedDelimiterIndex = 0;
        //                }
        //            });
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    //finally
        //    //{
        //    //    DelimiterCount = FilteredPMIFileDelimiterlist.Count;
        //    //}
        //}

        private static T GetNext<T>(IEnumerable<T> list, T current)
        {
            try
            {
                return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
            }
            catch
            {
                return default(T);
            }
        }

        private static T GetPrevious<T>(IEnumerable<T> list, T current)
        {
            try
            {
                return list.TakeWhile(x => !x.Equals(current)).Last();
            }
            catch
            {
                return default(T);
            }
        }

        private RelayCommand<SELECT_DIRECTION> _ChangePadIndexCommand;
        public ICommand ChangePadIndexCommand
        {
            get
            {
                if (null == _ChangePadIndexCommand) _ChangePadIndexCommand = new RelayCommand<SELECT_DIRECTION>(ChangePadIndexCommandFunc);
                return _ChangePadIndexCommand;
            }
        }

        private void ChangePadIndexCommandFunc(SELECT_DIRECTION obj)
        {
            try
            {
                if (SelectedImageItem != null)
                {
                    if (SelectedImageItem.SelectedPadResult != null)
                    {
                        if (obj == SELECT_DIRECTION.PREV)
                        {
                            var tmp = GetPrevious<SimplificatedPMIPadResult>(SelectedImageItem.PadResult, SelectedImageItem.SelectedPadResult);

                            if (tmp != null)
                            {
                                SelectedImageItem.SelectedPadResult = tmp;
                                SelectedImageItem.SelectedPadIndex = SelectedImageItem.PadResult.IndexOf(SelectedImageItem.SelectedPadResult);

                                if (SelectedImageItem.SelectedPadResult.MarkResults.Count > 0)
                                {
                                    SelectedImageItem.SelectedMarkResult = SelectedImageItem.SelectedPadResult.MarkResults.First();
                                    SelectedImageItem.SelectedMarkIndex = SelectedImageItem.SelectedPadResult.MarkResults.IndexOf(SelectedImageItem.SelectedMarkResult);
                                }
                                else
                                {//Mark가 없는 경우
                                    SelectedImageItem.SelectedMarkResult = new SimplificatedPMIMarkResult();
                                    SelectedImageItem.SelectedMarkIndex = -1;
                                }
                            }
                        }
                        else if (obj == SELECT_DIRECTION.NEXT)
                        {
                            var tmp = GetNext<SimplificatedPMIPadResult>(SelectedImageItem.PadResult, SelectedImageItem.SelectedPadResult);

                            if (tmp != null)
                            {
                                SelectedImageItem.SelectedPadResult = tmp;
                                SelectedImageItem.SelectedPadIndex = SelectedImageItem.PadResult.IndexOf(SelectedImageItem.SelectedPadResult);

                                if (SelectedImageItem.SelectedPadResult.MarkResults.Count > 0)
                                {
                                    SelectedImageItem.SelectedMarkResult = SelectedImageItem.SelectedPadResult.MarkResults.First();
                                    SelectedImageItem.SelectedMarkIndex = SelectedImageItem.SelectedPadResult.MarkResults.IndexOf(SelectedImageItem.SelectedMarkResult);
                                }
                                else
                                {//Mark가 없는 경우
                                    SelectedImageItem.SelectedMarkResult = new SimplificatedPMIMarkResult();
                                    SelectedImageItem.SelectedMarkIndex = -1;
                                }
                            }

                        }
                        else
                        {
                            LoggerManager.Error($"Unknown Status");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<SELECT_DIRECTION> _ChangeMarkIndexCommand;
        public ICommand ChangeMarkIndexCommand
        {
            get
            {
                if (null == _ChangeMarkIndexCommand) _ChangeMarkIndexCommand = new RelayCommand<SELECT_DIRECTION>(ChangeMarkIndexCommandFunc);
                return _ChangeMarkIndexCommand;
            }
        }

        private void ChangeMarkIndexCommandFunc(SELECT_DIRECTION obj)
        {
            try
            {
                if (SelectedImageItem != null)
                {
                    if (SelectedImageItem != null)
                    {
                        if (SelectedImageItem.SelectedPadResult != null)
                        {
                            if (SelectedImageItem.SelectedMarkResult != null)
                            {
                                if (obj == SELECT_DIRECTION.PREV)
                                {
                                    var tmp = GetPrevious<SimplificatedPMIMarkResult>(SelectedImageItem.SelectedPadResult.MarkResults, SelectedImageItem.SelectedMarkResult);

                                    if (tmp != null)
                                    {
                                        SelectedImageItem.SelectedMarkResult = tmp;
                                        SelectedImageItem.SelectedMarkIndex = SelectedImageItem.SelectedPadResult.MarkResults.IndexOf(SelectedImageItem.SelectedMarkResult);
                                    }
                                }
                                else if (obj == SELECT_DIRECTION.NEXT)
                                {
                                    var tmp = GetNext<SimplificatedPMIMarkResult>(SelectedImageItem.SelectedPadResult.MarkResults, SelectedImageItem.SelectedMarkResult);

                                    if (tmp != null)
                                    {
                                        SelectedImageItem.SelectedMarkResult = tmp;
                                        SelectedImageItem.SelectedMarkIndex = SelectedImageItem.SelectedPadResult.MarkResults.IndexOf(SelectedImageItem.SelectedMarkResult);
                                    }
                                }
                                else
                                {
                                    LoggerManager.Error($"Unknown Status");
                                }
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

        private RelayCommand<SELECT_DIRECTION> _ChangeFilteredImageIndexCommnad;
        public ICommand ChangeFilteredImageIndexCommnad
        {
            get
            {
                if (null == _ChangeFilteredImageIndexCommnad) _ChangeFilteredImageIndexCommnad = new RelayCommand<SELECT_DIRECTION>(ChangeFilteredImageIndexCommnadFunc);
                return _ChangeFilteredImageIndexCommnad;
            }
        }

        private void ChangeFilteredImageIndexCommnadFunc(SELECT_DIRECTION obj)
        {
            bool IsValid = false;

            try
            {
                if (SelectedImageItem != null)
                {
                    if (obj == SELECT_DIRECTION.PREV)
                    {
                        var tmp = GetPrevious<PMIImageInformation>(FilteredPMIImageInfolist, SelectedImageItem);

                        if (tmp != null)
                        {
                            SelectedImageItem = tmp;
                            SelectedImageItemIndex = FilteredPMIImageInfolist.IndexOf(SelectedImageItem);

                            IsValid = true;
                        }
                    }
                    else if (obj == SELECT_DIRECTION.NEXT)
                    {
                        var tmp = GetNext<PMIImageInformation>(FilteredPMIImageInfolist, SelectedImageItem);

                        if (tmp != null)
                        {
                            SelectedImageItem = tmp;
                            SelectedImageItemIndex = FilteredPMIImageInfolist.IndexOf(SelectedImageItem);

                            IsValid = true;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"Unknown Status");
                    }

                    if (IsValid)
                    {
                        ChangedSelectedImageItem();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ChangedSelectedImageItem()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                HighlightDies.Clear();

                HighlightDieComponent newitem = new HighlightDieComponent();

                if(this.SelectedImageItem != null)
                {
                    CurrentCursorX = SelectedImageItem.MI.XIndex;
                    CurrentCursorY = SelectedImageItem.MI.YIndex;

                    newitem.UI = this.SelectedImageItem.UI;
                    newitem.MI = this.SelectedImageItem.MI;
                    
                    if (this.SelectedImageItem.Status.ToUpper() == "PASS")
                    {
                        newitem.BrushAlias = "GreenBrush";
                    }
                    else if (this.SelectedImageItem.Status.ToUpper() == "FAIL")
                    {
                        newitem.BrushAlias = "RedBrush";
                    }
                    else
                    {
                        newitem.BrushAlias = "skipBrush";
                    }

                    HighlightDies.Add(newitem);
                }
            }));

            if (SelectedImageItem.PadResult != null && SelectedImageItem.PadResult.Count > 0)
            {
                SelectedImageItem.SelectedPadResult = SelectedImageItem.PadResult[0];
                SelectedImageItem.SelectedPadIndex = SelectedImageItem.PadResult.IndexOf(SelectedImageItem.SelectedPadResult);
            }

            if (SelectedImageItem.SelectedPadResult != null)
            {
                if(SelectedImageItem.SelectedPadResult.MarkResults.Count > 0)
                {
                    SelectedImageItem.SelectedMarkResult = SelectedImageItem.SelectedPadResult.MarkResults[0];
                    SelectedImageItem.SelectedMarkIndex = SelectedImageItem.SelectedPadResult.MarkResults.IndexOf(SelectedImageItem.SelectedMarkResult); // = 0;
                }
                else
                {
                    SelectedImageItem.SelectedMarkResult = new SimplificatedPMIMarkResult();
                    SelectedImageItem.SelectedMarkIndex = -1;
                }
            }
        }

        private RelayCommand<object> _WaferListClearCommand;
        public ICommand WaferListClearCommand
        {
            get
            {
                if (null == _WaferListClearCommand) _WaferListClearCommand = new RelayCommand<object>(WaferListClearCommandFunc);
                return _WaferListClearCommand;
            }
        }

        private void WaferListClearCommandFunc(object obj)
        {
            try
            {
                this.PMIWaferInfos = new ObservableCollection<PMIWaferInfo>();

                // Cell이 갖고 있는 데이터 초기화 필요.

                WaferListClear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private AsyncCommand<object> _WaferListRefreshCommand;
        public IAsyncCommand WaferListRefreshCommand
        {
            get
            {
                if (null == _WaferListRefreshCommand) _WaferListRefreshCommand = new AsyncCommand<object>(WaferListRefreshCommandFunc);
                return _WaferListRefreshCommand;
            }
        }

        private async Task WaferListRefreshCommandFunc(object obj)
        {
            try
            {
                Task task = new Task(() =>
                {
                    this.PMIWaferInfos = GetWaferlist();

                    if (PMIWaferInfos != null && PMIWaferInfos.Count > 0)
                    {
                        SelectedWaferInfo = PMIWaferInfos[0];
                        SelectedWaferIndex = 0;
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<PMIImageInformation> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand) _SelectedItemChangedCommand = new RelayCommand<PMIImageInformation>(SelectedItemChangedCommandFunc);
                return _SelectedItemChangedCommand;
            }
        }

        private void SelectedItemChangedCommandFunc(PMIImageInformation parameter)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    HighlightDies.Clear();

                    if(parameter != null)
                    {
                        HighlightDieComponent newitem = new HighlightDieComponent();
                        newitem.UI = parameter.UI;
                        newitem.MI = parameter.MI;

                        if (parameter.Status.ToUpper() == "PASS")
                        {
                            newitem.BrushAlias = "GreenBrush";
                        }
                        else if (parameter.Status.ToUpper() == "FAIL")
                        {
                            newitem.BrushAlias = "RedBrush";
                        }
                        else
                        {
                            newitem.BrushAlias = "skipBrush";
                        }

                        HighlightDies.Add(newitem);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _ImageReloadCommand;
        public IAsyncCommand ImageReloadCommand
        {
            get
            {
                if (null == _ImageReloadCommand) _ImageReloadCommand = new AsyncCommand<object>(ImageReloadCommandFunc);
                return _ImageReloadCommand;
            }
        }

        private async Task ImageReloadCommandFunc(object parameter)
        {
            try
            {
                EnumMessageDialogResult ret = EnumMessageDialogResult.UNDEFIND;

                int totalcount = GetTotalImageCount();

                if (totalcount == 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("[Load Image]", "There is no image to load.", EnumMessageStyle.Affirmative);
                }
                else if (totalcount > 0)
                {
                    string DateStr = $"Date : {StartDate.ToString("MM/dd/yy")} ~ {EndDate.Date.ToString("MM/dd/yy")}";
                    string StatusStr = $"Status : {StatusFilter}";

                    string WaferIDStr = "Wafer ID : Undefined";

                    if (SelectedWaferInfo != null && SelectedWaferInfo.WaferID != string.Empty)
                    {
                        WaferIDStr = $"Wafer ID : {SelectedWaferInfo.WaferID}";
                    }

                    string ImageCount = $"Number of images to be loaded : {totalcount}";

                    ret = await this.MetroDialogManager().ShowMessageDialog("[Load Image]",
                        $"Are you sure you want to load images?\n\n" +
                        $"{DateStr}\n" +
                        $"{StatusStr}\n" +
                        $"{WaferIDStr}\n\n" +
                        $"{ImageCount}", EnumMessageStyle.AffirmativeAndNegative);

                }
                else
                {
                    LoggerManager.Error($"[LoaderPMIViewerVM], ImageReloadCommandFunc() : Unknown case");
                }

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        PMIFileDelimiterlist.Clear();
                        FilteredPMIImageInfolist.Clear();
                    });

                    SelectedImageItem = null;
                    SelectedImageItemIndex = -1;

                    // TODO : 1장씩 로드하는 것이 맞는가?

                    // Stage load 시키고, 로드 된 데이터에서 한장씩 가져오기.

                    await LoadImage();

                    byte[] CompressedImageData = null;
                    byte[] DecompressedImageData = null;
                    //LoggerManager.Error($"[LoaderPMIViewerVM], ImageReloadCommandFunc() : Start, Get compressed image data");

                    for (int i = 0; i < totalcount; i++)
                    {
                        PMIImageInformationPack pack = GetImageFileData(i);

                        if (pack != null && pack.CompressedImageData != null)
                        {
                            DecompressedImageData = this.FileManager().Decompress(pack.CompressedImageData);

                            Image ImageObj = null;

                            BitmapImage RestoreImage = Byte2ControlsImage(DecompressedImageData);

                            PMIImageInformation delimiter = pack.PMIImageInfo;

                            Image newimage = null;

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                newimage = new Image();
                                newimage.Source = RestoreImage;
                            });

                            delimiter.OriginalImage = newimage;
                            delimiter.InterestImage = delimiter.OriginalImage;

                            PMIFileDelimiterlist.Add(delimiter);
                        }
                    }

                    if (PMIFileDelimiterlist.Count > 0)
                    {
                        FilteredPMIImageInfolist = new ObservableCollection<PMIImageInformation>(PMIFileDelimiterlist);

                        SelectedImageItem = FilteredPMIImageInfolist[0];
                        SelectedImageItemIndex = 0;

                        ChangedSelectedImageItem();

                        ImageInfoGroupboxVisibility = Visibility.Visible;
                        ActiveTabItemIndex = 1;
                    }

                    LoggerManager.Error($"[LoaderPMIViewerVM], ImageReloadCommandFunc() : End, Get compressed image data");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private BitmapImage Byte2ControlsImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
                image.StreamSource = null;
            }

            image.Freeze();

            return image;
        }


        //private async Task GetImagesFromCell()
        //{
        //    try
        //    {
        //        await Application.Current.Dispatcher.Invoke(
        //        async () =>
        //        {
        //            //if (uiLogListView.SelectedItems.Count != 0)
        //            //{
        //            //    byte[] compressedFile = null;

        //            //    string saveFilePath = LoggerManager.LoggerManagerParam.FilePath + @"\ImageTransfer\CELL" + _LoaderCommunicationManager.SelectedStageIndex + @"\" + "PMI";

        //            //    if (!Directory.Exists(saveFilePath))
        //            //    {
        //            //        Directory.CreateDirectory(saveFilePath);
        //            //    }

        //            //    compressedFile = _RemoteMediumProxy.LogTransfer_OpenLogFile(SelectedLogFilePath);
        //            //    saveFilePath += @"\" + SelectedLogFileName + ".txt";
        //            //    FileInfo fileInfo = new FileInfo(saveFilePath);

        //            //    DecompressFilesFromByteArray(compressedFile, saveFilePath);
        //            //}
        //        });
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public LoaderPMIViewerVM()
        {

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

        public WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        private void init()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    //UserInterestlist.Clear();

                    if(PMIFileDelimiterlist != null)
                    {
                        PMIFileDelimiterlist.Clear();
                    }
                    
                    if(FilteredPMIImageInfolist != null)
                    {
                        FilteredPMIImageInfolist.Clear();
                    }
                    
                    if(HighlightDies != null)
                    {
                        HighlightDies.Clear();
                    }
                    
                    if(PMIWaferInfos != null)
                    {
                        PMIWaferInfos.Clear();
                    }
                });

                SelectedWaferIndex = -1;
                SelectedWaferInfo = null;

                SelectedImageItem = null;
                SelectedImageItemIndex = -1;

                StatusFilter = PadStatusResultEnum.ALL;

                ActiveTabItemIndex = 0;

                ImageInfoGroupboxVisibility = Visibility.Hidden;

                MapOpacity = 0.5;

                MainViewTarget = Wafer;

                StartDate = DateTime.Now.Date;
                EndDate = DateTime.Now.Date;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                init();

                _RemoteMediumProxy.PMIViewer_PageSwitched();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                init();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
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

        public int GetTotalImageCount()
        {
            int retval = 0;

            try
            {
                UpdateFilterDatas(this.StartDate, this.EndDate, this.StatusFilter);

                retval = _RemoteMediumProxy.PMIViewer_GetTotalImageCount();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task<EventCodeEnum> LoadImage()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                _RemoteMediumProxy.PMIViewer_LoadImage();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PMIImageInformationPack GetImageFileData(int index)
        {
            PMIImageInformationPack retval = null;

            try
            {
                retval = _RemoteMediumProxy.PMIViewer_GetImageFileData(index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public ObservableCollection<PMIWaferInfo> GetWaferlist()
        {
            ObservableCollection<PMIWaferInfo> retval = null;

            try
            {
                UpdateFilterDatas(this.StartDate, this.EndDate, this.StatusFilter);

                retval = _RemoteMediumProxy.PMIViewer_GetWaferlist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 현재 로더가 갖고 있는 필터관련 데이터를 셀쪽으로 전달하여, 셀의 데이터를 업데이트 하는 함수.
        /// </summary>
        public void UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status)
        {
            try
            {
                _RemoteMediumProxy.PMIViewer_UpdateFilterDatas(Startdate, Enddate, Status);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void ChangedWaferListItem(PMIWaferInfo pmiwaferinfo)
        {
            try
            {
                _RemoteMediumProxy.PMIViewer_ChangedWaferListItem(pmiwaferinfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void WaferListClear()
        {
            try
            {
                _RemoteMediumProxy.PMIViewer_WaferListClear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
