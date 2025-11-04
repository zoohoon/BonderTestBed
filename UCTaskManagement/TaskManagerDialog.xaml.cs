using LogModule;
using MetroDialogInterfaces;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UCTaskManagement
{
    /// <summary>
    /// TaskManagerDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TaskManagerDialog : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private static TaskManagerDialog View;

        public static TaskManagerDialog GetInstance()
        {
            if (View == null)
                try
                {
                    {
                        View = new TaskManagerDialog();
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            return View;
        }

        public ISequenceEngineManager SequenceEngineManager
        {
            get;
            set;
        }

        public TaskManagerDialog()
        {
            try
            {
                InitializeComponent();
                SequenceEngineManager = Extensions_IModule.SequenceEngineManager(null);
                DataContext = this;
                Closing += OnWindowClosing;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }





        private float _ZoomLevel;
        public float ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _IsLotModule = true;
        public bool IsLotModule
        {
            get { return _IsLotModule; }
            set
            {
                if (value != _IsLotModule)
                {
                    _IsLotModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsLoaderModule = true;
        public bool IsLoaderModule
        {
            get { return _IsLoaderModule; }
            set
            {
                if (value != _IsLoaderModule)
                {
                    _IsLoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _IsIModule = true;
        public bool IsIModule
        {
            get { return _IsIModule; }
            set
            {
                if (value != _IsIModule)
                {
                    _IsIModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool? _ContinueLotFlag;
        public bool? ContinueLotFlag
        {
            get { return _ContinueLotFlag; }
            set
            {
                if (value != _ContinueLotFlag)
                {
                    Extensions_IModule.LotOPModule(null).LotInfo.ContinueLot = value;
                    _ContinueLotFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand<object> _cmdZoomLevel1;
        public ICommand cmdZoomLevel1
        {
            get
            {
                if (null == _cmdZoomLevel1) _cmdZoomLevel1 = new RelayCommand<object>(ZoomLevel1Func);
                return _cmdZoomLevel1;
            }
        }
        public void ZoomLevel1Func(object noparam)
        {
            ZoomLevel = 1;
        }
        private RelayCommand<object> _cmdZoomLevel2;
        public ICommand cmdZoomLevel2
        {
            get
            {
                if (null == _cmdZoomLevel2) _cmdZoomLevel2 = new RelayCommand<object>(ZoomLevel2Func);
                return _cmdZoomLevel2;
            }
        }
        public void ZoomLevel2Func(object noparam)
        {
            ZoomLevel = 2;
        }
        private RelayCommand<object> _cmdZoomLevel3;
        public ICommand cmdZoomLevel3
        {
            get
            {
                if (null == _cmdZoomLevel3) _cmdZoomLevel3 = new RelayCommand<object>(ZoomLevel3Func);
                return _cmdZoomLevel3;
            }
        }
        public void ZoomLevel3Func(object noparam)
        {
            ZoomLevel = 3;
        }
        private RelayCommand<object> _cmdZoomLevel4;
        public ICommand cmdZoomLevel4
        {
            get
            {
                if (null == _cmdZoomLevel4) _cmdZoomLevel4 = new RelayCommand<object>(ZoomLevel4Func);
                return _cmdZoomLevel4;
            }
        }
        public void ZoomLevel4Func(object noparam)
        {
            ZoomLevel = 4;
        }
        private RelayCommand<object> _cmdZoomLevel5;
        public ICommand cmdZoomLevel5
        {
            get
            {
                if (null == _cmdZoomLevel5) _cmdZoomLevel5 = new RelayCommand<object>(ZoomLevel5Func);
                return _cmdZoomLevel5;
            }
        }
        public void ZoomLevel5Func(object noparam)
        {
            ZoomLevel = 5;
        }
        private RelayCommand<object> _cmdZoomLevel6;
        public ICommand cmdZoomLevel6
        {
            get
            {
                if (null == _cmdZoomLevel6) _cmdZoomLevel6 = new RelayCommand<object>(ZoomLevel6Func);
                return _cmdZoomLevel6;
            }
        }
        public void ZoomLevel6Func(object noparam)
        {
            ZoomLevel = 6;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _SystemInitCommand;
        public ICommand SystemInitCommand
        {
            get
            {
                if (null == _SystemInitCommand) _SystemInitCommand = new AsyncCommand(SystemInit);
                return _SystemInitCommand;
            }
        }



        private async Task SystemInit()
        {
            try
            {
                ////Code block#1//call thread

                //LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                //var ret = await this.ViewModelManager().Prober._StageSuperVisor.SystemInit(); ////Code bloc#2 - //workqueue task threand

                //LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");

                ////Code block#3//call thread

                EnumMessageDialogResult ret;

                if (SequenceEngineManager.SequenceEngineManager().GetMovingState() == true)
                {
                    ret = await SequenceEngineManager.MetroDialogManager().ShowMessageDialog("Machine Initialize", "Are you sure you want to Machine Initialize?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                        var w = await SequenceEngineManager.StageSupervisor().SystemInit();

                        LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");
                    }
                }
                else
                {
                    ret = await SequenceEngineManager.MetroDialogManager().ShowMessageDialog("Machine Initialize", "Can not Initialize.", EnumMessageStyle.AffirmativeAndNegative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        private AsyncCommand _LoaderInitCommand;
        public ICommand LoaderInitCommand
        {
            get
            {
                if (null == _LoaderInitCommand) _LoaderInitCommand = new AsyncCommand(LoaderInit);
                return _LoaderInitCommand;
            }
        }



        private async Task LoaderInit()
        {
            try
            {
                ////Code block#1//call thread

                //LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                //var ret = await this.ViewModelManager().Prober._StageSuperVisor.SystemInit(); ////Code bloc#2 - //workqueue task threand

                //LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");

                ////Code block#3//call thread

                EnumMessageDialogResult ret;

                if (SequenceEngineManager.SequenceEngineManager().GetMovingState() == true)
                {
                    ret = await SequenceEngineManager.MetroDialogManager().ShowMessageDialog("Loader Init", "Are you sure you want to Loader Initialize?", EnumMessageStyle.AffirmativeAndNegative);

                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                        SequenceEngineManager.LoaderController().LoaderSystemInit();

                        LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");
                    }
                }
                else
                {
                    ret = await SequenceEngineManager.MetroDialogManager().ShowMessageDialog("Loader Init", "Can not quit.", EnumMessageStyle.AffirmativeAndNegative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //private RelayCommand<CancelEventArgs> _Window_Closing;
        //public ICommand WindowClosing
        //{
        //    get
        //    {
        //        if (null == _Window_Closing) _Window_Closing = new RelayCommand<CancelEventArgs>(Window_ClosingFunc);
        //        return _Window_Closing;
        //    }
        //}
        //private void Window_ClosingFunc(CancelEventArgs e)
        //{

        //}

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
                // Handle closing logic, set e.Cancel as needed
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
