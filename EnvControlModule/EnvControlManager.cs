using System;
using System.Linq;

namespace EnvControlModule
{
    using Autofac;
    using ControlModules;
    using EnvControlModule.Parameter;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.Chiller;
    using ProberInterfaces.Temperature.DewPoint;
    using ProberInterfaces.Temperature.DryAir;
    using System.Collections.ObjectModel;
    using ProberInterfaces.EnvControl.Parameter;

    public class EnvControlManager : IEnvControlManager
    {
        #region .. Property
        public bool Initialized { get; set; } = false;
        public InitPriorityEnum InitPriority { get; }

        public IParam EnvSysParam
        {
            get { return EnvControlParam; }
            set { EnvControlParam = (EnvControlParameter)value; }
        }

        /// <summary>
        /// Chiller 통신 및 스테이지 연결 위한 파라미터
        /// </summary>
        private EnvControlParameter _EnvControlParam;
        public EnvControlParameter EnvControlParam
        {
            get { return _EnvControlParam; }
            set { _EnvControlParam = value; }
        }

        private IEnvController _EnvControlCore;

        public IEnvController EnvControlCore
        {
            get { return _EnvControlCore; }
            set { _EnvControlCore = value; }
        }


        #region .. Control Modules
        private IFFUManager _FFUManager;
        public IFFUManager FFUManager
        {
            get { return _FFUManager; }
            set { _FFUManager = value; }
        }

        /// <summary>
        /// EnvControlParam 정보를 통해 만들어진 실제 Chiller, Stage 와 통신하기 위한 모듈 리스트
        /// </summary>
        private IChillerManager _ChillerManager;
        public IChillerManager ChillerManager
        {
            get { return _ChillerManager; }
            set { _ChillerManager = value; }
        }

        /// <summary>
        /// DryAirControl Module
        /// </summary>
        private IDryAirManager _DryAirManager;

        public IDryAirManager DryAirManager
        {
            get { return _DryAirManager; }
            set { _DryAirManager = value; }
        }

        /// <summary>
        /// DewPointControl Module
        /// </summary>
        private IDewPointManager _DewPointManager;
        public IDewPointManager DewPointManager
        {
            get { return _DewPointManager; }
            set { _DewPointManager = value; }
        }

        /// <summary>
        /// ValveControl Module
        /// </summary>
        private IValveManager _ValveManager;

        public IValveManager ValveManager
        {
            get { return _ValveManager; }
            set { _ValveManager = value; }
        }


        #endregion


        #endregion

        #region  < Load & Save Parameter >
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new EnvControlParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(EnvControlParameter));
                EnvControlParam = (EnvControlParameter)tmpParam;
                //SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(EnvControlParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }
        #endregion

        #region .. IModule Method
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {

                    if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL)
                    {
                        EnvControlCore = new EnvController();
                    }
                    else if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.REMOTE)
                    {
                        EnvControlCore = new EnvRemoteController();
                    }
                    else if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.EMUL)
                    {
                        EnvControlCore = new EnvControllerEmul();
                    }

                    if (EnvControlCore != null)
                    {
                        retVal = EnvControlCore.InitModule();
                    }

                    //LoadSysParameter();

                    FFUManager = new FFUManager(EnvControlParam.FFUSysParam);
                    ChillerManager = new ChillerManager(EnvControlParam.ChillerSysParam);
                    DryAirManager = new DryAirManager(EnvControlParam.DryAirSysParam);
                    DewPointManager = new DewPointManager();
                    ValveManager = new ValveManager(EnvControlParam.ValveSysParam);

                    retVal = InitHelpFunc(FFUManager);
                    retVal = InitHelpFunc(ChillerManager);
                    retVal = InitHelpFunc(ValveManager);
                    retVal = InitHelpFunc(DewPointManager);
                    retVal = InitHelpFunc(DryAirManager);

                    Initialized = true;
                }
                else
                {
                    //if (EnvControlCore != null)
                    //{
                    //    retVal = EnvControlCore.InitModule();
                    //}
                    //retVal = InitHelpFunc(ChillerManager);
                    //retVal = InitHelpFunc(ValveManager);
                    //retVal = InitHelpFunc(DewPointManager);
                    //retVal = InitHelpFunc(DryAirManager);
                }

                EventCodeEnum InitHelpFunc(IModule tempModule)
                {
                    EventCodeEnum isSucess = EventCodeEnum.UNDEFINED;

                    if (tempModule != null)
                    {
                        if (tempModule is IHasSysParameterizable)
                        {
                            isSucess = (tempModule as IHasSysParameterizable)?.LoadSysParameter() ?? EventCodeEnum.UNDEFINED;
                            if (isSucess != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"{tempModule.GetType().Name}.LoadSysParameter() Failed");
                            }
                        }

                        isSucess = tempModule.InitModule();

                        if (isSucess != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"{tempModule.GetType().Name}.InitModule() Failed");
                        }
                    }

                    return isSucess;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EnvControlCore?.InitConnect() ?? EventCodeEnum.UNDEFINED;
                retVal = ChillerManager?.InitConnect() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DisConnect(int chuckID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ChillerManager?.Disconnect(chuckID) ?? EventCodeEnum.UNDEFINED;
                retVal = EnvControlCore?.DisConnect(chuckID) ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InitModule(IContainer container)
        {
            //return InitModule();
            return EventCodeEnum.NONE;
        }

        public void DeInitModule()
        {
            try
            {
                FFUManager.DeInitModule();
                ChillerManager.DeInitModule();
                DryAirManager.DeInitModule();
                DewPointManager.DeInitModule();
                ValveManager.DeInitModule();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        

        #region Dispose
        ~EnvControlManager()
        {
            this.Dispose(false);
        }

        private bool disposed;
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            if (disposing)
            {
                // IDisposable 인터페이스를 구현하는 멤버들을 여기서 정리합니다.
            }
            // .NET Framework에 의하여 관리되지 않는 외부 리소스들을 여기서 정리합니다.
            this.disposed = true;
        }
        #endregion

        #region .. Chiller

        #endregion

        #region ..DewPoint
        public double GetDewPoint(int stageindex)
        {
            return DewPointManager.GetDewPoint(stageindex);
        }
        #endregion

        #region //.. GetModule

        public IChillerModule GetChillerModule()
        {
            IChillerModule module = null;

            try
            {
                module = ChillerManager?.GetChillerModule() ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return module;
        }

        public IDryAirModule GetDryAirModule()
        {
            IDryAirModule module = null;
            try
            {
                module = DryAirManager?.DryAirModule ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return module;
        }

        public IDewPointModule GetDewPointModule()
        {
            IDewPointModule module = null;
            try
            {
                module = DewPointManager?.DewPointModule ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return module;
        }
        #endregion

        public void RaiseFFUAlarm(string alarmmessage)
        {
            EnvControlCore.RaiseFFUAlarm(alarmmessage);
        }
        public EnumEnvControlModuleMode GetEnvcontrolMode()
        {
            return EnvControlParam.EnvControlMode;
        }

        /// <summary>
        /// GP 의 경우 같은 층의 다른 Stage 들의 상태를 확인하여 Chiller 를 사용할수 있을 지 없을지 확인..
        /// </summary>
        /// <param name="stageindex"></param>
        /// <returns></returns>

        public bool IsUsingDryAir(int stageindex = -1)
        {
            return EnvControlCore.IsUsingDryAir(stageindex);
        }

        public bool IsUsingChiller(int stageindex = -1)
        {
            return EnvControlCore.IsUsingChiller(stageindex);
            //if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL)
            //    return EnvControlCore.IsUsingChiller(stageindex);
            //else if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.REMOTE)
            //{
            //    return EnvControlCore.IsUsingChiller(stageindex);
            //}
            //else 
            //{
            //    return EnvControlCore.IsUsingChiller(stageindex);
            //}
            //return false;
        }
        public EventCodeEnum IsLotRunReady(int foupNumber)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Loader 에서 Lot 시작할 때, Foup 에 assign 된 Stage 의 
                //Chiller - DewPoint - DryAir - Valve : Cooler 동작 확인

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public IEnvControlServiceCallback GetEnvControlClient(int stageindex = -1)
        {
            IEnvControlServiceCallback client = null;
            try
            {
                client = EnvControlCore.GetEnvControlClient(stageindex);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return client;
        }

        private object valveLockObject = new object();
        public EventCodeEnum SetValveState(bool enableFlag, EnumValveType valveType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL||
                    EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.EMUL)
                {
                    lock (valveLockObject)
                    {
                        if (valveType == EnumValveType.IN | valveType == EnumValveType.OUT)
                        {
                            if (this.EnvControlManager().GetValveParam().ValveModuleType.Value == EnumValveModuleType.NA
                                 && (valveType == EnumValveType.IN || valveType == EnumValveType.OUT))
                            {
                                /// ++ Valve 없는 타입에서 Valve 닫는 시점에 내부순환모드로 변경되도록 수정. 
                                var chillerModule = ChillerManager.GetChillerModule(stageIndex);
                                ChillerManager.SetCircuationActive(enableFlag, stageIndex);
                                LoggerManager.Debug($"[EnvControl - SetValve(NA)] Chiller Set Circuation Action is {enableFlag}.");
                            }
                            else
                            {
                                bool existActiveStage = false;
                                foreach (var chillerparam in EnvControlParam.ChillerSysParam.ChillerParams)
                                {
                                    int retstage = chillerparam.StageIndexs.ToList<int>().Find(stageidx => stageidx == stageIndex);
                                    if (retstage != 0)
                                    {
                                        foreach (var stageidx in chillerparam.StageIndexs)
                                        {
                                            if (stageidx != stageIndex)
                                            {
                                                bool valveInState = ValveManager.GetValveState(EnumValveType.IN, stageidx);
                                                bool valveOutState = ValveManager.GetValveState(EnumValveType.OUT, stageidx);
                                                if ((valveInState && valveOutState) == false)
                                                {
                                                    existActiveStage = false;
                                                }
                                                else
                                                {
                                                    existActiveStage = true;
                                                    break;
                                                }
                                            }

                                        }
                                        break;
                                    }
                                }


                                if (existActiveStage == false)
                                {
                                    /// 솔라딘 새로운 칠러는 벨브가 닫히면 자동으로 내부순환하므로 모드 전환이 필요 없음.
                                    /// YMTC 의 솔라딘1호기는 6월에 교체예정 그전까지 Master에는 해당 코드가 있어야한다.

                                    if (EnvControlParam.ValveSysParam.ValveModuleType.Value == EnumValveModuleType.LOADER)
                                    {
                                        var chillerModule = ChillerManager.GetChillerModule(stageIndex);
                                        ChillerManager.SetCircuationActive(false, stageIndex);
                                        LoggerManager.Debug("[EnvControl - SetValve] Chiller Set Circuation Action is false.");
                                    }

                                }
                            }
                        }
                        else
                        {
                            // TODO : V19
                            // Coolant Valve 가 열려있을때는 Purge, Drain 명령 내리면 안된다.
                            // 수동 (Manual Purge )동작 할 경우, Drain valve 가 열려있을때만 Purge valve 를 열수 있다.
                        }
                        retVal = ValveManager.SetValveState(enableFlag, valveType, stageIndex);
                    }
                }
                else
                {
                    retVal = EnvControlCore.SetValveState(enableFlag, valveType, stageIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                ChillerManager.GetChillerModule(stageIndex)?.DisConnect();
            }
            return retVal;
        }

        public bool GetValveState(EnumValveType valveType, int stageIndex = -1)
        {
            bool retVal = false;
            try
            {
                if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL ||
                    EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.EMUL)
                {
                    if (this.EnvControlManager().GetValveParam().ValveModuleType.Value == EnumValveModuleType.NA
                        && (valveType == EnumValveType.IN || valveType == EnumValveType.OUT))
                    {
                        retVal = this.EnvControlManager().ChillerManager.IsCirculationActive(stageIndex);
                    }
                    else
                    {
                        retVal = this.EnvControlManager().ValveManager.GetValveState(valveType, (byte)stageIndex);
                    }
                }
                else
                {
                    retVal = EnvControlCore.GetValveState(valveType, stageIndex);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }




        public IValveSysParameter GetValveParam()
        {
            return EnvControlParam?.ValveSysParam ?? null;
        }

        #region ... Dry Air
        public byte[] GetDryAirParam(int stageindex = -1)
        {
            byte[] retVal = null;
            try
            {
                if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL)
                    retVal = DryAirManager.GetDryAirParam();
                else
                    retVal = EnvControlCore.GetDryAirParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL)
                    retVal = DryAirManager.DryAirForProber(value, dryairType, stageIndex);
                else
                {
                    retVal = EnvControlCore.DryAirForProber(value, dryairType, stageIndex);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageIndex = -1)
        {
            int retVal = -1;
            try
            {
                if (EnvControlParam.EnvControlMode == EnumEnvControlModuleMode.LOCAL)
                    retVal = DryAirManager.GetLeakSensor(out value, leakSensorIndex, stageIndex);
                else
                {
                    retVal = EnvControlCore.GetLeakSensor(out value, leakSensorIndex, stageIndex);
                }
            }
            catch (Exception err)
            {
                value = false;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        public bool CanInactivateChiller(ObservableCollection<int> stageIdxs)
        {
            bool retVal = true;
            try
            {
                //bool inValveState = false;
                //bool outValveState = false;
                if (stageIdxs != null)
                {
                    if (stageIdxs.Count > 1)
                    {
                        foreach (var index in stageIdxs)
                        {
                            ILoaderSupervisor LoaderSupervisor = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                            if (LoaderSupervisor.StageStates.Count >= index)
                            {
                                if (LoaderSupervisor.StageStates[index - 1] == ModuleStateEnum.RUNNING
                                       | LoaderSupervisor.StageStates[index - 1] == ModuleStateEnum.PAUSED
                                       | LoaderSupervisor.StageStates[index - 1] == ModuleStateEnum.SUSPENDED)
                                {
                                    retVal = true;
                                    break;
                                }
                            }

                            //inValveState = GetValveState(EnumValveType.IN, index);
                            //outValveState = GetValveState(EnumValveType.OUT, index);

                            //if ((inValveState | outValveState) == true)
                            //{
                            //    retVal = true;
                            //    break;
                            //}
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEMGSTOP()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = ChillerManager.SetEMGSTOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool GetIsExcute()
        {
            try
            {
                return EnvControlCore.GetIsExcute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }
        }
    }
}
