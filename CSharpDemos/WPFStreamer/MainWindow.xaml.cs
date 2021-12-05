using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace WPFStreamer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WSAData
    {
        public short version;
        public short highVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
        public string description;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
        public string systemStatus;
        public short maxSockets;
        public short maxUdpDg;
        public IntPtr vendorInfo;
    }

    public static class NativeMethods
    {
        [DllImport("Ws2_32.dll")]
        public static extern Int32 WSAStartup(short wVersionRequested, ref WSAData wsaData);

        [DllImport("Ws2_32.dll")]
        public static extern Int32 WSACleanup();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool m_socketAccessable = false;

        CaptureManager mCaptureManager = null;

        IEVRStreamControl mIEVRStreamControl = null;

        ISession mISession = null;

        ISourceControl mSourceControl;

        Guid MFMediaType_Video = new Guid(
 0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

        Guid MFVideoFormat_H264 = new Guid("34363248-0000-0010-8000-00AA00389B71");

        Guid MFMediaType_Audio = new Guid(
0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

        Guid MFAudioFormat_AAC = new Guid(
0x1610, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        Guid StreamingCBR = new Guid("8F6FF1B6-534E-49C0-B2A8-16D534EAF135");


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                ThreadPool.SetMinThreads(15, 10);

                mCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (System.Exception exc)
            {
                try
                {
                    mCaptureManager = new CaptureManager();
                }
                catch (System.Exception exc1)
                {

                }
            }

            if (mCaptureManager == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;


            mIEVRStreamControl = mCaptureManager.createEVRStreamControl();

            mSourceControl = mCaptureManager.createSourceControl();
        }

        RtmpClient s = null;
        
        private void startServer(string a_streamsXml)
        {
            if (s == null)
                s = RtmpClient.createInstance(a_streamsXml, mStreamSiteComboBox.Text);
        }

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_socketAccessable)
                return;

            if (mLaunchButton.Content == "Stop")
            {
                if (mISession != null)
                {
                    mISession.closeSession();

                    mLaunchButton.Content = "Launch";
                }  

                mISession = null;

                return;
            }


            string lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            var lReadMode = setContainerFormat(lContainerNode);

            var lSinkControl = mCaptureManager.createSinkControl();

            ISampleGrabberCallbackSinkFactory lSampleGrabberCallbackSinkFactory = null;

            lSinkControl.createSinkFactory(
            lReadMode,
            out lSampleGrabberCallbackSinkFactory);

            int lIndexCount = 0;

            var lVideoStreamSourceNode = createVideoStream(lSampleGrabberCallbackSinkFactory, lIndexCount);

            var lAudioStreamSourceNode = createAudioStream(lSampleGrabberCallbackSinkFactory, lIndexCount);


            XmlDocument l_streamMediaTypesXml = new XmlDocument();

            XmlNode ldocNode = l_streamMediaTypesXml.CreateXmlDeclaration("1.0", "UTF-8", null);

            l_streamMediaTypesXml.AppendChild(ldocNode);

            XmlElement rootNode = l_streamMediaTypesXml.CreateElement("MediaTypes");

            l_streamMediaTypesXml.AppendChild(rootNode);


            var lAttr = l_streamMediaTypesXml.CreateAttribute("StreamName");

            lAttr.Value = mStreamNameTxtBx.Text;

            rootNode.Attributes.Append(lAttr);

            List<object> lSourceMediaNodeList = new List<object>();

            if (lVideoStreamSourceNode.Item1 != null)
            {
                doc = new XmlDocument();

                doc.LoadXml(lVideoStreamSourceNode.Item2);

                var lMediaType = doc.SelectSingleNode("MediaType");

                if (lMediaType != null)
                {
                    rootNode.AppendChild(l_streamMediaTypesXml.ImportNode(lMediaType, true));
                }

                lSourceMediaNodeList.Add(lVideoStreamSourceNode.Item1);
            }

            if (lAudioStreamSourceNode.Item1 != null)
            {
                doc = new XmlDocument();

                doc.LoadXml(lAudioStreamSourceNode.Item2);

                var lMediaType = doc.SelectSingleNode("MediaType");

                if (lMediaType != null)
                {
                    rootNode.AppendChild(l_streamMediaTypesXml.ImportNode(lMediaType, true));
                }

                lSourceMediaNodeList.Add(lAudioStreamSourceNode.Item1);
            }

            var lSessionControl = mCaptureManager.createSessionControl();

            if (lSessionControl == null)
                return;

            mISession = lSessionControl.createSession(
                lSourceMediaNodeList.ToArray());

            if (mISession == null)
                return;

            startServer(l_streamMediaTypesXml.InnerXml);

            mISession.registerUpdateStateDelegate(UpdateStateDelegate);

            mISession.startSession(0, Guid.Empty);

            mLaunchButton.Content = "Stop";
        }

        void UpdateStateDelegate(uint aCallbackEventCode, uint aSessionDescriptor)
        {
            SessionCallbackEventCode k = (SessionCallbackEventCode)aCallbackEventCode;

            switch (k)
            {
                case SessionCallbackEventCode.Unknown:
                    break;
                case SessionCallbackEventCode.Error:
                    break;
                case SessionCallbackEventCode.Status_Error:
                    break;
                case SessionCallbackEventCode.Execution_Error:
                    break;
                case SessionCallbackEventCode.ItIsReadyToStart:
                    break;
                case SessionCallbackEventCode.ItIsStarted:
                    break;
                case SessionCallbackEventCode.ItIsPaused:
                    break;
                case SessionCallbackEventCode.ItIsStopped:
                    break;
                case SessionCallbackEventCode.ItIsEnded:
                    break;
                case SessionCallbackEventCode.ItIsClosed:
                    {
                        Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action(() => mLaunchButton.Content = "Launch"));

                        Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            if (s != null) s.disconnect();

                            s = null;
                        }
                        ));
                    }
                    break;
                case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
                    {
                        Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action(() => mLaunchButton_Click(null, null)));
                    }
                    break;
                default:
                    break;
            }
        }

        private Guid setContainerFormat(XmlNode aXmlNode)
        {

            Guid lContainerFormatGuid = Guid.Empty;

            do
            {
                if (aXmlNode == null)
                    break;

                var lAttrNode = aXmlNode.SelectSingleNode("@Value");

                if (lAttrNode == null)
                    break;

                lAttrNode = aXmlNode.SelectSingleNode("@GUID");

                if (lAttrNode == null)
                    break;

                if (Guid.TryParse(lAttrNode.Value, out lContainerFormatGuid))
                {
                }

            } while (false);

            return lContainerFormatGuid;
        }

        private Tuple<object, string, int> createVideoStream(ISampleGrabberCallbackSinkFactory aISampleGrabberCallbackSinkFactory, int aIndexCount)
        {
            object result = null;

            int index = 0;

            string lMediaType = "";

            do
            {

                var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    break;

                var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                uint lStreamIndex = 0;

                lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    break;

                lNode = lSourceNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    break;
                }


                IEVRSinkFactory lSinkFactory;

                var lSinkControl = mCaptureManager.createSinkControl();

                lSinkControl.createSinkFactory(
                Guid.Empty,
                out lSinkFactory);

                object lEVROutputNode = null;

                lSinkFactory.createOutputNode(
                        mVideoPanel.Handle,
                        out lEVROutputNode);

                if (lEVROutputNode == null)
                    break;

                ISampleGrabberCallback lH264SampleGrabberCallback;

                aISampleGrabberCallbackSinkFactory.createOutputNode(
                    MFMediaType_Video,
                    MFVideoFormat_H264,
                    out lH264SampleGrabberCallback);

                object lOutputNode = lEVROutputNode;

                if (lH264SampleGrabberCallback != null)
                {
                    lH264SampleGrabberCallback.mUpdateNativeFullEvent += delegate
                        (uint aSampleFlags, long aSampleTime, long aSampleDuration, IntPtr aData, uint aSize)
                    {
                        if (s != null)
                        {
                            lock (s)
                            {
                                Console.WriteLine("aSampleFlags: {0}", aSampleFlags);

                                currentmillisecond += 1;

                                s.sendVideoData(currentmillisecond, aData, (int)aSize, aSampleFlags, aIndexCount);

                                currentmillisecond += 33;
                            }
                        }
                    };

                    var lSampleGrabberCallNode = lH264SampleGrabberCallback.getTopologyNode();

                    if (lSampleGrabberCallNode != null)
                    {
                        IStreamControl streamControl = mCaptureManager.createStreamControl();
                        ISpreaderNodeFactory spreaderNodeFactory = null;
                        object spreaderNode = null;
                        List<object> outputNodeList = new List<object>();



                        var mEncoderControl = mCaptureManager.createEncoderControl();

                        string lxmldoc = "";

                        XmlDocument doc = new XmlDocument();

                        mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

                        doc.LoadXml(lxmldoc);

                        var l_VideoEncoderNode = doc.SelectSingleNode("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory[@IsStreaming='TRUE'][1]/@CLSID");

                        if (l_VideoEncoderNode == null)
                            break;

                        Guid lCLSIDVideoEncoder;

                        if (!Guid.TryParse(l_VideoEncoderNode.Value, out lCLSIDVideoEncoder))
                            break;



                        IEncoderNodeFactory lIEncoderNodeFactory;

                        mEncoderControl.createEncoderNodeFactory(lCLSIDVideoEncoder, out lIEncoderNodeFactory);

                        if (lIEncoderNodeFactory != null)
                        {

                            object lVideoSourceOutputMediaType;

                            if (mSourceControl == null)
                                break;

                            mSourceControl.getSourceOutputMediaType(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex,
                                out lVideoSourceOutputMediaType);




                            string lxmlDoc;

                            mEncoderControl.getMediaTypeCollectionOfEncoder(
                                    lVideoSourceOutputMediaType,
                                    lCLSIDVideoEncoder,
                                    out lxmlDoc);
                            
                            doc = new System.Xml.XmlDocument();

                            doc.LoadXml(lxmlDoc);
                            
                            var lGroup = doc.SelectSingleNode("EncoderMediaTypes/Group[@GUID='{8F6FF1B6-534E-49C0-B2A8-16D534EAF135}']");

                            uint lMaxBitRate = 0;

                            if (lGroup != null)
                            {                                
                                var lAttr = lGroup.SelectSingleNode("@MaxBitRate");

                                if (lAttr != null)
                                {
                                    uint.TryParse(lAttr.Value, out lMaxBitRate);
                                }
                            }





                            object lVideoEncoderNode;

                            lIEncoderNodeFactory.createEncoderNode(
                                lVideoSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0,
                                lSampleGrabberCallNode,
                                out lVideoEncoderNode);

                            object lCompressedMediaType;

                            lIEncoderNodeFactory.createCompressedMediaType(
                                lVideoSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0,
                                out lCompressedMediaType);


                            streamControl.createStreamControlNodeFactory(ref spreaderNodeFactory);
                            outputNodeList.Add(lEVROutputNode);
                            outputNodeList.Add(lVideoEncoderNode);
                            spreaderNodeFactory.createSpreaderNode(outputNodeList,
                                                                   out spreaderNode);


                            if (spreaderNode != null)
                                lOutputNode = spreaderNode;

                            mCaptureManager.parseMediaType(lCompressedMediaType, out lMediaType);

                        }
                    }
                }

                string lextendSymbolicLink = lSymbolicLink + " --options=" +
                    "<?xml version='1.0' encoding='UTF-8'?>" +
                    "<Options>" +
                        "<Option Type='Cursor' Visiblity='True'>" +
                            "<Option.Extensions>" +
                                "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
                            "</Option.Extensions>" +
                        "</Option>" +
                    "</Options>";


                lextendSymbolicLink += " --normalize=Landscape";

                mSourceControl.createSourceNode(
                    lextendSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lOutputNode,
                    out result);

                if (result != null)
                {
                    index = aIndexCount;
                }

            }
            while (false);

            return Tuple.Create<object, string, int>(result, lMediaType, index);
        }

        int currentmillisecond = 0;

        private Tuple<object, string, int> createAudioStream(ISampleGrabberCallbackSinkFactory aISampleGrabberCallbackSinkFactory, int aIndexCount)
        {
            object result = null;

            int index = 0;

            string lMediaType = "";

            do
            {

                ISampleGrabberCallback lAACSampleGrabberCallback;

                aISampleGrabberCallbackSinkFactory.createOutputNode(
                    MFMediaType_Audio,
                    MFAudioFormat_AAC,
                    out lAACSampleGrabberCallback);

                if (lAACSampleGrabberCallback != null)
                {
                    lAACSampleGrabberCallback.mUpdateNativeFullEvent += delegate
                        (uint aSampleFlags, long aSampleTime, long aSampleDuration, IntPtr aData, uint aSize)
                    {
                        if (s != null)
                        {
                            lock (s)
                            {
                                currentmillisecond = (int)(aSampleTime / (long)10000);

                                s.sendAudioData(currentmillisecond, aData, (int)aSize, aSampleFlags, aIndexCount);
                            }
                        }
                    };

                    var lSampleGrabberCallNode = lAACSampleGrabberCallback.getTopologyNode();

                    if (lSampleGrabberCallNode != null)
                    {

                        var mEncoderControl = mCaptureManager.createEncoderControl();

                        Guid lAACEncoder = new Guid("93AF0C51-2275-45d2-A35B-F2BA21CAED00");

                        IEncoderNodeFactory lIEncoderNodeFactory;

                        mEncoderControl.createEncoderNodeFactory(lAACEncoder, out lIEncoderNodeFactory);

                        if (lIEncoderNodeFactory != null)
                        {


                            var lSourceNode = mAudioSourcesComboBox.SelectedItem as XmlNode;

                            if (lSourceNode == null)
                                break;

                            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                            if (lNode == null)
                                break;

                            string lSymbolicLink = lNode.Value;

                            lSourceNode = mAudioStreamsComboBox.SelectedItem as XmlNode;

                            if (lSourceNode == null)
                                break;

                            lNode = lSourceNode.SelectSingleNode("@Index");

                            if (lNode == null)
                                break;

                            uint lStreamIndex = 0;

                            if (!uint.TryParse(lNode.Value, out lStreamIndex))
                            {
                                break;
                            }

                            lSourceNode = mAudioMediaTypesComboBox.SelectedItem as XmlNode;

                            if (lSourceNode == null)
                                break;

                            lNode = lSourceNode.SelectSingleNode("@Index");

                            if (lNode == null)
                                break;

                            uint lMediaTypeIndex = 0;

                            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                            {
                                break;
                            }


                            object lAudioSourceOutputMediaType;

                            mSourceControl.getSourceOutputMediaType(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex,
                                out lAudioSourceOutputMediaType);



                            string lxmlDoc;

                            mEncoderControl.getMediaTypeCollectionOfEncoder(
                                    lAudioSourceOutputMediaType,
                                    lAACEncoder,
                                    out lxmlDoc);

                            var doc = new System.Xml.XmlDocument();

                            doc.LoadXml(lxmlDoc);

                            var lGroup = doc.SelectSingleNode("EncoderMediaTypes/Group[@GUID='{8F6FF1B6-534E-49C0-B2A8-16D534EAF135}']");

                            uint lMaxBitRate = 0;

                            if (lGroup != null)
                            {
                                var lAttr = lGroup.SelectSingleNode("@MaxBitRate");

                                if (lAttr != null)
                                {
                                    uint.TryParse(lAttr.Value, out lMaxBitRate);
                                }
                            }


                            object lAudioEncoder;

                            lIEncoderNodeFactory.createEncoderNode(
                                lAudioSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0,
                                lSampleGrabberCallNode,
                                out lAudioEncoder);

                            object lCompressedMediaType;

                            lIEncoderNodeFactory.createCompressedMediaType(
                                lAudioSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0,
                                out lCompressedMediaType);

                            mSourceControl.createSourceNode(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex,
                                lAudioEncoder,
                                out result);

                            if (result != null)
                            {
                                index = aIndexCount;
                            }

                            mCaptureManager.parseMediaType(lCompressedMediaType, out lMediaType);
                        }
                    }
                }
            }
            while (false);

            return Tuple.Create<object, string, int>(result, lMediaType, index);
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WSAData dummy = new WSAData();

            m_socketAccessable = NativeMethods.WSAStartup(0x0202, ref dummy) == 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NativeMethods.WSACleanup();
        }
    }
}
