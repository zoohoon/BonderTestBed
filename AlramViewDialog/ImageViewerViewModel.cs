using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AlarmViewDialog
{
    public class ImageViewerViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        Window window;

        private ImageDataSet _imageDataSet;
        public ImageDataSet ImageDataSet
        {
            get { return _imageDataSet; }
            set
            {
                if (value != _imageDataSet)
                {
                    _imageDataSet = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ImageData _selectedImageData;
        public ImageData SelectedImageData
        {
            get { return _selectedImageData; }
            set
            {
                if (value != _selectedImageData)
                {
                    _selectedImageData = value;
                    RaisePropertyChanged();
                    CurrentImageIndex = 0;
                    UpdateCurrentImage();
                }
            }
        }

        private int _currentImageIndex;
        public int CurrentImageIndex
        {
            get { return _currentImageIndex; }
            set
            {
                if (value != _currentImageIndex)
                {
                    _currentImageIndex = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DisplayedImageIndex));
                    UpdateCurrentImage();
                }
            }
        }

        public int DisplayedImageIndex
        {
            get { return _currentImageIndex + 1; }
        }

        private BitmapSource _currentImage;
        public BitmapSource CurrentImage
        {
            get { return _currentImage; }
            set
            {
                if (value != _currentImage)
                {
                    _currentImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ExportPath = string.Empty;
        public string ExportPath
        {
            get
            {
                return _ExportPath;
            }
            set
            {
                _ExportPath = value;
                RaisePropertyChanged();
            }
        }

        private ImageBufferData _currentImageBufferData;
        public ImageBufferData CurrentImageBufferData
        {
            get { return _currentImageBufferData; }
            set
            {
                if (value != _currentImageBufferData)
                {
                    _currentImageBufferData = value;
                    RaisePropertyChanged();
                }
            }
        }

        private WriteableBitmap Byte2ControlsImage(byte[] imageData)
        {
            WriteableBitmap retval = null;

            if (imageData == null || imageData.Length == 0)
            {
                LoggerManager.Error("Image data is null or empty.");
                return null;
            }

            try
            {
                retval = WriteableBitmapFromArray(imageData, 960, 960, retval);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromArray(byte[] imgarray,
            int width, int height, System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {
                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Gray8;

                int rawStride = (width * pf.BitsPerPixel + 7) / 8;

                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);

                if (wrbitmap == null || wrbitmap.Format != pf
                   || width != wrbitmap.Width || height != wrbitmap.Height)
                {
                    wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, height, 96, 96,
                                pf, null);
                }

                if (imgarray.Length == (wrbitmap.Width * wrbitmap.Height))
                    wrbitmap.WritePixels(anImageRectangle, imgarray, rawStride, 0);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("BitMapSourceFromRawImage(): Exception caught.\n - Error message: {0}", err.Message));
                LoggerManager.Exception(err);
                throw err;
            }
            return wrbitmap;
        }

        private BitmapSource ByteToBitmapSource(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                LoggerManager.Error("Image data is null or empty.");
                return null;
            }

            try
            {
                int width = 960;
                int height = 960;
                int stride = width; // For Gray8 format
                BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, imageData, stride);

                return bitmap;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return null;
            }
        }

        private void RestoreImage()
        {
            try
            {
                BitmapSource bitmap = ByteToBitmapSource(CurrentImageBufferData.Buffer);

                if (bitmap != null)
                {
                    CurrentImage = bitmap;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private void RestoreImage()
        //{
        //    try
        //    {
        //        WriteableBitmap bitmap = Byte2ControlsImage(CurrentImageBufferData.Buffer);

        //        if (bitmap == null) return;

        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            CurrentImage = new Image { Source = bitmap };
        //        });
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public ICommand NextImageCommand => new RelayCommand(NextImage, CanNextImage);
        public ICommand PreviousImageCommand => new RelayCommand(PreviousImage, CanPreviousImage);

        private void NextImage()
        {
            if (CanNextImage())
            {
                CurrentImageIndex++;
            }
        }

        private bool CanNextImage()
        {
            return SelectedImageData != null && CurrentImageIndex < SelectedImageData.ImageBufferDataCollection.Count - 1;
        }

        private void PreviousImage()
        {
            if (CanPreviousImage())
            {
                CurrentImageIndex--;
            }
        }

        private bool CanPreviousImage()
        {
            return SelectedImageData != null && CurrentImageIndex > 0;
        }

        private void UpdateCurrentImage()
        {
            if (SelectedImageData != null && SelectedImageData.ImageBufferDataCollection.Count > 0)
            {
                CurrentImageBufferData = SelectedImageData.ImageBufferDataCollection[CurrentImageIndex];

                RestoreImage();
            }
        }

        public ImageViewerViewModel(Window window, ImageDataSet imageDataSet)
        {
            this.window = window;
            this.ImageDataSet = imageDataSet;

            if (this.ImageDataSet?.ImageDataCollection?.Count > 0)
            {
                SelectedImageData = this.ImageDataSet?.ImageDataCollection.LastOrDefault();
            }
        }


        private AsyncCommand _ExportCommand;
        public ICommand ExportCommand
        {
            get
            {
                if (null == _ExportCommand) _ExportCommand = new AsyncCommand(ExportCommandFunc, showWaitCancel: false);
                return _ExportCommand;
            }
        }

        private Window GetActiveWindow()
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                return Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            });
        }

        private async Task ExportCommandFunc()
        {
            var waitDialog = new WaitMessageDialog();

            var mainWindow = GetActiveWindow();

            if (mainWindow != null)
            {
                waitDialog.Owner = mainWindow;
            }

            try
            {
                waitDialog.Show();

                // Ensure ImageDataSet is not null and has at least one ImageData
                if (ImageDataSet != null && ImageDataSet.ImageDataCollection.Count > 0)
                {
                    // Loop through each ImageData in the collection
                    foreach (var imageData in ImageDataSet.ImageDataCollection)
                    {
                        // Ensure ImageData has at least one ImageBufferData
                        if (imageData.ImageBufferDataCollection.Count > 0)
                        {
                            foreach (var imageBufferData in imageData.ImageBufferDataCollection)
                            {
                                if (imageBufferData.Buffer != null)
                                {
                                    await Task.Run(() =>
                                    {
                                        System.Threading.Thread.Sleep(5000);

                                        string originalSavePath = imageBufferData.SavePath;

                                        // Split the path into parts
                                        string[] pathParts = originalSavePath.Split(Path.DirectorySeparatorChar);

                                        // Identify the root and next folder, if present
                                        string root = Path.GetPathRoot(originalSavePath);
                                        string subPath = originalSavePath.Substring(root.Length);
                                        string[] subPathParts = subPath.Split(new[] { Path.DirectorySeparatorChar }, 2);

                                        string newPath;
                                        if (subPathParts.Length > 1)
                                        {
                                            // Construct the new path with the "EMUL\\Image" inserted
                                            newPath = Path.Combine(root, subPathParts[0], "EMUL\\Image", subPathParts[1]);
                                        }
                                        else
                                        {
                                            // If there is no subfolder, just add "EMUL\\Image" after the root
                                            newPath = Path.Combine(root, "EMUL\\Image", subPathParts[0]);
                                        }

                                        // Ensure the directory exists
                                        string directoryPath = Path.GetDirectoryName(newPath);

                                        // Extract ExportPath including ModuleStartTime
                                        int moduleStartIndex = newPath.IndexOf(ImageDataSet.ModuleStartTime);
                                        if (moduleStartIndex != -1)
                                        {
                                            string moduleStartPath = newPath.Substring(0, moduleStartIndex + ImageDataSet.ModuleStartTime.Length);
                                            ExportPath = moduleStartPath;
                                        }

                                        if (!Directory.Exists(directoryPath))
                                        {
                                            Directory.CreateDirectory(directoryPath);
                                        }

                                        Bitmap bmp = null;

                                        try
                                        {
                                            bmp = new Bitmap(960, 960, imageBufferData.PixelFormat);

                                            ColorPalette pal = bmp.Palette;
                                            for (int i = 0; i < 256; i++)
                                            {
                                                pal.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                                            }

                                            bmp.Palette = pal;

                                            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                                            try
                                            {
                                                // Directly copy pixel data into the locked bitmap data
                                                int stride = bmpdata.Stride;
                                                IntPtr ptr = bmpdata.Scan0;

                                                for (int y = 0; y < 960; y++)
                                                {
                                                    int yOffset = y * 960;
                                                    int strideOffset = y * stride;
                                                    for (int x = 0; x < 960; x++)
                                                    {
                                                        Marshal.WriteByte(ptr, strideOffset + x, imageBufferData.Buffer[yOffset + x]);
                                                    }
                                                }
                                            }
                                            finally
                                            {
                                                bmp.UnlockBits(bmpdata);
                                            }

                                            // Save the image using the appropriate format
                                            switch (imageBufferData.iMAGE_SAVE_TYPE)
                                            {
                                                case IMAGE_SAVE_TYPE.BMP:
                                                    bmp.Save(newPath, System.Drawing.Imaging.ImageFormat.Bmp);
                                                    break;
                                                case IMAGE_SAVE_TYPE.JPEG:
                                                    bmp.Save(newPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                    break;
                                                case IMAGE_SAVE_TYPE.PNG:
                                                    bmp.Save(newPath, System.Drawing.Imaging.ImageFormat.Png);
                                                    break;
                                                default:
                                                    throw new NotSupportedException("Unsupported image save type");
                                            }
                                        }
                                        catch (Exception err)
                                        {
                                            LoggerManager.Exception(err);
                                        }
                                    });
                                }
                                else
                                {
                                    LoggerManager.Error($"[{this.GetType().Name}], ExportCommandFunc() : imageBufferData.Buffer is null.");
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                waitDialog.Close();
            }
        }

        private RelayCommand _OpenExplorerCommand;
        public ICommand OpenExplorerCommand
        {
            get
            {
                if (null == _OpenExplorerCommand) _OpenExplorerCommand = new RelayCommand(OpenExplorerCommandFunc);
                return _OpenExplorerCommand;
            }
        }

        private void OpenExplorerCommandFunc()
        {
            try
            {
                if (!string.IsNullOrEmpty(ExportPath))
                {
                    if (Directory.Exists(ExportPath))
                    {
                        Process.Start("explorer.exe", ExportPath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _CopyExportPathCommand;
        public ICommand CopyExportPathCommand
        {
            get
            {
                if (null == _CopyExportPathCommand) _CopyExportPathCommand = new RelayCommand(CopyExportPath);
                return _CopyExportPathCommand;
            }
        }

        private void CopyExportPath()
        {
            try
            {
                if (!string.IsNullOrEmpty(ExportPath))
                {
                    Clipboard.SetText(ExportPath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _CloseWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                if (null == _CloseWindowCommand) _CloseWindowCommand = new RelayCommand(CloseWindowCommandFunc);
                return _CloseWindowCommand;
            }
        }

        private void CloseWindowCommandFunc()
        {
            try
            {
                window.Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

}
