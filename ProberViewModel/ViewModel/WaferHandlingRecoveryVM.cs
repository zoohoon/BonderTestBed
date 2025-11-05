using System;
using System.Linq;
using System.Threading.Tasks;

namespace WaferHandlingRecoveryViewModel
{
    using Autofac;
    using LoaderControllerBase;
    using LoaderParameters;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.LoaderController;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class WaferHandlingRecoveryVM : IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        #region ==> NextCassetteCommand
        private RelayCommand _NextCassetteCommand;
        public ICommand NextCassetteCommand
        {
            get
            {
                if (null == _NextCassetteCommand) _NextCassetteCommand = new RelayCommand(NextCassetteCommandFunc);
                return _NextCassetteCommand;
            }
        }
        private void NextCassetteCommandFunc()
        {
            try
            {
                if (CassetteNum >= LoaderControllerExt.LoaderInfo.StateMap.CassetteModules.Length - 1)
                    return;

                CassetteNum++;
                CurCassette = LoaderControllerExt.LoaderInfo.StateMap.CassetteModules[CassetteNum];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PrevCassetteCommand
        private RelayCommand _PrevCassetteCommand;
        public ICommand PrevCassetteCommand
        {
            get
            {
                if (null == _PrevCassetteCommand) _PrevCassetteCommand = new RelayCommand(PrevCassetteCommandFunc);
                return _PrevCassetteCommand;
            }
        }
        private void PrevCassetteCommandFunc()
        {
            try
            {
                if (CassetteNum < 1)
                    return;
                CassetteNum--;
                CurCassette = LoaderControllerExt.LoaderInfo.StateMap.CassetteModules[CassetteNum];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> AddTranslateRecipeCommand
        private RelayCommand _AddTranslateRecipeCommand;
        public ICommand AddTranslateRecipeCommand
        {
            get
            {
                if (null == _AddTranslateRecipeCommand) _AddTranslateRecipeCommand = new RelayCommand(AddTranslateRecipeCommandFunc);
                return _AddTranslateRecipeCommand;
            }
        }
        private void AddTranslateRecipeCommandFunc()
        {
            try
            {
                if (_CurSelectedRecipe == null)
                    return;

                if (_CurSelectedRecipe.SrcHolderDataInfo.IconType == _CurSelectedRecipe.DstHolderDataInfo.IconType)
                {
                    double seprationDistance = _CurSelectedRecipe.SrcHolderDataInfo.ParentUserControl.ActualWidth / 30;
                    if (_CurSelectedRecipe.SrcHolderDataInfo.IconType == HolderDataInfo.EnumIconType.Slot)
                        _SlotMoveRecipe = _CurSelectedRecipe;
                    else//==> Hand
                        _HandMoveRecipe = _CurSelectedRecipe;
                }
                else
                    _SlotHandMoveRecipe = _CurSelectedRecipe;

                _CurSelectedRecipe = null;

                UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> DoTranslateRecipeCommand
        private RelayCommand _DoTranslateRecipeCommand;
        public ICommand DoTranslateRecipeCommand
        {
            get
            {
                if (null == _DoTranslateRecipeCommand) _DoTranslateRecipeCommand = new RelayCommand(DoTranslateRecipeCommandFunc);
                return _DoTranslateRecipeCommand;
            }
        }
        private void DoTranslateRecipeCommandFunc()
        {
            try
            {
                Func<TranslateRecipe, bool> DoRecipe = (TranslateRecipe recipe) =>
                {
                    TransferObject targetSub = recipe.SrcHolderDataInfo.ModuleInfo.Substrate;
                    //=> find dest pos
                    ModuleID destPos = recipe.DstHolderDataInfo.ModuleInfo.ID;

                    //(DstHandleIconVM as CassetteIcon).
                    //=> Req to loader
                    if (targetSub != null && destPos != ModuleID.UNDEFINED)
                    {
                        if (LoaderController.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.IDLE)
                        {
                            var editor = LoaderControllerExt.GetLoaderMapEditor();
                            editor.EditorState.SetTransfer(targetSub.ID.Value, destPos);

                            LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                            cmdParam.Editor = editor;
                            bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);
                        }
                    }
                    return true;
                };

                DoRecipe(_SlotMoveRecipe);
                DoRecipe(_HandMoveRecipe);
                DoRecipe(_SlotHandMoveRecipe);
                ClearTranslateRecipeCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> ClearTranslateRecipeCommand
        private RelayCommand _ClearTranslateRecipeCommand;
        public ICommand ClearTranslateRecipeCommand
        {
            get
            {
                if (null == _ClearTranslateRecipeCommand) _ClearTranslateRecipeCommand = new RelayCommand(ClearTranslateRecipeCommandFunc);
                return _ClearTranslateRecipeCommand;
            }
        }
        private void ClearTranslateRecipeCommandFunc()
        {
            try
            {
                if (_SrcHolderDataInfo != null)
                    _SrcHolderDataInfo.UserControl.BorderBrush = null;
                if (_DstHolderDataInfo != null)
                    _DstHolderDataInfo.UserControl.BorderBrush = null;
                _SrcHolderDataInfo = null;
                _DstHolderDataInfo = null;
                _CurSelectedRecipe = null;
                _SlotMoveRecipe = null;
                _HandMoveRecipe = null;
                _SlotHandMoveRecipe = null;
                UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> SacnCassetteCommand
        private RelayCommand _SacnCassetteCommand;
        public ICommand SacnCassetteCommand
        {
            get
            {
                if (null == _SacnCassetteCommand) _SacnCassetteCommand = new RelayCommand(SacnCassetteCommandFunc);
                return _SacnCassetteCommand;
            }
        }
        private void SacnCassetteCommandFunc()
        {
            try
            {
                var cassette = LoaderControllerExt.LoaderInfo.StateMap.CassetteModules.FirstOrDefault();

                if (cassette != null)
                {
                    if (LoaderController.ModuleState.GetState() == ProberInterfaces.ModuleStateEnum.IDLE)
                    {
                        var editor = LoaderControllerExt.GetLoaderMapEditor();
                        editor.EditorState.SetScanCassette(cassette.ID);

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        bool isInjected = CommandManager.SetCommand<ILoaderMapCommand>(this, cmdParam);
                    }
                }
                ClearTranslateRecipeCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> IconClickCommand
        private RelayCommand<Object> _IconClickCommand;
        public ICommand IconClickCommand
        {
            get
            {
                if (null == _IconClickCommand) _IconClickCommand = new RelayCommand<Object>(IconClickCommandFunc);
                return _IconClickCommand;
            }
        }

        Func<HolderDataInfo, bool> SelectHolder;
        private void IconClickCommandFunc(Object param)
        {
            try
            {
                Object[] paramArr = param as Object[];
                if (paramArr == null)
                    return;

                if (paramArr.Length < 3)
                    return;

                HolderModuleInfo moduleInfo = paramArr[0] as HolderModuleInfo;
                if (moduleInfo == null)
                    return;

                UserControl userControl = paramArr[1] as UserControl;
                if (userControl == null)
                    return;


                UserControl parentUserControl = paramArr[2] as UserControl;
                if (parentUserControl == null)
                    return;
                if (parentUserControl is Visual == false)
                    return;


                HolderDataInfo holderDataInfo = new HolderDataInfo(moduleInfo, userControl, parentUserControl);

                if (SelectHolder(holderDataInfo) == false)
                    return;

                if (SelectHolder == SelectSrcHolderDataInfo)
                    SelectHolder = SelectDstHolderDataInfo;
                else
                    SelectHolder = SelectSrcHolderDataInfo;

                _CurSelectedRecipe = new TranslateRecipe(_SrcHolderDataInfo, _DstHolderDataInfo);

                UpdateScreen();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool SelectSrcHolderDataInfo(HolderDataInfo holderDataInfo)
        {
            //==> Source has not wafer
            if (holderDataInfo.ModuleInfo.Substrate == null)
                return false;

            //==> other recipe aready select destination
            if (_SlotMoveRecipe != null && holderDataInfo.ModuleInfo == _SlotMoveRecipe.SrcHolderDataInfo.ModuleInfo)
                return false;
            if (_HandMoveRecipe != null && holderDataInfo.ModuleInfo == _HandMoveRecipe.SrcHolderDataInfo.ModuleInfo)
                return false;
            if (_SlotHandMoveRecipe != null && holderDataInfo.ModuleInfo == _SlotHandMoveRecipe.SrcHolderDataInfo.ModuleInfo)
                return false;

            if (_SrcHolderDataInfo != null)
                _SrcHolderDataInfo.UserControl.BorderBrush = null;

            _SrcHolderDataInfo = holderDataInfo;
            _SrcHolderDataInfo.UserControl.BorderBrush = Brushes.Red;
            _SrcHolderDataInfo.UserControl.BorderThickness = new Thickness(2.0);
            return true;
        }
        private bool SelectDstHolderDataInfo(HolderDataInfo holderDataInfo)
        {
            //==> Destination aready exist wafer
            if (holderDataInfo.ModuleInfo.Substrate != null)
                return false;

            //==> Source and destination is same
            if (holderDataInfo.ModuleInfo == _SrcHolderDataInfo.ModuleInfo)
                return false;

            //==> other recipe aready select destination
            if (_SlotMoveRecipe != null && holderDataInfo.ModuleInfo == _SlotMoveRecipe.DstHolderDataInfo.ModuleInfo)
                return false;
            if (_HandMoveRecipe != null && holderDataInfo.ModuleInfo == _HandMoveRecipe.DstHolderDataInfo.ModuleInfo)
                return false;
            if (_SlotHandMoveRecipe != null && holderDataInfo.ModuleInfo == _SlotHandMoveRecipe.DstHolderDataInfo.ModuleInfo)
                return false;

            if (_DstHolderDataInfo != null)
                _DstHolderDataInfo.UserControl.BorderBrush = null;

            _DstHolderDataInfo = holderDataInfo;
            _DstHolderDataInfo.UserControl.BorderBrush = Brushes.Blue;
            _DstHolderDataInfo.UserControl.BorderThickness = new Thickness(2.0);
            return true;
        }
        #endregion

        #region ==> CassetteNum
        private int _CassetteNum;
        public int CassetteNum
        {
            get { return _CassetteNum; }
            set { _CassetteNum = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ==> CurCassette
        private CassetteModuleInfo _CurCassette;
        public CassetteModuleInfo CurCassette
        {
            get { return _CurCassette; }
            set { _CurCassette = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ==> Enable
        private bool _Enable;
        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; RaisePropertyChanged(); }
        }
        #endregion

        private void UpdateScreen()
        {
            try
            {
                //==> Clear Arrow Line
                ArrowLineList.Clear();

                if (_CurSelectedRecipe != null)
                    DrawLineToScreen(_CurSelectedRecipe, Brushes.Yellow);

                if (_SlotMoveRecipe != null)
                    DrawLineToScreen(_SlotMoveRecipe, Brushes.Red);

                if (_HandMoveRecipe != null)
                    DrawLineToScreen(_HandMoveRecipe, Brushes.Green);

                if (_SlotHandMoveRecipe != null)
                    DrawLineToScreen(_SlotHandMoveRecipe, Brushes.Blue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DrawLineToScreen(TranslateRecipe translateCmd, Brush color)
        {
            try
            {
                /*
                 * [ICON] (1)---(2)
                 *               |
                 *               |
                 *               |
                 *              (3)-->(4) [ICON]
                 * 
                 * (1) : srcIconPortPoint
                 * (2) : betweenPoint1 
                 * (3) : betweenPoint2
                 * (4) : endIconPortPoint
                 */

                if (translateCmd.SrcHolderDataInfo == null)
                    return;
                if (translateCmd.DstHolderDataInfo == null)
                    return;

                Point srcIconPortPoint = translateCmd.SrcHolderDataInfo.GetIconPortPoint();
                Point endIconPortPoint = translateCmd.DstHolderDataInfo.GetIconPortPoint();

                double betweenMiddleX = 0;
                if (translateCmd.SrcHolderDataInfo.IconType == translateCmd.DstHolderDataInfo.IconType)
                {
                    double seprationDistance = translateCmd.SrcHolderDataInfo.ParentUserControl.ActualWidth / 30;
                    if (translateCmd.SrcHolderDataInfo.IconType == HolderDataInfo.EnumIconType.Slot)
                        betweenMiddleX = srcIconPortPoint.X + seprationDistance;
                    else//==> Hand
                        betweenMiddleX = srcIconPortPoint.X - seprationDistance;
                }
                else
                    betweenMiddleX = srcIconPortPoint.X + ((endIconPortPoint.X - srcIconPortPoint.X) / 2);

                Point betweenPoint1 = new Point(betweenMiddleX, srcIconPortPoint.Y);
                Point betweenPoint2 = new Point(betweenMiddleX, endIconPortPoint.Y);

                //==> Draw Arrow Line
                DrawStraight(srcIconPortPoint, betweenPoint1, color);
                DrawStraight(betweenPoint1, betweenPoint2, color);
                DrawStraight(betweenPoint2, endIconPortPoint, color);
                DrawArrowHeader(betweenPoint2, endIconPortPoint, color);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DrawStraight(Point startPt, Point endPt, Brush color)
        {
            try
            {
                Line line = MakeLine(startPt.X, startPt.Y, endPt.X, endPt.Y, color);
                ArrowLineList.Add(line);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DrawArrowHeader(Point startPt, Point endPt, Brush color)
        {
            try
            {
                double vx = endPt.X - startPt.X;
                double vy = endPt.Y - startPt.Y;
                double dist = Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2));
                double length = 10;
                vx /= dist;
                vy /= dist;

                double ax = length * (-vy - vx);
                double ay = length * (vx - vy);

                Line arrowLine1 = MakeLine(endPt.X + ax, endPt.Y + ay, endPt.X, endPt.Y, color);
                Line arrowLine2 = MakeLine(endPt.X - ay, endPt.Y + ax, endPt.X, endPt.Y, color);
                ArrowLineList.Add(arrowLine1);
                ArrowLineList.Add(arrowLine2);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Line MakeLine(double x1, double y1, double x2, double y2, Brush color)
        {
            Line line = new Line();
            try
            {
                line.Stroke = color;
                line.X1 = x1;
                line.Y1 = y1;
                line.X2 = x2;
                line.Y2 = y2;
                line.StrokeThickness = 3;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return line;
        }

        public Autofac.IContainer Container { get; set; }

        public ICommandManager CommandManager => Container.Resolve<ICommandManager>();
        public ILoaderController LoaderController => Container.Resolve<ILoaderController>();

        public ILoaderControllerExtension LoaderControllerExt => LoaderController as ILoaderControllerExtension;

        private HolderDataInfo _SrcHolderDataInfo;
        private HolderDataInfo _DstHolderDataInfo;
        public ObservableCollection<Line> ArrowLineList { get; set; }

        private TranslateRecipe _SlotMoveRecipe;
        private TranslateRecipe _HandMoveRecipe;
        private TranslateRecipe _SlotHandMoveRecipe;
        private TranslateRecipe _CurSelectedRecipe;

        readonly Guid _ViewModelGUID = new Guid("5034A071-F09C-45A2-BCAE-0D9560F572CB");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public void DeInitModule()
        {
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    _CassetteNum = 0;

                    if (LoaderControllerExt.LoaderInfo != null)
                    {
                        if (LoaderControllerExt.LoaderInfo.StateMap.CassetteModules.Length > 0)
                        {
                            _CurCassette = LoaderControllerExt.LoaderInfo.StateMap.CassetteModules[CassetteNum];
                        }

                        _Enable = true;
                        ArrowLineList = new ObservableCollection<Line>();
                        SelectHolder = SelectSrcHolderDataInfo;

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {

                    }

                    Initialized = true;
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
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
    }

    class TranslateRecipe
    {
        public HolderDataInfo SrcHolderDataInfo { get; set; }
        public HolderDataInfo DstHolderDataInfo { get; set; }
        public TranslateRecipe(HolderDataInfo srcHolderDataInfo, HolderDataInfo dstHolderDataInfo)
        {
            try
            {
                SrcHolderDataInfo = srcHolderDataInfo;
                DstHolderDataInfo = dstHolderDataInfo;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    class HolderDataInfo
    {
        public enum EnumIconType { Hand, Slot }
        public EnumIconType IconType { get; set; }
        public HolderModuleInfo ModuleInfo { get; set; }
        public UserControl UserControl { get; set; }
        public UserControl ParentUserControl { get; set; }
        public HolderDataInfo(HolderModuleInfo moduleInfo, UserControl userControl, UserControl parentUserControl)
        {
            try
            {
                if (moduleInfo.ID.ModuleType == ModuleTypeEnum.SLOT)
                    IconType = EnumIconType.Slot;
                else
                    IconType = EnumIconType.Hand;

                ModuleInfo = moduleInfo;
                UserControl = userControl;
                ParentUserControl = parentUserControl;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Point GetIconPortPoint()
        {
            Point iconLeftTop = UserControl.TransformToAncestor(ParentUserControl).Transform(new Point(0, 0));
            Point portPoint = iconLeftTop;
            try
            {
                if (IconType == EnumIconType.Hand)
                {
                    portPoint.Y += UserControl.ActualHeight / 2;
                }
                else
                {
                    portPoint.X += UserControl.ActualWidth;
                    portPoint.Y += UserControl.ActualHeight / 2;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return portPoint;
        }
        public Point GetIconCenterPoint()
        {
            Point iconLeftTop = UserControl.TransformToAncestor(ParentUserControl).Transform(new Point(0, 0));
            Point iconCenter = iconLeftTop;

            try
            {
                iconCenter.X += UserControl.ActualWidth / 2;
                iconCenter.Y += UserControl.ActualHeight / 2;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return iconCenter;
        }
    }
}
