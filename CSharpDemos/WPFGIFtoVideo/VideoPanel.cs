
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFGIFtoVideo.Interop;

namespace WPFGIFtoVideo
{
    public class VideoPanel : System.Windows.Controls.ContentControl
    {
        enum D3DFMT
        {
            D3DFMT_A8R8G8B8             = 21,
            D3DFMT_X8R8G8B8             = 22
        }

        private const int Format = (int)D3DFMT.D3DFMT_X8R8G8B8;

        private static uint m_width = 1280;// 800; 

        private static uint m_height = 720;// 600;

        private class D3D9Image : D3DImage
        {
            private Direct3DSurface9 surface;

            private D3D9Image() { }

            static public System.Tuple<D3DImage, Direct3DSurface9> createD3D9Image()
            {
                D3D9Image lImageSource = new D3D9Image();

                return lImageSource.init() ?
                    System.Tuple.Create<D3DImage, Direct3DSurface9>(lImageSource, lImageSource.surface)
                    :
                    null;
            }

            public BitmapSource getBackBuffer()
            {
                return this.CopyBackBuffer();
            }

            private bool init()
            {
                bool lresult = false;

                do
                {

                    // Free the old texture
                    if (this.surface != null)
                    {
                        this.surface.Dispose();
                        this.surface = null;
                    }

                    using (var device = CreateDevice(WPFGIFtoVideo.Interop.NativeMethods.GetDesktopWindow()))
                    {
                        surface = GetSharedSurface(device);

                        Lock();

                        this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.surface.NativeInterface);

                        Unlock();

                        lresult = true;
                    }

                } while (false);

                return lresult;
            }

            private Direct3DSurface9 GetSharedSurface(Direct3DDevice9Ex device)
            {
                return device.CreateRenderTarget(
                    m_width,
                    m_height,
                    Format, // D3DFMT_A8R8G8B8
                    0,
                    0,
                    0  // UNLOCKABLE
                    );

            }

            //private Direct3DTexture9 GetSharedSurface(Direct3DDevice9 device)
            //{

            //    return device.CreateTexture(
            //        m_width,
            //        m_height,
            //        1,
            //        1,  //D3DUSAGE_RENDERTARGET
            //        Format, 
            //        0  //D3DPOOL_DEFAULT
            //        );                
            //}



            private static Interop.Direct3DDevice9Ex CreateDevice(IntPtr handle)
            {
                const int D3D_SDK_VERSION = 32;
                using (var d3d9 = Interop.Direct3D9Ex.Create(D3D_SDK_VERSION))
                {
                    var present = new Interop.NativeStructs.D3DPRESENT_PARAMETERS();

                    try
                    {
                        var wih = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow);

                        if (wih.Handle != IntPtr.Zero)
                            handle = wih.Handle;
                    }
                    catch (Exception)
                    {
                    }

                    present.Windowed = 1; // TRUE
                    present.SwapEffect = 1; // D3DSWAPEFFECT_DISCARD
                    present.hDeviceWindow = handle;
                    present.PresentationInterval = unchecked((int)0x80000000); // D3DPRESENT_INTERVAL_IMMEDIATE;

                    return d3d9.CreateDeviceEx(
                        0, // D3DADAPTER_DEFAULT
                        1, // D3DDEVTYPE_HAL
                        handle,
                        70, // D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE
                        present,
                        null);
                }
            }

            //private static Direct3DDevice9 CreateDevice(IntPtr handle)
            //{
            //    const int D3D_SDK_VERSION = 32;
            //    using (var d3d9 = Direct3D9Ex.Create(D3D_SDK_VERSION))
            //    {
            //        var present = new NativeStructs.D3DPRESENT_PARAMETERS();

            //        try
            //        {
            //            var wih = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow);

            //            if (wih.Handle != IntPtr.Zero)
            //                handle = wih.Handle;
            //        }
            //        catch (Exception)
            //        {
            //        }

            //        present.BackBufferFormat = Format; 
            //        present.BackBufferHeight = 1;
            //        present.BackBufferWidth = 1;
            //        present.Windowed = 1; // TRUE
            //        present.SwapEffect = 1; // D3DSWAPEFFECT_DISCARD
            //        present.hDeviceWindow = handle;
            //        present.PresentationInterval = unchecked((int)0x80000000); // D3DPRESENT_INTERVAL_IMMEDIATE;

            //        return d3d9.CreateDevice(
            //            0, // D3DADAPTER_DEFAULT
            //            1, // D3DDEVTYPE_HAL
            //            handle,                        
            //            0x40 | 0x10,// D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_PUREDEVICE
            //            present,
            //            null);
            //    }
            //}
        }

        private System.Windows.Interop.D3DImage imageSource = null;

        private IntPtr sharedHandle = IntPtr.Zero;

        public IntPtr Handle { get { return sharedHandle; } }

        private Interop.Direct3DSurface9 sharedResource = null;

        internal Interop.Direct3DSurface9 SharedResource { get { return sharedResource; } }
        
        public VideoPanel()
        {
            var lTuple = D3D9Image.createD3D9Image();

            if(lTuple != null)
            {
                this.imageSource = lTuple.Item1;

                this.sharedResource = lTuple.Item2;

                this.sharedHandle = sharedResource.SharedHandle;
            }
            
            if (this.imageSource != null)
            {
                var image = new System.Windows.Controls.Image();
                image.Stretch = System.Windows.Media.Stretch.Uniform;
                image.Source = this.imageSource;
                this.AddChild(image);
                                
                // To greatly reduce flickering we're only going to AddDirtyRect
                // when WPF is rendering.
                System.Windows.Media.CompositionTarget.Rendering += this.CompositionTargetRendering;
            }
        }

        public byte[] takeScreenshot()
        {            
            byte[] l_result = null;

            var l_D3D9Image = imageSource as D3D9Image;

            if(l_D3D9Image != null)
            {
                var l_bitmap = l_D3D9Image.getBackBuffer();

                JpegBitmapEncoder l_encoder = new JpegBitmapEncoder();

                l_encoder.QualityLevel = 75;

                l_encoder.Frames.Add(BitmapFrame.Create(l_bitmap));

                using (var outputStream = new MemoryStream())
                {
                    l_encoder.Save(outputStream);

                    l_result = outputStream.ToArray();
                }
            }

            return l_result;
        }

        bool mSkip = false;

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            mSkip = !mSkip;

            if (mSkip)
                return;

            if (this.imageSource != null && this.imageSource.IsFrontBufferAvailable)
            {
                this.imageSource.Lock();
                this.imageSource.AddDirtyRect(new Int32Rect(0, 0, this.imageSource.PixelWidth, this.imageSource.PixelHeight));
                this.imageSource.Unlock();
            }

        }

    }
}
