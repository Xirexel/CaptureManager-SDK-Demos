using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMediaFoundationPlayerAsync
{
    class CaptureManagerEVRMultiSinkFactory : ICaptureManagerEVRMultiSinkFactoryAsync
    {
        IEVRMultiSinkFactoryAsync mIEVRMultiSinkFactory = null;

        IEVRStreamControlAsync mIEVRStreamControl = null;

        uint mMaxVideoRenderStreamCount = 0;

        public CaptureManagerEVRMultiSinkFactory(
            IEVRMultiSinkFactoryAsync aIEVRMultiSinkFactory,
            uint aMaxVideoRenderStreamCount,
            IEVRStreamControlAsync aIEVRStreamControl)
        {
            mIEVRMultiSinkFactory = aIEVRMultiSinkFactory;

            mMaxVideoRenderStreamCount = aMaxVideoRenderStreamCount;

            mIEVRStreamControl = aIEVRStreamControl;
        }

        public async Task<List<object>> createOutputNodesAsync(IntPtr aHandle, object aPtrUnkSharedResource, uint aOutputNodeAmount)
        {
            return await mIEVRMultiSinkFactory.createOutputNodesAsync(aHandle, aPtrUnkSharedResource, aOutputNodeAmount);
        }
        
        public async Task<uint> getMaxVideoRenderStreamCountAsync()
        {
            return mMaxVideoRenderStreamCount;
        }

        public async Task<IEVRStreamControlAsync> getIEVRStreamControlAsync()
        {
            return mIEVRStreamControl;
        }
    }
}
