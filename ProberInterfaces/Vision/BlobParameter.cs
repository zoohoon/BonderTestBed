using LogModule;
using System;
using System.Collections.Generic;


namespace ProberInterfaces.Vision
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, DataContract]
    public class BlobParameter : IParamNode        // Blob parameters
    {
        [XmlIgnore, JsonIgnore]
        public virtual Object Owner { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        private Element<int> mMinBlobArea = new Element<int>();
        [DataMember]
        public Element<int> MinBlobArea
        {
            get { return mMinBlobArea; }
            set { mMinBlobArea = value; }
        }

        private Element<int> mBlobThreshHold = new Element<int>();
        [DataMember]
        public Element<int> BlobThreshHold
        {
            get { return mBlobThreshHold; }
            set { mBlobThreshHold = value; }
        }
        private Element<int> mBlobMinRadius = new Element<int>();
        [DataMember]
        public Element<int> BlobMinRadius
        {
            get { return mBlobMinRadius; }
            set { mBlobMinRadius = value; }
        }

        private Element<double> _MIN_FERET_X = new Element<double>();
        [DataMember]
        public Element<double> MIN_FERET_X
        {
            get { return _MIN_FERET_X; }
            set { _MIN_FERET_X = value; }
        }

        private Element<double> _MAX_FERET_X = new Element<double>();
        [DataMember]
        public Element<double> MAX_FERET_X
        {
            get { return _MAX_FERET_X; }
            set { _MAX_FERET_X = value; }
        }

        private Element<double> _MIN_FERET_Y = new Element<double>();
        [DataMember]
        public Element<double> MIN_FERET_Y
        {
            get { return _MIN_FERET_Y; }
            set { _MIN_FERET_Y = value; }
        }

        private Element<double> _MAX_FERET_Y = new Element<double>();
        [DataMember]
        public Element<double> MAX_FERET_Y
        {
            get { return _MAX_FERET_Y; }
            set { _MAX_FERET_Y = value; }
        }

        private Element<double> mMaxBlobArea = new Element<double>();
        [DataMember]
        public Element<double> MaxBlobArea
        {
            get { return mMaxBlobArea; }
            set { mMaxBlobArea = value; }
        }

        private Element<int> _BlobLimitedCount = new Element<int>();
        [DataMember]
        public Element<int> BlobLimitedCount
        {
            get { return _BlobLimitedCount; }
            set { _BlobLimitedCount = value; }
        }

        //private Parameter mMinBlobArea = new Parameter("MinBlobArea");

        //public virtual Parameter MinBlobArea
        //{
        //    get { return mMinBlobArea; }
        //    set { mMinBlobArea = value; }
        //}

        //private Parameter mBlobThreshHold = new Parameter("BlobThreshHold");

        //public virtual Parameter BlobThreshHold
        //{
        //    get { return mBlobThreshHold; }
        //    set { mBlobThreshHold = value; }
        //}
        //private Parameter mBlobMinRadius = new Parameter("BlobMinRadius");

        //public virtual Parameter BlobMinRadius
        //{
        //    get { return mBlobMinRadius; }
        //    set { mBlobMinRadius = value; }
        //}

        //private bool mInvertedBlob;

        //public virtual bool InvertedBlob
        //{
        //    get { return mInvertedBlob; }
        //    set { mInvertedBlob = value; }
        //}
        public void CopyTo(BlobParameter target)
        {
            try
            {
                MinBlobArea.CopyTo(target.MinBlobArea);
                BlobThreshHold.CopyTo(target.BlobThreshHold);
                BlobMinRadius.CopyTo(target.BlobMinRadius);
                MIN_FERET_X.CopyTo(target.MIN_FERET_X);
                MAX_FERET_X.CopyTo(target.MAX_FERET_X);
                MIN_FERET_Y.CopyTo(target.MIN_FERET_Y);
                MAX_FERET_Y.CopyTo(target.MAX_FERET_Y);
                MaxBlobArea.CopyTo(target.MaxBlobArea);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
