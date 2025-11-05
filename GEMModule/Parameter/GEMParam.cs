using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace GEMModule
{
    [Serializable]
    public class GemSysParameter : ISystemParameterizable, INotifyPropertyChanged, IParamNode
                                ,IGemSysParameter
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region ==> IParam Implement
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; } = "GemSysParam";
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        #endregion

        public string FilePath { get; } = "GEM";
        public string FileName { get; } = "GEMParam.Json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public void SetElementMetaData()
        {
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
                //버전 롤백 시, gem관련 dll을 하위호환하기 위해서 현재 경로에서 찾아서 사용할 수 있도록 변경
                GEMServiceHostWCFPath.Value = Path.Combine(Environment.CurrentDirectory, @"SecsGemServiceHostApp.exe");
                GemMessageReceiveModulePath = Path.Combine(Environment.CurrentDirectory, @"SecsGemReceiveModules.dll");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        private Element<bool> _Enable = new Element<bool>();
        public Element<bool> Enable
        {
            get { return _Enable; }
            set
            {
                _Enable = value;
            }
        }


        private Element<string> _ConfigPath = new Element<string>();
        public Element<string> ConfigPath
        {
            get { return _ConfigPath; }
            set
            {
                _ConfigPath = value;
            }
        }

        private Element<string> _GEMServiceHostWCFPath = new Element<string>(@".\SecsGemServiceHostApp.exe");
        [JsonIgnore]
        public Element<string> GEMServiceHostWCFPath
        {
            get { return _GEMServiceHostWCFPath; }
            set
            {
                _GEMServiceHostWCFPath = value;
            }
        }

        private GemAPIType _GEMServiceHostAPIType ;

        public GemAPIType GEMServiceHostAPIType
        {
            get { return _GEMServiceHostAPIType; }
            set { _GEMServiceHostAPIType = value; }
        }


        private Element<string> _GEMSerialNum = new Element<string>();
        public Element<string> GEMSerialNum
        {
            get { return _GEMSerialNum; }
            set
            {
                _GEMSerialNum = value;
            }
        }

        

        private Element<GemProcessorType> _GemProcessrorType = new Element<GemProcessorType>();
        public Element<GemProcessorType> GemProcessrorType
        {
            get { return _GemProcessrorType; }
            set
            {
                _GemProcessrorType = value;
            }
        }
        private string _GemMessageReceiveModulePath = @".\SecsGemReceiveModules.dll";
        [JsonIgnore]
        public string GemMessageReceiveModulePath
        {
            get { return _GemMessageReceiveModulePath; }
            set { _GemMessageReceiveModulePath = value; }
        }


        private string _ReceiveMessageType;

        public string ReceiveMessageType
        {
            get { return _ReceiveMessageType; }
            set
            {
                _ReceiveMessageType = value;
                NotifyPropertyChanged("ReceiveMessageType");
            }
        }

        public GemSysParameter()
        {
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                ConfigPath.Value = @"C:\ProberSystem\Utility\GEM\Config\GEM_PROCESS.cfg";
                GEMSerialNum.Value = "SBMHJAQRKPUPKBQD";
                GemProcessrorType.Value = GemProcessorType.SINGLE;
                ReceiveMessageType = "SemicsGemReceiverSEKT";
                GEMServiceHostAPIType = GemAPIType.XGEM;
                //CommandRecipe = new GemCommandRecipe();
                //SerialPath = @"C:\ProberSystem\Parameters\SystemParam\GEM\GEMSerial.txt";
                //CommandRecipeFilePath.Value = @"Command\Command_RECIPE_GEM.bin";
                //DB_path.Value = @"C:\ProberSystem\Parameters\SystemParam\GEM\Semics\Semics\XDatabase\XGem.mdb";
                //DB_psw.Value = "B1594C47";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }
    }

    public class GemStateDefineParameter : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> IParam Implement
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; } = "GemSysParam";
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public string FilePath { get; } = "GEM";
        public string FileName { get; } = "GEMStateDefineParam.Json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        #region <remarks> Propertys </remarks>
        private List<StageStateDefineParameter> _StageStateDefineParam
             = new List<StageStateDefineParameter>();
        public List<StageStateDefineParameter> StageStateDefineParam
        {
            get { return _StageStateDefineParam; }
            set
            {
                if (value != _StageStateDefineParam)
                {
                    _StageStateDefineParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<FoupStateDefineParameter> _FoupStateDefineParam
             = new List<FoupStateDefineParameter>();
        public List<FoupStateDefineParameter> FoupStateDefineParam
        {
            get { return _FoupStateDefineParam; }
            set
            {
                if (value != _FoupStateDefineParam)
                {
                    _FoupStateDefineParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PreHeatStateDefineParameter> _PreHeatStateDefineParam
             = new List<PreHeatStateDefineParameter>();
        public List<PreHeatStateDefineParameter> PreHeatStateDefineParam
        {
            get { return _PreHeatStateDefineParam; }
            set
            {
                if (value != _PreHeatStateDefineParam)
                {
                    _PreHeatStateDefineParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<FoupStateDefineParameter> _CardLPStateDefineParam
             = new List<FoupStateDefineParameter>();
        public List<FoupStateDefineParameter> CardLPStateDefineParam
        {
            get { return _CardLPStateDefineParam; }
            set
            {
                if (value != _CardLPStateDefineParam)
                {
                    _CardLPStateDefineParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<SmokeSensorStateDefineParameter> _SmokeSensorStateDefineParam
             = new List<SmokeSensorStateDefineParameter>();
        public List<SmokeSensorStateDefineParameter> SmokeSensorStateDefineParam
        {
            get { return _SmokeSensorStateDefineParam; }
            set
            {
                if (value != _SmokeSensorStateDefineParam)
                {
                    _SmokeSensorStateDefineParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public GemStateDefineParameter()
        {

        }

        public void SetElementMetaData()
        {
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                HYNIX();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void MICRON()
        {
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.UNDIFIND, 0));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.OFFLINE, 1));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.ONLINE, 2));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.IDLE, 3));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.ALLOCATED, 4));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.PROCESSING, 5));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.READY_TO_TEST, 6));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.UNLOADING, 7));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.STAGE_ERROR, 8));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.READY_Z_UP, 9));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.NEXT_WAFER_PREPRCOESSING, 10));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.Z_DOWN, 11));

            #region For STM_CATANIA
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.MAINTENANCE, 12));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.DISCONNECTED, 13));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.LOTOP_PAUSED, 14));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.LOTOP_ABORTED, 15));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.LOTOP_IDLE, 16));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.LOTOP_RUNNING, 17));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.LOTOP_ERROR, 18));
            #endregion

            PreHeatStateDefineParam.Add(new PreHeatStateDefineParameter(GEMPreHeatStateEnum.UNDIFIND, 0));
            PreHeatStateDefineParam.Add(new PreHeatStateDefineParameter(GEMPreHeatStateEnum.NOT_PRE_HEATING, 1));
            PreHeatStateDefineParam.Add(new PreHeatStateDefineParameter(GEMPreHeatStateEnum.PRE_HEATING, 2));

            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.OFFLINE, 1));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.ONLINE, 2));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_LOAD, 3));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.PLACED, 4));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.ACTIVED, 5));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READ_CARRIER_MAP, 6));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.SLOT_SELECTED, 7));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.PROCESSING, 8));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_UNLOAD, 9));
        }

        private void YMTC()
        {
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.OUT_OF_SERVICE, 0));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.TRANSFER_BLOCKED, 1));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_LOAD, 2));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_UNLOAD, 3));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.IN_SERVICE, 4));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.TRANSFER_READY, 5));

            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.UNAVAILABLE, 0));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.AVAILABLE, 1));
        }


        private void HYNIX()
        {
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.OUT_OF_SERVICE, 0));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.TRANSFER_BLOCKED, 1));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_LOAD, 2));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_UNLOAD, 3));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.IN_SERVICE, 4));
            FoupStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.TRANSFER_READY, 5));

            CardLPStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.OUT_OF_SERVICE, 0));
            CardLPStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.TRANSFER_BLOCKED, 1));
            CardLPStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_LOAD, 2));
            CardLPStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.READY_TO_UNLOAD, 3));
            CardLPStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.IN_SERVICE, 4));
            CardLPStateDefineParam.Add(new FoupStateDefineParameter(GEMFoupStateEnum.TRANSFER_READY, 5));


            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.UNAVAILABLE, 0));
            StageStateDefineParam.Add(new StageStateDefineParameter(GEMStageStateEnum.AVAILABLE, 1));
            
            SmokeSensorStateDefineParam.Add(new SmokeSensorStateDefineParameter(GEMSensorStatusEnum.DISCONNECTED, 0));
            SmokeSensorStateDefineParam.Add(new SmokeSensorStateDefineParameter(GEMSensorStatusEnum.NORMAL, 1));
            SmokeSensorStateDefineParam.Add(new SmokeSensorStateDefineParameter(GEMSensorStatusEnum.ALARM, 2));                   
        }
    }

   




    public class StageStateDefineParameter : INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GEMStageStateEnum _StageStateEnum;
        public GEMStageStateEnum StageStateEnum
        {
            get { return _StageStateEnum; }
            set
            {
                if (value != _StageStateEnum)
                {
                    _StageStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Number;
        public int Number
        {
            get { return _Number; }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    RaisePropertyChanged();
                }
            }
        }

        public StageStateDefineParameter(GEMStageStateEnum stageStateEnum, int number)
        {
            this.StageStateEnum = stageStateEnum;
            this.Number = number;
        }
    }

    public class FoupStateDefineParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        private GEMFoupStateEnum _FoupStateEnum;
        public GEMFoupStateEnum FoupStateEnum
        {
            get { return _FoupStateEnum; }
            set
            {
                if (value != _FoupStateEnum)
                {
                    _FoupStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Number;
        public int Number
        {
            get { return _Number; }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        public FoupStateDefineParameter(GEMFoupStateEnum foupStateEnum, int number)
        {
            this.FoupStateEnum = foupStateEnum;
            this.Number = number;
        }
    }

    public class PreHeatStateDefineParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GEMPreHeatStateEnum _PreHeatStateEnum;
        public GEMPreHeatStateEnum PreHeatStateEnum
        {
            get { return _PreHeatStateEnum; }
            set
            {
                if (value != _PreHeatStateEnum)
                {
                    _PreHeatStateEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Number;
        public int Number
        {
            get { return _Number; }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PreHeatStateDefineParameter(GEMPreHeatStateEnum preHeatStateEnum, int number)
        {
            this.PreHeatStateEnum = preHeatStateEnum;
            this.Number = number;
        }

    }

    public class SmokeSensorStateDefineParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GEMSensorStatusEnum _SensorStatusEnum;
        public GEMSensorStatusEnum SensorStatusEnum
        {
            get { return _SensorStatusEnum; }
            set
            {
                if (value != _SensorStatusEnum)
                {
                    _SensorStatusEnum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _Number;
        public int Number
        {
            get { return _Number; }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SmokeSensorStateDefineParameter(GEMSensorStatusEnum sensorStatusEnum, int number)
        {
            this.SensorStatusEnum = sensorStatusEnum;
            this.Number = number;
        }
    }
}
