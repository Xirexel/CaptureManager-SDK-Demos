using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System.Windows.Threading;
using System.Xml;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WPFVideoAndAudioRecorderAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
	
	
    public class SubTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string l_result = value as String;

            if(l_result != null)
            {
                l_result = l_result.Replace("MFVideoFormat_", "");
                l_result = l_result.Replace("MFAudioFormat_", "");
            }

            return l_result;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
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
        
        IEVRSinkFactoryAsync mEVRSinkFactory = null;

        bool mIsStarted = false;

        string mFilename = null;
        
        public MainWindow()
        {
            mMediaTypesViewSource.Source = mMediaTypeCollection;

            mSubTypesViewSource.Source = mSubTypesCollection;

            InitializeComponent();
        }

        private object mCurrentSource = null;
        
        public object CurrentSource
        {
            get => this.mCurrentSource;
            set
            {
                this.mCurrentSource = value;
                createGroupSubType(value);
            }
        }

        private object mCurrentSubType = null;

        public object CurrentSubType
        {
            get => this.mCurrentSubType;
            set
            {
                this.mCurrentSubType = value;
                createGroupMediaTypes(value);
            }
        }

        ObservableCollection<string> mSubTypesCollection = new ObservableCollection<string>();

        CollectionViewSource mSubTypesViewSource = new CollectionViewSource();

        public ICollectionView SubTypes { get => mSubTypesViewSource.View; }

        ObservableCollection<XmlNode> mMediaTypeCollection = new ObservableCollection<XmlNode>();

        CollectionViewSource mMediaTypesViewSource = new CollectionViewSource();

        public ICollectionView MediaTypes { get => mMediaTypesViewSource.View; }

        private void createGroupSubType(object aCurrentSource)
        {
            var lCurrentSourceNode = aCurrentSource as XmlNode;

            if (lCurrentSourceNode == null)
                return;

            var lSubTypesNode = lCurrentSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType/MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

            if (lSubTypesNode == null)
                return;

            mSubTypesCollection.Clear();

            foreach (XmlNode item in lSubTypesNode)
            {
                if(!mSubTypesCollection.Contains(item.Value))
                    mSubTypesCollection.Add(item.Value);
            }
        }

        private void createGroupMediaTypes(object aCurrentSubType)
        {
            var lCurrentSubType = aCurrentSubType as string;

            var lCurrentSourceNode = mCurrentSource as XmlNode;

            if (lCurrentSourceNode == null)
                return;

            var lMediaTypesNode = lCurrentSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType[MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue[@Value='" + lCurrentSubType + "']]");

            if (lMediaTypesNode == null)
                return;

            mMediaTypeCollection.Clear();

            foreach (XmlNode item in lMediaTypesNode)
            {
                mMediaTypeCollection.Add(item);
            }
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
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

                mEVRSinkFactory = await mSinkControl.createEVRSinkFactoryAsync(Guid.Empty);

                if (mEVRSinkFactory == null)
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
            catch (Exception ex)
            {
            }
            finally
            {
            }
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

        private async void m_SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            do
            {
            var lselectedNode = m_FileFormatComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null)
                break;

            var lSelectedAttr = lselectedNode.Attributes["Value"];

            if (lSelectedAttr == null)
                break;

            String limageSourceDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            SaveFileDialog lsaveFileDialog = new SaveFileDialog();

            lsaveFileDialog.InitialDirectory = limageSourceDir;

            lsaveFileDialog.DefaultExt = "." + lSelectedAttr.Value.ToLower();

            lsaveFileDialog.AddExtension = true;

            lsaveFileDialog.CheckFileExists = false;

            lsaveFileDialog.Filter = "Media file (*." + lSelectedAttr.Value.ToLower() + ")|*." + lSelectedAttr.Value.ToLower();

            var lresult = lsaveFileDialog.ShowDialog();

            if (lresult != true)
                break;

            mFilename = lsaveFileDialog.FileName;
                
            lSelectedAttr = lselectedNode.Attributes["GUID"];

            if (lSelectedAttr == null)
                break;

            mFileSinkFactory = await  mSinkControl.createFileSinkFactoryAsync(
                Guid.Parse(lSelectedAttr.Value));

                m_StartStopBtn.IsEnabled = true;
                                
            } while (false);

        }

        private async void m_StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            m_StartStopBtn.IsEnabled = false;

            if (mIsStarted)
            {
                mIsStarted = false;

                if (mISession == null)
                    return;

                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

                mISession = null;

                m_BtnTxtBlk.Text = "Start";

                m_StartStopBtn.IsEnabled = true;

                return;
            }

            var l_videoStreamEnabled = (bool)m_VideoStreamChkBtn.IsChecked && m_VideoCompressedMediaTypesComboBox.SelectedIndex > -1;

            var l_previewEnabled = (bool)m_VideoStreamPreviewChkBtn.IsChecked;

            bool l_IsWithoutEncoder = m_SubTypeTxtBlk.Text == "MFVideoFormat_H264";

            if (l_IsWithoutEncoder)
                l_videoStreamEnabled = l_IsWithoutEncoder;

            var l_VideoSourceXmlNode = m_VideoSourceComboBox.SelectedItem as XmlNode;
            var l_VideoStreamXmlNode = m_VideoStreamComboBox.SelectedItem as XmlNode;
            var l_VideoSourceMediaTypeXmlNode = m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode;
            var l_VideoEncodersXmlNode = m_VideoEncodersComboBox.SelectedItem as XmlNode;
            var l_VideoEncodingModeXmlNode = m_VideoEncodingModeComboBox.SelectedItem as XmlNode;
            var l_VideoCompressedMediaTypeSelectedIndex = m_VideoCompressedMediaTypesComboBox.SelectedIndex;


            var l_audioStreamEnabled = (bool)m_AudioStreamChkBtn.IsChecked && m_AudioCompressedMediaTypesComboBox.SelectedIndex > -1;


            var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;
            var l_AudioStreamXmlNode = m_AudioStreamComboBox.SelectedItem as XmlNode;
            var l_AudioSourceMediaTypeXmlNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;
            var l_AudioEncodersXmlNode = m_AudioEncodersComboBox.SelectedItem as XmlNode;
            var l_AudioEncodingModeXmlNode = m_AudioEncodingModeComboBox.SelectedItem as XmlNode;
            var l_AudioCompressedMediaTypeSelectedIndexXmlNode = m_AudioCompressedMediaTypesComboBox.SelectedIndex;

            var lHandle = m_EVRDisplay.Handle;


            object RenderNode = null;

            if (l_previewEnabled)
            {
                if (mEVRSinkFactory != null)
                    RenderNode = await mEVRSinkFactory.createOutputNodeAsync(
                        lHandle);
                
            }


            List<object> lCompressedMediaTypeList = new List<object>();

            if (l_videoStreamEnabled)
            {
                object lCompressedMediaType = await getCompressedMediaType(
                        l_VideoSourceXmlNode,
                        l_VideoStreamXmlNode,
                        l_VideoSourceMediaTypeXmlNode,
                        l_VideoEncodersXmlNode,
                        l_VideoEncodingModeXmlNode,
                        l_VideoCompressedMediaTypeSelectedIndex,
                        l_IsWithoutEncoder);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }

            if (l_audioStreamEnabled)
            {
                object lCompressedMediaType = await getCompressedMediaType(
                         l_AudioSourceXmlNode,
                         l_AudioStreamXmlNode,
                         l_AudioSourceMediaTypeXmlNode,
                         l_AudioEncodersXmlNode,
                         l_AudioEncodingModeXmlNode,
                         l_AudioCompressedMediaTypeSelectedIndexXmlNode);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }

            List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList);

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return;


            int lOutputIndex = 0;

            List<object> lSourceNodes = new List<object>();

            if (l_videoStreamEnabled)
            {                               
                object lSourceNode = await getSourceNode(
                        l_VideoSourceXmlNode,
                        l_VideoStreamXmlNode,
                        l_VideoSourceMediaTypeXmlNode,
                        l_VideoEncodersXmlNode,
                        l_VideoEncodingModeXmlNode,
                        l_VideoCompressedMediaTypeSelectedIndex,
                        RenderNode,
                        lOutputNodes[lOutputIndex++],
                        l_IsWithoutEncoder);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }

            if (l_audioStreamEnabled)
            {
                object lSourceNode = await getSourceNode(
                 l_AudioSourceXmlNode,
                 l_AudioStreamXmlNode,
                 l_AudioSourceMediaTypeXmlNode,
                 l_AudioEncodersXmlNode,
                 l_AudioEncodingModeXmlNode,
                 l_AudioCompressedMediaTypeSelectedIndexXmlNode,
                null,
                    lOutputNodes[lOutputIndex++]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }

            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession == null)
                return;

            if (await mISession.startSessionAsync(0, Guid.Empty))
            {
                m_BtnTxtBlk.Text = "Stop";
            }

            mIsStarted = true;
            m_StartStopBtn.IsEnabled = true;
        }

        private async Task<object> getCompressedMediaType(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex,
            bool aIsWithoutEncoder = false)
        {
            object lresult = null;

            do
            {
                if (aSourceNode == null)
                    break;

                               
                if (aMediaTypeNode == null)
                    break;

                if (aIsWithoutEncoder)
                {
                    var lNode1 = aSourceNode.SelectSingleNode(
                "Source.Attributes/Attribute" +
                "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                "/SingleValue/@Value");

                    if (lNode1 == null)
                        break;

                    string lSymbolicLink1 = lNode1.Value;
                                        
                    uint lStreamIndex1 = 0;


                    if (aMediaTypeNode == null)
                        break;

                    lNode1 = aMediaTypeNode.SelectSingleNode("@Index");

                    if (lNode1 == null)
                        break;

                    uint lMediaTypeIndex1 = 0;

                    if (!uint.TryParse(lNode1.Value, out lMediaTypeIndex1))
                    {
                        break;
                    }

                    object lSourceMediaType1 = await mSourceControl.getSourceOutputMediaTypeAsync(
                        lSymbolicLink1,
                        lStreamIndex1,
                        lMediaTypeIndex1);

                    if (lSourceMediaType1 == null)
                        break;

                    lresult = lSourceMediaType1;

                    break;
                }

                if (aCompressedMediaTypeIndex < 0)
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

                object lCompressedMediaType = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);

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

                if (mFileSinkFactory == null)
                    break;

                if(string.IsNullOrEmpty(mFilename))
                    break;

                lresult = await mFileSinkFactory.createOutputNodesAsync(
                    aCompressedMediaTypeList,
                    mFilename);
                
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
            object aOutputNode,
            bool aIsWithoutEncoder = false)
        {
            object lresult = null;

            do
            {
                if (aSourceNode == null)
                    break;
                

                if (aMediaTypeNode == null)
                    break;


                if (aIsWithoutEncoder)
                {
                    var lNode1 = aSourceNode.SelectSingleNode(
                "Source.Attributes/Attribute" +
                "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                "/SingleValue/@Value");

                    if (lNode1 == null)
                        break;

                    string lSymbolicLink1 = lNode1.Value;
                    
                    uint lStreamIndex1 = 0;
                    
                    if (aMediaTypeNode == null)
                        break;

                    lNode1 = aMediaTypeNode.SelectSingleNode("@Index");

                    if (lNode1 == null)
                        break;

                    uint lMediaTypeIndex1 = 0;

                    if (!uint.TryParse(lNode1.Value, out lMediaTypeIndex1))
                    {
                        break;
                    }
                                       
                    object SpreaderNode1 = aOutputNode;

                    if (PreviewRenderNode != null)
                    {
                        List<object> lOutputNodeList = new List<object>();
                        
                        lOutputNodeList.Add(PreviewRenderNode);

                        lOutputNodeList.Add(aOutputNode);

                        SpreaderNode1 = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                            lOutputNodeList);

                    }

                    object lSourceNode1;

                    string lextendSymbolicLink1 = lSymbolicLink1 + " --options=" +
        "<?xml version='1.0' encoding='UTF-8'?>" +
        "<Options>" +
            "<Option Type='Cursor' Visiblity='True'>" +
                "<Option.Extensions>" +
                    "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
                "</Option.Extensions>" +
            "</Option>" +
        "</Options>";

                    lSourceNode1 = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                        lextendSymbolicLink1,
                        lStreamIndex1,
                        lMediaTypeIndex1,
                        SpreaderNode1);

                    lresult = lSourceNode1;

                    break;
                }

                if (aCompressedMediaTypeIndex < 0)
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

                object SpreaderNode = lEncoderNode;

                if(PreviewRenderNode != null)
                {

                    List<object> lOutputNodeList = new List<object>();

                    lOutputNodeList.Add(PreviewRenderNode);

                    lOutputNodeList.Add(lEncoderNode);

                    SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                        lOutputNodeList);

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

                lSourceNode = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lextendSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    SpreaderNode);

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }     
    }
}
