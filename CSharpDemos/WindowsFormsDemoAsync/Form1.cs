using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsDemoAsync
{
    public partial class WebViewer : Form
    {
        CaptureManager mCaptureManager = null;

        ISessionAsync mISession = null;

        public XmlNode mSelectedSourceXmlNode = null;

        class ContainerItem
        {
            public string mFriendlyName = "SourceItem";

            public XmlNode mXmlNode;

            public override string ToString()
            {
                return mFriendlyName;
            }
        }
        
        public WebViewer()
        {
            InitializeComponent();  
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
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

            await fillSourceCmboBox();
        }

        private async Task fillSourceCmboBox()
        {


            if (mCaptureManager == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            string lXPath = "//*[";

            if(toolStripMenuItem1.Checked)
            {

                lXPath += "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_CATEGORY']/SingleValue[@Value='CLSID_WebcamInterfaceDeviceCategory']";
            }
                        
            if (toolStripMenuItem2.Checked)
            {
                if (toolStripMenuItem1.Checked)
                    lXPath += "or ";

                lXPath += "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_HW_SOURCE']/SingleValue[@Value='Software device']";
            }

            if (dSCrossbarToolStripMenuItem.Checked)
            {
                if (toolStripMenuItem1.Checked || toolStripMenuItem2.Checked)
                    lXPath += "or ";

                lXPath += "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_CATEGORY']/SingleValue[@Value='CLSID_VideoInputDeviceCategory']";
            }                      
            
            lXPath += "]";

            XmlNodeList lSourceNodes = null;

            try
            {

                lSourceNodes = doc.SelectNodes(lXPath);
            }
            catch (Exception)
            {

            }

            sourceComboBox.Items.Clear();

            if (lSourceNodes == null)
                return;

            if (lSourceNodes != null)
            {
                foreach (var item in lSourceNodes)
                {
                    var lNode = (XmlNode)item;

                    if (lNode != null)
                    {
                        var lvalueNode = lNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME']/SingleValue/@Value");

                        ContainerItem lSourceItem = new ContainerItem()
                        {
                            mFriendlyName = lvalueNode.Value,
                            mXmlNode = lNode
                        };

                        sourceComboBox.Items.Add(lSourceItem);
                    }


                }
            }
        }

        private void sourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lSelectedSourceItem = (ContainerItem)sourceComboBox.SelectedItem;

            if (lSelectedSourceItem == null)
                return;

            var lSubTypesNode = lSelectedSourceItem.mXmlNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType/MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

            if (lSubTypesNode == null)
                return;

            mSelectedSourceXmlNode = lSelectedSourceItem.mXmlNode;

            streamComboBox.Items.Clear();

            foreach (XmlNode item in lSubTypesNode)
            {
                var lSubType = item.Value.Replace("MFVideoFormat_", "");

                if (!streamComboBox.Items.Contains(lSubType))
                    streamComboBox.Items.Add(lSubType);
            }
        }

        private void streamComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lCurrentSubType = (string)streamComboBox.SelectedItem;
            
            var lCurrentSourceNode = mSelectedSourceXmlNode as XmlNode;

            if (lCurrentSourceNode == null)
                return;

            var lMediaTypesNode = lCurrentSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType[MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue[@Value='MFVideoFormat_" + lCurrentSubType + "']]");

            if (lMediaTypesNode == null)
                return;

            mediaTypeComboBox.Items.Clear();

            foreach (var item in lMediaTypesNode)
            {
                var lNode = (XmlNode)item;

                if (lNode != null)
                {
                    var lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

                    string mTitle = lvalueNode.Value;

                    lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2 ]/@Value");

                    mTitle += "x" + lvalueNode.Value;

                    lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_RATE']/RatioValue/@Value");

                    mTitle += ", " + lvalueNode.Value + " FPS, ";

                    lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                    mTitle += lvalueNode.Value.Replace("MFVideoFormat_", "");


                    ContainerItem lSourceItem = new ContainerItem()
                    {
                        mFriendlyName = mTitle,// lvalueNode.Value.Replace("MFMediaType_", ""),
                        mXmlNode = lNode
                    };

                    mediaTypeComboBox.Items.Add(lSourceItem);
                }
            }
        }

        private async void start_stopBtn_Click(object sender, EventArgs e)
        {
            start_stopBtn.Enabled = false;

            do
            {
                if (start_stopBtn.Text == "Stop")
                {
                    if (mISession != null)
                    {
                        await mISession.closeSessionAsync();

                        start_stopBtn.Text = "Start";
                    }

                    mISession = null;

                    break;
                }

                var lSelectedSourceItem = (ContainerItem)sourceComboBox.SelectedItem;

                if (lSelectedSourceItem == null)
                    break;

                var lSourceNode = lSelectedSourceItem.mXmlNode;

                if (lSourceNode == null)
                    break;

                var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                uint lStreamIndex = 0;

                var lMediaTypeItem = (ContainerItem)mediaTypeComboBox.SelectedItem;

                if (lMediaTypeItem == null)
                    break;

                lSourceNode = lMediaTypeItem.mXmlNode;

                if (lSourceNode == null)
                    break;

                lNode = lSourceNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                    break;


                string lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(lxmldoc);

                var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{2F34AF87-D349-45AA-A5F1-E4104D5C458E}']");

                if (lSinkNode == null)
                    break;

                var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

                if (lContainerNode == null)
                    break;

                var lSinkControl = await mCaptureManager.createSinkControlAsync();

                var lSinkFactory = await lSinkControl.createEVRSinkFactoryAsync(Guid.Empty);

                object lEVROutputNode = await lSinkFactory.createOutputNodeAsync(
                    mVideoPanel.Handle);

                if (lEVROutputNode == null)
                    break;

                var lSourceControl = await mCaptureManager.createSourceControlAsync();

                if (lSourceControl == null)
                    break;

                object lPtrSourceNode = await lSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    lEVROutputNode);

                List<object> lSourceMediaNodeList = new List<object>();

                lSourceMediaNodeList.Add(lPtrSourceNode);

                var lSessionControl = await mCaptureManager.createSessionControlAsync();

                if (lSessionControl == null)
                    break;

                mISession = await lSessionControl.createSessionAsync(
                    lSourceMediaNodeList.ToArray());

                if (mISession == null)
                    break;

                await mISession.startSessionAsync(0, Guid.Empty);

                start_stopBtn.Text = "Stop";

            } while (false);

            start_stopBtn.Enabled = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void mediaTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void aLlToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            fillSourceCmboBox();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            fillSourceCmboBox();
        }

        private void dSCrossbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fillSourceCmboBox();
        }
    }
}
