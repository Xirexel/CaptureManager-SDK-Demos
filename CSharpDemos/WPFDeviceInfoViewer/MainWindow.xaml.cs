using CaptureManagerToCSharpProxy;
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

namespace WPFDeviceInfoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CaptureManager lCaptureManager = null;

            try
            {
                lCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (System.Exception exc)
            {
                try
                {
                    lCaptureManager = new CaptureManager();
                }
                catch (System.Exception exc1)
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

            lCaptureManager.getCollectionOfSources(ref lxmldoc);

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
            
            System.Xml.XmlDocument groupDoc = new System.Xml.XmlDocument();

            var lroot = groupDoc.CreateElement("Sources");

            groupDoc.AppendChild(lroot);

            foreach (var item in lDeviceLinkList)
            {
                var ldevices = doc.SelectNodes("Sources/Source[Source.Attributes/Attribute[@Name='CM_DEVICE_LINK']/SingleValue[@Value='" + item + "']]");

                if (ldevices != null)
                {
                    var lgroup = groupDoc.CreateElement("DeviceGroup");

                    var lTitle = groupDoc.CreateAttribute("Title");

                    lTitle.Value = item;

                    lgroup.Attributes.Append(lTitle);

                    foreach (var node in ldevices)
                    {
                        var lSourceNode = groupDoc.ImportNode((node as System.Xml.XmlNode), true);

                        lgroup.AppendChild(lSourceNode);
                    }

                    lroot.AppendChild(lgroup);
                }
            }

            
            lXmlDataProvider.XPath = "Sources/DeviceGroup";

            lXmlDataProvider.Document = groupDoc;
        }
    }
}
