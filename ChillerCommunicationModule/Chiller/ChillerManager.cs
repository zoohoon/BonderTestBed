using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlModules
{
    using Autofac;
    using ControlModules.Chiller;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.Chiller;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Temperature.Temp.Chiller;


    public class ChillerManager : INotifyPropertyChanged, IFactoryModule, IChillerManager, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #region < Property >

        private List<IChillerModule> _Chillers = new List<IChillerModule>(); //물리적인 칠러 연결
        public List<IChillerModule> Chillers
        {
            get { return _Chillers; }
            set
            {
                if (value != _Chillers)
                {
                    _Chillers = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ChillerSysParameter _ChillerCommParam = new ChillerSysParameter();

        public ChillerSysParameter ChillerCommParam
        {
            get { return _ChillerCommParam; }
            set { _ChillerCommParam = value; }
        }

        private ChillerErrorParameter _ChillerErrorParam;
        public ChillerErrorParameter ChillerErrorParam
        {
            get { return _ChillerErrorParam; }
            set
            {
                if (value != _ChillerErrorParam)
                {
                    _ChillerErrorParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ChillerServiceHost ServiceHost { get; set; }
        //private IChillerAdapter ChillerAdapter { get; set; }
        public bool Initialized { get; set; } = false;
        private object checkChillerGroupLockObj = new object();
        private object checkCanUseChillerLockObj = new object();
        #endregion

        public ChillerManager()
        {

        }

        public ChillerManager(ChillerSysParameter chillercommparam)
        {
            _ChillerCommParam = chillercommparam;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    if (this.EnvControlManager()?.GetEnvcontrolMode() == EnumEnvControlModuleMode.LOCAL
                        || this.EnvControlManager()?.GetEnvcontrolMode() == EnumEnvControlModuleMode.EMUL)
                    {
                        retVal = InitServiceHost();
                    }

                    Chillers.Clear();
                    for (int index = 0; index <= _ChillerCommParam.ChillerParams.Count - 1; index++)
                    {
                        Chillers.Add(new ChillerModule(_ChillerCommParam.ChillerParams[index], index + 1));
                        Chillers[Chillers.Count - 1].InitModule();

                        if (Chillers[Chillers.Count - 1].ChillerParam.ChillerModuleMode.Value != EnumChillerModuleMode.NONE)
                        {
                            Chillers[Chillers.Count - 1].Start(true);
                        }
                    }
                    Initialized = true;
                }
                else
                {
                    foreach (var chillerModule in Chillers)
                    {
                        retVal = chillerModule.InitModule();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            try
            {
                foreach (var chiller in Chillers)
                {
                    chiller.DeInitModule();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        #region < Load & Save Parameter >

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new ChillerErrorParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(ChillerErrorParameter));
                ChillerErrorParam = (ChillerErrorParameter)tmpParam;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(ChillerErrorParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

        public string GetErrorMessage(double errornumber)
        {
            string retMessage = "";
            try
            {
                if (ChillerErrorParam.ChillerErrorMessageDic != null)
                {
                    ChillerErrorParam.ChillerErrorMessageDic.TryGetValue(errornumber, out retMessage);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retMessage;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var chiller in Chillers)
                {
                    retVal = chiller.Start();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum InitServiceHost()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ServiceHost = new ChillerServiceHost() { Manager = this };
                if (ServiceHost != null)
                {
                    ServiceHost.InitModule();
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<IChillerModule> GetChillerModules()
        {
            List<IChillerModule> chillers = null;
            try
            {
                chillers = this.Chillers;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return chillers;
        }

        public IChillerModule GetChillerModule(int stageindex = -1, int chillerIndex = -1)
        {
            IChillerModule module = null;
            try
            {
                if (chillerIndex != -1)
                {
                    module = Chillers[chillerIndex - 1];
                    return module;
                }

                if (this.EnvControlManager().GetEnvcontrolMode() == EnumEnvControlModuleMode.LOCAL)
                {
                    if (Chillers.Count == 0)
                        return null;
                    if (stageindex != -1)
                    {
                        for (int index = 0; index <= ChillerCommParam.ChillerParams.Count; index++)
                        {
                            for (int sindex = 0; sindex < ChillerCommParam.ChillerParams[index].StageIndexs.Count; sindex++)
                            {
                                if (ChillerCommParam.ChillerParams[index].StageIndexs[sindex] == stageindex)
                                {
                                    module = Chillers[index];
                                    return module;
                                }
                            }
                        }
                    }
                    else
                    {
                        return Chillers[Chillers.Count - 1];
                    }

                }
                else if (this.EnvControlManager().GetEnvcontrolMode() == EnumEnvControlModuleMode.REMOTE)
                {
                    if (stageindex != -1)
                    {
                        for (int index = 0; index <= ChillerCommParam.ChillerParams.Count; index++)
                        {
                            for (int sindex = 0; sindex < ChillerCommParam.ChillerParams[index].StageIndexs.Count; sindex++)
                            {
                                if (ChillerCommParam.ChillerParams[index].StageIndexs[sindex] == stageindex)
                                {
                                    module = Chillers[index];
                                    return module;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Chillers.Count == 0)
                            return null;
                        else
                            module = Chillers[Chillers.Count - 1];
                    }
                }
                else if (this.EnvControlManager().GetEnvcontrolMode() == EnumEnvControlModuleMode.EMUL)
                {
                    if (stageindex != -1)
                    {
                        for (int index = 0; index <= ChillerCommParam.ChillerParams.Count; index++)
                        {
                            for (int sindex = 0; sindex < ChillerCommParam.ChillerParams[index].StageIndexs.Count; sindex++)
                            {
                                if (ChillerCommParam.ChillerParams[index].StageIndexs[sindex] == stageindex)
                                {
                                    module = Chillers[index];
                                    return module;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Chillers.Count > 0)
                            module = Chillers[0];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return module;
        }

        private IChillerComm GetChillerCommModule(int stageidx = -1, int chilleridx = -1)
        {
            IChillerComm module = null;
            try
            {
                if (stageidx != -1)
                {
                    var chillermodule = GetChillerModule(stageidx);
                    if (chillermodule != null)
                    {
                        module = (chillermodule as ChillerModule).ChillerComm;
                    }
                }
                else if (chilleridx != -1)
                {
                    var chillermodule = GetChillerModule(chillerIndex: chilleridx);
                    if (chillermodule != null)
                    {
                        module = (chillermodule as ChillerModule).ChillerComm;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return module;
        }

        public int GetChillerIndex(int stageindex)
        {
            int chillerIndex = -1;
            try
            {
                if (Chillers.Count == 0)
                    chillerIndex = -1;

                if (this.EnvControlManager().GetEnvcontrolMode() == EnumEnvControlModuleMode.LOCAL
                    | this.EnvControlManager().GetEnvcontrolMode() == EnumEnvControlModuleMode.EMUL)
                {
                    if (stageindex != -1)
                    {
                        for (int index = 0; index <= ChillerCommParam.ChillerParams.Count; index++)
                        {
                            for (int sindex = 0; sindex < ChillerCommParam.ChillerParams[index].StageIndexs.Count; sindex++)
                            {
                                if (ChillerCommParam.ChillerParams[index].StageIndexs[sindex] == stageindex)
                                {
                                    chillerIndex = index + 1;
                                    return chillerIndex;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return chillerIndex;
        }

        public EventCodeEnum Connect()
        {
            return EventCodeEnum.NONE;
        }


        public EventCodeEnum Disconnect(int stageindex = -1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            if (ServiceHost != null)
            {
                //chiller host인 경우(Loader)
                retVal = ServiceHost.Disconnect(stageindex);
            }
            else
            {
                //Cell
                foreach (var chiller in Chillers)
                {
                    retVal = chiller.DisConnect(true);
                }
            }
            return retVal;
        }

        //public EventCodeEnum InitChillerAdapter(EnumChillerModuleMode chillermode)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        if(chillermode == EnumChillerModuleMode.CHAGO)
        //        {
        //            if(ChillerAdapter == null)
        //            {
        //                ChillerAdapter = new ChagoChillerAdapter();
        //                ChillerAdapter.InitModule();
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }
        //    return retVal;
        //}

        //public EventCodeEnum DeInitChillerAdapter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        ChillerAdapter.Dispose();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }
        //    return retVal;
        //}

        //public IChillerAdapter GetChillerAdapter()
        //{
        //    return ChillerAdapter;
        //}
        public bool IsServiceAvailable()
        {
            return true;
        }

        public EventCodeEnum SetEMGSTOP()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                foreach (var chiiler in Chillers)
                {
                    retVal = chiiler.Inactivate();
                    chiiler.ChillerInfo.ChillerMode = CillerModeEnum.MAINTANANCE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EnumChillerModuleMode GetMode(int stageindex = -1)
        {
            EnumChillerModuleMode retVal = EnumChillerModuleMode.NONE;
            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    if (stageindex != -1)
                    {
                        for (int index = 0; index <= ChillerCommParam.ChillerParams.Count; index++)
                        {
                            var CommParam = ChillerCommParam.ChillerParams[index].StageIndexs.Where(
                                idx => idx == stageindex);
                            if (CommParam != null)
                            {
                                retVal = ChillerCommParam.ChillerParams[index].ChillerModuleMode.Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var chillerindex = stageindex;
                        retVal = _ChillerCommParam.ChillerParams[chillerindex - 1].
                                ChillerModuleMode.Value;

                    }
                }
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    if (_ChillerCommParam.ChillerParams.Count >= 1)
                    {
                        retVal = _ChillerCommParam.ChillerParams[0].
                            ChillerModuleMode.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EnumCommunicationState GetCommState(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetCommState(0) ?? EnumCommunicationState.UNAVAILABLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EnumCommunicationState.UNAVAILABLE;
            }
        }
        public EventCodeEnum Connect(string address, int port, int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.Connect(address, port) ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }
        public byte[] GetChillerParam(int stageindex = -1)
        {
            try
            {
                return GetChillerModule(stageindex)?.GetChillerParam() ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }
        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, int stageindex = -1, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (stageindex != -1)
                {
                    //var stages = 
                    var chiller = GetChillerModule(stageindex);
                    if (chiller != null)
                    {
                        //retVal = CheckCanUseChiller(sendVal, stageindex, true);
                        //if (retVal == EventCodeEnum.NONE)
                        //{
                            // Chiller 가 동작중인 Stage 가 없거나, Chiller 의 SV와 설정 온도가 다른경우
                            retVal = chiller.SetTargetTemp(sendVal, sendTempValueType);
                        //}
                        //else
                        //{
                        //    LoggerManager.Debug($"[CHI][Chiller #{chiller.ChillerInfo.Index}] Target Chiller already in-use. Chiller SV = {sendVal}℃");
                        //    retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_DIFF_ACTIVE;
                        //}
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager SetTargetTemp() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_ERROR;
            }
            //retVal = EventCodeEnum.NONE;
            return retVal;
        }

        /// <summary>
        /// Chiller 를 사용할 수 있는지, 확인하는 함수.
        /// 같은 층에서 
        /// </summary>
        /// <param name="sendVal">Chiller 설정 온도</param>
        /// <param name="stageindex">요청한 Stage Index</param>
        /// <param name="offvalve">Chiller 온도 변경이 가능하다면 (사용할 수 있다면), 다른 셀들의 벨브를 닫을 것인지 </param>
        ///                        true : 닫는다, false : 안닫는다.
        /// <returns></returns>
        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                var chillerModule = GetChillerModule(stageindex);
                if (chillerModule != null)
                {
                    if (chillerModule.GetChillerMode() == EnumChillerModuleMode.REMOTE)
                    {
                        retVal = chillerModule.CheckCanUseChiller(sendVal, stageindex);
                    }
                    else if (chillerModule.GetChillerMode() == EnumChillerModuleMode.HUBER || chillerModule.GetChillerMode() == EnumChillerModuleMode.SOLARDIN || chillerModule.GetChillerMode() == EnumChillerModuleMode.EMUL)
                    {
                        if (stageindex != -1)
                        {
                            var chiller = GetChillerModule(stageindex);
                            if (chiller != null)
                            {
                                retVal = chiller.CheckCanUseChiller(sendVal, stageindex);
                            }
                            else
                            {
                                retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_INVALID_DATA;
                            }
                        }
                        else
                        {
                            retVal = EventCodeEnum.CHILLER_SET_TARGET_TEMP_FAIL_INVALID_DATA;
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

        public bool CanRunningLot()
        {
            bool retVal = false;
            try
            {
                if(Chillers != null && Chillers.Count != 0)
                {
                    foreach (var chiller in Chillers)
                    {
                        retVal = chiller.CanRunningLot();
                        if(retVal == false)
                        {
                            return retVal;
                        }
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        
        public void SetTempActiveMode(bool bValue, int stageindex = -1)// 칠러 작동 
        {
            try
            {
                GetChillerCommModule(stageindex)?.SetTempActiveMode(bValue, 0);                

                #region Check Circulation Mode                                
                //bool isChillerExternalMode = GetChillerCommModule(stageindex)?.IsCirculationActive() ?? false;

                var chiller = GetChillerModule(stageindex);
                if (chiller != null)
                {
                    SetCircuationActive(bValue, stageindex);//ISSD-3554 임시수정. DewPoint 고려 필요
                }



                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager SetTempActiveMode() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
            }
        }

        public void SetSetTempPumpSpeed(int iValue, int stageindex = -1)
        {
            try
            {
                GetChillerCommModule(stageindex)?.SetSetTempPumpSpeed(iValue, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager SetSetTempPumpSpeed() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
            }
        }

        public void SetCircuationActive(bool bValue, int stageindex = -1, int chillerindex = -1)
        {
            try
            {
                if (stageindex != -1)
                {
                    GetChillerCommModule(stageindex)?.SetCircuationActive(bValue, 0);
                }
                else if (chillerindex != -1)
                {
                    GetChillerCommModule(chilleridx: chillerindex)?.SetCircuationActive(bValue, 0);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager SetCircuationActive() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
            }
        }

        public double GetSetTempValue(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetSetTempValue(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetSetTempValue() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetInternalTempValue(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetInternalTempValue(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetInternalTempValue() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetReturnTempVal(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetReturnTempVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetReturnTempVal() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetPumpPressureVal(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetPumpPressureVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetPumpPressureVal() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetCurrentPower(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetCurrentPower(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetCurrentPower() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetErrorReport(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetErrorReport(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetErrorReport() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetWarningMessage(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetWarningMessage(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetWarningMessage() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetProcessTempVal(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetProcessTempVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetProcessTempVal() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetExtMoveVal(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetExtMoveVal(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetExtMoveVal() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetStatusOfThermostat(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetStatusOfThermostat(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetStatusOfThermostat() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public bool IsAutoPID(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.IsAutoPID(0) ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager IsAutoPID() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool IsTempControlProcessMode(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.IsTempControlProcessMode(0) ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager IsTempControlProcessMode() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return false;
            }
        }

        public bool IsTempControlActive(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.IsTempControlActive(0) ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager IsTempControlActive() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return false;
            }
        }

        public (bool, bool) GetProcTempActValSetMode(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetProcTempActValSetMode(0) ?? (false, false);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetProcTempActValSetMode() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return (false, false);
            }
        }

        public int GetSerialNumLow(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetSerialNumLow(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetSerialNumLow() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetSerialNumHigh(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetSerialNumHigh(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetSerialNumHigh() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetSerialNumber(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetSerialNumber(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetSerialNumber() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public bool IsCirculationActive(int stageindex = -1, int chillerindex = -1)
        {
            try
            {
                if (stageindex != -1)
                {
                    return GetChillerCommModule(stageindex)?.IsCirculationActive(0) ?? false;
                }
                else if (chillerindex != -1)
                {
                    return GetChillerCommModule(chilleridx: chillerindex)?.IsCirculationActive(0) ?? false;
                }
                return false;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager IsCirculationActive() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return false;
            }
        }

        public (bool, bool) IsOperatingLock(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.IsOperatingLock(0) ?? (false, false);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager IsOperatingLock() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return (false, false);
            }
        }

        public int GetPumpSpeed(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetPumpSpeed(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetPumpSpeed() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetMinSetTemp(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetMinSetTemp(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetMinSetTemp() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetMaxSetTemp(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetMaxSetTemp(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetMaxSetTemp() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public int GetSetTempPumpSpeed(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetSetTempPumpSpeed(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetSetTempPumpSpeed() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetUpperAlramInternalLimit(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetUpperAlramInternalLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetUpperAlramInternalLimit() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetLowerAlramInternalLimit(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetLowerAlramInternalLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetLowerAlramInternalLimit() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetUpperAlramProcessLimit(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetUpperAlramProcessLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetUpperAlramProcessLimit() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public double GetLowerAlramProcessLimit(int stageindex = -1)
        {
            try
            {
                return GetChillerCommModule(stageindex)?.GetLowerAlramProcessLimit(0) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ChillerManager GetLowerAlramProcessLimit() Error occured [Stage Index #{stageindex}] : { err.Message}");
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public void InitService()
        {
            return;
        }

        public long GetCommReadDelayTime()
        {
            long retVal = 0;
            try
            {
                retVal = ChillerCommParam.CommReadDelayTime.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public long GetCommWriteDelayTime()
        {
            long retVal = 0;
            try
            {
                retVal = ChillerCommParam.CommWriteDelayTime.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public int GetPingTimeOut()
        {
            int retVal = 0;
            try
            {
                retVal = ChillerCommParam.PingTimeOut.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool GetChillerAbortActiveState(int stageindex = -1)
        {
            bool retVal = true;
            try
            {
                retVal = GetChillerModule(stageindex)?.ChillerParam.IsAbortActivate ?? true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// Chiller Index와 Lock Value를 받아 SetOperationLockFlag파라미터에 Set해주는 함수.
        /// Chiller가 실제 하드웨어 상 한대이고 그 안에 세개의 chiller system이 동작하는 Group형 일 때(ex.MPT)와 Chiller한대당 하나의 Chiller System이 동작하는 1 in 1 형일 때(ex. Opera, MD..)를 구분하여 SetOperationLockFlag를 Set해준다. 
        /// </summary>
        /// <param name="selectedchillerindex"></param>
        /// <param name="lockValue"></param>
        public void Set_OperaionLockValue(int selectedchillerindex = -1, bool lockValue = false)
        {
            try
            {
                for(int index = 0; index <= _ChillerCommParam.ChillerParams.Count-1; index++)
                {
                    if(_ChillerCommParam.ChillerParams[index].IP == _ChillerCommParam.ChillerParams[selectedchillerindex-1].IP)//같은 IP가 있으면
                    {
                        if (_ChillerCommParam.ChillerParams[index].GroupIndex == _ChillerCommParam.ChillerParams[selectedchillerindex-1].GroupIndex) //같은 IP이면서 같은 Group Index이면
                        {
                            if(Chillers.Count > 0 && _ChillerCommParam.ChillerParams[index].ChillerModuleMode.Value != EnumChillerModuleMode.REMOTE)//Chiller Module이 생성되어있다면
                            {
                                Chillers[index].ChillerInfo.SetOperationLockFlag = lockValue;
                                Chillers[index].SetOperatingLock(lockValue, false);
                                LoggerManager.Debug($"Set_OperaionLockValue() Chiller Index[{Chillers[index].ChillerInfo.Index}], SetOperationLockFlag : {lockValue}");
                            }
                        }
                    }
                    else
                    {
                        //IP가 다르면 아무것도 Set하지 않음.
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
