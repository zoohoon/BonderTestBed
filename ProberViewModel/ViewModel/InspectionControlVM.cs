using HexagonJogControl;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.LightJog;
using ProberInterfaces.Param;
using ProberInterfaces.PnpSetup;
using ProberInterfaces.State;
using ProbingModule;
using RelayCommandBase;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UcDisplayPort;
using VirtualKeyboardControl;
using System.Threading;
using ProberInterfaces.Utility;
using CylType;
using MetroDialogInterfaces;

namespace InspectionControlViewModel
{
    enum StageCam
    {
        WAFER_HIGH_CAM,
        WAFER_LOW_CAM,
        PIN_HIGH_CAM,
        PIN_LOW_CAM,
    }

    public class InspectionControlVM : IMainScreenViewModel, IUseLightJog, IInspectionControlVM, IDataErrorInfo, ISetUpState
    {


        readonly Guid _ViewModelGUID = new Guid("652c9ce4-f811-47ab-88f2-da972eeb66d9");
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

        private ProbeAxisObject _AxisX;
        public ProbeAxisObject AxisX
        {
            get { return _AxisX; }
            set
            {
                if (value != _AxisX)
                {
                    _AxisX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _AxisY;
        public ProbeAxisObject AxisY
        {
            get { return _AxisY; }
            set
            {
                if (value != _AxisY)
                {
                    _AxisY = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Property

        private IInspection _Inspection;
        public IInspection Inspection
        {
            get { return _Inspection; }
            set
            {
                if (value != _Inspection)
                {
                    _Inspection = value;
                    RaisePropertyChanged();
                }
            }
        }


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
        private double _UserXShiftValue;
        public double UserXShiftValue
        {
            get
            {
                return _UserXShiftValue;
            }
            set
            {
                if (value != _UserXShiftValue && value != 0)
                {
                    _UserXShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _UserXShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserYShiftValue;
        public double UserYShiftValue
        {
            get
            {
                return _UserYShiftValue;
            }
            set
            {
                if (value != _UserYShiftValue && value != 0)
                {
                    _UserYShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _UserYShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _SystemXShiftValue;
        public double SystemXShiftValue
        {
            get
            {
                return _SystemXShiftValue;
            }
            set
            {
                if (value != _SystemXShiftValue && value != 0)
                {
                    _SystemXShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _SystemXShiftValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SystemYShiftValue;
        public double SystemYShiftValue
        {
            get
            {
                return _SystemYShiftValue;
            }
            set
            {
                if (value != _SystemYShiftValue && value != 0)
                {
                    _SystemYShiftValue = value;
                    RaisePropertyChanged();
                }
                else if (value == 0)
                {
                    _SystemYShiftValue = value;
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

        //private IProbingModule _ProbingModule;
        //public IProbingModule ProbingModule
        //{
        //    get { return _ProbingModule; }
        //    set
        //    {
        //        if (value != _ProbingModule)
        //        {
        //            _ProbingModule = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public IPnpManager PnpManager
        {
            get { return this.PnPManager(); }
            set { }
        }

        #endregion

        #region ==> ViewSwapCommand
        //==> Main View와 Mini View를 Swap 하기 위한 버튼과의 Binding Command
        private RelayCommand<object> _ViewSwapCommand;
        public ICommand ViewSwapCommand
        {
            get
            {
                if (null == _ViewSwapCommand) _ViewSwapCommand = new RelayCommand<object>(ViewSwapFunc);
                return _ViewSwapCommand;
            }
        }
        private void ViewSwapFunc(object parameter)
        {
            try
            {

                object swap = MainViewTarget;
                //MainViewTarget = WaferObject;
                MainViewTarget = MiniViewTarget;
                MiniViewTarget = swap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

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

        private AsyncCommand _PadPrevCommand;
        public IAsyncCommand PadPrevCommand
        {
            get
            {
                if (null == _PadPrevCommand) _PadPrevCommand = new AsyncCommand(FuncPadPrevCommand);
                return _PadPrevCommand;
            }
        }

        private async Task FuncPadPrevCommand()
        {
            try
            {

                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.PrePad(CurCam);
                });
                task.Start();
                await task;
                #region del
                //
                //if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                //{
                //    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count > 0)
                //    {
                //        if (CurrentPadIndex == 0)
                //        {
                //            CurrentPadIndex = (int)this.Wafer.GetSubsInfo().Pads.DutPadInfos.Count - 1;
                //        }
                //        else
                //        {
                //            CurrentPadIndex = CurrentPadIndex - 1;
                //        }
                //        double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.X.Value);
                //        double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Y.Value);
                //        double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Z.Value);


                //        this.StageSupervisor.StageModuleState.WaferLowViewMove(xpos, ypos, zpos);
                //    }
                //}
                //else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                //{
                //    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count > 0)
                //    {
                //        if (CurrentPadIndex == 0)
                //        {
                //            CurrentPadIndex = (int)this.Wafer.GetSubsInfo().Pads.DutPadInfos.Count - 1;
                //        }
                //        else
                //        {
                //            CurrentPadIndex = CurrentPadIndex - 1;
                //        }
                //        double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.X.Value);
                //        double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Y.Value);
                //        double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Z.Value);


                //        this.StageSupervisor.StageModuleState.WaferHighViewMove(xpos, ypos, zpos);
                //    }
                //}
                //else if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                //{

                //    //this.StageSupervisor.StageModuleState.PinLowViewMove(xpos, ypos);
                //}
                //else if (CurCam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM)
                //{
                //    //this.StageSupervisor.StageModuleState.PinHighViewMove(xpos, ypos);
                //}
                //else
                //{

                //}
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        private AsyncCommand _PadNextCommand;
        public IAsyncCommand PadNextCommand
        {
            get
            {
                if (null == _PadNextCommand) _PadNextCommand = new AsyncCommand(FuncPadNextCommand);
                return _PadNextCommand;
            }
        }
        private async Task FuncPadNextCommand()
        {
            try
            {


                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.NextPad(CurCam);
                });
                task.Start();
                await task;
                #region del
                //
                //if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                //{
                //    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count > 0)
                //    {
                //        if (CurrentPadIndex > (int)this.Wafer.GetSubsInfo().Pads.DutPadInfos.Count - 1)
                //        {
                //            CurrentPadIndex = 0;
                //        }
                //        else
                //        {
                //            CurrentPadIndex = CurrentPadIndex + 1;
                //        }
                //        double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.X.Value);
                //        double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Y.Value);
                //        double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Z.Value);


                //        this.StageSupervisor.StageModuleState.WaferLowViewMove(xpos, ypos, zpos);
                //    }
                //}
                //else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                //{
                //    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count > 0)
                //    {
                //        if (CurrentPadIndex == (int)this.Wafer.GetSubsInfo().Pads.DutPadInfos.Count - 1)
                //        {
                //            CurrentPadIndex = 0;
                //        }
                //        else
                //        {
                //            CurrentPadIndex = CurrentPadIndex + 1;
                //        }
                //        double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.X.Value);
                //        double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Y.Value);
                //        double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + Wafer.GetSubsInfo().Pads.DutPadInfos[CurrentPadIndex].PadCenter.Z.Value);

                //        this.StageSupervisor.StageModuleState.WaferHighViewMove(xpos, ypos, zpos);
                //    }
                //}
                //else if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
                //{
                //    //this.StageSupervisor.StageModuleState.PinLowViewMove(xpos, ypos);

                //}
                //else if (CurCam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM)
                //{
                //    //this.StageSupervisor.StageModuleState.PinHighViewMove(xpos, ypos);

                //}
                //else
                //{

                //}
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }

        }
        //private bool ParamValidationFlag = true;
        public string this[string columnName]
        {
            get
            {
                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                if (columnName == "MapIndexX" && (this.MapIndexX < 0 || this.MapIndexX > dieXlength))
                {
                    //ParamValidationFlag = false;
                    return "Out Of Range!";

                }

                return null;
            }
        }
        #region ==> ChangeXManualCommand
        private RelayCommand<Object> _ChangeXManualCommand;
        public ICommand ChangeXManualCommand
        {
            get
            {
                if (null == _ChangeXManualCommand) _ChangeXManualCommand = new RelayCommand<Object>(ChangeXManualCommandFunc);
                return _ChangeXManualCommand;
            }
        }

        private void ChangeXManualCommandFunc(Object param)
        {
            try
            {
                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                double xIndex = 0;
                string xValue = string.Empty;
                bool isSuccessToDouble = false;

                xValue = MapIndexX.ToString();
                xValue = VirtualKeyboard.Show(xValue, KB_TYPE.DECIMAL, 0, 100);
                isSuccessToDouble = double.TryParse(xValue, out xIndex);

                if (isSuccessToDouble)
                {
                    if (xIndex < 0)
                    {
                        MapIndexX = 0;
                        xValue = "0";
                    }
                    else if (xIndex > dieXlength)
                    {
                        MapIndexX = 0;
                        xValue = "0";
                    }
                    else
                    {
                        MapIndexX = (long)xIndex;
                        //Inspection.ManualSetIndexX = (int)MapIndexX;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> ChangeYManualCommand
        private RelayCommand<Object> _ChangeYManualCommand;
        public ICommand ChangeYManualCommand
        {
            get
            {
                if (null == _ChangeYManualCommand) _ChangeYManualCommand = new RelayCommand<Object>(ChangeYManualCommandFunc);
                return _ChangeYManualCommand;
            }
        }

        private void ChangeYManualCommandFunc(Object param)
        {
            try
            {
                int dieYlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(1);
                double yIndex = 0;
                string yValue = string.Empty;
                bool isSuccessToDouble = false;

                yValue = MapIndexY.ToString();
                yValue = VirtualKeyboard.Show(yValue, KB_TYPE.DECIMAL, 0, 100);
                isSuccessToDouble = double.TryParse(yValue, out yIndex);

                if (isSuccessToDouble)
                {
                    if (yIndex < 0)
                    {
                        MapIndexY = 0;
                        yValue = "0";
                    }
                    else if (yIndex > dieYlength)
                    {
                        MapIndexY = 0;
                        yValue = "0";
                    }
                    else
                    {
                        MapIndexY = (long)yIndex;
                        //Inspection.ManualSetIndexY = (int)MapIndexY;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //private AsyncCommand _DUTNextCommand;
        //public ICommand DUTNextCommand
        //{
        //    get
        //    {
        //        if (null == _DUTNextCommand) _DUTNextCommand = new AsyncCommand(FuncDutNextCommand);
        //        return _DUTNextCommand;
        //    }
        //}
        //private async Task FuncDutNextCommand()
        //{
        //    try
        //    {
        //        
        //        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
        //        {
        //            if (this.StageSupervisor.ProbeCardInfo.DutList.Count > 0)
        //            {
        //                CurrentDutIndex = 0;
        //                if (CurrentDutIndex > (int)this.StageSupervisor.ProbeCardInfo.DutList.Count - 1)
        //                {
        //                    CurrentDutIndex = 0;
        //                }
        //                else
        //                {
        //                    CurrentDutIndex = CurrentDutIndex + 1;
        //                }

        //                double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.X.Value);
        //                double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Y.Value);
        //                double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Z.Value);

        //                this.StageSupervisor.StageModuleState.WaferLowViewMove(xpos, ypos, zpos);

        //            }
        //        }
        //        else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
        //        {
        //            if (this.StageSupervisor.ProbeCardInfo.DutList.Count > 0)
        //            {
        //                if (CurrentDutIndex == (int)this.StageSupervisor.ProbeCardInfo.DutList.Count - 1)
        //                {
        //                    CurrentDutIndex = 0;
        //                }
        //                else
        //                {
        //                    CurrentDutIndex = CurrentDutIndex + 1;
        //                }
        //                double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.X.Value);
        //                double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Y.Value);
        //                double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Z.Value);

        //                this.StageSupervisor.StageModuleState.WaferHighViewMove(xpos, ypos, zpos);
        //            }
        //        }
        //        else if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
        //        {
        //            //this.StageSupervisor.StageModuleState.PinLowViewMove(xpos, ypos);

        //        }
        //        else if (CurCam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM)
        //        {
        //            //this.StageSupervisor.StageModuleState.PinHighViewMove(xpos, ypos);

        //        }
        //        else
        //        {

        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        
        //    }

        //}

        //private AsyncCommand _DUTPrevCommand;
        //public ICommand DUTPrevCommand
        //{
        //    get
        //    {
        //        if (null == _DUTPrevCommand) _DUTPrevCommand = new AsyncCommand(FuncDutPrevCommand);
        //        return _DUTPrevCommand;
        //    }
        //}
        //private async Task FuncDutPrevCommand()
        //{
        //    try
        //    {
        //        //
        //        this.ViewModelManager().Lock(this.GetHashCode(), "", "");
        //        if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
        //        {
        //            if (this.StageSupervisor.ProbeCardInfo.DutList.Count > 0)
        //            {
        //                if (CurrentDutIndex == 0)
        //                {
        //                    CurrentDutIndex = this.StageSupervisor.ProbeCardInfo.DutList.Count - 1;
        //                }
        //                else
        //                {
        //                    CurrentDutIndex = CurrentDutIndex - 1;
        //                }
        //                double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.X.Value);
        //                double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Y.Value);
        //                double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Z.Value);

        //                this.StageSupervisor.StageModuleState.WaferLowViewMove(xpos, ypos, zpos);
        //            }
        //        }
        //        else if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
        //        {
        //            if (this.StageSupervisor.ProbeCardInfo.DutList.Count > 0)
        //            {
        //                if (CurrentDutIndex == 0)
        //                {
        //                    CurrentDutIndex = (int)this.StageSupervisor.ProbeCardInfo.DutList.Count - 1;
        //                }
        //                else
        //                {
        //                    CurrentDutIndex = CurrentDutIndex - 1;
        //                }
        //                double xpos = (Wafer.GetPhysInfo().LowLeftCorner.X.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.X.Value);
        //                double ypos = (Wafer.GetPhysInfo().LowLeftCorner.Y.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Y.Value);
        //                double zpos = (Wafer.GetPhysInfo().LowLeftCorner.Z.Value + StageSupervisor.ProbeCardInfo.DutList[CurrentDutIndex].RefCorner.Z.Value);

        //                this.StageSupervisor.StageModuleState.WaferHighViewMove(xpos, ypos, zpos);
        //            }
        //        }
        //        else if (CurCam.GetChannelType() == EnumProberCam.PIN_LOW_CAM)
        //        {
        //            //this.StageSupervisor.StageModuleState.PinLowViewMove(xpos, ypos);

        //        }
        //        else if (CurCam.GetChannelType() == EnumProberCam.PIN_HIGH_CAM)
        //        {
        //            //this.StageSupervisor.StageModuleState.PinHighViewMove(xpos, ypos);

        //        }
        //        else
        //        {

        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        this.ViewModelManager().UnLock(this.GetHashCode());
        //        
        //    }
        //}
        private RelayCommand<CUI.Button> _GoToBackScreenViewCommand;
        public ICommand GoToBackScreenViewCommand
        {
            get
            {
                if (null == _GoToBackScreenViewCommand) _GoToBackScreenViewCommand = new RelayCommand<CUI.Button>(GoToBackScreenView);
                return _GoToBackScreenViewCommand;
            }
        }
        private void GoToBackScreenView(CUI.Button cuiparam)
        {
            try
            {
                this.ViewModelManager().BackPreScreenTransition();

                //Guid ViewGUID = CUIServices.CUIService.GetTargetViewGUID(cuiparam.GUID);
                //this.ViewModelManager().ViewTransition(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private AsyncCommand _ClearCommand;
        public IAsyncCommand ClearCommand
        {
            get
            {
                if (null == _ClearCommand) _ClearCommand = new AsyncCommand(FuncClearCommand);
                return _ClearCommand;
            }
        }
        private async Task FuncClearCommand()
        {
            try
            {

                Task task = new Task(() =>
                {
                    Clear();
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());


            }

        }
        public void Clear()
        {
            try
            {
                LoggerManager.Debug($"InspectionControlVM : Clear()");

                param.UserProbeMarkShift.Value.X.Value = 0;
                param.UserProbeMarkShift.Value.Y.Value = 0;
                UserXShiftValue = 0;
                UserYShiftValue = 0;
                this.ProbingModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SystemClear()
        {
            try
            {
                LoggerManager.Debug($"InspectionControlVM : SystemClear()");

                param.ProbeMarkShift.Value.X.Value = 0;
                param.ProbeMarkShift.Value.Y.Value = 0;
                SystemXShiftValue = 0;
                SystemYShiftValue = 0;
                this.ProbingModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _SetFromCommand;
        public IAsyncCommand SetFromCommand
        {
            get
            {
                if (null == _SetFromCommand) _SetFromCommand = new AsyncCommand(FuncSetFromCommand);
                return _SetFromCommand;
            }
        }
        private async Task FuncSetFromCommand()
        {
            try
            {


                Task task = new Task(() =>
                {
                    SetFromToggle = !SetFromToggle;
                    SetFrom();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }


        private bool _SetFromToggle;
        public bool SetFromToggle
        {
            get
            {
                return _SetFromToggle;
            }
            set
            {
                _SetFromToggle = value;
                RaisePropertyChanged();
            }
        }

        private AsyncCommand _SetIndexCommand;
        public ICommand SetIndexCommand
        {
            get
            {
                if (null == _SetIndexCommand) _SetIndexCommand = new AsyncCommand(FuncSetIndexCommand);
                return _SetIndexCommand;
            }
        }
        private async Task FuncSetIndexCommand()
        {
            try
            {
                Task task = new Task(() =>
                {

                    if (ToggleSetIndex == true)
                    {
                        SetManualXIndex = (int)DisplayPort.AssignedCamera.CamSystemUI.XIndex;
                        SetManualYIndex = (int)DisplayPort.AssignedCamera.CamSystemUI.YIndex;
                    }
                    else
                    {
                        SetManualXIndex = (int)MapIndexX;
                        SetManualYIndex = (int)MapIndexY;
                        //Inspection.ManualSetIndexX = (int)MapIndexX;
                        //Inspection.ManualSetIndexY = (int)MapIndexY;
                    }
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }
        public ProbingModuleSysParam param
        {
            get { return this.ProbingModule().ProbingModuleSysParam_IParam as ProbingModuleSysParam; }
        }

        public ProbingModuleDevParam devparam
        {
            get { return this.ProbingModule().ProbingModuleDevParam_IParam as ProbingModuleDevParam; }
        }


        private AsyncCommand<Object> _ShiftTextBoxClickCommand;
        public ICommand ShiftTextBoxClickCommand
        {
            get
            {
                if (null == _ShiftTextBoxClickCommand) _ShiftTextBoxClickCommand = new AsyncCommand<Object>(ShiftTextBoxClickCommandFunc);
                return _ShiftTextBoxClickCommand;
            }
        }

        private async Task ShiftTextBoxClickCommandFunc(object param)
        {
            try
            {
                EventCodeEnum ret = EventCodeEnum.UNDEFINED;
                System.Windows.Controls.TextBox tb = null;
                EnumAxisConstants axis = EnumAxisConstants.Undefined;
                (System.Windows.Controls.TextBox, double) paramVal = (null, 0.0);
                string backup = "";
                double checkvalue = -1;
                string userPrefix = "User";
                string systemPrefix = "System";
                string xSuffix = "X";
                string ySuffix = "Y";

                await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    paramVal = ((System.Windows.Controls.TextBox, double))param;
                    tb = paramVal.Item1;

                    string tbName = tb.Name;
                    string userPart = NameExtraction(tbName, userPrefix);
                    string systemPart = NameExtraction(tbName, systemPrefix);
                    string axisPart = AxisExtraction(tbName, xSuffix, ySuffix);

                    if (axisPart == xSuffix)
                    {
                        axis = EnumAxisConstants.X;
                    }
                    else if (axisPart == ySuffix)
                    {
                        axis = EnumAxisConstants.Y;
                    }
                    backup = tb.Text;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);
                    if (!double.TryParse(tb.Text, out checkvalue))
                    {
                        checkvalue = -1;
                    }

                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                }));

                if (checkvalue != -1 && axis != EnumAxisConstants.Undefined)
                {
                    double userValue = GetUserValue(axis);
                    double systemValue = GetSystemValue(axis);
                    double totalValue = userValue + systemValue;

                    ret = await CheckPMShiftLimit(totalValue);

                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ret != EventCodeEnum.NONE) // 유효하지 않는 경우 원래 값으로 돌린다.
                        {
                            tb.Text = backup;
                            tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                        }
                    }));
                }
                if (ret == EventCodeEnum.NONE)
                {
                    paramVal.Item2 = checkvalue;
                    SetUserSystemMarkShiftValue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private string NameExtraction(string tbName, string prefix)
        {
            return tbName.StartsWith(prefix) ? prefix : string.Empty;
        }
        private string AxisExtraction(string tbName, string xSuffix, string ySuffix)
        {
            if (tbName.Contains(xSuffix))
            {
                return xSuffix;
            }

            if (tbName.Contains(ySuffix))
            {
                return ySuffix;
            }

            return string.Empty;
        }
        private double GetUserValue(EnumAxisConstants axis)
        {
            return axis == EnumAxisConstants.X ? UserXShiftValue : UserYShiftValue;
        }
        private double GetSystemValue(EnumAxisConstants axis)
        {
            return axis == EnumAxisConstants.X ? SystemXShiftValue : SystemYShiftValue;
        }
        private async Task SetUserSystemMarkShiftValue()
        {   
            try
            {
                param.UserProbeMarkShift.Value.X.Value = UserXShiftValue;
                param.UserProbeMarkShift.Value.Y.Value = UserYShiftValue;
                param.ProbeMarkShift.Value.X.Value = SystemXShiftValue;
                param.ProbeMarkShift.Value.Y.Value = SystemYShiftValue;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _ApplyCommand;
        public IAsyncCommand ApplyCommand
        {
            get
            {
                if (null == _ApplyCommand) _ApplyCommand = new AsyncCommand(FuncApplyCommand);
                return _ApplyCommand;
            }
        }
        private async Task FuncApplyCommand()
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;
                Task task = new Task(() =>
                {
                    if (SetFromToggle == true)
                    {
                        double axisX = this.AxisX.Status.Position.Ref;
                        double axisY = this.AxisY.Status.Position.Ref;

                        double xShift = Math.Round(axisX - XSetFromCoord, 1);
                        double yShift = Math.Round(axisY - YSetFromCoord, 1);

                        if (LimitCheck(xShift) && LimitCheck(yShift))
                        {
                            LoggerManager.Debug($"InspectionControlVM : ApplyRemoteCommand() UserXShift : {UserXShiftValue:0.00} UserYShift : {UserYShiftValue:0.00}");

                            UserXShiftValue = xShift;
                            UserYShiftValue = yShift;

                            param.UserProbeMarkShift.Value.X.Value = UserXShiftValue;
                            param.UserProbeMarkShift.Value.Y.Value = UserYShiftValue;
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("Limit Error",
                               $"Set ProbeMarkShift (X,Y) = ({xShift}, {yShift}) \nProbe Mark Shift X UpperLimit : {param.ProbeMarkShift.Value.X.UpperLimit:0.00}, X LowerLimit : {param.ProbeMarkShift.Value.X.LowerLimit:0.00} \n" +
                               $"Probe Mark Shift Y UpperLimit : {param.ProbeMarkShift.Value.Y.UpperLimit:0.00}, Y LowerLimit : {param.ProbeMarkShift.Value.Y.LowerLimit:0.00}", EnumMessageStyle.Affirmative);

                            LoggerManager.Debug($"InspectionControlVM : ApplyRemoteCommand() Limit Error");
                        }

                        SetFromToggle = false;

                        retval = this.ProbingModule().SaveSysParameter();
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


        private RelayCommand<object> _ViewSwipCommand;
        public ICommand ViewSwipCommand
        {
            get
            {
                if (null == _ViewSwipCommand) _ViewSwipCommand = new RelayCommand<object>(ViewSwipFunc);
                return _ViewSwipCommand;
            }
        }

        private void ViewSwipFunc(object parameter)
        {
            try
            {
                ViewSwip();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
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

        //private RelayCommand _SetZeroCommand;
        //public ICommand SetZeroCommand
        //{
        //    get
        //    {
        //        if (null == _SetZeroCommand) _SetZeroCommand = new RelayCommand(SetZero);
        //        return _SetZeroCommand;
        //    }
        //}
        //private void SetZero()
        //{
        //    try
        //    {

        //        if (this.CoordinateManager() != null)
        //        {
        //            //_PosCoord = DisplayPort.StageSuperVisor.
        //            //    CoordinateManager.StageCoordConvertToUserCoord(DisplayPort.AssignedCamera.Param.ChannelType);
        //            double zeroposx = 0.0;
        //            double zeroposy = 0.0;
        //            double zeroposz = 0.0;

        //            this.MotionManager().GetActualPos(EnumAxisConstants.X, ref zeroposx);
        //            ((UcDisplayPort.DisplayPort)DisplayPort).ZeroPosX = zeroposx;

        //            this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref zeroposy);
        //            ((UcDisplayPort.DisplayPort)DisplayPort).ZeroPosY = zeroposy;

        //            this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref zeroposz);
        //            ((UcDisplayPort.DisplayPort)DisplayPort).ZeroPosZ = zeroposz;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }

        //}

        #endregion

        public EventCodeEnum MovedDelegate(ImageBuffer img)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SetFromToggle == true)
                {
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
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
                    Inspection = this.InspectionModule();
                    SetFromToggle = false;
                    SetCurXIndex = 20;
                    SetCurYIndex = 20;
                    Thread ClearViewPort = new Thread(new ThreadStart(ViewPortClearRun));

                    Base = new MachineCoordinate(0, 0);

                    CurrentPadIndex = 0;
                    CurrentDutIndex = 0;

                    ZoomObject = Wafer;

                    Initialized = true;
                    var CamUI = UcDisplayPort.DisplayPort.AssignedCamearaProperty;
                    //MapIndexX = (int)DisplayPort.AssignedCamera.CamSystemUI.XIndex;
                    //MapIndexY = (int)DisplayPort.AssignedCamera.CamSystemUI.YIndex;

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
                ViewTarget = Wafer;

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

                MainViewTarget = DisplayPort;
                MiniViewTarget = Wafer;

                //int Xpos = (int)this.ManualContactModule().MXYIndex.X;
                //int Ypos = (int)this.ManualContactModule().MXYIndex.Y;

                //if (Inspection != null)
                //{
                //    Inspection.ManualSetIndexX = 0;
                //    Inspection.ManualSetIndexY = 0;
                //}

                //WaferCoordinate wc = this.WaferAligner().MIndexToWPos(Xpos, Ypos, true);
                //WaferCoordinate wc = this.WaferAligner().MachineIndexConvertToDieCenter(Xpos, Ypos);

                Task task = new Task(() =>
                {
                    this.PnPManager().PnpLightJog.InitCameraJog(this, CurCam.GetChannelType());
                });
                task.Start();
                await task;


                // this.StageSupervisor().StageModuleState.WaferHighViewMove(Xpos, Ypos, Wafer.GetPhysInfo().Thickness.Value);

                //CurrentDutIndex = this.DutObject().Duts.Count;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //this.SysState().SetSetUpState();

                AxisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                AxisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                var DutPadInfos = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos;

                LightJogEnable = true;
                ViewTarget = Wafer;

                Inspection.DUTCount = this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutList.Count();
                Inspection.PADCount = this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos.Count();

                if (Inspection.DUTCount == 0)
                {
                    Inspection.ViewDutIndex = 0;
                }
                if (Inspection.PADCount == 0)
                {
                    Inspection.ViewPadIndex = 0;
                }
                else
                {
                    Inspection.ViewPadIndex = 1;
                }

                CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                this.PnPManager().PnpLightJog.InitCameraJog(this, CurCam.GetChannelType());

                for (int lightindex = 0; lightindex < CurCam.LightsChannels.Count; lightindex++)
                {
                    CurCam.SetLight(CurCam.LightsChannels[lightindex].Type.Value, 85);
                }

                //MapIndexX = (int)DisplayPort.AssignedCamera.CamSystemUI.XIndex;
                //MapIndexY = (int)DisplayPort.AssignedCamera.CamSystemUI.YIndex;

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                Inspection.DutStartPoint();

                int xPos = 0;
                int yPos = 0;

                if (this.ProbingModule().ModuleState.State == ModuleStateEnum.PAUSED ||
                    this.ProbingModule().ModuleState.State == ModuleStateEnum.PAUSING)
                {
                    xPos = (int)this.ProbingModule().ProbingLastMIndex.XIndex;
                    yPos = (int)this.ProbingModule().ProbingLastMIndex.YIndex;
                }
                else
                {
                    xPos = (int)this.ManualContactModule().MXYIndex.X;
                    yPos = (int)this.ManualContactModule().MXYIndex.Y;
                }

                if (xPos == 0 && yPos == 0)
                {
                    xPos = (int)Wafer.GetPhysInfo().TeachDieMIndex.Value.XIndex;
                    yPos = (int)Wafer.GetPhysInfo().TeachDieMIndex.Value.YIndex;
                }

                UserXShiftValue = param.UserProbeMarkShift.Value.X.Value;
                UserYShiftValue = param.UserProbeMarkShift.Value.Y.Value;

                SystemXShiftValue = param.ProbeMarkShift.Value.X.Value;
                SystemYShiftValue = param.ProbeMarkShift.Value.Y.Value;

                int dieXlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(0);
                int dieYlength = this.GetParam_Wafer().GetSubsInfo().DIEs.GetLength(1);

                IProbeCard probeCard = this.GetParam_ProbeCard();

                int dutCount = probeCard.ProbeCardDevObjectRef.DutList.Count;

                long dutXIndex = xPos + probeCard.ProbeCardDevObjectRef.DutList[0].UserIndex.XIndex;
                long dutYIndex = yPos + probeCard.ProbeCardDevObjectRef.DutList[0].UserIndex.YIndex;

                Inspection.ManualSetIndexToggle = false;

                WaferCoordinate wcoord = this.WaferAligner().MachineIndexConvertToDieLeftCorner(dutXIndex, dutYIndex);
                WaferCoordinate CenterVal = this.WaferAligner().MachineIndexConvertToDieLeftCorner((dieXlength / 2), (dieYlength / 2));
                WaferCoordinate posCoord = this.WaferAligner().MachineIndexConvertToProbingCoord(dutXIndex, dutYIndex);

                //처음 화면 진입 시 Wafer.GetSubsInfo().ActualThickness 값을 쓰는게 아니라
                //Wafer Align Done 상태라면, GetHeightValue 값으로 Move 한다.
                //Wafer Align Idle 상태라면, ActualThickness 값으로 Move 한다.
                if (CurCam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM)
                {
                    if (dutXIndex < dieXlength)
                    {
                        if (dutYIndex < dieYlength)
                        {
                            if (DutPadInfos.Count == 0)
                            {
                                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                       posCoord.GetX(), posCoord.GetY(),
                                       Wafer.GetSubsInfo().ActualThickness,
                                       this.WaferAligner().WaferAlignInfo.AlignAngle);
                            }
                            else
                            {
                                var curcorner = this.WaferAligner().MachineIndexConvertToDieLeftCorner((long)xPos, (long)yPos);

                                this.StageSupervisor().StageModuleState.WaferHighViewMove(
                                    curcorner.X.Value + DutPadInfos[0].PadCenter.X.Value + (-(param.ProbeMarkShift.Value.X.Value + param.UserProbeMarkShift.Value.X.Value)),
                                    curcorner.Y.Value + DutPadInfos[0].PadCenter.Y.Value + (-(param.ProbeMarkShift.Value.Y.Value + param.UserProbeMarkShift.Value.Y.Value)),
                                    Wafer.GetSubsInfo().ActualThickness,
                                    this.WaferAligner().WaferAlignInfo.AlignAngle);
                            }
                        }
                    }
                }
                else if (CurCam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {
                    if (dutXIndex < dieXlength)
                    {
                        if (dutYIndex < dieYlength)
                        {
                            if (DutPadInfos.Count == 0)
                            {
                                this.StageSupervisor().StageModuleState.WaferLowViewMove(CenterVal.GetX(), CenterVal.GetY(), Wafer.GetSubsInfo().ActualThickness);
                            }
                            else
                            {
                                this.StageSupervisor().StageModuleState.WaferLowViewMove(wcoord.GetX() + (DutPadInfos[0].PadCenter.X.Value) + (-(param.ProbeMarkShift.Value.X.Value + param.UserProbeMarkShift.Value.X.Value)),
                                                                                  wcoord.GetY() + (DutPadInfos[0].PadCenter.Y.Value) + (-(param.ProbeMarkShift.Value.Y.Value + param.UserProbeMarkShift.Value.Y.Value)),
                                                                                  Wafer.GetSubsInfo().ActualThickness);
                                //this.StageSupervisor().StageModuleState.WaferLowViewMove(posCoord.GetX(), posCoord.GetY(), Wafer.GetSubsInfo().ActualThickness);

                            }
                        }
                    }
                }
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);

                Task task = new Task(() =>
                {
                    var waferobj = this.StageSupervisor().WaferObject.GetSubsInfo();
                    retval = this.WaferAligner().CalculateOffsetToAutoFocusedPosition(CurCam, DutPadInfos[0].PadCenter.X.Value, DutPadInfos[0].PadCenter.Y.Value);
                    if (retval == EventCodeEnum.NONE)
                    {
                        //Move Offset을 계산하기 위한 함수
                        this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    }

                    LoggerManager.Debug($"[InspectionControlVM] PageSwitched(), Focusing ResultCode = {retval} Move Z Offset = {waferobj.MoveZOffset:0.00}");
                });
                task.Start();
                await task;

                LightJogEnable = true;
                SetFromToggle = false;
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ProbingModule().SaveSysParameter();

                this.StageSupervisor().StageModuleState.ZCLEARED();
                this.VisionManager().StopGrab(CurCam.GetChannelType());
                StageCylinderType.MoveWaferCam.Retract();
                SetFromToggle = false;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retval);
        }

        #region module


        private object _ViewTarget;
        public object ViewTarget
        {
            get { return _ViewTarget; }
            set
            {
                if (value != _ViewTarget)
                {
                    _ViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    _MiniViewTarget = value;
                    if (_MiniViewTarget == ZoomObject)
                        MiniViewZoomVisible = Visibility.Visible;
                    else
                        MiniViewZoomVisible = Visibility.Hidden;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> MiniViewZoomVisible
        private Visibility _MiniViewZoomVisible;
        public Visibility MiniViewZoomVisible
        {
            get { return _MiniViewZoomVisible; }
            set
            {
                if (value != _MiniViewZoomVisible)
                {
                    _MiniViewZoomVisible = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

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

        public void ViewSwip()
        {
            try
            {
                object swap = MainViewTarget;
                MainViewTarget = MiniViewTarget;
                MiniViewTarget = swap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetFrom()
        {
            try
            {
                ImageBuffer buf;

                UserXShiftValue = param.UserProbeMarkShift.Value.X.Value;
                UserYShiftValue = param.UserProbeMarkShift.Value.Y.Value;

                SystemXShiftValue = param.ProbeMarkShift.Value.X.Value;
                SystemYShiftValue = param.ProbeMarkShift.Value.Y.Value;

                if (SetFromToggle == true)
                {
                    XSetFromCoord = this.AxisX.Status.Position.Ref - UserXShiftValue;
                    YSetFromCoord = this.AxisY.Status.Position.Ref - UserYShiftValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
                    //ConfirmDisplay(_CurCam, value);
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private void ConfirmDisplay(ICamera precam, ICamera curcam)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    //if (precam != null && precam.DrawDisplayDelegate != null)
        //    //{

        //    //    curcam.DrawDisplayDelegate
        //    //     = precam.DrawDisplayDelegate;
        //    //    precam.InDrawOverlayDisplay();

        //    //}

        //}
        #endregion
        #region Light Jog
        protected EventCodeEnum InitLightJog(IUseLightJog module, EnumProberCam camtype = EnumProberCam.UNDEFINED)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                //StageSupervisor = this.StageSupervisor();
                //VisionManager = this.VisionManager();
                //CurCam 이 할당된 뒤에 호출해야함.
                this.StageSupervisor().PnpLightJog.InitCameraJog(module, camtype);//==> Nick : Light Jog를 Update하여 UI 구성을 함, InitSetup마다 호출 해야함. 
                //EnableState = new EnableIdleState(EnableState);
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
        #endregion

        #region ==> PrevDutCommand
        private AsyncCommand _PrevDutCommand;
        public IAsyncCommand PrevDutCommand
        {
            get
            {
                if (null == _PrevDutCommand) _PrevDutCommand = new AsyncCommand(FuncPrevDutCommand);
                return _PrevDutCommand;
            }
        }
        private async Task FuncPrevDutCommand() // 이전 더트로 이동
        {
            try
            {


                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.PreDut(CurCam);
                });
                task.Start();
                await task;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }

        }
        #endregion

        #region ==> NextDutCommand
        private AsyncCommand _NextDutCommand;
        public IAsyncCommand NextDutCommand
        {
            get
            {
                if (null == _NextDutCommand) _NextDutCommand = new AsyncCommand(FuncNextDutCommand);
                return _NextDutCommand;
            }
        }

        private async Task FuncNextDutCommand()  //다음 더트로 이동
        {
            try
            {


                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.NextDut(CurCam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            //this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc);
        }

        #endregion
        #region ==> ManualSetIndexCommand
        //private AsyncCommand _ManualSetIndexCommand;
        //public IAsyncCommand ManualSetIndexCommand
        //{
        //    get
        //    {
        //        if (null == _ManualSetIndexCommand) _ManualSetIndexCommand = new AsyncCommand(FuncManualSetIndexCommand);
        //        return _ManualSetIndexCommand;
        //    }
        //}
        //private async Task FuncManualSetIndexCommand() // Manual Set Index 적용
        //{
        //    try
        //    {
        //        
        //        Task task = new Task(() =>
        //        {
        //            Inspection.ManualSetIndex();

        //            if (ToggleSetIndex == false) // 오토
        //            {
        //                //SetManualXIndex = (int)DisplayPort.AssignedCamera.CamSystemUI.XIndex;
        //                //SetManualYIndex = (int)DisplayPort.AssignedCamera.CamSystemUI.YIndex;
        //                SetManualXIndex = (int)CurCam.GetCurCoordIndex().XIndex;
        //                SetManualYIndex = (int)CurCam.GetCurCoordIndex().YIndex;

        //                Inspection.ManualSetIndexX = SetManualXIndex;
        //                Inspection.ManualSetIndexY = SetManualYIndex;
        //            }
        //            else//메뉴얼
        //            {
        //                SetManualXIndex = (int)MapIndexX;
        //                SetManualYIndex = (int)MapIndexY;

        //                Inspection.ManualSetIndexX = SetManualXIndex;
        //                Inspection.ManualSetIndexY = SetManualYIndex;
        //            }
        //        });
        //        task.Start();
        //        await task;

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //this.ViewModelManager().UnLock(this.GetHashCode());
        //        
        //    }
        //}

        #endregion
        #region ==> PinPadCameraCommand
        private AsyncCommand _PinPadCameraCommand;
        public ICommand PinPadCameraCommand
        {
            get
            {
                if (null == _PinPadCameraCommand) _PinPadCameraCommand = new AsyncCommand(FuncPinPadCameraCommand);
                return _PinPadCameraCommand;
            }
        }
        private async Task FuncPinPadCameraCommand() // 이전 더트로 이동
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (ToggleCamIndex == true)
                    {
                        // wafer
                        CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                    }
                    else
                    {
                        // pin
                        CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                    }
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }


        #endregion

        #region ==> WaferAlignmentCommand
        private AsyncCommand _WaferAlignCommand;
        public IAsyncCommand WaferAlignCommand
        {
            get
            {
                if (null == _WaferAlignCommand) _WaferAlignCommand = new AsyncCommand(WaferAlignCommandFunc);
                return _WaferAlignCommand;
            }
        }
        private async Task WaferAlignCommandFunc()
        {
            try
            {
                double prePosX = 0.0, prePosY = 0.0, prePosZ = 0.0, prePosT = 0.0;
                EnumProberCam preCamType;

                CatCoordinates catCoordinates = CurCam.GetCurCoordPos();

                prePosX = catCoordinates.X.Value;
                prePosY = catCoordinates.Y.Value;
                prePosZ = catCoordinates.Z.Value;
                preCamType = CurCam.GetChannelType();

                var retval = await this.StageSupervisor().DoWaferAlign();

                if (retval == EventCodeEnum.NONE)
                {
                    if (preCamType == EnumProberCam.WAFER_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferLowViewMove(prePosX, prePosY, prePosZ);
                    }
                    else if (preCamType == EnumProberCam.WAFER_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.WaferHighViewMove(prePosX, prePosY, prePosZ);
                    }
                }

                this.VisionManager().StartGrab(preCamType, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }


        #endregion
        #region ==> PinAlignmentCommand
        private AsyncCommand _PinAlignCommand;
        public IAsyncCommand PinAlignCommand
        {
            get
            {
                if (null == _PinAlignCommand) _PinAlignCommand = new AsyncCommand(PinAlignCommandFunc);
                return _PinAlignCommand;
            }
        }
        private async Task PinAlignCommandFunc()
        {
            try
            {
                var ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Are you sure you want to pin alignment?", EnumMessageStyle.AffirmativeAndNegative);

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    double prePosX = 0.0, prePosY = 0.0, prePosZ = 0.0, prePosT = 0.0;
                    EnumProberCam preCamType;

                    CatCoordinates catCoordinates = CurCam.GetCurCoordPos();

                    prePosX = catCoordinates.X.Value;
                    prePosY = catCoordinates.Y.Value;
                    prePosZ = catCoordinates.Z.Value;
                    preCamType = CurCam.GetChannelType();

                    var retval = this.StageSupervisor().DoManualPinAlign();

                    if (retval == EventCodeEnum.NONE)
                    {
                        if (preCamType == EnumProberCam.WAFER_LOW_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(prePosX, prePosY, prePosZ);
                        }
                        else if (preCamType == EnumProberCam.WAFER_HIGH_CAM)
                        {
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(prePosX, prePosY, prePosZ);
                        }
                    }

                    this.VisionManager().StartGrab(preCamType, this);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }


        #endregion
        private AsyncCommand _SavePadsCommand;
        public IAsyncCommand SavePadsCommand
        {
            get
            {
                if (null == _SavePadsCommand) _SavePadsCommand = new AsyncCommand(SavePadsCommandFunc);
                return _SavePadsCommand;
            }
        }
        private async Task SavePadsCommandFunc() // 
        {
            try
            {
                Task task = new Task(() =>
                {
                    Inspection.SavePadImages(CurCam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        //public void SavePadsRemoteCommand() // 
        //{
        //    try
        //    {
        //        Inspection.SavePadImages();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //    }
        //}
        public InspcetionDataDescription GetInscpetionInfo()
        {
            InspcetionDataDescription info = new InspcetionDataDescription();

            try
            {
                info.SetFromToggle = SetFromToggle;

                info.InspectionViewDutIndex = Inspection.ViewDutIndex;
                info.InspectionViewPadIndex = Inspection.ViewPadIndex;

                info.InspcetionDutCount = Inspection.DUTCount;
                info.InspcetionPADCount = Inspection.PADCount;
                info.InspcetoinXDutStartIndexPoint = Inspection.XDutStartIndexPoint;
                info.InspectionYDutStartIndexPoint = Inspection.YDutStartIndexPoint;

                info.InspectionManualSetIndexToggle = Inspection.ManualSetIndexToggle;

                //info.inspectionManualSetIndexX = Inspection.ManualSetIndexX;
                //info.inspectionManualSetIndexY = Inspection.ManualSetIndexY;


                info.MapIndexX = MapIndexX;
                info.MapIndexY = MapIndexY;

                info.XSetFromCoord = XSetFromCoord;
                info.YSetFromCoord = YSetFromCoord;

                info.UserXShiftValue = UserXShiftValue;
                info.UserYShiftValue = UserYShiftValue;
                info.SystemXShiftValue = SystemXShiftValue;
                info.SystemYShiftValue = SystemYShiftValue;

                info.ToggleSetIndex = ToggleSetIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return info;
        }

        public void Inspection_ChangeXManualIndex(long index)
        {
            try
            {
                this.MapIndexX = index;
                //Inspection.ManualSetIndexX = (int)MapIndexX;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Inspection_ChangeYManualIndex(long index)
        {
            try
            {
                this.MapIndexY = index;
                //Inspection.ManualSetIndexY = (int)MapIndexY;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task SetFromRemoteCommand()
        {
            try
            {
                Task task = new Task(() =>
                {
                    SetFromToggle = !SetFromToggle;
                    SetFrom();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        public async Task<EventCodeEnum> CheckPMShiftLimit(double checkvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                Task task = new Task(() =>
                {
                    if (LimitCheck(checkvalue))
                    {
                        ret = EventCodeEnum.NONE;
                    }
                    else 
                    {
                        double OffSetUpperLimit = devparam.ProbingXYOffSetUpperLimit.Value;
                        double OffSetLowLimit = devparam.ProbingXYOffSetLowLimit.Value;
                        //Limit Error
                        this.MetroDialogManager().ShowMessageDialog("Limit Error",
                            $"Set ProbeMarkShift Value : {checkvalue:0.00} \nProbeMarkShift UpperLimit : {OffSetUpperLimit:0.00} LowerLimit : {OffSetLowLimit:0.00}", EnumMessageStyle.Affirmative);

                        ret = EventCodeEnum.PARAM_ERROR;
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }

            return ret;
        }
        public async Task SaveRemoteCommand(InspcetionDataDescription info)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                Task task = new Task(() =>
                {
                    var sysparam = this.ProbingModule().ProbingModuleSysParam_IParam as ProbingModuleSysParam;


                    sysparam.UserProbeMarkShift.Value.X.Value = info.UserXShiftValue;
                    sysparam.UserProbeMarkShift.Value.Y.Value = info.UserYShiftValue;
                    UserXShiftValue = sysparam.UserProbeMarkShift.Value.X.Value;
                    UserYShiftValue = sysparam.UserProbeMarkShift.Value.Y.Value;
                    LoggerManager.Debug($"InspectionControlVM : SaveRemoteCommand() UserXShift : {UserXShiftValue:0.00} UserYShift : {UserYShiftValue:0.00}");

                    sysparam.ProbeMarkShift.Value.X.Value = info.SystemXShiftValue;
                    sysparam.ProbeMarkShift.Value.Y.Value = info.SystemYShiftValue;
                    SystemXShiftValue = sysparam.ProbeMarkShift.Value.X.Value;
                    SystemYShiftValue = sysparam.ProbeMarkShift.Value.Y.Value;

                    LoggerManager.Debug($"InspectionControlVM : SaveRemoteCommand() SystemXShift : {SystemXShiftValue:0.00} SystemYShift : {SystemYShiftValue:0.00}");

                    retval = this.ProbingModule().SaveSysParameter();

                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        //Save
        public async Task ApplyRemoteCommand()
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;
                Task task = new Task(() =>
                {
                    if (SetFromToggle == true)
                    {
                        double axisX = this.AxisX.Status.Position.Ref;
                        double axisY = this.AxisY.Status.Position.Ref;

                        double xShift = Math.Round(axisX - XSetFromCoord, 1);
                        double yShift = Math.Round(axisY - YSetFromCoord, 1);

                        if (LimitCheck(xShift) && LimitCheck(yShift))
                        {
                            LoggerManager.Debug($"InspectionControlVM : ApplyRemoteCommand() UserXShift : {UserXShiftValue:0.00} UserYShift : {UserYShiftValue:0.00}");

                            UserXShiftValue = xShift;
                            UserYShiftValue = yShift;

                            param.UserProbeMarkShift.Value.X.Value = UserXShiftValue;
                            param.UserProbeMarkShift.Value.Y.Value = UserYShiftValue;
                        }
                        else
                        {
                            this.MetroDialogManager().ShowMessageDialog("Limit Error",
                               $"Set ProbeMarkShift (X,Y) = ({xShift}, {yShift}) \nProbe Mark Shift X UpperLimit : {param.ProbeMarkShift.Value.X.UpperLimit:0.00}, X LowerLimit : {param.ProbeMarkShift.Value.X.LowerLimit:0.00} \n" +
                               $"Probe Mark Shift Y UpperLimit : {param.ProbeMarkShift.Value.Y.UpperLimit:0.00}, Y LowerLimit : {param.ProbeMarkShift.Value.Y.LowerLimit:0.00}", EnumMessageStyle.Affirmative);

                            LoggerManager.Debug($"InspectionControlVM : ApplyRemoteCommand() Limit Error");
                        }

                        SetFromToggle = false;

                        retval = this.ProbingModule().SaveSysParameter();
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

        public async Task SystemApplyRemoteCommand()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                Task task = new Task(() =>
                {
                    if (LimitCheck(SystemXShiftValue + UserXShiftValue) && LimitCheck(SystemYShiftValue + UserYShiftValue))
                    {
                        SystemXShiftValue = SystemXShiftValue + UserXShiftValue;
                        SystemYShiftValue = SystemYShiftValue + UserYShiftValue;

                        Clear();

                        LoggerManager.Debug($"InspectionControlVM : SystemApplyRemoteCommand() SystemXShift : {SystemXShiftValue:0.00} SystemYShift : {SystemYShiftValue:0.00}");
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog("Limit Error",
                               $"Set ProbeMarkShift (X,Y) = ({SystemXShiftValue + UserXShiftValue}, {SystemYShiftValue + UserYShiftValue}) \nProbe Mark Shift X UpperLimit : {param.ProbeMarkShift.Value.X.UpperLimit:0.00}, X LowerLimit : {param.ProbeMarkShift.Value.X.LowerLimit:0.00} \n" +
                               $"Probe Mark Shift Y UpperLimit : {param.ProbeMarkShift.Value.Y.UpperLimit:0.00}, Y LowerLimit : {param.ProbeMarkShift.Value.Y.LowerLimit:0.00}", EnumMessageStyle.Affirmative);

                        LoggerManager.Debug($"InspectionControlVM : SystemApplyRemoteCommand() Limit Error : {SystemXShiftValue + UserXShiftValue:0.00} SystemYShift : {SystemYShiftValue + UserYShiftValue:0.00}");

                        SystemXShiftValue = SystemXShiftValue;
                        SystemYShiftValue = SystemYShiftValue;
                    }

                    param.ProbeMarkShift.Value.X.Value = SystemXShiftValue;
                    param.ProbeMarkShift.Value.Y.Value = SystemYShiftValue;

                    retval = this.ProbingModule().SaveSysParameter();

                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }
        public async Task ClearRemoteCommand()
        {
            try
            {

                Task task = new Task(() =>
                {
                    Clear();
                    SetFromToggle = false;
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());


            }
        }

        public async Task SystemClearRemoteCommand()
        {
            try
            {
                Task task = new Task(() =>
                {
                    SystemClear();
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        public async Task PrevDutRemoteCommand()
        {
            try
            {


                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.PreDut(CurCam);
                });
                task.Start();
                await task;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }

        public async Task NextDutRemoteCommand()
        {
            try
            {


                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.NextDut(CurCam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
            //this.StageSupervisor().StageModuleState.WaferHighViewIndexMove(mcoord.XIndex + xInc, mcoord.YIndex + yInc);        }
        }
        public async Task PadPrevRemoteCommand()
        {
            try
            {

                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.PrePad(CurCam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        public async Task PadNextRemoteCommand()
        {
            try
            {

                Task task = new Task(() =>
                {
                    this.CoordinateManager().CalculateOffsetFromCurrentZ(CurCam.GetChannelType());
                    Inspection.NextPad(CurCam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {

            }
        }

        //public async Task ManualSetIndexRemoteCommand()
        //{
        //    try
        //    {
        //        
        //        Task task = new Task(() =>
        //        {
        //            Inspection.ManualSetIndex();
        //            if (ToggleSetIndex == false) // 오토
        //            {
        //                //SetManualXIndex = (int)DisplayPort.AssignedCamera.CamSystemUI.XIndex;
        //                //SetManualYIndex = (int)DisplayPort.AssignedCamera.CamSystemUI.YIndex;
        //                SetManualXIndex = (int)CurCam.GetCurCoordIndex().XIndex;
        //                SetManualYIndex = (int)CurCam.GetCurCoordIndex().YIndex;
        //                Inspection.ManualSetIndexX = SetManualXIndex;
        //                Inspection.ManualSetIndexY = SetManualYIndex;
        //            }
        //            else//메뉴얼
        //            {
        //                SetManualXIndex = (int)MapIndexX;
        //                SetManualYIndex = (int)MapIndexY;
        //                Inspection.ManualSetIndexX = SetManualXIndex;
        //                Inspection.ManualSetIndexY = SetManualYIndex;
        //            }
        //        });
        //        task.Start();
        //        await task;

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    finally
        //    {
        //        //this.ViewModelManager().UnLock(this.GetHashCode());
        //        
        //    }
        //}

        public async Task PinAlignRemoteCommand()
        {
            //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please Wait");
            try
            {
                await PinAlignCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.ViewModelManager().UnLock(this.GetHashCode());

                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        public async Task WaferAlignRemoteCommand()
        {
            await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Please Wait");

            try
            {
                await WaferAlignCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                //this.ViewModelManager().UnLock(this.GetHashCode());

            }
        }

        public async Task SavePadsRemoteCommand()
        {
            try
            {
                Task task = new Task(() =>
                {
                    Inspection.SavePadImages(CurCam);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }
        }

        public async Task SaveTempOffsetRemoteCommand(ObservableDictionary<double, CatCoordinates> table)
        {
            try
            {
                if (table.Count > param.ProbeTemperaturePositionTable.Value.Count)
                {
                    var resultDic = table
                            .Where(kvp => !param.ProbeTemperaturePositionTable.Value.ContainsKey(kvp.Key))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    foreach (var item in resultDic)
                    {
                        LoggerManager.Debug($"SaveTempOffsetRemoteCommand() Add Temp data : {item.Key} - (X: {item.Value.X}, Y: {item.Value.X}, T:{item.Value.T})");
                    }

                }
                else if (table.Count < param.ProbeTemperaturePositionTable.Value.Count)
                {
                    var resultDic = param.ProbeTemperaturePositionTable.Value
                            .Where(kvp => !table.ContainsKey(kvp.Key))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    foreach (var item in resultDic)
                    {
                        LoggerManager.Debug($"SaveTempOffsetRemoteCommand() Delete Temp data : {item.Key} - (X: {item.Value.X}, Y: {item.Value.X}, T:{item.Value.T})");
                    }
                }
                else
                {
                    foreach (var item in table)
                    {
                        bool result = param.ProbeTemperaturePositionTable.Value.TryGetValue(item.Key, out CatCoordinates value);
                        if (result)
                        {
                            if (item.Value.X.Value != value.X.Value || item.Value.Y.Value != value.Y.Value || item.Value.T.Value != value.T.Value)
                            {
                                LoggerManager.Debug($"SaveTempOffsetRemoteCommand() Change Temp data(old) : {item.Key} - (X: {value.X}, Y: {value.Y}, T:{value.T})" +
                                                                                                  $"(new) : {item.Key} - (X: {item.Value.X}, Y: {item.Value.Y}, T:{item.Value.T})");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"Different key..");
                        }
                    }
                }
                param.ProbeTemperaturePositionTable.Value.Clear();

                foreach (var item in table)
                {
                    param.ProbeTemperaturePositionTable.Value.Add(item.Key, item.Value);
                }
                this.ProbingModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool LimitCheck(double coord)
        {
            bool check = false;
            try
            {
                double offSetUpperLimit = devparam.ProbingXYOffSetUpperLimit.Value;
                double offSetLowLimit = devparam.ProbingXYOffSetLowLimit.Value;

                
                if (offSetLowLimit == 0)
                {
                    offSetLowLimit = -1000;
                }

                if (offSetUpperLimit == 0)
                {
                    offSetUpperLimit = 1000;
                }

                if (coord >= offSetLowLimit && coord <= offSetUpperLimit)
                {
                    check = true;
                }
                else
                {
                    check = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return check;
        }
    }
}
