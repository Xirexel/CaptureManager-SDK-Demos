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
using System.Xml;

namespace WPFStreamingAudioRenderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        ISessionControl mISessionControl = null;

        ISession mISession = null;

        ISinkControl mSinkControl = null;

        ISourceControl mSourceControl = null;

        ISARSinkFactory mSARSinkFactory = null;

        ISARVolumeControl mISARVolumeControl = null;

        object mSARSinkOutputNode = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                mCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (Exception)
            {
                mCaptureManager = new CaptureManager();
            }

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

            mISARVolumeControl = mCaptureManager.createSARVolumeControl();

            if (mISARVolumeControl == null)
                return;

            mRVolume.ValueChanged += (s, ev) => {

                if(mSARSinkOutputNode != null)
                    mISARVolumeControl.setChannelVolume(mSARSinkOutputNode, 0, (float)ev.NewValue);
            };

            mLVolume.ValueChanged += (s, ev) => {

                if (mSARSinkOutputNode != null)
                    mISARVolumeControl.setChannelVolume(mSARSinkOutputNode, 1, (float)ev.NewValue);
            };


            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;



            mSinkControl.createSinkFactory(Guid.Empty, out mSARSinkFactory);

        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mISession != null)
            {
                mISession.stopSession();

                mISession.closeSession();

                mISession = null;

                mSARSinkOutputNode = null;

                mTitleTxtBlk.Text = "Start playing";

                return;
            }
                                 
            List<object> lSourceNodes = new List<object>();
            
            mSARSinkFactory.createOutputNode(out mSARSinkOutputNode);

            if (mSARSinkOutputNode == null)
                return;


            var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;

            if (l_AudioSourceXmlNode == null)
                return;

            var lNode = l_AudioSourceXmlNode.SelectSingleNode(
"Source.Attributes/Attribute" +
"[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
"/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;

            object lSourceNode = null;

            mSourceControl.createSourceNode(
                lSymbolicLink,
                0,
                0,
                mSARSinkOutputNode,
                out lSourceNode);

            if (lSourceNode == null)
                return;

            lSourceNodes.Add(lSourceNode);

            mISession = mISessionControl.createSession(lSourceNodes.ToArray());

            if (mISession == null)
                return;

            if (mISession.startSession(0, Guid.Empty))
            {
                mTitleTxtBlk.Text = "Stop playing";

                uint lChannelCount = 0;

                mISARVolumeControl.getChannelCount(mSARSinkOutputNode, out lChannelCount);

                if(lChannelCount > 0)
                {
                    float lLevel = 0;

                    mISARVolumeControl.getChannelVolume(mSARSinkOutputNode, 0, out lLevel);
                    
                    mRVolume.Value = lLevel;

                    if (lChannelCount > 1)
                    {
                        lLevel = 0;

                        mISARVolumeControl.getChannelVolume(mSARSinkOutputNode, 1, out lLevel);
                        
                        mLVolume.Value = lLevel;
                    }
                }
            }
        }

    }
}
