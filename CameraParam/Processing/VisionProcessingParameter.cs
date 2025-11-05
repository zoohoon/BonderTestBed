using System;
using System.Collections.Generic;

namespace VisionParams
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;

    [Serializable]
    public class VisionProcessingParameter : IVisionProcessingParameter, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

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
        public void SetElementMetaData()
        {
            MaxFocusFlatness.CategoryID = "10015";
            MaxFocusFlatness.ElementName = "Maximum allowable focus flatness";
            MaxFocusFlatness.Description = "Maximum allowable focus flatness";
            MaxFocusFlatness.ReadMaskingLevel = 0;
            MaxFocusFlatness.WriteMaskingLevel = 0;
            MaxFocusFlatness.LowerLimit = 0;
            MaxFocusFlatness.UpperLimit = 99.9;

            FocusFlatnessTriggerValue.CategoryID = "10015";
            FocusFlatnessTriggerValue.ElementName = "focus flatness Trigger value";
            FocusFlatnessTriggerValue.Description = "focus flatness Trigger value";
            FocusFlatnessTriggerValue.ReadMaskingLevel = 0;
            FocusFlatnessTriggerValue.WriteMaskingLevel = 0;
            FocusFlatnessTriggerValue.LowerLimit = 0;
            FocusFlatnessTriggerValue.UpperLimit = 99.9;

        }
        [XmlIgnore, JsonIgnore]
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

        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "Vision";
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "VisionProcParameter.Json";
        [XmlIgnore, JsonIgnore]
        public string PMRecultPath { get; } = "PMImage";


        private Element<EnumVisionProcRaft> _ProcRaft
             = new Element<EnumVisionProcRaft>();

        public Element<EnumVisionProcRaft> ProcRaft
        {
            get { return _ProcRaft; }
            set { _ProcRaft = value; }
        }

        private Element<DispFlipEnum> _DisplayVerFlip
             = new Element<DispFlipEnum>{ Value = DispFlipEnum.NONE };

        public Element<DispFlipEnum> DisplayVerFlip
        {
            get { return _DisplayVerFlip; }
            set { _DisplayVerFlip = value; }
        }


        private Element<DispFlipEnum> _DisplayHorFlip
             = new Element<DispFlipEnum> { Value = DispFlipEnum.NONE };

        public Element<DispFlipEnum> DisplayHorFlip
        {
            get { return _DisplayHorFlip; }
            set { _DisplayHorFlip = value; }
        }

        public Element<double> MaxFocusFlatness { get; set; } = new Element<double>() { Value = 70.0 };
        public Element<double> FocusFlatnessTriggerValue { get; set; } = new Element<double>() { Value = 1.5 };

        private Element<int> _WaferLowPMDownAcceptance
             = new Element<int>() { Value = 65 };

        public Element<int> WaferLowPMDownAcceptance
        {
            get { return _WaferLowPMDownAcceptance; }
            set { _WaferLowPMDownAcceptance = value; }
        }

        private BlobParameter mBlobParam;
        [ParamIgnore]   
        public BlobParameter BlobParam
        {
            get { return mBlobParam; }
            set { mBlobParam = value; }
        }
        private ObservableCollection<PMParameter> mPMParam;
        [ParamIgnore]
        public ObservableCollection<PMParameter> PMParam
        {
            get { return mPMParam; }
            set { mPMParam = value; }
        }

        public VisionProcessingParameter()
        {
            try
            {
                mBlobParam = new BlobParameter();
                mPMParam = new ObservableCollection<PMParameter>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                //SetDefaultParamMIL();
                SetDefaultPramEmul();
                //SetDefaultPramOpenCV();

                BlobParameter mblobparam = new BlobParameter();
                PMParameter mpmparam = new PMParameter();

                mblobparam.BlobMinRadius.Value = 3;
                mblobparam.BlobThreshHold.Value = 120;
                mblobparam.MinBlobArea.Value = 50;

                mpmparam.PMAcceptance.Value = 80;
                mpmparam.PMCertainty.Value = 95;

                BlobParam = mblobparam;
                PMParam.Add(mpmparam);

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                //SetDefaultPramEmul();
                SetDefaultParamMIL();

                BlobParameter mblobparam = new BlobParameter();
                PMParameter mpmparam = new PMParameter();

                mblobparam.BlobMinRadius.Value = 3;
                mblobparam.BlobThreshHold.Value = 120;
                mblobparam.MinBlobArea.Value = 50;

                mpmparam.PMAcceptance.Value = 80;
                mpmparam.PMCertainty.Value = 95;

                BlobParam = mblobparam;
                PMParam.Add(mpmparam);

                MaxFocusFlatness = new Element<double>();
                MaxFocusFlatness.Value = 70.0;

                FocusFlatnessTriggerValue = new Element<double>();
                FocusFlatnessTriggerValue.Value = 1.5;

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private void SetDefaultPramEmul()
        {
            ProcRaft.Value = EnumVisionProcRaft.EMUL;
        }

        private void SetDefaultPramOpenCV()
        {
            ProcRaft.Value = EnumVisionProcRaft.OPENCV;
        }

        private void SetDefaultParamMIL()
        {
            ProcRaft.Value = EnumVisionProcRaft.MIL;
        }
    }
}
