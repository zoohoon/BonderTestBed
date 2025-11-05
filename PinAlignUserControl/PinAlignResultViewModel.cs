using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PinAlignUserControl
{
    //using DutViewer.ViewModel;
    using ProbeCardObject;
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.PinAlign;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Windows;
    using RelayCommandBase;
    using SerializerUtil;
    using System.Collections.ObjectModel;
    using ProberErrorCode;
    using System.Windows.Threading;
    using System.Windows.Data;
    using System.Collections;


    public class PagingCollectionView : ListCollectionView
    {
        private readonly IList _innerList;
        private readonly int _itemsPerPage;

        private int _currentPage = 1;

        public PagingCollectionView(IList innerList, int itemsPerPage)
            : base(innerList)
        {
            this._innerList = innerList;
            this._itemsPerPage = itemsPerPage;
        }

        public override int Count
        {
            get
            {
                //all pages except the last
                if (CurrentPage < PageCount)
                    return this._itemsPerPage;

                //last page
                int remainder = _innerList.Count % this._itemsPerPage;

                return remainder == 0 ? this._itemsPerPage : remainder;
            }
        }

        //public override int Count
        //{
        //    get
        //    {
        //        if (this._innerList.Count == 0) return 0;
        //        if (this._currentPage < this.PageCount) // page 1..n-1
        //        {
        //            return this._itemsPerPage;
        //        }
        //        else // page n
        //        {
        //            var itemsLeft = this._innerList.Count % this._itemsPerPage;
        //            if (0 == itemsLeft)
        //            {
        //                return this._itemsPerPage; // exactly itemsPerPage left
        //            }
        //            else
        //            {
        //                // return the remaining items
        //                return itemsLeft;
        //            }
        //        }
        //    }
        //}

        public int CurrentPage
        {
            get { return this._currentPage; }
            set
            {
                this._currentPage = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
            }
        }

        public int ItemsPerPage { get { return this._itemsPerPage; } }

        public int PageCount
        {
            get
            {
                return (this._innerList.Count + this._itemsPerPage - 1)
                    / this._itemsPerPage;
            }
        }

        private int EndIndex
        {
            get
            {
                var end = this._currentPage * this._itemsPerPage - 1;
                return (end > this._innerList.Count) ? this._innerList.Count : end;
            }
        }

        private int StartIndex
        {
            get { return (this._currentPage - 1) * this._itemsPerPage; }
        }

        public override object GetItemAt(int index)
        {
            var offset = index % (this._itemsPerPage);
            return this._innerList[this.StartIndex + offset];
        }

        public void MoveToNextPage()
        {
            bool isChanged = false;

            if (this._currentPage < this.PageCount)
            {
                this.CurrentPage += 1;

                isChanged = true;
            }
            else
            {
                if ((this._currentPage == this.PageCount) && (this.PageCount >= 2))
                {
                    this.CurrentPage = 1;
                    isChanged = true;
                }
            }

            if (isChanged == true)
            {
                this.Refresh();
            }
        }

        public void MoveToPreviousPage()
        {
            bool isChanged = false;

            if (this._currentPage > 1)
            {
                this.CurrentPage -= 1;
                isChanged = true;
            }
            else
            {
                if ((this._currentPage == 1) && (this.PageCount >= 2))
                {
                    this.CurrentPage = this.PageCount;
                    isChanged = true;
                }
            }

            if (isChanged == true)
            {
                this.Refresh();
            }
        }
    }


    public class CheckItemComponent : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _ItemStr;
        public string ItemStr
        {
            get { return _ItemStr; }
            set
            {
                if (value != _ItemStr)
                {
                    _ItemStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ToleranceStr;
        public string ToleranceStr
        {
            get { return _ToleranceStr; }
            set
            {
                if (value != _ToleranceStr)
                {
                    _ToleranceStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ResultStr;
        public string ResultStr
        {
            get { return _ResultStr; }
            set
            {
                if (value != _ResultStr)
                {
                    _ResultStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _StatusStr;
        public string StatusStr
        {
            get { return _StatusStr; }
            set
            {
                if (value != _StatusStr)
                {
                    _StatusStr = value;
                    RaisePropertyChanged();
                }
            }
        }

        public CheckItemComponent(string item, string tolerance, string result, string status)
        {
            this.ItemStr = item;
            this.ToleranceStr = tolerance;
            this.ResultStr = result;
            this.StatusStr = status;
        }
    }

    public class PinAlignResultViewModel : INotifyPropertyChanged, IFactoryModule, IDutViewControlVM, IPnpAdvanceSetupViewModel
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private PinAlignInfo _PinAlignInfo;
        public PinAlignInfo PinAlignInfo
        {
            get { return _PinAlignInfo; }
            set
            {
                if (value != _PinAlignInfo)
                {
                    _PinAlignInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinAlignDevParameters _PinAlignDevParameters;
        public PinAlignDevParameters PinAlignDevParameters
        {
            get { return _PinAlignDevParameters; }
            set
            {
                if (value != _PinAlignDevParameters)
                {
                    _PinAlignDevParameters = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinAlignSettignParameter _PinAlignSettignParameter;
        public PinAlignSettignParameter PinAlignSettignParameter
        {
            get { return _PinAlignSettignParameter; }
            set
            {
                if (value != _PinAlignSettignParameter)
                {
                    _PinAlignSettignParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EachPinResult _HighestPinResult;
        public EachPinResult HighestPinResult
        {
            get { return _HighestPinResult; }
            set
            {
                if (value != _HighestPinResult)
                {
                    _HighestPinResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EachPinResult _LowestPinResult;
        public EachPinResult LowestPinResult
        {
            get { return _LowestPinResult; }
            set
            {
                if (value != _LowestPinResult)
                {
                    _LowestPinResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EachPinResult _SelectedEachPinResult;
        public EachPinResult SelectedEachPinResult
        {
            get { return _SelectedEachPinResult; }
            set
            {
                if (value != _SelectedEachPinResult)
                {
                    _SelectedEachPinResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PagingCollectionView _EachPinCollectionView;
        public PagingCollectionView EachPinCollectionView
        {
            get { return _EachPinCollectionView; }
            set
            {
                if (value != _EachPinCollectionView)
                {
                    _EachPinCollectionView = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<CheckItemComponent> _CheckItems = new ObservableCollection<CheckItemComponent>();
        public ObservableCollection<CheckItemComponent> CheckItems
        {
            get { return _CheckItems; }
            set
            {
                if (value != _CheckItems)
                {
                    _CheckItems = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _EnablePinPlaneCompensation;
        public string EnablePinPlaneCompensation
        {
            get { return _EnablePinPlaneCompensation; }
            set
            {
                if (value != _EnablePinPlaneCompensation)
                {
                    _EnablePinPlaneCompensation = value;
                    RaisePropertyChanged();
                }
            }
        }


        public UcPinAlignResult DialogControl;
        private double _CurXPos;
        public double CurXPos
        {
            get { return _CurXPos; }
            set
            {
                if (value != _CurXPos)
                {
                    _CurXPos = value;
                    RaisePropertyChanged(nameof(CurXPos));
                }
            }
        }

        private double _CurYPos;
        public double CurYPos
        {
            get { return _CurYPos; }
            set
            {
                if (value != _CurYPos)
                {
                    _CurYPos = value;
                    RaisePropertyChanged(nameof(CurYPos));
                }
            }
        }
        private double _ZoomLevel;
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomIn;
        public Visibility VisibilityZoomIn
        {
            get { return _VisibilityZoomIn; }
            set
            {
                if (value != _VisibilityZoomIn)
                {
                    _VisibilityZoomIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomOut;
        public Visibility VisibilityZoomOut
        {
            get { return _VisibilityZoomOut; }
            set
            {
                if (value != _VisibilityZoomOut)
                {
                    _VisibilityZoomOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityMoveToCenter;
        public Visibility VisibilityMoveToCenter
        {
            get { return _VisibilityMoveToCenter; }
            set
            {
                if (value != _VisibilityMoveToCenter)
                {
                    _VisibilityMoveToCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowGrid;
        public bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPin;
        public bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPad;
        public bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _EnableDragMap;
        public bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPos;
        public bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowSelectedDut;
        public bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnableMoving = false;
        public bool IsEnableMoving
        {
            get { return _IsEnableMoving; }
            set
            {
                if (value != _IsEnableMoving)
                {
                    _IsEnableMoving = value;
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

        private IMotionManager _MotionManager;
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set
            {
                if (value != _MotionManager)
                {
                    _MotionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _strAlignResult;
        public string strAlignResult
        {
            get { return _strAlignResult; }
            set
            {
                _strAlignResult = value;
                RaisePropertyChanged();
            }
        }
        private string _strTextColor;
        public string strTextColor
        {
            get { return _strTextColor; }
            set
            {
                _strTextColor = value;
                RaisePropertyChanged();
            }
        }

        private IProbeCard _ProbeCard;
        public IProbeCard ProbeCard
        {
            get { return _ProbeCard; }
            set
            {
                if (value != _ProbeCard)
                {
                    _ProbeCard = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IWaferObject _WaferObject;
        public IWaferObject WaferObject
        {
            get { return _WaferObject; }
            set
            {
                if (value != _WaferObject)
                {
                    _WaferObject = value;
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

        public IVisionManager VisionManager { get; set; }
        public EnumProberCam CamType { get; set; }

        public bool? AddCheckBoxIsChecked { get; set; }

        //public DutViewerViewModel DutViewer { get; set; }


        public PinAlignResultViewModel()
        {
            try
            {
                DialogControl = new UcPinAlignResult();
                DialogControl.DataContext = this;

                StageSupervisor = this.StageSupervisor();
                MotionManager = this.MotionManager();
                VisionManager = this.VisionManager();

                ProbeCard = this.GetParam_ProbeCard();
                WaferObject = this.GetParam_Wafer();

                CurCam = null;
                CamType = EnumProberCam.UNDEFINED;

                ShowPad = false;
                ShowPin = true;

                EnableDragMap = false;
                ShowSelectedDut = false;
                ShowCurrentPos = false;

                ShowGrid = false;
                ZoomLevel = 5;

                VisibilityZoomIn = Visibility.Collapsed;
                VisibilityZoomOut = Visibility.Collapsed;
                VisibilityMoveToCenter = Visibility.Collapsed;

                //if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                //{
                //    strAlignResult = "Align Passed";
                //    strTextColor = "Lime";
                //}
                //else
                //{
                //    strAlignResult = "Align Failed";
                //    strTextColor = "Red";
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand _CmdExitClick;
        public ICommand CmdExitClick
        {
            get
            {
                if (null == _CmdExitClick) _CmdExitClick = new AsyncCommand(MaunalInputContolExitClick);
                return _CmdExitClick;
            }
        }

        private async Task MaunalInputContolExitClick()
        {
            try
            {
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IAsyncCommand DutAddMouseDownCommand => null;


        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();

            try
            {
                if (PinAlignInfo != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(PinAlignInfo));
                }

                if (PinAlignDevParameters != null)
                {
                    parameters.Add(SerializeManager.SerializeToByte(PinAlignDevParameters));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    if (datas.Count == 2)
                    {
                        // Index 사용 주의,
                        // [0] : PinAlignInfo
                        // [1] : PinAlignDevParameters

                        for (int i = 0; i < datas.Count; i++)
                        {
                            object target;

                            if (i == 0)
                            {
                                SerializeManager.DeserializeFromByte(datas[i], out target, typeof(PinAlignInfo));

                                if (target != null)
                                {
                                    PinAlignInfo = (PinAlignInfo)target;

                                    // TODO : 로더에서도 통하나..?

                                    var SortedPins = from s in PinAlignInfo.AlignResult.EachPinResultes orderby s.PinNum select s;

                                    PinAlignInfo.AlignResult.EachPinResultes = SortedPins.ToList();

                                    EachPinCollectionView = new PagingCollectionView(PinAlignInfo.AlignResult.EachPinResultes, 10);

                                    double lowestHeight = PinAlignInfo.AlignResult.EachPinResultes.Min(x => x.Height);
                                    double HighestHeight = PinAlignInfo.AlignResult.EachPinResultes.Max(x => x.Height);

                                    LowestPinResult = PinAlignInfo.AlignResult.EachPinResultes.FirstOrDefault(x => x.Height == lowestHeight);
                                    HighestPinResult = PinAlignInfo.AlignResult.EachPinResultes.FirstOrDefault(x => x.Height == HighestHeight);

                                    if (PinAlignInfo.AlignResult.Result == EventCodeEnum.NONE)
                                    {
                                        strAlignResult = "ALIGN PASSED";
                                        strTextColor = "Lime";
                                    }
                                    else
                                    {
                                        strAlignResult = $"ALIGN FAILED ({PinAlignInfo.AlignResult.Result})";
                                        strTextColor = "Red";
                                    }
                                }
                            }
                            else if (i == 1)
                            {
                                SerializeManager.DeserializeFromByte(datas[i], out target, typeof(PinAlignDevParameters));

                                if (target != null)
                                {
                                    PinAlignDevParameters = (PinAlignDevParameters)target;

                                    int pinRegIndex = (int)PinAlignInfo.AlignResult.AlignSource;

                                    PinAlignSettignParameter = PinAlignDevParameters.PinAlignSettignParam[pinRegIndex];

                                    if (PinAlignDevParameters.PinPlaneAdjustParam.EnablePinPlaneCompensation.Value == true)
                                    {
                                        EnablePinPlaneCompensation = "ENABLE";
                                    }
                                    else
                                    {
                                        EnablePinPlaneCompensation = "DISABLE";
                                    }

                                    if (Application.Current.Dispatcher.CheckAccess())
                                    {
                                        MakeCheckItems();
                                    }
                                    else
                                    {
                                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                                        {
                                            MakeCheckItems();
                                        }));
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"Unknown State");
                    }


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Init()
        {
            return;
        }

        public EventCodeEnum MakeCheckItems()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CheckItems == null)
                {
                    CheckItems = new ObservableCollection<CheckItemComponent>();
                }

                CheckItems.Clear();

                string result = string.Empty;
                string status = string.Empty;

                string Eachstatus = string.Empty;

                // (-2) Each pin X tolerance 

                bool IsOverTolerance = false;

                IsOverTolerance = PinAlignInfo.AlignResult.EachPinResultes.Any(x => x.PinResult == PINALIGNRESULT.PIN_OVER_TOLERANCE);

                if(IsOverTolerance == true)
                {
                    Eachstatus = "FAIL";
                }
                else
                {
                    Eachstatus = "PASS";
                }

                CheckItems.Add(new CheckItemComponent("Each pin X tolerance",
                                      PinAlignSettignParameter.EachPinToleranceX.Value.ToString() + " um",
                                      "",
                                      Eachstatus));

                // (-1) Each pin X tolerance 

                CheckItems.Add(new CheckItemComponent("Each pin Y tolerance",
                                      PinAlignSettignParameter.EachPinToleranceY.Value.ToString() + " um",
                                      "",
                                      Eachstatus));

                // (0) Each pin X tolerance 

                CheckItems.Add(new CheckItemComponent("Each pin Z tolerance",
                                      PinAlignSettignParameter.EachPinToleranceZ.Value.ToString() + " um",
                                      "",
                                      Eachstatus));


                // (1) Probe card X position shift 

                status = PinAlignInfo.AlignResult.CardCenterDiffX < PinAlignSettignParameter.CardCenterToleranceX.Value ? "PASS" : "FAIL";

                CheckItems.Add(new CheckItemComponent("Probe card X position shift",
                                                      PinAlignSettignParameter.CardCenterToleranceX.Value.ToString() + " um",
                                                      PinAlignInfo.AlignResult.CardCenterDiffX.ToString("N0") + " um",
                                                      status));

                // (2) Probe card Y position shift 

                status = PinAlignInfo.AlignResult.CardCenterDiffY < PinAlignSettignParameter.CardCenterToleranceY.Value ? "PASS" : "FAIL";

                CheckItems.Add(new CheckItemComponent("Probe card Y position shift",
                                                      PinAlignSettignParameter.CardCenterToleranceY.Value.ToString() + " um",
                                                      PinAlignInfo.AlignResult.CardCenterDiffY.ToString("N0") + " um",
                                                      status));

                // (3) Probe card Z position shift 

                status = PinAlignInfo.AlignResult.CardCenterDiffZ < PinAlignSettignParameter.CardCenterToleranceZ.Value ? "PASS" : "FAIL";

                CheckItems.Add(new CheckItemComponent("Probe card Z position shift",
                                                      PinAlignSettignParameter.CardCenterToleranceZ.Value.ToString() + " um",
                                                      PinAlignInfo.AlignResult.CardCenterDiffZ.ToString("N0") + " um",
                                                      status));

                // (4) Probe card Z position shift 

                status = PinAlignInfo.AlignResult.MinMaxZDiff < PinAlignSettignParameter.MinMaxZDiffLimit.Value ? "PASS" : "FAIL";

                CheckItems.Add(new CheckItemComponent("Probe card Z Max-Min",
                                                      PinAlignSettignParameter.MinMaxZDiffLimit.Value.ToString() + " um",
                                                      PinAlignInfo.AlignResult.MinMaxZDiff.ToString("N0") + " um",
                                                      status));


                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.EXCEPTION;
            }

            return retval;
        }


        private RelayCommand _EachPinDGPreviousPage;
        public ICommand EachPinDGPreviousPage
        {
            get
            {
                if (null == _EachPinDGPreviousPage) _EachPinDGPreviousPage = new RelayCommand(EachPinDGPreviousPageFunc);
                return _EachPinDGPreviousPage;
            }
        }

        private void EachPinDGPreviousPageFunc()
        {
            try
            {
                this.EachPinCollectionView.MoveToPreviousPage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _EachPinDGNextPage;
        public ICommand EachPinDGNextPage
        {
            get
            {
                if (null == _EachPinDGNextPage) _EachPinDGNextPage = new RelayCommand(EachPinDGNextPageFunc);
                return _EachPinDGNextPage;
            }
        }

        private void EachPinDGNextPageFunc()
        {
            try
            {
                this.EachPinCollectionView.MoveToNextPage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task DutAddbyMouseDown()
        {
            throw new NotImplementedException();
        }
    }

}
