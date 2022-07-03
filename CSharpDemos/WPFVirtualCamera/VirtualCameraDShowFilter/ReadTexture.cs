using Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Interop.NativeStructs;

namespace VirtualCameraDShowFilter
{
    [ComVisible(false)]
    class ReadTexture
    {
        private D3D11Texture2D m_read_texture = null;

        private uint m_BufferLength = 0;

        private ReadTexture()
        {
        }

        ~ReadTexture()
        {
            if (m_read_texture != null)
                m_read_texture.Dispose();
        }

        public static ReadTexture cretaeReadTexture(NativeStructs.D3D11_TEXTURE2D_DESC a_TextureDesc)
        {

            a_TextureDesc.BindFlags = 0;

            a_TextureDesc.MiscFlags = 0;

            a_TextureDesc.CPUAccessFlags = NativeStructs.D3D11_CPU_ACCESS_READ | NativeStructs.D3D11_CPU_ACCESS_WRITE; ;

            a_TextureDesc.Usage = NativeStructs.D3D11_USAGE_STAGING;

            ReadTexture l_return = new ReadTexture();

            l_return.m_read_texture = Direct3D11Device.Instance.Device.CreateTexture2D(a_TextureDesc);

            using (var lImmediateContext = Direct3D11Device.Instance.Device.GetImmediateContext())
            {

                D3D11_MAPPED_SUBRESOURCE resource = new D3D11_MAPPED_SUBRESOURCE();
                var subresource = D3D11DeviceContext.D3D11CalcSubresource(0, 0, 0);

                var lres = lImmediateContext.Map(l_return.m_read_texture.getD3D11Resource(), subresource, D3D11DeviceContext.D3D11_MAP_READ_WRITE, 0, resource);

                l_return.m_BufferLength = resource.RowPitch * a_TextureDesc.Height;

                lImmediateContext.Unmap(l_return.m_read_texture.getD3D11Resource(), subresource);
            }

            return l_return;
        }

        public void Read(SharedTexture a_TargetTexture, IntPtr aDest)
        {
            using (var lImmediateContext = Direct3D11Device.Instance.Device.GetImmediateContext())
            {

                // Copy image into CPU access texture            
                lImmediateContext.CopyResource(this.m_read_texture.getD3D11Resource(), a_TargetTexture.getD3D11Resource());

                lImmediateContext.Flush();

                // Copy from CPU access texture to bitmap buffer

                D3D11_MAPPED_SUBRESOURCE resource = new D3D11_MAPPED_SUBRESOURCE();
                var subresource = D3D11DeviceContext.D3D11CalcSubresource(0, 0, 0);
                var lres = lImmediateContext.Map(this.m_read_texture.getD3D11Resource(), subresource, D3D11DeviceContext.D3D11_MAP_READ_WRITE, 0, resource);

                NativeMethods.memcpy(aDest, resource.pData, m_BufferLength);

                lImmediateContext.Unmap(this.m_read_texture.getD3D11Resource(), subresource);
            }

        }
    }
}
