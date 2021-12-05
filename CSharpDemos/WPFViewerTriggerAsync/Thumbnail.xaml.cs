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

namespace WPFViewerTriggerAsync
{
    public delegate void ChangeState(bool isEnable);

    /// <summary>
    /// Interaction logic for Thumbnail.xaml
    /// </summary>
    public partial class Thumbnail : UserControl
    {
        uint mVideoWidth = 0;

        uint mVideoHeight = 0;

        int mChannels = 0;

        ISampleGrabberCallSinkFactoryAsync mSampleGrabberSinkFactory = null;

        ISinkControlAsync mSinkControl = null;

        DispatcherTimer mTimer = new DispatcherTimer();

        Guid mReadMode;

        byte[] mData = null;

        public event ChangeState mChangeState = null;

        public bool mEnableTrigger = false;

        public float mThreshold = 50.0f;

        public Thumbnail()
        {
            InitializeComponent();

            mTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private async Task initInterface()
        {
            mSinkControl = await MainWindow.mCaptureManager.createSinkControlAsync();

            if (mSinkControl == null)
                return;

            string lxmldoc = await MainWindow.mCaptureManager.getCollectionOfSinksAsync();

            XmlDocument doc = new XmlDocument();

            doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[3]");

            if (lContainerNode == null)
                return;

            setContainerFormat(lContainerNode);

            mSampleGrabberSinkFactory = await mSinkControl.createSampleGrabberCallSinkFactoryAsync(
            mReadMode);
        }

        public void start()
        {
            mTimer.Start();
        }

        public void stop()
        {
            mTimer.Stop();
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

        public async Task<object> init(XmlNode aMediaTypeXmlNode)
        {
            await initInterface();

            object lresult = null;

            do
            {
                if (aMediaTypeXmlNode == null)
                    break;

                var lNode = aMediaTypeXmlNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    break;
                }

                lNode = aMediaTypeXmlNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

                if (lNode == null)
                    break;

                if (!uint.TryParse(lNode.Value, out mVideoWidth))
                {
                    break;
                }

                lNode = aMediaTypeXmlNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value");

                if (lNode == null)
                    break;

                if (!uint.TryParse(lNode.Value, out mVideoHeight))
                {
                    break;
                }

                Guid MFMediaType_Video = new Guid(
         0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

                Guid MFVideoFormat_RGB24 = new Guid(
         20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

                Guid MFVideoFormat_RGB32 = new Guid(
         22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

                int lWidthInBytes = await MainWindow.mCaptureManager.getStrideForBitmapInfoHeaderAsync(
                    MFVideoFormat_RGB32,
                    mVideoWidth);

                uint lsampleByteSize = (uint)Math.Abs(lWidthInBytes) * mVideoHeight;

                mChannels = 4;

                var lSampleGrabberCall = await mSampleGrabberSinkFactory.createOutputNodeAsync(
                    MFMediaType_Video,
                    MFVideoFormat_RGB32,
                    lsampleByteSize);

                if (lSampleGrabberCall == null)
                    break;

                mData = new byte[lsampleByteSize];

                lresult = lSampleGrabberCall.getTopologyNode();

                mTimer.Tick += async delegate (object sender, EventArgs e)
                {
                    uint lByteSize = (uint)mData.Length;

                    try
                    {

                        lByteSize = await lSampleGrabberCall.readDataAsync(mData);

                        if (mEnableTrigger && mChangeState != null)
                        {
                            float lvalue = 0;

                            for (int i = 0; i < lByteSize; i++)
                            {
                                lvalue += mData[i];
                            }

                            lvalue = ((lvalue / (float)lByteSize) * 100) / 255.0f;

                            if (lvalue >= mThreshold)
                                mChangeState(true);
                            else
                                mChangeState(false);
                        }
                    }
                    finally
                    {

                        updateDisplayImage();
                    }
                };

            } while (false);

            return lresult;
        }

        private void updateDisplayImage()
        {
            if (mData != null)
                mDisplayImage.Source = FromArray(mData, mVideoWidth, mVideoHeight, mChannels);
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
    }
}
