using Autofac;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PMI;
using ProberInterfaces.Vision;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ProcessingModule
{
    class VisionAlgorithmesOpenCV : IVisionAlgorithmes
    {
        private IVisionProcessing VisionProcessing;

        public VisionAlgorithmesOpenCV(IVisionProcessing processing)
        {
            VisionProcessing = processing;
        }

        public ImageBuffer Arithmethic(ImageBuffer Src1Buf, ImageBuffer Src2Buf, ARITHMETIC_OPERATION_TYPE Operation)
        {
            return null;
        }

        public ImageBuffer Binarization(ImageBuffer Image, double threshold)
        {
            return null;
        }

        public ImageBuffer ConvolveImage(ImageBuffer image, ConvolveType type)
        {
            return null;
        }

        public ImageBuffer CopyClipImage(ImageBuffer SrcBuf, ImageBuffer CilpBuf, int offset_X, int offset_Y)
        {
            return null;
        }

        public ImageBuffer CropImage(ImageBuffer image, int offset_X, int offset_Y, int width, int height)
        {
            byte[] cropArray = new byte[width * height];

            ImageBuffer returnBuffer = new ImageBuffer(cropArray, width, height, image.Band, (int)ColorDept.BlackAndWhite);

            return returnBuffer;
        }

        public ImageBuffer DrawingPadEdge(ImageBuffer buffer, List<Point> pt, bool overlap = true)
        {
            return null;
        }

        public ImageBuffer DrawingShape(ImageBuffer CurrentButter, List<Point> pt, ColorDept colordept, bool? fill = false, bool? overlap = false)
        {
            return null;
        }

        public BlobResult FindBlobObject(ImageBuffer SrcBuf, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false, bool drawing = false)
        {
            return null;
        }

        public EventCodeEnum FindMarks(List<PMIPadObject> PadList, ImageBuffer SrcBuf, ImageBuffer OrginalBuf, ref ImageBuffer ResultBuf, PMIImageCombineMode CombindMdoe, MARK_AREA_CALCULATE_MODE MarkAreaCalculateMode, double OverlapPercenterToleranceX = 0, double OverlapPercenterToleranceY = 0, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false, bool IsDrawingPadForOverlay = false, bool IsDrawingMarkForOverlay = false)
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum FindPMIPads(List<PMIPadObject> PadList, Point MarginPixel, ImageBuffer SrcBuf, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false, bool drawing = false)
        {
            return EventCodeEnum.NONE;
        }

        public List<Point> GetChainListInFirstBlob(ImageBuffer SrcBuf, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false)
        {
            List<Point> retval = new List<Point>();

            return retval;
        }

        public ImageBuffer GetEdgeImage(ImageBuffer targetImage)
        {
            throw new NotImplementedException();
        }

        public ImageBuffer GetHistogramEqualizationImage(ImageBuffer InputImage, int Mode)
        {
            return null;
        }

        public int GetOtsuThreshold(ImageBuffer curImg)
        {

            return 0;

        }

        public double GetPixelSum(ImageBuffer Image)
        {
            double retval = 0;

            return retval;
        }

        public Rect GetRectInFirstBlob(ImageBuffer SrcBuf, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false)
        {
            Rect retval = new Rect();

            return retval;
        }

        public ImageBuffer GetSynthesisImage(ImageBuffer[] InputImages)
        {
            return null;
        }

        public long GetThreshold_ForPadDetection(ImageBuffer SrcBuf)
        {
            long retval = 0;

            return retval;
        }

        public double GetThreshold_ProbeMark(ImageBuffer CropImage, ImageBuffer maskImage, int PadColor)
        {
            double retval = 0;

            return retval;
        }

        public EventCodeEnum Init(int milID, IContainer container, long Attributes, long cl_iAttributes)
        {
            return EventCodeEnum.NONE;
        }

        public ImageBuffer GetPMIResultImage(List<PMIPadObject> PadList, ImageBuffer SrcBuf, ref bool IsPass)
        {
            return null;
        }

        public ImageBuffer MaskingBuffer(ImageBuffer buffer, List<Point> pt)
        {
            return null;
        }

        public ImageBuffer MilDefaultBinarize(ImageBuffer SrcBuf)
        {
            return null;
        }

        public ImageBuffer EnhancedImage(ImageBuffer Image, float fSigma, float fScaleFactor)
        {
            return null;
        }
        public ImageBuffer EnhancedOtsuImage(ImageBuffer Image, bool bWhite, int nThreshHoldLimit)
        {
            return null;
        }

        public ImageBuffer GetEdgeImageInFirstBlob(ImageBuffer SrcBuf, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false)
        {
            return null;
        }

        public EventCodeEnum FindPMIPads(List<PMIPadObject> PadList, Point MarginPixel, ImageBuffer SrcBuf, ImageBuffer SrcBuf_Original, BlobParameter blobparam = null, ROIParameter roiparam = null, bool UseAllFeatures = false, bool foregroundIsZero = false)
        {
            return EventCodeEnum.PMI_NOT_FOUND_PAD;
        }
    }
}
