namespace GEMModule
{
    using EventProcessModule;
    using EventProcessModule.GEM;
    using GEMModule.Parameter;
    using LogModule;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Event;
    using ProberInterfaces.Event.EventProcess;
    using ProberInterfaces.GEM;
    using SecsGemServiceInterface;
    using Serial;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using XGemCommModule;
    using System.Collections;
    using System.Threading.Tasks;
    using ProberInterfaces.Loader;

    [CallbackBehavior(UseSynchronizationContext = false)]
    public class GEM : IGEMModule, INotifyPropertyChanged,
                       IProbeEventSubscriber, IHasCommandRecipe,
                       IFactoryModule, IDisposable
    {
        #region <remarks> PropertyChanged                           </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region <remarks> GEM Module Property & Field               </remarks>
        /// <summary>
        ///     GEM 모듈에서 사용하는 속성과 필드 관리.
        /// </summary>
        /// <remarks>
        ///     First Create : 2017-12-14, Semics R&D1, Jake Kim.
        /// </remarks>

        private IGEMCommManager _GemCommManager;
        public IGEMCommManager GemCommManager
        {
            get { return _GemCommManager; }
            set
            {
                if (_GemCommManager != value)
                {
                    _GemCommManager = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private GemSysParameter _GemSysParam;
        public IGemSysParameter GemSysParam
        {
            get { return _GemSysParam; }
            set
            {
                if (_GemSysParam != value)
                {
                    _GemSysParam = value as GemSysParameter;
                    RaisePropertyChanged();
                }
            }
        }

        public XGemCommManager GemCommManagerRef => GemCommManager as XGemCommManager;
        public GemSysParameter GemSysParamRef => GemSysParam as GemSysParameter;

        private GEMAlarmParameter _GemAlarmSysParam;
        public IParam GemAlarmSysParam
        {
            get { return _GemAlarmSysParam; }
            set
            {
                if (value != _GemAlarmSysParam)
                {
                    _GemAlarmSysParam = value as GEMAlarmParameter;
                    RaisePropertyChanged();
                }
            }
        }

        public GemRemoteActionRecipeParam _GemRemoteActionRecipeParam { get; set; }

        private Dictionary<string, IGemActBehavior> _GemRemoteActionRecipeDic
             = new Dictionary<string, IGemActBehavior>();
        public Dictionary<string, IGemActBehavior> GemRemoteActionRecipeDic
        {
            get { return _GemRemoteActionRecipeDic; }
            set
            {
                if (value != _GemRemoteActionRecipeDic)
                {
                    _GemRemoteActionRecipeDic = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ProberGemIdDictionaryParam _DicSVID;
        public ProberGemIdDictionaryParam DicSVID
        {
            get { return _DicSVID; }
            set
            {
                if (_DicSVID != value)
                {
                    _DicSVID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProberGemIdDictionaryParam _DicDVID;
        public ProberGemIdDictionaryParam DicDVID
        {
            get { return _DicDVID; }
            set
            {
                if (_DicDVID != value)
                {
                    _DicDVID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProberGemIdDictionaryParam _DicECID;
        public ProberGemIdDictionaryParam DicECID
        {
            get { return _DicECID; }
            set
            {
                if (_DicECID != value)
                {
                    _DicECID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private GemStateDefineParameter _GemStateDefineParam;
        public GemStateDefineParameter GemStateDefineParam
        {
            get { return _GemStateDefineParam; }
            set
            {
                if (value != _GemStateDefineParam)
                {
                    _GemStateDefineParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private EnumCommunicationState _CommunicationState;
        public EnumCommunicationState CommunicationState
        {
            get { return _CommunicationState; }
            set
            {
                _CommunicationState = value;
                RaisePropertyChanged();
            }
        }

        public GEMHOST_MANUALACTION GEMHostManualAction { get; set; } = GEMHOST_MANUALACTION.UNDEFIND;
        public bool LoadPortModeEnable { get; set; }
        private PIVContainer pIVContainer;
        public EventProcessList SubscribeRecipeParam { get; set; }
        
        //[Test Property]Gem Emul로 Lot 돌리는걸 테스트 하기위한 프로퍼티 (XGemCommanderHost 에 사용코드 있음)
        public bool IsExternalLotMode { get; set; } = false;
        public bool Initialized { get; set; } = false;
        private bool IsDisposed = false;
        #endregion

        #region <remarks> GEM Module Constructor                    </remarks>
        /// <summary>
        ///     GEM Module 생성자
        /// </summary>
        public GEM()
        {
            try
            {
                CommunicationState = EnumCommunicationState.UNAVAILABLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        ~GEM()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if (this.IsDisposed == false)
                {
                    GemCommManager?.Dispose();

                    this.IsDisposed = true;
                    this.Initialized = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> Save & Load                               </remarks>
        /// <summary>
        ///     Save & Load
        /// </summary>
        /// <remarks>
        ///     First Create : 2018-04-26, Semics R&D1, Jake Kim.
        /// </remarks>
        #region ==> Load System Parameter
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = LoadGemSysParameter();
                RetVal = LoadSvidParameter();
                RetVal = LoadDvidParameter();
                RetVal = LoadEcidParameter();
                RetVal = LoadGemStateDefineParameter();
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadGemSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {

                // Gem System Parameter
                IParam tmpParam = null;
                tmpParam = new GemSysParameter();
                RetVal = this.LoadParameter(ref tmpParam, typeof(GemSysParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    GemSysParam = tmpParam as GemSysParameter;
                }

                //Gem Alarm System Parameter 
                tmpParam = new GEMAlarmParameter(); 
                RetVal = this.LoadParameter(ref tmpParam, typeof(GEMAlarmParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    GemAlarmSysParam = tmpParam as GEMAlarmParameter;
                }

                //Gem Remote Action Recipe Parameter
                tmpParam = new GemRemoteActionRecipeParam();
                RetVal = this.LoadParameter(ref tmpParam, typeof(GemRemoteActionRecipeParam));

                if (RetVal == EventCodeEnum.NONE)
                {
                    _GemRemoteActionRecipeParam = tmpParam as GemRemoteActionRecipeParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadSvidParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ProberGemIdDictionaryParam();
                LoggerManager.Debug($"[{this.GetType().Name}] Start Load to SVID Param.");
                RetVal = this.LoadParameter(ref tmpParam, typeof(ProberGemIdDictionaryParam), "SVID");

                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish loading SVID Param.");
                    DicSVID = tmpParam as ProberGemIdDictionaryParam;

                    //DicSVID.DicProberGemID.Value.Clear();
                    //var elementdic = this.ParamManager().CommonDBElementDictionary;
                    //foreach (var elements in elementdic.Values)
                    //{
                    //    foreach (var element in elements.Values)
                    //    {
                    //        if (!DicSVID.DicProberGemID.Value.ContainsKey(element.ElementID) & element.ElementID != -1)
                    //        {
                    //            DicSVID.DicProberGemID.Value.Add(element.ElementID, new GemVidInfo(element.VID, null));
                    //            changeflag = true;
                    //        }
                    //    }
                    //}
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail loading SVID Param.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadDvidParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ProberGemIdDictionaryParam();
                LoggerManager.Debug($"[{this.GetType().Name}] Start Load to DVID Param.");
                RetVal = this.LoadParameter(ref tmpParam, typeof(ProberGemIdDictionaryParam), "DVID");

                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish loading DVID Param.");
                    DicDVID = tmpParam as ProberGemIdDictionaryParam;
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail loading DVID Param.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadEcidParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ProberGemIdDictionaryParam();
                LoggerManager.Debug($"[{this.GetType().Name}] Start Load to ECID Param.");
                RetVal = this.LoadParameter(ref tmpParam, typeof(ProberGemIdDictionaryParam), "ECID");

                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish loading ECID Param.");
                    DicECID = tmpParam as ProberGemIdDictionaryParam;
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail loading ECID Param.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum LoadGemStateDefineParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new GemStateDefineParameter();
                LoggerManager.Debug($"[{this.GetType().Name}] Start Load to GemStateDefineParameter");
                RetVal = this.LoadParameter(ref tmpParam, typeof(GemStateDefineParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish GemStateDefineParameter.");
                    GemStateDefineParam = tmpParam as GemStateDefineParameter;
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail GemStateDefineParameter.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        #endregion

        #region ==> Save System Parameter
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = SaveGemSysParam();
                RetVal = SaveSvidParameter();
                RetVal = SaveDvidParameter();
                RetVal = SaveEcidParameter();
                RetVal = SaveGemAlaramSysParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        private EventCodeEnum SaveGemSysParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (GemSysParam is GemSysParameter)
                {
                    RetVal = this.SaveParameter(GemSysParam as GemSysParameter);

                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[GEMModule] SaveSysParam(): Serialize Error");
                        return RetVal;
                    }

                    RetVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        private EventCodeEnum SaveSvidParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Start saving SVID Param.");
                RetVal = this.SaveParameter(DicSVID, "SVID");

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail saving SVID Param.");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish saving SVID Param.");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        private EventCodeEnum SaveDvidParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Start saving DVID Param.");
                RetVal = this.SaveParameter(DicDVID, "DVID");

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail saving DVID Param.");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish saving DVID Param.");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        private EventCodeEnum SaveEcidParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}] Start saving ECID Param.");
                RetVal = this.SaveParameter(DicECID, "ECID");

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Fail saving ECID Param.");
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}] Finish saving ECID Param.");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        private EventCodeEnum SaveGemAlaramSysParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (GemAlarmSysParam is GEMAlarmParameter)
                {
                    RetVal = this.SaveParameter(GemAlarmSysParam as GEMAlarmParameter);

                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[GEMModule] SaveSysParam(): Serialize Error");
                        return RetVal;
                    }

                    RetVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        public EventCodeEnum SaveGemRemoteActionRecipeParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (_GemRemoteActionRecipeParam is GemRemoteActionRecipeParam)
                {
                    retVal = this.SaveParameter(_GemRemoteActionRecipeParam as GemRemoteActionRecipeParam);

                    if (retVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[GEMModule] SaveGemRemoteActionRecipeParam(): Serialize Error");
                        return retVal;
                    }

                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum LoadRemoteActionRecipeBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                DllImporter.DllImporter dllImporter = new DllImporter.DllImporter();
                //String strFolder = System.IO.Directory.GetCurrentDirectory();
                String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                foreach (var recipe in _GemRemoteActionRecipeParam.ModuleDllInfoDictionary)
                {
                    string fullpath = Path.Combine(strFolder, recipe.Value.AssemblyName);
                    //var retAssembly = dllImporter.LoadDLL(strFolder + "\\" + recipe.Value.AssemblyName);
                    var retAssembly = dllImporter.LoadDLL(fullpath);

                    if (retAssembly != null)
                    {
                        if (retAssembly.Item1 == true)
                        {
                            foreach (Type type in retAssembly.Item2.GetExportedTypes())
                            {
                                if (recipe.Value.ClassName != null)
                                {
                                    if (type.Name == recipe.Value.ClassName)
                                    {
                                        GemRemoteActionRecipeDic.Add(recipe.Key, (IGemActBehavior)Activator.CreateInstance(type));
                                        break;
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        //Load Assembly Error
                        LoggerManager.Debug($"[SECS/GEM Module]Gem remote action dll load fail. ActionName : {recipe.Value.ClassName}");
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

       

        #endregion

        #region <remarks> IProbeEventSubscriber Implements          </remarks>
        /// <summary>
        ///     IProbeEventSubscriber 구현.
        /// </summary>

        public long GetEventNumberFormEventName(string eventName)
        {
            long eventNum = -1;
            try
            {
                //LoadGEMSubscribeRecipe();
                var eventparameter = SubscribeRecipeParam.Find(param => param.EventFullName.Equals(eventName))?.Parameter;
                if (eventparameter != null)
                {
                    eventNum = Convert.ToInt64(eventparameter);
                }
                else
                {
                    LoggerManager.Error($"GEM, GetEventNumberFormEventName() : {eventName}, Can not found event number.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return eventNum;
        }

        private void MICRON(EventProcessList param)
        {
            try
            {
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageOnlineEvent).FullName, Parameter = 101 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageAllocatedEvent).FullName, Parameter = 102 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferLoadedInCurrentLotEvent).FullName, Parameter = 106 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ProbingZUpFirstProcessEvent).FullName, Parameter = 103 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ProbingZUpProcessEvent).FullName, Parameter = 104 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferTestEndEvent).FullName, Parameter = 105 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageStartedPreprocessingEvent).FullName, Parameter = 106 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageDeallocatedEvent).FullName, Parameter = 107 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferCancelledBeforeProbing).FullName, Parameter = 108 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageOffineEvent).FullName, Parameter = 109 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageReadyToEnterToProcessEvent).FullName, Parameter = 110 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageErrorEvent).FullName, Parameter = 111 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferTestingCanceledCanceledByHostEvent).FullName, Parameter = 112 });

                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MatchedToTestFirstProcessEvent).FullName, Parameter = 113});
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MatchedToTestEvent).FullName, Parameter = 114});

                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageSeparatedForNextIndexEvent).FullName, Parameter = 114 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferTestingAborted).FullName, Parameter = 115 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(NextWaferPreprocessingEvent).FullName, Parameter = 116 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(SetupStartEvent).FullName, Parameter = 117 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(SetupEndEvent).FullName, Parameter = 118 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ProbingZDownProcessEvent).FullName, Parameter = 119 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(TempMonitorChangeValueEvent).FullName, Parameter = 120 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(TempMoveOutofRangeEvent).FullName, Parameter = 121 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(TempMoveIntoRangeEvent).FullName, Parameter = 122 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(UpdateStageProbingDataEvent).FullName, Parameter = 123 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PMIStartEvent).FullName, Parameter = 131 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PMIEndEvent).FullName, Parameter = 132 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CleaningStartEvent_PolishWafer).FullName, Parameter = 135 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CleaningEndEvent_PolishWafer).FullName, Parameter = 136 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PreHeatStartEvent).FullName, Parameter = 141 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PreHeatEndEvent).FullName, Parameter = 142 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageProbecardInitalStateEvent).FullName, Parameter = 151 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CardLoadingEvent).FullName, Parameter = 152 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CardUnloadedEvent).FullName, Parameter = 153 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupOnlineEvent).FullName, Parameter = 201 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierPlacedEvent).FullName, Parameter = 202 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LoadportActivatedEvent).FullName, Parameter = 203 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ScanFoupDoneEvent).FullName, Parameter = 204 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(SlotsSelectedEvent).FullName, Parameter = 205 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupProcessingStartEvent).FullName, Parameter = 206 });
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotEndEvent).FullName, Parameter = 207 });
                }
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierRemovedEvent).FullName, Parameter = 208 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupUnloadedInLotAbortEvent).FullName, Parameter = 209 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupOfflineEvent).FullName, Parameter = 210 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(RecipeDownloadSucceededEvent).FullName, Parameter = 301 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(RecipeDownloadFailedEvent).FullName, Parameter = 302 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageRecipeDownloadEvent).FullName, Parameter = 303 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageRecipeDownloadSuccededEvent).FullName, Parameter = 304 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageRecipeDownloadFailedEvent).FullName, Parameter = 305 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageRecipeReadCompleteEvent).FullName, Parameter = 306 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageRecipeReadFailedEvent).FullName, Parameter = 307 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(TowerLightEvent).FullName, Parameter = 401 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteIDReadDoneEvent).FullName, Parameter = 9005 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteIDReadFailEvent).FullName, Parameter = 9006 });

                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotEndDueToUnloadAllWaferEvent).FullName, Parameter = 1331 , ConditionChecker = new DynamicFoupEnableChecker()});
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferUnloadedToSlotEvent).FullName, Parameter = 1351, ConditionChecker = new DynamicFoupEnableChecker() });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferUnloadedFailToSlotEvent).FullName, Parameter = 1352, ConditionChecker = new DynamicFoupEnableChecker() });

                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MarkAlignmentStart).FullName, Parameter = 3201 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MarkAlignmentDone).FullName, Parameter = 3202 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MarkAlignmentDoneBeforePinAlignment).FullName, Parameter = 3204 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MarkAlignmentDoneBeforeWaferAlignment).FullName, Parameter = 3206 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MarkAlignmentDoneAfterWaferAlignment).FullName, Parameter = 3208 });

                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupStateChangedToLoadAndUnloadEvent).FullName, Parameter = 9031 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupStateChangedToUnloadEvent).FullName, Parameter = 9032 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotModeChangeToDynamicEvent).FullName, Parameter = 9041 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotModeChangeToNormalEvent).FullName, Parameter = 9042 });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void YMTC(EventProcessList param)
        {
            try
            {
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageOnlineEvent).FullName, Parameter = 2017 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(DeviceChangedEvent).FullName, Parameter = 2018 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(RecipeDownloadSucceededEvent).FullName, Parameter = 301 }); // 임시
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(RecipeDownloadFailedEvent).FullName, Parameter = 302 });    // 임시
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferLoadedEvent).FullName, Parameter = 3101 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferUnloadedEvent).FullName, Parameter = 3102 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageAllocatedEvent).FullName, Parameter = 3103 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ProbingZDownProcessEvent).FullName, Parameter = 3104 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(UpdateStageProbingDataEvent).FullName, Parameter = 3112 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ProbingZUpFirstProcessEvent).FullName, Parameter = 3113 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ProbingZUpProcessEvent).FullName, Parameter = 3113 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MatchedToTestEvent).FullName, Parameter = 3114 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(GoToStartDieEvent).FullName, Parameter = 3115 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PinAlignStartEvent).FullName, Parameter = 3301 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PinAlignEndEvent).FullName, Parameter = 3302 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PinAlignFailEvent).FullName, Parameter = 3303 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferAlignStartEvent).FullName, Parameter = 3351 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferAlignEndEvent).FullName, Parameter = 3352 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(WaferAlignFailEvent).FullName, Parameter = 3353 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PreHeatStartEvent).FullName, Parameter = 3401 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PreHeatEndEvent).FullName, Parameter = 3402 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PreHeatFailEvent).FullName, Parameter = 3403 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PMIStartEvent).FullName, Parameter = 3451 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PMIEndEvent).FullName, Parameter = 3452 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PMIFailEvent).FullName, Parameter = 3453 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PolishWaferLoadedEvent).FullName, Parameter = 3501 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PolishWaferUnloadedEvent).FullName, Parameter = 3502 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CleaningStartEvent_PolishWafer).FullName, Parameter = 3503 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CleaningEndEvent_PolishWafer).FullName, Parameter = 3504 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CleaningFailEvent).FullName, Parameter = 3505 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CardUnloadedEvent).FullName, Parameter = 3601 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CardLoadingEvent).FullName, Parameter = 3602 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CardChangeFailEvent).FullName, Parameter = 3603 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageChuckVacuumErrorEvent).FullName, Parameter = 5001 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(MachineInitFailEvent).FullName, Parameter = 5002 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotStartEvent).FullName, Parameter = 6001 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotEndEvent).FullName, Parameter = 6002 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(LotAbortEvent).FullName, Parameter = 6006 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(StageDeallocatedEvent).FullName, Parameter = 6007 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteLoadDoneEvent).FullName, Parameter = 6452 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteUnloadDoneEvent).FullName, Parameter = 6453 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ScanFoupDoneEvent).FullName, Parameter = 6462 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ScanFoupFailEvent).FullName, Parameter = 6463 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupReadyToLoadEvent).FullName, Parameter = 9001 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupReadyToUnloadEvent).FullName, Parameter = 9002 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierPlacedEvent).FullName, Parameter = 9003 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierRemovedEvent).FullName, Parameter = 9004 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteIDReadDoneEvent).FullName, Parameter = 9005 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteIDReadFailEvent).FullName, Parameter = 9006 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ClampLockEvent).FullName, Parameter = 9014 }); ;
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ClampLockFailEvent).FullName, Parameter = 9015 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ClampUnlockEvent).FullName, Parameter = 9020 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ClampUnlockFailEvent).FullName, Parameter = 9021 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierCompleateEvent).FullName, Parameter = 9022 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupOpenErrorEvent).FullName, Parameter = 9036 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(FoupCloseErrorEvent).FullName, Parameter = 9037 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteLoadFailEvent).FullName, Parameter = 9038 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CassetteUnloadFailEvent).FullName, Parameter = 9039 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(PreAlignFailEvent).FullName, Parameter = 9046 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(ArmErrorEvent).FullName, Parameter = 9047 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(OcrReadFailEvent).FullName, Parameter = 9051 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierAccessModeOnlineEvent).FullName, Parameter = 9023 });
                param.Add(new GemEventProc_EventMessageSet() { EventFullName = typeof(CarrierAccessModeManualEvent).FullName, Parameter = 9024 });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadGEMSubscribeRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SubscribeRecipeParam = new EventProcessList();

                string FullPath = this.FileManager().GetSystemParamFullPath("Event", "Recipe_Subscribe_GEM.json");

                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                    }

                    if (File.Exists(FullPath) == false)
                    {

                        MICRON(SubscribeRecipeParam);
                        //YMTC(SubscribeRecipeParam);

                        RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

                        if (RetVal == EventCodeEnum.PARAM_ERROR)
                        {
                            LoggerManager.Error($"[GEM] LoadDevParam(): Serialize Error");
                            return RetVal;
                        }
                    }

                    IParam tmpPram = null;
                    RetVal = this.LoadParameter(ref tmpPram, typeof(EventProcessList), null, FullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        SubscribeRecipeParam = tmpPram as EventProcessList;
                    }
                    else
                    {
                        RetVal = EventCodeEnum.PARAM_ERROR;

                        LoggerManager.Error($"[GEM] LoadSysParam(): DeSerialize Error");
                        return RetVal;
                    }

                    RetVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("[GEM] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        //public EventCodeEnum LoadGEMSubscribeRecipe()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        SubscribeRecipeParam = new EventProcessList();

        //        string FullPath = this.FileManager().GetSystemParamFullPath("Event", "Recipe_Subscribe_GEM.json");

        //        try
        //        {
        //            if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
        //            {
        //                Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
        //            }

        //            if (File.Exists(FullPath) == false)
        //            {
        //                RetVal = Extensions_IParam.SaveParameter(null, SubscribeRecipeParam, null, FullPath);

        //                if (RetVal == EventCodeEnum.PARAM_ERROR)
        //                {
        //                    LoggerManager.Error($"[GEM] LoadDevParam(): Serialize Error");
        //                    return RetVal;
        //                }
        //            }

        //            IParam tmpPram = null;
        //            RetVal = this.LoadParameter(ref tmpPram, typeof(EventProcessList), null, FullPath);
        //            if (RetVal == EventCodeEnum.NONE)
        //            {
        //                SubscribeRecipeParam = tmpPram as EventProcessList;
        //            }
        //            else
        //            {
        //                RetVal = EventCodeEnum.PARAM_ERROR;

        //                LoggerManager.Error($"[GEM] LoadSysParam(): DeSerialize Error");
        //                return RetVal;
        //            }

        //            RetVal = EventCodeEnum.NONE;
        //        }
        //        catch (Exception err)
        //        {
        //            RetVal = EventCodeEnum.PARAM_ERROR;
        //            //LoggerManager.Error($String.Format("[GEM] LoadDevParam(): Error occurred while loading parameters. Err = {0}", err.Message));
        //            LoggerManager.Exception(err);

        //            throw;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }

        //    return RetVal;
        //}

        public EventCodeEnum RegistEventSubscribe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IEventManager EventManager = this.EventManager();
                RetVal = LoadGEMSubscribeRecipe();

                RetVal = this.EventManager().RegisterEventList(SubscribeRecipeParam, "ProbeEventSubscibers", "GEM");

                if (0 < (this.SubscribeRecipeParam?.Count ?? 0))
                {
                    foreach (var evtname in this.SubscribeRecipeParam)
                    {
                        RetVal = EventManager.RemoveEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GEM] Remove EventSubscribe Error...({evtname.EventFullName})");

                            //break;
                        }
                    }
                    this.SubscribeRecipeParam.Clear();
                }

                RetVal = this.LoadGEMSubscribeRecipe();

                foreach (var evtname in this.SubscribeRecipeParam)
                {
                    RetVal = EventManager.RegisterEvent(evtname.EventFullName, "ProbeEventSubscibers", evtname.EventNotify);

                    if (RetVal != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[GEM] Regist EventSubscribe Error...({evtname.EventFullName})");

                        //break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum SetCommandRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //this.CommandManager().AddCommandParameters(GemSysParam.CommandRecipe.Descriptors);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        #endregion

        #region <remarks> GEM Module Methods & Command              </remarks>

        #region ==> IGEMCommManager Implement
        public void SetGEMDisconnectCallBack(GEMDisconnectDelegate callback)
        {
            GemCommManager?.SetGEMDisconnectCallBack(callback);
        }

        public bool GemEnable()
        {
            bool retVal = false;
            try
            {
                //retVal = GemCommManager == null ? false : true;
                retVal = GemSysParam.Enable.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long CommEnable()
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager?.CommEnable() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }

            return retVal;
        }

        public long CommDisable()
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager?.CommDisable() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long ReqOffLine()
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager?.ReqOffLine() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long ReqLocal()
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager?.ReqLocal() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }

            return retVal;
        }

        public long ReqRemote()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.ReqRemote() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetEvent(long eventNum)//, int stageNum = -1), List<Dictionary<long, (int objtype, object value)>> ExecutorDataDic = null)
        {
            long retVal = 0;
            try
            {
                //if (GemEnable())
                //{
                //    long num = (long)SubscribeRecipeParam[0].Parameter;
                //    var param = SubscribeRecipeParam.Find(recipes => (long)recipes.Parameter == eventNum);
                //    //if (param.Enable == true)
                //    {
                //        retVal = GemCommManager?.SetEvent(eventNum) ?? -1;
                //        LoggerManager.Debug($"[GEM SET EVENT] {eventNum}");
                //    }
                //}

                if (GemEnable())
                {
                    retVal = GemCommManager?.SetEvent(eventNum) ?? -1;// 여기만 중볻 흐름
                    LoggerManager.Debug($"[GEM SET EVENT] {eventNum}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetEcvChanged(int vidLength, long[] convertDataID, string[] values)
        {
            long retVal = -1;
            try
            {
                if (GemEnable())
                {
                    retVal = GemCommManager?.GEMSetECVChanged(vidLength, convertDataID, values) ?? -1;
                    for (int i = 0; i < convertDataID.Count(); i++)
                    {
                        LoggerManager.Debug($"[GEM ECV EVENT] Vid:{convertDataID[i]}, value:{values[i]}");
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
            return retVal;
        }


        /// <summary>
        /// nState => 0 : Alarm Clear, 1 : Alaram Set (Occurred)
        /// </summary>
        /// <param name="nID"></param>
        /// <param name="nState"></param>
        /// <returns></returns>       
        public long SetAlarm(long nID, long nState, int cellIndex = 0)
        {
            long retVal = 0;            
            try
            {                

                if (GemSysParam.GemProcessrorType.Value == GemProcessorType.CELL)
                {
                    if (this.LoaderController().GetChuckIndex() <= 0)
                    {
                        retVal = -1;// 비상탈출.
                        return retVal;
                    }

                }
                else
                {
                    //cellIndex 0으로 사용
                }


                var alarmInfo = _GemAlarmSysParam.GemAlramInfos.Where(info => info.AlaramID == nID).FirstOrDefault();
                if (alarmInfo != null)
                {
                    if (alarmInfo.RaiseOnlyLot)
                    {
                        if (this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.IDLE)
                        {
                            //Lot 지행중일때만 Alarm 발생
                            retVal = GemCommManager?.SetAlarm(nID, nState, cellIndex) ?? -1;                            
                        }
                    }
                    else
                    {
                        retVal = GemCommManager?.SetAlarm(nID, nState, cellIndex) ?? -1;
                    }
                }
                else
                {
                    retVal = GemCommManager?.SetAlarm(nID, nState, cellIndex) ?? -1;                    
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
            return retVal;
        }


        public long ClearAlarm(long nID, long nState)
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetAlarm(nID, nState) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
            return retVal;
        }

        public EventCodeEnum ClearAllAlram()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                int indexOffset = 0;
                if (this.LoaderController() != null && this.LoaderController().GetChuckIndex() > 0)
                {
                    indexOffset = this.LoaderController().GetChuckIndex() - 1;
                }
                foreach (var alarm in _GemAlarmSysParam.GemAlramInfos)
                {
                    SetAlarm(alarm.AlaramID + indexOffset, (long)ProberInterfaces.GEM.GemAlarmState.CLEAR);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// Owner의 알람 상태를 Clear 함.       
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ClearAlarmOnly()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
           {
                if (this.GemSysParam.GemProcessrorType.Value == GemProcessorType.CELL)
                {
                    if (this.LoaderController().GetChuckIndex() == 0)
                    {
                        retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                        return retVal;
                    }
                    else
                    {
                        retVal = ClearTargetAlarmOnly(cellIndex: this.LoaderController().GetChuckIndex());
                    }

                }
                else
                {
                    retVal = ClearTargetAlarmOnly();
                }                

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                LoggerManager.Debug($"[GemModule] ClearAlarmOnly(): result: {retVal}");
            }
            return retVal;
        }

        private EventCodeEnum ClearTargetAlarmOnly(int cellIndex = 0)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = GemCommManager?.ClearAlarmOnly(cellIndex) ?? EventCodeEnum.EXCEPTION;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }


        public long SetInitControlState_Offline()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitControlState_Offline() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetInitControlState_Local()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitControlState_Local() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetInitControlState_Remote()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitControlState_Remote() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetInitProberEstablish()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitProberEstablish() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetInitHostEstablish()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitHostEstablish() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetInitCommunicationState_Enable()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitCommunicationState_Enable() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long SetInitCommunicationState_Disable()
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SetInitCommunicationState_Disable() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        public long TimeRequest()
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager?.TimeRequest() ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long SendTerminal(string sendStr)
        {
            long retVal = 0;
            try
            {
                retVal = GemCommManager?.SendTerminal(sendStr) ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = -1;
            }
            return retVal;
        }

        #endregion

        #region ==> Not Used.
        //public void DoUpdateGemVariables()
        //{
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_TEMP_REF_VALUE - 1025 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_BARCODE_LOTID  - 1102 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_PROBE_CARDID   - 1105 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_CASSETTE_MAP1  - 1150 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_STATION        - 1203 },       new string[] { "value" });

        //    //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_CHUCK_SLOTNO           - 1508 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_EVENT_WAFER_FLATANGLE  - 1522 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_DEVICE_NAME            - 30098 },      new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_OVERDRIVE              - 30198 },      new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { DVID_GROSS_DIE_CNT          - 30848 },      new string[] { "value" });
        //}

        //public void GEM_Init_SVID()
        //{
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_PROBE_CARDID   - 1105 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_PROBER_ID      - 1203 },       new string[] { "value" });
        //    //SecsGemClient.GEMSetVariable(1, new long[1] { SVID_STATION        - 1203 },       new string[] { "value" });
        //}
        #endregion

        public bool IsExcuteEvent(string eventname)
        {
            bool retVal = false;
            try
            {
                var eventRecipe = SubscribeRecipeParam.Find(param => param.EventFullName.Equals(eventname));
                if (eventRecipe != null)
                {
                    if (eventRecipe.Enable)
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool CanExecuteRemoteAction(string command)
        {
            bool retVal = false;

            try
            {
                retVal = GemRemoteActionRecipeDic.ContainsKey(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum ExcuteRemoteCommandAction(RemoteActReqData actReqData, object raiseobject)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IGemActBehavior action = null;
               
                GemRemoteActionRecipeDic.TryGetValue(actReqData.ActionType.ToString(), out action);

                if (action != null)
                {
                    if (raiseobject is GemExecutionProcessor.XGemExecutor)
                    {
                        action.ExcuteExcuter(actReqData);
                    }
                    else if (raiseobject is XGemCommandProcessor.XGemCommander)
                    {
                        action.ExcuteCommander(actReqData, (raiseobject as XGemCommandProcessor.XGemCommander).CommandHostService);
                    }
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], ExcuteRemoteCommandAction() : {actReqData.ActionType.ToString()} is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ExcuteCarrierCommandAction(CarrierActReqData actReqData, object raiseobject)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IGemActBehavior action = null;
                GemRemoteActionRecipeDic.TryGetValue(actReqData.ActionType.ToString(), out action);
                if (action != null)
                {
                    action.ExcuteCarrierCommander(actReqData, (raiseobject as XGemCommandProcessor.XGemCommander)?.CommandHostService);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public long SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = -1;
            try
            {
                if (nStream != 0 && nFunction != 0)
                {
                    retVal = GemCommManager.SendAck(pnObjectID, nStream, nFunction, nSysbyte, CAACK, nCount);
                }
                else
                {
                    LoggerManager.Debug($"GEM SendAck(). ObjectID : {pnObjectID}, Stream : {nStream}, Function : {nFunction}, Systembyte : {nSysbyte}, CAACK : {CAACK}.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public long S3F17SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount)
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager.S3F17SendAck(pnObjectID, nStream, nFunction, nSysbyte, CAACK, nCount);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        public long S3F27SendAck(long pnObjectID, long nStream, long nFunction, long nSysbyte, byte CAACK, long nCount, List<CarrierChangeAccessModeResult> result)
        {
            long retVal = -1;
            try
            {
                retVal = GemCommManager.S3F27SendAck(pnObjectID, nStream, nFunction, nSysbyte, CAACK, nCount, result);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public long MakeListObject(object list)
        {
            long ret = -1;
            try
            {
                ret = GemCommManager.MakeListObject(list);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        /// <summary>
        /// GemCommManager.SecsCommInformData의 Communication 파라미터를 GEM 동글에 적용합니다.
        /// </summary>
        public void CommunicationParamApply()
        {
            try
            {
                GemCommManager?.CommunicationParamApply();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetConnectState(int index = 0)
        {
            bool retVal = false;
            try
            {
                retVal = GemCommManager?.GetConnectState(index) ?? false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private Dictionary<int, bool> StageDownloadRecipeResultDic = new Dictionary<int, bool>();
        private object recipeResultlockObject = new object();
        public void ResetStageDownloadRecipe()
        {
            try
            {
                StageDownloadRecipeResultDic.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetComplateDownloadRecipe(List<int> stagelist)
        {
            bool retVal = false;
            try
            {
                foreach (var stage in stagelist)
                {
                    lock (recipeResultlockObject)
                    {
                        bool result = false;

                        if (StageDownloadRecipeResultDic.ContainsKey(stage) == true)
                        {
                            StageDownloadRecipeResultDic.TryGetValue(stage, out result);

                            if (result == true)
                            {
                                retVal = true;
                            }
                            else
                            {
                                retVal = false;
                                break;
                            }
                        }
                        else
                        {
                            retVal = false;
                            break;
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
        public void SetStageDownloadRecipeResult(int stageindex, bool flag)
        {
            try
            {
                lock (recipeResultlockObject)
                {
                    if (StageDownloadRecipeResultDic.ContainsKey(stageindex) == false)
                    {
                        StageDownloadRecipeResultDic.Add(stageindex, flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IPIVContainer GetPIVContainer()
        {
            return pIVContainer;
        }

        #endregion

        #region <remarks> IModule Implements                        </remarks>
        /// <summary>
        ///     IModule 구현부
        /// </summary>
        /// <remarks>
        ///     First Create : 2017-12-14, Semics R&D1, Jake Kim.
        /// </remarks>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            IParamManager paramManager = this.ParamManager();
            string thisComSerial = null;

            try
            {
                if (Initialized == false)
                {
                    IsDisposed = false;
                    pIVContainer = new PIVContainer();
                    retval = pIVContainer.Init();

                    Extensions_IParam.CollectCommonElement(pIVContainer, "GEM");

                    retval = LoadRemoteActionRecipeBehavior();
                    thisComSerial = SerialMaker.MakeSerialString();
                    LoggerManager.Debug($"[{this.GetType().Name}] Start Checking to GEM Serial number");

                    if (thisComSerial == GemSysParam.GEMSerialNum.Value)
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] The GEM Serial number is perfectly matched.");
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}] The GEM Serial number is not matched.");
                    }

                    GemCommManager = new XGemCommManager();
                    retval = GemCommManager.InitModule();
                    GemCommManager.SetRemoteActionRecipe(GemRemoteActionRecipeDic);
                    if (retval == EventCodeEnum.NONE)
                    {
                        this.ParamManager().OnLoadElementInfoFromDB -= ParamManager_OnLoadElementInfoFromDB;
                        this.ParamManager().OnLoadElementInfoFromDB += ParamManager_OnLoadElementInfoFromDB;
                        this.ParamManager().LoadComElementInfoFromDB(true);
                    }
                    else
                    {
                        LoggerManager.Error("Gem Can not regist event on load element info");
                    }


                    //GemAlarmBackupInfo.Initialize();
                    //}
                    //else
                    //{
                    //    LoggerManager.Debug($"[{this.GetType().Name}] The Serial number of GEM does not match.");
                    //    retval = EventCodeEnum.NONE;
                    //}

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void RegisteEvent_OnLoadElementInfo()
        {
            try
            {
                ParamManager_OnLoadElementInfoFromDB(null, null);
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }
        }


        //this.EventManager().RegisterEvent(typeof(ProbingZUpProcessEvent).FullName, "ProbeEventSubscibers", EventFired);
        //this.EventManager().RegisterEvent(typeof(WaferUnloadedEvent).FullName, "ProbeEventSubscibers", EventFired);
        //this.EventManager().RegisterEvent(typeof(MatchedToTestEvent).FullName, "ProbeEventSubscibers", EventFired);
        //private void EventFired(object sender, ProbeEventArgs e)
        //{
        //    try
        //    {
        //        var lockobj = this.StageSupervisor().GetStagePIV().GetPIVDataLockObject();
        //        lock (lockobj)
        //        {
        //            if (sender is ProbingZUpProcessEvent)
        //            {
        //                //this.StageSupervisor().GetStagePIV().SetStageState(GEMStageStateEnum.READY_TO_TEST);
        //                //SetEvent(GetEventNumberFormEventName(typeof(ProbingZUpFirstProcessEvent).FullName));
        //            }
        //            else if (sender is WaferUnloadedEvent)
        //            {
        //                //    this.StageSupervisor().GetStagePIV().SetStageState(GEMStageStateEnum.UNLOADING);
        //                //    SetEvent(GetEventNumberFormEventName(typeof(WaferEndEvent).FullName));
        //            }
        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}


        /// <summary>
        /// DB가 Load 된 후, ParamManager가 가지고 있는 
        /// Element들(GEM에서 사용하는 VID 대상으로)을 구독 하도록 합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParamManager_OnLoadElementInfoFromDB(object sender, EventArgs e)
        {
            try
            {
                GemCommManager.InitGemData();

                LoadSvidParameter();
                LoadDvidParameter();
                LoadEcidParameter();

                RegistNotifyEventToElement();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void RegistNotifyEventToElement(ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> DBElementDic)
        {
            IParamManager paramManager = this.ParamManager();            
            IEnumerable<IElement> Element = null;
            try
            {
                Element = FindSysElementUsingVID(DBElementDic);//paramManager.DevDBElementDictionary);
                RegistElementCollectionNotifyEventTo(Element);

                IEnumerable<IElement> FindSysElementUsingVID(ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> dbDictionary)
                {
                    return from dbElement in dbDictionary.Values
                           from element in dbElement.Values
                           where this.DicSVID.DicProberGemID.Value.ContainsKey(element.PropertyPath)
                               || this.DicDVID.DicProberGemID.Value.ContainsKey(element.PropertyPath)
                               || this.DicECID.DicProberGemID.Value.ContainsKey(element.PropertyPath)
                           select element;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// SVID, DVID, ECID의 Value Updating을 위하여 Element에 Event를 등록합니다.
        /// 1. ID를 이용하여 해당 Element를 찾는다.
        /// 2. Element의 RaisePropertyChanged에 GEM Function을 얹는다.
        /// </summary>
        public void RegistNotifyEventToElement()
        {
            try
            {
                IParamManager paramManager = this.ParamManager();
                //IEnumerable<IElement> sysElement = null;
                //IEnumerable<IElement> devElement = null;
                //IEnumerable<IElement> commonElement = null;
            

                //this.ParamManager().GetElementPath(2500);

                // 1. ID를 이용하여 해당 Element를 찾기.
                //sysElement = FindSysElementUsingVID(paramManager.SysDBElementDictionary);
                //devElement = FindSysElementUsingVID(paramManager.DevDBElementDictionary);
                //commonElement = FindSysElementUsingVID(paramManager.CommonDBElementDictionary);
                ////commonElement = paramManager.GetCommonElementList();
                //// 2. Element의 RaisePropertyChanged에 GEM Function을 얹는다.
                //RegistElementCollectionNotifyEventTo(sysElement);
                //RegistElementCollectionNotifyEventTo(devElement);
                //RegistElementCollectionNotifyEventTo(commonElement);

                RegistNotifyEventToElement(paramManager.SysDBElementDictionary);
                RegistNotifyEventToElement(paramManager.DevDBElementDictionary);
                RegistNotifyEventToElement(paramManager.CommonDBElementDictionary);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
       


        public void DeleteNotifyEventToElement()
        {
            try
            {
                IParamManager paramManager = this.ParamManager();
                IEnumerable<IElement> sysElement = null;
                IEnumerable<IElement> devElement = null;
                IEnumerable<IElement> commonElement = null;

                // 1. ID를 이용하여 해당 Element를 찾기.
                sysElement = FindSysElementUsingVID(paramManager.SysDBElementDictionary);
                devElement = FindSysElementUsingVID(paramManager.DevDBElementDictionary);
                commonElement = FindSysElementUsingVID(paramManager.CommonDBElementDictionary);
                //commonElement = paramManager.GetCommonElementList();
                // 2. Element의 RaisePropertyChanged에 GEM Function을 얹는다.
                DeleteElementCollectionNotifyEventTo(sysElement);
                DeleteElementCollectionNotifyEventTo(devElement);
                DeleteElementCollectionNotifyEventTo(commonElement);

                //dbDictionary에 svid, dvid, ecid에 해당하는 element들이 있는지 검색하여
                //결과값을 반환합니다.
                IEnumerable<IElement> FindSysElementUsingVID(ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> dbDictionary)
                {
                    return from dbElement in dbDictionary.Values
                           from element in dbElement.Values
                           where this.DicSVID.DicProberGemID.Value.ContainsKey(element.PropertyPath)
                               || this.DicDVID.DicProberGemID.Value.ContainsKey(element.PropertyPath)
                               || this.DicECID.DicProberGemID.Value.ContainsKey(element.PropertyPath)
                           select element;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// ElementCollection에 있는 Element의 PropertyChanged를 구독합니다.
        /// </summary>
        /// <param name="elementCollection"></param>
        public void RegistElementCollectionNotifyEventTo(IEnumerable<IElement> elementCollection)
        {
            try
            {
                if (elementCollection != null)
                {
                    foreach (var element in elementCollection)
                    {
                        if (element != null)
                        {
                            try
                            {
                                //Exist Check in Svid Dictionary
                                GemVidInfo gemvidInfo = FindVidInfo(DicSVID?.DicProberGemID?.Value, element.PropertyPath);
                                if (gemvidInfo == null)
                                {
                                    //Exist Check in Dvid Dictionary
                                    gemvidInfo = FindVidInfo(DicDVID?.DicProberGemID?.Value, element.PropertyPath);
                                    if (gemvidInfo == null)
                                    {
                                        //Exist Check in Ecid Dictionary
                                        gemvidInfo = FindVidInfo(DicECID?.DicProberGemID?.Value, element.PropertyPath);                                     
                                    }
                                }

                                if (gemvidInfo != null)
                                {
                                    //SysExcuteMode 가 Remote 라면 LoaderSystem,
                                    //SysExcuteMode 가 Prober 라면 ProberSystem
                                    if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                                    {
                                        if (gemvidInfo.ProcessorType == VidUpdateTypeEnum.COMMANDER)
                                            continue;

                                        if (gemvidInfo.VidPropertyType == VidPropertyTypeEnum.LIST)
                                        {
                                            // element 가 List 의 하나의 value 인것.
                                            // list 전체를 가져와서 index 비교.
                                            int cellindex = pIVContainer.StageNumber.Value;
                                            //bool findProperty = false;

                                            element.VID = (int)gemvidInfo.VID;
                                            if(element.PropertyPath.Contains($"[{cellindex-1}]") == false)
                                            {
                                                continue;
                                            }


                                            //var ExistProperty = pIVContainer.StageSVTemp.SingleOrDefault(property => property.VID == element.VID);

                                            //if (ExistProperty != null)
                                            //{
                                            //    findProperty = true;
                                            //    if (pIVContainer.StageSVTemp[cellindex - 1].VID != element.VID)
                                            //        continue;
                                            //    else
                                            //    {
                                            //        element.VID = (int)gemvidInfo.VID;
                                            //    }
                                            //}

                                            //if (findProperty == false)
                                            //{
                                            //    ExistProperty = pIVContainer.StageProberCardIDs.SingleOrDefault(property => property.VID == element.VID);
                                            //    if (ExistProperty != null)
                                            //    {
                                            //        findProperty = true;
                                            //        if (pIVContainer.StageProberCardIDs[cellindex - 1].VID != element.VID)
                                            //            continue;
                                            //        else
                                            //        {
                                            //            element.VID = (int)gemvidInfo.VID;
                                            //        }
                                            //    }
                                            //}

                                            //if (findProperty == false)
                                            //{
                                            //    ExistProperty = pIVContainer.StageRecipeNames.SingleOrDefault(property => property.VID == element.VID);
                                            //    if (ExistProperty != null)
                                            //    {
                                            //        findProperty = true;
                                            //        if (pIVContainer.StageRecipeNames[cellindex - 1].VID != element.VID)
                                            //            continue;
                                            //        else
                                            //        {
                                            //            element.VID = (int)gemvidInfo.VID;
                                            //        }
                                            //    }
                                            //}

                                            //if (findProperty == false)
                                            //{
                                            //    var sExistProperty = pIVContainer.PreHeatStates.SingleOrDefault(property => property.VID == element.VID);
                                            //    if (sExistProperty != null)
                                            //    {
                                            //        findProperty = true;
                                            //        if (pIVContainer.PreHeatStates[cellindex - 1].VID != element.VID)
                                            //            continue;
                                            //        else
                                            //        {
                                            //            element.VID = (int)gemvidInfo.VID;
                                            //        }
                                            //    }
                                            //}

                                            //if (findProperty == false)
                                            //{
                                            //    var sExistProperty = pIVContainer.StageStates.SingleOrDefault(property => property.VID == element.VID);
                                            //    if (sExistProperty != null)
                                            //    {
                                            //        findProperty = true;
                                            //        if (pIVContainer.StageStates[cellindex - 1].VID != element.VID)
                                            //            continue;
                                            //        else
                                            //        {
                                            //            element.VID = (int)gemvidInfo.VID;
                                            //        }
                                            //    }
                                            //}
                                        }
                                        if (gemvidInfo.VidPropertyType == VidPropertyTypeEnum.CLIST)
                                        {
                                            int cellindex = pIVContainer.StageNumber.Value;
                                            element.VID = (int)(gemvidInfo.VID + cellindex - 1);
                                        }
                                        else
                                        {
                                            element.VID = (int)gemvidInfo.VID ;
                                        }
                                    }
                                    else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                                    {
                                        if (gemvidInfo.ProcessorType == VidUpdateTypeEnum.SINGLE
                                            || gemvidInfo.ProcessorType == VidUpdateTypeEnum.CELL)
                                            continue;
                                        else
                                        {
                                            element.VID = (int)gemvidInfo.VID;
                                        }
                                    }
                                    element.GEMImmediatelyUpdate = gemvidInfo.GEMImmediatelyUpdate;
                                    element.RaisePropertyChangedFalg = gemvidInfo.GEMImmediatelyUpdate;
                                    element.GEMEnable = gemvidInfo.Enable;
                                  

                                    RegistElementNotifyEventTo(element);
                                    if (element.GetValue() != null)
                                        NotifyValueChanged(element.ElementID, element.GetValue(), element);
                                    else
                                        NotifyValueChanged(element.ElementID, null, element);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        /// <summary>
        /// Element의 PropertyChanged을 구독합니다.
        /// </summary>
        /// <param name="element"></param>
        private void RegistElementNotifyEventTo(IElement element)

        {
            if (element != null)
            {
                element.PropertyChanged -= OnElementValueChanged;
                element.PropertyChanged += OnElementValueChanged;

                LoggerManager.Debug($"[GEM]OnElementValueChanged add Element - Element Path : {element.PropertyPath}, Element ID : {element.ElementID}, Element VID : {element.VID}");
            }
        }


        private void DeleteElementCollectionNotifyEventTo(IEnumerable<IElement> elementCollection)
        {
            try
            {
                if (elementCollection != null)
                {
                    foreach (var element in elementCollection)
                    {
                        if (element != null)
                        {
                            try
                            {
                                //Exist Check in Svid Dictionary
                                GemVidInfo gemvidInfo = FindVidInfo(DicSVID?.DicProberGemID?.Value, element.PropertyPath);
                                if (gemvidInfo == null)
                                {
                                    //Exist Check in Dvid Dictionary
                                    gemvidInfo = FindVidInfo(DicDVID?.DicProberGemID?.Value, element.PropertyPath);
                                    if (gemvidInfo == null)
                                    {
                                        //Exist Check in Ecid Dictionary
                                        gemvidInfo = FindVidInfo(DicECID?.DicProberGemID?.Value, element.PropertyPath);
                                    }
                                }

                                if (gemvidInfo != null)
                                {
                                    //SysExcuteMode 가 Remote 라면 LoaderSystem,
                                    //SysExcuteMode 가 Prober 라면 ProberSystem
                                    if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                                    {
                                        if (gemvidInfo.ProcessorType == VidUpdateTypeEnum.COMMANDER)
                                            continue;

                                        if (gemvidInfo.VidPropertyType == VidPropertyTypeEnum.LIST)
                                        {
                                            // element 가 List 의 하나의 value 인것.
                                            // list 전체를 가져와서 index 비교.
                                            int cellindex = pIVContainer.StageNumber.Value;
                                            bool findProperty = false;
                                            var ExistProperty = pIVContainer.StageSVTemp.SingleOrDefault(property => property.VID == element.VID);
                                            if (ExistProperty != null)
                                            {
                                                findProperty = true;
                                                if (pIVContainer.StageSVTemp[cellindex - 1].VID != element.VID)
                                                    continue;
                                            }
                                            if (findProperty == false)
                                            {
                                                ExistProperty = pIVContainer.StagePVTemp.SingleOrDefault(property => property.VID == element.VID);
                                                if (ExistProperty != null)
                                                {
                                                    findProperty = true;
                                                    if (pIVContainer.StagePVTemp[cellindex - 1].VID != element.VID)
                                                        continue;
                                                }
                                            }
                                            if (findProperty == false)
                                            {
                                                ExistProperty = pIVContainer.StageProberCardIDs.SingleOrDefault(property => property.VID == element.VID);
                                                if (ExistProperty != null)
                                                {
                                                    findProperty = true;
                                                    if (pIVContainer.StageProberCardIDs[cellindex - 1].VID != element.VID)
                                                        continue;
                                                }
                                            }
                                            if (findProperty == false)
                                            {
                                                ExistProperty = pIVContainer.StageSVTemp.SingleOrDefault(property => property.VID == element.VID);
                                                if (ExistProperty != null)
                                                {
                                                    findProperty = true;
                                                    if (pIVContainer.StageSVTemp[cellindex - 1].VID != element.VID)
                                                        continue;
                                                }
                                            }
                                            if (findProperty == false)
                                            {
                                                ExistProperty = pIVContainer.StageRecipeNames.SingleOrDefault(property => property.VID == element.VID);
                                                if (ExistProperty != null)
                                                {
                                                    findProperty = true;
                                                    if (pIVContainer.StageRecipeNames[cellindex - 1].VID != element.VID)
                                                        continue;
                                                }
                                            }
                                            if (findProperty == false)
                                            {
                                                var sExistProperty = pIVContainer.PreHeatStates.SingleOrDefault(property => property.VID == element.VID);
                                                if (sExistProperty != null)
                                                {
                                                    findProperty = true;
                                                    if (pIVContainer.PreHeatStates[cellindex - 1].VID != element.VID)
                                                        continue;
                                                }
                                            }
                                            if (findProperty == false)
                                            {
                                                var sExistProperty = pIVContainer.StageStates.SingleOrDefault(property => property.VID == element.VID);
                                                if (sExistProperty != null)
                                                {
                                                    findProperty = true;
                                                    if (pIVContainer.StageStates[cellindex - 1].VID != element.VID)
                                                        continue;
                                                }
                                            }

                                            //string path = this.ParamManager().GetElementPath(element.ElementID);
                                            //if(path != null)
                                            //{

                                            //}

                                            //var pIVContainerProperties = pIVContainer.GetType().GetProperties();
                                            //foreach (var property in pIVContainerProperties)
                                            //{

                                            //}
                                        }
                                        if (gemvidInfo.VidPropertyType == VidPropertyTypeEnum.CLIST)
                                        {
                                            int cellindex = pIVContainer.StageNumber.Value;
                                            element.VID = (int)(gemvidInfo.VID + cellindex - 1);
                                        }
                                    }
                                    else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                                    {
                                        if (gemvidInfo.ProcessorType == VidUpdateTypeEnum.SINGLE
                                            || gemvidInfo.ProcessorType == VidUpdateTypeEnum.CELL)
                                            continue;
                                    }

                                    DisconnectElementnorifyEventTo(element);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DisconnectElementnorifyEventTo(IElement element)
        {
            try
            {
                if (element != null)
                {
                    element.PropertyChanged -= OnElementValueChanged;
                    LoggerManager.Debug($"[GEM]OnElementValueChanged delete Element - Element Path : {element.PropertyPath}, Element ID : {element.ElementID}, Element VID : {element.VID}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnElementValueChanged(object sender, PropertyChangedEventArgs e)
        {   
            try
            {
                var element = sender as IElement;
                if (element != null)
                {
                    NotifyValueChanged(element.ElementID, element.GetValue(), element);                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// DeInit 할 때의 처리를 담당합니다.
        /// </summary>
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

                Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// SV, DV, ECV 값을 Gem에 업데이트 합니다.
        /// </summary>
        /// <param name="proberValueID"></param>
        /// <param name="value"></param>
        public void NotifyValueChanged(long proberValueID, object value, IElement element)
        {
            try
            {
                SetSvUsingProberID(element, value);
                SetDvUsingProberID(element, value);
                SetEcvUsingProberID(element, value);
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }
        }

        public void SetValue(IElement element)
        {
            try
            {
                SetSvUsingProberID(element, element.GetValue());
                SetDvUsingProberID(element, element.GetValue());
                SetEcvUsingProberID(element, element.GetValue());
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }
        }

        public string GetGEMSerialNum()
        {
            return GemSysParam?.GEMSerialNum.Value ?? "";
        }

        /// <summary>
        /// Prober의 ID를 이용하여 GEM ID를 찾아 SV Value를 GEM 동글에 Set합니다.
        /// </summary>
        /// <param name="proberID"></param>
        /// <param name="value"></param>
        //private void SetSvUsingProberID(long proberID, string value, long vid = -1)
        private void SetSvUsingProberID(IElement element, object value, long vid = -1)
        {
            try
            {
                GemVidInfo gemSvidInfo = FindVidInfo(DicSVID?.DicProberGemID?.Value, element.PropertyPath);              
                if (gemSvidInfo != null)
                {
                    if (value is IList)
                    {
                        var sendDataList = GetSendListData(element, value, gemSvidInfo.Converter);
                        long objectid = GemCommManager.MakeListObject(sendDataList);
                        GemCommManager.SetVariables(objectid, element.VID, EnumVidType.SVID, immediatelyUpdate: element.GEMImmediatelyUpdate);
                    }
                    else
                    {
                        string sendData = GetSendData(element, value, gemSvidInfo.Converter).ToString();
                        GemCommManager.SetVariable(new long[] { element.VID }, new string[] { sendData }, EnumVidType.SVID, immediatelyUpdate: element.GEMImmediatelyUpdate);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }
        }
        //public bool IsIEnumerableOfT(Type type)
        //{
        //    return type.GetInterfaces().Any(x => x.IsGenericType
        //           && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        //}


        /// <summary>
        /// Prober의 ID를 이용하여 GEM ID를 찾아 DV Value를 GEM 동글에 Set합니다.
        /// </summary>
        /// <param name="proberID"></param>
        /// <param name="value"></param>
        //private void SetDvUsingProberID(long proberID, string value, long vid = -1)
        private void SetDvUsingProberID(IElement element, object value, long vid = -1)
        {
            try
            {
                GemVidInfo gemDvidInfo = FindVidInfo(DicDVID?.DicProberGemID?.Value, element.PropertyPath);
                       
                if (gemDvidInfo != null)
                {
                    if (value is IList)
                    {                      
                        var sendDataList = GetSendListData(element, value, gemDvidInfo.Converter);
                        long objectid = GemCommManager.MakeListObject(sendDataList);
                        GemCommManager.SetVariables(objectid, element.VID, EnumVidType.DVID, immediatelyUpdate: element.GEMImmediatelyUpdate);
                    }
                    else
                    {
                        string sendData = GetSendData(element, value, gemDvidInfo.Converter).ToString();
                        GemCommManager.SetVariable(new long[] { element.VID }, new string[] { sendData }, EnumVidType.DVID, immediatelyUpdate: element.GEMImmediatelyUpdate);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// Prober의 ID를 이용하여 GEM ID를 찾아 EC Value를 GEM 동글에 Set합니다.
        /// </summary>
        /// <param name="proberID"></param>
        /// <param name="value"></param>
        private void SetEcvUsingProberID(IElement element, object value, int vid = -1)
        {
            try
            {
                GemVidInfo gemEcidInfo = FindVidInfo(DicECID?.DicProberGemID?.Value, element.PropertyPath);

                if (gemEcidInfo != null)
                {
                    if (value is IList)
                    {
                        var sendDataList = GetSendListData(element, value, gemEcidInfo.Converter);
                        long objectid = GemCommManager.MakeListObject(sendDataList);
                        GemCommManager.SetVariables(objectid, element.VID, EnumVidType.ECID, immediatelyUpdate: element.GEMImmediatelyUpdate);
                    }
                    else
                    {
                        string sendData = GetSendData(element, value, gemEcidInfo.Converter).ToString();
                        //GemCommManager.SetVariable(new long[] { element.VID }, new string[] { sendData }, EnumVidType.ECID, immediatelyUpdate: element.GEMImmediatelyUpdate);
                        this.GEMModule()?.GemCommManager.GEMSetECVChanged(1, new long[] { element.VID }, new string[] { sendData.ToString() });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }
        }


        private object GetSendData(IElement element, object value, IDataConverter converter = null)
        {
            object convertVal = value;
            try
            {

                if (converter != null)
                {
                    convertVal = converter.Convert(value); // 이거 진짜 적용해도 되는것인가? PIVContainer 실제 오브젝트의 값이 바뀌어 버릴것 같음..
                }

                //약속된 값들에 대한 고정된 컨버팀
                if (convertVal == null)
                {
                    convertVal = "";
                }
                else if (convertVal.ToString() == "-9999")
                {
                    convertVal = "";
                }
                else if (convertVal is ProberInterfaces.PMI.OP_MODE)
                {
                    ProberInterfaces.PMI.OP_MODE valConv = (ProberInterfaces.PMI.OP_MODE)convertVal;
                    if (valConv == ProberInterfaces.PMI.OP_MODE.Disable)
                        convertVal = 0;
                    else if (valConv == ProberInterfaces.PMI.OP_MODE.Enable)
                        convertVal = 1;
                }
                else if (convertVal is bool)
                {
                    bool valConv = bool.Parse(convertVal.ToString());
                    if (valConv == false)
                        convertVal = 0;
                    else
                        convertVal = 1;
                }
                else
                {
                    convertVal = value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //ret = new string[1] {""};
            }
            return convertVal;
        }

      
        public object GetSendListData(IElement element, object list, IDataConverter converter = null)
        {
            object ret = null;
            try
            {
                if (list is IList)
                {
                    IList tolist = list as IList;                                        
                    for (int i = 0; i < tolist.Count; i++)
                    {
                        if (tolist[i] is IList)
                        {
                            tolist[i] = GetSendListData(element, (object)tolist[i], converter);
                        }
                        else
                        {
                            tolist[i] = GetSendData(element, (object)tolist[i], converter); 
                        }
                    }
                    list = tolist;
                }
                ret = list;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }


        /// <summary>
        /// Prober ID와 GEM ID가 맵핑이 된 Dictionary에서 GEM ID를 검색합니다.
        /// </summary>
        /// <param name="vidDic"></param>
        /// <param name="proberValueID"></param>
        /// <returns></returns>
        private GemVidInfo FindVidInfo(IDictionary<string, GemVidInfo> vidDic, string propertypath)
        {
            GemVidInfo foundInfo = null;

            try
            {
                if (vidDic != null)
                {
                    bool isFindData = false;
                    isFindData = vidDic.TryGetValue(propertypath, out foundInfo);

                    if (isFindData != true)
                    {
                        foundInfo = null;
                    }
                }
            }
            catch (Exception err)
            {
                foundInfo = null;
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }

            return foundInfo;
        }

        /// <summary>
        /// Overload: GemVidInfo FindVidInfo(IDictionary<string, GemVidInfo> vidDic, string propertypath)
        /// </summary>
        /// <param name="vidDic"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        public (string key, GemVidInfo val) FindVidInfo(IDictionary<string, GemVidInfo> vidDic, long vid)
        {
            (string, GemVidInfo) foundInfo = (null, null);

            try
            {
                if (vidDic != null)
                {                    
                    var tempinfo = vidDic.Where(v => v.Value.VID == (int)vid).FirstOrDefault(); //.TryGetValue(propertypath, out foundInfo);
                    foundInfo.Item1 = tempinfo.Key;
                    foundInfo.Item2 = tempinfo.Value;
                }
            }
            catch (Exception err)
            {
                foundInfo = (null, null);
                LoggerManager.Debug(err.Message);
                LoggerManager.Exception(err);
            }

            return foundInfo;
        }


        public bool CheckVIDGemEnable(string path)
        {
            bool retVal = true;
            try
            {
                GemVidInfo gemDvidInfo = FindVidInfo(DicSVID?.DicProberGemID?.Value, path);
                if (gemDvidInfo == null)
                {
                    gemDvidInfo = FindVidInfo(DicDVID?.DicProberGemID?.Value, path);
                    if (gemDvidInfo == null)
                    {
                        gemDvidInfo = FindVidInfo(DicECID?.DicProberGemID?.Value, path);
                    }
                }

                if (gemDvidInfo != null)
                {
                    retVal = gemDvidInfo.Enable;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        #endregion

        #region <remarks> Gem State Define Param  </remarks>
        public int GetStageStateEnumValue(GEMStageStateEnum stagenum)
        {
            int retVal = -1;
            try
            {
                if (GemStateDefineParam != null)
                {
                    var defineParam = GemStateDefineParam.StageStateDefineParam.Find(param => param.StageStateEnum == stagenum);
                    if (defineParam != null)
                    {
                        retVal = defineParam.Number;
                    }
                }
                else
                {
                    LoggerManager.Debug("GemStateDefineParam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetFoupStateEnumValue(GEMFoupStateEnum foupenum)
        {
            int retVal = -1;
            try
            {
                if (GemStateDefineParam != null)
                {
                    var defineParam = GemStateDefineParam.FoupStateDefineParam.Find(param => param.FoupStateEnum == foupenum);
                    if (defineParam != null)
                    {
                        retVal = defineParam.Number;
                    }
                }
                else
                {
                    LoggerManager.Debug("GemStateDefineParam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetCardLPStateEnumValue(GEMFoupStateEnum lpstateenum)
        {
            int retVal = -1;
            try
            {
                if (GemStateDefineParam != null)
                {
                    var defineParam = GemStateDefineParam.CardLPStateDefineParam.Find(param => param.FoupStateEnum == lpstateenum);
                    if (defineParam != null)
                    {
                        retVal = defineParam.Number;
                    }
                }
                else
                {
                    LoggerManager.Debug("GemStateDefineParam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public GEMFoupStateEnum GetfoupStatEnumType(int number)
        {
            GEMFoupStateEnum stateEnum = GEMFoupStateEnum.UNDIFIND;
            try
            {
                if (GemStateDefineParam != null)
                {
                    var defineParam = GemStateDefineParam.FoupStateDefineParam.Find(param => param.Number == number);
                    if (defineParam != null)
                    {
                        stateEnum = defineParam.FoupStateEnum;
                    }
                }
                else
                {
                    LoggerManager.Debug("GemStateDefineParam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stateEnum;
        }

        public int GetPreHeatStateEnumValue(GEMPreHeatStateEnum preheatenum)
        {
            int retVal = -1;
            try
            {
                if (GemStateDefineParam != null)
                {
                    var defineParam = GemStateDefineParam.PreHeatStateDefineParam.Find(param => param.PreHeatStateEnum == preheatenum);
                    if (defineParam != null)
                    {
                        retVal = defineParam.Number;
                    }
                }
                else
                {
                    LoggerManager.Debug("GemStateDefineParam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public int GetSmokeSensorStateEnumValue(GEMSensorStatusEnum sensorStatusEnum)
        {
            int retVal = -1;
            try
            {
                if (GemStateDefineParam != null)
                {
                    var defineParam = GemStateDefineParam.SmokeSensorStateDefineParam.Find(param => param.SensorStatusEnum == sensorStatusEnum);
                    if (defineParam != null)
                    {
                        retVal = defineParam.Number;
                    }
                }
                else
                {
                    LoggerManager.Debug("GemStateDefineParam is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);                
            }
            return retVal;
        }

        //public EventCodeEnum IsControlAvailableState(out string errorlog)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.INVALID_ACCESS;
        //    string errormsg = "";
        //    if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
        //    {
        //        if(this.StageSupervisor().GetStageMode().Item1 == GPCellModeEnum.ONLINE)
        //        {
        //            retVal = EventCodeEnum.NONE;
        //        }
        //        else
        //        {
        //            errormsg += $"Cannot remote controllable state.";
        //        }
        //    }
        //    errorlog += errormsg;
        //    return retVal;
        //}
        #endregion


        #region <remarks> AttachedModule VID Upgate

        public EventCodeEnum SetFixedTrayVID(ModuleID moduleID, IWaferHolder holder)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                int index = moduleID.Index;
                double touchCount = -1;
                string fixedid = null;
                if (holder.Status == EnumSubsStatus.EXIST && holder.TransferObject != null && holder.TransferObject.PolishWaferInfo != null)
                {
                    touchCount = holder.TransferObject.PolishWaferInfo.TouchCount.Value;
                    fixedid = holder.TransferObject.OCR.Value;
                    ret = EventCodeEnum.NONE;
                }
                else if (holder.Status == EnumSubsStatus.NOT_EXIST || holder.TransferObject != null && holder.TransferObject.PolishWaferInfo == null)
                {
                    touchCount = 0;
                    fixedid = "";
                    ret = EventCodeEnum.NONE;
                }
                else
                {
                    // 이전상태 유지가 컨셉
                }

                if (ret == EventCodeEnum.NONE)
                {
                    this.GetPIVContainer().SetFixedTrayInfo(index, fixedid, touchCount);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        #endregion
    }
}
