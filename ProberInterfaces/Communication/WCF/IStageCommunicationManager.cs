using LogModule;
using ProberErrorCode;
using ProberInterfaces.Proxies;
using ProberInterfaces.ViewModel;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface IStageCommunicationManager : IModule
    {
        int GetServicePort();
        bool InitServiceHosts(int port = -1);
        bool DeInitService();
        void SetAcceptUpdateDisp(bool flag);
        bool GetAcceptUpdateDisp();
        void BindDispService(string uri);
        void BindDelegateEventService(string uri);
        void BindDataGatewayService(string uri);
        bool IsEnableDialogProxy();
        void DisConnectdispService(bool bForceAbort = false);
        void DisConnectDelegateEventService(bool bForceAbort = false);
        void DisconnectDataGatewayService(bool bForceAbort = false);
        IDialogServiceProxy GetMessageEventHost();
        EventCodeEnum NotifyStageAlarm(EventCodeParam noticeCodeInfo);
    }

    public static class ServiceAddress
    {
        public static string IOPortsService = "IOPortsService";
        public static string MotionManagerService = "MotionManagerService";
        public static string StageSupervisorService = "StageSupervisorService";
        public static string StageMoveService = "StageMoveService";
        public static string LoaderRemoteMediatorService = "LoaderRemoteMediatorService";
        public static string VmGPCardChangeMainPage = "VmGPCardChangeMainPage";
        public static string TempControlService = "TempControlService";
        public static string PolishWaferModuleService = "PolishWaferModuleService";
        public static string FileManagerService = "FileManagerService";
        public static string LotOPModuleService = "LotOPModuleService";
        public static string SoakingModuleService = "SoakingModuleService";
        public static string PMIModuleSerice = "PMIModuleSerice";
        public static string CoordinateManagerService = "CoordinateManagerService";
        public static string ParamManagerService = "ParamManagerService";
        public static string WAService = "WAService";
        public static string PinAlignerService = "PinAlignerService";
        public static string DataGatewayService = "DataGatewayService";
    }

    public static class PortOffsets
    {
        public static int StageSupervisorServicePortOffset = 100;
        public static int StageMovePortOffset = 105;
        public static int MotionServicePortOffset = 110;
        public static int IOServicePortOffset = 120;
        public static int LoaderRemoteMediatorServicePortOffset = 130;
        public static int GPCCServicePortOffset = 140;
        public static int GPCCOPServicePortOffset = 141;
        public static int TempContServicePortOffset = 150;
        public static int CoordManagerServicePortOffset = 160;
        public static int WALServicePortOffset = 161;

        public static int ParamManagerServicePortOffset = 170;

        public static int RetestServicePortOffset = 175;

        public static int PWServicePortOffset = 180;
        public static int PMIServicePortOffset = 181;

        public static int FileManagerServicePortOffset = 182;

        public static int LotOPModuleServicePortOffset = 183;
        public static int SoakingModuleServicePortOffset = 184;

        public static int PinAlignerServicePortOffset = 185;

        public static int ChillerServicePortOffset = 186;
        public static int DryAirServicePortOffset = 187;
        public static int DewPointServicePortOffset = 188;

        public static Dictionary<Type, int> PortOffsetDict = new Dictionary<Type, int>();

        private static void InitPortOffsetDict()
        {
            try
            {
                if (PortOffsetDict == null)
                {
                    PortOffsetDict = new Dictionary<Type, int>();
                }

                PortOffsetDict.Add(typeof(IStageSupervisorProxy), StageSupervisorServicePortOffset);
                PortOffsetDict.Add(typeof(IStageMoveProxy), StageMovePortOffset);
                PortOffsetDict.Add(typeof(IMotionAxisProxy), MotionServicePortOffset);


                PortOffsetDict.Add(typeof(IIOPortProxy), IOServicePortOffset);
                PortOffsetDict.Add(typeof(IRemoteMediumProxy), LoaderRemoteMediatorServicePortOffset);

                PortOffsetDict.Add(typeof(ICCObservationVM), GPCCServicePortOffset);
                //PortOffsetDict.Add(typeof(), GPCCOPServicePortOffset);

                PortOffsetDict.Add(typeof(ITempControllerProxy), TempContServicePortOffset);
                PortOffsetDict.Add(typeof(ICoordinateManagerProxy), CoordManagerServicePortOffset);

                PortOffsetDict.Add(typeof(IWaferAlignerProxy), WALServicePortOffset);

                PortOffsetDict.Add(typeof(IParamManagerProxy), ParamManagerServicePortOffset);
                PortOffsetDict.Add(typeof(IPolishWaferModuleProxy), PWServicePortOffset);
                PortOffsetDict.Add(typeof(IPMIModuleProxy), PMIServicePortOffset);
                PortOffsetDict.Add(typeof(IFileManagerProxy), FileManagerServicePortOffset);
                PortOffsetDict.Add(typeof(ILotOPModuleProxy), LotOPModuleServicePortOffset);

                PortOffsetDict.Add(typeof(ISoakingModuleProxy), SoakingModuleServicePortOffset);
                PortOffsetDict.Add(typeof(IPinAlignerProxy), PinAlignerServicePortOffset);

                PortOffsetDict.Add(typeof(IRetestModuleProxy), RetestServicePortOffset);

                //PortOffsetDict.Add(typeof(), ChillerServicePortOffset);
                //PortOffsetDict.Add(typeof(), DryAirServicePortOffset);
                //PortOffsetDict.Add(typeof(), DewPointServicePortOffset);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public static bool GetPortOffset<T>(out int offset)
        {
            bool retval = false;
            offset = 0;

            try
            {
                if (PortOffsetDict == null || PortOffsetDict.Count == 0)
                {
                    InitPortOffsetDict();
                }

                retval = PortOffsetDict.TryGetValue(typeof(T), out offset);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public static class ServicePort
    {
        public static int EnvControlServicePort = 18423;
        public static int ChillerServicePort = 8424;
        public static int DryAirServicePort = 8425;
        public static int LoaderServicePort = 19001;
        public static int MultiLauncherControlServicePort = 19002;
        public static int DialogProxyPort = 9002;
        public static int DispProxyPort = 9003;
        public static int DataGatewayProxyPort = 19004;
    }
}
