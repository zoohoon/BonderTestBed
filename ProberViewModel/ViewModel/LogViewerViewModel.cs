using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LogViewerVM
{
    using LogModule;
    using Microsoft.Win32;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    //using Microsoft.Practices.Prism.Commands;

    public class LogViewerViewModel : Window, IMainScreenViewModel, IParamScrollingViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("29342882-1143-477c-90b6-062460bea1b3");

        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

        #region ==> ParamSearchBoxText
        private String _ParamSearchBoxText;
        public String ParamSearchBoxText
        {
            get { return _ParamSearchBoxText; }
            set
            {
                if (value != _ParamSearchBoxText)
                {
                    _ParamSearchBoxText = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private ObservableCollection<string> fileObjectCollection
            = new ObservableCollection<string>();
        public ObservableCollection<string> FileObjectCollection
        {
            get { return fileObjectCollection; }
            set
            {
                if (value != this.fileObjectCollection)
                    fileObjectCollection = value;
                RaisePropertyChanged("");
            }
        }

        private string _FileData;
        public string FileData
        {
            get { return _FileData; }
            set
            {
                if (value != _FileData)
                {
                    _FileData = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private string _SelectedItem;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (value != _SelectedItem)
                {
                    _SelectedItem = value;
                    RaisePropertyChanged("");
                }
            }
        }

        private string _FolderPath;
        public string FolderPath
        {
            get { return _FolderPath; }
            set
            {
                if (value != _FolderPath)
                {
                    _FolderPath = value;
                    RaisePropertyChanged("");
                }
            }
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

        //public bool HasParameterToSave()
        //{
        //    return false;
        //}

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    FolderPath = @"C:\Logs\Debug\2018-09-17\";

                    if (Directory.Exists(Path.GetDirectoryName(FolderPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FolderPath));
                    }

                    string[] fileEntries = Directory.GetFiles(FolderPath);

                    foreach (string fileName in fileEntries)
                    {
                        FileObjectCollection.Add(Path.GetFileName(fileName));
                        FolderPath = Path.GetDirectoryName(fileName);
                    }

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        //public EventCodeEnum RollBackParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //    //retVal = GPIB.SaveSysParameter();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //         throw;
        //    }
        //    return retVal;
        //}

        //public EventCodeEnum SaveParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //    //retVal = GPIB.SaveSysParameter();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //         throw;
        //    }
        //    return retVal;
        //}

        public EventCodeEnum UpProc()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            return EventCodeEnum.NONE;
        }

        private RelayCommand _FileOpenCommand;
        public ICommand FileOpenCommand
        {
            get
            {
                if (null == _FileOpenCommand) _FileOpenCommand = new RelayCommand(FileOpenCommandFunc);
                return _FileOpenCommand;
            }
        }

        private void FileOpenCommandFunc()
        {
            try
            {
                FileObjectCollection.Clear();

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        FileObjectCollection.Add(Path.GetFileName(filename));

                        FolderPath = Path.GetDirectoryName(filename);
                        //FileListBox.Items.Add(Path.GetFileName(filename));
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand _SelectionCommand;
        public ICommand SelectionCommand
        {
            get
            {
                if (null == _SelectionCommand) _SelectionCommand = new RelayCommand(SelectionCommandFunc);
                return _SelectionCommand;
            }
        }

        private void SelectionCommandFunc()
        {
            try
            {
                string select = FolderPath + "\\" + SelectedItem;

                var line = File.ReadAllText(select);
                FileData = line;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public EventCodeEnum CheckParameterToSave()
        //{
        //    return EventCodeEnum.NONE;
        //}

        #region ==> ParamSearchBoxClickCommand
        private RelayCommand _ParamSearchBoxClickCommand;
        public ICommand ParamSearchBoxClickCommand
        {
            get
            {
                if (null == _ParamSearchBoxClickCommand) _ParamSearchBoxClickCommand = new RelayCommand(ParamSearchBoxClickCommandFunc);
                return _ParamSearchBoxClickCommand;
            }
        }

        private void ParamSearchBoxClickCommandFunc()
        {
            try
            {
                String filterKeyword = VirtualKeyboard.Show(ParamSearchBoxText, KB_TYPE.DECIMAL | KB_TYPE.ALPHABET);

                if (filterKeyword == null)
                    return;

                //FilteringParameter(filterKeyword);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion
    }
}
