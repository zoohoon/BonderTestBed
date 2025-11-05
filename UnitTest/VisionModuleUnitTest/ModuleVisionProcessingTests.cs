using Vision.ProcessingModule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Matrox.MatroxImagingLibrary;
using VisionModuleUnitTests;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProberInterfaces;
using ProberInterfaces.Vision;

namespace Vision.ProcessingModule.Tests
{
    [TestClass()]
    public class ModuleVisionProcessingTests
    {
        /// <summary>
        /// 라이선스 키 정상일때 OK
        /// </summary>
        [TestMethod()]
        public void IsValidModLicenseOnTest()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            using (ModuleVisionProcessing module = new ModuleVisionProcessing())
            {
                module.InitProcessing((int)sys.Id);
                Assert.AreEqual(true, module.IsValidModLicense());
            }
        }

        /// <summary>
        /// 라이선스키 연결 안했을때 OK , 제대로 에러가 발생하는지를 확인하는 테스트
        /// </summary>
        [TestMethod()]
        public void IsValidModLicenseOffTest()
        {
            using (var app = new MilApp(false))
            using (var sys = new MilSys(false))
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);
                if ((LicenseModules & MIL.M_LICENSE_IM) != 0)
                {
                    Assert.Inconclusive("MIL 라이선스를 비활성화 해야 합니다. ");
                }

                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);
                    Assert.AreEqual(false, module.IsValidModLicense());
                }
            }
        }


        [TestMethod()]
        public void FindBlobTest()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);
                if ((LicenseModules & MIL.M_LICENSE_IM) != 0)
                {
                    Assert.Inconclusive("MIL 라이선스를 비활성화 해야 합니다. ");
                }

                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);

                    ImageBuffer grabbuffer = new ImageBuffer();

                    string filename = string.Empty;
                    string folderPath = @"C:\Logs\Test\";

                    // 폴더 내 모든 파일 가져오기
                    string[] files = Directory.GetFiles(folderPath);

                    double PosX = 0;
                    double PosY = 0;
                    int Threshold = 0;
                    int BlobAreaLow = 0;
                    int BlobAreaHigh = 0;
                    int OffsetX = 0;
                    int OffsetY = 0;
                    int SizeX = 0;
                    int SizeY = 0;
                    double BlobSizeX = 0;
                    double BlobSizeY = 0;
                    bool isDilation = false;
                    int BlobReq = 0;

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        module.FindBlob(grabbuffer, ref PosX, ref PosY, Threshold, BlobAreaLow, BlobAreaHigh, OffsetX, OffsetY, SizeX, SizeY, BlobSizeX, BlobSizeY, isDilation, BlobReq);
                    }
                }
            }
        }

        [TestMethod()]
        public void GetFocusValueTest()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);

                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);

                    ProberInterfaces.ImageBuffer grabbuffer = new ProberInterfaces.ImageBuffer();

                    string filename = string.Empty;

                    string folderPath = @"C:\ProberSystem\FocusImages\C01\1\";

                    // 폴더 내 모든 파일 가져오기
                    string[] files = Directory.GetFiles(folderPath);

                    List<int> FocusLevelValue_960 = new List<int>();
                    List<int> FocusLevelValue_480 = new List<int>();
                    List<int> FocusLevelValue_360 = new List<int>();
                    List<int> FocusLevelValue_240 = new List<int>();
                    List<int> FocusLevelValue_120 = new List<int>();

                    // (1). 960 x 960
                    Rect roi = new Rect(0, 0, 960, 960);

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        int focusval = module.GetFocusValue(grabbuffer, roi);
                        grabbuffer.FocusLevelValue = focusval;

                        FocusLevelValue_960.Add(grabbuffer.FocusLevelValue);
                    }

                    // (2). 480 x 480
                    roi = new Rect(240, 240, 480, 480);

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        int focusval = module.GetFocusValue(grabbuffer, roi);
                        grabbuffer.FocusLevelValue = focusval;

                        FocusLevelValue_480.Add(grabbuffer.FocusLevelValue);
                    }

                    // (3). 360 x 360
                    roi = new Rect(300, 300, 360, 360);

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        int focusval = module.GetFocusValue(grabbuffer, roi);
                        grabbuffer.FocusLevelValue = focusval;

                        FocusLevelValue_360.Add(grabbuffer.FocusLevelValue);
                    }

                    // (4). 240 x 240
                    roi = new Rect(360, 360, 240, 240);

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        int focusval = module.GetFocusValue(grabbuffer, roi);
                        grabbuffer.FocusLevelValue = focusval;

                        FocusLevelValue_240.Add(grabbuffer.FocusLevelValue);
                    }

                    // (5). 120 x 120
                    roi = new Rect(420, 420, 120, 120);

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        int focusval = module.GetFocusValue(grabbuffer, roi);
                        grabbuffer.FocusLevelValue = focusval;

                        FocusLevelValue_120.Add(grabbuffer.FocusLevelValue);
                    }

                    int i = 0;

                    string pattern_H = @"Height_(-?\d+\.\d+)";  // Regex pattern to match the desired value
                    string pattern_V = @"Value_(\d+)";  // Regex pattern to match the desired value

                    string height = string.Empty;
                    string value = string.Empty;

                    // Write
                    foreach (string filePath in files)
                    {
                        filename = Path.GetFileNameWithoutExtension(filePath);

                        Match match = Regex.Match(filename, pattern_H);

                        if (match.Success)
                        {
                            height = match.Groups[1].Value;
                        }

                        match = Regex.Match(filename, pattern_V);

                        if (match.Success)
                        {
                            value = match.Groups[1].Value;
                        }

                        Debug.WriteLine($"{filename}\t{height}\t{value}\t{FocusLevelValue_960[i]}\t{FocusLevelValue_480[i]}\t{FocusLevelValue_360[i]}\t{FocusLevelValue_240[i]}\t{FocusLevelValue_120[i]}");
                        i++;
                    }
                }
            }
        }

        [TestMethod()]
        public void EdgeFind_IndexAlignTest()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);

                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);

                    ImageBuffer grabbuffer = new ImageBuffer();

                    string filename = string.Empty;

                    string folderPath = @"C:\ProberSystem\EmulImages\WaferAlign\IndexAlignEdge\";

                    int Cpos = 0;
                    int RWidth = 5;
                    int Threshold = 20;

                    string[] files = Directory.GetFiles(folderPath);

                    foreach (string filePath in files)
                    {
                        grabbuffer = module.LoadImageFile(filePath);

                        var retval = module.EdgeFind_IndexAlign(grabbuffer, Cpos, RWidth, Threshold);
                    }
                }
            }
        }

        [TestMethod()]
        public void ModelFindTest()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);

                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);

                    ImageBuffer grabbuffer = new ImageBuffer();

                    string filename = string.Empty;

                    string folderPath = @"C:\Logs\Test\src_image";

                    string OutputfolderPath = @"C:\Logs\Test\Model Find";

                    DirectoryInfo directory = new DirectoryInfo(folderPath);
                    FileInfo[] files = directory.GetFiles();

                    foreach (FileInfo filePath in files)
                    {
                        string file_name = new DirectoryInfo(filePath.ToString()).Name;
                        grabbuffer = module.LoadImageFile(filePath.FullName);

                        List<LightValueParam> lightValueParams = new List<LightValueParam>();
                        MFParameter mFParameters = new MFParameter(62, 115, lightValueParams);

                        var baseResults = module.ModelFind_For_Key(targetImage: grabbuffer,
                                                                                         targettype: mFParameters.ModelTargetType.Value,
                                                                                         foreground: mFParameters.ForegroundType.Value,
                                                                                         size: new Size(mFParameters.ModelWidth.Value, mFParameters.ModelHeight.Value),
                                                                                         acceptance: mFParameters.Acceptance.Value,
                                                                                         scale_min: mFParameters.ScaleMin.Value,
                                                                                         scale_max: mFParameters.ScaleMax.Value,
                                                                                         smoothness: mFParameters.Smoothness.Value,
                                                                                         number: 0);

                        if (baseResults.Count > 0)
                        {
                            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;

                            int c = 0;

                            foreach (var result in baseResults)
                            {
                                var _grabedImage = result.ResultBuffer;

                                using (var dstImageBuf = new Milbuf2d(sys, _grabedImage.SizeX, _grabedImage.SizeY, 8, Attributes))
                                {
                                    MIL.MbufPut(dstImageBuf, _grabedImage.Buffer);

                                    c++;

                                    MIL.MbufExport(OutputfolderPath + "\\" + $"{file_name}_Result_{c}.bmp", MIL.M_BMP, dstImageBuf);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod()]
        public void FindBlobWithRectangularityRectanglarityBlobTest()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);

                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);

                    string filename = string.Empty;

                    string folderPath = @"C:\Logs\Test\Originals 2024-01-16 11-45-58";

                    string OutputfolderPath = @"C:\Logs\Test\Originals 2024-01-16 11-45-58\KeyBlob_Result";

                    //string[] files = Directory.GetFiles(folderPath);
                    DirectoryInfo directory = new DirectoryInfo(folderPath);
                    FileInfo[] files = directory.GetFiles();
                    ImageBuffer grabbuffer = new ImageBuffer();
                    double PosX = 0;
                    double PosY = 0;
                    int Threshold = 0;
                    int BlobAreaLow = 3629;
                    int BlobAreaHigh = 58064;
                    int OffsetX = 360;
                    int OffsetY = 336;
                    int SizeX = 241;
                    int SizeY = 337;
                    double BlobSizeX = 69;
                    double BlobSizeY = 115;
                    bool isDilation = false;
                    double BlobSizeXMinimumMargin = 0.4;
                    double BlobSizeXMaximumMargin = 0.4;
                    double BlobSizeYMinimumMargin = 0.4;
                    double BlobSizeYMaximumMargin = 0.4;
                    double minRectangularity = 0.8;
                    int c = 0;
                    foreach (FileInfo filePath in files)
                    {
                        string file_name = new DirectoryInfo(filePath.ToString()).Name;
                        grabbuffer = module.LoadImageFile(filePath.FullName);

                        var blobResult = module.FindBlobWithRectangularity(grabbuffer,
                                                    ref PosX,
                                                    ref PosY,
                                                    Threshold,
                                                    BlobAreaLow,
                                                    BlobAreaHigh,
                                                    OffsetX,
                                                    OffsetY,
                                                    SizeX,
                                                    SizeY,
                                                    BlobSizeX,
                                                    BlobSizeY,
                                                    isDilation,
                                                    BlobSizeXMinimumMargin,
                                                    BlobSizeXMaximumMargin,
                                                    BlobSizeYMinimumMargin,
                                                    BlobSizeYMaximumMargin,
                                                    minRectangularity);

                        if (blobResult?.DevicePositions?.Count == 1)
                        {
                            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;

                            using (var dstImageBuf = new Milbuf2d(sys, blobResult.ResultBuffer.SizeX, blobResult.ResultBuffer.SizeY, 8, Attributes))
                            {
                                MIL.MbufPut(dstImageBuf, blobResult.ResultBuffer.Buffer);
                                c++;
                                MIL.MbufExport(OutputfolderPath + "\\" + $"{file_name}_Result_{c}.bmp", MIL.M_BMP, dstImageBuf);
                            }
                        }

                    }
                }
            }
        }

        [TestMethod()]
        public void FindModelWithChild()
        {
            using (var app = new MilApp())
            using (var sys = new MilSys())
            {
                MIL_INT LicenseModules = MIL.M_NULL;
                MIL.MappInquire(app, MIL.M_LICENSE_MODULES, ref LicenseModules);
                using (ModuleVisionProcessing module = new ModuleVisionProcessing())
                {
                    module.InitProcessing((int)sys.Id);

                    ImageBuffer grabbuffer = new ImageBuffer();
                    MIL_ID milProcResultBuffer = MIL.M_NULL;

                    string folderPath = @"C:\Logs\Test\src_image";
                    string OutputfolderPath = @"C:\Logs\Test\ret_image";

                    DirectoryInfo directory = new DirectoryInfo(folderPath);
                    FileInfo[] files = directory.GetFiles();

                    foreach (FileInfo filePath in files)
                    {
                        string file_name = new DirectoryInfo(filePath.ToString()).Name;
                        grabbuffer = module.LoadImageFile(filePath.FullName);
                            
                        List<LightValueParam> lightValueParams = new List<LightValueParam>();
                        lightValueParams.Add(new LightValueParam(EnumLightType.COAXIAL, 100));
                        lightValueParams.Add(new LightValueParam(EnumLightType.OBLIQUE, 0));
                        MFParameter mFParameters = new MFParameter(185, 185, lightValueParams);

                        var baseResults = module.ModelFind(targetImage: grabbuffer,
                                                           targettype: mFParameters.ModelTargetType.Value,
                                                           foreground: mFParameters.ForegroundType.Value,
                                                           size: new Size(mFParameters.ModelWidth.Value, mFParameters.ModelHeight.Value),
                                                           acceptance: mFParameters.Acceptance.Value,
                                                           scale_min: mFParameters.ScaleMin.Value,
                                                           scale_max: mFParameters.ScaleMax.Value,
                                                           smoothness: mFParameters.Smoothness.Value,
                                                           number: 1);

                        if (baseResults.Count > 0)
                        {
                            List<ModelFinderResult> childResults = new List<ModelFinderResult>();
                            List<LightValueParam> childlightValueParams = new List<LightValueParam>();
                            childlightValueParams.Add(new LightValueParam(EnumLightType.COAXIAL, 100));
                            childlightValueParams.Add(new LightValueParam(EnumLightType.OBLIQUE, 0));
                            MFParameter childmFParameters = new MFParameter(38, 38, childlightValueParams, EnumModelTargetType.Circle);

                            childResults = module.ModelFind(
                                    targetImage: grabbuffer,
                                    targettype: childmFParameters.ModelTargetType.Value,
                                    foreground: childmFParameters.ForegroundType.Value,
                                    size: new Size(childmFParameters.ModelWidth.Value, childmFParameters.ModelHeight.Value),
                                    acceptance: childmFParameters.Acceptance.Value,
                                    posx: baseResults[0].Position.X.Value,
                                    posy: baseResults[0].Position.Y.Value,
                                    roiwidth: mFParameters.ModelWidth.Value,                                    
                                    roiheight: mFParameters.ModelHeight.Value,
                                    scale_min: childmFParameters.ScaleMin.Value,
                                    scale_max: childmFParameters.ScaleMax.Value,
                                    smoothness: childmFParameters.Smoothness.Value,
                                    number: 1);

                            long Attributes = MIL.M_IMAGE + MIL.M_DISP + MIL.M_PROC;                            

                            int c = 0;                            

                            foreach (var result in baseResults)
                            {
                                var _grabedImage = result.ResultBuffer;

                                using (var dstImageBuf = new Milbuf2d(sys, _grabedImage.SizeX, _grabedImage.SizeY, 8, Attributes))
                                {                                    
                                    MIL.MbufPut(dstImageBuf, _grabedImage.Buffer);
                                    c++;
                                    MIL.MbufExport(OutputfolderPath + "\\" + $"{file_name}_Result_{c}.bmp", MIL.M_BMP, dstImageBuf);                                    
                                }
                            }

                            foreach (var result in childResults)
                            {
                                var _grabedImage = result.ResultBuffer;

                                using (var dstImageBuf = new Milbuf2d(sys, _grabedImage.SizeX, _grabedImage.SizeY, 8, Attributes))
                                {
                                    MIL.MbufPut(dstImageBuf, _grabedImage.Buffer);
                                    c++;
                                    MIL.MbufExport(OutputfolderPath + "\\" + $"{file_name}_Result_child_{c}.bmp", MIL.M_BMP, dstImageBuf);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}