using System.Collections.Generic;

namespace SubstrateObjects
{
    //using MapObject;
    using ProberInterfaces;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberErrorCode;
    using System.Runtime.CompilerServices;
    using LogModule;
    using Newtonsoft.Json;

    [Serializable]
    public class SlotInfo : INotifyPropertyChanged, ISystemParameterizable, IStageSlotInformation
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
        public string FileName { get; } = "SlotInfo.json";
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


        private EnumSubsStatus _WaferStatus;
        public EnumSubsStatus WaferStatus
        {
            get { return _WaferStatus; }
            set
            {
                if (value != _WaferStatus)
                {
                    _WaferStatus = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferState _WaferState;
        public EnumWaferState WaferState
        {
            get { return _WaferState; }
            set
            {
                if (value != _WaferState)
                {
                    _WaferState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumWaferType _WaferType;
        public EnumWaferType WaferType
        {
            get { return _WaferType; }
            set
            {
                if (value != _WaferType)
                {
                    _WaferType = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SubstrateSizeEnum _WaferSize;
        public SubstrateSizeEnum WaferSize
        {
            get { return _WaferSize; }
            set
            {
                if (value != _WaferSize)
                {
                    _WaferSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WaferNotchTypeEnum _NotchType;
        public WaferNotchTypeEnum NotchType
        {
            get { return _NotchType; }
            set { _NotchType = value; RaisePropertyChanged(); }
        }



        private string _WaferID;
        public string WaferID
        {
            get { return _WaferID; }
            set
            {
                if (value != _WaferID)
                {
                    _WaferID = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _LoadingAngle;
        public double LoadingAngle
        {
            get { return _LoadingAngle; }
            set
            {
                if (value != _LoadingAngle)
                {
                    _LoadingAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _UnloadingAngle;
        public double UnloadingAngle
        {
            get { return _UnloadingAngle; }
            set
            {
                if (value != _UnloadingAngle)
                {
                    _UnloadingAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _OCRAngle;
        public double OCRAngle
        {
            get { return _OCRAngle; }
            set
            {
                if (value != _OCRAngle)
                {
                    _OCRAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _FoupIndex;
        public int FoupIndex
        {
            get { return _FoupIndex; }
            set
            {
                if (value != _FoupIndex)
                {
                    _FoupIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _SlotIndex;
        public int SlotIndex
        {
            get { return _SlotIndex; }
            set
            {
                if (value != _SlotIndex)
                {
                    _SlotIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _OriginSlotIndex;
        public int OriginSlotIndex
        {
            get { return _OriginSlotIndex; }
            set
            {
                if (value != _OriginSlotIndex)
                {
                    _OriginSlotIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ModuleID _OriginHolder = new ModuleID();
        public ModuleID OriginHolder
        {
            get { return _OriginHolder; }
            set
            {
                if (value != _OriginHolder)
                {
                    _OriginHolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CSTHashCode;
        public string CSTHashCode
        {
            get { return _CSTHashCode; }
            set
            {
                if (value != _CSTHashCode)
                {
                    _CSTHashCode = value;
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

        public SlotInfo()
        {

        }
        public SlotInfo(int CellIdx)
        {
            string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
            FilePath = $"C:\\Logs\\Backup\\{cellNo}\\";
        }
    }
}
