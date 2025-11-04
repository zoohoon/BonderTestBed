using LogModule;
using SplasherService;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SplashWindowViewModel
{
    public class SplashWindowVM : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


        //private readonly BackgroundWorker worker;
        //private readonly ICommand instigateWorkCommand;

        private string _Version;
        public string Version
        {
            get { return _Version; }
            set
            {
                if (value != _Version)
                {
                    _Version = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CurrentProgress;
        public double CurrentProgress
        {
            get
            {
                return _CurrentProgress;
            }
            set
            {
                if (value != _CurrentProgress)
                {

                    _CurrentProgress = value;
                    RaisePropertyChanged();
                    DispatcherHelper.DoEvents();
                }
            }
        }

        private bool _IsStoryboardCompleted;
        public bool IsStoryboardCompleted
        {
            get
            {
                return _IsStoryboardCompleted;
            }
            set
            {
                if (value != _IsStoryboardCompleted)
                {

                    _IsStoryboardCompleted = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _IsDBUpdate;
        public string IsDBUpdate
        {
            get { return _IsDBUpdate; }
            set
            {
                if (value != _IsDBUpdate)
                {
                    _IsDBUpdate = value;
                    RaisePropertyChanged();
                }
            }
        }


        public SplashWindowVM()
        {
            try
            {
                //this.instigateWorkCommand =
                //        new DelegateCommand(o => this.worker.RunWorkerAsync(),
                //                            o => !this.worker.IsBusy);

                //this.worker = new BackgroundWorker();
                //this.worker.DoWork += this.DoWork;
                //this.worker.ProgressChanged += this.ProgressChanged;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            // do time-consuming work here, calling ReportProgress as and when you can
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.CurrentProgress = e.ProgressPercentage;
        }
    }
}
