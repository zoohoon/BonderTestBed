using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using LogModule;

namespace GPLoaderRouter
{
    public enum EnumCardIDReaderType
    {
        NONE,
        BARCODE,
        RFID,
        DATETIME
    }
    public class GPLoaderSysParam : ISystemParameterizable, IParamNode
    {
        public string Genealogy { get; set; }
        [JsonIgnore]
        private Object _Owner;
        [JsonIgnore]
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
        public List<object> Nodes { get; set; }

        public string FilePath { get; } = "";
        public string FileName { get; } = "GPLoaderParameter.json";


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

        private Element<string> _AMSNetID;

        public Element<string> AMSNetID
        {
            get { return _AMSNetID; }
            set { _AMSNetID = value; }
        }
        private Element<string> _IPAddr;

        public Element<string> IPAddr
        {
            get { return _IPAddr; }
            set { _IPAddr = value; }
        }

        private Element<bool> _CardIDReaderAttatched
                    = new Element<bool>() { Value = true };
        public Element<bool> CardIDReaderAttatched
        {
            get { return _CardIDReaderAttatched; }
            set
            {
                if (value != _CardIDReaderAttatched)
                {
                    _CardIDReaderAttatched = value;
                }
            }
        }
        // CardID Reader Type 파라미터 추가
        private Element<EnumCardIDReaderType> _CardIDReaderType
                    = new Element<EnumCardIDReaderType>() { Value = EnumCardIDReaderType.BARCODE};

        public Element<EnumCardIDReaderType> CardIDReaderType
        {
            get { return _CardIDReaderType; }
            set { _CardIDReaderType = value; }
        }

        //private List<GPLoaderAccessParam> _FixedTrayAccessParams;

        //public List<GPLoaderAccessParam> FixedTrayAccessParams
        //{
        //    get { return _FixedTrayAccessParams; }
        //    set { _FixedTrayAccessParams = value; }
        //}
        //private List<GPLoaderAccessParam> _CardBufferAccessParams;

        //public List<GPLoaderAccessParam> CardBufferAccessParams
        //{
        //    get { return _CardBufferAccessParams; }
        //    set { _CardBufferAccessParams = value; }
        //}

        //private List<GPLoaderAccessParam> _CassetteAccessParams;

        //public List<GPLoaderAccessParam> CassetteAccessParams
        //{
        //    get { return _CassetteAccessParams; }
        //    set { _CassetteAccessParams = value; }
        //}

        //private List<GPLoaderAccessParam> _PAAccessParams;

        //public List<GPLoaderAccessParam> PAAccessParams
        //{
        //    get { return _PAAccessParams; }
        //    set { _PAAccessParams = value; }
        //}

        //private List<GPLoaderAccessParam> _ChuckAccessParams;

        //public List<GPLoaderAccessParam> ChuckAccessParams
        //{
        //    get { return _ChuckAccessParams; }
        //    set { _ChuckAccessParams = value; }
        //}
        //private List<GPLoaderAccessParam> _CardAccessParams;

        //public List<GPLoaderAccessParam> CardAccessParams
        //{
        //    get { return _CardAccessParams; }
        //    set { _CardAccessParams = value; }
        //}

        public bool IsParamChanged { get; set; }

        public EventCodeEnum SetDefaultParam()
        {
            AMSNetID = new Element<string>();
            IPAddr = new Element<string>();
            CardIDReaderType = new Element<EnumCardIDReaderType>();
            AMSNetID.Value = "5.57.26.84.1.1";
            IPAddr.Value = "5.57.26.84.1.1";
            CardIDReaderType.Value = EnumCardIDReaderType.BARCODE; // 기존에는 Default로 Barcode를 사용하고 있었으므로 기존 기능에 문제가 되지 않도록 Barcode 모드로 세팅한다.
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            AMSNetID = new Element<string>();
            IPAddr = new Element<string>();
            CardIDReaderType = new Element<EnumCardIDReaderType>();
            AMSNetID.Value = "5.57.26.84.1.1";
            IPAddr.Value = "5.57.26.84.1.1";
            CardIDReaderType.Value = EnumCardIDReaderType.NONE;
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            CardIDReaderAttatched.ElementName = $"Card ID Reader Attatched";
            CardIDReaderAttatched.ReadMaskingLevel = 0;
            CardIDReaderAttatched.WriteMaskingLevel = 0;
            CardIDReaderAttatched.CategoryID = "10014";
        }
    }
    public class GPLoaderAccessParam : ISystemParameterizable, IParamNode
    {
        public string Genealogy { get; set; }
        [JsonIgnore]
        private Object _Owner;
        [JsonIgnore]
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
        public List<object> Nodes { get; set; }

        public string FilePath { get; } = "";
        public string FileName { get; } = "GPLoaderParameter.json";

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

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
           
        }

        private Element<double> _LXPos;

        public Element<double> LXPos
        {
            get { return _LXPos; }
            set { _LXPos = value; }
        }

        private Element<double> _LZPos;

        public Element<double> LZPos
        {
            get { return _LZPos; }
            set { _LZPos = value; }
        }

        private Element<double> _LWPos;

        public Element<double> LWPos
        {
            get { return _LWPos; }
            set { _LWPos = value; }
        }

        private Element<double> _LUPos;

        public Element<double> LUPos
        {
            get { return _LUPos; }
            set { _LUPos = value; }
        }

        private Element<double> _LUCPos;

        public Element<double> LUCPos
        {
            get { return _LUCPos; }
            set { _LUCPos = value; }
        }

        private Element<double> _LTPos;

        public Element<double> LTPos
        {
            get { return _LTPos; }
            set { _LTPos = value; }
        }


        private Element<double> _PickOffset;

        public Element<double> PickOffset
        {
            get { return _PickOffset; }
            set { _PickOffset = value; }
        }

        public bool IsParamChanged { get; set; }
    }
}
