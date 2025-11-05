using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    using LogModule;
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media.Media3D;

    public enum CameraViewPoint
    {
        FRONT       = 0x0000,
        FOUP        = 0x0001,
        LOADER      = 0x0002,
        LOADER_BACK = 0x0003,
        BACK        = 0x0004,
        STAGE_BACK  = 0x0005,
        STAGE_1     = 0x0006,
        STAGE_2     = 0x0007,
    }

    public interface ILogVM : IModule
    {
        void EventMarkAllSet();
        int EventLogUserNotNotifiedCount { get; set; }
        void RefreshLoaderLogAlarmList();
        //CollectionViewSource ProLogList { get; set; }
        //SynchronizedObservableCollection<LogEventInfo> ProLogHistories { get; set; }
        //SynchronizedObservableCollection<LogEventInfo> DebugLogHistories { get; set; }
        //SynchronizedObservableCollection<LogEventInfo> EventLogHistories { get; set; }
        //LogHistoriesStorage LogHistories { get; set; }
        //EventCodeEnum UpdateNotifiedCount();
        //bool MadeHistoryForProlog { get; set; }
        //bool MadeHistoryForDebuglog { get; set; }
        //bool MadeHistoryForEventlog { get; set; }
    }

    public delegate void LockViewControlEventHandler(int hashcode);
    public delegate void UnLockViewControlEventHandler(int hashcode);
    public interface IViewModelManager : IFactoryModule, IModule
    {
        // Property
        ObservableCollection<IMainScreenView>   PreMainScreenViewList       { get; }
        List<SettingCategoryInfo>               DeviceSettingCategoryInfos  { get; }
        List<SettingCategoryInfo>               SystemSettingCategoryInfos  { get; }
        IMainScreenView                         PostMainScreenView          { get; }
        IMainScreenView                         MainScreenView              { get; }
        IMainScreenViewModel                    MainScreenViewModel         { get; }
        IMainScreenViewModel                    DiagnosisViewModel          { get; }

        IProberStation                          Prober                      { get; }
        //IMainTopBarView                         MainTopBarView              { get; }
        IMainScreenView                         MainTopBarView              { get; }
        IMainScreenView                         MainMenuView                { get; set; }
        IMapViewControl                         MapViewControl              { get; }
        IMapViewControl                         MapViewControlFD            { get; }
        IFilterPanelViewModel                   VMFilterPanel               { get; set; }
        IStage3DModel                           Stage3DModel                { get; set; }
        IMenuLockable                           MenuLoakables               { get; set; }
        INeedleCleanView                        NeedleCleanView             { get; set; }
        ILogVM                                  LogViewModel                { get; set; }
        ProberLogLevel                          LogLevel                    { get; set; }
        Window                                  MainWindowWidget            { get; set; }
        Point3D                                 CamPosition                 { get; set; }
        Vector3D                                CamLookDirection            { get; set; }
        Vector3D                                CamUpDirection              { get; set; }
        //string                                  HomeViewGuid                { get; }
        Guid                                    HomeViewGuid                { get; set; }
        //Guid                                    MainMenuViewGuid            { get; set; }
        Guid                                    TopBarViewGuid              { get; set; }
        bool                                    HelpQuestionEnable          { get; set; }
        bool                                    HasLockHandle               { get; set; }
        bool                                    LoginSkipEnable             { get; set; }
        IMainScreenViewModel FindViewModelObject(Guid viewguid);
        EventCodeEnum SetDataContext(object obj);
        EventCodeEnum MakeLogHistory();
        EventCodeEnum InsertView(IMainScreenView view);
        void ChangeFlyOutControlStatus(bool flag);
        void FlyOutLotInformation(Guid guid);
        void UpdateWidget();
        void UpdateCurMainViewModel();
        Task<bool> CheckUnlockWidget();
        //void SetLoadProgramFlag(bool loadflag);
        //bool GetLoadProgramFlag();
        void ChangeTabControlSelectedIndex(ProberLogLevel level);
        void Set3DCamPosition(CameraViewPoint ViewNUM, bool IsItDisplayed2RateMagnification);
        void TopBarDrawerOpen(bool ReloadAlarmList = false);
        // ViewTransition

        Task<EventCodeEnum> ViewTransitionAsync(Guid guid, object parameter = null, bool paramvalidatiaon = true, bool ChangePrev = false);

        Task<EventCodeEnum> ViewTransitionType(object obj);
        //EventCodeEnum       ViewTransitionToStatisticsErrorView();
        Task<EventCodeEnum> BackPreScreenTransition(bool paramvalidation = true);

        // Registe View/ViewModel
        EventCodeEnum RegisteViewInstance(UCControlInfo info);
        EventCodeEnum RegisteViewModelInstance(UCControlInfo info);
        EventCodeEnum ConnectControlInstances(Guid viewguid, Guid viewmodelguid);

        // Lock
        //EventCodeEnum Lock(int hashcode, string title, string message);
        //EventCodeEnum LockView(IMainScreenView view);
        //EventCodeEnum UnLock(int hashcode);
        EventCodeEnum AllUnLock();

        // Notify
        void ShowNotifyMessage(int hash, string title, string message);
        void HideNotifyMessage(int hash);
        void ShowNotifyToastMessage(int hash, string title, string message, int scond);
        Task<EventCodeEnum> InitScreen();
        Task<IMainScreenView> GetViewObj(Guid viewGuid, object vmParam = null);
        Task<IMainScreenView> GetViewObjFromViewModelGuid(Guid viewmodelguid);
        IMainScreenViewModel GetViewModelFromGuid(Guid viewmodelguid);
        IMainScreenViewModel GetViewModelFromInterface(Type inttype);
        IMainScreenViewModel GetViewModelFromViewGuid(Guid viewguid);
        Guid GetViewGuidFromViewModelGuid(Guid guid);
        EventCodeEnum InsertConnectView(IMainScreenView view, IMainScreenViewModel viewModel);

        void WriteCurrentViewAndViewModelName();
        void SetMainScreenView(IMainScreenView mainScreenView);

        void SetSystemGUID();

        bool FlyoutIsOpen { get; }
     }
}
