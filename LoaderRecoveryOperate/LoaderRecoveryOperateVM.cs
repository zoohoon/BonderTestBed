using Autofac;
using LoaderBase;
using LoaderCore;
using LogModule;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoaderRecoveryOperate
{
    public class LoaderRecoveryOperateVM
    {
        #region ==> PropertyChanged
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void RaiseStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public static ILoaderModule loaderModule = null;
        public static ILoaderSupervisor LoaderMaster = null;
        private static Autofac.IContainer LoaderContainer = null;
        private static MainWindow window;
        public static bool Show(Autofac.IContainer container)
        {
            bool isCheck = false;
            try
            {
                if (LoaderContainer == null)
                {
                    LoaderContainer = container;
                }
                loaderModule = LoaderContainer.Resolve<ILoaderModule>();
                LoaderMaster = LoaderContainer.Resolve<ILoaderSupervisor>();
                loaderModule.ResetUnknownModule();
                String retValue = String.Empty;
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    window = new MainWindow(container);
                    window.ShowDialog();
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return isCheck;
        }

        private static bool _IsEnableExitBtn = true;
        public static bool IsEnableExitBtn
        {
            get { return _IsEnableExitBtn; }
            set
            {
                if (value != _IsEnableExitBtn)
                {
                    _IsEnableExitBtn = value;
                    RaiseStaticPropertyChanged();
                }
            }
        }


        public int Cancel { get; set; }
        private static AsyncCommand _LoaderInitCommnad;

        public static ICommand LoaderInitCommnad
        {
            get
           {
                if (null == _LoaderInitCommnad) _LoaderInitCommnad = new AsyncCommand(LoaderInitFunc);
                return _LoaderInitCommnad;
            }
        }

        private static async Task LoaderInitFunc()
        {
            try
            {
                IsEnableExitBtn = false;
                await Task.Run(() =>
                {
                    loaderModule.GetLoaderCommands().ResetRobotCommand();
                    (loaderModule as LoaderModule).ClearRequestData();
                    LoaderMaster.ClearState();
                    LoggerManager.Debug($"[LoaderStateInitBtn_Click] LoaderJobViewList.Clear.");
                    (loaderModule as LoaderModule).LoaderJobViewList.Clear();
                        
                });

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    window.Close();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                IsEnableExitBtn = true;
            }
        }

        private static AsyncCommand _ExitCommand;

        public static ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new AsyncCommand(ExitFunc);
                return _ExitCommand;
            }
        }

        private static async Task ExitFunc()
        {
            try
            {
                //invoke
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    window.Close();
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
