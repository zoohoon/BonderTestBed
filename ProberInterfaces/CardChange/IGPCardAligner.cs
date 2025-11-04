namespace ProberInterfaces.CardChange
{
    using ProberErrorCode;
    using ProberInterfaces.Param;
    using System.Runtime.Serialization;

    public interface IGPCardAligner
    {
        EventCodeEnum Align();
        bool AlignCard(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double cardAvgZ, ProberCardListParameter proberCard = null);
        bool AlignPogo(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double pogoAvgZ);
        bool AlignPogo3P(out double centerDiffX, out double centerDiffY, out double degreeDiff, out double pogoAvgZ);
        bool CamAbsMove(EnumProberCam proberCam, CatCoordinates position);
        void RelMoveZ(EnumProberCam proberCam, double val);
        EventCodeEnum RegisterPattern(EnumProberCam proberCam, int index, EnumCCAlignModule module, ProberCardListParameter probeCard = null);
        IFocusing CardFocusModel { get; }


        #region Observation
        bool ObservationCard();
        void ObservationRegisterPattern(int index);
        #endregion
    }
    public class PatternRelPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public PatternRelPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    [DataContract]
    public class CardImageBuffer
    {
        private byte[] _ImageByte;
        [DataMember]
        public byte[] ImageByte
        {
            get { return _ImageByte; }
            set { _ImageByte = value; }
        }

        private string _ImgFileName;
        [DataMember]
        public string ImgFileName
        {
            get { return _ImgFileName; }
            set { _ImgFileName = value; }
        }


    }

}
