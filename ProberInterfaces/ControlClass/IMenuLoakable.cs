namespace ProberInterfaces
{
    public interface IMenuLockable
    {
        bool Operation_WaferInspectionLockable { get; set; }
        bool Operation_TesterHeadChangeLockable { get; set; }
        bool Operation_ProbeCardChangeLockable { get; set; }
        bool Operation_CleanSheetChangeLockable { get; set; }
        bool Operation_CleanPadCleaningLockable { get; set; }
        bool Operation_PolishWaferCleaningLockable { get; set; }
        bool Operation_MotionJogLockable { get; set; }
        bool Operation_CassettePortLockable { get; set; }
        bool Operation_Alignment_WaferAlignmentLockable { get; set; }
        bool Operation_Alignment_PinAlignmentLockable { get; set; }
        bool Operation_ManualSoakingLockable { get; set; }

        bool Recipe_CreateLockable { get; set; }
        bool Recipe_SettingChangeLockable { get; set; }
        bool Recipe_DeviceChangeLockable { get; set; }
        bool Recipe_Upload_DownloadLockable { get; set; }
        bool Recipe_Management_Copy_RemoveLockable { get; set; }

        bool User_LogOutLockable { get; set; }

        bool Utility_PMIViewerLockable { get; set; }
        bool Utility_ResultMapViewerLockable { get; set; }
        bool Utility_SnapShotLockable { get; set; }
        bool Utility_TaskManagement { get; set; }
        //bool Utility_ThreadLockExplorer { get; set; }

        bool Sytem_InitializationLockable { get; set; }

        bool Loader_InitializationLockable { get; set; }
        bool System_SettingChangeLockable { get; set; }
        bool System_QuitLockable { get; set; }
        
        void LockableFalse();
        void LockableTrue();
    }
}
