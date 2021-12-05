using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace WPFWebCamSerialShots
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager;

        ISampleGrabberCallSinkFactory mSinkFactory = null;

        ISession mISession = null;

        ISampleGrabberCall mISampleGrabber = null;

        Guid MFMediaType_Video = new Guid(
 0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

        Guid MFVideoFormat_RGB32 = new Guid(
 22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        Guid MFVideoFormat_MJPG = new Guid(
 0x47504A4D, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
                        
        
        bool mIsMJPG = false;

        uint mVideoWidth = 0;

        uint mVideoHeight = 0;

        int mChannels = 0;

        int mSampleCount = 0;

        IWebCamControl mWebCamControl;

        Guid mReadMode;

        ISampleGrabberCall mISampleGrabberCall;

        ISampleGrabberCall mISampleGrabberCallPull;

        DispatcherTimer mTimer = new DispatcherTimer();

        uint lsampleByteSize;

        byte[] mData = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
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

            {
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
            }
            

            {
                XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSampleAccumulatorsProvider"];

                if (lXmlDataProvider == null)
                    return;

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

                string lxmldoc = "";

                mCaptureManager.createStreamControl().getCollectionOfStreamControlNodeFactories(ref lxmldoc);

                if (string.IsNullOrEmpty(lxmldoc))
                    return;

                doc.LoadXml(lxmldoc);

                lXmlDataProvider.Document = doc;
            }

            


            mTimer.Interval = new TimeSpan(100000);

            mWebCamParametrsTab.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mParametrSlider_ValueChanged));

            mWebCamParametrsTab.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

            mWebCamParametrsTab.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

        }
        
        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content.ToString() == "Stop")
            {
                mTimer.Stop();

                if (mISession != null)
                    mISession.closeSession();

                mLaunchButton.Content = "Launch";

                mTakePhotoButton.IsEnabled = false;

                return;
            }

            mTimer = new DispatcherTimer();

            mTimer.Interval = new TimeSpan(100000);

            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;

            uint lStreamIndex = 0;

            lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null)
                return;

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
            {
                return;
            }

            lNode = lSourceNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

            if (lNode == null)
                return;

            uint lVideoWidth = 0;

            if (!uint.TryParse(lNode.Value, out lVideoWidth))
            {
                return;
            }

            mVideoWidth = lVideoWidth;

            lNode = lSourceNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value");

            if (lNode == null)
                return;

            uint lVideoHeight = 0;

            if (!uint.TryParse(lNode.Value, out lVideoHeight))
            {
                return;
            }

            mVideoHeight = lVideoHeight;

            int lWidthInBytes;

            mCaptureManager.getStrideForBitmapInfoHeader(
                MFVideoFormat_RGB32,
                lVideoWidth,
                out lWidthInBytes);

            lsampleByteSize = (uint)Math.Abs(lWidthInBytes) * lVideoHeight;

            mData = new byte[lsampleByteSize];

            var lSinkControl = mCaptureManager.createSinkControl();


            //  create Spread node

            var lStreamControl = mCaptureManager.createStreamControl();
            
            ISpreaderNodeFactory lSpreaderNodeFactory = null;

            if (!lStreamControl.createStreamControlNodeFactory(
                Guid.Parse("{85DFAAA1-4CC0-4A88-AE28-8F492E552CCA}"),
                ref lSpreaderNodeFactory))
            {
                return;
            }


            // create Sample Accumulator node factory

            ISpreaderNodeFactory lSampleAccumulatorNodeFactory = null;

            XmlNode lselectedSampleAccumulatorXml = (XmlNode)mSampleAccumulatorComboBox.SelectedItem;

            if (lselectedSampleAccumulatorXml == null)
                return;

            var lAttrNode = lselectedSampleAccumulatorXml.SelectSingleNode("@GUID");

            if (lAttrNode == null)
                return;

            if (!lStreamControl.createStreamControlNodeFactory(
                Guid.Parse(lAttrNode.Value),
                ref lSampleAccumulatorNodeFactory))
            {
                return;
            }

            lAttrNode = lselectedSampleAccumulatorXml.SelectSingleNode("@Samples");

            if (lAttrNode == null)
                return;

            if(!int.TryParse(lAttrNode.Value, out mSampleCount))
            {
                return;
            }


            string lxmldoc = "";

            XmlDocument doc = new XmlDocument();


            mCaptureManager.getCollectionOfSinks(ref lxmldoc);


            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}']");

            if (lSinkNode == null)
                return;
                        
            // create view output.

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[2]");

            if (lContainerNode == null)
                return;

            setContainerFormat(lContainerNode);

            lSinkControl.createSinkFactory(
            mReadMode,
            out mSinkFactory);

            lNode = lSourceNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

            if (lNode == null)
                return;
                        
            mSinkFactory.createOutputNode(
                MFMediaType_Video,
                MFVideoFormat_RGB32,
                lsampleByteSize,
                out mISampleGrabberCall);

            if (mISampleGrabberCall != null)
            {
                byte[] lData = new byte[lsampleByteSize];


                mTimer.Tick += delegate
                {
                    if (mISampleGrabberCall == null)
                        return;

                    uint lByteSize = 0;

                    try
                    {

                        mISampleGrabberCall.readData(lData, out lByteSize);
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        if (lByteSize > 0)
                            mDisplayImage.Source = FromArray(lData, lVideoWidth, lVideoHeight, 4);
                    }
                };


                // create take photo output
                
                lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[3]");

                if (lContainerNode == null)
                    return;

                setContainerFormat(lContainerNode);

                ISampleGrabberCallSinkFactory lSinkFactory = null;

                lSinkControl.createSinkFactory(
                mReadMode,
                out lSinkFactory);

                lNode = lSourceNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                if (lNode == null)
                    return;

                if (lNode.Value == "MFVideoFormat_MJPG")
                {
                    lSinkFactory.createOutputNode(
                        MFMediaType_Video,
                        MFVideoFormat_MJPG,
                        lsampleByteSize,
                        out mISampleGrabberCallPull);

                    mIsMJPG = true;

                }
                else
                {

                    mIsMJPG = false;

                    lSinkFactory.createOutputNode(
                        MFMediaType_Video,
                        MFVideoFormat_RGB32,
                        lsampleByteSize,
                        out mISampleGrabberCallPull);
                }

                // connect take photo output with sample accumulator


                var lISampleGrabberCallPullNode = mISampleGrabberCallPull.getTopologyNode();

                if (lISampleGrabberCallPullNode == null)
                    return;
                
                List<object> lSampleAccumulatorList = new List<object>();

                lSampleAccumulatorList.Add(lISampleGrabberCallPullNode);

                object lSampleAccumulatorNode = null;

                lSampleAccumulatorNodeFactory.createSpreaderNode(lSampleAccumulatorList, out lSampleAccumulatorNode);


                // connect view output and take photo output with spreader node





                var lSampleGrabberCallNode = mISampleGrabberCall.getTopologyNode();

                if (lSampleGrabberCallNode == null)
                    return;

                

                List<object> llSpreaderNodeList = new List<object>();

                llSpreaderNodeList.Add(lSampleAccumulatorNode);

                llSpreaderNodeList.Add(lSampleGrabberCallNode);

                object lSpreaderNode = null;

                lSpreaderNodeFactory.createSpreaderNode(llSpreaderNodeList, out lSpreaderNode);


                object lPtrSourceNode;

                var lSourceControl = mCaptureManager.createSourceControl();

                if (lSourceControl == null)
                    return;

                lSourceControl.createSourceNode(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lSpreaderNode,
                    out lPtrSourceNode);

                List<object> lSourceMediaNodeList = new List<object>();

                lSourceMediaNodeList.Add(lPtrSourceNode);

                var lSessionControl = mCaptureManager.createSessionControl();

                if (lSessionControl == null)
                    return;

                mISession = lSessionControl.createSession(
                    lSourceMediaNodeList.ToArray());

                if (mISession == null)
                    return;

                if (!mISession.startSession(0, Guid.Empty))
                    return;

                mLaunchButton.Content = "Stop";

                mWebCamControl = lSourceControl.createWebCamControl(lSymbolicLink);

                if (mWebCamControl != null)
                {
                    string lXMLstring;

                    mWebCamControl.getCamParametrs(out lXMLstring);

                    XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlWebCamParametrsProvider"];

                    if (lXmlDataProvider == null)
                        return;

                    System.Xml.XmlDocument ldoc = new System.Xml.XmlDocument();

                    ldoc.LoadXml(lXMLstring);

                    lXmlDataProvider.Document = ldoc;
                }

                mTimer.Start();

                mTakePhotoButton.IsEnabled = true;
            }
        }

        private void updateDisplayImage(Window aWindow, byte[] aData, uint aLength)
        {
            mDisplayImage.Source = FromArray(aData, mVideoWidth, mVideoHeight, mChannels);
        }


        private static BitmapSource FromArray(byte[] data, uint w, uint h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha

            WriteableBitmap wbm = new WriteableBitmap((int)w, (int)h, 96, 96, format, null);
            wbm.WritePixels(new Int32Rect(0, 0, (int)w, (int)h), data, ch * (int)w, 0);

            return wbm;
        }

        private void mParametrSlider_ValueChanged(object sender, RoutedEventArgs e)//(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            var lslider = e.OriginalSource as Slider;

            if (lslider == null)
                return;

            if (!lslider.IsFocused)
                return;

            var lParametrNode = lslider.Tag as XmlNode;

            if (lParametrNode == null)
                return;

            var lAttr = lParametrNode.Attributes["Index"];

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            int lvalue = (int)lslider.Value;

            mWebCamControl.setCamParametr(
                lindex,
                lvalue,
                2);

        }

        private void mParametrSlider_Checked(object sender, RoutedEventArgs e)
        {

            var lCheckBox = e.OriginalSource as CheckBox;

            if (lCheckBox == null)
                return;

            if (!lCheckBox.IsFocused)
                return;

            var lAttr = lCheckBox.Tag as XmlAttribute;

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            int lvalue = (bool)lCheckBox.IsChecked ? 2 : 1;

            int lCurrentValue;
            int lMin;
            int lMax;
            int lStep;
            int lDefault;
            int lFlag;

            mWebCamControl.getCamParametr(
                lindex,
                out lCurrentValue,
                out lMin,
                out lMax,
                out lStep,
                out lDefault,
                out lFlag);

            mWebCamControl.setCamParametr(
                lindex,
                lCurrentValue,
                lvalue);

        }

        private void mShowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mShowBtn.Content.ToString() == "Show")
            {
                Canvas.SetBottom(mWebCamParametrsPanel, 0);

                mShowBtn.Content = "Hide";
            }
            else if (mShowBtn.Content.ToString() == "Hide")
            {
                Canvas.SetBottom(mWebCamParametrsPanel, -150);

                mShowBtn.Content = "Show";
            }
        }


        private void setContainerFormat(XmlNode aXmlNode)
        {
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

                Guid lContainerFormatGuid;

                if (Guid.TryParse(lAttrNode.Value, out lContainerFormatGuid))
                {
                    mReadMode = lContainerFormatGuid;
                }

            } while (false);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mLaunchButton.Content == "Stop")
            {
                mTimer.Stop();

                if (mISession != null)
                    mISession.closeSession();

                mLaunchButton.Content = "Launch";

                return;
            }
        }

        private void mTakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            mPhotoStack.Children.Clear();

            if (mISampleGrabberCallPull == null)
                return;

            byte[] lData = new byte[lsampleByteSize];

            uint lByteSize = (uint)mData.Length;

            int lframeCount = mSampleCount;

            while (true)
            {

            try
            {

                mISampleGrabberCallPull.readData(lData, out lByteSize);
            }
            catch (Exception)
            {
                lByteSize = 0;

                break;
            }
            finally
            {

                if (lByteSize > 0)
                {
                    Image lthakePhoto = null;

                    if (mIsMJPG)
                    {
                        Stream imageStreamSource = new MemoryStream(lData, 0, (int)lByteSize, false);
                        JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        BitmapSource bitmapSource = decoder.Frames[0];

                        lthakePhoto = new Image();

                        lthakePhoto.Source = bitmapSource;
                    }
                    else
                    {
                        lthakePhoto = new Image();

                        lthakePhoto.Source = FromArray(lData, mVideoWidth, mVideoHeight, 4);
                    }

                    if (lthakePhoto != null)
                    {
                        mPhotoStack.Children.Add(lthakePhoto);

                        lthakePhoto.Height = 70;

                        lthakePhoto.Margin = new Thickness(5);
                    }
                }
            }

                if (lByteSize == 0)
                {
                    break;
                }

                if (--lframeCount == 0)
                    break;
                
            }

        }
    }
}
