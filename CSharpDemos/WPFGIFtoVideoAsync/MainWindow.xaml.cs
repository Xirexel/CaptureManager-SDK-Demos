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

namespace WPFGIFtoVideoAsync
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

        IFileSinkFactoryAsync mFileSinkFactory = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;




        Guid m_VideoEncoderMode = new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55);

        Guid m_AudioEncoderMode = new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55);




        public CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl mIEVRStreamControl = null;
        

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
            var l_image_source = await createImageSource();

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

            mFileSinkFactory = await mSinkControl.createFileSinkFactoryAsync(
            Guid.Parse(lSelectedAttr.Value));


            

            // Video Source
            uint lVideoSourceIndexStream = 0;

            uint lVideoSourceIndexMediaType = 0;

            int l_VideoCompressedMediaTypeSelectedIndex = 0;


            

            string l_EncodersXMLstring = await mEncoderControl.getCollectionOfEncodersAsync();


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
                object lCompressedMediaType = await getCompressedMediaType(
                        l_image_source,
                        lVideoSourceIndexStream,
                        lVideoSourceIndexMediaType,
                        l_VideoEncoder,
                        m_VideoEncoderMode,
                        l_VideoCompressedMediaTypeSelectedIndex);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }
                                   
            List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList, lFilename);

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return;

            var lSinkFactory = await mSinkControl.createEVRSinkFactoryAsync(
            Guid.Empty);

            object lEVROutputNode =  await lSinkFactory.createOutputNodeAsync(
                    mVideoPanel.Handle);

            if (lEVROutputNode == null)
                return;


            object SpreaderNode = lEVROutputNode;

            if (true)
            {
                var lEncoderNode = await getEncoderNode(
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

                SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                    lOutputNodeList);
            }           
                                          
            List<object> lSourceNodes = new List<object>();

            object lSourceNode = await mSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnectionAsync(
                l_image_source,
                lVideoSourceIndexStream,
                lVideoSourceIndexMediaType,
                SpreaderNode);

            if (lSourceNode == null)
                return;

            lSourceNodes.Add(lSourceNode);
                       
            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession != null && await mISession.startSessionAsync(0, Guid.Empty))
            {
                mStartStopTxtBlk.Text = "Stop";
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mISession != null)
            {
                mStartStopTxtBlk.Text = "Start";

                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

                mISession = null;
                
                return;
            }

            await init();
        }
        
        private async Task<object> createImageSource()
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

                    l_source = await mSourceControl.createSourceFromCaptureProcessorAsync(
                        lICaptureProcessor);
                }

            } while (false);

            return l_source;
        }
                        
        private async Task<object> getEncoderNode(
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

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeFromMediaSourceAsync(
                    aMediaSource,
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

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeFromMediaSourceAsync(
                    aMediaSource,
                    aStreamIndex,
                    aMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(
                    aCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                lresult = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    aCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);

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
