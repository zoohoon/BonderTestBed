//#define GP_COGNEX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognexOCRMainPageViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using Autofac;
    using Cognex.Command.CognexCommandPack.GetInformation;
    using LoaderBase;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LoaderControllerBase;
    using LoaderParameters;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Enum;
    using RelayCommandBase;
    using VirtualKeyboardControl;

    public class VmCognexOCRMainPage : IMainScreenViewModel, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> WaferMoveToOCRCommand
        private AsyncCommand _WaferMoveToOCRCommand;
        public ICommand WaferMoveToOCRCommand
        {
            get
            {
                if (null == _WaferMoveToOCRCommand) _WaferMoveToOCRCommand = new AsyncCommand(WaferMoveToOCRCommandFunc);
                return _WaferMoveToOCRCommand;
            }
        }
        private async Task WaferMoveToOCRCommandFunc()
        {
            try
            {
                if (IsWaferOnOCR)
                {

                    this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is already exist on OCR Position", EnumMessageStyle.Affirmative);

                    return;
                }

                var paModuleInfo = _LoaderControllerExt
                    .LoaderInfo.StateMap.PreAlignModules
                    .Where(item => item.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();

                var ocrModuleInfo = _LoaderControllerExt
                    .LoaderInfo.StateMap.CognexOCRModules
                    .Where(item => item.ID.ModuleType == ModuleTypeEnum.COGNEXOCR).FirstOrDefault();

                //var ocrModuleInfo = _LoaderControllerExt
                //    .LoaderInfo.StateMap.SemicsOCRModules
                //    .Where(item => item.ID.ModuleType == ModuleTypeEnum.SEMICSOCR).FirstOrDefault();

                var armModuleInfo = _LoaderControllerExt
                    .LoaderInfo.StateMap.ARMModules
                    .Where(item => item.WaferStatus == EnumSubsStatus.NOT_EXIST && item.ID.Label == "ARM1").FirstOrDefault();

                if (paModuleInfo == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is not exist on Pre Aligner", EnumMessageStyle.Affirmative);

                    return;
                }

                if (ocrModuleInfo == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "OCR Module is not exist", EnumMessageStyle.Affirmative);

                    return;
                }

                if (armModuleInfo == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is exist on OCR Position", EnumMessageStyle.Affirmative);

                    return;
                }

                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                try
                {
                    //await this.WaitCancelDialogService().ShowDialog("Wait");
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    LoaderDeviceParameter deviceParamClone = _LoaderControllerExt.LoaderDeviceParam.Clone<LoaderDeviceParameter>();

                    //Move
                    bool isInjected = MoveToOCR(paModuleInfo, ocrModuleInfo, false, false);
                    //Wait
                    if (isInjected)
                    {
                        await Task.Run(() =>
                        {
                            retVal = this.LoaderController().WaitForCommandDone();
                            if (retVal == EventCodeEnum.NONE)
                            {

                            }
                        });

                    }

                    //while (true)
                    //{
                    //    System.Threading.Thread.Sleep(1000);
                    //    if (this.LoaderController().ModuleState.State == ModuleStateEnum.RUNNING)
                    //        continue;
                    //    break;
                    //}


                    IsWaferOnOCR = true;

                    UpdateImage();

                    _OcrAccessParam = GetOCRAccessParam();

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                    await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private bool MoveToOCR(HolderModuleInfo paModule, OCRModuleInfo ocrModule, bool isPerformOCR = false, bool isOverrideDevice = false, OCRDeviceBase overrideDevice = null)
        {
            bool isInjected = false;
            try
            {
                _CurrSubID = paModule.Substrate.ID.Value;
                _CurrPreAlignID = paModule.ID;
                _CurrOcrID = ocrModule.ID;

                string targetSubID = _CurrSubID;
                ModuleID destPos = _CurrOcrID;

                //=> Req to loader
                if (string.IsNullOrEmpty(targetSubID) == false &&
                    destPos.ModuleType == ModuleTypeEnum.COGNEXOCR &&
                    this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    LoaderMapEditor editor = _LoaderControllerExt.GetLoaderMapEditor();

                    OCRPerformOption ocrOption = null;
                    if (isPerformOCR)
                    {
                        ocrOption = new OCRPerformOption();
                        ocrOption.IsEnable.Value = true;
                        ocrOption.IsPerform.Value = true;
                    }

                    OCRDeviceOption ocrdeviceOption = null;
                    if (isOverrideDevice)
                    {
                        ocrdeviceOption = new OCRDeviceOption();
                        ocrdeviceOption.IsEnable.Value = true;
                        ocrdeviceOption.OCRDeviceBase = overrideDevice;
                    }

                    editor.EditorState.SetOCR(targetSubID, destPos, ocrOption, ocrdeviceOption);

                    TransferObject transfer = editor.EditMap.GetTransferObjectAll()
                        .Where(item => item.ID.Value == targetSubID)
                        .FirstOrDefault();

                    _CurrOCRMode = transfer.OCRMode.Value;
                    transfer.OCRMode.Value = OCRModeEnum.NONE;

                    LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                    cmdParam.Editor = editor;
                    isInjected = this.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isInjected;
        }
        private OCRAccessParam GetOCRAccessParam()
        {
            List<ICognexOCRModule> cognexOCRModules = _ModuleManager.FindModules<ICognexOCRModule>();
            if (cognexOCRModules == null || cognexOCRModules.Count < 1)
            {
                return null;
            }

            ICognexOCRModule cognexOCRModule = cognexOCRModules[_CognexHostIndex];
            if (cognexOCRModule == null)
            {
                return null;
            }

            List<IARMModule> armModules = _ModuleManager.FindModules<IARMModule>();
            if (armModules == null || armModules.Count < 1)
            {
                return null;
            }

            IARMModule armModule = armModules.FirstOrDefault(item => item.ID.Label == "ARM1");
            if (armModule == null)
            {
                return null;
            }

            OCRAccessParam accparam = cognexOCRModule.GetAccessParam(
                armModule.Holder.TransferObject.Type.Value,
                armModule.Holder.TransferObject.Size.Value);

            return accparam;
        }
        #endregion

        #region ==> WaferMoveToPACommand
        private AsyncCommand _WaferMoveToPACommand;
        public ICommand WaferMoveToPACommand
        {
            get
            {
                if (null == _WaferMoveToPACommand) _WaferMoveToPACommand = new AsyncCommand(WaferMoveToPACommandFunc);
                return _WaferMoveToPACommand;
            }
        }
        private async Task WaferMoveToPACommandFunc()
        {
            if (IsWaferOnOCR == false)
            {
                this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is already exist on pre aligner position", EnumMessageStyle.Affirmative);

                return;
            }

            var ocrModuleInfo = _LoaderControllerExt
                .LoaderInfo.StateMap.CognexOCRModules
                .Where(item => item.ID.ModuleType == ModuleTypeEnum.COGNEXOCR).FirstOrDefault();

            var paModuleInfo = _LoaderControllerExt
                .LoaderInfo.StateMap.PreAlignModules
                .Where(item => item.WaferStatus == EnumSubsStatus.NOT_EXIST).FirstOrDefault();

            var armModuleInfo = _LoaderControllerExt
                .LoaderInfo.StateMap.ARMModules
                .Where(item => item.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();

            if (ocrModuleInfo == null)
            {
                this.MetroDialogManager().ShowMessageDialog("Error", "OCR Module is not exist", EnumMessageStyle.Affirmative);

                return;
            }

            if (paModuleInfo == null)
            {
                this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is not exist on PA", EnumMessageStyle.Affirmative);

                return;
            }

            if (armModuleInfo == null)
            {
                this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is not exist on OCR", EnumMessageStyle.Affirmative);

                return;
            }

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                bool isInjected = MoveToPreAlign();

                if (isInjected == false)
                    return;

                await Task.Run(() =>
                {
                    retVal = this.LoaderController().WaitForCommandDone();
                });

                //while (true)
                //{
                //    System.Threading.Thread.Sleep(1000);
                //    if (this.LoaderController().ModuleState.State == ModuleStateEnum.RUNNING)
                //        continue;
                //    break;
                //}


                IsWaferOnOCR = false;

                UpdateImage();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        private bool MoveToPreAlign()
        {
            bool isInjected = false;
            try
            {

                //=> Find Source Substrate
                string targetSubID = _CurrSubID;

                //=> Find Desitination Pos
                ModuleID destPos = _CurrPreAlignID;

                if (string.IsNullOrEmpty(targetSubID) == false && destPos.ModuleType == ModuleTypeEnum.PA)
                {
                    if (this.LoaderController().ModuleState.GetState() == ModuleStateEnum.IDLE)
                    {
                        //=> Req to loader
                        var editor = _LoaderControllerExt.GetLoaderMapEditor();

                        OCRPerformOption ocrOption = new OCRPerformOption();
                        ocrOption.IsEnable.Value = true;
                        ocrOption.IsPerform.Value = false;

                        editor.EditorState.SetOcrToPreAlign(targetSubID, destPos, ocrOption);

                        TransferObject transfer = editor.EditMap.GetTransferObjectAll()
                            .Where(item => item.ID.Value == targetSubID)
                            .FirstOrDefault();

                        transfer.OCRMode.Value = _CurrOCRMode;

                        LoaderMapCommandParameter cmdParam = new LoaderMapCommandParameter();
                        cmdParam.Editor = editor;
                        isInjected = this.CommandManager().SetCommand<ILoaderMapCommand>(this, cmdParam);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return isInjected;
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
            FlatJogUAxisMove(500 * -1);
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
            FlatJogUAxisMove(500);
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
            await FlatJogWAxisMove(5);
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
            await FlatJogWAxisMove(5 * -1);
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
            try
            {
                await WaferMoveToPACommandFunc();
                RotateOCRAngle(100);//==> 1 degree
                await WaferMoveToOCRCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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
            try
            {
                await WaferMoveToPACommandFunc();
                RotateOCRAngle(-100);//==> 1 degree
                await WaferMoveToOCRCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
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

        #region ==> IsWaferOnOCR
        private bool _IsWaferOnOCR;
        public bool IsWaferOnOCR
        {
            get { return _IsWaferOnOCR; }
            set
            {
                if (value != _IsWaferOnOCR)
                {
                    _IsWaferOnOCR = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private void FlatJogUAxisMove(double value)
        {
            if (_OcrAccessParam == null)
            {
                return;
            }

            if (JogArmConditionCheck() == false)
            {
                return;
            }

            try
            {
                //this.WaitCancelDialogService().ShowDialog("Wait");
                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                //==> OCR Offset U 파라미터 변경
                CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
                config.UOffset += value;
                _CognexProcessManager.SaveConfig();

                //==> OCR Offset U 보정
                _OcrAccessParam.OCROffsetU = config.UOffset;
                _LoaderControllerExt.OCRUaxisRelMove(value);

                UpdateImage();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private async Task FlatJogWAxisMove(double value)
        {
            if (_OcrAccessParam == null)
            {
                return;
            }

            if (JogArmConditionCheck() == false)
            {
                return;
            }

            try
            {
                Task task = new Task(() =>
                {
                    //this.WaitCancelDialogService().ShowDialog("Wait");

                    //==> OCR Offset W 파라미터 변경
                    CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
                    config.WOffset += value;
                    _CognexProcessManager.SaveConfig();

                    //==> OCR Offset W 보정
                    _OcrAccessParam.OCROffsetW = config.WOffset;
                    _LoaderControllerExt.OCRWaxisRelMove(value);

                    UpdateImage();
                });
                task.Start();
                await task;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void RotateOCRAngle(int ocrRotateAngle)
        {
            if (_OcrAccessParam == null)
            {
                return;
            }

            try
            {
                //this.WaitCancelDialogService().ShowDialog("Wait");
                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                //_OcrAccessParam.VPos.Value += ocrRotateAngle;
                //this.LoaderController().SaveSystemParam();

                //==> OCR Offset V 파라미터 변경
                CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
                config.AngleOffset += ocrRotateAngle;
                _CognexProcessManager.SaveConfig();

                //==> OCR Offset V 보정
                _OcrAccessParam.OCROffsetV = config.AngleOffset;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        private bool JogArmConditionCheck()
        {
            bool retval = false;

            try
            {
                var ocrModuleInfo = _LoaderControllerExt
                    .LoaderInfo.StateMap.CognexOCRModules
                    .Where(item => item.ID.ModuleType == ModuleTypeEnum.COGNEXOCR).FirstOrDefault();

                var armModuleInfo = _LoaderControllerExt
                    .LoaderInfo.StateMap.ARMModules
                    .Where(item => item.WaferStatus == EnumSubsStatus.EXIST).FirstOrDefault();

                if (ocrModuleInfo == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "OCR Module is not exist", EnumMessageStyle.Affirmative);

                    retval = false;
                }

                if (armModuleInfo == null)
                {
                    this.MetroDialogManager().ShowMessageDialog("Error", "Wafer is not exist on Arm", EnumMessageStyle.Affirmative);

                    retval = false;
                }

                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = false;
            }

            return retval;
        }
        public void MoveLoaderAxis(EnumAxisConstants axis, int val)
        {
            try
            {

                this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                var vAxis = this.MotionManager().LoaderAxes.ProbeAxisProviders.FirstOrDefault(item => item.AxisType.Value == axis);
                if (vAxis == null)
                {
                    UpdateImage();
                    
                    return;
                }

                double apos = 0;
                this.MotionManager().GetActualPos(vAxis.AxisType.Value, ref apos);
                double pos = Math.Abs(val);
                if (pos + apos < vAxis.Param.PosSWLimit.Value)
                {
                    this.MotionManager().RelMove(
                        vAxis,
                        pos,
                        vAxis.Param.Speed.Value,
                        vAxis.Param.Acceleration.Value);
                }
                else
                {
                    //Sw limit
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        #endregion

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
            CognexConfig selectedConfig = (CognexConfig)SelectedConfig.CommandArg;

            try
            {
                //await this.WaitCancelDialogService().ShowDialog("Wait");
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");
                String ocrScoreStr = String.Empty;
                switch (selectedConfig.RetryOption)
                {
                    case "1":
                    case "2":
                        //==> Adjust 수행
                        OCRString = _CognexProcessManager.GetOCRString(SelectedModuleIP.Name, true, out ocrScoreStr);
                        break;
                    default:
                    case "0":
                        //==> Adjust 안함
                        OCRString = _CognexProcessManager.GetOCRString(SelectedModuleIP.Name, false, out ocrScoreStr);
                        break;
                }

                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());

                OCRString = _CognexProcessManager.CognexCommandManager.ReadConfig.String;

                _CognexProcessManager.DO_GetConfigEx(SelectedModuleIP.Name);

                UpdateLightUI();
                UpdateGeneralUI();
                UpdateRegionField();
                UpdateCharRegionField();
                UpdateImage();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
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
            }
        }
        #endregion

        #endregion

        #region ====> LIGHT GROUP

        #region ==> LightModeList
        private ObservableCollection<VmComboBoxItem> _LightModeList;
        public ObservableCollection<VmComboBoxItem> LightModeList
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
        private VmComboBoxItem _SelectedLight;
        public VmComboBoxItem SelectedLight
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
            try
            {
                UpdateLight();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightNextTypeCommand
        private AsyncCommand _LightNextTypeCommand;
        public ICommand LightNextTypeCommand
        {
            get
            {
                if (null == _LightNextTypeCommand) _LightNextTypeCommand = new AsyncCommand(LightNextTypeCommandFunc);
                return _LightNextTypeCommand;
            }
        }
        private async Task LightNextTypeCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    int idx = LightModeList.IndexOf(SelectedLight);
                    idx++;

                    if (idx > LightModeList.Count - 1)
                        return;

                    SelectedLight = LightModeList[idx];
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightPrevTypeCommand
        private AsyncCommand _LightPrevTypeCommand;
        public ICommand LightPrevTypeCommand
        {
            get
            {
                if (null == _LightPrevTypeCommand) _LightPrevTypeCommand = new AsyncCommand(LightPrevTypeCommandFunc);
                return _LightPrevTypeCommand;
            }
        }
        private async Task LightPrevTypeCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    int idx = LightModeList.IndexOf(SelectedLight);
                    idx--;

                    if (idx < 0)
                        return;

                    SelectedLight = LightModeList[idx];
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightPowerIncreaseCommand
        private AsyncCommand _LightPowerIncreaseCommand;
        public ICommand LightPowerIncreaseCommand
        {
            get
            {
                if (null == _LightPowerIncreaseCommand) _LightPowerIncreaseCommand = new AsyncCommand(LightPowerIncreaseCommandFunc);
                return _LightPowerIncreaseCommand;
            }
        }
        private async Task LightPowerIncreaseCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (LightIntensity + 1 > _MaxLightIntensity)
                    return;

                    LightIntensity += 1;

                    UpdateLight();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> LightPowerDecreaseCommand
        private AsyncCommand _LightPowerDecreaseCommand;
        public ICommand LightPowerDecreaseCommand
        {
            get
            {
                if (null == _LightPowerDecreaseCommand) _LightPowerDecreaseCommand = new AsyncCommand(LightPowerDecreaseCommandFunc);
                return _LightPowerDecreaseCommand;
            }
        }
        private async Task LightPowerDecreaseCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (LightIntensity - 1 < 0)
                    return;

                    LightIntensity -= 1;

                    UpdateLight();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

                LightModeList = new ObservableCollection<VmComboBoxItem>();
                LightModeList.Add(new VmComboBoxItem("Mode0", "0"));
                LightModeList.Add(new VmComboBoxItem("Mode1", "1"));
                LightModeList.Add(new VmComboBoxItem("Mode2", "2"));
                LightModeList.Add(new VmComboBoxItem("Mode3", "3"));
                LightModeList.Add(new VmComboBoxItem("Mode4", "4"));
                LightModeList.Add(new VmComboBoxItem("Mode5", "5"));
                LightModeList.Add(new VmComboBoxItem("Mode6", "6"));
                LightModeList.Add(new VmComboBoxItem("Mode7", "7"));
                LightModeList.Add(new VmComboBoxItem("Mode8", "8"));
                LightModeList.Add(new VmComboBoxItem("Mode9", "9"));
                LightModeList.Add(new VmComboBoxItem("Mode10", "10"));
                LightModeList.Add(new VmComboBoxItem("Mode11", "11"));
                LightModeList.Add(new VmComboBoxItem("Custom", "12"));
                LightModeList.Add(new VmComboBoxItem("External", "13"));
                LightModeList.Add(new VmComboBoxItem("Expansion", "14"));
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
                //await this.WaitCancelDialogService().ShowDialog("Wait");
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

                if (bestScoreValue < 190)
                {
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
                        _CognexProcessManager.SaveConfig();
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

            CognexConfig selectedConfig = (CognexConfig)SelectedConfig.CommandArg;
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

            CognexConfig selectedConfig = (CognexConfig)SelectedConfig.CommandArg;
            selectedConfig.Direction = orientation;
            selectedConfig.Mark = mark;
            selectedConfig.CheckSum = checksum;
            selectedConfig.RetryOption = retryOption;
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

            CognexConfig selectedConfig = (CognexConfig)SelectedConfig.CommandArg;
            selectedConfig.RegionY = RegionY;
            selectedConfig.RegionX = RegionX;
            selectedConfig.RegionHeight = RegionHeight;
            selectedConfig.RegionWidth = RegionWidth;
        }
        private void UpdateCharRegionField()
        {
            CharHeight = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.CharHeight);
            CharWidth = Convert.ToDouble(_CognexProcessManager.CognexCommandManager.GetConfigEx.CharWidth);

            CognexConfig selectedConfig = (CognexConfig)SelectedConfig.CommandArg;
            selectedConfig.CharHeight = CharHeight;
            selectedConfig.CharWidth = CharWidth;

        }
        #endregion

        #endregion

        #region ====> CONFIG GROUP

        #region ==> ModuleIPList
        private ObservableCollection<VmComboBoxItem> _ModuleIPList;
        public ObservableCollection<VmComboBoxItem> ModuleIPList
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
        private VmComboBoxItem _SelectedModuleIP;
        public VmComboBoxItem SelectedModuleIP
        {
            get { return _SelectedModuleIP; }
            set
            {
                if (value == null)
                {
                    return;
                }

                if (value != _SelectedModuleIP)
                {
                    _SelectedModuleIP = value;
                    RaisePropertyChanged();

                    if (_SelectedModuleIP == null)
                    {
                        return;
                    }

                    CognexHost cognexHost = (CognexHost)_SelectedModuleIP.CommandArg;
                    foreach (VmComboBoxItem item in ConfigList)
                    {
                        CognexConfig cognexConfig = (CognexConfig)item.CommandArg;
                        String configName = cognexConfig.Name;
                        if (cognexHost.ConfigName == configName)
                        {
                            SelectedConfig = item;
                            break;
                        }
                    }

                }
            }
        }
        #endregion

        #region ==> ConfigList
        private ObservableCollection<VmComboBoxItem> _ConfigList;
        public ObservableCollection<VmComboBoxItem> ConfigList
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
        private VmComboBoxItem _SelectedConfig;
        public VmComboBoxItem SelectedConfig
        {
            get { return _SelectedConfig; }
            set
            {
                if (value == null)
                {
                    return;
                }

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
            if (String.IsNullOrEmpty(ConfigName))
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(ConfigName))
            {
                return;
            }
            if (ConfigList.FirstOrDefault(item => item.Name == ConfigName) != null)
            {
                return;
            }

            CognexConfig newConfig = new CognexConfig();
            newConfig.Name = ConfigName;
            newConfig.Direction = (String)SelectedImageDirection.CommandArg;
            newConfig.Mark = (String)SelectedMark.CommandArg;
            newConfig.CheckSum = (String)SelectedCheckSum.CommandArg;
            newConfig.RetryOption = (String)SelectedRetryOption.CommandArg;
            newConfig.FieldString = FieldString;
            newConfig.OCRCutLineScore = OCRCutLineScore;
            newConfig.RegionY = RegionY;
            newConfig.RegionX = RegionX;
            newConfig.RegionHeight = RegionHeight;
            newConfig.RegionWidth = RegionWidth;
            newConfig.CharY = CharY;
            newConfig.CharX = CharX;
            newConfig.CharHeight = CharHeight;
            newConfig.CharWidth = CharWidth;
            newConfig.Light = (String)_SelectedLight.CommandArg;
            newConfig.LightIntensity = LightIntensity;

            VmComboBoxItem newConfigItem = new VmComboBoxItem(newConfig.Name, newConfig);
            ConfigList.Add(newConfigItem);

            SelectedConfig = newConfigItem;

            _CognexProcessManager.CognexProcDevParam.ConfigList.Add(newConfig);
            _CognexProcessManager.SaveConfig();
        }
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
            if (SelectedConfig.Name == CognexProcessDevParameter.ManualConfigName)
            {
                return;
            }
            if (SelectedConfig.Name == CognexProcessDevParameter.DefaultConfigName)
            {
                return;
            }

            if (ConfigList.Count < 2)
            {
                return;
            }

            _CognexProcessManager.CognexProcDevParam.ConfigList.Remove((CognexConfig)SelectedConfig.CommandArg);

            ConfigList.Remove(SelectedConfig);
            SelectedConfig = ConfigList.First();
            _CognexProcessManager.SaveConfig();
        }
        #endregion

        #region ==> ConfigNameTextBoxClickCommand
        private RelayCommand _ConfigNameTextBoxClickCommand;
        public ICommand ConfigNameTextBoxClickCommand
        {
            get
            {
                if (null == _ConfigNameTextBoxClickCommand) _ConfigNameTextBoxClickCommand = new RelayCommand(ConfigNameTextBoxClickCommandFunc);
                return _ConfigNameTextBoxClickCommand;
            }
        }
        private void ConfigNameTextBoxClickCommandFunc()
        {
            String configName = VirtualKeyboard.Show(ConfigName, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET | KB_TYPE.SPECIAL);

            if (String.IsNullOrEmpty(configName))
                return;

            ConfigName = configName;
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

        private VmComboBoxItem _FirstModuleIP;
        private void SetupModuleConfig()
        {
            ModuleIPList = new ObservableCollection<VmComboBoxItem>();
            ConfigList = new ObservableCollection<VmComboBoxItem>();

            String selectedConfigName = String.Empty;
            for (int i = 0; i < _CognexProcessManager.CognexProcDevParam.CognexHostList.Count; ++i)
            {
                CognexHost cognexHost = _CognexProcessManager.CognexProcDevParam.CognexHostList[i];

                String ip = _CognexProcessManager.CognexProcSysParam.GetIPOrNull(cognexHost.ModuleName);

                if (String.IsNullOrEmpty(ip))
                {
                    continue;
                }

                if (_CognexProcessManager.ConnectNative(ip).Result == false)
                {
                    LoggerManager.Debug($"CognexHost connect Failed");
                    continue;
                }


                VmComboBoxItem cognexHostItem = new VmComboBoxItem(ip, cognexHost);
                ModuleIPList.Add(cognexHostItem);

                if (_SelectedModuleIP == null)
                {
                    _FirstModuleIP = _SelectedModuleIP;
                    _SelectedModuleIP = cognexHostItem;
                    RaisePropertyChanged(nameof(SelectedModuleIP));
                    selectedConfigName = cognexHost.ConfigName;
                }
            }


            foreach (CognexConfig config in _CognexProcessManager.CognexProcDevParam.ConfigList)
            {
                if (String.IsNullOrEmpty(config.Name))
                {
                    continue;
                }

                VmComboBoxItem configItem = new VmComboBoxItem(config.Name, config);
                if (config.Name == selectedConfigName)
                {
                    _SelectedConfig = configItem;
                    RaisePropertyChanged(nameof(SelectedConfig));
                }

                ConfigList.Add(configItem);
            }
        }
        private void LoadSelectedConfig()
        {
            if (SelectedConfig == null)
            {
                return;
            }
                
            CognexConfig selectedConfig = (CognexConfig)SelectedConfig.CommandArg;

            //==> Image config set
            VmComboBoxItem imageDirection = ImageDirectionList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.Direction);
            if (imageDirection != null)
            {
                _SelectedImageDirection = imageDirection;
                RaisePropertyChanged(nameof(SelectedImageDirection));
                _CognexProcessManager.DO_SetConfigOrientation(SelectedModuleIP.Name, (String)_SelectedImageDirection.CommandArg);
            }

            //==> Mark config set
            VmComboBoxItem mark = MarkList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.Mark);
            if (mark != null)
            {
                _SelectedMark = mark;
                RaisePropertyChanged(nameof(SelectedMark));
                _CognexProcessManager.DO_SetConfigMark(SelectedModuleIP.Name, (String)_SelectedMark.CommandArg);
            }

            //==> Checksum config set
            VmComboBoxItem checkSum = CheckSumList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.CheckSum);
            if (checkSum != null)
            {
                _SelectedCheckSum = checkSum;
                RaisePropertyChanged(nameof(SelectedCheckSum));
                _CognexProcessManager.DO_SetConfigChecksum(SelectedModuleIP.Name, (String)_SelectedCheckSum.CommandArg);
            }

            //==> Retry Option Set
            VmComboBoxItem retryOption = RetryOptionList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.RetryOption);
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
            VmComboBoxItem light = LightModeList?.FirstOrDefault(item => item.CommandArg.ToString() == selectedConfig.Light);
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
        private void SaveDirectionConfig()
        {
            if (SelectedConfig == null)
                return;

            String imageDirectionValue = (String)_SelectedImageDirection.CommandArg;
            ((CognexConfig)SelectedConfig.CommandArg).Direction = imageDirectionValue;
            _CognexProcessManager.SaveConfig();

        }
        private void SaveMarkConfig()
        {
            if (SelectedConfig == null)
                return;

            String markValue = (String)_SelectedMark.CommandArg;
            ((CognexConfig)SelectedConfig.CommandArg).Mark = markValue;
            _CognexProcessManager.SaveConfig();

        }
        private void SaveChecksumConfig()
        {
            if (SelectedConfig == null)
                return;

            String checksumValue = (String)_SelectedCheckSum.CommandArg;
            ((CognexConfig)SelectedConfig.CommandArg).CheckSum = checksumValue;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveRetryOptionConfig()
        {
            if (SelectedConfig == null)
                return;

            String modeValue = (String)_SelectedRetryOption.CommandArg;
            ((CognexConfig)SelectedConfig.CommandArg).RetryOption = modeValue;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveFieldStringConfig()
        {
            if (SelectedConfig == null)
                return;

            CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
            config.FieldString = FieldString;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveOCRCutLineScoreConfig()
        {
            if (SelectedConfig == null)
                return;

            CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
            config.OCRCutLineScore = OCRCutLineScore;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveRegionConfig()
        {
            if (SelectedConfig == null)
                return;

            CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
            config.RegionY = RegionY;
            config.RegionX = RegionX;
            config.RegionHeight = RegionHeight;
            config.RegionWidth = RegionWidth;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveCharRegionConfig()
        {
            if (SelectedConfig == null)
                return;

            CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
            config.CharY = CharY;
            config.CharX = CharX;
            config.CharHeight = CharHeight;
            config.CharWidth = CharWidth;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveLightModeConfig()
        {
            if (SelectedConfig == null)
                return;

            String lightModeValue = (String)_SelectedLight.CommandArg;
            ((CognexConfig)SelectedConfig.CommandArg).Light = lightModeValue;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveLightIntensityConfig()
        {
            if (SelectedConfig == null)
                return;

            CognexConfig config = (CognexConfig)SelectedConfig.CommandArg;
            config.LightIntensity = LightIntensity;
            _CognexProcessManager.SaveConfig();
        }
        private void SaveModuleConfig()
        {
            CognexHost cognexHost = (CognexHost)_SelectedModuleIP.CommandArg;
            cognexHost.ConfigName = SelectedConfig.Name;
            _CognexProcessManager.SaveConfig();
        }
        #endregion

        #region ====> GENERAL GROUP

        #region ==> ImageDirectionList
        private ObservableCollection<VmComboBoxItem> _ImageDirectionList;
        public ObservableCollection<VmComboBoxItem> ImageDirectionList
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
        private ObservableCollection<VmComboBoxItem> _MarkList;
        public ObservableCollection<VmComboBoxItem> MarkList
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
        private ObservableCollection<VmComboBoxItem> _CheckSumList;
        public ObservableCollection<VmComboBoxItem> CheckSumList
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
        private ObservableCollection<VmComboBoxItem> _RetryOptionList;
        public ObservableCollection<VmComboBoxItem> RetryOptionList
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
        private VmComboBoxItem _SelectedImageDirection;
        public VmComboBoxItem SelectedImageDirection
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
        private VmComboBoxItem _SelectedMark;
        public VmComboBoxItem SelectedMark
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
        private VmComboBoxItem _SelectedCheckSum;
        public VmComboBoxItem SelectedCheckSum
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
        private VmComboBoxItem _SelectedRetryOption;
        public VmComboBoxItem SelectedRetryOption
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
                //await this.WaitCancelDialogService().ShowDialog("Wait");
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
            if (_CognexProcessManager.DO_GetConfigEx(SelectedModuleIP?.Name) == false)
                return;

            ImageDirectionList = new ObservableCollection<VmComboBoxItem>();
            ImageDirectionList.Add(new VmComboBoxItem("Normal", "0"));
            ImageDirectionList.Add(new VmComboBoxItem("Mirrored horizontally", "1"));
            ImageDirectionList.Add(new VmComboBoxItem("Flipped vertically", "2"));
            ImageDirectionList.Add(new VmComboBoxItem("Rotated 180 degrees", "3"));

            MarkList = new ObservableCollection<VmComboBoxItem>();
            //MarkList.Add(new VmComboBoxItem("BC, BC 412", "1"));
            //MarkList.Add(new VmComboBoxItem("BC, IBM 412", "2"));
            MarkList.Add(new VmComboBoxItem("Internal Use Only", "3"));
            MarkList.Add(new VmComboBoxItem("Chars, SEMI", "4"));
            MarkList.Add(new VmComboBoxItem("Chars, IBM", "5"));
            MarkList.Add(new VmComboBoxItem("Chars, Triple", "6"));
            MarkList.Add(new VmComboBoxItem("Chars, OCR-A ", "7"));
            MarkList.Add(new VmComboBoxItem("SEMI M1.15", "11"));

            /*
             * Cognex의 CheckSum API를 사용하법을 모르기에 Manual Input Mode에서는 CheckSum 함수를 직접 사용한다.
             * 구현된 CheckSum 함수는 SEMI와 IBM 뿐이기에 2 CheckSum만 제공한다.
             */
            CheckSumList = new ObservableCollection<VmComboBoxItem>();
            CheckSumList.Add(new VmComboBoxItem("None", "0"));//==> Virtual
            //CheckSumList.Add(new VmComboBoxItem("SEMI", "1"));//==> SEMI
            CheckSumList.Add(new VmComboBoxItem("SEMI", "2"));//==> SEMI with Virtual
            //CheckSumList.Add(new VmComboBoxItem("BC 412 with Virtual", "3"));//==> BC 412 with Virtual
            CheckSumList.Add(new VmComboBoxItem("IBM 412", "4"));//==> IBM 412 with Virtual

            RetryOptionList = new ObservableCollection<VmComboBoxItem>();
            RetryOptionList.Add(new VmComboBoxItem("Not Adjust", "0"));
            RetryOptionList.Add(new VmComboBoxItem("Adjust", "1"));
            RetryOptionList.Add(new VmComboBoxItem("Adjust & Save", "2"));
        }
        #endregion

        private void SetupUI()
        {
            try
            {
                SetupModuleConfig();
                SetupLight();
                SetupGeneral();
                UpdateImage();
                LoadSelectedConfig();
            }
            catch (Exception ex)
            {
            }
        }
        private bool UpdateImage()
        {
            _CognexProcessManager.DO_AcquireConfig(SelectedModuleIP?.Name);

            if (_CognexProcessManager.DO_RI(SelectedModuleIP?.Name) == false)
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

            return true;
        }

        private Autofac.IContainer _Container;
        private ICognexProcessManager _CognexProcessManager;
        private IModuleManager _ModuleManager;
        private ILoaderControllerExtension _LoaderControllerExt;
        private String _CurrSubID;
        private ModuleID _CurrPreAlignID;
        private ModuleID _CurrOcrID;
        private OCRModeEnum _CurrOCRMode;
        private int _CognexHostIndex = 0;
        private OCRAccessParam _OcrAccessParam;
        public bool Initialized { get; set; } = false;
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        readonly Guid _ViewModelGUID = new Guid("242c19d4-1866-4501-9556-1bc1e7f9699e");

        public EventCodeEnum InitModule()
        {
            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return EventCodeEnum.NONE;
            }

            EventCodeEnum retval = EventCodeEnum.NONE;

            if (Initialized)
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                return EventCodeEnum.DUPLICATE_INVOCATION;
            }

            IsWaferOnOCR = false;

            _Container = this.LoaderController()?.GetLoaderContainer();

            if (_Container == null)
            {
                return EventCodeEnum.UNDEFINED;
            }

            _LoaderControllerExt = this.LoaderController() as ILoaderControllerExtension;
            _CognexProcessManager = _Container.Resolve<ICognexProcessManager>();
            _ModuleManager = _Container.Resolve<IModuleManager>();

            Initialized = true;

            return retval;
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }

            if(_CognexProcessManager != null)
            {
                if (_CognexProcessManager.IsInit())
                {
                    SetupUI();
                }
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            /*
             * TODO : PageSwitched 당시 SelectedModuleIP, SelectedConfig가 null 로 초기화 되는 현상 발생함.
             */

            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }

            try
            {
                if (_CognexProcessManager.IsInit())
                {
                    SetupUI();
                    SelectedModuleIP = _FirstModuleIP;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            if (SystemManager.SysteMode == SystemModeEnum.Multiple)
            {
                return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
            }

            if (IsWaferOnOCR)
            {
                WaferMoveToPACommandFunc().Wait();
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
    }
}