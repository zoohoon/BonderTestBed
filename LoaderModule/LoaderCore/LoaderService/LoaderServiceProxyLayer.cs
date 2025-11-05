namespace LoaderCore.LoaderService
{

    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    //public class LoaderServiceProxyLayer : ILoaderService, INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    private Autofac.IContainer container;

    //    #region //Properties
    //    public ILoaderService LoaderService { get; set; }
    //    public ILoaderService DAL { get; set; }
    //    public IViewModelManager ViewModelManager
    //    {
    //        get { return container.Resolve<IViewModelManager>(); }
    //    }

    //    private string _PipeName = "Service Host: Not initialized.";
    //    public string PipeName
    //    {
    //        get { return _PipeName; }
    //        set
    //        {
    //            if (value != _PipeName)
    //            {
    //                _PipeName = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    #endregion

    //    private string _Data;

    //    public string Data
    //    {
    //        get { return _Data; }
    //        set { _Data = value; }
    //    }

    //    private int _Value;
    //    public int Value
    //    {
    //        get { return _Value; }
    //        set
    //        {
    //            if (value != _Value)
    //            {
    //                _Value = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    public LoaderServiceProxyLayer()
    //    {
    //        Value = 1;
    //    }
    //    public int GetValue()
    //    {
    //        return Value;
    //    }

    //    public void SetValue(int value)
    //    {
    //        Value = value;
    //    }

    //    public EventCodeEnum Connect()
    //    {
    //        return EventCodeEnum.NONE;
    //    }

    //    public EventCodeEnum Disconnect()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool IsServiceAvailable()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum Initialize(string rootParamPath)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public LoaderInfo GetLoaderInfo()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool IsFoupAccessed(int cassetteNumber)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum LoaderSystemInit()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ResponseResult SetRequest(LoaderMap dstMap)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum AwakeProcessModule()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum AbortRequest()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum ClearRequestData()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SelfRecovery()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetPause()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetResume()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public LoaderSystemParameter GetSystemParam()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public LoaderDeviceParameter GetDeviceParam()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum RetractAll()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void FOUP_RaiseWaferOutDetected(int cassetteNumber)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_ChuckUpMove()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_ChuckDownMove()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_WriteARMVacuum(bool value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_MonitorForARMVacuum(bool value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_WaitForARMVacuum(bool value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_RetractARM()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_SafePosW()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_PickUpMove()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_PlaceDownMove()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_SelfRecoveryRetractARM()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum WTR_SelfRecoveryTransferToPreAlign()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool WTR_IsLoadWafer()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_GetOCRImage(out byte[] imgBuf, out int w, out int h)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_ChangeLight(int channel, ushort intensity)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_SetOcrID(string inputOCR)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_OCRRemoteEnd()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_GetOCRState()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_OCRRetry()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum OFR_OCRFail()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum RECOVERY_MotionInit()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum RECOVERY_ResetWaferLocation()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetTestCenteringFlag(bool testflag)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public double GetArmUpOffset()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetLoaderTestOption(LoaderTestOption option)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public LoaderServiceTypeEnum GetServiceType()
    //    {
    //        return LoaderServiceTypeEnum.REMOTE;
    //    }

    //    public void SetContainer(Autofac.IContainer container)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void SetCallBack(ILoaderServiceCallback loadercontroller)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
