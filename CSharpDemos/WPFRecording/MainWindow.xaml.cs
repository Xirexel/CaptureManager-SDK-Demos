using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Xml;

namespace WPFRecording
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum SinkType
        {
            Node,
            File,
            BitStream
        }

        SinkType mSinkType = SinkType.Node;
        
        CaptureManager mCaptureManager = null;

        ISessionControl mISessionControl = null;

        ISinkControl mSinkControl = null;

        ISourceControl mSourceControl = null;

        IEncoderControl mEncoderControl = null;

        AbstractSink mSink = null;

        ISession mSession = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                mCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (System.Exception exc)
            {
                try
                {
                    mCaptureManager = new CaptureManager();
                }
                catch (System.Exception exc1)
                {

                }
            }

            if (mCaptureManager == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            mSourceControl = mCaptureManager.createSourceControl();

            


            mCaptureManager.getCollectionOfSinks(ref lxmldoc);
            

            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSinkFactoryCollectionProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new System.Xml.XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;


            mISessionControl = mCaptureManager.createSessionControl();

            mSinkControl = mCaptureManager.createSinkControl();

            mEncoderControl = mCaptureManager.createEncoderControl();

            mEncodersComboBox.SelectionChanged += delegate
            {
                do
                {
                    if (mEncoderControl == null)
                        break;

                    var lselectedNode = mEncodersComboBox.SelectedItem as XmlNode;

                    if (lselectedNode == null)
                        break;

                    var lEncoderNameAttr = lselectedNode.Attributes["Title"];

                    if (lEncoderNameAttr == null)
                        break;
                    
                    var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

                    if (lCLSIDEncoderAttr == null)
                        break;

                    Guid lCLSIDEncoder;

                    if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                        break;



                    var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

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

                    lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

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



                    object lOutputMediaType;

                    if (mSourceControl == null)
                        return;

                    mSourceControl.getSourceOutputMediaType(
                        lSymbolicLink,
                        lStreamIndex,
                        lMediaTypeIndex,
                        out lOutputMediaType);

                    string lMediaTypeCollection;

                    if (!mEncoderControl.getMediaTypeCollectionOfEncoder(
                        lOutputMediaType,
                        lCLSIDEncoder,
                        out lMediaTypeCollection))
                        break;



                    XmlDataProvider lXmlEncoderModeDataProvider = (XmlDataProvider)this.Resources["XmlEncoderModeProvider"];

                    if (lXmlEncoderModeDataProvider == null)
                        return;

                    XmlDocument lEncoderModedoc = new XmlDocument();

                    lEncoderModedoc.LoadXml(lMediaTypeCollection);

                    lXmlEncoderModeDataProvider.Document = lEncoderModedoc;


                } while (false);
            };


            mEncodingModeComboBox.SelectionChanged += delegate
            {
                do
                {
                    if (mEncoderControl == null)
                        break;

                    var lselectedNode = mEncodingModeComboBox.SelectedItem as XmlNode;

                    if (lselectedNode == null)
                        break;

                    var lGUIDEncodingModeAttr = lselectedNode.Attributes["GUID"];

                    if (lGUIDEncodingModeAttr == null)
                        break;

                    Guid lGUIDEncodingMode;

                    if (!Guid.TryParse(lGUIDEncodingModeAttr.Value, out lGUIDEncodingMode))
                        break;

                    var lConstantMode = Guid.Parse("{CA37E2BE-BEC0-4B17-946D-44FBC1B3DF55}");
 
                    XmlDataProvider lXmlMediaTypeProvider = (XmlDataProvider)this.Resources["XmlMediaTypesCollectionProvider"];

                    if (lXmlMediaTypeProvider == null)
                        return;

                    XmlDocument lMediaTypedoc = new XmlDocument();

                    var lClonedMediaTypesNode = lMediaTypedoc.ImportNode(lselectedNode, true);

                    lMediaTypedoc.AppendChild(lClonedMediaTypesNode);

                    lXmlMediaTypeProvider.Document = lMediaTypedoc;


                } while (false);
            };

            mSinkFactoryComboBox.SelectionChanged += delegate
            {
                do
                {
                    if (mEncoderControl == null)
                        break;

                    var lselectedNode = mSinkFactoryComboBox.SelectedItem as XmlNode;

                    if (lselectedNode == null)
                        break;

                    var lAttr = lselectedNode.Attributes["GUID"];

                    if (lAttr == null)
                        throw new System.Exception("GUID is empty");

                    mContainerTypeComboBox.IsEnabled = false;

                    mSinkType = SinkType.Node;

                    if (lAttr.Value == "{D6E342E3-7DDD-4858-AB91-4253643864C2}")
                    {
                        mContainerTypeComboBox.IsEnabled = true;

                        mSinkType = SinkType.File;
                    }
                    else if (lAttr.Value == "{2E891049-964A-4D08-8F36-95CE8CB0DE9B}")
                    {
                        mContainerTypeComboBox.IsEnabled = true;

                        mSinkType = SinkType.BitStream;
                    }
                    else if (lAttr.Value == "{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}")
                    {
                    }
                    else if (lAttr.Value == "{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}")
                    {
                    }
                    else if (lAttr.Value == "{2F34AF87-D349-45AA-A5F1-E4104D5C458E}")
                    {
                    }

                    XmlDataProvider lXmlMediaTypeProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

                    if (lXmlMediaTypeProvider == null)
                        return;

                    XmlDocument lMediaTypedoc = new XmlDocument();

                    var lClonedMediaTypesNode = lMediaTypedoc.ImportNode(lselectedNode, true);

                    lMediaTypedoc.AppendChild(lClonedMediaTypesNode);

                    lXmlMediaTypeProvider.Document = lMediaTypedoc;

                } while (false);
            };
        }

        private void mMediaTypesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                        
            var lAttr = mMediaTypesComboBox.Tag as XmlAttribute;

            if (lAttr == null)
                return;

            string lXPath = "EncoderFactories/Group[@GUID='blank']/EncoderFactory";

            lXPath = lXPath.Replace("blank", lAttr.Value);

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoderMediaTypesProvider"];

            if (lXmlDataProvider == null)
                return;

            string lxmldoc = "";

            mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.XPath = lXPath;

            lXmlDataProvider.Document = doc;

        }
        
        private void mDo_Click(object sender, RoutedEventArgs e)
        {

            if (mSession != null)
            {
                mSession.closeSession();

                mSession = null;

                mDo.Content = "Stopped";

                return;
            }

            if (mSink == null)
                return;

            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

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

            lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

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

            object lOutputMediaType;

            mSourceControl.getSourceOutputMediaType(
                        lSymbolicLink,
                        lStreamIndex,
                        lMediaTypeIndex,
                        out lOutputMediaType);

            var lselectedNode = mEncodersComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null)
                return;

            var lEncoderNameAttr = lselectedNode.Attributes["Title"];

            if (lEncoderNameAttr == null)
                return;
            
            var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

            if (lCLSIDEncoderAttr == null)
                return;

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                return;

            IEncoderNodeFactory lEncoderNodeFactory;

            mEncoderControl.createEncoderNodeFactory(
                lCLSIDEncoder,
                out lEncoderNodeFactory);

            

            lselectedNode = mEncodingModeComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null)
                return;

            var lGUIDEncodingModeAttr = lselectedNode.Attributes["GUID"];

            if (lGUIDEncodingModeAttr == null)
                return;

            Guid lGUIDEncodingMode;

            if (!Guid.TryParse(lGUIDEncodingModeAttr.Value, out lGUIDEncodingMode))
                return;

            if (mCompressedMediaTypesComboBox.SelectedIndex < 0)
                return;
            
            object lCompressedMediaType;

            lEncoderNodeFactory.createCompressedMediaType(
                lOutputMediaType,
                lGUIDEncodingMode,
                70,
                (uint)mCompressedMediaTypesComboBox.SelectedIndex,
                out lCompressedMediaType);

            var lOutputNode = mSink.getOutputNode(lCompressedMediaType);

            IEncoderNodeFactory lIEncoderNodeFactory;

            mEncoderControl.createEncoderNodeFactory(
                lCLSIDEncoder,
                out lIEncoderNodeFactory);

            object lEncoderNode;

            lIEncoderNodeFactory.createEncoderNode(
                lOutputMediaType,
                lGUIDEncodingMode,
                70,
                (uint)mCompressedMediaTypesComboBox.SelectedIndex,
                lOutputNode,
                out lEncoderNode);

            object lSourceMediaNode;

            string lextendSymbolicLink = lSymbolicLink + " --options=" +
    "<?xml version='1.0' encoding='UTF-8'?>" +
    "<Options>" +
        "<Option Type='Cursor' Visiblity='True'>" +
            "<Option.Extensions>" +
                "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7000ff55' />" +
            "</Option.Extensions>" +
        "</Option>" +
    "</Options>";

            mSourceControl.createSourceNode(
                        lextendSymbolicLink,
                        lStreamIndex,
                        lMediaTypeIndex,
                        lEncoderNode,
                        out lSourceMediaNode);

            List<object> lSourcesList = new List<object>();

            lSourcesList.Add(lSourceMediaNode);

            mSession = mISessionControl.createSession(lSourcesList.ToArray());


            if (mSession != null)
                mSession.startSession(0, Guid.Empty);

            mDo.Content = "Record is executed!!!";
         
        }

        private void mOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            switch (mSinkType)
            {
                case SinkType.Node:
                    break;
                case SinkType.File:
                    {
                        do
                        {
                            

                            var lselectedNode = mContainerTypeComboBox.SelectedItem as XmlNode;

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

                            mDo.IsEnabled = true;
                            
                            lSelectedAttr = lselectedNode.Attributes["GUID"];

                            if (lSelectedAttr == null)
                                break;

                            IFileSinkFactory lFileSinkFactory;

                            mSinkControl.createSinkFactory(
                                Guid.Parse(lSelectedAttr.Value),
                                out lFileSinkFactory);

                            mSink = new FileSink(lFileSinkFactory);

                            mSink.setOptions(lsaveFileDialog.FileName);

                        }
                        while (false);
                    }
                    break;
                case SinkType.BitStream:
                    {
                       
                            var lselectedNode = mContainerTypeComboBox.SelectedItem as XmlNode;

                            if (lselectedNode == null)
                                break; 
                            
                            var lSelectedAttr = lselectedNode.Attributes["GUID"];

                            if (lSelectedAttr == null)
                                break;

                            IByteStreamSinkFactory lByteStreamSinkFactory;

                            mSinkControl.createSinkFactory(
                                Guid.Parse(lSelectedAttr.Value),
                                out lByteStreamSinkFactory);

                            lSelectedAttr = lselectedNode.Attributes["MIME"];

                            if (lSelectedAttr == null)
                                break;

                            mSink = new NetworkStreamSink(
                                lByteStreamSinkFactory,
                                lSelectedAttr.Value);

                            mSink.setOptions("8080");

                            mDo.IsEnabled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mSession != null)
            {
                var ltimer = new System.Windows.Threading.DispatcherTimer();

                ltimer.Interval = new TimeSpan(0, 0, 0, 1);

                ltimer.Tick += delegate
                (object sender1, EventArgs e1)
                {
                    if (mSession != null)
                    {
                        mSession.closeSession();
                    }

                    mSession = null;

                    Close();

                    (sender1 as System.Windows.Threading.DispatcherTimer).Stop();
                };

                ltimer.Start();

                e.Cancel = true;
            }
        }
    }
}
