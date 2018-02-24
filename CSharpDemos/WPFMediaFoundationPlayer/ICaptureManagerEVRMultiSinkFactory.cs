using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMediaFoundationPlayer
{
    interface ICaptureManagerEVRMultiSinkFactory
    {
        bool createOutputNodes(
            IntPtr aHandle,
            object aPtrUnkSharedResource,
            uint aOutputNodeAmount,
            out List<object> aTopologyOutputNodesList);

        uint getMaxVideoRenderStreamCount();

        CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl getIEVRStreamControl();
    }
}
