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

namespace WPFWebCamShot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static CaptureManager mCaptureManager;
        
        ISampleGrabberCallSinkFactory mSinkFactory = null;

        ISession mISession = null;

        ISampleGrabberCall mISampleGrabber = null;

        Guid MFMediaType_Video = new Guid(
 0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
        
        Guid MFVideoFormat_RGB32 = new Guid(
 22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        Guid MFVideoFormat_RGB24 = new Guid(
 20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
                
        Guid MFVideoFormat_MJPG = new Guid(
 0x47504A4D, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        bool mIsMJPG = false;

        uint mVideoWidth = 0;

        uint mVideoHeight = 0;

        int mChannels = 0;

        IWebCamControl mWebCamControl;

        Guid mReadMode;

        ISampleGrabberCall mISampleGrabberCall;

        //DispatcherTimer mTimer = new DispatcherTimer();

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

            //mTimer.Interval = new TimeSpan(100000);

            mWebCamParametrsTab.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mParametrSlider_ValueChanged));

            mWebCamParametrsTab.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

            mWebCamParametrsTab.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

            //mTimer.Tick += mTimer_Tick;
        }

        //void mTimer_Tick(object sender, EventArgs e)
        //{
        //    if (mISampleGrabber == null)
        //        return;

        //    uint lByteSize = (uint)mData.Length;

        //    try
        //    {

        //        mISampleGrabber.readData(mData, out lByteSize);
        //    }
        //    catch (Exception exc)
        //    {

        //    }
        //    finally
        //    {

        //        updateDisplayImage(this, mData, lByteSize);
        //    }
        //}

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content == "Stop")
            {
                //mTimer.Stop();

                if (mISession != null)
                    mISession.closeSession();

                mLaunchButton.Content = "Launch";

                mTakePhotoButton.IsEnabled = false;

                return;
            }

            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;

            lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null)
                return;

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex))
            {
                return;
            }

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

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[3]");

            if (lContainerNode == null)
                return;

            setContainerFormat(lContainerNode);

            lSinkControl.createSinkFactory(
            mReadMode,
            out mSinkFactory);

            lNode = lSourceNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");
  
            if (lNode == null)
                return;

            if(lNode.Value == "MFVideoFormat_MJPG")
            {
                mSinkFactory.createOutputNode(
                    MFMediaType_Video,
                    MFVideoFormat_MJPG,
                    lsampleByteSize,
                    out mISampleGrabberCall);

                mIsMJPG = true;

            }
            else if (lNode.Value == "MFVideoFormat_RGB24")
            {

                mSinkFactory.createOutputNode(
                    MFMediaType_Video,
                    MFVideoFormat_RGB24,
                    lsampleByteSize,
                    out mISampleGrabberCall);

                mChannels = 3;

            }
            else
            {

                mSinkFactory.createOutputNode(
                    MFMediaType_Video,
                    MFVideoFormat_RGB32,
                    lsampleByteSize,
                    out mISampleGrabberCall);

                mChannels = 4;
            }

            

            if (mISampleGrabberCall != null)
            {
                byte[] lData = new byte[lsampleByteSize];
                
                var lSampleGrabberCallNode = mISampleGrabberCall.getTopologyNode();

                if (lSampleGrabberCallNode == null)
                    return;

                object lPtrSourceNode;

                var lSourceControl = mCaptureManager.createSourceControl();

                if (lSourceControl == null)
                    return;

                lSourceControl.createSourceNode(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lSampleGrabberCallNode,
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

                //mTimer.Start();

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
                //mTimer.Stop();

                if (mISession != null)
                    mISession.closeSession();

                mLaunchButton.Content = "Launch";

                return;
            }
        }

        private void mTakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            if (mISampleGrabberCall == null)
                return;

            byte[] lData = new byte[lsampleByteSize];

            uint lByteSize = (uint)mData.Length;

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
                {
                    if(mIsMJPG)
                    {
                        Stream imageStreamSource = new MemoryStream(lData, 0, (int)lByteSize, false);
                        JpegBitmapDecoder decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        BitmapSource bitmapSource = decoder.Frames[0];
                                                
                        mDisplayImage.Source = bitmapSource;
                    }
                    else
                    {
                        mDisplayImage.Source = FromArray(lData, mVideoWidth, mVideoHeight, mChannels);
                    }
                }
            }
              
        }
    }
}
