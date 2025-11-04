using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LogService
{
    using LogModule;
    using NLog;
    using System.ComponentModel;

    /// <summary>
    /// LogViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogViewer : UserControl, ILogViewer, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _SearchKeyword = string.Empty;
        public string SearchKeyword
        {
            get { return _SearchKeyword; }
            set
            {
                if (value != _SearchKeyword)
                {
                    _SearchKeyword = value;
                    NotifyPropertyChanged("SearchKeyword");
                    SearchMatched();
                }
            }
        }

        public List<LogEventInfo> LogCollection { get; private set; } = new List<LogEventInfo>();
        public ObservableCollection<LogEventInfo> LogInfos { get; private set; } = new ObservableCollection<LogEventInfo>();

        public ObservableCollection<LogEventInfo> FilteredLogInfos { get; private set; } = new ObservableCollection<LogEventInfo>();


        private async void SearchMatched()
        {
            string upper = SearchKeyword.ToUpper();
            //string lower = SearchKeyword.ToLower();

            try
            {
                await Task.Run(() =>
                {
                    if (SearchKeyword.Length > 0)
                    {
                        var filtered = LogCollection.Where(t =>
                            t.FormattedMessage.ToUpper().Contains(upper)
                        | t.Level.ToString().ToUpper().Contains(upper));
                        if (filtered != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredLogInfos.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredLogInfos.Add(item);
                                }
                                MaterialDesignThemes.Wpf.DrawerHost.OpenDrawerCommand.Execute(null, Drawer);
                                FilteredLogView.Visibility = Visibility.Visible;
                                logView.Visibility = Visibility.Hidden;
                            });
                        }


                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            MaterialDesignThemes.Wpf.DrawerHost.CloseDrawerCommand.Execute(null, Drawer);
                            FilteredLogView.Visibility = Visibility.Hidden;
                            logView.Visibility = Visibility.Visible;
                            FilteredLogInfos.Clear();
                        });
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public LogViewer()
        {
            try
            {
                InitializeComponent();
                DataContext = this;

                SearchKeyword = "";

                Binding binding = new Binding();
                binding.Path = new PropertyPath("LogInfos");
                binding.Source = this;
                logView.SetBinding(ListView.ItemsSourceProperty, binding);

                binding = new Binding();
                binding.Path = new PropertyPath("FilteredLogInfos");
                binding.Source = this;
                FilteredLogView.SetBinding(ListView.ItemsSourceProperty, binding);

                //Binding keyBinding = new Binding();
                //keyBinding.Path = new PropertyPath("SearchKeyword");
                //keyBinding.Mode = BindingMode.TwoWay;
                //keyBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                //keyBinding.Source = this;
                //SearchTextBox.SetBinding(ListView.ItemsSourceProperty, keyBinding);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }
        public void Target_EventReceived(LogEventInfo log)
        {
            try
            {
                if (log != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (LogCollection.Count >= 5000)
                        //if (Collection.Count <= 10)
                        {
                            //LogCollection.Clear();
                            //LogInfos.Clear();
                            //LogCollection.RemoveAt(0);
                            //LogInfos.RemoveAt(0);
                        }

                        //LogCollection.Add(log);
                        //LogInfos.Add(log);

                        if (logView.Items.Count > 0)
                            {
                            //logView.ScrollIntoView(logView.Items[logView.Items.Count - 1]);
                        }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
