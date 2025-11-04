using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProberVision
{
    public class ProberImageController : IFactoryModule
    {
        public List<ModuleImageCollection> ModuleImageCollections = new List<ModuleImageCollection>();

        public EnumProberModule? CurrentProberModule { get; set; }
        public EventCodeEnum ConnectedNotifyCode { get; set; }
        public string LastHashCode = string.Empty;
        public string LastModuleStartTime = string.Empty;

        private readonly object _lock = new object();

        public ProberImageController()
        {
            InitModuleImageDataList();
        }

        public void InitModuleImageDataList()
        {
            try
            {
                EnumProberModule[] modules = (EnumProberModule[])Enum.GetValues(typeof(EnumProberModule));

                foreach (var module in modules)
                {
                    // 각 모듈에 대한 ModuleImageData 객체를 생성하여 리스트에 추가
                    ModuleImageCollections.Add(new ModuleImageCollection(module));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StartImageCollection(EnumProberModule? module, EventCodeEnum notifycode)
        {
            try
            {
                CurrentProberModule = module;
                ConnectedNotifyCode = notifycode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void EndImageCollection()
        {
            CurrentProberModule = null;
        }

        public ModuleImageCollection GetModule(EnumProberModule? enumProberModule = null)
        {
            ModuleImageCollection retval = null;

            try
            {
                if (ModuleImageCollections != null)
                {
                    if (CurrentProberModule != null)
                    {
                        retval = ModuleImageCollections.FirstOrDefault(m => m.ModuleType == CurrentProberModule);
                    }
                    else
                    {
                        if(enumProberModule != null)
                        {
                            retval = ModuleImageCollections.FirstOrDefault(m => m.ModuleType == enumProberModule);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ImageDataSet GetImageDataSet(EnumProberModule moduletype, string startTime, string hashCode = "")
        {
            ImageDataSet retval = null;

            try
            {
                lock (_lock)
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], GetImageDataSet() : moduletype = {moduletype}, startTime = {startTime}, hashCode = {hashCode}");

                    if (string.IsNullOrEmpty(hashCode) == false)
                    {
                        // hashCode가 존재하는 경우, 선행적으로 데이터를 찾아본다.
                        foreach (var module in ModuleImageCollections)
                        {
                            foreach (var dataSet in module.ImageDataSetList)
                            {
                                if (dataSet.ImageDatasHashCode == hashCode)
                                {
                                    retval = dataSet;
                                }
                            }
                        }
                    }

                    if (retval == null && !string.IsNullOrEmpty(startTime))
                    {
                        // If no dataset is found, create a new one using startTime
                        retval = MakeImageDataSet(moduletype, startTime);

                        LoggerManager.Debug($"[{this.GetType().Name}], GetImageDataSet() : Called MakeImageDataSet");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ImageDataSet MakeImageDataSet(EnumProberModule moduleType, string startTime)
        {
            var retval = new ImageDataSet();

            try
            {
                retval.ModuleStartTime = startTime;

                string basePath = this.FileManager().GetImageSavePath(moduleType);
                string startTimePath = Path.Combine(basePath, startTime);

                if (Directory.Exists(startTimePath))
                {
                    // Dictionary to keep track of folder name counts
                    var folderNameCounts = new Dictionary<string, int>();

                    // Loop through each folder in the startTimePath
                    var processingTypeDirectories = Directory.GetDirectories(startTimePath);

                    foreach (var processingTypeDir in processingTypeDirectories)
                    {
                        var baseProcessingType = Path.GetFileName(processingTypeDir);
                        var uniqueProcessingType = baseProcessingType;

                        if (folderNameCounts.ContainsKey(baseProcessingType))
                        {
                            folderNameCounts[baseProcessingType]++;
                            uniqueProcessingType = $"{baseProcessingType} ({folderNameCounts[baseProcessingType]})";
                        }
                        else
                        {
                            folderNameCounts[baseProcessingType] = 0;
                        }

                        // Extract the meaningful part from baseProcessingType
                        string cleanProcessingType = baseProcessingType.Split('(')[0].Trim();

                        var imageData = new ImageData((IMAGE_PROCESSING_TYPE)Enum.Parse(typeof(IMAGE_PROCESSING_TYPE), cleanProcessingType))
                        {
                            IMAGE_PROCESSING_TYPE = uniqueProcessingType // Store the unique processing type in ImageData
                        };

                        // Get all image files in the processingTypeDir with specified extensions
                        var imageFiles = Directory.GetFiles(processingTypeDir, "*.*")
                                                  .Where(f => f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                                              f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                              f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                              f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                                                  .ToList();

                        foreach (var imageFile in imageFiles)
                        {
                            var fileName = Path.GetFileName(imageFile);
                            var fileInfo = new FileInfo(imageFile);

                            // Assuming file name format is correct: yyMMddHHmmss_LOGTYPE_EVENTCODE.extension
                            var fileParts = fileName.Split('_');
                            if (fileParts.Length >= 3)
                            {
                                var capturedTime = DateTime.ParseExact(fileParts[0], "yyMMddHHmmss", null);
                                var logType = (IMAGE_LOG_TYPE)Enum.Parse(typeof(IMAGE_LOG_TYPE), fileParts[1]);

                                // Join all parts after the log type for the event code, remove the extension from the last part
                                var eventCodeStr = string.Join("_", fileParts.Skip(2)).Replace(Path.GetExtension(fileName), "");
                                var eventCode = (EventCodeEnum)Enum.Parse(typeof(EventCodeEnum), eventCodeStr);

                                // Determine the image save type based on the file extension
                                IMAGE_SAVE_TYPE saveType;
                                switch (Path.GetExtension(imageFile).ToLower())
                                {
                                    case ".bmp":
                                        saveType = IMAGE_SAVE_TYPE.BMP;
                                        break;
                                    case ".jpeg":
                                    case ".jpg":
                                        saveType = IMAGE_SAVE_TYPE.JPEG;
                                        break;
                                    case ".png":
                                        saveType = IMAGE_SAVE_TYPE.PNG;
                                        break;
                                    default:
                                        throw new NotSupportedException("Unsupported file type");
                                }

                                // Load and resize the image buffer data to 960x960 and get raw pixel data
                                byte[] buffer = GetResizedImageBuffer(imageFile, 960, 960);

                                var imageBuffer = new ImageBuffer
                                {
                                    Buffer = buffer,
                                    CapturedTime = capturedTime
                                };

                                var imageBufferData = new ImageBufferData(imageBuffer, logType, saveType, eventCode)
                                {
                                    FileName = fileName,
                                    SavePath = imageFile
                                };

                                imageData.ImageBufferDataCollection.Add(imageBufferData);
                            }
                        }

                        // Add the ImageData to the ImageDataSet
                        retval.ImageDataCollection.Add(imageData);
                    }
                }

                return retval;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private byte[] GetResizedImageBuffer(string imagePath, int width, int height)
        {
            using (var originalBitmap = new Bitmap(imagePath))
            {
                PixelFormat format = originalBitmap.PixelFormat;

                // Create a new bitmap with the desired size and original format
                using (var resizedBitmap = new Bitmap(width, height, format))
                {
                    BitmapData originalData = null;
                    BitmapData resizedData = null;
                    try
                    {
                        // Lock bits for both original and resized bitmaps
                        originalData = originalBitmap.LockBits(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                                                                ImageLockMode.ReadOnly, format);
                        resizedData = resizedBitmap.LockBits(new Rectangle(0, 0, width, height),
                                                              ImageLockMode.WriteOnly, format);

                        // Calculate the scaling factors
                        float xScale = (float)originalBitmap.Width / width;
                        float yScale = (float)originalBitmap.Height / height;

                        int bytesPerPixel = Image.GetPixelFormatSize(format) / 8;

                        // Create arrays for pixel data
                        byte[] originalPixels = new byte[originalData.Stride * originalData.Height];
                        byte[] resizedPixels = new byte[resizedData.Stride * resizedData.Height];

                        // Copy original bitmap data to the array
                        System.Runtime.InteropServices.Marshal.Copy(originalData.Scan0, originalPixels, 0, originalPixels.Length);

                        // Resize image manually
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                // Find the corresponding pixel in the original image
                                int originalX = (int)(x * xScale);
                                int originalY = (int)(y * yScale);

                                // Calculate positions in byte arrays
                                int originalPos = originalY * originalData.Stride + originalX * bytesPerPixel;
                                int resizedPos = y * resizedData.Stride + x * bytesPerPixel;

                                // Copy pixel data
                                for (int i = 0; i < bytesPerPixel; i++)
                                {
                                    resizedPixels[resizedPos + i] = originalPixels[originalPos + i];
                                }
                            }
                        }

                        // Copy resized pixel data back to the bitmap
                        System.Runtime.InteropServices.Marshal.Copy(resizedPixels, 0, resizedData.Scan0, resizedPixels.Length);

                        // Extract pixel data to return as byte array
                        byte[] buffer = new byte[resizedData.Stride * resizedData.Height];
                        System.Runtime.InteropServices.Marshal.Copy(resizedData.Scan0, buffer, 0, buffer.Length);

                        return buffer;
                    }
                    finally
                    {
                        // Unlock bits
                        if (originalData != null)
                        {
                            originalBitmap.UnlockBits(originalData);
                        }
                        if (resizedData != null)
                        {
                            resizedBitmap.UnlockBits(resizedData);
                        }
                    }
                }
            }
        }


        public ImageBufferData MakeImageBufferData(ImageBuffer imageBuffer, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, EventCodeEnum eventCodeEnum)
        {
            ImageBufferData retval = null;

            try
            {
                retval = new ImageBufferData(imageBuffer, iMAGE_LOG_TYPE, iMAGE_SAVE_TYPE, eventCodeEnum);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void AssignDataSet(ImageData imageData)
        {
            try
            {
                ModuleImageCollection module = GetModule();
                ImageDataSet dataSet = null;

                if (module != null)
                {
                    if (string.IsNullOrEmpty(LastHashCode))
                    {
                        // 모듈 스타트 이후, 최초 데이터 할당 
                        // 새로운 DateSet을 생성하여 할당 될 수 있도록 하자.
                        dataSet = new ImageDataSet();
                        module.ImageDataSetList.Add(dataSet);

                        LastHashCode = dataSet.ImageDatasHashCode;
                        LastModuleStartTime = dataSet.ModuleStartTime;
                    }
                    else
                    {
                        // 해쉬 코드가 존재한다는 것은 하나의 세트에 연속적으로 데이터가 할당되어야 함.
                    }

                    dataSet = module.ImageDataSetList.LastOrDefault();
                    dataSet?.ImageDataCollection.Add(imageData);

                    SetName(module.ModuleType, dataSet);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetName(EnumProberModule moduleType, ImageDataSet dataSet)
        {
            try
            {
                if (dataSet == null)
                {
                    return;
                }

                var folderNameCounts = new Dictionary<string, int>();

                foreach (var data in dataSet.ImageDataCollection)
                {
                    string baseFolderName = data.IMAGE_PROCESSING_TYPE.ToString();
                    string uniqueFolderName = baseFolderName;

                    if (folderNameCounts.ContainsKey(baseFolderName))
                    {
                        folderNameCounts[baseFolderName]++;
                        uniqueFolderName = $"{baseFolderName} ({folderNameCounts[baseFolderName]})";
                    }
                    else
                    {
                        folderNameCounts[baseFolderName] = 0;
                    }

                    foreach (var item in data.ImageBufferDataCollection)
                    {
                        item.FileName = $"{item.CapturedTime:yyMMddHHmmss}_{item.iMAGE_LOG_TYPE}_{item.EventCode}";
                        item.SavePath = this.FileManager().GetImageSaveFullPath(moduleType, item.iMAGE_SAVE_TYPE, false, $"\\{dataSet.ModuleStartTime}\\{uniqueFolderName}", item.FileName);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetImage(ImageBuffer imageBuffer, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum)
        {
            try
            {
                var module = GetModule();

                if (module != null)
                {
                    ImageBufferData buffer = MakeImageBufferData(imageBuffer, iMAGE_LOG_TYPE, iMAGE_SAVE_TYPE, eventCodeEnum);

                    var Imagedata = new ImageData(iMAGE_PROCESSING_TYPE);

                    Imagedata.ImageBufferDataCollection.Add(buffer);

                    AssignDataSet(Imagedata);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetImages(List<ImageBuffer> imageBuffers, IMAGE_LOG_TYPE iMAGE_LOG_TYPE, IMAGE_SAVE_TYPE iMAGE_SAVE_TYPE, IMAGE_PROCESSING_TYPE iMAGE_PROCESSING_TYPE, EventCodeEnum eventCodeEnum)
        {
            try
            {
                var module = GetModule();

                if (module != null)
                {
                    if (imageBuffers != null && imageBuffers.Count > 0)
                    {
                        ObservableCollection<ImageBufferData> buffers = new ObservableCollection<ImageBufferData>();

                        foreach (var item in imageBuffers)
                        {
                            var buffer = MakeImageBufferData(item, iMAGE_LOG_TYPE, iMAGE_SAVE_TYPE, eventCodeEnum);
                            buffers.Add(buffer);
                        }

                        var Imagedata = new ImageData(iMAGE_PROCESSING_TYPE);

                        Imagedata.ImageBufferDataCollection = buffers;

                        AssignDataSet(Imagedata);
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task SaveModuleImagesAsync(EnumProberModule? enumProberModule, ImageSaveFilter imageSaveFilter)
        {
            return Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        ModuleImageCollection module = this.GetModule(enumProberModule);

                        if (module != null)
                        {
                            ImageDataSet ImageDataSet = module.ImageDataSetList.LastOrDefault();

                            if (ImageDataSet != null)
                            {
                                ObservableCollection<ImageData> imageDatas = ImageDataSet.ImageDataCollection;

                                if (imageDatas != null)
                                {
                                    foreach (var data in imageDatas)
                                    {
                                        foreach (var item in data.ImageBufferDataCollection)
                                        {
                                            bool shouldSave = false;

                                            switch (imageSaveFilter)
                                            {
                                                case ImageSaveFilter.All:
                                                    shouldSave = true;
                                                    break;
                                                case ImageSaveFilter.OnlyFail:
                                                    shouldSave = (item.iMAGE_LOG_TYPE == IMAGE_LOG_TYPE.FAIL);
                                                    break;
                                                case ImageSaveFilter.OnlyPass:
                                                    shouldSave = (item.iMAGE_LOG_TYPE == IMAGE_LOG_TYPE.PASS);
                                                    break;
                                            }

                                            if (shouldSave)
                                            {
                                                this.VisionManager().SaveImageBuffer(item.ImageBuffer, item.SavePath, item.iMAGE_LOG_TYPE, item.EventCode);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    LoggerManager.Debug($"[{this.GetType().Name}], SaveModuleImages() : imageDatas is null.");
                                }

                                // Remove the processed ImageDataSet from the list
                                module.ImageDataSetList.Remove(ImageDataSet);
                            }
                            else
                            {
                                LoggerManager.Debug($"[{this.GetType().Name}], SaveModuleImages() : ImageDataSet is null.");
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"[{this.GetType().Name}], SaveModuleImages() : module is null.");
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            });
        }
    }
}
