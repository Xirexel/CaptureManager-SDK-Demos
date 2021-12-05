using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMediaFoundationPlayerAsync
{
    interface ICaptureManagerEVRMultiSinkFactoryAsync
    {
        Task<List<object>> createOutputNodesAsync(
            IntPtr aHandle,
            object aPtrUnkSharedResource,
            uint aOutputNodeAmount);

        Task<uint> getMaxVideoRenderStreamCountAsync();

        Task<CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControlAsync> getIEVRStreamControlAsync();
    }
}
