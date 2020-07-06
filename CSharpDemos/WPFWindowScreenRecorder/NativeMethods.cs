using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFWindowScreenRecorder
{
    class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint      biSize;
            public int       biWidth;
            public int       biHeight;
            public short     biPlanes;
            public short     biBitCount;
            public uint      biCompression;
            public uint      biSizeImage;
            public int       biXPelsPerMeter;
            public int       biYPelsPerMeter;
            public uint      biClrUsed;
            public uint      biClrImportant;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD 
        {
            public byte    rgbBlue;
            public byte    rgbGreen;
            public byte    rgbRed;
            public byte    rgbReserved;
        } ;

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO 
        {
            public BITMAPINFOHEADER bmiHeader;
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public RGBQUAD[] bmiColors;
        };


        [DllImport("User32.dll")]
        public static extern IntPtr WindowFromPhysicalPoint(POINT Point);

        [DllImport("User32.dll")]
        public static extern int GetCursorPos(IntPtr aPtrPoint);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int cch);

        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, IntPtr aPtrRect);

        [DllImport("User32.dll")]
        public static extern int GetClientRect(IntPtr hwnd, IntPtr aPtrRect);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, IntPtr pbmi, uint usage, out IntPtr ppvBits, IntPtr hSection, uint offset);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
        
        [DllImport("Gdi32.dll")]
        public static extern int BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

        [DllImport("Gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("Gdi32.dll")]
        public static extern int DeleteObject(IntPtr ho);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }
}
