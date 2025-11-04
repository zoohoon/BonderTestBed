using Autofac;
using GPLoaderRouter;
using LoaderBase;
using LoaderParameters;
using LoaderParameters.Data;
using LoaderServiceBase;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace LoaderCore.LoaderService
{

    /// <summary>
    /// ADS(TwinCAT)기반 원격 로더 시스템 연결을 위한 로더 서비스 입니다.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class RemoteLoaderService : ILoaderService, IFactoryModule
    {
        public Autofac.IContainer cont;
        private ILoaderServiceCallback callback;

        private ILoaderModule Loader => cont.Resolve<ILoaderModule>();
        //private ILoaderModule Loader { get { return this.LoaderOPModule(); } }

        public IGPLoader GPLoader
        {
            get { return cont.Resolve<IGPLoader>(); }
        }

        public EventCodeEnum AbortRequest()
        {

            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum AwakeProcessModule()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ClearRequestData()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum Connect()
        {
            //var loader = container.Resolve<ILoaderModule>();

            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            errorCode = GPLoader.Connect();
            Loader.SetLoaderContainer(cont);
            Loader.Initialize(LoaderServiceTypeEnum.REMOTE, "C:\\ProberSystem\\LoaderSystem\\EMUL\\Parameters");

            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }
        object lockObj = new object();
        /// <summary>
        /// foupIndex 를 -1 로 사용한다는 의미
        ///     1.Card 관련 포지션을 업데이트 한다.
        ///     2. 호밍관련 포지션을 업데이트 한다.
        ///     3. etc 관련 정보를 업데이트 한다.
        ///Wafer 관련 정보 업데이트 시에는 foupIndex 를 넘길때 실수 하면 Cassette type 혼동으로 PLC Loader Pick Put 동작 포지션에 영향이 갈수있음 ( wafer 깨먹을 수 있으니 주의 )
        /// </summary>
        /// <param name="foupIndex"></param>
        /// <returns></returns>
        public EventCodeEnum UpdateLoaderSystem(int foupIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            lock (lockObj)
            {
                try
                {
                    var slotSize = Loader.DeviceSize;
                    if (slotSize == SubstrateSizeEnum.UNDEFINED)
                    {
                        slotSize = Loader.GetDefaultWaferSize();
                        LoggerManager.Debug($"UpdateLoaderSystem(). Wafer size is set to default wafer size {slotSize}");
                    }
                    LoggerManager.Debug($"UpdateLoaderSystem(). Wafer Size {slotSize}");
                    bool shouldCheckCassetteType = false;
                    CassetteTypeEnum cassetteTypeEnum = CassetteTypeEnum.FOUP_25;
                    if (foupIndex > 0)
                    {
                        cassetteTypeEnum = Loader.GetCassetteType(foupIndex);
                        if (cassetteTypeEnum != CassetteTypeEnum.UNDEFINED)
                        {
                            shouldCheckCassetteType = true;
                        }
                        else 
                        {
                            cassetteTypeEnum = CassetteTypeEnum.FOUP_25;
                            LoggerManager.Debug($"UpdateLoaderSystem(). cassette Type is set to default wafer size {slotSize}");
                        }
                        LoggerManager.Debug($"UpdateLoaderSystem(). Wafer Size {slotSize} foup Index {foupIndex} cassetteTypeEnum {cassetteTypeEnum}");
                    }

                    for (int i = 0; i < Loader.SystemParameter.CassetteModules.Count; i++) //Slot
                    {
                        var accessPos = Loader.SystemParameter.CassetteModules[i].Slot1AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                        && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                        if (accessPos != null)
                        {
                            GPLoader.CSTAccParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                            GPLoader.CSTAccParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                            GPLoader.CSTAccParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                            GPLoader.CSTAccParams[i].nLU_Pos = (int)accessPos.Position.U.Value;
                            GPLoader.CSTAccParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                        }
                    }

                    for (int i = 0; i < Loader.SystemParameter.FixedTrayModules.Count; i++) //Slot
                    {
                        var accessPos = Loader.SystemParameter.FixedTrayModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize);
                        if (accessPos != null)
                        {
                            GPLoader.FixTrayAccParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                            GPLoader.FixTrayAccParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                            GPLoader.FixTrayAccParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                            GPLoader.FixTrayAccParams[i].nLU_Pos = (int)accessPos.Position.U.Value;
                            GPLoader.FixTrayAccParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                        }
                    }

                    //CSTDRWParams
                    for (int i = 0; i < Loader.SystemParameter.InspectionTrayModules.Count; i++) //Slot
                    {
                        var accessPos = Loader.SystemParameter.InspectionTrayModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                         && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                        if (accessPos != null)
                        {
                            GPLoader.CSTDRWParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                            GPLoader.CSTDRWParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                            GPLoader.CSTDRWParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                            GPLoader.CSTDRWParams[i].nLU_Pos = (int)accessPos.Position.U.Value;
                            GPLoader.CSTDRWParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                        }
                    }
                    
                    int trayIndexOffset = 0;
                    if (Loader.SystemParameter.CardTrayIndexOffset.Value > 0)
                    {
                        trayIndexOffset = Loader.SystemParameter.CardTrayIndexOffset.Value;
                    }
                    else if (Loader.SystemParameter.CardTrayIndexOffset.Value == 0)
                    {
                        if (SystemManager.SystemType == SystemTypeEnum.Opera)
                        {
                            Loader.SystemParameter.CardTrayIndexOffset.Value = 4;
                        }
                        else
                        {
                            Loader.SystemParameter.CardTrayIndexOffset.Value = Loader.SystemParameter.CardBufferModules.Count;
                        }
                    }

                    if (GPLoader.CardBufferAccParams.Count< trayIndexOffset+ Loader.SystemParameter.CardBufferTrayModules.Count)
                    {
                        int count = trayIndexOffset + Loader.SystemParameter.CardBufferTrayModules.Count - GPLoader.CardBufferAccParams.Count;
                        for(int i=0;i< count;i++)
                        {
                            GPLoader.CardBufferAccParams.Add(new stAccessParam());
                        }
                    }
                    if (Loader.SystemParameter.CardBufferModules.Count > 0)
                    {
                        for (int i = 0; i < Loader.SystemParameter.CardBufferModules.Count; i++)
                        {
                            var accessPos = Loader.SystemParameter.CardBufferModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize);
                            if (accessPos != null)
                            {
                                GPLoader.CardBufferAccParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                                GPLoader.CardBufferAccParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                                GPLoader.CardBufferAccParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                                GPLoader.CardBufferAccParams[i].nLU_Pos = (int)0;
                                GPLoader.CardBufferAccParams[i].nLUC_Pos = (int)accessPos.Position.U.Value;
                                GPLoader.CardBufferAccParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                            }
                        }
                    }

                    if (Loader.SystemParameter.CardBufferTrayModules.Count > 0)
                    {
                      

                        for (int i = 0; i < Loader.SystemParameter.CardBufferTrayModules.Count; i++) //CardTray
                        {
                            var accessPos = Loader.SystemParameter.CardBufferTrayModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize);
                            if (accessPos != null)
                            {
                                GPLoader.CardBufferAccParams[i + trayIndexOffset].nLX_Pos = (int)accessPos.Position.LX.Value;
                                GPLoader.CardBufferAccParams[i + trayIndexOffset].nLZ_Pos = (int)accessPos.Position.A.Value;
                                GPLoader.CardBufferAccParams[i + trayIndexOffset].nLW_Pos = (int)accessPos.Position.W.Value;
                                GPLoader.CardBufferAccParams[i + trayIndexOffset].nLU_Pos = (int)0;
                                GPLoader.CardBufferAccParams[i + trayIndexOffset].nLUC_Pos = (int)accessPos.Position.U.Value;
                                GPLoader.CardBufferAccParams[i + trayIndexOffset].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                            }
                        }
                    }


                    for (int i = 0; i < Loader.SystemParameter.CCModules.Count; i++) //Card
                    {
                        var accessPos = Loader.SystemParameter.CCModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize);
                        if (accessPos != null)
                        {
                            GPLoader.CardAccParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                            GPLoader.CardAccParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                            GPLoader.CardAccParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                            GPLoader.CardAccParams[i].nLU_Pos = (int)0;
                            GPLoader.CardAccParams[i].nLUC_Pos = (int)accessPos.Position.U.Value;
                            GPLoader.CardAccParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                        }
                    }

                    for (int i = 0; i < Loader.SystemParameter.PreAlignModules.Count; i++) //PreAlign
                    {
                        var accessPos = Loader.SystemParameter.PreAlignModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                        && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                        if (accessPos != null)
                        {
                            GPLoader.PAAccParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                            GPLoader.PAAccParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                            GPLoader.PAAccParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                            GPLoader.PAAccParams[i].nLU_Pos = (int)accessPos.Position.U.Value;
                            GPLoader.PAAccParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                        }
                    }


                    for (int i = 0; i < Loader.SystemParameter.ChuckModules.Count; i++) //ChuckModule
                    {
                        var accessPos = Loader.SystemParameter.ChuckModules[i].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                         && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                        if (accessPos != null)
                        {
                            GPLoader.ChuckAccParams[i].nLX_Pos = (int)accessPos.Position.LX.Value;
                            GPLoader.ChuckAccParams[i].nLZ_Pos = (int)accessPos.Position.A.Value;
                            GPLoader.ChuckAccParams[i].nLW_Pos = (int)accessPos.Position.W.Value;
                            GPLoader.ChuckAccParams[i].nLU_Pos = (int)accessPos.Position.U.Value;
                            GPLoader.ChuckAccParams[i].nLZ_PickOffset = (int)accessPos.PickupIncrement.Value;
                        }
                    }

                    if (Loader.SystemParameter.BufferModules.Count > 0)
                    {
                        var bufferPos = Loader.SystemParameter.BufferModules[0].AccessParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                         && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                        if (bufferPos != null)
                        {
                            GPLoader.GPLoaderParam.nLT_Gap = (int)bufferPos.Position.A.Value;
                            GPLoader.GPLoaderParam.nLT_LT_PickOffset = (int)bufferPos.PickupIncrement.Value;
                            GPLoader.GPLoaderParam.nLT_StartPos = (int)bufferPos.Position.BT.Value;
                            GPLoader.GPLoaderParam.nLT_LWPos = (int)bufferPos.Position.W.Value;
                            GPLoader.GPLoaderParam.nLT_LUC_Back_Pos = (int)bufferPos.Position.U.Value; //temp param
                            GPLoader.GPLoaderParam.nLT_LUC_For_Pos = (int)bufferPos.Position.LX.Value; ; //temp Param
                        }
                    }

                    var lt = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LB);
                    if (lt != null)
                    {
                        GPLoader.GPLoaderParam.nLT_HomeImpulsVelo = (int)lt.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLT_HomePos = (int)lt.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLT_HomeOffset = (int)lt.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLT_HomeVelo = (int)lt.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLT_Velo = (int)lt.Param.Speed.Value;
                    }

                    var lx = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LX);
                    if (lx != null)
                    {
                        GPLoader.GPLoaderParam.nLX_HomeImpulsVelo = (int)lx.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLX_HomePos = (int)lx.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLX_HomeOffset = (int)lx.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLX_HomeVelo = (int)lx.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLX_Velo = (int)lx.Param.Speed.Value;
                    }

                    var lw = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LW);
                    if (lw != null)
                    {
                        GPLoader.GPLoaderParam.nLW_HomeImpulsVelo = (int)lw.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLW_HomePos = (int)lw.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLW_HomeOffset = (int)lw.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLW_HomeVelo = (int)lw.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLW_Velo = (int)lw.Param.Speed.Value;
                    }

                    var lUU = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LUU);
                    if (lUU != null)
                    {
                        GPLoader.GPLoaderParam.nLU_HomeImpulsVelo = (int)lUU.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLUU_HomePos = (int)lUU.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLUU_HomeOffset = (int)lUU.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLU_HomeVelo = (int)lUU.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLU_Velo = (int)lUU.Param.Speed.Value;
                    }

                    var lUD = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LUD);
                    if (lUD != null)
                    {
                        GPLoader.GPLoaderParam.nLU_HomeImpulsVelo = (int)lUD.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLUD_HomePos = (int)lUD.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLUD_HomeOffset = (int)lUD.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLU_HomeVelo = (int)lUD.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLU_Velo = (int)lUD.Param.Speed.Value;
                    }
                    var lCC = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LCC);
                    if (lCC != null)
                    {
                        GPLoader.GPLoaderParam.nLCC_HomeImpulsVelo = (int)lCC.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLCC_HomePos = (int)lCC.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLCC_HomeOffset = (int)lCC.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLCC_HomeVelo = (int)lCC.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLCC_Velo = (int)lCC.Param.Speed.Value;
                    }

                    var lZM = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LZM);
                    if (lZM != null)
                    {
                        GPLoader.GPLoaderParam.nLZ_HomeImpulsVelo = (int)lZM.Param.IndexSearchingSpeed.Value;
                        GPLoader.GPLoaderParam.nLZM_HomePos = (int)lZM.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLZM_HomeOffset = (int)lZM.Param.ClearedPosition.Value;
                        GPLoader.GPLoaderParam.nLZ_HomeVelo = (int)lZM.Param.HommingSpeed.Value;
                        GPLoader.GPLoaderParam.nLZ_Velo = (int)lZM.Param.Speed.Value;
                    }
                    var lZS = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(i => i.AxisType.Value == EnumAxisConstants.LZS);
                    if (lZS != null)
                    {
                        GPLoader.GPLoaderParam.nLZS_HomePos = (int)lZS.Param.HomeOffset.Value;
                        GPLoader.GPLoaderParam.nLZS_HomeOffset = (int)lZS.Param.ClearedPosition.Value;
                    }
                    
                    GPLoader.GPLoaderParam.nLU_Gap = (int)Loader.SystemParameter.ARMModules[1].EndOffset.Value;

                    GPLoader.GPLoaderParam.nLUD_HeightOffset = (int)Loader.SystemParameter.ARMModules[0].UpOffset.Value;
                    GPLoader.GPLoaderParam.nLUU_HeightOffset = (int)Loader.SystemParameter.ARMModules[1].UpOffset.Value;


                    GPLoader.GPLoaderParam.nLCC_HeightOffset = (int)Loader.SystemParameter.CardARMModules[0].UpOffset.Value;

                    GPLoader.GPLoaderParam.nTimeOut = 5000;

                    GPLoader.GPLoaderParam.nLT_LUC_Back_Pos = -3500;
                    
                    GPLoader.Card_ID_AccParam.nLX_Pos = (int)Loader.SystemParameter.Card_ID_Position.LX.Value;
                    GPLoader.Card_ID_AccParam.nLZ_Pos = (int)Loader.SystemParameter.Card_ID_Position.A.Value;
                    GPLoader.Card_ID_AccParam.nLUC_Pos = (int)Loader.SystemParameter.Card_ID_Position.U.Value;
                    GPLoader.Card_ID_AccParam.nLU_Pos = 0;
                    GPLoader.Card_ID_AccParam.nLW_Pos = 0;

                    if (Loader.GetLoaderCommands() is GPLoaderCommandEmulator)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = GPLoader.RenewData();
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            return retVal;
        }

        public EventCodeEnum UpdateCassetteSystem(SubstrateSizeEnum wafersize, int foupIndex)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            lock (lockObj)
            {
                try
                {
                    var slotSize = wafersize;
                    if (slotSize == SubstrateSizeEnum.UNDEFINED)
                    {
                        slotSize = Loader.GetDefaultWaferSize();
                        LoggerManager.Debug($"UpdateLoaderSystem(). Wafer size is set to default wafer size {slotSize}");
                    }
                    int i = 0;

                    bool shouldCheckCassetteType = false;
                    CassetteTypeEnum cassetteTypeEnum = CassetteTypeEnum.FOUP_25;
                    if (foupIndex > 0)
                    {
                        cassetteTypeEnum = Loader.GetCassetteType(foupIndex);
                        if (cassetteTypeEnum != CassetteTypeEnum.UNDEFINED)
                        {
                            shouldCheckCassetteType = true;
                        }
                        else
                        {
                            cassetteTypeEnum = CassetteTypeEnum.FOUP_25;
                            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(). cassette Type is set to default wafer size {slotSize}");
                        }
                    }
                    LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Wafer Size {slotSize} , foup Index {foupIndex} , cassetteType {cassetteTypeEnum}");

                    if (foupIndex == 1)
                    {
                        var Foup1 = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(fi => fi.AxisType.Value == EnumAxisConstants.FC1);
                        if (Loader.SystemParameter.ScanSensorModules[foupIndex - 1] != null)
                        {
                            i = foupIndex - 1;
                            var Foup1accessPos = Loader.SystemParameter.ScanSensorModules[i].ScanParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize 
                            && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                            if (Foup1 != null && Foup1accessPos != null)
                            {
                                GPLoader.GPLoaderParam.nFoup_1_HomePos = (int)Foup1.Param.HomeOffset.Value;
                                GPLoader.GPLoaderParam.nFoup_1_HomeOffset = (int)Foup1.Param.ClearedPosition.Value;
                                GPLoader.GPLoaderParam.nFoup1_UpPos = (int)Foup1accessPos.UpOffset.Value;
                                GPLoader.GPLoaderParam.nFoup1_DownPos = (int)Foup1accessPos.DownOffset.Value;
                                GPLoader.GPLoaderParam.nFoup1_MappingStartPos = (int)Foup1accessPos.SensorOffset.Value; //scan startPos
                                GPLoader.GPLoaderParam.nFoup_HomeVelo = (int)Foup1.Param.HommingSpeed.Value;
                                GPLoader.GPLoaderParam.nFoup_HomeImpulsVelo = (int)Foup1.Param.IndexSearchingSpeed.Value;
                                GPLoader.GPLoaderParam.nFoup_Velo = (int)Foup1.Param.Speed.Value;
                            }
                            else 
                            {
                                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): NULL Error Foup1 {Foup1} , Foup1accessPos {Foup1accessPos}");
                            }
                        }
                        else 
                        {
                            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): NULL Error Wafer Size {slotSize} , foup Index {foupIndex}");
                        }
                    }
                    else if (foupIndex == 2)
                    {
                        if (Loader.SystemParameter.ScanSensorModules.Count > 1)
                        {
                            var Foup2 = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(dev => dev.AxisType.Value == EnumAxisConstants.FC2);
                            var Foup2accessPos = Loader.SystemParameter.ScanSensorModules[1].ScanParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                             && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                            if (Foup2 != null && Foup2accessPos != null)
                            {
                                GPLoader.GPLoaderParam.nFoup_2_HomePos = (int)Foup2.Param.HomeOffset.Value;
                                GPLoader.GPLoaderParam.nFoup_2_HomeOffset = (int)Foup2.Param.ClearedPosition.Value;
                                GPLoader.GPLoaderParam.nFoup2_UpPos = (int)Foup2accessPos.UpOffset.Value;
                                GPLoader.GPLoaderParam.nFoup2_DownPos = (int)Foup2accessPos.DownOffset.Value;
                                GPLoader.GPLoaderParam.nFoup2_MappingStartPos = (int)Foup2accessPos.SensorOffset.Value; //scan startPos
                            }
                            else
                            {
                                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): NULL Error Foup1 {Foup2} , Foup1accessPos {Foup2accessPos}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): NULL Error Wafer Size {slotSize} , foup Index {foupIndex}");
                        }
                    }
                    else if (foupIndex == 3)
                    {
                        if (Loader.SystemParameter.ScanSensorModules.Count > 2)
                        {
                            var Foup3 = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(dev => dev.AxisType.Value == EnumAxisConstants.FC3);
                            var Foup3accessPos = Loader.SystemParameter.ScanSensorModules[2].ScanParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                             && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                            if (Foup3 != null && Foup3accessPos != null)
                            {
                                GPLoader.GPLoaderParam.nFoup_3_HomePos = (int)Foup3.Param.HomeOffset.Value;
                                GPLoader.GPLoaderParam.nFoup_3_HomeOffset = (int)Foup3.Param.ClearedPosition.Value;
                                GPLoader.GPLoaderParam.nFoup3_UpPos = (int)Foup3accessPos.UpOffset.Value;
                                GPLoader.GPLoaderParam.nFoup3_DownPos = (int)Foup3accessPos.DownOffset.Value;
                                GPLoader.GPLoaderParam.nFoup3_MappingStartPos = (int)Foup3accessPos.SensorOffset.Value; //scan startPos
                            }
                            else
                            {
                                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): NULL Error Foup1 {Foup3} , Foup1accessPos {Foup3accessPos}");
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): NULL Error Wafer Size {slotSize} , foup Index {foupIndex}");
                        }
                    }

                    i = foupIndex - 1;
                    var foupAxisType = (EnumAxisConstants)((short)EnumAxisConstants.FC1 + i);
                    var foupAxisDef = Loader.MotionManager.LoaderAxes.ProbeAxisProviders.FirstOrDefault(f => f.AxisType.Value == foupAxisType);
                    var foupAccs = Loader.SystemParameter.ScanSensorModules[i].ScanParams.FirstOrDefault(dev => dev.SubstrateSize.Value == slotSize
                     && (!shouldCheckCassetteType || dev.CassetteType.Value == cassetteTypeEnum));
                    if (foupAxisDef != null && foupAccs != null)
                    {
                        GPLoader.FOUPParams[i].HomePos = (int)foupAxisDef.Param.HomeOffset.Value;
                        GPLoader.FOUPParams[i].HomeOffset = (int)foupAxisDef.Param.ClearedPosition.Value;
                        GPLoader.FOUPParams[i].UpPos = (int)foupAccs.UpOffset.Value;
                        GPLoader.FOUPParams[i].DownPos = (int)foupAccs.DownOffset.Value;
                        GPLoader.FOUPParams[i].MappingStartPos = (int)foupAccs.SensorOffset.Value; //scan startPos
                    }
                    else
                    {
                        LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): {foupAxisType} NULL Error Wafer Size {slotSize} , foup Index {foupIndex} , foupAccs {foupAccs}");
                    }

                    GPLoader.GPLoaderParam.nWaferThick = (int)Loader.DeviceParameter.CassetteModules[0].WaferThickness.Value;
                    GPLoader.GPLoaderParam.nWaferGap = (int)Loader.DeviceParameter.CassetteModules[0].SlotSize.Value;
                    
                    if (Loader.GetLoaderCommands() is GPLoaderCommandEmulator)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = GPLoader.RenewData();
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            return retVal;
        }

        public EventCodeEnum Deinitialize()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Disconnect()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public void FOUP_RaiseFoupStateChanged(FoupModuleInfo foupInfo)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
        }

        public void FOUP_RaiseWaferOutDetected(int cassetteNumber)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
        }

        public double GetArmUpOffset()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return 0;
        }

        public LoaderDeviceParameter GetDeviceParam()
        {
            LoaderDeviceParameter retVal = null;

            try
            {
                retVal = Loader.DeviceParameter.Clone<LoaderDeviceParameter>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderInfo GetLoaderInfo()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            LoaderInfo retVal = null;

            try
            {
                retVal = Loader.GetLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderSystemParameter GetSystemParam()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            LoaderSystemParameter retVal = null;

            try
            {
                retVal = Loader.SystemParameter.Clone<LoaderSystemParameter>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum Initialize(string rootParamPath)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //retVal = Loader.Initialize(LoaderServiceTypeEnum.REMOTE, rootParamPath);
                retVal = GPLoader.Connect();


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsFoupAccessed(int cassetteNumber)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            bool retVal = false;

            try
            {
                //retVal = Loader.IsFoupAccessed(cassetteNumber);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum LoaderSystemInit()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.SystemInit();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MOTION_JogAbsMove(EnumAxisConstants axis, double value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;

        }

        public EventCodeEnum MOTION_JogRelMove(EnumAxisConstants axis, double value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum MoveToModuleForSetup(ModuleTypeEnum module, bool skipuaxis, int slot, int index)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }


        public EventCodeEnum OFR_ChangeLight(int channel, ushort intensity)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.OCRRemoteService.ChangeLight(channel, intensity);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum OFR_GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //retVal = Loader.OCRRemoteService.GetOCRImage(out imgBuf, out w, out h);
            w = 480;
            h = 480;
            imgBuf = new byte[w * h];

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

        public EventCodeEnum OFR_GetOCRState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.OCRRemoteService.GetOCRState();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OFR_OCRFail()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.OCRRemoteService.OCRFail();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OFR_OCRRemoteEnd()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.OCRRemoteService.OCRRemoteEnd();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OFR_OCRRetry()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.OCRRemoteService.OCRRetry();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum OFR_SetOcrID(string ocrID)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.OCRRemoteService.SetOcrID(ocrID);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum RECOVERY_MotionInit()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum RECOVERY_ResetWaferLocation()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum RetractAll()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDeviceParam(LoaderDeviceParameter deviceParam)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.SaveDeviceParam(deviceParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSystemParam(LoaderSystemParameter systemParam)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.SaveSystemParam(systemParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SelfRecovery()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
        }

        public void SetLoaderTestOption(LoaderTestOption option)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            //Loader.LoaderOption = option;
        }

        public void SetPause()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            try
            {
                //Loader.SetPause();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ResponseResult SetRequest(LoaderMap dstMap)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            ResponseResult retVal = new ResponseResult();

            try
            {
                //retVal = Loader.SetRequest(dstMap);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetResume()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            try
            {
                //Loader.SetResume();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetTestCenteringFlag(bool testflag)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            try
            {
                //Loader.SetTestCenteringFlag(testflag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum UpdateDeviceParam(LoaderDeviceParameter deviceParam)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.UpdateDeviceParam(deviceParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum UpdateSystemParam(LoaderSystemParameter systemParam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.UpdateSystemParam(systemParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_ChuckDownMove()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.ChuckDownMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_ChuckUpMove(int option = 0)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.ChuckUpMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_Wafer_MoveLoadingPosition()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.ChuckUpMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public bool WTR_IsLoadWafer()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            bool retVal = false;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.IsLoadWafer();
                retVal = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_MonitorForARMVacuum(bool value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.MonitorForVacuum(value);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyLoadedToThreeLeg(out TransferObject loadedObject)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadedObject = new TransferObject();
            try
            {
                //retVal = Loader.WaferTransferRemoteService.NotifyLoadedToThreeLeg(out loadedObject);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyUnloadedFromThreeLeg(EnumWaferState waferState, int cellIdx, bool NotifyUnloadedFromThreeLeg = false)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.NotifyUnloadedFromThreeLeg(waferState);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_NotifyWaferTransferResult(bool isSucceed)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.NotifyWaferTransferResult(isSucceed);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_PickUpMove()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.PickUpMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_PlaceDownMove()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.PlaceDownMove();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_RetractARM()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.RetractARM();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SafePosW()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SafePosW();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SelfRecoveryRetractARM()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SelfRecoveryRetractARM();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SelfRecoveryTransferToPreAlign()
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SelfRecoveryTransferToPreAlign();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_SetWaferUnknownStatus(bool isARMUnknown, bool isChuckUnknown)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.SetWaferUnknownStatus(isARMUnknown, isChuckUnknown);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum WTR_Notifyhandlerholdwafer(bool ishandlerhold)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
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
        public EventCodeEnum WTR_WaitForARMVacuum(bool value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.WaitForVacuum(value);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum WTR_WriteARMVacuum(bool value)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //retVal = Loader.WaferTransferRemoteService.WriteVacuum(value);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public LoaderServiceTypeEnum GetServiceType()
        {
            return LoaderServiceTypeEnum.REMOTE;
        }

        public void SetContainer(Autofac.IContainer container)
        {
            cont = container;
            Loader.LoaderService = this;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void SetCallBack(ILoaderServiceCallback loadercontroller)
        {

            callback = loadercontroller;
            Loader.Connect(callback);
        }
        public string GetResonOfError()
        {
            string error = Loader.ResonOfError;
            Loader.ResonOfError = null;
            return error;
        }

        public EventCodeEnum GetWaferLoadObject(out TransferObject loadobj)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            loadobj = new TransferObject();
            try
            {
                //retVal = Loader.WaferTransferRemoteService.NotifyLoadedToThreeLeg(out loadedObject);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public string GetProbeCardID()
        {
            return Loader.GetLoaderInfo().ModuleInfo.ProbeCardID;
        }

        public EventCodeEnum OFR_OCRAbort()
        {
            throw new NotImplementedException();
        }

        public bool SetNoReadScanState(int cassetteNumber)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ResultMapUpload(int v)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum ResultMapDownload(int stageindex, string filename)
        {
            LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name}(): Method excuted");
            return EventCodeEnum.NONE;
        }

    }

}
