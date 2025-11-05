using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPCognexOCRMainPageViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Autofac;
    using Cognex.Command.CognexCommandPack.GetInformation;
    using Cognex.Controls;
    using LoaderBase;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LoaderParameters;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Loader;
    using RelayCommandBase;
    using VirtualKeyboardControl;

    public class VmGPCognexOCRMainPage : IFactoryModule, ILoaderSubModule, IMainScreenViewModel, INotifyPropertyChanged
    {
        private readonly int AbAngle = 360;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ImageSource
        private System.Windows.Media.Imaging.BitmapImage _ImageSource;
        public System.Windows.Media.Imaging.BitmapImage ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (value != _ImageSource)
                {
                    _ImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        private bool _IsWafer = false;
        public bool IsWafer
        {
            get { return _IsWafer; }
            set
            {
                if (value != _IsWafer)
                {

                    _IsWafer = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _BtnEnable = true;
        public bool BtnEnable
        {
            get { return _BtnEnable; }
            set
            {
                if (value != _BtnEnable)
                {

                    _BtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _TickValue = 0;
        public double TickValue
        {
            get { return _TickValue; }
            set
            {
                if (value != _TickValue)
                {
                    _TickValue = value;
                    if (_TickValue == 1)
                    {
                        AngleValue = 1;
                    }
                    else if (_TickValue == 2)
                    {
                        AngleValue = 5;
                    }
                    else if (_TickValue == 3)
                    {
                        AngleValue = 10;
                    }
                    else if (_TickValue == 4)
                    {
                        AngleValue = 100;
                    }
                    else if (_TickValue == 5)
                    {
                        AngleValue = 180;
                    }
                    else if (_TickValue == 6)
                    {
                        AngleValue = 270;
                    }
                    RaisePropertyChanged();
                }
            }
        }
        private double _AngleValue = 0;
        public double AngleValue
        {
            get { return _AngleValue; }
            set
            {
                if (value != _AngleValue)
                {
                    _AngleValue = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region ====> JOG GROUP

        #region ==> FlatJogUpCommand
        private AsyncCommand _FlatJogUpCommand;
        public ICommand FlatJogUpCommand
        {
            get
            {
                if (null == _FlatJogUpCommand) _FlatJogUpCommand = new AsyncCommand(FlatJogUpCommandFunc);
                return _FlatJogUpCommand;
            }
        }
        private async Task FlatJogUpCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}

            double value = 50 * -1;

            var accparam = GetOCRAccessParam();
            if (accparam == null)
            {
                return;
            }

            accparam.Position.U.Value += value;//=> FOR TEST

            //double x = 0;
            //double y = 0;
            //double t = 275;
            //accparam.Position.U.Value = x;
            //accparam.Position.W.Value = y;
            //accparam.VPos.Value = t;

            JogMove(accparam);

            _LoaderModule.SaveSystemParam(_LoaderModule.SystemParameter);

            UpdateImage();
        }
        #endregion

        #region ==> FlatJogDownCommand
        private AsyncCommand _FlatJogDownCommand;
        public ICommand FlatJogDownCommand
        {
            get
            {
                if (null == _FlatJogDownCommand) _FlatJogDownCommand = new AsyncCommand(FlatJogDownCommandFunc);
                return _FlatJogDownCommand;
            }
        }
        private async Task FlatJogDownCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}

            double value = 50;

            var accparam = GetOCRAccessParam();
            if (accparam == null)
            {
                return;
            }

            accparam.Position.U.Value += value;

            JogMove(accparam);

            _LoaderModule.SaveSystemParam(_LoaderModule.SystemParameter);

            UpdateImage();
        }
        #endregion

        #region ==> FlatJogLeftCommand
        private AsyncCommand _FlatJogLeftCommand;
        public ICommand FlatJogLeftCommand
        {
            get
            {
                if (null == _FlatJogLeftCommand) _FlatJogLeftCommand = new AsyncCommand(FlatJogLeftCommandFunc);
                return _FlatJogLeftCommand;
            }
        }
        private async Task FlatJogLeftCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}

            double value = 50;

            var accparam = GetOCRAccessParam();
            if (accparam == null)
            {
                return;
            }

            accparam.Position.W.Value += value;

            JogMove(accparam);

            _LoaderModule.SaveSystemParam(_LoaderModule.SystemParameter);

            UpdateImage();
        }
        #endregion

        #region ==> FlatJogRightCommand
        private AsyncCommand _FlatJogRightCommand;
        public ICommand FlatJogRightCommand
        {
            get
            {
                if (null == _FlatJogRightCommand) _FlatJogRightCommand = new AsyncCommand(FlatJogRightCommandFunc);
                return _FlatJogRightCommand;
            }
        }
        private async Task FlatJogRightCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}

            double value = 50 * -1;

            var accparam = GetOCRAccessParam();
            if (accparam == null)
            {
                return;
            }

            accparam.Position.W.Value += value;

            JogMove(accparam);

            _LoaderModule.SaveSystemParam(_LoaderModule.SystemParameter);

            UpdateImage();
        }
        #endregion


        #region ==> OCRPosition
        private OCRAccessParam _OCRPosition;
        public OCRAccessParam OCRPosition
        {
            get { return _OCRPosition; }
            set
            {
                if (value != _OCRPosition)
                {
                    _OCRPosition = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region ==> CurAngleMoveCommand
        private AsyncCommand _CurAngleMoveCommand;
        public ICommand CurAngleMoveCommand
        {
            get
            {
                if (null == _CurAngleMoveCommand) _CurAngleMoveCommand = new AsyncCommand(CurAngleMoveCommandFunc);
                return _CurAngleMoveCommand;
            }
        }
        private async Task CurAngleMoveCommandFunc()
        {

            if (OCRDevParam == null)
            {
                return;
            }
            AngleMove(OCRDevParam.OCRAngle);

            _LoaderModule.SaveSystemParam(_LoaderModule.SystemParameter);

            UpdateImage();
        }
        #endregion
        #region ==> FlatJogCRCommand
        private AsyncCommand _FlatJogCRCommand;
        public ICommand FlatJogCRCommand
        {
            get
            {
                if (null == _FlatJogCRCommand) _FlatJogCRCommand = new AsyncCommand(FlatJogCRCommandFunc);
                return _FlatJogCRCommand;
            }
        }
        private async Task FlatJogCRCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}

            double value = RelAngle * -1;

            if (OCRDevParam != null)
            {

                OCRDevParam.OCRAngle += value;

                if (OCRDevParam.OCRAngle < 0)
                {
                    OCRDevParam.OCRAngle = AbAngle - OCRDevParam.OCRAngle;
                }
                AngleMove(OCRDevParam.OCRAngle);

                SaveOCRDevParam();
                RaisePropertyChanged(nameof(OCRDevParam));
                UpdateImage();
            }
        }
        #endregion

        #region ==> FlatJogCCRCommand
        private AsyncCommand _FlatJogCCRCommand;
        public ICommand FlatJogCCRCommand
        {
            get
            {
                if (null == _FlatJogCCRCommand) _FlatJogCCRCommand = new AsyncCommand(FlatJogCCRCommandFunc);
                return _FlatJogCCRCommand;
            }
        }
        private async Task FlatJogCCRCommandFunc()
        {
            //==> 구현 해야 함.
            //if (this.GetLoaderCommands().CheckWaferIsOnPA(_CognexHostIndex) == false)
            //{
            //    return;
            //}

            double value = RelAngle;
            if (OCRDevParam != null)
            {

                OCRDevParam.OCRAngle += value;
                if (OCRDevParam.OCRAngle > AbAngle)
                {
                    OCRDevParam.OCRAngle = OCRDevParam.OCRAngle - AbAngle;
                }
                AngleMove(OCRDevParam.OCRAngle);

                SaveOCRDevParam();
                RaisePropertyChanged(nameof(OCRDevParam));
                UpdateImage();
            }
        }
        #endregion

        #region ==> HeightJogUpCommand
        //private AsyncCommand _HeightJogUpCommand;
        //public ICommand HeightJogUpCommand
        //{
        //    get
        //    {
        //        if (null == _HeightJogUpCommand) _HeightJogUpCommand = new AsyncCommand(HeightJogUpCommandFunc);
        //        return _HeightJogUpCommand;
        //    }
        //}
        //private async Task HeightJogUpCommandFunc()
        //{
        //    if (JogArmConditionCheck() == false)
        //        return;


        //    int U_STEP_DIST = 50;
        //    int value = U_STEP_DIST;

        //    MoveLoaderAxis(EnumAxisConstants.A, value);

        //    if (_OcrAccessParam != null)
        //    {
        //        _OcrAccessParam.Position.A.Value += value;
        //        this.LoaderController().SaveSystemParam();
        //    }

        //    UpdateImage();

        //}
        #endregion

        #region ==> HeightJogDownCommand
        //private AsyncCommand _HeightJogDownCommand;
        //public ICommand HeightJogDownCommand
        //{
        //    get
        //    {
        //        if (null == _HeightJogDownCommand) _HeightJogDownCommand = new AsyncCommand(HeightJogDownCommandFunc);
        //        return _HeightJogDownCommand;
        //    }
        //}
        //private async Task HeightJogDownCommandFunc()
        //{
        //    if (JogArmConditionCheck() == false)
        //        return;


        //    int U_STEP_DIST = 50;
        //    int value = U_STEP_DIST * -1;

        //    MoveLoaderAxis(EnumAxisConstants.A, value);

        //    if (_OcrAccessParam != null)
        //    {
        //        _OcrAccessParam.Position.A.Value += value;
        //        this.LoaderController().SaveSystemParam();
        //    }

        //    UpdateImage();

        //}
        #endregion

        private void JogMove(OCRAccessParam accparam)
        {
            IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, SelectedModuleIndex + 1);
            if (paModule == null)
            {
                return;
            }
            accparam.Position.U.Value = 0;
            accparam.Position.W.Value = 0;
            this.GetLoaderCommands().PAMove(paModule,
                accparam.Position.U.Value, accparam.Position.W.Value, accparam.VPos.Value);
        }




        #endregion


        private void AngleMove(double angle)
        {
            try
            {
                BtnEnable = false;
                IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, SelectedModuleIndex + 1);
                if (paModule == null)
                {
                    return;
                }

                ICognexOCRModule cognexOCRModule = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, SelectedModuleIndex + 1);
                if (cognexOCRModule == null)
                {
                    return ;
                }
                SubchuckMotionParam subchuckMotionParam = null;
                if (paModule.Holder.TransferObject != null)
                {
                    double angleoffset = 0;
                    double OCRReadXPos = 0;
                    double OCRReadYPos = 0;
                    subchuckMotionParam = cognexOCRModule.GetSubchuckMotionParam(paModule.Holder.TransferObject.Size.Value);
                    if (subchuckMotionParam != null) 
                    {
                        angleoffset = subchuckMotionParam.SubchuckAngle_Offset.Value;
                        OCRReadXPos = subchuckMotionParam.SubchuckXCoord.Value;
                        OCRReadYPos = subchuckMotionParam.SubchuckYCoord.Value;
                        LoggerManager.Debug($"[AngleMove] Host Index: {SelectedModuleIndex + 1}, Wafer Size: {paModule.Holder.TransferObject.Size.Value}," +
                        $" OCR Angle: {angle}, OCR Position (X, Y, Angle Offset): ({OCRReadXPos}, {OCRReadYPos}, {angleoffset})");
                    }

                    var oCRangle = angle + angleoffset;
                    if (oCRangle < 0)
                    {
                        oCRangle = 360 + oCRangle;
                    }
                    else if (oCRangle >= 360)
                    {
                        oCRangle = oCRangle % 360;
                    }
                    
                    var ret = _LoaderModule.GetLoaderCommands().PAMove(paModule, oCRangle);
                    if (ret == EventCodeEnum.NONE)
                    {
                        _LoaderModule.GetLoaderCommands().PAMove(paModule, OCRReadXPos, OCRReadYPos, 0);
                    }

                }
               
                BtnEnable = true;
            }
            catch (Exception err)
            {
                BtnEnable = true;
                LoggerManager.Exception(err);
            }
        }

        #region ====> SETTING GROUP

        #region ==> OCRString
        private String _OCRString;
        public String OCRString
        {
            get { return _OCRString; }
            set
            {
                if (value != _OCRString)
                {
                    _OCRString = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> OCRString
        private String _OCRResult;
        public String OCRResult
        {
            get { return _OCRResult; }
            set
            {
                if (value != _OCRResult)
                {
                    _OCRResult = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ReadingCommand

        private AsyncCommand _ReadingCommand;
        public ICommand ReadingCommand
        {
            get
            {
                if (null == _ReadingCommand) _ReadingCommand = new AsyncCommand(ReadingCommandFunc);
                return _ReadingCommand;
            }
        }
        private async Task ReadingCommandFunc()
        {
            UpdateImage();
            _CognexProcessManager.DO_ReadConfig(SelectedModuleIP.Name, "1");
            OCRString = _CognexProcessManager.CognexCommandManager.ReadConfig.String;
            OCRScore = _CognexProcessManager.CognexCommandManager.ReadConfig.Score;
            int ocrValue = 0;
            OCRResult = "";
            OCRConfig selectedConfig = (OCRConfig)SelectedConfig.CommandArg;
            if (Int32.TryParse(OCRScore, out ocrValue))
            {
                if (ocrValue < selectedConfig.OCRCutLineScore)
                {
                    OCRResult = "Failed";
                    LoggerManager.Debug($"[OCR] Result = {OCRResult}, Socre = {ocrValue}");
                }
                else
                {
                    OCRResult = "Pass";
                    LoggerManager.Debug($"[OCR] Result = {OCRResult}, Score = {ocrValue}, String = {OCRString}");
                }
            }
            else
            {
                OCRResult = "";
            }
            if (selectedConfig.Mark == "11" && OCRResult == "Pass" &&
                (_CognexProcessManager.GetCheckDotMat() == _CognexProcessManager.CognexCommandManager.ReadConfig.OrcList.Count))
            {
                OCRString = _CognexProcessManager.CalOcrChecksum(OCRString);
                LoggerManager.Debug($"[OCR] String after adding checksum = {OCRString}");
            }
        }

        #endregion
        #region ==> OCRScore
        private String _OCRScore;
        public String OCRScore
        {
            get { return _OCRScore; }
            set
            {
                if (value != _OCRScore)
                {
                    _OCRScore = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> IPSelectedChange
        private RelayCommand _IPSelectedChange;
        public ICommand IPSelectedChange
        {
            get
            {
                if (null == _IPSelectedChange) _IPSelectedChange = new RelayCommand(IPSelectedChangeFunc);
                return _IPSelectedChange;
            }
        }
        private void IPSelectedChangeFunc()
        {
            GetOCRAccessParam();
        }
        #endregion

        #endregion

        #region ====> REGION GROUP

        private double _RegionTheta;
        private double _RegionPhi;
        private double _RegionChangeValue = 5;
        private Rect _ImageRect = new Rect(0, 0, 752, 480);

        #region ==> RegionHeight
        private double _RegionHeight;
        public double RegionHeight
        {
            get { return _RegionHeight; }
            set
            {
                if (value != _RegionHeight)
                {
                    _RegionHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegionWidth
        private double _RegionWidth;
        public double RegionWidth
        {
            get { return _RegionWidth; }
            set
            {
                if (value != _RegionWidth)
                {
                    _RegionWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegionY
        private double _RegionY;
        public double RegionY
        {
            get { return _RegionY; }
            set
            {
                if (value != _RegionY)
                {
                    _RegionY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegionX
        private double _RegionX;
        public double RegionX
        {
            get { return _RegionX; }
            set
            {
                if (value != _RegionX)
                {
                    _RegionX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegionDownMoveCommand
        private RelayCommand _RegionDownMoveCommand;
        public ICommand RegionDownMoveCommand
        {
            get
            {
                if (null == _RegionDownMoveCommand) _RegionDownMoveCommand = new RelayCommand(RegionDownMoveCommandFunc);
                return _RegionDownMoveCommand;
            }
        }
        private void RegionDownMoveCommandFunc()
        {
            try
            {
                double yValue = RegionY + _RegionChangeValue;

                if (yValue + RegionHeight > _ImageRect.Height)
                {
                    yValue -= (yValue + RegionHeight) - _ImageRect.Height;
                }

                RegionY = yValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionUpMoveCommand
        private RelayCommand _RegionUpMoveCommand;
        public ICommand RegionUpMoveCommand
        {
            get
            {
                if (null == _RegionUpMoveCommand) _RegionUpMoveCommand = new RelayCommand(RegionUpMoveCommandFunc);
                return _RegionUpMoveCommand;
            }
        }
        private void RegionUpMoveCommandFunc()
        {
            try
            {
                double yValue = RegionY - _RegionChangeValue;

                if (yValue < 0)
                {
                    yValue = 0;
                }

                RegionY = yValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionLeftMoveCommand
        private RelayCommand _RegionLeftMoveCommand;
        public ICommand RegionLeftMoveCommand
        {
            get
            {
                if (null == _RegionLeftMoveCommand) _RegionLeftMoveCommand = new RelayCommand(RegionLeftMoveCommandFunc);
                return _RegionLeftMoveCommand;
            }
        }
        private void RegionLeftMoveCommandFunc()
        {
            try
            {
                double xValue = RegionX - _RegionChangeValue;

                if (xValue < 0)
                {
                    xValue = 0;
                }

                RegionX = xValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionRightMoveCommand
        private RelayCommand _RegionRightMoveCommand;
        public ICommand RegionRightMoveCommand
        {
            get
            {
                if (null == _RegionRightMoveCommand) _RegionRightMoveCommand = new RelayCommand(RegionRightMoveCommandFunc);
                return _RegionRightMoveCommand;
            }
        }
        private void RegionRightMoveCommandFunc()
        {
            try
            {
                double xValue = RegionX + _RegionChangeValue;

                //if (_ImageRect.Contains(new Rect(xValue, RegionY, RegionWidth, RegionHeight)) == false)
                if (xValue + RegionWidth > _ImageRect.Width)
                {
                    xValue -= (xValue + RegionWidth) - _ImageRect.Width;
                }

                RegionX = xValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionWidthPlusCommand
        private RelayCommand _RegionWidthPlusCommand;
        public ICommand RegionWidthPlusCommand
        {
            get
            {
                if (null == _RegionWidthPlusCommand) _RegionWidthPlusCommand = new RelayCommand(RegionWidthPlusCommandFunc);
                return _RegionWidthPlusCommand;
            }
        }
        private void RegionWidthPlusCommandFunc()
        {
            try
            {
                double xValue = RegionX - (_RegionChangeValue / 2);
                double widthValue = RegionWidth + _RegionChangeValue;

                bool leftReached = false;
                bool rightReached = false;
                if (xValue < 0)
                {
                    xValue = 0;
                    leftReached = true;
                }
                if (xValue + widthValue > _ImageRect.Width)
                {
                    xValue -= (xValue + widthValue) - _ImageRect.Width;
                    rightReached = true;
                }

                if (leftReached && rightReached)
                {
                    xValue = 0;
                    widthValue = _ImageRect.Width;
                }

                RegionX = xValue;
                RegionWidth = widthValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionHeightPlusCommand
        private RelayCommand _RegionHeightPlusCommand;
        public ICommand RegionHeightPlusCommand
        {
            get
            {
                if (null == _RegionHeightPlusCommand) _RegionHeightPlusCommand = new RelayCommand(RegionHeightPlusCommandFunc);
                return _RegionHeightPlusCommand;
            }
        }
        private void RegionHeightPlusCommandFunc()
        {
            try
            {
                double yValue = RegionY - (_RegionChangeValue / 2);
                double heightValue = RegionHeight + _RegionChangeValue;

                bool upReached = false;
                bool downReached = false;
                if (yValue < 0)
                {
                    yValue = 0;
                    upReached = true;
                }
                if (yValue + heightValue > _ImageRect.Height)
                {
                    yValue -= (yValue + heightValue) - _ImageRect.Height;
                    downReached = true;
                }

                if (upReached && downReached)
                {
                    yValue = 0;
                    heightValue = _ImageRect.Height;
                }

                RegionY = yValue;
                RegionHeight = heightValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionWidthMinusCommand
        private RelayCommand _RegionWidthMinusCommand;
        public ICommand RegionWidthMinusCommand
        {
            get
            {
                if (null == _RegionWidthMinusCommand) _RegionWidthMinusCommand = new RelayCommand(RegionWidthMinusCommandFunc);
                return _RegionWidthMinusCommand;
            }
        }
        private void RegionWidthMinusCommandFunc()
        {
            try
            {
                double xValue = RegionX + (_RegionChangeValue / 2);
                double widthValue = RegionWidth - _RegionChangeValue;

                if (widthValue <= 0)
                    return;

                RegionX = xValue;
                RegionWidth = widthValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionHeightMinusCommand
        private RelayCommand _RegionHeightMinusCommand;
        public ICommand RegionHeightMinusCommand
        {
            get
            {
                if (null == _RegionHeightMinusCommand) _RegionHeightMinusCommand = new RelayCommand(RegionHeightMinusCommandFunc);
                return _RegionHeightMinusCommand;
            }
        }
        private void RegionHeightMinusCommandFunc()
        {
            try
            {
                double yValue = RegionY + (_RegionChangeValue / 2);
                double heightValue = RegionHeight - _RegionChangeValue;

                if (heightValue <= 0)
                    return;

                RegionY = yValue;
                RegionHeight = heightValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> RegionSet
        private RelayCommand _RegionSet;
        public ICommand RegionSet
        {
            get
            {
                if (null == _RegionSet) _RegionSet = new RelayCommand(RegionSetFunc);
                return _RegionSet;
            }
        }
        private void RegionSetFunc()
        {
            try
            {
                _CognexProcessManager.DO_SetConfigRegion(SelectedModuleIP.Name, RegionY.ToString(), RegionX.ToString(), RegionHeight.ToString(), RegionWidth.ToString(), _RegionTheta.ToString(), _RegionPhi.ToString());
                SaveRegionConfig();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #endregion

        #region ====> CHAR REGION GROUP

        private double _CharChangeValue = 5;

        #region ==> CharY
        private double _CharY = 217;
        public double CharY
        {
            get { return _CharY; }
            set
            {
                if (value != _CharY)
                {
                    _CharY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CharX
        private double _CharX = 389;
        public double CharX
        {
            get { return _CharX; }
            set
            {
                if (value != _CharX)
                {
                    _CharX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CharHeight
        private double _CharHeight;
        public double CharHeight
        {
            get { return _CharHeight; }
            set
            {
                if (value != _CharHeight)
                {
                    _CharHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CharWidth
        private double _CharWidth;
        public double CharWidth
        {
            get { return _CharWidth; }
            set
            {
                if (value != _CharWidth)
                {
                    _CharWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CharRegionLeftMoveCommand
        private RelayCommand _CharRegionLeftMoveCommand;
        public ICommand CharRegionLeftMoveCommand
        {
            get
            {
                if (null == _CharRegionLeftMoveCommand) _CharRegionLeftMoveCommand = new RelayCommand(CharRegionLeftMoveCommandFunc);
                return _CharRegionLeftMoveCommand;
            }
        }
        private void CharRegionLeftMoveCommandFunc()
        {
            try
            {
                double xValue = CharX - _CharChangeValue;

                //if (_ImageRect.Contains(new Rect(xValue, CharY, CharWidth, CharHeight)) == false)
                if (xValue < 0)
                {
                    xValue = 0;
                }

                CharX = xValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionRightMoveCommand
        private RelayCommand _CharRegionRightMoveCommand;
        public ICommand CharRegionRightMoveCommand
        {
            get
            {
                if (null == _CharRegionRightMoveCommand) _CharRegionRightMoveCommand = new RelayCommand(CharRegionRightMoveCommandFunc);
                return _CharRegionRightMoveCommand;
            }
        }
        private void CharRegionRightMoveCommandFunc()
        {
            try
            {
                double xValue = CharX + _CharChangeValue;

                //if (_ImageRect.Contains(new Rect(xValue, CharY, CharWidth, CharHeight)) == false)
                if (xValue + CharWidth > _ImageRect.Width)
                {
                    xValue -= (xValue + CharWidth) - _ImageRect.Width;
                }

                CharX = xValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionUpMoveCommand
        private RelayCommand _CharRegionUpMoveCommand;
        public ICommand CharRegionUpMoveCommand
        {
            get
            {
                if (null == _CharRegionUpMoveCommand) _CharRegionUpMoveCommand = new RelayCommand(CharRegionUpMoveCommandFunc);
                return _CharRegionUpMoveCommand;
            }
        }
        private void CharRegionUpMoveCommandFunc()
        {
            try
            {
                double yValue = CharY - _CharChangeValue;

                //if (_ImageRect.Contains(new Rect(CharX, yValue, CharWidth, CharHeight)) == false)
                if (yValue < 0)
                {
                    yValue = 0;
                }

                CharY = yValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionDownMoveCommand
        private RelayCommand _CharRegionDownMoveCommand;
        public ICommand CharRegionDownMoveCommand
        {
            get
            {
                if (null == _CharRegionDownMoveCommand) _CharRegionDownMoveCommand = new RelayCommand(CharRegionDownMoveCommandFunc);
                return _CharRegionDownMoveCommand;
            }
        }
        private void CharRegionDownMoveCommandFunc()
        {
            try
            {
                double yValue = CharY + _CharChangeValue;

                if (yValue + CharHeight > _ImageRect.Height)
                {
                    yValue -= (yValue + CharHeight) - _ImageRect.Height;
                }

                CharY = yValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionWidthPlusCommand
        private RelayCommand _CharRegionWidthPlusCommand;
        public ICommand CharRegionWidthPlusCommand
        {
            get
            {
                if (null == _CharRegionWidthPlusCommand) _CharRegionWidthPlusCommand = new RelayCommand(CharRegionWidthPlusCommandFunc);
                return _CharRegionWidthPlusCommand;
            }
        }
        private void CharRegionWidthPlusCommandFunc()
        {
            try
            {
                double xValue = CharX - (_CharChangeValue / 2);
                double widthValue = CharWidth + _CharChangeValue;

                bool leftReached = false;
                bool rightReached = false;
                if (xValue < 0)
                {
                    xValue = 0;
                    leftReached = true;
                }
                if (xValue + widthValue > _ImageRect.Width)
                {
                    xValue -= (xValue + widthValue) - _ImageRect.Width;
                    rightReached = true;
                }

                if (leftReached && rightReached)
                {
                    xValue = 0;
                    widthValue = _ImageRect.Width;
                }

                CharX = xValue;
                CharWidth = widthValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionHeightPlusCommand
        private RelayCommand _CharRegionHeightPlusCommand;
        public ICommand CharRegionHeightPlusCommand
        {
            get
            {
                if (null == _CharRegionHeightPlusCommand) _CharRegionHeightPlusCommand = new RelayCommand(CharRegionHeightPlusCommandFunc);
                return _CharRegionHeightPlusCommand;
            }
        }
        private void CharRegionHeightPlusCommandFunc()
        {
            try
            {
                double yValue = CharY - (_CharChangeValue / 2);
                double heightValue = CharHeight + _CharChangeValue;

                bool upReached = false;
                bool downReached = false;
                if (yValue < 0)
                {
                    yValue = 0;
                    upReached = true;
                }
                if (yValue + heightValue > _ImageRect.Height)
                {
                    yValue -= (yValue + heightValue) - _ImageRect.Height;
                    downReached = true;
                }

                if (upReached && downReached)
                {
                    yValue = 0;
                    heightValue = _ImageRect.Height;
                }

                CharY = yValue;
                CharHeight = heightValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionWidthMinusCommand
        private RelayCommand _CharRegionWidthMinusCommand;
        public ICommand CharRegionWidthMinusCommand
        {
            get
            {
                if (null == _CharRegionWidthMinusCommand) _CharRegionWidthMinusCommand = new RelayCommand(CharRegionWidthMinusCommandFunc);
                return _CharRegionWidthMinusCommand;
            }
        }
        private void CharRegionWidthMinusCommandFunc()
        {
            try
            {
                double xValue = CharX + (_CharChangeValue / 2);
                double widthValue = CharWidth - _CharChangeValue;

                if (widthValue <= 0)
                    return;

                CharX = xValue;
                CharWidth = widthValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionHeightMinusCommand
        private RelayCommand _CharRegionHeightMinusCommand;
        public ICommand CharRegionHeightMinusCommand
        {
            get
            {
                if (null == _CharRegionHeightMinusCommand) _CharRegionHeightMinusCommand = new RelayCommand(CharRegionHeightMinusCommandFunc);
                return _CharRegionHeightMinusCommand;
            }
        }
        private void CharRegionHeightMinusCommandFunc()
        {
            try
            {
                double yValue = CharY + (_CharChangeValue / 2);
                double heightValue = CharHeight - _CharChangeValue;

                if (heightValue <= 0)
                    return;

                CharY = yValue;
                CharHeight = heightValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #region ==> CharRegionSet
        private RelayCommand _CharRegionSet;
        public ICommand CharRegionSet
        {
            get
            {
                if (null == _CharRegionSet) _CharRegionSet = new RelayCommand(CharRegionSetFunc);
                return _CharRegionSet;
            }
        }
        private void CharRegionSetFunc()
        {
            try
            {
                _CognexProcessManager.DO_SetConfigCharSize(SelectedModuleIP.Name, CharHeight.ToString(), CharWidth.ToString());
                SaveCharRegionConfig();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        #endregion
        #region ====> OCR Check Option
        private AsyncCommand<object> _LotIntegrityEnableCommand;
        public ICommand LotIntegrityEnableCommand
        {
            get
            {
                if (null == _LotIntegrityEnableCommand)
                    _LotIntegrityEnableCommand = new AsyncCommand<object>(LotIntegrityEnableCommandFunc);
                return _LotIntegrityEnableCommand;
            }
        }
        private async Task LotIntegrityEnableCommandFunc(object param)
        {
            try
            {
                SaveOCRDevParam();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private String _LotidLength;
        public String LotidLength
        {
            get { return _LotidLength; }
            set
            {
                if (value != _LotidLength)
                {
                    _LotidLength = value;
                    RaisePropertyChanged();
                }
            }
        }
        private RelayCommand _LotIntegrityLengthCommand;
        public ICommand LotIntegrityLengthCommand
        {
            get
            {
                if (null == _LotIntegrityLengthCommand) _LotIntegrityLengthCommand = new RelayCommand(LotIntegrityLengthCommandFunc);
                return _LotIntegrityLengthCommand;
            }
        }
        private void LotIntegrityLengthCommandFunc()
        {
            String input = VirtualKeyboard.Show(OCRDevParam.lotIntegrity.Lotnamelength.ToString(), KB_TYPE.DECIMAL | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL);

            if (String.IsNullOrEmpty(input))
                return;
            LotidLength = input;
            //OCRDevParam.lotIntegrity.LotIntegrityEnable;
            //OCRDevParam.lotIntegrity.LotnameDigit;
            OCRDevParam.lotIntegrity.Lotnamelength = Int32.Parse(LotidLength);
            SaveOCRDevParam();

        }
        private String _LotidDigit;
        public String LotidDigit
        {
            get { return _LotidDigit; }
            set
            {
                if (value != _LotidDigit)
                {
                    _LotidDigit = value;
                    RaisePropertyChanged();
                }
            }
        }
        private RelayCommand _LotIntegrityDigitCommand;
        public ICommand LotIntegrityDigitCommand
        {
            get
            {
                if (null == _LotIntegrityDigitCommand) _LotIntegrityDigitCommand = new RelayCommand(LotIntegrityDigitCommandFunc);
                return _LotIntegrityDigitCommand;
            }
        }
        private void LotIntegrityDigitCommandFunc()
        {
            String input = VirtualKeyboard.Show(OCRDevParam.lotIntegrity.LotnameDigit.ToString(), KB_TYPE.DECIMAL | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL);

            if (String.IsNullOrEmpty(input))
                return;
            LotidDigit = input;
            //OCRDevParam.lotIntegrity.LotIntegrityEnable;
            OCRDevParam.lotIntegrity.LotnameDigit = Int32.Parse(LotidDigit);
            //OCRDevParam.lotIntegrity.LotnameTlength;
            SaveOCRDevParam();
            
        }





        #endregion
        #region ====> LIGHT GROUP

        #region ==> LightModeList
        private ObservableCollection<VmGPComboBoxItem> _LightModeList;
        public ObservableCollection<VmGPComboBoxItem> LightModeList
        {
            get { return _LightModeList; }
            set
            {
                if (value != _LightModeList)
                {
                    _LightModeList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedLight
        private VmGPComboBoxItem _SelectedLight;
        public VmGPComboBoxItem SelectedLight
        {
            get { return _SelectedLight; }
            set
            {
                if (value != _SelectedLight)
                {
                    _SelectedLight = value;
                    RaisePropertyChanged();

                    if (_SelectedLight == null)
                        return;

                    _CognexProcessManager.DO_SetConfigLightMode(SelectedModuleIP.Name, (String)_SelectedLight.CommandArg);
                    UpdateImage();

                    SaveLightModeConfig();
                }
            }
        }
        #endregion

        #region ==> LightIntensity
        private int _LightIntensity;
        public int LightIntensity
        {
            get { return _LightIntensity; }
            set
            {
                if (value != _LightIntensity)
                {
                    _LightIntensity = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ScrollStopCommand
        private RelayCommand _ScrollStopCommand;
        public ICommand ScrollStopCommand
        {
            get
            {
                if (null == _ScrollStopCommand) _ScrollStopCommand = new RelayCommand(ScrollStopCommandFunc);
                return _ScrollStopCommand;
            }
        }
        private void ScrollStopCommandFunc()
        {
            UpdateLight();
        }
        #endregion

        #region ==> LightNextTypeCommand
        private RelayCommand _LightNextTypeCommand;
        public ICommand LightNextTypeCommand
        {
            get
            {
                if (null == _LightNextTypeCommand) _LightNextTypeCommand = new RelayCommand(LightNextTypeCommandFunc);
                return _LightNextTypeCommand;
            }
        }
        private void LightNextTypeCommandFunc()
        {
            int idx = LightModeList.IndexOf(SelectedLight);
            idx++;

            if (idx > LightModeList.Count - 1)
                return;

            SelectedLight = LightModeList[idx];
        }
        #endregion

        #region ==> LightPrevTypeCommand
        private RelayCommand _LightPrevTypeCommand;
        public ICommand LightPrevTypeCommand
        {
            get
            {
                if (null == _LightPrevTypeCommand) _LightPrevTypeCommand = new RelayCommand(LightPrevTypeCommandFunc);
                return _LightPrevTypeCommand;
            }
        }
        private void LightPrevTypeCommandFunc()
        {
            int idx = LightModeList.IndexOf(SelectedLight);
            idx--;

            if (idx < 0)
                return;

            SelectedLight = LightModeList[idx];
        }
        #endregion

        #region ==> LightPowerIncreaseCommand
        private RelayCommand _LightPowerIncreaseCommand;
        public ICommand LightPowerIncreaseCommand
        {
            get
            {
                if (null == _LightPowerIncreaseCommand) _LightPowerIncreaseCommand = new RelayCommand(LightPowerIncreaseCommandFunc);
                return _LightPowerIncreaseCommand;
            }
        }
        private void LightPowerIncreaseCommandFunc()
        {
            if (LightIntensity + 1 > _MaxLightIntensity)
                return;

            LightIntensity += 1;
            this.MetroDialogManager()?.ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
            UpdateLight();
            this.MetroDialogManager()?.CloseWaitCancelDialaog(this.GetHashCode().ToString());
        }
        #endregion

        #region ==> LightPowerDecreaseCommand
        private RelayCommand _LightPowerDecreaseCommand;
        public ICommand LightPowerDecreaseCommand
        {
            get
            {
                if (null == _LightPowerDecreaseCommand) _LightPowerDecreaseCommand = new RelayCommand(LightPowerDecreaseCommandFunc);
                return _LightPowerDecreaseCommand;
            }
        }
        private void LightPowerDecreaseCommandFunc()
        {
            if (LightIntensity - 1 < 0)
                return;

            LightIntensity -= 1;

            UpdateLight();
        }
        #endregion

        private const int _MaxLightIntensity = 255;
        private const int _CognextMaxLightIntensity = 51;
        private const float _ConvertToCognexLightRatio = (float)_CognextMaxLightIntensity / (float)_MaxLightIntensity;
        private const float _ConvertToUILightRatio = (float)_MaxLightIntensity / (float)_CognextMaxLightIntensity;

        private void SetupLight()
        {
            try
            {
                if (SelectedModuleIP == null)
                {
                    return;
                }
                if (_CognexProcessManager.DO_GetConfigEx(SelectedModuleIP.Name) == false)
                    return;

                LightModeList = new ObservableCollection<VmGPComboBoxItem>();
                LightModeList.Add(new VmGPComboBoxItem("Mode0", "0"));
                LightModeList.Add(new VmGPComboBoxItem("Mode1", "1"));
                LightModeList.Add(new VmGPComboBoxItem("Mode2", "2"));
                LightModeList.Add(new VmGPComboBoxItem("Mode3", "3"));
                LightModeList.Add(new VmGPComboBoxItem("Mode4", "4"));
                LightModeList.Add(new VmGPComboBoxItem("Mode5", "5"));
                LightModeList.Add(new VmGPComboBoxItem("Mode6", "6"));
                LightModeList.Add(new VmGPComboBoxItem("Mode7", "7"));
                LightModeList.Add(new VmGPComboBoxItem("Mode8", "8"));
                LightModeList.Add(new VmGPComboBoxItem("Mode9", "9"));
                LightModeList.Add(new VmGPComboBoxItem("Mode10", "10"));
                LightModeList.Add(new VmGPComboBoxItem("Mode11", "11"));
                LightModeList.Add(new VmGPComboBoxItem("Custom", "12"));
                LightModeList.Add(new VmGPComboBoxItem("External", "13"));
                LightModeList.Add(new VmGPComboBoxItem("Expansion", "14"));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void UpdateLight()
        {
            _CognexProcessManager.Async_SetConfigLightPower(SelectedModuleIP.Name, (LightIntensity * _ConvertToCognexLightRatio).ToString(), LightPowerChangeCallBack);
        }
        private bool LightPowerChangeCallBack()
        {
            UpdateImage();
            SaveLightIntensityConfig();
            return true;
        }
        #endregion

        #region ====> TUNE GROUP

        #region ==> ProgressValue
        private int _ProgressValue;
        public int ProgressValue
        {
            get { return _ProgressValue; }
            set
            {
                if (value != _ProgressValue)
                {
                    _ProgressValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> TuneStartCommand
        private AsyncCommand _TuneStartCommand;
        public ICommand TuneStartCommand
        {
            get
            {
                if (null == _TuneStartCommand) _TuneStartCommand = new AsyncCommand(TuneStartCommandFunc);
                return _TuneStartCommand;
            }
        }

        bool isTuneRun;

        private async Task TuneStartCommandFunc()
        {
            try
            {
                //await this.WaitCancelDialogService().ShowDialog("OCR");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                isTuneRun = true;

                bool filterTuneEnable = false;
                bool lightTuneEnable = true;
                bool sizeTuneEnable = true;
                int bestScoreValue = 0;

                _CognexProcessManager.DO_TuneConfigEx(SelectedModuleIP.Name, $"{0}", filterTuneEnable, lightTuneEnable, sizeTuneEnable);

                while (isTuneRun)
                {
                    _CognexProcessManager.DO_TuneConfigEx(SelectedModuleIP.Name, $"{1}", filterTuneEnable, lightTuneEnable, sizeTuneEnable);

                    ProgressValue = Convert.ToInt32(_CognexProcessManager.CognexCommandManager.TuneConfigEx.Percent);
                    String bestScoreStr = _CognexProcessManager.CognexCommandManager.TuneConfigEx.BestScore;
                    bestScoreValue = Convert.ToInt32(bestScoreStr);

                    if (ProgressValue >= 100)
                    {
                        break;
                    }
                }

                //==> 안전 장치
                ProgressValue = 100;
                isTuneRun = false;

                if (bestScoreValue < OCRCutLineScore)
                {
                    this.MetroDialogManager().ShowMessageDialog("[OCR Auto Tune]", $"[Score : {bestScoreValue}] Auto tuning failed", EnumMessageStyle.Affirmative);
                    isTuneRun = false;
                    UpdateImage();
                }
                else
                {

                    EnumMessageDialogResult msgRes = this.MetroDialogManager().ShowMessageDialog("Overwrite", $"[Score : {bestScoreValue}] Do you want accept this Tune?", EnumMessageStyle.AffirmativeAndNegative).Result;

                    if (msgRes == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        _CognexProcessManager.DO_TuneConfigEx(SelectedModuleIP.Name, $"{2}", filterTuneEnable, lightTuneEnable, sizeTuneEnable);
                        _CognexProcessManager.DO_GetConfigEx(SelectedModuleIP.Name);

                        UpdateLightUI();
                        UpdateGeneralUI();
                        UpdateRegionField();
                        UpdateCharRegionField();
                        UpdateImage();
                        SaveOCRDevParam();
                    }
                    else
                    {
                        isTuneRun = false;
                        UpdateImage();
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        private void UpdateLightUI()
        {
            String currentLightMode = _CognexProcessManager.CognexCommandManager.GetConfigEx.LightMode;
            String lightValueString = _CognexProcessManager.CognexCommandManager.GetConfigEx.LightPower;

            float cogLightIntensity = 0;
            float.TryParse(lightValueString, out cogLightIntensity);

            int mod = (int)(cogLightIntensity / _ConvertToCognexLightRatio);
            float a = mod * _ConvertToCognexLightRatio;
            double b = cogLightIntensity - a;
            if (b < 0.1)
            {
                LightIntensity = (int)((mod) * _ConvertToCognexLightRatio * _ConvertToUILightRatio);
            }
            else
            {
                LightIntensity = (int)((mod + 1) * _ConvertToCognexLightRatio * _ConvertToUILightRatio);
            }

            //LightIntensity = cogLightIntensity * _ConvertToUILightRatio;
            SelectedLight = LightModeList.FirstOrDefault(item => item.CommandArg.ToString() == currentLightMode);

            OCRConfig selectedConfig = (OCRConfig)SelectedConfig.CommandArg;
            selectedConfig.LightIntensity = LightIntensity;
            selectedConfig.Light = currentLightMode;

        }
        private void UpdateGeneralUI()
        {
            String orientation = _CognexProcessManager.CognexCommandManager.GetConfigEx.Orientation;
            String mark = _CognexProcessManager.CognexCommandManager.GetConfigEx.Mark;
            String checksum = _CognexProcessManager.CognexCommandManager.GetConfigEx.Checksum;
            String retryOption = _CognexProcessManager.CognexCommandManager.GetConfigEx.Retry;
            String fieldString = _CognexProcessManager.CognexCommandManager.GetConfigEx.FieldString;

            SelectedImageDirection = ImageDirectionList.FirstOrDefault(item => item.CommandArg.ToString() == orientation);
            SelectedMark = MarkList.FirstOrDefault(item => item.CommandArg.ToString() == mark);
            SelectedCheckSum = CheckSumList.FirstOrDefault(item => item.CommandArg.ToString() == checksum);
            SelectedRetryOption = RetryOptionList.FirstOrDefault(item => item.CommandArg.ToString() == retryOption);
            FieldString = fieldString;

            OCRConfig selectedConfig = (OCRConfig)SelectedConfig.CommandArg;
            selectedConfig.Direction = orientation;
            selectedConfig.Mark = mark;
            selectedConfig.CheckSum = checksum;
            selectedConfig.FieldString = fieldString;

            OCRCutLineScore = selectedConfig.OCRCutLineScore;
        }
        private void UpdateRegionField()
        {
            RegionY = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.RegionY);
            RegionX = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.RegionX);
            RegionHeight = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.RegionHeight);
            RegionWidth = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.RegionWidth);
            _RegionTheta = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.RegionTheta);
            _RegionPhi = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.RegionPhi);

            OCRConfig selectedConfig = (OCRConfig)SelectedConfig.CommandArg;
            selectedConfig.RegionY = RegionY;
            selectedConfig.RegionX = RegionX;
            selectedConfig.RegionHeight = RegionHeight;
            selectedConfig.RegionWidth = RegionWidth;
        }
        private void UpdateCharRegionField()
        {
            CharHeight = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.CharHeight);
            CharWidth = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.CharWidth);

            OCRConfig selectedConfig = (OCRConfig)SelectedConfig.CommandArg;
            selectedConfig.CharHeight = CharHeight;
            selectedConfig.CharWidth = CharWidth;

        }
        #endregion

        #endregion

        #region ====> CONFIG GROUP

        #region ==> ModuleIPList
        private ObservableCollection<VmGPComboBoxItem> _ModuleIPList;
        public ObservableCollection<VmGPComboBoxItem> ModuleIPList
        {
            get { return _ModuleIPList; }
            set
            {
                if (value != _ModuleIPList)
                {
                    _ModuleIPList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedModuleIP
        private VmGPComboBoxItem _SelectedModuleIP;
        public VmGPComboBoxItem SelectedModuleIP
        {
            get { return _SelectedModuleIP; }
            set
            {
                if (value != _SelectedModuleIP)
                {
                    _SelectedModuleIP = value;
                    RaisePropertyChanged();

                    if (_SelectedModuleIP == null)
                    {
                        return;
                    }

                    CognexHost cognexHost = (CognexHost)_SelectedModuleIP.CommandArg;
                    //foreach (VmGPComboBoxItem item in DeviceList)
                    //{
                    //    CognexConfig cognexConfig = (CognexConfig)item.CommandArg;
                    //    String configName = cognexConfig.Name;
                    //    if (cognexHost.ConfigName == configName)
                    //    {
                    //        //==> 선택된 Config 로 Update
                    //        if (DeviceConfigDic.ContainsKey(cognexHost.ConfigName)&& DeviceConfigDic[cognexHost.ConfigName].Count()>0)
                    //        {
                    //            SelectedConfig = DeviceConfigDic[cognexHost.ConfigName].First();
                    //        }
                    //        RaisePropertyChanged(nameof(SelectedConfig));
                    //        LoadSelectedConfig();
                    //        SaveModuleConfig();

                    //        break;
                    //    }
                    //}

                }
            }
        }
        #endregion

        #region ==> SelectedModuleIndex
        private int _SelectedModuleIndex;
        public int SelectedModuleIndex
        {
            get { return _SelectedModuleIndex; }
            set
            {
                if (value != _SelectedModuleIndex)
                {
                    _SelectedModuleIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> DeviceName
        private string _DeviceName;
        public string DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (value != _DeviceName)
                {
                    _DeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _DeviceNameText;
        public string DeviceNameText
        {
            get { return _DeviceNameText; }
            set
            {
                if (value != _DeviceNameText)
                {
                    _DeviceNameText = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DeviceList
        private ObservableCollection<VmGPComboBoxItem> _DeviceList;
        public ObservableCollection<VmGPComboBoxItem> DeviceList
        {
            get { return _DeviceList; }
            set
            {
                if (value != _DeviceList)
                {
                    _DeviceList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private Dictionary<string, ObservableCollection<VmGPComboBoxItem>> _DeviceConfigDic = new Dictionary<string, ObservableCollection<VmGPComboBoxItem>>();
        public Dictionary<string, ObservableCollection<VmGPComboBoxItem>> DeviceConfigDic
        {
            get { return _DeviceConfigDic; }
            set
            {
                if (value != _DeviceConfigDic)
                {
                    _DeviceConfigDic = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> ConfigList
        private ObservableCollection<VmGPComboBoxItem> _ConfigList;
        public ObservableCollection<VmGPComboBoxItem> ConfigList
        {
            get { return _ConfigList; }
            set
            {
                if (value != _ConfigList)
                {
                    _ConfigList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedConfig
        private VmGPComboBoxItem _SelectedConfig;
        public VmGPComboBoxItem SelectedConfig
        {
            get { return _SelectedConfig; }
            set
            {
                if (value != _SelectedConfig)
                {
                    _SelectedConfig = value;
                    RaisePropertyChanged();

                    if (_SelectedConfig == null)
                    {
                        ConfigName = String.Empty;
                        return;
                    }

                    ConfigName = _SelectedConfig.Name;
                    LoadSelectedConfig();

                    SaveModuleConfig();
                }
            }
        }
        #endregion

        #region ==> SelectedConfig
        private VmGPComboBoxItem _SelectedDevice;
        public VmGPComboBoxItem SelectedDevice
        {
            get { return _SelectedDevice; }
            set
            {
                if (value != _SelectedDevice)
                {
                    _SelectedDevice = value;
                    RaisePropertyChanged();

                    if (_SelectedDevice == null)
                    {
                        DeviceName = String.Empty;
                        return;
                    }

                    DeviceName = _SelectedDevice.Name;
                    

                    var ret = LoadOCRDevParam();

                    if (ret == EventCodeEnum.NONE)
                    {
                        if (ConfigList == null)
                        {
                            ConfigList = new ObservableCollection<VmGPComboBoxItem>();
                        }
                        ConfigList.Clear();
                        for (int i = 0; i < OCRDevParam.ConfigList.Count; i++)
                        {
                            VmGPComboBoxItem newConfigItem = new VmGPComboBoxItem("Config" + i, OCRDevParam.ConfigList[i]);
                            ConfigList.Add(newConfigItem);
                        }
                        SelectedConfig = ConfigList.First();
                        LotidLength = OCRDevParam.lotIntegrity.Lotnamelength.ToString();
                        LotidDigit = OCRDevParam.lotIntegrity.LotnameDigit.ToString();
                    }
                    else
                    {
                        ConfigList = new ObservableCollection<VmGPComboBoxItem>();
                        SelectedConfig = ConfigList.First();
                        LotidLength = string.Empty;
                        LotidDigit = string.Empty;
                    }

                    //if (DeviceConfigDic.ContainsKey(DeviceName)&& DeviceConfigDic[DeviceName].Count()>0)
                    //{
                    //    SelectedConfig = DeviceConfigDic[DeviceName].First();
                    //    ConfigName = SelectedConfig.Name;
                    //    ConfigList = DeviceConfigDic[DeviceName];
                    //    RaisePropertyChanged(nameof(ConfigList));
                    //}
                    RaisePropertyChanged(nameof(ConfigList));
                    RaisePropertyChanged(nameof(SelectedConfig));
                    // LoadSelectedConfig();

                    //  SaveModuleConfig();
                }
            }
        }
        private string ParameterPath = null;

        private OCRDevParameter _OCRDevParam;
        public OCRDevParameter OCRDevParam
        {
            get { return _OCRDevParam; }
            set
            {
                if (value != _OCRDevParam)
                {
                    _OCRDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        public EventCodeEnum LoadOCRDevParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IDeviceManager devicemanager = _Container.Resolve<IDeviceManager>();

                if (devicemanager != null)
                {
                    if(ShowingWaferType == EnumWaferType.POLISH)
                    {
                        var polishInfo = devicemanager.GetPolishWaferSources().Where(w => w.DefineName.Value == DeviceName).FirstOrDefault();
                        if (polishInfo != null)
                        {
                            OCRDevParam = polishInfo.OCRConfigParam;
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"[VmGPCognexOCRMainPage] LoadOCRDevParam(): PolishInfo({DeviceName}) is Null");
                        }
                    }
                    else
                    {
                        var path = devicemanager.GetLoaderDevicePath();

                        OCRDevParameter OCRDev = new OCRDevParameter();
                        path += "\\" + DeviceName + "\\" + OCRDev.FilePath + "\\" + OCRDev.FileName;
                        ParameterPath = path;
                        IParam tmpParam = new OCRDevParameter();
                        tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                        retVal = this.LoadParameter(ref tmpParam, typeof(OCRDevParameter), null, ParameterPath);
                        if (retVal == EventCodeEnum.NONE)
                        {
                            OCRDevParam = tmpParam as OCRDevParameter;
                        }
                    }
                    

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum SaveOCRDevParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(ShowingWaferType == EnumWaferType.POLISH)
                {
                    //현재 보고 있는 Device의 OCRDevParam을 저장해줘야함.
                    var polinshInfo = this.DeviceManager().GetPolishWaferSources().Where(w => w.DefineName.Value == DeviceName).FirstOrDefault();
                    polinshInfo.OCRConfigParam.Copy(OCRDevParam);

                    this.DeviceManager().SaveSysParameter();
                }
                else
                {
                    if (ParameterPath != null)
                    {
                        retVal = this.SaveParameter(OCRDevParam, null, ParameterPath);
                    }
                    this._LoaderModule.SaveLoaderOCRConfig();//TODO: System용으로 쓰고자 하는 파라밑터로 보이는데 Host에서 OCR Mode를 변경할 때 사용하는 것으로 보임.. 여기 꼭 있어야하는 코드 일까? 검토 필요
                }
              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        #region ==> ConfigName
        private String _ConfigName;
        public String ConfigName
        {
            get { return _ConfigName; }
            set
            {
                if (value != _ConfigName)
                {
                    _ConfigName = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DeviceAddCommand
        private RelayCommand _DeviceAddCommand;
        public ICommand DeviceAddCommand
        {
            get
            {
                if (null == _DeviceAddCommand) _DeviceAddCommand = new RelayCommand(DeviceAddCommandFunc);
                return _DeviceAddCommand;
            }
        }
        private void DeviceAddCommandFunc()
        {
            if (String.IsNullOrEmpty(DeviceNameText))
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(DeviceNameText))
            {
                return;
            }
            if (DeviceList.FirstOrDefault(item => item.Name == DeviceNameText) != null)
            {
                return;
            }

            CognexConfig newConfig = new CognexConfig();
            newConfig.Name = DeviceNameText;
            if (!DeviceConfigDic.ContainsKey(newConfig.Name))
                DeviceConfigDic.Add(newConfig.Name, new ObservableCollection<VmGPComboBoxItem>());

            VmGPComboBoxItem newDevItem = new VmGPComboBoxItem(newConfig.Name, newConfig);
            VmGPComboBoxItem newConfigItem = new VmGPComboBoxItem("Config0", newConfig);
            DeviceConfigDic[newConfig.Name].Add(newConfigItem);
            DispatcherService.Invoke((System.Action)(() =>
            {
                DeviceList.Add(newDevItem);
            }));


            SelectedConfig = newConfigItem;
            ConfigList = DeviceConfigDic[newConfig.Name];
            RaisePropertyChanged(nameof(ConfigList));
            RaisePropertyChanged(nameof(DeviceList));
            _CognexProcessManager.CognexProcDevParam.ConfigList.Add(newConfig);
            _CognexProcessManager.SaveConfig();
        }
        #endregion

        #region ==> DeviceDelCommand
        private RelayCommand _DeviceDelCommand;
        public ICommand DeviceDelCommand
        {
            get
            {
                if (null == _DeviceDelCommand) _DeviceDelCommand = new RelayCommand(DeviceDelCommandFunc);
                return _DeviceDelCommand;
            }
        }
        private void DeviceDelCommandFunc()
        {
            if (SelectedConfig == null)
            {
                return;
            }
            if (SelectedConfig.Name == CognexProcessDevParameter.ManualConfigName)
            {
                return;
            }
            if (SelectedConfig.Name == CognexProcessDevParameter.DefaultConfigName)
            {
                return;
            }

            if (DeviceList.Count < 2)
            {
                return;
            }
            DeviceConfigDic.Remove(SelectedConfig.Name);
            _CognexProcessManager.CognexProcDevParam.ConfigList.Remove((CognexConfig)SelectedConfig.CommandArg);

            DeviceList.Remove(SelectedConfig);
            SelectedConfig = DeviceConfigDic[DeviceList.First().Name].First();
            ConfigList = DeviceConfigDic[DeviceList.First().Name];
            RaisePropertyChanged(nameof(ConfigList));
            RaisePropertyChanged(nameof(DeviceList));
            _CognexProcessManager.SaveConfig();
        }
        #endregion


        #region ==> ConfigAddCommand
        private RelayCommand _ConfigAddCommand;
        public ICommand ConfigAddCommand
        {
            get
            {
                if (null == _ConfigAddCommand) _ConfigAddCommand = new RelayCommand(ConfigAddCommandFunc);
                return _ConfigAddCommand;
            }
        }

        private void ConfigAddCommandFunc()
        {
            if (String.IsNullOrEmpty(DeviceName))
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(DeviceName))
            {
                return;
            }

            OCRConfig newConfig = new OCRConfig();

            string newConfigName = "Config0";
            int newConfigNum = 1;
            while (true)
            {
                if (null == ConfigList.Where(x => x.Name == newConfigName).FirstOrDefault())
                {
                    break;
                }

                newConfigName = "Config" + newConfigNum.ToString();
                newConfigNum++;
            }

            //VmGPComboBoxItem newConfigItem = new VmGPComboBoxItem("Config" + ConfigList.Count(), newConfig);
            VmGPComboBoxItem newConfigItem = new VmGPComboBoxItem(newConfigName, newConfig);
            OCRDevParam.ConfigList.Add(newConfig);
            SelectedConfig = newConfigItem;

            ConfigList.Add(newConfigItem);
            if (this._LoaderModule.OCRConfig.ConfigList == null)
            {
                this._LoaderModule.OCRConfig.ConfigList = new List<OCRConfig>();
            }
            this._LoaderModule.OCRConfig.ConfigList.Add(newConfig);
            RaisePropertyChanged(nameof(ConfigList));
            this.SaveOCRDevParam();

        }

        //private void ConfigAddCommandFunc()
        //{
        //    if (String.IsNullOrEmpty(DeviceName))
        //    {
        //        return;
        //    }
        //    if (String.IsNullOrWhiteSpace(DeviceName))
        //    {
        //        return;
        //    }
        //    //if (ConfigList.FirstOrDefault(item => item.Name == DeviceName) != null)
        //    //{
        //    //    return;
        //    //}

        //    VmGPComboBoxItem newConfigItem = null;

        //    OCRConfig newConfig = new OCRConfig();
        //    for (int config = 0; config < ConfigList?.Count; config++)
        //    {
        //        if (!ConfigList.Any(e => e.Name == "Config" + config))
        //        {
        //            newConfigItem = new VmGPComboBoxItem("Config" + config, newConfig);
        //            break;
        //        }
        //    }
        //    if (newConfigItem == null)
        //    {
        //        newConfigItem = new VmGPComboBoxItem("Config" + ConfigList.Count(), newConfig);
        //    }

        //    OCRDevParam.ConfigList.Add(newConfig);
        //    SelectedConfig = newConfigItem;

        //    ConfigList.Add(newConfigItem);
        //    if (this._LoaderModule.OCRConfig.ConfigList == null)
        //    {
        //        this._LoaderModule.OCRConfig.ConfigList = new List<OCRConfig>();
        //    }
        //    this._LoaderModule.OCRConfig.ConfigList.Add(newConfig);
        //    RaisePropertyChanged(nameof(ConfigList));
        //    this.SaveOCRDevParam();
        //}
        #endregion

        #region ==> ConfigDelCommand
        private RelayCommand _ConfigDelCommand;
        public ICommand ConfigDelCommand
        {
            get
            {
                if (null == _ConfigDelCommand) _ConfigDelCommand = new RelayCommand(ConfigDelCommandFunc);
                return _ConfigDelCommand;
            }
        }
        private void ConfigDelCommandFunc()
        {
            if (SelectedConfig == null)
            {
                return;
            }

            if (ConfigList.Count < 2)
            {
                return;
            }
            var config = (OCRConfig)SelectedConfig.CommandArg;
            ConfigList.Remove(SelectedConfig);
            RaisePropertyChanged(nameof(ConfigList));
            OCRDevParam.ConfigList.Remove(config);
            SelectedConfig = ConfigList.First();
            this.SaveOCRDevParam();
        }
        #endregion
        #region ==> DeviceNameTextBoxClickCommand
        private RelayCommand _DevieTextBoxClickCommand;
        public ICommand DeiveTextBoxClickCommand
        {
            get
            {
                if (null == _DevieTextBoxClickCommand) _DevieTextBoxClickCommand = new RelayCommand(DeiveTextBoxClickCommandFunc);
                return _DevieTextBoxClickCommand;
            }
        }
        private void DeiveTextBoxClickCommandFunc()
        {
            String devName = VirtualKeyboard.Show(DeviceNameText, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL);

            if (String.IsNullOrEmpty(devName))
                return;
            DeviceNameText = devName;
            DeviceName = devName;
        }
        #endregion

        #region ==> FieldStringTextBoxClickCommand
        private RelayCommand _FieldStringTextBoxClickCommand;
        public ICommand FieldStringTextBoxClickCommand
        {
            get
            {
                if (null == _FieldStringTextBoxClickCommand) _FieldStringTextBoxClickCommand = new RelayCommand(FieldStringTextBoxClickCommandFunc);
                return _FieldStringTextBoxClickCommand;
            }
        }
        private void FieldStringTextBoxClickCommandFunc()
        {
            String fieldString = VirtualKeyboard.Show(FieldString, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL);

            if (String.IsNullOrEmpty(fieldString))
                return;

            FieldString = fieldString;
        }
        #endregion



        #region ==> RelAngle
        private double _RelAngle;
        public double RelAngle
        {
            get { return _RelAngle; }
            set
            {
                if (value != _RelAngle)
                {
                    _RelAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> AngleTextBoxClickCommand
        private RelayCommand _AngleTextBoxClickCommand;
        public ICommand AngleTextBoxClickCommand
        {
            get
            {
                if (null == _AngleTextBoxClickCommand) _AngleTextBoxClickCommand = new RelayCommand(AngleTextBoxClickCommandFunc);
                return _AngleTextBoxClickCommand;
            }
        }
        private void AngleTextBoxClickCommandFunc()
        {
            String angleString = VirtualKeyboard.Show(RelAngle.ToString(), KB_TYPE.DECIMAL);

            if (String.IsNullOrEmpty(angleString))
                return;
            double angle = 0;
            if (Double.TryParse(angleString, out angle))
            {

                RelAngle = angle % AbAngle;
            }
        }
        #endregion

        #region ==> AngleSettingTextBoxClickCommand
        private RelayCommand _AngleSettingTextBoxClickCommand;
        public ICommand AngleSettingTextBoxClickCommand
        {
            get
            {
                if (null == _AngleSettingTextBoxClickCommand) _AngleSettingTextBoxClickCommand = new RelayCommand(AngleSettingTextBoxClickCommandFunc);
                return _AngleSettingTextBoxClickCommand;
            }
        }
        private void AngleSettingTextBoxClickCommandFunc()
        {
            String angleString = VirtualKeyboard.Show(RelAngle.ToString(), KB_TYPE.DECIMAL);

            if (String.IsNullOrEmpty(angleString))
                return;
            double angle = 0;
            if (Double.TryParse(angleString, out angle))
            {

                OCRDevParam.OCRAngle = angle;
                RaisePropertyChanged(nameof(OCRDevParam));
                SaveOCRDevParam();
            }
        }
        #endregion
        #region ==> OCRScoreTextBoxClickCommand
        private RelayCommand _OCRScoreTextBoxClickCommand;
        public ICommand OCRScoreTextBoxClickCommand
        {
            get
            {
                if (null == _OCRScoreTextBoxClickCommand) _OCRScoreTextBoxClickCommand = new RelayCommand(OCRScoreTextBoxClickCommandFunc);
                return _OCRScoreTextBoxClickCommand;
            }
        }
        private void OCRScoreTextBoxClickCommandFunc()
        {
            String ocrScroeStr = VirtualKeyboard.Show(OCRCutLineScore.ToString(), KB_TYPE.DECIMAL);

            if (String.IsNullOrEmpty(ocrScroeStr))
            {
                return;
            }

            int ocrScroeInt;
            if (int.TryParse(ocrScroeStr, out ocrScroeInt) == false)
            {
                return;
            }

            OCRCutLineScore = ocrScroeInt;
            SaveOCRCutLineScoreConfig();
        }
        #endregion

        private void SetupModuleConfig()
        {
            try
            {
                ModuleIPList = new ObservableCollection<VmGPComboBoxItem>();
                DeviceList = new ObservableCollection<VmGPComboBoxItem>();
                ConfigList = new ObservableCollection<VmGPComboBoxItem>();
                String selectedConfigName = String.Empty;                
                for (int i = 0; i < _CognexProcessManager.CognexProcSysParam.CognexIPList.Count; ++i)
                {
                    String ip = _CognexProcessManager.CognexProcSysParam.CognexIPList[i].IP;


                    if (String.IsNullOrEmpty(ip))
                    {
                        continue;
                    }

                    if (_CognexProcessManager.ConnectNative(ip).Result == false)
                    {
                        LoggerManager.Debug($"CognexHost connect Failed");
                        continue;
                    }

                   

                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        VmGPComboBoxItem cognexHostItem = new VmGPComboBoxItem(ip, null);
                        ModuleIPList.Add(cognexHostItem);

                        if (_SelectedModuleIP == null)
                        {
                            _SelectedModuleIP = cognexHostItem;
                            RaisePropertyChanged(nameof(SelectedModuleIP));
                        }
                    }));
                }
                




                //foreach (CognexConfig config in _CognexProcessManager.CognexProcDevParam.ConfigList)
                //{
                //    if (String.IsNullOrEmpty(config.Name))
                //    {
                //        continue;
                //    }

                //    VmGPComboBoxItem devItem = new VmGPComboBoxItem(config.Name, config);

                //    if (!DeviceConfigDic.ContainsKey(config.Name))
                //        DeviceConfigDic.Add(config.Name, new ObservableCollection<VmGPComboBoxItem>());
                //    VmGPComboBoxItem configItem = new VmGPComboBoxItem("Config" + DeviceConfigDic[config.Name].Count(), config);

                //    if (config.Name == selectedConfigName)
                //    {
                //        _SelectedConfig = devItem;
                //        RaisePropertyChanged(nameof(SelectedConfig));
                //    }
                //    if (DeviceList.Where(i=>i.Name== config.Name).FirstOrDefault()==null)
                //    {
                //        DispatcherService.Invoke((System.Action)(() =>
                //        {
                //            DeviceList.Add(devItem);
                //        }));
                //    }
                //    DeviceConfigDic[config.Name].Add(configItem);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        
        /// <summary>
        /// 현재 PA에 있는 웨이퍼 기준으로 OCR Config 및 DeviceList 업데이트 
        /// </summary>
        private void SelectShowingDevice()
        {
            try
            {
                bool IsWaferExist = false;
                EnumWaferType prev_ShoingWaferType = ShowingWaferType;
                for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                {
                    //현재는 장비의 PA와 Cognex의 관계가 1:1이고 PA가 사용하는 CognexModule이 명시되어있지 않으므로 PA.Index = Conex.Index 으로 사용한다.
                    //SelectedModuleIndex 는 Combobox에 바인딩되어있어 0부터 시작하는 값으로 보인다.
                    IPreAlignModule pa = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, i + 1);
                    ICognexOCRModule cognex = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, i + 1);

                    if (pa == null)
                    {
                        LoggerManager.Debug($"[VmGPCognexOCRMainPage] GetOCRAccessParam(): PA({i + 1}) is null.");
                    }
                    else if (cognex == null)
                    {
                        LoggerManager.Debug($"[VmGPCognexOCRMainPage] GetOCRAccessParam(): COGNEX({i + 1}) is null.");
                    }
                    else
                    {
                        if (pa.Enable && pa.Holder.TransferObject != null)
                        {
                            IsWaferExist = true;
                            IsWafer = true;
                            SelectedTransferObject = pa.Holder.TransferObject;
                            SelectedModuleIndex = i;                           
                            break;
                        }
                    }
                }

                if (IsWaferExist)//Wafer가 있을 때만 업데이트한다.
                {
                    ShowingWaferType = SelectedTransferObject.WaferType.Value;
                    if(prev_ShoingWaferType != ShowingWaferType)
                    {
                        // ShowingWaferType이 변경되었으므로 DeviceList를 한번 더 업데이트 해준다.
                        initDeviceList();
                    }

                    if (SelectedTransferObject.WaferType.Value == EnumWaferType.POLISH)
                    {
                        var config = DeviceList.FirstOrDefault(cf => cf.Name == SelectedTransferObject.PolishWaferInfo.DefineName.Value);
                        DeviceName = SelectedTransferObject.PolishWaferInfo.DefineName.Value;
                        //LotidLength = OCRDevParam.lotIntegrity.Lotnamelength.ToString();// SelectedDevice에서 해주고 있음// OCRDevParam은 SelectedDevice가 바뀌었을때 LoadOCRDevParam()가 호출되면서 변경됨. 
                        //LotidDigit = OCRDevParam.lotIntegrity.LotnameDigit.ToString();
                        if (config != null)
                        {
                            DispatcherService.Invoke((System.Action)(() =>
                            {                                
                                SelectedDevice = config;
                            }));
                        }

                    }
                    else
                    {
                        var config = DeviceList.FirstOrDefault(cf => cf.Name == SelectedTransferObject.DeviceName.Value);
                        DeviceName = SelectedTransferObject.DeviceName.Value;
                        //LotidLength = OCRDevParam.lotIntegrity.Lotnamelength.ToString();
                        //LotidDigit = OCRDevParam.lotIntegrity.LotnameDigit.ToString();
                        if (config != null)
                        {
                            DispatcherService.Invoke((System.Action)(() =>
                            {
                                SelectedDevice = config;
                            }));
                        }

                    }

                }
                else
                {                    
                    // 없는 경우 그냥 List 중에 첫번째 Device 
                    if (DeviceList.Count > 0)
                    {
                        SelectedDevice = DeviceList.First();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return;
        }

        private EnumWaferType _ShowingWaferType = EnumWaferType.STANDARD;
        
        public EnumWaferType ShowingWaferType
        {
            get { return _ShowingWaferType; }
            set { _ShowingWaferType = value; RaisePropertyChanged(); }
        }


        /// <summary>
        /// Loader의 Devices 폴더 내용을 통해 DeviceList를 업데이트한다.
        /// "현재 보고 있는 Device"= SelectedDevice를 로더가 가지고 있는 디바이스의 첫번째 Device로 변경해준다.
        /// 만약 Pa에 웨이퍼가 있는 경우 GetOCRAccessParam()에 의해 SelectedDevice가 변경된다. 
        /// </summary>
        private void initDeviceList()
        {
            try
            {
                IDeviceManager devicemanager = _Container.Resolve<IDeviceManager>();

                if (devicemanager != null)
                {
                    //TODO: 현재 보고 있는 TransferObject의 Type에 따라서 다르게 DeviceList를 구성한다. 
                    // Type이 Standarad이면 이전과 동일하게 구성하고 웨이퍼가 있는 경우 
                    // Type이 Standarad이면 이전과 동일하게 구성하고 웨이퍼가 없는 경우 첫번째것 부터 나열한다. 
                    // Type이 Polish이면 이전과 동일하게 구성하고 웨이퍼가 없는 경우 
                    DispatcherService.Invoke((System.Action)(() =>
                    {
                        DeviceList.Clear();
                    }));

                    if (ShowingWaferType != EnumWaferType.POLISH)
                    {
                        var devpath = devicemanager.GetLoaderDevicePath();
                        if (Directory.Exists(devpath))
                        {
                            string[] dirs = Directory.GetDirectories(devpath);

                            foreach (var dir in dirs)
                            {
                                string dirName = new DirectoryInfo(dir).Name;

                                if (String.IsNullOrEmpty(dirName))
                                {
                                    return;
                                }
                                if (String.IsNullOrWhiteSpace(dirName))
                                {
                                    return;
                                }
                                if (DeviceList.FirstOrDefault(item => item.Name == dirName) != null)
                                {
                                    continue;
                                }


                                VmGPComboBoxItem newDevItem = new VmGPComboBoxItem(dirName, null);

                                DispatcherService.Invoke((System.Action)(() =>
                                {
                                    DeviceList.Add(newDevItem);
                                }));
                                RaisePropertyChanged(nameof(DeviceList));
                            }

                            // SelectShowingDevice()로 감.
                            //if (DeviceList.Count > 0)
                            //{
                            //    SelectedDevice = DeviceList.First();
                            //}
                        }
                        else
                            Directory.CreateDirectory(devpath);
                    }
                    else
                    {
                        var polishList = devicemanager.GetPolishWaferSources();
                        if (polishList.Count() > 0)
                        {
                            foreach (var item in polishList)
                            {
                                VmGPComboBoxItem newDevItem = new VmGPComboBoxItem(item.DefineName.Value, null);
                                DispatcherService.Invoke((System.Action)(() =>
                                {
                                    DeviceList.Add(newDevItem);
                                }));
                                RaisePropertyChanged(nameof(DeviceList));

                            }
                        }
                        else
                        {
                            var wafers =_LoaderModule.ModuleManager.GetTransferObjectAll().Where(w => w.CurrHolder.ModuleType == ModuleTypeEnum.PA || w.CurrHolder.ModuleType == ModuleTypeEnum.COGNEXOCR).ToList();
                            var waferOnlowestIndexPA = wafers.FirstOrDefault();
                            Task task = new Task(async () =>
                            {
                                await this.MetroDialogManager().ShowMessageDialog("[Cannot Set OCR]", $"There is Polish wafer On PA({waferOnlowestIndexPA.CurrHolder.Index}).\nNeed to setup Polish wafer Type. current setup information is null.", EnumMessageStyle.Affirmative);

                                await this.ViewModelManager().BackPreScreenTransition();
                            });
                            task.Start();


                        }


                    }


                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

       

        //private void SetupModuleConfig()
        //{
        //    try
        //    {
        //        ModuleIPList = new ObservableCollection<VmGPComboBoxItem>();
        //        DeviceList = new ObservableCollection<VmGPComboBoxItem>();
        //        ConfigList = new ObservableCollection<VmGPComboBoxItem>();
        //        String selectedConfigName = String.Empty;
        //        for (int i = 0; i < _CognexProcessManager.CognexProcDevParam.CognexHostList.Count; ++i)
        //        {
        //            CognexHost cognexHost = _CognexProcessManager.CognexProcDevParam.CognexHostList[i];
        //            String ip = _CognexProcessManager.CognexProcSysParam.GetIPOrNull(cognexHost.ModuleName);


        //            if (String.IsNullOrEmpty(ip))
        //            {
        //                continue;
        //            }

        //            if (_CognexProcessManager.ConnectNative(ip).Result == false)
        //            {
        //                LoggerManager.Debug($"CognexHost connect Failed");
        //                continue;
        //            }


        //            VmGPComboBoxItem cognexHostItem = new VmGPComboBoxItem(ip, cognexHost);
        //            ModuleIPList.Add(cognexHostItem);

        //            if (_SelectedModuleIP == null)
        //            {
        //                _SelectedModuleIP = cognexHostItem;
        //                RaisePropertyChanged(nameof(SelectedModuleIP));

        //                selectedConfigName = cognexHost.ConfigName;
        //            }
        //        }

        //        foreach (CognexConfig config in _CognexProcessManager.CognexProcDevParam.ConfigList)
        //        {
        //            if (String.IsNullOrEmpty(config.Name))
        //            {
        //                continue;
        //            }

        //            VmGPComboBoxItem devItem = new VmGPComboBoxItem(config.Name, config);

        //            if (!DeviceConfigDic.ContainsKey(config.Name))
        //                DeviceConfigDic.Add(config.Name, new ObservableCollection<VmGPComboBoxItem>());
        //            VmGPComboBoxItem configItem = new VmGPComboBoxItem("Config" + DeviceConfigDic[config.Name].Count(), config);

        //            if (config.Name == selectedConfigName)
        //            {
        //                _SelectedConfig = devItem;
        //                RaisePropertyChanged(nameof(SelectedConfig));
        //            }
        //            if (DeviceList.Where(i => i.Name == config.Name).FirstOrDefault() == null)
        //            {
        //                DispatcherService.Invoke((System.Action)(() =>
        //                {
        //                    DeviceList.Add(devItem);
        //                }));
        //            }
        //            DeviceConfigDic[config.Name].Add(configItem);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}
        private void LoadSelectedConfig()
        {
            try
            {
                if (SelectedConfig != null)
                {
                    OCRConfig selectedConfig = (OCRConfig)SelectedConfig.CommandArg;

                    //==> Image config set
                    VmGPComboBoxItem imageDirection = ImageDirectionList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.Direction);
                    if (imageDirection != null)
                    {
                        _SelectedImageDirection = imageDirection;
                        RaisePropertyChanged(nameof(SelectedImageDirection));
                        _CognexProcessManager.DO_SetConfigOrientation(SelectedModuleIP.Name, (String)_SelectedImageDirection.CommandArg);
                    }

                    //==> Mark config set
                    VmGPComboBoxItem mark = MarkList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.Mark);
                    if (mark != null)
                    {
                        _SelectedMark = mark;
                        RaisePropertyChanged(nameof(SelectedMark));
                        _CognexProcessManager.DO_SetConfigMark(SelectedModuleIP.Name, (String)_SelectedMark.CommandArg);
                    }

                    //==> Checksum config set
                    VmGPComboBoxItem checkSum = CheckSumList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.CheckSum);
                    if (checkSum != null)
                    {
                        _SelectedCheckSum = checkSum;
                        RaisePropertyChanged(nameof(SelectedCheckSum));
                        _CognexProcessManager.DO_SetConfigChecksum(SelectedModuleIP.Name, (String)_SelectedCheckSum.CommandArg);
                    }

                    //==> Retry Option Set
                    VmGPComboBoxItem retryOption = RetryOptionList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.RetryOption);
                    if (retryOption != null)
                    {
                        _SelectedRetryOption = retryOption;
                        RaisePropertyChanged(nameof(SelectedRetryOption));
                        _CognexProcessManager.DO_SetConfigRetry(SelectedModuleIP.Name, (String)_SelectedRetryOption.CommandArg);
                    }

                    //==> FieldString
                    _FieldString = selectedConfig.FieldString;
                    RaisePropertyChanged(nameof(FieldString));
                    _CognexProcessManager.DO_SetConfigFieldString(SelectedModuleIP.Name, FieldString);

                    //==> OCR Cutline Score
                    _OCRCutLineScore = selectedConfig.OCRCutLineScore;
                    RaisePropertyChanged(nameof(OCRCutLineScore));

                    //==> Region config set
                    _RegionY = selectedConfig.RegionY;
                    RaisePropertyChanged(nameof(RegionY));
                    _RegionX = selectedConfig.RegionX;
                    RaisePropertyChanged(nameof(RegionX));
                    _RegionHeight = selectedConfig.RegionHeight;
                    RaisePropertyChanged(nameof(RegionHeight));
                    _RegionWidth = selectedConfig.RegionWidth;
                    RaisePropertyChanged(nameof(RegionWidth));
                    _CognexProcessManager.DO_SetConfigRegion(SelectedModuleIP.Name, RegionY.ToString(), RegionX.ToString(), RegionHeight.ToString(), RegionWidth.ToString(), _RegionTheta.ToString(), _RegionPhi.ToString());

                    //==> Char region config Set
                    _CharY = selectedConfig.CharY;
                    RaisePropertyChanged(nameof(CharY));
                    _CharX = selectedConfig.CharX;
                    RaisePropertyChanged(nameof(CharX));
                    _CharHeight = selectedConfig.CharHeight;
                    RaisePropertyChanged(nameof(CharHeight));
                    _CharWidth = selectedConfig.CharWidth;
                    RaisePropertyChanged(nameof(CharWidth));
                    _CognexProcessManager.DO_SetConfigCharSize(SelectedModuleIP.Name, CharHeight.ToString(), CharWidth.ToString());

                    //==> Light mode config Set
                    VmGPComboBoxItem light = LightModeList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.Light);
                    if (light != null)
                    {
                        _SelectedLight = light;
                        RaisePropertyChanged(nameof(SelectedLight));
                        _CognexProcessManager.DO_SetConfigLightMode(SelectedModuleIP.Name, (String)_SelectedLight.CommandArg);
                    }

                    //==> Light intensity Config Set & update Image
                    LightIntensity = selectedConfig.LightIntensity;
                    UpdateLight();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SaveDirectionConfig()
        {
            if (SelectedConfig == null)
                return;

            String imageDirectionValue = (String)_SelectedImageDirection.CommandArg;
            ((OCRConfig)SelectedConfig.CommandArg).Direction = imageDirectionValue;
            this.SaveOCRDevParam();
        }
        private void SaveMarkConfig()
        {
            if (SelectedConfig == null)
                return;

            String markValue = (String)_SelectedMark.CommandArg;
            ((OCRConfig)SelectedConfig.CommandArg).Mark = markValue;
            this.SaveOCRDevParam();

        }
        private void SaveChecksumConfig()
        {
            if (SelectedConfig == null)
                return;

            String checksumValue = (String)_SelectedCheckSum.CommandArg;
            ((OCRConfig)SelectedConfig.CommandArg).CheckSum = checksumValue;
            this.SaveOCRDevParam();

        }
        private void SaveRetryOptionConfig()
        {
            if (SelectedConfig == null)
                return;

            String modeValue = (String)_SelectedRetryOption.CommandArg;
            ((OCRConfig)SelectedConfig.CommandArg).RetryOption = modeValue;
            this.SaveOCRDevParam();
        }
        private void SaveFieldStringConfig()
        {
            if (SelectedConfig == null)
                return;

            OCRConfig config = (OCRConfig)SelectedConfig.CommandArg;
            config.FieldString = FieldString;
            this.SaveOCRDevParam();
        }
        private void SaveOCRCutLineScoreConfig()
        {
            if (SelectedConfig == null)
                return;

            OCRConfig config = (OCRConfig)SelectedConfig.CommandArg;
            config.OCRCutLineScore = OCRCutLineScore;
            this.SaveOCRDevParam();
        }
        private void SaveRegionConfig()
        {
            if (SelectedConfig == null)
                return;

            OCRConfig config = (OCRConfig)SelectedConfig.CommandArg;
            config.RegionY = RegionY;
            config.RegionX = RegionX;
            config.RegionHeight = RegionHeight;
            config.RegionWidth = RegionWidth;
            this.SaveOCRDevParam();

        }
        private void SaveCharRegionConfig()
        {
            if (SelectedConfig == null)
                return;

            OCRConfig config = (OCRConfig)SelectedConfig.CommandArg;
            config.CharY = CharY;
            config.CharX = CharX;
            config.CharHeight = CharHeight;
            config.CharWidth = CharWidth;
            this.SaveOCRDevParam();

        }
        private void SaveLightModeConfig()
        {
            if (SelectedConfig == null)
                return;

            String lightModeValue = (String)_SelectedLight.CommandArg;
            ((OCRConfig)SelectedConfig.CommandArg).Light = lightModeValue;
            this.SaveOCRDevParam();

        }
        private void SaveLightIntensityConfig()
        {
            if (SelectedConfig == null)
                return;

            OCRConfig config = (OCRConfig)SelectedConfig.CommandArg;
            config.LightIntensity = LightIntensity;
            this.SaveOCRDevParam();

        }
        private void SaveModuleConfig()
        {
            CognexHost cognexHost = (CognexHost)_SelectedModuleIP.CommandArg;
            //cognexHost.ConfigName = SelectedConfig.Name;
            this.SaveOCRDevParam();
        }
        #endregion

        #region ====> GENERAL GROUP

        #region ==> ImageDirectionList
        private ObservableCollection<VmGPComboBoxItem> _ImageDirectionList;
        public ObservableCollection<VmGPComboBoxItem> ImageDirectionList
        {
            get { return _ImageDirectionList; }
            set
            {
                if (value != _ImageDirectionList)
                {
                    _ImageDirectionList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MarkList
        private ObservableCollection<VmGPComboBoxItem> _MarkList;
        public ObservableCollection<VmGPComboBoxItem> MarkList
        {
            get { return _MarkList; }
            set
            {
                if (value != _MarkList)
                {
                    _MarkList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CheckSumList
        private ObservableCollection<VmGPComboBoxItem> _CheckSumList;
        public ObservableCollection<VmGPComboBoxItem> CheckSumList
        {
            get { return _CheckSumList; }
            set
            {
                if (value != _CheckSumList)
                {
                    _CheckSumList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RetryOptionList
        private ObservableCollection<VmGPComboBoxItem> _RetryOptionList;
        public ObservableCollection<VmGPComboBoxItem> RetryOptionList
        {
            get { return _RetryOptionList; }
            set
            {
                if (value != _RetryOptionList)
                {
                    _RetryOptionList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedImageDirection
        private VmGPComboBoxItem _SelectedImageDirection;
        public VmGPComboBoxItem SelectedImageDirection
        {
            get { return _SelectedImageDirection; }
            set
            {
                if (value != _SelectedImageDirection)
                {
                    _SelectedImageDirection = value;
                    RaisePropertyChanged();

                    if (_SelectedImageDirection == null)
                        return;

                    _CognexProcessManager.DO_SetConfigOrientation(SelectedModuleIP.Name, (String)_SelectedImageDirection.CommandArg);
                    UpdateImage();
                    SaveDirectionConfig();
                }
            }
        }
        #endregion

        #region ==> SelectedMark
        private VmGPComboBoxItem _SelectedMark;
        public VmGPComboBoxItem SelectedMark
        {
            get { return _SelectedMark; }
            set
            {
                if (value != _SelectedMark)
                {
                    _SelectedMark = value;
                    RaisePropertyChanged();

                    if (_SelectedMark == null)
                        return;

                    _CognexProcessManager.DO_SetConfigMark(SelectedModuleIP.Name, (String)_SelectedMark.CommandArg);
                    SaveMarkConfig();
                }
            }
        }
        #endregion

        #region ==> SelectedCheckSum
        private VmGPComboBoxItem _SelectedCheckSum;
        public VmGPComboBoxItem SelectedCheckSum
        {
            get { return _SelectedCheckSum; }
            set
            {
                if (value != _SelectedCheckSum)
                {
                    _SelectedCheckSum = value;
                    RaisePropertyChanged();

                    if (_SelectedCheckSum == null)
                        return;

                    _CognexProcessManager.DO_SetConfigChecksum(SelectedModuleIP.Name, (String)_SelectedCheckSum.CommandArg);
                    SaveChecksumConfig();
                }
            }
        }
        #endregion

        #region ==> SelectedRetryOption
        private VmGPComboBoxItem _SelectedRetryOption;
        public VmGPComboBoxItem SelectedRetryOption
        {
            get { return _SelectedRetryOption; }
            set
            {
                if (value != _SelectedRetryOption)
                {
                    _SelectedRetryOption = value;
                    RaisePropertyChanged();

                    if (_SelectedRetryOption == null)
                        return;

                    _CognexProcessManager.DO_SetConfigRetry(SelectedModuleIP.Name, (String)_SelectedRetryOption.CommandArg);
                    SaveRetryOptionConfig();
                }
            }
        }
        #endregion

        #region ==> FieldString
        private String _FieldString;
        public String FieldString
        {
            get { return _FieldString; }
            set
            {
                if (value != _FieldString)
                {
                    _FieldString = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> OCRCutLineScore
        private int _OCRCutLineScore;
        public int OCRCutLineScore
        {
            get { return _OCRCutLineScore; }
            set
            {
                if (value != _OCRCutLineScore)
                {
                    _OCRCutLineScore = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> AutoFieldCommand
        private AsyncCommand _AutoFieldCommand;
        public ICommand AutoFieldCommand
        {
            get
            {
                if (null == _AutoFieldCommand) _AutoFieldCommand = new AsyncCommand(AutoFieldCommandFunc);
                return _AutoFieldCommand;
            }
        }
        private async Task AutoFieldCommandFunc()
        {
            try
            {
                //await this.WaitCancelDialogService().ShowDialog("OCR");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                int xStartLen = 29;
                StringBuilder defulatFieldString = new StringBuilder();

                while (xStartLen > 0)
                {
                    for (int i = 0; i < xStartLen; i++)
                        defulatFieldString.Append("X");

                    _CognexProcessManager.DO_SetConfigFieldString(SelectedModuleIP.Name, defulatFieldString.ToString());
                    _CognexProcessManager.DO_ReadConfig(SelectedModuleIP.Name, "1");

                    var data = _CognexProcessManager.
                                    CognexCommandManager.
                                    ReadConfig.
                                    OrcList.
                                    FirstOrDefault(item => Convert.ToInt32(item.Value) < 50);

                    if (data == null)
                        break;

                    xStartLen--;
                    defulatFieldString.Clear();
                }


                StringBuilder filedString = new StringBuilder();
                foreach (Orc orc in _CognexProcessManager.
                                    CognexCommandManager.
                                    ReadConfig.
                                    OrcList)
                {
                    int digit = Convert.ToInt32(orc.Value);
                    char ch = Convert.ToChar(digit);

                    if (ch == '-' || ch == '.' || ch == '#' || ch == ' ')
                        filedString.Append(ch);
                    else if ('A' <= ch && ch <= 'Z')
                        filedString.Append('A');
                    else if ('a' <= ch && ch <= 'z')
                        filedString.Append('A');
                    else if ('0' <= ch && ch <= '9')
                        filedString.Append('N');
                    else
                        filedString.Append('*');
                }
                if (filedString.Length < 1)
                {
                    FieldString = "XXXXXXXXXXXXXXX";//==> 'X' * 15
                }
                else
                {
                    FieldString = filedString.ToString();
                }

                SetFieldCommandFunc();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

        #region ==> SetFieldCommand
        private RelayCommand _SetFieldCommand;
        public ICommand SetFieldCommand
        {
            get
            {
                if (null == _SetFieldCommand) _SetFieldCommand = new RelayCommand(SetFieldCommandFunc);
                return _SetFieldCommand;
            }
        }
        private void SetFieldCommandFunc()
        {
            _CognexProcessManager.DO_SetConfigFieldString(SelectedModuleIP.Name, FieldString);
            SaveFieldStringConfig();
        }
        #endregion

        private void SetupGeneral()
        {
            try
            {
                if (SelectedModuleIP == null)
                {
                    return;
                }
                if (_CognexProcessManager.DO_GetConfigEx(SelectedModuleIP.Name) == false)
                    return;

                ImageDirectionList = new ObservableCollection<VmGPComboBoxItem>();
                ImageDirectionList.Add(new VmGPComboBoxItem("Normal", "0"));
                ImageDirectionList.Add(new VmGPComboBoxItem("Mirrored horizontally", "1"));
                ImageDirectionList.Add(new VmGPComboBoxItem("Flipped vertically", "2"));
                ImageDirectionList.Add(new VmGPComboBoxItem("Rotated 180 degrees", "3"));

                MarkList = new ObservableCollection<VmGPComboBoxItem>();
                //MarkList.Add(new VmComboBoxItem("BC, BC 412", "1"));
                //MarkList.Add(new VmComboBoxItem("BC, IBM 412", "2"));
                MarkList.Add(new VmGPComboBoxItem("Internal Use Only", "3"));
                MarkList.Add(new VmGPComboBoxItem("Chars, SEMI", "4"));
                MarkList.Add(new VmGPComboBoxItem("Chars, IBM", "5"));
                MarkList.Add(new VmGPComboBoxItem("Chars, Triple", "6"));
                MarkList.Add(new VmGPComboBoxItem("Chars, OCR-A ", "7"));
                MarkList.Add(new VmGPComboBoxItem("SEMI M1.15", "11"));

                /*
                 * Cognex의 CheckSum API를 사용하법을 모르기에 Manual Input Mode에서는 CheckSum 함수를 직접 사용한다.
                 * 구현된 CheckSum 함수는 SEMI와 IBM 뿐이기에 2 CheckSum만 제공한다.
                 */
                CheckSumList = new ObservableCollection<VmGPComboBoxItem>();
                CheckSumList.Add(new VmGPComboBoxItem("None", "0"));//==> Virtual
                                                                    //CheckSumList.Add(new VmComboBoxItem("SEMI", "1"));//==> SEMI
                CheckSumList.Add(new VmGPComboBoxItem("SEMI", "2"));//==> SEMI with Virtual
                                                                    //CheckSumList.Add(new VmComboBoxItem("BC 412 with Virtual", "3"));//==> BC 412 with Virtual
                CheckSumList.Add(new VmGPComboBoxItem("IBM 412", "4"));//==> IBM 412 with Virtual

                RetryOptionList = new ObservableCollection<VmGPComboBoxItem>();
                RetryOptionList.Add(new VmGPComboBoxItem("Not Adjust", "0"));
                RetryOptionList.Add(new VmGPComboBoxItem("Adjust", "1"));
                RetryOptionList.Add(new VmGPComboBoxItem("Adjust & Save", "2"));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #endregion

        //private void SetupUI()
        //{

        //    SetupModuleConfig();    
        //    initDeviceList(); // 기존 SetupModuleConfig 함수에 포함되어있던 내용
        //    SelectShowingDevice(); // 기존 SetupModuleConfig 함수에 포함되어있던 내용
        //    SetupLight();
        //    SetupGeneral();
        //    UpdateImage();
        //    LoadSelectedConfig();
        //}

        private bool UpdateImage()
        {
            try
            {
                if (SelectedModuleIP == null)
                {
                    return false;
                }
                _CognexProcessManager.DO_AcquireConfig(SelectedModuleIP.Name);
                if (_CognexProcessManager.DO_RI(SelectedModuleIP.Name) == false)
                {
                    return false;
                }

                if (_CognexProcessManager.CognexCommandManager.CognexRICommand.Status != "1")
                {
                    return false;
                }

                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ImageSource = _CognexProcessManager.CognexCommandManager.CognexRICommand.GetBitmapImage();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return true;
        }

        private Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager;
        private IModuleManager _ModuleManager;
        //private String _CurrSubID;
        //private ModuleID _CurrPreAlignID;
        //private ModuleID _CurrOcrID;
        //private OCRModeEnum _CurrOCRMode;
        //private OCRAccessParam _OcrAccessParam;
        public bool Initialized { get; set; } = false;
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        readonly Guid _ViewModelGUID = new Guid("C0A0A12E-2504-4B18-83B3-39A51420781A");
        private ILoaderModule _LoaderModule;

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            if (Initialized)
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION");
                return EventCodeEnum.DUPLICATE_INVOCATION;
            }

            _Container = container;

            _CognexProcessManager = _Container.Resolve<ICognexProcessManager>();
            _ModuleManager = _Container.Resolve<IModuleManager>();
            _LoaderModule = _Container.Resolve<ILoaderModule>();
           
            Initialized = true;

            return retval;
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            InitModule(InSightDisplayApp.Container);

            if (_CognexProcessManager.IsInit())
            {
                SetupModuleConfig();
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                SetupModuleConfig();
                SetupLight();
                SetupGeneral();

                initDeviceList();// SelectShowingDevice()에서 DeviceList를 사용하기 때문에 먼저 불러줘야함.

                SelectShowingDevice();  // 현재 PA에 있는 웨이퍼 가지고 옴. WaferType 변경되었을 경우 DeviceList(STANDARD/POLISH) 업데이트한 후 SelectedDevice를 업데이트 해줌;                
                LoadSelectedConfig();   // SelectedDevice 정보대로 Config를 업데이트함.

                UpdateImage();
                GetOCRAccessParam();                
                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            if (SelectedTransferObject != null)
            {
                ShowingWaferType = EnumWaferType.STANDARD;// 
                IPreAlignModule paModule = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, SelectedModuleIndex + 1);
                if (paModule == null)
                {
                    return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
                }
                this.GetLoaderCommands().PAMove(paModule, SelectedTransferObject.SlotNotchAngle.Value);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
        }
        private TransferObject _SelectedTransferObject;

        public TransferObject SelectedTransferObject
        {
            get { return _SelectedTransferObject; }
            set {

                if (_SelectedTransferObject != value)
                {                                       
                    _SelectedTransferObject = value;
                    if (_SelectedTransferObject != null)
                    {
                        LoggerManager.Debug($"[VmGPCognexOCRMainPage] SelectedTransferObject Changed to DeviceName: {_SelectedTransferObject.DeviceName.Value}");
                    }
                }
            }
        }

        /// <summary>
        /// PA에 있는 웨이퍼를 찾아서 어떤 Cognex 모듈을 사용할 지, OCRPosition, DeviceName, SelectedDevice 를 변경해준다.
        /// PA1 번과 N번에 모두 웨이퍼가 있는 경우 숫자가 낮은 PA에 있는 웨이퍼가 표시된다. 
        /// </summary>
        /// <returns></returns>
        private OCRAccessParam GetOCRAccessParam()
        {

            try
            {
                bool IsWaferExist = false;
                for (int i = 0; i < SystemModuleCount.ModuleCnt.PACount; i++)
                {
                    //현재는 장비의 PA와 Cognex의 관계가 1:1이고 PA가 사용하는 CognexModule이 명시되어있지 않으므로 PA.Index = Conex.Index 으로 사용한다.
                    //SelectedModuleIndex 는 Combobox에 바인딩되어있어 0부터 시작하는 값으로 보인다.
                    IPreAlignModule pa = _ModuleManager.FindModule<IPreAlignModule>(ModuleTypeEnum.PA, i + 1);
                    ICognexOCRModule cognex = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, i + 1);

                    if (pa == null)
                    {
                        LoggerManager.Debug($"[VmGPCognexOCRMainPage] GetOCRAccessParam(): PA({i + 1}) is null.");
                    }
                    else if (cognex == null)
                    {
                        LoggerManager.Debug($"[VmGPCognexOCRMainPage] GetOCRAccessParam(): COGNEX({i + 1}) is null.");
                    }
                    else 
                    {
                        if(pa.Enable && pa.Holder.TransferObject != null)
                        {
                            IsWafer = true;
                            IsWaferExist = true;

                            SelectedTransferObject = pa.Holder.TransferObject;
                            SelectedModuleIndex = i;

                            //var config = DeviceList.FirstOrDefault(cf => cf.Name == SelectedTransferObject.DeviceName.Value);
                            //DeviceName = SelectedTransferObject.DeviceName.Value;
                            //LotidLength = OCRDevParam.lotIntegrity.Lotnamelength.ToString();
                            //LotidDigit = OCRDevParam.lotIntegrity.LotnameDigit.ToString();
                            //if (config != null)
                            //{
                            //    DispatcherService.Invoke((System.Action)(() =>
                            //    {
                            //        SelectedDevice = config;
                            //    }));
                            //}

                            SelectShowingDevice();

                            // 웨이퍼가 있는 경우 OCRPosition값을 리턴한다. 
                            ICognexOCRModule cognexOCRModule = _ModuleManager.FindModule<ICognexOCRModule>(ModuleTypeEnum.COGNEXOCR, SelectedModuleIndex + 1);
                            if (cognexOCRModule == null)
                            {
                                return null;
                            }
                            OCRPosition = cognexOCRModule.GetAccessParam(
                               SelectedTransferObject.Type.Value,
                                SelectedTransferObject.Size.Value);
                            
                            break;
                        }
                        else
                        {
                            IsWafer = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return OCRPosition;
        }
    }
    public static class DispatcherService
    {
        public static void Invoke(Action action)
        {
            Dispatcher dispatchObject = Application.Current != null ? Application.Current.Dispatcher : null;
            if (dispatchObject == null || dispatchObject.CheckAccess())
                action();
            else
                dispatchObject.Invoke(action);
        }
    }


}