using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

namespace WPFHotRecordingAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CaptureManager mCaptureManager = null;

        ISessionControlAsync mISessionControl = null;

        ISessionAsync mISession = null;

        ISinkControlAsync mSinkControl = null;

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;

        IFileSinkFactoryAsync mFileSinkFactory = null;

        IStreamControlAsync mStreamControl = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;

        IEVRMultiSinkFactoryAsync mEVRMultiSinkFactory = null;

        ISwitcherControlAsync mSwitcherControl = null;
        
        object mVideoSwitcherNode = null;

        object mAudioSwitcherNode = null;
        
        enum State
        {
            Stopped, Started, Paused
        }

        State mState = State.Stopped;

        State mRecordingState = State.Stopped;

        uint lFileCount = 0;

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

            lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();


            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;
            
        }
        
        async Task detachRecorder()
        {
            await mSwitcherControl.detachSwitchersAsync(mISession);
        }
        
        private async void m_VideoEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {
                if (mEncoderControl == null)
                    break;

                var lselectedNode = m_VideoEncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

                if (lCLSIDEncoderAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                    break;



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
                
                uint lStreamIndex = 0;

                lSourceNode = m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode;

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




                if (mSourceControl == null)
                    return;

                object lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                string lMediaTypeCollection = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                    lOutputMediaType,
                    lCLSIDEncoder);



                XmlDataProvider lXmlEncoderModeDataProvider = (XmlDataProvider)this.Resources["XmlEncoderModeProvider"];

                if (lXmlEncoderModeDataProvider == null)
                    return;

                XmlDocument lEncoderModedoc = new XmlDocument();

                lEncoderModedoc.LoadXml(lMediaTypeCollection);

                lXmlEncoderModeDataProvider.Document = lEncoderModedoc;


            } while (false);
        }

        private string createFileName()
        {
            return string.Format("Video_{0}", ++lFileCount);
        }
        
        private async void m_StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mState == State.Paused && mISession != null)
            {
                await mSwitcherControl.resumeSwitchersAsync(mISession);

                mState = State.Started;

                m_PauseBtn.IsEnabled = true;

                m_StopBtn.IsEnabled = true;

                m_StartStopBtn.IsEnabled = false;

                return;
            }

            mVideoSwitcherNode = null;

            mAudioSwitcherNode = null;

            object RenderNode = null;


            List<object> lRenderOutputNodesList = await mEVRMultiSinkFactory.createOutputNodesAsync(
                    IntPtr.Zero,
                    m_EVRDisplay.Surface.texture,
                    1);

            if (lRenderOutputNodesList.Count == 1)
            {
                RenderNode = lRenderOutputNodesList[0];
            }

            List<object> lSourceNodes = new List<object>();

            if ((bool)m_VideoStreamChkBtn.IsChecked)
            {

                mVideoSwitcherNode = await createSwitcher(null);

                if (mVideoSwitcherNode == null)
                    return;

                object lSourceNode = await getSourceNode(
                    m_VideoSourceComboBox.SelectedItem as XmlNode,
                    m_VideoStreamComboBox.SelectedItem as XmlNode,
                    m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                    m_VideoEncodersComboBox.SelectedItem as XmlNode,
                    m_VideoEncodingModeComboBox.SelectedItem as XmlNode,
                    m_VideoCompressedMediaTypesComboBox.SelectedIndex,
                    RenderNode,
                    mVideoSwitcherNode);

                if (lSourceNodes != null && lSourceNode != null)
                    lSourceNodes.Add(lSourceNode);
                else
                    mVideoSwitcherNode = null;
            }

            if ((bool)m_AudioStreamChkBtn.IsChecked)
            {

                mAudioSwitcherNode = await createSwitcher(null);

                if (mAudioSwitcherNode == null)
                    return;

                object lSourceNode = await getSourceNode(
                    m_AudioSourceComboBox.SelectedItem as XmlNode,
                    m_AudioStreamComboBox.SelectedItem as XmlNode,
                    m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode,
                    m_AudioEncodersComboBox.SelectedItem as XmlNode,
                    m_AudioEncodingModeComboBox.SelectedItem as XmlNode,
                    m_AudioCompressedMediaTypesComboBox.SelectedIndex,
                    null,
                    mAudioSwitcherNode);

                if (lSourceNodes != null && lSourceNode != null)
                    lSourceNodes.Add(lSourceNode);
                else
                    mAudioSwitcherNode = null;
            }

            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession == null)
                return;
            
            if (await mISession.startSessionAsync(0, Guid.Empty))
            {
                mState = State.Started;

                mRecordingState = State.Paused;

                mStateLabel.Visibility = System.Windows.Visibility.Collapsed;

                m_PauseBtn.IsEnabled = true;

                m_StopBtn.IsEnabled = true;

                m_StartStopBtn.IsEnabled = false;

                mSourceGrid.IsEnabled = false;
            }
        }

        private async void m_PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            await detachRecorder();

            await attachRecorder();
        }

        async Task attachRecorder()
        {
            List<object> lEncoderNodes = await createEncoderNodes();

            if (lEncoderNodes == null)
                return;

            int lencoderCount = 0;

            if (mVideoSwitcherNode != null)
                await mSwitcherControl.atttachSwitcherAsync(mVideoSwitcherNode, lEncoderNodes[lencoderCount++]);

            if (mAudioSwitcherNode != null)
                await mSwitcherControl.atttachSwitcherAsync(mAudioSwitcherNode, lEncoderNodes[lencoderCount++]);

            await mSwitcherControl.resumeSwitchersAsync(mISession);
        }

        private async Task<List<object>> createEncoderNodes()
        {

            List<object> lCompressedMediaTypeList = new List<object>();


            object lVideoCompressedMediaType = await getCompressedMediaType(
                m_VideoSourceComboBox.SelectedItem as XmlNode,
                m_VideoStreamComboBox.SelectedItem as XmlNode,
                m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                m_VideoEncodersComboBox.SelectedItem as XmlNode,
                m_VideoEncodingModeComboBox.SelectedItem as XmlNode,
                m_VideoCompressedMediaTypesComboBox.SelectedIndex);

            if (lVideoCompressedMediaType != null)
                lCompressedMediaTypeList.Add(lVideoCompressedMediaType);


            object lAudioCompressedMediaType = await getCompressedMediaType(
                m_AudioSourceComboBox.SelectedItem as XmlNode,
                m_AudioStreamComboBox.SelectedItem as XmlNode,
                m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode,
                m_AudioEncodersComboBox.SelectedItem as XmlNode,
                m_AudioEncodingModeComboBox.SelectedItem as XmlNode,
                m_AudioCompressedMediaTypesComboBox.SelectedIndex);

            if (lAudioCompressedMediaType != null)
                lCompressedMediaTypeList.Add(lAudioCompressedMediaType);

            List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList, createFileName());

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return null;

            int lOutputIndex = 0;

            List<object> lEncoderNodes = new List<object>();

            object lEncoderNode = null;

            if(lVideoCompressedMediaType != null)
                lEncoderNode = await getEncoder(
                    m_VideoSourceComboBox.SelectedItem as XmlNode,
                    m_VideoStreamComboBox.SelectedItem as XmlNode,
                    m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                    m_VideoEncodersComboBox.SelectedItem as XmlNode,
                    m_VideoEncodingModeComboBox.SelectedItem as XmlNode,
                    m_VideoCompressedMediaTypesComboBox.SelectedIndex,
                    lOutputNodes[lOutputIndex++]);

            if (lEncoderNode != null)
                lEncoderNodes.Add(lEncoderNode);

            lEncoderNode = null;

            if (lAudioCompressedMediaType != null)
                lEncoderNode = await getEncoder(
                    m_AudioSourceComboBox.SelectedItem as XmlNode,
                    m_AudioStreamComboBox.SelectedItem as XmlNode,
                    m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode,
                    m_AudioEncodersComboBox.SelectedItem as XmlNode,
                    m_AudioEncodingModeComboBox.SelectedItem as XmlNode,
                    m_AudioCompressedMediaTypesComboBox.SelectedIndex,
                    lOutputNodes[lOutputIndex++]);

            if (lEncoderNode != null)
                lEncoderNodes.Add(lEncoderNode);

            return lEncoderNodes;
        }

        private async void m_StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mState != State.Stopped)
            {
                m_StopBtn.IsEnabled = false;

                m_PauseBtn.IsEnabled = false;

                mSourceGrid.IsEnabled = true;
                
                mState = State.Stopped;

                if (mISession == null)
                    return;

                mStateLabel.Visibility = System.Windows.Visibility.Collapsed;

                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

                mISession = null;
                
                m_StartStopBtn.IsEnabled = true;

                m_PauseBtn.Content = "Start recording";
            }
        }

        private async Task<object> getCompressedMediaType(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;


                if (aSourceNode == null)
                    break;
                               
                if (aMediaTypeNode == null)
                    break;


                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
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

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(
                    lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                lresult = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);
                
            } while (false);

            return lresult;
        }

        private async Task<List<object>> getOutputNodes(List<object> aCompressedMediaTypeList, string aFileName)
        {
            List<object> lresult = new List<object>();

            do
            {
                if (aCompressedMediaTypeList == null)
                    break;

                if (aCompressedMediaTypeList.Count == 0)
                    break;
                
                var lselectedNode = m_FileFormatComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lSelectedAttr = lselectedNode.Attributes["Value"];

                if (lSelectedAttr == null)
                    break;
                aFileName += "." + lSelectedAttr.Value.ToLower();
                
                lSelectedAttr = lselectedNode.Attributes["GUID"];

                if (lSelectedAttr == null)
                    break;

                mFileSinkFactory = await mSinkControl.createFileSinkFactoryAsync(
                    Guid.Parse(lSelectedAttr.Value));
                
                if (string.IsNullOrEmpty(aFileName))
                    break;

                foreach (var item in mFileList.Items)
	            {
		            var lFileItemControl = item as FileItemControl;

                    if(lFileItemControl != null)
                        lFileItemControl.stopAnimation();
	            }

                mFileList.Items.Add(new FileItemControl(aFileName));
                
                String limageSourceDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                lresult = await mFileSinkFactory.createOutputNodesAsync(
                    aCompressedMediaTypeList,
                    limageSourceDir + @"\" + aFileName);

            } while (false);

            return lresult;
        }

        private async Task<object> getEncoder(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;


                if (aSourceNode == null)
                    break;

                if (aMediaTypeNode == null)
                    break;


                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
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

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(
                    lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode = await lEncoderNodeFactory.createEncoderNodeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex,
                    aOutputNode);

                lresult = lEncoderNode;

            } while (false);

            return lresult;
        }

        private async Task<object> createSwitcher(
            object aEncoderNode)
        {
            object lresult = null;

            do
            {

                var lSwitcherNodeFactory = await mStreamControl.createSwitcherNodeFactoryAsync();

                if (lSwitcherNodeFactory == null)
                    break;

                lresult = await lSwitcherNodeFactory.createSwitcherNodeAsync(aEncoderNode);

            } while (false);

            return lresult;
        }

        private async Task<object> getSourceNode(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex,
            object PreviewRenderNode,
            object aSwitcherNode)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;


                if (aSourceNode == null)
                    break;
                               
                if (aMediaTypeNode == null)
                    break;


                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
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

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(
                    lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object SpreaderNode = aSwitcherNode;

                if (PreviewRenderNode != null)
                {

                    List<object> lOutputNodeList = new List<object>();

                    lOutputNodeList.Add(PreviewRenderNode);

                    lOutputNodeList.Add(aSwitcherNode);

                    SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                        lOutputNodeList);

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

                lresult = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lextendSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    SpreaderNode);
                
            } while (false);

            return lresult;
        }

        private async void m_AudioEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {
                if (mEncoderControl == null)
                    break;

                var lselectedNode = m_AudioEncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

                if (lCLSIDEncoderAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                    break;



                var lSourceNode = m_AudioSourceComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    return;

                var lNode = lSourceNode.SelectSingleNode(
            "Source.Attributes/Attribute" +
            "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
            "/SingleValue/@Value");

                if (lNode == null)
                    return;

                string lSymbolicLink = lNode.Value;

                lSourceNode = m_AudioStreamComboBox.SelectedItem as XmlNode;

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

                lSourceNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;

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
                
                if (mSourceControl == null)
                    return;

                object lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                string lMediaTypeCollection = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                    lOutputMediaType,
                    lCLSIDEncoder);



                XmlDataProvider lXmlEncoderModeDataProvider = (XmlDataProvider)this.Resources["XmlAudioEncoderModeProvider"];

                if (lXmlEncoderModeDataProvider == null)
                    return;

                XmlDocument lEncoderModedoc = new XmlDocument();

                lEncoderModedoc.LoadXml(lMediaTypeCollection);

                lXmlEncoderModeDataProvider.Document = lEncoderModedoc;


            } while (false);
        }

    }
}
