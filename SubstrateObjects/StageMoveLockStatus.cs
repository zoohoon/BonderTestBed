namespace SubstrateObjects
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public class StageMoveLockStatus : INotifyPropertyChanged, ISystemParameterizable, IStageMoveLockStatus
    {

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string FilePath { get; set; } = "";
        public string FileName { get; } = "StageMoveLockStatus.json";
        public string Genealogy { get; set; }

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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }


        private StageLockMode _LastStageMoveLockState = StageLockMode.UNLOCK;
        public StageLockMode LastStageMoveLockState
        {
            get { return _LastStageMoveLockState; }
            set
            {
                if (value != _LastStageMoveLockState)
                {
                    _LastStageMoveLockState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<ReasonOfStageMoveLock> _LastStageMoveLockReasonList = new List<ReasonOfStageMoveLock>();
        public List<ReasonOfStageMoveLock> LastStageMoveLockReasonList
        {
            get { return _LastStageMoveLockReasonList; }
            set
            {
                if (value != _LastStageMoveLockReasonList)
                {
                    _LastStageMoveLockReasonList = value;
                    RaisePropertyChanged();
                }
            }
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

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }

        public void SetElementMetaData()
        {
        }

        public StageMoveLockStatus()
        {

        }
        public StageMoveLockStatus(int CellIdx)
        {
            string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
            FilePath = $"C:\\Logs\\Backup\\{cellNo}\\";
        }
    }
}
