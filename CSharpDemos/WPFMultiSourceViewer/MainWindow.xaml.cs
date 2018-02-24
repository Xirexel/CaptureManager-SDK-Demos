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
using System.Windows.Threading;
using System.Xml;

namespace WPFMultiSourceViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        IDictionary<int, ISession> mISessions = new Dictionary<int, ISession>();

        IWebCamControl mWebCamControl;

        List<object> mEVROutputNodes = null;

        uint mStreams = 2;

        int mStreamCount = 0;

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

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            mWebCamParametrsTab.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mParametrSlider_ValueChanged));

            mWebCamParametrsTab.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

            mWebCamParametrsTab.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

        }

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {

            var lButton = (Button)sender;

            if (lButton == null)
                return;

            if (lButton.Tag == null)
            {
                lButton.Tag = mStreamCount++;
            }

            int lSessionIndex = (int)lButton.Tag;

            if (lButton.Content == "Stop")
            {
                if (mISessions.ContainsKey(lSessionIndex))
                {
                    mISessions[lSessionIndex].closeSession();

                    //System.Threading.Thread.Sleep(200);
                    
                    var lEVRStreamControl1 = mCaptureManager.createEVRStreamControl();

                    if (lEVRStreamControl1 != null)
                    {
                        lEVRStreamControl1.flush(mEVROutputNodes[lSessionIndex]);
                    }

                    mISessions.Remove(lSessionIndex);
                }

                lButton.Content = "Launch";
                
                return;
            }

            string lSymbolicLink = "CaptureManager///Software///Sources///ScreenCapture///ScreenCapture";
            
            string lextendSymbolicLink = lSymbolicLink + " --options=" +
                "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Options>" +
                    "<Option Type='Cursor' Visiblity='True'>" +
                        "<Option.Extensions>" +
                            "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
                        "</Option.Extensions>" +
                    "</Option>" +
                "</Options>";

            uint lStreamIndex = 0;
            
            uint lMediaTypeIndex = 4;
            
            string lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{10E52132-A73F-4A9E-A91B-FE18C91D6837}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            IEVRMultiSinkFactory lSinkFactory;

            var lSinkControl = mCaptureManager.createSinkControl();

            lSinkControl.createSinkFactory(
            Guid.Empty,
            out lSinkFactory);

            if (mEVROutputNodes == null)
                lSinkFactory.createOutputNodes(
                    mVideoPanel.Handle,
                    mStreams,
                    out mEVROutputNodes);

            if (mEVROutputNodes == null)
                return;

            if (mEVROutputNodes.Count == 0)
                return;

            var lSourceControl = mCaptureManager.createSourceControl();

            if (lSourceControl == null)
                return;



            object lPtrSourceNode;

            lSourceControl.createSourceNode(
                lextendSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex,
                mEVROutputNodes[lSessionIndex],
                out lPtrSourceNode);


            List<object> lSourceMediaNodeList = new List<object>();

            lSourceMediaNodeList.Add(lPtrSourceNode);

            var lSessionControl = mCaptureManager.createSessionControl();

            if (lSessionControl == null)
                return;

            ISession lISession = lSessionControl.createSession(
                lSourceMediaNodeList.ToArray());

            if (lISession == null)
                return;

            lISession.registerUpdateStateDelegate(UpdateStateDelegate);

            lISession.startSession(0, Guid.Empty);

            mISessions.Add(lSessionIndex, lISession);

            var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

            if (lEVRStreamControl != null)
            {
                lEVRStreamControl.setPosition(mEVROutputNodes[lSessionIndex],
                    0.5f * lSessionIndex,
                    0.5f + (0.5f * lSessionIndex),                                        
                    0.5f * lSessionIndex,
                    0.5f + (0.5f * lSessionIndex));

                                
                lEVRStreamControl.setZOrder(mEVROutputNodes[lSessionIndex],
                    1);

                float lLeft;
                float lRight;
                float lTop;
                float lBottom;

                lEVRStreamControl.getPosition(mEVROutputNodes[lSessionIndex],
                out lLeft,
                out lRight,
                out lTop,
                out lBottom);
                
                uint lZOrder;

                lEVRStreamControl.getZOrder(mEVROutputNodes[lSessionIndex],
                    out lZOrder);
            }

            lButton.Content = "Stop";

        }

        void UpdateStateDelegate(uint aCallbackEventCode, uint aSessionDescriptor)
        {
            SessionCallbackEventCode k = (SessionCallbackEventCode)aCallbackEventCode;

            switch (k)
            {
                case SessionCallbackEventCode.Unknown:
                    break;
                case SessionCallbackEventCode.Error:
                    break;
                case SessionCallbackEventCode.Status_Error:
                    break;
                case SessionCallbackEventCode.Execution_Error:
                    break;
                case SessionCallbackEventCode.ItIsReadyToStart:
                    break;
                case SessionCallbackEventCode.ItIsStarted:
                    break;
                case SessionCallbackEventCode.ItIsPaused:
                    break;
                case SessionCallbackEventCode.ItIsStopped:
                    break;
                case SessionCallbackEventCode.ItIsEnded:
                    break;
                case SessionCallbackEventCode.ItIsClosed:
                    break;
                case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
                    {


                        //Dispatcher.Invoke(
                        //DispatcherPriority.Normal,
                        //new Action(() => mLaunchButton_Click(null, null)));

                    }
                    break;
                default:
                    break;
            }
        }

        private void mParametrSlider_ValueChanged(object sender, RoutedEventArgs e)
        {

            var lslider = e.OriginalSource as Slider;

            if (lslider == null)
                return;

            if (!lslider.IsFocused)
                return;

            var lParametrNode = lslider.Tag as XmlNode;

            if (lParametrNode == null)
                return;

            var lAttr = lParametrNode.Attributes["Index"];

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            int lvalue = (int)lslider.Value;

            mWebCamControl.setCamParametr(
                lindex,
                lvalue,
                2);

        }

        private void mParametrSlider_Checked(object sender, RoutedEventArgs e)
        {

            var lCheckBox = e.OriginalSource as CheckBox;

            if (lCheckBox == null)
                return;

            if (!lCheckBox.IsFocused)
                return;

            var lAttr = lCheckBox.Tag as XmlAttribute;

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            int lvalue = (bool)lCheckBox.IsChecked ? 2 : 1;

            int lCurrentValue;
            int lMin;
            int lMax;
            int lStep;
            int lDefault;
            int lFlag;

            mWebCamControl.getCamParametr(
                lindex,
                out lCurrentValue,
                out lMin,
                out lMax,
                out lStep,
                out lDefault,
                out lFlag);

            mWebCamControl.setCamParametr(
                lindex,
                lCurrentValue,
                lvalue);

        }

        private void mShowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mShowBtn.Content.ToString() == "Show")
            {
                Canvas.SetBottom(mWebCamParametrsPanel, 0);

                mShowBtn.Content = "Hide";
            }
            else if (mShowBtn.Content.ToString() == "Hide")
            {
                Canvas.SetBottom(mWebCamParametrsPanel, -150);

                mShowBtn.Content = "Show";
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mISessions.Count > 0)
            {
                var ltimer = new DispatcherTimer();

                ltimer.Interval = new TimeSpan(0, 0, 0, 1);

                ltimer.Tick += delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content == "Stop")
                    {                        
                        foreach (var item in mISessions)
                        {
                            item.Value.closeSession();
                        }

                        mISessions.Clear();

                        mLaunchButton.Content = "Launch";
                    }
                    
                    Close();

                    (sender1 as DispatcherTimer).Stop();
                };

                ltimer.Start();

                e.Cancel = true;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(mEVROutputNodes != null && mEVROutputNodes.Count > 0)
            {            
                var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

                if (lEVRStreamControl != null)
                {
                            
                    lEVRStreamControl.setSrcPosition(mEVROutputNodes[0],
                    0.0f,
                    (float)e.NewValue,
                    0.0f,
                    (float)e.NewValue);

                }


            }

            
        }
    }
}
