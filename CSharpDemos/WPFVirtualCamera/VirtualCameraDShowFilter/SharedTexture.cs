using Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VirtualCameraDShowFilter
{
    [ComVisible(false)]
    internal class SharedTexture
    {
        private D3D11Texture2D m_shared_texture = null;
         
        private SharedTexture()
        {
        }

        ~SharedTexture()
        {
            if (m_shared_texture != null)
                m_shared_texture.Dispose();
        }

        public static SharedTexture createSharedTexture(IntPtr a_sharedHandler)
        {
            SharedTexture l_resturn = new SharedTexture();

            if (a_sharedHandler == IntPtr.Zero)
                return l_resturn;

            l_resturn.m_shared_texture = Direct3D11Device.Instance.Device.CreateTexture2D(a_sharedHandler);

            return l_resturn;
        }

        public D3D11Resource getD3D11Resource()
        {
            if (m_shared_texture == null)
                return null;

            return m_shared_texture.getD3D11Resource();
        }

        public NativeStructs.D3D11_TEXTURE2D_DESC GetDesc()
        {
            NativeStructs.D3D11_TEXTURE2D_DESC l_resturn = new NativeStructs.D3D11_TEXTURE2D_DESC();

            if(m_shared_texture != null)
                l_resturn = m_shared_texture.GetDesc();

            return l_resturn;
        }        
    }
}