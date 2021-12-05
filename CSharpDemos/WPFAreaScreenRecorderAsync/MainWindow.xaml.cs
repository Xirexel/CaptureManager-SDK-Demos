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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using WPFAreaScreenRecorderAsync.Properties;

namespace WPFAreaScreenRecorderAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static CaptureManager mCaptureManager = null;

        ISessionControlAsync mISessionControl = null;

        ISessionAsync mISession = null;

        ISinkControlAsync mSinkControl = null;

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;

        IStreamControlAsync mStreamControl = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;

        IEVRMultiSinkFactoryAsync mEVRMultiSinkFactory = null;

        System.Drawing.Rectangle SelectionWindowArea = new System.Drawing.Rectangle();

        Guid mCLSIDVideoEncoder;

        Guid mCLSIDAudioEncoder;


        enum State
        {
            Stopped, Started, Paused
        }

        State mState = State.Stopped;

        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(Settings.Default.StoringDir))
                Settings.Default.StoringDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            System.Windows.MessageBox.Show(aMessage);
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

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            if (mCaptureManager == null)
                return;


            mSourceControl = await mCaptureManager.createSourceControlAsync();

            if (mSourceControl == null)
                return;

            mEncoderControl = await mCaptureManager.createEncoderControlAsync();

            if (mEncoderControl == null)
                return;

            mSinkControl = await mCaptureManager.createSinkControlAsync();

            if (mSinkControl == null)
                return;

            mISessionControl = await mCaptureManager.createSessionControlAsync();

            if (mISessionControl == null)
                return;

            mStreamControl = await mCaptureManager.createStreamControlAsync();

            if (mStreamControl == null)
                return;

            mSpreaderNodeFactory = await mStreamControl.createSpreaderNodeFactoryAsync();

            if (mSpreaderNodeFactory == null)
                return;

            mEVRMultiSinkFactory = await mSinkControl.createEVRMultiSinkFactoryAsync(Guid.Empty);

            if (mEVRMultiSinkFactory == null)
                return;


            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            lxmldoc = await mCaptureManager.getCollectionOfEncodersAsync();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;



            lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();


            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;
        }

        private string getVideoSybolicLink()
        {
            var lSourceNode = m_VideoSourceComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return "";

            var lNode = lSourceNode.SelectSingleNode(
        "Source.Attributes/Attribute" +
        "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
        "/SingleValue/@Value");

            if (lNode == null)
                return "";

            return lNode.Value;
        }

        private string getAudioSybolicLink()
        {
            return ConfigWindow.mAudioSymbolicLink;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ConfigWindow.mCurrentSymbolicLink = getVideoSybolicLink();

            new ConfigWindow().ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            var lSourceNode = m_VideoSourceComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            var lNode = lSourceNode.SelectSingleNode(
        "Source.Attributes/Attribute" +
        "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
        "/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;

            ConfigWindow.mCurrentSymbolicLink = lSymbolicLink;

            Window lAreaWindow = new AreaWindow();

            lAreaWindow.Top = SelectionWindowArea.Top;

            lAreaWindow.Height = SelectionWindowArea.Height;

            lAreaWindow.Left = SelectionWindowArea.Left;

            lAreaWindow.Width = SelectionWindowArea.Width;

            lAreaWindow.ShowDialog();

            if (AreaWindow.mSelectedRegion.IsEmpty)
                mAreaLable.Text = "Whole Area";
            else
            {
                int lLeft = 0, lTop = 0, lWidth = 0, lHeight = 0;

                lLeft = ((int)AreaWindow.mSelectedRegion.X >> 1) << 1;

                lTop = ((int)AreaWindow.mSelectedRegion.Y >> 1) << 1;

                lWidth = ((int)AreaWindow.mSelectedRegion.Width >> 1) << 1;

                lHeight = ((int)AreaWindow.mSelectedRegion.Height >> 1) << 1;

                mAreaLable.Text = string.Format("Left {0}, Top {1}, Widt {2}, Height {3}", lLeft, lTop, lWidth, lHeight);
            }

        }

        string m_VideoSymbolicLink = "";

        private async void mStartStop_Click(object sender, RoutedEventArgs e)
        {
            m_VideoSymbolicLink = getVideoSybolicLink();

            bool l_is_VideoStreamPreview = (bool)m_VideoStreamPreviewChkBtn.IsChecked;

            mStartStop.IsEnabled = false;


            if (mState == State.Started)
            {
                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

                mISession = null;

                mState = State.Stopped;

                mStartStop.Content = "Start";

                mStartStop.IsEnabled = true;

                return;
            }

            getEncoderInfo();

            List<object> lCompressedMediaTypeList = new List<object>();

            object lCompressedMediaType = await getVideoCompressedMediaType();

            if (lCompressedMediaType != null)
                lCompressedMediaTypeList.Add(lCompressedMediaType);

            lCompressedMediaType = await getAudioCompressedMediaType();

            if (lCompressedMediaType != null)
                lCompressedMediaTypeList.Add(lCompressedMediaType);

            List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList);

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return;

            int lOutputIndex = 0;

            List<object> lSourceNodes = new List<object>();


            object RenderNode = null;

            if (l_is_VideoStreamPreview)
            {
                List<object> lRenderOutputNodesList = new List<object>();

                if (mEVRMultiSinkFactory != null)
                    lRenderOutputNodesList = await mEVRMultiSinkFactory.createOutputNodesAsync(
                        m_EVRDisplay.Handle,
                        null,
                        1);

                if (lRenderOutputNodesList.Count == 1)
                {
                    RenderNode = lRenderOutputNodesList[0];
                }
            }



            object lSourceNode = await getVideoSourceNode(
                RenderNode,
                lOutputNodes[lOutputIndex++]);

            if (lSourceNodes != null)
                lSourceNodes.Add(lSourceNode);


            lSourceNode = await getAudioSourceNode(
                lOutputNodes[lOutputIndex++]);

            if (lSourceNodes != null)
                lSourceNodes.Add(lSourceNode);

            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession == null)
                return;

            var lisStarted = await mISession.startSessionAsync(0, Guid.Empty);

            if (lisStarted)
            {
                mState = State.Started;

                mStartStop.Content = "Stop";

                mStartStop.IsEnabled = true;
            }
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "0x" + c.A.ToString("X2") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private string getScreenCaptureSymbolicLink(string aSymbolicLink)
        {

            string loptions = " --options=" +
            "<?xml version='1.0' encoding='UTF-8'?>" +
            "<Options>";

            {
                string lcursorOption =
                "<Option Type='Cursor' Visiblity='Temp_Visiblity'>";

                lcursorOption = lcursorOption.Replace("Temp_Visiblity", Settings.Default.ShowCursor.ToString());


                if (Settings.Default.CursorMask > 0)
                {
                    string lshape = "";

                    switch (Settings.Default.CursorMask)
                    {
                        case 1:
                            lshape = "Rectangle";
                            break;

                        case 2:
                            lshape = "Ellipse";
                            break;
                        default:
                            break;
                    }

                    if (!string.IsNullOrEmpty(lshape))
                    {

                        lcursorOption +=
                            "<Option.Extensions>" +
                                "<Extension Type='BackImage' Height='100' Width='100' Fill='Temp_Fill' Shape='Temp_Shape' />" +
                            "</Option.Extensions>";

                        lcursorOption = lcursorOption.Replace("Temp_Shape", lshape).Replace("Temp_Fill", HexConverter(Settings.Default.MaskColor));


                    }
                }

                lcursorOption += "</Option>";

                loptions += lcursorOption;
            }






            if (!AreaWindow.mSelectedRegion.IsEmpty)
            {

                int lLeft = 0, lTop = 0, lWidth = 0, lHeight = 0;

                lLeft = ((int)AreaWindow.mSelectedRegion.X >> 1) << 1;

                lTop = ((int)AreaWindow.mSelectedRegion.Y >> 1) << 1;

                lWidth = ((int)AreaWindow.mSelectedRegion.Width >> 1) << 1;

                lHeight = ((int)AreaWindow.mSelectedRegion.Height >> 1) << 1;

                string lcursorOption =
                "<Option Type='Clip'>" +
                    "<Option.Extensions>" +
                        "<Extension Left='Temp_Left' Top='Temp_Top' Height='Temp_Height' Width='Temp_Width'/>" +
                    "</Option.Extensions>" +
                "</Option>";

                loptions += lcursorOption.Replace("Temp_Left", lLeft.ToString())
                    .Replace("Temp_Top", lTop.ToString())
                    .Replace("Temp_Height", lHeight.ToString())
                    .Replace("Temp_Width", lWidth.ToString());
            }

            loptions += "</Options>";

            return aSymbolicLink + loptions;
        }

        private async void getEncoderInfo()
        {
            XmlDocument doc = new XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfEncodersAsync();

            doc = new XmlDocument();


            doc.LoadXml(lxmldoc);

            var lNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory");

            var lXmlNode = lNodes.Item((int)Settings.Default.VideoEncoderNumber);

            if (lXmlNode == null)
                return;

            var lEncoderGuidAttr = lXmlNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null)
                return;

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                return;

            mCLSIDVideoEncoder = lCLSIDEncoder;





            lNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory");

            lXmlNode = lNodes.Item((int)Settings.Default.AudioEncoderNumber);

            if (lXmlNode == null)
                return;

            lEncoderGuidAttr = lXmlNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null)
                return;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                return;

            mCLSIDAudioEncoder = lCLSIDEncoder;
        }

        private async Task<Guid> getVideoEncoderMode()
        {
            Guid lEncoderMode = Guid.Empty;

            do
            {
                if (mEncoderControl == null)
                    break;

                uint lStreamIndex = Settings.Default.VideoSourceStream;

                uint lMediaTypeIndex = Settings.Default.VideoSourceMediaType;

                object lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    m_VideoSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                string lMediaTypeCollection = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                    lOutputMediaType,
                    mCLSIDVideoEncoder);

                if (string.IsNullOrWhiteSpace(lMediaTypeCollection))
                    break;


                XmlDocument lEncoderModedoc = new XmlDocument();

                lEncoderModedoc.LoadXml(lMediaTypeCollection);

                var lNodes = lEncoderModedoc.SelectNodes("EncoderMediaTypes/Group");

                var lXmlNode = lNodes.Item((int)Settings.Default.VideoEncoderMode);

                if (lXmlNode == null)
                    break;

                var lEncoderModeGuidAttr = lXmlNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;

                lEncoderMode = lCLSIDEncoderMode;

            } while (false);

            return lEncoderMode;
        }

        private async Task<Guid> getAudioEncoderMode()
        {
            Guid lEncoderMode = Guid.Empty;

            do
            {
                if (mEncoderControl == null)
                    break;

                string lSymbolicLink = getAudioSybolicLink();

                uint lStreamIndex = Settings.Default.AudioSourceStream;

                uint lMediaTypeIndex = Settings.Default.AudioSourceMediaType;

                object lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                string lMediaTypeCollection = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                    lOutputMediaType,
                    mCLSIDAudioEncoder);

                if (string.IsNullOrWhiteSpace(lMediaTypeCollection))
                    break;


                XmlDocument lEncoderModedoc = new XmlDocument();

                lEncoderModedoc.LoadXml(lMediaTypeCollection);

                var lNodes = lEncoderModedoc.SelectNodes("EncoderMediaTypes/Group");

                var lXmlNode = lNodes.Item((int)Settings.Default.AudioEncoderMode);

                if (lXmlNode == null)
                    break;

                var lEncoderModeGuidAttr = lXmlNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;

                lEncoderMode = lCLSIDEncoderMode;

            } while (false);

            return lEncoderMode;
        }

        private XmlNode getFileFormat()
        {
            XmlNode lXmlNode = null;

            do
            {
                if (mEncoderControl == null)
                    break;

                string lxmldoc = "";

                mCaptureManager.getCollectionOfSinks(ref lxmldoc);

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(lxmldoc);

                var lNodes = doc.SelectNodes("SinkFactories/SinkFactory[@GUID='{D6E342E3-7DDD-4858-AB91-4253643864C2}']/Value.ValueParts/ValuePart");

                lXmlNode = lNodes.Item((int)Settings.Default.FileFormatNumber);

                if (lXmlNode == null)
                    break;

            } while (false);

            return lXmlNode;
        }

        private async Task<object> getVideoCompressedMediaType()
        {
            object lresult = null;

            do
            {
                Guid lCLSIDEncoderMode = await getVideoEncoderMode();

                uint lStreamIndex = Settings.Default.VideoSourceStream;

                uint lMediaTypeIndex = Settings.Default.VideoSourceMediaType;

                var lSymbolicLink = getScreenCaptureSymbolicLink(m_VideoSymbolicLink);

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(mCLSIDVideoEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lCompressedMediaType = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    (uint)Settings.Default.VideoCompressionQuality,
                    Settings.Default.VideoEncoderMediaType);

                lresult = lCompressedMediaType;

            } while (false);

            return lresult;
        }

        private async Task<object> getAudioCompressedMediaType()
        {
            object lresult = null;

            do
            {
                Guid lCLSIDEncoderMode = await getAudioEncoderMode();

                string lSymbolicLink = getAudioSybolicLink();

                uint lStreamIndex = Settings.Default.AudioSourceStream;

                uint lMediaTypeIndex = Settings.Default.AudioSourceMediaType;

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(mCLSIDAudioEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lCompressedMediaType = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    (uint)Settings.Default.AudioCompressionQuality,
                    Settings.Default.AudioEncoderMediaType);

                lresult = lCompressedMediaType;

            } while (false);

            return lresult;
        }

        private async Task<List<object>> getOutputNodes(List<object> aCompressedMediaTypeList)
        {
            List<object> lresult = new List<object>();

            do
            {
                if (aCompressedMediaTypeList == null)
                    break;

                if (aCompressedMediaTypeList.Count == 0)
                    break;

                var lselectedNode = getFileFormat();


                var lSelectedAttr = lselectedNode.Attributes["GUID"];

                if (lSelectedAttr == null)
                    break;

                var mFileSinkFactory = await mSinkControl.createFileSinkFactoryAsync(Guid.Parse(lSelectedAttr.Value));

                if (mFileSinkFactory == null)
                    break;

                lSelectedAttr = lselectedNode.Attributes["Value"];

                if (lSelectedAttr == null)
                    break;

                string s = String.Format("Video_{0:yyyy_MM_dd_HH_mm_ss}.", DateTime.Now);

                string mFilename = s + lSelectedAttr.Value.ToLower();

                mStatus.Text = "File: " + mFilename;

                mFilename = Settings.Default.StoringDir + @"\" + mFilename;

                if (string.IsNullOrEmpty(mFilename))
                    break;

                lresult = await mFileSinkFactory.createOutputNodesAsync(
                    aCompressedMediaTypeList,
                    mFilename);

            } while (false);

            return lresult;
        }

        private async Task<object> getVideoSourceNode(
            object PreviewRenderNode,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                Guid lCLSIDEncoderMode = await getVideoEncoderMode();

                uint lStreamIndex = Settings.Default.VideoSourceStream;

                uint lMediaTypeIndex = Settings.Default.VideoSourceMediaType;

                var lSymbolicLink = getScreenCaptureSymbolicLink(m_VideoSymbolicLink);

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(mCLSIDVideoEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode = await lEncoderNodeFactory.createEncoderNodeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    (uint)Settings.Default.VideoCompressionQuality,
                    Settings.Default.VideoEncoderMediaType,
                    aOutputNode);


                object SpreaderNode = lEncoderNode;

                if (PreviewRenderNode != null)
                {

                    List<object> lOutputNodeList = new List<object>();

                    lOutputNodeList.Add(PreviewRenderNode);

                    lOutputNodeList.Add(lEncoderNode);

                    SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(lOutputNodeList);

                }

                object lSourceNode = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    SpreaderNode);

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }

        private async Task<object> getAudioSourceNode(
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                Guid lCLSIDEncoderMode = await getAudioEncoderMode();

                string lSymbolicLink = getAudioSybolicLink();

                uint lStreamIndex = Settings.Default.AudioSourceStream;

                uint lMediaTypeIndex = Settings.Default.AudioSourceMediaType;

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(mCLSIDAudioEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode = await lEncoderNodeFactory.createEncoderNodeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    (uint)Settings.Default.AudioCompressionQuality,
                    Settings.Default.AudioEncoderMediaType,
                    aOutputNode);
                    
                object lSourceNode = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lEncoderNode);

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }

        private void m_VideoSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AreaWindow.mSelectedRegion = Rect.Empty;

            var lSymbolicLink = getVideoSybolicLink();

            Screen lselectedScreen = null;

            if (!lSymbolicLink.Contains("DISPLAY"))
            {
                lselectedScreen = Screen.PrimaryScreen;
            }
            else
            {
                foreach (var item in Screen.AllScreens)
                {
                    if (lSymbolicLink.Contains(item.DeviceName))
                    {
                        lselectedScreen = item;

                        break;
                    }

                }
            }

            SelectionWindowArea = lselectedScreen.WorkingArea;
        }
    }
}
