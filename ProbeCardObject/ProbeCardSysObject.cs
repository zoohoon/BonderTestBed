using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProbeCardObject
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class ProbeCardSysObject : IParam, IParamNode, ISystemParameterizable, IProbeCardSysObject, INotifyPropertyChanged
    {
        // card가 동일할 경우엔 핀의 위치를 system 파라미터로 들고 있으면서 핀얼라인 동작시에 사용을 할수 있게 만들기 위함
        // 디바이스 변경에 따라 핀 얼라인 Pos error를 방지하기 위함 
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region ==> IProbeCardSysObject
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; set; } = @"PinAlign";

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; set; } = "BackupCardinfo.json";
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
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
        public void SetElementMetaData()
        {
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbercardinfoClear();
                UsePinPosWithCardID.Value = false;
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        #endregion

        #region ==> Probercardinfo
        private Element<bool> _UsePinPosWithCardID = new Element<bool>();
        public Element<bool> UsePinPosWithCardID
        {
            get { return _UsePinPosWithCardID; }
            set
            {
                if (value != _UsePinPosWithCardID)
                {
                    _UsePinPosWithCardID = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ProbercardBackupinfo _ProbercardBackupinfo = new ProbercardBackupinfo();
        public ProbercardBackupinfo ProbercardBackupinfo
        {
            get { return _ProbercardBackupinfo; }
            set
            {
                if (value != _ProbercardBackupinfo)
                {
                    _ProbercardBackupinfo = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        #region ==> Probercardinfo clear
        public EventCodeEnum ProbercardinfoClear()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (ProbercardBackupinfo == null) 
                {
                    ProbercardBackupinfo = new ProbercardBackupinfo();
                }
                ProbercardBackupinfo.ProbeCardID = "";
                ProbercardBackupinfo.TotalDutCnt = 0;
                ProbercardBackupinfo.TotalPinCnt = 0;
                ProbercardBackupinfo.FirstDutMI = new MachineIndex();
                if (ProbercardBackupinfo.BackupPinDataList == null)
                {
                    ProbercardBackupinfo.BackupPinDataList = new List<BackupPinData>();
                }
                ProbercardBackupinfo.BackupPinDataList.Clear();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    #endregion

}
}
