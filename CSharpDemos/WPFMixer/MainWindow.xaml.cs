using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace WPFMixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static CaptureManager mCaptureManager = null;

        ISessionControl mISessionControl = null;

        ISession mISession = null;

        ISession mAddCameraSession = null;

        ISession mAddImageSession = null;

        ISession mAddMicSession = null;

        ISinkControl mSinkControl = null;

        ISourceControl mSourceControl = null;

        IEncoderControl mEncoderControl = null;

        IStreamControl mStreamControl = null;

        IFileSinkFactory mFileSinkFactory = null;

        ISpreaderNodeFactory mSpreaderNodeFactory = null;



        
        Guid m_VideoEncoderMode = new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55);
        
        Guid m_AudioEncoderMode = new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55);
                            



        public CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl mIEVRStreamControl = null;

        List<object> mVideoTopologyInputMixerNodes = new List<object>();

        object mCameraVideoTopologyInputMixerNode = null;

        object mImageVideoTopologyInputMixerNode = null;

        object mAudioTopologyInputMixerNode = null;


        public MainWindow()
        {
            InitializeComponent();


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

            mSourceControl = mCaptureManager.createSourceControl();

            mEncoderControl = mCaptureManager.createEncoderControl();

            mSinkControl = mCaptureManager.createSinkControl();

            mStreamControl = mCaptureManager.createStreamControl();

            mISessionControl = mCaptureManager.createSessionControl();


            mStreamControl.createStreamControlNodeFactory(ref mSpreaderNodeFactory);

            if (mSpreaderNodeFactory == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;




            XmlDataProvider lXmlContainerTypeProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlContainerTypeProvider.Document = doc;



        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private void init()
        {

            var lselectedNode = m_FileFormatComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null)
                return;

            var lSelectedAttr = lselectedNode.Attributes["Value"];

            if (lSelectedAttr == null)
                return;

            String limageSourceDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            SaveFileDialog lsaveFileDialog = new SaveFileDialog();

            lsaveFileDialog.InitialDirectory = limageSourceDir;

            lsaveFileDialog.DefaultExt = "." + lSelectedAttr.Value.ToLower();

            lsaveFileDialog.AddExtension = true;

            lsaveFileDialog.CheckFileExists = false;

            lsaveFileDialog.Filter = "Media file (*." + lSelectedAttr.Value.ToLower() + ")|*." + lSelectedAttr.Value.ToLower();

            var lresult = lsaveFileDialog.ShowDialog();

            if (lresult != true)
                return;

            var lFilename = lsaveFileDialog.FileName;

            lSelectedAttr = lselectedNode.Attributes["GUID"];

            if (lSelectedAttr == null)
                return;

            mSinkControl.createSinkFactory(
            Guid.Parse(lSelectedAttr.Value),
            out mFileSinkFactory);



            string lScreenCaptureSymbolicLink = "CaptureManager///Software///Sources///ScreenCapture///ScreenCapture";

            string lAudioLoopBack = "CaptureManager///Software///Sources///AudioEndpointCapture///AudioLoopBack";


            // Video Source
            uint lVideoSourceIndexStream = 0;

            uint lVideoSourceIndexMediaType = 2;

            int l_VideoCompressedMediaTypeSelectedIndex = 0;



            // Audio Source
            uint lAudioSourceIndexStream = 0;

            uint lAudioSourceIndexMediaType = 0;


            int l_AudioCompressedMediaTypeSelectedIndex = 0;


            string l_EncodersXMLstring = "";

            mEncoderControl.getCollectionOfEncoders(out l_EncodersXMLstring);


            XmlDocument doc = new XmlDocument();
            
            doc.LoadXml(l_EncodersXMLstring);

            var lAttrNode = doc.SelectSingleNode("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory[1]/@CLSID");

            if (lAttrNode == null)
                return;
                       
            Guid l_VideoEncoder = Guid.Empty;

            Guid.TryParse(lAttrNode.Value, out l_VideoEncoder);


            lAttrNode = doc.SelectSingleNode("EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory[1]/@CLSID");

            if (lAttrNode == null)
                return;

            Guid l_AudioEncoder = Guid.Empty;

            Guid.TryParse(lAttrNode.Value, out l_AudioEncoder);
            


            List<object> lCompressedMediaTypeList = new List<object>();

            if (true)
            {
                object lCompressedMediaType = getCompressedMediaType(
                        lScreenCaptureSymbolicLink,
                        lVideoSourceIndexStream,
                        lVideoSourceIndexMediaType,
                        l_VideoEncoder,
                        m_VideoEncoderMode,
                        l_VideoCompressedMediaTypeSelectedIndex);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }


            if (true)
            {
                object lCompressedMediaType = getCompressedMediaType(
                        lAudioLoopBack,
                        lAudioSourceIndexStream,
                        lAudioSourceIndexMediaType,
                        l_AudioEncoder,
                        m_AudioEncoderMode,
                        l_AudioCompressedMediaTypeSelectedIndex);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }


            List<object> lOutputNodes = getOutputNodes(lCompressedMediaTypeList, lFilename);

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return;
                                          
            IEVRSinkFactory lSinkFactory;

            mSinkControl.createSinkFactory(
            Guid.Empty,
            out lSinkFactory);

            object lEVROutputNode = null;

            lSinkFactory.createOutputNode(
                    mVideoPanel.Handle,
                    out lEVROutputNode);

            if (lEVROutputNode == null)
                return;
            

            object SpreaderNode = lEVROutputNode;

            if (true)
            {
                var lEncoderNode = getEncoderNode(
                        lScreenCaptureSymbolicLink,
                        lVideoSourceIndexStream,
                        lVideoSourceIndexMediaType,
                        l_VideoEncoder,
                        m_VideoEncoderMode,
                        l_VideoCompressedMediaTypeSelectedIndex,
                        lOutputNodes[0]);

                List<object> lOutputNodeList = new List<object>();

                lOutputNodeList.Add(lEncoderNode);

                lOutputNodeList.Add(lEVROutputNode);

                mSpreaderNodeFactory.createSpreaderNode(
                    lOutputNodeList,
                    out SpreaderNode);

                //SpreaderNode = lEncoderNode;
            }






            IMixerNodeFactory lMixerNodeFactory = null;

            mStreamControl.createStreamControlNodeFactory(ref lMixerNodeFactory);

            List<object> lVideoTopologyInputMixerNodes;

            lMixerNodeFactory.createMixerNodes(
                SpreaderNode,
                2,
                out lVideoTopologyInputMixerNodes);

            if (lVideoTopologyInputMixerNodes.Count == 0)
                return;

            for (int i = 1; i < lVideoTopologyInputMixerNodes.Count; i++)
            {
                mVideoTopologyInputMixerNodes.Add(lVideoTopologyInputMixerNodes[i]);
            }






            object lAudioEncoderNode = null;

            if (true)
            {
                lAudioEncoderNode = getEncoderNode(
                        lAudioLoopBack,
                        lAudioSourceIndexStream,
                        lAudioSourceIndexMediaType,
                        l_AudioEncoder,
                        m_AudioEncoderMode,
                        l_AudioCompressedMediaTypeSelectedIndex,
                        lOutputNodes[1]);
            }


            List<object> lAudioTopologyInputMixerNodes;

            lMixerNodeFactory.createMixerNodes(
                lAudioEncoderNode,
                2,
                out lAudioTopologyInputMixerNodes);

            if (lAudioTopologyInputMixerNodes.Count == 0)
                return;

            mAudioTopologyInputMixerNode = lAudioTopologyInputMixerNodes[1];




            List<object> lSourceNodes = new List<object>();

            if (true)
            {
                object lSourceNode = getSourceNode(
                        lScreenCaptureSymbolicLink,
                        lVideoSourceIndexStream,
                        lVideoSourceIndexMediaType,
                        lVideoTopologyInputMixerNodes[0]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }



            if (true)
            {
                object lSourceNode = getSourceNode(
                        lAudioLoopBack,
                        lAudioSourceIndexStream,
                        lAudioSourceIndexMediaType,
                        lAudioTopologyInputMixerNodes[0]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }


            mISession = mISessionControl.createSession(lSourceNodes.ToArray());

            if (mISession != null)
            {
                mStartStopTxtBlk.Text = "Stop";

                mISession.startSession(0, Guid.Empty);

                mSourcesPanel.IsEnabled = true;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mISession != null)
            {
                mStartStopTxtBlk.Text = "Start";

                mISession.stopSession();

                mISession.closeSession();

                mISession = null;

                mSourcesPanel.IsEnabled = false;

                if (mAddCameraSession != null)
                {
                    mAddCameraSession.stopSession();

                    mAddCameraSession.closeSession();

                    mAddCameraSession = null;

                    mAddCameraTxtBlk.Text = "Add Camera";

                    mVideoTopologyInputMixerNodes.Add(mCameraVideoTopologyInputMixerNode);

                    mCameraVideoTopologyInputMixerNode = null;
                }

                if (mAddImageSession != null)
                {
                    mAddImageSession.stopSession();

                    mAddImageSession.closeSession();

                    mAddImageSession = null;

                    mAddImageTxtBlk.Text = "Add Image";

                    mVideoTopologyInputMixerNodes.Add(mImageVideoTopologyInputMixerNode);

                    mImageVideoTopologyInputMixerNode = null;
                }

                if (mAddMicSession != null)
                {
                    mAddMicSession.stopSession();

                    mAddMicSession.closeSession();

                    mAddMicSession = null;
                }
                                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(mAudioTopologyInputMixerNode);

                mAudioTopologyInputMixerNode = null;

                foreach (var item in mVideoTopologyInputMixerNodes)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
                }

                mVideoTopologyInputMixerNodes.Clear();

                return;
            }

            init();
        }

        private void MAddCameraBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mAddCameraSession != null)
            {
                mAddCameraSession.stopSession();

                mAddCameraSession.closeSession();

                mAddCameraSession = null;

                mAddCameraTxtBlk.Text = "Add Camera";
                
                mVideoTopologyInputMixerNodes.Add(mCameraVideoTopologyInputMixerNode);

                var lVideoMixerControlRelease = mCaptureManager.createVideoMixerControl();

                if (lVideoMixerControlRelease != null)
                    lVideoMixerControlRelease.flush(mCameraVideoTopologyInputMixerNode);

                mCameraVideoTopologyInputMixerNode = null;

                m_ImagePanel.IsEnabled = true;

                return;
            }



            var l_VideoSourceXmlNode = m_VideoSourceComboBox.SelectedItem as XmlNode;
            var l_VideoStreamXmlNode = m_VideoStreamComboBox.SelectedItem as XmlNode;
            var l_VideoSourceMediaTypeXmlNode = m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode;

            var lVideoTopologyInputMixerNode = mVideoTopologyInputMixerNodes[0];

            mCameraVideoTopologyInputMixerNode = lVideoTopologyInputMixerNode;

            mVideoTopologyInputMixerNodes.RemoveAt(0);

            object lVideoSourceSourceNode = getSourceNode(
                                l_VideoSourceXmlNode,
                                l_VideoStreamXmlNode,
                                l_VideoSourceMediaTypeXmlNode,
                                lVideoTopologyInputMixerNode);


            object[] lSourceNodes = { lVideoSourceSourceNode };

            mAddCameraSession = mISessionControl.createSession(lSourceNodes);

            if (mAddCameraSession != null)
                mAddCameraTxtBlk.Text = "Remove Camera";

            mAddCameraSession.startSession(0, Guid.Empty);


            var lVideoMixerControl = mCaptureManager.createVideoMixerControl();

            if (lVideoMixerControl != null)
                lVideoMixerControl.setPosition(lVideoTopologyInputMixerNode, 0.0f, 0.5f, 0.0f, 0.5f);

            if (lVideoMixerControl != null)
                lVideoMixerControl.setOpacity(lVideoTopologyInputMixerNode, 0.5f);

            m_ImagePanel.IsEnabled = false;

            //if (lVideoMixerControl != null)
            //    lVideoMixerControl.setSrcPosition(lVideoTopologyInputMixerNode, 0.0f, 0.5f, 0.0f, 0.5f);

        }

        private void MAddImageBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mAddImageSession != null)
            {
                mAddImageSession.stopSession();

                mAddImageSession.closeSession();

                mAddImageSession = null;

                mAddImageTxtBlk.Text = "Add Image";

                mVideoTopologyInputMixerNodes.Add(mImageVideoTopologyInputMixerNode);

                var lVideoMixerControlRelease = mCaptureManager.createVideoMixerControl();

                if (lVideoMixerControlRelease != null)
                    lVideoMixerControlRelease.flush(mImageVideoTopologyInputMixerNode);

                mImageVideoTopologyInputMixerNode = null;

                m_CameraPanel.IsEnabled = true;

                return;
            }

            var lOpenFileDialog = new Microsoft.Win32.OpenFileDialog();

            lOpenFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            lOpenFileDialog.Filter = "Image files (*.png, *.gif)|*.png;*.gif";

            bool l_result = (bool)lOpenFileDialog.ShowDialog();

            if (l_result &&
                File.Exists(lOpenFileDialog.FileName))
            {
                var lICaptureProcessor = ImageCaptureProcessor.createCaptureProcessor(lOpenFileDialog.FileName);

                if (lICaptureProcessor == null)
                    return;

                object lImageSourceSource = null;

                mSourceControl.createSourceFromCaptureProcessor(
                    lICaptureProcessor,
                    out lImageSourceSource);

                var lVideoTopologyInputMixerNode = mVideoTopologyInputMixerNodes[0];

                mImageVideoTopologyInputMixerNode = lVideoTopologyInputMixerNode;

                mVideoTopologyInputMixerNodes.RemoveAt(0);

                object lImageSourceSourceNode = null;

                mSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(
                    lImageSourceSource,
                    0,
                    0,
                    lVideoTopologyInputMixerNode,
                    out lImageSourceSourceNode);


                object[] lSourceNodes = { lImageSourceSourceNode };

                mAddImageSession = mISessionControl.createSession(lSourceNodes);

                if (mAddImageSession != null)
                    mAddImageTxtBlk.Text = "Remove Camera";

                mAddImageSession.startSession(0, Guid.Empty);


                var lVideoMixerControl = mCaptureManager.createVideoMixerControl();

                if (lVideoMixerControl != null)
                    lVideoMixerControl.setPosition(lVideoTopologyInputMixerNode, 0.5f, 1.0f, 0.0f, 0.5f);

                //if (lVideoMixerControl != null)
                //    lVideoMixerControl.setSrcPosition(lVideoTopologyInputMixerNode, 0.0f, 0.5f, 0.0f, 0.5f);

                if (lVideoMixerControl != null)
                    lVideoMixerControl.setOpacity(lVideoTopologyInputMixerNode, 0.5f);

                m_CameraPanel.IsEnabled = false;


            }
        }

        private void MAddMicBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mAddMicSession != null)
            {
                mAddMicSession.stopSession();

                mAddMicSession.closeSession();

                mAddMicTxtBlk.Text = "Add Mic";

                mAddMicSession = null;

                return;
            }



            var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;
            var l_AudioStreamXmlNode = m_AudioStreamComboBox.SelectedItem as XmlNode;
            var l_AudioSourceMediaTypeXmlNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;


            object lAudioSourceSourceNode = getSourceNode(
                                l_AudioSourceXmlNode,
                                l_AudioStreamXmlNode,
                                l_AudioSourceMediaTypeXmlNode,
                                mAudioTopologyInputMixerNode);


            object[] lSourceNodes = { lAudioSourceSourceNode };

            mAddMicSession = mISessionControl.createSession(lSourceNodes);

            if (mAddMicSession != null)
                mAddMicTxtBlk.Text = "Remove Mic";

            mAddMicSession.startSession(0, Guid.Empty);


            var lAudioMixerControl = mCaptureManager.createAudioMixerControl();

            if (lAudioMixerControl != null)
                lAudioMixerControl.setRelativeVolume(mAudioTopologyInputMixerNode, (float)m_AudioVolume.Value);
        }

        private object getSourceNode(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            object MixerNode)
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
                    MixerNode,
                    out lSourceNode))
                    break;

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }


        private object getSourceNode(
            string aSymbolicLink,
            uint aStreamIndex,
            uint aMediaTypeIndex,
            object aOutputNode)
        {
            object lresult = null;

            do
            {                
                object lSourceMediaType = null;

                if (!mSourceControl.getSourceOutputMediaType(
                    aSymbolicLink,
                    aStreamIndex,
                    aMediaTypeIndex,
                    out lSourceMediaType))
                    break;

                if (lSourceMediaType == null)
                    break;
                                                
                object lSourceNode;

                string lextendSymbolicLink = aSymbolicLink + " --options=" +
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
                    aStreamIndex,
                    aMediaTypeIndex,
                    aOutputNode,
                    out lSourceNode))
                    break;

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }
               
        private object getEncoderNode(
            string aSymbolicLink,
            uint aStreamIndex,
            uint aMediaTypeIndex,
            Guid aCLSIDEncoder,
            Guid aCLSIDEncoderMode,
            int aCompressedMediaTypeIndex,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;

                object lSourceMediaType = null;

                if (!mSourceControl.getSourceOutputMediaType(
                    aSymbolicLink,
                    aStreamIndex,
                    aMediaTypeIndex,
                    out lSourceMediaType))
                    break;

                if (lSourceMediaType == null)
                    break;

                IEncoderNodeFactory lEncoderNodeFactory;

                if (!mEncoderControl.createEncoderNodeFactory(
                    aCLSIDEncoder,
                    out lEncoderNodeFactory))
                    break;

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode;

                if (!lEncoderNodeFactory.createEncoderNode(
                    lSourceMediaType,
                    aCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex,
                    aOutputNode,
                    out lEncoderNode))
                    break;
                
                lresult = lEncoderNode;

            } while (false);

            return lresult;
        }

        private object getCompressedMediaType(
            string aSymbolicLink,
            uint aStreamIndex,
            uint aMediaTypeIndex,
            Guid aCLSIDEncoder,
            Guid aCLSIDEncoderMode,
            int aCompressedMediaTypeIndex)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;
                
                object lSourceMediaType = null;

                if (!mSourceControl.getSourceOutputMediaType(
                    aSymbolicLink,
                    aStreamIndex,
                    aMediaTypeIndex,
                    out lSourceMediaType))
                    break;

                if (lSourceMediaType == null)
                    break;

                IEncoderNodeFactory lEncoderNodeFactory;

                if (!mEncoderControl.createEncoderNodeFactory(
                    aCLSIDEncoder,
                    out lEncoderNodeFactory))
                    break;

                if (lEncoderNodeFactory == null)
                    break;

                object lCompressedMediaType;

                if (!lEncoderNodeFactory.createCompressedMediaType(
                    lSourceMediaType,
                    aCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex,
                    out lCompressedMediaType))
                    break;

                lresult = lCompressedMediaType;

            } while (false);

            return lresult;
        }
        
        private List<object> getOutputNodes(List<object> aCompressedMediaTypeList, string aFilename)
        {
            List<object> lresult = new List<object>();

            do
            {
                if (aCompressedMediaTypeList == null)
                    break;

                if (aCompressedMediaTypeList.Count == 0)
                    break;

                if (mFileSinkFactory == null)
                    break;
                
                mFileSinkFactory.createOutputNodes(
                    aCompressedMediaTypeList,
                    aFilename,
                    out lresult);

            } while (false);

            return lresult;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mAudioTopologyInputMixerNode == null)
                return;

            var lAudioMixerControl = mCaptureManager.createAudioMixerControl();

            if (lAudioMixerControl != null)
                lAudioMixerControl.setRelativeVolume(mAudioTopologyInputMixerNode, (float)e.NewValue);
        }
    }
}
