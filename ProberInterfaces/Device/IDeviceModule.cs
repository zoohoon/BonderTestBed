namespace ProberInterfaces.Device
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public interface IDeviceModule : IStateModule
    {
        bool GetReadyReicpeState();
        void SetReadyReicpeState(bool flag);
        EventCodeEnum SetLoadReserveDevice(int foupnumber, string lotid, string cstHashCode);

        bool IsHaveReservationRecipe(int foupnumber, string lotid, string cstHashCode = "");
        bool IsHaveDontCareLotReservationRecipe();
        EventCodeEnum SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1, bool showprogress = true, bool manualDownload = false);
        EventCodeEnum LoadDevice();
        void RemoveAllReservationRecipe();
        void RemoveSpecificReservationRecipe();
        void ClearActiveDeviceDic(int foupnumber, string lotid, string cstHashCode);
        void SetDeviceLoadResult(int foupNumber, string lotId, string deviceName, bool result);
        EventCodeEnum SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter);
        bool IsExistSetTempParemterFromNeedChangeParameter();
    }

    public class DeviceInformation
    {
        private int _FoupNumber;

        public int FoupNumber
        {
            get { return _FoupNumber; }
            set { _FoupNumber = value; }
        }

        private string _LotID = "";

        public string LotID
        {
            get { return _LotID; }
            set { _LotID = value; }
        }

        private string _DeviceNamge;

        public string DeviceName
        {
            get { return _DeviceNamge; }
            set { _DeviceNamge = value; }
        }

        private string _DeviceZipPath;

        public string DeviceZipPath
        {
            get { return _DeviceZipPath; }
            set { _DeviceZipPath = value; }
        }

        private bool _NeedImmediateLoad;

        public bool NeedImmediateLoad
        {
            get { return _NeedImmediateLoad; }
            set { _NeedImmediateLoad = value; }
        }

        private string _LotCstHashCode = "";

        public string LotCstHashCode
        {
            get { return _LotCstHashCode; }
            set { _LotCstHashCode = value; }
        }


        private string _DeviceHashcode = "";

        /// <summary>
        /// Device Load가 Trigger되었을 때 생성되는 Hashcode 
        /// Device Load 및 ZipFile 생성 및 삭제 시 Path를 구분하기 위함. 
        /// </summary>
        public string DeviceHashcode
        {
            get { return _DeviceHashcode; }
            set { _DeviceHashcode = value; }
        }

        private NeedChangeParameterInDevice _NeedChangeParameterInfo;

        public NeedChangeParameterInDevice NeedChangeParameterInfo
        {
            get { return _NeedChangeParameterInfo; }
            set { _NeedChangeParameterInfo = value; }
        }

        public DeviceInformation()
        {

        }
        public DeviceInformation(int foupnumber, string lotid, string devicename)
        {
            FoupNumber = foupnumber;
            LotID = lotid;
            DeviceName = devicename;
        }

        public DeviceInformation(int foupnumber, string lotid, string devicename, string devicezippath, bool needImmediateLoad)
        {
            FoupNumber = foupnumber;
            LotID = lotid;
            DeviceName = devicename;
            DeviceZipPath = devicezippath;
            NeedImmediateLoad = needImmediateLoad;
        }

        public DeviceInformation(int foupnumber, string lotid, string lotcsthashcode, string devicename, string devicezippath, bool needImmediateLoad, string device_hash)
        {

            FoupNumber = foupnumber;
            if(lotid != null)
            {
                LotID = lotid;
            }
            if(lotcsthashcode != null)
            {
                LotCstHashCode = lotcsthashcode;
            }
            DeviceName = devicename;
            DeviceZipPath = devicezippath;
            NeedImmediateLoad = needImmediateLoad;
            DeviceHashcode = device_hash;
        }
    }

    [Serializable]
    public class NeedChangeParameterInDevice
    {
        private int _CellIndex;
        [DataMember]
        public int CellIndex
        {
            get { return _CellIndex; }
            set { _CellIndex = value; }
        }

        private int _FoupNumber;
        [DataMember]
        public int FoupNumber
        {
            get { return _FoupNumber; }
            set { _FoupNumber = value; }
        }


        private string _LOTID;
        [DataMember]
        public string LOTID
        {
            get { return _LOTID; }
            set { _LOTID = value; }
        }

        private string _DeviceName;
        [DataMember]
        public string DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value; }
        }



        private List<ElementParameterInfomation> _ElementParameters
             = new List<ElementParameterInfomation>();
        [DataMember]
        public List<ElementParameterInfomation> ElementParameters
        {
            get { return _ElementParameters; }
            set { _ElementParameters = value; }
        }
    }

    [Serializable]
    public class ElementParameterInfomation
    {
        private string _PropertyPath;
        [DataMember]
        public string PropertyPath
        {
            get { return _PropertyPath; }
            set { _PropertyPath = value; }
        }

        private int _VID;
        [DataMember]
        public int VID
        {
            get { return _VID; }
            set { _VID = value; }
        }

        private object _Value;
        [DataMember]
        public object Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public ElementParameterInfomation(string propertyPath, object value)
        {
            PropertyPath = propertyPath;
            Value = value;
        }
        public ElementParameterInfomation(int vid, object value)
        {
            VID = vid;
            Value = value;
        }
    }
}
