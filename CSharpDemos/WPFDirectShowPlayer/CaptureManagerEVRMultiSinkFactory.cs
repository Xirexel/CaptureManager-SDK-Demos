using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDirectShowPlayer
{
    class CaptureManagerEVRMultiSinkFactory : ICaptureManagerEVRMultiSinkFactory
    {
        IEVRMultiSinkFactory mIEVRMultiSinkFactory = null;

        IEVRStreamControl mIEVRStreamControl = null;

        uint mMaxVideoRenderStreamCount = 0;

        public CaptureManagerEVRMultiSinkFactory(
            IEVRMultiSinkFactory aIEVRMultiSinkFactory,
            uint aMaxVideoRenderStreamCount,
            IEVRStreamControl aIEVRStreamControl)
        {
            mIEVRMultiSinkFactory = aIEVRMultiSinkFactory;

            mMaxVideoRenderStreamCount = aMaxVideoRenderStreamCount;

            mIEVRStreamControl = aIEVRStreamControl;
        }

        public bool createOutputNodes(IntPtr aHandle, object aPtrUnkSharedResource, uint aOutputNodeAmount, out List<object> aTopologyOutputNodesList)
        {
            if (mIEVRMultiSinkFactory == null)
            {
                aTopologyOutputNodesList = new List<object>();

                return false;
            }

            return mIEVRMultiSinkFactory.createOutputNodes(aHandle, aPtrUnkSharedResource, aOutputNodeAmount, out aTopologyOutputNodesList);
        }
        
        public uint getMaxVideoRenderStreamCount()
        {
            return mMaxVideoRenderStreamCount;
        }

        public IEVRStreamControl getIEVRStreamControl()
        {
            return mIEVRStreamControl;
        }
    }
}
