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

namespace WPFGIFtoVideo
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

        IEncoderControl mEncoderControl = null;

        IStreamControl mStreamControl = null;

        IFileSinkFactory mFileSinkFactory = null;

        ISpreaderNodeFactory mSpreaderNodeFactory = null;




        Guid m_VideoEncoderMode = new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55);

        Guid m_AudioEncoderMode = new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55);




        public CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl mIEVRStreamControl = null;
        

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
            var l_image_source = createImageSource();

            if (l_image_source == null)
                return;

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


            

            // Video Source
            uint lVideoSourceIndexStream = 0;

            uint lVideoSourceIndexMediaType = 0;

            int l_VideoCompressedMediaTypeSelectedIndex = 0;


            

            string l_EncodersXMLstring = "";

            mEncoderControl.getCollectionOfEncoders(out l_EncodersXMLstring);


            XmlDocument doc = new XmlDocument();

            doc.LoadXml(l_EncodersXMLstring);

            var lAttrNode = doc.SelectSingleNode("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory[1]/@CLSID");

            if (lAttrNode == null)
                return;

            Guid l_VideoEncoder = Guid.Empty;

            Guid.TryParse(lAttrNode.Value, out l_VideoEncoder);



            List<object> lCompressedMediaTypeList = new List<object>();

            if (true)
            {
                object lCompressedMediaType = getCompressedMediaType(
                        l_image_source,
                        lVideoSourceIndexStream,
                        lVideoSourceIndexMediaType,
                        l_VideoEncoder,
                        m_VideoEncoderMode,
                        l_VideoCompressedMediaTypeSelectedIndex);

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
                        l_image_source,
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
            }           
                                          
            List<object> lSourceNodes = new List<object>();

            object lSourceNode = null;

            mSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(
                l_image_source,
                lVideoSourceIndexStream,
                lVideoSourceIndexMediaType,
                SpreaderNode,
                out lSourceNode);

            if (lSourceNode == null)
                return;

            lSourceNodes.Add(lSourceNode);
                       
            mISession = mISessionControl.createSession(lSourceNodes.ToArray());

            if (mISession != null)
            {
                mStartStopTxtBlk.Text = "Stop";

                mISession.startSession(0, Guid.Empty);
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
                
                return;
            }

            init();
        }
        
        private object createImageSource()
        {
            object l_source = null;

            do
            {
                var lOpenFileDialog = new Microsoft.Win32.OpenFileDialog();

                lOpenFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                lOpenFileDialog.Filter = "Image files (*.png, *.gif)|*.png;*.gif";

                bool l_result = (bool)lOpenFileDialog.ShowDialog();

                if (l_result &&
                    File.Exists(lOpenFileDialog.FileName))
                {
                    var lICaptureProcessor = ImageCaptureProcessor.createCaptureProcessor(lOpenFileDialog.FileName);

                    if (lICaptureProcessor == null)
                        break;

                    object lImageSourceSource = null;

                    mSourceControl.createSourceFromCaptureProcessor(
                        lICaptureProcessor,
                        out lImageSourceSource);

                    l_source = lImageSourceSource;
                }

            } while (false);

            return l_source;
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
            object aMediaSource,
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

                if (!mSourceControl.getSourceOutputMediaTypeFromMediaSource(
                    aMediaSource,
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
            object aMediaSource,
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

                if (!mSourceControl.getSourceOutputMediaTypeFromMediaSource(
                    aMediaSource,
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
    }
}
