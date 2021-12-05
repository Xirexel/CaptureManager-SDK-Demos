using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WPFMediaFoundationPlayerAsync
{
    class CaptureManagerVideoRendererMultiSinkFactory
    {

        private CaptureManagerVideoRendererMultiSinkFactory()
        {
            LibraryIndex = 0;
        }

        private async Task<ICaptureManagerEVRMultiSinkFactoryAsync> uploadCaptureManagerToCSharpProxy()
        {
            ICaptureManagerEVRMultiSinkFactoryAsync aICaptureManagerEVRMultiSinkFactory = null;

            do
            {
                CaptureManager mCaptureManager = null;

                try
                {
                    mCaptureManager = new CaptureManager("CaptureManager.dll");
                }
                catch (Exception)
                {
                    try
                    {
                        mCaptureManager = new CaptureManager();
                    }
                    catch (Exception)
                    {

                    }
                }

                if (mCaptureManager == null)
                    break;

                string lXMLSinkString = await mCaptureManager.getCollectionOfSinksAsync();

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(lXMLSinkString);

                var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{A2224D8D-C3C1-4593-8AC9-C0FCF318FF05}']");

                if (lSinkNode == null)
                    break;

                var lMaxPortCountAttributeNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart/@MaxPortCount");

                if (lMaxPortCountAttributeNode == null)
                    break;

                uint lmaxPorts = 0;

                if (!uint.TryParse(lMaxPortCountAttributeNode.Value, out lmaxPorts))
                    break;

                if (lmaxPorts == 0)
                    break;

//                "<SinkFactory Name="CaptureManagerVRMultiSinkFactory" GUID="{A2224D8D-C3C1-4593-8AC9-C0FCF318FF05}" Title="CaptureManager Video Renderer multi sink factory">
//- <Value.ValueParts>
//  <ValuePart Title="Container format" Value="Default" MIME="" Description="Default EVR implementation" MaxPortCount="8" GUID="{E926E7A7-7DD0-4B15-88D7-413704AF865F}" /> 
//  </Value.ValueParts>
//  </SinkFactory>
//"


                var lISinkControl = await mCaptureManager.createSinkControlAsync();

                if (lISinkControl == null)
                    break;

                var lIEVRMultiSinkFactory = await lISinkControl.createCompatibleEVRMultiSinkFactoryAsync(Guid.Empty);

                if (lIEVRMultiSinkFactory == null)
                    break;

                var lIEVRStreamControl = await mCaptureManager.createEVRStreamControlAsync();

                if (lIEVRStreamControl == null)
                    break;

                aICaptureManagerEVRMultiSinkFactory = new CaptureManagerEVRMultiSinkFactory(
                    lIEVRMultiSinkFactory,
                    lmaxPorts,
                    lIEVRStreamControl);
                                                
            }
            while (false);

            return aICaptureManagerEVRMultiSinkFactory;
        }

        private async Task<ICaptureManagerEVRMultiSinkFactoryAsync> uploadCaptureManagerVideoRendererToCSharpProxy()
        {
            ICaptureManagerEVRMultiSinkFactoryAsync aICaptureManagerEVRMultiSinkFactory = null;

            //bool lresult = false;

            //do
            //{
            //    aICaptureManagerEVRMultiSinkFactory = new CaptureManagerEVRMultiSinkFactory(
            //        CMVRMultiSinkFactoryLoader.getInstance().mIEVRMultiSinkFactory,
            //        CMVRMultiSinkFactoryLoader.getInstance().MaxPorts,
            //        CMVRMultiSinkFactoryLoader.getInstance().mIEVRStreamControl);

            //    lresult = true;
            //}
            //while (false);

            return aICaptureManagerEVRMultiSinkFactory;
        }

        private static CaptureManagerVideoRendererMultiSinkFactory mInstance = null;

        public static CaptureManagerVideoRendererMultiSinkFactory getInstance()
        {
            if (mInstance == null)
            {
                mInstance = new CaptureManagerVideoRendererMultiSinkFactory();
            }

            return mInstance;
        }

        public async Task<ICaptureManagerEVRMultiSinkFactoryAsync> getICaptureManagerEVRMultiSinkFactory()
        {

            ICaptureManagerEVRMultiSinkFactoryAsync mICaptureManagerEVRMultiSinkFactory = null;


            if(LibraryIndex == 0)
            {
                mICaptureManagerEVRMultiSinkFactory = await uploadCaptureManagerToCSharpProxy();
            }
            else if (LibraryIndex == 1)
            {
                mICaptureManagerEVRMultiSinkFactory = await uploadCaptureManagerVideoRendererToCSharpProxy();
            }


            return mICaptureManagerEVRMultiSinkFactory;
        }

        public int LibraryIndex { get; set; }
    }
}
