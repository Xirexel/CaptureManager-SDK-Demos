using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Interop
{
    [ComVisible(false)]
    internal class D3D11Resource
    {
        private ComInterface.ID3D11Resource comObject;
        private IntPtr native = IntPtr.Zero;

        public ComInterface.ID3D11Resource ComObject { get { return comObject; } }

        public IntPtr Native { get { return native; } }

        internal D3D11Resource(ComInterface.ID3D11Resource obj)
        {
            this.comObject = obj;
            native = Marshal.GetIUnknownForObject(comObject);
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
                System.Runtime.InteropServices.Marshal.ReleaseComObject(this.comObject);
                this.comObject = null;
            }
        }
    }
}
