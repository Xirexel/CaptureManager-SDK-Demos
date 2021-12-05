using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsDemo
{
    public partial class WebViewer : Form
    {
        CaptureManager mCaptureManager = null;

        ISession mISession = null;

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

            fillSourceCmboBox();
        }

        private void fillSourceCmboBox()
        {


            if (mCaptureManager == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

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

        private void start_stopBtn_Click(object sender, EventArgs e)
        {

            if (start_stopBtn.Text == "Stop")
            {
                if (mISession != null)
                {
                    mISession.closeSession();

                    start_stopBtn.Text = "Start";
                }

                mISession = null;

                return;
            }

            var lSelectedSourceItem = (ContainerItem)sourceComboBox.SelectedItem;

            if (lSelectedSourceItem == null)
                return;

            var lSourceNode = lSelectedSourceItem.mXmlNode;

            if (lSourceNode == null)
                return;

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;
            
            uint lStreamIndex = 0;
            
            var lMediaTypeItem = (ContainerItem)mediaTypeComboBox.SelectedItem;

            if (lMediaTypeItem == null)
                return;

            lSourceNode = lMediaTypeItem.mXmlNode;

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


            string lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{2F34AF87-D349-45AA-A5F1-E4104D5C458E}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            IEVRSinkFactory lSinkFactory;

            var lSinkControl = mCaptureManager.createSinkControl();

            lSinkControl.createSinkFactory(
            Guid.Empty,
            out lSinkFactory);

            object lEVROutputNode;

            lSinkFactory.createOutputNode(
                mVideoPanel.Handle,
                out lEVROutputNode);

            if (lEVROutputNode == null)
                return;

            object lPtrSourceNode;

            var lSourceControl = mCaptureManager.createSourceControl();

            if (lSourceControl == null)
                return;


            lSourceControl.createSourceNode(
                lSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex,
                lEVROutputNode,
                out lPtrSourceNode);


            List<object> lSourceMediaNodeList = new List<object>();

            lSourceMediaNodeList.Add(lPtrSourceNode);

            var lSessionControl = mCaptureManager.createSessionControl();

            if (lSessionControl == null)
                return;

            mISession = lSessionControl.createSession(
                lSourceMediaNodeList.ToArray());

            if (mISession == null)
                return;

            mISession.startSession(0, Guid.Empty);

            start_stopBtn.Text = "Stop";
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
