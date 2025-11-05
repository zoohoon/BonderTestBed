using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WAUserCoordStandardModule
{
    using ProberInterfaces;
    using ProberInterfaces.WaferAlignEX;
    using RelayCommandBase;
    using System.Windows.Input;
    using ProberInterfaces.PnpSetup;
    using SubstrateObjects;
    using Vision.GraphicsContext;
    using PnPControl;
    using System.Windows;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using LogModule;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    using SerializerUtil;
    using ProberInterfaces.Enum;

    public class UserCoordsStandard : PNPSetupBase, ISetup, ITemplateModule, IParamNode, IDutViewControlVM, IPackagable
    {
        public override Guid ScreenGUID { get; } = new Guid("8DC02412-4BA1-E1F4-E62E-0091081C67A8");
        public override bool Initialized { get; set; } = false;


        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [ParamIgnore]
        public new List<object> Nodes { get; set; }


        private IParam _SysParam;
        [ParamIgnore]
        public IParam SysParam
        {
            get { return _SysParam; }
            set
            {
                if (value != _SysParam)
                {
                    _SysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferObject Wafer { get { return this.StageSupervisor().WaferObject as WaferObject; } }

        public SubModuleMovingStateBase MovingState { get; set; }
        private SubModuleStateBase _AlignModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _AlignModuleState; }
            set
            {
                if (value != _AlignModuleState)
                {
                    _AlignModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MapVertDirectionEnum MapVertDir;
        private MapHorDirectionEnum MapHorDir;

        #region ..//DrawMapDirectionProperty

        //DrawLineModule DrawLine = new DrawLineModule();


        #endregion

        public UserCoordsStandard()
        {

        }

        #region ..//Command & CommandMethod

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.WaferObject.MapViewStageSyncEnable = true;
                this.StageSupervisor().SaveWaferObject();

                if (parameter is EventCodeEnum)
                {
                    if ((EventCodeEnum)parameter == EventCodeEnum.NONE)
                        await base.Cleanup(parameter);
                    return retVal;
                }

                retVal = await base.Cleanup(parameter);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        //public new void CleanUp()
        //{
        //    if (ModuleState.GetState() == AlignModuleStateEnum.MODIFY)
        //    {
        //        ModuleState.Verify();
        //    }
        //    base.CleanUp();
        //    MiniViewTargetVisiability = Visibility.Visible;
        //    MainViewImageSource = null;
        //}

        private RelayCommand _UIModeChangeCommand;
        public ICommand UIModeChangeCommand
        {
            get
            {
                if (null == _UIModeChangeCommand) _UIModeChangeCommand = new RelayCommand(
                    UIModeChange, EvaluationPrivilege.Evaluate(
                            CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                             new Action(() => { ShowMessages("UIModeChange"); }));
                return _UIModeChangeCommand;
            }
        }

        private void UIModeChange()
        {

        }


        //private RelayCommand _Button1Command;
        //public ICommand Button1Command
        //{
        //    get
        //    {
        //        if (null == _Button1Command) _Button1Command
        //                = new RelayCommand(ChangeDirection);
        //        return _Button1Command;
        //    }
        //}

        public IProbeCard ProbeCardInfo => throw new NotImplementedException();

        public new EnumProberCam CamType => throw new NotImplementedException();

        public new double ZoomLevel => throw new NotImplementedException();

        public new bool? AddCheckBoxIsChecked => throw new NotImplementedException();

        public new bool? EnableDragMap => throw new NotImplementedException();

        public new bool? ShowCurrentPos => throw new NotImplementedException();

        public new bool? ShowGrid => throw new NotImplementedException();

        public new bool? ShowPad => throw new NotImplementedException();

        public new bool? ShowPin => throw new NotImplementedException();

        public new bool? ShowSelectedDut => throw new NotImplementedException();

        private void ChangeDirection()
        {
            //try
            //{

            //    if (MapHorDir == MapHorDirectionEnum.RIGHT &&
            //    MapVertDir == MapVertDirectionEnum.UP)
            //    {
            //        MapHorDir = MapHorDirectionEnum.RIGHT;
            //        MapVertDir = MapVertDirectionEnum.DOWN;
            //        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown.png");


            //        OneButton.SetIconSoruceBitmap(Properties.Resources.CoordRightDown2);
            //    }
            //    else if (MapHorDir == MapHorDirectionEnum.RIGHT &&
            //        MapVertDir == MapVertDirectionEnum.DOWN)
            //    {
            //        MapHorDir = MapHorDirectionEnum.LEFT;
            //        MapVertDir = MapVertDirectionEnum.DOWN;

            //        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown.png");
            //        OneButton.SetIconSoruceBitmap(Properties.Resources.CoordLeftDown2);
            //    }
            //    else if (MapHorDir == MapHorDirectionEnum.LEFT &&
            //        MapVertDir == MapVertDirectionEnum.DOWN)
            //    {
            //        MapHorDir = MapHorDirectionEnum.LEFT;
            //        MapVertDir = MapVertDirectionEnum.UP;

            //        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp.png");
            //        OneButton.SetIconSoruceBitmap(Properties.Resources.CoordLeftUp2);
            //    }
            //    else if (MapHorDir == MapHorDirectionEnum.LEFT &&
            //        MapVertDir == MapVertDirectionEnum.UP)
            //    {
            //        MapHorDir = MapHorDirectionEnum.RIGHT;
            //        MapVertDir = MapVertDirectionEnum.UP;
            //        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp.png");
            //        OneButton.SetIconSoruceBitmap(Properties.Resources.CoordRightUp2);
            //    }

            //    Wafer.GetPhysInfo().MapDirX.Value = MapHorDir;
            //    Wafer.GetPhysInfo().MapDirY.Value = MapVertDir;
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}

        }



        private async Task SetLeftUpDirection()
        {
            try
            {
                Task task = new Task(() =>
                    {
                        MapHorDir = MapHorDirectionEnum.LEFT;
                        MapVertDir = MapVertDirectionEnum.UP;
                        
                        Wafer.GetPhysInfo().MapDirX.Value = MapHorDir;
                        Wafer.GetPhysInfo().MapDirY.Value = MapVertDir;
                        UpdateViewDirectionImage(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);

                        var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                        Wafer.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;
                        Wafer.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;

                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPMainViewImageSourceUpdated(MainViewImageSourceArray);
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedMapDirection(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);
                    });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task SetLeftDownDirection()
        {
            try
            {
                Task task = new Task(() =>
                    {
                        MapHorDir = MapHorDirectionEnum.LEFT;
                        MapVertDir = MapVertDirectionEnum.DOWN;
                        
                        Wafer.GetPhysInfo().MapDirX.Value = MapHorDir;
                        Wafer.GetPhysInfo().MapDirY.Value = MapVertDir;
                        UpdateViewDirectionImage(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);

                        var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                        Wafer.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;
                        Wafer.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;

                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPMainViewImageSourceUpdated(MainViewImageSourceArray);
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedMapDirection(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);
                    });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async Task SetRightDownDirection()
        {
            try
            {
                Task task = new Task(() =>
                    {
                        MapHorDir = MapHorDirectionEnum.RIGHT;
                        MapVertDir = MapVertDirectionEnum.DOWN;
                        
                        Wafer.GetPhysInfo().MapDirX.Value = MapHorDir;
                        Wafer.GetPhysInfo().MapDirY.Value = MapVertDir;
                        UpdateViewDirectionImage(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);

                        var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                        Wafer.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;
                        Wafer.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;

                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPMainViewImageSourceUpdated(MainViewImageSourceArray);
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedMapDirection(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);
                    });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task SetRightUpDirection()
        {
            try
            {
                Task task = new Task(() =>
                    {
                        MapHorDir = MapHorDirectionEnum.RIGHT;
                        MapVertDir = MapVertDirectionEnum.UP;
                        
                        Wafer.GetPhysInfo().MapDirX.Value = MapHorDir;
                        Wafer.GetPhysInfo().MapDirY.Value = MapVertDir;
                        UpdateViewDirectionImage(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);

                        var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                        Wafer.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;
                        Wafer.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;

                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPMainViewImageSourceUpdated(MainViewImageSourceArray);
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.ChangedMapDirection(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);
                    });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private async Task SetManualInputControl()
        {
            try
            {
                long userx = Wafer.GetPhysInfo().OrgU.XIndex.Value;
                long usery = Wafer.GetPhysInfo().OrgU.YIndex.Value;
                long machx = Wafer.GetPhysInfo().OrgM.XIndex.Value;
                long machy = Wafer.GetPhysInfo().OrgM.YIndex.Value;

                await base.ShowAdvanceSetupView();

                SetUserCoord();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }


        #endregion


        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ParamValidation();
                //Test Code
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SetupState = new NotCompletedState(this);

                    Initialized = true;

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

                throw err;
            }

            return retval;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = Properties.Resources.Header;
                //AdvanceSetupView = new UC.UCUserCoordStandard();
                //AdvanceSetupViewModel = (IPnpAdvanceSetupViewModel)AdvanceSetupView;
                AdvanceSetupView = new UserCoordAdvanceSetup.View.UserCoordStandardAdvanceSetupView();
                AdvanceSetupViewModel = new UserCoordAdvanceSetup.ViewModel.UserCoordStandardAdvanceSetupViewModel();

                InitPnpModuleStage_AdvenceSetting();

                InitLightJog(this, EnumProberCam.WAFER_LOW_CAM);
                SetNodeSetupState(EnumMoudleSetupState.NONE);

                retVal = InitPNPSetupUI();
                NoneCleanUp = true;
                FiveButton.Command = new AsyncCommand(SetManualInputControl);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(WaferObject.GetPhysInfo()));

                MainViewTarget = this.StageSupervisor().WaferObject;
                this.WaferObject.MapViewStageSyncEnable = false;
                this.WaferObject.MapViewCurIndexVisiablity = true;
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                MotionJogEnabled = false;
                //if (this.WaferAligner().GetWAInnerStateEnum() == WaferAlignInnerStateEnum.SETUP)
                //(AdvanceSetupView as UC.UCUserCoordStandard).SettingData();
                InitPNPSetupUI();
                retVal = await InitSetup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                throw err;
            }
            return retVal;
        }

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();
                PackagableParams.Add(SerializeManager.SerializeToByte(this.GetParam_Wafer().GetPhysInfo()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void UpdateViewDirectionImage(MapHorDirectionEnum mapHorDirection, MapVertDirectionEnum mapVertDirection)
        {
            if(VisionManager.GetDispHorFlip() == DispFlipEnum.FLIP && VisionManager.GetDispVerFlip() == DispFlipEnum.FLIP)
            {
                if (mapHorDirection == MapHorDirectionEnum.RIGHT)
                {
                    if (mapVertDirection == MapVertDirectionEnum.UP)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp2.png");
                    }
                    else if (mapVertDirection == MapVertDirectionEnum.DOWN)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown2.png");
                    }

                }
                else if (mapHorDirection == MapHorDirectionEnum.LEFT)
                {
                    if (mapVertDirection == MapVertDirectionEnum.UP)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp2.png");
                    }

                    else if (mapVertDirection == MapVertDirectionEnum.DOWN)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown2.png");
                    }
                }
            }
            else
            {
                if (mapHorDirection == MapHorDirectionEnum.RIGHT)
                {
                    if (mapVertDirection == MapVertDirectionEnum.UP)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp2.png");
                    }
                    else if (mapVertDirection == MapVertDirectionEnum.DOWN)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown2.png");
                    }

                }
                else if (mapHorDirection == MapHorDirectionEnum.LEFT)
                {
                    if (mapVertDirection == MapVertDirectionEnum.UP)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp2.png");
                    }

                    else if (mapVertDirection == MapVertDirectionEnum.DOWN)
                    {
                        SetMainViewImageSource("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown.png");
                        //OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown2.png");
                    }
                }
            }
        }

        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                
                UseUserControl = UserControlFucEnum.DEFAULT;

                UpdateViewDirectionImage(Wafer.GetPhysInfo().MapDirX.Value, Wafer.GetPhysInfo().MapDirY.Value);


                MainViewTarget = this.StageSupervisor().WaferObject;
                MiniViewTargetVisibility = Visibility.Hidden;


                MiniViewSwapVisibility = Visibility.Hidden;
                LightJogVisibility = Visibility.Hidden;

                MapHorDir = MapHorDirectionEnum.RIGHT;
                MapVertDir = MapVertDirectionEnum.UP;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }



            return Task.FromResult<EventCodeEnum>(retVal);
        }


        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                MainViewTarget = this.StageSupervisor().WaferObject;
                MiniViewTarget = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);

                
                

                if(VisionManager.GetDispHorFlip() == DispFlipEnum.FLIP && VisionManager.GetDispVerFlip() == DispFlipEnum.FLIP)
                {
                    OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown2.png");
                    TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp2.png");
                    ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp2.png");
                    FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown2.png");

                    PadJogUp.CommandParameter = EnumArrowDirection.DOWN;
                    PadJogDown.CommandParameter = EnumArrowDirection.UP;
                    PadJogLeft.CommandParameter = EnumArrowDirection.RIGHT;
                    PadJogRight.CommandParameter = EnumArrowDirection.LEFT;
               

                }
                else
                {
                    OneButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightUp2.png");
                    TwoButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordRightDown2.png");
                    ThreeButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftDown2.png");
                    FourButton.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/CoordLeftUp2.png");
                    PadJogUp.CommandParameter = EnumArrowDirection.UP;
                    PadJogDown.CommandParameter = EnumArrowDirection.DOWN;
                    PadJogLeft.CommandParameter = EnumArrowDirection.LEFT;
                    PadJogRight.CommandParameter = EnumArrowDirection.RIGHT;
                    
                }



                OneButton.Command = new AsyncCommand(SetRightUpDirection);
                TwoButton.Command = new AsyncCommand(SetRightDownDirection);
                ThreeButton.Command = new AsyncCommand(SetLeftDownDirection);
                FourButton.Command = new AsyncCommand(SetLeftUpDirection);


                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowUpW.png");
                PadJogUp.RepeatEnable = true;
                PadJogUp.Command = new RelayCommand<int>(MoveMapIndex);
                
                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowDownW.png");
                PadJogDown.RepeatEnable = true;
                PadJogDown.Command = new RelayCommand<int>(MoveMapIndex);
                
                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowLeftW.png");
                PadJogLeft.RepeatEnable = true;
                PadJogLeft.Command = new RelayCommand<int>(MoveMapIndex);
                
                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/ArrowRightW.png");
                PadJogRight.RepeatEnable = true;
                PadJogRight.Command = new RelayCommand<int>(MoveMapIndex);
                



                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        private void MoveMapIndex(int index)
        {
            try
            {
                //0 UP 1 DOWN 2 LEFT 3 RIGHT
                switch (index)
                {
                    case (int)EnumArrowDirection.UP:
                        Wafer.CurrentMYIndex += 1;
                        break;
                    case (int)EnumArrowDirection.DOWN:
                        Wafer.CurrentMYIndex -= 1;
                        break;
                    case (int)EnumArrowDirection.LEFT:
                        Wafer.CurrentMXIndex -= 1;
                        break;
                    case (int)EnumArrowDirection.RIGHT:
                        Wafer.CurrentMXIndex += 1;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        private EventCodeEnum SetUserCoord()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //Wafer.GetPhysInfo().MapDirX.Value = MapHorDir;
                //Wafer.GetPhysInfo().MapDirY.Value = MapVertDir;

                UserIndex cenU = this.CoordinateManager().WMIndexConvertWUIndex(Wafer.GetPhysInfo().CenM.XIndex.Value, Wafer.GetPhysInfo().CenM.YIndex.Value);
                Wafer.GetPhysInfo().CenU = new ElemUserIndex(cenU.XIndex, cenU.YIndex);

                MachineZeroCoordPos();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "SetUserCoord() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //private bool IsNumber(object obj)
        //{
        //    if (Equals(obj, null))
        //    {
        //        return false;
        //    }

        //    Type objtype = obj.GetType();
        //    objtype = Nullable.GetUnderlyingType(objtype) ?? objtype;

        //    if (objtype.IsPrimitive)
        //    {
        //        return objtype != typeof(bool) &&
        //            objtype != typeof(char) &&
        //            objtype != typeof(IntPtr) &&
        //            objtype != typeof(UIntPtr);
        //    }

        //    return objtype == typeof(decimal);
        //}

        private void MachineZeroCoordPos()
        {
            try
            {
                double MachineZeroPosX = 0.0;
                double MachineZeroPosY = 0.0;
                double IndexSizeX = Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                double IndexSizeY = Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                if (Wafer.GetPhysInfo().MapCountY.Value / 2 != 0) // Y축 다이 갯수가 홀수라면
                {
                    //다이사이즈보다 2/1 더감.
                    if (Wafer.GetPhysInfo().MapCountX.Value / 2 != 0) //X축 다이 갯수가 홀수라면
                    {
                        int DieNumX = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountY.Value / 2)));
                        MachineZeroPosX = (IndexSizeX * DieNumX) + (IndexSizeX / 2);
                        int DieNumY = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountX.Value / 2)));
                        MachineZeroPosY = (IndexSizeY * DieNumY) + (IndexSizeY / 2);
                    }
                    else
                    {
                        int DieNumX = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountY.Value / 2)));
                        MachineZeroPosX = (IndexSizeX * DieNumX);
                        int DieNumY = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountX.Value / 2)));
                        MachineZeroPosY = (IndexSizeY * DieNumY) + (IndexSizeY / 2);
                    }
                }
                else// Y축 다이 갯수가 짝수라면
                {
                    if (Wafer.GetPhysInfo().MapCountX.Value / 2 != 0) //X축 다이 갯수가 홀수라면
                    {
                        int DieNumX = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountY.Value / 2)));
                        MachineZeroPosX = (IndexSizeX * DieNumX) + (IndexSizeX / 2);
                        int DieNumY = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountX.Value / 2)));
                        MachineZeroPosY = (IndexSizeY * DieNumY);
                    }
                    else
                    {
                        int DieNumX = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountY.Value / 2)));
                        MachineZeroPosX = (IndexSizeX * DieNumX);
                        int DieNumY = (int)(Math.Truncate(Convert.ToDouble(Wafer.GetPhysInfo().MapCountX.Value / 2)));
                        MachineZeroPosY = (IndexSizeY * DieNumY);
                    }
                }

                Wafer.GetSubsInfo().MachineCoordZeroPosX = MachineZeroPosX;
                Wafer.GetSubsInfo().MachineCoordZeroPosY = MachineZeroPosY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Wafer.GetPhysInfo().MapDirX.Value != MapHorDirectionEnum.UNDEFINED
                        && Wafer.GetPhysInfo().MapDirY.Value != MapVertDirectionEnum.UNDEFINED)
                {
                    if (Wafer.GetPhysInfo().MapDirX.DoneState != ElementStateEnum.NEEDSETUP
                        && Wafer.GetPhysInfo().MapDirY.DoneState != ElementStateEnum.NEEDSETUP
                        && Wafer.GetPhysInfo().OrgM.XIndex.DoneState != ElementStateEnum.NEEDSETUP
                        && Wafer.GetPhysInfo().OrgM.YIndex.DoneState != ElementStateEnum.NEEDSETUP
                        && Wafer.GetPhysInfo().OrgU.XIndex.DoneState != ElementStateEnum.NEEDSETUP
                         && Wafer.GetPhysInfo().OrgU.YIndex.DoneState != ElementStateEnum.NEEDSETUP)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                        retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {

                //if (Wafer.GetPhysInfo().MapDirX.Value != MapHorDirectionEnum.UNDEFINED
                //        && Wafer.GetPhysInfo().MapDirY.Value != MapVertDirectionEnum.UNDEFINED)
                //{

                //}

                retVal = IsParamChanged | retVal;
                retVal = true;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {
                SetNodeSetupState(EnumMoudleSetupState.NONE);
                //if (IsParameterChanged())
                //    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                //else
                //    SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region //..IPackagable
        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    //SerializeManager.DeserializeFromByte(param, out target, typeof(WaferObject));
                    SerializeManager.DeserializeFromByte(param, out target, typeof(PhysicalInfo));
                    
                    if (target != null)
                    {
                        this.StageSupervisor().WaferObject.SetPhysInfo((IPhysicalInfo)target);
                        break;
                    }
                }

                this.CoordinateManager().UpdateCenM();

                //var CenUx = this.StageSupervisor().WaferObject.GetPhysInfo().CenU.XIndex.Value;
                //var CenUy = this.StageSupervisor().WaferObject.GetPhysInfo().CenU.YIndex.Value;

                //MachineIndex CenDieMI = null;
                //CenDieMI = this.CoordinateManager().UserIndexConvertToMachineIndex(new UserIndex(CenUx, CenUy));

                //Wafer.GetPhysInfo().CenM.XIndex.Value = CenDieMI.XIndex;
                //Wafer.GetPhysInfo().CenM.YIndex.Value = CenDieMI.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
