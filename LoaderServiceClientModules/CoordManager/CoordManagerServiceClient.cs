using LoaderBase.Communication;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace LoaderServiceClientModules.CoordManager
{
    using Autofac;
    using LogModule;
    using ProberInterfaces.Proxies;
    using SubstrateObjects;

    public class CoordManagerServiceClient : ICoordinateManager, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }

        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        private WaferObject Wafer
        {
            get
            {
                return this.StageSupervisor()?.WaferObject as WaferObject;
            }
        }

        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            NotifyPropertyChanged("SysParam");
        //        }
        //    }
        //}
        private StageCoords _StageCoord;
        public StageCoords StageCoord
        {
            get { return _StageCoord; }
            set
            {
                if (value != _StageCoord)
                {
                    _StageCoord = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private DeviceParams _DeviceParam;
        //public DeviceParams DeviceParam
        //{
        //    get { return _DeviceParam; }
        //    set
        //    {
        //        if (value != _DeviceParam)
        //        {
        //            _DeviceParam = value;
        //            NotifyPropertyChanged("DeviceParam");
        //        }
        //    }
        //}

        //================================


        private double _ChuckCoordXPos;
        public double ChuckCoordXPos
        {
            get { return _ChuckCoordXPos; }
            set
            {
                if (value != _ChuckCoordXPos)
                {
                    _ChuckCoordXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ChuckCoordYPos;
        public double ChuckCoordYPos
        {
            get { return _ChuckCoordYPos; }
            set
            {
                if (value != _ChuckCoordYPos)
                {
                    _ChuckCoordYPos = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _ChuckCoordZPos;
        public double ChuckCoordZPos
        {
            get { return _ChuckCoordZPos; }
            set
            {
                if (value != _ChuckCoordZPos)
                {
                    _ChuckCoordZPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ViewCoordXPos;
        public double ViewCoordXPos
        {
            get { return _ViewCoordXPos; }
            set
            {
                if (value != _ViewCoordXPos)
                {
                    _ViewCoordXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ViewCoordYPos;
        public double ViewCoordYPos
        {
            get { return _ViewCoordYPos; }
            set
            {
                if (value != _ViewCoordYPos)
                {
                    _ViewCoordYPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private UserIndex _CurUserIndex;
        [ParamIgnore]
        public UserIndex CurUserIndex
        {
            get { return _CurUserIndex; }
            set
            {
                if (value != _CurUserIndex)
                {
                    _CurUserIndex = value;
                    RaisePropertyChanged();

                }
            }
        }

        private MachineIndex _MachineIdx;
        [ParamIgnore]
        public MachineIndex MachineIdx
        {
            get { return _MachineIdx; }
            set
            {
                if (value != _MachineIdx)
                {
                    _MachineIdx = value;
                    RaisePropertyChanged();
                }
            }
        }

        Thread UpdateThread;


        WaferCoordinate waferhigh = new WaferCoordinate();
        PinCoordinate pinhigh = new PinCoordinate();
        WaferCoordinate waferlow = new WaferCoordinate();
        PinCoordinate pinlow = new PinCoordinate();



        private WaferHighChuckCoordConvert _WaferHighChuckConvert = new WaferHighChuckCoordConvert();

        public WaferHighChuckCoordConvert WaferHighChuckConvert
        {
            get { return _WaferHighChuckConvert; }
            set { _WaferHighChuckConvert = value; }
        }

        private WaferLowChuckCoordConvert _WaferLowChuckConvert = new WaferLowChuckCoordConvert();

        public WaferLowChuckCoordConvert WaferLowChuckConvert
        {
            get { return _WaferLowChuckConvert; }
            set { _WaferLowChuckConvert = value; }
        }
        private PinHighPinCoordConvert _PinHighPinConvert = new PinHighPinCoordConvert();

        public PinHighPinCoordConvert PinHighPinConvert
        {
            get { return _PinHighPinConvert; }
            set { _PinHighPinConvert = value; }
        }
        private PinLowPinCoordinateConvert _PinLowPinConvert = new PinLowPinCoordinateConvert();

        public PinLowPinCoordinateConvert PinLowPinConvert
        {
            get { return _PinLowPinConvert; }
            set { _PinLowPinConvert = value; }
        }
        private WaferHighNCPadCoordConvert _WaferHighNCPadConvert = new WaferHighNCPadCoordConvert();

        public WaferHighNCPadCoordConvert WaferHighNCPadConvert
        {
            get { return _WaferHighNCPadConvert; }
            set { _WaferHighNCPadConvert = value; }
        }
        private WaferLowNCPadCoordinate _WaferLowNCPadConvert = new ProberInterfaces.WaferLowNCPadCoordinate();

        public WaferLowNCPadCoordinate WaferLowNCPadConvert
        {
            get { return _WaferLowNCPadConvert; }
            set { _WaferLowNCPadConvert = value; }
        }

        //================================
        private Point _WfCenterLowCam;

        public Point WfCenterLowCam
        {
            get { return _WfCenterLowCam; }
            set { _WfCenterLowCam = value; }
        }

        private Point _WfCenterHighCam;

        public Point WfCenterHighCam
        {
            get { return _WfCenterHighCam; }
            set { _WfCenterHighCam = value; }
        }


        private EnumProberCam _CamType;
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _ReverseManualMoveX;

        public bool ReverseManualMoveX
        {
            get { return _ReverseManualMoveX; }
            set { _ReverseManualMoveX = value; }
        }


        private bool _ReverseManualMoveY;

        public bool ReverseManualMoveY
        {
            get { return _ReverseManualMoveY; }
            set { _ReverseManualMoveY = value; }
        }

        private CatCoordinates _CurrentCoordinate = new CatCoordinates();
        [ParamIgnore]
        public CatCoordinates CurrentCoordinate
        {
            get { return _CurrentCoordinate; }
            set
            {
                if (value != _CurrentCoordinate)
                {
                    _CurrentCoordinate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CurUserCoord = new CatCoordinates();

        private EnumAxisConstants _ProberPinAxis;
        public EnumAxisConstants ProberPinAxis
        {
            get { return _ProberPinAxis; }
            private set { _ProberPinAxis = value; }
        }

        private bool _IsCoordToChuckContinus = false;
        public bool IsCoordToChuckContinus
        {
            get { return _IsCoordToChuckContinus; }
            set { _IsCoordToChuckContinus = value; }
        }

        public OverlayUpdate OverlayUpdateDelegate { get; set; }
        public Thread UpdateThread1 { get => UpdateThread; set => UpdateThread = value; }

        public double CalcP2PAngle(double x1, double y1, double x2, double y2)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }

        public double DistanceOfPoints(double x1, double y1, double x2, double y2)
        {
            throw new NotImplementedException();
        }

        public double[,] GetAnyDieCornerPos(UserIndex ui)
        {
            //return LoaderCommunicationManager
            //    .SelectedStage.StageInfo.CoordManagerProxy
            //    .GetAnyDieCornerPos(ui);
            LoggerManager.Debug($"GetAnyDieCornerPos(): Not supported for coordinate manager service client.");
            return new double[2, 2];
        }

        public double[,] GetAnyDieCornerPos(UserIndex ui, EnumProberCam camtype)
        {
            //return LoaderCommunicationManager
            //    .SelectedStage.StageInfo.CoordManagerProxy
            //    .GetAnyDieCornerPos(ui, camtype);
            return new double[2, 2];
        }

        public WaferCoordinate GetChuckCoordinate(EnumProberCam camtype)
        {
            //return LoaderCommunicationManager
            //               .SelectedStage.StageInfo.CoordManagerProxy
            //               .GetChuckCoordinate(camtype);
            LoggerManager.Debug($"GetChuckCoordinate(): Not supported for coordinate manager service client.");
            return new WaferCoordinate();
        }

        public WaferCoordinate GetChuckCoordinate(MachineCoordinate coords, EnumProberCam camtype)
        {
            throw new NotImplementedException();
        }

        public MachineIndex GetCurMachineIndex(WaferCoordinate Pos)
        {
            MachineIndex retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().GetCurMachineIndex(Pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().GetCurMachineIndex(Pos);
        }

        public UserIndex GetCurUserIndex(CatCoordinates Pos)
        {
            UserIndex retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().GetCurUserIndex(Pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().GetCurUserIndex(Pos);
        }
      
        public MachineCoordinate GetRotatedPoint(MachineCoordinate point, MachineCoordinate pivotPoint, double degrees)
        {
            throw new NotImplementedException();
        }

        public void InitCoordinateManager()
        {
            LoggerManager.Debug($"CoordManagerServiceClient(): Initialized.");
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public UserIndex MachineIndexConvertToUserIndex(MachineIndex MI)
        {
            UserIndex retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().MachineIndexConvertToUserIndex(MI);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().MachineIndexConvertToUserIndex(MI);
        }

        public CatCoordinates PmResultConverToUserCoord(PMResult pmresult)
        {
            CatCoordinates retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().PmResultConverToUserCoord(pmresult);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().PmResultConverToUserCoord(pmresult);
        }

        public MachineCoordinate RelPosToAbsPos(MachineCoordinate RelPos)
        {
            MachineCoordinate retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().RelPosToAbsPos(RelPos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().RelPosToAbsPos(RelPos);
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }

        public void SetPinAxisAs(EnumAxisConstants axis)
        {
            throw new NotImplementedException();
        }

        public void StageCoordConvertToChuckCoord()
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().StageCoordConvertToChuckCoord();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //LoaderCommunicationManager.GetCoordManagerClient().StageCoordConvertToChuckCoord();
        }

        public CatCoordinates StageCoordConvertToUserCoord(EnumProberCam camtype)
        {
            CatCoordinates retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().StageCoordConvertToUserCoord(camtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().StageCoordConvertToUserCoord(camtype);
        }

        public void StopStageCoordConvertToChuckCoord()
        {
            try
            {
                LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().StopStageCoordConvertToChuckCoord();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //LoaderCommunicationManager.GetCoordManagerClient().StopStageCoordConvertToChuckCoord();
        }

        public MachineIndex UserIndexConvertToMachineIndex(UserIndex UI)
        {
            MachineIndex retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().UserIndexConvertToMachineIndex(UI);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().UserIndexConvertToMachineIndex(UI);
        }
        public bool GetReverseManualMoveX()
        {          
            bool ret = false;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        ret = LoaderCommunicationManager.SelectedStage.ReverseManualMoveX;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else
                {
                    ret = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public bool GetReverseManualMoveY()
        {
            bool ret = false;
            try
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        ret = LoaderCommunicationManager.SelectedStage.ReverseManualMoveY;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else
                {
                    ret = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }


        private UserIndex convertui = new UserIndex();

        public UserIndex WMIndexConvertWUIndex(long mindexX, long mindexY)
        {
            long uindexx = 0;
            long uindexy = 0;

            try
            {
                IPhysicalInfo physicalinfo = Wafer.GetPhysInfo();

                uindexx = mindexX - physicalinfo.OrgM.XIndex.Value;
                uindexy = mindexY - physicalinfo.OrgM.YIndex.Value;

                uindexx *= (int)physicalinfo.MapDirX.Value;
                uindexy *= (int)physicalinfo.MapDirY.Value;

                uindexx = physicalinfo.OrgU.XIndex.Value + uindexx;
                uindexy = physicalinfo.OrgU.YIndex.Value + uindexy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            convertui.XIndex = uindexx;
            convertui.YIndex = uindexy;

            return convertui;

            //return LoaderCommunicationManager.GetCoordManagerClient().WMIndexConvertWUIndex(mindexX, mindexY);
        }

        public MachineIndex WUIndexConvertWMIndex(long uindexX, long uindexY)
        {
            MachineIndex retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<ICoordinateManagerProxy>().WUIndexConvertWMIndex(uindexX, uindexY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetCoordManagerClient().WUIndexConvertWMIndex(uindexX, uindexY);
        }

        public void InitService()
        {
            return;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        private MachineIndex UserIndexConvertToMachineIndexNotUseProxy(UserIndex UI)
        {
            long mindexx = 0;
            long mindexy = 0;

            try
            {
                mindexx = ((UI.XIndex - Wafer.GetPhysInfo().OrgU.XIndex.Value) * (long)Wafer.GetPhysInfo().MapDirX.Value) + Wafer.GetPhysInfo().OrgM.XIndex.Value;
                mindexy = ((UI.YIndex - Wafer.GetPhysInfo().OrgU.YIndex.Value) * (long)Wafer.GetPhysInfo().MapDirY.Value) + Wafer.GetPhysInfo().OrgM.YIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return new MachineIndex(mindexx, mindexy);
        }

        public void UpdateCenM()
        {
            try
            {
                IPhysicalInfo physicalInfo = Wafer.GetPhysInfo();

                var CenM = UserIndexConvertToMachineIndexNotUseProxy(new UserIndex(physicalInfo.CenU.XIndex.Value, physicalInfo.CenU.YIndex.Value));

                physicalInfo.CenM.XIndex.Value = CenM.XIndex;
                physicalInfo.CenM.YIndex.Value = CenM.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void CalculateOffsetFromCurrentZ(EnumProberCam camchannel)
        {
            return;
        }
    }
}
