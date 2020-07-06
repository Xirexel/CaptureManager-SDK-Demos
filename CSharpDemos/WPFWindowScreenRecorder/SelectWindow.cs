using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WPFWindowScreenRecorder
{
    delegate void updateWindow(string a_Name, IntPtr a_HWND);

    delegate void pressedKey(char a_KeyChar);

    delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    class SelectWindow
    {
        gma.System.Windows.UserActivityHook actHook;

        private const int WH_MOUSE = 7;
        private const int WM_LBUTTONDOWN = 0x0201;



        private SelectWindow() { }

        private static SelectWindow m_Instance = null;
     
        public static event updateWindow m_updateWindowNameEvent;

        public static event pressedKey m_pressedKey;

        public static SelectWindow getInstance()
        {
            if (m_Instance == null)
                m_Instance = new SelectWindow();

            return m_Instance;
        }

        public void setupMouseHook()
        {
            actHook = new gma.System.Windows.UserActivityHook();

            actHook.OnMouseActivity += actHook_OnMouseActivity;

            actHook.KeyPress += actHook_KeyPress;
        }

        void actHook_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (m_pressedKey != null)
                m_pressedKey(e.KeyChar);
        }

        public void uninstallMouseHook()
        {
            actHook.Stop();
        }


        void actHook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {           
            if (m_updateWindowNameEvent != null)
            {
                NativeMethods.POINT pt = new NativeMethods.POINT();

                IntPtr buf = Marshal.AllocHGlobal(
                Marshal.SizeOf(typeof(NativeMethods.POINT)));

                var h = NativeMethods.GetCursorPos(buf);

                pt = (NativeMethods.POINT)Marshal.PtrToStructure(buf, typeof(NativeMethods.POINT));
                
                Marshal.FreeHGlobal(buf);

                IntPtr hwnd = NativeMethods.WindowFromPhysicalPoint(pt);

                m_updateWindowNameEvent(GetCaptionOfWindow(hwnd), hwnd);                                
            }
        }
        
        static string GetCaptionOfWindow(IntPtr hwnd)
        {
            string caption = "";
            StringBuilder windowText = null;
            try
            {
                int max_length = NativeMethods.GetWindowTextLength(hwnd);
                windowText = new StringBuilder("", max_length + 5);
                NativeMethods.GetWindowText(hwnd, windowText, max_length + 2);

                if (!String.IsNullOrEmpty(windowText.ToString()) && !String.IsNullOrWhiteSpace(windowText.ToString()))
                    caption = windowText.ToString();
            }
            catch (Exception ex)
            {
                caption = ex.Message;
            }
            finally
            {
                windowText = null;
            }
            return caption;
        } 

    }
}
