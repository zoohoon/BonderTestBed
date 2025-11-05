using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Vision.DisplayModule
{
    using ProberInterfaces;
    using System.Windows.Media.Imaging;
    using System.Windows.Controls;
    using System.Windows.Media;
    using ProberInterfaces.Vision;
    using System.Collections.ObjectModel;
    using System.Windows;
    using ProberErrorCode;
    using LogModule;

    /// <summary>
    /// ModuleVisionDisplay 
    /// </summary>
    public class ModuleVisionDisplay : IDisplay, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event ImageUpdatedEventHandler ImageUpdated;

        #region //..Property

        private ICamera _OwnerCamera;

        public ICamera OwnerCamera
        {
            get { return _OwnerCamera; }
            set { _OwnerCamera = value; }
        }


        private List<IDisplayPort> _DispPorts = new List<IDisplayPort>();

        public List<IDisplayPort> DispPorts
        {
            get { return _DispPorts; }
            set { _DispPorts = value; }
        }

        public Vision.GraphicsContext.GraphicsContext GraphicsContext { get; set; }

        private WriteableBitmap _WrbDispImage;
        public WriteableBitmap WrbDispImage
        {
            get { return _WrbDispImage; }
            set
            {
                if (value != _WrbDispImage)
                {
                    _WrbDispImage = value;

                    // Call OnPropertyChanged whenever the property is updated
                    NotifyPropertyChanged("WrbDispImage");
                }
            }
        }

        private Canvas _OverlayCanvas
             = new Canvas();
        public Canvas OverlayCanvas
        {
            get { return _OverlayCanvas; }
            set
            {
                if (value != _OverlayCanvas)
                {
                    _OverlayCanvas = value;
                    NotifyPropertyChanged("OverlayCanvas");
                }
            }
        }

        private ObservableCollection<IDrawable> _DrawOverlayContexts;
        public ObservableCollection<IDrawable> DrawOverlayContexts
        {
            get { return _DrawOverlayContexts; }
            set
            {
                if (value != _DrawOverlayContexts)
                {
                    _DrawOverlayContexts = value;
                    NotifyPropertyChanged("DrawOverlayContexts");
                }
            }
        }
        private bool _needUpdateOverlayCanvasSize = false;

        public bool needUpdateOverlayCanvasSize
        {
            get { return _needUpdateOverlayCanvasSize; }
            set { _needUpdateOverlayCanvasSize = value; }
        }

        private byte[] EmulImage = new byte[0];

        #endregion

        public ModuleVisionDisplay()
        {
            try
            {

                GraphicsContext = new Vision.GraphicsContext.GraphicsContext();
                DrawOverlayContexts = new ObservableCollection<IDrawable>();
                GraphicsContext.InitModule(this);
                OverlayCanvas.SizeChanged += OverlayCanvasSizeChangedEventHandler;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void OverlayCanvasSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            if(OverlayCanvas.ActualWidth !=0 & OverlayCanvas.ActualHeight !=0)
            {
                needUpdateOverlayCanvasSize = true;
            }
        }

        //private void SetImage(ICamera cam, WriteableBitmap wb = null)
        //{
        //    try
        //    {
        //        foreach (IDisplayPort port in DispPorts)
        //        {
        //            if (port != null)
        //                port.SetImage(cam, wb);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw err;
        //    }
        //}

        public bool IsImageUpdatedNull()
        {
            if (ImageUpdated == null)
                return true;
            else
                return false;
        }

        private bool IsSendTimming = true;
        public void SetImage(ICamera cam, ImageBuffer img = null)
        {
            try
            {
                // STAGE
                foreach (IDisplayPort port in DispPorts.ToList())
                {
                    if (port != null)
                        port.SetImage(cam, img);
                }

                // LOADER
                if (ImageUpdated != null)
                {
                    if (this.StageCommunicationManager() != null)
                    {
                        IsSendTimming = true;

                        if (this.StageCommunicationManager().GetAcceptUpdateDisp() & IsSendTimming)
                        {
                            try
                            {
                                //img.DrawOverlayContexts = new ObservableCollection<IDrawable>(DrawOverlayContexts);
                                ImageUpdated(img);

                                var ret = (IsSendTimming == true)? IsSendTimming = false : IsSendTimming = true;
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }
                        else
                        {
                            IsSendTimming = true;
                        }
                    }
                    else
                    {
                        try
                        {
                            ImageUpdated(img);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        /// <summary>
        /// Color 출력
        /// </summary>
        /// <param name="image"></param>
        /// <param name="sizex"></param>
        /// <param name="sizey"></param>
        public void ConvArrTOWRB_BAndW_Overlay(byte[] image, int sizex, int sizey, ICamera cam)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (image != null && image.Count() != 0 && image != new byte[0])
                    {
                        lock (image)
                        {


                            WrbDispImage = ImageConverter.WriteableBitmapFromColoredArray(
                                    image, sizex, sizey, WrbDispImage);

                            //SetImage(cam, WrbDispImage);
                        }
                    }
                    else
                    {
                        SetImage(cam);
                    }


                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        /// <summary>
        /// (현재 카메라 흑백) . 흑백 출력
        /// </summary>
        /// <param name="image"></param>
        /// <param name="sizex"></param>
        /// 
        /// 
        /// <param name="sizey"></param>
        public void ConvArrTOWRB_BAndW(ImageBuffer image, int sizex, int sizey, ICamera cam)
        {
            try
            {
                var appCurr = System.Windows.Application.Current;
                if (appCurr != null)
                {
                    if (image.ColorDept == (int)ColorDept.Color24)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            if (image.Buffer != EmulImage)
                            {
                                WrbDispImage = ImageConverter.WriteableBitmapFromColoredArray(image.Buffer, sizex, sizey, WrbDispImage);
                                //SetImage(cam, WrbDispImage);
                            }
                            else
                            {
                                SetImage(cam);
                            }

                        }));

                    }
                    else
                    {
                        if (image.ColorDept == (int)ColorDept.BlackAndWhite)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                if (image.Buffer.Count() != 0 && image != null && image.Buffer != new byte[0]
                                && sizex != 0 && sizey != 0)
                                {

                                    WrbDispImage = ImageConverter.WriteableBitmapFromArray(image.Buffer, sizex, sizey, WrbDispImage);
                                }
                            }));

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }

        private Action EmptyDelegate = delegate () { };
        public EventCodeEnum Draw(ImageBuffer img)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                //lock(img)
                //{
                if (OverlayCanvas.Dispatcher.CheckAccess())
                {
                    //if(OverlayCanvas.ActualWidth == 0
                    //    | OverlayCanvas.ActualHeight ==0)
                    //{
                    //    needUpdateOverlayCanvasSize = true;
                    //}
                    //else
                    //{
                    //    needUpdateOverlayCanvasSize = false;
                    //}
                    OverlayCanvas.UpdateLayout();
                    OverlayCanvas.Children.Clear();
                    //UIElementCollection canvasChildren = OverlayCanvas.Children;
                    //overlaydelays.DelayFor(500);
                    //LoggerManager.Debug("[Canvas Clear] - Disply Module");
                    ObservableCollection<IDrawable> drawables = new ObservableCollection<IDrawable>();
                    foreach (var drawable in DrawOverlayContexts)
                    {
                        drawable.Draw(this, img);
                    }


                    OverlayCanvas.UpdateLayout();

                    //overlaydelays.DelayFor(500);
                    //LoggerManager.Debug("[Canvas UpdateLayout] - Disply Module");
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        OverlayCanvas.Children.Clear();
                        ObservableCollection<IDrawable> drawables = new ObservableCollection<IDrawable>();
                        foreach (var drawable in DrawOverlayContexts)
                        {
                            drawable.Draw(this, img);
                        }
                        OverlayCanvas.UpdateLayout();

                       
                    }));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }

        public EventCodeEnum ClearOverlayCanvas()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                DrawOverlayContexts.Clear();
                //OverlayCanvas.Children.Clear();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum OverlayRect(ImageBuffer img, double xCenter, double yCenter,
            double width, double heitgth, Color color = default(Color), double thickness = 1, double angle = 0.0)
        {
            try
            {

                return GraphicsContext.OverlayRect(
                    img, xCenter, yCenter, width, heitgth, color, thickness, angle);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public EventCodeEnum OverlayEllipse(ImageBuffer img, double xCenter, double yCenter, double radius,
             Color color = default(Color), double startAngle = 0, double endAngle = 360, double thickness = 1)
        {
            try
            {
                return GraphicsContext.OverlayEllipse(img, xCenter, yCenter, radius,
                        color, startAngle, endAngle, thickness);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public EventCodeEnum OverlayLine(ImageBuffer img, double xStart, double yStart,
            double xEnd, double yEnd, Color color = default(Color))
        {
            try
            {
                return GraphicsContext.OverlayLine(img, xStart, yStart, xEnd, yEnd, color);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public EventCodeEnum OverlayString(ImageBuffer img, string text, double xStart, double yStart,
            double fontsize = 12, Color fontcolor = default(Color), Color backcolor = default(Color))
        {
            try
            {
                return GraphicsContext.OverlayText(img, text, xStart, yStart,
                    fontsize, fontcolor, backcolor);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }
    }

    #region //..ImageConverter class
    /// <summary>
    /// byte[]을 WriteableBitmap으로변환하여 주는 static class
    /// </summary>
    public static class ImageConverter
    {

        #region // Bitmap conversio
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgarray"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="wrbitmap"></param>
        /// <returns></returns>
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



        public static class DisplayProcessingConverter
        {
            
        }

        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromEdgeArray(byte[] imgarray, int width, int height, System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {
                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Gray8;

                int rawStride = (width * pf.BitsPerPixel + 7) / 8;
                int aImageStride;
                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);
                wrbitmap = null;
                WriteableBitmap convertsizewb = null;
                if (wrbitmap == null)
                {

                    if (width > height)
                    {
                        convertsizewb = new System.Windows.Media.Imaging.WriteableBitmap(
                                height, height, 96, 96,
                                System.Windows.Media.PixelFormats.Gray8,
                                null);
                    }
                    else if (height > width)
                    {
                        convertsizewb = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, width, 96, 96,
                                System.Windows.Media.PixelFormats.Gray8,
                                null);
                    }
                    else
                    {
                        wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                               width, height, 96, 96,
                               System.Windows.Media.PixelFormats.Gray8,
                               null);

                    }
                }
                pf = System.Windows.Media.PixelFormats.Gray8;


                aImageStride = width * wrbitmap.Format.BitsPerPixel / 8;
                wrbitmap.WritePixels(anImageRectangle, imgarray, aImageStride, 0);

            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("BitMapSourceFromRawImage(): Exception caught.\n - Error message: {0}", err.Message));
                LoggerManager.Exception(err);
                throw err;

            }
            return wrbitmap;
        }


        //System.IO.FileStream tmpfileStream;
        /// <summary>
        /// overlay할때 등 
        /// </summary>
        /// <param name="imgarray"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="wrbitmap"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.WriteableBitmap WriteableBitmapFromColoredArray(byte[] imgarray, int width, int height,
                                                System.Windows.Media.Imaging.WriteableBitmap wrbitmap)
        {
            try
            {

                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Rgb24;

                int rawStride = width * ((pf.BitsPerPixel + 7) / 8);
                rawStride = width * ((pf.BitsPerPixel) / 8);

                System.Windows.Int32Rect anImageRectangle = new System.Windows.Int32Rect(0, 0, width, height);


                if (wrbitmap == null || wrbitmap.Format != pf)
                {
                    wrbitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                                width, height, 96, 96,
                                pf, null);
                }


                wrbitmap.WritePixels(anImageRectangle, imgarray, rawStride, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return wrbitmap;
        }

        #endregion
    }
    #endregion
}
