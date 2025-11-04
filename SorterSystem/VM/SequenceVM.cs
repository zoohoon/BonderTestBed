using LogModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SorterSystem.VM
{
    public enum EnumSequenceType
    {
        Location,
        IO,
        Vision,
        Delay,
    }

    [Serializable]
    public abstract class SequenceParamBase
    {
        [JsonProperty(Order = 1)]
        public string Name { get; set; } = "";
    }

    [Serializable]
    public class SequenceParamLocation : SequenceParamBase
    {
        [Serializable]
        public class AxisPosition
        {
            public double X { get; set; } = 0.0;
            public double Y { get; set; } = 0.0;
            public double Z { get; set; } = Double.NaN;
            public double PZ { get; set; } = Double.NaN;
            public double T { get; set; } = Double.NaN;

            public EnumProberCam Cam { get; set; } = EnumProberCam.UNDEFINED;
        }

        [JsonProperty(Order = 2)]
        public AxisPosition Position { get; set; } = new AxisPosition();
    }

    [Serializable]
    public class SequenceParamIO : SequenceParamBase
    {
        [JsonProperty(Order = 2)]
        public string Key { get; set; } = "Key";

        [JsonProperty(Order = 3)]
        public bool Value { get; set; } = false;
    }

    [Serializable]
    public class SequenceParamAction : SequenceParamBase
    {
        [JsonProperty(Order = 2)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumSequenceType Action { get; set; } = EnumSequenceType.Location;

        [JsonProperty(Order = 3)]
        public string Target { get; set; } = "";
    }


    [Serializable]
    public class SequenceJobParameter
    {
        [JsonIgnore]
        public string FilePath { get; } = @"C:\SorterSystem";

        [JsonIgnore]
        public string FileName { get; } = @"SequenceJobParameter.json";

        public List<SequenceParamLocation> LocationList { get; set; } = new List<SequenceParamLocation>();
        public List<SequenceParamIO> IOList { get; set; } = new List<SequenceParamIO>();
        public List<SequenceParamAction> ActionList { get; set; } = new List<SequenceParamAction>();
    }


    public partial class SequenceVM : IModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ISorterModuleVM _MainVM;
        public SequenceVM(ISorterModuleVM vm)
        {
            _MainVM = vm;
        }

        private EnumSequenceType _SequenceTypeSelected;
        public EnumSequenceType SequenceTypeSelected
        {
            get { return this._SequenceTypeSelected; }
            set
            {
                if (this._SequenceTypeSelected != value)
                {
                    this._SequenceTypeSelected = value;

                    HideAllSequenceTypeDataGrid();
                    UnSelectedAllSequenceTypeDataGrid();
                    switch (value)
                    {
                        case EnumSequenceType.Location: VisibleLocation = Visibility.Visible; break;
                        case EnumSequenceType.IO: VisibleIOList = Visibility.Visible; break;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibleLocation = Visibility.Hidden;
        public Visibility VisibleLocation
        {
            get { return this._VisibleLocation; }
            set
            {
                if (this._VisibleLocation != value)
                {
                    this._VisibleLocation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibleIOList = Visibility.Hidden;
        public Visibility VisibleIOList
        {
            get { return this._VisibleIOList; }
            set
            {
                if (this._VisibleIOList != value)
                {
                    this._VisibleIOList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SequenceJobParameter _SequenceJobParameter = new SequenceJobParameter();

        private ObservableCollection<SequenceParamLocation> _SequenceLocationList;
        public ObservableCollection<SequenceParamLocation> SequenceLocationList
        {
            get { return this._SequenceLocationList; }
            set
            {
                if (this._SequenceLocationList != value)
                {
                    this._SequenceLocationList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SequenceParamIO> _SequenceIOList;
        public ObservableCollection<SequenceParamIO> SequenceIOList
        {
            get { return this._SequenceIOList; }
            set
            {
                if (this._SequenceIOList != value)
                {
                    this._SequenceIOList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SequenceParamAction> _SequenceActionList;
        public ObservableCollection<SequenceParamAction> SequenceActionList
        {
            get { return this._SequenceActionList; }
            set
            {
                if (this._SequenceActionList != value)
                {
                    this._SequenceActionList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SequenceParamLocation _SequenceLocationSelected = null;
        public SequenceParamLocation SequenceLocationSelected
        {
            get { return this._SequenceLocationSelected; }
            set
            {
                if (this._SequenceLocationSelected != value)
                {
                    this._SequenceLocationSelected = value;
                    IsParamSelected = this._SequenceLocationSelected != null || this._SequenceIOSelected != null;
                    RaisePropertyChanged();
                }
            }
        }

        private SequenceParamIO _SequenceIOSelected = null;
        public SequenceParamIO SequenceIOSelected
        {
            get { return this._SequenceIOSelected; }
            set
            {
                if (this._SequenceIOSelected != value)
                {
                    this._SequenceIOSelected = value;
                    IsParamSelected = this._SequenceLocationSelected != null || this._SequenceIOSelected != null;
                    RaisePropertyChanged();
                }
            }
        }

        private SequenceParamAction _SequenceActionSelected = null;
        public SequenceParamAction SequenceActionSelected
        {
            get { return this._SequenceActionSelected; }
            set
            {
                if (this._SequenceActionSelected != value)
                {
                    this._SequenceActionSelected = value;
                    IsActionSelected = value != null;

                    RaisePropertyChanged();
                }
            }
        }

        private int _SequenceActionSelectedIndex;
        public int SequenceActionSelectedIndex
        {
            get { return this._SequenceActionSelectedIndex; }
            set
            {
                if (this._SequenceActionSelectedIndex != value)
                {
                    this._SequenceActionSelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _AddParamCommand;
        public ICommand AddParamCommand
        {
            get
            {
                if (null == _AddParamCommand) _AddParamCommand = new RelayCommand(OnAddParamCommand);
                return _AddParamCommand;
            }
        }

        private void OnAddParamCommand()
        {
            if (VisibleLocation == Visibility.Visible)
            {
                SequenceParamLocation param = new SequenceParamLocation();
                param.Name = $"Name{SequenceLocationList.Count + 1}";

                SequenceLocationList.Add(param);

            }
            else if (VisibleIOList == Visibility.Visible)
            {
                SequenceParamIO param = new SequenceParamIO();
                param.Name = $"Name{SequenceIOList.Count + 1}";

                SequenceIOList.Add(param);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private RelayCommand _SetLocationCommand;
        public ICommand SetLocationCommand
        {
            get
            {
                if (null == _SetLocationCommand) _SetLocationCommand = new RelayCommand(OnSetLocation);
                return _SetLocationCommand;
            }
        }

        private void OnSetLocation()
        {
            if (VisibleLocation == Visibility.Visible)
            {
                if (!Double.IsNaN(SequenceLocationSelected.Position.X)) SequenceLocationSelected.Position.X = _MainVM.AxisX.AxisObject.Status.Position.Command;
                if (!Double.IsNaN(SequenceLocationSelected.Position.Y)) SequenceLocationSelected.Position.Y = _MainVM.AxisY.AxisObject.Status.Position.Command;
                if (!Double.IsNaN(SequenceLocationSelected.Position.Z)) SequenceLocationSelected.Position.Z = _MainVM.AxisZ.AxisObject.Status.Position.Command;
                if (!Double.IsNaN(SequenceLocationSelected.Position.T)) SequenceLocationSelected.Position.T = _MainVM.AxisT.AxisObject.Status.Position.Command;
                if (!Double.IsNaN(SequenceLocationSelected.Position.PZ)) SequenceLocationSelected.Position.PZ = _MainVM.AxisPZ.AxisObject.Status.Position.Command;
                SequenceLocationSelected.Position.Cam = _MainVM.CurrCam;
                SaveParameterSequence();
                RaisePropertyChanged(nameof(SequenceLocationSelected));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private RelayCommand _DelParamCommand;
        public ICommand DelParamCommand
        {
            get
            {
                if (null == _DelParamCommand) _DelParamCommand = new RelayCommand(OnDelParamCommand);
                return _DelParamCommand;
            }
        }

        private void OnDelParamCommand()
        {
            if (VisibleLocation == Visibility.Visible)
            {
                if (null == SequenceLocationSelected)
                    return;

                SequenceLocationList.Remove(SequenceLocationSelected);

            }
            else if (VisibleIOList == Visibility.Visible)
            {
                if (null == SequenceIOSelected)
                    return;

                SequenceIOList.Remove(SequenceIOSelected);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool DelParamEnable
        {
            get { return true; }
        }

        private bool _IsParamSelected = false;
        public bool IsParamSelected
        {
            get { return this._IsParamSelected; }
            set
            {
                if (this._IsParamSelected != value)
                {
                    this._IsParamSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsActionSelected = false;
        public bool IsActionSelected
        {
            get { return this._IsActionSelected; }
            set
            {
                if (this._IsActionSelected != value)
                {
                    this._IsActionSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        private RelayCommand _AddActionCommand;
        public ICommand AddActionCommand
        {
            get
            {
                if (null == _AddActionCommand) _AddActionCommand = new RelayCommand(OnAddActionCommand);
                return _AddActionCommand;
            }
        }

        private void OnAddActionCommand()
        {
            if (VisibleLocation == Visibility.Visible)
            {
                if (null == SequenceLocationSelected)
                    return;

                SequenceParamAction param = new SequenceParamAction();
                param.Name = $"Name{SequenceActionList.Count + 1}";
                param.Action = EnumSequenceType.Location;
                param.Target = SequenceLocationSelected.Name;

                SequenceActionList.Add(param);
            }
            else if (VisibleIOList == Visibility.Visible)
            {
                if (null == SequenceIOSelected)
                    return;

                SequenceParamAction param = new SequenceParamAction();
                param.Name = $"Name{SequenceActionList.Count + 1}";
                param.Action = EnumSequenceType.IO;
                param.Target = SequenceIOSelected.Name;

                SequenceActionList.Add(param);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private RelayCommand _DelActionCommand;
        public ICommand DelActionCommand
        {
            get
            {
                if (null == _DelActionCommand) _DelActionCommand = new RelayCommand(OnDelActionCommand);
                return _DelActionCommand;
            }
        }

        private void OnDelActionCommand()
        {
            if (SequenceActionSelected != null)
            {
                SequenceActionList.Remove(SequenceActionSelected);
            }
        }

        private RelayCommand _ActionOrderUpCommand;
        public ICommand ActionOrderUpCommand
        {
            get
            {
                if (null == _ActionOrderUpCommand) _ActionOrderUpCommand = new RelayCommand(OnActionOrderUpCommand);
                return _ActionOrderUpCommand;
            }
        }

        private void OnActionOrderUpCommand()
        {
            if (SequenceActionSelectedIndex > 0)
            {
                int index = SequenceActionSelectedIndex;
                SwapAction(index, index - 1);
                SequenceActionSelected = SequenceActionList[index - 1];
            }
        }

        private RelayCommand _ActionOrderDownCommand;
        public ICommand ActionOrderDownCommand
        {
            get
            {
                if (null == _ActionOrderDownCommand) _ActionOrderDownCommand = new RelayCommand(OnActionOrderDownCommand);
                return _ActionOrderDownCommand;
            }
        }

        private void OnActionOrderDownCommand()
        {
            if (SequenceActionSelectedIndex < SequenceActionList.Count - 1)
            {
                int index = SequenceActionSelectedIndex;
                SwapAction(index, index + 1);
                SequenceActionSelected = SequenceActionList[index + 1];
            }
        }


        private RelayCommand _LoadParameterCommand;
        public ICommand LoadParameterCommand
        {
            get
            {
                if (null == _LoadParameterCommand) _LoadParameterCommand = new RelayCommand(OnLoadParameterCommand);
                return _LoadParameterCommand;
            }
        }

        private void OnLoadParameterCommand()
        {
            LoadParameterSequence();
        }

        private RelayCommand _SaveParameterCommand;
        public ICommand SaveParameterCommand
        {
            get
            {
                if (null == _SaveParameterCommand) _SaveParameterCommand = new RelayCommand(OnSaveParameterCommand);
                return _SaveParameterCommand;
            }
        }

        public bool Initialized => throw new NotImplementedException();

        private void OnSaveParameterCommand()
        {
            _SequenceJobParameter.LocationList = new List<SequenceParamLocation>(SequenceLocationList.ToList());
            _SequenceJobParameter.IOList = new List<SequenceParamIO>(SequenceIOList.ToList());
            _SequenceJobParameter.ActionList = new List<SequenceParamAction>(SequenceActionList.ToList());

            SaveParameterSequence();
        }

        protected void HideAllSequenceTypeDataGrid()
        {
            VisibleLocation = Visibility.Hidden;
            VisibleIOList = Visibility.Hidden;
        }

        protected void UnSelectedAllSequenceTypeDataGrid()
        {
            SequenceLocationSelected = null;
            SequenceIOSelected = null;
        }

        protected void SwapAction(int Index1, int Index2)
        {
            SequenceParamAction tmp = SequenceActionList[Index1];
            SequenceActionList[Index1] = SequenceActionList[Index2];
            SequenceActionList[Index2] = tmp;
        }

        public void LoadParameterSequence()
        {
            string path = _SequenceJobParameter.FilePath;
            string name = _SequenceJobParameter.FileName;
            string fullPath = Path.Combine(path, name);

            try
            {
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException(fullPath);
                
                using (StreamReader file = File.OpenText(fullPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _SequenceJobParameter = (SequenceJobParameter)serializer.Deserialize(file, typeof(SequenceJobParameter));
                    SequenceLocationList = new ObservableCollection<SequenceParamLocation>(_SequenceJobParameter.LocationList.ToList());
                    SequenceIOList = new ObservableCollection<SequenceParamIO>(_SequenceJobParameter.IOList.ToList());
                    SequenceActionList = new ObservableCollection<SequenceParamAction>(_SequenceJobParameter.ActionList.ToList());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _SequenceJobParameter = new SequenceJobParameter();
                SequenceLocationList = new ObservableCollection<SequenceParamLocation>();
                SequenceIOList = new ObservableCollection<SequenceParamIO>();
                SequenceActionList = new ObservableCollection<SequenceParamAction>();
            }
        }

        public void SaveParameterSequence()
        {
            string path = _SequenceJobParameter.FilePath;
            string name = _SequenceJobParameter.FileName;
            string fullPath = Path.Combine(path, name);

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (!File.Exists(fullPath))
                {
                    File.Create(fullPath);
                }

                using (StreamWriter sw = File.CreateText(fullPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(sw, _SequenceJobParameter);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DeInitModule()
        {
            SaveParameterSequence();
        }

        public EventCodeEnum InitModule()
        {
            try
            {
                LoadParameterSequence();
                SequenceTypeSelected = EnumSequenceType.Location;
                VisibleLocation = Visibility.Visible;
                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.UNDEFINED;
        }
    }
}