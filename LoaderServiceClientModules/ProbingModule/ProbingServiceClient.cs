using System;
using System.Collections.Generic;
using System.Linq;

namespace LoaderServiceClientModules
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Autofac;
    using BinParamObject;
    using LoaderBase.Communication;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.BinData;
    using ProberInterfaces.Command;
    using ProberInterfaces.Loader.RemoteDataDescription;
    using ProberInterfaces.Param;
    using ProberInterfaces.Retest;
    using ProberInterfaces.State;
    using ProbingModule;
    using SerializerUtil;

    public class ProbingServiceClient : IProbingModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        #region //..Property

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        private IParam _ProbingModuleDevParam_IParam;
        public IParam ProbingModuleDevParam_IParam
        {
            get
            {
                _ProbingModuleDevParam_IParam = ProbingDevParam();
                return _ProbingModuleDevParam_IParam;
            }
            set { _ProbingModuleDevParam_IParam = value; }
        }


        private IParam _ProbingModuleSysParam_IParam;
        public IParam ProbingModuleSysParam_IParam
        {
            get { return _ProbingModuleSysParam_IParam; }
            set
            {
                if (value != _ProbingModuleSysParam_IParam)
                {
                    _ProbingModuleSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ProbingModuleSysParam ProbingModuleSysParamRef
        {
            get { return ProbingModuleSysParam_IParam as ProbingModuleSysParam; }
        }
        public ProbingModuleDevParam ProbingModuleDevParamRef
        {
            get { return ProbingModuleDevParam_IParam as ProbingModuleDevParam; }
        }

        private ProbingInfo _ProbingProcessStatus;
        public ProbingInfo ProbingProcessStatus
        {
            get { return _ProbingProcessStatus; }
            set
            {
                if (value != _ProbingProcessStatus)
                {
                    _ProbingProcessStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IParam GetProbingDevIParam(int idx = -1)
        {
            return ProbingDevParam(idx);
        }


        public void SetProbingDevParam(IParam param, int idx = -1)
        {
            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(idx);

                if (proxy != null)
                {
                    //// IParam to byte array
                    //byte[] compressedData = null;

                    //var bytes = SerializeManager.SerializeToByte(param, typeof(ProbingModuleDevParam));
                    //compressedData = bytes;

                    proxy.SetProbingDevParam(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public byte[] GetProbingDevParam(int idx = -1)
        {
            byte[] retval = null;

            IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(idx);

            if (proxy != null)
            {
                retval = proxy.GetProbingDevParam();
            }

            return retval;
        }



        public ProbingModuleDevParam ProbingDevParam(int idx = -1)
        {
            ProbingModuleDevParam retval = null;

            try
            {
                byte[] obj = GetProbingDevParam(idx);
                object target = null;

                if (obj != null)
                {
                    var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(ProbingModuleDevParam));
                    retval = target as ProbingModuleDevParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retval;
        }
        public PinCoordinate PinZeroPos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public EnumProbingState ProbingStateEnum => throw new NotImplementedException();

        public MachineIndex ProbingLastMIndex => throw new NotImplementedException();

        public double ZClearence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double OverDrive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private double _FirstContactHeight;
        public double FirstContactHeight
        {
            get { return _FirstContactHeight; }
            set
            {
                if (value != _FirstContactHeight)
                {
                    _FirstContactHeight = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _AllContactHeight;
        public double AllContactHeight
        {
            get { return _AllContactHeight; }
            set
            {
                if (value != _AllContactHeight)
                {
                    _AllContactHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsFirstSequence = true;
        public bool IsFirstZupSequence
        {
            get { return _IsFirstSequence; }
            set
            {
                if (value != _IsFirstSequence)
                {
                    _IsFirstSequence = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProbingMXIndex;
        public int ProbingMXIndex
        {
            get { return _ProbingMXIndex; }
            set
            {
                if (value != _ProbingMXIndex)
                {
                    _ProbingMXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProbingMYIndex;
        public int ProbingMYIndex
        {
            get { return _ProbingMYIndex; }
            set
            {
                if (value != _ProbingMYIndex)
                {
                    _ProbingMYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsReservePause { get; set; }
        public bool StilProbingZUpFlag { get; set; }
        public bool ProbingDryRunFlag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IRetestModule RetestModule { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ProbingEndReason ProbingEndReason { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ReasonOfError ReasonOfError { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState => throw new NotImplementedException();

        public ObservableCollection<TransitionInfo> TransitionInfo => throw new NotImplementedException();

        public EnumModuleForcedState ForcedDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Initialized => throw new NotImplementedException();

        EnumProbingState IProbingModule.PreProbingStateEnum { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanResume => throw new NotImplementedException();
        #endregion

        #region //..Method
        public void SetProbingUnderdut(ObservableCollection<IDeviceObject> UnderDutDevs)
        {
            ProbingProcessStatus.UnderDutDevs = UnderDutDevs;
        }
        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public void AddCPCParameter(IChuckPlaneCompParameter cpcparam)
        {
            throw new NotImplementedException();
        }

        public double CalculateZClearenceUsingOD(double overDrive, double zClearence)
        {
            throw new NotImplementedException();
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            return;
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Execute()
        {
            throw new NotImplementedException();
        }



        public void GetCPCValues(double position, out double z0, out double z1, out double z2)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<IChuckPlaneCompParameter> GetCPCValues()
        {
            throw new NotImplementedException();
        }

        public double GetDeflectX()
        {
            throw new NotImplementedException();
        }

        public double GetDeflectY()
        {
            throw new NotImplementedException();
        }

        public double GetInclineZHor()
        {
            throw new NotImplementedException();
        }

        public double GetInclineZVer()
        {
            throw new NotImplementedException();
        }

        public OverDriveStartPositionType GetOverDriveStartPosition()
        {
            throw new NotImplementedException();
        }

        public CatCoordinates GetPMShifhtValue()
        {
            throw new NotImplementedException();
        }

        public double GetSquarenceValue()
        {
            throw new NotImplementedException();
        }

        public double GetTwistValue()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetUnderDutDices(MachineIndex mCoord)
        {
            throw new NotImplementedException();
        }

        public List<IDeviceObject> GetUnderDutList(MachineIndex mCoord)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbingModuleSysParam_IParam = new ProbingModuleSysParam();
                ProbingProcessStatus = new ProbingInfo();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retVal;
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

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }

        public bool IsLotHasSuspendedState()
        {
            throw new NotImplementedException();
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

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public void ProbingRestart()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ProbingSequenceTransfer(int moveIdx)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ProbingSequenceTransfer(MachineIndex curMI, int remainSeqCnt)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveDevParameter()
        {
            return LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>().ProbingModuleSaveDevParameter();
        }

        public EventCodeEnum SaveDevParameter(IParam param, int idx = -1)
        {
            return LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>(idx).ProbingModuleSaveDevParameter();
        }

        public EventCodeEnum SaveSysParameter()
        {
            throw new NotImplementedException();
        }

        public void SetDeflectX(double value)
        {
            throw new NotImplementedException();
        }

        public void SetDeflectY(double value)
        {
            throw new NotImplementedException();
        }

        public void SetInclineZHor(double value)
        {
            throw new NotImplementedException();
        }

        public void SetInclineZVer(double value)
        {
            throw new NotImplementedException();
        }

        public void SetProbeShiftValue(CatCoordinates shiftvalue)
        {
            throw new NotImplementedException();
        }

        public void SetProbingMachineIndexToCenter()
        {
            throw new NotImplementedException();
        }

        public void SetSquarenceValue(double value)
        {
            throw new NotImplementedException();
        }

        public void SetTwistValue(double value)
        {
            throw new NotImplementedException();
        }

        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum GetStagePIVProbingData(ref long XIndex, ref long YIndex, UserIndex userindex, ref string full_site)
        {
            return EventCodeEnum.NONE;
        }

        public CatCoordinates GetSetTemperaturePMShifhtValue()
        {
            return LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>().ProbingModule().GetSetTemperaturePMShifhtValue();
        }

        public void SetProbeTempShiftValue(CatCoordinates shiftvalue)
        {
            throw new NotImplementedException();
        }

        public void SetLastProbingMIndex(MachineIndex index)
        {
            throw new NotImplementedException();
        }

        public bool DutIsInRange(long xindex, long yindex)
        {
            throw new NotImplementedException();
        }

        public bool GetRetestEnable(RetestTypeEnum type, long xindex, long yindex)
        {
            throw new NotImplementedException();
        }

        public bool NeedRetest(long xindex, long yindex)
        {
            throw new NotImplementedException();
        }

        public bool NeedRetestbyBIN(RetestTypeEnum type, long xindex, long yindex)
        {
            throw new NotImplementedException();
        }
        public bool GetEnablePTPAEnhancer()
        {
            throw new NotImplementedException();
        }

        public void SetCanResume(bool canResume)
        {
            throw new NotImplementedException();
        }

        public Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable()
        {
            throw new NotImplementedException();
        }

        public List<IBINInfo> GetBinInfos()
        {
            List<IBINInfo> retval = null;

            try
            {
                byte[] bytes = GetBinDevParam();

                object target = null;

                if (bytes != null)
                {
                    var result = SerializeManager.DeserializeFromByte(bytes, out target, typeof(BinDeviceParam));

                    if (result == true && target != null)
                    {
                        BinDeviceParam bindevparam = target as BinDeviceParam;

                        if (bindevparam != null && bindevparam.BinInfos != null && bindevparam.BinInfos.Value != null)
                        {
                            retval = bindevparam.BinInfos.Value.ToList<IBINInfo>();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public byte[] GetBinDevParam()
        {
            byte[] retval = null;

            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                retval = proxy.GetBinDevParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetBinInfos(List<IBINInfo> binInfos)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                retval = proxy.SetBinInfos(binInfos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveBinDevParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

                retval = proxy.SaveBinDevParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsTestedDIE(long xindex, long yindex)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ResetOnWaferInformation()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            throw new NotImplementedException();
        }
        public void SetProbingEndState(ProbingEndReason probingEndReason, EnumWaferState WaferState = EnumWaferState.UNDEFINED)
        {

        }

        public EventCodeEnum ClearUnderDutDevs()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                // Loader 가 가진 정보만 clear
                // Stage 는 정보를 유지 하고 있다 dut map 이 그려지는것을 원하는 화면 진입 시에 다시 Loader 정보를 채워줘야 함.
                if (this.ProbingProcessStatus?.UnderDutDevs != null)
                {
                    this.ProbingProcessStatus.UnderDutDevs.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public DateTime GetZupStartTime()
        {
            return new DateTime();
        }

        public MarkShiftValues GetUserSystemMarkShiftValue()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
