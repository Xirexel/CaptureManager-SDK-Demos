using System;
using System.Runtime.InteropServices;

namespace WPFViewerTrigger.Interop
{
    internal sealed class Direct3DDevice9Ex : IDisposable
    {
        private ComInterface.IDirect3DDevice9Ex comObject;
        private ComInterface.CreateRenderTarget createRenderTarget;

        internal Direct3DDevice9Ex(ComInterface.IDirect3DDevice9Ex obj)
        {
            this.comObject = obj;
            ComInterface.GetComMethod(this.comObject, 28, out this.createRenderTarget);
        }

        ~Direct3DDevice9Ex()
        {
            this.Release();
        }

        public void Dispose()
        {
            this.Release();
            GC.SuppressFinalize(this);
        }
        
        public Direct3DSurface9 CreateRenderTarget(uint Width, uint Height, int Format, int MultiSample, uint MultisampleQuality, int Lockable, ref IntPtr pSharedHandle)
        {
            ComInterface.IDirect3DSurface9 obj = null;
            int result = this.createRenderTarget(this.comObject, Width, Height, Format, MultiSample, MultisampleQuality, Lockable, out obj, ref pSharedHandle);
            Marshal.ThrowExceptionForHR(result);

            return new Direct3DSurface9(obj);
        }

        private void Release()
        {
            if (this.comObject != null)
            {
                Marshal.ReleaseComObject(this.comObject);
                this.comObject = null;
                this.createRenderTarget = null;
            }
        }
    }
}
