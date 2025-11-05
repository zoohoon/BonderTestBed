using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace VisionParams
{
    using LogModule;

    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable]
    public class VisionProcessingParameter : IVisionProcessingParameter, ISystemParameterizable
    {
        public List<object> Nodes { get; set; }


        public ErrorCodeEnum Init()
        {
            ErrorCodeEnum retval = ErrorCodeEnum.UNDEFINED;

            try
            {
                retval = ErrorCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = ErrorCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        [XmlIgnore]
        public string Genealogy { get; set; }
        [XmlIgnore]
        public object Owner { get; set; }

        [XmlIgnore]
        public Object OwnerModule { get; set; }
        [XmlIgnore]
        public string FilePath { get; } = "Vision";
        [XmlIgnore]
        public string FileName { get; } = "VisionProcParameter.bin";
        [XmlIgnore]
        public string PMRecultPath { get; } = "PMImage";


        private Element<EnumVisionProcRaft> _ProcRaft
             = new Element<EnumVisionProcRaft>();

        public Element<EnumVisionProcRaft> ProcRaft
        {
            get { return _ProcRaft; }
            set { _ProcRaft = value; }
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
        public ErrorCodeEnum SetEmulParam()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
            try
            {

                //SetDefaultParamMIL();
                SetDefaultPramEmul();

                BlobParameter mblobparam = new BlobParameter();
                PMParameter mpmparam = new PMParameter();

                mblobparam.BlobMinRadius.Value = 3;
                mblobparam.BlobThreshHold.Value = 120;
                mblobparam.MinBlobArea.Value = 50;

                mpmparam.PMAcceptance.Value = 80;
                mpmparam.PMCertainty.Value = 95;
                mpmparam.PMOccurrence.Value = 1;

                BlobParam = mblobparam;
                PMParam.Add(mpmparam);

                RetVal = ErrorCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public ErrorCodeEnum SetDefaultParam()
        {
            ErrorCodeEnum RetVal = ErrorCodeEnum.UNDEFINED;
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
                mpmparam.PMOccurrence.Value = 1;

                BlobParam = mblobparam;
                PMParam.Add(mpmparam);

                RetVal = ErrorCodeEnum.NONE;

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

        private void SetDefaultParamMIL()
        {
            ProcRaft.Value = EnumVisionProcRaft.MIL;
        }
    }
}
