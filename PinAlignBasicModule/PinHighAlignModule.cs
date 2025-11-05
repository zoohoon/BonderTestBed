using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PinHighAlignModule
{
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PnpSetup;
    using RelayCommandBase;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using PnPControl;
    using ProberInterfaces.PinAlign;
    using PinAlignUserControl;
    using ProberErrorCode;
    using SinglePinAlign;
    using ProberInterfaces.State;
    using LogModule;
    using ProbeCardObject;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using MetroDialogInterfaces;
    using SerializerUtil;
    using System.Numerics;

    public class PinHighAlignModule : PNPSetupBase, ISetup, IProcessingModule, IRecovery, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("CF4DA650-9016-278A-3E19-4ACEE77A01B2");

        int CurPinIndex = 0;            // 현재 선택된 핀 번호
        int CurDutIndex = 0;
        int CurPinArrayIndex = 0;       // 현재 선택된 핀의 더트 데이터 상 인덱스 번호

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

        private bool IsInfo = true;
        public new List<object> Nodes { get; set; }
        public PinHighAlignModule()
        {
        }

        #region DutViewerProperties
        private double? _ZoomLevel;
        public new double? ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowGrid;
        public new bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPin;
        public new bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowPad;
        public new bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowCurrentPos;
        public new bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private EnumProberCam _CamType;
        //public EnumProberCam CamType
        //{
        //    get { return _CamType; }
        //    set
        //    {
        //        if (value != _CamType)
        //        {
        //            _CamType = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private bool? _EnableDragMap;
        public new bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ShowSelectedDut;
        public new bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged();
                }
            }
        }

        public new IStageSupervisor StageSupervisor
        {
            get { return this.StageSupervisor(); }
        }

        public new IVisionManager VisionManager
        {
            get { return this.VisionManager(); }
        }
        #endregion

        private AlginParamBase _Param;
        public AlginParamBase Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IPinData _PinData;
        public IPinData PinData
        {
            get { return _PinData; }
            set
            {
                if (value != _PinData)
                {
                    _PinData = value;

                    RaisePropertyChanged();
                }
            }
        }

        public new UserControlFucEnum UseUserControl { get; set; }
        private IFocusing _PinFocusModel;
        public IFocusing PinFocusModel
        {
            get
            {
                if (_PinFocusModel == null)
                {
                    _PinFocusModel = this.FocusManager().GetFocusingModel((this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.FocusingModuleDllInfo);
                }

                return _PinFocusModel;
            }
        }
        private IFocusParameter FocusParam => (this.PinAligner().PinAlignDevParam as PinAlignDevParameters)?.FocusParam;
        private PinAlignDevParameters PinAlignParam => this.PinAligner().PinAlignDevParam as PinAlignDevParameters;
        private PinAlignResultes AlignResult => (this.PinAligner().PinAlignInfo as PinAlignInfo)?.AlignResult;
        private PinAlignInfo AlignInfo => this.PinAligner().PinAlignInfo as PinAlignInfo;
        IProbeCardSysObject probeCardSysObject;
        private PinAlignProcVariables AlignProcInfo => (this.PinAligner().PinAlignInfo as PinAlignInfo)?.AlignProcInfo;
        private PinAlignResultViewModel PinAlignResultViewModel;
        public new double TargetRectangleLeft { get; set; }
        public new double TargetRectangleTop { get; set; }
        public new double TargetRectangleWidth { get; set; }
        public new double TargetRectangleHeight { get; set; }

        private bool DoingFocusing = false;
        private bool DoingOnepinAlign = false;

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

        //Don`t Touch...why?
        public void ClearState()
        {
            SubModuleState = new SubModuleIdleState(this);
        }
        private Task DoNext()
        {
            try
            {

                Next();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return Task.CompletedTask;
        }
        public void Next()
        {
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                if (TotalPinNum <= 0) return;

                if (CurPinIndex >= TotalPinNum - 1)
                {
                    CurPinIndex = 0;
                }
                else
                {
                    CurPinIndex++;
                }

                tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);

                if (tmpPinData == null)
                {
                    return;
                }

                CurDutIndex = tmpPinData.DutNumber.Value - 1;

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                if (CurPinArrayIndex < 0 || CurDutIndex < 0)
                {
                    return;
                }

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                }

                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value,
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value,
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)

                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value,
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value,
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);

                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }

                LoggerManager.Debug($"[PinHighAlignModule] Next() : Move to Next Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Task DoPrev()
        {
            try
            {
                Prev();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return Task.CompletedTask;
        }
        public void Prev()
        {
            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                if (TotalPinNum <= 0)
                {
                    return;
                }

                if (CurPinIndex <= 0)
                {
                    CurPinIndex = TotalPinNum - 1;
                }
                else
                {
                    CurPinIndex--;
                }

                tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);

                if (tmpPinData == null)
                {
                    return;
                }

                CurDutIndex = tmpPinData.DutNumber.Value - 1;

                // CurPinArrayIndex는 현재 선택된 실제 핀 번호이지만, 데이터 상으로는 더트별로 해당 핀이 들어가 있는 배열의 인덱스는 핀 번호하고는 다르다.
                // 따라서 배열상의 어레이로 변환해 주어야 한다.
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);

                if (CurDutIndex < 0 || CurPinArrayIndex < 0)
                {
                    return;
                }

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                }

                this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);

                if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);

                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }
                else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                {
                    this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);

                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.LightForTip)
                    {
                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                    }
                }

                LoggerManager.Debug($"[PinHighAlignModule] Prev() : Move to Previous Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private Task DoNextFailPin()
        {
            try
            {

                NextFailPin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return Task.CompletedTask;
        }
        public void NextFailPin()
        {
            bool isFoundFailPin = false;

            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;

                int i = 0;
                int j = 0;
                int k = 0;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                if (TotalPinNum <= 0)
                {
                    return;
                }

                for (i = CurPinIndex + 1; i <= TotalPinNum - 1; i++)
                {
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                    k = tmpPinData.DutNumber.Value - 1;

                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)
                    {
                        isFoundFailPin = true;

                        CurPinIndex = i;
                        CurPinArrayIndex = j;
                        CurDutIndex = k;

                        break;
                    }
                }

                if (isFoundFailPin == false)
                {
                    for (i = 0; i <= CurPinIndex - 1; i++)
                    {
                        tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                        j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                        k = tmpPinData.DutNumber.Value - 1;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)
                        {
                            isFoundFailPin = true;

                            CurPinIndex = i;
                            CurPinArrayIndex = j;
                            CurDutIndex = k;

                            break;
                        }
                    }
                }

                if (isFoundFailPin)
                {
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }

                    LoggerManager.Debug($"[PinHighAlignModule] NextFailPin() : Move to Next Fail Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "NextFailPin() : Error occured.");
            }
        }
        private Task DoPrevFailPin()
        {
            try
            {
                PrevFailPin();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return Task.CompletedTask;
        }
        public void PrevFailPin()
        {
            bool isFoundFailPin = false;

            try
            {
                int TotalPinNum = 0;
                IPinData tmpPinData;

                int i = 0;
                int j = 0;
                int k = 0;

                TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                for (i = CurPinIndex - 1; i >= 0; i--)
                {
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                    j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                    k = tmpPinData.DutNumber.Value - 1;

                    if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)
                    {
                        isFoundFailPin = true;

                        CurPinIndex = i;
                        CurPinArrayIndex = j;
                        CurDutIndex = k;

                        break;
                    }
                }

                if (isFoundFailPin == false)
                {
                    for (i = TotalPinNum - 1; i >= CurPinIndex + 1; i--)
                    {
                        tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);
                        j = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(i);
                        k = tmpPinData.DutNumber.Value - 1;

                        if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_FORCED_PASS &&
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_SKIP &&
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                            this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[k].PinList[j].Result.Value != PINALIGNRESULT.PIN_PASSED)
                        {
                            isFoundFailPin = true;

                            CurPinIndex = i;
                            CurPinArrayIndex = j;
                            CurDutIndex = k;

                            break;
                        }
                    }
                }

                if (isFoundFailPin)
                {
                    if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                    {
                        string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                        this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                    }

                    if (CurCam.CameraChannel.Type == EnumProberCam.PIN_HIGH_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinHighViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }
                    else if (CurCam.CameraChannel.Type == EnumProberCam.PIN_LOW_CAM)
                    {
                        this.StageSupervisor().StageModuleState.PinLowViewMove(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.X.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Y.Value, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].AbsPos.Z.Value);
                    }

                    LoggerManager.Debug($"[PinHighAlignModule] PrevFailPin() : Move to Previous Fail Pin #" + this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err + "PrevFailPin() : Error occured.");
            }
        }
        public SubModuleMovingStateBase MovingState { get; set; }
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[PinHighAlignModule] InitModule() : Init Module Start");
            try
            {
                if (Initialized == false)
                {
                    //AlignInfo = (PinAlignInfo)this.PinAligner().AlignInfo;

                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    SubModuleState = new SubModuleIdleState(this);

                    MovingState = new SubModuleStopState(this);

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
                LoggerManager.Debug($"[PinHighAlignModule] InitModule() : Init Module Error");
                LoggerManager.Exception(err);

                throw err;
            }
            LoggerManager.Debug($"[PinHighAlignModule] InitModule() : Init Module Done");
            return retval;
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }
        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum DoExecute()
        {
            EventCodeEnum errcode = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[PinHighAlignModule] DoExecute() : DoExecute Start");

            try
            {
                errcode = PinAlign(this.PinAligner().PinAlignSource);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            LoggerManager.Debug($"[PinHighAlignModule] DoExecute() : DoExecute Done");
            return errcode;
        }
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Pin High Alignment";

                retVal = InitPnpModuleStage_AdvenceSetting();
                retVal = InitLightJog(this);

                PinAlignResultViewModel = new PinAlignResultViewModel();

                AdvanceSetupView = PinAlignResultViewModel.DialogControl;
                AdvanceSetupViewModel = PinAlignResultViewModel;

                SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await InitSetup();
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);

                if (PnpManager.PnpLightJog.CurSelectedMag == CameraBtnType.High)
                {
                    TwoButton.IsEnabled = true;
                }
                else
                {
                    TwoButton.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                throw err;
            }

            return retVal;
        }
        public Task<EventCodeEnum> InitSetup()
        {
            LoggerManager.Debug($"[PinHighAlignModule] InitSetup() : InitSetup Start");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor().ProbeCardInfo.CheckValidPinParameters();

                //ref pin의 dut로 이동하기 위한 작업
                CurPinIndex = 0; //ref pin index = 0, number = 1
                CurPinArrayIndex = this.StageSupervisor().ProbeCardInfo.GetPinArrayIndex(CurPinIndex);
                IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);
                if (tmpPinData != null)
                    CurDutIndex = tmpPinData.DutNumber.Value - 1;
                else
                    CurDutIndex = 0;

                if (CurPinArrayIndex >= 0 && tmpPinData != null && CurDutIndex >= 0)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] InitSetup() : Move To Ref Pin");
                    this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value, tmpPinData.AbsPos.Z.Value);
                }
                else
                {
                    LoggerManager.Debug($"[PinHighAlignModule] InitSetup() : invalid ref pin position, Move To Center Position");
                    this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinDefaultHeight.Value);
                }


                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                this.PnPManager().RestoreLastLightSetting(CurCam);

                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE)
                {
                    AlignResult.EachPinResultes.Clear();
                }

                retVal = InitPNPSetupUI();
                retVal = InitLightJog(this);

                UseUserControl = UserControlFucEnum.DEFAULT;

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                MainViewTarget = DisplayPort;
                MiniViewTarget = ProbeCard;

                if (PinAlignParam.PinAlignMode.Value == PINALIGNMODE.EMUL)
                {
                    string imgpath = @"C:\ProberSystem\EmulImages\PinImage\" + (Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinNum.Value) % 5).ToString() + ".bmp";
                    this.VisionManager().LoadImageFromFileToGrabber(imgpath, EnumProberCam.PIN_HIGH_CAM);
                }

                // 현재 랏드 State가 IDLE인 경우, 핀 셋업 모드로 Align이 동작되어야 한다.
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE ||
                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    this.PinAligner().PinAlignSource = PINALIGNSOURCE.PIN_REGISTRATION;
                }

                ShowPad = false;
                ShowPin = true;
                EnableDragMap = true;
                ShowSelectedDut = false;
                ShowGrid = false;
                ZoomLevel = 5;
                ShowCurrentPos = true;

                this.PinAligner().StopDrawDutOverlay(CurCam);
                this.PinAligner().DrawDutOverlay(CurCam);

                SetStepSetupState();

                CurCam.UpdateOverlayFlag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            LoggerManager.Debug($"[PinHighAlignModule] InitSetup() : InitSetup Done");

            return Task.FromResult<EventCodeEnum>(retVal);
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum ret = EventCodeEnum.NONE;
            return ret;
        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[PinHighAlignModule] LoadDevParameter() : LoadDevParameter Start");

            try
            {
                FocusParam.FocusingAxis.Value = this.StageSupervisor().StageModuleState.PinViewAxis;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            LoggerManager.Debug($"[PinHighAlignModule] LoadDevParameter() : LoadDevParameter Done");

            return retval;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                this.StageSupervisor().LoadProberCard();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum ChangeResultViewButtons()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING)
                {
                    // 얼라인 결과가 존재하는 경우, 결과창을 볼 수 있는 버튼을 활성화시킨다.
                    if (AlignResult.EachPinResultes.Count > 0)
                    {
                        FourButton.IsEnabled = true;

                        // 얼라인 결과가 존재하고, Fail pin이 존재하는 경우, Fail pin을 쉽게 이동하면서 볼 수 있는 버튼을 활성화시킨다.

                        bool isExistFailPin = AlignResult.EachPinResultes.Any(x => (x.PinResult != PINALIGNRESULT.PIN_PASSED) &&
                                                                                   (x.PinResult != PINALIGNRESULT.PIN_FORCED_PASS) &&
                                                                                   (x.PinResult != PINALIGNRESULT.PIN_SKIP));
                        if (isExistFailPin == true)
                        {
                            PadJogRightDown.IsEnabled = true;
                            PadJogLeftDown.IsEnabled = true;
                        }
                        else
                        {
                            PadJogRightDown.IsEnabled = false;
                            PadJogLeftDown.IsEnabled = false;
                        }
                    }
                    else
                    {
                        FourButton.IsEnabled = false;

                        PadJogRightDown.IsEnabled = false;
                        PadJogLeftDown.IsEnabled = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Header = "Pin High Alignment";

                MainViewTarget = DisplayPort;

                ProcessingType = EnumSetupProgressState.IDLE;

                PadJogLeftUp.Caption = "Prev";
                PadJogRightUp.Caption = "Next";

                PadJogSelect.Caption = "Update";

                PadJogLeftUp.Command = new AsyncCommand(DoPrev);
                PadJogRightUp.Command = new AsyncCommand(DoNext);

                PadJogSelect.Command = new AsyncCommand(DoSet);

                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
                PadJogLeft.IsEnabled = false;
                PadJogRight.IsEnabled = false;

                PadJogSelect.IsEnabled = true;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                #region PadJogLeftDown

                PadJogLeftDown.Caption = "Prev Fail";
                PadJogLeftDown.Command = new AsyncCommand(DoPrevFailPin);

                #endregion

                #region MyRegion

                PadJogRightDown.Caption = "Next Fail";
                PadJogRightDown.Command = new AsyncCommand(DoNextFailPin);

                #endregion

                #region OneButton

                OneButton.Visibility = System.Windows.Visibility.Visible;
                OneButton.Caption = "Align";
                OneButton.Command = Button1Command;

                #endregion

                #region TwoButton

                TwoButton.Visibility = System.Windows.Visibility.Visible;
                TwoButton.Caption = "Focusing";
                TwoButton.CaptionSize = 17;
                TwoButton.Command = Button2Command;

                #endregion

                #region ThreeButton

                ThreeButton.Visibility = System.Windows.Visibility.Visible;
                ThreeButton.Caption = "One Pin\nAlign";
                ThreeButton.CaptionSize = 19;
                ThreeButton.Command = Button3Command;

                #endregion

                #region FourButton

                FourButton.Visibility = System.Windows.Visibility.Visible;
                FourButton.Caption = "Result\nView";
                //FourButton.Command = ShowResultCommand;

                FourButton.Command = null;
                AdvanceSetupUISetting(FourButton);

                #endregion

                #region FiveButton

                FiveButton.Visibility = System.Windows.Visibility.Hidden;
                FiveButton.Caption = "";
                FiveButton.Command = null;

                #endregion

                PnpManager.PnpLightJog.HighBtnEventHandler = new RelayCommand(CameraHighButton);
                PnpManager.PnpLightJog.LowBtnEventHandler = new RelayCommand(CameraLowButton);

                ChangeResultViewButtons();

                // TODO : check logic
                if (this.PinAligner().IsRecoveryStarted == true)
                {
                    PadJogRightDown.IsEnabled = true;
                    PadJogLeftDown.IsEnabled = true;
                }
                else
                {
                    PadJogRightDown.IsEnabled = false;
                    PadJogLeftDown.IsEnabled = false;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command)
                {
                    _Button1Command = new AsyncCommand(Button1);
                }

                return _Button1Command;
            }
        }

        private async Task Button1()
        {
            try
            {
                AlignResult.EachPinResultes.Clear();
                AlignResult.TotalPassPinCount = 0;
                AlignResult.TotalFailPinCount = 0;
                AlignResult.TotalAlignPinCount = 0;

                this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                // 현재 랏드 State가 IDLE인 경우, 핀 셋업 모드로 Align이 동작되어야 한다.
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE ||
                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    this.PinAligner().PinAlignSource = PINALIGNSOURCE.PIN_REGISTRATION;
                }

                Task task = new Task(() =>
                {
                    PinAlign(this.PinAligner().PinAlignSource);
                });
                task.Start();
                await task;

                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                {
                    StringBuilder stb = new StringBuilder();
                    stb.Append("Pin Alignment is done successfully.");
                    stb.Append(System.Environment.NewLine);

                    stb.Append($"Source : {AlignResult.AlignSource}");
                    stb.Append(System.Environment.NewLine);

                    List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                    foreach (var result in eachPinResultsSortList)
                    {
                        stb.Append($" - Pin #{result.PinNum}. Shift = ");
                        stb.Append($"X: {result.DiffX,4:0.0}um, ");
                        stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                        stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                        stb.Append($" Height: {result.Height,7:0.0}um");
                        if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value == true)
                        {
                            stb.Append($", Tip Result = {result.PinTipOptResult}");
                        }
                        stb.Append(System.Environment.NewLine);
                    }

                    await this.MetroDialogManager().ShowMessageDialog("Information", stb.ToString(), EnumMessageStyle.Affirmative);

                    this.GetParam_ProbeCard().PinSetupChangedToggle.DoneState = ElementStateEnum.DONE;
                }
                else if (this.StageSupervisor().ProbeCardInfo.GetAlignState() != AlignStateEnum.DONE)
                {
                    StringBuilder stb = new StringBuilder();
                    stb.Append($"Pin Alignment is failed.");
                    stb.Append(System.Environment.NewLine);

                    stb.Append($"Source : {AlignResult.AlignSource}");
                    stb.Append(System.Environment.NewLine);

                    stb.Append($"Reason : { this.PinAligner().ReasonOfError.GetLastEventMessage()}");

                    stb.Append(System.Environment.NewLine);

                    string FailDescription = this.PinAligner().MakeFailDescription();

                    if (FailDescription != string.Empty)
                    {
                        stb.Append(FailDescription);
                        stb.Append(System.Environment.NewLine);
                    }

                    if (AlignResult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE)
                    {
                        stb.Append($"Center Diff X: {AlignResult.CardCenterDiffX,4:0.0}, Y: {AlignResult.CardCenterDiffY,4:0.0}, Z:{AlignResult.CardCenterDiffZ,4:0.0}");
                        stb.Append(System.Environment.NewLine);
                    }

                    List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                    foreach (var result in eachPinResultsSortList)
                    {
                        stb.Append($" - Pin #{result.PinNum}. Shift = ");
                        stb.Append($"X: {result.DiffX,4:0.0}um, ");
                        stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                        stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                        stb.Append(System.Environment.NewLine);
                        stb.Append($"   Height: {result.Height,7:0.0}um");

                        if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value == true)
                        {
                            stb.Append($", Tip Result = {result.PinTipOptResult}");
                        }

                        stb.Append(System.Environment.NewLine);
                    }

                    await this.MetroDialogManager().ShowMessageDialog("Warning", stb.ToString(), EnumMessageStyle.Affirmative, "OK");
                }
                else
                {
                    EventCodeInfo lastevent = this.PinAligner().ReasonOfError.GetLastEventCode();
                    string failreason = lastevent?.Message;

                    await this.MetroDialogManager().ShowMessageDialog("Warning", $"Pin to Pad Alignment is failed\n{failreason}", EnumMessageStyle.Affirmative, "OK");
                }

                ChangeResultViewButtons();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PinHighAlignModule - PinAlign() : Error occured.");
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                this.PinAligner().DrawDutOverlay(CurCam);
            }
        }

        #endregion

        #region pnp button 2
        private AsyncCommand _Button2Command;
        public ICommand Button2Command
        {
            get
            {
                if (null == _Button2Command)
                {
                    _Button2Command = new AsyncCommand(DoFocusing);
                }

                return _Button2Command;
            }
        }

        private async Task DoFocusing()
        {
            try
            {


                if (!DoingFocusing)
                {
                    DoingFocusing = true;
                    await Focusing();
                    DoingFocusing = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PinHighAlignModule - DoFocusing() : Error occured.");
            }
        }
        private async Task Focusing()
        {
            try
            {
                if (FocusParam.FocusingCam.Value == EnumProberCam.UNDEFINED)
                {
                    FocusParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;
                }

                FocusParam.FocusingCam.Value = CurCam.GetChannelType();

                if (FocusParam.FocusingAxis.Value != EnumAxisConstants.PZ)
                {
                    FocusParam.FocusingAxis.Value = EnumAxisConstants.PZ;
                }

                ICamera cam = this.VisionManager().GetCam(FocusParam.FocusingCam.Value);

                int OffsetX = cam.Param.GrabSizeX.Value / 2 - Convert.ToInt32(Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width)) / 2;
                int OffsetY = cam.Param.GrabSizeY.Value / 2 - Convert.ToInt32(Convert.ToInt32(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height)) / 2;

                FocusParam.FocusingROI.Value = new System.Windows.Rect(OffsetX, OffsetY, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Width, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinSearchParam.SearchArea.Value.Height);

                FocusParam.FocusRange.Value = Convert.ToInt32(PinAlignParam.PinFocusingRange.Value);
                FocusParam.PeakRangeThreshold.Value = 100;

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                if (PinFocusModel.Focusing_Retry(FocusParam, false, false, false, this) != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Fail", "Focusing Fail", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Focusing Success", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CameraHighButton()
        {
            TwoButton.IsEnabled = true;
        }
        private void CameraLowButton()
        {
            TwoButton.IsEnabled = false;
        }
        #endregion

        #region pnp button 3
        private AsyncCommand _Button3Command;
        public ICommand Button3Command
        {
            get
            {
                if (null == _Button3Command)
                {
                    _Button3Command = new AsyncCommand(DoOnepinAlign);
                }

                return _Button3Command;
            }
        }



        private async Task DoOnepinAlign()
        {
            try
            {
                if (!DoingOnepinAlign)
                {
                    DoingOnepinAlign = true;
                    await OnePinAlign();
                    DoingOnepinAlign = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}. PinHighAlignModule - DoOnepinAlign() : Error occured.");
            }
        }
        private async Task OnePinAlign()
        {
            PINALIGNRESULT EachPinResult = PINALIGNRESULT.PIN_SKIP;

            PinCoordinate NewPinPos;

            try
            {
                this.VisionManager().StopGrab(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);

                IPinData tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(CurPinIndex);

                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                {
                    if (tmpPinData.PinSearchParam.AlignKeyHigh.Count > 0)
                    {
                        retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value,
                                                                                                         tmpPinData.AbsPos.Y.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value,
                                                                                                         tmpPinData.AbsPos.Z.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value);
                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[PinHighAlignModule] OnePinAling() Error : PinHighViewMove(), type = MEMS_Dual_AlignKey");
                        }
                    }
                    else
                    {
                        // Key 정보가 존재하지 않는 경우.
                        retval = EventCodeEnum.PIN_HIGH_KEY_INVAILD;

                        this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, "High Key information is invalid.", this.GetType().Name);
                    }
                }
                else if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.Cantilever_Standard)
                {
                    LoggerManager.Debug($"OnePinAlign() : pin #" + tmpPinData.PinNum.Value + $" Align Start,  " +
                                                $"position = ({tmpPinData.AbsPos.X.Value}, " +
                                                $"{tmpPinData.AbsPos.Y.Value}, " +
                                                $"{tmpPinData.AbsPos.Z.Value})");

                    retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value, tmpPinData.AbsPos.Z.Value);

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[PinHighAlignModule] OnePinAling() Error : PinHighViewMove(), type = Cantilever_Standard");
                    }
                }
                else if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.VerticalType)
                {
                    if (tmpPinData.PinSearchParam.BaseParam != null)
                    {
                        if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.BaseFocsuingEnable.Value == BaseFocusingEnable.ENABLE)
                        {
                            LoggerManager.Debug($"OnePinAlign() : pin #" + tmpPinData.PinNum.Value + $" Align Start,  " +
                                                $"position = ({tmpPinData.AbsPos.X.Value}, " +
                                                $"{tmpPinData.AbsPos.Y.Value}, " +
                                                $"{tmpPinData.AbsPos.Z.Value + tmpPinData.PinSearchParam.BaseParam.DistanceBaseAndTip})");

                            double BaseOffsetX = tmpPinData.PinSearchParam.BaseParam.BaseOffsetX;
                            double BaseOffsetY = tmpPinData.PinSearchParam.BaseParam.BaseOffsetY;
                            double DistanceBaseAndTip = tmpPinData.PinSearchParam.BaseParam.DistanceBaseAndTip;

                            retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value + BaseOffsetX, tmpPinData.AbsPos.Y.Value + BaseOffsetY, tmpPinData.AbsPos.Z.Value + DistanceBaseAndTip);

                            if (CurCam == null) CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                            if (((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams == null)
                            {
                                LoggerManager.Debug($"OnePinAlign() : Failed, LightParam is wrong.");

                                retval = EventCodeEnum.PIN_ALIGN_FAILED;
                            }
                            else
                            {
                                foreach (var light in ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                                {
                                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                }
                            }

                            if (retval != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[PinHighAlignModule] OnePinAling() Error : PinHighViewMove(), type = VerticalType");
                            }
                        }
                        else
                        {
                            // pin tip 위치로 이동
                            retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value, tmpPinData.AbsPos.Z.Value);

                            if (CurCam == null) CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                            if (((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams == null)
                            {
                                LoggerManager.Debug($"PinAlign() : Failed, LightParam is wrong.");

                                retval = EventCodeEnum.PIN_ALIGN_FAILED;
                            }
                            else
                            {
                                foreach (var light in ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                                {
                                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                }
                            }

                            if (retval != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[PinHighAlignModule] OnePinAling() Error : PinHighViewMove(), type = VerticalType");
                            }
                        }
                    }
                }

                if (retval == EventCodeEnum.NONE)
                {
                    EachPinResult = this.PinAligner().SinglePinAligner.SinglePinalign(out NewPinPos, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex], this.PinFocusModel, FocusParam);

                    // 기존의 If문을 Switch문으로 변경 함. 
                    switch (EachPinResult)
                    {
                        case PINALIGNRESULT.PIN_NOT_PERFORMED:
                            await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Failed", EnumMessageStyle.Affirmative);
                            break;
                        case PINALIGNRESULT.PIN_PASSED:
                            if (PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value == true)
                            {
                                switch (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinTipResult.Value)
                                {
                                    case PINALIGNRESULT.PIN_TIP_FOCUS_FAILED:
                                        await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Tip Align  Focusing Fail", EnumMessageStyle.Affirmative);
                                        break;
                                    case PINALIGNRESULT.PIN_BLOB_FAILED:
                                        await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Tip Align Blob Fail", EnumMessageStyle.Affirmative);
                                        break;
                                    case PINALIGNRESULT.PIN_PASSED:
                                        await this.MetroDialogManager().ShowMessageDialog("Success", "One Pin Tip Align Test Success", EnumMessageStyle.Affirmative);
                                        LoggerManager.Debug($"OnePinAlign() : position is updated ({NewPinPos.X.Value}, {NewPinPos.Y.Value}, {NewPinPos.Z.Value}),");
                                        break;
                                    default:
                                        await this.MetroDialogManager().ShowMessageDialog("Fail", $"One Pin Tip Align Error: {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[CurDutIndex].PinList[CurPinArrayIndex].PinTipResult.Value} ", EnumMessageStyle.Affirmative);
                                        break;
                                }
                            }
                            else
                            {
                                await this.MetroDialogManager().ShowMessageDialog("Success", "One Pin Align Test Success", EnumMessageStyle.Affirmative);
                                LoggerManager.Debug($"OnePinAlign() : position is updated ({NewPinPos.X.Value}, {NewPinPos.Y.Value}, {NewPinPos.Z.Value}),");
                            }
                            break;
                        case PINALIGNRESULT.PIN_SKIP:
                            break;
                        case PINALIGNRESULT.PIN_FORCED_PASS:
                            break;
                        case PINALIGNRESULT.PIN_FOCUS_FAILED:
                            await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Focusing Fail", EnumMessageStyle.Affirmative);
                            break;
                        case PINALIGNRESULT.PIN_TIP_FOCUS_FAILED:
                            await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Tip Focusing Fail", EnumMessageStyle.Affirmative);
                            break;
                        case PINALIGNRESULT.PIN_BLOB_FAILED:
                            await this.MetroDialogManager().ShowMessageDialog("Fail", "One Pin Align Blob Fail", EnumMessageStyle.Affirmative);
                            break;
                        case PINALIGNRESULT.PIN_OVER_TOLERANCE:
                            break;
                        case PINALIGNRESULT.PIN_BLOB_TOLERANCE:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);
            }
        }

        #endregion

        private async Task DoSet()
        {
            CatCoordinates curPos;

            double dist;
            double mindist = 300000.0;

            PinCoordinate refPos;

            int pinNum = 0;

            try
            {
                // 현재 위치에서 가장 가까운 핀을 찾는다.
                curPos = CurCam.GetCurCoordPos();
                refPos = new PinCoordinate(0, 0, 0);
                refPos.X.Value = curPos.X.Value;
                refPos.Y.Value = curPos.Y.Value;
                refPos.Z.Value = curPos.Z.Value;

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData cur_pin in dut.PinList)
                    {
                        dist = GetDistance2D(refPos, cur_pin.AbsPos);

                        if (mindist > dist)
                        {
                            mindist = dist;
                            pinNum = cur_pin.PinNum.Value;
                        }
                    }
                }

                if (mindist > 150)
                {
                    string message = "There is no pin near current position.\nYou must place cursor on a pin firstly.";
                    string caption = "Information";
                    await this.MetroDialogManager().ShowMessageDialog(caption, message, EnumMessageStyle.Affirmative);

                    return;
                }

                string message2 = $"Pin #{pinNum} position will be updated...\nDo you want to continue?";
                string caption2 = "Notice";

                EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog(caption2, message2, EnumMessageStyle.AffirmativeAndNegative);

                if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    UpdatePinPos(pinNum);

                    // 핀 데이터가 변경됐으니, 얼라인 데이터를 깨버리자.
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);

                    SetStepSetupState();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
                this.VisionManager().StartGrab(CurCam.GetChannelType(), this);
                CurCam.UpdateOverlayFlag = true;
            }
        }
        public void UpdatePinPos(int pinNum)
        {
            CatCoordinates curPos;

            double diffX;
            double diffY;
            double diffZ;

            try
            {
                // 단일 핀의 포지션 메뉴얼 업데이트
                curPos = CurCam.GetCurCoordPos();

                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (IPinData cur_pin in dut.PinList)
                    {
                        if (cur_pin.PinNum.Value == pinNum)
                        {
                            diffX = cur_pin.AbsPos.X.Value - curPos.X.Value;
                            diffY = cur_pin.AbsPos.Y.Value - curPos.Y.Value;
                            diffZ = cur_pin.AbsPos.Z.Value - curPos.Z.Value;

                            //cur_pin.AlignedOffset.X.Value -= diffX;
                            //cur_pin.AlignedOffset.Y.Value -= diffY;
                            //cur_pin.AlignedOffset.Z.Value -= diffZ;

                            cur_pin.AbsPosOrg.X.Value = cur_pin.AbsPosOrg.X.Value + cur_pin.AlignedOffset.X.Value - diffX;
                            cur_pin.AbsPosOrg.Y.Value = cur_pin.AbsPosOrg.Y.Value + cur_pin.AlignedOffset.Y.Value - diffY;
                            cur_pin.AbsPosOrg.Z.Value = cur_pin.AbsPosOrg.Z.Value + cur_pin.AlignedOffset.Z.Value - diffZ;

                            cur_pin.AlignedOffset.X.Value = 0;
                            cur_pin.AlignedOffset.Y.Value = 0;
                            cur_pin.AlignedOffset.Z.Value = 0;

                            cur_pin.MarkCumulativeCorrectionValue.X.Value = 0;
                            cur_pin.MarkCumulativeCorrectionValue.Y.Value = 0;
                            cur_pin.MarkCumulativeCorrectionValue.Z.Value = 0;

                            cur_pin.LowCompensatedOffset.X.Value = 0;
                            cur_pin.LowCompensatedOffset.Y.Value = 0;
                            cur_pin.LowCompensatedOffset.Z.Value = 0;

                            LoggerManager.Debug($"[PinHighAlignModule] UpdatePinPos() : Pin #{pinNum} position is updated manualy. Prev ({cur_pin.AbsPos.X.Value + diffX}, {cur_pin.AbsPos.Y.Value + diffY}, {cur_pin.AbsPos.Z.Value + diffZ})  Current ({cur_pin.AbsPos.X.Value}, {cur_pin.AbsPos.Y.Value}, {cur_pin.AbsPos.Z.Value})", isInfo: IsInfo);

                            //// 업데이트 하려고 하는 핀이, 첫 번째 더트의 첫 번째 핀인 경우 : 즉 Reference Pin 데이터인 경우
                            //// Pin Low 패턴 정보의 위치도 같이 업데이트 되어야 한다.

                            //if (dut.DutNumber == 1 && cur_pin.PinNum.Value == 1)
                            //{
                            //    if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                            //    {
                            //        PinLowAlignPatternInfo FirstPattInfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.FIRST);
                            //        PinLowAlignPatternInfo SecondPattInfo = PinAlignParam.PinLowAlignParam.Patterns.FirstOrDefault(x => x.PatternOrder.Value == PinLowAlignPatternOrderEnum.SECOND);

                            //        if (FirstPattInfo != null && SecondPattInfo != null)
                            //        {
                            //            LoggerManager.Debug($"[PinHightAlignModule]] UpdatePinPos() : First Pattern Info. (OLD) : ({FirstPattInfo.X.Value:0.0}, {FirstPattInfo.Y.Value:0.0}, {FirstPattInfo.Z.Value:0.0})");

                            //            FirstPattInfo.X.Value = FirstPattInfo.X.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].PinList[0].AbsPosOrg.X.Value;
                            //            FirstPattInfo.Y.Value = FirstPattInfo.Y.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].PinList[0].AbsPosOrg.Y.Value;
                            //            FirstPattInfo.Z.Value = FirstPattInfo.Z.Value;

                            //            LoggerManager.Debug($"[PinHightAlignModule]] UpdatePinPos() : First Pattern Info. (NEW) : ({FirstPattInfo.X.Value:0.0}, {FirstPattInfo.Y.Value:0.0}, {FirstPattInfo.Z.Value:0.0})");

                            //            LoggerManager.Debug($"[PinHightAlignModule]] UpdatePinPos() : Second Pattern Info. (NEW) : ({SecondPattInfo.X.Value:0.0}, {SecondPattInfo.Y.Value:0.0}, {SecondPattInfo.Z.Value:0.0})");

                            //            SecondPattInfo.X.Value = SecondPattInfo.X.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].PinList[0].AbsPosOrg.X.Value;
                            //            SecondPattInfo.Y.Value = SecondPattInfo.Y.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[0].PinList[0].AbsPosOrg.Y.Value;
                            //            SecondPattInfo.Z.Value = SecondPattInfo.Z.Value;

                            //            LoggerManager.Debug($"[PinHightAlignModule]] UpdatePinPos() : Second Pattern Info. (NEW) : ({SecondPattInfo.X.Value:0.0}, {SecondPattInfo.Y.Value:0.0}, {SecondPattInfo.Z.Value:0.0})");

                            //            LoggerManager.Debug($"[PinHightAlignModule]] UpdatePinPos() Pattern info. updated.");
                            //        }
                            //    }
                            //}

                            SaveProbeCardData();

                            return;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err + "Set() : Error occured.");
            }
        }
        private double GetDistance2D(PinCoordinate FirstPin, PinCoordinate SecondPin)
        {
            double Distance;

            Distance = Math.Sqrt(Math.Pow(FirstPin.GetX() - SecondPin.GetX(), 2) + Math.Pow(FirstPin.GetY() - SecondPin.GetY(), 2));

            return Distance;
        }
        #region Not 구현
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    this.PnPManager().GetPnpSteps(this.PinAligner());

                    this.ViewModelManager().ViewTransitionType(this.PnPManager());
                }
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;
        }

        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        #endregion
        private double Distance2D(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
        private bool MakeAlignList(PINALIGNSOURCE _PinAlignSource)
        {
            #region 핀그룹 설명

            /* 이 함수는 주어진 갯수만큼 얼라인을 수행하기 위한 최적의 핀을 각 그룹에서 골라 리스트로 만든다.
               AlignResult.RequiredAlignList  <== 여기에 데이터를 담는다.
               만약 필요한 만큼 핀이 존재하지 않으면 False를 리턴한다. (핀 얼라인 에러 발생)
               핀 얼라인 할 상황이 아닌데 불려도 False를 리턴한다.


               핀 고르는 로직

               1) 그룹별 할당량을 구한다.
               2) 리스트에서 앞쪽에 존재하는 그룹이 우선 순위를 가지고 먼저 배정된다. 
               3) 순서대로 앞쪽의 그룹부터 하나씩 꺼내어 할당량이 끝났으면 넘어가고 남았으면 해당 그룹의 리스트에서 핀 번호를 꺼내 리스트에 담는다.


               만약 이번 핀 얼라인에서 총 7개 핀 얼라인이 성공해야 한다면, 그룹이 4개 이므로 그룹별 할당량은 2 + 2 + 2 + 1이 된다.
               아래와 같이 핀 얼라인이 수행되다가 3번째 그룹에서 9번 핀에서 Fail이 발생하여 다시 들어왔다면

                   P  P  -  -         P  P  -        P  F  -   -         P   -   -    
               G1[ 1, 2, 3, 4 ]   G2[ 5, 6, 7 ]  G3[ 8, 9, 10, 11 ]  G4[ 12, 13, 14 ]

               앞의 그룹부터 순서대로
               1번 그룹은 할당량을 채웠으니 통과,
               2번 그룹도 할당량을 채웠으니 통과,
               3번 그룹은 할당량이 하나 남았으니 3번 그룹에서 다음 핀(10번)을 꺼내어 리스트에 담는다. 
               4번 그룹도 할당량을 채웠으니 통과

               결과적으로 리스트에는 10번 핀의 번호가 담긴다.
            */
            #endregion

            int total_required = 0;
            List<int> RequiredPerGroup = new List<int>();       // 그룹 별 할당량
            int group_count = 0;
            int cur_group = 0;
            int cur_applied = 0;
            int i = 0;
            int iPassed = 0;
            List<int> iRemained = new List<int>();
            int iPinNo = 0;

            //double max_dist = 0.0;
            List<double> PinAngleData = new List<double>();
            List<double> PinDistanceData = new List<double>();
            List<int> PinGroupData = new List<int>();
            int TotalPinNum = 0;
            List<int> PinNumList = new List<int>();
            List<IGroupData> tmpGroupList = new List<IGroupData>();
            IPinData tmpPinData;
            int iCount = 0;
            EachPinResult cur_pinResult;
            List<int> RequiredNumInGroup = new List<int>();
            List<int> GrouNumInList = new List<int>();
            List<IPinData> PinListData = new List<IPinData>();

            /* 
                주의 사항

                EachPinResult 데이터는 현재 진행중인 핀 얼라인에서 어떤 핀이 실패했고 어떤 핀이 남았는지를 판단하여 다음 핀을 고르기 위해 사용되며
                이번 얼라인이 성공해서 끝나거나 사용자에 의해 핀 얼라인이 중단되면 값이 초기화 된다.

                핀 데이터의 속성으로 있는 Result 값은 바로 이전 얼라인에서의 결과가 저장되며 이 값은 다음 핀 얼라인을 할 때 어떤 핀을 우선적으로 먼저
                핀 얼라인을 해 볼 것인지를 결정하는 척도로 사용된다. (Pass 핀을 먼저 얼라인 한다, 그 다음 Not performed - skip - fail 순이다.)

            */

            try
            {
                PinAlignSettignParameter CurrentPinSetting = PinAlignParam.PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == _PinAlignSource);

                if (CurrentPinSetting == null)
                {
                    LoggerManager.Error("[PinHighAlignModule] MakeAlignList() : PinAlignSettignParameter is null value.");

                    return false;
                }

                // 필요한 핀 얼라인 횟수 확인
                if (_PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION ||
                    _PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE ||
                    _PinAlignSource == PINALIGNSOURCE.CARD_CHANGE)
                {
                    // 핀 등록시에는 전체 핀을 돌린다.
                    total_required = this.StageSupervisor().ProbeCardInfo.GetPinCount() - (int)AlignResult.TotalPassPinCount;
                }
                else
                {
                    if (AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.CONTINUE)
                    {
                        total_required = CurrentPinSetting.SamplePinAlignmentPinCount.Value;
                    }
                    else if ((AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.DISABLED) ||
                             (AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.FAIL))
                    {
                        // Sample Pin Align Fail일 경우에 여기로 들어와서 total_required 인 핀의 개수를 반환함.
                        total_required = CurrentPinSetting.PinCount.Value - (int)AlignResult.TotalPassPinCount;
                    }
                    else
                    {
                        return false;   // 잘 못 들어온 케이스
                    }
                }

                if (group_count <= 1)
                {
                    // 그룹 지정이 되어 있지 않은 경우, 프로버가 임의로 핀을 고른다.
                    // 핀은 전체를 4분면으로 나누어 각 분면에서 골고루 고른다. (중심에서 멀리 있는 핀이 우선적으로 골라진다)

                    LoggerManager.Debug($"Pin Grouping Start");

                    LoggerManager.Debug($"―――――――――――――――――|");
                    LoggerManager.Debug($"                ｜                |");
                    LoggerManager.Debug($"       2Q       ｜       1Q       |");
                    LoggerManager.Debug($"                ｜                |");
                    LoggerManager.Debug($"――――――――｜――――――――|");
                    LoggerManager.Debug($"                ｜                |");
                    LoggerManager.Debug($"       3Q       ｜       4Q       |");
                    LoggerManager.Debug($"                ｜                |");
                    LoggerManager.Debug($"―――――――――――――――――|");

                    PinListData = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList[0].GetPinList();        // 핀 리스트 - 지난 번 결과가 Pass인 놈들이 앞쪽에 있다.

                    // 핀 데이터에서 DUT 중심으로부터 각 핀들의 거리와 각도를 미리 계산
                    TotalPinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                    PinDistanceData.Clear();
                    PinAngleData.Clear();

                    for (i = 0; i <= TotalPinNum - 1; i++)
                    {
                        tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);

                        if (tmpPinData != null)
                        {
                            PinDistanceData.Add(Distance2D(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX, this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY, tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value));

                            LoggerManager.Debug($"Pin Number = {tmpPinData.PinNum}, X : {tmpPinData.AbsPos.X.Value}, Y : {tmpPinData.AbsPos.Y.Value}, Distance = {PinDistanceData[PinDistanceData.Count - 1]}", isInfo: IsInfo);

                            PinAngleData.Add(Math.Atan2(tmpPinData.AbsPos.Y.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY, tmpPinData.AbsPos.X.Value - this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX));
                        }
                    }

                    // 각 사분면 별로 핀 분류

                    // 3사분면
                    #region 3rd_layer

                    foreach (IPinData iPindata in PinListData)
                    {
                        iPinNo = iPindata.PinNum.Value - 1;
                        if ((PinAngleData[iPinNo] >= -Math.PI) && (PinAngleData[iPinNo] < -Math.PI * 0.5))
                        {
                            cur_pinResult = AlignResult.GetResult(iPinNo);
                            if (cur_pinResult == null)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_FORCED_PASS ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_PASSED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_SKIP)
                            { }
                            else
                            {
                                // Fail 핀 중 이번 얼라인에서 아직 안 해본 핀
                                if (cur_pinResult.AlignToken == true) PinNumList.Add(iPinNo);
                            }
                        }
                    }
                    if (PinNumList.Count > 0)
                    {
                        // DUT 중심에서 거리가 먼 순으로 재정렬
                        PinNumList.Sort(delegate (int pin_num1, int pin_num2)
                        {
                            if (PinDistanceData[pin_num1] > PinDistanceData[pin_num1]) return 1;
                            if (PinDistanceData[pin_num1] < PinDistanceData[pin_num1]) return -1;
                            return 0;
                        });
                        IGroupData tmpGroupdata = new GroupData();
                        tmpGroupdata.PinNumList = new List<int>();
                        tmpGroupdata.PinNumList.AddRange(PinNumList);
                        tmpGroupList.Add(tmpGroupdata);
                    }
                    #endregion

                    // 2사분면
                    #region 2nd_layer

                    PinNumList.Clear();
                    foreach (IPinData iPindata in PinListData)
                    {
                        iPinNo = iPindata.PinNum.Value - 1;
                        if (PinAngleData[iPinNo] <= Math.PI && PinAngleData[iPinNo] > Math.PI * 0.5)
                        {
                            cur_pinResult = AlignResult.GetResult(iPinNo);
                            if (cur_pinResult == null)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_FORCED_PASS ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_PASSED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_SKIP)
                            { }
                            else
                            {
                                // Fail 핀 중 이번 얼라인에서 아직 안 해본 핀
                                if (cur_pinResult.AlignToken == true) PinNumList.Add(iPinNo);
                            }
                        }
                    }
                    if (PinNumList.Count > 0)
                    {
                        // DUT 중심에서 거리가 먼 순으로 재정렬
                        PinNumList.Sort(delegate (int pin_num1, int pin_num2)
                        {
                            if (PinDistanceData[pin_num1] > PinDistanceData[pin_num1]) return 1;
                            if (PinDistanceData[pin_num1] < PinDistanceData[pin_num1]) return -1;
                            return 0;
                        });
                        IGroupData tmpGroupdata = new GroupData();
                        tmpGroupdata.PinNumList = new List<int>();
                        tmpGroupdata.PinNumList.AddRange(PinNumList);
                        tmpGroupList.Add(tmpGroupdata);
                    }
                    #endregion

                    // 1사분면
                    #region 1st_layer

                    PinNumList.Clear();
                    foreach (IPinData iPindata in PinListData)
                    {
                        iPinNo = iPindata.PinNum.Value - 1;
                        if (PinAngleData[iPinNo] > 0 && PinAngleData[iPinNo] <= Math.PI * 0.5)
                        {
                            cur_pinResult = AlignResult.GetResult(iPinNo);
                            if (cur_pinResult == null)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_FORCED_PASS ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_PASSED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_SKIP)
                            { }
                            else
                            {
                                // Fail 핀 중 이번 얼라인에서 아직 안 해본 핀
                                if (cur_pinResult.AlignToken == true) PinNumList.Add(iPinNo);
                            }
                        }
                    }
                    if (PinNumList.Count > 0)
                    {
                        // DUT 중심에서 거리가 먼 순으로 재정렬
                        PinNumList.Sort(delegate (int pin_num1, int pin_num2)
                        {
                            if (PinDistanceData[pin_num1] > PinDistanceData[pin_num1]) return 1;
                            if (PinDistanceData[pin_num1] < PinDistanceData[pin_num1]) return -1;
                            return 0;
                        });
                        IGroupData tmpGroupdata = new GroupData();
                        tmpGroupdata.PinNumList = new List<int>();
                        tmpGroupdata.PinNumList.AddRange(PinNumList);
                        tmpGroupList.Add(tmpGroupdata);
                    }
                    #endregion

                    // 4사분면
                    #region 4th_layer

                    PinNumList.Clear();
                    foreach (IPinData iPindata in PinListData)
                    {
                        iPinNo = iPindata.PinNum.Value - 1;
                        if (PinAngleData[iPinNo] <= 0 && PinAngleData[iPinNo] > -Math.PI * 0.5)
                        {
                            cur_pinResult = AlignResult.GetResult(iPinNo);
                            if (cur_pinResult == null)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED)
                                PinNumList.Add(iPinNo);
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_FORCED_PASS ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_PASSED ||
                                     cur_pinResult.PinResult == PINALIGNRESULT.PIN_SKIP)
                            { }
                            else
                            {
                                // Fail 핀 중 이번 얼라인에서 아직 안 해본 핀
                                if (cur_pinResult.AlignToken == true) PinNumList.Add(iPinNo);
                            }
                        }
                    }
                    if (PinNumList.Count > 0)// 얼라인을 해야되는 핀들을 모았음. 
                    {
                        // DUT 중심에서 거리가 먼 순으로 재정렬
                        PinNumList.Sort(delegate (int pin_num1, int pin_num2)
                        {
                            if (PinDistanceData[pin_num1] > PinDistanceData[pin_num1]) return 1;
                            if (PinDistanceData[pin_num1] < PinDistanceData[pin_num1]) return -1;
                            return 0;
                        });
                        IGroupData tmpGroupdata = new GroupData();
                        tmpGroupdata.PinNumList = new List<int>();
                        tmpGroupdata.PinNumList.AddRange(PinNumList);
                        tmpGroupList.Add(tmpGroupdata);
                    }
                    #endregion


                    // 그룹에 할당된 총 핀 갯수로 얼라인이 가능한지 확인
                    // 여기서 숫자 안 맞으면 밑에서 무한루프 돌게 되니 주의!
                    iCount = 0;

                    foreach (IGroupData GrpVal in tmpGroupList)
                    {
                        iCount += GrpVal.PinNumList.Count();
                    }

                    if (iCount < total_required)
                    {
                        LoggerManager.Debug($"PinAlign() :  End total_required");

                        return false;      // 핀 부족
                    }

                    // 각 사분면 별 할당량 배분. 얼라인 횟수가 다 찰 때까지 그룹별로 각 리스트에 첫번째 핀을 하나씩 고른다.
                    AlignProcInfo.RequiredAlignList.Clear();

                    cur_group = 0;
                    group_count = tmpGroupList.Count();
                    GrouNumInList.Clear();

                    while (total_required > 0)
                    {
                        if (this.MonitoringManager().IsSystemError == true)
                        {
                            LoggerManager.Debug($"PinAlign() : Operation faild because system error state [2]");
                            break;
                        }

                        if (cur_group > group_count - 1)
                        {
                            cur_group = 0;
                        }

                        if (tmpGroupList[cur_group].PinNumList.Count() > 0)
                        {
                            cur_applied = tmpGroupList[cur_group].PinNumList[0];
                            RequiredNumInGroup.Add(cur_applied);
                            GrouNumInList.Add(cur_group);
                            tmpGroupList[cur_group].PinNumList.RemoveAt(0);
                            total_required -= 1;
                        }

                        cur_group += 1;
                    }

                    // 그룹별로 재정렬. (핀 얼라인 시간을 최소화 하기 위하여)
                    for (cur_group = 0; cur_group <= tmpGroupList.Count() - 1; cur_group++)
                    {
                        for (i = 0; i <= GrouNumInList.Count() - 1; i++)
                        {
                            if (GrouNumInList[i] == cur_group)
                            {
                                AlignProcInfo.RequiredAlignList.Add(RequiredNumInGroup[i]);
                            }
                        }
                    }
                }
                else
                {
                    // 그룹 별 할당량 버퍼 초기화
                    RequiredPerGroup.Clear();
                    group_count = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Count();

                    // 그룹 지정이 되어 있는 경우, 그룹 별로 핀 얼라인 할당량을 구하고 그만큼 핀을 배분한다.

                    for (i = 0; i <= group_count; i++)
                    {
                        RequiredPerGroup.Add(0);
                    }

                    // 그룹 별 할당량 자동 배분. 
                    // 만약 할당량이 메뉴얼로 사용자에 의해 직접 지정되어 있는 경우 루프 돌릴 필요없이 RequiredPerGroup에 필요한 수량만 적어주면 됨.
                    cur_group = 0;

                    while (total_required > 0)
                    {
                        if (this.MonitoringManager().IsSystemError == true)
                        {
                            LoggerManager.Debug($"PinAlign() : Operation faild because system error state [3]");
                            break;
                        }

                        if (cur_group > group_count - 1)
                        {
                            cur_group = 0;
                        }

                        RequiredPerGroup[cur_group] += 1;
                        total_required -= 1;
                        cur_group += 1;
                    }

                    // 각 그룹별로 할당량만큼 핀 고르기
                    AlignProcInfo.RequiredAlignList.Clear();

                    foreach (IGroupData group_data in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList)
                    {
                        cur_group = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.IndexOf(group_data);

                        if (RequiredPerGroup[cur_group] <= 0)
                        {
                            // 이미 끝난 그룹
                            break;
                        }

                        // 이 그룹에 pass된 핀이 몇 개나 있는지 확인
                        iRemained.Clear();
                        iPassed = 0;

                        // 핀 리스트 - 지난 번 결과가 Pass인 놈들이 앞쪽에 있다.
                        PinListData = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList[cur_group].GetPinList();

                        foreach (IPinData ipin in PinListData)
                        {
                            cur_pinResult = AlignResult.GetResult(ipin.PinNum.Value - 1);

                            if (cur_pinResult == null)
                            {
                                iRemained.Add(ipin.PinNum.Value - 1);
                            }
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED)
                            {
                                iRemained.Add(ipin.PinNum.Value - 1);
                            }
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_PASSED || cur_pinResult.PinResult == PINALIGNRESULT.PIN_FORCED_PASS)
                            {
                                iPassed += 1;
                            }
                            else if (cur_pinResult.PinResult == PINALIGNRESULT.PIN_SKIP)
                            {

                            }
                            else
                            {
                                if (cur_pinResult.AlignToken == true)
                                {
                                    iRemained.Add(ipin.PinNum.Value - 1);
                                }
                            }
                        }

                        if (iPassed >= RequiredPerGroup[cur_group])
                        {
                            // 이미 끝난 그룹
                            break;
                        }

                        if (iRemained.Count < RequiredPerGroup[cur_group])
                        {
                            // 핀 부족. FAIL처리
                            return false;
                        }

                        // 그룹에 할당
                        foreach (int spin in iRemained)
                        {
                            if (RequiredPerGroup[cur_group] <= 0)
                            {
                                break;
                            }

                            AlignProcInfo.RequiredAlignList.Add(spin);
                            RequiredPerGroup[cur_group] -= 1;
                        }
                    }
                }

                return true;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug("PinAlign() : Exception error in MakeAlignList()");

                return false;
            }
        }
        private PINPROCESSINGSTATE GeneratePinSequence(PINALIGNSOURCE _PinAlignSource, bool usebackupAlignedOffset)
        {
            // 여기가 핀 얼라인에서 제일 복잡한 곳임. 정신 똑바로 차리고 건들 것.
            // 이 함수는 현재 핀 얼라인 상태를 정의하고, pass인지 fail인지 구분하며 다음 할 일을 정의하는 역할을 한다.

            PINPROCESSINGSTATE retPinProcState = PINPROCESSINGSTATE.ALIGN_FAILED;

            int PinNum = 0;
            int iRequiredCount = 0;
            List<int> tmplist = new List<int>();
            tmplist.Clear();
            int RequiredCount = 0;
            bool bCheck = false;
            int iCount = 0;
            double cardcen_diff_x = 0;
            double cardcen_diff_y = 0;
            double cardcen_diff_z = 0;
            double min_z = 0;
            double max_z = 0;
            //IPinData tmpPinData;
            //int i = 0;
            double cardcen_total_diff_x = 0;
            double cardcen_total_diff_y = 0;
            double cardcen_total_diff_z = 0;

            bool isSucessCalCenterTotalDiff = false;

            try
            {
                PinNum = this.StageSupervisor().ProbeCardInfo.GetPinCount();

                if (PinNum == 0)
                {
                    retPinProcState = PINPROCESSINGSTATE.NO_MORE_AVAIABLE_PIN;
                }
                else
                {
                    /* 우선 샘플 얼라인 조건을 확인하고 스테이트를 정리해 준다. 샘플 얼라인이냐 아니냐에 따라 고르는 우선순위가 달라진다.
                   얼라인이 끝나고 핀 얼라인 결과를 가지고 데이터를 업데이트 할 때, 핀 얼라인이 성공하더라도 샘플 얼라인 스테이트를 가지고 
                   업데이트 할 것인지 말 것인지 결정하게 되므로 주의할 것! */

                    PinAlignSettignParameter CurrentPinSetting = PinAlignParam.PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == _PinAlignSource);

                    if (CurrentPinSetting == null)
                    {
                        LoggerManager.Error("[PinHighAlignModule] GeneratePinSequence() : PinAlignSettignParameter is null value.");

                        retPinProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                        return retPinProcState;
                    }
                    
                    if (_PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION 
                        || _PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE 
                        || _PinAlignSource == PINALIGNSOURCE.CARD_CHANGE
                        || CurrentPinSetting.SamplePinAlignmentPinCount.Value <= 0
                        || this.PinAligner().IsManualTriggered) // 무조건 Diable인 조건들
                    {
                        AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.DISABLED;
                    }
                    else
                    {
                        bool ref_Module_skip_state = false; //Check for skip state in previous modules(Referance Module)
                        ref_Module_skip_state = this.PinAligner().CheckSubModulesInTheSkipstate(this);//반환 값이 False이면 Low 안했다는 뜻

                        bool forcedFullPin = false;
                        forcedFullPin = !ref_Module_skip_state || (_PinAlignSource == PINALIGNSOURCE.SOAKING && AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.DISABLED);

                        if (CurrentPinSetting.SamplePinAlignmentPinCount.Value > 0 && forcedFullPin == false)
                        {
                            if (AlignResult.EachPinResultes.Count <= 0)// Align시도한게 없을 경우
                            {
                                AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.CONTINUE;
                            }
                        }
                        else
                        {
                            AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.DISABLED;
                        }
                    }

                    if (AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.CONTINUE)// 첫번째 핀 뿐만이 아니라 while루프기때문에 다시 들어올수 있다는 점을 주의해야함. 
                    {
                        if (AlignResult.TotalPassPinCount >= CurrentPinSetting.SamplePinAlignmentPinCount.Value)// Sample Pin Count보다 Pass 된 Pin의 개수가 많은경우 
                        {
                            // 성공한 핀들이 샘플 얼라인 허용치를 초과했는지 확인하고 패스냐 실패냐를 결정한다.
                            bCheck = true;
                            iCount = 0;
                            foreach (var listval in AlignResult.EachPinResultes)
                            {//방금얼란인한 한개의 핀에 대해서만 Diff  값을 비교하는 것이 아니라 이전에 얼라인한 값들에서 diff값을 비교하는가? 
                                if (listval.PinResult == PINALIGNRESULT.PIN_PASSED)
                                {
                                    if ((Math.Abs(listval.DiffX) > CurrentPinSetting.SamplePinToleranceX.Value) ||
                                        (Math.Abs(listval.DiffY) > CurrentPinSetting.SamplePinToleranceY.Value) ||
                                        (Math.Abs(listval.DiffZ) > CurrentPinSetting.SamplePinToleranceZ.Value))
                                    {
                                        bCheck = false;
                                        LoggerManager.Debug($"GeneratePinSequence(), The pin position is out of sample pin tolerance. X Diff, Y Diff, Z Diff = ({listval.DiffX}, {listval.DiffY}, {listval.DiffZ})," +
                                            $" Sample Pin Tolerance = {CurrentPinSetting.SamplePinToleranceX.Value}, {CurrentPinSetting.SamplePinToleranceY.Value}, {CurrentPinSetting.SamplePinToleranceZ.Value}");
                                        break;
                                    }
                                    iCount += 1;
                                }
                                if (iCount >= CurrentPinSetting.SamplePinAlignmentPinCount.Value) break;
                            }

                            if (bCheck == true)
                                AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.PASS;
                            else
                            {
                                AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.FAIL;//Sample PinAlign Fail일 경우 
                                foreach (EachPinResult tmp_result_data in AlignResult.EachPinResultes)
                                {
                                    if (tmp_result_data.PinResult == PINALIGNRESULT.PIN_FOCUS_FAILED ||
                                        tmp_result_data.PinResult == PINALIGNRESULT.PIN_TIP_FOCUS_FAILED ||
                                        tmp_result_data.PinResult == PINALIGNRESULT.PIN_UNKNOWN_FAILED)
                                    {
                                        tmp_result_data.AlignToken = true;// true이므로 MakeAlignList에 핀넘버가 추가됨. 
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (AlignResult.EachPinResultes.Count <= 0)// Align시도한게 없을 경우
                            {
                                AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.CONTINUE;
                            }
                            else
                            {
                                AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.FAIL;//Sample PinAlign Fail일 경우
                            }
                        }
                    }

                    // 앞으로 몇 개 핀을 더 얼라인 해야 하는지 계산한다.
                    if (_PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION || _PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE || _PinAlignSource == PINALIGNSOURCE.CARD_CHANGE)
                        // 핀 등록시에는 전체 핀을 돌린다.
                        RequiredCount = PinNum - (int)AlignResult.TotalPassPinCount;
                    else
                    {
                        if (AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.CONTINUE)
                            // 아직 샘플 얼라인이 안 끝난 상태
                            RequiredCount = CurrentPinSetting.SamplePinAlignmentPinCount.Value - (int)AlignResult.TotalPassPinCount;
                        else
                        {
                            if (AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.PASS)
                            {
                                // 샘플 핀 얼라인이 패스 되었으니 핀 얼라인은 여기서 종료한다.
                                RequiredCount = 0;
                            }
                            else if (AlignProcInfo.ProcSampleAlign == SAMPLEPINALGINRESULT.FAIL)
                            {
                                // 샘플 핀 얼라인이 실패하였으니 끝내거나 남은 요구 갯수를 구한다.
                                int failCount = 0;
                                foreach (var listval in AlignResult.EachPinResultes)
                                {
                                    if (listval.PinResult != PINALIGNRESULT.PIN_PASSED)
                                    {
                                        failCount += 1;
                                    }
                                }

                                if(failCount > 0)// Sample Pin Find Fail
                                {
                                    retPinProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                                    LoggerManager.Debug($"GeneratePinSequence(), Sample pin find fail.");
                                    return retPinProcState;
                                }
                                else//Sample Pin Tol Fail인 Case, 이 경우는 남은 Pin 진행 필요.
                                {
                                    RequiredCount = CurrentPinSetting.PinCount.Value - AlignResult.EachPinResultes.Count;
                                    LoggerManager.Debug($"GeneratePinSequence(), The pin position is out of sample pin tolerance. Pin Count: {CurrentPinSetting.PinCount.Value}, " +
                                        $"PassPinCount: {AlignResult.EachPinResultes.Count}, PinAlignSource:{_PinAlignSource}");
                                }
                            }
                            else
                            {
                                // 샘플 핀 얼라인이 꺼져 있다. 남은 전체 요구 갯수만큼 핀 얼라인을 수행한다.
                                RequiredCount = CurrentPinSetting.PinCount.Value - (int)AlignResult.TotalPassPinCount;
                            }
                        }
                    }

                    // 남은 핀 얼라인 카운트로 더 얼라인이 필요한지 따지고, 리스트를 만든다.
                    if (RequiredCount > 0)
                    {
                        // 아직 얼라인 해야 할 핀이 더 있음. 리스트를 생성한다.
                        if (MakeAlignList(_PinAlignSource) == true)
                            retPinProcState = PINPROCESSINGSTATE.BE_CONTINUE;
                        else
                            retPinProcState = PINPROCESSINGSTATE.NO_MORE_AVAIABLE_PIN;
                    }
                    else
                    {
                        // 얼라인 종료.
                        // 이쪽은 정상적으로 다 찾고 더이상 찾을 것이 없을 때 타는 곳.
                        // Tolerance를 초과하였는지 확인한다.
                        iCount = 0;
                        min_z = 9999999;
                        max_z = -9999999;

                        // Pin align의 Result 결과를 이용하여
                        // 1. passed된 pin들의 card center로부터의 x, y, z 평균값,
                        // 2. passed된 pin들의 min, max 값
                        // 3. passed된 pin들의 Count
                        // 를 가져옵니다.

                        CalPinAlignPassedResult(ref cardcen_diff_x, ref cardcen_diff_y, ref cardcen_diff_z, ref min_z, ref max_z, out iCount);

                        if (iCount > 0)
                        {
                            AlignInfo.AlignResult.MinMaxZDiff = Math.Abs(max_z - min_z);

                            if (AlignInfo.AlignResult.MinMaxZDiff > CurrentPinSetting.MinMaxZDiffLimit.Value)
                            {
                                LoggerManager.Error($"PinAlign() : Align failed - Z Min/Max Tolerance Error min = {min_z}, max = {max_z}, tolerance = {CurrentPinSetting.MinMaxZDiffLimit.Value}");

                                AlignResult.Result = EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE;

                                retPinProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                            }
                            else
                            {
                                if ((_PinAlignSource != PINALIGNSOURCE.PIN_REGISTRATION && _PinAlignSource != PINALIGNSOURCE.CARD_CHANGE && _PinAlignSource != PINALIGNSOURCE.DEVICE_CHANGE))
                                {
                                    isSucessCalCenterTotalDiff = CalCardCenterTotalDiff(PinNum, usebackupAlignedOffset, ref cardcen_total_diff_x, ref cardcen_total_diff_y, ref cardcen_total_diff_z);

                                    if (isSucessCalCenterTotalDiff)
                                    {
                                        AlignInfo.AlignResult.CardCenterDiffX = (Math.Abs(cardcen_total_diff_x + cardcen_diff_x));
                                        AlignInfo.AlignResult.CardCenterDiffY = (Math.Abs(cardcen_total_diff_y + cardcen_diff_y));
                                        AlignInfo.AlignResult.CardCenterDiffZ = (Math.Abs(cardcen_total_diff_z + cardcen_diff_z));

                                        if ((CurrentPinSetting.CardCenterToleranceX.Value < AlignInfo.AlignResult.CardCenterDiffX)
                                            || (CurrentPinSetting.CardCenterToleranceY.Value < AlignInfo.AlignResult.CardCenterDiffY)
                                            || (CurrentPinSetting.CardCenterToleranceZ.Value < AlignInfo.AlignResult.CardCenterDiffZ)
                                            )
                                        {
                                            LoggerManager.Error($"PinAlign() : Align failed - Center Tolerance Error     " +
                                                                $"center diff = ({cardcen_total_diff_x} + {cardcen_diff_x}, {cardcen_total_diff_y} + {cardcen_diff_y}, {cardcen_total_diff_z} + {cardcen_diff_z})    " +
                                                                $"tolerance =  ({CurrentPinSetting.CardCenterToleranceX.Value}, " +
                                                                                $"{CurrentPinSetting.CardCenterToleranceY.Value}, " +
                                                                                $"{CurrentPinSetting.CardCenterToleranceZ.Value})");

                                            AlignResult.Result = EventCodeEnum.PIN_CARD_CENTER_TOLERANCE;

                                            retPinProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                                        }
                                        else
                                        {
                                            iRequiredCount = CalPinDataAboutOverTolerance(_PinAlignSource);

                                            if (iRequiredCount > 0)
                                            {
                                                if (MakeAlignList(_PinAlignSource) == true)
                                                {
                                                    retPinProcState = PINPROCESSINGSTATE.BE_CONTINUE;
                                                }
                                                else
                                                {
                                                    retPinProcState = PINPROCESSINGSTATE.NO_MORE_AVAIABLE_PIN;
                                                }
                                            }
                                            else
                                            {
                                                retPinProcState = PINPROCESSINGSTATE.ALIGN_PASSED;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        retPinProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                                    }
                                }
                                else
                                {
                                    retPinProcState = PINPROCESSINGSTATE.ALIGN_PASSED;
                                }
                            }
                        }
                        else
                        {
                            // 예외 상황. 원래 발생해서는 안 됨. 
                            LoggerManager.Debug($"PinAlign() : Unexpected state - no passed pin in result");
                            retPinProcState = PINPROCESSINGSTATE.UNKNOWN_STATE;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retPinProcState = PINPROCESSINGSTATE.UNKNOWN_STATE;
            }

            return retPinProcState;
        }
        public void CalPinAlignPassedResult(ref double cardcenDiffX, ref double cardcenDiffY, ref double cardcenDiffZ, ref double minZ, ref double maxZ, out int passedCount)
        {
            passedCount = 0;

            cardcenDiffX = 0;
            cardcenDiffY = 0;
            cardcenDiffZ = 0;

            try
            {
                foreach (var listval in AlignResult.EachPinResultes)
                {
                    if (listval.PinResult == PINALIGNRESULT.PIN_PASSED)
                    {
                        cardcenDiffX += listval.DiffX;
                        cardcenDiffY += listval.DiffY;
                        cardcenDiffZ += listval.DiffZ;

                        if (minZ > listval.Height)
                        {
                            minZ = listval.Height;
                        }

                        if (maxZ < listval.Height)
                        {
                            maxZ = listval.Height;
                        }
                        passedCount += 1;
                    }
                }

                if (passedCount > 0)
                {
                    cardcenDiffX = cardcenDiffX / passedCount;
                    cardcenDiffY = cardcenDiffY / passedCount;
                    cardcenDiffZ = cardcenDiffZ / passedCount;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool CalCardCenterTotalDiff(int pinNum, bool usebackupAlignedOffset, ref double cardCenTotalDiffX, ref double cardCenTotalDiffY, ref double cardCenTotalDiffZ)
        {
            bool isSucessCalCardCenterTotalDiff = false;

            try
            {
                if (pinNum > 0)
                {
                    for (int i = 0; i <= pinNum - 1; i++)
                    {
                        var tmpPD = this.StageSupervisor().ProbeCardInfo.GetPin(i);

                        if (tmpPD != null)
                        {
                            if (usebackupAlignedOffset != true)
                            {
                                cardCenTotalDiffX += (tmpPD.AlignedOffset.X.Value - tmpPD.LowCompensatedOffset.X.Value);
                                cardCenTotalDiffY += (tmpPD.AlignedOffset.Y.Value - tmpPD.LowCompensatedOffset.Y.Value);
                                cardCenTotalDiffZ += (tmpPD.AlignedOffset.Z.Value - tmpPD.LowCompensatedOffset.Z.Value);
                            }
                            else
                            {
                                // 이미 usebackupAlignedOffset 가 true 라는 뜻은 백업된 핀얼라인 데이터를 사용했다라는 뜻과 같음 
                                // 백업된 핀얼라인 데이터를 사용했다는 말은 low 보정 없이 시스템파라미터를 활용해서 이미 얼라인을 이전에 성공 시켰다는 말!
                                var backupAlignedOffset = probeCardSysObject.ProbercardBackupinfo.BackupPinDataList.FirstOrDefault(n => n.PinNo == i + 1);

                                double AlignedOffset_X = tmpPD.AbsPosOrg.X.Value - backupAlignedOffset.BackupAbsPos.X.Value;
                                double AlignedOffset_Y = tmpPD.AbsPosOrg.Y.Value - backupAlignedOffset.BackupAbsPos.Y.Value;
                                double AlignedOffset_Z = tmpPD.AbsPosOrg.Z.Value - backupAlignedOffset.BackupAbsPos.Z.Value;

                                cardCenTotalDiffX += (tmpPD.AlignedOffset.X.Value + AlignedOffset_X);
                                cardCenTotalDiffY += (tmpPD.AlignedOffset.Y.Value + AlignedOffset_Y);
                                cardCenTotalDiffZ += (tmpPD.AlignedOffset.Z.Value + AlignedOffset_Z);

                                LoggerManager.Debug($"PinAlign() : Pin #{i + 1} - " +
                                                      $"backup pin diff = ({AlignedOffset_X}, {AlignedOffset_Y}, {AlignedOffset_Z})    " +
                                                      $"card Cen diff =  ({cardCenTotalDiffX}, {cardCenTotalDiffY}, {cardCenTotalDiffZ})", isInfo: IsInfo);
                            }
                        }


                    }

                    cardCenTotalDiffX = cardCenTotalDiffX / pinNum;
                    cardCenTotalDiffY = cardCenTotalDiffY / pinNum;
                    cardCenTotalDiffZ = cardCenTotalDiffZ / pinNum;

                    isSucessCalCardCenterTotalDiff = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                isSucessCalCardCenterTotalDiff = false;
            }

            return isSucessCalCardCenterTotalDiff;
        }
        public int CalPinDataAboutOverTolerance(PINALIGNSOURCE pinAlignSource)
        {
            int requiredCount = 0;

            try
            {
                PinAlignSettignParameter CurrentPinSetting = PinAlignParam.PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == pinAlignSource);

                if (CurrentPinSetting == null)
                {
                    LoggerManager.Error("[PinHighAlignModule] CalPinDataAboutOverTolerance() : PinAlignSettignParameter is null value.");
                }

                bool IsWroteFomulaDescription = false;

                foreach (var listval in AlignResult.EachPinResultes)
                {
                    if (listval.PinResult == PINALIGNRESULT.PIN_PASSED)
                    {
                        var tmpPD = this.StageSupervisor().ProbeCardInfo.GetPin(listval.PinNum - 1);

                        //double CompareValueX = (Math.Abs((tmpPD.AlignedOffset.X.Value - tmpPD.LowCompensatedOffset.X.Value) + listval.DiffX));
                        //double CompareValueY = (Math.Abs((tmpPD.AlignedOffset.Y.Value - tmpPD.LowCompensatedOffset.Y.Value) + listval.DiffY));
                        //double CompareValueZ = (Math.Abs((tmpPD.AlignedOffset.Z.Value - tmpPD.LowCompensatedOffset.Z.Value) + listval.DiffZ));

                        // TODO : 컨셉 및 로직 정확히 이해한 뒤, 다시 확인할 것.
                        // 현재 : 이전 핀과의 차이의 의미

                        //double CompareValueX = Math.Abs(tmpPD.AlignedOffset.X.Value - tmpPD.LowCompensatedOffset.X.Value + listval.DiffX + tmpPD.MarkCumulativeCorrectionValue.X.Value);
                        //double CompareValueY = Math.Abs(tmpPD.AlignedOffset.Y.Value - tmpPD.LowCompensatedOffset.Y.Value + listval.DiffY + tmpPD.MarkCumulativeCorrectionValue.Y.Value);
                        //double CompareValueZ = Math.Abs(tmpPD.AlignedOffset.Z.Value - tmpPD.LowCompensatedOffset.Z.Value + listval.DiffZ + tmpPD.MarkCumulativeCorrectionValue.Z.Value);

                        double CompareValueX = Math.Abs(listval.DiffX);
                        double CompareValueY = Math.Abs(listval.DiffY);
                        double CompareValueZ = Math.Abs(listval.DiffZ);

                        if ((CompareValueX > CurrentPinSetting.EachPinToleranceX.Value) ||
                            (CompareValueY > CurrentPinSetting.EachPinToleranceY.Value) ||
                            (CompareValueZ > CurrentPinSetting.EachPinToleranceZ.Value))
                        {
                            // "PinAlign() : Pin #5 failed - Each Tolerance Error     pin diff = (46.2801847075607 + 4.54892013041535, 17.6417820377537 + 4.27597799511568, -30.6666666665478 + 3.20833333331393)    tolerance =  (50, 50, 100)"
                            //LoggerManager.Debug($"PinAlign() : Pin #{listval.PinNum} failed - Each Tolerance Error     " +
                            //                    $"pin diff = ({tmpPD.AlignedOffset.X.Value - tmpPD.LowCompensatedOffset.X.Value} + {listval.DiffX}, {tmpPD.AlignedOffset.Y.Value - -tmpPD.LowCompensatedOffset.Y.Value} + {listval.DiffY}, {tmpPD.AlignedOffset.Z.Value - tmpPD.LowCompensatedOffset.Z.Value} + {listval.DiffZ})    " +
                            //                    $"tolerance =  ({CurrentPinSetting.EachPinToleranceX.Value}, " +
                            //                                  $"{CurrentPinSetting.EachPinToleranceY.Value}, " +
                            //                                  $"{CurrentPinSetting.EachPinToleranceZ.Value})");

                            if (IsWroteFomulaDescription == false)
                            {
                                LoggerManager.Debug($"PinAlign() : Comparison calculation formula = (AlignedOffset - LowCompensatedOffset + Diff + MarkCumulativeCorrectionValue)");

                                IsWroteFomulaDescription = true;
                            }

                            LoggerManager.Error($"PinAlign() : Pin #{listval.PinNum} failed - Each Tolerance Error     " +
                                                   $"pin diff = (" +
                                                   $"{listval.DiffX}, " +
                                                   $"{listval.DiffY}, " +
                                                   $"{listval.DiffZ}" +
                                                   $")    " +
                                                   $"tolerance =  ({CurrentPinSetting.EachPinToleranceX.Value}, " +
                                                                 $"{CurrentPinSetting.EachPinToleranceY.Value}, " +
                                                                 $"{CurrentPinSetting.EachPinToleranceZ.Value})");

                            //LoggerManager.Debug($"PinAlign() : Pin #{listval.PinNum} failed - Each Tolerance Error     " +
                            //                   $"pin diff = (" +
                            //                   $"{tmpPD.AlignedOffset.X.Value} - {tmpPD.LowCompensatedOffset.X.Value} + {listval.DiffX} + {tmpPD.MarkCumulativeCorrectionValue.X.Value}, " +
                            //                   $"{tmpPD.AlignedOffset.Y.Value} - {tmpPD.LowCompensatedOffset.Y.Value} + {listval.DiffY} + {tmpPD.MarkCumulativeCorrectionValue.Y.Value}, " +
                            //                   $"{tmpPD.AlignedOffset.Z.Value} - {tmpPD.LowCompensatedOffset.Z.Value} + {listval.DiffZ} + {tmpPD.MarkCumulativeCorrectionValue.Z.Value}" +
                            //                   $")    " +
                            //                   $"tolerance =  ({CurrentPinSetting.EachPinToleranceX.Value}, " +
                            //                                 $"{CurrentPinSetting.EachPinToleranceY.Value}, " +
                            //                                 $"{CurrentPinSetting.EachPinToleranceZ.Value})");

                            listval.PinResult = PINALIGNRESULT.PIN_OVER_TOLERANCE;

                            //this.PinAligner().ReasonOfError.Reason = "Each Tolerance Error";

                            AlignResult.Result = EventCodeEnum.PIN_EACH_PIN_TOLERANCE_FAIL;

                            AlignResult.TotalPassPinCount -= 1;
                            AlignResult.TotalFailPinCount += 1;

                            requiredCount += 1;
                        }
                        else
                        {
                            LoggerManager.Debug($"PinAlign() : Pin #{listval.PinNum} - Each Tolerance Succes     " +
                                                   $"pin diff = (" +
                                                   $"{listval.DiffX}, " +
                                                   $"{listval.DiffY}, " +
                                                   $"{listval.DiffZ}" +
                                                   $")    " +
                                                   $"tolerance =  ({CurrentPinSetting.EachPinToleranceX.Value}, " +
                                                                 $"{CurrentPinSetting.EachPinToleranceY.Value}, " +
                                                                 $"{CurrentPinSetting.EachPinToleranceZ.Value})", isInfo: IsInfo);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }



            return requiredCount;
        }
        public EventCodeEnum PinAlign(PINALIGNSOURCE _PinAlignSource, bool FourPinAlign = false)
        {
            EventCodeEnum retval = EventCodeEnum.NONE;

            LoggerManager.Debug($"PinAlign() : PinAlignStart, PinAlignSource = {this.PinAligner().PinAlignSource.ToString()}", isInfo: true);

            bool PinResultTestFlag = false;

            PinCoordinate NewPinPos = null;
            EventCodeEnum result = EventCodeEnum.NONE;
            PINALIGNRESULT EachPinResult = PINALIGNRESULT.PIN_SKIP;
            IPinData tmpPinData;
            PINPROCESSINGSTATE PinAlignProcState = PINPROCESSINGSTATE.UNKNOWN_STATE;

            int i = 0;

            this.PinAligner().StopDrawDutOverlay(CurCam);

            int TotalFailCnt = 0;
            int TotalPassCnt = 0;
            int TotalPinTipPassCnt = 0;

            List<EachPinResult> tmpEachResult = new List<EachPinResult>();
            List<IPinData> CurPinList = new List<IPinData>();
            MachineCoordinate machine = new MachineCoordinate();

            double tmpcenx = 0;
            double tmpceny = 0;
            double tmpdutcenx = 0;
            double tmpdutceny = 0;

            double avr_changed_x = 0;
            double avr_changed_y = 0;
            double avr_changed_z = 0;
            int aligned_cnt = 0;
            string strResult = "";
            double tmpDiffX = 0;
            double tmpDiffY = 0;
            double tmpDiffZ = 0;
            int DutArrayIndex = 0;
            int PinArrayIndex = 0;
            PINALIGNRESULT tmp_cur_result;
            int idx;
            //// 이전 데이터 초기화
            //foreach (Dut myDutList in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
            //{
            //    foreach (PinData pin in myDutList.PinList)
            //    {
            //        if (pin.Result.Value == PINALIGNRESULT.PIN_PASSED)
            //        {
            //        }
            //    }
            //}

            AlignResult.EachPinResultes.Clear();
            AlignResult.TotalAlignPinCount = 0;
            AlignResult.TotalFailPinCount = 0;
            AlignResult.TotalPassPinCount = 0;
            AlignResult.PassPercentage = 0;
            AlignResult.AlignSource = _PinAlignSource;

            #region 핀 얼라인 로직 설명

            /*
            ----------------------------------------------------------------------------------------------------------------------------------
            당신을 위해 준비한 핀 얼라인 동작 로직 정리 ver 1.0 by Ralph

            1) Sample Pin 얼라인이 켜져 있는 경우 일단 그룹별로 Sample Pin 갯수를 할당한다.
            2) Sample Pin 얼라인은 각 그룹에 있는 핀 리스트 중 우선순위 앞쪽에 있는 핀부터 한다.
            3) Sample Pin 얼라인이 종료 되었을 때 혹은 그룹 중 하나라도 Sample Pin 얼라인 조건에 만족하지 못했을 경우 그 즉시 Sample Align을 멈춘다.
            4) 이상 Sample Pin 얼라인 결과는 저장되어 있어야 한다. (다음 얼라인 프로세스에서 중복되어 얼라인 되지 않도록) = not_performed가 아니도록 
            5) 이제 본편 핀 얼라인을 돌린다.
            6) 그룹 별로 할당된 핀 얼라인 갯수를 만족할때까지 루프를 돈다. (더 이상 얼라인할 핀이 없게되면 그 즉시 핀 얼라인은 Fail된다)
            7) 핀 얼라인 과정에서 Sample 얼라인 과정에서 이미 진행되었던 핀은 Pass로 간주하고 건너뛴다.
            8) 이런 식으로 각 그룹 별로 할당된 핀들을 얼라인하고 결과를 AlignResult에 저장해 둔다.
            9) 일단 Pass 갯수가 만족되면 루프를 멈추고 결과 데이터 분석을 시작한다.


            분석 과정 로직 정리

            1) 얼라인 결과 데이터로부터 카드 전체의 변화량을 구한다.
            2) 카드 전체 변화량이 Card Center Tolerance를 초과하였는지 확인한다.
               (이 부분에서 에러가 발생한 경우, 핀 얼라인은 결과적으로 Fail 처리된다)
            3) 얼라인 결과 데이터로부터 가장 높은 핀과 가장 낮은 핀의 높이를 구하여 차이를 계산하고 Min/Max Tolerance를 초과하였는지 확인한다.
               (이 부분에서 에러가 발생한 경우, 핀 얼라인은 결과적으로 Fail 처리된다)
            4) 각 핀의 개별 변화량을 구하여 Each Pin Tolerance를 초과하였는지 확인한다.
               (특정 핀 하나만 유별나게 튀는 값이 나왔을 경우를 대비하여 확인한다)
            5) Each Pin Tolerance를 초과한 핀이 있다면 물리적으로 혼자 어긋나 있거나, 혹은 이미지 처리를 잘 못하여 엉뚱한 곳을 잡은 결과일 것이다.
               확률적으로 그리고 경험적으로 후자일 것이라고 보고 해당 핀은 Fail 처리한 뒤 다시 전 단계로 돌아가 해당 그룹에서 핀 얼라인을 다시 한다.
               (따라서 해당 그룹에서 PassCount를 하나 빼고 다시 돌린다)
            6) 핀 각을 구한다. 만약 계산된 핀 각이 허용치를 벗어나면 에러를 발생시킨다. 
            7) 핀 Center를 구하여 패드 Center와 맞춘 뒤 계산상으로 Angle을 적용시켜 Pin - Pad 매칭 에러량을 구한다.
            8) 각 핀/패드 관계에서 에러량이 허용치를 벗어난 경우 에러를 발생시킨다.

            위 모든 과정에서 Pass가 되면, 

            1) Align Result 로부터 변화량을 구해 실제 각 핀 위치를 보정한다.
            2) 각 그룹을 뒤져서 Fail 되었던 핀들을 리스트에서 뒤쪽으로 배치하여 다음 얼라인 할 때에는 Pass 되었던 핀부터 얼라인 할 수 있도록 한다.


            리커버리 과정 로직 정리

            1) 핀 얼라인이 도중에 Fail되어 Recovery 프로세스로 진입된 경우 사용자에 의해 이미 Pass된 핀 위치로 이동할 경우 핀 얼라인 결과가 보상된
               위치로 가야 하는데 핀 얼라인이 최종적으로 완료되지 않은 상황이므로 원본 데이터는 얼라인 이전 값을 가지고 있게 된다.
               이 상황을 해결하기 위해서 AlignResult 값을 사용한다.
               핀 위치로 이동할 때 AlignResult에 있는 Diff값을 더해서 움직이고, AlignResult 값은 

                - 핀 얼라인이 최종적으로 성공했을 때
                - 핀 얼라인이 처음부터 다시 시작할 때
                - 리커버리 프로세스가 사용자에 의해 Abort 되었을 때

               초기화 한다.

               즉 평소에도 핀 위치로 이동할 때 AlignResult의 diff 값이 더해지게 되지만 초기화 되어 0이 더해지게 되므로 문제가 없게 된다.
               따라서 초기화를 잘~! 해주어야 한다.


            핀 결과 데이터 사용 로직 정리

            핀 위치 데이터로 사용하는 변수는 크게 2가지이다. AbsPos와 AlignedOffset.
            AbsPos는 패드 데이터로부터 초기화되며, 핀을 등록하는 화면에서만 업데이트가 가능하다.
            AbsPos는 랏드런 상태 혹은 등록과정이 아닌 다른 상태에서는 아무리 핀 얼라인을 해도 값이 바뀌지 않는다.
            AlignedOffset은 핀을 새로 등록하거나 디바이스가 바뀌거나, 카드가 바뀌면 0으로 초기화 된다.
            등록 과정이 아닌 상태에서 핀 얼라인을 진행하면 핀이 바뀐 위치 옵셋만큼 이 값이 변경되며, EachTolerance는 이 값으로 비교하여 계산한다.
            즉, EachTolerance는 바로 이전 핀 얼라인 결과와 비교하는 것이 아니라 사용자가 처음 등록했던 위치로부터 변한 총량과 비교하게 된다.
            코드상에서 AbsPos를 사용하면 자동으로 AbsPos + AlignedOffset이 리턴되니 주의할 것. 


            주의사항

            EachPinResultes값은 마지막으로 핀 얼라인이 수행되었던 결과가 담겨있다.
            핀 얼라인은 FAIL 이후에도 수차례 재시도가 가능해야 하므로 이전 결과가 초기화 되면 안 된다.
            이 값이 초기화 되는 경우는,
                - 핀 얼라인이 새로 시작될 때. 즉, 핀 모듈의 상태가 IDLE에서 RUN으로 변경될 때
                - 카드 체인지 이후
                - 디바이스 변경 이후
                - 핀 등록 과정에서 사용자가 Reset 버튼을 눌러서 패드데이터로부터 초기화 했거나 Ref핀을 새로 Teach 한 경우
                - 핀 얼라인이 Pass 된 경우
                - 사용자가 메뉴얼로 FAIL된 핀들을 다른 조건으로 업데이트 한 경우 (해당 핀의 결과만 NOT PERFORMED로 부분 변경한다)

            ----------------------------------------------------------------------------------------------------------------------------------
            */
            #endregion

            try
            {
                //this.VisionManager().AllStageCameraStopGrab();
                this.VisionManager().StopGrab(EnumProberCam.PIN_HIGH_CAM);
                this.VisionManager().StopGrab(EnumProberCam.PIN_LOW_CAM);

                //Pin Low Camera 의 모든 조명 OFF
                var lowCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                for (int lightindex = 0; lightindex < lowCam.LightsChannels.Count; lightindex++)
                {
                    lowCam.SetLight(lowCam.LightsChannels[lightindex].Type.Value, 0);
                }

                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_High_Mag_Align_Start, EventCodeEnum.NONE, $"({_PinAlignSource.ToString()})");

                if (this.PinAligner().ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.DONE);
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);
                    PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer = true;
                    //this.InnerStateTransition(new PinAlignDoneState(this));

                    PinAlignProcState = PINPROCESSINGSTATE.ALIGN_PASSED;

                    LoggerManager.Debug($"PinAlign() ForcedDone.");

                    #region // Test code
                    //List<Vector3> pins = new List<Vector3>();

                    //pins.Clear();
                    // Cell#03
                    //Line 4338: 2020 - 05 - 15 00:32:38.178 | PinAlign() : Succeed to find pin #1 : (34351.6633206655, 128630.812713399, 1488.3773060266)
                    //Line 4395: 2020 - 05 - 15 00:32:43.175 | PinAlign() : Succeed to find pin #2 : (92448.0577827777, 86931.8759511695, 1464.06156345469)
                    //Line 4452: 2020 - 05 - 15 00:32:49.991 | PinAlign() : Succeed to find pin #3 : (92493.1633926621, -93732.7325223717, 1445.76247035015)
                    //Line 4223: 2020 - 05 - 15 00:32:26.179 | PinAlign() : Succeed to find pin #4 : (-103448.074373213, -93713.9506813242, 1499.82343140638)
                    //Line 4280: 2020 - 05 - 15 00:32:32.474 | PinAlign() : Succeed to find pin #5 : (-103487.884988699, 86951.6925850024, 1531.57904555245)
                    //Line 2051: 2020 - 05 - 13 04:00:45.353 | [DI] | SV = 90.00, PV = 90.10, DP = -1.80 | 
                    //  PinAlign() : Succeed to find pin #1 : (34343.9442133624, 128645.813678039, 1486.00636465528)
                    //Line 2109: 2020 - 05 - 13 04:00:50.724 | [DI] | SV = 90.00, PV = 90.10, DP = -1.80 | 
                    //  PinAlign() : Succeed to find pin #2 : (92445.6135037575, 86944.5382067164, 1462.93194314552)
                    //Line 2166: 2020 - 05 - 13 04:00:57.396 | [DI] | SV = 90.00, PV = 90.10, DP = -1.80 | 
                    //  PinAlign() : Succeed to find pin #3 : (92490.9093055548, -93735.3394826104, 1445.89975803069)
                    //Line 1936: 2020 - 05 - 13 04:00:31.239 | [DI] | SV = 90.00, PV = 90.00, DP = -1.57 | 
                    //  PinAlign() : Succeed to find pin #4 : (-103468.470678033, -93717.0666717616, 1500.52445527894)
                    //Line 1993: 2020 - 05 - 13 04:00:39.198 | [DI] | SV = 90.00, PV = 90.00, DP = -1.57 | 
                    //  PinAlign() : Succeed to find pin #5 : (-103509.29223037, 86964.5475064306, 1529.22265794495)
                    //pins.Add(new Vector3(34343, 128645, 1486));
                    //pins.Add(new Vector3(92445, 86944, 1462));
                    //pins.Add(new Vector3(92490, -93735, 1445));
                    //pins.Add(new Vector3(-103468, -93717, 1500));
                    //pins.Add(new Vector3(-103509, 86964, 1529));

                    //Line 2051: 2020 - 05 - 13 04:00:45.353 | [DI] | SV = 90.00, PV = 90.10, DP = -1.80 | 
                    //  PinAlign() : Succeed to find pin #1 : (34343.9442133624, 128645.813678039, 1486.00636465528)
                    //Line 2109: 2020 - 05 - 13 04:00:50.724 | [DI] | SV = 90.00, PV = 90.10, DP = -1.80 | 
                    //  PinAlign() : Succeed to find pin #2 : (92445.6135037575, 86944.5382067164, 1462.93194314552)
                    //Line 2166: 2020 - 05 - 13 04:00:57.396 | [DI] | SV = 90.00, PV = 90.10, DP = -1.80 | 
                    //  PinAlign() : Succeed to find pin #3 : (92490.9093055548, -93735.3394826104, 1445.89975803069)
                    //Line 1936: 2020 - 05 - 13 04:00:31.239 | [DI] | SV = 90.00, PV = 90.00, DP = -1.57 | 
                    //  PinAlign() : Succeed to find pin #4 : (-103468.470678033, -93717.0666717616, 1500.52445527894)
                    //Line 1993: 2020 - 05 - 13 04:00:39.198 | [DI] | SV = 90.00, PV = 90.00, DP = -1.57 | 
                    //  PinAlign() : Succeed to find pin #5 : (-103509.29223037, 86964.5475064306, 1529.22265794495)
                    //pins.Add(new Vector3(34351.6f, 128630.8f, 1486.006f));
                    //pins.Add(new Vector3(92448.0f, 86931.8f, 1462.931f));
                    //pins.Add(new Vector3(92493.1f, -93732.7f, 1445.899f));
                    //pins.Add(new Vector3(-103448.0f, -93713.9f, 1500.524f));
                    //pins.Add(new Vector3(-103487.8f, 86951.6f, 1529.222f));

                    //C12
                    //Line 2071: 2020 - 05 - 13 04:03:46.913 | [DI] | SV = 90.00, PV = 90.00, DP = -0.36 | 
                    //  PinAlign() : Succeed to find pin #1 : (35299.1954956859, 128200.999272854, 1521.30731612087)
                    //Line 2128: 2020 - 05 - 13 04:03:52.507 | [DI] | SV = 90.00, PV = 90.10, DP = -0.35 | 
                    //  PinAlign() : Succeed to find pin #2 : (93374.8585691032, 86458.4902982586, 1505.06105151388)
                    //Line 2185: 2020 - 05 - 13 04:03:58.538 | [DI] | SV = 90.00, PV = 90.10, DP = -0.35 | 
                    //  PinAlign() : Succeed to find pin #3 : (93295.8854477556, -94209.2697096947, 1501.69401641201)
                    //Line 1956: 2020 - 05 - 13 04:03:34.313 | [DI] | SV = 90.00, PV = 90.00, DP = -0.36 | 
                    //  PinAlign() : Succeed to find pin #4 : (-102632.292236341, -94055.680319644, 1545.54407507994)
                    //Line 2013: 2020 - 05 - 13 04:03:40.484 | [DI] | SV = 90.00, PV = 90.00, DP = -0.36 | 
                    //  PinAlign() : Succeed to find pin #5 : (-102551.71206922, 86610.7901448163, 1569.36850490807)
                    //pins.Add(new Vector3(35299, 128200, 1521));
                    //pins.Add(new Vector3(93374, 86458, 1505));
                    //pins.Add(new Vector3(93295, -94209, 1501));
                    //pins.Add(new Vector3(-102632, -94055, 1545));
                    //pins.Add(new Vector3(-102551, 86610, 1569));

                    //C10
                    //Line 2089: 2020 - 05 - 13 04:02:53.111 | [DI] | SV = 90.00, PV = 90.00, DP = -3.19 | 
                    //          PinAlign() : Succeed to find pin #1 : (35146.5440445273, 128982.297383796, 1473.35609302464)
                    //Line 2146: 2020 - 05 - 13 04:02:58.328 | [DI] | SV = 90.00, PV = 90.00, DP = -3.19 | 
                    //          PinAlign() : Succeed to find pin #2 : (93254.0954073436, 87294.8535947033, 1475.54707523673)
                    //Line 2203: 2020 - 05 - 13 04:03:03.720 | [DI] | SV = 90.00, PV = 90.10, DP = -3.17 | 
                    //          PinAlign() : Succeed to find pin #3 : (93300.7890621194, -93392.809221122, 1456.69074238815)
                    //Line 1972: 2020 - 05 - 13 04:02:41.922 | [DI] | SV = 90.00, PV = 90.00, DP = -3.17 | 
                    //          PinAlign() : Succeed to find pin #4 : (-102646.34925393, -93429.8289210233, 1447.68139313029)
                    //Line 2030: 2020 - 05 - 13 04:02:47.441 | [DI] | SV = 90.00, PV = 90.00, DP = -3.19 | 
                    //          PinAlign() : Succeed to find pin #5 : (-102695.468217046, 87253.3118759504, 1442.32895081703)
                    //pins.Add(new Vector3(35146, 128982, 1473));
                    //pins.Add(new Vector3(93254, 87294, 1475));
                    //pins.Add(new Vector3(93300, -93392, 1456));
                    //pins.Add(new Vector3(-102646, -93429, 1447));
                    //pins.Add(new Vector3(-102695, 87253, 1442));

                    //C09
                    //Line 2198: 2020-05-13 04:01:12.691 | PinAlign() : Succeed to find pin #1 : (35439.4697036851, 127701.60368703, 1534.80142276533)
                    //Line 2255: 2020-05-13 04:01:17.820 | PinAlign() : Succeed to find pin #2 : (93553.9832876126, 86006.9113997161, 1527.94314610823)
                    //Line 2312: 2020-05-13 04:01:23.052 | PinAlign() : Succeed to find pin #3 : (93599.0184616769, -94684.6298646577, 1521.2481360258)
                    //Line 2084: 2020-05-13 04:01:01.708 | PinAlign() : Succeed to find pin #4 : (-102369.66799297, -94716.5213646059, 1492.83342793691)
                    //Line 2141: 2020-05-13 04:01:07.056 | PinAlign() : Succeed to find pin #5 : (-102420.105150987, 85976.7804454184, 1526.02831492259)
                    //pins.Add(new Vector3(35439, 127701, 1534));
                    //pins.Add(new Vector3(93553, 86006, 1527));
                    //pins.Add(new Vector3(93599, -94684, 1521));
                    //pins.Add(new Vector3(-102369, -94716, 1492));
                    //pins.Add(new Vector3(-102420, 85976, 1526));

                    //C01
                    //Line 2218: 2020-05-13 03:59:39.506 | PinAlign() : Succeed to find pin #1 : (34122.9719306979, 128948.636173187, 1483.33415353965)
                    //Line 2277: 2020-05-13 03:59:44.946 | PinAlign() : Succeed to find pin #2 : (92229.5881121536, 87252.500517625, 1482.08427670595)
                    //Line 2335: 2020-05-13 03:59:50.581 | PinAlign() : Succeed to find pin #3 : (92265.4366588856, -93440.926984445, 1444.314546457)
                    //Line 2102: 2020-05-13 03:59:28.322 | PinAlign() : Succeed to find pin #4 : (-103679.951872174, -93464.5442139669, 1449.7217821958)
                    //Line 2160: 2020-05-13 03:59:33.709 | PinAlign() : Succeed to find pin #5 : (-103715.822292938, 87227.3133953337, 1485.11626786792)
                    //pins.Add(new Vector3(34122, 128948, 1483));
                    //pins.Add(new Vector3(92229, 87252, 1482));
                    //pins.Add(new Vector3(92265, -93440, 1444));
                    //pins.Add(new Vector3(-103679, -93464, 1449));
                    //pins.Add(new Vector3(-103715, 87227, 1485));

                    //CalcTiltedPlane(pins);
                    //ResetPlaneOffsets();    // Reset data for safety.
                    #endregion

                    SetStepSetupState();

                    return EventCodeEnum.NONE;
                }

                bool ManualSoakingWorking = false;

                if (this.StageSupervisor().StageMode == GPCellModeEnum.MAINTENANCE && this.SoakingModule().ManualSoakingStart)
                {
                    LoggerManager.SoakingLog($"Manual soaking - PinAlign(High Module)");
                    ManualSoakingWorking = true;
                }

                bool EnableStatusSoaking = false;
                bool IsGettingOptionSuccessul = this.SoakingModule().StatusSoakingParamIF.IsEnableStausSoaking(ref EnableStatusSoaking);

                if (false == IsGettingOptionSuccessul)
                {
                    LoggerManager.SoakingErrLog($"Failed to get 'IsEnableStausSoaking'");
                }

                if (EnableStatusSoaking || ManualSoakingWorking)
                {
                    if (PINALIGNSOURCE.SOAKING == _PinAlignSource && this.PinAligner().UseSoakingSamplePinAlign)
                    {
                        AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.CONTINUE;
                    }
                    else if (PINALIGNSOURCE.SOAKING == _PinAlignSource && false == this.PinAligner().UseSoakingSamplePinAlign)
                    {
                        AlignProcInfo.ProcSampleAlign = SAMPLEPINALGINRESULT.DISABLED;
                    }
                }

                FocusParam.FocusingCam.Value = EnumProberCam.PIN_HIGH_CAM;

                this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

                tmpEachResult.Clear();
                probeCardSysObject = this.StageSupervisor().ProbeCardInfo.ProbeCardSysObjectRef;

                // 백업된 핀 얼라인 데이터 사용 할건지 확인하는 로직
                bool usebackupAlignedOffset = false;

                if (probeCardSysObject.UsePinPosWithCardID.Value)
                {
                    IPinData PinData;
                    int totalpincount = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                    for (i = 0; i <= totalpincount - 1; i++)
                    {
                        PinData = this.StageSupervisor().ProbeCardInfo.GetPin(i);

                        this.StageSupervisor().ProbeCardInfo.GetArrayIndex(PinData.PinNum.Value - 1, out DutArrayIndex, out PinArrayIndex);

                        usebackupAlignedOffset = CheckPinDataAlignedOffset(PinData.AbsPos, DutArrayIndex + 1, PinData.PinNum.Value, _PinAlignSource);
                        if (usebackupAlignedOffset == false)
                            break;
                    }
                }

                while (true)
                {

                    // 얼라인 토큰 초기화
                    // 모든 Fail pin들은 1회의 얼라인 기회를 부여한다. 즉 아직 얼라인 하지 않은 핀들과 추가 기회를 가진 fail 핀들만 얼라인 한다.
                    //foreach (EachPinResult tmp_result_data in AlignResult.EachPinResultes)
                    //{
                    //    if (tmp_result_data.PinResult != PINALIGNRESULT.PIN_FORCED_PASS &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_NOT_PERFORMED &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_PASSED &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_SKIP &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_BLOB_FAILED &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_BLOB_TOLERANCE &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_FORCED_PASS &&
                    //        tmp_result_data.PinResult != PINALIGNRESULT.PIN_OVER_TOLERANCE)
                    //    {
                    //        tmp_result_data.AlignToken = true;
                    //    }
                    //    else
                    //    {
                    //        // Fail 핀들
                    //        tmp_result_data.AlignToken = false;
                    //    }
                    //}

                    if (this.MonitoringManager().IsSystemError == true)
                    {
                        LoggerManager.Debug($"PinAlign() : Operation faild because system error state [1]");
                        PinAlignProcState = PINPROCESSINGSTATE.ALIGN_FAILED;

                        this.PinAligner().ReasonOfError.AddEventCodeInfo(EventCodeEnum.SYSTEM_ERROR, "System error is occurred.", this.GetType().Name);
                        //this.PinAligner().ReasonOfError.Reason = "System error state";
                        break;
                    }

                    PinAlignProcState = GeneratePinSequence(_PinAlignSource, usebackupAlignedOffset);// RequiredAlignList를 업데이트 해주고 핀얼라인을 더해야하는지 말아야하는지 반환하는 역할.

                    if (PinAlignProcState != PINPROCESSINGSTATE.BE_CONTINUE || AlignProcInfo.RequiredAlignList.Count <= 0)
                    {
                        LoggerManager.Debug($"PinAlign(): GeneratePinSequence RetVal: {PinAlignProcState}, RequiredAlignList: {AlignProcInfo.RequiredAlignList.Count}", isInfo: IsInfo);
                        break;
                    }

                    for (i = 0; i <= AlignProcInfo.RequiredAlignList.Count - 1; i++)
                    {
                        tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(AlignProcInfo.RequiredAlignList[i]);

                        this.StageSupervisor().ProbeCardInfo.GetArrayIndex(tmpPinData.PinNum.Value - 1, out DutArrayIndex, out PinArrayIndex);

                        if (usebackupAlignedOffset == true)
                        {
                            tmpPinData.AlignedOffset += SetPinDataAlignedOffset(tmpPinData.AbsPos, AlignProcInfo.RequiredAlignList[i] + 1);
                        }

                        if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, this.CoordinateManager().PinHighPinConvert.ConvertBack(tmpPinData.AbsPos).X.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR ||
                            this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, this.CoordinateManager().PinHighPinConvert.ConvertBack(tmpPinData.AbsPos).Y.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR ||
                            this.MotionManager().CheckSWLimit(EnumAxisConstants.Z, this.CoordinateManager().PinHighPinConvert.ConvertBack(tmpPinData.AbsPos).Z.Value) == EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR)
                        {
                            LoggerManager.Error($"PinAlign() : SW Limit Error #" + tmpPinData.PinNum.Value);

                            retval = EventCodeEnum.PIN_SW_LIMIT;

                            this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, "Could not move to pin position : S/W Limit error", this.GetType().Name);

                            AlignResult.Result = retval;

                            return retval;
                        }
                        else
                        {
                            if (tmpPinData != null && DutArrayIndex >= 0 && (PinArrayIndex) >= 0)
                            {
                                // Micron TODO (if문 말고 나중에 따로 분류할 것)
                                if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.Cantilever_Standard)
                                {
                                    LoggerManager.Debug($"PinAlign() : pin #" + tmpPinData.PinNum.Value + $" Align Start,  position = ({tmpPinData.AbsPos.X.Value}, {tmpPinData.AbsPos.Y.Value}, {tmpPinData.AbsPos.Z.Value})", isInfo: IsInfo);
                                    retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value, tmpPinData.AbsPos.Z.Value);

                                    if (retval != EventCodeEnum.NONE)
                                    {
                                        this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, retval.ToString(), this.GetType().Name);

                                        return retval;
                                    }

                                    if (CurCam == null)
                                    {
                                        CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                                    }

                                    foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[DutArrayIndex].PinList[PinArrayIndex].PinSearchParam.LightForTip)
                                    {
                                        CurCam.SetLight(light.Type.Value, light.Value.Value);
                                    }
                                }
                                else if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
                                {
                                    if (tmpPinData.PinSearchParam.AlignKeyHigh != null && tmpPinData.PinSearchParam.AlignKeyHigh.Count > 0)
                                    {
                                        // 핀 위치로 이동 및 조명 조절
                                        LoggerManager.Debug($"PinAlign() : pin #" + tmpPinData.PinNum.Value + $" Align Start,  " +
                                                            $"position = ({tmpPinData.AbsPos.X.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value}, " +// 이거 하드코딩 버그? 의도된것?
                                                                        $"{tmpPinData.AbsPos.Y.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value}, " +
                                                                        $"{tmpPinData.AbsPos.Z.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value})", isInfo: IsInfo);

                                        // TODO : 첫 번째 Align Key만 적용 되어 있음

                                        retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.X.Value,
                                                                                                tmpPinData.AbsPos.Y.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Y.Value,
                                                                                                tmpPinData.AbsPos.Z.Value + tmpPinData.PinSearchParam.AlignKeyHigh[0].AlignKeyPos.Z.Value);
                                        if (retval != EventCodeEnum.NONE)
                                        {
                                            this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, retval.ToString(), this.GetType().Name);

                                            return retval;
                                        }

                                        if (CurCam == null)
                                        {
                                            CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                                        }

                                        // Key에 할당된 Light 값을 사용
                                        if (tmpPinData.PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams == null)
                                        {
                                            LoggerManager.Error($"PinAlign() : Failed, LightParam is wrong.");

                                            result = EventCodeEnum.PIN_ALIGN_FAILED;
                                        }
                                        else
                                        {
                                            foreach (var light in tmpPinData.PinSearchParam.AlignKeyHigh[0].PatternIfo.LightParams)
                                            {
                                                CurCam.SetLight(light.Type.Value, light.Value.Value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Key 정보가 존재하지 않는 경우.

                                        retval = EventCodeEnum.PIN_HIGH_KEY_INVAILD;

                                        this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, "High Key information is invalid.", this.GetType().Name);

                                        AlignResult.Result = retval;

                                        return retval;
                                    }
                                }
                                else if (this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value == PROBECARD_TYPE.VerticalType)
                                {
                                    if (tmpPinData.PinSearchParam.BaseParam != null)
                                    {
                                        if ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.BaseFocsuingEnable.Value == BaseFocusingEnable.ENABLE)
                                        {
                                            // Base 위치로 이동 및 조명 조절
                                            LoggerManager.Debug($"PinAlign() : pin #" + tmpPinData.PinNum.Value + $" Align Start,  " +
                                                                $"position = ({tmpPinData.AbsPos.X.Value}, " +
                                                                            $"{tmpPinData.AbsPos.Y.Value}, " +
                                                                            $"{tmpPinData.AbsPos.Z.Value + tmpPinData.PinSearchParam.BaseParam.DistanceBaseAndTip})", isInfo: IsInfo);

                                            double BaseOffsetX = tmpPinData.PinSearchParam.BaseParam.BaseOffsetX;
                                            double BaseOffsetY = tmpPinData.PinSearchParam.BaseParam.BaseOffsetY;
                                            double DistanceBaseAndTip = tmpPinData.PinSearchParam.BaseParam.DistanceBaseAndTip;

                                            // Setting된 Base위치로 이동
                                            retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value + BaseOffsetX, tmpPinData.AbsPos.Y.Value + BaseOffsetY, tmpPinData.AbsPos.Z.Value + DistanceBaseAndTip);

                                            if (retval != EventCodeEnum.NONE)
                                            {
                                                this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, retval.ToString(), this.GetType().Name);
                                                return retval;
                                            }

                                            if (CurCam == null)
                                            {
                                                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                                            }

                                            if (((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams == null)
                                            {
                                                LoggerManager.Error($"PinAlign() : Failed, LightParam is wrong.");

                                                result = EventCodeEnum.PIN_ALIGN_FAILED;
                                            }
                                            else
                                            {
                                                foreach (var light in ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                                                {
                                                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // pin tip 위치로 이동 및 조명 조절
                                            LoggerManager.Debug($"PinAlign() : pin #" + tmpPinData.PinNum.Value + $" Align Start,  " +
                                                                $"position = ({tmpPinData.AbsPos.X.Value}, " +
                                                                            $"{tmpPinData.AbsPos.Y.Value}, " +
                                                                            $"{tmpPinData.AbsPos.Z.Value})", isInfo: IsInfo);

                                            // pin tip 위치로 이동
                                            retval = this.StageSupervisor().StageModuleState.PinHighViewMove(tmpPinData.AbsPos.X.Value,
                                                                                                    tmpPinData.AbsPos.Y.Value,
                                                                                                    tmpPinData.AbsPos.Z.Value);
                                            if (retval != EventCodeEnum.NONE)
                                            {
                                                this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, retval.ToString(), this.GetType().Name);
                                                return retval;
                                            }

                                            if (CurCam == null)
                                            {
                                                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);
                                            }

                                            if (((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams == null)
                                            {
                                                LoggerManager.Error($"PinAlign() : Failed, LightParam is wrong.");

                                                result = EventCodeEnum.PIN_ALIGN_FAILED;
                                            }
                                            else
                                            {
                                                // TODO : PinHighAlignBaseFocusingParam 에서 저장되는 LightParams은 베이스 포커싱에 관한 정보 pin 조명은 threshold 설정하는 단계에서 사용할는 것으로 알고 있는데 이부분 확인 필요
                                                //foreach (var light in ((this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinHighAlignBaseFocusingParam.FocusingParam as FocusParameter).LightParams)
                                                //{
                                                //    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                                //}
                                                foreach (var light in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList[DutArrayIndex].PinList[PinArrayIndex].PinSearchParam.LightForTip)
                                                {
                                                    CurCam.SetLight(light.Type.Value, light.Value.Value);
                                                }
                                            }
                                        }
                                    }
                                }

                                // 실제 핀 얼라인
                                EachPinResult = this.PinAligner().SinglePinAligner.SinglePinalign(out NewPinPos, tmpPinData, this.PinFocusModel, FocusParam);
                                CurPinIndex = i;

                                if (PinResultTestFlag == true)
                                {
                                    PinAlignSettignParameter CurrentPinSetting = PinAlignParam.PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == _PinAlignSource);

                                    if (CurrentPinSetting == null)
                                    {
                                        LoggerManager.Error("[PinHighAlignModule] PinAlign() : PinAlignSettignParameter is null value.");
                                    }

                                    double EachPinToleranceX = CurrentPinSetting.EachPinToleranceX.Value;
                                    double EachPinToleranceY = CurrentPinSetting.EachPinToleranceY.Value;
                                    double EachPinToleranceZ = CurrentPinSetting.EachPinToleranceZ.Value;

                                    Random r = new Random();

                                    double OffsetX = r.Next(0, (int)(EachPinToleranceX * 2 + 1));
                                    int signX = r.Next(0, 2) == 0 ? -1 : 1;

                                    double OffsetY = r.Next(0, (int)(EachPinToleranceY * 2 + 1));
                                    int signY = r.Next(0, 2) == 0 ? -1 : 1;

                                    double OffsetZ = r.Next(0, (int)(EachPinToleranceZ * 2 + 1));
                                    int signZ = r.Next(0, 2) == 0 ? -1 : 1;

                                    NewPinPos.X.Value = tmpPinData.AbsPos.X.Value + (signX * OffsetX);
                                    NewPinPos.Y.Value = tmpPinData.AbsPos.Y.Value + (signY * OffsetY);
                                    NewPinPos.Z.Value = tmpPinData.AbsPos.Z.Value + (signZ * OffsetZ);
                                }

                                if (EachPinResult != PINALIGNRESULT.PIN_PASSED)
                                {
                                    // 얼라인 FAIL
                                    TotalFailCnt += 1;

                                    result = EventCodeEnum.PIN_ALIGN_FAILED;

                                    tmpPinData.Result.Value = EachPinResult;// fail이든 말든 방금 align한 값들을 tmpPinData에 넣어둠.
                                    LoggerManager.Error($"PinAlign() : Failed to find pin #{tmpPinData.PinNum.Value} : {EachPinResult.ToString()}");
                                    tmpEachResult.Add(new EachPinResult(tmpPinData.DutNumber.Value, 0, 0, 0, 0, tmpPinData.DutMacIndex.Value, tmpPinData.PinNum.Value, tmpPinData.GroupNum.Value, result, EachPinResult, false));
                                }
                                else
                                {
                                    // 얼라인 PASS
                                    TotalPassCnt += 1;
                                    if (tmpPinData.PinTipResult.Value == PINALIGNRESULT.PIN_PASSED)
                                    {
                                        TotalPinTipPassCnt += 1;
                                    }

                                    result = EventCodeEnum.NONE;

                                    tmpPinData.Result.Value = EachPinResult;// fail이든 말든 방금 align한 값들을 tmpPinData에 넣어둠.

                                    LoggerManager.Debug($"PinAlign() : Succeed to find pin #{tmpPinData.PinNum.Value} : ({NewPinPos.X.Value}, {NewPinPos.Y.Value}, {NewPinPos.Z.Value})", isInfo: IsInfo);

                                    tmpEachResult.Add(new EachPinResult(tmpPinData.DutNumber.Value,
                                        diffX: tmpPinData.AbsPos.X.Value - NewPinPos.X.Value,
                                        diffY: tmpPinData.AbsPos.Y.Value - NewPinPos.Y.Value,
                                        diffZ: tmpPinData.AbsPos.Z.Value - NewPinPos.Z.Value,
                                        height: NewPinPos.Z.Value,
                                        dutmachineindex: tmpPinData.DutMacIndex.Value,
                                        pinnum: tmpPinData.PinNum.Value,
                                        group: tmpPinData.GroupNum.Value,
                                        status: result,
                                        result: EachPinResult,
                                        alignToken: false,
                                        tipoptresult: tmpPinData.PinTipResult.Value));
                                }
                            }
                            else
                            {
                                // 찾는 핀이 리스트에 없음
                                LoggerManager.Error($"PinAlign() : Failed to find pin #{i} in registered pin list");

                                retval = EventCodeEnum.PIN_INVALID_LIST;

                                this.PinAligner().ReasonOfError.AddEventCodeInfo(retval, "Could not move to pin position : Invalid pin number", this.GetType().Name);

                                return retval;
                            }
                        }
                    }

                    // 얼라인 결과 업데이트를 AlignResult.EachPinResultes로함
                    foreach (EachPinResult result_data in tmpEachResult)
                    {
                        result_data.AlignToken = false; // false이므로 MakeAlignList에 핀넘버가 추가되지 않음.

                        idx = AlignResult.EachPinResultes.FindIndex(pin => pin.PinNum == result_data.PinNum);

                        if (idx == -1)
                        {
                            AlignResult.EachPinResultes.Add(result_data);
                        }
                        else
                        {
                            // 원래는 로직상 발생할 수 없는 경우... 일단 예외 처리 해 둠. 혹시 이전에 얼라인했던 핀을 다시 하는 경우가 발생하면 이전 데이터를 지우고 갱신한다.
                            AlignResult.EachPinResultes.RemoveAt(idx);
                            AlignResult.EachPinResultes.Add(result_data);
                        }
                    }

                    AlignResult.TotalAlignPinCount = TotalFailCnt + TotalPassCnt;
                    AlignResult.TotalFailPinCount = TotalFailCnt;
                    AlignResult.TotalPassPinCount = TotalPassCnt;

                    AlignResult.PassPercentage = Convert.ToInt32(TotalPassCnt * 100 / (TotalFailCnt + TotalPassCnt));

                    LoggerManager.Debug($"Total align count : {AlignResult.TotalAlignPinCount}, Pass : {AlignResult.TotalPassPinCount}, Fail : {AlignResult.TotalFailPinCount}, Pass percenage : {AlignResult.PassPercentage}", isInfo: IsInfo);
                }

                if (PinAlignProcState == PINPROCESSINGSTATE.ALIGN_PASSED && PinAlignParam.PinHighAlignParam.PinTipFocusEnable.Value)
                {
                    var pintipOptionPassed = ((double)TotalPinTipPassCnt / (double)TotalPassCnt) * 100;

                    if (PinAlignParam.PinHighAlignParam.PinTipFocusPassPercentage.Value <= 0)
                    {
                        PinAlignParam.PinHighAlignParam.PinTipFocusPassPercentage.Value = 100;
                    }

                    if (pintipOptionPassed < PinAlignParam.PinHighAlignParam.PinTipFocusPassPercentage.Value)//pintip result check
                    {
                        PinAlignProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                        AlignResult.Result = EventCodeEnum.PIN_TIP_ALIGN_FAILED;
                        LoggerManager.Debug($"PinAlign(): PIN_TIP_ALIGN_FAILED, TotalPassCnt:{TotalPassCnt}, TotalPinTipPassCnt:{TotalPinTipPassCnt}, pintipOptionPassed:{pintipOptionPassed}");
                    }
                    else
                    {
                        if (PinAlignParam.PinHighAlignParam.PinTipSizeValidationEnable.Value)
                        {
                            if (this.PinAligner().ValidPinTipSize_OutOfSize_List.Count > 0)
                            {
                                // OFF
                                if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_Lot_Pause_Option.Value == false)
                                {
                                    PinAlignProcState = PINPROCESSINGSTATE.ALIGN_PASSED;
                                    AlignResult.Result = EventCodeEnum.PIN_TIP_ALIGN_PASSED;
                                }
                                //ON
                                else
                                {
                                    PinAlignProcState = PINPROCESSINGSTATE.ALIGN_FAILED;
                                    AlignResult.Result = EventCodeEnum.PIN_TIP_SIZE_VALIDATION_FAIL;
                                }

                                this.NotifyManager().Notify(EventCodeEnum.PIN_TIP_SIZE_VALIDATION_FAIL);
                                this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, $"Pin Tip Size Min/Max Tolerance error is occurred. Pin Number : { string.Join(", ", this.PinAligner().ValidPinTipSize_OutOfSize_List)}", this.GetType().Name);
                                LoggerManager.PinLog($"PinAlign(): A pin tip that is not the appropriate size has been detected. Pin Number : { string.Join(", ", this.PinAligner().ValidPinTipSize_OutOfSize_List)}");
                            }
                            else
                            {
                                PinAlignProcState = PINPROCESSINGSTATE.ALIGN_PASSED;
                                AlignResult.Result = EventCodeEnum.PIN_TIP_ALIGN_PASSED;
                                this.NotifyManager().ClearNotify(EventCodeEnum.PIN_TIP_SIZE_VALIDATION_FAIL);

                                LoggerManager.Debug($"PinAlign(): PIN_TIP_ALIGN_PASSED, TotalPassCnt:{TotalPassCnt}, TotalPinTipPassCnt:{TotalPinTipPassCnt}, pintipOptionPassed:{pintipOptionPassed}");
                            }

                            // 업로드 파라미터가 켜져있고, Save Image 옵션이 둘중 하나라도 켜져 있을 경우에만 올린다.
                            // 둘다 꺼져있을 경우에는 새로 Save된 이미지가 없기 때문에 업로드 될 이미지가 어차피 없긴 하다.
                            if (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_UploadToServer.Value &&
                                (PinAlignParam.PinHighAlignParam.PinTipSizeValidation_OutOfSize_Image.Value || PinAlignParam.PinHighAlignParam.PinTipSizeValidation_SizeInRange_Image.Value))
                            {
                                // 밑에 PinAlignResultServerUpload 함수를 Task로 띄우고 현재 함수 끝에서 List를 Clear해주기 때문에 객체를 새로 생성하고 값을 복사 한 뒤 넘겨준다.
                                List<PinSizeValidateResult> originList = this.PinAligner().ValidPinTipSize_Original_List.ToList();
                                List<PinSizeValidateResult> outList = this.PinAligner().ValidPinTipSize_OutOfSize_List.ToList();
                                List<PinSizeValidateResult> inList = this.PinAligner().ValidPinTipSize_SizeInRange_List.ToList();
                                // 서버 업로드 부분은 시간 소요가 있기 때문에 PinAlign 자체에 딜레이를 주지 않기 위해 Task 처리 함.
                                Task task = new Task(() =>
                                {
                                    this.PinAligner().PinAlignResultServerUpload(originList, outList, inList);
                                });
                                task.Start();
                            }
                            else
                            {
                                LoggerManager.Debug($"PinAlign(): PinTipSizeValidation_UploadToServer is False");
                            }
                        }
                        else
                        {
                            PinAlignProcState = PINPROCESSINGSTATE.ALIGN_PASSED;
                            AlignResult.Result = EventCodeEnum.PIN_TIP_ALIGN_PASSED;
                            LoggerManager.Debug($"PinAlign(): PIN_TIP_ALIGN_PASSED, TotalPassCnt:{TotalPassCnt}, TotalPinTipPassCnt:{TotalPinTipPassCnt}, pintipOptionPassed:{pintipOptionPassed}");
                        }
                    }
                }

                // 얼라인 결과에 따라 핀 모듈 스테이트 갱신
                if (PinAlignProcState != PINPROCESSINGSTATE.ALIGN_PASSED)
                {
                    LoggerManager.Debug($"PinAlign() : Pin Align Fail, {AlignResult.Result}");
                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);

                    LoggerManager.Debug($"PinAlign() : State Transition [Recovery]");
                    SubModuleState.SubModuleStateTransition(SubModuleStateEnum.RECOVERY);

                    // 실패한 이유를 살펴보는 부분

                    // 이곳으로 들어오는 경우 중, GeneratePinSequence() 함수에서 아래의 경우가 발생할 수 있다.

                    // AlignResult.Result = EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE;
                    // AlignResult.Result = EventCodeEnum.PIN_CARD_CENTER_TOLERANCE;
                    // AlignResult.Result = EventCodeEnum.PIN_EACH_PIN_TOLERANCE_FAIL;

                    if ((AlignResult.Result == EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE) ||
                        (AlignResult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE) ||
                        (AlignResult.Result == EventCodeEnum.PIN_EACH_PIN_TOLERANCE_FAIL))
                    {
                        if (AlignResult.Result == EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE)
                        {
                            this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, $"Z Min/Max Tolerance error is occurred.", this.GetType().Name);
                            this.NotifyManager().Notify(EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE);
                        }
                        else if (AlignResult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE)
                        {
                            this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, "Card Center Tolerance error is occurred.", this.GetType().Name);
                            this.NotifyManager().Notify(EventCodeEnum.PIN_CARD_CENTER_TOLERANCE);
                        }
                        else if (AlignResult.Result == EventCodeEnum.PIN_EACH_PIN_TOLERANCE_FAIL)
                        {
                            this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, "Each Pin Tolerance error is occurred.", this.GetType().Name);
                            this.NotifyManager().Notify(EventCodeEnum.PIN_EACH_PIN_TOLERANCE_FAIL);
                        }

                        retval = AlignResult.Result;
                    }
                    else
                    {
                        foreach (EachPinResult result_data in tmpEachResult)
                        {
                            switch (result_data.PinResult)
                            {
                                case PINALIGNRESULT.PIN_PASSED:
                                case PINALIGNRESULT.PIN_FORCED_PASS:

                                    retval = EventCodeEnum.NONE;

                                    break;

                                case PINALIGNRESULT.PIN_NOT_PERFORMED:
                                case PINALIGNRESULT.PIN_SKIP:
                                case PINALIGNRESULT.PIN_OVER_TOLERANCE:

                                    this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, $"Pin alignment error : Undefined error code {result_data.PinResult}", this.GetType().Name);

                                    LoggerManager.Debug($"Abnormal error code in pinalignment process : {result_data.PinResult.ToString()}", isInfo: IsInfo);

                                    retval = EventCodeEnum.PIN_ALIGN_FAILED;
                                    break;

                                case PINALIGNRESULT.PIN_FOCUS_FAILED:
                                case PINALIGNRESULT.PIN_TIP_FOCUS_FAILED:

                                    this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, "Pin focusing error is occurred. Please check pin position", this.GetType().Name);

                                    retval = EventCodeEnum.PIN_FOCUS_FAILED;
                                    this.NotifyManager().Notify(EventCodeEnum.PIN_FOCUS_FAILED);
                                    break;

                                case PINALIGNRESULT.PIN_BLOB_FAILED:
                                case PINALIGNRESULT.PIN_BLOB_TOLERANCE:

                                    this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, "Find pin error is occurred. Please check searching parameters and brightness", this.GetType().Name);

                                    retval = EventCodeEnum.PIN_FIND_PIN_FAIL;
                                    this.NotifyManager().Notify(EventCodeEnum.PIN_FIND_PIN_FAIL);
                                    break;
                                default:
                                    break;
                            }

                            if (retval != EventCodeEnum.NONE)
                            {
                                break;
                            }
                        }
                    }

                    // TODO: 예외 Case 없나요?

                    if (retval == EventCodeEnum.NONE)
                    {
                        // 이곳으로 들어왔다는 것은, 모든 핀이 PINALIGNRESULT.PIN_PASSED 또는 PINALIGNRESULT.PIN_FORCED_PASS 값을 갖고 있을 때
                        // 예외가 발생하는 경우를 찾기 위해 아래의 로그를 추가 해놓음.
                        //LoggerManager.Debug($"Result = {AlignResult.Result}, Reason = {this.PinAligner().ReasonOfError.Reason}");
                        LoggerManager.Debug($"Result = {AlignResult.Result}");

                        //this.PinAligner().ReasonOfError.Reason = $"Pin alignment error : Abnormal result data";
                        this.PinAligner().ReasonOfError.AddEventCodeInfo(AlignResult.Result, "Pin Alignment error is occurred. Abnormal result data.", this.GetType().Name);

                        // 여기까지 왔는데, FAIL된 결과가 없다? 즉 다시 말해서 핀 얼라인 해보지도 않고 여기까지 내려 왔다는 이야기, 플래그 초기화가 제대로 안 되어 있을듯
                        LoggerManager.Debug("Abnormal state in pinalignment process : alignment result is empty.");

                        retval = EventCodeEnum.PIN_ALIGN_FAILED;
                    }

                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_High_Mag_Align_Fail, retval);
                }
                else
                {
                    // 얼라인 결과 업데이트

                    #region Result_updating
                    LoggerManager.Debug($"PinAlign() : Pin Align Succeed");

                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.DONE);
                    AlignResult.Result = EventCodeEnum.NONE;
                    if (AlignProcInfo.ProcSampleAlign != SAMPLEPINALGINRESULT.PASS)
                    {
                        // 샘플 얼라인 된 결과값은 데이터에 반영하지 않는다

                        avr_changed_x = 0;
                        avr_changed_y = 0;
                        avr_changed_z = 0;

                        foreach (EachPinResult result_data in AlignResult.EachPinResultes)// 실제로 데이터를 업데이트하는곳.
                        {
                            if (result_data.PinResult == PINALIGNRESULT.PIN_PASSED)
                            {
                                avr_changed_x += result_data.DiffX;
                                avr_changed_y += result_data.DiffY;
                                avr_changed_z += result_data.DiffZ;
                                aligned_cnt += 1;
                            }
                        }

                        probeCardSysObject.ProbercardinfoClear();

                        ProbercardBackupinfo probercardsysinfo = new ProbercardBackupinfo();
                        BackupPinData backupPinData;

                        probercardsysinfo.ProbeCardID = this.CardChangeModule().GetProbeCardID();
                        probercardsysinfo.BackupPinDataList = new List<BackupPinData>();

                        if (aligned_cnt > 0)
                        {
                            avr_changed_x = avr_changed_x / aligned_cnt * -1;
                            avr_changed_y = avr_changed_y / aligned_cnt * -1;
                            avr_changed_z = avr_changed_z / aligned_cnt * -1;

                            // 얼라인에 사용된 핀들은 마지막 얼라인 후 구해진 Diff값을 적용해야 한다.
                            // 얼라인에 사용되지 않은 핀들은 패스된 핀들의 평균 변화량 만큼을 더해준다.
                            // 따라서, 모든 핀들에 접근 및 계산 해줘야 함.
                            StringBuilder stb = new StringBuilder();

                            foreach (Dut DutList in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                            {
                                foreach (PinData pin in DutList.PinList)
                                {
                                    // PinNo를 이용하여, AlignResult.EachPinResultes에 포함된 핀인지 아닌지 구분 
                                    backupPinData = new BackupPinData();
                                    EachPinResult AlignedPinData = null;

                                    AlignedPinData = AlignResult.EachPinResultes.FirstOrDefault(n => n.PinNum == pin.PinNum.Value);

                                    // 현재 핀에 적용해야 할 보상량을 고르고 핀 값에 적용한다.
                                    tmp_cur_result = PINALIGNRESULT.PIN_NOT_PERFORMED;

                                    if (AlignedPinData != null)
                                    {
                                        tmp_cur_result = AlignedPinData.PinResult;

                                        if (AlignedPinData.PinResult == PINALIGNRESULT.PIN_PASSED)
                                        {
                                            // 패스된 핀은 얼라인 한 결과를 직접 대입한다.
                                            tmpDiffX = AlignedPinData.DiffX * -1;
                                            tmpDiffY = AlignedPinData.DiffY * -1;
                                            tmpDiffZ = AlignedPinData.DiffZ * -1;
                                        }
                                        else if (AlignedPinData.PinResult == PINALIGNRESULT.PIN_NOT_PERFORMED ||
                                                 AlignedPinData.PinResult == PINALIGNRESULT.PIN_SKIP)
                                        {
                                            // 얼라인하지않은 핀은 패스된 핀들의 평균 변화량 만큼을 더해준다.
                                            // 이곳에 들어오지 않을 것이다
                                            // TODO : 확인 필요
                                            tmpDiffX = avr_changed_x;
                                            tmpDiffY = avr_changed_y;
                                            tmpDiffZ = avr_changed_z;
                                        }
                                        else if (AlignedPinData.PinResult == PINALIGNRESULT.PIN_FORCED_PASS)
                                        {
                                            // 강제 PASS인 경우, 즉 사용자가 직접 위치를 지정한 경우에는 추가로 업데이트를 지정하지 않는다.
                                            tmpDiffX = 0;
                                            tmpDiffY = 0;
                                            tmpDiffZ = 0;
                                        }
                                        else
                                        {
                                            // 이곳에 들어오지 않을 것이다
                                            // TODO : 확인 필요

                                            tmpDiffX = avr_changed_x;
                                            tmpDiffY = avr_changed_y;
                                            tmpDiffZ = avr_changed_z;
                                        }
                                    }
                                    else
                                    {
                                        // 얼라인하지않은 핀은 패스된 핀들의 평균 변화량 만큼을 더해준다.

                                        tmpDiffX = avr_changed_x;
                                        tmpDiffY = avr_changed_y;
                                        tmpDiffZ = avr_changed_z;
                                    }

                                    if (_PinAlignSource == PINALIGNSOURCE.PIN_REGISTRATION)
                                    {
                                        pin.AbsPosOrg.X.Value = pin.AbsPosOrg.X.Value + pin.AlignedOffset.X.Value + tmpDiffX;
                                        pin.AbsPosOrg.Y.Value = pin.AbsPosOrg.Y.Value + pin.AlignedOffset.Y.Value + tmpDiffY;
                                        pin.AbsPosOrg.Z.Value = pin.AbsPosOrg.Z.Value + pin.AlignedOffset.Z.Value + tmpDiffZ;

                                        pin.AlignedOffset.X.Value = 0;
                                        pin.AlignedOffset.Y.Value = 0;
                                        pin.AlignedOffset.Z.Value = 0;

                                        pin.MarkCumulativeCorrectionValue.X.Value = 0;
                                        pin.MarkCumulativeCorrectionValue.Y.Value = 0;
                                        pin.MarkCumulativeCorrectionValue.Z.Value = 0;

                                        pin.LowCompensatedOffset.X.Value = 0;
                                        pin.LowCompensatedOffset.Y.Value = 0;
                                        pin.LowCompensatedOffset.Z.Value = 0;

                                        pin.Result.Value = tmp_cur_result;
                                    }
                                    else
                                    {
                                        if (_PinAlignSource == PINALIGNSOURCE.DEVICE_CHANGE || _PinAlignSource == PINALIGNSOURCE.CARD_CHANGE)
                                        {
                                            pin.AlignedOffset.X.Value += tmpDiffX;
                                            pin.AlignedOffset.Y.Value += tmpDiffY;
                                            pin.AlignedOffset.Z.Value += tmpDiffZ;

                                            pin.LowCompensatedOffset.X.Value += tmpDiffX;
                                            pin.LowCompensatedOffset.Y.Value += tmpDiffY;
                                            pin.LowCompensatedOffset.Z.Value += tmpDiffZ;

                                            pin.Result.Value = tmp_cur_result;
                                        }
                                        else
                                        {
                                            pin.AlignedOffset.X.Value += tmpDiffX;
                                            pin.AlignedOffset.Y.Value += tmpDiffY;
                                            pin.AlignedOffset.Z.Value += tmpDiffZ;
                                            pin.Result.Value = tmp_cur_result;
                                        }
                                    }

                                    stb.Append($"{ pin.PinNum.Value}, ");
                                    backupPinData.DutNo = pin.DutNumber.Value;
                                    backupPinData.PinNo = pin.PinNum.Value;
                                    backupPinData.BackupAbsPos = pin.AbsPos;
                                    probercardsysinfo.BackupPinDataList.Add(backupPinData);
                                }
                            }
                            if (stb != null && stb.Length != 0)
                            {
                                LoggerManager.Debug($"Pin Processing Order : {stb.ToString().Substring(0, stb.ToString().Length - 2)}");
                            }

                            //ISSD-4317 핀얼라인 결과 값 핀 번호 순서대로 로그 찍기.
                            IPinData pinForLogging = null;
                            PinAlignResultes AlignResultForLogging = (this.PinAligner().PinAlignInfo as PinAlignInfo).AlignResult;

                            if (AlignResultForLogging != null)
                            {
                                if (AlignResult.EachPinResultes.Count != 0)
                                {
                                    List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                                    foreach (var resultData in eachPinResultsSortList)
                                    {
                                        foreach (Dut DutList in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                                        {
                                            if (resultData != null)
                                            {
                                                pinForLogging = DutList.PinList.FirstOrDefault(x => x.PinNum.Value == resultData.PinNum);
                                            }

                                            if (pinForLogging != null)
                                            {
                                                if (resultData != null)
                                                {
                                                    switch (resultData.PinResult)
                                                    {
                                                        case PINALIGNRESULT.PIN_PASSED:
                                                            strResult = "[P]";
                                                            break;
                                                        case PINALIGNRESULT.PIN_NOT_PERFORMED:
                                                            strResult = "[-]";
                                                            break;
                                                        case PINALIGNRESULT.PIN_SKIP:
                                                            strResult = "[-]";
                                                            break;
                                                        case PINALIGNRESULT.PIN_FORCED_PASS:
                                                            strResult = "[M]";
                                                            break;
                                                        default:
                                                            strResult = "[F]";
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    strResult = "[-]";
                                                }

                                                LoggerManager.PinLog($"PinAlign() : {strResult} Pin #{pinForLogging.PinNum.Value} " + $" Dut #{pinForLogging.DutNumber.Value}, " +
                                                                        $"position is updated : ({pinForLogging.AbsPosOrg.X.Value + pinForLogging.AlignedOffset.X.Value:0.00}, " +
                                                                                               $"{pinForLogging.AbsPosOrg.Y.Value + pinForLogging.AlignedOffset.Y.Value:0.00}, " +
                                                                                               $"{pinForLogging.AbsPosOrg.Z.Value + pinForLogging.AlignedOffset.Z.Value:0.00}) " +
                                                                        $"Machine position is updated : ({pinForLogging.MachineCoordPos.X.Value:0.00}, " +
                                                                                               $"{pinForLogging.MachineCoordPos.Y.Value:0.00}, " +
                                                                                               $"{pinForLogging.MachineCoordPos.Z.Value:0.00}) " +
                                                                        $"aligned offset : ({pinForLogging.AlignedOffset.X.Value:0.00}, " +
                                                                                               $"{pinForLogging.AlignedOffset.Y.Value:0.00}, " +
                                                                                               $"{pinForLogging.AlignedOffset.Z.Value:0.00})");
                                            }
                                        }
                                    }
                                }
                            }

                            if (probeCardSysObject.UsePinPosWithCardID.Value)
                            {
                                var firstdut = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Where(x => x.DutNumber == 1).FirstOrDefault();
                                if (firstdut != null)
                                {
                                    probercardsysinfo.FirstDutMI = firstdut.MacIndex;
                                }
                                probercardsysinfo.TotalDutCnt = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count();
                                probercardsysinfo.TotalPinCnt = this.StageSupervisor().ProbeCardInfo.GetPinCount();
                                probeCardSysObject.ProbercardBackupinfo = probercardsysinfo;
                            }
                        }
                        double pinHeight = 0.0;
                        string pinHeightLabel = string.Empty;

                        switch (PinAlignParam.UserPinHeight.Value)
                        {
                            case USERPINHEIGHT.LOWEST:
                                pinHeight = this.StageSupervisor().ProbeCardInfo.CalcLowestPin();
                                pinHeightLabel = "Lowest";
                                break;

                            case USERPINHEIGHT.HIGHEST:
                                pinHeight = this.StageSupervisor().ProbeCardInfo.CalcHighestPin();
                                pinHeightLabel = "Highest";
                                break;

                            case USERPINHEIGHT.AVERAGE:
                                pinHeight = this.StageSupervisor().ProbeCardInfo.CalcPinAverageHeight();
                                pinHeightLabel = "Average";
                                break;
                        }

                        // 핀 높이 업데이트
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinHeight = pinHeight;
                        LoggerManager.Debug($"PinAlign() : Pin height = {pinHeight} ({pinHeightLabel})", isInfo: IsInfo);

                        // 핀 센터 계산
                        this.StageSupervisor().ProbeCardInfo.CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);

                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX = tmpcenx;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY = tmpceny;

                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX = tmpdutcenx;
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY = tmpdutceny;

                        LoggerManager.Debug($"PinAlign() : pin cen = ({this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenX}, {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinCenY}), " +
                                                         $"dut cen = ({this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenX}, {this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutCenY})", isInfo: IsInfo);

                        SaveProbeCardData();

                        if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                        {
                            SubModuleState.SubModuleStateTransition(SubModuleStateEnum.DONE);

                            if (this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || this.NeedleCleaner().LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                            {
                                if (PinAlignParam.PinAlignInterval.WaferIntervalCount.Value <= ((long)this.LotOPModule().SystemInfo.WaferCount - PinAlignParam.PinAlignInterval.MarkedWaferCountVal))
                                {
                                    PinAlignParam.PinAlignInterval.MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount;
                                }

                                if (PinAlignParam.PinAlignInterval.DieInterval.Value <= ((long)this.LotOPModule().SystemInfo.DieCount - PinAlignParam.PinAlignInterval.MarkedDieCountVal))
                                {
                                    PinAlignParam.PinAlignInterval.MarkedDieCountVal = (long)this.LotOPModule().SystemInfo.DieCount;
                                }

                                PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer = true;
                            }

                            this.PinAligner().IsRecoveryStarted = false;

                            PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterCardChange = true;
                            PinAlignParam.PinAlignInterval.FlagAlignProcessedAfterDeviceChange = true;

                            this.PinAligner().LastAlignDoneTime = DateTime.Now;

                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_High_Mag_Align_OK);
                        }
                        else
                        {
                            SubModuleState.SubModuleStateTransition(SubModuleStateEnum.RECOVERY);
                            LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_High_Mag_Align_Fail);
                            retval = EventCodeEnum.Pin_High_Mag_Align_Fail;
                        }

                        // 핀 얼라인이 성공했을 때, 틸트 옵션이 켜져있는 경우
                        LoggerManager.Debug($"PinAlign() : EnablePinPlaneCompensation = {PinAlignParam.PinPlaneAdjustParam.EnablePinPlaneCompensation.Value}");
                        if (PinAlignParam.PinPlaneAdjustParam.EnablePinPlaneCompensation.Value == true)
                        {

                            List<EachPinResult> PassedPinResults = AlignResult.EachPinResultes.FindAll(x => x.PinResult == PINALIGNRESULT.PIN_PASSED).ToList();
                            List<IPinData> PassedPinDatas = new List<IPinData>();
                            List<IPinData> SimilarHeightGroup = new List<IPinData>();

                            foreach (var pin in PassedPinResults)
                            {
                                IPinData tmp = this.ProbeCard.GetPin(pin.PinNum - 1);

                                if (tmp != null)
                                {
                                    PassedPinDatas.Add(tmp);
                                }
                                else
                                {
                                    LoggerManager.Error($"Get Pin Error");
                                }
                            }
                            if (PassedPinDatas.Count >= 3)
                            {
                                List<Vector3> pins = new List<Vector3>();

                                pins.Clear();

                                foreach (var passedPin in PassedPinDatas)
                                {
                                    pins.Add(new Vector3((float)passedPin.AbsPos.X.Value, (float)passedPin.AbsPos.Y.Value, (float)passedPin.AbsPos.Z.Value));
                                }

                                float z0angle = this.CoordinateManager().StageCoord.Z0Angle.Value;
                                float z1angle = this.CoordinateManager().StageCoord.Z1Angle.Value;
                                float z2angle = this.CoordinateManager().StageCoord.Z2Angle.Value;

                                float pcd = this.CoordinateManager().StageCoord.PCD.Value;

                                var tiltResult = CalcTiltedPlane(pins, z0angle, z1angle, z2angle, pcd);

                                if (tiltResult != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Debug($"Plane adjustment failed. Clear plane offsets.");
                                    ResetPlaneOffsets();
                                }
                            }
                            else
                            {
                                LoggerManager.Debug($"Insufficient pin data for plane adjustment. Clear plane offsets.");
                                ResetPlaneOffsets();
                            }

                            #region // Deprecated
                            // IPinData의 Result를 기반으로 데이터를 사용하는 경우, 변수가 발생할 가능성이 있다고 판단 됨.
                            // 따라서 모든 변수를 제거하고 유효한 데이터를 사용하기 위해, AlignResult.EachPinResultes안의 데이터를 활용하여 필요한 정보를 획득한다.
                            // 1차적으로 얻어야 될 데이터는 얼라인이 성공 한 핀들의 집합(List<IPinData>)

                            // 틸트를 안해도 조건 (데이터로 의미있는 틸트 값을 계산하지 못하는 경우)

                            // 1. Lowest Pin - Highst Pin = Threshold (5 um ) : X
                            // 2. 제일 낮은 핀과 그 비슷한 값들을 갖는 그룹을 묶고(비슷하다를 5 um) 160 ~ 180도 내에 있는 경우 틸트 하지마... 못해.. 그만해...
                            // 3-1. 2번을 통과 했을 때, 그룹의 전체 개수가 2개인 경우, x = (x1 + x2) * 1/2, y = (x1 + x2) * 1/2 
                            // 3-2. 2번을 통과 했을 때, 그룹의 전체 개수가 3개 이상인 경우, 일단 보류, 경우의 수 다 따져보고, 추가 개선할 것.

                            //double PinAvgZValue = -9000;

                            //double DiffThreshold = 5;
                            //double SimilarValue = 5;
                            //double DiffDegreeThreshold = 10;
                            //double similarity = 0.3;
                            //MachineCoordinate lowestpin = new MachineCoordinate();
                            //MachineCoordinate Highstpin = new MachineCoordinate();

                            //MachineCoordinate FinalPos = new MachineCoordinate();

                            //if (PassedPinDatas.Count > 0)
                            //{
                            //    PinAvgZValue = PassedPinDatas.Sum(x => x.AbsPos.Z.Value) / PassedPinDatas.Count;

                            //    double min = PassedPinDatas[0].AbsPos.Z.Value;
                            //    double max = PassedPinDatas[0].AbsPos.Z.Value;

                            //    int minIndex = 0;
                            //    int maxIndex = 0;

                            //    for (int p = 0; p < PassedPinDatas.Count; p++)
                            //    {
                            //        if (PassedPinDatas[p].AbsPos.Z.Value < min)
                            //        {
                            //            min = PassedPinDatas[p].AbsPos.Z.Value;
                            //            minIndex = p;
                            //        }

                            //        if (PassedPinDatas[p].AbsPos.Z.Value > max)
                            //        {
                            //            max = PassedPinDatas[p].AbsPos.Z.Value;
                            //            maxIndex = p;
                            //        }
                            //    }

                            //    int LowestPinNo = PassedPinDatas[minIndex].PinNum.Value;
                            //    int HighstPinNo = PassedPinDatas[maxIndex].PinNum.Value;

                            //    LoggerManager.Debug($"Lowest Pin No. {LowestPinNo}, Height = {PassedPinDatas[minIndex].AbsPos.Z.Value}");
                            //    LoggerManager.Debug($"Highest Pin No. {HighstPinNo}, Height = {PassedPinDatas[maxIndex].AbsPos.Z.Value}");
                            //    LoggerManager.Debug($"Pin Height Avearge : {PinAvgZValue}");

                            //    lowestpin.X.Value = PassedPinDatas[minIndex].AbsPos.X.Value;
                            //    lowestpin.Y.Value = PassedPinDatas[minIndex].AbsPos.Y.Value;
                            //    lowestpin.Z.Value = PassedPinDatas[minIndex].AbsPos.Z.Value;

                            //    Highstpin.X.Value = PassedPinDatas[maxIndex].AbsPos.X.Value;
                            //    Highstpin.Y.Value = PassedPinDatas[maxIndex].AbsPos.Y.Value;
                            //    Highstpin.Z.Value = PassedPinDatas[maxIndex].AbsPos.Z.Value;

                            //    double DiffHeight = Math.Abs(Highstpin.Z.Value - lowestpin.Z.Value);
                            //    if(DiffHeight > 5)
                            //    {
                            //        SimilarValue = DiffHeight * similarity;
                            //    }

                            //    if (DiffHeight > DiffThreshold)
                            //    {
                            //        SimilarHeightGroup = PassedPinDatas.FindAll(x => (x.AbsPos.Z.Value > lowestpin.Z.Value - SimilarValue) &&
                            //                                                         (x.AbsPos.Z.Value < lowestpin.Z.Value + SimilarValue));

                            //        LoggerManager.Debug($"Height Group Count : {SimilarHeightGroup.Count}");

                            //        if (SimilarHeightGroup.Count == 1)
                            //        {
                            //            FinalPos.X.Value = SimilarHeightGroup[0].AbsPos.X.Value;
                            //            FinalPos.Y.Value = SimilarHeightGroup[0].AbsPos.Y.Value;
                            //            FinalPos.Z.Value = SimilarHeightGroup[0].AbsPos.Z.Value;
                            //        }
                            //        else if (SimilarHeightGroup.Count == 2)
                            //        {
                            //            // 두 점 사이의 각도 확인

                            //            double Angle_First;
                            //            double Angle_Second;

                            //            double Drad = 0;

                            //            Angle_First = Math.Atan2(SimilarHeightGroup[0].AbsPos.Y.Value, SimilarHeightGroup[0].AbsPos.X.Value);
                            //            Angle_Second = Math.Atan2(SimilarHeightGroup[1].AbsPos.Y.Value, SimilarHeightGroup[1].AbsPos.X.Value);

                            //            LoggerManager.Debug($"First Pin No.{SimilarHeightGroup[0].PinNum.Value}, Second Pin No.{SimilarHeightGroup[1].PinNum.Value}");

                            //            Drad = Math.Abs(RadianToDegree(Angle_First - Angle_Second));

                            //            if ((Drad < 180 - DiffDegreeThreshold) || (Drad > 180 + DiffDegreeThreshold))
                            //            {
                            //                double xsum;
                            //                double ysum;
                            //                double zsum;

                            //                xsum = SimilarHeightGroup[0].AbsPos.X.Value + SimilarHeightGroup[1].AbsPos.X.Value;
                            //                ysum = SimilarHeightGroup[0].AbsPos.Y.Value + SimilarHeightGroup[1].AbsPos.Y.Value;
                            //                zsum = SimilarHeightGroup[0].AbsPos.Z.Value + SimilarHeightGroup[1].AbsPos.Z.Value;

                            //                FinalPos.X.Value = (SimilarHeightGroup[0].AbsPos.X.Value / xsum) * SimilarHeightGroup[0].AbsPos.X.Value +
                            //                                   (SimilarHeightGroup[1].AbsPos.X.Value / xsum) * SimilarHeightGroup[1].AbsPos.X.Value;

                            //                FinalPos.Y.Value = (SimilarHeightGroup[0].AbsPos.Y.Value / ysum) * SimilarHeightGroup[0].AbsPos.Y.Value +
                            //                                   (SimilarHeightGroup[1].AbsPos.Y.Value / ysum) * SimilarHeightGroup[1].AbsPos.Y.Value;

                            //                FinalPos.Z.Value = (SimilarHeightGroup[0].AbsPos.Z.Value / zsum) * SimilarHeightGroup[0].AbsPos.Z.Value +
                            //                                   (SimilarHeightGroup[1].AbsPos.Z.Value / zsum) * SimilarHeightGroup[1].AbsPos.Z.Value;
                            //            }
                            //        }
                            //        else if (SimilarHeightGroup.Count > 2) // 추후 고려할 것.
                            //        {
                            //            // 두 점 사이의 각도 확인

                            //            double Angle_First;
                            //            double Angle_Second;

                            //            double Drad = 0;
                            //            SimilarHeightGroup.OrderBy(p => p.AbsPos.Z.Value);

                            //            Angle_First = Math.Atan2(SimilarHeightGroup[0].AbsPos.Y.Value, SimilarHeightGroup[0].AbsPos.X.Value);
                            //            Angle_Second = Math.Atan2(SimilarHeightGroup[1].AbsPos.Y.Value, SimilarHeightGroup[1].AbsPos.X.Value);

                            //            LoggerManager.Debug($"First Pin No.{SimilarHeightGroup[0].PinNum.Value}, Second Pin No.{SimilarHeightGroup[1].PinNum.Value}");

                            //            Drad = Math.Abs(RadianToDegree(Angle_First - Angle_Second));

                            //            if ((Drad < 180 - DiffDegreeThreshold) || (Drad > 180 + DiffDegreeThreshold))
                            //            {
                            //                double xsum;
                            //                double ysum;
                            //                double zsum;

                            //                xsum = SimilarHeightGroup[0].AbsPos.X.Value + SimilarHeightGroup[1].AbsPos.X.Value;
                            //                ysum = SimilarHeightGroup[0].AbsPos.Y.Value + SimilarHeightGroup[1].AbsPos.Y.Value;
                            //                zsum = SimilarHeightGroup[0].AbsPos.Z.Value + SimilarHeightGroup[1].AbsPos.Z.Value;

                            //                FinalPos.X.Value = (SimilarHeightGroup[0].AbsPos.X.Value / xsum) * SimilarHeightGroup[0].AbsPos.X.Value +
                            //                                   (SimilarHeightGroup[1].AbsPos.X.Value / xsum) * SimilarHeightGroup[1].AbsPos.X.Value;

                            //                FinalPos.Y.Value = (SimilarHeightGroup[0].AbsPos.Y.Value / ysum) * SimilarHeightGroup[0].AbsPos.Y.Value +
                            //                                   (SimilarHeightGroup[1].AbsPos.Y.Value / ysum) * SimilarHeightGroup[1].AbsPos.Y.Value;

                            //                FinalPos.Z.Value = (SimilarHeightGroup[0].AbsPos.Z.Value / zsum) * SimilarHeightGroup[0].AbsPos.Z.Value +
                            //                                   (SimilarHeightGroup[1].AbsPos.Z.Value / zsum) * SimilarHeightGroup[1].AbsPos.Z.Value;
                            //            }
                            //        }
                            //        else
                            //        {

                            //        }
                            //    }

                            //    if (FinalPos.X.Value != 0 && FinalPos.Y.Value != 0 && FinalPos.Z.Value != 0)
                            //    {
                            //        double tiltvalue = (FinalPos.GetZ() - PinAvgZValue) * 2.0 * PinAlignParam.PinPlaneAdjustParam.CompRatio.Value;
                            //        double degree = Math.Atan2(FinalPos.GetY() - 0, FinalPos.GetX() - 0) * 180 / Math.PI;

                            //        //degree += 45;
                            //        degree += 60;   //wayne 190916

                            //        LoggerManager.Debug($"Tilt value = {tiltvalue}(Ratio = {PinAlignParam.PinPlaneAdjustParam.CompRatio.Value}), Degree = {degree}");

                            //        CalcZOffset(degree, tiltvalue);
                            //    }
                            //    else
                            //    {
                            //        ResetPlaneOffsets();
                            //        LoggerManager.Debug($"Plane does not need to be compensated. Clear plane offsets.");
                            //    }
                            //}
                            #endregion
                        }
                    }
                    else
                    {
                        if (PinAlignParam.PinAlignInterval.WaferIntervalCount.Value <= ((long)this.LotOPModule().SystemInfo.WaferCount - PinAlignParam.PinAlignInterval.MarkedWaferCountVal))
                        { PinAlignParam.PinAlignInterval.MarkedWaferCountVal = (long)this.LotOPModule().SystemInfo.WaferCount; }

                        if (PinAlignParam.PinAlignInterval.DieInterval.Value <= ((long)this.LotOPModule().SystemInfo.DieCount - PinAlignParam.PinAlignInterval.MarkedDieCountVal))
                        { PinAlignParam.PinAlignInterval.MarkedDieCountVal = (long)this.LotOPModule().SystemInfo.DieCount; }

                        PinAlignParam.PinAlignInterval.FlagAlignProcessedForThisWafer = true;

                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.SAMPLE_PIN_ALIGN_PASSED, retval);
                    }

                    //(this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinAlignLastAlignResult.TotalAlignPinCount = AlignResult.TotalAlignPinCount;
                    //(this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinAlignLastAlignResult.TotalFailPinCount = AlignResult.TotalFailPinCount;
                    //(this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinAlignLastAlignResult.TotalPassPinCount = AlignResult.TotalPassPinCount;
                    //(this.PinAligner().PinAlignDevParam as PinAlignDevParameters).PinAlignLastAlignResult.PassPercentage = AlignResult.PassPercentage;

                    // 성공했을 때, 마지막 얼라인 데이터를 기억.
                    //AlignInfo.AlignLastResult.TotalAlignPinCount = AlignResult.TotalAlignPinCount;
                    //AlignInfo.AlignLastResult.TotalFailPinCount = AlignResult.TotalFailPinCount;
                    //AlignInfo.AlignLastResult.TotalPassPinCount = AlignResult.TotalPassPinCount;
                    //AlignInfo.AlignLastResult.PassPercentage = AlignResult.PassPercentage;

                    // TODO : Check Logic
                    //AlignResult.EachPinResultes.Clear();
                    //AlignResult.TotalAlignPinCount = 0;
                    //AlignResult.TotalFailPinCount = 0;
                    //AlignResult.TotalPassPinCount = 0;
                    //AlignResult.PassPercentage = 0;

                    //AlignResult.EachPinResultes.Clear();
                    //AlignResult.TotalAlignPinCount = 0;
                    //AlignResult.TotalFailPinCount = 0;
                    //AlignResult.TotalPassPinCount = 0;
                    //AlignResult.PassPercentage = 0; 

                    #endregion
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.PIN_ALIGN_FAILED;
                LoggerManager.Debug("PinAlign() exception error occured");
                LoggerManager.Exception(err);
            }
            finally
            {
                LoggerManager.Debug($"PinAlign(), PinAlignProceState is {PinAlignProcState.ToString()}");
                if (this.PinAligner().ValidPinTipSize_Original_List != null)
                {
                    this.PinAligner().ValidPinTipSize_Original_List.Clear();
                }

                if (this.PinAligner().ValidPinTipSize_OutOfSize_List != null)
                {
                    this.PinAligner().ValidPinTipSize_OutOfSize_List.Clear();
                }

                if (this.PinAligner().ValidPinTipSize_SizeInRange_List != null)
                {
                    this.PinAligner().ValidPinTipSize_SizeInRange_List.Clear();
                }

                // 틸트 옵션이 꺼져있거나, 핀 얼라인이 실패했을 경우, 초기화를 해줘야 한다.
                if (PinAlignParam.PinPlaneAdjustParam.EnablePinPlaneCompensation.Value == false || PinAlignProcState != PINPROCESSINGSTATE.ALIGN_PASSED || retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"PinAlign(), Tilt value is reset. Tilt enable = {PinAlignParam.PinPlaneAdjustParam.EnablePinPlaneCompensation.Value}, PinAlignProceState = {PinAlignProcState}, PinAlignErr = {retval}", isInfo: IsInfo);

                    bool forceResetWaferAlign = false;

                    if (PinAlignProcState != PINPROCESSINGSTATE.ALIGN_PASSED || retval != EventCodeEnum.NONE)
                    {
                        forceResetWaferAlign = true;
                    }

                    ResetPlaneOffsets(forceResetWaferAlign);
                }
            }

            SetStepSetupState();

            this.VisionManager().StartGrab(EnumProberCam.PIN_HIGH_CAM, this);

            return retval;
        }
        private void ResetPlaneOffsets(bool forceResetWaferAlign = true)
        {
            if (AlignResult.PlaneOffset.Count != 3)
            {
                AlignResult.PlaneOffset.Clear();
                AlignResult.PlaneOffset.Add(0);
                AlignResult.PlaneOffset.Add(0);
                AlignResult.PlaneOffset.Add(0);
            }

            AlignResult.PlaneOffset[0] = 0;
            AlignResult.PlaneOffset[1] = 0;
            AlignResult.PlaneOffset[2] = 0;

            // Reset wafer align state when clearing plane offsets.
            // Reset wafer align state when adjusting plane offsets.            
            if (forceResetWaferAlign)
            {
                //soaking으로 인한 sample pin align인 경우에는 wafer align state를 유지한다.
                if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.SOAKING && this.PinAligner().UseSoakingSamplePinAlign)
                {
                    LoggerManager.SoakingLog($"Mode is 'sample pin align(soaking)'. Therefore the wafer status is not changed to 'IDLE'");
                }
                else
                {
                    this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                }
            }
        }
        /// <summary>
        ///  For Cell system, float z0angle = 210, float z1angle = 90, float z2angle = 330, double pcd = 110000
        ///  For Single system, float z0angle = 150, float z1angle = 30, float z2angle = 270, double pcd = 110000
        /// </summary>
        /// <param name="pins"></param>
        /// <param name="z0angle"></param>
        /// <param name="z1angle"></param>
        /// <param name="z2angle"></param>
        /// <param name="pcd"></param>
        /// <returns></returns>
        private EventCodeEnum CalcTiltedPlane(List<Vector3> pins, float z0angle = 210, float z1angle = 90, float z2angle = 330, double pcd = 110000)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            double pillar0X = 0d;
            double pillar1X = 0d;
            double pillar2X = 0d;

            double pillar0Y = 0d;
            double pillar1Y = 0d;
            double pillar2Y = 0d;

            try
            {
                List<PlaneCoeff> planes = new List<PlaneCoeff>();
                List<List<Vector3>> pinPacks = new List<List<Vector3>>();

                float avgPinHeight = -10000;
                float pinHeightSum = 0;

                pillar0X = pcd * Math.Cos(Math.PI * (z0angle) / 180);
                pillar0Y = pcd * Math.Sin(Math.PI * (z0angle) / 180);
                pillar1X = pcd * Math.Cos(Math.PI * (z1angle) / 180);
                pillar1Y = pcd * Math.Sin(Math.PI * (z1angle) / 180);
                pillar2X = pcd * Math.Cos(Math.PI * (z2angle) / 180);
                pillar2Y = pcd * Math.Sin(Math.PI * (z2angle) / 180);

                foreach (var pin in pins)
                {
                    pinHeightSum += pin.Z;
                }

                avgPinHeight = pinHeightSum / pins.Count;

                var dutHeight = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault().DutSizeHeight;
                var dutWidth = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.FirstOrDefault().DutSizeWidth;

                var minAllowableDist = Math.Sqrt(Math.Pow(dutHeight, 2) + Math.Pow(dutWidth, 2));
                minAllowableDist = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSize_um.Value * 0.3;
                foreach (var scoppedPin in pins)
                {
                    var sortedPin = pins.OrderBy(p => Vector3.Distance(scoppedPin, p)).ToList();

                    Func<List<Vector3>, Vector3, List<Vector3>> removeDCP = delegate (List<Vector3> pinpack, Vector3 targetpin)
                    {
                        List<Vector3> filteredPinPack = new List<Vector3>();
                        try
                        {
                            foreach (var pin in pinpack)
                            {
                                filteredPinPack.Add(new Vector3(pin.X, pin.Y, pin.Z));
                            }

                            var dieWidth = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value;
                            var dieHeight = this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value;

                            var minDCDist = Math.Sqrt(Math.Pow(dieWidth, 2) + Math.Pow(dieHeight, 2));

                            filteredPinPack.RemoveAll(p => ((Vector3.Distance(targetpin, p) < minDCDist)) & ((Vector3.Distance(targetpin, p) != 0)));

                        }
                        catch (Exception err)
                        {
                            LoggerManager.Debug($"CalcTiltedPlane(): Pin filtering failed. Err = {err.Message}");
                        }

                        return filteredPinPack;
                    };

                    int targetPinIndex = 0;
                    bool dcpRemoved = false;

                    while (dcpRemoved == false)
                    {
                        if (targetPinIndex >= sortedPin.Count)
                        {
                            dcpRemoved = true;
                        }
                        var filteredPins = removeDCP(sortedPin, sortedPin[targetPinIndex]);

                        if (sortedPin.Count == filteredPins.Count)
                        {
                            targetPinIndex++;
                            if (targetPinIndex >= sortedPin.Count)
                            {
                                dcpRemoved = true;
                            }
                        }
                        else
                        {
                            sortedPin.Clear();
                            sortedPin = filteredPins.ToList();
                        }
                    }

                    if (sortedPin.Count >= 3)
                    {
                        pinPacks.Add(sortedPin);
                    }
                }

                foreach (var pinPack in pinPacks)
                {
                    planes.Add(GetPlaneFromVector(pinPack[0], pinPack[pinPack.Count - 1], pinPack[pinPack.Count - 2]));
                }

                List<double> z0Offsets = new List<double>();
                List<double> z1Offsets = new List<double>();
                List<double> z2Offsets = new List<double>();

                float offsetZ0 = 0;
                float offsetZ1 = 0;
                float offsetZ2 = 0;

                foreach (var plane in planes)
                {
                    z0Offsets.Add(plane.GetHeight((float)pillar0X, (float)pillar0Y));
                    z1Offsets.Add(plane.GetHeight((float)pillar1X, (float)pillar1Y));
                    z2Offsets.Add(plane.GetHeight((float)pillar2X, (float)pillar2Y));
                }

                offsetZ0 = (float)(z0Offsets.Average() - avgPinHeight) * (float)PinAlignParam.PinPlaneAdjustParam.CompRatio.Value;
                offsetZ1 = (float)(z1Offsets.Average() - avgPinHeight) * (float)PinAlignParam.PinPlaneAdjustParam.CompRatio.Value;
                offsetZ2 = (float)(z2Offsets.Average() - avgPinHeight) * (float)PinAlignParam.PinPlaneAdjustParam.CompRatio.Value;

                var calcPlane = GetPlaneFromVector(new Vector3((float)pillar0X, (float)pillar0Y, offsetZ0), new Vector3((float)pillar1X, (float)pillar1Y, offsetZ1), new Vector3((float)pillar2X, (float)pillar2Y, offsetZ2));

                float similarity = 0;

                foreach (var pin in pins)
                {
                    var expectedHeight = calcPlane.GetHeight((float)pin.X, (float)pin.Y);
                    var ratio = (expectedHeight + avgPinHeight) / (float)pin.Z;
                    similarity += ratio;
                }

                similarity = similarity / pins.Count;

                LoggerManager.Debug($"Pin height verification result = {similarity * 100.0:0.000000}%", isInfo: IsInfo);
                LoggerManager.Debug($"Result Plane A = {calcPlane.A:0.000}, B = {calcPlane.B:0.000}, C = {calcPlane.C:0.000}, D = {calcPlane.D:0.000}", isInfo: IsInfo);

                var zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double compLimit = PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value;
                List<double> offsetlist = new List<double>();

                if (offsetZ0 < compLimit)
                {
                    offsetlist.Add(offsetZ0);
                    LoggerManager.Debug($"Z0  Comp. Value is {offsetZ0:0.00}", isInfo: IsInfo);
                }
                else
                {
                    offsetZ0 = (float)compLimit;
                    offsetlist.Add(offsetZ0);
                    LoggerManager.Debug($"Comp. Value for Z0 is out of tolerence(Tol:{compLimit}). Comp. Value is {offsetZ0:0.00}");
                }

                if (offsetZ1 < compLimit)
                {
                    offsetlist.Add(offsetZ1);
                    LoggerManager.Debug($"Z1  Comp. Value is {offsetZ1:0.00}");
                }
                else
                {
                    offsetZ1 = (float)compLimit;
                    offsetlist.Add(offsetZ1);
                    LoggerManager.Debug($"Comp. Value for Z1 is out of tolerence(Tol:{compLimit}). Comp. Value is {offsetZ1:0.00}");
                }

                if (offsetZ2 < compLimit)
                {
                    offsetlist.Add(offsetZ2);
                    LoggerManager.Debug($"Z2  Comp. Value is {offsetZ2:0.00}");
                }
                else
                {
                    offsetZ2 = (float)compLimit;
                    offsetlist.Add(offsetZ2);
                    LoggerManager.Debug($"Comp. Value for Z2 is out of tolerence(Tol:{compLimit}). Comp. Value is {offsetZ2:0.00}");
                }

                if (offsetlist.Count == 3)
                {
                    var offDev = offsetlist[0];

                    if (AlignResult.PlaneOffset.Count != offsetlist.Count)
                    {
                        AlignResult.PlaneOffset.Clear();

                        for (int i = 0; i < offsetlist.Count; i++)
                        {
                            AlignResult.PlaneOffset.Add(0.0);
                        }
                    }

                    for (int i = 0; i < zaxis.GroupMembers.Count; i++)
                    {
                        var offsetToApply = offsetlist[i] - offDev;

                        if (Math.Abs(offsetToApply) > PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value)
                        {
                            LoggerManager.Debug($"Pin Plane Compensation Limit exceeded on Axis Z{i}. Offset = ({offsetToApply:0.00}), Limit to maximum value({PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value}).");

                            if (offsetToApply > 0)
                            {
                                offsetToApply = PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value;
                            }
                            else
                            {
                                offsetToApply = PinAlignParam.PinPlaneAdjustParam.MaxCompHeight.Value * -1.0;
                            }
                        }

                        zaxis.GroupMembers[i].Status.CompValue = offsetToApply;
                        AlignResult.PlaneOffset[i] = zaxis.GroupMembers[i].Status.CompValue;
                    }

                    if (zaxis.GroupMembers.Count == 3)
                    {
                        LoggerManager.Debug($"Z Comp. values = [{zaxis.GroupMembers[0].Status.CompValue:0.00}], [{zaxis.GroupMembers[1].Status.CompValue:0.00}], [{zaxis.GroupMembers[2].Status.CompValue:0.00}]", isInfo: IsInfo);
                    }

                    // Reset wafer align state when adjusting plane offsets.
                    if (this.PinAligner().PinAlignSource == PINALIGNSOURCE.SOAKING && this.PinAligner().UseSoakingSamplePinAlign)
                    {
                        LoggerManager.SoakingLog($"Mode is 'sample pin align(soaking)'. Therefore the wafer status is not changed to 'IDLE'");
                    }
                    else
                    {
                        this.StageSupervisor().WaferObject.SetAlignState(AlignStateEnum.IDLE);
                    }

                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    ret = EventCodeEnum.MOTION_CHUCKTILT_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"CalcPinPlane(): Error occurred. Err = {err.Message}");
            }

            return ret;
        }
        private PlaneCoeff GetPlaneFromVector(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            float a = 0;
            float b = 0;
            float c = 0;
            float d = 0;
            try
            {
                a = point1.Y * (point2.Z - point3.Z) +
                    point2.Y * (point3.Z - point1.Z) +
                    point3.Y * (point1.Z - point2.Z);
                b = point1.Z * (point2.X - point3.X) +
                    point2.Z * (point3.X - point1.X) +
                    point3.Z * (point1.X - point2.X);
                c = point1.X * (point2.Y - point3.Y) +
                    point2.X * (point3.Y - point1.Y) +
                    point3.X * (point1.Y - point2.Y);
                d = -(point1.X * ((point2.Y * point3.Z) - (point3.Y * point2.Z)))
                    - (point2.X * ((point3.Y * point1.Z) - (point1.Y * point3.Z)))
                    - (point3.X * ((point1.Y * point2.Z) - (point2.Y * point1.Z)));
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetPlaneFromVector(): Error occurred. Err = {err.Message}");
            }

            return new PlaneCoeff(a, b, c, d);
        }
        private bool CheckPinDataAlignedOffset(PinCoordinate DevPinAbsPos, int DutIndex, int PinIndex, PINALIGNSOURCE Source)
        {
            bool IsChange = true;
            try
            {
                PinAlignSettignParameter CurrentPinSetting = PinAlignParam.PinAlignSettignParam.FirstOrDefault(s => s.SourceName.Value == Source);
                if (CurrentPinSetting == null)
                {
                    LoggerManager.Debug("[PinHighAlignModule] GettmpPinDataAbsPos() : PinAlignSettignParameter is null value.");
                    return false;
                }
                LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : PinAlignSource is {Source.ToString()}.");

                double EachPinToleranceX = Math.Abs(CurrentPinSetting.EachPinToleranceX.Value);
                double EachPinToleranceY = Math.Abs(CurrentPinSetting.EachPinToleranceY.Value);
                double EachPinToleranceZ = Math.Abs(CurrentPinSetting.EachPinToleranceZ.Value);

                var cardinfo = probeCardSysObject.ProbercardBackupinfo;
                if (cardinfo == null || this.CardChangeModule().GetProbeCardID() == "")
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() :  Invaild Parameter error (Probe Card ID )" +
                        $"[Cur id: {this.CardChangeModule().GetProbeCardID()}.");
                    return false;
                }

                if (cardinfo.ProbeCardID != this.CardChangeModule().GetProbeCardID())
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Probe Card ID Name is not matched" +
                        $"[Cur id: {this.CardChangeModule().GetProbeCardID()}.");
                    return false;
                }

                if (cardinfo.BackupPinDataList.Count() == 0)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Pin Count is not matched" +
                        $"[Cur id: {this.CardChangeModule().GetProbeCardID()}, Sys Count:{cardinfo.BackupPinDataList.Count()}.");
                    return false;
                }

                if (cardinfo.TotalDutCnt != this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count())
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Total number of Duts not correct" +
                        $"[Cur Total Dut Count: {cardinfo.TotalDutCnt}. Dev Total Dut Count:{this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count()}");
                    return false;
                }

                if (cardinfo.TotalPinCnt != this.StageSupervisor().ProbeCardInfo.GetPinCount())
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Total number of Pins not correct" +
                        $"[Cur Total Pin Cnt Count: {cardinfo.TotalPinCnt}. Dev Total Pin Cnt Count:{this.StageSupervisor().ProbeCardInfo.GetPinCount()}");
                    return false;
                }

                var FirstDutMI = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Where(x => x.DutNumber == 1).FirstOrDefault();
                if (FirstDutMI == null)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Invaild Parameter error");
                    return false;
                }

                if (cardinfo.FirstDutMI.XIndex != FirstDutMI.MacIndex.XIndex || cardinfo.FirstDutMI.YIndex != FirstDutMI.MacIndex.YIndex)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : The MI of the first DUT does not matched" +
                        $"[Cur FirstDutMI: ({cardinfo.FirstDutMI.XIndex},{cardinfo.FirstDutMI.YIndex})" +
                        $"Dev FirstDutMI: ({FirstDutMI.MacIndex.XIndex},{FirstDutMI.MacIndex.YIndex}).");
                    return false;
                }

                var pinNo = cardinfo.BackupPinDataList.FirstOrDefault(n => n.PinNo == PinIndex);
                var dutNo = cardinfo.BackupPinDataList.FirstOrDefault(n => n.DutNo == DutIndex && n.PinNo == PinIndex);
                if (pinNo == null || dutNo == null)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : The number of dut and pin are not the same" +
                        $" CurDev Pin Index:{PinIndex}, Dut index: {DutIndex}] .");
                    return false;
                }

                if (pinNo != null && dutNo != null)
                {
                    if (dutNo != pinNo)
                    {
                        LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : The information of dut and pin is not the same");
                        return false;
                    }
                }

                /* 
                //brett// device change tolerance 값 변경을 통해 check 하는게 맞는것으로 보여 주석 처리함
                // EachPinTolerance는 이전 pin 결과와 현재 pin 결과를 비교할때 사용하는 것이다. system에 저장된 pin 위치를 사용하는 것에 대한 판단 기준으로 사용하면 안됨
                if (Math.Abs(DevPinAbsPos.X.Value) - Math.Abs(pinNo.BackupAbsPos.X.Value) > EachPinToleranceX)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : PinAlignSettignParameter X tolerance error" +
                        $" [PinIndex:{PinIndex}, Pos:{pinNo.BackupAbsPos.X.Value}, Pos diff: {Math.Abs(DevPinAbsPos.X.Value) - Math.Abs(pinNo.BackupAbsPos.X.Value)}, Tolerance:{EachPinToleranceX}] .");
                    return false;
                }

                if (Math.Abs(DevPinAbsPos.Y.Value) - Math.Abs(pinNo.BackupAbsPos.Y.Value) > EachPinToleranceY)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : PinAlignSettignParameter Y tolerance error " +
                        $"[PinIndex:{PinIndex}, Pos:{pinNo.BackupAbsPos.Y.Value}, Pos diff: {Math.Abs(DevPinAbsPos.Y.Value) - Math.Abs(pinNo.BackupAbsPos.Y.Value)}, Tolerance:{EachPinToleranceY}] .");
                    return false;
                }

                if (Math.Abs(DevPinAbsPos.Z.Value) - Math.Abs(pinNo.BackupAbsPos.Z.Value) > EachPinToleranceZ)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : PinAlignSettignParameter Z tolerance error " +
                        $"[PinIndex:{PinIndex}, Pos:{pinNo.BackupAbsPos.Z.Value}, Pos diff: {Math.Abs(DevPinAbsPos.Z.Value) - Math.Abs(pinNo.BackupAbsPos.Z.Value)}, Tolerance:{EachPinToleranceZ}] .");
                    return false;
                }
                */
                LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : PinAlignSettignParameter tolerance Info" +
                        $"[PinIndex:{PinIndex}, X_Pos:{pinNo.BackupAbsPos.X.Value}, X_Pos diff: {Math.Abs(DevPinAbsPos.X.Value) - Math.Abs(pinNo.BackupAbsPos.X.Value)}, X_Tolerance:{EachPinToleranceX}" +
                        $", Y_Pos:{pinNo.BackupAbsPos.Y.Value}, Y_Pos diff: {Math.Abs(DevPinAbsPos.Y.Value) - Math.Abs(pinNo.BackupAbsPos.Y.Value)}, Y_Tolerance:{EachPinToleranceY}" +
                        $", Z_Pos:{pinNo.BackupAbsPos.Z.Value}, Z_Pos diff: {Math.Abs(DevPinAbsPos.Z.Value) - Math.Abs(pinNo.BackupAbsPos.Z.Value)}, Z_Tolerance:{EachPinToleranceZ}]");

                if (IsChange == true)
                {
                    LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Dut index: [{DutIndex}], Pin[{PinIndex}] AbsPos uses Sys Pin Abs Pos.");
                }
            }
            catch (Exception err)
            {
                IsChange = false;
                LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Exception error {err}");
            }

            return IsChange;
        }
        private PinCoordinate SetPinDataAlignedOffset(PinCoordinate DevPinAbsPos, int PinIndex)
        {
            PinCoordinate alignedOffset = new PinCoordinate();
            try
            {
                var pinNo = probeCardSysObject.ProbercardBackupinfo.BackupPinDataList.FirstOrDefault(n => n.PinNo == PinIndex);

                alignedOffset.X.Value = pinNo.BackupAbsPos.X.Value - DevPinAbsPos.X.Value;
                alignedOffset.Y.Value = pinNo.BackupAbsPos.Y.Value - DevPinAbsPos.Y.Value;
                alignedOffset.Z.Value = pinNo.BackupAbsPos.Z.Value - DevPinAbsPos.Z.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[PinHighAlignModule] GettmpPinDataAbsPos() : Exception error {err}");
            }

            return alignedOffset;
        }
        public EventCodeEnum SaveProbeCardData()
        {
            EventCodeEnum serialRes = EventCodeEnum.UNDEFINED;
            LoggerManager.Debug($"[PinHighAlignModule] SaveProbeCardData() : Probe Card Data Save Start");

            try
            {
                serialRes = this.StageSupervisor().SaveProberCard();

                if (serialRes == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Ok.");
                }
                else
                {
                    LoggerManager.Debug("SaveDevParameter() : Save Fail.");
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($": SaveProbeCardData() ERROR");
            }

            LoggerManager.Debug($"[PinHighAlignModule] SaveProbeCardData() : Probe Card Data Save done");

            return serialRes;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public Task<EventCodeEnum> InitRecovery()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int NumOfPin = 0;

            try
            {
                foreach (IDut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    NumOfPin += dut.PinList.Count;
                }

                // TODO : DutList와 NumOfPin의 개수가 유효하지 않을 때 처리

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override void DeInitModule()
        {
        }
        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;

            try
            {

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
                //if (this.StageSupervisor().ProbeCardInfo.GetPinPadAlignState() == AlignStateEnum.DONE &&
                //    this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                if (this.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                {
                    // 카드 체인지 하고 핀 얼라인 한 적 있음
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
                else
                {
                    // 카드 체인지 하고 아직 핀 얼라인 한 적 없음
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum DoClearData()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                DoRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }
        public bool IsExecute()
        {
            return true;
        }
        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SaveProbeCardData();

                //SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                //SetNodeSetupState(EnumMoudleSetupState.NONE);

                //this.PinAligner().StopDrawPinOverlay(CurCam);
                this.PinAligner().StopDrawDutOverlay(CurCam);
                retVal = await base.Cleanup(parameter);

                if (retVal == EventCodeEnum.NONE)
                {
                    this.StageSupervisor().StageModuleState.ZCLEARED();
                }

                // 셋업 화면 들어온 뒤, 나가서 메뉴얼로 동작 할 때, PIN REGISTRATION 소스로 동작하는 것을 막음.
                //this.PinAligner().IsChangedSource = false;
                this.PinAligner().PinAlignSource = PINALIGNSOURCE.WAFER_INTERVAL;
                this.PnPManager().RememberLastLightSetting(CurCam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();

                PackagableParams.Add(SerializeManager.SerializeToByte(this.PinAligner().PinAlignInfo));
                PackagableParams.Add(SerializeManager.SerializeToByte(this.PinAligner().PinAlignDevParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
