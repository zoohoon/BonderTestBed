using System.Collections.Generic;

namespace PIVManagerModule
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ProberErrorCode;
    using ProberInterfaces;

    public class PIVParameter : INotifyPropertyChanged, ISystemParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string FilePath { get; set; } = "";

        public string FileName { get; set; } = "PIVParameter.Json";

        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }

        private ObservableCollection<FoupStateConvertParam> _FoupStates
             = new ObservableCollection<FoupStateConvertParam>();
        public ObservableCollection<FoupStateConvertParam> FoupStates
        {
            get { return _FoupStates; }
            set
            {
                if (value != _FoupStates)
                {
                    _FoupStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<StageStateConvertParam> _StageStates
             = new ObservableCollection<StageStateConvertParam>();
        public ObservableCollection<StageStateConvertParam> StageStates
        {
            get { return _StageStates; }
            set
            {
                if (value != _StageStates)
                {
                    _StageStates = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PreHeatStateConvertParam> _PreHeatStates
             = new ObservableCollection<PreHeatStateConvertParam>();
        public ObservableCollection<PreHeatStateConvertParam> PreHeatStates
        {
            get { return _PreHeatStates; }
            set
            {
                if (value != _PreHeatStates)
                {
                    _PreHeatStates = value;
                    RaisePropertyChanged();
                }
            }
        }




        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void SetElementMetaData()
        {
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }

    }

    public class FoupStateConvertParam : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GEMFoupStateEnum _State;
        public GEMFoupStateEnum State
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

        private int _Value;
        public int Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnable;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                if (value != _IsEnable)
                {
                    _IsEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FoupStateConvertParam()
        {

        }
        
        public FoupStateConvertParam(GEMFoupStateEnum foupstate, int value, bool isenable)
        {
            State = foupstate;
            Value = value;
            IsEnable = isenable;
        }

    }

    public class StageStateConvertParam : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GEMStageStateEnum _State;
        public GEMStageStateEnum State
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

        private int _Value;
        public int Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnable;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                if (value != _IsEnable)
                {
                    _IsEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public StageStateConvertParam()
        {

        }

        public StageStateConvertParam(GEMStageStateEnum stagestate, int value, bool isenable)
        {
            State = stagestate;
            Value = value;
            IsEnable = isenable;
        }
    }

    public class PreHeatStateConvertParam : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GEMPreHeatStateEnum _State;
        public GEMPreHeatStateEnum State
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

        private int _Value;
        public int Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnable;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set
            {
                if (value != _IsEnable)
                {
                    _IsEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PreHeatStateConvertParam()
        {

        }

        public PreHeatStateConvertParam(GEMPreHeatStateEnum preheatstate, int value, bool isenable)
        {
            State = preheatstate;
            Value = value;
            IsEnable = isenable;
        }
    }
}
