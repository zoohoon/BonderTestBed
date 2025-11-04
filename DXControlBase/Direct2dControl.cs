// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// This file is derived from a file under the copy right above. 
// Copyright was given by 2010-2012 SharpDX - Alexandre Mutel

namespace DXControlBase
{
    using SharpDX.Direct2D1;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.Mathematics.Interop;
    using SharpDX.DXGI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;
    using System.Runtime.InteropServices;
    using LogModule;
    using System.Threading;

    public static class MemoryManagement
    {
        /// <summary>
        /// Clear un wanted memory
        /// </summary>
        public static void FlushMemory()
        {
            try
            {
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    //Process.GetCurrentProcess().MinWorkingSet = new IntPtr(-1);
                    //Process.GetCurrentProcess().MaxWorkingSet = new IntPtr(-1);

                    //SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// set process working size
        /// </summary>
        /// <param name="process">Gets process</param>
        /// <param name="minimumWorkingSetSize">Gets minimum working size</param>
        /// <param name="maximumWorkingSetSize">Gets maximum working size</param>
        /// <returns>Returns value</returns>
        [DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet =
          CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
    }

    public abstract class D2dControl : System.Windows.Controls.Image
    {
        // - field -----------------------------------------------------------------------

        private SharpDX.Direct3D11.Device device;
        private Texture2D renderTarget;
        private Texture2D renderTargetMSAA;
        private Dx11ImageSource d3DSurface;
        private RenderTarget d2DRenderTarget;
        private SharpDX.Direct2D1.Factory d2DFactory;

        private readonly Stopwatch renderTimer = new Stopwatch();

        protected ResourceCache resCache = new ResourceCache();

        private long lastFrameTime = 0;
        private long lastRenderTime = 0;
        private int frameCount = 0;
        private int frameCountHistTotal = 0;
        private Queue<int> frameCountHist = new Queue<int>();

        // - property --------------------------------------------------------------------

        public static bool IsInDesignMode
        {
            get
            {
                var prop = DesignerProperties.IsInDesignModeProperty;
                var isDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
                return isDesignMode;
            }
        }

        private static readonly DependencyPropertyKey FpsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Fps",
            typeof(int),
            typeof(D2dControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None)
            );

        public static readonly DependencyProperty FpsProperty = FpsPropertyKey.DependencyProperty;

        public int Fps
        {
            get { return (int)GetValue(FpsProperty); }
            protected set { SetValue(FpsPropertyKey, value); }
        }

        public static DependencyProperty RenderWaitProperty = DependencyProperty.Register(
            "RenderWait",
            typeof(int),
            typeof(D2dControl),
            new FrameworkPropertyMetadata(2, OnRenderWaitChanged)
            );

        public int RenderWait
        {
            get { return (int)GetValue(RenderWaitProperty); }
            set { SetValue(RenderWaitProperty, value); }
        }

        // - public methods --------------------------------------------------------------

        public D2dControl()
        {
            try
            {
                base.Loaded += Window_Loaded;
                base.Unloaded += Window_Closing;

                base.Stretch = System.Windows.Media.Stretch.Fill;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public abstract void Render(RenderTarget target);

        // - event handler ---------------------------------------------------------------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (D2dControl.IsInDesignMode)
                {
                    return;
                }

                StartD3D();
                StartRendering();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Window_Closing(object sender, RoutedEventArgs e)
        {
            try
            {

                if (D2dControl.IsInDesignMode)
                {
                    return;
                }

                StopRendering();
                EndD3D();
                MemoryManagement.FlushMemory();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            try
            {
                if (!renderTimer.IsRunning)
                {
                    return;
                }

                PrepareAndCallRender();
                d3DSurface.InvalidateD3DImage();

                lastRenderTime = renderTimer.ElapsedMilliseconds;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //EndD3D();
            try
            {
                CreateAndBindTargets();
                base.OnRenderSizeChanged(sizeInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (d3DSurface.IsFrontBufferAvailable)
                {
                    try
                    {
                     //   LoggerManager.Debug($"StartRendering() Start");

                        StartRendering();

                     //   LoggerManager.Debug($"StartRendering() End");
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                }
                else
                {
                   // LoggerManager.Debug($"StopRendering() Start");

                    StopRendering();

                  //  LoggerManager.Debug($"StopRendering() End");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private static void OnRenderWaitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var control = (D2dControl)d;
               //   control.d3DSurface.RenderWait = (int)e.NewValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        // - private methods -------------------------------------------------------------

        private void StartD3D()
        {
            try
            {
                device = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);

                d3DSurface = new Dx11ImageSource();
                d3DSurface.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;

                CreateAndBindTargets();

                base.Source = d3DSurface;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EndD3D()
        {
            try
            {
                if (d3DSurface != null)
                {
                    d3DSurface.SetRenderTargetDX11(null);
                    d3DSurface.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
                }
                //d2DRenderTarget.EndDraw();
                //var srv = new ShaderResourceView(this.device, renderTarget);
                //device.ImmediateContext.PixelShader.SetShaderResource(0, srv);
                //device.ImmediateContext.ClearState();
                //device.ImmediateContext.Flush();
                base.Source = null;

                resCache.RenderTarget = null;

                Disposer.SafeDispose(ref d2DRenderTarget);
                Disposer.SafeDispose(ref d2DFactory);
                Disposer.SafeDispose(ref d3DSurface);
                Disposer.SafeDispose(ref renderTarget);
                Disposer.SafeDispose(ref renderTargetMSAA);
                Disposer.SafeDispose(ref device);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CreateAndBindTargets()
        {
            try
            {
                if (d3DSurface == null)
                {
                    return;
                }

                d3DSurface.SetRenderTargetDX11(null);
                Disposer.SafeDispose(ref d2DRenderTarget);
                Disposer.SafeDispose(ref d2DFactory);
                Disposer.SafeDispose(ref renderTarget);
                Disposer.SafeDispose(ref renderTargetMSAA);
                var width = Math.Max((int)ActualWidth, 100);
                var height = Math.Max((int)ActualHeight, 100);

                var renderDesc = new Texture2DDescription
                {
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    OptionFlags = ResourceOptionFlags.Shared,
                    CpuAccessFlags = CpuAccessFlags.None,
                    ArraySize = 1,
                };
                //renderTarget = new Texture2D(IntPtr.Zero);
                renderTarget = new Texture2D(device, renderDesc);
                var surface = renderTarget.QueryInterface<Surface>();

                renderTargetMSAA = new Texture2D(device, renderDesc);
                var surfacemsaa = renderTargetMSAA.QueryInterface<Surface>();

                d2DFactory = new SharpDX.Direct2D1.Factory();
                var rtp = new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
                d2DRenderTarget = new RenderTarget(d2DFactory, surfacemsaa, rtp);
                resCache.RenderTarget = d2DRenderTarget;

                d3DSurface.SetRenderTargetDX11(renderTarget);

                device.ImmediateContext.Rasterizer.SetViewport(0, 0, width, height, 0.0f, 1.0f);

                surface.Dispose();
                surfacemsaa.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void StartRendering()
        {
            try
            {
                if (renderTimer.IsRunning)
                {
                    return;
                }

                System.Windows.Media.CompositionTarget.Rendering += OnRendering;
                renderTimer.Start();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void StopRendering()
        {
            try
            {
                if (!renderTimer.IsRunning)
                {
                    return;
                }

                System.Windows.Media.CompositionTarget.Rendering -= OnRendering;
                renderTimer?.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private async void PrepareAndCallRender()
        {
            try
            {
                if (device == null)
                {
                    return;
                }

                await Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate
                {
                    if (d2DRenderTarget == null)
                    {
                        return; /*Debugger.Break();*/
                    }
                    d2DRenderTarget.BeginDraw();
                    Render(d2DRenderTarget);
                    d2DRenderTarget.EndDraw();

                    //CalcFps();

                    int sourceSubresource;
                    int sourceMipLevels;
                    sourceSubresource = renderTargetMSAA.CalculateSubResourceIndex(0, 0, out sourceMipLevels);
                    int destinationSubresource;
                    int destinationMipLevels;
                    destinationSubresource = renderTarget.CalculateSubResourceIndex(0, 0, out destinationMipLevels);
                    device.ImmediateContext.ResolveSubresource(renderTargetMSAA, sourceSubresource,
                       renderTarget, destinationSubresource, Format.B8G8R8A8_UNorm);

                    device.ImmediateContext.Flush();
                });



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void CalcFps()
        {
            try
            {
                frameCount++;
                if (renderTimer.ElapsedMilliseconds - lastFrameTime > 1000)
                {
                    frameCountHist.Enqueue(frameCount);
                    frameCountHistTotal += frameCount;
                    if (frameCountHist.Count > 5)
                    {
                        frameCountHistTotal -= frameCountHist.Dequeue();
                    }

                    Fps = frameCountHistTotal / frameCountHist.Count;

                    frameCount = 0;
                    lastFrameTime = renderTimer.ElapsedMilliseconds;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
    public static class Transformation
    {
        public static RawMatrix3x2 Rotation(float anglel, RawVector2 pivot)
        {
            try
            {

                float c = (float)Math.Cos(anglel * Math.PI / 180.0);
                float s = (float)Math.Sin(anglel * Math.PI / 180.0);
                var matrix = new RawMatrix3x2()
                {
                    M11 = c,
                    M12 = s,
                    M21 = -s,
                    M22 = c,
                    M31 = pivot.X * (1 - c) + pivot.Y * s,
                    M32 = pivot.Y * (1 - c) - pivot.X * s
                };
                return matrix;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return new RawMatrix3x2();
            }
        }

        public static RawVector2 Rotation(RawVector2 target, RawVector2 pivot,float angle)
        {
            try
            {
                double radian = (double)(((angle) / 180) * Math.PI);
                double cosq = Math.Cos(radian);
                double sinq = Math.Sin(radian);
                double sx = target.X - pivot.X;
                double sy = target.Y - pivot.Y;
                double rx = (sx * cosq - sy * sinq) + pivot.X; // °á°ú ÁÂÇ¥ x
                double ry = (sx * sinq + sy * cosq) + pivot.Y; // °á°ú ÁÂÇ¥ y
                return new RawVector2((int)rx, (int)ry);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return target;
            }
        }

    }
}
