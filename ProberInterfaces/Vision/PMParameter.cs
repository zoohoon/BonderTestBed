using LogModule;
using System;
using System.Collections.Generic;


namespace ProberInterfaces.Vision
{
    using Newtonsoft.Json;
    using ProberInterfaces.Param;
    using System.Xml.Serialization;

    /// <summary>
    /// Pattern matching parameters
    /// </summary>
    [Serializable]
    public class PMParameter : IParamNode
    {
        [XmlIgnore, JsonIgnore]

        private MachineCoordinate _MachineCoordPos = new MachineCoordinate();
        public MachineCoordinate MachineCoordPos
        {
            get
            {
                return _MachineCoordPos;
            }
            set
            {
                //_UpdatePinsHistory.Add(value);
                _MachineCoordPos = value;
            }
        }

        [XmlIgnore, JsonIgnore]
        public virtual Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        private Element<int> mPMAcceptance = new Element<int>();
        public Element<int> PMAcceptance
        {
            get { return mPMAcceptance; }
            set { mPMAcceptance = value; }
        }
        private Element<int> mPMCertainty = new Element<int>();
        public Element<int> PMCertainty
        {
            get { return mPMCertainty; }
            set { mPMCertainty = value; }
        }
        //private Element<int> mPatternSize = new Element<int>("PatternSize");
        //public Element<int> PatternSize
        //{
        //    get { return mPatternSize; }
        //    set { mPatternSize = value; }
        //}
        //private Element<int> mPMOccurrence = new Element<int>("PMOccurrence");
        //public Element<int> PMOccurrence
        //{
        //    get { return mPMOccurrence; }
        //    set { mPMOccurrence = value; }
        //}
        private Element<int> mPattWidth = new Element<int>();
        public Element<int> PattWidth
        {
            get { return mPattWidth; }
            set { mPattWidth = value; }
        }
        private Element<int> mPattHeight = new Element<int>();
        public Element<int> PattHeight
        {
            get { return mPattHeight; }
            set { mPattHeight = value; }
        }
        private Element<string> mModelFilePath = new Element<string>();
        public Element<string> ModelFilePath
        {
            get { return mModelFilePath; }
            set { mModelFilePath = value; }
        }
        private Element<string> mMaskFilePath = new Element<string>();
        public Element<string> MaskFilePath
        {
            get { return mMaskFilePath; }
            set { mMaskFilePath = value; }
        }

        private Element<string> mPatternFileExtension = new Element<string>();
        public Element<string> PatternFileExtension
        {
            get { return mPatternFileExtension; }
            set { mPatternFileExtension = value; }
        }

        public void CopyTo(PMParameter target)
        {
            try
            {
                if (target == null) target = new PMParameter();
                target.PMAcceptance.Value = this.PMAcceptance.Value;
                target.PMCertainty.Value = this.PMCertainty.Value;
                target.PattWidth.Value = this.PattWidth.Value;
                target.PattHeight.Value = this.PattHeight.Value;
                //PMAcceptance.CopyTo(target.PMAcceptance);
                //PMCertainty.CopyTo(target.PMCertainty);
                //PatternSize.CopyTo(target.PatternSize);
                //PattWidth.CopyTo(target.PattWidth);
                //PattHeight.CopyTo(target.PattHeight);

                if (ModelFilePath.Value != null)
                    target.ModelFilePath.Value = (string)ModelFilePath.Value.Clone();
                if (MaskFilePath.Value != null)
                    target.MaskFilePath.Value = (string)MaskFilePath.Value.Clone();
                if (PatternFileExtension.Value != null)
                    target.PatternFileExtension.Value = (string)PatternFileExtension.Value.Clone();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PMParameter()
        {
            try
            {
                mPMAcceptance.Value = 75;
                mPMCertainty.Value = 95;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public PMParameter(int acceptance, int certainty)
        {
            try
            {
                mPMAcceptance.Value = acceptance;
                mPMCertainty.Value = certainty;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    public class ROIParameter : IParamNode
    {
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
        public List<object> Nodes { get; set; }

        private Element<int> _OffsetX = new Element<int>();
        public Element<int> OffsetX
        {
            get { return _OffsetX; }
            set { _OffsetX = value; }
        }

        private Element<int> _OffsetY = new Element<int>();
        public Element<int> OffsetY
        {
            get { return _OffsetY; }
            set { _OffsetY = value; }
        }

        private Element<int> _Width = new Element<int>();
        public Element<int> Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private Element<int> _Height = new Element<int>();
        public Element<int> Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public ROIParameter()
        {

        }
        public ROIParameter(int offsetx, int offsety, int width, int height)
        {
            try
            {
                this.OffsetX.Value = offsetx;
                this.OffsetY.Value = offsety;
                this.Width.Value = width;
                this.Height.Value = height;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

    }

}
