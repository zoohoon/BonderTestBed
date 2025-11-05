using LogModule;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ViewModelModule
{

    public class MenuLockable : INotifyPropertyChanged, IMenuLockable
    {
        #region //..PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region //..Properties
        #region //..Operation

        private bool _Operation_ManualProbeLockable;
        public bool Operation_ManualProbeLockable
        {
            get { return _Operation_ManualProbeLockable; }
            set
            {
                if (value != _Operation_ManualProbeLockable)
                {
                    _Operation_ManualProbeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_WaferInspectionLockable;
        public bool Operation_WaferInspectionLockable
        {
            get { return _Operation_WaferInspectionLockable; }
            set
            {
                if (value != _Operation_WaferInspectionLockable)
                {
                    _Operation_WaferInspectionLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_WaferHandlingLockable;
        public bool Operation_WaferHandlingLockable
        {
            get { return _Operation_WaferHandlingLockable; }
            set
            {
                if (value != _Operation_WaferHandlingLockable)
                {
                    _Operation_WaferHandlingLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_ProbeCardChangeLockable;
        public bool Operation_ProbeCardChangeLockable
        {
            get { return _Operation_ProbeCardChangeLockable; }
            set
            {
                if (value != _Operation_ProbeCardChangeLockable)
                {
                    _Operation_ProbeCardChangeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_TesterHeadChangeLockable;
        public bool Operation_TesterHeadChangeLockable
        {
            get { return _Operation_TesterHeadChangeLockable; }
            set
            {
                if (value != _Operation_TesterHeadChangeLockable)
                {
                    _Operation_TesterHeadChangeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_CleanSheetChangeLockable;
        public bool Operation_CleanSheetChangeLockable
        {
            get { return _Operation_CleanSheetChangeLockable; }
            set
            {
                if (value != _Operation_CleanSheetChangeLockable)
                {
                    _Operation_CleanSheetChangeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_CleanPadCleaningLockable;
        public bool Operation_CleanPadCleaningLockable
        {
            get { return _Operation_CleanPadCleaningLockable; }
            set
            {
                if (value != _Operation_CleanPadCleaningLockable)
                {
                    _Operation_CleanPadCleaningLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_PolishWaferCleaningLockable;
        public bool Operation_PolishWaferCleaningLockable
        {
            get { return _Operation_PolishWaferCleaningLockable; }
            set
            {
                if (value != _Operation_PolishWaferCleaningLockable)
                {
                    _Operation_PolishWaferCleaningLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_MotionJogLockable;
        public bool Operation_MotionJogLockable
        {
            get { return _Operation_MotionJogLockable; }
            set
            {
                if (value != _Operation_MotionJogLockable)
                {
                    _Operation_MotionJogLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_CassettePortLockable;
        public bool Operation_CassettePortLockable
        {
            get { return _Operation_CassettePortLockable; }
            set
            {
                if (value != _Operation_CassettePortLockable)
                {
                    _Operation_CassettePortLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_Alignment_WaferAlignmentLockable;
        public bool Operation_Alignment_WaferAlignmentLockable
        {
            get { return _Operation_Alignment_WaferAlignmentLockable; }
            set
            {
                if (value != _Operation_Alignment_WaferAlignmentLockable)
                {
                    _Operation_Alignment_WaferAlignmentLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Operation_Alignment_PinAlignmentLockable;
        public bool Operation_Alignment_PinAlignmentLockable
        {
            get { return _Operation_Alignment_PinAlignmentLockable; }
            set
            {
                if (value != _Operation_Alignment_PinAlignmentLockable)
                {
                    _Operation_Alignment_PinAlignmentLockable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Operation_ManualSoakingLockable;
        public bool Operation_ManualSoakingLockable
        {
            get { return _Operation_ManualSoakingLockable; }
            set
            {
                if (value != _Operation_ManualSoakingLockable)
                {
                    _Operation_ManualSoakingLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Recipe

        private bool _Recipe_CreateLockable;
        public bool Recipe_CreateLockable
        {
            get { return _Recipe_CreateLockable; }
            set
            {
                if (value != _Recipe_CreateLockable)
                {
                    _Recipe_CreateLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Recipe_SettingChangeLockable;
        public bool Recipe_SettingChangeLockable
        {
            get { return _Recipe_SettingChangeLockable; }
            set
            {
                if (value != _Recipe_SettingChangeLockable)
                {
                    _Recipe_SettingChangeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Recipe_DeviceChangeLockable;
        public bool Recipe_DeviceChangeLockable
        {
            get { return _Recipe_DeviceChangeLockable; }
            set
            {
                if (value != _Recipe_DeviceChangeLockable)
                {
                    _Recipe_DeviceChangeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Recipe_Upload_DownloadLockable;
        public bool Recipe_Upload_DownloadLockable
        {
            get { return _Recipe_Upload_DownloadLockable; }
            set
            {
                if (value != _Recipe_Upload_DownloadLockable)
                {
                    _Recipe_Upload_DownloadLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Recipe_Management_Copy_RemoveLockable;
        public bool Recipe_Management_Copy_RemoveLockable
        {
            get { return _Recipe_Management_Copy_RemoveLockable; }
            set
            {
                if (value != _Recipe_Management_Copy_RemoveLockable)
                {
                    _Recipe_Management_Copy_RemoveLockable = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        #region //..User

        private bool _User_LogOutLockable;
        public bool User_LogOutLockable
        {
            get { return _User_LogOutLockable; }
            set
            {
                if (value != _User_LogOutLockable)
                {
                    _User_LogOutLockable = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

        #region //..Utility

        private bool _Utility_PMIViewerLockable;
        public bool Utility_PMIViewerLockable
        {
            get { return _Utility_PMIViewerLockable; }
            set
            {
                if (value != _Utility_PMIViewerLockable)
                {
                    _Utility_PMIViewerLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Utility_ResultMapViewerLockable;
        public bool Utility_ResultMapViewerLockable
        {
            get { return _Utility_ResultMapViewerLockable; }
            set
            {
                if (value != _Utility_ResultMapViewerLockable)
                {
                    _Utility_ResultMapViewerLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        

        private bool _Utility_SnapShotLockable;
        public bool Utility_SnapShotLockable
        {
            get { return _Utility_SnapShotLockable; }
            set
            {
                if (value != _Utility_SnapShotLockable)
                {
                    _Utility_SnapShotLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Utility_TaskManagement;
        public bool Utility_TaskManagement
        {
            get { return _Utility_TaskManagement; }
            set
            {
                if (value != _Utility_TaskManagement)
                {
                    _Utility_TaskManagement = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool _Utility_ThreadLockExplorer;
        //public bool Utility_ThreadLockExplorer
        //{
        //    get { return _Utility_ThreadLockExplorer; }
        //    set
        //    {
        //        if (value != _Utility_ThreadLockExplorer)
        //        {
        //            _Utility_ThreadLockExplorer = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        #endregion

        #region //..System


        private bool _Sytem_InitializationLockable;
        public bool Sytem_InitializationLockable
        {
            get { return _Sytem_InitializationLockable; }
            set
            {
                if (value != _Sytem_InitializationLockable)
                {
                    _Sytem_InitializationLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Loader_InitializationLockable;
        public bool Loader_InitializationLockable
        {
            get { return _Loader_InitializationLockable; }
            set
            {
                if (value != _Loader_InitializationLockable)
                {
                    _Loader_InitializationLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _System_SettingChangeLockable;
        public bool System_SettingChangeLockable
        {
            get { return _System_SettingChangeLockable; }
            set
            {
                if (value != _System_SettingChangeLockable)
                {
                    _System_SettingChangeLockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _System_QuitLockable;
        public bool System_QuitLockable
        {
            get { return _System_QuitLockable; }
            set
            {
                if (value != _System_QuitLockable)
                {
                    _System_QuitLockable = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion
        #endregion

        public MenuLockable()
        {
            LockableTrue();
        }

        public void LockableFalse()
        {
            try
            {
                Operation_ManualProbeLockable = false;
                Operation_WaferInspectionLockable = false;
                Operation_WaferHandlingLockable = false;
                Operation_ProbeCardChangeLockable = false;
                Operation_TesterHeadChangeLockable = false;
                Operation_CleanSheetChangeLockable = false;
                Operation_CleanPadCleaningLockable = false;
                Operation_PolishWaferCleaningLockable = false;
                Operation_MotionJogLockable = false;
                Operation_CassettePortLockable = false;
                Operation_Alignment_WaferAlignmentLockable = false;
                Operation_Alignment_PinAlignmentLockable = false;
                Operation_ManualSoakingLockable = false;

                Recipe_CreateLockable = false;
                Recipe_SettingChangeLockable = false;
                Recipe_DeviceChangeLockable = false;
                Recipe_Upload_DownloadLockable = false;
                Recipe_Management_Copy_RemoveLockable = false;

                User_LogOutLockable = false;

                Utility_PMIViewerLockable = false;
                Utility_ResultMapViewerLockable = false;
                Utility_SnapShotLockable = false;
                Utility_TaskManagement = false;

                Sytem_InitializationLockable = false;
                Loader_InitializationLockable = false;

                System_SettingChangeLockable = false;
                System_QuitLockable = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void LockableTrue()
        {
            try
            {
                Operation_ManualProbeLockable = true;
                Operation_WaferInspectionLockable = true;
                Operation_WaferHandlingLockable = true;
                Operation_ProbeCardChangeLockable = true;
                Operation_TesterHeadChangeLockable = true;
                Operation_CleanSheetChangeLockable = true;
                Operation_CleanPadCleaningLockable = true;
                Operation_PolishWaferCleaningLockable = true;
                Operation_MotionJogLockable = true;
                Operation_CassettePortLockable = true;
                Operation_Alignment_WaferAlignmentLockable = true;
                Operation_Alignment_PinAlignmentLockable = true;
                Operation_ManualSoakingLockable = true;

                Recipe_CreateLockable = true;
                Recipe_SettingChangeLockable = true;
                Recipe_DeviceChangeLockable = true;
                Recipe_Upload_DownloadLockable = true;
                Recipe_Management_Copy_RemoveLockable = true;

                User_LogOutLockable = true;

                Utility_PMIViewerLockable = true;
                Utility_ResultMapViewerLockable = true;
                Utility_SnapShotLockable = true;
                Utility_TaskManagement = true;

                Sytem_InitializationLockable = true;
                Loader_InitializationLockable = true;
                System_SettingChangeLockable = true;
                System_QuitLockable = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
