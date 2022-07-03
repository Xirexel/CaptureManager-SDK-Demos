using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Interop.NativeStructs;

namespace Interop
{
    [ComVisible(false)]
    internal sealed class D3D11DeviceContext : IDisposable
    {
        public const uint D3D11_MAP_READ = 1;
        public const uint D3D11_MAP_WRITE = 2;
        public const uint D3D11_MAP_READ_WRITE = 3;
        public const uint D3D11_MAP_WRITE_DISCARD = 4;
        public const uint D3D11_MAP_WRITE_NO_OVERWRITE = 5;
   

        private ComInterface.ID3D11DeviceContext    comObject;
        private ComInterface.CopyResource           copyResource;
        private ComInterface.Map                    map;
        private ComInterface.Unmap                  unmap;
        private ComInterface.Flush                  flush;

        internal D3D11DeviceContext(ComInterface.ID3D11DeviceContext obj)
        {
            this.comObject = obj;
            ComInterface.GetComMethod(this.comObject, 14, out this.map);
            ComInterface.GetComMethod(this.comObject, 15, out this.unmap);
            ComInterface.GetComMethod(this.comObject, 47, out this.copyResource);
            ComInterface.GetComMethod(this.comObject, 111, out this.flush);
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
                this.map = null;
                this.unmap = null;
                this.copyResource = null;
            }
        }
       
        static public uint D3D11CalcSubresource(uint MipSlice, uint ArraySlice, uint MipLevels){ 
            return MipSlice + ArraySlice * MipLevels; 
        }

        public int Map(D3D11Resource pResource, uint Subresource, uint MapType, uint MapFlags, D3D11_MAPPED_SUBRESOURCE pMappedResource)
        {
            return map(this.comObject, pResource.ComObject, Subresource, MapType, MapFlags, pMappedResource);
        }

        public void Unmap(D3D11Resource pResource, uint Subresource)
        {
            unmap(this.comObject, pResource.ComObject, Subresource);
        }

        public void CopyResource(D3D11Resource pDstResource, D3D11Resource pSrcResource)
        {
            copyResource(this.comObject, pDstResource.ComObject, pSrcResource.ComObject);
        }

        public void Flush()
        {
            flush(this.comObject);
        }
    }
}
