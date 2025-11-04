using LoaderParameters;
using LogModule;
using System;
using System.Windows;
using System.ComponentModel;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RelayCommandBase;
using System.Threading.Tasks;
using LoaderMaster;
using LoaderBase;
using ProberInterfaces;
using Autofac;

namespace AlignRecoveryViewDialog
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        
       private ObservableCollection<AlignRecoveryData> _RecoveryDataList=new ObservableCollection<AlignRecoveryData>();
        public ObservableCollection<AlignRecoveryData> RecoveryDataList
        {
            get { return _RecoveryDataList; }
            set
            {
                if (value != _RecoveryDataList)
                {
                    _RecoveryDataList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsCheck = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        public ILoaderSupervisor LoaderMaster;
        public MainWindow(Autofac.IContainer container, StageLotData lotData, int index) : this()
        {
            try
            {
                LoaderMaster= container.Resolve<ILoaderSupervisor>();
                Index = index;
                RecoveryDataList = new ObservableCollection<AlignRecoveryData>();
                AlignRecoveryData waferData = new AlignRecoveryData();
                AlignRecoveryData pinData = new AlignRecoveryData();
                waferData.ModuleName = "Wafer Align";
                waferData.State = lotData.WaferAlignState;
                if(waferData.State.Equals("FAIL"))
                {
                    waferData.IsRecovery = true;
                }else
                {
                    waferData.IsRecovery = false;
                }
                pinData.ModuleName = "Pin Align";
                pinData.State = lotData.PinAlignState;

                if (pinData.State.Equals("FAIL"))
                {
                    pinData.IsRecovery = true;
                }
                else
                {
                    pinData.IsRecovery = false;
                }

                RecoveryDataList.Add(waferData);
                RecoveryDataList.Add(pinData);
                DataContext = this;
                Loaded += ToolWindow_Loaded;
                WindowStartupLocation = WindowStartupLocation.Manual;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Code to remove close box from window
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private AsyncCommand<string> _GoToRecoveryCommand;
        public ICommand GoToRecoveryCommand
        {
            get
            {
                if (null == _GoToRecoveryCommand)
                {
                    _GoToRecoveryCommand = new AsyncCommand<string>(GoToRecoveryCommandFunc);
                }
                return _GoToRecoveryCommand;
            }
        }
        private async Task GoToRecoveryCommandFunc(string Module)
        {
            try
            {
                if (Module.Contains("Wafer"))
                {
                    await Task.Run(async () =>
                    {
                        this.WaferAligner().IsNewSetup = false;
                        await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                        await this.PnPManager().SettingRemoteRecoveryPNP("WaferAlign", "IWaferAligner", new Guid("D0A33FFE-DD22-4572-5B69-73F66C38CEB4"), true);
                    });
                }
                else if (Module.Contains("Pin"))
                {

                }
                    

                this.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsCheck = false;
                this.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        
    }

   
    public class AlignRecoveryData : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _ModuleName;
        public string ModuleName
        {
            get { return _ModuleName; }
            set
            {
                if (value != _ModuleName)
                {
                    _ModuleName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _State;
        public string State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsRecovery;
        public bool IsRecovery
        {
            get { return _IsRecovery; }
            set
            {
                if (value != _IsRecovery)
                {
                    _IsRecovery = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
