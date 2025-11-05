using System;

namespace ProberInterfaces.GEM
{
    [Serializable]
    public class GemVidInfo
    {
        public GemVidInfo()
        {
        }
        public GemVidInfo(int vid, IDataConverter converter)
        {
            this.VID = vid;
            this.Converter = converter;
        }
        public GemVidInfo(int vid, VidUpdateTypeEnum processorType)
        {
            this.VID = vid;
            this.ProcessorType = processorType;
        }
        public GemVidInfo(int vid, VidUpdateTypeEnum processorType, bool gEMImmediatelyUpdate)
        {
            this.VID = vid;
            this.ProcessorType = processorType;
            this.GEMImmediatelyUpdate = gEMImmediatelyUpdate;
        }
        public GemVidInfo(int vid, VidPropertyTypeEnum propertyType, VidUpdateTypeEnum processorType)
        {
            this.VID = vid;
            this.VidPropertyType = propertyType;
            this.ProcessorType = processorType;
        }
        public GemVidInfo(int vid, VidPropertyTypeEnum propertyType, VidUpdateTypeEnum processorType, bool gEMImmediatelyUpdate)
        {
            this.VID = vid;
            this.VidPropertyType = propertyType;
            this.ProcessorType = processorType;
            this.GEMImmediatelyUpdate = gEMImmediatelyUpdate;
        }
        public GemVidInfo(int vid, IDataConverter converter, VidPropertyTypeEnum propertyType, VidUpdateTypeEnum processorType)
        {
            this.VID = vid;
            this.Converter = converter;
            this.VidPropertyType = propertyType;
            this.ProcessorType = processorType;
        }

        private bool _Enable = true;

        public bool Enable
        {
            get { return _Enable; }
            set { _Enable = value; }
        }

        private int _VID;

        public int VID
        {
            get { return _VID; }
            set { _VID = value; }
        }

        private IDataConverter _Converter;

        public IDataConverter Converter
        {
            get { return _Converter; }
            set { _Converter = value; }
        }

        private VidPropertyTypeEnum _VidPropertyType;

        public VidPropertyTypeEnum VidPropertyType
        {
            get { return _VidPropertyType; }
            set { _VidPropertyType = value; }
        }

        private VidUpdateTypeEnum _ProcessorType;

        public VidUpdateTypeEnum ProcessorType
        {
            get { return _ProcessorType; }
            set { _ProcessorType = value; }
        }

        private bool _GEMImmediatelyUpdate;
        ///GP GEM 연결으로 Commander 에 각 Stage 별 Buffer 개념이 생겼고 Stage 는 값이 변경되었을때
        ///바로 Gem 동글에 업데이트 하지않고 버퍼에 없데이트한다. 이때 버퍼가 아닌 바로 업데이트가 
        ///필요한 값들에 대해 설정하기위해. 즉 true : element 값 변경시 바로 gem 동글 업데이트 | false : 버퍼에 업데이트
        public bool GEMImmediatelyUpdate
        {
            get { return _GEMImmediatelyUpdate; }
            set { _GEMImmediatelyUpdate = value; }
        }
    }
}
