using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ProberInterfaces;
using ProberInterfaces.AutoTilt;
using ProberInterfaces.PinAlign.ProbeCardData;
using ProberInterfaces.Command;
using ProberErrorCode;

using ProberInterfaces.State;
using ProberInterfaces.Wizard;
using LogModule;
using System.Runtime.CompilerServices;

namespace AutoTiltModule
{

    public class AutoTiltModule : IAutoTiltModule, INotifyPropertyChanged, IHasDevParameterizable, IHasSysParameterizable
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private CommandInformation _CommandInfo;
        public CommandInformation CommandInfo
        {
            get { return _CommandInfo; }
            set { _CommandInfo = value; }
        }
        
        private ReasonOfError _ReasonOfError = new ReasonOfError(ModuleEnum.AutoTilt);
        public ReasonOfError ReasonOfError
        {
            get { return _ReasonOfError; }
            set
            {
                if (value != _ReasonOfError)
                {
                    _ReasonOfError = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AutoTiltStateBase _AutoTiltState;
        public AutoTiltStateBase AutoTiltState
        {
            get { return _AutoTiltState; }
        }
        public IInnerState InnerState
        {
            get { return _AutoTiltState; }
            set
            {
                if (value != _AutoTiltState)
                {
                    _AutoTiltState = value as AutoTiltStateBase;
                }
            }
        }
        public IInnerState PreInnerState { get; set; }

        #region Properties
        private ModuleStateBase _ModuleState;
        public ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            set
            {
                if (value != _ModuleState)
                {
                    _ModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
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
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private AutoTiltDeviceFile _ATDeviceFile;
        public AutoTiltDeviceFile ATDeviceFile
        {
            get { return _ATDeviceFile; }
            set
            {
                if (value != _ATDeviceFile)
                {
                    _ATDeviceFile = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AutoTiltSystemFile _ATSysFile;
        public AutoTiltSystemFile ATSysFile
        {
            get { return _ATSysFile; }
            set
            {
                if (value != _ATSysFile)
                {
                    _ATSysFile = value;
                    RaisePropertyChanged();
                }
            }
        }

        const double TOPPLATE_Diag = 1120;
        const double COEF_TO_DEG = 57.2957795130823;
        const double COEF_TO_RAD = 1.74532925199433E-02;
        const double TOPPLATE_width = 890;
        const double TOPPLATE_hgt = 680;


        public double _TZ1Offset;
        public double TZ1Offset
        {
            get { return _TZ1Offset; }
            set
            {
                if (value != _TZ1Offset)
                {
                    _TZ1Offset = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double _TZ2Offset;
        public double TZ2Offset
        {
            get { return _TZ2Offset; }
            set
            {
                if (value != _TZ2Offset)
                {
                    _TZ2Offset = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double _TZ3Offset;
        public double TZ3Offset
        {
            get { return _TZ3Offset; }
            set
            {
                if (value != _TZ3Offset)
                {
                    _TZ3Offset = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<IPinData> AlignPinData = new List<IPinData>();



        private CommandSlot _CommandRecvSlot = new CommandSlot();
        public CommandSlot CommandRecvSlot
        {
            get { return _CommandRecvSlot; }
            set { _CommandRecvSlot = value; }
        }

        private CommandSlot _CommandProcSlot = new CommandSlot();
        public CommandSlot CommandRecvProcSlot
        {
            get { return _CommandProcSlot; }
            set { _CommandProcSlot = value; }
        }

        private CommandSlot _CommandRecvDoneSlot = new CommandSlot();
        public CommandSlot CommandRecvDoneSlot
        {
            get { return _CommandRecvDoneSlot; }
            set { _CommandRecvDoneSlot = value; }
        }

        private CommandSlot _CommandSendSlot = new CommandSlot();
        public CommandSlot CommandSendSlot
        {
            get { return _CommandSendSlot; }
            set { _CommandSendSlot = value; }
        }
        private CommandTokenSet _RunTokenSet;

        public CommandTokenSet RunTokenSet
        {
            get { return _RunTokenSet; }
            set { _RunTokenSet = value; }
        }
        private SubStepModules _SubModules;
        public ISubStepModules SubModules
        {
            get { return _SubModules; }
            set
            {
                if (value != _SubModules)
                {
                    _SubModules = (SubStepModules)value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumModuleForcedState _ForcedDone = EnumModuleForcedState.Normal;
        public EnumModuleForcedState ForcedDone
        {
            get { return _ForcedDone; }
            set { _ForcedDone = value; }
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
            bool retVal = false;
            try
            {
                foreach (var subModule in SubModules.SubModules)
                {
                    if (subModule.GetMovingState() == MovingStateEnum.MOVING)
                    {
                        retVal = true;
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
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    _AutoTiltState = new AutoTiltIDLEState(this);
                    ModuleState = new ModuleIdleState(this);

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
            }

            return retval;
        }
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
        public void StateTransition(ModuleStateBase state)
        {
            ModuleState = state;
        }

        public EventCodeEnum InnerStateTransition(IInnerState state)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                PreInnerState = InnerState;
                InnerState = state;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }



        public EventCodeEnum ClearState()  //Data 초기화 함=> Done에서 IDLE 상태로 넘어감
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = InnerState.ClearState();
                ModuleState.StateTransition(InnerState.GetModuleState());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public ModuleStateEnum Pause()  //Pause가 호출했을때 해야하는 행동
        {
            try
            {
                InnerState.Pause();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Resume() // Pause가 풀렸을때 해야하는 행동
        {
            try
            {
                InnerState.Resume();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum End() // Abort 시킬때 해야하는 행동
        {
            try
            {
                InnerState.End();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public ModuleStateEnum Abort()
        {
            try
            {
                InnerState.Abort();
                ModuleState.StateTransition(InnerState.GetModuleState());
                return InnerState.GetModuleState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public ModuleStateEnum Execute() // Don`t Touch
        {
            ModuleStateEnum stat = ModuleStateEnum.ERROR;
            try
            {
                EventCodeEnum retVal = InnerState.Execute();
                ModuleState.StateTransition(InnerState.GetModuleState());
                RunTokenSet.Update();
                stat = InnerState.GetModuleState();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return stat;
        }

        public void RunAsync()
        {
            throw new NotImplementedException();
        }



        public void inputPinData(ref double[] npposx, ref double[] npposy, ref double[] npposz)
        {
            try
            {

                for (int i = 0; i < AlignPinData.Count; i++)
                {
                    npposx[i] = AlignPinData[i].AbsPos.X.Value;
                    npposy[i] = AlignPinData[i].AbsPos.Y.Value;
                    npposz[i] = AlignPinData[i].AbsPos.Z.Value;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public double CheckPinPlanePlanarity_By_Pin_HPT(out double pindif)
        {
            try
            {
                UpdatePinInfo();
                double[,] local_HPT_hgt_focus_pos = new double[4, 3];

                double[] dblPPX = new double[4];
                double[] dblPPY = new double[4];
                double[] dblPPZ = new double[4];

                double[] dblPPMinDist = new double[4];
                int[] dblPPRepPinNum = new int[4];
                // double dblTmpBuff = 0.0;
                int[] intPlPtsNum = new int[4];

                double[,] dblCardMaxSize = new double[4, 2];
                double[,] dblPPMeanPos = new double[4, 3];

                // double hpt_tilt_angle = 0.0;
                // double hpt_tilt_hgt = 0.0;

                double[] dbPlaneLowestPinZ = new double[4];
                double[] dbPlaneHighestPinZ = new double[4];

                double[,] dbPlaneXpos = new double[2, 4];
                double[,] dbPlaneYpos = new double[2, 4];


                double local_lowestplane = -1;
                double local_highestplane = -1;
                double local_DiffX = 0.0;
                double local_DiffY = 0.0;

                int local_available_plane_cnt = 0;

                for (int i = 0; i < 4; i++)
                {
                    intPlPtsNum[i] = 0;

                    dblPPX[i] = 0;
                    dblPPY[i] = 0;
                    dblPPZ[i] = 0;
                }


                //Todo 데이터 다시 가져오기 
                int pinNum = AlignPinData.Count;

                double[] NewPinPosX = new double[pinNum]; // >>할당을 몇개 해야할지 모르겠음 
                double[] NewPinPosY = new double[pinNum]; //어떤 데이터인지 모르겠음
                double[] NewPinPosZ = new double[pinNum];
                ////////////////////////////////////////////////////
                inputPinData(ref NewPinPosX, ref NewPinPosY, ref NewPinPosZ);  /////////////////PinData를 얻어와야함 얻어오는게 필요함 
                                                                               ////////////////////////////////////////////////////

                const int PP_NUM0 = 0; // 무엇 인지 모름 
                const int PP_NUM1 = 1;
                const int PP_NUM2 = 2;
                const int PP_NUM3 = 3;

                double gTilt_SKIP_AREA_RatioX = ATDeviceFile.TiltSkipAreaRatioX.Value; // 무엇인지 모름 
                double gTilt_SKIP_AREA_RatioY = ATDeviceFile.TiltSkipAreaRatioY.Value;

                int PinAdjustEnable = ATDeviceFile.PinAdjustEnable.Value; //핀얼라인 기능이 켜져있냐 

                double total_avg = 0.0;

                int gTilt_intPlaneMinMaxtol = ATDeviceFile.TiltIntPlaneMinMaxtol.Value;
                int gTilt_Check_Method = 0;

                for (int i = 0; i < pinNum; i++) //PinNum이 어떤걸 의미 하는지 
                {
                    if (PinAdjustEnable > 0 &&
                        !(AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_PASSED ||
                        AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_OVER_TOLERANCE))
                    {
                        // 
                    }
                    else
                    {
                        if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                                           NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[0] = intPlPtsNum[0] + 1;
                            dblPPMeanPos[PP_NUM0, 0] = dblPPMeanPos[PP_NUM0, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM0, 1] = dblPPMeanPos[PP_NUM0, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM0, 2] = dblPPMeanPos[PP_NUM0, 2] + NewPinPosZ[i];

                            if (NewPinPosX[i] > dblCardMaxSize[PP_NUM0, 0])
                            {
                                dblCardMaxSize[PP_NUM0, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] > dblCardMaxSize[PP_NUM0, 1])
                            {
                                dblCardMaxSize[PP_NUM0, 1] = NewPinPosY[i];
                            }
                        }
                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[1] = intPlPtsNum[1] + 1;
                            dblPPMeanPos[PP_NUM1, 0] = dblPPMeanPos[PP_NUM1, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM1, 1] = dblPPMeanPos[PP_NUM1, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM1, 2] = dblPPMeanPos[PP_NUM1, 2] + NewPinPosZ[i];

                            if (NewPinPosX[i] < dblCardMaxSize[PP_NUM1, 0])
                            {
                                dblCardMaxSize[PP_NUM1, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] > dblCardMaxSize[PP_NUM1, 1])
                            {
                                dblCardMaxSize[PP_NUM1, 1] = NewPinPosY[i];
                            }
                        }

                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[2] = intPlPtsNum[2] + 1;
                            dblPPMeanPos[PP_NUM2, 0] = dblPPMeanPos[PP_NUM2, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM2, 1] = dblPPMeanPos[PP_NUM2, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM2, 2] = dblPPMeanPos[PP_NUM2, 2] + NewPinPosZ[i];
                            if (NewPinPosX[i] < dblCardMaxSize[PP_NUM2, 0])
                            {
                                dblCardMaxSize[PP_NUM2, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] < dblCardMaxSize[PP_NUM2, 1])
                            {
                                dblCardMaxSize[PP_NUM2, 1] = NewPinPosY[i];
                            }
                        }

                        else if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[3] = intPlPtsNum[3] + 1;
                            dblPPMeanPos[PP_NUM3, 0] = dblPPMeanPos[PP_NUM3, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM3, 1] = dblPPMeanPos[PP_NUM3, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM3, 2] = dblPPMeanPos[PP_NUM3, 2] + NewPinPosZ[i];

                            if (NewPinPosX[i] < dblCardMaxSize[PP_NUM3, 1])
                            {
                                dblCardMaxSize[PP_NUM3, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] < dblCardMaxSize[PP_NUM3, 1])
                            {
                                dblCardMaxSize[PP_NUM3, 1] = NewPinPosY[i];
                            }
                        }
                        else
                        {

                        }
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    if (intPlPtsNum[i] == 0)
                    {
                        dblPPMeanPos[i, 0] = 0;
                        dblPPMeanPos[i, 1] = 0;
                        dblPPMeanPos[i, 2] = 0;
                    }
                    else
                    {
                        dblPPMeanPos[i, 0] = dblPPMeanPos[i, 0] / intPlPtsNum[i];
                        dblPPMeanPos[i, 1] = dblPPMeanPos[i, 1] / intPlPtsNum[i];
                        dblPPMeanPos[i, 2] = dblPPMeanPos[i, 2] / intPlPtsNum[i];
                        local_available_plane_cnt = local_available_plane_cnt + 1;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    dblPPX[i] = 0;
                    dblPPY[i] = 0;
                    dblPPZ[i] = 0;
                }

                for (int i = 0; i < pinNum; i++)
                {
                    if (PinAdjustEnable > 0 &&
                        !(AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_PASSED ||
                        AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_OVER_TOLERANCE))
                    {
                        //
                    }
                    else
                    {
                        if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[0, 2])
                            {
                                if (dbPlaneLowestPinZ[0] != 0)
                                {
                                    if (dbPlaneLowestPinZ[0] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[0] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 0] = NewPinPosX[i];
                                        dbPlaneYpos[0, 0] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[0] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 0] = NewPinPosX[i];
                                    dbPlaneYpos[0, 0] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[0, 2])
                            {
                                if (dbPlaneHighestPinZ[0] != 0)
                                {
                                    if (dbPlaneHighestPinZ[0] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[0] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 0] = NewPinPosX[i];
                                        dbPlaneYpos[1, 0] = NewPinPosY[i];
                                    }
                                }
                            }
                        }

                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[1, 2])
                            {
                                if (dbPlaneLowestPinZ[1] != 0)
                                {
                                    if (dbPlaneLowestPinZ[1] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[1] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 1] = NewPinPosX[i];
                                        dbPlaneYpos[0, 1] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[1] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 1] = NewPinPosX[i];
                                    dbPlaneYpos[0, 1] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[1, 2])
                            {
                                if (dbPlaneHighestPinZ[1] != 0)
                                {
                                    if (dbPlaneHighestPinZ[1] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[1] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 1] = NewPinPosX[i];
                                        dbPlaneYpos[1, 1] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[1] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 1] = NewPinPosX[i];
                                    dbPlaneYpos[1, 1] = NewPinPosY[i];
                                }
                            }
                        }
                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[2, 2])
                            {
                                if (dbPlaneLowestPinZ[2] != 0)
                                {
                                    if (dbPlaneLowestPinZ[2] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[2] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 2] = NewPinPosX[i];
                                        dbPlaneYpos[0, 2] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[2] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 2] = NewPinPosX[i];
                                    dbPlaneYpos[0, 2] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[2, 2])
                            {
                                if (dbPlaneHighestPinZ[2] != 0)
                                {
                                    if (dbPlaneHighestPinZ[2] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[2] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 2] = NewPinPosX[i];
                                        dbPlaneYpos[1, 2] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[2] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 2] = NewPinPosX[i];
                                    dbPlaneYpos[1, 2] = NewPinPosY[i];
                                }
                            }
                        }
                        else if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[3, 2])
                            {
                                if (dbPlaneLowestPinZ[3] != 0)
                                {
                                    if (dbPlaneLowestPinZ[3] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[3] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 3] = NewPinPosX[i];
                                        dbPlaneYpos[0, 3] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[3] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 3] = NewPinPosX[i];
                                    dbPlaneYpos[0, 3] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[3, 2])
                            {
                                if (dbPlaneHighestPinZ[3] != 0)
                                {
                                    if (dbPlaneHighestPinZ[3] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[3] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 3] = NewPinPosX[i];
                                        dbPlaneYpos[1, 3] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[3] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 3] = NewPinPosX[i];
                                    dbPlaneYpos[1, 3] = NewPinPosY[i];
                                }
                            }
                        }

                    }

                }

                total_avg = (dblPPMeanPos[0, 2] + dblPPMeanPos[1, 2] + dblPPMeanPos[2, 2] + dblPPMeanPos[3, 2]) / 4;

                for (int i = 0; i < 4; i++)
                {
                    dblPPZ[i] = dblPPMeanPos[i, 2];
                    dblPPX[i] = dblPPMeanPos[i, 0];
                    dblPPY[i] = dblPPMeanPos[i, 1];

                    if (intPlPtsNum[i] > 1 &&
                        i != 0 &&
                        Math.Abs(dbPlaneHighestPinZ[i] - total_avg) > Math.Abs(dbPlaneLowestPinZ[i] - total_avg))
                    {
                        if (dbPlaneHighestPinZ[i] != 0 &&
                            (gTilt_intPlaneMinMaxtol <= Math.Abs(dbPlaneHighestPinZ[i] - total_avg)) &&
                            intPlPtsNum[i] > 1)
                        {
                            local_HPT_hgt_focus_pos[i, 2] = (dblPPMeanPos[i, 2] * intPlPtsNum[i] - dbPlaneHighestPinZ[i]) / (intPlPtsNum[i] - 1);
                            local_HPT_hgt_focus_pos[i, 0] = (dblPPMeanPos[i, 0] * intPlPtsNum[i] - dbPlaneXpos[1, i]) / (intPlPtsNum[i] - 1);
                            local_HPT_hgt_focus_pos[i, 1] = (dblPPMeanPos[i, 1] * intPlPtsNum[i] - dbPlaneYpos[1, i]) / (intPlPtsNum[i] - 1);
                        }
                        else
                        {
                            local_HPT_hgt_focus_pos[i, 2] = dblPPMeanPos[i, 2];
                            local_HPT_hgt_focus_pos[i, 0] = dblPPMeanPos[i, 0];
                            local_HPT_hgt_focus_pos[i, 1] = dblPPMeanPos[i, 1];
                        }
                    }

                    else if (intPlPtsNum[i] > 1 &&
                        i != 0 &&
                        Math.Abs(dbPlaneHighestPinZ[i] - total_avg) < Math.Abs(dbPlaneLowestPinZ[i] - total_avg))
                    {
                        if (dbPlaneLowestPinZ[i] != 0 &&
                            (gTilt_intPlaneMinMaxtol <= (Math.Abs(dbPlaneLowestPinZ[i] - total_avg))) &&
                            intPlPtsNum[i] > 1)
                        {
                            local_HPT_hgt_focus_pos[i, 2] = (dblPPMeanPos[i, 2] * intPlPtsNum[i] - dbPlaneLowestPinZ[i]) / (intPlPtsNum[i] - 1);
                            local_HPT_hgt_focus_pos[i, 0] = (dblPPMeanPos[i, 0] * intPlPtsNum[i] - dbPlaneXpos[0, i]) / (intPlPtsNum[i] - 1);
                            local_HPT_hgt_focus_pos[i, 1] = (dblPPMeanPos[i, 1] * intPlPtsNum[i] - dbPlaneYpos[0, i]) / (intPlPtsNum[i] - 1);
                        }
                        else
                        {
                            local_HPT_hgt_focus_pos[i, 2] = dblPPMeanPos[i, 2];
                            local_HPT_hgt_focus_pos[i, 0] = dblPPMeanPos[i, 0];
                            local_HPT_hgt_focus_pos[i, 1] = dblPPMeanPos[i, 2];
                        }
                    }
                    else
                    {
                        if (intPlPtsNum[i] >= 1)
                        {
                            local_HPT_hgt_focus_pos[i, 2] = dblPPMeanPos[i, 2];
                            local_HPT_hgt_focus_pos[i, 0] = dblPPMeanPos[i, 0];
                            local_HPT_hgt_focus_pos[i, 1] = dblPPMeanPos[i, 1];
                        }
                        else
                        {
                            local_HPT_hgt_focus_pos[i, 2] = 0;
                            local_HPT_hgt_focus_pos[i, 0] = 0;
                            local_HPT_hgt_focus_pos[i, 1] = 0;
                        }
                    }
                }

                if (local_available_plane_cnt < 4)
                {
                    // 아마 에러를 리턴하지 않을까 싶은데
                    pindif = -1;
                    return -1;
                }
                else
                {
                    if (gTilt_Check_Method == 0)
                    {
                        local_highestplane = local_HPT_hgt_focus_pos[0, 2];
                        for (int i = 1; i <= 3; i++)
                        {
                            if (local_HPT_hgt_focus_pos[i, 2] > local_highestplane)
                            {
                                local_highestplane = local_HPT_hgt_focus_pos[i, 2];
                            }
                        }

                        local_lowestplane = local_HPT_hgt_focus_pos[0, 2];
                        for (int i = 1; i <= 3; i++)
                        {
                            if (local_HPT_hgt_focus_pos[i, 2] < local_lowestplane)
                            {
                                local_lowestplane = local_HPT_hgt_focus_pos[i, 2];
                            }
                        }

                        //TODO 
                        //CheckPinPlanePlanarity_By_Pin_HPT = Abs(local_highestplane - local_lowestplane)
                        // 리턴값을 계산하여 넘겨주는 건가?
                        pindif = Math.Abs(local_highestplane - local_lowestplane);
                        return 0;
                    }
                    else
                    {
                        local_DiffY = (local_HPT_hgt_focus_pos[0, 2] + local_HPT_hgt_focus_pos[1, 2]) / 2 - (local_HPT_hgt_focus_pos[2, 2] + local_HPT_hgt_focus_pos[3, 2]) / 2;
                        local_DiffX = (local_HPT_hgt_focus_pos[0, 2] + local_HPT_hgt_focus_pos[3, 2]) / 2 - (local_HPT_hgt_focus_pos[1, 2] + local_HPT_hgt_focus_pos[2, 2]) / 2;

                        if (Math.Abs(local_DiffX) > Math.Abs(local_DiffY))
                        {
                            //CheckPinPlanePlanarity_By_Pin_HPT = Abs(local_DiffX)
                            //아마 리턴 값인것 같은데
                            pindif = Math.Abs(local_DiffX);
                            return 0;
                        }
                        else
                        {
                            //CheckPinPlanePlanarity_By_Pin_HPT = Abs(local_DiffY)
                            //아마 리턴 값인것 같은데
                            pindif = Math.Abs(local_DiffY);
                            return 0;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Get_hgt_each_axis_HPT()
        {
            try
            {
                //double radius = 150000;
                //   double rad_pos = 0.0;
                //  double tmp_angle = 0.0;
                //  double stk_ratio = 0.0;
                double axis1_stk_300;
                double axis2_stk_300;
                double axis3_stk_300;
                double axis1_stk_HPT;
                double axis2_stk_HPT;
                double axis3_stk_HPT;
                double ref_dist;
                double axis1_ratio;
                double axis2_ratio;
                double axis3_ratio;

                double[,] local_HPT_hgt_focus_pos = new double[4, 3];
                int gTilt_delt_Zval = 0;

                ref_dist = 531.26 * 1000;

                axis3_stk_300 = local_HPT_hgt_focus_pos[0, 2] - local_HPT_hgt_focus_pos[1, 2];
                axis1_stk_300 = local_HPT_hgt_focus_pos[0, 2] - local_HPT_hgt_focus_pos[2, 2];
                axis2_stk_300 = local_HPT_hgt_focus_pos[0, 2] - local_HPT_hgt_focus_pos[3, 2];

                axis3_ratio = Distance2D(local_HPT_hgt_focus_pos[2, 0], local_HPT_hgt_focus_pos[1, 1], 0, 0);
                axis1_ratio = Distance2D(local_HPT_hgt_focus_pos[2, 0], local_HPT_hgt_focus_pos[2, 1], 0, 0);
                axis2_ratio = Distance2D(local_HPT_hgt_focus_pos[2, 0], local_HPT_hgt_focus_pos[3, 1], 0, 0);

                axis1_stk_HPT = axis1_stk_300 * (ref_dist / axis1_ratio);
                axis2_stk_HPT = axis2_stk_300 * (ref_dist / axis2_ratio);
                axis3_stk_HPT = axis3_stk_300 * (ref_dist / axis3_ratio);

                if (axis1_stk_HPT > 0)
                {
                    if (axis3_stk_HPT > 0)
                    {
                        axis3_stk_HPT = axis3_stk_HPT - (axis1_stk_HPT / 3);
                    }
                    else
                    {
                        axis3_stk_HPT = axis3_stk_HPT + (axis1_stk_HPT / 3);
                    }
                    if (axis2_stk_HPT > 0)
                    {
                        axis2_stk_HPT = axis2_stk_HPT - (axis1_stk_HPT / 3);
                    }
                    else
                    {
                        axis2_stk_HPT = axis2_stk_HPT + (axis1_stk_HPT / 3);

                    }
                }
                else
                {
                    if (axis3_stk_HPT > 0)
                    {
                        axis3_stk_HPT = axis3_stk_HPT + (axis1_stk_HPT / 3);
                    }
                    else
                    {
                        axis3_stk_HPT = axis3_stk_HPT - (axis1_stk_HPT / 3);
                    }
                    if (axis2_stk_HPT > 0)
                    {
                        axis2_stk_HPT = axis2_stk_HPT + (axis1_stk_HPT / 3);
                    }
                    else
                    {
                        axis2_stk_HPT = axis2_stk_HPT - (axis1_stk_HPT / 3);
                    }
                }

                gTilt_delt_Zval = Convert.ToInt32(axis1_stk_HPT / 2 + axis2_stk_HPT / 2 + axis3_stk_HPT / 2);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Get_hgt_each_axis_HPT2(double[,] hpthgtfocuspos)
        {
            try
            {
                double[,] local_HPT_hgt_focus_pos = new double[4, 3]; //zbuffer

                local_HPT_hgt_focus_pos = hpthgtfocuspos;
                double[] gHPT_ADJ_Height = new double[3];

                double axis1_ratio;
                double axis2_ratio;
                double axis3_ratio;

                double[] tmphgt = new double[4];
                double[] tmprelpos = new double[4];
                double[] tmpXdir_planar = new double[2];
                double[] tmpYdir_planar = new double[2];
                double[] tmpSTKAXIS = new double[3];

                double tmp_plate_ang1;
                double tmp_plate_ang2;
                double tmp_plate_ang3;
                double tmp_stk_byAxis1;
                double tmp_stk_byAxis2;
                double tmp_delt_x = 0;
                double tmp_delt_y = 0;
                //double tmp_delt_z = 0;
                double ref_dist;


                int gTilt_delt_Zval;
                double[] gTilt_delt_Zval_Ref = new double[3];

                ref_dist = 531.26 * 1000;

                axis3_ratio = Distance2D(local_HPT_hgt_focus_pos[2, 0], local_HPT_hgt_focus_pos[1, 1], 0, 0);
                axis1_ratio = Distance2D(local_HPT_hgt_focus_pos[2, 0], local_HPT_hgt_focus_pos[2, 1], 0, 0);
                axis2_ratio = Distance2D(local_HPT_hgt_focus_pos[2, 0], local_HPT_hgt_focus_pos[3, 1], 0, 0);

                tmprelpos[0] = 0;
                tmprelpos[1] = local_HPT_hgt_focus_pos[1, 2] - local_HPT_hgt_focus_pos[0, 2];
                tmprelpos[2] = local_HPT_hgt_focus_pos[2, 2] - local_HPT_hgt_focus_pos[0, 2];
                tmprelpos[3] = local_HPT_hgt_focus_pos[3, 2] - local_HPT_hgt_focus_pos[0, 2];

                tmpYdir_planar[0] = (tmphgt[0] + tmphgt[1]) / 2; // back
                tmpYdir_planar[1] = (tmphgt[2] + tmphgt[3]) / 2; // front

                tmpXdir_planar[0] = (tmphgt[0] + tmphgt[3]) / 2; //right
                tmpXdir_planar[1] = (tmphgt[1] + tmphgt[2]) / 2; //left

                tmpSTKAXIS[0] = tmprelpos[2] * -1 * (ref_dist / axis1_ratio); // TOPPLATE_Pillar 

                //TODO Rad,deg,distance함수들 Cint 함수들 확인 해야함 
                tmp_plate_ang1 = deg(Math.Atan2(Math.Abs(tmpSTKAXIS[0]), (TOPPLATE_Diag * 1000)));
                tmp_stk_byAxis1 = Math.Sin(rad(tmp_plate_ang1)) * TOPPLATE_Diag / 2 * 1000;

                if (tmpSTKAXIS[0] >= 0)
                {
                    if (tmprelpos[3] > 0)
                    {
                        tmpSTKAXIS[1] = (tmprelpos[3] * (ref_dist / axis2_ratio) + tmp_stk_byAxis1) * -1;
                    }
                    else
                    {
                        tmpSTKAXIS[1] = Math.Abs(tmprelpos[3] * (ref_dist / axis2_ratio)) - tmp_stk_byAxis1;
                    }

                    tmp_plate_ang2 = deg(Math.Atan2(Math.Abs(tmpSTKAXIS[1]), TOPPLATE_Diag * 1000));
                    tmp_stk_byAxis2 = Math.Sin(rad(tmp_plate_ang2)) * TOPPLATE_Diag / 2 * 1000;

                    if (tmprelpos[1] > 0)
                    {
                        if (tmpSTKAXIS[1] > 1)
                        {
                            tmpSTKAXIS[2] = (tmprelpos[1] * (ref_dist / axis3_ratio) + tmp_stk_byAxis1 + tmp_stk_byAxis2) * -1;
                        }
                        else
                        {
                            tmpSTKAXIS[2] = (tmprelpos[1] * (ref_dist / axis3_ratio) + tmp_stk_byAxis1 - tmp_stk_byAxis2) * -1;
                        }
                    }
                    else
                    {
                        if (tmpSTKAXIS[1] > 0)
                        {
                            tmpSTKAXIS[2] = Math.Abs(tmprelpos[1] * (ref_dist / axis3_ratio)) - tmp_stk_byAxis1 - tmp_stk_byAxis2;
                        }
                        else
                        {
                            tmpSTKAXIS[2] = Math.Abs(tmprelpos[1] * (ref_dist / axis3_ratio)) - tmp_stk_byAxis1 + tmp_stk_byAxis2;
                        }
                    }
                }
                else
                {
                    if (tmprelpos[3] > 0)
                    {
                        tmpSTKAXIS[1] = (tmprelpos[3] * (ref_dist / axis2_ratio) - tmp_stk_byAxis1) * -1;
                    }
                    else
                    {
                        tmpSTKAXIS[1] = Math.Abs(tmprelpos[3] * (ref_dist / axis2_ratio)) + tmp_stk_byAxis1;
                    }

                    tmp_plate_ang2 = deg(Math.Atan2(Math.Abs(tmpSTKAXIS[1]), TOPPLATE_Diag * 1000));
                    tmp_stk_byAxis2 = Math.Sin(rad(tmp_plate_ang2)) * TOPPLATE_Diag / 2 * 1000;

                    if (tmprelpos[1] > 0)
                    {
                        if (tmpSTKAXIS[1] > 0)
                        {
                            tmpSTKAXIS[2] = (tmprelpos[1] * (ref_dist / axis3_ratio) - tmp_stk_byAxis1 + tmp_stk_byAxis2) * -1;
                        }
                        else
                        {
                            tmpSTKAXIS[2] = (tmprelpos[1] * (ref_dist / axis3_ratio) - tmp_stk_byAxis1 - tmp_stk_byAxis2) * -1;
                        }
                    }
                    else
                    {
                        if (tmpSTKAXIS[1] > 0)
                        {
                            tmpSTKAXIS[2] = Math.Abs(tmprelpos[1] * (ref_dist / axis3_ratio) + tmp_stk_byAxis1 - tmp_stk_byAxis2);
                        }
                        else
                        {
                            tmpSTKAXIS[2] = Math.Abs(tmprelpos[1] * (ref_dist / axis3_ratio)) + tmp_stk_byAxis1 + tmp_stk_byAxis2;
                        }
                    }
                }

                tmp_plate_ang3 = deg(Math.Atan2(Math.Abs(tmpSTKAXIS[2]), TOPPLATE_Diag * 1000));
                //tmp_delt_z = 0;

                gHPT_ADJ_Height[0] = tmpSTKAXIS[0]; // motor가 움직여야 되는 값  51 0
                gHPT_ADJ_Height[1] = tmpSTKAXIS[1];
                gHPT_ADJ_Height[2] = tmpSTKAXIS[2];

                TZ1Offset = gHPT_ADJ_Height[0];
                TZ2Offset = gHPT_ADJ_Height[1];
                TZ3Offset = gHPT_ADJ_Height[2];

                gTilt_delt_Zval = Convert.ToInt32(tmpSTKAXIS[0] / 2 + tmpSTKAXIS[1] / 2 + tmpSTKAXIS[2] / 2);

                gTilt_delt_Zval_Ref[0] = tmpSTKAXIS[0];
                gTilt_delt_Zval_Ref[1] = tmpSTKAXIS[1];
                gTilt_delt_Zval_Ref[2] = tmpSTKAXIS[2];

                tmp_delt_x = Math.Abs(tmpSTKAXIS[2]) * Math.Sin(rad(Math.Atan2(Math.Abs(tmpSTKAXIS[2]), TOPPLATE_width * 1000)));
                tmp_delt_y = Math.Abs(tmpSTKAXIS[1]) * Math.Sin(rad(Math.Atan2(Math.Abs(tmpSTKAXIS[1]), TOPPLATE_hgt * 1000)));

                if (tmpSTKAXIS[2] < 0)
                {
                    tmp_delt_x = tmp_delt_x * 1;
                }
                else
                {
                    tmp_delt_x = tmp_delt_x * -1;
                }

                if (tmpSTKAXIS[1] < 0)
                {
                    tmp_delt_y = tmp_delt_y * 1;
                }
                else
                {
                    tmp_delt_y = tmp_delt_y * -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int GetPinPlaneCalVal_By_Pin_HPT()
        {
            UpdatePinInfo();
            int ret = -1;
            try
            {
                double[] dblPPX = new double[4];
                double[] dblPPY = new double[4];
                double[] dblPPZ = new double[4];

                double[] dblPPMinDist = new double[4];
                int[] dblPPRepPinNum = new int[4];

                int[] intPlPtsNum = new int[4];

                double[,] dblCardMaxSize = new double[4, 2];
                double[,] dblPPMeanPos = new double[4, 3];
                //double hpt_tilt_angle;
                //double hpt_tilt_hgt;

                double[] dbPlaneLowestPinZ = new double[4];
                double[] dbPlaneHighestPinZ = new double[4];

                double[,] dbPlaneXpos = new double[2, 4];
                double[,] dbPlaneYpos = new double[2, 4];

                int pinNum = AlignPinData.Count;
                int pinAdjustEnable = ATDeviceFile.PinAdjustEnable.Value;

                double[] NewPinPosX = new double[pinNum];
                double[] NewPinPosY = new double[pinNum];
                double[] NewPinPosZ = new double[pinNum];

                inputPinData(ref NewPinPosX, ref NewPinPosY, ref NewPinPosZ);

                double gTilt_SKIP_AREA_RatioX = ATDeviceFile.TiltSkipAreaRatioX.Value;
                double gTilt_SKIP_AREA_RatioY = ATDeviceFile.TiltSkipAreaRatioY.Value;

                const int PP_NUM0 = 0;
                const int PP_NUM1 = 1;
                const int PP_NUM2 = 2;
                const int PP_NUM3 = 3;

                double total_avg = 0.0;
                int gTilt_intPlaneMinMaxtol = ATDeviceFile.TiltIntPlaneMinMaxtol.Value;
                double[,] HPT_hgt_foucs_pos = new double[4, 3];
                bool gHPT_Verifying_done = false;
                bool gHPT_Adjust_done = false;
                double[,] HPT_hgt_foucs_pos_adj = new double[4, 1];
                if (pinNum < 4)
                {
                    //hpt_tilt_hgt = 0;
                    //hpt_tilt_angle = 0;
                    return -1;
                }

                for (int i = 0; i < 4; i++)
                {
                    intPlPtsNum[i] = 0;
                    dblPPX[i] = 0;
                    dblPPY[i] = 0;
                    dblPPZ[i] = 0;
                }

                for (int i = 0; i < pinNum; i++)
                {
                    if (pinAdjustEnable > 0 && !(AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_PASSED ||
                        AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_OVER_TOLERANCE))
                    {
                        //nextpinsearch
                    }
                    else
                    {
                        if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[0] = intPlPtsNum[0] + 1;
                            dblPPMeanPos[PP_NUM0, 0] = dblPPMeanPos[PP_NUM0, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM0, 1] = dblPPMeanPos[PP_NUM0, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM0, 2] = dblPPMeanPos[PP_NUM0, 2] + NewPinPosZ[i];
                            if (NewPinPosX[i] > dblCardMaxSize[PP_NUM0, 0])
                            {
                                dblCardMaxSize[PP_NUM0, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] > dblCardMaxSize[PP_NUM0, 1])
                            {
                                dblCardMaxSize[PP_NUM0, 1] = NewPinPosY[i];
                            }
                        }
                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[1] = intPlPtsNum[1] + 1;
                            dblPPMeanPos[PP_NUM1, 0] = dblPPMeanPos[PP_NUM1, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM1, 1] = dblPPMeanPos[PP_NUM1, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM1, 2] = dblPPMeanPos[PP_NUM1, 2] + NewPinPosZ[i];
                            if (NewPinPosX[i] < dblCardMaxSize[PP_NUM1, 0])
                            {
                                dblCardMaxSize[PP_NUM1, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] > dblCardMaxSize[PP_NUM1, 1])
                            {
                                dblCardMaxSize[PP_NUM1, 1] = NewPinPosY[i];
                            }
                        }
                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[2] = intPlPtsNum[2] + 1;
                            dblPPMeanPos[PP_NUM2, 0] = dblPPMeanPos[PP_NUM2, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM2, 1] = dblPPMeanPos[PP_NUM2, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM2, 2] = dblPPMeanPos[PP_NUM2, 2] + NewPinPosZ[i];

                            if (NewPinPosX[i] < dblCardMaxSize[PP_NUM2, 0])
                            {
                                dblCardMaxSize[PP_NUM2, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] < dblCardMaxSize[PP_NUM2, 1])
                            {
                                dblCardMaxSize[PP_NUM2, 1] = NewPinPosY[i];
                            }
                        }
                        else if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            intPlPtsNum[3] = intPlPtsNum[3] + 1;
                            dblPPMeanPos[PP_NUM3, 0] = dblPPMeanPos[PP_NUM3, 0] + NewPinPosX[i];
                            dblPPMeanPos[PP_NUM3, 1] = dblPPMeanPos[PP_NUM3, 1] + NewPinPosY[i];
                            dblPPMeanPos[PP_NUM3, 2] = dblPPMeanPos[PP_NUM3, 2] + NewPinPosZ[i];

                            if (NewPinPosX[i] > dblCardMaxSize[PP_NUM3, 0])
                            {
                                dblCardMaxSize[PP_NUM3, 0] = NewPinPosX[i];
                            }
                            if (NewPinPosY[i] < dblCardMaxSize[PP_NUM3, 1])
                            {
                                dblCardMaxSize[PP_NUM3, 1] = NewPinPosY[i];
                            }
                        }
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    dblPPMeanPos[i, 0] = dblPPMeanPos[i, 0] / intPlPtsNum[i];
                    dblPPMeanPos[i, 1] = dblPPMeanPos[i, 1] / intPlPtsNum[i];
                    dblPPMeanPos[i, 2] = dblPPMeanPos[i, 2] / intPlPtsNum[i];
                }

                for (int i = 0; i < 4; i++)
                {
                    dblPPX[i] = 0;
                    dblPPY[i] = 0;
                    dblPPZ[i] = 0;
                }

                for (int i = 0; i < pinNum - 1; i++)
                {
                    if (pinAdjustEnable > 0 && !(AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_PASSED ||
                        AlignPinData[i].Result.Value == PINALIGNRESULT.PIN_OVER_TOLERANCE))
                    {
                        //nextpinsearch
                    }
                    else
                    {
                        if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[0, 2])
                            {
                                if (dbPlaneLowestPinZ[0] != 0)
                                {
                                    if (dbPlaneLowestPinZ[0] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[0] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 0] = NewPinPosX[i];
                                        dbPlaneYpos[0, 0] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[0] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 0] = NewPinPosX[i];
                                    dbPlaneYpos[0, 0] = NewPinPosY[i];
                                }
                            }

                            if (NewPinPosZ[i] > dblPPMeanPos[0, 2])
                            {
                                if (dbPlaneHighestPinZ[0] != 0)
                                {
                                    if (dbPlaneHighestPinZ[0] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[0] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 0] = NewPinPosX[i];
                                        dbPlaneYpos[1, 0] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[0] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 0] = NewPinPosX[i];
                                    dbPlaneYpos[1, 0] = NewPinPosY[i];
                                }
                            }

                        }

                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] >= (13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[1, 2])
                            {
                                if (dbPlaneLowestPinZ[1] != 0)
                                {
                                    if (dbPlaneLowestPinZ[1] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[1] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 1] = NewPinPosX[i];
                                        dbPlaneYpos[0, 1] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[1] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 1] = NewPinPosX[i];
                                    dbPlaneYpos[0, 1] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[1, 2])
                            {
                                if (dbPlaneHighestPinZ[1] != 0)
                                {
                                    if (dbPlaneHighestPinZ[1] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[1] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 1] = NewPinPosX[i];
                                        dbPlaneYpos[1, 1] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[1] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 1] = NewPinPosX[i];
                                    dbPlaneYpos[1, 1] = NewPinPosY[i];
                                }
                            }
                        }

                        else if (NewPinPosX[i] < (-13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[2, 2])
                            {
                                if (dbPlaneLowestPinZ[2] != 0)
                                {
                                    if (dbPlaneLowestPinZ[2] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[2] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 2] = NewPinPosX[i];
                                        dbPlaneYpos[0, 2] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[2] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 2] = NewPinPosX[i];
                                    dbPlaneYpos[0, 2] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[2, 2])
                            {
                                if (dbPlaneHighestPinZ[2] != 0)
                                {
                                    if (dbPlaneHighestPinZ[2] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[2] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 2] = NewPinPosX[i];
                                        dbPlaneYpos[1, 2] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[2] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 2] = NewPinPosX[i];
                                    dbPlaneYpos[1, 2] = NewPinPosY[i];
                                }
                            }
                        }
                        else if (NewPinPosX[i] >= (13000 * gTilt_SKIP_AREA_RatioX) &&
                            NewPinPosY[i] < (-13000 * gTilt_SKIP_AREA_RatioY))
                        {
                            if (NewPinPosZ[i] < dblPPMeanPos[3, 2])
                            {
                                if (dbPlaneLowestPinZ[3] != 0)
                                {
                                    if (dbPlaneLowestPinZ[3] > NewPinPosZ[i])
                                    {
                                        dbPlaneLowestPinZ[3] = NewPinPosZ[i];
                                        dbPlaneXpos[0, 3] = NewPinPosX[i];
                                        dbPlaneYpos[0, 3] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneLowestPinZ[3] = NewPinPosZ[i];
                                    dbPlaneXpos[0, 3] = NewPinPosX[i];
                                    dbPlaneYpos[0, 3] = NewPinPosY[i];
                                }
                            }
                            if (NewPinPosZ[i] > dblPPMeanPos[3, 2])
                            {
                                if (dbPlaneHighestPinZ[3] != 0)
                                {
                                    if (dbPlaneHighestPinZ[3] < NewPinPosZ[i])
                                    {
                                        dbPlaneHighestPinZ[3] = NewPinPosZ[i];
                                        dbPlaneXpos[1, 3] = NewPinPosX[i];
                                        dbPlaneYpos[1, 3] = NewPinPosY[i];
                                    }
                                }
                                else
                                {
                                    dbPlaneHighestPinZ[3] = NewPinPosZ[i];
                                    dbPlaneXpos[1, 3] = NewPinPosX[i];
                                    dbPlaneYpos[1, 3] = NewPinPosY[i];
                                }
                            }
                        }
                    }
                }

                total_avg = (dblPPMeanPos[0, 2] + dblPPMeanPos[1, 2] + dblPPMeanPos[2, 2] + dblPPMeanPos[3, 2]) / 4;

                for (int i = 0; i < 4; i++)
                {
                    dblPPZ[i] = dblPPMeanPos[i, 2];
                    dblPPX[i] = dblPPMeanPos[i, 0];
                    dblPPY[i] = dblPPMeanPos[i, 1];

                    if (intPlPtsNum[i] > 1 &&
                        i != 0 &&
                        Math.Abs(dbPlaneHighestPinZ[i] - total_avg) > Math.Abs(dbPlaneLowestPinZ[i] = total_avg))
                    {
                        if ((dbPlaneHighestPinZ[i] != 0) &&
                            (gTilt_intPlaneMinMaxtol <= Math.Abs(dbPlaneHighestPinZ[i] - total_avg)) &&
                            intPlPtsNum[i] > 1)
                        {
                            HPT_hgt_foucs_pos[i, 2] = (dblPPMeanPos[i, 2] * intPlPtsNum[i] - dbPlaneHighestPinZ[i]) / (intPlPtsNum[i] - 1);
                            HPT_hgt_foucs_pos[i, 0] = (dblPPMeanPos[i, 0] * intPlPtsNum[i] - dbPlaneXpos[1, i]) / (intPlPtsNum[i] - 1);
                            HPT_hgt_foucs_pos[i, 1] = (dblPPMeanPos[i, 1] * intPlPtsNum[i] - dbPlaneYpos[1, i]) / (intPlPtsNum[i] - 1);
                        }
                        else
                        {
                            HPT_hgt_foucs_pos[i, 2] = dblPPMeanPos[i, 2];
                            HPT_hgt_foucs_pos[i, 0] = dblPPMeanPos[i, 0];
                            HPT_hgt_foucs_pos[i, 1] = dblPPMeanPos[i, 1];
                        }
                    }
                    else if (intPlPtsNum[i] > 1 &&
                        i != 0 &&
                        Math.Abs(dbPlaneHighestPinZ[i] - total_avg) < Math.Abs(dbPlaneLowestPinZ[i] - total_avg))
                    {
                        if (dbPlaneLowestPinZ[i] != 0 &&
                            (gTilt_intPlaneMinMaxtol <= Math.Abs(dbPlaneLowestPinZ[i] - total_avg)) &&
                            intPlPtsNum[i] > 1)
                        {
                            HPT_hgt_foucs_pos[i, 2] = (dblPPMeanPos[i, 2] * intPlPtsNum[i] - dbPlaneLowestPinZ[i]) / (intPlPtsNum[i] - 1);
                            HPT_hgt_foucs_pos[i, 0] = (dblPPMeanPos[i, 0] * intPlPtsNum[i] - dbPlaneXpos[0, i]) / (intPlPtsNum[i] - 1);
                            HPT_hgt_foucs_pos[i, 1] = (dblPPMeanPos[i, 1] * intPlPtsNum[i] - dbPlaneYpos[0, i]) / (intPlPtsNum[i] - 1);
                        }
                        else
                        {
                            HPT_hgt_foucs_pos[i, 2] = dblPPMeanPos[i, 2];
                            HPT_hgt_foucs_pos[i, 0] = dblPPMeanPos[i, 0];
                            HPT_hgt_foucs_pos[i, 1] = dblPPMeanPos[i, 1];
                        }
                    }
                    else
                    {
                        HPT_hgt_foucs_pos[i, 2] = dblPPMeanPos[i, 2];
                        HPT_hgt_foucs_pos[i, 0] = dblPPMeanPos[i, 0];
                        HPT_hgt_foucs_pos[i, 1] = dblPPMeanPos[i, 1];
                    }

                    if (gHPT_Verifying_done == true || gHPT_Adjust_done == true)
                    {
                        HPT_hgt_foucs_pos[i, 0] = dblPPZ[i];
                    }
                }

                Get_hgt_each_axis_HPT2(HPT_hgt_foucs_pos);
                ret = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }
        private int UpdatePinInfo()
        {
            int ret = -1;
            try
            {
                if (AlignPinData == null)
                {
                    AlignPinData = new List<IPinData>();
                }

                AlignPinData.Clear();

                foreach (var dutlist in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    foreach (var pin in dutlist.PinList)
                    {
                        if (pin.Result.Value == PINALIGNRESULT.PIN_PASSED)
                        {
                            AlignPinData.Add(pin);
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        private double Distance2D(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }
        private double deg(double val)
        {
            return val * COEF_TO_DEG;
        }
        private double rad(double val)
        {
            return val * COEF_TO_RAD;
        }


        public EventCodeEnum SaveSysFileRefPos()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam param = null;
                RetVal = this.LoadParameter(ref param, typeof(AutoTiltSystemFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ATSysFile = param as AutoTiltSystemFile;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SaveSysFileLastPos()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ATSysFile == null)
                {
                    ATSysFile = new AutoTiltSystemFile();
                }
                //object deserializedObj;

                double tz1lastpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.TZ1, ref tz1lastpos);
                ATSysFile.TZ1LastPos.Value = tz1lastpos;

                double tz2lastpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.TZ2, ref tz2lastpos);
                ATSysFile.TZ2LastPos.Value = tz2lastpos;

                double tz3lastpos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.TZ3, ref tz3lastpos);
                ATSysFile.TZ3LastPos.Value = tz3lastpos;

                //double tz1ref = ATSysFile.TZ1RefPos;
                //double tz2ref = ATSysFile.TZ2RefPos;
                //double tz3ref = ATSysFile.TZ3RefPos;

                RetVal = this.SaveParameter(ATSysFile);
                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[AutoTilt] SaveSysFile(): Serialize Error");
                    return RetVal;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new AutoTiltDeviceFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(AutoTiltDeviceFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ATDeviceFile = tmpParam as AutoTiltDeviceFile;
                }

                //DevParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = SaveATDeviceFile();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum LoadAutoTiltDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            ATDeviceFile = new AutoTiltDeviceFile();

            string fullPath = this.FileManager().GetDeviceParamFullPath(ATDeviceFile.FilePath, ATDeviceFile.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(AutoTiltDeviceFile), null, fullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    ATDeviceFile = tmpParam as AutoTiltDeviceFile;
                }
            }
            catch (Exception err)
            {

                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AutoTilt] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveATDeviceFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            string fullPath = this.FileManager().GetDeviceParamFullPath(ATDeviceFile.FilePath, ATDeviceFile.FileName);

            try
            {
                RetVal = Extensions_IParam.SaveParameter(null, ATDeviceFile, null, fullPath);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveATDeviceFile]Faile SaveParameter");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                //LoggerManager.Error($String.Format("[AutoTiltModule] SaveATDeviceFile(): Serialize Error. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return RetVal;
        }


        public EventCodeEnum LoadAutoTiltSysFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            ATSysFile = new AutoTiltSystemFile();

            string FullPath = this.FileManager().GetSystemParamFullPath(ATSysFile.FilePath, ATSysFile.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(AutoTiltSystemFile), null, FullPath);
                if (RetVal == EventCodeEnum.NONE)
                {
                    ATSysFile = tmpParam as AutoTiltSystemFile;
                }
            }
            catch (Exception err)
            {

                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AutoTilt] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum SaveAutoTiltSysFile()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            string FullPath = this.FileManager().GetSystemParamFullPath(ATSysFile.FilePath, ATSysFile.FileName);

            try
            {
                RetVal = Extensions_IParam.SaveParameter(null, ATSysFile, null, FullPath);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[{this.GetType().Name} - SaveAutoTiltSysFile]Faile SaveParameter");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[AutoTilt] SaveSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new AutoTiltSystemFile();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(AutoTiltSystemFile));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ATSysFile = tmpParam as AutoTiltSystemFile;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = SaveAutoTiltSysFile();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
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
                throw;
            }
            return retVal;
        }

        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }
        public bool IsLotReady(out string msg) //Lot 시작시 조건 체크
        {
            msg = "";
            return true;
        }
        
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }
    }
}
