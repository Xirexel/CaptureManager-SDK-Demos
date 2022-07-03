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
    internal sealed class ComInterface
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int CreateTexture2D(ID3D11Device a_device, D3D11_TEXTURE2D_DESC pDesc, IntPtr pInitialData, out ID3D11Texture2D ppTexture2D);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetImmediateContext(ID3D11Device a_device, out ID3D11DeviceContext ppImmediateContext);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int OpenSharedResource(ID3D11Device a_device, IntPtr hResource, ref Guid ReturnedInterface, out IntPtr ppResource);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int GetSharedHandle(IDXGIResource a_resource, out IntPtr pSharedHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CopyResource(ID3D11DeviceContext a_context, ID3D11Resource pDstResource, ID3D11Resource pSrcResource);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int Map(ID3D11DeviceContext a_context, ID3D11Resource pResource, uint Subresource, uint MapType, uint MapFlags, D3D11_MAPPED_SUBRESOURCE pMappedResource);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Unmap(ID3D11DeviceContext a_context, ID3D11Resource pResource, uint Subresource);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Flush(ID3D11DeviceContext a_context);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetDesc(ID3D11Texture2D a_texture, IntPtr pDesc);



        [ComImport, Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ID3D11Device
        {
        }

        [ComImport, Guid("1841e5c8-16b0-489b-bcc8-44cfb0d5deae"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ID3D11DeviceChild
        {
        }

        [ComImport, Guid("c0bfa96c-e089-44fb-8eaf-26f8796190da"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ID3D11DeviceContext
        {
        }        

        [ComImport, Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ID3D11Texture2D
        {
        }

        [ComImport, Guid("dc8e63f3-d12b-4952-b47b-5e45026a862d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ID3D11Resource
        {
        }

        [ComImport, Guid("035f3ab4-482e-4e50-b41f-8a7f8bd8960b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDXGIResource
        {
        }


        // This is a helper method that accesses the COM objects v-table and
        // turns it into a delegate.
        public static bool GetComMethod<T, U>(T comObj, int slot, out U method) where U : class
        {
            IntPtr objectAddress = Marshal.GetComInterfaceForObject(comObj, typeof(T));
            if (objectAddress == IntPtr.Zero)
            {
                method = null;
                return false;
            }

            try
            {
                IntPtr vTable = Marshal.ReadIntPtr(objectAddress, 0);
                IntPtr methodAddress = Marshal.ReadIntPtr(vTable, slot * IntPtr.Size);

                // We can't have a Delegate constraint, so we have to cast to
                // object then to our desired delegate
                method = (U)((object)Marshal.GetDelegateForFunctionPointer(methodAddress, typeof(U)));
                return true;
            }
            finally
            {
                Marshal.Release(objectAddress); // Prevent memory leak
            }
        }
    }
}
