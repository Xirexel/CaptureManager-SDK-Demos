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

namespace WPFMultiSourceRecorderAsync
{
    /// <summary>
    /// Interaction logic for SourceControl.xaml
    /// </summary>
    public partial class SourceControl : UserControl, ISource
    {

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;

        ISinkControlAsync mSinkControl = null;

        IStreamControlAsync mStreamControl = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;

        IEVRMultiSinkFactoryAsync mEVRMultiSinkFactory = null;

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
            if ((bool)mUsingChkBx.IsChecked)
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

        private async void m_EncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
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

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.mISourceItems.Add(this);

            mEncoderControl = await MainWindow.mCaptureManager.createEncoderControlAsync();

            if (mEncoderControl == null)
                return;

            mSourceControl = await MainWindow.mCaptureManager.createSourceControlAsync();

            if (mSourceControl == null)
                return;



            mStreamControl = await MainWindow.mCaptureManager.createStreamControlAsync();

            if (mStreamControl == null)
                return;

            mSpreaderNodeFactory = await mStreamControl.createSpreaderNodeFactoryAsync();

            if (mSpreaderNodeFactory == null)
                return;



            mSinkControl = await MainWindow.mCaptureManager.createSinkControlAsync();

            if (mSinkControl == null)
                return;

            mEVRMultiSinkFactory = await mSinkControl.createEVRMultiSinkFactoryAsync(Guid.Empty);

            if (mEVRMultiSinkFactory == null)
                return;
        }


        public async Task<object> getCompressedMediaType()
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
                    (uint)m_CompressedMediaTypesComboBox.SelectedIndex);

                lresult = lCompressedMediaType;

            } while (false);

            return lresult;
        }

        public async Task<object> getSourceNode(object aOutputNode)
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
                    (uint)m_CompressedMediaTypesComboBox.SelectedIndex,
                    aOutputNode);


                object SpreaderNode = lEncoderNode;

                if (TypeSource == "Video")
                {
                    object PreviewRenderNode = null;

                    // if ((bool)m_VideoStreamPreviewChkBtn.IsChecked)
                    {
                        List<object> lRenderOutputNodesList = new List<object>();

                        if (mEVRMultiSinkFactory != null)
                            lRenderOutputNodesList = await mEVRMultiSinkFactory.createOutputNodesAsync(
                                IntPtr.Zero,
                                m_EVRDisplay.Surface.texture,
                                1);

                        if (lRenderOutputNodesList.Count == 1)
                        {
                            PreviewRenderNode = lRenderOutputNodesList[0];
                        }
                    }


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
