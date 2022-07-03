using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VirtualCameraDShowFilter
{

    [ComVisible(false)]
    [Guid("EEE2F595-722F-4279-B919-313189A72C36")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IVirtualCameraServer
    {
        IntPtr get_DirectX11TextureHandler(out int retVal);
    }

    [ComVisible(false)]
    public class CapturePipeline
    {

        [ComVisible(false)]
        private class RemoteAccess : IDisposable
        {
            private object m_ClassFactoryObj = null;

            private IVirtualCameraServer m_IVirtualCameraServer = null;

            public SharedTexture m_SharedTexture = null;

            public ReadTexture m_ReadTexture = null;

            public Interop.NativeStructs.D3D11_TEXTURE2D_DESC TextureDesc = new Interop.NativeStructs.D3D11_TEXTURE2D_DESC();


            public RemoteAccess()
            {
                var res = Interop.NativeMethods.CLSIDFromProgID("WPFVirtualCameraServer.VirtualCameraServer", out Guid clsid);

                if (res != 0)
                    return;

                var l_IID_IClassFactory = new Guid(Interop.NativeMethods.IID_IClassFactory);

                var l_IID_IVirtualCameraServer = typeof(IVirtualCameraServer).GUID;




                m_ClassFactoryObj = Interop.NativeMethods.CoGetClassObject(clsid, Interop.NativeMethods.CLSCTX.CLSCTX_LOCAL_SERVER, IntPtr.Zero, l_IID_IClassFactory);

                var l_ClassFactory = m_ClassFactoryObj as Interop.NativeMethods.IClassFactory;

                if (l_ClassFactory == null)
                {
                    m_ClassFactoryObj = null;

                    return;
                }

                IntPtr l_ptr = IntPtr.Zero;

                res = l_ClassFactory.CreateInstance(IntPtr.Zero, ref l_IID_IVirtualCameraServer, out l_ptr);

                if (res != 0)
                    return;

                m_IVirtualCameraServer = Marshal.GetObjectForIUnknown(l_ptr) as IVirtualCameraServer;

                if (m_IVirtualCameraServer == null)
                    return;

                var l_sharedHadler = m_IVirtualCameraServer.get_DirectX11TextureHandler(out res);

                if (res != 0)
                    return;

                m_SharedTexture = SharedTexture.createSharedTexture(l_sharedHadler);

                if (m_SharedTexture == null)
                    return;

                TextureDesc = m_SharedTexture.GetDesc();

                m_ReadTexture = ReadTexture.cretaeReadTexture(TextureDesc);
            }



            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {

                    }

                    if(m_IVirtualCameraServer != null)
                        Marshal.ReleaseComObject(m_IVirtualCameraServer);

                    if (m_ClassFactoryObj != null)
                        Marshal.ReleaseComObject(m_ClassFactoryObj);

                    m_SharedTexture = null;

                    m_IVirtualCameraServer = null;

                    m_ClassFactoryObj = null;

                    disposedValue = true;
                }
            }

            // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
            ~RemoteAccess()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        RemoteAccess m_RemoteAccess = null;

        public uint VideoWidth { get; private set; }

        public uint VideoHeight { get; private set; }

        public static CapturePipeline Instance { get; private set; } = new CapturePipeline();

        private CapturePipeline()
        {
            using (var l_RemoteAccess = new RemoteAccess())
            {

                VideoWidth = l_RemoteAccess.TextureDesc.Width;

                VideoHeight = l_RemoteAccess.TextureDesc.Height;

            }
        }

        public void Start()
        {
            m_RemoteAccess = new RemoteAccess();
        }
           
        public void Stop()
        {
            m_RemoteAccess.Dispose();

            m_RemoteAccess = null;
        }

        public void updateData(IntPtr aPtr)
        {
            if(m_RemoteAccess != null)
                m_RemoteAccess.m_ReadTexture.Read(m_RemoteAccess.m_SharedTexture, aPtr);
        }
    }
}
