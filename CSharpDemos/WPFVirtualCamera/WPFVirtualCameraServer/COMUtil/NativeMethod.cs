/****************************** Module Header ******************************\
* Module Name:  NativeMethod.cs
* Project:      CSCOMService
* Copyright (c) Microsoft Corporation.
* 
* The P/Invoke signatures of some native APIs.
* 
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/en-us/openness/licenses.aspx#MPL.
* All other rights reserved.
* 
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
* EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
* WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
#endregion


/// <summary>
/// Native methods
/// </summary>
internal class NativeMethod
{
    /// <summary>
    /// Get current thread ID.
    /// </summary>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    internal static extern uint GetCurrentThreadId();

    /// <summary>
    /// Get current process ID.
    /// </summary>
    [DllImport("kernel32.dll")]
    internal static extern uint GetCurrentProcessId();

    /// <summary>
    /// The GetMessage function retrieves a message from the calling thread's 
    /// message queue. The function dispatches incoming sent messages until a 
    /// posted message is available for retrieval. 
    /// </summary>
    /// <param name="lpMsg">
    /// Pointer to an MSG structure that receives message information from 
    /// the thread's message queue.
    /// </param>
    /// <param name="hWnd">
    /// Handle to the window whose messages are to be retrieved.
    /// </param>
    /// <param name="wMsgFilterMin">
    /// Specifies the integer value of the lowest message value to be 
    /// retrieved. 
    /// </param>
    /// <param name="wMsgFilterMax">
    /// Specifies the integer value of the highest message value to be 
    /// retrieved.
    /// </param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    internal static extern bool GetMessage(
        out MSG lpMsg, 
        IntPtr hWnd, 
        uint wMsgFilterMin, 
        uint wMsgFilterMax);

    /// <summary>
    /// The TranslateMessage function translates virtual-key messages into 
    /// character messages. The character messages are posted to the calling 
    /// thread's message queue, to be read the next time the thread calls the 
    /// GetMessage or PeekMessage function.
    /// </summary>
    /// <param name="lpMsg"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    internal static extern bool TranslateMessage([In] ref MSG lpMsg);

    /// <summary>
    /// The DispatchMessage function dispatches a message to a window 
    /// procedure. It is typically used to dispatch a message retrieved by 
    /// the GetMessage function.
    /// </summary>
    /// <param name="lpMsg"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    internal static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

    /// <summary>
    /// The PostThreadMessage function posts a message to the message queue 
    /// of the specified thread. It returns without waiting for the thread to 
    /// process the message.
    /// </summary>
    /// <param name="idThread">
    /// Identifier of the thread to which the message is to be posted.
    /// </param>
    /// <param name="Msg">Specifies the type of message to be posted.</param>
    /// <param name="wParam">
    /// Specifies additional message-specific information.
    /// </param>
    /// <param name="lParam">
    /// Specifies additional message-specific information.
    /// </param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    internal static extern bool PostThreadMessage(
        uint idThread, 
        uint Msg, 
        UIntPtr wParam,
        IntPtr lParam);


    internal const Int32 SWP_NOSIZE          = 0x0001;
    internal const Int32 SWP_NOMOVE          = 0x0002;
    internal const Int32 SWP_NOZORDER        = 0x0004;
    internal const Int32 SWP_NOREDRAW        = 0x0008;
    internal const Int32 SWP_NOACTIVATE      = 0x0010;
    internal const Int32 SWP_FRAMECHANGED    = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
    internal const Int32 SWP_SHOWWINDOW      = 0x0040;
    internal const Int32 SWP_HIDEWINDOW      = 0x0080;


    internal const Int32 SW_HIDE             = 0;
    internal const Int32 SW_SHOWNORMAL       = 1;
    internal const Int32 SW_NORMAL           = 1;
    internal const Int32 SW_SHOWMINIMIZED    = 2;
    internal const Int32 SW_SHOWMAXIMIZED    = 3;
    internal const Int32 SW_MAXIMIZE         = 3;
    internal const Int32 SW_SHOWNOACTIVATE   = 4;
    internal const Int32 SW_SHOW             = 5;

    internal const Int32 WM_QUIT = 0x0012;

    internal const Int32 WM_CLOSE = 0x0010;
            
    internal const Int32 WS_OVERLAPPEDWINDOW = 0x00800000;  //(WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX)
       
    internal const Int32 WS_CLIPSIBLINGS = 0x04000000;
    internal const Int32 WS_CLIPCHILDREN = 0x02000000;



    //
    // Window style information
    //

    public const int GWL_HINSTANCE = -6;
    public const int GWL_ID = -12;
    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;

    public const int WS_MINIMIZE = 0x20000000;
    public const int WS_MAXIMIZE = 0x01000000;
    public const int WS_THICKFRAME = 0x00040000;
    public const int WS_SYSMENU = 0x00080000;
    public const int WS_BORDER = 0x00800000;
    public const int WS_DLGFRAME = 0x00400000;
    public const int WS_CAPTION = 0x00C00000;
    public const int WS_MINIMIZEBOX = 0x00020000;
    public const int WS_MAXIMIZEBOX = 0x00010000;
    public const int WS_DISABLED = 0x08000000;
    public const int WS_CHILD = 0x40000000;
    public const int WS_POPUP = unchecked((int)0x80000000);
    public const int WS_VISIBLE = 0x10000000;

    public const int WS_EX_DLGMODALFRAME = 0x00000001;
    public const int WS_EX_TOPMOST = 0x00000008;
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int WS_EX_MDICHILD = 0x00000040;
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    public const int WS_EX_APPWINDOW = 0x00040000;
    public const int WS_EX_LAYERED = 0x00080000;
    public const int WS_EX_CONTROLPARENT = 0x00010000;
    public const int WS_EX_COMPOSITED = 0x02000000;

}

[StructLayout(LayoutKind.Sequential)]
internal struct MSG
{
    public IntPtr hWnd;
    public uint message;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public POINT pt;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}