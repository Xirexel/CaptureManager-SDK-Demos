using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WPFDirectShowPlayerAsync
{
    class CMVRMultiSinkFactoryLoader
    {
        [DllImport("CMVRMultiSinkFactory.dll", CharSet = CharSet.Unicode)]
        public static extern int getMaxOutputNodeCount(out uint aPtrOutputNodeAmount);

        [DllImport("CMVRMultiSinkFactory.dll", CharSet = CharSet.Unicode)]
        public static extern int createOutputNodes(
            IntPtr aHandle,
            IntPtr aPtrUnkTarget,
            uint aOutputNodeAmount,
            out IntPtr aRefOutputNodes);

        [DllImport("CMVRMultiSinkFactory.dll", CharSet = CharSet.Unicode)]
        public static extern int createEVRStreamControl(
            out IntPtr aPtrPtrUnkIEVRStreamControl);


        //[Guid("47F9883C-77B1-4A0B-9233-B3EAFA8F387E")]
        //[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        //private interface IEVRStreamControl
        //{
        //    [DispId(1)]
        //    void setPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);
        //    [DispId(2)]
        //    void setZOrder(object aPtrEVROutputNode, uint aZOrder);
        //    [DispId(3)]
        //    void getPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);
        //    [DispId(4)]
        //    void getZOrder(object aPtrEVROutputNode, out uint aPtrZOrder);
        //    [DispId(5)]
        //    void flush(object aPtrEVROutputNode);
        //    [DispId(6)]
        //    void setSrcPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);
        //    [DispId(7)]
        //    void getSrcPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);
        //    [DispId(8)]
        //    void getCollectionOfFilters(object aPtrEVROutputNode, out string aPtrPtrXMLstring);
        //    [DispId(9)]
        //    void setFilterParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue, int aIsEnabled);
        //    [DispId(10)]
        //    void getCollectionOfOutputFeatures(object aPtrEVROutputNode, out string aPtrPtrXMLstring);
        //    [DispId(11)]
        //    void setOutputFeatureParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue);
        //}


        [Guid("47F9883C-77B1-4A0B-9233-B3EAFA8F387E")]
        //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [SuppressUnmanagedCodeSecurity]
        public interface IEVRStreamControl
        {
            [DispId(1)]
            void setPosition(IntPtr aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);
            [DispId(2)]
            void setZOrder(IntPtr aPtrEVROutputNode, uint aZOrder);
            [DispId(3)]
            void getPosition(IntPtr aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);
            [DispId(4)]
            void getZOrder(IntPtr aPtrEVROutputNode, out uint aPtrZOrder);
            [DispId(5)]
            void flush(IntPtr aPtrEVROutputNode);
            [DispId(6)]
            void setSrcPosition(IntPtr aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);
            [DispId(7)]
            void getSrcPosition(IntPtr aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);
            [DispId(8)]
            void getCollectionOfFilters(IntPtr aPtrEVROutputNode, out string aPtrPtrXMLstring);
            [DispId(9)]
            void setFilterParametr(IntPtr aPtrEVROutputNode, uint aParametrIndex, int aNewValue, int aIsEnabled);
            [DispId(10)]
            void getCollectionOfOutputFeatures(IntPtr aPtrEVROutputNode, out string aPtrPtrXMLstring);
            [DispId(11)]
            void setOutputFeatureParametr(IntPtr aPtrEVROutputNode, uint aParametrIndex, int aNewValue);
        }

        class EVRStreamControl : CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl
        {
            private CMVRMultiSinkFactoryLoader.IEVRStreamControl mIEVRStreamControl = null;

            public EVRStreamControl(
                CMVRMultiSinkFactoryLoader.IEVRStreamControl aIEVRStreamControl)
            {
                mIEVRStreamControl = aIEVRStreamControl;
            }

            public bool flush(object aPtrEVROutputNode)
            {
                bool lresult = false;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.flush(Marshal.GetIUnknownForObject(aPtrEVROutputNode));

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }

            public bool getPosition(
                object aPtrEVROutputNode,
                out float aPtrLeft,
                out float aPtrRight,
                out float aPtrTop,
                out float aPtrBottom)
            {
                bool lresult = false;

                aPtrLeft = 0.0f;

                aPtrRight = 0.0f;

                aPtrTop = 0.0f;

                aPtrBottom = 0.0f;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.getPosition(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            out aPtrLeft,
                            out aPtrRight,
                            out aPtrTop,
                            out aPtrBottom);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }

            public bool getZOrder(object aPtrEVROutputNode, out uint aPtrZOrder)
            {
                bool lresult = false;

                aPtrZOrder = 0;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.getZOrder(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            out aPtrZOrder);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }

            public bool setPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom)
            {
                bool lresult = false;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {                        
                        mIEVRStreamControl.setPosition(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            aLeft,
                            aRight,
                            aTop,
                            aBottom);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }

            public bool setZOrder(object aPtrEVROutputNode, uint aZOrder)
            {
                bool lresult = false;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.setZOrder(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            aZOrder);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }


            public bool setSrcPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom)
            {
                bool lresult = false;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.setSrcPosition(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            aLeft,
                            aRight,
                            aTop,
                            aBottom);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }

            public bool getSrcPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom)
            {
                bool lresult = false;

                aPtrLeft = 0.0f;

                aPtrRight = 0.0f;

                aPtrTop = 0.0f;

                aPtrBottom = 0.0f;

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.getSrcPosition(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            out aPtrLeft,
                            out aPtrRight,
                            out aPtrTop,
                            out aPtrBottom);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }


            public void getCollectionOfFilters(object aPtrEVROutputNode, out string aPtrPtrXMLstring)
            {
                aPtrPtrXMLstring = "";

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.getCollectionOfFilters(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            out aPtrPtrXMLstring);

                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);
            }

            public void getCollectionOfOutputFeatures(object aPtrEVROutputNode, out string aPtrPtrXMLstring)
            {
                aPtrPtrXMLstring = "";

                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.getCollectionOfOutputFeatures(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            out aPtrPtrXMLstring);

                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);
            }

            public void setFilterParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue, bool aIsEnabled)
            {
                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.setFilterParametr(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            aParametrIndex,
                            aNewValue,
                            aIsEnabled ? 1 : 0);

                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);
            }

            public void setOutputFeatureParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue)
            {
                do
                {
                    if (mIEVRStreamControl == null)
                        break;

                    if (aPtrEVROutputNode == null)
                        break;

                    try
                    {
                        mIEVRStreamControl.setOutputFeatureParametr(
                            Marshal.GetIUnknownForObject(aPtrEVROutputNode),
                            aParametrIndex,
                            aNewValue);

                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);
            }
        }


        class EVRMultiSinkFactory : IEVRMultiSinkFactory
        {
            public EVRMultiSinkFactory()
            {
            }

            public bool createOutputNodes(
                IntPtr aHWND,
                uint aOutputNodeAmount,
                out List<object> aTopologyOutputNodesList)
            {
                bool lresult = false;

                aTopologyOutputNodesList = new List<object>();

                do
                {

                    try
                    {
                        object lArrayMediaNodes = new Object();


                        //mIEVRMultiSinkFactory.createOutputNodes(
                        //    aHWND,
                        //    null,
                        //    aOutputNodeAmount,
                        //    out lArrayMediaNodes);

                        if (lArrayMediaNodes == null)
                            break;

                        object[] lArray = lArrayMediaNodes as object[];

                        if (lArray == null)
                            break;

                        aTopologyOutputNodesList.AddRange(lArray);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }


            public bool createOutputNodes(IntPtr aHandle, object aPtrUnkSharedResource, uint aOutputNodeAmount, out List<object> aTopologyOutputNodesList)
            {
                bool lresult = false;

                aTopologyOutputNodesList = new List<object>();

                do
                {

                    try
                    {
                        IntPtr lOutputNodes = IntPtr.Zero;

                        int lhresult = CMVRMultiSinkFactoryLoader.createOutputNodes(
                            aHandle,
                            Marshal.GetIUnknownForObject(aPtrUnkSharedResource),
                            aOutputNodeAmount,
                            out lOutputNodes);

                        IntPtr[] lOutputNodesArray = new IntPtr[aOutputNodeAmount];

                        Marshal.Copy(lOutputNodes, lOutputNodesArray, 0, (int)aOutputNodeAmount);

                        for (int i = 0; i < aOutputNodeAmount; i++)
                        {
                            aTopologyOutputNodesList.Add(Marshal.GetObjectForIUnknown(lOutputNodesArray[i]));

                            Marshal.Release(lOutputNodesArray[i]);
                        }

                        Marshal.FreeCoTaskMem(lOutputNodes);

                        lresult = true;
                    }
                    catch (Exception exc)
                    {
                    }

                } while (false);

                return lresult;
            }
        }

        public uint MaxPorts { get; set; }

        public CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl mIEVRStreamControl = null;

        private static CMVRMultiSinkFactoryLoader mInstance = null;

        public IEVRMultiSinkFactory mIEVRMultiSinkFactory = null;

        public static CMVRMultiSinkFactoryLoader getInstance()
        {
            if (mInstance == null)
            {
                mInstance = new CMVRMultiSinkFactoryLoader();
            }

            return mInstance;
        }

        private CMVRMultiSinkFactoryLoader()
        {

            do
            {
                mIEVRMultiSinkFactory = new EVRMultiSinkFactory();

                uint lMaxPorts = 0;

                MaxPorts = 0;

                var lhresult = getMaxOutputNodeCount(out lMaxPorts);

                if (lhresult != 0)
                    break;

                MaxPorts = lMaxPorts;


                IntPtr lPtrUnkIEVRStreamControl = IntPtr.Zero;

                lhresult = createEVRStreamControl(out lPtrUnkIEVRStreamControl);

                if (lhresult != 0)
                    break;

                var lObjUnkIEVRStreamControl = Marshal.GetObjectForIUnknown(lPtrUnkIEVRStreamControl);

                Marshal.Release(lPtrUnkIEVRStreamControl);
                
                var lIEVRStreamControl = lObjUnkIEVRStreamControl as CMVRMultiSinkFactoryLoader.IEVRStreamControl;

                if (lIEVRStreamControl == null)
                    break;
                
                mIEVRStreamControl = new EVRStreamControl(lIEVRStreamControl);
                
            } while (false);
        }
    }
}
