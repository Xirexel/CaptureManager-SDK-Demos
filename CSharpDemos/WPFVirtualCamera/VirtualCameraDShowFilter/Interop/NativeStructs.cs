using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Interop
{
    [ComVisible(false)]
    internal static class NativeStructs
    {

        public static uint DXGI_FORMAT_B8G8R8A8_UNORM = 87;

        public static uint DXGI_FORMAT_B8G8R8X8_UNORM = 88;

        public static uint D3D11_BIND_RENDER_TARGET = 0x20;

        public static uint D3D11_RESOURCE_MISC_SHARED = 0x2;

        public static uint D3D11_USAGE_DEFAULT = 0x0;

        public static uint D3D11_USAGE_STAGING = 0x3;

        public static uint D3D11_CPU_ACCESS_WRITE = 0x10000;
        
        public static uint D3D11_CPU_ACCESS_READ = 0x20000;

        [StructLayout(LayoutKind.Sequential)]
        public sealed class D3D11_TEXTURE2D_DESC
        {
            public uint Width;
            public uint Height;
            public uint MipLevels;
            public uint ArraySize;
            public uint Format;
            public DXGI_SAMPLE_DESC SampleDesc;
            public uint Usage;
            public uint BindFlags;
            public uint CPUAccessFlags;
            public uint MiscFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class DXGI_SAMPLE_DESC
        {
            public uint Count;
            public uint Quality;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class D3D11_MAPPED_SUBRESOURCE
        {
            public IntPtr   pData;
            public uint     RowPitch;
            public uint     DepthPitch;

            public static int getSizeof()
            {
                return IntPtr.Size + sizeof(uint) + sizeof(uint);
            }
        };
    }
}
