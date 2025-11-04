using System;
using LogModule;

namespace WA_IndexAlignParameter_Pattern
{
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Param;
    using ProberInterfaces.WaferAlignEX.Enum;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [Serializable]
    public class WA_IndexAlignParam_Pattern : AlginParamBase, INotifyPropertyChanged, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public override string FilePath { get; } = "WaferAlignParam\\Standard\\";
        [XmlIgnore, JsonIgnore]
        public override string FileName { get; } = "WA_IndexAlignParam_Pattern.json";
        public string PatternbasePath { get; } = "\\WaferAlignParam\\AlignPattern\\IndexAlign";
        public string PatternName { get; } = "\\IndexAlign_Pattern";

        private Element<ObservableCollection<WA_IAStnadrdPTInfomation>> _Patterns
    = new Element<ObservableCollection<WA_IAStnadrdPTInfomation>>();
        public Element<ObservableCollection<WA_IAStnadrdPTInfomation>> Patterns
        {
            get { return _Patterns; }
            set
            {
                if (value != _Patterns)
                {
                    _Patterns = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumWASubModuleEnable _AlignEnable;
        public EnumWASubModuleEnable AlignEnable
        {
            get { return _AlignEnable; }
            set
            {
                if (value != _AlignEnable)
                {
                    _AlignEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ModuleDllInfo _FocusingModuleDllInfo;

        public ModuleDllInfo FocusingModuleDllInfo
        {
            get { return _FocusingModuleDllInfo; }
            set { _FocusingModuleDllInfo = value; }
        }

        public WA_IndexAlignParam_Pattern()
        {

        }

        public WA_IndexAlignParam_Pattern(WA_IndexAlignParam_Pattern param)
        {
            try
            {
                AlignEnable = param.AlignEnable;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public override EventCodeEnum InitParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retval = EventCodeEnum.PARAM_ERROR;
            }
            return retval;
        }
        public override EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Version = typeof(WA_IndexAlignParam_Pattern).Assembly.GetName().Version;
                CamType = EnumProberCam.WAFER_LOW_CAM;

                Patterns.Value = new ObservableCollection<WA_IAStnadrdPTInfomation>();
                AlignEnable = EnumWASubModuleEnable.ENABLE;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public override EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultParam();
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
    }


    [Serializable()]
    public class WA_IAStnadrdPTInfomation : PatternInfomation, INotifyPropertyChanged, IParamNode
    {
        private EnumIndexAlignDirection _Direction;
        public EnumIndexAlignDirection Direction
        {
            get { return _Direction; }
            set
            {
                if (value != _Direction)
                {
                    _Direction = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ObservableCollection<EnumIndexAlignDirection> _Directions;
        public ObservableCollection<EnumIndexAlignDirection> Directions
        {
            get { return _Directions; }
            set
            {
                if (value != _Directions)
                {
                    _Directions = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _MIndex;
        public MachineIndex MIndex
        {
            get { return _MIndex; }
            set
            {
                if (value != _MIndex)
                {
                    _MIndex = value;
                    RaisePropertyChanged();
                }
            }
        }


        public WA_IAStnadrdPTInfomation()
        {

        }
        public WA_IAStnadrdPTInfomation(EnumIndexAlignDirection direction)
        {
            Direction = direction;
        }
        public WA_IAStnadrdPTInfomation(EnumIndexAlignDirection direction, ObservableCollection<EnumIndexAlignDirection> directions)
        {
            try
            {
                Direction = direction;
                Directions = directions;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        public WA_IAStnadrdPTInfomation(EnumIndexAlignDirection direction, ObservableCollection<EnumIndexAlignDirection> directions, MachineIndex mindex)
        {
            try
            {
                Direction = direction;
                Directions = directions;
                MIndex = mindex;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
