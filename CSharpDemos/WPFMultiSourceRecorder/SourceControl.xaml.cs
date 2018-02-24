using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
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
using System.Xml;

namespace WPFMultiSourceRecorder
{
    /// <summary>
    /// Interaction logic for SourceControl.xaml
    /// </summary>
    public partial class SourceControl : UserControl, ISource
    {

        ISourceControl mSourceControl = null;

        IEncoderControl mEncoderControl = null;

        ISinkControl mSinkControl = null;

        IStreamControl mStreamControl = null;

        ISpreaderNodeFactory mSpreaderNodeFactory = null;

        IEVRMultiSinkFactory mEVRMultiSinkFactory = null;

        bool isLoaded = false;

        public string FriendlyName
        {
            get { return (string)GetValue(FriendlyNameProperty); }
            set { SetValue(FriendlyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FriendlyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FriendlyNameProperty =
            DependencyProperty.Register("FriendlyName", typeof(string), typeof(SourceControl), new UIPropertyMetadata(string.Empty));



        public string SymbolicLink
        {
            get { return (string)GetValue(SymbolicLinkProperty); }
            set { SetValue(SymbolicLinkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolicLink.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolicLinkProperty =
            DependencyProperty.Register("SymbolicLink", typeof(string), typeof(SourceControl), new UIPropertyMetadata(string.Empty));





        public string TypeSource
        {
            get { return (string)GetValue(TypeSourceProperty); }
            set { SetValue(TypeSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TypeSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeSourceProperty =
            DependencyProperty.Register("TypeSource", typeof(string), typeof(SourceControl), new UIPropertyMetadata(string.Empty));




        public SourceControl()
        {
            InitializeComponent();
        }

        public static UserControl create()
        {
            SourceControl lSourceControl = new SourceControl();

            return lSourceControl;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if((bool)mUsingChkBx.IsChecked)
            {
                if (isLoaded)
                    return;

                XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

                if (lXmlDataProvider == null)
                    return;

                XmlDocument doc = new XmlDocument();

                string lxmldoc = "";

                MainWindow.mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

                doc.LoadXml(lxmldoc);

                if (TypeSource == "Video")
                {
                    m_EncodersComboBox.SelectedIndex = 0;

                    lXmlDataProvider.XPath = "EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory";
                }
                else
                {
                    m_EncodersComboBox.SelectedIndex = 1;

                    lXmlDataProvider.XPath = "EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory";
                }
                                
                lXmlDataProvider.Document = doc;

                MainWindow.addSourceControl(this);

                isLoaded = true;
            }
            else
            {
                mExpander.IsExpanded = false;

                MainWindow.removeSourceControl(this);
            }
        }
        
        private void m_EncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {
                if (mEncoderControl == null)
                    break;

                var lselectedNode = m_EncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

                if (lCLSIDEncoderAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                    break;


                string lSymbolicLink = SymbolicLink;

                if (m_StreamComboBox.SelectedIndex < 0)
                    return;
                
                uint lStreamIndex = (uint)m_StreamComboBox.SelectedIndex;



                if (m_MediaTypeComboBox.SelectedIndex < 0)
                    return;
                
                uint lMediaTypeIndex = (uint)m_MediaTypeComboBox.SelectedIndex;
                
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
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.mISourceItems.Add(this);

            mEncoderControl = MainWindow.mCaptureManager.createEncoderControl();

            if (mEncoderControl == null)
                return;

            mSourceControl = MainWindow.mCaptureManager.createSourceControl();

            if (mSourceControl == null)
                return;



            mStreamControl = MainWindow.mCaptureManager.createStreamControl();

            if (mStreamControl == null)
                return;

            mStreamControl.createStreamControlNodeFactory(ref mSpreaderNodeFactory);

            if (mSpreaderNodeFactory == null)
                return;



            mSinkControl = MainWindow.mCaptureManager.createSinkControl();

            if (mSinkControl == null)
                return;

            mSinkControl.createSinkFactory(Guid.Empty, out mEVRMultiSinkFactory);

            if (mEVRMultiSinkFactory == null)
                return;
        }


        public object getCompressedMediaType()
        {
            object lresult = null;

            do
            {

                var lselectedNode = m_EncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lEncoderGuidAttr = lselectedNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;


                lselectedNode = m_EncodingModeComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lEncoderModeGuidAttr = lselectedNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;                               

                string lSymbolicLink = SymbolicLink;

                if (m_StreamComboBox.SelectedIndex < 0)
                    break;

                uint lStreamIndex = (uint)m_StreamComboBox.SelectedIndex;
                

                if (m_MediaTypeComboBox.SelectedIndex < 0)
                    break;

                uint lMediaTypeIndex = (uint)m_MediaTypeComboBox.SelectedIndex;

                object lSourceMediaType = null;

                if (!mSourceControl.getSourceOutputMediaType(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    out lSourceMediaType))
                    break;

                if (lSourceMediaType == null)
                    break;

                IEncoderNodeFactory lEncoderNodeFactory;

                if (!mEncoderControl.createEncoderNodeFactory(
                    lCLSIDEncoder,
                    out lEncoderNodeFactory))
                    break;

                if (lEncoderNodeFactory == null)
                    break;

                object lCompressedMediaType;

                if (!lEncoderNodeFactory.createCompressedMediaType(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)m_CompressedMediaTypesComboBox.SelectedIndex,
                    out lCompressedMediaType))
                    break;

                lresult = lCompressedMediaType;

            } while (false);

            return lresult;
        }

        public object getSourceNode(object aOutputNode)
        {
            object lresult = null;

            do
            {

                var lselectedNode = m_EncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lEncoderGuidAttr = lselectedNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;


                lselectedNode = m_EncodingModeComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lEncoderModeGuidAttr = lselectedNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;

                string lSymbolicLink = SymbolicLink;

                if (m_StreamComboBox.SelectedIndex < 0)
                    break;

                uint lStreamIndex = (uint)m_StreamComboBox.SelectedIndex;


                if (m_MediaTypeComboBox.SelectedIndex < 0)
                    break;

                uint lMediaTypeIndex = (uint)m_MediaTypeComboBox.SelectedIndex;

                object lSourceMediaType = null;

                if (!mSourceControl.getSourceOutputMediaType(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    out lSourceMediaType))
                    break;

                if (lSourceMediaType == null)
                    break;

                IEncoderNodeFactory lEncoderNodeFactory;

                if (!mEncoderControl.createEncoderNodeFactory(
                    lCLSIDEncoder,
                    out lEncoderNodeFactory))
                    break;

                if (lEncoderNodeFactory == null)
                    break;
                
                object lEncoderNode;

                if (!lEncoderNodeFactory.createEncoderNode(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)m_CompressedMediaTypesComboBox.SelectedIndex,
                    aOutputNode,
                    out lEncoderNode))
                    break;


                object SpreaderNode = lEncoderNode;

                if (TypeSource == "Video")
                {
                    object PreviewRenderNode = null;

                   // if ((bool)m_VideoStreamPreviewChkBtn.IsChecked)
                    {
                        List<object> lRenderOutputNodesList = new List<object>();

                        if (mEVRMultiSinkFactory != null)
                            mEVRMultiSinkFactory.createOutputNodes(
                                IntPtr.Zero,
                                m_EVRDisplay.Surface.texture,
                                1,
                                out lRenderOutputNodesList);

                        if (lRenderOutputNodesList.Count == 1)
                        {
                            PreviewRenderNode = lRenderOutputNodesList[0];
                        }
                    }


                    List<object> lOutputNodeList = new List<object>();

                    lOutputNodeList.Add(PreviewRenderNode);

                    lOutputNodeList.Add(lEncoderNode);

                    mSpreaderNodeFactory.createSpreaderNode(
                        lOutputNodeList,
                        out SpreaderNode);
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
                    SpreaderNode,
                    out lSourceNode))
                    break;

                lresult = lSourceNode;

            } while (false);

            return lresult;
        }


        public void access(bool aState)
        {
            mUsingChkBx.IsEnabled = aState;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.mISourceItems.Remove(this);
        }
    }
}
