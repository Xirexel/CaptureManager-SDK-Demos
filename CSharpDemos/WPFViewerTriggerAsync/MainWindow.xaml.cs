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

namespace WPFViewerTriggerAsync
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

        IStreamControlAsync mStreamControl = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;

        IEVRMultiSinkFactoryAsync mEVRMultiSinkFactory = null;

        ISwitcherControlAsync mSwitcherControl = null;

        bool mIsPausedDisplay = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
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

            m_Thumbnail.mChangeState += m_Thumbnail_mChangeState;

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            if (mCaptureManager == null)
                return;

            mSourceControl = await mCaptureManager.createSourceControlAsync();

            if (mSourceControl == null)
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

            mSwitcherControl = await mCaptureManager.createSwitcherControlAsync();

            if (mSwitcherControl == null)
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

        }

        private async void m_Thumbnail_mChangeState(bool isEnable)
        {
            if (isEnable)
            {
                await mSwitcherControl.resumeSwitchersAsync(mISession);
            }
            else
            {
                await mSwitcherControl.pauseSwitchersAsync(mISession);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            m_Thumbnail.stop();

            if (mISession != null)
            {
                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

                mISession = null;

                return;
            }

            var lThumbnailOutputNode = await m_Thumbnail.init(m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode);

            if (lThumbnailOutputNode == null)
                return;

            object lSwitcherNode = await getSwitcher();

            if (lSwitcherNode == null)
                return;

            object SpreaderNode = lSwitcherNode;


            List<object> lOutputNodeList = new List<object>();

            lOutputNodeList.Add(lSwitcherNode);

            lOutputNodeList.Add(lThumbnailOutputNode);

            SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                lOutputNodeList);

            object lSourceNode = await getSourceNode(
                m_VideoSourceComboBox.SelectedItem as XmlNode,
                m_VideoStreamComboBox.SelectedItem as XmlNode,
                m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                SpreaderNode);

            List<object> lSourceNodes = new List<object>();

            lSourceNodes.Add(lSourceNode);

            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession == null)
                return;

            await mSwitcherControl.pauseSwitchersAsync(mISession);

            mIsPausedDisplay = true;

            if (await mISession.startSessionAsync(0, Guid.Empty))
            {
                m_Thumbnail.start();
            }
        }

        private async Task<object> getSourceNode(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                if (aSourceNode == null)
                    break;

                if (aMediaTypeNode == null)
                    break;


                if (aOutputNode == null)
                    break;


                if (aSourceNode == null)
                    break;

                var lNode = aSourceNode.SelectSingleNode(
            "Source.Attributes/Attribute" +
            "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
            "/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                uint lStreamIndex = 0;

                if (aMediaTypeNode == null)
                    break;

                lNode = aMediaTypeNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    break;
                }

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                string lextendSymbolicLink = lSymbolicLink + " --options=" +
    "<?xml version='1.0' encoding='UTF-8'?>" +
    "<Options>" +
        "<Option Type='Cursor' Visiblity='True'>" +
            "<Option.Extensions>" +
                "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
            "</Option.Extensions>" +
        "</Option>" +
    "</Options>";

                object lSourceNode = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lextendSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    aOutputNode);

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }

        private async Task<object> getSwitcher()
        {
            object lresult = null;

            do
            {
                object RenderNode = null;

                List<object> lRenderOutputNodesList = new List<object>();

                if (mEVRMultiSinkFactory != null)
                    lRenderOutputNodesList = await mEVRMultiSinkFactory.createOutputNodesAsync(
                        IntPtr.Zero,
                        m_EVRDisplay.Surface.texture,
                        1);

                if (lRenderOutputNodesList.Count == 1)
                {
                    RenderNode = lRenderOutputNodesList[0];
                }

                var lSwitcherNodeFactory = await mStreamControl.createSwitcherNodeFactoryAsync();
                 
                if (lSwitcherNodeFactory == null)
                    break;

                lresult = await lSwitcherNodeFactory.createSwitcherNodeAsync(RenderNode);

            } while (false);

            return lresult;
        }

        private async void mPauseResumeDisplayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mISession != null)
            {
                if (mIsPausedDisplay)
                {
                    await mSwitcherControl.resumeSwitchersAsync(mISession);

                    //m_EVRDisplay.Visibility = System.Windows.Visibility.Visible;

                    mPauseResumeDisplayBtn.Content = "Pause Display";
                }
                else
                {
                    await mSwitcherControl.pauseSwitchersAsync(mISession);

                    //m_EVRDisplay.Visibility = System.Windows.Visibility.Hidden;

                    mPauseResumeDisplayBtn.Content = "Resume Display";
                }

                mIsPausedDisplay = !mIsPausedDisplay;
            }
        }

        private void mTrigger_Click(object sender, RoutedEventArgs e)
        {
            m_Thumbnail.mEnableTrigger = (bool)mTrigger.IsChecked;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider lSlider = (Slider)sender;

            if (lSlider != null)
            {
                m_Thumbnail.mThreshold = (float)lSlider.Value * 100.0f;
            }
        }
    }
}
