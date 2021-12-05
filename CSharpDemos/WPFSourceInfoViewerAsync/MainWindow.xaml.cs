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
using CaptureManagerToCSharpProxy;

namespace WPFSourceInfoViewerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CaptureManager lCaptureManager = null;

            try
            {
                lCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (System.Exception)
            {
                try
                {
                    lCaptureManager = new CaptureManager();
                }
                catch (System.Exception)
                {

                }
            }

            if (lCaptureManager == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = "";

            lxmldoc = await lCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrWhiteSpace(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            var lDeviceLinkNodeList = doc.SelectNodes("Sources/Source/Source.Attributes/Attribute[@Name='CM_DEVICE_LINK']/SingleValue/@Value");

            List<string> lDeviceLinkList = new List<string>();

            for (int i = 0; i < lDeviceLinkNodeList.Count; i++)
            {
                if (!lDeviceLinkList.Contains(lDeviceLinkNodeList.Item(i).Value))
                {
                    lDeviceLinkList.Add(lDeviceLinkNodeList.Item(i).Value);
                }
            }
            
            lXmlDataProvider.Document = doc;
        }
    }
}
