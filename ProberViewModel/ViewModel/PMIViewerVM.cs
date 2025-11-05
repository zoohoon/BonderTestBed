using HexagonJogControl;
using LogModule;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UcDisplayPort;
using VirtualKeyboardControl;
using System.Threading;
using System.IO;
using System.Windows.Controls;
using ProberInterfaces.PMI;
using ProberInterfaces.ControlClass.ViewModel.PMI;
using MetroDialogInterfaces;

namespace PMIViewerViewModel
{
    using Parago.Windows;
    using PMIModuleParameter;
    using System.Text.RegularExpressions;

    public class PMIViewerVM : IMainScreenViewModel, IUseLightJog, IPMIViewerVM
    {
        readonly Guid _ViewModelGUID = new Guid("60165151-0af4-48fc-9c1f-70497576f4af");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public string Error { get { return string.Empty; } }
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public const int ImageSize = 2764854;

        public const string PMISTART = "PMI START";
        public const string PADINDEX = "Pad Index";
        public const string DETECTEDMARKCOUNT = "Detected Mark Count :";
        public const string MARKINDEX = "Mark Index : ";
        public const string MARKSUMMATIONINFO = "Mark summation Info";
        public const string FAILREASON = "Fail reason : ";
        public const string PMIEND = "PMI END";
        public const string GROUPINGMETHOD = "GroupingMethod";
        public const string GROUPNO = "Group No.";

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

        private Visibility _MiniViewZoomVisibility;
        public Visibility MiniViewZoomVisibility
        {
            get { return _MiniViewZoomVisibility; }
            set
            {
                if (value != _MiniViewZoomVisibility)
                {
                    _MiniViewZoomVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ToggleSetIndex = true;
        public bool ToggleSetIndex
        {
            get { return _ToggleSetIndex; }
            set
            {
                if (value != _ToggleSetIndex)
                {
                    _ToggleSetIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ToggleCamIndex = true;
        public bool ToggleCamIndex
        {
            get { return _ToggleCamIndex; }
            set
            {
                if (value != _ToggleCamIndex)
                {
                    _ToggleCamIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _MapIndexX;
        public long MapIndexX
        {
            get { return _MapIndexX; }
            set
            {
                if (value != _MapIndexX)
                {
                    _MapIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MapIndexY;
        public long MapIndexY
        {
            get { return _MapIndexY; }
            set
            {
                if (value != _MapIndexY)
                {
                    _MapIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Property

        public WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }
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


        private IDisplayPort _DisplayPort;
        public IDisplayPort DisplayPort
        {
            get { return _DisplayPort; }
            set
            {
                if (value != _DisplayPort)
                {
                    _DisplayPort = value;
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


        private MachineCoordinate _Base;
        public MachineCoordinate Base
        {
            get { return _Base; }
            set
            {
                if (value != _Base)
                {
                    _Base = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _LightJogEnable = true;
        public bool LightJogEnable
        {
            get { return _LightJogEnable; }
            set
            {
                if (value != _LightJogEnable)
                {
                    _LightJogEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        public IViewModelManager ViewModelManager { get; set; }

        private long _XCoord;
        public long XCoord
        {
            get
            {
                return _XCoord;
            }
            set
            {
                if (value != _XCoord && value != 0)
                {
                    _XCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _YCoord;
        public long YCoord
        {
            get
            {
                return _YCoord;
            }
            set
            {
                if (value != _YCoord && value != 0)
                {
                    _YCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _XSetFromCoord;
        public double XSetFromCoord
        {
            get
            {
                return _XSetFromCoord;
            }
            set
            {
                if (value != _XSetFromCoord && value != 0)
                {
                    _XSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _YSetFromCoord;
        public double YSetFromCoord
        {
            get
            {
                return _YSetFromCoord;
            }
            set
            {
                if (value != _YSetFromCoord && value != 0)
                {
                    _YSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _StockXSetFromCoord = 0;
        public double StockXSetFromCoord
        {
            get
            {
                return _StockXSetFromCoord;
            }
            set
            {
                if (value != _StockXSetFromCoord && value != 0)
                {
                    _StockXSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _StockYSetFromCoord = 0;
        public double StockYSetFromCoord
        {
            get
            {
                return _StockYSetFromCoord;
            }
            set
            {
                if (value != _StockYSetFromCoord && value != 0)
                {
                    _StockYSetFromCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineCoordinate _FirstSet;

        public MachineCoordinate FirstSet
        {
            get { return _FirstSet; }
            set { _FirstSet = value; }
        }

        private MachineIndex _ProbingMIndex;
        public MachineIndex ProbingMIndex
        {
            get { return _ProbingMIndex; }
            set
            {
                if (value != _ProbingMIndex)
                {
                    _ProbingMIndex = value;
                    if (_ProbingMIndex.XIndex != 0)
                        XCoord = _ProbingMIndex.XIndex;
                    if (_ProbingMIndex.YIndex != 0)
                        YCoord = _ProbingMIndex.YIndex;

                    RaisePropertyChanged();
                }
            }
        }

        private IProbingModule _ProbingModule;
        public IProbingModule ProbingModule
        {
            get { return _ProbingModule; }
            set
            {
                if (value != _ProbingModule)
                {
                    _ProbingModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IPnpManager PnpManager
        {
            get { return this.PnPManager(); }
            set { }
        }

        private int _InterestPadNumber = -1;
        public int InterestPadNumber
        {
            get { return _InterestPadNumber; }
            set
            {
                if (value != _InterestPadNumber)
                {
                    _InterestPadNumber = value;
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

        private PMIWaferInfo _SelectedWaferInfo;
        public PMIWaferInfo SelectedWaferInfo
        {
            get { return _SelectedWaferInfo; }
            set
            {
                if (value != _SelectedWaferInfo)
                {
                    _SelectedWaferInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private ObservableCollection<PMIImageInformation> _PMIImageInfolist = new ObservableCollection<PMIImageInformation>();
        public ObservableCollection<PMIImageInformation> PMIImageInfolist
        {
            get { return _PMIImageInfolist; }
            set
            {
                if (value != _PMIImageInfolist)
                {
                    _PMIImageInfolist = value;
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


        //private double _CropMarginX;
        //public double CropMarginX
        //{
        //    get { return _CropMarginX; }
        //    set
        //    {
        //        if (value != _CropMarginX)
        //        {
        //            _CropMarginX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _CropMarginY;
        //public double CropMarginY
        //{
        //    get { return _CropMarginY; }
        //    set
        //    {
        //        if (value != _CropMarginY)
        //        {
        //            _CropMarginY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

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

        #endregion

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

        private PMIImageInformation PMIFileDelimiterParser(string savebasepath, string filepath)
        {
            PMIImageInformation retval = null;

            try
            {
                //string SaveImagePath = $"{NowTime}_Dut#{DutNo}_X#{Info.LastPMIDieResult.UI.XIndex}_Y#{Info.LastPMIDieResult.UI.YIndex}__PMIGroup#_{index}{Save_Extension}";
                //string SaveImageFullPath = SaveBasePath + SaveImagePath;

                // Output v1. : 200528011550 _ Dut#22 _ X#-3 _ Y#2 _ _PMIGroup# _ 0 _ PASS.bmp
                // string SaveImagePath = $"{NowTime}_Dut#{DutNo}_X#{Info.LastPMIDieResult.UI.XIndex}_Y#{Info.LastPMIDieResult.UI.YIndex}__PMIGroup#_{index}{Save_Extension}";

                // Output v2. : 200528011550 _ WAFERID _ Dut#22 _ X#-3 _ Y#2 _ MAP#1 _ TABLE#1 _ PMIGroup#0 _ PASS.bmp
                // string SaveImagePath = $"{NowTime}_{WaferID}_Dut#{DutNo}_X#{Info.LastPMIDieResult.UI.XIndex}_Y#{Info.LastPMIDieResult.UI.YIndex}_MAP#1_TABLE#1_PMIGroup#_{index}{Save_Extension}";

                // TODO : parse의 개수로 단순 비교하는 방식...?

                string[] parse = filepath.Replace(savebasepath, "").Split('_');

                if (parse.Length == 10)
                {
                    retval = new PMIImageInformation();

                    retval.FullPath = filepath;

                    retval.DateAndTime = DateTime.ParseExact(parse[0].Substring(0, 12), "yyMMddHHmmss", null);

                    retval.Date = retval.DateAndTime.ToString("MM/dd/yy");
                    retval.Time = retval.DateAndTime.ToString("HH:mm:ss");

                    retval.WaferID = parse[1];

                    if (parse[2].Contains("Unknown") == false)
                    {
                        retval.DutNumber = Int32.Parse(parse[2].Replace("Dut#", ""));
                    }
                    else
                    {
                        retval.DutNumber = -1;
                    }

                    int ux = Int32.Parse(parse[3].Replace("X#", ""));
                    int uy = Int32.Parse(parse[4].Replace("Y#", ""));

                    retval.UI = new UserIndex(ux, uy);
                    retval.MI = this.CoordinateManager().UserIndexConvertToMachineIndex(retval.UI);

                    retval.WaferMapIndex = Int32.Parse(parse[5].Replace("MAP#", ""));
                    retval.TableIndex = Int32.Parse(parse[6].Replace("TABLE#", ""));

                    retval.GroupIndex = Int32.Parse(parse[7].Replace("PMIGroup#", ""));
                    retval.GroupIndex = retval.GroupIndex + 1;

                    retval.GroupingMode = (parse[8].Replace("Mode#", ""));

                    retval.Status = parse[9].Replace(".bmp", "");
                }


                //if (parse.Length == 8)
                //{
                //    //string NowTime = DateTime.Now.ToString("yyMMddHHmmss");
                //    //retval.DateAndTime = new DateTime(Int32.Parse(parse[0].Substring(0, 2)),
                //    //                           Int32.Parse(parse[0].Substring(2, 2)),
                //    //                           Int32.Parse(parse[0].Substring(4, 2)),
                //    //                           Int32.Parse(parse[0].Substring(6, 2)),
                //    //                           Int32.Parse(parse[0].Substring(8, 2)),
                //    //                           Int32.Parse(parse[0].Substring(10, 2))
                //    //                           );

                //    retval.DateAndTime = DateTime.ParseExact(parse[0].Substring(0, 12), "yyMMddHHmmss", null);

                //    retval.Date = retval.DateAndTime.ToString("MM/dd/yy");
                //    retval.Time = retval.DateAndTime.ToString("HH:mm:ss");

                //    if (parse[1].Contains("Unknown") == false)
                //    {
                //        retval.DutNumber = Int32.Parse(parse[1].Replace("Dut#", ""));
                //    }
                //    else
                //    {
                //        retval.DutNumber = -1;
                //    }

                //    int ux = Int32.Parse(parse[2].Replace("X#", ""));
                //    int uy = Int32.Parse(parse[3].Replace("Y#", ""));

                //    retval.UI = new UserIndex(ux, uy);
                //    retval.MI = this.CoordinateManager().UserIndexConvertToMachineIndex(retval.UI);

                //    retval.GroupIndex = Int32.Parse(parse[6]);
                //    retval.Status = parse[7].Replace(".bmp", "");

                //    // Undefined
                //    retval.WaferID = "Undefined";
                //    retval.TableIndex = -1;
                //    retval.GroupIndex = -1;
                //    retval.GroupingMode = "Undefined";
                //}
                //else if (parse.Length == 10)
                //{
                //    //string NowTime = DateTime.Now.ToString("yyMMddHHmmss");
                //    //retval.DateAndTime = new DateTime(Int32.Parse(parse[0].Substring(0, 2)),
                //    //                           Int32.Parse(parse[0].Substring(2, 2)),
                //    //                           Int32.Parse(parse[0].Substring(4, 2)),
                //    //                           Int32.Parse(parse[0].Substring(6, 2)),
                //    //                           Int32.Parse(parse[0].Substring(8, 2)),
                //    //                           Int32.Parse(parse[0].Substring(10, 2))
                //    //                           );

                //    retval.DateAndTime = DateTime.ParseExact(parse[0].Substring(0, 12), "yyMMddHHmmss", null);

                //    retval.Date = retval.DateAndTime.ToString("MM/dd/yy");
                //    retval.Time = retval.DateAndTime.ToString("HH:mm:ss");

                //    retval.WaferID = parse[1];

                //    if (parse[2].Contains("Unknown") == false)
                //    {
                //        retval.DutNumber = Int32.Parse(parse[2].Replace("Dut#", ""));
                //    }
                //    else
                //    {
                //        retval.DutNumber = -1;
                //    }

                //    int ux = Int32.Parse(parse[3].Replace("X#", ""));
                //    int uy = Int32.Parse(parse[4].Replace("Y#", ""));

                //    retval.UI = new UserIndex(ux, uy);
                //    retval.MI = this.CoordinateManager().UserIndexConvertToMachineIndex(retval.UI);

                //    retval.WaferMapIndex = Int32.Parse(parse[5].Replace("MAP#", ""));
                //    retval.TableIndex = Int32.Parse(parse[6].Replace("TABLE#", ""));

                //    retval.GroupIndex = Int32.Parse(parse[7].Replace("PMIGroup#", ""));

                //    retval.GroupingMode = (parse[8].Replace("Mode#", ""));

                //    retval.Status = parse[9].Replace(".bmp", "");
                //}
                //else
                //{
                //    retval = null;

                //    LoggerManager.Error($"");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #region Command

        private RelayCommand<SELECT_DIRECTION> _ChangeFilteredImageIndexCommnad;
        public ICommand ChangeFilteredImageIndexCommnad
        {
            get
            {
                if (null == _ChangeFilteredImageIndexCommnad) _ChangeFilteredImageIndexCommnad = new RelayCommand<SELECT_DIRECTION>(ChangeFilteredImageIndexCommnadFunc);
                return _ChangeFilteredImageIndexCommnad;
            }
        }

        private void ChangedSelectedImageItem()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HighlightDies.Clear();

                HighlightDieComponent newitem = new HighlightDieComponent();

                if (SelectedImageItem != null)
                {
                    newitem.UI = SelectedImageItem.UI;
                    newitem.MI = SelectedImageItem.MI;

                    if (SelectedImageItem.Status.ToUpper() == "PASS")
                    {
                        newitem.BrushAlias = "GreenBrush";
                    }
                    else if (SelectedImageItem.Status.ToUpper() == "FAIL")
                    {
                        newitem.BrushAlias = "RedBrush";
                    }
                    else
                    {
                        newitem.BrushAlias = "skipBrush";
                    }
                }

                HighlightDies.Add(newitem);
            });

            if (SelectedImageItem.PadResult != null && SelectedImageItem.PadResult.Count > 0)
            {
                SelectedImageItem.SelectedPadResult = SelectedImageItem.PadResult[0];
                SelectedImageItem.SelectedPadIndex = SelectedImageItem.PadResult.IndexOf(SelectedImageItem.SelectedPadResult);
            }

            if (SelectedImageItem.SelectedPadResult != null && SelectedImageItem.SelectedPadResult.MarkResults.Count > 0)
            {
                SelectedImageItem.SelectedMarkResult = SelectedImageItem.SelectedPadResult.MarkResults[0];
                SelectedImageItem.SelectedMarkIndex = SelectedImageItem.SelectedPadResult.MarkResults.IndexOf(SelectedImageItem.SelectedMarkResult);
            }
            else
            {
                SelectedImageItem.SelectedMarkResult = SelectedImageItem.SelectedMarkResult;
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
                if (parameter != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        HighlightDies.Clear();

                        if (parameter != null)
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
                //string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
                //SaveBasePath = $"C:\\Logs\\{cellNo}\\Image\\PMI\\";

                //IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                int curMapIndex = 0;

                //bool IsEnable = false;
                bool IsInPeriod = false;
                bool IsCheckedStatus = false;

                bool IsAutoMode = false;

                this.PMIWaferInfos = new ObservableCollection<PMIWaferInfo>();

                //if (PMIInfo.NormalPMIMapTemplateInfo.Count > 0)
                //{
                //var PMIMap = PMIInfo.NormalPMIMapTemplateInfo[curMapIndex];

                if (Directory.Exists(Path.GetDirectoryName(SaveBasePath)) == true)
                {
                    string[] FilePathArray = Directory.GetFiles(SaveBasePath);

                    LoggerManager.Debug($"[PMIViewerVM], WaferListRefreshCommandFunc() : Total files count : {FilePathArray.Length}");

                    TimeSpan t0 = EndDate - StartDate;

                    if (t0.Days >= 0)
                    {
                        List<PMIWaferInfo> tmplist = new List<PMIWaferInfo>();

                        foreach (var file in FilePathArray)
                        {
                            //IsEnable = false;
                            IsInPeriod = false;
                            IsCheckedStatus = false;

                            // 파일 이름으로부터, 데이터 추출
                            var delimiter = PMIFileDelimiterParser(SaveBasePath, file);

                            if (delimiter == null)
                            {
                                continue;
                            }

                            // 추출 된 데이터 중, Machine Index를 이용하여, 설정 된 PMI Map의 Enable 데이터 획득
                            // Enable 되어 있는 데이터만 취합

                            // Manual 동작 중, 저장 된 이미지는 사용할 수 없음.
                            if (delimiter.DutNumber >= 0)
                            {
                                IsAutoMode = true;
                            }
                            else
                            {
                                IsAutoMode = false;
                            }

                            if (IsAutoMode == true)
                            {
                                //if (PMIMap.GetEnable((int)delimiter.MI.XIndex, (int)delimiter.MI.YIndex) == 0x01)
                                //{
                                //    IsEnable = true;
                                //}

                                // 설정 된 기간 사이의 값인지 확인
                                DateTime date = DateTime.ParseExact(delimiter.Date, "MM/dd/yy", null);

                                TimeSpan t1 = date - StartDate;
                                TimeSpan t2 = date - EndDate;

                                if ((t1.Days >= 0) && (t2.Days <= 0))
                                {
                                    IsInPeriod = true;
                                }

                                if (StatusFilter == PadStatusResultEnum.ALL)
                                {
                                    IsCheckedStatus = true;
                                }
                                else if ((StatusFilter == PadStatusResultEnum.FAIL) ||
                                          (StatusFilter == PadStatusResultEnum.PASS)
                                        )
                                {
                                    if (delimiter.Status == StatusFilter.ToString().ToUpper())
                                    {
                                        IsCheckedStatus = true;
                                    }
                                }
                                else
                                {
                                    IsCheckedStatus = false;

                                    LoggerManager.Debug($"[PMIViewerVM], WaferListRefreshCommandFunc() : Can not comapre status. {StatusFilter.ToString()}");
                                }

                                // 조건 만족, 데이터 추가.
                                if (IsInPeriod && IsCheckedStatus)
                                {
                                    // Wafer ID가 없는 경우 Unknown으로 생성 됨.

                                    if (delimiter.WaferID != null && delimiter.WaferID != string.Empty && delimiter.WaferID != "Unknown")
                                    {
                                        PMIWaferInfo tmp = new PMIWaferInfo();
                                        tmp.WaferID = delimiter.WaferID;

                                        tmplist.Add(tmp);
                                    }
                                }
                            }
                        }

                        // 중복 제거 & To ObservableCollection
                        if (tmplist.Count > 0)
                        {
                            List<PMIWaferInfo> distinctlist = tmplist.GroupBy(I => I.WaferID).Select(g => g.First()).ToList();

                            PMIWaferInfos = new ObservableCollection<PMIWaferInfo>(distinctlist);

                            if (PMIWaferInfos.Count > 0)
                            {
                                SelectedWaferInfo = PMIWaferInfos[0];
                                SelectedWaferIndex = 0;
                            }
                        }
                        else
                        {
                            // 데이터가 존재하지 않는 경우 메시지 다이얼로그 출력
                            await this.MetroDialogManager().ShowMessageDialog("[Refresh Wafer Data]", "There is no data.", EnumMessageStyle.Affirmative);
                        }
                    }
                    else
                    {
                        // 기간 설정이 잘못 되어 있는 경우
                        LoggerManager.Debug($"[PMIViewerVM], WaferListRefreshCommandFunc() : The period is wrong. Start = {StartDate.Date}, End = {EndDate.Date}");
                    }

                    LoggerManager.Debug($"[PMIViewerVM], WaferListRefreshCommandFunc() : Total filtered files count : {PMIImageInfolist.Count}");
                }
                else
                {
                    LoggerManager.Error($"[PMIViewerVM], WaferListRefreshCommandFunc() : Directory does not exist.");
                }
                //}
                //else
                //{
                //    LoggerManager.Debug($"[PMIViewerVM], WaferListRefreshCommandFunc() : PMI Map does not exist.");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _ImageReloadCommand;
        public ICommand ImageReloadCommand
        {
            get
            {
                if (null == _ImageReloadCommand) _ImageReloadCommand = new RelayCommand<object>(ImageReloadCommandFunc);
                return _ImageReloadCommand;
            }
        }

        private async void ImageReloadCommandFunc(object parameter)
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
                    //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    // TODO : TEST 
                    //ProgressDialogTestWithCancelButtonAndProgressDisplay();

                    await LoadImage();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public void ProgressDialogTestWithCancelButtonAndProgressDisplay()
        {
            // Easy way to pass data to the async method
            int millisecondsTimeout = 250;

            Action action = delegate ()
            {
                for (int i = 1; i <= 20; i++)
                {
                    ProgressDialog.Current.ReportWithCancellationCheck(i * 5, "Executing step {0}/20...", i);

                    Thread.Sleep(millisecondsTimeout);
                }
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Loading data", action, new ProgressDialogSettings(true, true, false));

            //ProgressDialogResult result = ProgressDialog.Execute(this, "Loading data", 
            //    () => 
            //{
            //    for (int i = 1; i <= 20; i++)
            //    {
            //        ProgressDialog.Current.ReportWithCancellationCheck(i * 5, "Executing step {0}/20...", i);

            //        Thread.Sleep(millisecondsTimeout);
            //    }

            //}, new ProgressDialogSettings(true, true, false));

            if (result.Cancelled)
                MessageBox.Show("ProgressDialog cancelled.");
            else if (result.OperationFailed)
                MessageBox.Show("ProgressDialog failed.");
            else
                MessageBox.Show("ProgressDialog successfully executed.");
        }

        //public EventCodeEnum LoadImage(int GroupIndex = -1, PadDataInGroup PadGroupData = null, PMIPadObject PadData = null)
        public async Task<EventCodeEnum> LoadImage()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            ProgressDialogController controller = null;

            try
            {
                // (1) 현재 로드되어 있는 디바이스의 설정 된, Index에 해당하는 데이터만 로드.
                // (2) 현재 설정되어 있는 시작 시간(StartDate)와 끝 시간(EndDate)에 해당하는 기간의 데이터만 로드.
                // (3) GroupIndex가 -1이 아닌 경우, GroupIndex와 동일한 데이터만 로드.

                MetroWindow metrowindow = this.MetroDialogManager().GetMetroWindow() as MetroWindow;

                //string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
                //SaveBasePath = $"C:\\Logs\\{cellNo}\\Image\\PMI\\";

                //IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                int curMapIndex = 0;

                //bool IsEnable = false;
                bool IsInPeriod = false;
                //bool IsInGroup = false;
                bool IsCheckedStatus = false;
                bool IsSameWaferID = false;

                bool IsAutoMode = false;
                bool GetSuccessDetailInfo = false;

                string DebugLogPath = LoggerManager.GetLogDirPath(EnumLoggerType.DEBUG);
                string PMILogPath = LoggerManager.GetLogDirPath(EnumLoggerType.PMI);

                bool CanAccessPMILogPath = false;

                List<PMILogParser> PMILogParserData = new List<PMILogParser>();

                if (Directory.Exists(Path.GetDirectoryName(PMILogPath)) == true)
                {
                    CanAccessPMILogPath = true;
                }
                else
                {
                    LoggerManager.Error($"[PMIViewerViewModel], LoadImage() : Can not access PMI log path. Path = {PMILogPath}");
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    PMIImageInfolist.Clear();
                    FilteredPMIImageInfolist.Clear();
                });

                SelectedImageItem = null;
                SelectedImageItemIndex = -1;

                //if (PMIInfo.NormalPMIMapTemplateInfo.Count > 0)
                //{
                //    var PMIMap = PMIInfo.NormalPMIMapTemplateInfo[curMapIndex];

                if (Directory.Exists(Path.GetDirectoryName(SaveBasePath)) == true)
                {
                    string[] FilePathArray = Directory.GetFiles(SaveBasePath);

                    LoggerManager.Debug($"[PMIViewerVM], ImageReloadCommandFunc() : Total files count : {FilePathArray.Length}");

                    //DateTime dayEndDate;
                    //DateTime dayStartDate;

                    //string endDateCnv = EndDate.ToString("yyyy-MM-dd");
                    //string[] splitTime = endDateCnv.Split('-');
                    //dayEndDate = new DateTime(Convert.ToInt32(splitTime[0]), Convert.ToInt32(splitTime[1]), Convert.ToInt32(splitTime[2]));

                    //string startDateCnv = StartDate.ToString("yyyy-MM-dd");
                    //splitTime = startDateCnv.Split('-');
                    //dayStartDate = new DateTime(Convert.ToInt32(splitTime[0]), Convert.ToInt32(splitTime[1]), Convert.ToInt32(splitTime[2]));

                    TimeSpan t0 = EndDate - StartDate;

                    if (t0.Days >= 0)
                    {
                        List<string> Datelist = new List<string>();

                        foreach (var file in FilePathArray)
                        {
                            //IsEnable = false;
                            IsInPeriod = false;
                            IsCheckedStatus = false;
                            IsSameWaferID = false;

                            // 파일 이름으로부터, 데이터 추출
                            var delimiter = PMIFileDelimiterParser(SaveBasePath, file);

                            if (delimiter == null)
                            {
                                continue;
                            }

                            // 추출 된 데이터 중, Machine Index를 이용하여, 설정 된 PMI Map의 Enable 데이터 획득
                            // Enable 되어 있는 데이터만 취합

                            // Manual 동작 중, 저장 된 이미지는 사용할 수 없음.
                            if (delimiter.DutNumber >= 0)
                            {
                                IsAutoMode = true;
                            }
                            else
                            {
                                IsAutoMode = false;
                            }

                            if (IsAutoMode == true)
                            {
                                //if (PMIMap.GetEnable((int)delimiter.MI.XIndex, (int)delimiter.MI.YIndex) == 0x01)
                                //{
                                //    IsEnable = true;
                                //}

                                // 설정 된 기간 사이의 값인지 확인
                                DateTime date = DateTime.ParseExact(delimiter.Date, "MM/dd/yy", null);

                                TimeSpan t1 = date - StartDate;
                                TimeSpan t2 = date - EndDate;

                                if ((t1.Days >= 0) && (t2.Days <= 0))
                                {
                                    IsInPeriod = true;
                                }

                                if (StatusFilter == PadStatusResultEnum.ALL)
                                {
                                    IsCheckedStatus = true;
                                }
                                else if ((StatusFilter == PadStatusResultEnum.FAIL) ||
                                          (StatusFilter == PadStatusResultEnum.PASS)
                                        )
                                {
                                    if (delimiter.Status == StatusFilter.ToString().ToUpper())
                                    {
                                        IsCheckedStatus = true;
                                    }
                                }
                                else
                                {
                                    IsCheckedStatus = false;

                                    LoggerManager.Debug($"[PMIViewerVM], LoadImage() : Can not comapre status. {StatusFilter.ToString()}");
                                }


                                if (SelectedWaferInfo != null && SelectedWaferInfo.WaferID != string.Empty && SelectedWaferInfo.WaferID != "Unknown")
                                {
                                    if (delimiter.WaferID == SelectedWaferInfo.WaferID)
                                    {
                                        IsSameWaferID = true;
                                    }
                                }
                                else
                                {
                                    // 선택 된 웨이퍼 정보가 없는 경우
                                    IsSameWaferID = true;
                                }

                                //if (GroupIndex == -1)
                                //{
                                //    IsInGroup = true;
                                //}
                                //else
                                //{
                                //    if (delimiter.GroupIndex == GroupIndex)
                                //    {
                                //        IsInGroup = true;
                                //    }
                                //    else
                                //    {
                                //        IsInGroup = false;
                                //    }
                                //}

                                // 조건 만족, 데이터 추가.
                                if (IsInPeriod && IsCheckedStatus && IsSameWaferID)
                                {
                                    // 바인딩을 위한 이미지 데이터 생성.
                                    var uriSource = new Uri(delimiter.FullPath);

                                    Image newimage = null;

                                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        newimage = new Image();
                                        newimage.Source = new BitmapImage(uriSource);
                                    });

                                    //BitmapImage newimage = new BitmapImage(new Uri(delimiter.FullPath));

                                    delimiter.OriginalImage = newimage;

                                    delimiter.InterestImage = delimiter.OriginalImage;

                                    //// Group 데이터를 입력하지 않은 경우, 디폴트를 원본영상으로

                                    //if (GroupIndex == -1)
                                    //{
                                    //    delimiter.InterestImage = delimiter.OriginalImage;
                                    //}
                                    //else
                                    //{
                                    //    // Group 데이터가 입력 -> 사용자 관심 패드 사용

                                    //    double padwidth = PadData.PadSizeX.Value;
                                    //    double padheight = PadData.PadSizeY.Value;

                                    //    double padwidthWithMargin = padwidth + CropMarginX;
                                    //    double padheightWithMargin = padheight + CropMarginY;

                                    //    double startposx = PadGroupData.PadCenPosInGroup.Value.X - (padwidthWithMargin / 2);
                                    //    double startposy = PadGroupData.PadCenPosInGroup.Value.Y - (padheightWithMargin / 2);

                                    //    CroppedBitmap cb = new CroppedBitmap((BitmapSource)delimiter.OriginalImage.Source, new Int32Rect((int)startposx, (int)startposy, (int)padwidthWithMargin, (int)padheightWithMargin));

                                    //    delimiter.InterestImage.Source = cb;
                                    //}

                                    PMIImageInfolist.Add(delimiter);

                                    Datelist.Add(delimiter.Date);
                                }
                            }
                        }

                        // TODO : 로그 Parse를 통해 추가 데이터 획득 및 할당

                        // (1) PMIImageInfolist를 검색, 분석해야 되는 로그들의 Date 획득 
                        // (2) Date 기반으로, 로컬 Debug log와 PMI log가 존재하는지 확인
                        // (3) PMILogParser 데이터 생성 - Debug log와 PMI log에서 필요한 데이터 추출 및 병합
                        // (4) PMIImageInfolist 재탐색 + 필요한 데이터 추출 및 할당 

                        Datelist = Datelist.GroupBy(I => I).Select(g => g.First()).ToList();

                        //foreach (var imageinfo in PMIImageInfolist)
                        //{
                        //    Datelist.Add(imageinfo.Date);
                        //}

                        //if (CanAccessDebugLogPath == true && CanAccessPMILogPath == true)
                        if (CanAccessPMILogPath == true)
                        {
                            foreach (var item in Datelist)
                            {
                                bool DebugLogFileExist = false;
                                bool PMILogFileExist = false;

                                string[] timedatas = null;

                                if (item.Contains('/'))
                                {
                                    timedatas = item.Split('/');
                                }
                                else if (item.Contains('-'))
                                {
                                    timedatas = item.Split('-');
                                }

                                string date_str = string.Empty;
                                string PMIlogfileFullPath = string.Empty;

                                if (timedatas != null && timedatas.Count() >= 3)
                                {
                                    string datetimestr = $"{timedatas[2]} {timedatas[0]} {timedatas[1]}";

                                    //DateTime dti = Convert.ToDateTime($"{timedatas[2]} {timedatas[0]} {timedatas[1]}");
                                    DateTime dti = DateTime.ParseExact(datetimestr, "yy MM dd", null);

                                    date_str = dti.ToString("yyyy-MM-dd");

                                    //string DebuglogfileFullPath = $"{DebugLogPath}\\Debug_{date_str}.log";
                                    PMIlogfileFullPath = $"{PMILogPath}\\PMI_{date_str}.log";
                                }

                                //if (File.Exists(DebuglogfileFullPath))
                                //{
                                //    DebugLogFileExist = true;
                                //}

                                if (File.Exists(PMIlogfileFullPath))
                                {
                                    PMILogFileExist = true;
                                }

                                //if (DebugLogFileExist == true && PMILogFileExist == true)
                                if (PMILogFileExist == true)
                                {
                                    // TODO : File Read and Analysis & Merge

                                    //List<string[]> PMILogdata = new List<string[]>();
                                    //List<string[]> DebugLogdata = new List<string[]>();
                                    //List<string[]> FilteredDebugLogdata = new List<string[]>();

                                    //PMILogdata.Add(File.ReadAllLines(PMIlogfileFullPath));
                                    //DebugLogdata.Add(File.ReadAllLines(DebuglogfileFullPath));

                                    List<string> PMILogdata = new List<string>();
                                    //List<string> DebugLogdata = new List<string>();
                                    //List<string> FilteredDebugLogdata = new List<string>();

                                    PMILogdata = new List<string>(File.ReadAllLines(PMIlogfileFullPath));
                                    //DebugLogdata = new List<string>(File.ReadAllLines(DebuglogfileFullPath));

                                    bool IncludePMiData = false;

                                    //for (int i = 0; i < DebugLogdata.Count; i++)
                                    //{
                                    //    IncludePMiData = DebugLogdata[i].Contains("MakeMarkStatus");

                                    //    if (IncludePMiData == true)
                                    //    {
                                    //        FilteredDebugLogdata.Add(DebugLogdata[i]);
                                    //    }
                                    //}

                                    //PMILogdata.AddRange(FilteredDebugLogdata);
                                    //PMILogdata.Sort();

                                    PMILogParser logparser = new PMILogParser();

                                    logparser.Date = item;

                                    DateTime StartTime = default(DateTime);
                                    DateTime EndTime = default(DateTime);
                                    int GroupNo = -1;

                                    List<string> tmplogs = new List<string>();

                                    ObservableCollection<SimplificatedPMIPadResult> PMIPadResults = new ObservableCollection<SimplificatedPMIPadResult>();

                                    SimplificatedPMIPadResult padresult = null;

                                    PMILogSet tmpset = null;

                                    bool IsStart = false;
                                    GROUPING_METHOD? GroupbingMethod = null;

                                    int MarkCount = 0;
                                    int MarkIndex = -1;

                                    for (int i = 0; i < PMILogdata.Count; i++)
                                    {
                                        if (IsStart == false)
                                        {
                                            if (PMILogdata[i].Contains(PMISTART) == true &&
                                                PMILogdata[i].Contains(GROUPINGMETHOD) == true &&
                                                PMILogdata[i].Contains("MANUAL") == false)
                                            {
                                                if (PMILogdata[i].Contains("Single"))
                                                {
                                                    GroupbingMethod = GROUPING_METHOD.Single;
                                                }
                                                else if (PMILogdata[i].Contains("Multi"))
                                                {
                                                    GroupbingMethod = GROUPING_METHOD.Multi;
                                                }
                                                else
                                                {
                                                    GroupbingMethod = null;
                                                    LoggerManager.Error($"[PMIViewerVM], LoadImage() : Grouping Method is null.");
                                                }

                                                if (GroupbingMethod != null)
                                                {
                                                    IsStart = true;
                                                }

                                                continue;
                                            }
                                        }

                                        if (IsStart)
                                        {
                                            // 2020-08-03 12:55:28.735 | [DI] | SV = 90.00, PV = 90.10, DP =  0.00 | [Pad Index 1], Detected Mark Count : 0, Pad Size X = 50.778, Pad Size Y = 65.160
                                            if (PMILogdata[i].Contains(GROUPNO) == true)
                                            {
                                                StartTime = DateTime.ParseExact(PMILogdata[i].Substring(0, 23), "yyyy-MM-dd HH\\:mm\\:ss.fff", null);
                                                StartTime = RoundToSecond(StartTime);

                                                GroupNo = StringToIntParser(PMILogdata[i], GROUPNO, ",");
                                            }
                                            else if (PMILogdata[i].Contains(PADINDEX) == true)
                                            {
                                                // Index# 
                                                // Detected Mark Count
                                                // Pad Size X & Y

                                                //if (padresult == null)
                                                //{
                                                padresult = new SimplificatedPMIPadResult();

                                                padresult.PadIndex = StringToIntParser(PMILogdata[i], PADINDEX, "]");

                                                MarkCount = StringToIntParser(PMILogdata[i], DETECTEDMARKCOUNT, ",");
                                                //}
                                            }
                                            else if (PMILogdata[i].Contains(MARKINDEX) == true)
                                            {
                                                // Get mark data

                                                if (MarkCount > 0)
                                                {

                                                    string pattern = @"Mark Index : (?<MarkIndex>\d+), Status : (?<Status>\w+), Scrub Size X : (?<ScrubSizeX>[\d.]+), Scrub Size Y : (?<ScrubSizeY>[\d.]+), Scrub Center X : (?<ScrubCenterX>[-\d.]+), Scrub Center Y : (?<ScrubCenterY>[-\d.]+), Scrub Area : (?<ScrubArea>[\d.]+)um\^2, Proximity : \(Top : (?<ProximityTop>[\d.]+), Bottom : (?<ProximityBottom>[\d.]+), Left : (?<ProximityLeft>[\d.]+), Right : (?<ProximityRight>[\d.]+)\)";
                                                    
                                                    // Match the pattern
                                                    Match match = Regex.Match(PMILogdata[i], pattern);

                                                    if (match.Success == true)
                                                    {
                                                        SimplificatedPMIMarkResult tmpmark = new SimplificatedPMIMarkResult();

                                                        // Extract other values from the sections
                                                        MarkIndex = Convert.ToInt32(match.Groups["MarkIndex"].Value.Trim());
                                                        string status = match.Groups["Status"].Value.Trim();
                                                        string ScrubSizeX = match.Groups["ScrubSizeX"].Value.Trim();
                                                        string ScrubSizeY = match.Groups["ScrubSizeY"].Value.Trim();
                                                        string ScrubCenterX = match.Groups["ScrubCenterX"].Value.Trim();
                                                        string ScrubCenterY = match.Groups["ScrubCenterY"].Value.Trim();
                                                        string ScrubArea = match.Groups["ScrubArea"].Value.Replace("um^2", "").Trim();
                                                        string ProximityTop = match.Groups["ProximityTop"].Value.Trim();
                                                        string ProximityBottom = match.Groups["ProximityBottom"].Value.Trim();
                                                        string ProximityLeft = match.Groups["ProximityLeft"].Value.Trim();
                                                        string ProximityRight = match.Groups["ProximityRight"].Value.Trim();

                                                        try
                                                        {
                                                            tmpmark.ScrubSizeX = Convert.ToDouble(ScrubSizeX);
                                                            tmpmark.ScrubSizeY = Convert.ToDouble(ScrubSizeY);
                                                            tmpmark.ScrubCenterX = Convert.ToDouble(ScrubCenterX);
                                                            tmpmark.ScrubCenterY = Convert.ToDouble(ScrubCenterY);
                                                            tmpmark.ScrubArea = Convert.ToDouble(ScrubArea);
                                                        }
                                                        catch (Exception err)
                                                        {
                                                            LoggerManager.Exception(err);
                                                        }

                                                        try
                                                        {
                                                            tmpmark.MarkProximity = new MarkProximity();
                                                            tmpmark.MarkProximity.Top = Convert.ToDouble(ProximityTop);
                                                            tmpmark.MarkProximity.Bottom = Convert.ToDouble(ProximityBottom);
                                                            tmpmark.MarkProximity.Left = Convert.ToDouble(ProximityLeft);
                                                            tmpmark.MarkProximity.Right = Convert.ToDouble(ProximityRight);
                                                        }
                                                        catch (Exception err)
                                                        {
                                                            LoggerManager.Exception(err);
                                                        }
                                                    
                                                        if (status.ToUpper() == "PASS")
                                                        {
                                                            tmpmark.Status.Add(MarkStatusCodeEnum.PASS);
                                                        }
                                                        else if (status.ToUpper() == "FAIL")
                                                        {
                                                            // NEED Get detail fail reason
                                                        }
                                                        else
                                                        {
                                                            // ERROR
                                                        }

                                                        padresult.MarkResults.Add(tmpmark);
                                                    }
                                                    else
                                                    {
                                                        LoggerManager.Debug($"[PMIViewerVM], LoadImage() : format is not matched. format = {pattern}, data = {PMILogdata[i]}");
                                                    }
                                                }
                                            }
                                            else if (PMILogdata[i].Contains(FAILREASON))
                                            {
                                                if (padresult.MarkResults.Count > 0 && padresult.MarkResults.Count >= MarkIndex)
                                                {
                                                    int IndexStartStr = PMILogdata[i].IndexOf(FAILREASON);

                                                    string[] reasonSplit = PMILogdata[i].Substring(IndexStartStr, PMILogdata[i].Length - IndexStartStr).Replace(FAILREASON, "").Trim().Split('|');

                                                    foreach (var reason in reasonSplit)
                                                    {
                                                        if (reason != string.Empty)
                                                        {
                                                            try
                                                            {
                                                                MarkStatusCodeEnum statuscode = (MarkStatusCodeEnum)Enum.Parse(typeof(MarkStatusCodeEnum), reason.Trim(), true);

                                                                padresult.MarkResults[MarkIndex - 1].Status.Add(statuscode);
                                                            }
                                                            catch (Exception err)
                                                            {
                                                                LoggerManager.Error($"[PMIViewerVM], LoadImage() : Mark status parsing is failed.");

                                                                LoggerManager.Exception(err);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (PMILogdata[i].Contains(MARKSUMMATIONINFO))
                                            {
                                                if (GroupbingMethod == GROUPING_METHOD.Single)
                                                {
                                                    EndTime = DateTime.ParseExact(PMILogdata[i].Substring(0, 23), "yyyy-MM-dd HH\\:mm\\:ss.fff", null);
                                                    EndTime = RoundToSecond(EndTime);

                                                    tmpset = new PMILogSet();

                                                    tmpset.StartTime = (DateTime)StartTime;
                                                    tmpset.EndTime = (DateTime)EndTime;
                                                    tmpset.GroupNo = GroupNo;

                                                    if (padresult != null)
                                                    {
                                                        retval = MakePadStatusUsingMarkResults(padresult);

                                                        SimplificatedPMIPadResult padresultCopy = new SimplificatedPMIPadResult();

                                                        padresultCopy.GroupIndex = GroupNo;

                                                        padresult.CopyTo(padresultCopy);
                                                        PMIPadResults.Add(padresultCopy);

                                                        padresult = null;
                                                    }

                                                    tmpset.PMIPadResult = new ObservableCollection<SimplificatedPMIPadResult>(PMIPadResults);

                                                    PMIPadResults.Clear();

                                                    logparser.PMILogDatas.Add(tmpset);

                                                    //IsStart = false;
                                                    MarkCount = 0;

                                                    StartTime = default(DateTime);
                                                    EndTime = default(DateTime);
                                                    tmplogs.Clear();
                                                }
                                                else if (GroupbingMethod == GROUPING_METHOD.Multi)
                                                {
                                                    if (tmpset == null)
                                                    {
                                                        tmpset = new PMILogSet();
                                                    }

                                                    if (padresult != null)
                                                    {
                                                        retval = MakePadStatusUsingMarkResults(padresult);

                                                        SimplificatedPMIPadResult padresultCopy = new SimplificatedPMIPadResult();

                                                        padresultCopy.GroupIndex = GroupNo;

                                                        padresult.CopyTo(padresultCopy);
                                                        PMIPadResults.Add(padresultCopy);

                                                        padresult = null;
                                                    }
                                                }
                                            }
                                            else if (PMILogdata[i].Contains(PMIEND) == true)
                                            {
                                                if (GroupbingMethod == GROUPING_METHOD.Multi)
                                                {
                                                    EndTime = DateTime.ParseExact(PMILogdata[i].Substring(0, 23), "yyyy-MM-dd HH\\:mm\\:ss.fff", null);
                                                    EndTime = RoundToSecond(EndTime);

                                                    if (tmpset != null)
                                                    {
                                                        tmpset.StartTime = (DateTime)StartTime;
                                                        tmpset.EndTime = (DateTime)EndTime;

                                                        tmpset.PMIPadResult = new ObservableCollection<SimplificatedPMIPadResult>(PMIPadResults);

                                                        PMIPadResults.Clear();

                                                        logparser.PMILogDatas.Add(tmpset);

                                                        MarkCount = 0;

                                                        StartTime = default(DateTime);
                                                        EndTime = default(DateTime);
                                                        tmplogs.Clear();

                                                        tmpset = new PMILogSet();
                                                    }
                                                }

                                                IsStart = false;
                                            }
                                        }
                                    }

                                    // 일 기준 데이터 생성
                                    PMILogParserData.Add(logparser);
                                }
                                else
                                {
                                    this.MetroDialogManager().ShowMessageDialog("[Load Image]", $"PMI logs is not found. \nFile Path = {PMIlogfileFullPath} ", EnumMessageStyle.Affirmative);
                                }
                            }
                        }

                        foreach (var imageinfo in PMIImageInfolist)
                        {
                            imageinfo.PadResult.Clear();

                            PMILogParser parser = PMILogParserData.FirstOrDefault(d => d.Date == imageinfo.Date);

                            if (parser != null && parser.PMILogDatas != null)
                            {
                                // StartTime과 EndTime 

                                // 이미지의 파일이름으로부터 파싱 된 시간을 이용
                                var tmpSet = parser.PMILogDatas.FindAll(x => (x.StartTime <= imageinfo.DateAndTime) && (x.EndTime >= imageinfo.DateAndTime));

                                if (tmpSet != null && tmpSet.Count > 0)
                                {
                                    PMILogSet logSet = null;

                                    if (tmpSet.Count == 1)
                                    {
                                        logSet = tmpSet.First();
                                    }
                                    else
                                    {
                                        logSet = tmpSet.FirstOrDefault(x => x.GroupNo == imageinfo.GroupIndex);
                                    }

                                    if (logSet != null)
                                    {
                                        // 각 이미지에 로그로부터 생성 된 결과 데이터 할당
                                        imageinfo.PadResult = new ObservableCollection<SimplificatedPMIPadResult>(
                                            logSet.PMIPadResult.Where(x => x.GroupIndex == imageinfo.GroupIndex));

                                        if (imageinfo.PadResult.Count > 0)
                                        {
                                            imageinfo.SelectedPadIndex = 0;
                                            imageinfo.SelectedPadResult = imageinfo.PadResult[imageinfo.SelectedPadIndex];

                                            // Set initial selectedItem 
                                            foreach (var pr in imageinfo.PadResult)
                                            {
                                                if (pr.MarkResults.Count > 0)
                                                {
                                                    imageinfo.SelectedMarkIndex = 0;
                                                    imageinfo.SelectedMarkResult = pr.MarkResults[imageinfo.SelectedMarkIndex];
                                                }
                                                else
                                                {
                                                    imageinfo.SelectedMarkIndex = -1;
                                                    imageinfo.SelectedMarkResult = null;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // 패드 정보가 없는 경우
                                            imageinfo.SelectedPadIndex = -1;
                                            imageinfo.SelectedPadResult = null;
                                        }
                                    }

                                }
                            }
                        }

                        if (PMIImageInfolist.Count > 0)
                        {
                            FilteredPMIImageInfolist = new ObservableCollection<PMIImageInformation>(PMIImageInfolist);

                            if (FilteredPMIImageInfolist[0].SelectedPadResult != null)
                            {
                            }
                            else
                            {
                                FilteredPMIImageInfolist[0].SelectedPadIndex = -1;
                            }

                            if (FilteredPMIImageInfolist[0].SelectedMarkResult != null)
                            {
                            }
                            else
                            {
                                FilteredPMIImageInfolist[0].SelectedMarkIndex = -1;
                            }

                            SelectedImageItem = FilteredPMIImageInfolist[0];
                            SelectedImageItemIndex = 0;

                            ChangedSelectedImageItem();

                            ImageInfoGroupboxVisibility = Visibility.Visible;
                            ActiveTabItemIndex = 1;
                        }
                    }
                    else
                    {
                        // 기간 설정이 잘못 되어 있는 경우
                        LoggerManager.Debug($"[PMIViewerVM], ImageReloadCommandFunc() : The period is wrong. Start = {StartDate.Date}, End = {EndDate.Date}");
                    }

                    LoggerManager.Debug($"[PMIViewerVM], ImageReloadCommandFunc() : Total filtered files count : {PMIImageInfolist.Count}");
                }
                else
                {
                    LoggerManager.Error($"[PMIViewerVM], ImageReloadCommandFunc() : Directory does not exist.");
                }
                //}
                //else
                //{
                //    LoggerManager.Debug($"[PMIViewerVM], ImageReloadCommandFunc() : PMI Map does not exist.");
                //}

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //DelimiterCount = FilteredPMIFileDelimiterlist.Count;
            }

            return retval;
        }

        private EventCodeEnum MakePadStatusUsingMarkResults(SimplificatedPMIPadResult padresult)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var PMIDevParam = this.PMIModule().PMIModuleDevParam_IParam as PMIModuleDevParam;

                if (padresult.PadStatus == null)
                {
                    padresult.PadStatus = new ObservableCollection<PadStatusCodeEnum>();
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableNoProbeMark.Value == true)
                {
                    if (padresult.MarkResults.Count <= 0)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.NO_PROBE_MARK);
                    }
                }

                if (PMIDevParam.FailCodeInfo.FailCodeEnableMarkCntOver.Value == true)
                {
                    if (padresult.MarkResults.Count > PMIDevParam.MaximumMarkCnt.Value)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.TOO_MANY_PROBE_MARK);
                    }
                }

                if (padresult.MarkResults.Count > 0)
                {
                    // 모든 마크가 PASS가 아닌 경우

                    bool isFaild = false;

                    foreach (var mark in padresult.MarkResults)
                    {
                        isFaild = mark.Status.ToList().Any(x => x != MarkStatusCodeEnum.PASS);

                        if (isFaild == true)
                        {
                            break;
                        }
                    }

                    if (isFaild == true)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.FAIL);
                    }
                    else
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.PASS);
                    }
                }
                else
                {
                    // 마크 데이터가 없지만, 없을 때, Fail을 내지 않는 조건으로, Pad는 Pass되어야 한다.
                    if (PMIDevParam.FailCodeInfo.FailCodeEnableNoProbeMark.Value == false)
                    {
                        padresult.PadStatus.Add(PadStatusCodeEnum.PASS);
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private DateTime RoundToSecond(DateTime dt)
        {
            DateTime retval = default(DateTime);

            try
            {
                retval = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private int StringToIntParser(string Data, string StartKey, string LastKey)
        {
            int retval = 0;

            try
            {
                string CutStr = Data.Substring(Data.IndexOf(StartKey));
                string ReplaceStr = CutStr.Replace(StartKey, "");
                int lastChrIndex = ReplaceStr.IndexOf(LastKey);
                string LastStr = ReplaceStr.Substring(0, lastChrIndex).Trim();

                retval = Convert.ToInt32(LastStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private GROUPING_METHOD? StringToGroupingMethodParser(string Data, string StartKey, string LastKey)
        {
            GROUPING_METHOD? retval = null;

            try
            {
                string CutStr = Data.Substring(Data.IndexOf(StartKey));
                string ReplaceStr = CutStr.Replace(StartKey, "");
                int lastChrIndex = ReplaceStr.IndexOf(LastKey);
                string LastStr = ReplaceStr.Substring(0, lastChrIndex).Trim();

                retval = (GROUPING_METHOD)Enum.Parse(typeof(GROUPING_METHOD), LastStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private double? StringToDoubleParser(string Data, string StartKey, string LastKey)
        {
            double? retval = null;

            try
            {
                string CutStr = Data.Substring(Data.IndexOf(StartKey));
                string ReplaceStr = CutStr.Replace(StartKey, "");
                int lastChrIndex = ReplaceStr.IndexOf(LastKey);
                string LastStr = ReplaceStr.Substring(0, lastChrIndex).Trim();

                retval = Convert.ToDouble(LastStr);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private RelayCommand<object> _AddUserInterestCommand;
        public ICommand AddUserInterestCommand
        {
            get
            {
                if (null == _AddUserInterestCommand) _AddUserInterestCommand = new RelayCommand<object>(AddUserInterestCommandFunc);
                return _AddUserInterestCommand;
            }
        }

        private void AddUserInterestCommandFunc(object parameter)
        {
            try
            {
                // 맵의 커서 데이터는 MI

                //UserInterestlist.Add(this.CoordinateManager().MachineIndexConvertToUserIndex(new MachineIndex(CurrentCursorX, CurrentCursorY)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ClearUserInterestCommand;
        public ICommand ClearUserInterestCommand
        {
            get
            {
                if (null == _ClearUserInterestCommand) _ClearUserInterestCommand = new RelayCommand<object>(ClearUserInterestCommandFunc);
                return _ClearUserInterestCommand;
            }
        }
        private void ClearUserInterestCommandFunc(object parameter)
        {
            try
            {
                //UserInterestlist.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

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
        //    finally
        //    {
        //        //DelimiterCount = FilteredPMIFileDelimiterlist.Count;
        //    }
        //}

        //private RelayCommand<object> _CropCommand;
        //public ICommand CropCommand
        //{
        //    get
        //    {
        //        if (null == _CropCommand) _CropCommand = new RelayCommand<object>(CropCommandFunc);
        //        return _CropCommand;
        //    }
        //}
        //private void CropCommandFunc(object parameter)
        //{
        //    try
        //    {
        //        // TODO : 설정 된 패드 번호를 이용하여, 원본 이미지에서 해당 패드 영역 추출 및, 바인딩 변경
        //        // 이미 데이터를 갖고 있는 경우 고려

        //        if (InterestPadNumber > 0)
        //        {
        //            // (1) 해당 패드 넘버가 속한 Group 정보 획득
        //            // (2) 저장 된 이미지의 Group Index와 비교
        //            // (3) 동일한 Group인 경우, 해당 패드의 위치 확보

        //            // CROP

        //            IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

        //            int tableindex = 0;

        //            // 각 Group 별, Pad 데이터를 N개 (PadDataInGroup) 갖고 있다.

        //            List<PMIGroupData> pmigroups = PMIInfo.PadTableTemplateInfo[tableindex].Groups.OrderBy(o => o.SeqNum.Value).ToList();

        //            int interestGroupIndex = -1;
        //            PadDataInGroup interestPadData = null;

        //            foreach (var group in pmigroups)
        //            {
        //                // 사용자는 패드 인덱스를 1부터 접근 가능.
        //                interestPadData = group.PadDataInGroup.FirstOrDefault(n => n.PadRealIndex.Value == InterestPadNumber - 1);

        //                if (interestPadData != null)
        //                {
        //                    interestGroupIndex = pmigroups.IndexOf(group);

        //                    break;
        //                }
        //            }

        //            this.StageSupervisor().WaferObject.GetSubsInfo().Pads.PMIPadInfos[0].Index

        //            if (interestPadData != null && interestGroupIndex != -1)
        //            {
        //                LoadImage(interestGroupIndex, interestPadData);
        //            }
        //            else
        //            {
        //                LoggerManager.Debug($"[PMIViewerVM], CropCommandFunc() : Not found interest Group.");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private RelayCommand<object> _CropFreeCommand;
        //public ICommand CropFreeCommand
        //{
        //    get
        //    {
        //        if (null == _CropFreeCommand) _CropFreeCommand = new RelayCommand<object>(CropFreeCommandFunc);
        //        return _CropFreeCommand;
        //    }
        //}
        //private void CropFreeCommandFunc(object parameter)
        //{
        //    try
        //    {
        //        foreach (var data in PMIFileDelimiterlist)
        //        {
        //            data.InterestImage = data.OriginalImage;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand<Object> _PadNumberInputCommand;
        public ICommand PadNumberInputCommand
        {
            get
            {
                if (null == _PadNumberInputCommand) _PadNumberInputCommand = new RelayCommand<Object>(PadNumberInputCommandFunc);
                return _PadNumberInputCommand;
            }
        }

        private void PadNumberInputCommandFunc(Object param)
        {
            try
            {
                bool isChanged = false;

                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;

                int OldValue = Int32.Parse(tb.Text);

                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 1, 10);

                // TODO : Compare 

                var NewValue = Int32.Parse(tb.Text);

                if (NewValue > 0)
                {
                    if (NewValue != OldValue)
                    {
                        LoggerManager.Debug($"[PMIViewerVM], PadNumberInputCommandFunc() : Pad Number is changed. OldValue = {OldValue}, NewValue = {NewValue}");
                        isChanged = true;
                    }

                    if (isChanged == true)
                    {
                        tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    }
                }
                else
                {
                    LoggerManager.Debug($"[PMIViewerVM], PadNumberInputCommandFunc() : Pad Number can not changed. Input value = {NewValue}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion

        //#region ==> ViewSwapCommand
        ////==> Main View와 Mini View를 Swap 하기 위한 버튼과의 Binding Command
        //private RelayCommand<object> _ViewSwapCommand;
        //public ICommand ViewSwapCommand
        //{
        //    get
        //    {
        //        if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
        //        return _ViewSwapCommand;
        //    }
        //}
        //private void ViewSwapFunc(object parameter)
        //{
        //    try
        //    {

        //        object swap = MainViewTarget;
        //        //MainViewTarget = WaferObject;
        //        MainViewTarget = MiniViewTarget;
        //        MiniViewTarget = swap;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //#endregion

        #region Command
        private int _CurrentPadIndex;
        public int CurrentPadIndex
        {
            get { return _CurrentPadIndex; }
            set
            {
                if (value != _CurrentPadIndex)
                {
                    _CurrentPadIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _SetCurXIndex;
        public int SetCurXIndex
        {
            get { return _SetCurXIndex; }
            set
            {
                if (value != _SetCurXIndex)
                {
                    _SetCurXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SetCurYIndex;
        public int SetCurYIndex
        {
            get { return _SetCurYIndex; }
            set
            {
                if (value != _SetCurYIndex)
                {
                    _SetCurYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SetManualXIndex;
        public int SetManualXIndex
        {
            get { return _SetManualXIndex; }
            set
            {
                if (value != _SetManualXIndex)
                {
                    _SetManualXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SetManualYIndex;
        public int SetManualYIndex
        {
            get { return _SetManualYIndex; }
            set
            {
                if (value != _SetManualYIndex)
                {
                    _SetManualYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _CurrentDutIndex;
        public int CurrentDutIndex
        {
            get { return _CurrentDutIndex; }
            set
            {
                if (value != _CurrentDutIndex)
                {
                    _CurrentDutIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private RelayCommand<CUI.Button> _GoToBackScreenViewCommand;
        //public ICommand GoToBackScreenViewCommand
        //{
        //    get
        //    {
        //        if (null == _GoToBackScreenViewCommand) _GoToBackScreenViewCommand = new RelayCommand<CUI.Button>(GoToBackScreenView);
        //        return _GoToBackScreenViewCommand;
        //    }
        //}
        //private void GoToBackScreenView(CUI.Button cuiparam)
        //{
        //    try
        //    {
        //        this.ViewModelManager().BackPreScreenTransition();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private RelayCommand<object> _ViewSwipCommand;
        //public ICommand ViewSwipCommand
        //{
        //    get
        //    {
        //        if (null == _ViewSwipCommand) _ViewSwipCommand = new RelayCommand<object>(ViewSwipFunc);
        //        return _ViewSwipCommand;
        //    }
        //}

        //private void ViewSwipFunc(object parameter)
        //{
        //    try
        //    {
        //        ViewSwip();

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private RelayCommand<object> _CompressCommand1;
        //public ICommand CompressCommand1
        //{
        //    get
        //    {
        //        if (null == _CompressCommand1) _CompressCommand1 = new RelayCommand<object>(CompressCommand1Func);
        //        return _CompressCommand1;
        //    }
        //}

        //private void CompressCommand1Func(object parameter)
        //{
        //    try
        //    {
        //        if (PMIFileDelimiterlist.Count > 0)
        //        {
        //            foreach (var item in PMIFileDelimiterlist)
        //            {
        //                string path = item.FullPath;

        //                byte[] ImageBytes = this.FileManager().CompressFileToStream(path, 16 * 1024);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private RelayCommand<object> _CompressCommand2;
        //public ICommand CompressCommand2
        //{
        //    get
        //    {
        //        if (null == _CompressCommand2) _CompressCommand2 = new RelayCommand<object>(CompressCommand2Func);
        //        return _CompressCommand2;
        //    }
        //}

        //private void CompressCommand2Func(object parameter)
        //{
        //    try
        //    {
        //        if (PMIFileDelimiterlist.Count > 0)
        //        {
        //            foreach (var item in PMIFileDelimiterlist)
        //            {
        //                string path = item.FullPath;

        //                byte[] ImageBytes = this.FileManager().CompressFileToStream(path, ImageSize);
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        private RelayCommand _PlusCommand;
        public ICommand PlusCommand
        {
            get
            {
                if (null == _PlusCommand) _PlusCommand = new RelayCommand(Plus);
                return _PlusCommand;
            }
        }
        private void Plus()
        {
            try
            {

                string Plus = string.Empty;
                ZoomObject.ZoomLevel--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _MinusCommand;
        public ICommand MinusCommand
        {
            get
            {
                if (null == _MinusCommand) _MinusCommand = new RelayCommand(Minus);
                return _MinusCommand;
            }
        }
        private void Minus()
        {
            string Minus = string.Empty;

            try
            {
                ZoomObject.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private IProbeCard _ProbeCardData;
        public IProbeCard ProbeCardData
        {
            get { return _ProbeCardData; }
            set
            {
                if (value != _ProbeCardData)
                {
                    _ProbeCardData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _DutNum;

        public long DutNum
        {
            get { return _DutNum; }
            set
            {
                if (value != _DutNum)
                {
                    _DutNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _PadNum;

        public long PadNum
        {
            get { return _PadNum; }
            set
            {
                if (value != _PadNum)
                {
                    _PadNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        private DutObject _DutObject;
        public DutObject DutObject
        {
            get { return _DutObject; }
            set
            {
                if (value != _DutObject)
                {
                    _DutObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HexagonJogViewModel _MotionJogViewModel;
        public HexagonJogViewModel MotionJogViewModel
        {
            get { return _MotionJogViewModel; }
            set
            {
                if (_MotionJogViewModel != value)
                {
                    _MotionJogViewModel = value;
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    ViewModelManager = this.ViewModelManager();
                    PropertyInfo[] propertyInfos;
                    CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    ICamera curcam = CurCam;
                    StageSupervisor = this.StageSupervisor();
                    MotionManager = this.MotionManager();
                    VisionManager = this.VisionManager();
                    CoordManager = this.CoordinateManager();
                    PnpManager = this.PnPManager();
                    SetCurXIndex = 20;
                    SetCurYIndex = 20;
                    Thread ClearViewPort = new Thread(new ThreadStart(ViewPortClearRun));

                    Base = new MachineCoordinate(0, 0);

                    CurrentPadIndex = 0;
                    CurrentDutIndex = 0;

                    ZoomObject = Wafer;
                    Initialized = true;
                    var CamUI = UcDisplayPort.DisplayPort.AssignedCamearaProperty;

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
        void ViewPortClearRun()
        {
            try
            {
                if (this.DisplayPortDialog().DisplayPortDialogDone == true)
                {
                    ICamera curcam = CurCam;
                    List<LightValueParam> lights = new List<LightValueParam>();

                    foreach (var light in curcam.LightsChannels)
                    {
                        lights.Add(new LightValueParam(light.Type.Value, (ushort)curcam.GetLight(light.Type.Value)));
                    }

                    CurCam = Extensions_IModule.VisionManager(null).GetCam(EnumProberCam.WAFER_HIGH_CAM);
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region INITMODULEBASE

        public EventCodeEnum InitModuleBase()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //ViewTarget = Wafer;

                //Application.Current.Dispatcher.Invoke((Action)(() =>
                //{
                DisplayPort = new DisplayPort() { GUID = new Guid("63802E31-70BF-4814-9F2F-295F69B876CD") };

                Array stagecamvalues = Enum.GetValues(typeof(StageCam));

                foreach (var cam in this.VisionManager().GetCameras())
                {
                    for (int index = 0; index < stagecamvalues.Length; index++)
                    {
                        if (((StageCam)stagecamvalues.GetValue(index)).ToString() == cam.GetChannelType().ToString())
                        {
                            this.VisionManager().SetDisplayChannel(cam, DisplayPort);
                            break;
                        }
                    }
                }

                ((UcDisplayPort.DisplayPort)DisplayPort).DataContext = this;

                Binding bindX = new Binding
                {
                    Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToX, bindX);

                Binding bindY = new Binding
                {
                    Path = new System.Windows.PropertyPath("StageSupervisor.MoveTargetPosY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.MoveToY, bindY);

                Binding bindCamera = new Binding
                {
                    Path = new System.Windows.PropertyPath("CurCam"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding((UcDisplayPort.DisplayPort)DisplayPort, UcDisplayPort.DisplayPort.AssignedCamearaProperty, bindCamera);

                RetVal = EventCodeEnum.NONE;
                //}));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        #endregion
        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = InitModuleBase();

                //MainViewTarget = DisplayPort;
                //MiniViewTarget = Wafer;

                MainViewTarget = Wafer;

                //int Xpos = 0;
                //int Ypos = 0;

                //if (this.ManualContactModule() != null)
                //{
                //    Xpos = (int)this.ManualContactModule().MXYIndex.X;
                //    Ypos = (int)this.ManualContactModule().MXYIndex.Y;
                //}

                //WaferCoordinate wc = this.WaferAligner().MachineIndexConvertToDieCenter(Xpos, Ypos);

                this.PnPManager().PnpLightJog.InitCameraJog(this, CurCam.GetChannelType());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private string _SaveBasePath;
        public string SaveBasePath
        {
            get { return _SaveBasePath; }
            set
            {
                if (value != _SaveBasePath)
                {
                    _SaveBasePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurrentCursorX;
        public int CurrentCursorX
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

        private int _CurrentCursorY;
        public int CurrentCursorY
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

        //private List<UserIndex> _UserInterestlist = new List<UserIndex>();
        //public List<UserIndex> UserInterestlist
        //{
        //    get { return _UserInterestlist; }
        //    set
        //    {
        //        if (value != _UserInterestlist)
        //        {
        //            _UserInterestlist = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Image _CurrentLoadedImage;
        //public Image CurrentLoadedImage
        //{
        //    get { return _CurrentLoadedImage; }
        //    set
        //    {
        //        if (value != _CurrentLoadedImage)
        //        {
        //            _CurrentLoadedImage = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private int _UniformGridColumns;
        //public int UniformGridColumns
        //{
        //    get { return _UniformGridColumns; }
        //    set
        //    {
        //        if (value != _UniformGridColumns)
        //        {
        //            _UniformGridColumns = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private int _UniformGridRows;
        //public int UniformGridRows
        //{
        //    get { return _UniformGridRows; }
        //    set
        //    {
        //        if (value != _UniformGridRows)
        //        {
        //            _UniformGridRows = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        //private double _UniformGridWidth;
        //public double UniformGridWidth
        //{
        //    get { return _UniformGridWidth; }
        //    set
        //    {
        //        if (value != _UniformGridWidth)
        //        {
        //            _UniformGridWidth = value;
        //            ImageWidth = UniformGridWidth / UniformGridColumns;

        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private double _UniformGridHeight;
        //public double UniformGridHeight
        //{
        //    get { return _UniformGridHeight; }
        //    set
        //    {
        //        if (value != _UniformGridHeight)
        //        {
        //            _UniformGridHeight = value;
        //            ImageHeight = UniformGridHeight / UniformGridRows;

        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private double _ImageWidth;
        public double ImageWidth
        {
            get { return _ImageWidth; }
            set
            {
                if (value != _ImageWidth)
                {
                    _ImageWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ImageHeight;
        public double ImageHeight
        {
            get { return _ImageHeight; }
            set
            {
                if (value != _ImageHeight)
                {
                    _ImageHeight = value;
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

        private void init()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    //UserInterestlist.Clear();
                    PMIImageInfolist.Clear();
                    FilteredPMIImageInfolist.Clear();
                    HighlightDies.Clear();
                    PMIWaferInfos.Clear();
                });

                SelectedWaferIndex = -1;
                SelectedWaferInfo = null;

                SelectedImageItem = null;
                SelectedImageItemIndex = -1;

                StatusFilter = PadStatusResultEnum.ALL;

                ActiveTabItemIndex = 0;

                ImageInfoGroupboxVisibility = Visibility.Hidden;

                //DelimiterCount = PMIFileDelimiterlist.Count;

                //CropMarginX = 15;
                //CropMarginY = 15;

                MapOpacity = 0.5;

                SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PMI, true);

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

        #region ==> MainViewZoomVisible
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
        #endregion

        #region ==> MainViewTarget
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
        #endregion
        private int _DutIndex;
        public int DutIndex
        {
            get { return _DutIndex; }
            set
            {
                if (value != _DutIndex)
                {
                    _DutIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region Vision

        private IVisionManager _VisionManager;
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set
            {
                if (value != _VisionManager)
                {
                    _VisionManager = value;
                    RaisePropertyChanged();
                }
            }
        }

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
        #endregion
        #region ==> UcDispaly Port Target Rectangle

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

        #endregion

        #region Light Jog
        protected EventCodeEnum InitLightJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //CurCam 이 할당된 뒤에 호출해야함.
                this.StageSupervisor().PnpLightJog.InitCameraJog(module, camtype);//==> Nick : Light Jog를 Update하여 UI 구성을 함, InitSetup마다 호출 해야함. 

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
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
                // (1) 현재 로드되어 있는 디바이스의 설정 된, Index에 해당하는 데이터만 로드.
                // (2) 현재 설정되어 있는 시작 시간(StartDate)와 끝 시간(EndDate)에 해당하는 기간의 데이터만 로드.
                // (3) GroupIndex가 -1이 아닌 경우, GroupIndex와 동일한 데이터만 로드.

                //string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
                //SaveBasePath = $"C:\\Logs\\{cellNo}\\Image\\PMI\\";

                IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();

                int curMapIndex = 0;

                //bool IsEnable = false;
                bool IsInPeriod = false;
                bool IsCheckedStatus = false;
                bool IsAutoMode = false;

                bool IsCheckedWaferID = false;

                if (PMIInfo.NormalPMIMapTemplateInfo.Count > 0)
                {
                    var PMIMap = PMIInfo.NormalPMIMapTemplateInfo[curMapIndex];

                    if (Directory.Exists(Path.GetDirectoryName(SaveBasePath)) == true)
                    {
                        string[] FilePathArray = Directory.GetFiles(SaveBasePath);

                        TimeSpan t0 = EndDate - StartDate;

                        if (t0.Days >= 0)
                        {
                            foreach (var file in FilePathArray)
                            {
                                //IsEnable = false;
                                IsInPeriod = false;
                                IsCheckedStatus = false;
                                IsCheckedWaferID = false;

                                // 파일 이름으로부터, 데이터 추출
                                var delimiter = PMIFileDelimiterParser(SaveBasePath, file);

                                // 추출 된 데이터 중, Machine Index를 이용하여, 설정 된 PMI Map의 Enable 데이터 획득
                                // Enable 되어 있는 데이터만 취합

                                // Manual 동작 중, 저장 된 이미지는 사용할 수 없음.

                                if (delimiter != null)
                                {
                                    if (delimiter.DutNumber >= 0)
                                    {
                                        IsAutoMode = true;
                                    }
                                    else
                                    {
                                        IsAutoMode = false;
                                    }
                                }

                                if (IsAutoMode == true)
                                {
                                    //if (delimiter.MI != null && delimiter.MI.XIndex >= 0 && delimiter.MI.YIndex >= 0)
                                    //{
                                    //    if (PMIMap.GetEnable((int)delimiter.MI.XIndex, (int)delimiter.MI.YIndex) == 0x01)
                                    //    {
                                    //        IsEnable = true;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    IsEnable = false;
                                    //}

                                    // 설정 된 기간 사이의 값인지 확인
                                    DateTime date = DateTime.ParseExact(delimiter.Date, "MM/dd/yy", null);

                                    TimeSpan t1 = date - StartDate;
                                    TimeSpan t2 = date - EndDate;

                                    if ((t1.Days >= 0) && (t2.Days <= 0))
                                    {
                                        IsInPeriod = true;
                                    }

                                    if (StatusFilter == PadStatusResultEnum.ALL)
                                    {
                                        IsCheckedStatus = true;
                                    }
                                    else if ((StatusFilter == PadStatusResultEnum.FAIL) ||
                                              (StatusFilter == PadStatusResultEnum.PASS)
                                            )
                                    {
                                        if (delimiter.Status == StatusFilter.ToString().ToUpper())
                                        {
                                            IsCheckedStatus = true;
                                        }
                                    }
                                    else
                                    {
                                        IsCheckedStatus = false;

                                        LoggerManager.Debug($"[PMIViewerVM], LoadImage() : Can not comapre status. {StatusFilter.ToString()}");
                                    }

                                    if (SelectedWaferInfo != null && SelectedWaferInfo.WaferID != string.Empty && SelectedWaferInfo.WaferID != "Unknown")
                                    {
                                        if (SelectedWaferInfo.WaferID == delimiter.WaferID)
                                        {
                                            IsCheckedWaferID = true;
                                        }
                                    }
                                    else
                                    {
                                        // 선택 된 웨이퍼 정보가 없는 경우
                                        IsCheckedWaferID = true;
                                    }

                                    // 조건 만족
                                    if (IsInPeriod && IsCheckedStatus && IsCheckedWaferID)
                                    {
                                        retval++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 기간 설정이 잘못 되어 있는 경우
                            LoggerManager.Debug($"[PMIViewerVM], GetTotalImageCount() : The period is wrong. Start = {StartDate.Date}, End = {EndDate.Date}");
                        }

                        LoggerManager.Debug($"[PMIViewerVM], GetTotalImageCount() : Total filtered files count : {PMIImageInfolist.Count}");
                    }
                    else
                    {
                        LoggerManager.Error($"[PMIViewerVM], GetTotalImageCount() : Directory does not exist.");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[PMIViewerVM], GetTotalImageCount() : PMI Map does not exist.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PMIImageInformationPack GetImageFileData(int index)
        {
            PMIImageInformationPack retval = new PMIImageInformationPack();

            try
            {
                int totalcount = PMIImageInfolist.Count;

                if (index >= 0 && totalcount > 0 && index <= totalcount - 1)
                {
                    string path = PMIImageInfolist[index].FullPath;

                    retval.CompressedImageData = this.FileManager().CompressFileToStream(path, ImageSize);
                    retval.PMIImageInfo = PMIImageInfolist[index];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status)
        {
            try
            {
                this.StartDate = Startdate;
                this.EndDate = Enddate;
                this.StatusFilter = Status;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public ObservableCollection<PMIWaferInfo> GetWaferlist()
        {
            WaferListRefreshCommandFunc(null);

            return PMIWaferInfos;
        }

        public void ChangedWaferListItem(PMIWaferInfo pmiwaferinfo)
        {
            try
            {
                if (PMIWaferInfos.Count > 0 && pmiwaferinfo != null)
                {
                    this.SelectedWaferInfo = PMIWaferInfos.FirstOrDefault(x => x.WaferID == pmiwaferinfo.WaferID);
                }

                if (this.SelectedWaferInfo != null)
                {
                    SelectedWaferIndex = PMIWaferInfos.IndexOf(this.SelectedWaferInfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void WaferListClear()
        {
            WaferListClearCommandFunc(null);
        }
        #endregion
    }


}
