using LoaderBase.Communication;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LoaderServiceClientModules
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using Autofac;
    using LogModule;
    using ProberInterfaces.Align;
    using ProberInterfaces.Command;
    using ProberInterfaces.State;
    using ProberInterfaces.Template;
    using ProberInterfaces.WaferAlignEX;
    using ProberInterfaces.Wizard;
    using WA_HighMagParameter_Standard;

    public class WaferAlignerServiceClient : IWaferAligner, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private bool _IsModify { get; set; }
        public bool IsOnDubugMode { get; set; }
        public string IsOnDebugImagePathBase { get; set; }
        public string IsOnDebugPadPathBase { get; set; }
        public bool GetIsModify()
        {
            bool retval = false;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetIsRecovery();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetIsRecovery();
        }


        public bool WaferAlignRunning { get; set; }
        public void SetIsModify(bool flag)
        {
            _IsModify = flag;
        }
        public double GetReviseSquarness(double xpos, double ypos)
        {
            throw new NotImplementedException();
        }

        //public int HeightSearchIndex(double x, double y)
        //{
        //    throw new NotImplementedException();
        //}

        public void AddHeighPlanePoint(WAHeightPositionParam param = null, bool center_standard = false)
        {
            throw new NotImplementedException();
        }

        public void ResetHeightPlanePoint()
        {
            throw new NotImplementedException();
        }

        public WaferAlignInnerStateEnum GetWAInnerStateEnum()
        {
            throw new NotImplementedException();
        }

        public List<WaferCoordinate> GetHieghtPlanePoint()
        {
            throw new NotImplementedException();
        }

        public double GetHeightValue(double xpos, double ypos, bool logwrite = false)
        {
            throw new NotImplementedException();
        }
        public double GetHeightValueAddZOffset(double xpos, double ypos, double zoffset, bool logwrite = false)
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum GetHeightValueAddZOffsetFromDutIndex(EnumProberCam camtype, long mach_x, long mach_y, double zoffset, out double zpos)
        {
            throw new NotImplementedException();
        }
        public int SaveWaferAveThick()
        {
            throw new NotImplementedException();
        }

        public WaferCoordinate MachineIndexConvertToDieLeftCorner(long xindex, long yindex)
        {
            WaferCoordinate retval = new WaferCoordinate(0, 0);

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToDieLeftCorner(xindex, yindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //if (LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>() != null)
            //{
            //    return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToDieLeftCorner(xindex, yindex);
            //}
            //else
            //{
            //    return new WaferCoordinate(0, 0);
            //}
        }

        public WaferCoordinate MachineIndexConvertToDieLeftCorner_NonCalcZ(long xindex, long yindex)
        {
            WaferCoordinate retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToDieLeftCorenr_NonCalcZ(xindex, yindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToDieLeftCorenr_NonCalcZ(xindex, yindex);
        }

        public WaferCoordinate MachineIndexConvertToDieCenter(long xindex, long yindex)
        {
            WaferCoordinate retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToDieCenter(xindex, yindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToDieCenter(xindex, yindex);
        }

        public WaferCoordinate MachineIndexConvertToProbingCoord(long xindex, long yindex, bool logWrite = true)
        {
            WaferCoordinate retval = null;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToProbingCoord(xindex, yindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().MachineIndexConvertToProbingCoord(xindex, yindex);
        }

        public Point GetLeftCornerPosition(double positionx, double positiony)
        {
            Point retval = new Point(0, 0);

            try
            {
                WaferCoordinate coordinate = new WaferCoordinate(positionx, positiony);

                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetLeftCornerPositionForWAFCoord(coordinate);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //if (LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>() != null)
            //{
            //    WaferCoordinate coordinate = new WaferCoordinate(positionx, positiony);

            //    return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetLeftCornerPositionForWAFCoord(coordinate);
            //}
            //else
            //{
            //    return new Point(0, 0);
            //}
        }

        public Point GetLeftCornerPosition(WaferCoordinate position)
        {
            Point retval = new Point(0, 0);

            try
            {
                var proxy = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>();
                if (proxy != null)
                {
                    retval = proxy.GetLeftCornerPositionForWAFCoord(position);
                }
                else 
                { 
                    // 로더쪽이지만 ... 혹시나 UI에서 그릴떄 문제될수 있을것 같아서 로그는 안바름
                }
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //if (LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>() != null)
            //{
            //    return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetLeftCornerPositionForWAFCoord(position);
            //}
            //else
            //{
            //    return new Point(0, 0);
            //}
        }

        public Point UserIndexConvertLeftBottomCorner(UserIndex uindex)
        {
            throw new NotImplementedException();
        }

        public Point UserIndexConvertLeftBottomCorner(long xindex, long yindex)
        {
            throw new NotImplementedException();
        }

        public MachineIndex WPosToMIndex(WaferCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public void SetSetupState()
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.SetSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetModuleDoneState()
        {
            throw new NotImplementedException();
        }

        public void InitIdelState()
        {
            throw new NotImplementedException();
        }

        public void UpdatePadCen()
        {
            return;
        }

        public void CreateBaseHeightProfiling(WaferCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetTeachDevice(bool isMoving = true, long xindex = -1, long yindex = -1, EnumProberCam enumProberCam = EnumProberCam.WAFER_HIGH_CAM)
        {
            throw new NotImplementedException();
        }

        public List<DutWaferIndex> GetDutDieIndexs()
        {
            throw new NotImplementedException();
        }

        public void SetDefaultDutDieIndexs()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckPossibleSetup(bool isrecovery = false)
        {
            bool retval = false;

            try
            {
                retval = await LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.CheckPossibleSetup(isrecovery);

                //return await LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.CheckPossibleSetup(isrecovery);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return await LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.CheckPossibleSetup(isrecovery);
        }

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            throw new NotImplementedException();
        }

        public void SetHeader(string name)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<IWizardStep> GetWizardStep()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearSettingData()
        {
            throw new NotImplementedException();
        }

        public void SetEnableState(EnumEnableState state)
        {
            throw new NotImplementedException();
        }

        public void ChangeSetupState(IMoudleSetupState state)
        {
            throw new NotImplementedException();
        }

        public void SetNodeSetupState(EnumMoudleSetupState state, bool isparent = false)
        {
            throw new NotImplementedException();
        }
        public void SetNodeSetupRecoveryState(EnumMoudleSetupState state, bool isparent = false)
        {
            throw new NotImplementedException();
        }
        

        public void SetStepSetupState(string header = null)
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            throw new NotImplementedException();
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            throw new NotImplementedException();
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Execute()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().ClearState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().ClearState();
        }

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {

        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsLotReady(out string msg)
        {
            bool retval = true;
            try
            {
                msg = "";
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public Point GetLeftCornerPositionForWAFCoord(WaferCoordinate position)
        {
            Point retval = new Point(0, 0);

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetLeftCornerPositionForWAFCoord(position);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetLeftCornerPositionForWAFCoord(position);
        }

        public bool IsServiceAvailable()
        {
            bool retval = false;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().IsServiceAvailable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().IsServiceAvailable();
        }

        public void SetIsNewSetup(bool flag)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().SetIsNewSetup(flag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumMoudleSetupState GetModuleSetupState()
        {
            return EnumMoudleSetupState.NONE;
        }

        public EventCodeEnum EdgeCheck(ref WaferCoordinate centeroffset, ref double maximum_value_X, ref double maximum_value_Y)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().EdgeCheck(ref centeroffset, ref maximum_value_X, ref maximum_value_Y);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().EdgeCheck(ref centeroffset);
        }
        public ModuleStateEnum GetModuleState()
        {
            ModuleStateEnum retval = ModuleStateEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetModuleState();
        }
        public ModuleStateEnum GetPreModuleState()
        {
            ModuleStateEnum retval = ModuleStateEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetPreModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetPreModuleState();
        }
        public EventCodeEnum ClearRecoveryData()
        {
            return EventCodeEnum.NONE;
        }

        public double CalcThreePodTiltedPlane(double posx, double posy, bool logwrite = false)
        {
            throw new NotImplementedException();
        }

        public void AddHeighPlanePoint(WaferCoordinate wcoord)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetHighStandardParam()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetHighStandardParam(HeightPointEnum heightpoint)
        {
            throw new NotImplementedException();
        }


        public void PlanePointChangetoFocusing9pt(ObservableCollection<WAHeightPositionParam> heightparam)
        {
            throw new NotImplementedException();
        }

        public void PlanePointChangetoFocusing5pt(double xpos, double ypos, double zpos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveRecoveryLowPattern()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SaveRecoveryHighPattern()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ClearRecoverySetupPattern()
        {
            return EventCodeEnum.NONE;
        }
        public void SetRefPad(IList<DUTPadObject> padinfo)
        {

        }
        public (double, double) GetVerifyCenterLimitXYValue()
        {
            (double, double) retVal = (0,0);

            try
            {
                retVal = LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().GetVerifyCenterLimitXYValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void SetVerifyCenterLimitXYValue(double xLimit, double yLimit)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().SetVerifyCenterLimitXYValue(xLimit, yLimit);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public List<Guid> GetRecoverySteps()
        {
            throw new NotImplementedException();
        }
        public WaferCoordinate GetPatternWaferCenter()
        {
            throw new NotImplementedException();
        }

        public bool IsManualOPReady(out string msg)
        {
            throw new NotImplementedException();
        }

        public void CompleteManualOperation(EventCodeEnum retval)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoManualOperation(IProbeCommandParameter param = null)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoWaferAlignProcess()
        {
            throw new NotImplementedException();
        }

        public void SetIsModifySetup(bool flag)
        {
            LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>().SetIsModifySetup(flag);
        }

        public bool GetIsModifySetup()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum CalculateOffsetToAutoFocusedPosition(ICamera curcam, double padsize_x, double padsize_y)
        {
            throw new NotImplementedException();
        }

        public void WaferEdgeImageDecode()
        {
            throw new NotImplementedException();
        }

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
        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        private bool _IsNewSetup;

        public bool IsNewSetup
        {
            get { return _IsNewSetup; }
            set
            {
                _IsNewSetup = value;

                LoaderCommunicationManager.GetProxy<IWaferAlignerProxy>()?.SetIsNewSetup(_IsNewSetup);
            }
        }

        public int TotalHeightPoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool waferaligncontinus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public WaferAlignInfomation WaferAlignInfo => throw new NotImplementedException();

        public WARecoveryModule RecoveryInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long TeachDieXIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long TeachDieYIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IWaferAlignControItems WaferAlignControItems => throw new NotImplementedException();

        public TemplateStateCollection Template { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITemplateFileParam TemplateParameter => throw new NotImplementedException();

        public ITemplateParam LoadTemplateParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubRoutine SubRoutine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public UserControl UCDetailSummary => throw new NotImplementedException();

        public ICategoryNodeItem Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ObservableCollection<ITemplateModule> Categories { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Header { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RecoveryHeader { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid PageGUID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public EnumEnableState StateEnable => throw new NotImplementedException();

        public EnumMoudleSetupState StateSetup => throw new NotImplementedException();

        public EnumMoudleSetupState StateRecoverySetup => throw new NotImplementedException();

        public bool NoneCleanUp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Genealogy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public object Owner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<object> Nodes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Guid ScreenGUID => throw new NotImplementedException();

        public ReasonOfError ReasonOfError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState { get; set; }

        public ObservableCollection<TransitionInfo> TransitionInfo => throw new NotImplementedException();

        public EnumModuleForcedState ForcedDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IInnerState PreInnerState => throw new NotImplementedException();

        public string ManuallAlignmentErrTxt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public WaferCoordinate PlanePointCenter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HeightPointEnum HeightProfilingPointType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsManualTriggered { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsManualOPFinished { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EventCodeEnum ManualOPResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double SafeHeightOnException => throw new NotImplementedException();
    }
}
