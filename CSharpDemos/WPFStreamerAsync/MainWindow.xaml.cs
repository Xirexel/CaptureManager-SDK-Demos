using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace WPFStreamerAsync
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

        IEVRStreamControlAsync mIEVRStreamControl = null;

        ISessionAsync mISession = null;

        ISourceControlAsync mSourceControl;

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
        }

        RtmpClient s = null;

        private void startServer(string a_streamsXml)
        {
            if (s == null)
                s = RtmpClient.createInstance(a_streamsXml, mStreamSiteComboBox.Text);
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_socketAccessable)
                return;

            mLaunchButton.IsEnabled = false;

            do
            {

                if (mLaunchButton.Content == "Stop")
                {
                    if (mISession != null)
                    {
                        await mISession.closeSessionAsync();

                        mLaunchButton.Content = "Launch";
                    }

                    mISession = null;

                    if (s != null)
                        s.disconnect();

                    s = null;

                    mLaunchButton.IsEnabled = true;

                    break;
                }


                string lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(lxmldoc);

                var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}']");

                if (lSinkNode == null)
                    break;

                var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

                if (lContainerNode == null)
                    break;

                var lReadMode = setContainerFormat(lContainerNode);

                var lSinkControl = await mCaptureManager.createSinkControlAsync();

                ISampleGrabberCallbackSinkFactoryAsync lSampleGrabberCallbackSinkFactory = await lSinkControl.createSampleGrabberCallbackSinkFactoryAsync(
                lReadMode);

                int lIndexCount = 0;

                var lVideoStreamSourceNode = await createVideoStream(lSampleGrabberCallbackSinkFactory, lIndexCount);

                var lAudioStreamSourceNode = await createAudioStream(lSampleGrabberCallbackSinkFactory, lIndexCount);


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

                var lSessionControl = await mCaptureManager.createSessionControlAsync();

                if (lSessionControl == null)
                    break;

                mISession = await lSessionControl.createSessionAsync(
                    lSourceMediaNodeList.ToArray());

                if (mISession == null)
                    break;

                startServer(l_streamMediaTypesXml.InnerXml);

                await mISession.registerUpdateStateDelegateAsync(UpdateStateDelegate);

                await mISession.startSessionAsync(0, Guid.Empty);

                mLaunchButton.Content = "Stop";

            } while (false);

            mLaunchButton.IsEnabled = true;
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

        private async Task<Tuple<object, string, int>> createVideoStream(ISampleGrabberCallbackSinkFactoryAsync aISampleGrabberCallbackSinkFactory, int aIndexCount)
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

                var lSinkControl = await mCaptureManager.createSinkControlAsync();

                var lSinkFactory = await lSinkControl.createEVRSinkFactoryAsync(
                Guid.Empty);

                object lEVROutputNode = await lSinkFactory.createOutputNodeAsync(
                        mVideoPanel.Handle);

                if (lEVROutputNode == null)
                    break;

                ISampleGrabberCallback lH264SampleGrabberCallback = await aISampleGrabberCallbackSinkFactory.createOutputNodeAsync(
                    MFMediaType_Video,
                    MFVideoFormat_H264);

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
                        var streamControl = await mCaptureManager.createStreamControlAsync();
                        object spreaderNode = null;
                        List<object> outputNodeList = new List<object>();



                        var mEncoderControl = await mCaptureManager.createEncoderControlAsync();

                        XmlDocument doc = new XmlDocument();

                        string lxmldoc = await mCaptureManager.getCollectionOfEncodersAsync();

                        doc.LoadXml(lxmldoc);

                        var l_VideoEncoderNode = doc.SelectSingleNode("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory[@IsStreaming='TRUE'][1]/@CLSID");

                        if (l_VideoEncoderNode == null)
                            break;

                        Guid lCLSIDVideoEncoder;

                        if (!Guid.TryParse(l_VideoEncoderNode.Value, out lCLSIDVideoEncoder))
                            break;
                                               
                        var lIEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDVideoEncoder);

                        if (lIEncoderNodeFactory != null)
                        {

                            if (mSourceControl == null)
                                break;

                            object lVideoSourceOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex);




                            string lxmlDoc = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                                    lVideoSourceOutputMediaType,
                                    lCLSIDVideoEncoder);

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

                            lMaxBitRate = 1000000;



                            object lVideoEncoderNode = await lIEncoderNodeFactory.createEncoderNodeAsync(
                                lVideoSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0,
                                lSampleGrabberCallNode);

                            object lCompressedMediaType = await lIEncoderNodeFactory.createCompressedMediaTypeAsync(
                                lVideoSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0);

                            var spreaderNodeFactory = await streamControl.createSpreaderNodeFactoryAsync();
                            outputNodeList.Add(lEVROutputNode);
                            outputNodeList.Add(lVideoEncoderNode);
                            spreaderNode = await spreaderNodeFactory.createSpreaderNodeAsync(outputNodeList);


                            if (spreaderNode != null)
                                lOutputNode = spreaderNode;

                            lMediaType = await mCaptureManager.parseMediaTypeAsync(lCompressedMediaType);

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

                result = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lextendSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lOutputNode);

                if (result != null)
                {
                    index = aIndexCount;
                }

            }
            while (false);

            return Tuple.Create<object, string, int>(result, lMediaType, index);
        }

        int currentmillisecond = 0;

        private async Task<Tuple<object, string, int>> createAudioStream(ISampleGrabberCallbackSinkFactoryAsync aISampleGrabberCallbackSinkFactory, int aIndexCount)
        {
            object result = null;

            int index = 0;

            string lMediaType = "";

            do
            {

                var lAACSampleGrabberCallback = await aISampleGrabberCallbackSinkFactory.createOutputNodeAsync(
                    MFMediaType_Audio,
                    MFAudioFormat_AAC);

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

                        var mEncoderControl = await mCaptureManager.createEncoderControlAsync();

                        Guid lAACEncoder = new Guid("93AF0C51-2275-45d2-A35B-F2BA21CAED00");

                        var lIEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lAACEncoder);

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


                            object lAudioSourceOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex);



                            string lxmlDoc = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                                    lAudioSourceOutputMediaType,
                                    lAACEncoder);

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


                            object lAudioEncoder = await lIEncoderNodeFactory.createEncoderNodeAsync(
                                lAudioSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0,
                                lSampleGrabberCallNode);

                            object lCompressedMediaType = await lIEncoderNodeFactory.createCompressedMediaTypeAsync(
                                lAudioSourceOutputMediaType,
                                StreamingCBR,
                                lMaxBitRate,
                                0);

                            result = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex,
                                lAudioEncoder);

                            if (result != null)
                            {
                                index = aIndexCount;
                            }

                            lMediaType = await mCaptureManager.parseMediaTypeAsync(lCompressedMediaType);
                        }
                    }
                }
            }
            while (false);

            return Tuple.Create<object, string, int>(result, lMediaType, index);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WSAData dummy = new WSAData();

            m_socketAccessable = NativeMethods.WSAStartup(0x0202, ref dummy) == 0;


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

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;


            mIEVRStreamControl = await mCaptureManager.createEVRStreamControlAsync();

            mSourceControl = await mCaptureManager.createSourceControlAsync();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NativeMethods.WSACleanup();
        }
    }
}
