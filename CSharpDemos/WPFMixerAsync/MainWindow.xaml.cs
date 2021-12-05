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

namespace WPFMixerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static CaptureManager mCaptureManager = null;

        ISessionControlAsync mISessionControl = null;

        ISessionAsync mISession = null;

        ISessionAsync mAddCameraSession = null;

        ISessionAsync mAddImageSession = null;

        ISessionAsync mAddMicSession = null;

        ISinkControlAsync mSinkControl = null;

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;

        IStreamControlAsync mStreamControl = null;

        IFileSinkFactoryAsync mFileSinkFactory = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;



        
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
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private async Task init()
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

            mFileSinkFactory = await mSinkControl.createFileSinkFactoryAsync(Guid.Parse(lSelectedAttr.Value));



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


            string l_EncodersXMLstring = await mEncoderControl.getCollectionOfEncodersAsync();


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
                object lCompressedMediaType = await getCompressedMediaType(
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
                object lCompressedMediaType = await getCompressedMediaType(
                        lAudioLoopBack,
                        lAudioSourceIndexStream,
                        lAudioSourceIndexMediaType,
                        l_AudioEncoder,
                        m_AudioEncoderMode,
                        l_AudioCompressedMediaTypeSelectedIndex);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }


            List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList, lFilename);

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return;
                                          
            var lSinkFactory = await mSinkControl.createEVRSinkFactoryAsync(
            Guid.Empty);

            object lEVROutputNode = await lSinkFactory.createOutputNodeAsync(
                    mVideoPanel.Handle);

            if (lEVROutputNode == null)
                return;
            

            object SpreaderNode = lEVROutputNode;

            if (true)
            {
                var lEncoderNode = await getEncoderNode(
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

                SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                    lOutputNodeList);

                //SpreaderNode = lEncoderNode;
            }






            var lMixerNodeFactory = await mStreamControl.createMixerNodeFactoryAsync();

            List<object> lVideoTopologyInputMixerNodes = await lMixerNodeFactory.createMixerNodesAsync(
                SpreaderNode,
                2);

            if (lVideoTopologyInputMixerNodes.Count == 0)
                return;

            for (int i = 1; i < lVideoTopologyInputMixerNodes.Count; i++)
            {
                mVideoTopologyInputMixerNodes.Add(lVideoTopologyInputMixerNodes[i]);
            }






            object lAudioEncoderNode = null;

            if (true)
            {
                lAudioEncoderNode = await getEncoderNode(
                        lAudioLoopBack,
                        lAudioSourceIndexStream,
                        lAudioSourceIndexMediaType,
                        l_AudioEncoder,
                        m_AudioEncoderMode,
                        l_AudioCompressedMediaTypeSelectedIndex,
                        lOutputNodes[1]);
            }


            List<object> lAudioTopologyInputMixerNodes = await lMixerNodeFactory.createMixerNodesAsync(
                lAudioEncoderNode,
                2);

            if (lAudioTopologyInputMixerNodes.Count == 0)
                return;

            mAudioTopologyInputMixerNode = lAudioTopologyInputMixerNodes[1];




            List<object> lSourceNodes = new List<object>();

            if (true)
            {
                object lSourceNode = await getSourceNode(
                        lScreenCaptureSymbolicLink,
                        lVideoSourceIndexStream,
                        lVideoSourceIndexMediaType,
                        lVideoTopologyInputMixerNodes[0]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }



            if (true)
            {
                object lSourceNode = await getSourceNode(
                        lAudioLoopBack,
                        lAudioSourceIndexStream,
                        lAudioSourceIndexMediaType,
                        lAudioTopologyInputMixerNodes[0]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }


            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession != null)
            {
                mStartStopTxtBlk.Text = "Stop";

                await mISession.startSessionAsync(0, Guid.Empty);

                mSourcesPanel.IsEnabled = true;
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            mStartStopBtn.IsEnabled = false;

            do
            {
                if (mISession != null)
                {
                    mStartStopTxtBlk.Text = "Start";

                    await mISession.stopSessionAsync();

                    await mISession.closeSessionAsync();

                    mISession = null;

                    mSourcesPanel.IsEnabled = false;

                    if (mAddCameraSession != null)
                    {
                        await mAddCameraSession.stopSessionAsync();

                        await mAddCameraSession.closeSessionAsync();

                        mAddCameraSession = null;

                        mAddCameraTxtBlk.Text = "Add Camera";

                        mVideoTopologyInputMixerNodes.Add(mCameraVideoTopologyInputMixerNode);

                        mCameraVideoTopologyInputMixerNode = null;
                    }

                    if (mAddImageSession != null)
                    {
                        await mAddImageSession.stopSessionAsync();

                        await mAddImageSession.closeSessionAsync();

                        mAddImageSession = null;

                        mAddImageTxtBlk.Text = "Add Image";

                        mVideoTopologyInputMixerNodes.Add(mImageVideoTopologyInputMixerNode);

                        mImageVideoTopologyInputMixerNode = null;
                    }

                    if (mAddMicSession != null)
                    {
                        await mAddMicSession.stopSessionAsync();

                        await mAddMicSession.closeSessionAsync();

                        mAddMicSession = null;
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mAudioTopologyInputMixerNode);

                    mAudioTopologyInputMixerNode = null;

                    foreach (var item in mVideoTopologyInputMixerNodes)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
                    }

                    mVideoTopologyInputMixerNodes.Clear();

                    break;
                }

                await init();

            } while (false);

            mStartStopBtn.IsEnabled = true;
        }

        private async void MAddCameraBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mAddCameraSession != null)
            {
                await mAddCameraSession.stopSessionAsync();

                await mAddCameraSession.closeSessionAsync();

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

            object lVideoSourceSourceNode = await getSourceNode(
                                l_VideoSourceXmlNode,
                                l_VideoStreamXmlNode,
                                l_VideoSourceMediaTypeXmlNode,
                                lVideoTopologyInputMixerNode);


            object[] lSourceNodes = { lVideoSourceSourceNode };

            mAddCameraSession = await mISessionControl.createSessionAsync(lSourceNodes);

            if (mAddCameraSession != null)
                mAddCameraTxtBlk.Text = "Remove Camera";

            await mAddCameraSession.startSessionAsync(0, Guid.Empty);


            var lVideoMixerControl = mCaptureManager.createVideoMixerControl();

            if (lVideoMixerControl != null)
                lVideoMixerControl.setPosition(lVideoTopologyInputMixerNode, 0.0f, 0.5f, 0.0f, 0.5f);

            if (lVideoMixerControl != null)
                lVideoMixerControl.setOpacity(lVideoTopologyInputMixerNode, 0.5f);

            m_ImagePanel.IsEnabled = false;

            //if (lVideoMixerControl != null)
            //    lVideoMixerControl.setSrcPosition(lVideoTopologyInputMixerNode, 0.0f, 0.5f, 0.0f, 0.5f);

        }

        private async void MAddImageBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mAddImageSession != null)
            {
                await mAddImageSession.stopSessionAsync();

                await mAddImageSession.closeSessionAsync();

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

                object lImageSourceSource = await mSourceControl.createSourceFromCaptureProcessorAsync(
                    lICaptureProcessor);

                var lVideoTopologyInputMixerNode = mVideoTopologyInputMixerNodes[0];

                mImageVideoTopologyInputMixerNode = lVideoTopologyInputMixerNode;

                mVideoTopologyInputMixerNodes.RemoveAt(0);

                object lImageSourceSourceNode = await mSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnectionAsync(
                    lImageSourceSource,
                    0,
                    0,
                    lVideoTopologyInputMixerNode);


                object[] lSourceNodes = { lImageSourceSourceNode };

                mAddImageSession = await mISessionControl.createSessionAsync(lSourceNodes);

                if (mAddImageSession != null)
                    mAddImageTxtBlk.Text = "Remove Camera";

                await mAddImageSession.startSessionAsync(0, Guid.Empty);


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

        private async void MAddMicBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mAddMicSession != null)
            {
                await mAddMicSession.stopSessionAsync();

                await mAddMicSession.closeSessionAsync();

                mAddMicTxtBlk.Text = "Add Mic";

                mAddMicSession = null;

                return;
            }



            var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;
            var l_AudioStreamXmlNode = m_AudioStreamComboBox.SelectedItem as XmlNode;
            var l_AudioSourceMediaTypeXmlNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;


            object lAudioSourceSourceNode = await getSourceNode(
                                l_AudioSourceXmlNode,
                                l_AudioStreamXmlNode,
                                l_AudioSourceMediaTypeXmlNode,
                                mAudioTopologyInputMixerNode);


            object[] lSourceNodes = { lAudioSourceSourceNode };

            mAddMicSession = await mISessionControl.createSessionAsync(lSourceNodes);

            if (mAddMicSession != null)
                mAddMicTxtBlk.Text = "Remove Mic";

            await mAddMicSession.startSessionAsync(0, Guid.Empty);


            var lAudioMixerControl = await mCaptureManager.createAudioMixerControlAsync();

            if (lAudioMixerControl != null)
                await lAudioMixerControl.setRelativeVolumeAsync(mAudioTopologyInputMixerNode, (float)m_AudioVolume.Value);
        }

        private async Task<object> getSourceNode(
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
                    MixerNode);

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }


        private async Task<object> getSourceNode(
            string aSymbolicLink,
            uint aStreamIndex,
            uint aMediaTypeIndex,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    aSymbolicLink,
                    aStreamIndex,
                    aMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                string lextendSymbolicLink = aSymbolicLink + " --options=" +
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
                    aStreamIndex,
                    aMediaTypeIndex,
                    aOutputNode);

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }

        private async Task<object> getEncoderNode(
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

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    aSymbolicLink,
                    aStreamIndex,
                    aMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(
                    aCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode = await lEncoderNodeFactory.createEncoderNodeAsync(
                    lSourceMediaType,
                    aCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex,
                    aOutputNode);
                
                lresult = lEncoderNode;

            } while (false);

            return lresult;
        }

        private async Task<object> getCompressedMediaType(
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

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    aSymbolicLink,
                    aStreamIndex,
                    aMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(
                    aCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lCompressedMediaType = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    aCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);

                lresult = lCompressedMediaType;

            } while (false);

            return lresult;
        }
        
        private async Task<List<object>> getOutputNodes(List<object> aCompressedMediaTypeList, string aFilename)
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

                lresult = await mFileSinkFactory.createOutputNodesAsync(
                    aCompressedMediaTypeList,
                    aFilename);

            } while (false);

            return lresult;
        }

        private async void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mAudioTopologyInputMixerNode == null)
                return;

            var lAudioMixerControl = await mCaptureManager.createAudioMixerControlAsync();

            if (lAudioMixerControl != null)
                await lAudioMixerControl.setRelativeVolumeAsync(mAudioTopologyInputMixerNode, (float)e.NewValue);
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

            mSourceControl = await mCaptureManager.createSourceControlAsync();

            mEncoderControl = await mCaptureManager.createEncoderControlAsync();

            mSinkControl = await mCaptureManager.createSinkControlAsync();

            mStreamControl = await mCaptureManager.createStreamControlAsync();

            mISessionControl = await mCaptureManager.createSessionControlAsync();


            mSpreaderNodeFactory = await mStreamControl.createSpreaderNodeFactoryAsync();

            if (mSpreaderNodeFactory == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;




            XmlDataProvider lXmlContainerTypeProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

            doc.LoadXml(lxmldoc);

            lXmlContainerTypeProvider.Document = doc;

        }
    }
}
