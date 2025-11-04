using Autofac;
using LoaderRecoveryOperate;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LoaderRecoveryControl
{
    public static class LoaderRecoveryControlVM
    {
        private static Autofac.IContainer LoaderContainer = null;
        private static IGPLoader _GPLoader = null;
        private static LoaderRecoveryMaster _RecoveryMaster;
        public static LoaderRecoveryMaster RecoveryMaster
        {
            get { return _RecoveryMaster; }
            set
            {
                if (value != _RecoveryMaster)
                {
                    _RecoveryMaster = value;
                }
            }
        }

        public static EventCodeEnum InitModule()
        {
            try
            {
                RecoveryMaster = new LoaderRecoveryMaster();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public static bool Show(Autofac.IContainer container, string ErrorMsg, string details, string recoveryBeh = "", int cellIdx = -1)
        {
            bool isCheck = false;
            try
            {
                if (LoaderContainer == null)
                {
                    LoaderContainer = container;
                    _GPLoader= container.Resolve<IGPLoader>();
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    RecoveryMaster.SetRecoveryBehavior(recoveryBeh, cellIdx);

                    MainWindow wd = new MainWindow(1, ErrorMsg, details, RecoveryMaster);
                    wd.DataContext = RecoveryMaster;
                    wd.ShowDialog();
                    isCheck = wd.IsCheck;
                    if (isCheck)
                    {
                        _GPLoader.LoaderBuzzer(false);

                        LoaderRecoveryOperateVM.Show(LoaderContainer);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }
        public static bool Show(Autofac.IContainer container)
        {
            bool isCheck = false;
            try
            {
                if (LoaderContainer == null)
                {
                    LoaderContainer = container;
                }
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow wd = new MainWindow();
                    wd.ShowDialog();
                    isCheck = wd.IsCheck;
                    
                });
                if (isCheck)
                {
                    LoaderRecoveryOperateVM.Show(LoaderContainer);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }


    }
}
