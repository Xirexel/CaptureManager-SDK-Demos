using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WPFVirtualCameraServer.UI;

namespace WPFVirtualCameraServer
{
    [ComVisible(false)]
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            var aBlockEvent = new System.Threading.AutoResetEvent(false);

            // Run the out-of-process COM server
            COMUtil.ExeCOMServer.Instance.Run(new WPFVirtualCameraServer.ProxyObject(
                new WPFVirtualCameraServer.VirtualCameraServer(), () => {
                    try
                    {
                        aBlockEvent.WaitOne(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception)
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }),
                    new Action(() =>
                    {
                       new System.Windows.Application().Run(new ComponentWindow(() => { aBlockEvent.Set(); }));
                    }
            ));
        }
    }
}
