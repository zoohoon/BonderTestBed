using LoaderBase;
using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using LoaderBase.Communication;
using RelayCommandBase;
using System.Windows.Input;

namespace TestSimulationDialog
{
    public class TestSimulationDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private object portLock = new object();


        #region Properties

        #endregion

        #region Commands

        private RelayCommand<object[]> _SetForcedIOCommand;
        public ICommand SetForcedIOCommand
        {
            get
            {
                if (null == _SetForcedIOCommand) _SetForcedIOCommand = new RelayCommand<object[]>(SetForcedIOCommandFunc);
                return _SetForcedIOCommand;
            }
        }

        #endregion

        #region Functions
        private void SetForcedIOCommandFunc(object[] param)
        {
            try
            {
                var collection = param[0] as ObservableCollection<ObservableCollection<IOPortDescripter<bool>>>;

                if (collection != null)
                {
                    ObservableCollection<IOPortDescripter<bool>> iOPorts = param[1] as ObservableCollection<IOPortDescripter<bool>>;

                    if (iOPorts != null)
                    {
                        var itemIndex = collection.IndexOf(iOPorts);

                        var iOPortProxy = LoaderCommunicationManager.GetProxy<IIOPortProxy>(itemIndex + 1);

                        if (iOPortProxy != null)
                        {
                            IOPortDescripter<bool> iOPort = param[2] as IOPortDescripter<bool>;

                            if (iOPort != null)
                            {
                                iOPortProxy.SetForcedIO(iOPort, iOPort.ForcedIO.IsForced, iOPort.ForcedIO.ForecedValue);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    RaisePropertyChanged();

                    SearchMatched();
                }
            }
        }

        private int _SelectedCellIndex;
        public int SelectedCellIndex
        {
            get { return _SelectedCellIndex; }
            set
            {
                if (value != _SelectedCellIndex)
                {
                    _SelectedCellIndex = value;
                    RaisePropertyChanged();

                    SearchMatched();
                }
            }
        }


        private ObservableCollection<IOPortDescripter<bool>> _LoaderIOs = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> LoaderIOs
        {
            get { return _LoaderIOs; }
            set
            {
                if (value != _LoaderIOs)
                {
                    _LoaderIOs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IOPortDescripter<bool>> _LoaderFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> LoaderFilteredPorts
        {
            get { return _LoaderFilteredPorts; }
            set
            {
                if (value != _LoaderFilteredPorts)
                {
                    _LoaderFilteredPorts = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ObservableCollection<IOPortDescripter<bool>>> _CellsIOs = new ObservableCollection<ObservableCollection<IOPortDescripter<bool>>>();
        public ObservableCollection<ObservableCollection<IOPortDescripter<bool>>> CellsIOs
        {
            get { return _CellsIOs; }
            set
            {
                if (value != _CellsIOs)
                {
                    _CellsIOs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<ObservableCollection<IOPortDescripter<bool>>> _FilteredCellsIOs = new ObservableCollection<ObservableCollection<IOPortDescripter<bool>>>();
        public ObservableCollection<ObservableCollection<IOPortDescripter<bool>>> FilteredCellsIOs
        {
            get { return _FilteredCellsIOs; }
            set
            {
                if (value != _FilteredCellsIOs)
                {
                    _FilteredCellsIOs = value;
                    RaisePropertyChanged();
                }
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
                    RaisePropertyChanged();

                    SearchMatched();
                }
            }
        }

        private ILoaderModule _LoaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _LoaderModule; }
            set
            {
                if (value != _LoaderModule)
                {
                    _LoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ILoaderSupervisor LoaderMaster => _Container.Resolve<ILoaderSupervisor>();
        private ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();


        public TestSimulationDialogViewModel()
        {
            try
            {
                LoaderModule = _Container.Resolve<ILoaderModule>();

                GetLoaderRemoteInputs();

                GetCellsInputs();

               
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetLoaderRemoteInputs()
        {
            try
            {
                if (LoaderIOs == null)
                {
                    LoaderIOs = new ObservableCollection<IOPortDescripter<bool>>();
                }

                LoaderIOs.Clear();

                var inputs = LoaderModule.IOManager.IOMappings.RemoteInputs;
                var inputType = inputs.GetType();
                var props = inputType.GetProperties();

                foreach (var item in props)
                {
                    //var port = item.GetValue(inputs) as List<IOPortDescripter<bool>>;
                    if (item.PropertyType == typeof(List<IOPortDescripter<bool>>))
                    {
                        var ios = item.GetValue(inputs) as List<IOPortDescripter<bool>>;

                        if (ios != null)
                        {
                            foreach (var port in ios)
                            {
                                if (port is IOPortDescripter<bool>)
                                {
                                    LoaderIOs.Add(port);
                                }
                            }
                        }
                    }
                    else if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        var iodesc = item.GetValue(inputs) as IOPortDescripter<bool>;
                        LoaderIOs.Add(iodesc);
                    }
                }

                SearchMatched();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GetCellsInputs()
        {
            try
            {
                CellsIOs?.Clear();

                for (int i = 0; i < SystemModuleCount.ModuleCnt.StageCount; i++)
                {
                    CellsIOs.Add(new ObservableCollection<IOPortDescripter<bool>>());
                    FilteredCellsIOs.Add(new ObservableCollection<IOPortDescripter<bool>>());
                }

                for (int index = 0; index < SystemModuleCount.ModuleCnt.StageCount; index++)
                {
                    int nCellIndex = index + 1;
                    var client = LoaderMaster.GetClient(nCellIndex);

                    if (client != null)
                    {
                        var iOPortProxy = LoaderCommunicationManager.GetProxy<IIOPortProxy>(nCellIndex);

                        if (iOPortProxy != null)
                        {
                            var list = iOPortProxy.GetInputPorts();

                            foreach (var item in list)
                            {
                                item.ForcedIO = new ForcedIOValue();
                                CellsIOs[index].Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void SearchMatched()
        {
            try
            {
                string upper = "";
                string lower = "";
                int keyLength = 0;

                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (SearchKeyword != null)
                    {
                        upper = SearchKeyword.ToUpper();
                        lower = SearchKeyword.ToLower();
                        keyLength = SearchKeyword.Length;
                    }
                });
                
                // LOADER
                if (SelectedIndex == 0)
                {
                    if (LoaderFilteredPorts == null)
                    {
                        LoaderFilteredPorts = new ObservableCollection<IOPortDescripter<bool>>();
                    }

                    await Task.Run(() =>
                    {
                        if (keyLength > 0)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                lock (portLock)
                                {
                                    if (LoaderIOs != null)
                                    {
                                        var outs = LoaderIOs.Where(
                                            t => t.Key.Value.StartsWith(upper) ||
                                            t.Key.Value.StartsWith(lower) ||
                                            t.Key.Value.ToUpper().Contains(upper));

                                        var filtered = new ObservableCollection<IOPortDescripter<bool>>(outs);

                                        LoaderFilteredPorts.Clear();

                                        foreach (var item in filtered)
                                        {
                                            LoaderFilteredPorts.Add(item);
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {
                            lock (portLock)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    if (LoaderIOs != null)
                                    {
                                        LoaderFilteredPorts.Clear();

                                        foreach (var item in LoaderIOs)
                                        {
                                            LoaderFilteredPorts.Add(item);
                                        }
                                    }
                                });
                            }
                        }
                    });
                }
                else // CELL
                {
                    await Task.Run(() =>
                    {
                        if (keyLength > 0)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                lock (portLock)
                                {
                                    if (CellsIOs[SelectedCellIndex] != null)
                                    {
                                        var outs = CellsIOs[SelectedCellIndex].Where(
                                            t => t.Key.Value.StartsWith(upper) ||
                                            t.Key.Value.StartsWith(lower) ||
                                            t.Key.Value.ToUpper().Contains(upper));

                                        var filtered = new ObservableCollection<IOPortDescripter<bool>>(outs);

                                        FilteredCellsIOs[SelectedCellIndex].Clear();

                                        foreach (var item in filtered)
                                        {
                                            FilteredCellsIOs[SelectedCellIndex].Add(item);
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {
                            lock (portLock)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    if (CellsIOs[SelectedCellIndex] != null)
                                    {
                                        FilteredCellsIOs[SelectedCellIndex].Clear();

                                        foreach (var item in CellsIOs[SelectedCellIndex])
                                        {
                                            FilteredCellsIOs[SelectedCellIndex].Add(item);
                                        }
                                    }
                                });
                            }
                        }
                    });
                }   
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
