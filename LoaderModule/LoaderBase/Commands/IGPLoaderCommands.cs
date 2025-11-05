using ProberErrorCode;
using ProberInterfaces;
using System;
using Autofac;
using ProberInterfaces.Enum;

namespace LoaderBase
{
    public interface IGPLoaderCommands
    {
        #region // Loader commands
        
        EventCodeEnum BufferPick(IBufferModule Buffer, IARMModule arm);
        EventCodeEnum BufferPut(IARMModule arm, IBufferModule Buffer);
        EventCodeEnum CardBufferPick(ICardBufferModule CCBuffer, ICardARMModule arm, int holderNum = -1);
        EventCodeEnum CardBufferPut(ICardARMModule arm, ICardBufferModule CCBuffer, int holderNum = -1);
        EventCodeEnum CardTrayPick(ICardBufferTrayModule CCTray, ICardARMModule arm);
        EventCodeEnum CardTrayPick(ICardBufferTrayModule CCTray, ICardARMModule arm, int holderNum = -1);
        EventCodeEnum CardTrayPut(ICardARMModule arm, ICardBufferTrayModule CCTray);
        EventCodeEnum CardTrayPut(ICardARMModule arm, ICardBufferTrayModule CCTray, int holderNum = -1);
        EventCodeEnum CardChangerPick(ICCModule CardChanger, ICardARMModule arm,int holderNum=-1);
        EventCodeEnum CardChangerPut(ICardARMModule arm, ICCModule CardChanger);
        EventCodeEnum CardMoveLoadingPosition(ICCModule CardChanger, ICardARMModule arm);
        EventCodeEnum CassettePick(ISlotModule slot, IARMModule arm);
        EventCodeEnum CassettePut(IARMModule arm, ISlotModule slot);
        EventCodeEnum DRWPick(IInspectionTrayModule drw, IARMModule arm);
        EventCodeEnum DRWPut(IARMModule arm, IInspectionTrayModule drw);
        EventCodeEnum ChuckMoveLoadingPosition(IChuckModule Chuck, IARMModule arm);
        EventCodeEnum ChuckPick(IChuckModule Chuck, IARMModule arm, int option = 0);
        EventCodeEnum ChuckPut(IARMModule arm, IChuckModule Chuck, int option = 0);
        EventCodeEnum FixedTrayPick(IFixedTrayModule FixedTray, IARMModule arm);
        EventCodeEnum FixedTrayPut(IARMModule arm, IFixedTrayModule FixedTray);
        EventCodeEnum PAMove(IPreAlignModule pa, double x, double y, double angle);
        EventCodeEnum CheckWaferIsOnPA(int index, out bool isExist);
        EventCodeEnum CassetteScan(ICassetteModule cassetteModule);
        EventCodeEnum CassetteLoad(ICassetteModule cassetteModule);
        EventCodeEnum CassetteUnLoad(ICassetteModule cassetteModule);
        EventCodeEnum PAForcedPick(IPreAlignModule pa, IARMModule arm);
        EventCodeEnum PAPick_NotVac(IPreAlignModule pa, IARMModule arm);
        EventCodeEnum PAPick(IPreAlignModule pa, IARMModule arm);
        EventCodeEnum PAPut(IARMModule arm, IPreAlignModule pa);
        EventCodeEnum PAPutAync(IARMModule arm, IPreAlignModule pa);
        EventCodeEnum PAPut_NotVac(IARMModule arm, IPreAlignModule pa);
        EventCodeEnum InitRobot();
        EventCodeEnum HomingRobot();
        void HomingResultAlarm(EventCodeEnum result);
        EventCodeEnum JogMove(ProbeAxisObject axisobj, double dist);
        EventCodeEnum PrealignWafer(IPreAlignModule pa, double notchAngle);
        EventCodeEnum PARotateTo(IPreAlignModule pa, double notchAngle);
        #region // Foup Commands
        //EventCodeEnum LockCassette(int index);
        //EventCodeEnum UnLockCassette(int index);
        //EventCodeEnum DockingPortIn(int index);
        //EventCodeEnum DockingPortOut(int index);
        //EventCodeEnum CoverOpen(int index);
        //EventCodeEnum CoverClose(int index);
        //EventCodeEnum CoverLock(int index);
        //EventCodeEnum CoverUnLock(int index);
        //EventCodeEnum FOUPReset(int index);
        //EventCodeEnum WaitForCSTStatus(int index, EnumCSTCtrl cstState, long timeout = 0);
        #endregion
        /// <summary>
        /// Loading 또는 Unloading 각도로 Align 할 때까지 PA를 기다린 후 PA.Busy가 종료되거나 Timeout으로 인해 종료되는 함수 
        /// </summary>
        /// <param name="pa"></param>
        /// <returns></returns>
        int WaitForPA(IPreAlignModule pa);
        /// <summary>
        /// OCR Config를 성공하는게 있을 때까지 PA를 기다린 후 모든 Config가 실패하거나 Timeout나거나 성공한게 있는 경우 (HostRunning 변수 에 의해) 종료되는 함수
        /// 하나라도 OCR Config 중 성공하는 게 있는 경우 HostRunning == READ_OCR
        /// 모든 Config를 실패할 경우 HostRunning == FAIL
        /// </summary>
        /// <param name="pa"></param>
        /// <param name="ocr"></param>
        /// <param name="ocrScore"></param>
        /// <returns></returns>
        EventCodeEnum WaitForOCR(IPreAlignModule pa, out String ocr, out double ocrScore);
        void EnableAxis();
        void DisableAxis();
        EventCodeEnum PAMove(IPreAlignModule pa, double angle);
        void LoaderLampSetState(ModuleStateEnum state);
        void StageLampSetState(ModuleStateEnum state);
        void LoaderBuzzer(bool isBuzzerOn);
        EventCodeEnum SetCardTrayVac(bool value);
        EventCodeEnum RFIDReInitialize();
        EnumCommunicationState GetRFIDCommState_ForCardID();
        string GetReceivedCardID();
        bool GetCardIDReadDataReady();
        EventCodeEnum CardIDMovePosition(CardHolder holder);
        EventCodeEnum SetCardID(CardHolder holder, string cardid);
        
        EventCodeEnum ResetRobotCommand();
        SubstrateSizeEnum GetDeviceSize(int index);

        EventCodeEnum SetBufferedMove(double xpos, double zpos, double wpos);

        EventCodeEnum LockRobot();
        EventCodeEnum UnlockRobot();
        bool IsIdleState();
        bool IsMovingOnCardOwner();
        EventCodeEnum SetTesterCoolantValve(int index, bool open);
        EventCodeEnum GetTesterCoolantValveState(int index, out bool isopened);
        EventCodeEnum CardTrayLock(bool locktray);
        IRFIDModule GetRFIDReaderForCard();
        #endregion
    }
    public static class Extensions_LoaderModule
    {
        private static Autofac.IContainer LoaderContainer;
        public static void AssignLoaderContainer(this IFactoryModule module, Autofac.IContainer container)
        {
            LoaderContainer = container;
        }
        static IGPLoader _GPLoader;
        static IGPLoaderCommands loaderCommands;
        public static IGPLoaderCommands GetLoaderCommands(this ILoaderSubModule module)
        {
            if (loaderCommands != null)
            {
                return loaderCommands;
            }

            if (_GPLoader == null)
            {
                if(SystemManager.SysteMode != SystemModeEnum.Single)
                {
                    _GPLoader = LoaderContainer.Resolve<IGPLoader>();
                }
                else
                {
                    loaderCommands = LoaderContainer.ResolveNamed<IGPLoaderCommands>("CmdEmul");
                }
            }

            if(_GPLoader?.DevConnected == true)
            {
                return (IGPLoaderCommands)_GPLoader;
            }
            else
            {
                if (loaderCommands == null)
                {
                    if (SystemManager.SysteMode != SystemModeEnum.Single)
                    {
                        loaderCommands = LoaderContainer.ResolveNamed<IGPLoaderCommands>("CmdEmul");
                    }
                }
                return loaderCommands;
            }
        }

        public static IGPLoader GetGPLoader(this ILoaderSubModule module)
        {
            if (_GPLoader != null)
            {
                return _GPLoader;
            }
            if (_GPLoader == null)
            {
                _GPLoader = LoaderContainer.Resolve<IGPLoader>();
            }
            if (_GPLoader.DevConnected == true)
            {
                return _GPLoader;
            }
            else
            {
                return null;
            }

        }

    }
}
