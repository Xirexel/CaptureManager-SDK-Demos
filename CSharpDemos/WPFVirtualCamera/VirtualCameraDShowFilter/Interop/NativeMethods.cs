using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Interop
{
    [ComVisible(false)]
    internal static class NativeMethods
    {
        [ComVisible(false)]
        [Flags]
        public enum CLSCTX : uint
        {
            CLSCTX_INPROC_SERVER = 0x1,
            CLSCTX_INPROC_HANDLER = 0x2,
            CLSCTX_LOCAL_SERVER = 0x4,
            CLSCTX_INPROC_SERVER16 = 0x8,
            CLSCTX_REMOTE_SERVER = 0x10,
            CLSCTX_INPROC_HANDLER16 = 0x20,
            CLSCTX_RESERVED1 = 0x40,
            CLSCTX_RESERVED2 = 0x80,
            CLSCTX_RESERVED3 = 0x100,
            CLSCTX_RESERVED4 = 0x200,
            CLSCTX_NO_CODE_DOWNLOAD = 0x400,
            CLSCTX_RESERVED5 = 0x800,
            CLSCTX_NO_CUSTOM_MARSHAL = 0x1000,
            CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
            CLSCTX_NO_FAILURE_LOG = 0x4000,
            CLSCTX_DISABLE_AAA = 0x8000,
            CLSCTX_ENABLE_AAA = 0x10000,
            CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
            CLSCTX_ACTIVATE_32_BIT_SERVER = 0x40000,
            CLSCTX_ACTIVATE_64_BIT_SERVER = 0x80000,
            CLSCTX_INPROC = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,
            CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
            CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER
        }

        public const string IID_IClassFactory =
            "00000001-0000-0000-C000-000000000046";

        [ComImport(), ComVisible(false),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid(IID_IClassFactory)]
        internal interface IClassFactory
        {
            /// <summary>
            /// Creates an uninitialized object.
            /// </summary>
            /// <param name="pUnkOuter"></param>
            /// <param name="riid">
            /// Reference to the identifier of the interface to be used to 
            /// communicate with the newly created object. If pUnkOuter is NULL, this
            /// parameter is frequently the IID of the initializing interface.
            /// </param>
            /// <param name="ppvObject">
            /// Address of pointer variable that receives the interface pointer 
            /// requested in riid. 
            /// </param>
            /// <returns>S_OK means success.</returns>
            [PreserveSig]
            int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);

            /// <summary>
            /// Locks object application open in memory.
            /// </summary>
            /// <param name="fLock">
            /// If TRUE, increments the lock count; 
            /// if FALSE, decrements the lock count.
            /// </param>
            /// <returns>S_OK means success.</returns>
            [PreserveSig]
            int LockServer(bool fLock);
        }


        [DllImport("d3d11.dll")]
        public static extern int D3D11CreateDevice(IntPtr a_Adapter, uint a_D3D_DRIVER_TYPE, IntPtr a_HMODULE, uint a_Flags, IntPtr a_D3D_FEATURE_LEVELs, uint a_levels_Count, uint a_D3D11_SDK_VERSION, out ComInterface.ID3D11Device a_ID3D11Device, IntPtr a_D3D_FEATURE_LEVEL, out IntPtr a_ID3D11DeviceContext);
        
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, uint count);

        [DllImport("ole32.dll")]
        public static extern int CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid pclsid);

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern object CoGetClassObject(
                       [In, MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
                       CLSCTX dwClsContext,
                       IntPtr pServerInfo,
                       [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }
}
