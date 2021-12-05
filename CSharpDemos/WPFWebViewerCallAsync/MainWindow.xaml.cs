using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace WPFWebViewerCallAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager;

        DispatcherTimer mTimer = new DispatcherTimer();

        ISampleGrabberCallSinkFactoryAsync mSinkFactory = null;

        ISampleGrabberCallAsync mISampleGrabberCall = null;

        ISessionAsync mISession = null;

        IWebCamControlAsync mWebCamControl = null;


        //ISessionAsync mISession = null;

        uint mVideoWidth = 0;

        uint mVideoHeight = 0;

        int mChannels = 4;

        Guid mReadMode;

        uint lsampleByteSize;

        byte[] mData = null;

        Guid MFMediaType_Video = new Guid(
0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

        Guid MFVideoFormat_RGB24 = new Guid(
 20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        Guid MFVideoFormat_RGB32 = new Guid(
 22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
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

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = "";

            lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            mTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);

            mWebCamParametrsTab.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mParametrSlider_ValueChanged));

            mWebCamParametrsTab.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

            mWebCamParametrsTab.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

            mTimer.Tick += mTimer_Tick;
        }

        async void mTimer_Tick(object sender, EventArgs e)
        {
            if (mISampleGrabberCall == null)
                return;

            uint lByteSize = (uint)mData.Length;

            try
            {

                lByteSize = await mISampleGrabberCall.readDataAsync(mData);
            }
            catch (Exception)
            {

            }
            finally
            {
                updateDisplayImage(this, mData, lByteSize);
            }
        }

        private async void mParametrSlider_ValueChanged(object sender, RoutedEventArgs e)//(object sender, RoutedPropertyChangedEventArgs<double> e)
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

            await mWebCamControl.setCamParametrAsync(
                lindex,
                lvalue,
                2);

        }

        private async void mParametrSlider_Checked(object sender, RoutedEventArgs e)
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

            if (lvalue == 2)
            {
                if (lCheckBox.Parent != null && lCheckBox.Parent is Grid)
                {
                    if ((lCheckBox.Parent as Grid).Children.Count == 2 && (lCheckBox.Parent as Grid).Children[0] is Slider)
                    {
                        lvalue = (int)((lCheckBox.Parent as Grid).Children[0] as Slider).Value;

                        await mWebCamControl.setCamParametrAsync(
                                lindex,
                                lvalue,
                                2);
                    }
                }

                return;
            }
            
            var lWebCamParametr = await mWebCamControl.getCamParametrAsync(
                lindex);

            await mWebCamControl.setCamParametrAsync(
                lindex,
                lWebCamParametr.mCurrentValue,
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

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (mLaunchButton.Content.ToString() == "Stop")
            {
                mTimer.Stop();

                if (mISession != null)
                    await mISession.closeSessionAsync();

                mLaunchButton.Content = "Launch";

                return;
            }
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content.ToString() == "Stop")
            {
                mTimer.Stop();

                if (mISession != null)
                    await mISession.closeSessionAsync();

                mLaunchButton.Content = "Launch";

                if (mISampleGrabberCall != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mISampleGrabberCall.getTopologyNode());

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
            
            if (!uint.TryParse(lNode.Value, out mVideoWidth))
            {
                return;
            }

            lNode = lSourceNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value");

            if (lNode == null)
                return;
            
            if (!uint.TryParse(lNode.Value, out mVideoHeight))
            {
                return;
            }

            int lWidthInBytes = await mCaptureManager.getStrideForBitmapInfoHeaderAsync(
                MFVideoFormat_RGB32,
                mVideoWidth);

            lsampleByteSize = (uint)Math.Abs(lWidthInBytes) * mVideoHeight;

            mData = new byte[lsampleByteSize];

            var lSinkControl = await mCaptureManager.createSinkControlAsync();

            string lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            setContainerFormat(lContainerNode);

            mSinkFactory = await lSinkControl.createSampleGrabberCallSinkFactoryAsync(mReadMode);

            mISampleGrabberCall = await mSinkFactory.createOutputNodeAsync(
                MFMediaType_Video,
                MFVideoFormat_RGB32,
                lsampleByteSize);

            if (mISampleGrabberCall != null)
            {
                var lSampleGrabberCallNode = mISampleGrabberCall.getTopologyNode();

                if (lSampleGrabberCallNode == null)
                    return;

                object lPtrSourceNode;

                var lSourceControl = await mCaptureManager.createSourceControlAsync();

                if (lSourceControl == null)
                    return;

                lPtrSourceNode = await lSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lSampleGrabberCallNode);

                List<object> lSourceMediaNodeList = new List<object>();

                lSourceMediaNodeList.Add(lPtrSourceNode);

                var lSessionControl = await mCaptureManager.createSessionControlAsync();

                if (lSessionControl == null)
                    return;

                mISession = await lSessionControl.createSessionAsync(
                    lSourceMediaNodeList.ToArray());

                if (mISession == null)
                    return;

                await mISession.startSessionAsync(0, Guid.Empty);

                mLaunchButton.Content = "Stop";

                mWebCamControl = await lSourceControl.createWebCamControlAsync(lSymbolicLink);

                if (mWebCamControl != null)
                {
                    string lXMLstring = await mWebCamControl.getCamParametrsAsync();

                    XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlWebCamParametrsProvider"];

                    if (lXmlDataProvider == null)
                        return;

                    System.Xml.XmlDocument ldoc = new System.Xml.XmlDocument();

                    if(!string.IsNullOrWhiteSpace(lXMLstring))
                        ldoc.LoadXml(lXMLstring);

                    lXmlDataProvider.Document = ldoc;

                }

                mTimer.Start();
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
    }
}
