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

namespace WPFViewerTrigger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static CaptureManager mCaptureManager = null;

        ISessionControl mISessionControl = null;

        ISession mISession = null;

        ISinkControl mSinkControl = null;

        ISourceControl mSourceControl = null;
                
        IStreamControl mStreamControl = null;

        ISpreaderNodeFactory mSpreaderNodeFactory = null;

        IEVRMultiSinkFactory mEVRMultiSinkFactory = null;

        ISwitcherControl mSwitcherControl = null;

        bool mIsPausedDisplay = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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

            m_Thumbnail.mChangeState+=m_Thumbnail_mChangeState;

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            if (mCaptureManager == null)
                return;

            mSourceControl = mCaptureManager.createSourceControl();

            if (mSourceControl == null)
                return;
            
            mSinkControl = mCaptureManager.createSinkControl();

            if (mSinkControl == null)
                return;

            mISessionControl = mCaptureManager.createSessionControl();

            if (mISessionControl == null)
                return;

            mStreamControl = mCaptureManager.createStreamControl();

            if (mStreamControl == null)
                return;

            mStreamControl.createStreamControlNodeFactory(ref mSpreaderNodeFactory);

            if (mSpreaderNodeFactory == null)
                return;

            mSinkControl.createSinkFactory(Guid.Empty, out mEVRMultiSinkFactory);

            if (mEVRMultiSinkFactory == null)
                return;

            mSwitcherControl = mCaptureManager.createSwitcherControl();

            if (mSwitcherControl == null)
                return;





            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

        }

        private void m_Thumbnail_mChangeState(bool isEnable)
        {
            if(isEnable)
            {
                mSwitcherControl.resumeSwitchers(mISession);
            }
            else
            {
                mSwitcherControl.pauseSwitchers(mISession);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var lThumbnailOutputNode = m_Thumbnail.init(m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode);

            if (lThumbnailOutputNode == null)
                return;
            
            object lSwitcherNode = getSwitcher();

            if (lSwitcherNode == null)
                return;

            object SpreaderNode = lSwitcherNode;


            List<object> lOutputNodeList = new List<object>();

            lOutputNodeList.Add(lSwitcherNode);

            lOutputNodeList.Add(lThumbnailOutputNode);

            mSpreaderNodeFactory.createSpreaderNode(
                lOutputNodeList,
                out SpreaderNode);

            object lSourceNode = getSourceNode(
                m_VideoSourceComboBox.SelectedItem as XmlNode,
                m_VideoStreamComboBox.SelectedItem as XmlNode,
                m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                SpreaderNode);

            List<object> lSourceNodes = new List<object>();

            lSourceNodes.Add(lSourceNode);

            mISession = mISessionControl.createSession(lSourceNodes.ToArray());

            if (mISession == null)
                return;

            mSwitcherControl.pauseSwitchers(mISession);

            mIsPausedDisplay = true;

            if (mISession.startSession(0, Guid.Empty))
            {
                m_Thumbnail.start();
            }
        }
                
        private object getSourceNode(
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


                if (aStreamNode == null)
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

                if (aStreamNode == null)
                    break;

                lNode = aStreamNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lStreamIndex = 0;

                if (!uint.TryParse(lNode.Value, out lStreamIndex))
                {
                    break;
                }

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

                object lSourceMediaType = null;

                if (!mSourceControl.getSourceOutputMediaType(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    out lSourceMediaType))
                    break;

                if (lSourceMediaType == null)
                    break;
                
                object lSourceNode;

                string lextendSymbolicLink = lSymbolicLink + " --options=" +
    "<?xml version='1.0' encoding='UTF-8'?>" +
    "<Options>" +
        "<Option Type='Cursor' Visiblity='True'>" +
            "<Option.Extensions>" +
                "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
            "</Option.Extensions>" +
        "</Option>" +
    "</Options>";

                if (!mSourceControl.createSourceNode(
                    lextendSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    aOutputNode,
                    out lSourceNode))
                    break;

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }
        
        private object getSwitcher()
        {
            object lresult = null;

            do
            {
                object RenderNode = null;

                List<object> lRenderOutputNodesList = new List<object>();

                if (mEVRMultiSinkFactory != null)
                    mEVRMultiSinkFactory.createOutputNodes(
                        IntPtr.Zero,
                        m_EVRDisplay.Surface.texture,
                        1,
                        out lRenderOutputNodesList);

                if (lRenderOutputNodesList.Count == 1)
                {
                    RenderNode = lRenderOutputNodesList[0];
                }

                ISwitcherNodeFactory lSwitcherNodeFactory = null;

                if (!mStreamControl.createStreamControlNodeFactory(ref lSwitcherNodeFactory))
                    break;

                if (lSwitcherNodeFactory == null)
                    break;

                lSwitcherNodeFactory.createSwitcherNode(RenderNode, out lresult);

            } while (false);

            return lresult;
        }

        private void mPauseResumeDisplayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mISession != null)
            {
                if (mIsPausedDisplay)
                {
                    mSwitcherControl.resumeSwitchers(mISession);

                    //m_EVRDisplay.Visibility = System.Windows.Visibility.Visible;

                    mPauseResumeDisplayBtn.Content = "Pause Display";
                }
                else
                {
                    mSwitcherControl.pauseSwitchers(mISession);

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

            if(lSlider != null)
            {
                m_Thumbnail.mThreshold = (float)lSlider.Value * 100.0f;
            }
        }        
    }
}
