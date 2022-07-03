using Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Interop.NativeStructs;

namespace WPFVirtualCameraServer.Tools
{
    [ComVisible(false)]
    internal class SharedTexture
    {
        private D3D11Texture2D m_shared_texture = null;

        private static SharedTexture m_Instance = null;

        public IntPtr SharedHadler { get; private set; }

        public static SharedTexture Instance { get { if (m_Instance == null) m_Instance = new SharedTexture(); return m_Instance; } }

        private SharedTexture()
        {
        }

        ~SharedTexture()
        {
            if (m_shared_texture != null)
                m_shared_texture.Dispose();
        }

        public void init(IntPtr a_sharedHandler)
        {
            if (a_sharedHandler == IntPtr.Zero)
                return;

            SharedHadler = a_sharedHandler;

            m_shared_texture = Direct3D11Device.Instance.Device.CreateTexture2D(a_sharedHandler);
        }

        public void Update(TargetTexture a_TargetTexture)
        {
            if(this.m_shared_texture != null)
            using (var lImmediateContext = Direct3D11Device.Instance.Device.GetImmediateContext())
            {
                // Copy image into shared texture            
                lImmediateContext.CopyResource(this.m_shared_texture.getD3D11Resource(), a_TargetTexture.getD3D11Resource());

                lImmediateContext.Flush();
            }
        }
    }
}
