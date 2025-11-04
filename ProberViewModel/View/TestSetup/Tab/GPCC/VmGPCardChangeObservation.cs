using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestSetupDialog.Tab.GPCC
{
    using CylType;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Param;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class VmGPCardChangeObservation : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public IStageSupervisor StageSupervisor => this.StageSupervisor();
        private IGPCCObservationTempParam GPCCObservationTempParam => this.CardChangeModule().GPCCObservationTempParams_IParam as IGPCCObservationTempParam;

        //==> Binding Property

        #region ==> AssignedCamera
        private ICamera _AssignedCamera;
        public ICamera AssignedCamera
        {
            get { return _AssignedCamera; }
            set
            {
                if (value != _AssignedCamera)
                {
                    _AssignedCamera = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedLightType
        private EnumLightType _SelectedLightType;
        public EnumLightType SelectedLightType
        {
            get { return _SelectedLightType; }
            set
            {
                if (value != _SelectedLightType)
                {
                    _SelectedLightType = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LightValue
        private ushort _LightValue;
        public ushort LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MarkPositionList
        private ObservableCollection<MarkPosition> _MarkPositionList;
        public ObservableCollection<MarkPosition> MarkPositionList
        {
            get { return _MarkPositionList; }
            set
            {
                if (value != _MarkPositionList)
                {
                    _MarkPositionList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedMarkPosition
        private MarkPosition _SelectedMarkPosition;
        public MarkPosition SelectedMarkPosition
        {
            get { return _SelectedMarkPosition; }
            set
            {
                if (value != _SelectedMarkPosition)
                {
                    _SelectedMarkPosition = value;
                    RaisePropertyChanged();

                    if (_SelectedMarkPosition == null)
                    {
                        return;
                    }

                    foreach (MarkPosition item in MarkPositionList)
                    {
                        if (item == _SelectedMarkPosition)
                        {
                            continue;
                        }
                        item.ButtonEnable = false;
                    }
                    _SelectedMarkPosition.ButtonEnable = true;
                    _SelectedMarkPosition.MoveToMark();
                }
            }
        }
        #endregion

        #region ==> PatternWidth
        private double _PatternWidth;
        public double PatternWidth
        {
            get { return _PatternWidth; }
            set
            {
                if (value != _PatternWidth)
                {
                    _PatternWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PatternHeight
        private double _PatternHeight;
        public double PatternHeight
        {
            get { return _PatternHeight; }
            set
            {
                if (value != _PatternHeight)
                {
                    _PatternHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZTickValue
        private int _ZTickValue;
        public int ZTickValue
        {
            get { return _ZTickValue; }
            set
            {
                if (value != _ZTickValue)
                {
                    _ZTickValue = value;

                    ZDistanceValue = Math.Pow(10, _ZTickValue);
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZDistanceValue
        private double _ZDistanceValue;
        public double ZDistanceValue
        {
            get { return _ZDistanceValue; }
            set
            {
                if (value != _ZDistanceValue)
                {
                    _ZDistanceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LightTickValue
        private int _LightTickValue;
        public int LightTickValue
        {
            get { return _LightTickValue; }
            set
            {
                if (value != _LightTickValue)
                {
                    _LightTickValue = value;

                    if (_LightTickValue == 0)
                    {
                        SelectedLightType = EnumLightType.COAXIAL;
                    }
                    else
                    {
                        SelectedLightType = EnumLightType.OBLIQUE;
                    }

                    ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                    LightValue = (ushort)cam.GetLight(SelectedLightType);

                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SetCardChangeStateCommand
        private RelayCommand _SetCardChangeStateCommand;
        public ICommand SetCardChangeStateCommand
        {
            get
            {
                if (null == _SetCardChangeStateCommand) _SetCardChangeStateCommand = new RelayCommand(SetCardChangeStateCommandFunc);
                return _SetCardChangeStateCommand;
            }
        }
        private void SetCardChangeStateCommandFunc()
        {
            this.StageSupervisor().StageModuleState.LockCCState();
        }
        #endregion

        #region ==> UnSetCardChangeStateCommand
        private RelayCommand _UnSetCardChangeStateCommand;
        public ICommand UnSetCardChangeStateCommand
        {
            get
            {
                if (null == _UnSetCardChangeStateCommand) _UnSetCardChangeStateCommand = new RelayCommand(UnSetCardChangeStateCommandFunc);
                return _UnSetCardChangeStateCommand;
            }
        }
        private void UnSetCardChangeStateCommandFunc()
        {
            this.StageSupervisor().StageModuleState.UnLockCCState();
        }
        #endregion

        //==> Command

        #region ==> ObservationCommand
        private AsyncCommand _ObservationCommand;
        public ICommand ObservationCommand
        {
            get
            {
                if (null == _ObservationCommand) _ObservationCommand = new AsyncCommand(ObservationCommandFunc);
                return _ObservationCommand;
            }
        }
        public async Task ObservationCommandFunc()
        {
            this.GPCardAligner().ObservationCard();
        }
        #endregion

        #region ==> PatternWidthPlusCommand
        private RelayCommand _PatternWidthPlusCommand;
        public ICommand PatternWidthPlusCommand
        {
            get
            {
                if (null == _PatternWidthPlusCommand) _PatternWidthPlusCommand = new RelayCommand(PatternWidthPlusCommandFunc);
                return _PatternWidthPlusCommand;
            }
        }
        public void PatternWidthPlusCommandFunc()
        {
            if (PatternWidth + 10 > 890)
            {
                return;
            }

            PatternWidth += 10;

            SavePatternWidthHeightParam();
        }
        #endregion

        #region ==> PatternWidthMinusCommand
        private RelayCommand _PatternWidthMinusCommand;
        public ICommand PatternWidthMinusCommand
        {
            get
            {
                if (null == _PatternWidthMinusCommand) _PatternWidthMinusCommand = new RelayCommand(PatternWidthMinusCommandFunc);
                return _PatternWidthMinusCommand;
            }
        }
        public void PatternWidthMinusCommandFunc()
        {
            if (PatternWidth - 10 < 10)
            {
                return;
            }

            PatternWidth -= 10;

            SavePatternWidthHeightParam();
        }
        #endregion

        #region ==> PatternHeightPlusCommand
        private RelayCommand _PatternHeightPlusCommand;
        public ICommand PatternHeightPlusCommand
        {
            get
            {
                if (null == _PatternHeightPlusCommand) _PatternHeightPlusCommand = new RelayCommand(PatternHeightPlusCommandFunc);
                return _PatternHeightPlusCommand;
            }
        }
        public void PatternHeightPlusCommandFunc()
        {
            if (PatternHeight + 10 > 890)
            {
                return;
            }

            PatternHeight += 10;

            SavePatternWidthHeightParam();
        }
        #endregion

        #region ==> PatternHeightMinusCommand
        private RelayCommand _PatternHeightMinusCommand;
        public ICommand PatternHeightMinusCommand
        {
            get
            {
                if (null == _PatternHeightMinusCommand) _PatternHeightMinusCommand = new RelayCommand(PatternHeightMinusCommandFunc);
                return _PatternHeightMinusCommand;
            }
        }
        public void PatternHeightMinusCommandFunc()
        {
            if (PatternHeight - 10 < 10)
            {
                return;
            }

            PatternHeight -= 10;

            SavePatternWidthHeightParam();
        }
        #endregion

        #region ==> WaferCamExtendCommand
        private RelayCommand _WaferCamExtendCommand;
        public ICommand WaferCamExtendCommand
        {
            get
            {
                if (null == _WaferCamExtendCommand) _WaferCamExtendCommand = new RelayCommand(WaferCamExtendCommandFunc);
                return _WaferCamExtendCommand;
            }
        }
        private void WaferCamExtendCommandFunc()
        {
            double axisZsafeOffset = 15000; //마크를 봤을때 핀하이, 웨이퍼하이의 거리는 35.5mm이다 마크 보는 포지션에서 척은 pz보다 20mm높다. 
            double axisPZsafeOffset = 35000;
            var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
            var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
            var markRefposZ = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;
            if (axisz.Status.RawPosition.Actual > markRefposZ + axisZsafeOffset)
            {
                this.StageSupervisor().StageModuleState.CCZCLEARED();
            }
            if (axispz.Status.RawPosition.Actual > markRefposZ + axisPZsafeOffset)
            {
                this.StageSupervisor().StageModuleState.CCZCLEARED();
            }
            StageCylinderType.MoveWaferCam.Extend();
        }
        #endregion

        #region ==> WaferCamFoldCommand
        private RelayCommand _WaferCamFoldCommand;
        public ICommand WaferCamFoldCommand
        {
            get
            {
                if (null == _WaferCamFoldCommand) _WaferCamFoldCommand = new RelayCommand(WaferCamFoldCommandFunc);
                return _WaferCamFoldCommand;
            }
        }
        private void WaferCamFoldCommandFunc()
        {
            StageCylinderType.MoveWaferCam.Retract();
        }
        #endregion

        #region ==> MoveToCenterCommand
        private RelayCommand _MoveToCenterCommand;
        public ICommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new RelayCommand(MoveToCenterCommandFunc);
                return _MoveToCenterCommand;
            }
        }
        private void MoveToCenterCommandFunc()
        {
            EventCodeEnum result = EventCodeEnum.NONE;

            result = this.StageSupervisor().StageModuleState.ZCLEARED();
            if (result != EventCodeEnum.NONE)
            {
                return;
            }

            ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

            StageCylinderType.MoveWaferCam.Retract();

            cam.SetLight(EnumLightType.COAXIAL, GPCCObservationTempParam.ObservationCOAXIAL);
            cam.SetLight(EnumLightType.OBLIQUE, GPCCObservationTempParam.ObservationOBLIQUE);

            PinCoordinate pinPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();

            result = this.StageSupervisor().StageModuleState.PogoViewMove(0, 0, pinPos.Z.Value);
            if (result != EventCodeEnum.NONE)
            {
                return;
            }
        }
        #endregion

        #region ==> RaiseZCommand
        private RelayCommand _RaiseZCommand;
        public ICommand RaiseZCommand
        {
            get
            {
                if (null == _RaiseZCommand) _RaiseZCommand = new RelayCommand(RaiseZCommandFunc);
                return _RaiseZCommand;
            }
        }
        public void RaiseZCommandFunc()
        {
            this.GPCardAligner().RelMoveZ(EnumProberCam.PIN_LOW_CAM, ZDistanceValue);
        }
        #endregion

        #region ==> DropZCommand
        private RelayCommand _DropZCommand;
        public ICommand DropZCommand
        {
            get
            {
                if (null == _DropZCommand) _DropZCommand = new RelayCommand(DropZCommandFunc);
                return _DropZCommand;
            }
        }
        public void DropZCommandFunc()
        {
            this.GPCardAligner().RelMoveZ(EnumProberCam.PIN_LOW_CAM, ZDistanceValue * -1);
        }
        #endregion

        #region ==> IncreaseLightIntensityCommand
        private RelayCommand _IncreaseLightIntensityCommand;
        public ICommand IncreaseLightIntensityCommand
        {
            get
            {
                if (null == _IncreaseLightIntensityCommand) _IncreaseLightIntensityCommand = new RelayCommand(IncreaseLightIntensityCommandFunc);
                return _IncreaseLightIntensityCommand;
            }
        }
        public void IncreaseLightIntensityCommandFunc()
        {
            ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

            int lightIntensity = cam.GetLight(SelectedLightType);
            if (lightIntensity < 0)
            {
                return;
            }

            lightIntensity += 1;
            if (lightIntensity > 255)
            {
                return;
            }

            cam.SetLight(SelectedLightType, (ushort)lightIntensity);
            LightValue = (ushort)cam.GetLight(SelectedLightType);

            SaveLightParam();
        }
        #endregion

        #region ==> DecreaseLightIntensityCommand
        private RelayCommand _DecreaseLightIntensityCommand;
        public ICommand DecreaseLightIntensityCommand
        {
            get
            {
                if (null == _DecreaseLightIntensityCommand) _DecreaseLightIntensityCommand = new RelayCommand(DecreaseLightIntensityCommandFunc);
                return _DecreaseLightIntensityCommand;
            }
        }
        public void DecreaseLightIntensityCommandFunc()
        {
            ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

            int lightIntensity = cam.GetLight(SelectedLightType);
            if (lightIntensity < 0)
            {
                return;
            }

            lightIntensity -= 1;
            if (lightIntensity < 0)
            {
                return;
            }

            cam.SetLight(SelectedLightType, (ushort)lightIntensity);
            LightValue = (ushort)cam.GetLight(SelectedLightType);

            SaveLightParam();

        }
        #endregion

        const int SAVE_DELAY = 3;
        int patterSizeSaveWait = 0;
        private void SavePatternWidthHeightParam()
        {
            if (patterSizeSaveWait > 0)
            {
                patterSizeSaveWait = SAVE_DELAY;
                return;
            }

            Task.Run(() =>
            {
                patterSizeSaveWait = SAVE_DELAY;
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);

                    patterSizeSaveWait--;

                    if (patterSizeSaveWait < 1)
                    {
                        break;
                    }
                }

                GPCCObservationTempParam.ObservationPatternWidth = PatternWidth;
                GPCCObservationTempParam.ObservationPatternHeight = PatternHeight;

                LoggerManager.Debug("Save Pattern Size");
                EventCodeEnum saveResult = this.CardChangeModule().SaveGPCCObservationTempParam();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pattern Parameter Fail");
                }
            });
        }

        int lightSaveWait = 0;
        private void SaveLightParam()
        {
            if (lightSaveWait > 0)
            {
                lightSaveWait = SAVE_DELAY;
                return;
            }

            Task.Run(() =>
            {
                lightSaveWait = SAVE_DELAY;
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);

                    lightSaveWait--;

                    if (lightSaveWait < 1)
                    {
                        break;
                    }
                }

                ICamera plCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

                GPCCObservationTempParam.ObservationCOAXIAL = (ushort)plCam.GetLight(EnumLightType.COAXIAL);
                GPCCObservationTempParam.ObservationOBLIQUE = (ushort)plCam.GetLight(EnumLightType.OBLIQUE);

                LoggerManager.Debug("Save Light");
                EventCodeEnum saveResult = this.CardChangeModule().SaveGPCCObservationTempParam();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pattern Parameter Fail");
                }
            });
        }

        private void SetAlignSettingMode(List<CatCoordinates> markCoordinates, double patternWidth, double patternHeight, ushort coaxialLightValue, ushort obliqueLightValue)
        {
            EventCodeEnum result = EventCodeEnum.UNDEFINED;

            if (this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM) != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"{EnumProberCam.PIN_LOW_CAM} : {EnumProberCam.PIN_LOW_CAM}, Start grab Error");
            }

            MarkPositionList = new ObservableCollection<MarkPosition>();
            for (int i = 0; i < markCoordinates.Count; ++i)
            {
                CatCoordinates markPosition = markCoordinates[i];
                MarkPositionList.Add(new MarkPosition(markPosition, i));
            }

            PatternWidth = patternWidth;
            PatternHeight = patternHeight;

            if (SelectedLightType == EnumLightType.COAXIAL)
            {
                LightValue = coaxialLightValue;
            }
            else if (SelectedLightType == EnumLightType.OBLIQUE)
            {
                LightValue = obliqueLightValue;
            }

            //==> Light Value Update
            ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
            cam.SetLight(SelectedLightType, LightValue);
            LightValue = (ushort)cam.GetLight(SelectedLightType);

            //==> Change camera state
            AssignedCamera = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
            
            this.VisionManager().StartGrab(EnumProberCam.PIN_LOW_CAM, this);
        }

        public VmGPCardChangeObservation()
        {
            //==> View Model            

            ZTickValue = 1;//==> 10^1, ZValue
            LightTickValue = 1;//==> OBLIQUE, Select Light Type, Light Value

            SetAlignSettingMode(
                new List<CatCoordinates>(GPCCObservationTempParam.RegisteredMarkPosList),
                GPCCObservationTempParam.ObservationPatternWidth,
                GPCCObservationTempParam.ObservationPatternHeight,
                GPCCObservationTempParam.ObservationCOAXIAL,
                GPCCObservationTempParam.ObservationOBLIQUE);
        }
    }

    public class MarkPosition : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> Description
        private String _Description;
        public String Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ButtonEnable
        private bool _ButtonEnable;
        public bool ButtonEnable
        {
            get { return _ButtonEnable; }
            set
            {
                if (value != _ButtonEnable)
                {
                    _ButtonEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegisterPatternCommand
        private RelayCommand _RegisterPatternCommand;
        public ICommand RegisterPatternCommand
        {
            get
            {
                if (null == _RegisterPatternCommand) _RegisterPatternCommand = new RelayCommand(RegisterPatternCommandFunc);
                return _RegisterPatternCommand;
            }
        }
        private void RegisterPatternCommandFunc()
        {
            this.GPCardAligner().ObservationRegisterPattern(Index);
            SaveCurrentPos();
        }
        #endregion

        public void MoveToMark()
        {
            double posX = _Position.X.Value;
            double posY = _Position.Y.Value;
            double posZ = _Position.Z.Value;

            ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);

            //==> 안전하게 먼저 Wafer Camera 접는다.
            StageCylinderType.MoveWaferCam.Retract();

            cam.SetLight(EnumLightType.COAXIAL, GPCCObservationTempParam.ObservationCOAXIAL);
            cam.SetLight(EnumLightType.OBLIQUE, GPCCObservationTempParam.ObservationOBLIQUE);

            //==> TODO : 좌표계 변환으로 인한 조치
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            //{
            //    result = this.StageSupervisor().StageModuleState.ZCLEARED_SWNegLimit();
            //    if (result != EventCodeEnum.NONE)
            //    {
            //        return;
            //    }
            //    var currentPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
            //    posZ = currentPos.Z.Value;
            //}

            result = this.StageSupervisor().StageModuleState.PogoViewMove(posX, posY, posZ);
            if (result != EventCodeEnum.NONE)
            {
                return;
            }
        }
        private void SaveCurrentPos()
        {
            CatCoordinates currentPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
            if (currentPos == null)
            {
                return;
            }

            CatCoordinates selectedMarkPos = GPCCObservationTempParam.RegisteredMarkPosList[Index];
            if (selectedMarkPos == null)
            {
                return;
            }

            selectedMarkPos.X.Value = currentPos.X.Value;
            selectedMarkPos.Y.Value = currentPos.Y.Value;
            selectedMarkPos.Z.Value = currentPos.Z.Value;
            EventCodeEnum saveResult = this.CardChangeModule().SaveGPCCObservationTempParam();
            if (saveResult != EventCodeEnum.NONE)
            {
                this.MetroDialogManager().ShowMessageDialog("Error", "Save Z Position Fail", EnumMessageStyle.Affirmative);
            }
        }

        private IGPCCObservationTempParam GPCCObservationTempParam => this.CardChangeModule().GPCCObservationTempParams_IParam as IGPCCObservationTempParam;
        private CatCoordinates _Position;
        public int Index { get; set; }
        public MarkPosition(CatCoordinates position, int index)
        {
            _Position = position;
            Index = index;

            Description = $"X: {_Position.X.Value}, Y: {_Position.Y.Value}";
            ButtonEnable = false;
        }
    }
}
