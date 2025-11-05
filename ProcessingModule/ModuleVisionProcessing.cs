using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Matrox.MatroxImagingLibrary;
using ProberInterfaces.Param;

namespace Vision.ProcessingModule
{
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.ComponentModel;
    using System.Windows;
    using global::ProcessingModule;
    using ProberErrorCode;
    using VisionParams;
    using ProberInterfaces.Vision;
    using SystemExceptions.VisionException;
    using LogModule;
    using System.Drawing.Imaging;
    using System.Drawing;
    using Point = System.Windows.Point;
    using Size = System.Windows.Size;
    using static ProberInterfaces.ModelFinderResult;
    using GeometryHelp;
    using ProberInterfaces.WaferAligner;
    using System.Runtime.InteropServices;

    public class ModuleVisionProcessing : IVisionProcessing, IDisposable, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public VisionProcessingParameter VisionProcParam;

        public ImageBuffer.ImageReadyDelegate ImageProcessing { get; set; }
        public ImageBuffer.ImageReadyDelegate ImagePattView { get; set; }

        public IVisionAlgorithmes Algorithmes { get; set; }


        #region //..Property      

        private bool IsInfo = true;

        private int MilSystem = 0;

        bool disposed = false;

        public bool Initialized { get; set; } = false;

        long lAttributes = MIL.M_IMAGE + MIL.M_PROC;
        long iAttributes = MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP;
        long cl_iAttributes = MIL.M_IMAGE + MIL.M_PROC + MIL.M_RGB24 + MIL.M_PACKED;
        long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;

        int PattSizeX;
        int PattSizeY;

        //IVisionManager vm;
        //ICamera cam;
        //IPadRegist PadRegister;

        private double _PMScore;

        public double PMScore
        {
            get { return _PMScore; }
            set
            {
                _PMScore = value;
                NotifyPropertyChanged("PMScore");
            }
        }
        private int _BlobCount;

        public int BlobCount
        {
            get { return _BlobCount; }
            set
            {
                _BlobCount = value;
                NotifyPropertyChanged("BlobCount");
            }
        }
        private double _PMpositionX;

        public double PMpositionX
        {
            get { return _PMpositionX; }
            set
            {
                _PMpositionX = value;
                NotifyPropertyChanged("PMpositionX");
            }
        }

        private double _PMpositionY;

        public double PMpositionY
        {
            get { return _PMpositionY; }
            set
            {
                _PMpositionY = value;
                NotifyPropertyChanged("PMpositionY");
            }
        }

        #endregion

        #region //..Init , Dispose 

        public ModuleVisionProcessing()
        {

        }
        /// <summary>
        /// ModuleVisionProcessing 생성자 초기화
        /// </summary>
        /// <param name="milSystem"></param>
        ~ModuleVisionProcessing()
        {
            Dispose(false);
        }

        public void InitProcessing(int milSystem)
        {
            MilSystem = milSystem;

        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //Autofac.IContainer ContainerBuffer = this.GetContainer();
                    Algorithmes = new VisionAlgorithmes(this);
                    Algorithmes.Init(MilSystem, this.GetContainer(), Attributes, cl_iAttributes);

                    //ContainerBuffer.TryResolve<IVisionManager>(out vm);
                    //ContainerBuffer.TryResolve<ICamera>(out cam);
                    //ContainerBuffer.TryResolve<IPadRegist>(out PadRegister);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {

        }

        public EventCodeEnum LoadSytemParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(VisionProcessingParameter));
                if (RetVal == EventCodeEnum.NONE)
                {
                    VisionProcParam = tmpParam as VisionProcessingParameter;
                }

                string PmResultPath = null;
                PmResultPath = this.FileManager().GetLogRootPath() + "\\" + VisionProcParam.FilePath + "\\" + VisionProcParam.PMRecultPath + "\\";

                if (Directory.Exists(Path.GetDirectoryName(PmResultPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(PmResultPath));
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($"[VisionManager] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// 
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// IDisposable
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Code to dispose the managed resources of the class

                //try
                //{

                //}
                //catch (Exception err)
                //{

                //}
            }

            // Code to dispose the un-managed resources of the class

            disposed = true;
        }
        #endregion

        /// <summary>
        /// ProberStation.cs method 와 연결 이벤트 핸들러
        /// </summary>
        /// <param name="image"></param>
        private void OnProcessingResultEvent(ImageBuffer image)
        {
            try
            {
                ImageProcessing(new ImageBuffer(image.Buffer, image.SizeX, image.SizeY, image.Band, image.ColorDept));
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }

        private void OnProcessingPattEvnet(byte[] pattimage, int ptSizeX, int ptSizeY, int colorDept)
        {
            try
            {
                ImagePattView(new ImageBuffer(pattimage, ptSizeX, ptSizeY, colorDept));
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }


        public void ViewResult(ImageBuffer procResultBuffer)
        {
            try
            {
                OnProcessingResultEvent(procResultBuffer);
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }
        public void ViewResult_Patt(int colorDept, byte[] _PattImageBuff)
        {
            try
            {
                OnProcessingPattEvnet(_PattImageBuff, PattSizeX, PattSizeY, colorDept);
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }

        public bool IsValidModLicense()
        {
            bool isValid = false;
            MIL_ID licenseModules = MIL.M_NULL;
            MIL_ID milapplication = MIL.M_NULL;
            try
            {
                MIL.MsysInquire(MilSystem, MIL.M_OWNER_APPLICATION, ref milapplication);

                MIL.MappInquire(milapplication, MIL.M_LICENSE_MODULES, ref licenseModules);


                if ((licenseModules & MIL.M_LICENSE_MOD) != MIL.M_LICENSE_MOD)
                {
                    LoggerManager.Debug($"IsValidModLicense(): Model Finder License Is Not Valid. Check Mil Dongle");
                }
                else
                {
                    isValid = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return isValid;
        }

        public IStageSupervisor StageSupervisor { get; set; }

        #region //..Blob

        /// <summary>
        /// Color Camera로 부터 넘어온 Color Grab이미지를 흑백 이미지로 변환.
        /// </summary>
        /// <param name="_grabedImage"></param>
        private void ColorToBlackAndWrite(ImageBuffer _grabedImage, ref MIL_ID milProcBuffer)
        {
            try
            {
                MIL_ID milColorGrabImage = MIL.M_NULL;
                MIL.MbufAllocColor(MilSystem, 3, _grabedImage.SizeX, _grabedImage.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milColorGrabImage);
                MIL.MbufPutColor(milColorGrabImage, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                MIL.MimConvert(milColorGrabImage, milProcBuffer, MIL.M_RGB_TO_L);
                MIL.MbufFree(milColorGrabImage);
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }



        /// <summary>
        /// 흑백 영상 Blob
        /// </summary>
        /// <param name="grabedImage"></param>
        /// <param name="blobparam"></param>
        /// <param name="fillholes"></param>
        /// <param name="invert"></param>
        /// <param name="runsort"></param>
        /// <returns></returns>
        public BlobResult BlobObject(ImageBuffer _grabedImage, BlobParameter blobparam, ROIParameter roiparam = null, bool fillholes = true, bool invert = false, bool runsort = true)
        {
            //SetProcessing(_grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.DataType);
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milBlobFeatureList = MIL.M_NULL;

            byte[] resultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY * 3];
            int nTotalBlobs = 0;

            ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();

            try
            {
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milProcBuffer);
                MIL.MbufAllocColor(MilSystem, 3, _grabedImage.SizeX, _grabedImage.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);

                MIL.MbufClear(milProcBuffer, 0);  //MilProcBuffer 버퍼를 0색상으로 지워준다.
                MIL.MbufClear(milProcResultBuffer, 0); //MilProcResultBuffer 버퍼를 0색상으로 지워준다.

                MIL.MbufPut(milProcBuffer, _grabedImage.Buffer);

                MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                if (invert == true)
                {
                    MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_IN_RANGE, 0, blobparam.BlobThreshHold.Value);
                }
                else
                {
                    MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_BIMODAL + MIL.M_GREATER, 0, 50);
                }

                if (blobparam.BlobMinRadius.Value > 0)
                {
                    //사전처리 : opening , closing =>노이즈제거 
                    //Remove small particles and then remove small holes
                    MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                    MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                }

                if (milBlobFeatureList == MIL.M_NULL)
                {
                    //Allocate a feature list
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                // Enable the Area and Center Of Gravity feature calculation.
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);

                // Calculate selected feature for each blob.
                //MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer, MIL.M_ERASE_BORDER_BLOBS, MIL.M_BINARY);

                if (fillholes == true)
                {
                    MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer, MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);
                }

                if (milBlobResult == MIL.M_NULL)
                {
                    //Allocate a blob result buffer.
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }

                //MIL.MblobControl(milBlobResult, MIL.M_FOREGROUND_VALUE, MIL.M_ZERO);

                //BlobAnalyzer[chnIndex].Calculate();
                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, milBlobFeatureList, milBlobResult);

                // Apply blob filter
                MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, blobparam.MinBlobArea.Value, MIL.M_NULL);

                if (roiparam != null)
                {
                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS, roiparam.OffsetX.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER, roiparam.OffsetX.Value + roiparam.Width.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS, roiparam.OffsetY.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER, roiparam.OffsetY.Value + roiparam.Height.Value, MIL.M_NULL);
                }

                // Get the total number of blobs.
                MIL_INT mIntTotalBlobs = 0;
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);
                nTotalBlobs = (int)mIntTotalBlobs;

                if (nTotalBlobs > 0)
                {
                    BlobCount = nTotalBlobs;
                    double[] dblCOGX = new double[nTotalBlobs];
                    double[] dblCOGY = new double[nTotalBlobs];

                    double[] offset = new double[2] { 0, 0 };

                    GrabDevPosition mChipPosition;

                    double[] dblBlobArea = new double[nTotalBlobs];
                    double[] dblAreainum = new double[nTotalBlobs];
                    double[] dblSizex = new double[nTotalBlobs];
                    double[] dblSizey = new double[nTotalBlobs];

                    double[] dblBox_X_Max = new double[nTotalBlobs];
                    double[] dblBox_X_Min = new double[nTotalBlobs];
                    double[] dblBox_Y_Max = new double[nTotalBlobs];
                    double[] dblBox_Y_Min = new double[nTotalBlobs];
                    double[] dblBox_Center = new double[nTotalBlobs];

                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblCOGX);// 각 블롭의 무게 중심의 X 위치를 반환합니다.
                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblCOGY);
                    MIL.MblobGetResult(milBlobResult, MIL.M_AREA, dblBlobArea);

                    for (int index = 0; index < nTotalBlobs; index++)
                    {
                        mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);

                        mChipPosition.Area = dblAreainum[index];
                        mChipPosition.DevIndex = index;
                        mChipPosition.PosX = Math.Round(dblCOGX[index], 3);
                        mChipPosition.PosY = Math.Round(dblCOGY[index], 3);

                        dblSizex[index] = dblBox_X_Max[index] - dblBox_X_Min[index];
                        mChipPosition.SizeX = dblSizex[index];


                        dblSizey[index] = dblBox_Y_Max[index] - dblBox_Y_Min[index];
                        mChipPosition.SizeY = dblSizey[index];

                        devicePositions.Add(mChipPosition);
                    }

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer, MIL.M_DRAW_CENTER_OF_GRAVITY, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer, MIL.M_DRAW_BOX, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                    MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                    MIL.MblobFree(milBlobFeatureList); milBlobFeatureList = MIL.M_NULL;
                    MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;
                    MIL.MbufFree(milProcBuffer); milProcBuffer = MIL.M_NULL;
                    MIL.MblobFree(milBlobResult); milBlobResult = MIL.M_NULL;

                    return new BlobResult(new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.ColorDept), devicePositions);
                }
                else //nTotalBlobs <= 0
                {
                    double fontsize = 3.0;
                    MIL.MbufClear(milProcResultBuffer, 0);
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MgraFontScale(MIL.M_DEFAULT, fontsize, fontsize);
                    MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer, _grabedImage.SizeX / 20, _grabedImage.SizeY / 8, " ==Blob NULL== ");
                    MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                    MIL.MblobFree(milBlobFeatureList); milBlobFeatureList = MIL.M_NULL;
                    MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;
                    MIL.MbufFree(milProcBuffer); milProcBuffer = MIL.M_NULL;
                    MIL.MblobFree(milBlobResult); milBlobResult = MIL.M_NULL;

                    return new BlobResult(new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.ColorDept), devicePositions);
                }
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_BLOB_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }
        /// <summary>
        /// 컬러 영상 Blob
        /// </summary>
        /// <param name="_grabedImage"></param>
        /// <param name="blobparam"></param>
        /// <param name="fillholes"></param>
        /// <param name="invert"></param>
        /// <param name="runsort"></param>
        /// <returns></returns>
        public BlobResult BlobColorObject(ImageBuffer _grabedImage, BlobParameter blobparam,
            bool fillholes = true, bool invert = false, bool runsort = true)
        {
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milColorGrabImage = MIL.M_NULL;
            MIL_ID milBlobFeatureList = MIL.M_NULL;

            byte[] resultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY * 3];
            int nTotalBlobs = 0;
            ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();
            try
            {
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY,
                                    _grabedImage.ColorDept, iAttributes, ref milProcBuffer);
                MIL.MbufAllocColor(MilSystem, 3, _grabedImage.SizeX, _grabedImage.SizeY,
                                                    8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);
                MIL.MbufAllocColor(MilSystem, 3, _grabedImage.SizeX, _grabedImage.SizeY,
                             8 + MIL.M_UNSIGNED, cl_iAttributes, ref milColorGrabImage);
                MIL.MbufClear(milProcBuffer, 0);
                MIL.MbufClear(milProcResultBuffer, 0);

                ColorToBlackAndWrite(_grabedImage, ref milProcBuffer);

                if (invert == true)
                {
                    //Binarize image
                    MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_IN_RANGE,
                                   0, blobparam.BlobThreshHold.Value);
                }
                else
                {
                    MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_IN_RANGE,
                                                      blobparam.BlobThreshHold.Value, 255);
                }

                if (blobparam.BlobMinRadius.Value > 0)
                {
                    //사전처리 : opening , closing =>노이즈제거 
                    //Remove small particles and then remove small holes
                    MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                    MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);

                }
                if (milBlobFeatureList == MIL.M_NULL)
                {
                    //Allocate a feature list
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                // Enable the Area and Center Of Gravity feature calculation.
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);

                // Calculate selected feature for each blob.
                MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer,
                                    MIL.M_ERASE_BORDER_BLOBS, MIL.M_BINARY);
                if (fillholes == true)
                {
                    MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer,
                                                        MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);
                }

                if (milBlobResult == MIL.M_NULL)
                {
                    //Allocate a blob result buffer.
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }
                //BlobAnalyzer[chnIndex].Calculate();
                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL,
                                    milBlobFeatureList, milBlobResult);

                // Apply blob filter
                MIL.MblobSelect(milBlobResult,
                                MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL,
                                blobparam.MinBlobArea.Value, MIL.M_NULL);

                // Get the total number of blobs.
                MIL_INT mIntTotalBlobs = 0;
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);
                nTotalBlobs = (int)mIntTotalBlobs;

                if (nTotalBlobs > 0)
                {
                    double[] dblCOGX = new double[nTotalBlobs];
                    double[] dblCOGY = new double[nTotalBlobs];

                    double[] offset = new double[2] { 0, 0 };

                    GrabDevPosition mChipPosition;

                    double[] dblBlobArea = new double[nTotalBlobs];
                    double[] dblAreainum = new double[nTotalBlobs];
                    double[] dblSizex = new double[nTotalBlobs];
                    double[] dblSizey = new double[nTotalBlobs];

                    double[] dblBox_X_Max = new double[nTotalBlobs];
                    double[] dblBox_X_Min = new double[nTotalBlobs];
                    double[] dblBox_Y_Max = new double[nTotalBlobs];
                    double[] dblBox_Y_Min = new double[nTotalBlobs];

                    //devicePositions = null;

                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);


                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblCOGX);// 각 블롭의 무게 중심의 X 위치를 반환합니다.
                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblCOGY);
                    MIL.MblobGetResult(milBlobResult, MIL.M_AREA, dblBlobArea);



                    for (int index = 0; index < nTotalBlobs; index++)
                    {
                        mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);

                        //dblAreainum[index] = Math.Pow((Math.Pow(dblBlobArea[index], 0.5) *
                        //mVisionSysParam.DigitizerParameters[1].VisionRatioX.Value), 2) / 1000;

                        mChipPosition.Area = dblAreainum[index];
                        mChipPosition.DevIndex = index;
                        mChipPosition.PosX = Math.Round(dblCOGX[index], 3);
                        mChipPosition.PosY = Math.Round(dblCOGY[index], 3);

                        dblSizex[index] = dblBox_X_Max[index] - dblBox_X_Min[index];
                        mChipPosition.SizeX = dblSizex[index];

                        dblSizey[index] = dblBox_Y_Max[index] - dblBox_Y_Min[index];
                        mChipPosition.SizeY = dblSizey[index];

                        devicePositions.Add(mChipPosition);

                    }
                    if (runsort == true) //PC_CAM은 flase , SRCSTAGE_CAM 은 true
                    {
                        /*
                        GrabDevPoss.DevPos = devicePositions;
                        SortPositionList(ref GrabDevPoss.DevPos, (double)blobparam.MinBlobArea.Value);
                      */
                    }
                    MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer,
                                    MIL.M_DRAW_CENTER_OF_GRAVITY, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer,
                                   MIL.M_DRAW_BOX, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);
                    MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                    MIL.MblobFree(milBlobResult); milBlobResult = MIL.M_NULL;
                    MIL.MblobFree(milBlobFeatureList); milBlobFeatureList = MIL.M_NULL;
                    MIL.MbufFree(milColorGrabImage); milColorGrabImage = MIL.M_NULL;
                    MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;
                    MIL.MbufFree(milProcBuffer); milProcBuffer = MIL.M_NULL;

                    return new BlobResult(new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.ColorDept), devicePositions);
                }
                else
                {
                    double fontsize = 3.0;
                    MIL.MbufClear(milProcResultBuffer, 0);
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MgraFontScale(MIL.M_DEFAULT, fontsize, fontsize);
                    MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer, _grabedImage.SizeX / 40, _grabedImage.SizeY / 16, " ==Blob NULL== ");
                    MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                    MIL.MblobFree(milBlobResult); milBlobResult = MIL.M_NULL;
                    MIL.MblobFree(milBlobFeatureList); milBlobFeatureList = MIL.M_NULL;
                    MIL.MbufFree(milColorGrabImage); milColorGrabImage = MIL.M_NULL;
                    MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;
                    MIL.MbufFree(milProcBuffer); milProcBuffer = MIL.M_NULL;

                    return new BlobResult(new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.ColorDept), devicePositions);
                }
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_BLOB_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
        }
        #endregion

        #region //..Patten Matching

        public PMResult PatternMatching(ImageBuffer targetImage, ImageBuffer patternImage, PMParameter pmparam, string pattpath, string maskpath, bool angleretry = false, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            MachineCoordinate machine = new MachineCoordinate();
            PMResult pMResult = new PMResult();

            MIL_ID milPMModels = MIL.M_NULL;
            MIL_ID milPattImageBuffer = MIL.M_NULL;
            MIL_ID maskImage = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_ID milResult = MIL.M_NULL;

            byte[] pattImageBuff = null;

            bool maskexistflag = false;
            bool IsExistPattern = false;

            MIL_INT miOffsetX, miOffsetY, pttSizeX = 0, pttSizeY = 0, pttType = 0;

            try
            {
                try
                {
                    double encoderxpos = 0.0;
                    double encoderypos = 0.0;
                    double encoderzpos = 0.0;
                    double encoderpzpos = 0.0;

                    this.MotionManager().GetRefPos(EnumAxisConstants.X, ref encoderxpos);
                    this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref encoderypos);
                    this.MotionManager().GetRefPos(EnumAxisConstants.Z, ref encoderzpos);
                    this.MotionManager().GetRefPos(EnumAxisConstants.PZ, ref encoderpzpos);

                    pmparam.MachineCoordPos = new MachineCoordinate(encoderxpos, encoderypos, encoderzpos, encoderpzpos, 0.0);

                    LoggerManager.Debug($"{this.GetType().Name}, PatternMatching() : pattpath = {pattpath}, maskpath = {maskpath}");

                    LoggerManager.Debug($"{this.GetType().Name}, PatternMatching() : X [{pmparam.MachineCoordPos.X.Value:0.00}] Y [{pmparam.MachineCoordPos.Y.Value:0.00}] Z [{pmparam.MachineCoordPos.Z.Value:0.00}] PZ [{pmparam.MachineCoordPos.PZ.Value:0.00}]", isInfo: IsInfo);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                if (patternImage != null)
                {
                    MIL.MbufAlloc2d(MilSystem, patternImage.SizeX, patternImage.SizeY, patternImage.ColorDept, MIL.M_IMAGE + MIL.M_PROC, ref milPattImageBuffer);
                    MIL.MbufPut2d(milPattImageBuffer, 0, 0, patternImage.SizeX, patternImage.SizeY, patternImage.Buffer);

                    pmparam.PattWidth.Value = patternImage.SizeX;
                    pmparam.PattHeight.Value = patternImage.SizeY;

                    IsExistPattern = true;
                }
                else
                {
                    if (File.Exists(pattpath))
                    {
                        MIL.MbufDiskInquire(pattpath, MIL.M_SIZE_X, ref pttSizeX);
                        MIL.MbufDiskInquire(pattpath, MIL.M_SIZE_Y, ref pttSizeY);
                        MIL.MbufDiskInquire(pattpath, MIL.M_TYPE, ref pttType);

                        pttType = 8 + MIL.M_UNSIGNED;

                        MIL.MbufAlloc2d(MilSystem, pttSizeX, pttSizeY, pttType, MIL.M_IMAGE + MIL.M_PROC, ref milPattImageBuffer);
                        MIL.MbufLoad(pattpath, milPattImageBuffer);

                        #region // Load 3-Band image and copy to 1-Band Image buffer.
                        MIL_ID milPattColImageBuffer = MIL.M_NULL;
                        MIL.MbufAllocColor(MilSystem, 3, pttSizeX, pttSizeY, 8 + MIL.M_UNSIGNED, Attributes, ref milPattColImageBuffer);
                        MIL.MbufLoad(pattpath, milPattColImageBuffer);
                        MIL.MbufCopy(milPattColImageBuffer, milPattImageBuffer);
                        #endregion

                        if (milPattColImageBuffer != MIL.M_NULL)
                        {
                            MIL.MbufFree(milPattColImageBuffer);
                            milPattColImageBuffer = MIL.M_NULL;
                        }

                        pmparam.PattWidth.Value = Convert.ToInt32(pttSizeX);
                        pmparam.PattHeight.Value = Convert.ToInt32(pttSizeY);

                        IsExistPattern = true;
                    }
                }

                if (IsExistPattern)
                {
                    if (File.Exists(maskpath))
                    {
                        MIL.MbufAlloc2d(MilSystem, pttSizeX, pttSizeY, pttType, MIL.M_IMAGE + MIL.M_PROC, ref maskImage);
                        MIL.MbufLoad(maskpath, maskImage);

                        maskexistflag = true;
                    }
                    else
                    {
                        maskexistflag = false;
                    }

                    pttSizeX = MIL.MbufInquire(milPattImageBuffer, MIL.M_SIZE_X);
                    pttSizeY = MIL.MbufInquire(milPattImageBuffer, MIL.M_SIZE_Y);

                    miOffsetX = pttSizeX / 2;
                    miOffsetY = pttSizeY / 2;

                    PattSizeX = (int)pttSizeX;
                    PattSizeY = (int)pttSizeY;

                    pattImageBuff = new byte[pttSizeX * pttSizeY];

                    MIL.MpatAllocModel(MilSystem, milPattImageBuffer, 0, 0, pttSizeX, pttSizeY, MIL.M_NORMALIZED, ref milPMModels);

                    if (maskexistflag)
                    {
                        MIL.MpatSetDontCare(milPMModels, maskImage, 0, 0, 0);
                    }

                    //Set the number of occurrences to find to ALL
                    MIL.MpatSetNumber(milPMModels, MIL.M_ALL);

                    MIL.MpatSetSpeed(milPMModels, MIL.M_LOW);
                    MIL.MpatSetAccuracy(milPMModels, MIL.M_HIGH);
                    MIL.MpatSetAcceptance(milPMModels, pmparam.PMAcceptance.Value);
                    MIL.MpatSetCertainty(milPMModels, pmparam.PMCertainty.Value);

                    //Theta
                    // Activate the search model angle mode.

                    double ROTATED_FIND_MIN_ANGLE_ACCURACY = 0.25;
                    int ROTATED_FIND_ROTATION_DELTA_ANGLE = 5;
                    int ROTATED_FIND_ANGLE_DELTA_POS = ROTATED_FIND_ROTATION_DELTA_ANGLE;
                    int ROTATED_FIND_ANGLE_DELTA_NEG = ROTATED_FIND_ROTATION_DELTA_ANGLE;

                    // TODO: Need to Check Parameter.
                    MIL.MpatSetAngle(milPMModels, MIL.M_SEARCH_ANGLE_MODE, MIL.M_ENABLE);
                    //MIL.MpatSetAngle(milPMModels, MIL.M_SEARCH_ANGLE_MODE, MIL.M_DISABLE);

                    // Set the search model range angle.
                    MIL.MpatSetAngle(milPMModels, MIL.M_SEARCH_ANGLE_DELTA_NEG, ROTATED_FIND_ANGLE_DELTA_NEG);
                    MIL.MpatSetAngle(milPMModels, MIL.M_SEARCH_ANGLE_DELTA_POS, ROTATED_FIND_ANGLE_DELTA_POS);

                    // Set the search model angle accuracy.
                    MIL.MpatSetAngle(milPMModels, MIL.M_SEARCH_ANGLE_ACCURACY, ROTATED_FIND_MIN_ANGLE_ACCURACY);

                    // Set the search model angle interpolation mode to bilinear.
                    MIL.MpatSetAngle(milPMModels, MIL.M_SEARCH_ANGLE_INTERPOLATION_MODE, MIL.M_BILINEAR);

                    // Preprocess the model.
                    MIL.MpatPreprocModel(milPattImageBuffer, milPMModels, MIL.M_DEFAULT);

                    if (sizex != 0 || sizey != 0)
                    {
                        if (sizex != 0 && sizey != 0)
                        {
                            SetPMSearchRegion(offsetx, offsety, sizex, sizey, ref milPMModels);
                        }
                        else
                        {
                            SetPMSearchRegion(offsetx, offsety, targetImage.SizeX - offsetx, targetImage.SizeY - offsety, ref milPMModels);
                        }
                    }
                    else
                    {
                        SetPMSearchRegion(offsetx, offsety, targetImage.SizeX, targetImage.SizeY, ref milPMModels);
                    }

                    //==FindModels

                    ObservableCollection<PMResultParameter> devicePositions = new ObservableCollection<PMResultParameter>();

                    byte[] ResultBuffer = new byte[targetImage.SizeX * targetImage.SizeY * 3];

                    int numOfOccur;

                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                    MIL.MbufAlloc2d(MilSystem, targetImage.SizeX, targetImage.SizeY, targetImage.ColorDept, iAttributes, ref milProcBuffer);

                    MIL.MbufAllocColor(MilSystem, 3, targetImage.SizeX, targetImage.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);

                    MIL.MpatAllocResult(MilSystem, MIL.M_DEFAULT, ref milResult);

                    if (targetImage.ColorDept == (int)ColorDept.BlackAndWhite)
                    {
                        MIL.MbufPut(milProcBuffer, targetImage.Buffer);
                    }
                    else if (targetImage.ColorDept == (int)ColorDept.Color24)
                    {
                        ColorToBlackAndWrite(targetImage, ref milProcBuffer);
                    }

                    MIL.MpatFindModel(milProcBuffer, milPMModels, milResult);

                    numOfOccur = (int)MIL.MpatGetNumber(milResult);

                    if (numOfOccur == 1)
                    {
                        double angles = 0.0;
                        double xposs = 0.0;
                        double yposs = 0.0;
                        double score = 0.0;

                        MIL.MpatGetResult(milResult, MIL.M_ANGLE, ref angles);
                        MIL.MpatGetResult(milResult, MIL.M_POSITION_X, ref xposs);
                        MIL.MpatGetResult(milResult, MIL.M_POSITION_Y, ref yposs);
                        MIL.MpatGetResult(milResult, MIL.M_SCORE, ref score);

                        if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                        {
                            if (targetImage.SizeX / 2.0 == (int)xposs)
                            {
                                xposs = targetImage.SizeX / 2.0;
                            }

                            if (targetImage.SizeY / 2.0 == (int)yposs)
                            {
                                yposs = targetImage.SizeY / 2.0;
                            }
                        }

                        devicePositions.Add(new PMResultParameter(xposs, yposs, angles, score));

                        retVal = EventCodeEnum.NONE;

                        MIL.MbufClear(milProcResultBuffer, 0);
                        MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                        MIL.MpatDraw(MIL.M_DEFAULT, milResult, milProcResultBuffer, MIL.M_DRAW_BOX + MIL.M_DRAW_POSITION, MIL.M_DEFAULT, MIL.M_DEFAULT);
                        MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ResultBuffer);

                        MIL.MbufGet(milPattImageBuffer, pattImageBuff);

                        LoggerManager.Debug($"Pattern Matching Result, Pixel X : {xposs}, Pixel Y : {yposs}, Angle : {angles}, Acceptance : {pmparam.PMAcceptance.Value}, Certainty : {pmparam.PMCertainty.Value}, Score : {score}", isInfo: IsInfo);
                    }
                    else if (numOfOccur > 1)
                    {
                        double[] angles;
                        double[] xposs;
                        double[] yposs;
                        double[] score;

                        angles = new double[numOfOccur];
                        xposs = new double[numOfOccur];
                        yposs = new double[numOfOccur];
                        score = new double[numOfOccur];

                        MIL.MpatGetResult(milResult, MIL.M_ANGLE, angles);
                        MIL.MpatGetResult(milResult, MIL.M_POSITION_X, xposs);
                        MIL.MpatGetResult(milResult, MIL.M_POSITION_Y, yposs);
                        MIL.MpatGetResult(milResult, MIL.M_SCORE, score);

                        // Calculate the screen center
                        double centerX = targetImage.SizeX / 2.0;
                        double centerY = targetImage.SizeY / 2.0;

                        // Find the closest occurrence to the screen center
                        int closestIndex = 0;
                        double minDistance = double.MaxValue;

                        // Score가 높은순으로 정렬 됨
                        for (int index = 0; index < numOfOccur; index++)
                        {
                            double distance = Math.Sqrt(Math.Pow(centerX - xposs[index], 2) + Math.Pow(centerY - yposs[index], 2));

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestIndex = index;
                            }

                            LoggerManager.Debug($"Pattern Matching Multi Result: Index : {index + 1}, Pixel X : {xposs[index]}, Pixel Y : {yposs[index]}, Angle : {angles[index]}, Acceptance : {pmparam.PMAcceptance.Value}, Certainty : {pmparam.PMCertainty.Value}, Score : {score[index]}, Distance : {distance.ToString("0.00")}");
                        }

                        LoggerManager.Debug($"Pattern Matching selected index = {closestIndex + 1}");

                        devicePositions.Add(new PMResultParameter(xposs[closestIndex], yposs[closestIndex], angles[closestIndex], score[closestIndex]));

                        retVal = EventCodeEnum.NONE;

                        MIL.MbufClear(milProcResultBuffer, 0);
                        MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                        MIL.MpatDraw(MIL.M_DEFAULT, milResult, milProcResultBuffer, MIL.M_DRAW_BOX + MIL.M_DRAW_POSITION, closestIndex, MIL.M_DEFAULT);
                        MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ResultBuffer);

                        MIL.MbufGet(milPattImageBuffer, pattImageBuff);
                    }
                    else
                    {
                        retVal = EventCodeEnum.VISION_PM_NOT_FOUND;

                        string targetImagePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.VISIONPROCESSING, IMAGE_SAVE_TYPE.BMP, true, "\\PM\\FAIL\\", "TargetImage");
                        string patternImagePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.VISIONPROCESSING, IMAGE_SAVE_TYPE.BMP, true, "\\PM\\FAIL\\", "Patternimage");

                        SaveMilIdToImage(milProcBuffer, targetImagePath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);
                        SaveMilIdToImage(milPattImageBuffer, patternImagePath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                        LoggerManager.Debug($"PatternMaching Fail(Pattern not found), Acceptance : {pmparam.PMAcceptance.Value}", isInfo: IsInfo);

                        MIL.MbufGet(milProcBuffer, targetImage.Buffer);
                        MIL.MbufGet(milPattImageBuffer, pattImageBuff);

                        if (pMResult.FailOriginImageBuffer == null)
                        {
                            pMResult.FailOriginImageBuffer = new ImageBuffer();
                        }
                        if (pMResult.FailOriginImageBuffer.Buffer == null)
                        {
                            pMResult.FailOriginImageBuffer.Buffer = new byte[targetImage.Buffer.Length];
                        }

                        targetImage.CopyTo(pMResult.FailOriginImageBuffer);
                        pMResult.FailPatternImageBuffer = new ImageBuffer(pattImageBuff, PattSizeX, PattSizeY, targetImage.Band, targetImage.ColorDept, targetImage.CamType);
                    }

                    pMResult.ResultBuffer = targetImage;
                    pMResult.ResultBuffer.Buffer = ResultBuffer;
                    pMResult.RetValue = retVal;
                    pMResult.PattImage = pattImageBuff;
                    pMResult.ResultParam = devicePositions;

                    if (retVal == EventCodeEnum.NONE)
                    {
                        GetGrayLevel(ref targetImage);
                        pMResult.ResultBuffer.GrayLevelValue = targetImage.GrayLevelValue;
                        pMResult.ResultBuffer.ColorDept = (int)ColorDept.Color24;
                    }
                }
                else
                {
                    pMResult.RetValue = EventCodeEnum.VISION_PATTERN_NOTEXIST;
                    LoggerManager.Error($"PatternMaching Fail(Pattern not exist) : pattern path [{pattpath}]");
                }

            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PM_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milProcBuffer != 0) MIL.MbufFree(milProcBuffer); milProcBuffer = MIL.M_NULL;
                if (milProcResultBuffer != 0) MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;
                if (milResult != 0) MIL.MpatFree(milResult); milResult = MIL.M_NULL;
                if (milPMModels != 0) MIL.MpatFree(milPMModels); milPMModels = MIL.M_NULL;
                if (milPattImageBuffer != 0) MIL.MbufFree(milPattImageBuffer); milPattImageBuffer = MIL.M_NULL;
                if (maskImage != 0) MIL.MbufFree(maskImage); maskImage = MIL.M_NULL;
            }

            return pMResult;
        }
        private void SetPMSearchRegion(int offx, int offy, int sizex, int sizey, ref MIL_ID milPMModels)
        {
            try
            {
                MIL.MpatSetPosition(milPMModels, offx, offy, sizex, sizey);
            }
            catch (MILException err)
            {
                throw new VisionException(err.Message, err, EventCodeEnum.VISION_PM_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }

        }
        public PMResult SearchPatternMatching(ImageBuffer img, PatternInfomation ptinfo, bool findone = true)
        {
            PMResult pMResult = new PMResult();

            try
            {
                int offset = 6;
                MIL_INT pttSizeX = 0, pttSizeY = 0, pttType = 0;
                string pattpath = ptinfo.PMParameter.ModelFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value;
                MIL.MbufDiskInquire(pattpath, MIL.M_SIZE_X, ref pttSizeX);
                MIL.MbufDiskInquire(pattpath, MIL.M_SIZE_Y, ref pttSizeY);

                int offsetx = (int)pttSizeX + offset;
                int offsety = (int)pttSizeY + offset;

                while (true)
                {
                    pMResult = PatternMatching(img, null, ptinfo.PMParameter, pattpath, ptinfo.PMParameter.MaskFilePath.Value + ptinfo.PMParameter.PatternFileExtension.Value, true, img.SizeX - (offsetx / 2), img.SizeY - (offsety / 2), offsetx, offsety);

                    offsetx += offset;
                    offsety += offset;

                    if (offsetx >= img.SizeX || offsety >= img.SizeY)
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return pMResult;
        }
        public EventCodeEnum GetGrayLevel(ref ImageBuffer img)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MIL_ID milGrabImage = MIL.M_NULL;
                MIL_ID milMean = MIL.M_NULL;
                MIL_ID mimImage = MIL.M_NULL;
                int graylevelValues = 0;

                MIL.MbufAlloc2d(MilSystem, img.SizeX, img.SizeY,
                                 img.ColorDept, iAttributes, ref milGrabImage);
                MIL.MbufAlloc2d(MilSystem, img.SizeX, img.SizeY, img.ColorDept, lAttributes, ref mimImage);
                if (img.ColorDept == (int)ColorDept.BlackAndWhite) { MIL.MbufPut(mimImage, img.Buffer); }
                else if (img.ColorDept == (int)ColorDept.Color24)
                {
                    ColorToBlackAndWrite(img, ref milGrabImage);
                    MIL.MbufCopy(milGrabImage, mimImage);
                }
                MIL.MimAllocResult(MilSystem, MIL.M_DEFAULT, MIL.M_STAT_LIST, ref milMean);
                MIL.MimStat(mimImage, milMean, MIL.M_MEAN, MIL.M_IN_RANGE, 0, 255);
                MIL.MimGetResult(milMean, MIL.M_MEAN + MIL.M_TYPE_MIL_INT, ref graylevelValues);

                img.GrayLevelValue = graylevelValues;

                MIL.MimFree(milMean);
                MIL.MbufFree(mimImage);
                MIL.MbufFree(milGrabImage); milGrabImage = MIL.M_NULL;
                retVal = EventCodeEnum.NONE;
            }
            catch (MILException err)
            {
                throw new VisionException("GetGrayLevel Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            return retVal;
        }

        public int GetFocusValue(ImageBuffer img, Rect roi = new Rect())
        {
            Rect roiRect = roi;
            int focusval = 0;

            MIL_ID ROIImage = MIL.M_NULL;
            MIL_ID focusImage = MIL.M_NULL;

            if ((roiRect.Left < 0) || (roiRect.Top < 0) ||
                (roiRect.Left >= img.SizeX) || (roiRect.Top >= img.SizeY) ||
                (roiRect.Width <= 0) || (roiRect.Height <= 0))
            {
                roiRect.X = 0;
                roiRect.Y = 0;
                roiRect.Width = img.SizeX;
                roiRect.Height = img.SizeY;
            }

            try
            {
                if (img.SizeX != 0 && img.SizeY != 0)
                {
                    MIL_INT focuslevelValue = 0;

                    MIL.MbufAlloc2d(MilSystem, img.SizeX, img.SizeY, (int)ColorDept.BlackAndWhite, iAttributes, ref focusImage);

                    if (img.ColorDept == (int)ColorDept.Color24)
                    {
                        ColorToBlackAndWrite(img, ref focusImage);
                    }

                    MIL.MbufPut(focusImage, img.Buffer);
                    MIL.MbufChild2d(focusImage, (int)roiRect.X, (int)roiRect.Y, (int)roiRect.Width, (int)roiRect.Height, ref ROIImage);

                    MIL.MdigFocus(MIL.M_NULL, focusImage, ROIImage, null, MIL.M_NULL, MIL.M_NULL,
                    MIL.M_NULL, MIL.M_NULL, MIL.M_NULL, MIL.M_EVALUATE + MIL.M_NO_FILTER, ref focuslevelValue); //+MIL.M_NO_SUBSAMPLING

                    focusval = (int)focuslevelValue;
                }
                else
                {
                    focusval = 0;
                }

            }
            catch (MILException err)
            {
                LoggerManager.Debug($"GetFoucusValue(): Exception occurred. Err = {err.Message}, Stack Trace = {err.StackTrace}");
                throw new VisionException("GetFoucusValue Error ", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"GetFoucusValue(): Exception occurred. Err = {err.Message}, Stack Trace = {err.StackTrace}");
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (ROIImage != MIL.M_NULL) MIL.MbufFree(ROIImage);
                if (focusImage != MIL.M_NULL) MIL.MbufFree(focusImage);
            }

            return focusval;
        }


        #endregion

        #region //.. RegistPattenModel

        byte[] ResultBuffer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pmparam"></param>
        /// <param name="PattWidth"></param>
        /// <param name="PattHeight"></param>
        /// <param name="rotangle"></param>
        /// <param name="pattFullPath"></param>
        public void RegistModel(ImageBuffer _grabedImage, int offsetx = 0, int offsety = 0, int PattWidth = 128, int PattHeight = 128, double rotangle = 0.0, string pattFullPath = "", bool isregistMask = true)
        {
            MIL_ID MilGrabImage = MIL.M_NULL;
            MIL_ID MilProcBuffer = MIL.M_NULL;

            ResultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY];

            try
            {
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, iAttributes, ref MilProcBuffer);
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, iAttributes, ref MilGrabImage);

                if (_grabedImage.ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    MIL.MbufPut(MilProcBuffer, _grabedImage.Buffer);
                }
                else if (_grabedImage.ColorDept == (int)ColorDept.Color24)
                {
                    ColorToBlackAndWrite(_grabedImage, ref MilProcBuffer);
                }

                if (rotangle != 0.0)
                {
                    MIL.MimRotate(MilProcBuffer, MilProcBuffer, rotangle, offsetx, offsety, _grabedImage.SizeX / 2, _grabedImage.SizeY / 2, MIL.M_BILINEAR);
                }

                RegistModel_2(MilProcBuffer, _grabedImage.ColorDept, PattWidth, PattHeight, offsetx, offsety, pattFullPath, isregistMask);
            }
            catch (MILException err)
            {
                throw new VisionException("RegistModel Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (MilGrabImage != MIL.M_NULL) MIL.MbufFree(MilGrabImage);
                if (MilProcBuffer != MIL.M_NULL) MIL.MbufFree(MilProcBuffer);
            }
        }

        /// <summary>
        /// 실질적 파일 저장 함수 _1에서 호출
        /// </summary>
        /// <param name="modelimage"></param>
        /// <param name="pmparam"></param>
        /// <param name="pattwidth"></param>
        /// <param name="pattheight"></param>
        /// <param name="pattposX"></param>
        /// <param name="pattposy"></param>
        /// <param name="pattFullPath"></param>
        private void RegistModel_2(MIL_ID modelimage, int colordept, int pattwidth = 128, int pattheight = 128, int pattposX = 0, int pattposy = 0, string pattFullPath = "", bool isregistMask = true)
        {
            MIL_ID milPattImageBuffer = new MIL_ID();
            MIL_ID milChildBuffer = new MIL_ID();

            try
            {
                long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;

                MIL.MbufAlloc2d(MilSystem, pattwidth, pattheight, 8, Attributes, ref milPattImageBuffer);

                MIL.MbufChild2d(modelimage, pattposX, pattposy, pattwidth, pattheight, ref milChildBuffer);

                MIL.MbufCopy(milChildBuffer, milPattImageBuffer);

                if (pattFullPath != "")
                {
                    if (Directory.Exists(Path.GetDirectoryName(pattFullPath)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(pattFullPath));
                    }

                    MIL.MbufSave(pattFullPath, milPattImageBuffer);

                    var newFilePath = Path.ChangeExtension(pattFullPath, ".bmp");

                    MIL.MbufExport(newFilePath, MIL.M_BMP, milPattImageBuffer);
                }
                else
                {
                    LoggerManager.Debug($"RegistModel(): Model path is not specified.");
                }
            }
            catch (MILException err)
            {
                throw new VisionException("RegistModel Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milChildBuffer != MIL.M_NULL) MIL.MbufFree(milChildBuffer);
                if (milPattImageBuffer != MIL.M_NULL) MIL.MbufFree(milPattImageBuffer);
            }
        }
        public EventCodeEnum SaveImageBuffer(ImageBuffer _grabedImage, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MIL_ID SaveBuffer = MIL.M_NULL;

            try
            {
                if (_grabedImage != null)
                {
                    ResultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY];

                    if (width == 0 && height == 0)
                    {
                        width = _grabedImage.SizeX;
                        height = _grabedImage.SizeY;
                    }

                    if (offsetx != 0 || offsety != 0)
                    {
                        _grabedImage = ReduceImageSize(_grabedImage, offsetx, offsety, width, height);
                    }

                    if (_grabedImage.ColorDept == (int)ColorDept.BlackAndWhite)
                    {
                        MIL.MbufAlloc2d(MilSystem, width, height, _grabedImage.ColorDept, iAttributes, ref SaveBuffer);
                        MIL.MbufPut(SaveBuffer, _grabedImage.Buffer);
                    }
                    else if (_grabedImage.ColorDept == (int)ColorDept.Color24)
                    {
                        MIL.MbufAllocColor(MilSystem, 3, width, height, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref SaveBuffer);
                        MIL.MbufPutColor(SaveBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                    }
                    else if (_grabedImage.ColorDept == (int)ColorDept.Color32)
                    {
                        MIL.MbufAllocColor(MilSystem, 3, width, height, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref SaveBuffer);
                        MIL.MbufPutColor(SaveBuffer, MIL.M_RGB32 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                    }

                    if (SaveBuffer != MIL.M_NULL)
                    {
                        if (rotangle != 0.0)
                        {
                            MIL.MimRotate(SaveBuffer, SaveBuffer, rotangle, offsetx, offsety, width / 2, height / 2, MIL.M_BILINEAR);
                        }

                        retVal = SaveMilIdToImage(SaveBuffer, path, logtype, eventcode);
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], SaveImage(): SaveBuffer is null.");
                    }
                }
            }
            catch (MILException err)
            {
                throw new VisionException("SaveImage Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (SaveBuffer != MIL.M_NULL) MIL.MbufFree(SaveBuffer); SaveBuffer = MIL.M_NULL;
            }
            return retVal;
        }

        public EventCodeEnum SaveImageBufferWithRectnagle(ImageBuffer _grabedImage, string path, IMAGE_LOG_TYPE logtype, EventCodeEnum eventcode, Rect focusROI, int offsetx = 0, int offsety = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            MIL_ID SaveBufferTemp = MIL.M_NULL;
            MIL_ID SaveBuffer = MIL.M_NULL;

            try
            {
                if (_grabedImage != null)
                {
                    ResultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY];

                    if (width == 0 && height == 0)
                    {
                        width = _grabedImage.SizeX;
                        height = _grabedImage.SizeY;
                    }

                    if (offsetx != 0 || offsety != 0)
                    {
                        _grabedImage = ReduceImageSize(_grabedImage, offsetx, offsety, width, height);
                    }

                    if (_grabedImage.ColorDept == (int)ColorDept.BlackAndWhite)
                    {
                        MIL.MbufAlloc2d(MilSystem, width, height, _grabedImage.ColorDept, iAttributes, ref SaveBufferTemp);
                        MIL.MbufPut(SaveBufferTemp, _grabedImage.Buffer);

                        BlackAndWhiteToColor(SaveBufferTemp, ref SaveBuffer, true);
                    }
                    else if (_grabedImage.ColorDept == (int)ColorDept.Color24)
                    {
                        MIL.MbufAllocColor(MilSystem, 3, width, height, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref SaveBuffer);
                        MIL.MbufPutColor(SaveBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                    }
                    else if (_grabedImage.ColorDept == (int)ColorDept.Color32)
                    {
                        MIL.MbufAllocColor(MilSystem, 3, width, height, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref SaveBuffer);
                        MIL.MbufPutColor(SaveBuffer, MIL.M_RGB32 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                    }

                    if (SaveBuffer != MIL.M_NULL)
                    {
                        if(focusROI != null)
                        {
                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                            
                            MIL.MgraText(MIL.M_DEFAULT, SaveBuffer, 5, 5, $"({focusROI.X:0.0}, {focusROI.Y:0.0}, {focusROI.Width:0.0}, {focusROI.Height:0.0})");
                            MIL.MgraRect(MIL.M_DEFAULT, SaveBuffer, focusROI.X, focusROI.Y, focusROI.X + focusROI.Width, focusROI.Y + focusROI.Height);
                        }

                        if (rotangle != 0.0)
                        {
                            MIL.MimRotate(SaveBuffer, SaveBuffer, rotangle, offsetx, offsety, width / 2, height / 2, MIL.M_BILINEAR);
                        }

                        retVal = SaveMilIdToImage(SaveBuffer, path, logtype, eventcode);
                    }
                    else
                    {
                        LoggerManager.Debug($"[{this.GetType().Name}], SaveImage(): SaveBuffer is null.");
                    }
                }
            }
            catch (MILException err)
            {
                throw new VisionException("SaveImage Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (SaveBufferTemp != MIL.M_NULL)
                {
                    MIL.MbufFree(SaveBufferTemp);
                    SaveBufferTemp = MIL.M_NULL;
                }

                if (SaveBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(SaveBuffer);
                    SaveBuffer = MIL.M_NULL;
                }
            }
            return retVal;
        }

        /// <summary>
        ///  이미지 바이트 스트림을 Bitmap로 변환.   
        /// </summary>
        /// <param name="rdata"></param>
        /// <param name="gdata"></param>
        /// <param name="bdata"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap RGBByteStreamtoImage(byte[] data, int width, int height)
        {
            Bitmap bmp = null;

            try
            {
                if (width == 0 || height == 0)
                {
                    return bmp;
                }

                bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int bytes = Math.Abs(bmpdata.Stride) * bmp.Height;
                byte[] rgbValues = new byte[bytes];

                System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, rgbValues, 0, bytes);

                int pixelByte = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;

                for (int y = 0; y < height; y++)
                {
                    int invHeight = y;

                    for (int x = 0; x < width; x++)
                    {
                        rgbValues[invHeight * bmpdata.Stride + (x * pixelByte)] = data[(y * width + x) * 3 + 2];
                        rgbValues[invHeight * bmpdata.Stride + (x * pixelByte) + 1] = data[(y * width + x) * 3 + 1];
                        rgbValues[invHeight * bmpdata.Stride + (x * pixelByte) + 2] = data[(y * width + x) * 3];
                        rgbValues[invHeight * bmpdata.Stride + (x * pixelByte) + 3] = 255;
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpdata.Scan0, bytes);
                bmp.UnlockBits(bmpdata);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bmp;
        }
        public static Bitmap GrayByteStreamtoImage(byte[] data, int width, int height)
        {
            Bitmap bmp = null;

            try
            {
                if (width == 0 || height == 0)
                {
                    return bmp;
                }

                bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                ColorPalette pal = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    pal.Entries[i] = Color.FromArgb(i, i, i);
                }
                bmp.Palette = pal;

                BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                int bytes = Math.Abs(bmpdata.Stride) * bmp.Height;
                byte[] grayValues = new byte[bytes];

                System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, grayValues, 0, bytes);

                int pixelByte = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;

                for (int y = 0; y < height; y++)
                {
                    int invHeight = y;

                    for (int x = 0; x < width; x++)
                    {
                        grayValues[invHeight * bmpdata.Stride + x] = data[y * width + x];
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(grayValues, 0, bmpdata.Scan0, bytes);
                bmp.UnlockBits(bmpdata);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bmp;
        }
        private EventCodeEnum SaveMilIdToImage(MIL_ID SrcBufId, string FileName, IMAGE_LOG_TYPE logtype, EventCodeEnum eventCodeEnum)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (FileName != string.Empty)
                {
                    if (SrcBufId != MIL.M_NULL)
                    {
                        Bitmap ori = null;

                        MIL_INT SizeX, SizeY, BufBand;

                        SizeX = MIL.MbufInquire(SrcBufId, MIL.M_SIZE_X, MIL.M_NULL);
                        SizeY = MIL.MbufInquire(SrcBufId, MIL.M_SIZE_Y, MIL.M_NULL);
                        BufBand = MIL.MbufInquire(SrcBufId, MIL.M_SIZE_BAND, MIL.M_NULL);

                        if (BufBand == 1)
                        {
                            byte[] data = new byte[SizeX * SizeY];

                            MIL.MbufGet2d(SrcBufId, 0, 0, SizeX, SizeY, data);

                            ori = GrayByteStreamtoImage(data, (int)SizeX, (int)SizeY);
                        }
                        else
                        {
                            // TODO : BufBand 값과 RGBByteStreamtoImage() 동작 테스트 필요

                            byte[] data = new byte[SizeX * SizeY * 3];

                            MIL.MbufGetColor(SrcBufId, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, data);

                            ori = RGBByteStreamtoImage(data, (int)SizeX, (int)SizeY);
                        }

                        if (ori != null)
                        {
                            string extension = Path.GetExtension(FileName).ToLower();
                            ImageFormat format = ImageFormat.Jpeg; // 기본값

                            switch (extension)
                            {
                                case ".bmp":
                                    format = ImageFormat.Bmp;
                                    break;
                                case ".png":
                                    format = ImageFormat.Png;
                                    break;
                                case ".jpeg":
                                case ".jpg":
                                    format = ImageFormat.Jpeg;
                                    break;
                                    // 추가 확장자에 따른 처리
                            }

                            string directoryPath = Path.GetDirectoryName(FileName);

                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            ori.Save(FileName, format);
                            retVal = EventCodeEnum.NONE;
                            LoggerManager.Debug($"[{this.GetType().Name}] SaveImage() : Path = {FileName}, EventCodeEnum = {eventCodeEnum}", isInfo: IsInfo);
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}] SaveImage() : Fail, Bitmap is null, {FileName}");
                        }
                    }

                    // Can perform the same operation as MbufSave(), using MbufExport() with its FileFormat parameter set to M_MIL.
                    //MIL.MbufExport(FileName, FileFormat, SrcBufId);
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], SaveImage() : FileName is Empty.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SaveMilIdToImage() FileName : {FileName}");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum AddEdgePosBuffer(ImageBuffer _grabedImage, double x = 0, double y = 0, int width = 0, int height = 0, double rotangle = 0.0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MIL_ID SaveBuffer = MIL.M_NULL;
            try
            {
                ResultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY];

                if (width == 0 && height == 0)
                {
                    width = _grabedImage.SizeX;
                    height = _grabedImage.SizeY;
                }

                if (_grabedImage.ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    MIL.MbufAlloc2d(MilSystem, width, height,
                                     _grabedImage.ColorDept, iAttributes, ref SaveBuffer);
                    MIL.MbufPut(SaveBuffer, _grabedImage.Buffer);
                }
                else if (_grabedImage.ColorDept == (int)ColorDept.Color24)
                {
                    MIL.MbufAllocColor(MilSystem, 3, width, height,
                        8 + MIL.M_UNSIGNED, cl_iAttributes, ref SaveBuffer);
                    MIL.MbufPutColor(SaveBuffer, MIL.M_RGB24 + MIL.M_PACKED,
                        MIL.M_ALL_BANDS, _grabedImage.Buffer);
                }
                else if (_grabedImage.ColorDept == (int)ColorDept.Color32)
                {
                    MIL.MbufAllocColor(MilSystem, 3, width, height,
                        8 + MIL.M_UNSIGNED, cl_iAttributes, ref SaveBuffer);
                    MIL.MbufPutColor(SaveBuffer, MIL.M_RGB32 + MIL.M_PACKED,
                        MIL.M_ALL_BANDS, _grabedImage.Buffer);
                }

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                MIL.MgraRectFill(MIL.M_DEFAULT, SaveBuffer, x - 2, y - 2, x + 2, y + 2);

                if (rotangle != 0.0)
                {
                    MIL.MimRotate(SaveBuffer, SaveBuffer,
                    rotangle, x, y, width / 2, height / 2, MIL.M_BILINEAR);
                }

                if (_grabedImage.ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    MIL.MbufGet(SaveBuffer, _grabedImage.Buffer);
                }
                else if (_grabedImage.ColorDept == (int)ColorDept.Color24)
                {
                    MIL.MbufGetColor(SaveBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                }
                else if (_grabedImage.ColorDept == (int)ColorDept.Color32)
                {
                    MIL.MbufGetColor(SaveBuffer, MIL.M_RGB32 + MIL.M_PACKED, MIL.M_ALL_BANDS, _grabedImage.Buffer);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (MILException err)
            {
                throw new VisionException("AddEdgePosBuffer Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (SaveBuffer != MIL.M_NULL) MIL.MbufFree(SaveBuffer); SaveBuffer = MIL.M_NULL;
            }
            return retVal;
        }

        public ImageBuffer ResizeImageBuffer(ImageBuffer ib, int ScaleFactorX, int ScaleFactorY)
        {
            ImageBuffer img = null;
            byte[] byteArr = null;

            MIL_ID originalBuffer = new MIL_ID();
            MIL_ID resizedBuffer = new MIL_ID();

            try
            {
                if (ib.ColorDept == (int)ColorDept.BlackAndWhite)
                {
                    byteArr = new byte[ScaleFactorX * ScaleFactorY];

                    MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, iAttributes, ref originalBuffer);
                    MIL.MbufPut2d(originalBuffer, 0, 0, ib.SizeX, ib.SizeY, ib.Buffer);

                    MIL.MbufAlloc2d(MilSystem, ScaleFactorX, ScaleFactorY, ib.ColorDept, iAttributes, ref resizedBuffer);
                    MIL.MimResize(originalBuffer, resizedBuffer, MIL.M_FILL_DESTINATION, MIL.M_FILL_DESTINATION, MIL.M_NEAREST_NEIGHBOR);

                    MIL.MbufGet(resizedBuffer, byteArr);
                }
                else if (ib.ColorDept == (int)ColorDept.Color24 || ib.ColorDept == (int)ColorDept.Color32)
                {
                    byteArr = new byte[ScaleFactorX * ScaleFactorY * 3];

                    MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref originalBuffer);
                    MIL.MbufPutColor(originalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, ib.Buffer);

                    MIL.MbufAllocColor(MilSystem, 3, ScaleFactorX, ScaleFactorY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref resizedBuffer);
                    MIL.MimResize(originalBuffer, resizedBuffer, MIL.M_FILL_DESTINATION, MIL.M_FILL_DESTINATION, MIL.M_NEAREST_NEIGHBOR);

                    MIL.MbufGetColor(resizedBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, byteArr);
                }

                img = new ImageBuffer();
                ib.CopyTo(img);

                img.SizeX = ScaleFactorX;
                img.SizeY = ScaleFactorY;
                img.Buffer = byteArr;
            }
            catch (MILException err)
            {
                throw new VisionException("ReduceImageSize Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (resizedBuffer != 0) MIL.MbufFree(resizedBuffer);
                if (originalBuffer != 0) MIL.MbufFree(originalBuffer);
            }

            return img;
        }

        public ImageBuffer ReduceImageSize(ImageBuffer ib, int offsetx = 0, int offsety = 0, int sizex = 0, int sizey = 0)
        {
            ImageBuffer img = null;
            MIL_ID originalBuffer = new MIL_ID();
            MIL_ID sizeconvertBuffer = MIL.M_NULL;
            byte[] byteArr = null;

            try
            {
                if (ib.SizeX <= sizex && ib.SizeY <= sizey)
                {
                    return ib;
                }
                else
                {
                    if (ib.ColorDept == (int)ColorDept.BlackAndWhite)
                    {
                        byteArr = new byte[sizex * sizey];

                        MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, iAttributes, ref originalBuffer);
                        MIL.MbufPut2d(originalBuffer, 0, 0, ib.SizeX, ib.SizeY, ib.Buffer);

                        if (ib.SizeX < offsetx + sizex || ib.SizeY < offsety + sizey)
                        {
                            return ib;
                        }

                        MIL.MbufChild2d(originalBuffer, offsetx, offsety, sizex, sizey, ref sizeconvertBuffer);

                        MIL.MbufGet(sizeconvertBuffer, byteArr);

                    }
                    else if (ib.ColorDept == (int)ColorDept.Color24 || ib.ColorDept == (int)ColorDept.Color32)
                    {
                        byteArr = new byte[sizex * sizey * 3];

                        MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref originalBuffer);
                        MIL.MbufPutColor(originalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, ib.Buffer);

                        MIL.MbufChild2d(originalBuffer, offsetx, offsety, sizex, sizey, ref sizeconvertBuffer);
                        MIL.MbufGetColor(sizeconvertBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, byteArr);
                    }

                    img = new ImageBuffer();
                    ib.CopyTo(img);

                    img.SizeX = sizex;
                    img.SizeY = sizey;
                    img.Buffer = byteArr;
                }
            }
            catch (MILException err)
            {
                throw new VisionException("ReduceImageSize Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (sizeconvertBuffer != 0) MIL.MbufFree(sizeconvertBuffer);
                if (originalBuffer != 0) MIL.MbufFree(originalBuffer);
            }

            return img;
        }

        public int CutSizeConvertEdge(ref ImageBuffer ib)
        {
            MIL_ID originalBuffer = new MIL_ID();
            int offsetValue = 0;

            try
            {
                if (ib.SizeX > ib.SizeY)
                {
                    MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept,
                                    iAttributes, ref originalBuffer);
                    MIL.MbufPut(originalBuffer, ib.Buffer);

                    MIL_ID sizeconvertBuffer = MIL.M_NULL;
                    offsetValue = (ib.SizeX - ib.SizeY) / 2;

                    MIL.MbufChild2d(originalBuffer, offsetValue, 0, ib.SizeY,
                       ib.SizeY, ref sizeconvertBuffer);

                    byte[] byteArr = new byte[ib.SizeY * ib.SizeY];
                    MIL.MbufGet(sizeconvertBuffer, byteArr);

                    ib.Buffer = byteArr;
                    ib.SizeX = ib.SizeY;
                    MIL.MbufFree(sizeconvertBuffer); sizeconvertBuffer = MIL.M_NULL;
                }
                else if (ib.SizeX < ib.SizeY)
                {
                    MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, 8,
                                    iAttributes, ref originalBuffer);
                    MIL.MbufPut(originalBuffer, ib.Buffer);

                    MIL_ID sizeconvertBuffer = MIL.M_NULL;

                    offsetValue = (ib.SizeY - ib.SizeX) / 2;

                    MIL.MbufChild2d(originalBuffer, 0, offsetValue, ib.SizeX,
                        ib.SizeY - offsetValue, ref sizeconvertBuffer);
                    byte[] byteArr = new byte[ib.SizeX * ib.SizeX];
                    MIL.MbufGet(sizeconvertBuffer, byteArr);

                    ib.Buffer.CopyTo(byteArr, 0);
                    ib.SizeY = ib.SizeX;
                    MIL.MbufFree(sizeconvertBuffer); sizeconvertBuffer = MIL.M_NULL;

                }
                return 1;
            }
            catch (MILException err)
            {
                throw new VisionException("CutSizeConvertEdge Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                MIL.MbufFree(originalBuffer); originalBuffer = MIL.M_NULL;
            }
        }

        private byte[,] ConvertByte1DTo2DArray(ImageBuffer img)
        {
            byte[,] RetByte2DArray = new byte[img.SizeY, img.SizeX];
            try
            {

                int WidthStep;

                for (int j = 0; j < img.SizeY; j++)
                {
                    WidthStep = j * img.SizeX;

                    for (int i = 0; i < img.SizeX; i++)
                    {
                        RetByte2DArray[j, i] = img.Buffer[WidthStep + i];
                    }
                }

            }

            catch (MILException err)
            {
                throw new VisionException("ConvertByte1DTo2DArray Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            return RetByte2DArray;
        }

        public ImageBuffer Line_Equalization_Processing(ImageBuffer img, int Cpos)
        {
            ImageBuffer RetImg = null;

            int Line_Pixel_count = 0;

            double Line_PSum = 0;
            double Line_PAvg = 0;

            int i, j, m, n;

            MIL_ID Test_Img = MIL.M_NULL;
            try
            {

                byte[,] ImgByte2DArray_INPUT = ConvertByte1DTo2DArray(img);
                byte[,] ImgByte2DArray_OUTPUT = new byte[img.SizeY, img.SizeX];


                if (img.SizeX != img.SizeY)
                {
                    return RetImg;
                }

                MIL.MbufAlloc2d(MilSystem, img.SizeX, img.SizeY, img.ColorDept, iAttributes, ref Test_Img);

                MIL.MbufPut2d(Test_Img, 0, 0, img.SizeX, img.SizeY, ImgByte2DArray_INPUT);

                RetImg = new ImageBuffer(img.SizeX, img.SizeY, img.Band, (img.ColorDept * img.Band));

                int RWidth = img.SizeX - 1;
                int RHeight = img.SizeY - 1;

                if (Cpos == 0 || Cpos == 2)
                {
                    for (i = RWidth; i >= 0; i--)
                    {
                        m = 0;
                        n = 0;

                        Line_PSum = 0;
                        Line_Pixel_count = 0;

                        while (true)
                        {
                            Line_PSum += ImgByte2DArray_INPUT[m, (i + m)];

                            Line_Pixel_count++;

                            if ((i + m) == RWidth)
                            {
                                break;
                            }

                            m++;

                        }

                        Line_PAvg = Line_PSum / Line_Pixel_count;

                        while (true)
                        {
                            ImgByte2DArray_OUTPUT[n, (i + n)] = (byte)Line_PAvg;

                            if ((i + n) == RWidth)
                            {
                                break;
                            }

                            n++;

                        }

                    }

                    for (j = RHeight; j >= 0; j--)
                    {
                        m = 0;
                        n = 0;

                        Line_PSum = 0;
                        Line_Pixel_count = 0;

                        while (true)
                        {
                            Line_PSum += ImgByte2DArray_INPUT[(j + m), m];

                            Line_Pixel_count++;

                            if ((j + m) == RHeight)
                            {
                                break;
                            }

                            m++;
                        }

                        Line_PAvg = Line_PSum / Line_Pixel_count;

                        while (true)
                        {
                            ImgByte2DArray_OUTPUT[(j + n), n] = (byte)Line_PAvg;

                            if ((j + n) == RHeight)
                            {
                                break;
                            }

                            n++;

                        }
                    }
                }
                else
                {
                    for (i = 0; i <= RWidth; i++)
                    {
                        m = 0;
                        n = 0;

                        Line_PSum = 0;
                        Line_Pixel_count = 0;

                        while (true)
                        {
                            Line_PSum += ImgByte2DArray_INPUT[m, (i - m)];

                            Line_Pixel_count++;

                            if ((i - m) == 0)
                            {
                                break;
                            }

                            m++;
                        }

                        Line_PAvg = Line_PSum / Line_Pixel_count;

                        while (true)
                        {
                            ImgByte2DArray_OUTPUT[n, (i - n)] = (byte)Line_PAvg;

                            if ((i - n) == 0)
                            {
                                break;
                            }

                            n++;
                        }

                    }

                    for (j = RHeight; j >= 0; j--)
                    {
                        m = RWidth;
                        n = RWidth;

                        Line_PSum = 0;
                        Line_Pixel_count = 0;

                        while (true)
                        {
                            Line_PSum += ImgByte2DArray_INPUT[m, j + (RWidth - m)];

                            Line_Pixel_count++;

                            if (j + (RWidth - m) == RWidth)
                            {
                                break;
                            }

                            m--;
                        }

                        Line_PAvg = Line_PSum / Line_Pixel_count;

                        while (true)
                        {
                            ImgByte2DArray_OUTPUT[n, j + (RWidth - n)] = (byte)Line_PAvg;

                            if (j + (RWidth - n) == RWidth)
                            {
                                break;
                            }

                            n--;

                        }
                    }
                }

                MIL.MbufPut2d(Test_Img, 0, 0, img.SizeX, img.SizeY, ImgByte2DArray_OUTPUT);
                MIL.MbufGet(Test_Img, RetImg.Buffer);
            }
            catch (MILException err)
            {
                throw new VisionException("Line_Equalization_Processing Error", err,
                    EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (Test_Img != MIL.M_NULL)
                {
                    MIL.MbufFree(Test_Img); Test_Img = MIL.M_NULL;
                }
            }
            return RetImg;
        }

        public EdgeResult EdgeProcessing(ImageBuffer ib)
        {
            MIL_ID MilEdgeContext = MIL.M_NULL;
            MIL_ID MilEdgeResult = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_INT NumEdgeFound = 0;
            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milBlobFeatureList = MIL.M_NULL;
            try
            {

                EdgeResult result = new EdgeResult();

                // Number of results found.
                MIL_INT NumResults = 0;

                byte[] resultarr = new byte[ib.SizeX * ib.SizeY * 3];

                MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, iAttributes, ref milProcBuffer);
                MIL.MbufPut(milProcBuffer, ib.Buffer);
                MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);

                MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                //MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_IN_RANGE, 120, 255);
                MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_BIMODAL + MIL.M_GREATER, MIL.M_NULL, MIL.M_NULL);

                MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);

                string curDate = string.Format("C:\\Logs\\Images\\Binarized_{0:00}{1:00}{2:00}{3:00}.bmp", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Millisecond);

                //===
                if (milBlobResult == MIL.M_NULL)
                {
                    //Allocate a blob result buffer.
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }
                if (milBlobFeatureList == MIL.M_NULL)
                {
                    //Allocate a feature list
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                // Enable the Area and Center Of Gravity feature calculation.
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);



                MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer,
                                      MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);


                //MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer, MIL.M_ERASE_BORDER_BLOBS, MIL.M_BINARY + MIL.M_8_CONNECTED);
                //MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer, MIL.M_FOREGROUND_ZERO, MIL.M_BINARY + MIL.M_8_CONNECTED);

                //BlobAnalyzer[chnIndex].Calculate();
                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL,
                                    milBlobFeatureList, milBlobResult);
                MIL_INT mIntTotalBlobs = 0;
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                if (mIntTotalBlobs > 0)
                {
                    double[] blobArea = new double[mIntTotalBlobs];
                    MIL.MblobGetResult(milBlobResult, MIL.M_AREA, blobArea);

                    for (int blobIndex = 0; blobIndex < mIntTotalBlobs; blobIndex++)
                    {
                        LoggerManager.Debug($"EdgeProcessing(): Blob#{blobIndex} area = {blobArea[blobIndex]}", isInfo: IsInfo);
                    }

                    double[] boxX = new double[mIntTotalBlobs];
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, boxX);
                }

                // Apply blob filter
                //MIL.MblobSelect(milBlobResult,MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL,20000, MIL.M_NULL);
                //MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, 20000, MIL.M_NULL);


                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);
                //===
                MIL.MblobFill(milBlobResult, milProcBuffer, MIL.M_ALL_BLOBS, 255);

                MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcBuffer, MIL.M_DRAW_BLOBS, MIL.M_EXCLUDED_BLOBS, MIL.M_DEFAULT);

                MIL.MedgeAlloc(MilSystem, MIL.M_CONTOUR, MIL.M_DEFAULT, ref MilEdgeContext);
                MIL.MedgeAllocResult(MilSystem, MIL.M_DEFAULT, ref MilEdgeResult);

                MIL.MedgeControl(MilEdgeContext, MIL.M_MOMENT_ELONGATION, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_FERET_MEAN_DIAMETER + MIL.M_SORT1_DOWN, MIL.M_DISABLE);

                MIL.MedgeControl(MilEdgeContext, MIL.M_FAST_LENGTH, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_MOMENT_ELONGATION_ANGLE, MIL.M_ENABLE);

                MIL.MedgeControl(MilEdgeContext, MIL.M_FILTER_SMOOTHNESS, 70.0);

                MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_TYPE, MIL.M_FULL_HYSTERESIS);
                MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_MODE, MIL.M_HIGH);
                MIL.MedgeControl(MilEdgeContext, MIL.M_LINE_FIT_ERROR, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_BOX_X_MAX, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_FERET_X, MIL.M_ENABLE);

                MIL.MedgeCalculate(MilEdgeContext, milProcBuffer, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL, MilEdgeResult, MIL.M_DEFAULT);

                // Get the number of edges found.
                MIL.MedgeSelect(MilEdgeResult, MIL.M_DELETE, MIL.M_FAST_LENGTH, MIL.M_LESS_OR_EQUAL, 120, 120);
                //MIL.MedgeSelect(MilEdgeResult, MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_LESS_OR_EQUAL, 200, MIL.M_NULL);

                MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_CHAINS + MIL.M_TYPE_MIL_INT, ref NumEdgeFound);

                if (NumEdgeFound > 0)
                {
                    double[] xpos = new double[NumEdgeFound];
                    double[] ypos = new double[NumEdgeFound];
                    double[] lengths = new double[NumEdgeFound];
                    double[] feretx = new double[NumEdgeFound];
                    double[] boxX = new double[NumEdgeFound];
                    double[] boxY = new double[NumEdgeFound];
                    double[] angles = new double[NumEdgeFound];

                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_FAST_LENGTH, ref lengths[0]);
                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_MOMENT_ELONGATION_ANGLE, ref angles[0]);

                    for (int edgeIndex = 0; edgeIndex < NumEdgeFound; edgeIndex++)
                    {
                        if (lengths[edgeIndex] > 100 & (Math.Abs(angles[edgeIndex]) > 38 & Math.Abs(angles[edgeIndex]) < 50) | (Math.Abs(angles[edgeIndex]) > 130 & Math.Abs(angles[edgeIndex]) < 140))
                        {
                            MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY, ref xpos[0], ref ypos[0]);
                            MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_FERET_X, ref feretx[0], MIL.M_NULL);
                            Point templist = new Point();
                            templist.X = xpos[0];
                            templist.Y = ypos[0];
                            result.EdgePoint = new Point(xpos[0], ypos[0]);

                        }
                        else
                        {
                            // Invalid edge
                            LoggerManager.Error($"Invalid edge");
                        }

                        MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_LINE_FIT_ERROR, ref xpos[0]);
                    }

                    result.Result = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"Edge Not Found");
                }

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milProcResultBuffer, MIL.M_DRAW_EDGES, MIL.M_DEFAULT, MIL.M_DEFAULT);
                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milProcResultBuffer, MIL.M_DRAW_CENTER_OF_GRAVITY, MIL.M_DEFAULT, MIL.M_DEFAULT);
                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);
                MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milProcResultBuffer, MIL.M_DRAW_INDEX, MIL.M_DEFAULT, MIL.M_DEFAULT);

                MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultarr);

                ImageBuffer reltib = new ImageBuffer(resultarr, ib.SizeX, ib.SizeY, ib.ColorDept, ib.ColorDept);

                return result;
            }

            catch (MILException err)
            {
                throw new VisionException("EdgeProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milBlobFeatureList != MIL.M_NULL)
                {
                    MIL.MblobFree(milBlobFeatureList);
                    milBlobFeatureList = MIL.M_NULL;
                }
                if (milBlobResult != MIL.M_NULL)
                {
                    MIL.MblobFree(milBlobResult);
                    milBlobResult = MIL.M_NULL;
                }
                if (MilEdgeContext != MIL.M_NULL)
                {
                    MIL.MedgeFree(MilEdgeContext);
                    MilEdgeContext = MIL.M_NULL;
                }
                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                    milProcBuffer = MIL.M_NULL;
                }
                if (MilEdgeResult != MIL.M_NULL)
                {
                    MIL.MedgeFree(MilEdgeResult);
                    MilEdgeResult = MIL.M_NULL;
                }

                if (milProcResultBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcResultBuffer);
                    milProcResultBuffer = MIL.M_NULL;
                }
            }
        }

        public int CreatePACLMaskImage(ImageBuffer ib, string path)
        {
            int retVal = -1;
            MIL_ID background = MIL.M_NULL;
            MIL_ID roi = MIL.M_NULL;

            try
            {
                MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, 8, iAttributes, ref background);
                MIL.MbufClear(background, 255);

                MIL.MbufAlloc2d(MilSystem, ib.SizeX, (ib.SizeY / 5), 8, iAttributes, ref roi);
                MIL.MbufClear(roi, 0);

                int offsety = (ib.SizeY / 5);
                MIL.MbufCopyClip(roi, background, 0, (offsety * 2));

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                MIL.MbufSave(Path.Combine(path, "PACL_mask.bmp"), background);
                MIL.MbufSave(Path.Combine(path, "PACL_mask.mmo"), background);

                retVal = 1;
            }
            catch (MILException err)
            {
                throw new VisionException("CreatePACLMaskImage Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (background != MIL.M_NULL)
                {
                    MIL.MbufFree(background);
                    background = MIL.M_NULL;
                }
                if (roi != MIL.M_NULL)
                {
                    MIL.MbufFree(roi);
                    roi = MIL.M_NULL;
                }

            }
            return retVal;
        }

        public ImageBuffer LoadImageFile(string filepath)
        {
            MIL_ID milPattImageBuffer = MIL.M_NULL;
            MIL_INT pttSizeX = 0, pttSizeY = 0, pttType = 0;
            byte[] ResultBuffer = null;
            int band = 1;

            try
            {
                if (File.Exists(filepath))
                {
                    MIL.MbufDiskInquire(filepath, MIL.M_SIZE_X, ref pttSizeX);
                    MIL.MbufDiskInquire(filepath, MIL.M_SIZE_Y, ref pttSizeY);
                    MIL.MbufDiskInquire(filepath, MIL.M_TYPE, ref pttType);

                    MIL.MbufAlloc2d(MilSystem, pttSizeX, pttSizeY, pttType,MIL.M_IMAGE + MIL.M_PROC,ref milPattImageBuffer);

                    MIL.MbufLoad(filepath, milPattImageBuffer);

                    if ((int)pttType == (int)ColorDept.BlackAndWhite)
                    {
                        band = 1;
                        ResultBuffer = new byte[pttSizeX * pttSizeY * band];
                        MIL.MbufGet(milPattImageBuffer, ResultBuffer);
                    }
                    else if ((int)pttType == (int)ColorDept.Color24)
                    {
                        band = 3;
                        ResultBuffer = new byte[pttSizeX * pttSizeY * band];
                        MIL.MbufGetColor(milPattImageBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ResultBuffer);
                    }
                }

                return new ImageBuffer(ResultBuffer, (int)pttSizeX, (int)pttSizeY, band, (int)pttType);
            }
            catch (MILException err)
            {
                throw new VisionException("LoadImageFile Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milPattImageBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milPattImageBuffer); milPattImageBuffer = MIL.M_NULL;
                }
            }
        }

        public int GetIntegralImg(ImageBuffer Ori, ref IntegralImage Result)
        {
            try
            {
                int WidthStep;

                //Result = new IntegralImage(Ori.SizeX,Ori.SizeY);

                //byte[] image_input = (byte[])Ori.Buffer.Clone();
                long[] integralImg = new long[Ori.Buffer.Length];

                int idx = 0;

                for (int h = 0; h < Ori.SizeY; h++)
                {
                    WidthStep = h * Ori.SizeX;

                    for (int w = 0; w < Ori.SizeX; w++)
                    {
                        idx = WidthStep + w;

                        if (h == 0)
                        {
                            if (w == 0)
                            {
                                integralImg[idx] = Ori.Buffer[idx];
                            }
                            else
                            {
                                integralImg[idx] = integralImg[idx - 1] + Ori.Buffer[idx];
                            }
                        }
                        else
                        {
                            if (w == 0)
                            {
                                integralImg[idx] = integralImg[idx - Ori.SizeX] + Ori.Buffer[idx];
                            }
                            else
                            {
                                integralImg[idx] = integralImg[idx - Ori.SizeX] + integralImg[idx - 1] + Ori.Buffer[idx] - integralImg[idx - Ori.SizeX - 1];
                            }
                        }

                    }
                }

                Result.nSumImage = Convert1Dto2D(integralImg, Ori.SizeX, Ori.SizeY);


            }
            catch (MILException err)
            {
                throw new VisionException("GetIntegralImg Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }

            return 1;

        }

        public long[][] Convert1Dto2D(long[] ori1D, int width, int height)
        {
            try
            {

                long[][] Data2D = new long[height][];
                for (int i = 0; i < height; i++)
                    Data2D[i] = new long[width];

                for (int srcStartOff = 0, h = 0; h < height; srcStartOff += width, h++)
                    Array.Copy(ori1D, srcStartOff, Data2D[h], 0, width);

                return Data2D;

            }
            catch (MILException err)
            {
                throw new VisionException("Convert1Dto2D Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }

        }

        public long GetIntegralSum(IntegralImage Input, int x, int y, int bx, int by)
        {
            try
            {
                if (bx < 1 || by < 1)
                    return -1;

                if (x >= Input.Width || y >= Input.Height || x < 1 || y < 1)
                    return -1;

                if (x + bx >= Input.Width || y + by >= Input.Height)
                    return -1;

                bx--;
                by--;

                return Input.nSumImage[y + by][x + bx] - Input.nSumImage[y + by][x - 1] - Input.nSumImage[y - 1][x + bx] + Input.nSumImage[y - 1][x - 1];
            }
            catch (MILException err)
            {
                throw new VisionException("GetIntegralSum Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }

        }

        public ObservableCollection<GrabDevPosition> Detect_Pad(ImageBuffer _grabedImage, double UserPadSizeX, double UserPadSizeY, ROIParameter roiparam, bool fillholes = true)
        {
            MIL_ID milOriBuf = MIL.M_NULL;
            MIL_ID milBinanyBuf = MIL.M_NULL;
            MIL_ID milNoiseRemoveBuf = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;

            MIL_ID milBlobFeatureList = MIL.M_NULL;
            MIL_ID milBlobResult = MIL.M_NULL;

            int nTotalBlobs = 0;

            byte[] resultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY * 3];

            List<Point> pointlist = new List<Point>();

            ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();

            try
            {
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milOriBuf);
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milBinanyBuf);
                MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milNoiseRemoveBuf);

                MIL.MbufAllocColor(MilSystem, 3, _grabedImage.SizeX, _grabedImage.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);

                MIL.MbufClear(milOriBuf, 0);
                MIL.MbufClear(milBinanyBuf, 0);
                MIL.MbufClear(milNoiseRemoveBuf, 0);

                MIL.MbufPut(milOriBuf, _grabedImage.Buffer);

                MIL.MimBinarize(milOriBuf, milBinanyBuf, MIL.M_BIMODAL + MIL.M_GREATER, MIL.M_NULL, MIL.M_NULL);

                //사전처리 : opening , closing =>노이즈제거 
                //Remove small particles and then remove small holes
                MIL.MimOpen(milBinanyBuf, milNoiseRemoveBuf, 1, MIL.M_BINARY);
                MIL.MimClose(milNoiseRemoveBuf, milNoiseRemoveBuf, 1, MIL.M_BINARY);

                MIL.MbufCopy(milNoiseRemoveBuf, milProcResultBuffer);

                if (milBlobFeatureList == MIL.M_NULL)
                {
                    //Allocate a feature list
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                // Enable the Area and Center Of Gravity feature calculation.
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);

                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_FERET_X);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_FERET_Y);

                if (fillholes == true)
                {
                    MIL.MblobReconstruct(milNoiseRemoveBuf, MIL.M_NULL, milNoiseRemoveBuf,
                                                        MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);
                    MIL.MblobReconstruct(milNoiseRemoveBuf, MIL.M_NULL, milNoiseRemoveBuf,
                                                        MIL.M_FOREGROUND_ZERO, MIL.M_BINARY + MIL.M_8_CONNECTED);
                }

                if (milBlobResult == MIL.M_NULL)
                {
                    //Allocate a blob result buffer.
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }

                MIL.MblobCalculate(milNoiseRemoveBuf, MIL.M_NULL, milBlobFeatureList, milBlobResult);

                // Apply blob filter
                //MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, PadRegister.Pads.BlobParam.MinBlobArea.Value, MIL.M_NULL);

                if (roiparam != null)
                {
                    MIL.MblobSelect(milBlobResult,
                    MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS,
                    roiparam.OffsetX.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult,
                    MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER,
                    roiparam.OffsetX.Value + roiparam.Width.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult,
                    MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS,
                    roiparam.OffsetY.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult,
                    MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER,
                    roiparam.OffsetY.Value + roiparam.Height.Value, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult,
                    MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_LESS,
                    UserPadSizeX * 2 / 3, MIL.M_NULL);

                    MIL.MblobSelect(milBlobResult,
                    MIL.M_EXCLUDE, MIL.M_FERET_Y, MIL.M_LESS,
                    UserPadSizeY * 2 / 3, MIL.M_NULL);
                }

                // Get the total number of blobs.
                MIL_INT mIntTotalBlobs = 0;
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);
                nTotalBlobs = (int)mIntTotalBlobs;

                if (nTotalBlobs > 0)
                {
                    //double[] offset = new double[2] { 0, 0 };
                    //GrabDevPosition mChipPosition;

                    double[] dblBlobArea = new double[nTotalBlobs];
                    double[] dblCOGX = new double[nTotalBlobs];
                    double[] dblCOGY = new double[nTotalBlobs];

                    double[] dblBox_X_Max = new double[nTotalBlobs];
                    double[] dblBox_X_Min = new double[nTotalBlobs];
                    double[] dblBox_Y_Max = new double[nTotalBlobs];
                    double[] dblBox_Y_Min = new double[nTotalBlobs];

                    //double[] dblAreainum = new double[nTotalBlobs];

                    double[] dblSizex = new double[nTotalBlobs];
                    double[] dblSizey = new double[nTotalBlobs];

                    double[] dblFeretX = new double[nTotalBlobs];
                    double[] dblFeretY = new double[nTotalBlobs];

                    //devicePositions = null;

                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblCOGX);// 각 블롭의 무게 중심의 X 위치를 반환합니다.
                    MIL.MblobGetResult(milBlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblCOGY);
                    MIL.MblobGetResult(milBlobResult, MIL.M_AREA, dblBlobArea);

                    MIL.MblobGetResult(milBlobResult, MIL.M_FERET_X, dblFeretX);
                    MIL.MblobGetResult(milBlobResult, MIL.M_FERET_Y, dblFeretY);

                    for (int index = 0; index < nTotalBlobs; index++)
                    {
                        GrabDevPosition mChipPosition;

                        mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);

                        mChipPosition.DevIndex = index;
                        mChipPosition.Area = dblBlobArea[index];
                        //mChipPosition.PosX = Math.Round(dblCOGX[index], 3);
                        //mChipPosition.PosY = Math.Round(dblCOGY[index], 3);

                        mChipPosition.PosX = dblCOGX[index];
                        mChipPosition.PosY = dblCOGY[index];

                        //mChipPosition.PosX = dblFeretX[index] / 2;
                        //mChipPosition.PosY = dblFeretY[index] / 2;

                        dblSizex[index] = dblBox_X_Max[index] - dblBox_X_Min[index];
                        mChipPosition.SizeX = dblSizex[index];

                        dblSizey[index] = dblBox_Y_Max[index] - dblBox_Y_Min[index];
                        mChipPosition.SizeY = dblSizey[index];

                        devicePositions.Add(mChipPosition);
                    }
                    MIL.MbufCopy(milNoiseRemoveBuf, milProcResultBuffer);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer,
                                    MIL.M_DRAW_CENTER_OF_GRAVITY, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcResultBuffer,
                                   MIL.M_DRAW_BOX, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                    MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);
                    //blobresult.ResultBuffer = new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, _grabedImage.DataType, _grabedImage.ColorDept);
                }
            }
            catch (MILException err)
            {
                throw new VisionException("Detect_Pad Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                MIL.MbufFree(milOriBuf); milOriBuf = MIL.M_NULL;
                MIL.MbufFree(milBinanyBuf); milBinanyBuf = MIL.M_NULL;
                MIL.MbufFree(milNoiseRemoveBuf); milNoiseRemoveBuf = MIL.M_NULL;
                MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;

                MIL.MblobFree(milBlobFeatureList); milBlobFeatureList = MIL.M_NULL;
                MIL.MblobFree(milBlobResult); milBlobResult = MIL.M_NULL;
            }

            return devicePositions;
        }

        #endregion

        public EdgeProcResult FindEdgeProcessing(ImageBuffer ib, bool saveDump = false)
        {
            LoggerManager.Debug($"Before.FindEdgeProcessing");
            EdgeProcResult edgeProcRel = new EdgeProcResult();

            edgeProcRel.Edges = new List<Point>();
            edgeProcRel.ImageSize = new Point(ib.SizeX, ib.SizeY);

            MIL_ID MilEdgeContext = MIL.M_NULL;
            MIL_ID MilEdgeResult = MIL.M_NULL;
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_INT NumEdgeFound = 0;
            MIL_ID milBlobResult = MIL.M_NULL;
            MIL_ID milBlobFeatureList = MIL.M_NULL;
            MIL_ID maskBuffer = MIL.M_NULL;

            MIL_INT NumResults = 0;                                         // Number of results found.

            LoggerManager.Debug($"Before.createbuffer");
            byte[] resultarr = new byte[ib.SizeX * ib.SizeY * 3];
            LoggerManager.Debug($"after.createbuffer");

            LoggerManager.Debug($"Before.MbufAlloc2d");
            //MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, iAttributes, ref milProcBuffer);

            // Process buffer는 흑백 이므로 ib.ColorDept와 상관 없이 8비트로 설정
            MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, 8, iAttributes, ref milProcBuffer);
            LoggerManager.Debug($"after.MbufAlloc2d");

            try
            {
                if (ib.Band == 3)
                {
                    ColorToBlackAndWrite(ib, ref milProcBuffer);
                }
                else
                {
                    MIL.MbufPut(milProcBuffer, ib.Buffer);
                }

                MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);
                MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                double lowParam = 160;
                MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_IN_RANGE, lowParam, 255);

                MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);

                //===
                if (milBlobResult == MIL.M_NULL)
                {
                    //Allocate a blob result buffer.
                    MIL.MblobAllocResult(MilSystem, ref milBlobResult);
                }

                if (milBlobFeatureList == MIL.M_NULL)
                {
                    //Allocate a feature list
                    MIL.MblobAllocFeatureList(MilSystem, ref milBlobFeatureList);
                }

                // Enable the Area and Center Of Gravity feature calculation.
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_AXIS_PRINCIPAL_ANGLE);

                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MAX);
                MIL.MblobSelectFeature(milBlobFeatureList, MIL.M_BOX_Y_MIN);

                MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer, MIL.M_FILL_HOLES, MIL.M_BINARY + MIL.M_8_CONNECTED);

                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, milBlobFeatureList, milBlobResult);
                MIL_INT mIntTotalBlobs = 0;
                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);

                if (mIntTotalBlobs > 0)
                {
                    double[] blobArea = new double[mIntTotalBlobs];
                    MIL.MblobGetResult(milBlobResult, MIL.M_AREA, blobArea);

                    double[] boxX = new double[mIntTotalBlobs];
                    MIL.MblobGetResult(milBlobResult, MIL.M_BOX_X_MAX, boxX);
                }

                // Apply blob filter
                MIL.MblobSelect(milBlobResult, MIL.M_EXCLUDE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, 30000, MIL.M_NULL);

                MIL.MblobGetNumber(milBlobResult, ref mIntTotalBlobs);
                //===
                MIL.MblobFill(milBlobResult, milProcBuffer, MIL.M_ALL_BLOBS, 255);

                MIL.MblobDraw(MIL.M_DEFAULT, milBlobResult, milProcBuffer, MIL.M_DRAW_BLOBS, MIL.M_EXCLUDED_BLOBS, MIL.M_DEFAULT);

                MIL.MedgeAlloc(MilSystem, MIL.M_CONTOUR, MIL.M_DEFAULT, ref MilEdgeContext);
                MIL.MedgeAllocResult(MilSystem, MIL.M_DEFAULT, ref MilEdgeResult);

                MIL.MedgeControl(MilEdgeContext, MIL.M_MOMENT_ELONGATION, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_FERET_MEAN_DIAMETER + MIL.M_SORT1_DOWN, MIL.M_DISABLE);

                MIL.MedgeControl(MilEdgeContext, MIL.M_FAST_LENGTH, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_MOMENT_ELONGATION_ANGLE, MIL.M_ENABLE);

                MIL.MedgeControl(MilEdgeContext, MIL.M_FILTER_SMOOTHNESS, 70.0);

                MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_TYPE, MIL.M_FULL_HYSTERESIS);
                MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_MODE, MIL.M_HIGH);
                MIL.MedgeControl(MilEdgeContext, MIL.M_LINE_FIT_ERROR, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_BOX_X_MAX, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_FERET_X, MIL.M_ENABLE);
                //MIL.MedgeControl(MilEdgeContext, MIL.M_RESULT_OUTPUT_UNITS, MIL.M_PIXEL);

                string path = "C:\\Logs\\Images";
                string maskpath = Path.Combine(path, "PACL_mask.mmo");

                if (File.Exists(maskpath) == false)
                {
                    CreatePACLMaskImage(ib, path);
                }

                //MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, MIL.M_IMAGE + MIL.M_PROC, ref maskBuffer);

                // Process buffer는 흑백 이므로 ib.ColorDept와 상관 없이 8비트로 설정
                MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, 8, MIL.M_IMAGE + MIL.M_PROC, ref maskBuffer);

                MIL.MbufLoad(maskpath, maskBuffer);

                MIL.MedgeMask(MilEdgeContext, maskBuffer, MIL.M_DEFAULT);

                MIL.MedgeCalculate(MilEdgeContext, milProcBuffer, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL, MilEdgeResult, MIL.M_DEFAULT);

                // Get the number of edges found.
                MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_CHAINS + MIL.M_TYPE_MIL_INT, ref NumEdgeFound);

                if (NumEdgeFound > 0)
                {
                    double[] xpos = new double[NumEdgeFound];
                    double[] ypos = new double[NumEdgeFound];
                    double[] lengths = new double[NumEdgeFound];
                    double[] boxX = new double[NumEdgeFound];
                    double[] boxY = new double[NumEdgeFound];
                    double[] angles = new double[NumEdgeFound];

                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_FAST_LENGTH, ref lengths[0]);
                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_MOMENT_ELONGATION_ANGLE, ref angles[0]);

                    for (int edgeIndex = 0; edgeIndex < NumEdgeFound; edgeIndex++)
                    {
                        MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_CENTER_OF_GRAVITY, ref xpos[0], ref ypos[0]);
                        MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_FERET_X, ref boxX[0], MIL.M_NULL);

                        edgeProcRel.Edges.Add(new Point(xpos[edgeIndex], ypos[edgeIndex]));

                        MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_LINE_FIT_ERROR, ref xpos[0]);
                    }
                }
                else
                {
                    LoggerManager.Error($"Edge Not Found");
                }

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milProcResultBuffer, MIL.M_DRAW_EDGES, MIL.M_DEFAULT, MIL.M_DEFAULT);
                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milProcResultBuffer, MIL.M_DRAW_CENTER_OF_GRAVITY, MIL.M_DEFAULT, MIL.M_DEFAULT);

                MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultarr);
            }

            catch (MILException err)
            {
                throw new VisionException("FindEdgeProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (maskBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(maskBuffer);
                    maskBuffer = MIL.M_NULL;
                }
                if (milBlobFeatureList != MIL.M_NULL)
                {
                    MIL.MblobFree(milBlobFeatureList);
                    milBlobFeatureList = MIL.M_NULL;
                }
                if (milBlobResult != MIL.M_NULL)
                {
                    MIL.MblobFree(milBlobResult);
                    milBlobResult = MIL.M_NULL;
                }
                if (MilEdgeContext != MIL.M_NULL)
                {
                    MIL.MedgeFree(MilEdgeContext);
                    MilEdgeContext = MIL.M_NULL;
                }
                if (milProcBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcBuffer);
                    milProcBuffer = MIL.M_NULL;
                }
                if (MilEdgeResult != MIL.M_NULL)
                {
                    MIL.MedgeFree(MilEdgeResult);
                    MilEdgeResult = MIL.M_NULL;
                }

                if (milProcResultBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milProcResultBuffer);
                    milProcResultBuffer = MIL.M_NULL;
                }
            }
            return edgeProcRel;
        }

        public CassetteScanSlotResult CassetteScanProcessing(ImageBuffer ib, CassetteScanSlotParam slotparam, bool saveDump = false)
        {
            MIL_ID MilEdgeContext = MIL.M_NULL;                             // Edge context.
            MIL_ID MilEdgeResult = MIL.M_NULL;                              // Edge result identifier.
            MIL_INT NumEdgeFound = 0;                                       // Number of edges found.

            MIL_ID milBufProc = MIL.M_NULL;
            MIL_ID milBufResult = MIL.M_NULL;

            MIL_ID milBufMask = MIL.M_NULL;
            MIL_ID milBufRoi = MIL.M_NULL;

            bool isColorBuffer = ib.Band > 1;

            ConvolveType convolveType = ConvolveType.M_HORIZONTAL_EDGE_PREWITT;
            bool performConvoleProcessing = false;
            try
            {
                //ib.DataType = 32;
                int imgWidth = ib.SizeX;
                int imgHeight = ib.SizeY;
                int dataType = ib.ColorDept;

                //MIL.MbufDiskInquire(file, MIL.M_SIZE_X, ref imgWidth);
                //MIL.MbufDiskInquire(file, MIL.M_SIZE_Y, ref imgHeight);
                //MIL.MbufDiskInquire(file, MIL.M_TYPE, ref dataType);

                //var colAttr = MIL.M_IMAGE + MIL.M_PROC + MIL.M_BGR24 + MIL.M_PACKED;

                MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, iAttributes, ref milBufProc);

                if (saveDump) //create save buffer
                    //MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY, 8 + MIL.M_UNSIGNED, colAttr, ref milBufResult);
                    MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref milBufResult);

                //MIL.MbufPutColor(milBufResult, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, ib.Buffer);

                //MIL.MimConvert(milBufResult, milBufProc, MIL.M_RGB_TO_L);

                // Allocate a Edge Finder context.
                MIL.MedgeAlloc(MilSystem, MIL.M_CONTOUR, MIL.M_DEFAULT, ref MilEdgeContext);


                MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY, ib.ColorDept, iAttributes, ref milBufMask);

                var FirstROI = slotparam.ROI;
                MIL.MbufAlloc2d(MilSystem, (int)FirstROI.Width, (int)FirstROI.Height, ib.ColorDept, iAttributes, ref milBufRoi);

                // Allocate a result buffer.
                MIL.MedgeAllocResult(MilSystem, MIL.M_DEFAULT, ref MilEdgeResult);
                #region # MILControl

                // Enable features to compute.
                MIL.MedgeControl(MilEdgeContext, MIL.M_MOMENT_ELONGATION, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_FERET_MEAN_DIAMETER + MIL.M_SORT1_DOWN, MIL.M_ENABLE);
                //MIL.MedgeControl(MilEdgeContext, MIL.M_FAST_LENGTH, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_MOMENT_ELONGATION_ANGLE, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_FILTER_SMOOTHNESS, 70.0);
                MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_TYPE, MIL.M_HYSTERESIS);
                //MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_MODE, MIL.M_VERY_HIGH);
                MIL.MedgeControl(MilEdgeContext, MIL.M_THRESHOLD_MODE, MIL.M_HIGH);
                MIL.MedgeControl(MilEdgeContext, MIL.M_LENGTH, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_TORTUOSITY, MIL.M_ENABLE);

                //MIL.MedgeControl(MilEdgeContext, MIL.M_LINE_FIT_ERROR, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_BOX_X_MIN, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_BOX_X_MAX, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_BOX_Y_MIN, MIL.M_ENABLE);
                MIL.MedgeControl(MilEdgeContext, MIL.M_BOX_Y_MAX, MIL.M_ENABLE);

                //MIL.MedgeControl(MilEdgeContext, MIL.M_SEARCH_ANGLE, MIL.M_ENABLE);
                //MIL.MedgeControl(MilEdgeContext, MIL.M_SEARCH_ANGLE_SIGN, MIL.M_SAME_OR_REVERSE);
                //MIL.MedgeControl(MilEdgeContext, MIL.M_SEARCH_ANGLE_TOLERANCE, 10);
                #endregion

                // Select Edges
                double edgeWidthTol = ib.SizeX * 0.1; //10%
                edgeWidthTol = ib.SizeX * 0.2;

                var roi = slotparam.ROI;

                if (isColorBuffer)
                    ColorToBlackAndWrite(ib, ref milBufProc);
                else
                    MIL.MbufPut(milBufProc, ib.Buffer);


                if (performConvoleProcessing)
                {
                    MIL.MimConvolve(milBufProc, milBufProc, (int)convolveType);
                }

                // Create Mask Buf
                MIL.MbufClear(milBufMask, 255);

                MIL.MbufClear(milBufRoi, 0);

                int offsetX = ib.SizeX;
                MIL.MbufCopyClip(milBufRoi, milBufMask, (int)roi.X, (int)roi.Y);

                // Apply mask
                MIL.MedgeMask(MilEdgeContext, milBufMask, MIL.M_DEFAULT);

                // Calculate edges and features.
                MIL.MedgeCalculate(MilEdgeContext, milBufProc, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL, MilEdgeResult, MIL.M_DEFAULT);

                #region # MedgeSelects
                MIL.MedgeSelect(MilEdgeResult, MIL.M_EXCLUDE, MIL.M_MOMENT_ELONGATION, MIL.M_GREATER, 0.01, MIL.M_NULL);
                MIL.MedgeSelect(MilEdgeResult, MIL.M_EXCLUDE, MIL.M_MOMENT_ELONGATION_ANGLE, MIL.M_GREATER, 10, MIL.M_NULL);
                MIL.MedgeSelect(MilEdgeResult, MIL.M_EXCLUDE, MIL.M_LENGTH, MIL.M_LESS, edgeWidthTol, MIL.M_NULL);
                MIL.MedgeSelect(MilEdgeResult, MIL.M_EXCLUDE, MIL.M_TORTUOSITY, MIL.M_LESS, 0.95, MIL.M_NULL);
                #endregion

                // Get the number of edges found.
                MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_NUMBER_OF_CHAINS + MIL.M_TYPE_MIL_INT, ref NumEdgeFound);

                //MIL.MedgeSelect(MilEdgeResult, MIL.M_INCLUDE, MIL.M_ALL_EDGES, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);

                Rect unionRect = Rect.Empty;
                if (NumEdgeFound > 0)
                {
                    double[] boxMinX = new double[NumEdgeFound];
                    double[] boxMaxX = new double[NumEdgeFound];
                    double[] boxMinY = new double[NumEdgeFound];
                    double[] boxMaxY = new double[NumEdgeFound];

                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_BOX_X_MIN, ref boxMinX[0], MIL.M_NULL);
                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_BOX_X_MAX, ref boxMaxX[0], MIL.M_NULL);
                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MIN, ref boxMinY[0], MIL.M_NULL);
                    MIL.MedgeGetResult(MilEdgeResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MAX, ref boxMaxY[0], MIL.M_NULL);

                    for (int i = 0; i < NumEdgeFound; i++)
                    {
                        var minPt = new Point(boxMinX[i], boxMinY[i]);
                        var maxPt = new Point(boxMaxX[i], boxMaxY[i]);

                        Rect edgeRect = new Rect(minPt, maxPt);

                        if (unionRect == Rect.Empty)
                            unionRect = edgeRect;
                        else
                            unionRect.Union(edgeRect);
                    }
                }

                // Result data
                CassetteScanSlotResult rel = new CassetteScanSlotResult();
                rel.SlotLabel = slotparam.SlotLabel;
                rel.HasEdges = NumEdgeFound > 0;
                rel.ROE = unionRect;

                if (saveDump)
                {
                    MIL.MbufCopy(milBufProc, milBufResult);

                    // Draw edges in the source image to show the result.
                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                    MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milBufResult, MIL.M_DRAW_EDGES, MIL.M_DEFAULT, MIL.M_DEFAULT);

                    // Draw the index of each edge.
                    //MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    //MIL.MedgeDraw(MIL.M_DEFAULT, MilEdgeResult, milProcResultBuffer, MIL.M_DRAW_INDEX, MIL.M_DEFAULT, MIL.M_DEFAULT);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_YELLOW);
                    MIL.MgraRect(MIL.M_DEFAULT, milBufResult, roi.X, roi.Y, roi.X + roi.Width, roi.Y + roi.Height);

                    if (rel.HasEdges)
                    {
                        MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                        MIL.MgraRect(MIL.M_DEFAULT, milBufResult, rel.ROE.X, rel.ROE.Y, rel.ROE.X + rel.ROE.Width, rel.ROE.Y + rel.ROE.Height);

                        if (rel.ROE.Height < 1)
                        {

                        }

                        string hightPixelStr = string.Format("{0:f1}", rel.ROE.Height);
                        MIL.MgraText(MIL.M_DEFAULT, milBufResult, rel.ROE.X, rel.ROE.Y - 15, hightPixelStr);
                    }
                }

                return rel;
            }
            catch (MILException err)
            {
                throw new VisionException("CassetteScanProcessing Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }

            finally
            {
                if (milBufProc != MIL.M_NULL)
                    MIL.MbufFree(milBufProc);
                if (milBufResult != MIL.M_NULL)
                    MIL.MbufFree(milBufResult);
                if (milBufMask != MIL.M_NULL)
                    MIL.MbufFree(milBufMask);
                if (milBufRoi != MIL.M_NULL)
                    MIL.MbufFree(milBufRoi);

                if (MilEdgeContext != MIL.M_NULL)
                    MIL.MedgeFree(MilEdgeContext);
                if (MilEdgeResult != MIL.M_NULL)
                    MIL.MedgeFree(MilEdgeResult);
            }
        }

        #region OCR

        private EventCodeEnum GetOCRResult(MIL_ID ImageBuf, MIL_ID ResultOcrIdPtr, int OcrReadLength, int image_sx, int image_sy, ref ReadOCRResult ocrresult)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StringBuilder ReadString = new StringBuilder(OcrReadLength + 1); // Characters to read.
                double Score = 0;

                //char[] ReadChar = new char[OcrReadLength];       /* Array of characters to read.   */
                //double[] CharScore = new double[OcrReadLength];  /* Reading char score.            */
                //double[] PosX = new double[OcrReadLength];       /* Array of character X-positions.*/
                //double[] PosY = new double[OcrReadLength];       /* Array of character Y-positions.*/
                //double[] SizeX = new double[OcrReadLength];      /* Array of character X-size.     */
                //double[] SizeY = new double[OcrReadLength];      /* Array of character Y-size.     */

                if (ocrresult.OCRCharInfo == null)
                {
                    ocrresult.OCRCharInfo = new OCRCharacterInfo(OcrReadLength);
                }

                // Get the string and its reading score.
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_STRING, ReadString);
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_STRING_SCORE, ref Score);

                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_POSITION_X, ocrresult.OCRCharInfo.CharPositionX);
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_POSITION_Y, ocrresult.OCRCharInfo.CharPositionY);
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_SCORE + MIL.M_TYPE_MIL_DOUBLE, ocrresult.OCRCharInfo.CharScore);
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_SIZE_X + MIL.M_TYPE_MIL_DOUBLE, ocrresult.OCRCharInfo.CharSizeX);
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_SIZE_Y + MIL.M_TYPE_MIL_DOUBLE, ocrresult.OCRCharInfo.CharSizeY);
                MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_SPACING + MIL.M_TYPE_MIL_DOUBLE, ocrresult.OCRCharInfo.CharSpacing);
                //MIL.MocrGetResult(ResultOcrIdPtr, MIL.M_CHAR_VALID_FLAG + MIL.M_TYPE_MIL_DOUBLE, ref ocrresult.OCRCharInfo.CharValidFlag[0]);

                // result data
                ocrresult.OCRResultStr = ReadString.ToString();
                ocrresult.OCRResultScore = Score;

                //ocrresult.OCRResultScoreByChar = new double[OcrReadLength];

                //for (int i = 0; i < OcrReadLength; i++)
                //{
                //    ocrresult.OCRResultScoreByChar[i] = CharScore[i];
                //}

                ocrresult.OCRResultBuf = new ImageBuffer();
                ocrresult.OCRResultBuf.SizeX = image_sx;
                ocrresult.OCRResultBuf.SizeY = image_sy;
                ocrresult.OCRResultBuf.Buffer = new byte[image_sx * image_sy];

                MIL.MbufPut(ImageBuf, ocrresult.OCRResultBuf.Buffer);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;

                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ReadOCRResult ReadOCRProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_get_path, bool saveDump = false)
        {
            ReadOCRResult ocrResult = new ReadOCRResult();

            MIL_ID MilImage = MIL.M_NULL;                     // Image buffer identifier.
            MIL_ID MilSubImage = MIL.M_NULL;                  // Sub-image buffer identifier.
            MIL_ID OcrFont = MIL.M_NULL;                      // OCR font identifier.
            MIL_ID OcrResult = MIL.M_NULL;                    // OCR result buffer identifier.

            int READ_REGION_POS_X = Convert.ToInt32(ocrParams.OcrReadRegionPosX);
            int READ_REGION_POS_Y = Convert.ToInt32(ocrParams.OcrReadRegionPosY);
            int READ_REGION_WIDTH = Convert.ToInt32(ocrParams.OcrReadRegionWidth);
            int READ_REGION_HEIGHT = Convert.ToInt32(ocrParams.OcrReadRegionHeight);

            string FONT_FILE_IN = this.FileManager().GetSystemParamFullPath("Loader", "OCR" + "SemiCalibrated.mfo");  //Prober.LotOP.LotDeviceParam.DeviceName
            //string FONT_FILE_IN = @"C:\ProberSystem\Parameters\Loader\OCR" + "SemiCalibrated.mfo";  //Prober.LotOP.LotDeviceParam.DeviceName
            string FONT_FILE_SEMI = MIL.M_CONTEXT_PATH + "SEMI.mfo";

            StringBuilder ReadString = new StringBuilder(ocrParams.OcrMaxStringLength + 1); // Characters to read.
            //double Score = 0;                                 // Reading score.

            try
            {
                // Allocate an OCR result buffer.
                MIL.MocrAllocResult(MilSystem, MIL.M_DEFAULT, ref OcrResult);

                // Load and display the source image into a new image buffer.
                MIL.MbufAlloc2d(MilSystem, ocrImage.SizeX, ocrImage.SizeY, ocrImage.ColorDept, lAttributes, ref MilImage);

                MIL.MbufClear(MilImage, 0);

                MIL.MbufPut(MilImage, ocrImage.Buffer);

                // Restrict the region of the image where to read the string.
                MIL.MbufChild2d(MilImage, READ_REGION_POS_X, READ_REGION_POS_Y, READ_REGION_WIDTH, READ_REGION_HEIGHT, ref MilSubImage);

                // Restore the OCR character font from disk.
                if (File.Exists(FONT_FILE_IN) == false)
                {
                    //if (OcrCalibrateFontProcessing(ocrImage, ocrParams, saveDump) == true)
                    //{
                    //    MIL.MocrRestoreFont(FONT_FILE_IN, MIL.M_RESTORE, MilSystem, ref OcrFont);
                    //}
                    //else
                    //{
                    //    throw new Exception("OCR Calibrate Font Error");
                    //    // forced pass 
                    //    //ocrResult.OCRResultStr = "TESTLOT-01-C1";
                    //    //ocrResult.OCRResultScore = 99;                        
                    //}
                }
                else
                {
                    MIL.MocrRestoreFont(FONT_FILE_IN, MIL.M_RESTORE, MilSystem, ref OcrFont);
                }

                //temp
                MIL.MocrRestoreFont(FONT_FILE_SEMI, MIL.M_RESTORE, MilSystem, ref OcrFont);

                // Set the user-specific character constraints for each string position.
                for (int i = 0; i < ocrParams.OcrMaxStringLength; i++)
                {
                    if (ocrParams.OcrConstraint[i] == 'L')    // Alpha
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_LETTER);
                    }
                    else if (ocrParams.OcrConstraint[i] == 'D')   // Num
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_DIGIT);
                    }
                    else if (ocrParams.OcrConstraint[i] == 'F')   // Fix
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY);
                    }
                    else if (ocrParams.OcrConstraint[i] == 'X')   // Letter & Digit
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY);
                    }
                    else if (ocrParams.OcrConstraint[i] == 'C')   // CheckSum
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, "ABCDEFGH01234567");
                    }
                    else if (ocrParams.OcrConstraint[i] == 'Z')   // Letter & Digit & - & .
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY);
                    }
                    else if (ocrParams.OcrConstraint[i] == '-')
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, "-");
                    }
                    else if (ocrParams.OcrConstraint[i] == '.')
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, ".");
                    }
                }

                // Set OCR Font controls
                //if (font color is black)
                //{
                //    MIL.MocrModifyFont(OcrFont, MIL.M_INVERT, MIL.M_DEFAULT);    // font color
                //}
                MIL.MocrControl(OcrFont, MIL.M_SPEED, MIL.M_VERY_LOW);
                //MIL.MocrControl(OcrFont, MIL.M_CONTEXT_CONVERT, MIL.M_GENERAL);
                MIL.MocrControl(OcrFont, MIL.M_STRING_NUMBER, MIL.M_ALL);
                MIL.MocrControl(OcrFont, MIL.M_THRESHOLD, MIL.M_AUTO);
                MIL.MocrControl(OcrFont, MIL.M_STRING_SIZE, ocrParams.OcrMaxStringLength);                              // String length
                MIL.MocrControl(OcrFont, MIL.M_TARGET_CHAR_SIZE_X, ocrParams.OcrCharSizeX);                             // Size X
                MIL.MocrControl(OcrFont, MIL.M_TARGET_CHAR_SIZE_Y, ocrParams.OcrCharSizeY);                             // Size Y
                MIL.MocrControl(OcrFont, MIL.M_TARGET_CHAR_SPACING, ocrParams.OcrCharSizeX + ocrParams.OcrCharSpacing); // Size Spacing
                MIL.MocrControl(OcrFont, MIL.M_STRING_ACCEPTANCE, ocrParams.OcrStrAcceptance);                          // Str Acceptance
                MIL.MocrControl(OcrFont, MIL.M_CHAR_ACCEPTANCE, ocrParams.OcrCharAcceptance);                           // Char Acceptance

                // Read the string.
                MIL.MocrReadString(MilSubImage, OcrFont, OcrResult);

                GetOCRResult(MilSubImage, OcrResult, ocrParams.OcrMaxStringLength, ocrImage.SizeX, ocrImage.SizeY, ref ocrResult);

                //// Get the string and its reading score.
                //MIL.MocrGetResult(OcrResult, MIL.M_STRING, ReadString);
                //MIL.MocrGetResult(OcrResult, MIL.M_STRING_SCORE, ref Score);
                //MIL.MocrGetResult(OcrResult, MIL.M_CHAR_SCORE, CharScore);
                //MIL.MocrGetResult(OcrResult, MIL.M_CHAR_POSITION_X, PosX);
                //MIL.MocrGetResult(OcrResult, MIL.M_CHAR_POSITION_Y, PosY);
                //MIL.MocrGetResult(OcrResult, MIL.M_CHAR_SIZE_X, SizeX);
                //MIL.MocrGetResult(OcrResult, MIL.M_CHAR_SIZE_Y, SizeY);

                //// result data
                //ocrResult.OCRResultStr = ReadString.ToString();
                //ocrResult.OCRResultScore = Score;

                //ocrResult.OCRResultScoreByChar = new double[ocrParams.OcrMaxStringLength];
                //for (int i = 0; i < ocrParams.OcrMaxStringLength; i++)
                //{
                //    ocrResult.OCRResultScoreByChar[i] = CharScore[i];
                //}

                //ocrResult.OCRResultBuf = new ImageBuffer();
                //ocrResult.OCRResultBuf.SizeX = ocrImage.SizeX;
                //ocrResult.OCRResultBuf.SizeY = ocrImage.SizeY;
                //ocrResult.OCRResultBuf.Buffer = new byte[ocrImage.SizeX * ocrImage.SizeY];
                //MIL.MbufPut(MilSubImage, ocrResult.OCRResultBuf.Buffer);

                return ocrResult;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw err;
            }
            finally
            {
                // Free all allocations.
                if (OcrFont != MIL.M_NULL)
                {
                    MIL.MocrFree(OcrFont); OcrFont = MIL.M_NULL;
                }
                if (OcrResult != MIL.M_NULL)
                {
                    MIL.MocrFree(OcrResult); OcrResult = MIL.M_NULL;
                }
                if (MilSubImage != MIL.M_NULL)
                {
                    MIL.MbufFree(MilSubImage); MilSubImage = MIL.M_NULL;
                }
                if (MilImage != MIL.M_NULL)
                {
                    MIL.MbufFree(MilImage); MilImage = MIL.M_NULL;
                }
            }
        }

        public ReadOCRResult OcrCalibrateFontProcessing(ImageBuffer ocrImage, ReadOCRProcessingParam ocrParams, string font_output_path, bool saveDump = false)
        {
            ReadOCRResult ocrResult = new ReadOCRResult();

            // MIL_ID MilApplication = MIL.M_NULL;               // Application identifier.
            MIL_ID MilImage = MIL.M_NULL;                     // Image buffer identifier.
            MIL_ID MilFontImage = MIL.M_NULL;                  // Sub-image buffer identifier.
            MIL_ID OcrFont = MIL.M_NULL;                      // OCR font identifier.
            MIL_ID OcrResult = MIL.M_NULL;                    // OCR result buffer identifier.

            MIL_ID OCRResultImage = MIL.M_NULL;

            //double CHAR_SIZE_X_MIN = ocrParams.OcrCharSizeX - 2;
            //double CHAR_SIZE_X_MAX = ocrParams.OcrCharSizeX + 2;
            double CHAR_SIZE_X_MIN = ocrParams.OcrCalibrateMinX;
            double CHAR_SIZE_X_MAX = ocrParams.OcrCalibrateMaxX;
            double CHAR_SIZE_X_STEP = ocrParams.OcrCalibrateStepX;
            //double CHAR_SIZE_Y_MIN = ocrParams.OcrCharSizeY - 2;
            //double CHAR_SIZE_Y_MAX = ocrParams.OcrCharSizeY + 2;
            double CHAR_SIZE_Y_MIN = ocrParams.OcrCalibrateMinY;
            double CHAR_SIZE_Y_MAX = ocrParams.OcrCalibrateMaxY;
            double CHAR_SIZE_Y_STEP = ocrParams.OcrCalibrateStepY;
            int STRING_LENGTH = ocrParams.OcrMaxStringLength;

            int READ_REGION_POS_X = Convert.ToInt32(ocrParams.OcrReadRegionPosX);
            int READ_REGION_POS_Y = Convert.ToInt32(ocrParams.OcrReadRegionPosY);
            int READ_REGION_WIDTH = Convert.ToInt32(ocrParams.OcrReadRegionWidth);
            int READ_REGION_HEIGHT = Convert.ToInt32(ocrParams.OcrReadRegionHeight);

            string STRING_CALIBRATION = ocrParams.OcrSampleString;
            string STRING_CONSTRAINT = ocrParams.OcrConstraint;

            double READ_SCORE_MIN = ocrParams.OcrStrAcceptance;

            //StringBuilder ReadString = new StringBuilder(STRING_LENGTH); // Characters to read.
            //double Score = 0;                                            // Reading score.

            //string FONT_FILE_IN = @"C:\ProberSystem\Parameters\Loader\OCR" + "SemiCalibrated.mfo";
            string FONT_FILE_IN = MIL.M_CONTEXT_PATH + "SEMI_M12-92.mfo";

            try
            {
                // Allocate an OCR result buffer.
                MIL.MocrAllocResult(MilSystem, MIL.M_DEFAULT, ref OcrResult);

                // Load and display the source image into a new image buffer.
                MIL.MbufAlloc2d(MilSystem, ocrImage.SizeX, ocrImage.SizeY, ocrImage.ColorDept, lAttributes, ref MilImage);

                MIL.MbufClear(MilImage, 0);

                MIL.MbufPut(MilImage, ocrImage.Buffer);

                // Crop and copy image
                MIL.MbufChild2d(MilImage, READ_REGION_POS_X, READ_REGION_POS_Y, READ_REGION_WIDTH, READ_REGION_HEIGHT, ref MilFontImage);

                MIL.MbufAllocColor(MilSystem, 3, READ_REGION_WIDTH, READ_REGION_HEIGHT, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref OCRResultImage);
                MIL.MbufCopy(MilFontImage, OCRResultImage);

                MIL.MocrRestoreFont(FONT_FILE_IN, MIL.M_RESTORE, MilSystem, ref OcrFont);

                MIL.MocrControl(OcrFont, MIL.M_SPEED, MIL.M_VERY_LOW);

                for (int i = 0; i < STRING_LENGTH; i++)
                {
                    if (STRING_CONSTRAINT[i] == 'L')    // Alpha
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_LETTER);
                    }
                    else if ((STRING_CONSTRAINT[i] == 'D') || (STRING_CONSTRAINT[i] == 'S'))  // Num
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_DIGIT);
                    }
                    else if (STRING_CONSTRAINT[i] == 'F')   // Fix
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY);
                    }
                    else if (STRING_CONSTRAINT[i] == 'X')   // Letter & Digit
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
                    }
                    else if (STRING_CONSTRAINT[i] == 'C')   // CheckSum
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, "ABCDEFGH01234567");
                    }
                    else if (STRING_CONSTRAINT[i] == 'Z')   // Letter & Digit & - & .
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY);
                    }
                    else if (STRING_CONSTRAINT[i] == '-')
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, "-");
                    }
                    else if (STRING_CONSTRAINT[i] == '.')
                    {
                        MIL.MocrSetConstraint(OcrFont, i, MIL.M_ANY, ".");
                    }
                }

                // Calibrate the OCR font.
                MIL.MocrCalibrateFont(MilFontImage, OcrFont, STRING_CALIBRATION,
                                  CHAR_SIZE_X_MIN, CHAR_SIZE_X_MAX, CHAR_SIZE_X_STEP,
                                  CHAR_SIZE_Y_MIN, CHAR_SIZE_Y_MAX, CHAR_SIZE_Y_STEP,
                                  MIL.M_DEFAULT);

                //double char_size_x = 0;
                //double Target_char_size_x = 0;

                //double char_size_y = 0;
                //double Target_char_size_y = 0;

                //double char_acceptance = 0;

                //int morphologic_filtering = 0;

                //MIL.MocrInquire(OcrFont, MIL.M_CHAR_SIZE_X + MIL.M_TYPE_MIL_DOUBLE, ref char_size_x);
                //MIL.MocrInquire(OcrFont, MIL.M_TARGET_CHAR_SIZE_X + MIL.M_TYPE_MIL_DOUBLE, ref Target_char_size_x);

                //MIL.MocrInquire(OcrFont, MIL.M_CHAR_SIZE_Y + MIL.M_TYPE_MIL_DOUBLE, ref char_size_y);
                //MIL.MocrInquire(OcrFont, MIL.M_TARGET_CHAR_SIZE_Y + MIL.M_TYPE_MIL_DOUBLE, ref Target_char_size_y);

                //MIL.MocrInquire(OcrFont, MIL.M_CHAR_ACCEPTANCE + MIL.M_TYPE_MIL_DOUBLE, ref char_acceptance);

                //MIL.MocrInquire(OcrFont, MIL.M_MORPHOLOGIC_FILTERING, ref morphologic_filtering);

                // Read the string.
                MIL.MocrReadString(MilFontImage, OcrFont, OcrResult);

                //double Nb_string = 0;

                //int string_valid_flag = 0;

                GetOCRResult(MilImage, OcrResult, ocrParams.OcrMaxStringLength, ocrImage.SizeX, ocrImage.SizeY, ref ocrResult);

                // Save the calibrated font if the reading score was sufficiently high.
                if (ocrResult.OCRResultScore > READ_SCORE_MIN)
                {
                    // For Debug

                    // (0) Spacing
                    // (1) Size X
                    // (2) Size Y

                    double spacing = 0;
                    double sizex = 0;
                    double sizey = 0;

                    MIL.MocrInquire(OcrFont, MIL.M_TARGET_CHAR_SPACING + MIL.M_TYPE_MIL_DOUBLE, ref spacing);
                    MIL.MocrInquire(OcrFont, MIL.M_TARGET_CHAR_SIZE_X + MIL.M_TYPE_MIL_DOUBLE, ref sizex);
                    MIL.MocrInquire(OcrFont, MIL.M_TARGET_CHAR_SIZE_Y + MIL.M_TYPE_MIL_DOUBLE, ref sizey);

                    MIL.MocrSaveFont(font_output_path, MIL.M_SAVE, OcrFont);

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_LIGHT_BLUE);
                    MIL.MocrDraw(MIL.M_DEFAULT, OcrResult, OCRResultImage, MIL.M_DRAW_STRING_CHAR_BOX, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_DEFAULT);
                    MIL.MocrDraw(MIL.M_DEFAULT, OcrResult, OCRResultImage, MIL.M_DRAW_STRING_CHAR_POSITION, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_DEFAULT);

                    double xcen = 0;
                    double ycen = 0;
                    double offsety = 5;

                    StringBuilder ReadString = new StringBuilder(ocrResult.OCRResultStr);

                    for (int i = 0; i < ocrResult.OCRCharInfo.length; i++)
                    {
                        xcen = ocrResult.OCRCharInfo.CharPositionX[i];
                        ycen = ocrResult.OCRCharInfo.CharPositionY[i] + (ocrResult.OCRCharInfo.CharSizeY[i] / 2) + offsety;

                        MIL.MgraText(MIL.M_DEFAULT, OCRResultImage, xcen, ycen, ReadString[i].ToString());
                    }

                    LoggerManager.Debug($"Spacing = {spacing}, Size X = {sizex}, Size Y = {sizey}", isInfo: IsInfo);
                    LoggerManager.Debug($"Read successful, calibrated OCR font was saved to disk.");
                }
                else
                {
                    LoggerManager.Error($"Error: Read score too low, calibrated OCR font not saved.");
                }

                LoggerManager.Debug($"Score = { ocrResult.OCRResultScore}, Acceptance : {READ_SCORE_MIN}", isInfo: IsInfo);
            }
            catch (Exception err)
            {
                //int AllocError = 0;                                 // Allocation error variable.

                //MIL.MappGetError(MIL.M_DEFAULT, MIL.M_CURRENT, ref AllocError);

                LoggerManager.Exception(err);
            }
            finally
            {
                // Free all allocations.
                if (OcrFont != MIL.M_NULL)
                {
                    MIL.MocrFree(OcrFont);
                }
                if (OcrResult != MIL.M_NULL)
                {
                    MIL.MocrFree(OcrResult);
                }
                if (MilFontImage != MIL.M_NULL)
                {
                    MIL.MbufFree(MilFontImage);
                }
                if (MilImage != MIL.M_NULL)
                {
                    MIL.MbufFree(MilImage);
                }

                if (OCRResultImage != MIL.M_NULL)
                {
                    MIL.MbufFree(OCRResultImage);
                }
            }

            return ocrResult;
        }
        #endregion

        public EventCodeEnum BlockBinarize(MIL_ID sourceImage, ref MIL_ID targetImage, MIL_INT BlockSizeX, MIL_INT BlockSizeY, MIL_INT Threshold, MIL_INT ObjecrColor)
        {
            //이건 아직 고민중
            return EventCodeEnum.NONE;
        }
        public MIL_INT GetGrayValueAverage(MIL_ID sourceImage, MIL_INT ImageSizeX, MIL_INT ImageSizeY)
        {
            try
            {
                byte[,] ImgBuf = new byte[ImageSizeY, ImageSizeX];

                MIL_INT Sum = MIL.M_NULL, Avg = MIL.M_NULL;

                MIL.MbufGet(sourceImage, ImgBuf);

                for (int j = 0; j < ImageSizeY; j++)
                {
                    for (int i = 0; i < ImageSizeX; i++)
                    {
                        Sum += (MIL_INT)ImgBuf[j, i];
                    }
                }
                Avg = Sum / (ImageSizeY * ImageSizeX);

                return Avg;
            }
            catch (MILException err)
            {
                throw new VisionException("GetGrayValueAverage Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }

        }
        public MIL_INT GetGrayValueAverageWithoutPureBlack(MIL_ID sourceImage, MIL_INT ImageSizeX, MIL_INT ImageSizeY)
        {
            byte[,] ImgBuf = new byte[ImageSizeY, ImageSizeX];

            MIL_INT Sum = MIL.M_NULL, Avg = MIL.M_NULL, PixelCnt = 0;
            try
            {
                MIL.MbufGet(sourceImage, ImgBuf);

                for (int j = 0; j < ImageSizeY; j++)
                {
                    for (int i = 0; i < ImageSizeX; i++)
                    {
                        if (ImgBuf[j, i] > 0)
                        {
                            Sum += (MIL_INT)ImgBuf[j, i];
                            PixelCnt++;
                        }
                    }
                }
                Avg = Sum / PixelCnt;
            }
            catch (MILException err)
            {
                throw new VisionException("GetGrayValueAverageWithoutPureBlack Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            return Avg;
        }

        private void BlackAndWhiteToColor(ImageBuffer input, ref MIL_ID output, bool? overlap = false)
        {
            MIL_ID milImage = MIL.M_NULL;

            try
            {
                MIL.MbufAllocColor(MilSystem, 3, input.SizeX, input.SizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref output);

                if (overlap == true)
                {
                    if (input.Band != 1)
                    {
                        input.Band = 1;
                    }

                    MIL.MbufAlloc2d(MilSystem, input.SizeX, input.SizeY, input.Band * input.ColorDept, Attributes, ref milImage);
                    MIL.MbufPut2d(milImage, 0, 0, input.SizeX, input.SizeY, input.Buffer);

                    MIL.MimConvert(milImage, output, MIL.M_L_TO_RGB);
                }
                else
                {
                    MIL.MbufClear(output, MIL.M_COLOR_BLACK);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milImage != MIL.M_NULL)
                {
                    MIL.MbufFree(milImage);
                    milImage = MIL.M_NULL;
                }
            }
        }

        private void BlackAndWhiteToColor(MIL_ID input, ref MIL_ID output, bool? overlap = false)
        {
            MIL_ID milImage = MIL.M_NULL;

            try
            {
                MIL_INT BufSizeX = MIL.MbufInquire(input, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT BufSizeY = MIL.MbufInquire(input, MIL.M_SIZE_Y, MIL.M_NULL);
                MIL_INT BufBand = MIL.MbufInquire(input, MIL.M_SIZE_BAND, MIL.M_NULL);
                MIL_INT BufSizeBit = MIL.MbufInquire(input, MIL.M_SIZE_BIT, MIL.M_NULL);

                MIL.MbufAllocColor(MilSystem, 3, BufSizeX, BufSizeY, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref output);

                if (overlap == true)
                {
                    MIL.MbufAlloc2d(MilSystem, BufSizeX, BufSizeY, BufBand * BufSizeBit, Attributes, ref milImage);

                    byte[] InputArr = new byte[BufSizeX * BufSizeY];
                    MIL.MbufGet(input, InputArr);

                    MIL.MbufPut2d(milImage, 0, 0, BufSizeX, BufSizeY, InputArr);

                    MIL.MimConvert(milImage, output, MIL.M_L_TO_RGB);
                }
                else
                {
                    MIL.MbufClear(output, MIL.M_COLOR_BLACK);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (milImage != MIL.M_NULL)
                {
                    MIL.MbufFree(milImage);
                    milImage = MIL.M_NULL;
                }
            }
        }
        public BlobResult FindBlobWithRectangularity(ImageBuffer _grabedImage,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            int Threshold,
                            int BlobAreaLow,
                            int BlobAreaHigh,
                            int OffsetX,
                            int OffsetY,
                            int SizeX,
                            int SizeY,
                            double BlobSizeX,
                            double BlobSizeY,
                            bool isDilation = false,
                            double BlobSizeXMinimumMargin = 0.5,
                            double BlobSizeXMaximumMargin = 0.5,
                            double BlobSizeYMinimumMargin = 0.5,
                            double BlobSizeYMaximumMargin = 0.5,
                            double minRectangularity = 0.0)
        {
            MIL_ID FeatureList = 0, BlobResult = 0;

            MIL_INT TotalBlobs = 0;

            MIL_ID milImageBuffer = MIL.M_NULL;
            MIL_ID milOrgImgBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            MIL_ID milBinBuffer = MIL.M_NULL;
            //MIL_ID milTargetBuffer = MIL.M_NULL;

            MIL_INT pttSizeX = 0, pttSizeY = 0, pttType = 0;

            MIL_ID milDrawingOriginalBuffer = MIL.M_NULL;
            MIL_ID milDrawingChildBuffer = MIL.M_NULL;
            MIL_ID m_Context = MIL.M_NULL;

            bool rectFilterEnabled = false;

            ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();
            BlobResult retval = null;

            try
            {
                LoggerManager.Debug($"[ModuleVisionProcessing] FindBlobWithRectangularity() : Input Value : " +
                    $"ImgPosX = {ImgPosX:0.00}, " +
                    $"ImgPosY = {ImgPosY:0.00}" +
                    $"Threshold = {Threshold}" +
                    $"BlobAreaLow = {BlobAreaLow}" +
                    $"BlobAreaHigh = {BlobAreaHigh}" +
                    $"Offset X, Y = {OffsetX:0.00}, {OffsetY:0.00}" +
                    $"SizeX, SizeY = {SizeX:0.00}, {SizeY:0.00}" +
                    $"BlobSizeX, Y = {BlobSizeX:0.00}, {BlobSizeY:0.00}" +
                    $"isDilation = {isDilation}" +
                    $"BlobSizeXMinimumMargin = {BlobSizeXMinimumMargin}" +
                    $"BlobSizeXMaximumMargin = {BlobSizeXMaximumMargin}" +
                    $"BlobSizeYMinimumMargin = {BlobSizeYMinimumMargin}" +
                    $"BlobSizeYMaximumMargin = {BlobSizeYMaximumMargin}" +
                    $"minRectangularity = {minRectangularity}", isInfo: IsInfo);

                if ((SizeX <= 0) || (SizeY <= 0))
                {
                    return retval;
                }

                if (_grabedImage != null)
                {
                    MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milImageBuffer);
                    MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milOrgImgBuffer);
                    MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milBinBuffer);
                    MIL.MbufClear(milImageBuffer, 0); //MilProcBuffer 버퍼를 0색상으로 지워준다.
                    MIL.MbufClear(milBinBuffer, 0); //MilProcBuffer 버퍼를 0색상으로 지워준다.
                    MIL.MbufPut(milImageBuffer, _grabedImage.Buffer);
                    MIL.MbufPut(milOrgImgBuffer, _grabedImage.Buffer);

                }
                if (Threshold > 0)
                {
                    MIL.MbufChild2d(milImageBuffer, OffsetX, OffsetY, SizeX, SizeY, ref milProcBuffer);
                    MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_GREATER_OR_EQUAL, Threshold, MIL.M_NULL);
                    MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                    MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                }
                else//0으로 들어왔을 때에는 Otsu로 구해진 Threshold가 아닌 Adaptive Threshold를 수행
                {
                    MIL.MimAlloc(MilSystem, MIL.M_BINARIZE_ADAPTIVE_CONTEXT, MIL.M_DEFAULT, ref m_Context);
                    MIL.MimControl(m_Context, MIL.M_THRESHOLD_MODE, MIL.M_NIBLACK);
                    MIL.MimControl(m_Context, MIL.M_FOREGROUND_VALUE, MIL.M_FOREGROUND_WHITE);
                    MIL.MimControl(m_Context, MIL.M_MINIMUM_CONTRAST, 1);
                    MIL.MimControl(m_Context, MIL.M_NIBLACK_BIAS, 0.2);
                    MIL.MimControl(m_Context, MIL.M_AVERAGE_MODE, MIL.M_GAUSSIAN);
                    MIL.MimBinarizeAdaptive(m_Context, milImageBuffer, MIL.M_NULL, MIL.M_NULL, milBinBuffer, MIL.M_NULL, MIL.M_DEFAULT);
                    MIL.MimClose(milBinBuffer, milBinBuffer, 1, MIL.M_BINARY);
                    MIL.MbufChild2d(milBinBuffer, OffsetX, OffsetY, SizeX, SizeY, ref milProcBuffer);
                }


                MIL.MblobAllocFeatureList(MilSystem, ref FeatureList);
                MIL.MblobSelectFeature(FeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_AREA);
                MIL.MblobSelectFeature(FeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(FeatureList, MIL.M_ELONGATION);

                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MIN);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MAX);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MAX);

                if (minRectangularity != 0)
                {
                    MIL.MblobSelectFeature(FeatureList, MIL.M_RECTANGULARITY);
                    rectFilterEnabled = true;
                }
                MIL.MblobReconstruct(milProcBuffer, MIL.M_NULL, milProcBuffer, MIL.M_FILL_HOLES, MIL.M_DEFAULT);
                MIL.MblobAllocResult(MilSystem, ref BlobResult);
                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, FeatureList, BlobResult);
                MIL.MblobSelect(BlobResult, MIL.M_INCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                MIL.MblobGetNumber(BlobResult, ref TotalBlobs);

                LoggerManager.Debug($"[ModuleVisionProcessing] FindBlobWithCompactness() : Before select, Total blobs : {TotalBlobs}", isInfo: IsInfo);

                double arealow = BlobAreaLow;
                double areahigh = BlobAreaHigh;

                if (areahigh < arealow)
                {
                    LoggerManager.Debug($"[ModuleVisionProcessing] FindBlobWithCompactness() : BlobAreaLow : {BlobAreaLow} > BlobAreaHigh : {BlobAreaHigh}", isInfo: IsInfo);

                    areahigh = arealow;
                }

                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS_OR_EQUAL, 0, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER_OR_EQUAL, SizeX - 1, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS_OR_EQUAL, 0, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER_OR_EQUAL, SizeY - 1, MIL.M_NULL);

                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_FERET_X, MIL.M_LESS_OR_EQUAL, BlobSizeX * BlobSizeXMinimumMargin, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_FERET_Y, MIL.M_LESS_OR_EQUAL, BlobSizeY * BlobSizeYMinimumMargin, MIL.M_NULL);

                MIL.MblobGetNumber(BlobResult, ref TotalBlobs);
                LoggerManager.Debug($"[ModuleVisionProcessing] FindBlobWithCompactness() : After select [POSITION], Total blobs : {TotalBlobs}", isInfo: IsInfo);
                double[] dblSelectBlobArea = new double[TotalBlobs];
                MIL.MblobGetResult(BlobResult, MIL.M_AREA, dblSelectBlobArea);
                if (rectFilterEnabled == true)
                {
                    LoggerManager.Debug($"[ModuleVisionProcessing] FindBlobWithCompactness() : Min. Rectangularity = {minRectangularity}", isInfo: IsInfo);

                    if (minRectangularity != 0)
                    {
                        MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_RECTANGULARITY, MIL.M_LESS_OR_EQUAL, minRectangularity, MIL.M_NULL);
                    }

                    MIL.MblobGetNumber(BlobResult, ref TotalBlobs);
                    LoggerManager.Debug($"[ModuleVisionProcessing] FindBlobWithCompactness() : Rectangularity filter applied, Total blobs : {TotalBlobs}", isInfo: IsInfo);

                }

                bool IsSuccess = false;

                double AreaInnerMarginRatio = 0.1;

                bool NotUseSizeXFilter = false;
                bool NotUseSizeYFilter = false;

                double AreaLowAddMarginValue = arealow * (1.0 - AreaInnerMarginRatio);
                double AreaHighAddMarginValue = areahigh * (1.0 + AreaInnerMarginRatio);

                double SizeXLowAddMarginValue = 0;
                double SizeXHighAddMarginValue = 0;

                double SizeYLowAddMarginValue = 0;
                double SizeYHighAddMarginValue = 0;

                if (BlobSizeX == 0)
                {
                    NotUseSizeXFilter = true;
                }
                else
                {
                    SizeXLowAddMarginValue = BlobSizeX * (1.0 - BlobSizeXMinimumMargin);
                    SizeXHighAddMarginValue = BlobSizeX * (1.0 + BlobSizeXMaximumMargin);
                }

                if (BlobSizeY == 0)
                {
                    NotUseSizeYFilter = true;
                }
                else
                {
                    SizeYLowAddMarginValue = BlobSizeY * (1.0 - BlobSizeYMinimumMargin);
                    SizeYHighAddMarginValue = BlobSizeY * (1.0 + BlobSizeYMaximumMargin);
                }

                LoggerManager.Debug($"Mark Tolerance Information", isInfo: IsInfo);

                LoggerManager.Debug($"{AreaLowAddMarginValue:0.00} <= AREA <= {AreaHighAddMarginValue:0.00}", isInfo: IsInfo);

                LoggerManager.Debug($"{0} <= Position X <= {SizeX - 1}", isInfo: IsInfo);
                LoggerManager.Debug($"{0} <= Position Y <= {SizeX - 1}", isInfo: IsInfo);

                if (NotUseSizeXFilter == false)
                {
                    LoggerManager.Debug($"{SizeXLowAddMarginValue:0.00} <= Size X <= {SizeXHighAddMarginValue:0.00}, Margin (%) : MIN({BlobSizeXMinimumMargin:0.00}), MAX({BlobSizeXMaximumMargin:0.00})", isInfo: IsInfo);
                }
                else
                {
                    LoggerManager.Debug($"Size X Filter not used.", isInfo: IsInfo);
                }

                if (NotUseSizeYFilter == false)
                {
                    LoggerManager.Debug($"{SizeYLowAddMarginValue:0.00} <= Size Y <= {SizeYHighAddMarginValue:0.00}, Margin (%) : MIN({BlobSizeYMinimumMargin:0.00}), MAX({BlobSizeYMaximumMargin:0.00})", isInfo: IsInfo);
                }
                else
                {
                    LoggerManager.Debug($"Size Y Filter not used.", isInfo: true);
                }

                if (TotalBlobs > 0)
                {
                    double[] dblBlobArea = new double[TotalBlobs];

                    double[] dblSizeX = new double[TotalBlobs];
                    double[] dblSizeY = new double[TotalBlobs];

                    double[] dblBox_X_Max = new double[TotalBlobs];
                    double[] dblBox_X_Min = new double[TotalBlobs];
                    double[] dblBox_Y_Max = new double[TotalBlobs];
                    double[] dblBox_Y_Min = new double[TotalBlobs];

                    double[] dblGravity_X = new double[TotalBlobs];
                    double[] dblGravity_Y = new double[TotalBlobs];

                    double[] dblBlobIndex = new double[TotalBlobs];
                    double[] dblRectangularity = new double[TotalBlobs];

                    bool[] IsFound = new bool[TotalBlobs];

                    MIL.MblobGetResult(BlobResult, MIL.M_AREA, dblBlobArea);
                    //MIL.MblobGetResult(BlobResult, MIL.M_BOX_AREA, dblBlobArea);

                    MIL.MblobGetResult(BlobResult, MIL.M_FERET_X, dblSizeX);
                    MIL.MblobGetResult(BlobResult, MIL.M_FERET_Y, dblSizeY);

                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                    MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblGravity_X);
                    MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblGravity_Y);

                    MIL.MblobGetResult(BlobResult, MIL.M_RECTANGULARITY, dblRectangularity);

                    MIL.MblobGetResult(BlobResult, MIL.M_LABEL_VALUE, dblBlobIndex);

                    GrabDevPosition mChipPosition;

                    byte[] binaryTargetBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY];
                    MIL.MbufGet(milOrgImgBuffer, binaryTargetBuffer);

                    for (int index = 0; index < TotalBlobs; index++)
                    {
                        LoggerManager.Debug($"Mark Index = {index + 1} : SIZE X, Y = ({dblSizeX[index]:0.00}, {dblSizeY[index]:0.00}), AREA = {dblBlobArea[index]:0.00}, Rectangularity = {dblRectangularity[index]:0.00}", isInfo: true);

                        if ((dblBlobArea[index] >= AreaLowAddMarginValue) &&
                             (dblBlobArea[index] <= AreaHighAddMarginValue)
                             )
                        {
                            bool SizeXFilterPassed = false;
                            bool SizeYFilterPassed = false;

                            if (NotUseSizeXFilter == false)
                            {
                                if ((dblSizeX[index] >= SizeXLowAddMarginValue) &&
                                    (dblSizeX[index] <= SizeXHighAddMarginValue))
                                {
                                    SizeXFilterPassed = true;
                                }
                            }
                            else
                            {
                                SizeXFilterPassed = true;
                            }

                            if (NotUseSizeXFilter == false)
                            {
                                if ((dblSizeY[index] >= SizeYLowAddMarginValue) &&
                                    (dblSizeY[index] <= SizeYHighAddMarginValue))
                                {
                                    SizeYFilterPassed = true;
                                }
                            }
                            else
                            {
                                SizeYFilterPassed = true;
                            }

                            if (SizeXFilterPassed == true && SizeYFilterPassed == true)
                            {
                                IsFound[index] = true;

                                mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);

                                mChipPosition.Area = dblBlobIndex[index];
                                mChipPosition.DevIndex = index;
                                mChipPosition.PosX = Math.Round(dblGravity_X[index], 3);
                                mChipPosition.PosY = Math.Round(dblGravity_Y[index], 3);

                                mChipPosition.SizeX = dblBox_X_Max[index] - dblBox_X_Min[index];
                                mChipPosition.SizeY = dblBox_Y_Max[index] - dblBox_Y_Min[index];

                                int startX = (int)dblBox_X_Min[index] + OffsetX;
                                int endX = (int)dblBox_X_Max[index] + OffsetX;
                                int startY = (int)dblBox_Y_Min[index] + OffsetY;
                                int endY = (int)dblBox_Y_Max[index] + OffsetY;
                                double pixelSum = 0;
                                int pixelCount = 0;
                                for (int y = startY; y <= endY; y++)
                                {
                                    int rowIndex = y * _grabedImage.SizeX;

                                    for (int x = startX; x <= endX; x++)
                                    {
                                        int pixelOffset = rowIndex + x;

                                        pixelSum += binaryTargetBuffer[pixelOffset];
                                        pixelCount++;
                                    }
                                }
                                mChipPosition.AveragePixelValue = pixelSum / pixelCount;
                                mChipPosition.PixelSumValue = pixelSum;
                                mChipPosition.CenterWeight = GeometryHelper.GetDistance2D(
                                                                SizeX / 2, SizeY / 2,
                                                                mChipPosition.SizeX, mChipPosition.SizeY);
                                int ref_avg = 90;
                                if (mChipPosition.AveragePixelValue <= ref_avg)
                                {
                                    IsFound[index] = false;
                                    devicePositions.Add(mChipPosition);
                                    LoggerManager.Debug($"Mark Index = {index + 1} :  Average pixel value = {mChipPosition.AveragePixelValue:0.00}, The luminance of the target is too low.( <= {ref_avg})");
                                }
                                else
                                {
                                    IsFound[index] = true;
                                    devicePositions.Add(mChipPosition);
                                    LoggerManager.Debug($"Mark Index = {index + 1} : Average pixel value = {mChipPosition.AveragePixelValue:0.00}, ( <= {ref_avg})");
                                }
                            }
                            else
                            {
                                IsFound[index] = false;
                            }
                        }
                        else
                        {
                            IsFound[index] = false;

                            MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_LABEL_VALUE, MIL.M_EQUAL, dblBlobIndex[index], MIL.M_NULL);
                        }
                    }
                    int ReturnBlobIndex = -1;

                    if (devicePositions.Count == 1)
                    {
                        if (IsFound.FirstOrDefault() == false)
                        {
                            devicePositions.Clear();
                            IsSuccess = false;
                        }
                        else
                        {
                            ReturnBlobIndex = IsFound.ToList().FindIndex(x => x == true);
                            IsSuccess = true;
                        }
                    }
                    else if (devicePositions.Count > 1)
                    {
                        LoggerManager.Debug($"FindBlobWithRectangularity(): Multiple occurrences. Occurrence = {devicePositions.Count}");
                        try
                        {
                            var devCand = from dev in devicePositions.ToList()
                                          orderby dev.AveragePixelValue descending
                                          select dev;
                            ReturnBlobIndex = (int)devCand.FirstOrDefault().DevIndex;
                            if (IsFound[ReturnBlobIndex])
                            {
                                devicePositions.Clear();
                                devicePositions.Add(devCand.FirstOrDefault());
                                IsSuccess = true;
                            }
                            else
                            {
                                devicePositions.Clear();
                                IsSuccess = false;
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"FindBlobWithRectangularity(): Error occurred. Err = {err.Message}");
                            IsSuccess = false;
                        }
                    }
                    else
                    {
                        IsSuccess = false;
                    }

                    if (IsSuccess == true)
                    {
                        ImgPosX = dblGravity_X[ReturnBlobIndex];
                        ImgPosY = dblGravity_Y[ReturnBlobIndex];


                        int centerX = Convert.ToInt32(ImgPosX);
                        int centerY = Convert.ToInt32(ImgPosY);

                        ImgPosX += OffsetX;
                        ImgPosY += OffsetY;

                        LoggerManager.Debug($"PinBlob Center : [{centerX},{centerY}]", isInfo: IsInfo);
                        LoggerManager.Debug($"PinBlob Area : [{dblBlobArea[ReturnBlobIndex]}]", isInfo: IsInfo);
                    }
                }

                // Alloc Color Buffer

                BlackAndWhiteToColor(_grabedImage, ref milDrawingOriginalBuffer, true);
                BlackAndWhiteToColor(milProcBuffer, ref milDrawingChildBuffer, true);

                // Blob을 계산할 때, ChildBuffer(Search Area가 적용된)를 사용했기 때문에
                // 해당 사이즈와 같은 컬러 버퍼를 할당하고 Drawing을 진행한다.
                // 이 때, 저장할 이미지는 원본 영상의 크기로 저장하기 때문에, Childer Buffer의 Offset을 이용하여 MbufPutColor2d를 통해, 이미지를 합성시킨 후 저장한다.

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                MIL.MblobDraw(MIL.M_DEFAULT, BlobResult, milDrawingChildBuffer, MIL.M_DRAW_CENTER_OF_GRAVITY + MIL.M_DRAW_BLOBS_CONTOUR, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);
                MIL.MgraText(MIL.M_DEFAULT, milDrawingChildBuffer,
                    ImgPosX - OffsetX - 32, ImgPosY - OffsetY + BlobSizeY / 2 + 8, "SELECTED");

                MIL_INT BufSizeX = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT BufSizeY = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_Y, MIL.M_NULL);
                MIL_INT BufBand = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_BAND, MIL.M_NULL);
                MIL_INT BufSizeBit = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_BIT, MIL.M_NULL);

                byte[] DrawingChildBufferArr = new byte[BufSizeX * BufSizeY * BufBand];

                MIL.MbufGetColor(milDrawingChildBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, DrawingChildBufferArr);
                MIL.MbufPutColor2d(milDrawingOriginalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, OffsetX, OffsetY, SizeX, SizeY, DrawingChildBufferArr);

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);
                MIL.MgraRect(MIL.M_DEFAULT, milDrawingOriginalBuffer, OffsetX, OffsetY, SizeX + OffsetX - 1, SizeY + OffsetY - 1);

                byte[] resultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY * 3];
                MIL.MbufGetColor(milDrawingOriginalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                var originalimagebuffer = new ImageBuffer(_grabedImage);
                var resultimagebuffer = new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, (int)ColorDept.Color24);

                retval = new BlobResult(originalimagebuffer, resultimagebuffer, devicePositions);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milOrgImgBuffer != null)
                {
                    MIL.MbufFree(milOrgImgBuffer);
                }
                if (milProcBuffer != null)
                    MIL.MbufFree(milProcBuffer);
                if (milImageBuffer != null)
                    MIL.MbufFree(milImageBuffer);
                if (milBinBuffer != null)
                {
                    MIL.MbufFree(milBinBuffer);
                }
                if (BlobResult != null)
                    MIL.MblobFree(BlobResult);

                if (FeatureList != null)
                    MIL.MblobFree(FeatureList);

                if (milDrawingOriginalBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milDrawingOriginalBuffer);
                    milDrawingOriginalBuffer = MIL.M_NULL;
                }

                if (milDrawingChildBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milDrawingChildBuffer);
                    milDrawingChildBuffer = MIL.M_NULL;
                }

                if(m_Context != MIL.M_NULL)
                {
                    MIL.MimFree(m_Context);
                }
            }

            return retval;
        }
        public BlobResult FindBlob(ImageBuffer _grabedImage,
                            ref double ImgPosX,
                            ref double ImgPosY,
                            int Threshold,
                            int BlobAreaLow,
                            int BlobAreaHigh,
                            int OffsetX,
                            int OffsetY,
                            int SizeX,
                            int SizeY,
                            double BlobSizeX,
                            double BlobSizeY,
                            bool isDilation,
                            double BlobSizeXMinimumMargin = 0.5,
                            double BlobSizeXMaximumMargin = 0.5,
                            double BlobSizeYMinimumMargin = 0.5,
                            double BlobSizeYMaximumMargin = 0.5)
        {
            MIL_ID FeatureList = 0, BlobResult = 0;

            MIL_INT TotalBlobs = 0;

            MIL_ID milImageChildBuffer = MIL.M_NULL;
            MIL_ID milImageBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;

            MIL_INT pttSizeX = 0, pttSizeY = 0, pttType = 0;

            MIL_ID milDrawingOriginalBuffer = MIL.M_NULL;
            MIL_ID milDrawingChildBuffer = MIL.M_NULL;

            ObservableCollection<GrabDevPosition> devicePositions = new ObservableCollection<GrabDevPosition>();
            BlobResult retval = null;

            try
            {
                if ((SizeX <= 0) || (SizeY <= 0))
                {
                    return retval;
                }

                if (_grabedImage != null)
                {
                    MIL.MbufAlloc2d(MilSystem, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.ColorDept, Attributes, ref milImageBuffer);

                    MIL.MbufClear(milImageBuffer, 0); //MilProcBuffer 버퍼를 0색상으로 지워준다.                

                    MIL.MbufPut(milImageBuffer, _grabedImage.Buffer);

                    MIL.MbufChild2d(milImageBuffer, OffsetX, OffsetY, SizeX, SizeY, ref milImageChildBuffer);
                    
                    MIL.MbufAlloc2d(MilSystem, SizeX, SizeY, _grabedImage.ColorDept, Attributes, ref milProcBuffer);
                    MIL.MbufCopy(milImageChildBuffer, milProcBuffer);
                }

                //<2022-09-28-9E23  comment = "Auto Threshold 기준을 Threshold 값이 음수 인지를 기준으로함" UnitTest = "RecipeExecuteFixedSizeNReservedRecipeSinglePinAlignTipBlobTest" >
                if (isDilation)
                {
                    byte[] imageProcbuffer = new byte[SizeX * SizeY];
                    MIL.MbufGet(milProcBuffer, imageProcbuffer);
                    //<2022-05-03-3C89  comment = "replace by dwbae" UnitTest = "MakeSameResultTestEnhancedOtsu" >
                    (byte[], int, int, int)[] inputImages = { (imageProcbuffer, SizeX, SizeY, 1) };

                    bool autoMode = (Threshold < 0);

                    string recipeParam = $"{{" +
                               $" \"ThresholdFilter\" : {{\"Params\":[  {{\"Key\":\"Threshold\", \"Value\":\"{Threshold}\"}}]}}" +
                               $", \"PinBlobModeSelector\" : {{\"Params\":[  {{\"Key\":\"AutoMode\", \"Value\":\"{autoMode}\"}}]}}" +
                               $"}}";

                    //var visionLib = new VisionFrameworkLib();
                    var visionLib = this.VisionManager().VisionLib;

                    MIL.MbufPut(milProcBuffer, visionLib.RecipeExecuteFixedSize(inputImages, visionLib.GetReservedRecipe(ProberInterfaces.VisionFramework.ReservedRecipe.SinglePinAlignTipBlob), out string result, recipeParam));
                    //</2022-05-03-3C89>
                }
                else
                {
                    if (Threshold > 0)
                    {
                        MIL.MimBinarize(milProcBuffer, milProcBuffer, MIL.M_GREATER_OR_EQUAL, Threshold, MIL.M_NULL);
                        MIL.MimOpen(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                        MIL.MimClose(milProcBuffer, milProcBuffer, 1, MIL.M_BINARY);
                    }
                }//<2022-09-28-9E23 >

                MIL.MblobAllocFeatureList(MilSystem, ref FeatureList);
                MIL.MblobSelectFeature(FeatureList, MIL.M_AREA);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_AREA);
                MIL.MblobSelectFeature(FeatureList, MIL.M_CENTER_OF_GRAVITY);
                MIL.MblobSelectFeature(FeatureList, MIL.M_ELONGATION);

                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_X_MIN);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_X_MAX);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MIN);
                MIL.MblobSelectFeature(FeatureList, MIL.M_BOX_Y_MAX);

                MIL.MblobAllocResult(MilSystem, ref BlobResult);

                MIL.MblobCalculate(milProcBuffer, MIL.M_NULL, FeatureList, BlobResult);

                MIL.MblobSelect(BlobResult, MIL.M_INCLUDE, MIL.M_ALL_BLOBS, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                MIL.MblobGetNumber(BlobResult, ref TotalBlobs);

                LoggerManager.Debug($"[ModuleVisionProcessing] FindBlob() : Before select, Total blobs : {TotalBlobs}", isInfo: IsInfo);

                double arealow = BlobAreaLow;
                double areahigh = BlobAreaHigh;

                if (areahigh < arealow)
                {
                    LoggerManager.Debug($"[ModuleVisionProcessing] FindBlob() : BlobAreaLow : {BlobAreaLow} > BlobAreaHigh : {BlobAreaHigh}", isInfo: IsInfo);

                    areahigh = arealow;
                }

                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MIN, MIL.M_LESS_OR_EQUAL, 0, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_X_MAX, MIL.M_GREATER_OR_EQUAL, SizeX - 1, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MIN, MIL.M_LESS_OR_EQUAL, 0, MIL.M_NULL);
                MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_BOX_Y_MAX, MIL.M_GREATER_OR_EQUAL, SizeY - 1, MIL.M_NULL);

                MIL.MblobGetNumber(BlobResult, ref TotalBlobs);
                LoggerManager.Debug($"[ModuleVisionProcessing] FindBlob() : After select [POSITION], Total blobs : {TotalBlobs}", isInfo: IsInfo);

                bool IsSuccess = false;
                bool UseMergeProcess = true;

                double AreaInnerMarginRatio = 0.1;

                bool NotUseSizeXFilter = false;
                bool NotUseSizeYFilter = false;

                double AreaLowAddMarginValue = arealow * (1.0 - AreaInnerMarginRatio);
                double AreaHighAddMarginValue = areahigh * (1.0 + AreaInnerMarginRatio);

                double SizeXLowAddMarginValue = 0;
                double SizeXHighAddMarginValue = 0;

                double SizeYLowAddMarginValue = 0;
                double SizeYHighAddMarginValue = 0;

                if (BlobSizeX == 0)
                {
                    NotUseSizeXFilter = true;
                }
                else
                {
                    SizeXLowAddMarginValue = BlobSizeX * (1.0 - BlobSizeXMinimumMargin);
                    SizeXHighAddMarginValue = BlobSizeX * (1.0 + BlobSizeXMaximumMargin);
                }

                if (BlobSizeY == 0)
                {
                    NotUseSizeYFilter = true;
                }
                else
                {
                    SizeYLowAddMarginValue = BlobSizeY * (1.0 - BlobSizeYMinimumMargin);
                    SizeYHighAddMarginValue = BlobSizeY * (1.0 + BlobSizeYMaximumMargin);
                }

                LoggerManager.Debug($"Mark Tolerance Information", isInfo: IsInfo);

                LoggerManager.Debug($"{AreaLowAddMarginValue} <= AREA <= {AreaHighAddMarginValue}", isInfo: IsInfo);

                LoggerManager.Debug($"{0} <= Position X <= {SizeX - 1}", isInfo: IsInfo);
                LoggerManager.Debug($"{0} <= Position Y <= {SizeY - 1}", isInfo: IsInfo);

                if (NotUseSizeXFilter == false)
                {
                    LoggerManager.Debug($"{SizeXLowAddMarginValue} <= Size X <= {SizeXHighAddMarginValue}, Margin (%) : MIN({BlobSizeXMinimumMargin}), MAX({BlobSizeXMaximumMargin})", isInfo: IsInfo);
                }
                else
                {
                    LoggerManager.Debug($"Size X Filter not used.", isInfo: IsInfo);
                }

                if (NotUseSizeYFilter == false)
                {
                    LoggerManager.Debug($"{SizeYLowAddMarginValue} <= Size Y <= {SizeYHighAddMarginValue}, Margin (%) : MIN({BlobSizeYMinimumMargin}), MAX({BlobSizeYMaximumMargin})", isInfo: IsInfo);
                }
                else
                {
                    LoggerManager.Debug($"Size Y Filter not used.", isInfo: IsInfo);
                }

                if (TotalBlobs > 0)
                {
                    double[] dblBlobArea = new double[TotalBlobs];

                    double[] dblSizeX = new double[TotalBlobs];
                    double[] dblSizeY = new double[TotalBlobs];

                    double[] dblBox_X_Max = new double[TotalBlobs];
                    double[] dblBox_X_Min = new double[TotalBlobs];
                    double[] dblBox_Y_Max = new double[TotalBlobs];
                    double[] dblBox_Y_Min = new double[TotalBlobs];

                    double[] dblGravity_X = new double[TotalBlobs];
                    double[] dblGravity_Y = new double[TotalBlobs];

                    double[] dblBlobIndex = new double[TotalBlobs];

                    bool[] IsFound = new bool[TotalBlobs];

                    MIL.MblobGetResult(BlobResult, MIL.M_AREA, dblBlobArea);

                    MIL.MblobGetResult(BlobResult, MIL.M_FERET_X, dblSizeX);
                    MIL.MblobGetResult(BlobResult, MIL.M_FERET_Y, dblSizeY);

                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                    MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                    MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblGravity_X);
                    MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblGravity_Y);

                    MIL.MblobGetResult(BlobResult, MIL.M_LABEL_VALUE, dblBlobIndex);

                    GrabDevPosition mChipPosition;

                    for (int index = 0; index < TotalBlobs; index++)
                    {
                        LoggerManager.Debug($"Mark Index = {index + 1} : SIZE X, Y = ({dblSizeX[index]}, {dblSizeY[index]}), AREA = {dblBlobArea[index]}", isInfo: IsInfo);

                        if ((dblBlobArea[index] >= AreaLowAddMarginValue) &&
                             (dblBlobArea[index] <= AreaHighAddMarginValue)
                             )
                        {
                            bool SizeXFilterPassed = false;
                            bool SizeYFilterPassed = false;

                            if (NotUseSizeXFilter == false)
                            {
                                if ((dblSizeX[index] >= SizeXLowAddMarginValue) &&
                                    (dblSizeX[index] <= SizeXHighAddMarginValue))
                                {
                                    SizeXFilterPassed = true;
                                }
                            }
                            else
                            {
                                SizeXFilterPassed = true;
                            }

                            if (NotUseSizeXFilter == false)
                            {
                                if ((dblSizeY[index] >= SizeYLowAddMarginValue) &&
                                    (dblSizeY[index] <= SizeYHighAddMarginValue))
                                {
                                    SizeYFilterPassed = true;
                                }
                            }
                            else
                            {
                                SizeYFilterPassed = true;
                            }

                            if (SizeXFilterPassed == true && SizeYFilterPassed == true)
                            {
                                IsFound[index] = true;

                                mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);

                                mChipPosition.Area = dblBlobIndex[index];
                                mChipPosition.DevIndex = index;
                                mChipPosition.PosX = Math.Round(dblBlobIndex[index], 3);
                                mChipPosition.PosY = Math.Round(dblBlobIndex[index], 3);

                                mChipPosition.SizeX = dblBox_X_Max[index] - dblBox_X_Min[index];
                                mChipPosition.SizeY = dblBox_Y_Max[index] - dblBox_Y_Min[index];

                                devicePositions.Add(mChipPosition);
                            }
                            else
                            {
                                IsFound[index] = false;
                            }
                        }
                        else
                        {
                            IsFound[index] = false;

                            MIL.MblobSelect(BlobResult, MIL.M_EXCLUDE, MIL.M_LABEL_VALUE, MIL.M_EQUAL, dblBlobIndex[index], MIL.M_NULL);
                        }
                    }

                    int ReturnBlobIndex = -1;
                    double dblFinalBlobArea = 0;
                    
                    if (devicePositions.Count == 1)
                    {
                        ReturnBlobIndex = IsFound.ToList().FindIndex(x => x == true);

                        ImgPosX = dblGravity_X[ReturnBlobIndex];
                        ImgPosY = dblGravity_Y[ReturnBlobIndex];
                        dblFinalBlobArea = dblBlobArea[ReturnBlobIndex];

                        IsSuccess = true;
                    }
                    else if (devicePositions.Count >= 2)
                    {
                        MIL.MblobGetNumber(BlobResult, ref TotalBlobs);

                        dblBlobArea = new double[TotalBlobs];

                        dblSizeX = new double[TotalBlobs];
                        dblSizeY = new double[TotalBlobs];

                        dblBox_X_Max = new double[TotalBlobs];
                        dblBox_X_Min = new double[TotalBlobs];
                        dblBox_Y_Max = new double[TotalBlobs];
                        dblBox_Y_Min = new double[TotalBlobs];

                        dblGravity_X = new double[TotalBlobs];
                        dblGravity_Y = new double[TotalBlobs];

                        dblBlobIndex = new double[TotalBlobs];

                        MIL.MblobGetResult(BlobResult, MIL.M_AREA, dblBlobArea);

                        MIL.MblobGetResult(BlobResult, MIL.M_FERET_X, dblSizeX);
                        MIL.MblobGetResult(BlobResult, MIL.M_FERET_Y, dblSizeY);

                        MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MAX, dblBox_X_Max);
                        MIL.MblobGetResult(BlobResult, MIL.M_BOX_X_MIN, dblBox_X_Min);
                        MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MAX, dblBox_Y_Max);
                        MIL.MblobGetResult(BlobResult, MIL.M_BOX_Y_MIN, dblBox_Y_Min);

                        MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_X, dblGravity_X);
                        MIL.MblobGetResult(BlobResult, MIL.M_CENTER_OF_GRAVITY_Y, dblGravity_Y);

                        MIL.MblobGetResult(BlobResult, MIL.M_LABEL_VALUE, dblBlobIndex);

                        // 말도 안되는 경우는 Fail 시키자.

                        double dblGravity_Total_X = 0;
                        double dblGravity_Total_Y = 0;
                        
                        double dblArea_Total = 0;

                        double dblGravity_Avg_X = 0;
                        double dblGravity_Avg_Y = 0;

                        for (int j = 0; j < TotalBlobs; j++)
                        {
                            dblGravity_Total_X += dblGravity_X[j];
                            dblGravity_Total_Y += dblGravity_Y[j];
                            dblArea_Total += dblBlobArea[j];
                        }

                        // 전체 블랍의 무게 중심의 평균값을 구한다.
                        dblGravity_Avg_X = dblGravity_Total_X / TotalBlobs;
                        dblGravity_Avg_Y = dblGravity_Total_Y / TotalBlobs;
                        dblFinalBlobArea = dblArea_Total / TotalBlobs;

                        double[] distance = new double[TotalBlobs];

                        double minDistThreshold = Math.Sqrt(AreaHighAddMarginValue) * Math.Sqrt(2);

                        for (int j = 0; j < TotalBlobs; j++)
                        {
                            // 각 Blob 무게중심과 전체 Blob 무게중심 간 거리 계산
                            double deltaX = dblGravity_Avg_X - dblGravity_X[j];
                            double deltaY = dblGravity_Avg_Y - dblGravity_Y[j];

                            distance[j] = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

                            if (distance[j] >= minDistThreshold)
                            {
                                // Fail
                                UseMergeProcess = false;
                                break;
                            }
                        }

                        if(!UseMergeProcess)
                        {
                            IsSuccess = false;
                        }

                        if(UseMergeProcess)
                        {
                            double[] dblBlobAveragePixelValue = new double[TotalBlobs];

                            byte[] childBuffer = new byte[SizeX * SizeY];
                            MIL.MbufGet(milImageChildBuffer, childBuffer);

                            byte[] BinaryBuffer = new byte[SizeX * SizeY];
                            MIL.MbufGet(milProcBuffer, BinaryBuffer);

                            double maxArea = AreaHighAddMarginValue;
                            double grayWeight = 0.7;
                            double areaWeight = 0.3;

                            double totalWeight = 0;
                            double weightedXSum = 0;
                            double weightedYSum = 0;

                            LoggerManager.Debug($"[{this.GetType().Name}], FindBlob() : [Wg = {grayWeight}, Wa = {areaWeight}, maxArea = {AreaHighAddMarginValue}]");

                            for (int j = 0; j < TotalBlobs; j++)
                            {
                                // Define the bounding box for the blob
                                int startX = (int)dblBox_X_Min[j];
                                int endX = (int)dblBox_X_Max[j];
                                int startY = (int)dblBox_Y_Min[j];
                                int endY = (int)dblBox_Y_Max[j];

                                // Sum all pixel values within the blob's area
                                double pixelSum = 0;
                                int pixelCount = 0;

                                for (int y = startY; y <= endY; y++)
                                {
                                    int rowIndex = y * SizeX;

                                    for (int x = startX; x <= endX; x++)
                                    {
                                        int index = rowIndex + x;

                                        if (BinaryBuffer[index] == 255)
                                        {
                                            pixelSum += childBuffer[index];
                                            pixelCount++;
                                        }
                                    }
                                }

                                // Calculate the average pixel value for the blob
                                if (pixelCount != 0)
                                {
                                    dblBlobAveragePixelValue[j] = pixelSum / pixelCount;
                                }
                                else
                                {
                                    dblBlobAveragePixelValue[j] = 0;
                                }

                                double Gnorm = dblBlobAveragePixelValue[j] / 255.0;
                                double Anorm = dblBlobArea[j] / maxArea;

                                double blobWeight = grayWeight * Gnorm + areaWeight * Anorm; 

                                totalWeight += blobWeight;
                                weightedXSum += dblGravity_X[j] * blobWeight;
                                weightedYSum += dblGravity_Y[j] * blobWeight;

                                LoggerManager.Debug($"[{this.GetType().Name}], FindBlob() : GravityX = {dblGravity_X[j]:0.00}, GravityY = {dblGravity_Y[j]:0.00}, Gray Value = {dblBlobAveragePixelValue[j]:0.00}, Area = {dblBlobArea[j]:0.00}, xmin = {dblBox_X_Min[j]:0.00}, xmax = {dblBox_X_Max[j]:0.00}, ymin = {dblBox_Y_Min[j]:0.00}, ymax = {dblBox_Y_Max[j]:0.00}, Width = {dblSizeX[j]:0.00}, Height = {dblSizeY[j]:0.00}, Weight = {blobWeight}");
                            }

                            ImgPosX = weightedXSum / totalWeight;
                            ImgPosY = weightedYSum / totalWeight;

                            if(devicePositions != null)
                            {
                                devicePositions.Clear();
                            }

                            // 외부에서 GrabDevPosition의 개수를 확인하는 로직이 있기 때문에, 1개를 넣어놓는다.
                            mChipPosition = new GrabDevPosition(0, 0, 0, 0, 0, 0);
                            devicePositions.Add(mChipPosition);

                            IsSuccess = true;
                        }
                    }
                    else
                    {
                        IsSuccess = false;
                    }

                    if (IsSuccess == true)
                    {
                        int centerX = Convert.ToInt32(ImgPosX);
                        int centerY = Convert.ToInt32(ImgPosY);

                        ImgPosX += OffsetX;
                        ImgPosY += OffsetY;

                        LoggerManager.Debug($"PinBlob Center : [{centerX},{centerY}]", isInfo: IsInfo);
                        LoggerManager.Debug($"PinBlob Area : [{dblFinalBlobArea}]", isInfo: IsInfo);
                    }
                }

                // Alloc Color Buffer
                BlackAndWhiteToColor(_grabedImage, ref milDrawingOriginalBuffer, true);
                BlackAndWhiteToColor(milProcBuffer, ref milDrawingChildBuffer, true);

                // Blob을 계산할 때, ChildBuffer(Search Area가 적용된)를 사용했기 때문에
                // 해당 사이즈와 같은 컬러 버퍼를 할당하고 Drawing을 진행한다.
                // 이 때, 저장할 이미지는 원본 영상의 크기로 저장하기 때문에, Childer Buffer의 Offset을 이용하여 MbufPutColor2d를 통해, 이미지를 합성시킨 후 저장한다.

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);
                MIL.MblobDraw(MIL.M_DEFAULT, BlobResult, milDrawingChildBuffer, MIL.M_DRAW_CENTER_OF_GRAVITY + MIL.M_DRAW_BLOBS_CONTOUR, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT);

                if(IsSuccess && UseMergeProcess)
                {
                    int arrowlength = 3;

                    double xpos = ImgPosX - OffsetX;
                    double ypos = ImgPosY - OffsetY;

                    MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_RED);
                    MIL.MgraLine(MIL.M_DEFAULT, milDrawingChildBuffer, xpos, ypos - arrowlength, xpos, ypos + arrowlength);
                    MIL.MgraLine(MIL.M_DEFAULT, milDrawingChildBuffer, xpos - arrowlength, ypos, xpos + arrowlength, ypos);
                }

                MIL_INT BufSizeX = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_X, MIL.M_NULL);
                MIL_INT BufSizeY = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_Y, MIL.M_NULL);
                MIL_INT BufBand = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_BAND, MIL.M_NULL);
                MIL_INT BufSizeBit = MIL.MbufInquire(milDrawingChildBuffer, MIL.M_SIZE_BIT, MIL.M_NULL);

                byte[] DrawingChildBufferArr = new byte[BufSizeX * BufSizeY * BufBand];

                MIL.MbufGetColor(milDrawingChildBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, DrawingChildBufferArr);
                MIL.MbufPutColor2d(milDrawingOriginalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, OffsetX, OffsetY, SizeX, SizeY, DrawingChildBufferArr);

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_BLUE);
                MIL.MgraRect(MIL.M_DEFAULT, milDrawingOriginalBuffer, OffsetX, OffsetY, SizeX + OffsetX - 1, SizeY + OffsetY - 1);

                byte[] resultBuffer = new byte[_grabedImage.SizeX * _grabedImage.SizeY * 3];
                MIL.MbufGetColor(milDrawingOriginalBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, resultBuffer);

                var originalimagebuffer = new ImageBuffer(_grabedImage);
                var resultimagebuffer = new ImageBuffer(resultBuffer, _grabedImage.SizeX, _grabedImage.SizeY, _grabedImage.Band, (int)ColorDept.Color24);

                retval = new BlobResult(originalimagebuffer, resultimagebuffer, devicePositions);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (milImageChildBuffer != null)
                {
                    MIL.MbufFree(milImageChildBuffer);
                }

                if (milProcBuffer != null)
                {
                    MIL.MbufFree(milProcBuffer);
                }
                
                if (milImageBuffer != null)
                {
                    MIL.MbufFree(milImageBuffer);
                }

                if (BlobResult != null)
                {
                    MIL.MblobFree(BlobResult);
                }

                if (FeatureList != null)
                {
                    MIL.MblobFree(FeatureList);
                }

                if (milDrawingOriginalBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milDrawingOriginalBuffer);
                    milDrawingOriginalBuffer = MIL.M_NULL;
                }

                if (milDrawingChildBuffer != MIL.M_NULL)
                {
                    MIL.MbufFree(milDrawingChildBuffer);
                    milDrawingChildBuffer = MIL.M_NULL;
                }
            }

            return retval;
        }

        public void Mil_Crop(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480)
        {
            MIL_ID Source = 0;
            MIL_ID ProcessingImg = 0;

            try
            {
                byte[] resultBuffer = new byte[SizeX * SizeY];

                MIL.MbufAlloc2d(MilSystem, SourceImg.SizeX, SourceImg.SizeY, SourceImg.ColorDept, lAttributes, ref Source);
                MIL.MbufClear(Source, 0);
                MIL.MbufPut(Source, SourceImg.Buffer);

                MIL.MbufChild2d(Source, OffsetX, OffsetY, SizeX, SizeY, ref ProcessingImg);

                MIL.MbufGet(ProcessingImg, resultBuffer);

                ResultImg = new ImageBuffer(resultBuffer, SizeX, SizeY, SourceImg.Band, SourceImg.ColorDept);

                resultBuffer = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (ProcessingImg != MIL.M_NULL)
                {
                    MIL.MbufFree(ProcessingImg);
                    MIL.MbufFree(Source);
                }
                else if (Source != null)
                {
                    MIL.MbufFree(ProcessingImg);
                    MIL.MbufFree(Source);
                }
            }
        }
        public void Mil_Binarize(ImageBuffer SourceImg, ref ImageBuffer ResultImg, int Threshold, int OffsetX = 0, int OffsetY = 0, int SizeX = 480, int SizeY = 480)
        {
            MIL_ID Source = 0;
            MIL_ID ProcessingImg = 0;

            try
            {
                //MIL_ID Result = 0;

                byte[] resultBuffer = new byte[SizeX * SizeY];

                MIL.MbufAlloc2d(MilSystem, SourceImg.SizeX, SourceImg.SizeY, SourceImg.ColorDept, lAttributes, ref Source);

                MIL.MbufClear(Source, 0);

                MIL.MbufPut(Source, SourceImg.Buffer);

                MIL.MbufChild2d(Source, OffsetX, OffsetY, SizeX, SizeY, ref ProcessingImg);

                if (Threshold > 0)
                {
                    MIL.MimBinarize(ProcessingImg, ProcessingImg, MIL.M_GREATER, Threshold, MIL.M_NULL);
                    MIL.MimOpen(ProcessingImg, ProcessingImg, 1, MIL.M_BINARY);
                    MIL.MimClose(ProcessingImg, ProcessingImg, 1, MIL.M_BINARY);

                    MIL.MbufGet(ProcessingImg, resultBuffer);
                }
                for (int i = 0; i < SizeX; i++)
                {
                    for (int j = 0; j < SizeY; j++)
                    {
                        ResultImg.Buffer[(i + OffsetX) + ((j + OffsetY) * SourceImg.SizeX)] = resultBuffer[i + j * SizeX];
                    }
                }
                resultBuffer = null;
                //ResultImg = new ImageBuffer(SourceImg);

                //MIL.MbufAlloc2d(MilSystem, ResultImg.SizeX, ResultImg.SizeY, ResultImg.DataType, iAttributes, ref Result);

                //MIL.MbufClear(Result, 0);

                //MIL.MbufPut(Result, ResultImg.Buffer);

                //MIL.MbufFree(ProcessingImg);
                //MIL.MbufFree(Source);

                //SourceImg = null;
                //GC.Collect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (ProcessingImg != MIL.M_NULL)
                {
                    MIL.MbufFree(ProcessingImg);
                    MIL.MbufFree(Source);
                }
                else if (Source != null)
                {
                    MIL.MbufFree(ProcessingImg);
                    MIL.MbufFree(Source);
                }
            }
        }

        public EventCodeEnum GetPatternSize(string path, out Size size)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int patternSizeX = 0;
            int patternSizeY = 0;
            try
            {
                if (File.Exists(path))
                {

                    MIL_INT pttSizeX = 0, pttSizeY = 0;
                    MIL.MbufDiskInquire(path, MIL.M_SIZE_X, ref pttSizeX);
                    MIL.MbufDiskInquire(path, MIL.M_SIZE_Y, ref pttSizeY);

                    patternSizeX = (int)pttSizeX;
                    patternSizeY = (int)pttSizeY;

                    size = new Size(patternSizeX, patternSizeY);
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    size = new Size();
                    retVal = EventCodeEnum.VISION_PATTERN_NOTEXIST;
                }
            }
            catch (MILException err)
            {
                throw new VisionException("GetPatternSize Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            return retVal;
        }

        public byte[] ConvertToImageBuffer(object image, int sizex, int sizey)
        {
            byte[] ResultBuffer = new byte[sizex * sizey * 3];
            MIL_ID originalImg = (MIL_ID)image;
            try
            {
                if (sizex != 0 && sizey != 0)
                {

                    MIL.MbufGetColor(originalImg, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BAND, ResultBuffer);
                }
                else
                {
                    ResultBuffer = null;
                }
            }
            catch (MILException err)
            {
                throw new VisionException("ConvertToImageBuffer Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (originalImg != null) MIL.MbufFree(originalImg);
            }
            return ResultBuffer;
        }

        public ImageBuffer WirteTextToBuffer(List<string> sb, ImageBuffer ib)
        {
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            try
            {
                double ypos = 0.0;
                MIL.MbufAlloc2d(MilSystem, ib.SizeX, ib.SizeY,
                                                  ib.ColorDept, iAttributes, ref milProcBuffer);

                MIL.MbufAllocColor(MilSystem, 3, ib.SizeX, ib.SizeY,
                                                    8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);
                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_DARK_GREEN);
                double fontsize = 0.8;
                MIL.MgraFontScale(MIL.M_DEFAULT, fontsize, fontsize);
                MIL.MgraFont(MIL.M_DEFAULT, MIL.MIL_FONT_NAME(MIL.M_FONT_DEFAULT_TTF));

                if (ib.Buffer != null)
                {
                    if (ib.ColorDept == (int)ColorDept.BlackAndWhite)
                    {
                        MIL.MbufPut(milProcBuffer, ib.Buffer);
                        MIL.MbufCopy(milProcBuffer, milProcResultBuffer);
                        foreach (var text in sb)
                        {
                            MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer, 0, ib.SizeY / 2, text);
                            ypos += 14.0;
                        }

                    }

                    else if (ib.ColorDept == (int)ColorDept.Color24)
                    {
                        MIL.MbufPutColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, ib.Buffer);
                        foreach (var text in sb)
                        {
                            MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer, 0, ib.SizeY / 2, text);
                            ypos += 14.0;
                        }
                    }

                }
                else
                {


                    MIL.MbufClear(milProcResultBuffer, 0);
                    foreach (var text in sb)
                    {
                        MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer, 0, ypos, text);
                        ypos += 14.0;
                    }
                }

                ib.Buffer = new byte[ib.SizeX * ib.SizeY * 3];
                MIL.MbufGetColor(milProcResultBuffer, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, ib.Buffer);
                ib.Band = 3;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                MIL.MbufFree(milProcResultBuffer); milProcResultBuffer = MIL.M_NULL;
                MIL.MbufFree(milProcBuffer); milProcBuffer = MIL.M_NULL;
            }
            return ib;
        }

        public double EdgeFind_IndexAlign(ImageBuffer SrcBuf, int Cpos, int RWidth, int Threshold)
        {
            double retval = -1;

            try
            {
                LoggerManager.Debug($"IndexAlign_FindEdge : Cpos = {Cpos}, RWidth = {RWidth}, Threshold = {Threshold}");

                ImageBuffer Line_Equalization_Buf;

                int ImageWidth = SrcBuf.SizeX;
                int ImageHeight = SrcBuf.SizeY;

                int alphaS, BetaS;
                int alphaE, BetaE;
                int PMFLAG;

                int Except_Pixel = 30;
                int Acc_Pixel = 20;
                int Cmp_Pixel = 10;

                double Temp_Sum = 0;
                double Temp_Avg = 0;

                int j;

                int XPos, YPos;

                //double[] EdgeOVal = new double[480];
                //double[] EdgePLAvg = new double[480];
                //double[] EdgePRAvg = new double[480];
                //double[] EdgeMOVal = new double[480];

                double[] EdgeOVal = new double[ImageWidth];
                double[] EdgePLAvg = new double[ImageWidth];
                double[] EdgePRAvg = new double[ImageWidth];
                double[] EdgeMOVal = new double[ImageWidth];


                double P_RSum;
                double P_LSum;

                bool OKFLAG = false;

                int innerAreaMin = (ImageWidth / 2 - 1) - (RWidth / 2);
                int innerAreaMax = (ImageWidth / 2 - 1) + (RWidth / 2);

                int lastWidthPixelValue = ImageWidth - 1;
                int lastHeightPixelValue = ImageHeight - 1;

                Line_Equalization_Buf = Line_Equalization_Processing(SrcBuf, Cpos);

                byte[,] Equal2DBufArray = ConvertByte1DTo2DArray(Line_Equalization_Buf);

                if (Line_Equalization_Buf == null)
                {
                    retval = -1;
                }
                else
                {
                    if ((Cpos == 1) || (Cpos == 2))
                    {
                        alphaS = 0;
                        alphaE = lastWidthPixelValue;
                    }
                    else
                    {
                        alphaS = lastWidthPixelValue;
                        alphaE = 0;
                    }

                    if ((Cpos == 0) || (Cpos == 1))
                    {
                        BetaS = 0;
                        BetaE = lastWidthPixelValue;

                        PMFLAG = 1;
                    }
                    else
                    {
                        BetaS = lastWidthPixelValue;
                        BetaE = 0;

                        PMFLAG = -1;
                    }

                    j = BetaS + (Except_Pixel * PMFLAG);

                    LoggerManager.Debug($"IndexAlign_FindEdge : Min = {innerAreaMin} Max = {innerAreaMax} ");

                    // 0
                    if (Cpos == 0)
                    {
                        if (alphaS > alphaE)
                        {
                            for (int i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                            {
                                Temp_Sum = 0;

                                for (int k = 1; k <= Acc_Pixel; k++)
                                {
                                    XPos = i + k;
                                    YPos = (j - (k * PMFLAG));

                                    Temp_Sum += Equal2DBufArray[YPos, XPos];
                                }

                                Temp_Avg = Temp_Sum / Acc_Pixel;

                                if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                                {
                                    EdgeOVal[lastWidthPixelValue - i] = Temp_Avg - Equal2DBufArray[j, i];
                                }

                                j = j + PMFLAG;
                            }
                        }

                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                        {
                            P_RSum = 0;
                            P_LSum = 0;

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                //P_RSum += Equal2DBufArray[(i - k) * ImageWidth, (ImageWidth - 1 - i + k)];
                                P_RSum += Equal2DBufArray[(i - k), (lastWidthPixelValue - i + k)];
                            }

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                //P_LSum += Equal2DBufArray[(i + k) * ImageWidth, (ImageWidth - 1 - i - k)];
                                P_LSum += Equal2DBufArray[(i + k), (lastWidthPixelValue - i - k)];
                            }

                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                        }

                        for (int i = 0; i < ImageWidth; i++)
                        {
                            EdgeMOVal[lastWidthPixelValue - i] = Math.Abs(EdgeOVal[lastWidthPixelValue - i]) + Math.Abs(EdgePLAvg[lastWidthPixelValue - i] - EdgePRAvg[lastWidthPixelValue - i]);

                            if (EdgeMOVal[lastWidthPixelValue - i] > Threshold)
                            {
                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                {
                                    OKFLAG = true;
                                }
                            }

                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                            {
                                if (EdgeMOVal[lastWidthPixelValue - i] > retval)
                                {
                                    retval = EdgeMOVal[lastWidthPixelValue - i];
                                }
                            }
                        }
                    }

                    // 1
                    if (Cpos == 1)
                    {
                        for (int i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                        {
                            Temp_Sum = 0;

                            for (int k = 1; k <= Acc_Pixel; k++)
                            {
                                XPos = i - k;
                                YPos = (j - (k * PMFLAG));

                                Temp_Sum += Equal2DBufArray[YPos, XPos];
                            }

                            Temp_Avg = Temp_Sum / Acc_Pixel;

                            if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                            {
                                EdgeOVal[i] = Temp_Avg - Equal2DBufArray[j, i];
                            }

                            j = j + PMFLAG;
                        }

                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                        {
                            P_RSum = 0;
                            P_LSum = 0;

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                P_RSum += Equal2DBufArray[(i + k), (i + k)];
                            }

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                P_LSum += Equal2DBufArray[(i - k), (i - k)];
                            }

                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                        }

                        for (int i = 0; i < ImageWidth; i++)
                        {
                            EdgeMOVal[i] = Math.Abs(EdgeOVal[i]) + Math.Abs(EdgePLAvg[i] - EdgePRAvg[i]);
                            if (EdgeMOVal[i] > Threshold)
                            {
                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                {
                                    OKFLAG = true;
                                }
                            }

                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                            {
                                if (EdgeMOVal[i] > retval)
                                {
                                    retval = EdgeMOVal[i];
                                }
                            }
                        }
                    }

                    // 2
                    if (Cpos == 2)
                    {
                        for (int i = (alphaS + Except_Pixel); i <= (alphaE - 1); i++)
                        {
                            Temp_Sum = 0;

                            for (int k = 1; k <= Acc_Pixel; k++)
                            {
                                XPos = i - k;
                                YPos = (j - (k * PMFLAG));

                                Temp_Sum += Equal2DBufArray[YPos, XPos];
                            }

                            Temp_Avg = Temp_Sum / Acc_Pixel;

                            if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                            {
                                EdgeOVal[i] = Temp_Avg - Equal2DBufArray[j, i];
                            }

                            j = j + PMFLAG;
                        }

                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                        {
                            P_RSum = 0;
                            P_LSum = 0;

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                P_RSum += Equal2DBufArray[(lastWidthPixelValue - i + k), (i - k)];
                            }

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                //P_LSum += Equal2DBufArray[(ImageHeight - i + k), (i - k)];
                                P_LSum += Equal2DBufArray[(lastWidthPixelValue - i - k), (i + k)];
                            }

                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                        }

                        for (int i = 0; i < ImageWidth; i++)
                        {
                            EdgeMOVal[i] = Math.Abs(EdgeOVal[i]) + Math.Abs(EdgePLAvg[i] - EdgePRAvg[i]);

                            if (EdgeMOVal[i] > Threshold)
                            {
                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                {
                                    OKFLAG = true;
                                }
                            }

                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                            {
                                if (EdgeMOVal[i] > retval)
                                {
                                    retval = EdgeMOVal[i];
                                }
                            }
                        }
                    }

                    // 3
                    if (Cpos == 3)
                    {
                        for (int i = (alphaS - Except_Pixel); i >= (alphaE + 1); i--)
                        {
                            Temp_Sum = 0;

                            for (int k = 1; k <= Acc_Pixel; k++)
                            {
                                XPos = i + k;
                                YPos = (j - (k * PMFLAG));

                                Temp_Sum += Equal2DBufArray[YPos, XPos];
                            }

                            Temp_Avg = Temp_Sum / Acc_Pixel;

                            if ((i > Except_Pixel) && (i < ImageWidth - Except_Pixel))
                            {
                                EdgeOVal[lastWidthPixelValue - i] = Temp_Avg - Equal2DBufArray[j, i];
                            }

                            j = j + PMFLAG;
                        }

                        for (int i = Except_Pixel; i < lastWidthPixelValue - Except_Pixel; i++)
                        {
                            P_RSum = 0;
                            P_LSum = 0;

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                P_RSum += Equal2DBufArray[(lastWidthPixelValue - i - k), (lastWidthPixelValue - i - k)];
                            }

                            for (int k = 1; k <= Cmp_Pixel; k++)
                            {
                                //P_LSum += Equal2DBufArray[(ImageHeight - i - k), (ImageWidth - i - k)];
                                P_LSum += Equal2DBufArray[(lastWidthPixelValue - i + k), (lastWidthPixelValue - i + k)];
                            }

                            EdgePRAvg[i] = P_RSum / Cmp_Pixel;
                            EdgePLAvg[i] = P_LSum / Cmp_Pixel;
                        }

                        for (int i = 0; i < ImageWidth; i++)
                        {
                            EdgeMOVal[lastWidthPixelValue - i] = Math.Abs(EdgeOVal[lastWidthPixelValue - i]) + Math.Abs(EdgePLAvg[lastWidthPixelValue - i] - EdgePRAvg[lastWidthPixelValue - i]);

                            if (EdgeMOVal[lastWidthPixelValue - i] > Threshold)
                            {
                                if ((i >= innerAreaMin) && (i <= innerAreaMax))
                                {
                                    OKFLAG = true;
                                }
                            }

                            if ((i >= innerAreaMin) && (i <= innerAreaMax))
                            {
                                if (EdgeMOVal[lastWidthPixelValue - i] > retval)
                                {
                                    retval = EdgeMOVal[lastWidthPixelValue - i];
                                }
                            }
                        }
                    }

                    if (OKFLAG == true)
                    {
                        //LoggerManager.Debug($"IndexAlign_FindEdge(Sucess) : Result = {retval}", isInfo: IsInfo);
                    }
                    else
                    {
                        //LoggerManager.Error($"IndexAlign_FindEdge(Fail) : Result = {retval}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retval;
        }

        public List<ModelFinderResult> ModelFind(ImageBuffer targetImage,
            EnumModelTargetType targettype,
            EnumForegroundType foreground,
            Size size,
            int acceptance,
            double posx = 0, double posy = 0, double roiwidth = 0, double roiheight = 0,
            double scale_min = 0, double scale_max = 0,
            double horthick = 0, double verthick = 0, double smoothness = 70, int number = 0)
        {
            int maxOccurrences = 16;
            int foreGround = (int)MIL.M_ANY; ;
            List<ModelFinderResult> mfResults = new List<ModelFinderResult>();
            MIL_ID mMilImage = MIL.M_NULL;                               // Image buffer identifier.
            MIL_ID mMilSearchContext = MIL.M_NULL;                       // Search context.
            MIL_ID mMilResult = MIL.M_NULL;                              // Result identifier.
            double ModelDrawColor = MIL.M_COLOR_RED;                    // Model draw color.
            MIL_INT[] Model = new MIL_INT[maxOccurrences];               // Model index.
            MIL_INT mNumResults = 0;                                         // Number of results found.
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            int numOfOccur = 0;
            try
            {
                MIL.MbufAlloc2d(MilSystem, targetImage.SizeX, targetImage.SizeY,
                                 targetImage.ColorDept, iAttributes, ref milProcBuffer);

                MIL.MbufAllocColor(MilSystem, 3, targetImage.SizeX, targetImage.SizeY,
                                                    8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);

                if (targetImage.ColorDept == (int)ColorDept.BlackAndWhite) MIL.MbufPut(milProcBuffer, targetImage.Buffer);
                else if (targetImage.ColorDept == (int)ColorDept.Color24) ColorToBlackAndWrite(targetImage, ref milProcBuffer);

                MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                // Allocate a Geometric Model Finder context.
                MIL.MmodAlloc(MilSystem, MIL.M_GEOMETRIC, MIL.M_DEFAULT, ref mMilSearchContext);
                // Allocate a result buffer.
                MIL.MmodAllocResult(MilSystem, MIL.M_DEFAULT, ref mMilResult);
                // Define the model.
                if (horthick == 0)
                {
                    horthick = size.Width / 2d;
                }
                if (verthick == 0)
                {
                    verthick = size.Height / 2d;
                }
                if (smoothness <= 0.0)    // 기존 파라미터에 경우에는 smoothness 값이 0이기 때문에 default 값인 70으로 셋팅해줌.
                {
                    smoothness = 70.0;
                }
                switch (foreground)
                {
                    case EnumForegroundType.ANY:
                        foreGround = (int)MIL.M_ANY;
                        break;
                    case EnumForegroundType.FOREGROUND_WHITE:
                        foreGround = (int)MIL.M_FOREGROUND_WHITE;
                        break;
                    case EnumForegroundType.FOREGROUND_BLACK:
                        foreGround = (int)MIL.M_FOREGROUND_BLACK;
                        break;
                    default:
                        break;
                }
                switch (targettype)
                {
                    case EnumModelTargetType.Undefined:
                        throw new VisionException("ModelFind(): Undefined Target Type.");
                    case EnumModelTargetType.Circle:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_CIRCLE, foreGround,size.Width, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Ellipse:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_ELLIPSE, foreGround,size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Rectangle:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_RECTANGLE, foreGround,size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Square:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_SQUARE, foreGround,size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Cross:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_CROSS, foreGround,size.Width, size.Height, horthick, verthick);
                        break;
                    case EnumModelTargetType.Ring:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_RING, foreGround,size.Width, horthick, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Triangle:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_TRIANGLE, foreGround,size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Diamond:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_DIAMOND, foreGround,size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.DXF:
                        break;
                    default:
                        break;
                }

                // Set the smoothness.
                MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SMOOTHNESS, smoothness);
                // Set the search speed.
                MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SPEED, MIL.M_MEDIUM);
                // Set the search area.
                if (roiwidth != 0 && roiheight != 0)
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_POSITION_RANGE, MIL.M_ENABLE);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_X, posx);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_Y, posy);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_POS_X, roiwidth / 2);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_POS_Y, roiheight / 2);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_NEG_X, roiwidth / 2);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_NEG_Y, roiheight / 2);
                }
                else
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_POSITION_RANGE, MIL.M_DISABLE);
                }
                // Set the Scale range
                if (scale_max != 0 || scale_min != 0)
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_SCALE_RANGE, MIL.M_ENABLE);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_SCALE, 1);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_SCALE_MIN_FACTOR, scale_min);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_SCALE_MAX_FACTOR, scale_max);
                }
                else
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_SCALE_RANGE, MIL.M_DISABLE);
                }

                MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_ACCEPTANCE, acceptance);
                if(number == 0)
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, MIL.M_ALL);
                }
                else
                {
                    if(number > maxOccurrences)
                    {
                        MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, MIL.M_ALL);
                    }
                    else
                    {
                        MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, number);
                    }
                    
                }
                //MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_ACCURACY, MIL.M_HIGH);
                //MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_DETAIL_LEVEL, MIL.M_HIGH);

                // Preprocess the search context.
                MIL.MmodPreprocess(mMilSearchContext, MIL.M_DEFAULT);
                // Find the model.
                MIL.MmodFind(mMilSearchContext, milProcBuffer, mMilResult);
                // Get the number of models found.
                MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref mNumResults);

                numOfOccur = (int)mNumResults;
                double[] xPoss = new double[maxOccurrences];
                double[] yPoss = new double[maxOccurrences];
                double[] angles = new double[maxOccurrences];
                double[] scales = new double[maxOccurrences];
                double[] scores = new double[maxOccurrences];

                // If a model was found above the acceptance threshold.
                if ((numOfOccur >= 1))
                {
                    if (numOfOccur < maxOccurrences)
                    {
                        // Get the results of the single model.
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_INDEX + MIL.M_TYPE_MIL_INT, Model);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_POSITION_X, xPoss);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_POSITION_Y, yPoss);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_ANGLE, angles);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_SCALE, scales);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_SCORE, scores);

                        // Draw edges, position and box over the occurrences that were found.
                        for (int i = 0; i < numOfOccur; i++)
                        {
                            MIL.MgraColor(MIL.M_DEFAULT, ModelDrawColor);
                            MIL.MmodDraw(MIL.M_DEFAULT, mMilResult, milProcResultBuffer, MIL.M_DRAW_EDGES + MIL.M_DRAW_BOX + MIL.M_DRAW_POSITION, i, MIL.M_DEFAULT);

                            mfResults.Add(new ModelFinderResult(new CatCoordinates(xPoss[i], yPoss[i]), angles[i], scores[i], targettype));

                            ImageBuffer resultBuff = new ImageBuffer();
                            targetImage.CopyTo(resultBuff);
                            resultBuff.Buffer = new byte[targetImage.SizeX * targetImage.SizeY * 3];
                            MIL.MbufGet(milProcResultBuffer, resultBuff.Buffer);
                            mfResults.Last().ResultBuffer = resultBuff;

                            LoggerManager.Debug($"ModelFind({targettype}): Model found Index:{i} @(X: {xPoss[i]:0.00}, Y: {yPoss[i]:0.00}, Angle: {angles[i]:0.00}, Scale = {scales[i]:0.00}, Score = {scores[i]:0.0})", isInfo: IsInfo);

                            string resultImagePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, false, "\\MFImage\\", "MFResult");
                            SaveMilIdToImage(milProcResultBuffer, resultImagePath, IMAGE_LOG_TYPE.NORMAL, EventCodeEnum.NONE);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"ModelFind({targettype}): Too many occurrences. Occurrences = {numOfOccur}", isInfo: IsInfo);
                    }
                }

                // 해당 함수 상위에서 저장함.
                //if (numOfOccur < 1)
                //{
                //    string imagePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\", "TargetImage");
                //    SaveMilIdToImage(milProcBuffer, imagePath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                //    LoggerManager.Error($"ModelFind({targettype}): Fail Modelfinder find {numOfOccur} models. Saved Image Path: " + imagePath);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ModelFind({targettype}): Error occurred. Err = {err.Message}");
            }
            finally
            {
                if (milProcBuffer != null) MIL.MbufFree(milProcBuffer);
                if (milProcResultBuffer != null) MIL.MbufFree(milProcResultBuffer);
                if (mMilSearchContext != null) MIL.MmodFree(mMilSearchContext);
                if (mMilResult != null) MIL.MmodFree(mMilResult);
            }
            return mfResults;
        }
        public List<ModelFinderResult> ModelFind_For_Key(ImageBuffer targetImage,
            EnumModelTargetType targettype,
            EnumForegroundType foreground,
            Size size,
            int acceptance,
            double posx = 0, double posy = 0, double roiwidth = 0, double roiheight = 0,
            double scale_min = 0, double scale_max = 0,
            double horthick = 0, double verthick = 0, double smoothness = 70, int number = 0)
        {
            int maxOccurrences = 16;
            int foreGround = (int)MIL.M_ANY; ;
            List<ModelFinderResult> mfResults = new List<ModelFinderResult>();
            MIL_ID mMilImage = MIL.M_NULL;                               // Image buffer identifier.
            MIL_ID mMilSearchContext = MIL.M_NULL;                       // Search context.
            MIL_ID mMilResult = MIL.M_NULL;                              // Result identifier.
            double ModelDrawColor = MIL.M_COLOR_RED;                    // Model draw color.
            MIL_INT[] Model = new MIL_INT[maxOccurrences];               // Model index.
            MIL_INT mNumResults = 0;                                         // Number of results found.
            MIL_ID milProcResultBuffer = MIL.M_NULL;
            MIL_ID milProcBuffer = MIL.M_NULL;
            //MIL_ID milBinBuffer = MIL.M_NULL;
            //MIL_ID m_Context = MIL.M_NULL;
            int numOfOccur = 0;
            try
            {
                LoggerManager.Debug($"[ModuleVisionProcessing] ModelFind_For_Key() : Input Value : " +
                    $"targettype = {targettype}, " +
                    $"foreground = {foreground}" +
                    $"size = {size}" +
                    $"acceptance = {acceptance}" +
                    $"pos x, y = {posx:0.00}, {posy:0.00}" +
                    $"roiwidth, roiheight = {roiwidth}, {roiheight}" +
                    $"scale_min = {scale_min}" +
                    $"scale_max = {scale_max}" +
                    $"horthick = {horthick}" +
                    $"verthick = {verthick}" +
                    $"smoothness = {smoothness}" +
                    $"number = {number}" +
                    $"maxOccurrences = {maxOccurrences}", isInfo: IsInfo);

                if(posx == 0 && posy == 0)
                {
                    posx = targetImage.SizeX / 2;
                    posy = targetImage.SizeY / 2;
                }

                MIL.MbufAlloc2d(MilSystem, targetImage.SizeX, targetImage.SizeY,
                                 targetImage.ColorDept, iAttributes, ref milProcBuffer);

                //MIL.MbufAlloc2d(MilSystem, targetImage.SizeX, targetImage.SizeY,
                //                 targetImage.ColorDept, iAttributes, ref milBinBuffer);

                MIL.MbufAllocColor(MilSystem, 3, targetImage.SizeX, targetImage.SizeY,
                                                    8 + MIL.M_UNSIGNED, cl_iAttributes, ref milProcResultBuffer);

                if (targetImage.ColorDept == (int)ColorDept.BlackAndWhite) MIL.MbufPut(milProcBuffer, targetImage.Buffer);
                else if (targetImage.ColorDept == (int)ColorDept.Color24) ColorToBlackAndWrite(targetImage, ref milProcBuffer);

                MIL.MbufCopy(milProcBuffer, milProcResultBuffer);

                // Allocate a Geometric Model Finder context.
                MIL.MmodAlloc(MilSystem, MIL.M_GEOMETRIC, MIL.M_DEFAULT, ref mMilSearchContext);
                // Allocate a result buffer.
                MIL.MmodAllocResult(MilSystem, MIL.M_DEFAULT, ref mMilResult);
                // Define the model.
                if (horthick == 0)
                {
                    horthick = size.Width / 2d;
                }
                if (verthick == 0)
                {
                    verthick = size.Height / 2d;
                }
                if (smoothness <= 0.0)    // 기존 파라미터에 경우에는 smoothness 값이 0이기 때문에 default 값인 70으로 셋팅해줌.
                {
                    smoothness = 70.0;
                }
                switch (foreground)
                {
                    case EnumForegroundType.ANY:
                        foreGround = (int)MIL.M_ANY;
                        break;
                    case EnumForegroundType.FOREGROUND_WHITE:
                        foreGround = (int)MIL.M_FOREGROUND_WHITE;
                        break;
                    case EnumForegroundType.FOREGROUND_BLACK:
                        foreGround = (int)MIL.M_FOREGROUND_BLACK;
                        break;
                    default:
                        break;
                }
                switch (targettype)
                {
                    case EnumModelTargetType.Undefined:
                        throw new VisionException("ModelFind(): Undefined Target Type.");
                    case EnumModelTargetType.Circle:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_CIRCLE, foreGround, size.Width, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Ellipse:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_ELLIPSE, foreGround, size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Rectangle:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_RECTANGLE, foreGround, size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Square:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_SQUARE, foreGround, size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Cross:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_CROSS, foreGround, size.Width, size.Height, horthick, verthick);
                        break;
                    case EnumModelTargetType.Ring:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_RING, foreGround, size.Width, horthick, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Triangle:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_TRIANGLE, foreGround, size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.Diamond:
                        MIL.MmodDefine(mMilSearchContext, MIL.M_DIAMOND, foreGround, size.Width, size.Height, MIL.M_NULL, MIL.M_NULL);
                        break;
                    case EnumModelTargetType.DXF:
                        break;
                    default:
                        break;
                }

                // Set the smoothness.
                MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SMOOTHNESS, smoothness);
                // Set the search speed.
                MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SPEED, MIL.M_MEDIUM);
                // Set the search area.
                if (roiwidth != 0 && roiheight != 0)
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_POSITION_RANGE, MIL.M_ENABLE);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_X, posx);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_Y, posy);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_POS_X, roiwidth / 2);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_POS_Y, roiheight / 2);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_NEG_X, roiwidth / 2);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_POSITION_DELTA_NEG_Y, roiheight / 2);
                }
                else
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_POSITION_RANGE, MIL.M_DISABLE);
                }
                // Set the Scale range
                if (scale_max != 0 || scale_min != 0)
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_SCALE_RANGE, MIL.M_ENABLE);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_SCALE, 1);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_SCALE_MIN_FACTOR, scale_min);
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_SCALE_MAX_FACTOR, scale_max);
                }
                else
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_SCALE_RANGE, MIL.M_DISABLE);
                }

                MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_ACCEPTANCE, acceptance);
                if (number == 0)
                {
                    MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, MIL.M_ALL);
                }
                else
                {
                    if (number > maxOccurrences)
                    {
                        MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, MIL.M_ALL);
                    }
                    else
                    {
                        MIL.MmodControl(mMilSearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, number);
                    }

                }

                // Preprocess the search context.
                MIL.MmodPreprocess(mMilSearchContext, MIL.M_DEFAULT);
                // Find the model.
                MIL.MmodFind(mMilSearchContext, milProcBuffer, mMilResult);

                // Get the number of models found.
                MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref mNumResults);

                numOfOccur = (int)mNumResults;
                double[] xPoss = new double[maxOccurrences];
                double[] yPoss = new double[maxOccurrences];
                double[] angles = new double[maxOccurrences];
                double[] scales = new double[maxOccurrences];
                double[] scores = new double[maxOccurrences];
                double[] modelWidth = new double[maxOccurrences];
                double[] modelHeight = new double[maxOccurrences];

                double[] box_x_min = new double[maxOccurrences];
                double[] box_y_min = new double[maxOccurrences];
                double[] box_x_max = new double[maxOccurrences];
                double[] box_y_max = new double[maxOccurrences];

                double[] averagePixelValue = new double[maxOccurrences];
                // If a model was found above the acceptance threshold.
                if (numOfOccur >= 1)
                {
                    if (numOfOccur < maxOccurrences)
                    {
                        // Get the results of the single model.
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_INDEX + MIL.M_TYPE_MIL_INT, Model);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_POSITION_X, xPoss);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_POSITION_Y, yPoss);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_WIDTH, modelWidth);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_HEIGHT, modelHeight);

                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_ANGLE, angles);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_SCALE, scales);
                        MIL.MmodGetResult(mMilResult, MIL.M_DEFAULT, MIL.M_SCORE, scores);

                        byte[] calcPixelValueTargetBuffer = new byte[targetImage.SizeX * targetImage.SizeY];
                        MIL.MbufGet(milProcBuffer, calcPixelValueTargetBuffer);

                        for (int index = 0; index < numOfOccur; index++)
                        {
                            int startX = (int)(xPoss[index] - (modelWidth[index] / 2));
                            int endX = (int)(xPoss[index] + modelWidth[index] / 2);
                            int startY = (int)(yPoss[index] - modelHeight[index] / 2);
                            int endY = (int)(yPoss[index] + modelHeight[index] / 2);
                            double pixelSum = 0;
                            int pixelCount = 0;

                            for (int y = startY; y <= endY; y++)
                            {
                                int rowIndex = y * targetImage.SizeX;

                                for (int x = startX; x <= endX; x++)
                                {
                                    int pixelOffset = rowIndex + x;

                                    pixelSum += calcPixelValueTargetBuffer[pixelOffset];
                                    pixelCount++;
                                }
                            }
                            averagePixelValue[index] = pixelSum / pixelCount;
                        }

                        // Draw edges, position and box over the occurrences that were found.
                        for (int i = 0; i < numOfOccur; i++)
                        {
                            MIL.MgraColor(MIL.M_DEFAULT, ModelDrawColor);
                            MIL.MmodDraw(MIL.M_DEFAULT, mMilResult, milProcResultBuffer, MIL.M_DRAW_EDGES + MIL.M_DRAW_BOX + MIL.M_DRAW_POSITION, i, MIL.M_DEFAULT);

                            mfResults.Add(new ModelFinderResult(new CatCoordinates(xPoss[i], yPoss[i]), angles[i], scores[i], targettype)  { AvgPixelValue = averagePixelValue[i]});
                            
                            ImageBuffer resultBuff = new ImageBuffer();
                            targetImage.CopyTo(resultBuff);
                            resultBuff.Buffer = new byte[targetImage.SizeX * targetImage.SizeY * 3];
                            MIL.MbufGet(milProcResultBuffer, resultBuff.Buffer);
                            mfResults[i].Width = modelWidth[i];
                            mfResults[i].Height = modelHeight[i];
                            mfResults.Last().ResultBuffer = resultBuff;

                            LoggerManager.Debug($"ModelFind({targettype}): Model found Index:{i} @(X: {xPoss[i]:0.00}, Y: {yPoss[i]:0.00}, Width: {modelWidth[i]}, Height: {modelHeight[i]}, Angle: {angles[i]:0.00}, Scale = {scales[i]:0.00}, Score = {scores[i]:0.0})", isInfo: IsInfo);
                        }

                        var result = from model in mfResults.ToList()
                                     orderby model.AvgPixelValue descending
                                     select model;
                        if(result.Count() == numOfOccur)
                        {
                            if (foreground == EnumForegroundType.ANY | foreground == EnumForegroundType.FOREGROUND_WHITE)
                            {
                                mfResults.Clear();
                                var dev = result.FirstOrDefault();
                                if(dev.AvgPixelValue > 90)
                                {
                                    mfResults.Add(dev);
                                    LoggerManager.Debug($"ModelFind({targettype}): Average pixel value: {dev.AvgPixelValue:0.00} foreground: {foreground}", isInfo: IsInfo);
                                }
                                else
                                {
                                    LoggerManager.Debug($"ModelFind({targettype}): The luminance of the target is too low.  Average pixel value: {dev.AvgPixelValue:0.00},  foreground: {foreground}", isInfo: IsInfo);
                                }
                            }
                            else
                            {
                                mfResults.Clear();
                                var dev = result.LastOrDefault();
                                if (dev.AvgPixelValue < 90)
                                {
                                    mfResults.Add(dev);
                                    LoggerManager.Debug($"ModelFind({targettype}): Average pixel value: {dev.AvgPixelValue:0.00} foreground: {foreground}", isInfo: IsInfo);
                                }
                                else
                                {
                                    LoggerManager.Debug($"ModelFind({targettype}): The luminance of the target is too high.  Average pixel value: {dev.AvgPixelValue:0.00},  foreground: {foreground}", isInfo: IsInfo);
                                }
                            }
                        }
                        var findedModel = mfResults.FirstOrDefault();
                        if(findedModel != null)
                        {
                            MIL.MgraText(MIL.M_DEFAULT, milProcResultBuffer,
                                                findedModel.Position.GetX() - 32, findedModel.Position.GetY() + findedModel.Height / 2 + 8, "SELECTED");
                            MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_WHITE);
                            MIL.MgraRect(MIL.M_DEFAULT, milProcResultBuffer, (targetImage.SizeX / 2) - (roiwidth / 2), (targetImage.SizeY / 2) - (roiheight / 2), (targetImage.SizeX / 2) + (roiwidth / 2), (targetImage.SizeY / 2) + (roiheight / 2));

                            MIL.MbufGet(milProcResultBuffer, findedModel.ResultBuffer.Buffer);
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"ModelFind({targettype}): Too many occurrences. Occurrences = {numOfOccur}", isInfo: IsInfo);
                    }
                }

                // 해당 함수 상위에서 저장함.
                //if (numOfOccur < 1)
                //{
                //    string imagePath = this.FileManager().GetImageSaveFullPath(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\", "TargetImage");
                //    SaveMilIdToImage(milProcBuffer, imagePath, IMAGE_LOG_TYPE.FAIL, EventCodeEnum.NONE);

                //    LoggerManager.Error($"ModelFind({targettype}): Fail Modelfinder find {numOfOccur} models. Saved Image Path: " + imagePath);
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"ModelFind({targettype}): Error occurred. Err = {err.Message}");
            }
            finally
            {
                if (milProcBuffer != null) MIL.MbufFree(milProcBuffer);
                //if (milBinBuffer != null) MIL.MbufFree(milBinBuffer);
                if (milProcResultBuffer != null) MIL.MbufFree(milProcResultBuffer);
                if (mMilSearchContext != null) MIL.MmodFree(mMilSearchContext);
                if (mMilResult != null) MIL.MmodFree(mMilResult);
            }
            return mfResults;
        }


        public ImageBuffer DrawCrosshair(ImageBuffer grabbedImage, Point pt, int length, int thickness = 1)
        {
            ImageBuffer retVal;
            MIL_ID resultID = MIL.M_NULL;

            int width = grabbedImage.SizeX;
            int height = grabbedImage.SizeY;

            try
            {
                BlackAndWhiteToColor(grabbedImage, ref resultID, true);

                MIL.MgraColor(MIL.M_DEFAULT, MIL.M_COLOR_GREEN);

                // 가로 선 그리기 (사각형으로 표현)
                MIL.MgraRect(MIL.M_DEFAULT, resultID, pt.X - length, pt.Y - thickness / 2, pt.X + length, pt.Y + thickness / 2);

                // 세로 선 그리기 (사각형으로 표현)
                MIL.MgraRect(MIL.M_DEFAULT, resultID, pt.X - thickness / 2, pt.Y - length, pt.X + thickness / 2, pt.Y + length);

                byte[] drawingChildBufferArr = new byte[width * height * 3];
                MIL.MbufGetColor(resultID, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, drawingChildBufferArr);

                retVal = new ImageBuffer(drawingChildBufferArr, width, height, 3, (int)ColorDept.Color24);
            }
            catch (MILException err)
            {
                throw new VisionException("AddEdgePosBuffer Error", err, EventCodeEnum.VISION_PROC_EXCEPTION, this);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message, err);
            }
            finally
            {
                if (resultID != MIL.M_NULL)
                {
                    MIL.MbufFree(resultID);
                    resultID = MIL.M_NULL;
                }
            }

            return retVal;
        }
        public ImageBuffer CombineImages(ImageBuffer[] images, int width, int height, int rows, int columns)
        {
            ImageBuffer resultImageBuffer = null;
            MIL_ID combinedImage = MIL.M_NULL;
            int numImages = images.Length;
            MIL_ID[] imageBuffers = new MIL_ID[numImages];

            try
            {
                if (numImages > 0)
                {
                    int combinedWidth = width * columns;
                    int combinedHeight = height * rows;

                    // 결과 이미지를 할당합니다.
                    int colorDepth = images[0].ColorDept;
                    if (colorDepth == (int)ColorDept.BlackAndWhite)
                    {
                        MIL.MbufAlloc2d(MilSystem, combinedWidth, combinedHeight, colorDepth, iAttributes, ref combinedImage);
                    }
                    else if (colorDepth == (int)ColorDept.Color24 || colorDepth == (int)ColorDept.Color32)
                    {
                        MIL.MbufAllocColor(MilSystem, 3, combinedWidth, combinedHeight, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref combinedImage);
                    }

                    // 각 이미지를 합칩니다.
                    for (int i = 0; i < numImages; i++)
                    {
                        int currentRow = i / columns;
                        int currentColumn = i % columns;
                        int xOffset = currentColumn * width;
                        int yOffset = currentRow * height;

                        // 각 이미지 버퍼를 할당합니다.
                        if (colorDepth == (int)ColorDept.BlackAndWhite)
                        {
                            MIL.MbufAlloc2d(MilSystem, width, height, colorDepth, iAttributes, ref imageBuffers[i]);
                            MIL.MbufPut(imageBuffers[i], images[i].Buffer);
                        }
                        else if (colorDepth == (int)ColorDept.Color24 || colorDepth == (int)ColorDept.Color32)
                        {
                            MIL.MbufAllocColor(MilSystem, 3, width, height, 8 + MIL.M_UNSIGNED, cl_iAttributes, ref imageBuffers[i]);
                            MIL.MbufPutColor(imageBuffers[i], MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, images[i].Buffer);
                        }

                        MIL.MbufCopyClip(imageBuffers[i], combinedImage, xOffset, yOffset);
                    }

                    // 결합된 이미지를 ImageBuffer로 변환합니다.
                    if (colorDepth == (int)ColorDept.BlackAndWhite)
                    {
                        byte[] combinedBuffer = new byte[combinedWidth * combinedHeight];
                        MIL.MbufGet(combinedImage, combinedBuffer);
                        resultImageBuffer = new ImageBuffer(combinedBuffer, combinedWidth, combinedHeight, 1, colorDepth);
                    }
                    else if (colorDepth == (int)ColorDept.Color24 || colorDepth == (int)ColorDept.Color32)
                    {
                        byte[] combinedBuffer = new byte[combinedWidth * combinedHeight * 3];
                        MIL.MbufGetColor(combinedImage, MIL.M_RGB24 + MIL.M_PACKED, MIL.M_ALL_BANDS, combinedBuffer);
                        resultImageBuffer = new ImageBuffer(combinedBuffer, combinedWidth, combinedHeight, 3, colorDepth);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (combinedImage != MIL.M_NULL)
                {
                    MIL.MbufFree(combinedImage);
                }

                foreach (var buffer in imageBuffers)
                {
                    if (buffer != MIL.M_NULL)
                    {
                        MIL.MbufFree(buffer);
                    }
                }
            }

            return resultImageBuffer;
        }

    }
}

