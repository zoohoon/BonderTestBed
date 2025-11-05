using LogModule;
using ProberErrorCode;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;

namespace ProberInterfaces
{
    public interface ISinglePinAlign : IFactoryModule, IModule
    {
        SinglePinAlignTestMock TestMock { get; set; }
        //PinAlignInfo AlignInfo { get; set; }
        //PINALIGNRESULT SinglePinalign(out PinCoordinate NewPinPosm, PinData AlignPin, IFocusing focusing);
        PinCoordinate NewPinPos { get; set; }
        IPinData AlignPin { get; set; }
        IFocusing Focusing { get; set; }
        IFocusParameter FocusingParam { get; set; }
        
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        
        int BlobMinSize { get; set; }
        int BlobMaxSize { get; set; }
        
        //int BlobResult { get; set; }
        
        int AlignKeyIndex { get; set; }

        //int getOtsuThreshold(EnumProberCam camtype, int SizeX, int SizeY);
        int getOtsuThreshold(EnumProberCam camtype, int OffsetX, int OffsetY, int SizeX, int SizeY);

        PinCoordinate ConvertPosPixelToPin(ICamera CurCam, PinCoordinate OldPos, double PosX, double PosY);

        PINALIGNRESULT SinglePinalign(out PinCoordinate NewPinPos, IPinData AlignPin, IFocusing focusing, IFocusParameter focusingParam);
        new EventCodeEnum InitModule();
    }
    public class SinglePinAlignTestMock
    {
        public Dictionary<int, PinCoordinate> PinRelMapping { get; set; }
        public Dictionary<int, EnumPinTest> PinTestMapping { get; set; }

        public SinglePinAlignTestMock()
        {
            try
            {
                PinRelMapping = new Dictionary<int, PinCoordinate>();
                PinTestMapping = new Dictionary<int, EnumPinTest>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SinglePinalign(out PinCoordinate NewPinPos, IPinData AlignPin)
        {
            try
            {
                PinCoordinate relPos = PinRelMapping[AlignPin.PinNum.Value];
                NewPinPos = new PinCoordinate(
                    AlignPin.AbsPos.X.Value + relPos.GetX(),
                    AlignPin.AbsPos.Y.Value + relPos.GetY(),
                    AlignPin.AbsPos.Z.Value + relPos.GetZ());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EnumPinTest GetPinTestStatus(IPinData pinData)
        {
            return PinTestMapping[pinData.PinNum.Value];
        }
    }
    public enum EnumPinTest { NONE, PASS, FAIL }
}
