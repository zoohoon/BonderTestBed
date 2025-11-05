namespace ProberInterfaces
{

    //public abstract class ManualContactControlVMBase : IMainScreenViewModel, IManualContactControlVM
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion

    //    #region //..IMainScreenViewModel 
    //    public abstract Guid ViewModelGUID { get; }
    //    public bool Initialized { get; set; } = false;

    //    public ManualContactControlVMBase()
    //    {

    //    }
    //    public void DeInitModule()
    //    {
    //        try
    //        {
    //            if (Initialized == false)
    //            {
    //                Initialized = true;
    //            }
    //        }
    //        catch (Exception err)
    //        {

    //            throw err;
    //        }
    //    }

    //    public EventCodeEnum InitModule()
    //    {
    //        try
    //        {
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return EventCodeEnum.NONE;
    //    }

    //    public Task<EventCodeEnum> InitViewModel()
    //    {

    //        PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png");
    //        PadJogLeft.Command = new RelayCommand(DecreaseX);

    //        PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png");
    //        PadJogRight.Command = new RelayCommand(IncreaseX);

    //        PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png");
    //        PadJogUp.Command = new RelayCommand(IncreaseY);

    //        PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png");
    //        PadJogDown.Command = new RelayCommand(DecreaseY);

    //        PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png");
    //        PadJogLeftUp.Command = new RelayCommand(DecreaseXIncreaseY);

    //        PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png");
    //        PadJogRightUp.Command = new RelayCommand(IncreaseXIncreaseY);

    //        PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png");
    //        PadJogLeftDown.Command = new RelayCommand(DecreaseXDecreaseY);

    //        PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png");
    //        PadJogRightDown.Command = new RelayCommand(IncreaseXDecreaseY);

    //        return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
    //    }

    //    public abstract Task<EventCodeEnum> PageSwitched(object parameter = null);
    //    public abstract Task<EventCodeEnum> Cleanup(object parameter = null);

    //    public abstract Task<EventCodeEnum> DeInitViewModel(object parameter = null);
    //    #endregion

    //    #region //..Property
    //    public abstract double FirstContactHeight { get; set; }
    //    public abstract double AllContactHeight { get; set; }
    //    public abstract OverDriveStartPositionType OverDriveStartPosition { get; set; }

    //    public abstract ObservableCollection<IDeviceObject> UnderDutDevs  { get;set;}

    //    public abstract double OverDrive { get; set; }
    //    public abstract bool IsZUpState { get; set; }
    //    #region //..Jog
    //    #region ==> PadJogLeft
    //    private PNPCommandButtonDescriptor _PadJogLeft = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogLeft
    //    {
    //        get { return _PadJogLeft; }
    //        set { _PadJogLeft = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogRight
    //    private PNPCommandButtonDescriptor _PadJogRight = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogRight
    //    {
    //        get { return _PadJogRight; }
    //        set { _PadJogRight = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogUp
    //    private PNPCommandButtonDescriptor _PadJogUp = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogUp
    //    {
    //        get { return _PadJogUp; }
    //        set { _PadJogUp = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogDown
    //    private PNPCommandButtonDescriptor _PadJogDown = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogDown
    //    {
    //        get { return _PadJogDown; }
    //        set { _PadJogDown = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogLeftUp
    //    private PNPCommandButtonDescriptor _PadJogLeftUp = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogLeftUp
    //    {
    //        get { return _PadJogLeftUp; }
    //        set { _PadJogLeftUp = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogRightUp
    //    private PNPCommandButtonDescriptor _PadJogRightUp = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogRightUp
    //    {
    //        get { return _PadJogRightUp; }
    //        set { _PadJogRightUp = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogLeftDown
    //    private PNPCommandButtonDescriptor _PadJogLeftDown = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogLeftDown
    //    {
    //        get { return _PadJogLeftDown; }
    //        set { _PadJogLeftDown = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogRightDown
    //    private PNPCommandButtonDescriptor _PadJogRightDown = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogRightDown
    //    {
    //        get { return _PadJogRightDown; }
    //        set { _PadJogRightDown = value; }
    //    }
    //    #endregion

    //    #region ==> PadJogSelect
    //    private PNPCommandButtonDescriptor _PadJogSelect = new PNPCommandButtonDescriptor();
    //    public PNPCommandButtonDescriptor PadJogSelect
    //    {
    //        get { return _PadJogSelect; }
    //        set { _PadJogSelect = value; }
    //    }
    //    #endregion

    //    #endregion

    //    #endregion

    //    #region //..Command
    //    private AsyncCommand _ControlLoadedCommand;
    //    public ICommand ControlLoadedCommand
    //    {
    //        get
    //        {
    //            if (null == _ControlLoadedCommand)
    //                _ControlLoadedCommand = new AsyncCommand(ControlLoaded);
    //            return _ControlLoadedCommand;
    //        }
    //    }


    //    private RelayCommand _FirstContactSetCommand;
    //    public ICommand FirstContactSetCommand
    //    {
    //        get
    //        {
    //            if (null == _FirstContactSetCommand) _FirstContactSetCommand = new RelayCommand(FirstContactSet);
    //            return _FirstContactSetCommand;
    //        }
    //    }

    //    private RelayCommand _AllContactSetCommand;
    //    public ICommand AllContactSetCommand
    //    {
    //        get
    //        {
    //            if (null == _AllContactSetCommand) _AllContactSetCommand = new RelayCommand(AllContactSet);
    //            return _AllContactSetCommand;
    //        }
    //    }

    //    private RelayCommand _ResetContactStartPositionCommand;
    //    public ICommand ResetContactStartPositionCommand
    //    {
    //        get
    //        {
    //            if (null == _ResetContactStartPositionCommand)
    //                _ResetContactStartPositionCommand = new RelayCommand(ResetContactStartPosition);
    //            return _ResetContactStartPositionCommand;
    //        }
    //    }

    //    private RelayCommand _OverDriveTBClickCommand;
    //    public ICommand OverDriveTBClickCommand
    //    {
    //        get
    //        {
    //            if (null == _OverDriveTBClickCommand) _OverDriveTBClickCommand = new RelayCommand(OverDriveTBClick);
    //            return _OverDriveTBClickCommand;
    //        }
    //    }

    //    private AsyncCommand _ChangeZUpStateCommand;
    //    public ICommand ChangeZUpStateCommand
    //    {
    //        get
    //        {
    //            if (null == _ChangeZUpStateCommand)
    //                _ChangeZUpStateCommand = new AsyncCommand(ChangeZUpStateFunc);
    //            return _ChangeZUpStateCommand;
    //        }
    //    }

    //    private RelayCommand _MoveToWannaZIntervalPlusCommand;
    //    public ICommand MoveToWannaZIntervalPlusCommand
    //    {
    //        get
    //        {
    //            if (null == _MoveToWannaZIntervalPlusCommand)
    //                _MoveToWannaZIntervalPlusCommand = new RelayCommand(MoveToWannaZIntervalPlus);
    //            return _MoveToWannaZIntervalPlusCommand;
    //        }
    //    }

    //    private RelayCommand _WantToMoveZIntervalTBClickCommand;
    //    public ICommand WantToMoveZIntervalTBClickCommand
    //    {
    //        get
    //        {
    //            if (null == _WantToMoveZIntervalTBClickCommand) _WantToMoveZIntervalTBClickCommand = new RelayCommand(WantToMoveZIntervalTBClick);
    //            return _WantToMoveZIntervalTBClickCommand;
    //        }
    //    }
    //    private RelayCommand _MoveToWannaZIntervalMinusCommand;
    //    public ICommand MoveToWannaZIntervalMinusCommand
    //    {
    //        get
    //        {
    //            if (null == _MoveToWannaZIntervalMinusCommand)
    //                _MoveToWannaZIntervalMinusCommand = new RelayCommand(MoveToWannaZIntervalMinus);
    //            return _MoveToWannaZIntervalMinusCommand;
    //        }
    //    }

    //    private AsyncCommand<CUI.Button> _GoToInspectionViewCommand;
    //    public ICommand GoToInspectionViewCommand
    //    {
    //        get
    //        {
    //            if (null == _GoToInspectionViewCommand) _GoToInspectionViewCommand = new AsyncCommand<CUI.Button>(GoToInspectionView);
    //            return _GoToInspectionViewCommand;
    //        }
    //    }

    //    #endregion

    //}
}
