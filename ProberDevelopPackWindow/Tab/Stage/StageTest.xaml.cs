namespace ProberDevelopPackWindow.Tab
{
    using ProberInterfaces;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using LogModule;
    using System.Threading;

    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StageTest : UserControl, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public StageTest()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region Property
        private int _BridgeContinueCount;
        public int BridgeContinueCount
        {
            get { return _BridgeContinueCount; }
            set
            {
                if (value != _BridgeContinueCount)
                {
                    _BridgeContinueCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _BridgeContinueDelay; // ms
        public int BridgeContinueDelay
        {
            get { return _BridgeContinueDelay; }
            set
            {
                if (value != _BridgeContinueDelay)
                {
                    _BridgeContinueDelay = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool bridgecontinueflag = false;
        #endregion

        private void btn_bridgecontinuestart_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                this.StageSupervisor().StageModuleState.ZCLEARED();

                Task.Run(() =>
                {
                    for (int i = 0; i < BridgeContinueCount; i++)
                    {
                        this.StageSupervisor().StageModuleState.SetWaferCamBasePos(true);
                        this.StageSupervisor().StageModuleState.SetWaferCamBasePos(false);
                        Thread.Sleep(BridgeContinueDelay);

                        if (bridgecontinueflag == true)
                        {
                            bridgecontinueflag = false;
                            break;
                        }
                        if(i +1 == BridgeContinueCount)
                        {
                            bridgecontinueflag = false;
                        }
                    }
                });




            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_bridgecontinuestop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bridgecontinueflag = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
