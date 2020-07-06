using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using System.Xml;
using WPFAreaScreenRecorder.Properties;

namespace WPFAreaScreenRecorder
{
    class MediaTypeConvertor : IValueConverter
    {

        public string Type
        {
            get;
            set;
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string lresult = "";

            do
            {

                XmlNode lnode = value as XmlNode;

                if (lnode == null || lnode.LocalName != "MediaType")
                    break;

                if (Type == "Video")
                {
                    var lAttr = lnode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                    if (lAttr != null)
                        lresult = lAttr.Value.Replace("MFVideoFormat_", "");

                    var ltempNode = lnode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_MPEG2_PROFILE']/SingleValue/@Value");

                    if (ltempNode != null)
                    {
                        switch (ltempNode.Value)
                        {
                            case "66":
                                lresult += ", " + "Baseline Profile";
                                break;
                            case "77":
                                lresult += ", " + "Main Profile";
                                break;
                            case "100":
                                lresult += ", " + "High Profile";
                                break;
                            default:
                                break;
                        }
                    }
                }

            } while (false);

            return lresult;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    class RemoveConvertor : IValueConverter
    {

        public string Template
        {
            get;
            set;
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string lresult = "";

            do
            {
                if (value == null)
                    break;

                lresult = value.ToString();
                
                var lremoves = Template.Split(new char[]{'|'});

                foreach (var item in lremoves)
	            {
                    lresult = lresult.Replace(item, "");
	            }                
            } while (false);

            return lresult;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public static string mCurrentSymbolicLink = "";

        IEncoderControl mEncoderControl = null;

        ISourceControl mSourceControl = null;
        
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ControlWindow.mCaptureManager == null)
                return;

            mSourceControl = ControlWindow.mCaptureManager.createSourceControl();

            if (mSourceControl == null)
                return;

            mEncoderControl = ControlWindow.mCaptureManager.createEncoderControl();

            if (mEncoderControl == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = "";

            var lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            ControlWindow.mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;



            ControlWindow.mCaptureManager.getCollectionOfSinks(ref lxmldoc);


            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;




            ControlWindow.mCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc = new XmlDocument();

            doc.LoadXml(lxmldoc);
            
            string lxpath = "//*[Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue[@Value='Temp']]";

            lxpath = lxpath.Replace("Temp", mCurrentSymbolicLink);

            var lNode = doc.SelectSingleNode(lxpath);

            if (lNode != null)
            {
                m_VideoStreamComboBox.DataContext = lNode;
            }

            mSourceName.Text = mCurrentSymbolicLink;
        }

        private void m_VideoEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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


                string lSymbolicLink = mCurrentSymbolicLink;
                
                uint lStreamIndex = 0;
                
                uint lMediaTypeIndex = 0;
                
                object lOutputMediaType;
                
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

        private void mWDBtn_Click(object sender, RoutedEventArgs e)
        {

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = Settings.Default.StoringDir;

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.Default.StoringDir = dialog.SelectedPath;
                }
            }
        }

        private void mConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();

            Close();
        }

        private void mCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();

            Close();
        }

        private void mResetBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
        }
    }
}
