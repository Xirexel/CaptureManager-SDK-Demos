using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Interop
{
    [ComVisible(false)]
    internal sealed class D3D11Texture2D : IDisposable
    {
        private ComInterface.ID3D11Texture2D comObject;
        private IntPtr native;

        public IntPtr Native { get { return native; } }

        private ComInterface.GetDesc getDesc;

        internal D3D11Texture2D(ComInterface.ID3D11Texture2D obj)
        {
            this.comObject = obj;
            ComInterface.GetComMethod(this.comObject, 10, out this.getDesc);
            this.native = Marshal.GetIUnknownForObject(this.comObject);
        }

        public void Dispose()
        {
            this.Release();
            GC.SuppressFinalize(this);
        }

        private void Release()
        {
            if (this.comObject != null)
            {
                Marshal.ReleaseComObject(this.comObject);
                this.comObject = null;
                this.getDesc = null;
            }
        }

        public NativeStructs.D3D11_TEXTURE2D_DESC GetDesc()
        {
            var l_size = Marshal.SizeOf(new NativeStructs.D3D11_TEXTURE2D_DESC());

            var l_Memory = Marshal.AllocHGlobal(l_size);

            getDesc(this.comObject, l_Memory);

            var l_Desc = Marshal.PtrToStructure<NativeStructs.D3D11_TEXTURE2D_DESC>(l_Memory);

            Marshal.FreeHGlobal(l_Memory);

            return l_Desc;
        }

        public DXGIResource getDXGIResource()
        {
            ComInterface.IDXGIResource l_obj = null;

            l_obj = comObject as ComInterface.IDXGIResource;

            return new DXGIResource(l_obj);
        }

        public D3D11Resource getD3D11Resource()
        {
            ComInterface.ID3D11Resource l_obj = null;

            l_obj = comObject as ComInterface.ID3D11Resource;

            return new D3D11Resource(l_obj);
        }        
    }
}
