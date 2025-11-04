using System;
using System.ComponentModel;

using System.Collections.ObjectModel;

namespace AppSelector.ViewModel
{
    using LogModule;
    using System.Windows.Input;
    using System.Windows.Threading;

    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }


        private ObservableCollection<AppModuleItem> _AppItems
             = new ObservableCollection<AppModuleItem>();
        public ObservableCollection<AppModuleItem> AppItems
        {
            get { return _AppItems; }
            set { this.MutateVerbose(ref _AppItems, value, RaisePropertyChanged()); }
        }
        public ICommand SaveComand { get; }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.MutateVerbose(ref _isSaving, value, RaisePropertyChanged()); }
        }

        private bool _isSaveComplete;
        public bool IsSaveComplete
        {
            get { return _isSaveComplete; }
            private set { this.MutateVerbose(ref _isSaveComplete, value, RaisePropertyChanged()); }
        }

        private double _saveProgress;
        public double SaveProgress
        {
            get { return _saveProgress; }
            private set { this.MutateVerbose(ref _saveProgress, value, RaisePropertyChanged()); }
        }
        public MainViewModel()
        {
            try
            {
                AppItems = new ObservableCollection<AppModuleItem>();
                //AppItems.Add(new AppModuleItem("Lot", "O", "Lot Opertaion", new UcTest { DataContext = this }));
                //AppItems.Add(new AppModuleItem("Statistics", "S", "Statistics View", new UcTest2 { DataContext = this }));
                //AppItems.Add(new AppModuleItem("Inspection", "I", "PM Inspection", new UserControlInspection { DataContext = this }));
                //AppItems.Add(new AppModuleItem("Sequences", "Q", "Sequence Editor", new UserControlSequences { DataContext = this }));

                int badge = 0;
                Random rnd = new Random();


                foreach (var item in AppItems)
                {
                    badge = rnd.Next(0, 25);
                    item.Badge = badge.ToString();
                    if (item.Name != "Loader") item.IsSelected = true;
                }

                SaveComand = new AnotherCommandImplementation(_ =>
                {
                    if (IsSaveComplete == true)
                    {
                        IsSaveComplete = false;
                        return;
                    }

                    if (SaveProgress != 0) return;

                    var started = DateTime.Now;
                    IsSaving = true;

                    new DispatcherTimer(
                        TimeSpan.FromMilliseconds(50),
                        DispatcherPriority.Normal,
                        new EventHandler((o, e) =>
                        {
                            var totalDuration = started.AddSeconds(3).Ticks - started.Ticks;
                            var currentProgress = DateTime.Now.Ticks - started.Ticks;
                            var currentProgressPercent = 100.0 / totalDuration * currentProgress;

                            SaveProgress = currentProgressPercent;

                            if (SaveProgress >= 100)
                            {
                                IsSaveComplete = true;
                                IsSaving = false;
                                SaveProgress = 0;
                                ((DispatcherTimer)o).Stop();
                            }

                        }), Dispatcher.CurrentDispatcher);
                    System.Threading.Thread.Sleep(1000);
                    AppItems.Add(new AppModuleItem("Loader", "L", "Loader control", null));

                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public MainViewModel(ObservableCollection<AppModuleItem> appimtes)
        {
            try
            {
                AppItems = appimtes;
                SaveComand = new AnotherCommandImplementation(_ =>
                {
                    if (IsSaveComplete == true)
                    {
                        IsSaveComplete = false;
                        return;
                    }

                    if (SaveProgress != 0) return;

                    var started = DateTime.Now;
                    IsSaving = true;

                    new DispatcherTimer(
                        TimeSpan.FromMilliseconds(50),
                        DispatcherPriority.Normal,
                        new EventHandler((o, e) =>
                        {
                            var totalDuration = started.AddSeconds(3).Ticks - started.Ticks;
                            var currentProgress = DateTime.Now.Ticks - started.Ticks;
                            var currentProgressPercent = 100.0 / totalDuration * currentProgress;

                            SaveProgress = currentProgressPercent;

                            if (SaveProgress >= 100)
                            {
                                IsSaveComplete = true;
                                IsSaving = false;
                                SaveProgress = 0;
                                ((DispatcherTimer)o).Stop();
                            }

                        }), Dispatcher.CurrentDispatcher);
                    System.Threading.Thread.Sleep(1000);
                    AppItems.Add(new AppModuleItem("Loader", "L", "Loader control", null));

                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }
}
