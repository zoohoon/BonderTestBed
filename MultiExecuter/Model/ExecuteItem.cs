using Newtonsoft.Json;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Xml.Serialization;
using LogModule;


namespace MultiExecuter.Model
{
    [Serializable]
    public class ExecuteItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public ExecuteItem(int cellnum)
        {
            this.CellNum = cellnum;
        }
        //[XmlElement]
        //private string _CellIp;
        //public string CellIp
        //{
        //    get { return _CellIp; }
        //    set
        //    {
        //        if (value != _CellIp)
        //        {
        //            _CellIp = value;
        //            NotifyPropertyChanged(nameof(CellIp));
        //        }
        //    }
        //}

        [XmlElement]
        private int _CellNum;
        public int CellNum
        {
            get { return _CellNum; }
            set
            {
                if (value != _CellNum)
                {
                    _CellNum = value;
                    NotifyPropertyChanged(nameof(CellNum));
                }
            }
        }

        [XmlElement]
        private string _Path;
        public string Path
        {
            get { return _Path; }
            set
            {
                if (value != _Path)
                {
                    _Path = value;
                    NotifyPropertyChanged(nameof(Path));
                }
            }
        }

      

        [XmlElement]
        private bool _ConnectCheck;
        public bool ConnectCheck
        {
            get { return _ConnectCheck; }
            set
            {
                if (value != _ConnectCheck)
                {
                    _ConnectCheck = value;
                    NotifyPropertyChanged(nameof(ConnectCheck));

                    //RequestSave?.Invoke();
                }
            }
        }

        [XmlElement]
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    NotifyPropertyChanged(nameof(IsChecked));

                    //RequestSave?.Invoke();
                }
            }
        }
        
        private Process _process;
        [XmlIgnore, JsonIgnore]
        public Process process
        {
            get { return _process; }
            set
            {
                if (value != _process)
                {
                    _process = value;
                    NotifyPropertyChanged(nameof(process));
                }
            }
        }

        private bool _IsConnected;
        [XmlIgnore, JsonIgnore]
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                    NotifyPropertyChanged(nameof(IsConnected));
                }
            }
        }

        private bool _Accessible;
        [XmlIgnore, JsonIgnore]
        public bool Accessible
        {
            get { return _Accessible; }
            set
            {
                if (value != _Accessible)
                {
                    _Accessible = value;
                    NotifyPropertyChanged(nameof(Accessible));
                }
            }
        }
        private EnumCellProcessState _CellState;
        [XmlIgnore, JsonIgnore]
        public EnumCellProcessState CellState
        {
            get { return _CellState; }
            set
            {
                if (value != _CellState)
                {
                    _CellState = value;
                    NotifyPropertyChanged(nameof(CellState));
                }
            }
        }
        private EnumCellProcessState _CellPrevState;
        [XmlIgnore, JsonIgnore]
        public EnumCellProcessState CellPrevState
        {
            get { return _CellPrevState; }
            set
            {
                if (value != _CellPrevState)
                {
                    _CellPrevState = value;
                    NotifyPropertyChanged(nameof(CellPrevState));
                }
            }
        }
        //[JsonIgnore]
        //private int _processid;
        //public int processid
        //{
        //    get { return _processid; }
        //    set
        //    {
        //        if (value != _processid)
        //        {
        //            _processid = value;
        //            NotifyPropertyChanged(nameof(processid));
        //        }
        //    }
        //}

        public delegate void RequestSaveDelegate();
        [XmlIgnore, JsonIgnore]
        //public RequestSaveDelegate RequestSave { get; set; }
        private RelayCommand<object> _FindPathCommand;
        [JsonIgnoreAttribute]
        public ICommand FindPathCommand
        {
            get
            {
                if (null == _FindPathCommand) _FindPathCommand = new RelayCommand<object>(FindPath);
                return _FindPathCommand;
            }
        }

        [XmlIgnore, JsonIgnore]
        //public RequestSaveDelegate RequestSave { get; set; }
        private RelayCommand<object> _FindPathCommand_Log;
        [JsonIgnoreAttribute]
        public ICommand FindPathCommand_Log
        {
            get
            {
                if (null == _FindPathCommand_Log) _FindPathCommand_Log = new RelayCommand<object>(FindPath);
                return _FindPathCommand_Log;
            }
        }
        private void FindPath(object obj)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (dialog.SelectedPath != "")
                    {
                        Path = dialog.SelectedPath;
                    }
                }
                //RequestSave?.Invoke();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
