using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WPFMediaFoundationPlayerAsync
{
    class EVRMultiSink
    {
        [DllImport("EVRMultiSink.dll")]
        public static extern int createRendererOutputNodes(
                IntPtr aHandler,
                IntPtr aPtrRenderTarget,
                uint aStreamAmount,
                out IntPtr aPtrPtrTopologyNodeList);

        [DllImport("EVRMultiSink.dll")]
        public static extern int createEVRStreamControl(
                IntPtr aPtrTopologyNode,
                ref IntPtr aPtrPtrTopologyNode);

        [DllImport("EVRMultiSink.dll")]
        public static extern int getMaxVideoRenderStreamCount(
            ref uint aPtrOutputNodeAmount);
    }
}
