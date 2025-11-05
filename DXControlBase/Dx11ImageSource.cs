using LogModule;
using System;
using System.Windows.Interop;

namespace DXControlBase
{
    //class Dx11ImageSource : D3DImage, IDisposable
    //{

    //    // - field -----------------------------------------------------------------------

    //    private static int ActiveClients;
    //    private static Direct3DEx D3DContext;
    //    private static DeviceEx D3DDevice;

    //    private Texture renderTarget;

    //    // - property --------------------------------------------------------------------

    //    public int RenderWait { get; set; } = 2; // default: 2ms

    //    // - public methods --------------------------------------------------------------

    //    public Dx11ImageSource()
    //    {
    //        StartD3D();
    //        Dx11ImageSource.ActiveClients++;
    //    }

    //    public void Dispose()
    //    {
    //        SetRenderTarget(null);

    //        Disposer.SafeDispose(ref renderTarget);

    //        Dx11ImageSource.ActiveClients--;
    //        EndD3D();
    //    }

    //    public void InvalidateD3DImage()
    //    {
    //        if (renderTarget != null)
    //        {
    //            base.Lock();
    //            if (RenderWait != 0)
    //            {
    //                Thread.Sleep(RenderWait);
    //            }
    //            base.AddDirtyRect(new System.Windows.Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
    //            base.Unlock();
    //        }
    //    }
    //    WeakReference wr;
    //    public void SetRenderTarget(SharpDX.Direct3D11.Texture2D target)
    //    {
    //        if (renderTarget != null)
    //        {
    //            //var surface = renderTarget.GetSurfaceLevel(0);
    //            //surface.ReleaseDC(surface.NativePointer);
    //            renderTarget = null;
    //            base.Lock();
    //            if(wr.IsAlive)
    //            {

    //            }
    //            base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
    //            GC.Collect();
    //            if (wr.IsAlive)
    //            {

    //            }
    //            base.Unlock();
    //        }

    //        if (target == null)
    //        {
    //            return;
    //        }

    //        var format = Dx11ImageSource.TranslateFormat(target);
    //        var handle = GetSharedHandle(target);

    //        if (!IsShareable(target))
    //        {
    //            throw new ArgumentException("Texture must be created with ResouceOptionFlags.Shared");
    //        }

    //        if (format == Format.Unknown)
    //        {
    //            throw new ArgumentException("Texture format is not compatible with OpenSharedResouce");
    //        }

    //        if (handle == IntPtr.Zero)
    //        {
    //            throw new ArgumentException("Invalid handle");
    //        }
    //        wr = new WeakReference(new Texture(Dx11ImageSource.D3DDevice, target.Description.Width, target.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle));
    //        //renderTarget = new Texture(Dx11ImageSource.D3DDevice, target.Description.Width, target.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle);
    //        renderTarget =(Texture)wr.Target;

    //        using (var surface = renderTarget.GetSurfaceLevel(0))
    //        {
    //            base.Lock();
    //            base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
    //            base.Unlock();
    //        }
    //    }

    //    // - private methods -------------------------------------------------------------

    //    private void StartD3D()
    //    {
    //        if (Dx11ImageSource.ActiveClients != 0)
    //        {
    //            return;
    //        }

    //        var presentParams = GetPresentParameters();
    //        var createFlags = CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve;

    //        Dx11ImageSource.D3DContext = new Direct3DEx();
    //        Dx11ImageSource.D3DDevice = new DeviceEx(D3DContext, 0, DeviceType.Hardware, IntPtr.Zero, createFlags, presentParams);
    //    }

    //    private void EndD3D()
    //    {
    //        if (Dx11ImageSource.ActiveClients != 0)
    //        {
    //            return;
    //        }

    //        Disposer.SafeDispose(ref renderTarget);
    //        Disposer.SafeDispose(ref Dx11ImageSource.D3DDevice);
    //        Disposer.SafeDispose(ref Dx11ImageSource.D3DContext);
    //    }

    //    private static void ResetD3D()
    //    {
    //        if (Dx11ImageSource.ActiveClients == 0)
    //        {
    //            return;
    //        }

    //        var presentParams = GetPresentParameters();
    //        Dx11ImageSource.D3DDevice.ResetEx(ref presentParams);
    //    }

    //    private static PresentParameters GetPresentParameters()
    //    {
    //        var presentParams = new PresentParameters();

    //        presentParams.Windowed = true;
    //        presentParams.SwapEffect = SwapEffect.Discard;
    //        presentParams.DeviceWindowHandle = NativeMethods.GetDesktopWindow();
    //        presentParams.PresentationInterval = PresentInterval.Default;

    //        return presentParams;
    //    }

    //    private IntPtr GetSharedHandle(SharpDX.Direct3D11.Texture2D texture)
    //    {
    //        using (var resource = texture.QueryInterface<SharpDX.DXGI.Resource>())
    //        {
    //            return resource.SharedHandle;
    //        }
    //    }

    //    private static Format TranslateFormat(SharpDX.Direct3D11.Texture2D texture)
    //    {
    //        switch (texture.Description.Format)
    //        {
    //            case SharpDX.DXGI.Format.R10G10B10A2_UNorm: return SharpDX.Direct3D9.Format.A2B10G10R10;
    //            case SharpDX.DXGI.Format.R16G16B16A16_Float: return SharpDX.Direct3D9.Format.A16B16G16R16F;
    //            case SharpDX.DXGI.Format.B8G8R8A8_UNorm: return SharpDX.Direct3D9.Format.A8R8G8B8;
    //            default: return SharpDX.Direct3D9.Format.Unknown;
    //        }
    //    }

    //    private static bool IsShareable(SharpDX.Direct3D11.Texture2D texture)
    //    {
    //        return (texture.Description.OptionFlags & SharpDX.Direct3D11.ResourceOptionFlags.Shared) != 0;
    //    }
    //}
    using global::SharpDX.Direct3D11;

    using global::SharpDX.Direct3D9;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    // Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
    // 
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
    public sealed class Dx11ImageSource : D3DImage, IDisposable
    {
        private Direct3DEx context;
        private DeviceEx device;

        private readonly int adapterIndex;
        private Texture renderTarget;

        //public int RenderWait { get; set; } = 2; // default: 2ms

        public static Duration duration;

        public Dx11ImageSource(int adapterIndex = 0)
        {
            try
            {
                this.adapterIndex = adapterIndex;
                this.StartD3D();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        Object obj = new object();
        public void InvalidateD3DImage()
        {
            // So, creating a copy of the texture: 
            // DXDevice11.Device.ImmediateContext.ResolveSubresource(_dx11RenderTexture, 0, _dx11BackpageTexture, 0, ColorFormat);
            // (_dx11BackpageTexture is shared texture) will wait until the rendering is ready and will create a copy.
            // This is how I got rid of the flickering....

            if (this.renderTarget != null)
            {

                try
                {
                    if (TryLock(new Duration(default(TimeSpan))))
                    {
                        base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                    }
                    base.Unlock();
                }
                catch (Exception ex)
                {
                    LoggerManager.Exception(ex);
                }
                //base.Lock();

                //base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);

                //base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                //base.Unlock();

                //                using (Surface surface = this.renderTarget.GetSurfaceLevel(0))
                //                {
                //                    base.Lock();
                //#if NET40
                //                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                //#else
                //                    // "enableSoftwareFallback = true" makes Remote Desktop possible.
                //                    // See: http://msdn.microsoft.com/en-us/library/hh140978%28v=vs.110%29.aspx
                //                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer, true);
                //#endif

                //                    base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));

                //                    base.Unlock();
                //                }
            }
        }

        public void SetRenderTargetDX11(Texture2D target)
        {
            try
            {
                if (this.renderTarget != null)
                {
                    Disposer.SafeDispose(ref this.renderTarget);

                    base.Lock();
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                    base.Unlock();
                }

                if (target == null)
                    return;

                if (!IsShareable(target))
                    throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");

                var format = TranslateFormat(target);
                if (format == Format.Unknown)
                    throw new ArgumentException("Texture format is not compatible with OpenSharedResource");

                var handle = GetSharedHandle(target);
                if (handle == IntPtr.Zero)
                    throw new ArgumentNullException("Handle");

                this.renderTarget = new Texture(device, target.Description.Width, target.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle);
                using (Surface surface = this.renderTarget.GetSurfaceLevel(0))
                {
                    base.Lock();
#if NET40
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
#else
                        // "enableSoftwareFallback = true" makes Remote Desktop possible.
                        // See: http://msdn.microsoft.com/en-us/library/hh140978%28v=vs.110%29.aspx
                        base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer, true);
#endif
                    base.Unlock();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LoggerManager.Exception(ex);
            }
        }

        private void StartD3D()
        {
            try
            {
                context = new Direct3DEx();
                // Ref: https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/wpf-and-direct3d9-interoperation
                var presentparams = new PresentParameters
                {
                    Windowed = true,
                    SwapEffect = SwapEffect.Discard,
                    //DeviceWindowHandle = GetDesktopWindow(),
                    PresentationInterval = PresentInterval.Default,
                    BackBufferHeight = 1,
                    BackBufferWidth = 1,
                    BackBufferFormat = Format.Unknown
                };

                device = new DeviceEx(context,
                                        this.adapterIndex,
                                        DeviceType.Hardware,
                                        IntPtr.Zero,
                                        CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
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
                Disposer.RemoveAndDispose(ref renderTarget);
                Disposer.RemoveAndDispose(ref device);
                Disposer.RemoveAndDispose(ref context);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }

        private static IntPtr GetSharedHandle(Texture2D sharedTexture)
        {
            try
            {
                using (var resource = sharedTexture.QueryInterface<global::SharpDX.DXGI.Resource>())
                {
                    IntPtr result = resource.SharedHandle;
                    return result;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private static Format TranslateFormat(Texture2D sharedTexture)
        {
            try
            {
                switch (sharedTexture.Description.Format)
                {
                    case global::SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                        return Format.A2B10G10R10;

                    case global::SharpDX.DXGI.Format.R16G16B16A16_Float:
                        return Format.A16B16G16R16F;

                    case global::SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                        return Format.A8R8G8B8;

                    default:
                        return Format.Unknown;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private static bool IsShareable(Texture2D sharedTexture)
        {
            try
            {
                return (sharedTexture.Description.OptionFlags & ResourceOptionFlags.Shared) != 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }


        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        [SuppressMessage("Microsoft.Usage", "CA2213: Disposable fields should be disposed", Justification = "False positive.")]
        void Dispose(bool disposing)
        {
            try
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        EndD3D();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DX11ImageSource() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            try
            {
                Dispose(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}


