using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;

namespace WPFRtspServerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        IEVRStreamControlAsync mIEVRStreamControl = null;

        ISessionAsync mISession = null;

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;
        
        Guid MFMediaType_Video = new Guid(
 0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);


        Guid MFMediaType_Audio = new Guid(
0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
        

        Guid StreamingCBR = new Guid("8F6FF1B6-534E-49C0-B2A8-16D534EAF135");
               

        public MainWindow()
        {
            InitializeComponent();
        }

        RtspServer s = null;

        //rtsp://127.0.0.1:8554

        private void startServer(List<Tuple<RtspServer.StreamType, int, string>> streams)
        {
            int port = 8554;
            string username = "user";      // or use NUL if there is no username
            string password = "password";  // or use NUL if there is no password

            if (s == null)
                s = new RtspServer(streams, port, null, null);

            s.StartListen();
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content == "Stop")
            {
                if (s != null)
                    s.StopListen();

                s = null;

                if (mISession != null)
                {
                    await mISession.closeSessionAsync();

                    mLaunchButton.Content = "Launch";
                }

                mISession = null;

                return;
            }


            string lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);
            
            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            var lReadMode = setContainerFormat(lContainerNode);

            var lSinkControl = await mCaptureManager.createSinkControlAsync();

            var lSampleGrabberCallbackSinkFactory = await lSinkControl.createSampleGrabberCallbackSinkFactoryAsync(
            lReadMode);

            int lIndexCount = 120;

            var lVideoStreamSourceNode = await createVideoStream(lSampleGrabberCallbackSinkFactory, lIndexCount++);

            var lAudioStreamSourceNode = await createAudioStream(lSampleGrabberCallbackSinkFactory, lIndexCount++);           


            List<object> lSourceMediaNodeList = new List<object>();

            List<Tuple<RtspServer.StreamType, int, string>> streams = new List<Tuple<RtspServer.StreamType, int, string>>();

            if (lVideoStreamSourceNode.Item1 != null)
            {
                lSourceMediaNodeList.Add(lVideoStreamSourceNode.Item1);

                streams.Add(Tuple.Create<RtspServer.StreamType, int, string>(lVideoStreamSourceNode.Item2, lVideoStreamSourceNode.Item3, lVideoStreamSourceNode.Item4));
            }

            if (lAudioStreamSourceNode.Item1 != null)
            {
                lSourceMediaNodeList.Add(lAudioStreamSourceNode.Item1);

                streams.Add(Tuple.Create<RtspServer.StreamType, int, string>(lAudioStreamSourceNode.Item2, lAudioStreamSourceNode.Item3, ""));
            }

            var lSessionControl = await mCaptureManager.createSessionControlAsync();

            if (lSessionControl == null)
                return;

            mISession = await lSessionControl.createSessionAsync(
                lSourceMediaNodeList.ToArray());

            if (mISession == null)
                return;


            await mISession.registerUpdateStateDelegateAsync(UpdateStateDelegate);

            await mISession.startSessionAsync(0, Guid.Empty);

            mLaunchButton.Content = "Stop";

            startServer(streams);

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
                                if (s != null) s.StopListen();
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

        private async Task<Tuple<object, RtspServer.StreamType, int, string>> createVideoStream(ISampleGrabberCallbackSinkFactoryAsync aISampleGrabberCallbackSinkFactory, int aIndexCount)
        {
            object result = null;

            RtspServer.StreamType type = RtspServer.StreamType.Video;

            int index = 0;

            string format = "";

            do
            {

                var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    break;

                var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

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

                Guid lVideoFormat = Guid.Empty;

                var lVideoCompressedNode = mVideoMediaTypeComboBox.SelectedItem as XmlNode;
              
                if(lVideoCompressedNode != null)
                {
                    var lGUID = lVideoCompressedNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@GUID");

                    if(lGUID != null)
                    {
                        Guid.TryParse(lGUID.Value, out lVideoFormat);
                    }


                    var lvalue = lVideoCompressedNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                    if (lvalue != null && lvalue.Value != null)
                    {
                        var lsplit = lvalue.Value.Split(new char[] { '_' });

                        if (lsplit != null && lsplit.Length == 2)
                            format = lsplit[1];

                        if (format == "HEVC")
                            format = "H265";
                    }
                }
                


                var lSampleGrabberCallback = await aISampleGrabberCallbackSinkFactory.createOutputNodeAsync(
                    MFMediaType_Video,
                    lVideoFormat);

                object lOutputNode = lEVROutputNode;

                if (lSampleGrabberCallback != null)
                {
                    lSampleGrabberCallback.mUpdateFullEvent += delegate
                        (uint aSampleFlags, long aSampleTime, long aSampleDuration, byte[] aData, uint aLength)
                    {
                        if (s != null)
                        {
                            lock (s)
                            {
                                currentmillisecond += 1;

                                s.sendData(aIndexCount, (int)type, currentmillisecond * 90, aData);

                                currentmillisecond += 20;
                            }
                        }
                    };

                    var lSampleGrabberCallNode = lSampleGrabberCallback.getTopologyNode();

                    if (lSampleGrabberCallNode != null)
                    {
                        IStreamControl streamControl = mCaptureManager.createStreamControl();
                        ISpreaderNodeFactory spreaderNodeFactory = null;
                        object spreaderNode = null;
                        List<object> outputNodeList = new List<object>();



                        var mEncoderControl = await mCaptureManager.createEncoderControlAsync();

                        var lEncoderNode = mVideoEncoderComboBox.SelectedItem as XmlNode;

                        if (lEncoderNode == null)
                            break;

                        lNode = lEncoderNode.SelectSingleNode("@CLSID");

                        if (lNode == null)
                            break;

                        Guid lCLSIDEncoder;

                        if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder))
                            break;




                         var lBitRate = (uint)mBitRateComboBox.SelectedItem;




                        var lIEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                        if (lIEncoderNodeFactory != null)
                        {

                            if (mSourceControl == null)
                                break;

                            object lVideoSourceOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex);


                            object lVideoEncoderNode = await lIEncoderNodeFactory.createEncoderNodeAsync(lVideoSourceOutputMediaType,
                                StreamingCBR,
                                lBitRate,
                                0,
                                lSampleGrabberCallNode);


                            streamControl.createStreamControlNodeFactory(ref spreaderNodeFactory);
                            outputNodeList.Add(lEVROutputNode);
                            outputNodeList.Add(lVideoEncoderNode);
                            spreaderNodeFactory.createSpreaderNode(outputNodeList,
                                                                   out spreaderNode);


                            if (spreaderNode != null)
                                lOutputNode = spreaderNode;
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

            return Tuple.Create<object, RtspServer.StreamType, int, string>(result, type, index, format);
        }

        uint currentmillisecond = 0;

        private async Task<Tuple<object, RtspServer.StreamType, int>> createAudioStream(ISampleGrabberCallbackSinkFactoryAsync aISampleGrabberCallbackSinkFactory, int aIndexCount)
        {
            object result = null;

            RtspServer.StreamType type = RtspServer.StreamType.Audio;

            int index = 0;

            do
            {

                Guid lAudioFormat = Guid.Empty;

                var lAudioCompressedNode = mAudioMediaTypeComboBox.SelectedItem as XmlNode;

                if (lAudioCompressedNode != null)
                {
                    var lGUID = lAudioCompressedNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@GUID");

                    if (lGUID != null)
                    {
                        Guid.TryParse(lGUID.Value, out lAudioFormat);
                    }
                }

                var lSampleGrabberCallback = await aISampleGrabberCallbackSinkFactory.createOutputNodeAsync(
                    MFMediaType_Audio,
                    lAudioFormat);

                if (lSampleGrabberCallback != null)
                {
                    lSampleGrabberCallback.mUpdateFullEvent += delegate
                        (uint aSampleFlags, long aSampleTime, long aSampleDuration, byte[] aData, uint aLength)
                    {
                        if (s != null)
                        {
                            lock (s)
                            {
                                currentmillisecond = (uint)aSampleTime / 10000;

                                s.sendData(aIndexCount, (int)type, currentmillisecond*90, aData);
                            }
                        }
                    };

                    var lSampleGrabberCallNode = lSampleGrabberCallback.getTopologyNode();

                    if (lSampleGrabberCallNode != null)
                    {

                        var mEncoderControl = await mCaptureManager.createEncoderControlAsync();

                        var lEncoderNode = mAudioEncoderComboBox.SelectedItem as XmlNode;

                        if (lEncoderNode == null)
                            break;

                        var lNode = lEncoderNode.SelectSingleNode("@CLSID");

                        if (lNode == null)
                            break;

                        Guid lCLSIDEncoder;

                        if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder))
                            break;                     

                        var lIEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                        if (lIEncoderNodeFactory != null)
                        {


                            var lSourceNode = mAudioSourcesComboBox.SelectedItem as XmlNode;

                            if (lSourceNode == null)
                                break;

                            lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

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

                            object lAudioEncoder = await lIEncoderNodeFactory.createEncoderNodeAsync(lAudioSourceOutputMediaType,
                                //new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55),
                                //new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55),
                                StreamingCBR,
                                75,
                                0,
                                lSampleGrabberCallNode);

                            result = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                                lSymbolicLink,
                                lStreamIndex,
                                lMediaTypeIndex,
                                lAudioEncoder);

                            if (result != null)
                            {
                                index = aIndexCount;
                            }

                        }
                    }
                }
            }
            while (false);

            return Tuple.Create<object, RtspServer.StreamType, int>(result, type, index);            
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var lHyperlink = sender as Hyperlink;

            if (lHyperlink == null)
                return;

            var lRun = lHyperlink.Inlines.FirstInline as Run;

            if (lRun == null)
                return;

            Clipboard.SetText(lRun.Text);

            mPopupCopy.IsOpen = true;

            DoubleAnimation fadeInAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromMilliseconds(1000)));

            fadeInAnimation.Completed += fadeInAnimation_Completed;
            fadeInAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            mPopupCopy.Opacity = 0.0;
            mPopupCopy.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline)fadeInAnimation.GetAsFrozen());

           
        }

        private void fadeInAnimation_Completed(object sender, EventArgs e)
        {

            mPopupCopy.IsOpen = false;
        }

        private async void mVideoEncoderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {

                mBitRateComboBox.Items.Clear();


                var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    break;

                var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

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

                if (mSourceControl == null)
                    break;
                
                object lVideoSourceOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);



                var lVideoEncoderNode = mVideoEncoderComboBox.SelectedItem as XmlNode;

                if (lVideoEncoderNode == null)
                    break;

                lNode = lVideoEncoderNode.SelectSingleNode("@CLSID");

                if (lNode == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder))
                    break;

                string lxmlDoc = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                        lVideoSourceOutputMediaType,
                        lCLSIDEncoder);
                
                var lXmlDataProvider = (XmlDataProvider)this.Resources["XmlVideoCompressedMediaTypesProvider"];

                if (lXmlDataProvider == null)
                    return;

                var doc = new System.Xml.XmlDocument();

                doc.LoadXml(lxmlDoc);

                lXmlDataProvider.Document = doc;


                var lGroup = doc.SelectSingleNode("EncoderMediaTypes/Group[@GUID='{8F6FF1B6-534E-49C0-B2A8-16D534EAF135}']");

                if(lGroup != null)
                {
                    uint lMaxBitRate = 0;

                    uint lMinBitRate = 0;

                    var lAttr = lGroup.SelectSingleNode("@MaxBitRate");

                    if (lAttr != null)
                    {
                        if (uint.TryParse(lAttr.Value, out lMaxBitRate))
                        {

                        }
                    }

                    lAttr = lGroup.SelectSingleNode("@MinBitRate");

                    if (lAttr != null)
                    {
                        if (uint.TryParse(lAttr.Value, out lMinBitRate))
                        {

                        }
                    }

                    for (uint i = lMaxBitRate; i >= lMinBitRate; i >>= 1)
			        {
                        mBitRateComboBox.Items.Add(i);
			        }
                }                                
            } while (false);
        }

        private async void mAudioEncoderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {

                mAudioBitRateComboBox.Items.Clear();


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

                if (mSourceControl == null)
                    break;

                object lAudioSourceOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);



                var lAudioEncoderNode = mAudioEncoderComboBox.SelectedItem as XmlNode;

                if (lAudioEncoderNode == null)
                    break;

                lNode = lAudioEncoderNode.SelectSingleNode("@CLSID");

                if (lNode == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder))
                    break;

                string lxmlDoc = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                        lAudioSourceOutputMediaType,
                        lCLSIDEncoder);

                var lXmlDataProvider = (XmlDataProvider)this.Resources["XmlAudioCompressedMediaTypesProvider"];

                if (lXmlDataProvider == null)
                    return;

                var doc = new System.Xml.XmlDocument();

                doc.LoadXml(lxmlDoc);

                lXmlDataProvider.Document = doc;


                var lGroup = doc.SelectSingleNode("EncoderMediaTypes/Group[@GUID='{8F6FF1B6-534E-49C0-B2A8-16D534EAF135}']");

                if (lGroup != null)
                {
                    uint lMaxBitRate = 0;

                    uint lMinBitRate = 0;

                    var lAttr = lGroup.SelectSingleNode("@MaxBitRate");

                    if (lAttr != null)
                    {
                        if (uint.TryParse(lAttr.Value, out lMaxBitRate))
                        {

                        }
                    }

                    lAttr = lGroup.SelectSingleNode("@MinBitRate");

                    if (lAttr != null)
                    {
                        if (uint.TryParse(lAttr.Value, out lMinBitRate))
                        {

                        }
                    }

                    for (uint i = lMaxBitRate; i >= lMinBitRate; i >>= 1)
                    {
                        mAudioBitRateComboBox.Items.Add(i);
                    }
                }
            } while (false);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                ThreadPool.SetMinThreads(15, 10);

                mCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (System.Exception)
            {
                try
                {
                    mCaptureManager = new CaptureManager();
                }
                catch (System.Exception)
                {

                }
            }

            if (mCaptureManager == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSourcesProvider"];

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




            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncodersProvider"];

            if (lXmlDataProvider == null)
                return;


            mEncoderControl = await mCaptureManager.createEncoderControlAsync();

            lxmldoc = await mCaptureManager.getCollectionOfEncodersAsync();

            doc = new System.Xml.XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;
        }
    }
}