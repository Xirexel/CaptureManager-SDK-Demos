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

namespace WPFWebViewerEVR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CaptureManager mCaptureManager = null;

        IEVRStreamControl mIEVRStreamControl = null;

        ISession mISession = null;
        
        object mEVROutputNode = null;

        public MainWindow()
        {
            InitializeComponent();

            LogManager.getInstance().WriteLogDelegateEvent += MainWindow_WriteLogDelegateEvent;

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




            mEVRStreamFiltersTabItem.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_ValueChanged));

            mEVRStreamFiltersTabItem.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_Checked));

            mEVRStreamFiltersTabItem.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_Checked));





            mEVRStreamOutputFeaturesTabItem.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mEVRStreamOutputFeaturesSlider_ValueChanged));
            



            mIEVRStreamControl = mCaptureManager.createEVRStreamControl();

        }

        private void MainWindow_WriteLogDelegateEvent(CaptureManagerToCSharpProxy.WrapClasses.LogLevel aLogLevel, string aPtrLogString)
        {
            switch (aLogLevel)
            {
                case CaptureManagerToCSharpProxy.WrapClasses.LogLevel.INFO_LEVEL:
                    log.Info(aPtrLogString);
                    break;
                case CaptureManagerToCSharpProxy.WrapClasses.LogLevel.ERROR_LEVEL:
                    log.Error(aPtrLogString);
                    break;
                default:
                    break;
            }


        }

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content.ToString() == "Stop")
            {
                if (mISession != null)
                {
                    mISession.closeSession();

                    mLaunchButton.Content = "Launch";
                }

                mISession = null;

                //mEVROutputNode = null;

                return;
            }

            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;

            lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null)
                return;

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex))
            {
                return;
            }

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

            if (mEVROutputNode == null)
                lSinkFactory.createOutputNode(
                    mVideoPanel.Handle,
                    out mEVROutputNode);

            if (mEVROutputNode == null)
                return;

            object lPtrSourceNode;

            var lSourceControl = mCaptureManager.createSourceControl();

            if (lSourceControl == null)
                return;

            string lextendSymbolicLink = lSymbolicLink + " --options=" +
                "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Options>" +
                    "<Option Type='Cursor' Visiblity='True'>" +
                        "<Option.Extensions>" +
                            "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
                        "</Option.Extensions>" +
                    "</Option>" +
                "</Options>";


            lextendSymbolicLink += " --normalize=Landscape";

            lSourceControl.createSourceNode(
                lextendSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex,
                mEVROutputNode,
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


            mISession.registerUpdateStateDelegate(UpdateStateDelegate);
            
            var lf = mISession.startSession(0, Guid.Empty);

            mLaunchButton.Content = "Stop";

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


                        Dispatcher.Invoke(
                        DispatcherPriority.Normal,
                        new Action(() => mLaunchButton_Click(null, null)));

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
        }

        private void mParametrSlider_Checked(object sender, RoutedEventArgs e)
        {
        }




        private void mEVRStreamFilterSlider_ValueChanged(object sender, RoutedEventArgs e)
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


            lAttr = lParametrNode.Attributes["Multiplier"];

            if (lAttr == null)
                return;

            var lMultiplier = double.Parse(lAttr.Value);            

            //int lvalue = (int)(lslider.Value * lMultiplier);

            int lvalue = (int)lslider.Value;

            if (mIEVRStreamControl != null)
            mIEVRStreamControl.setFilterParametr(
                mEVROutputNode,
                lindex,
                lvalue,
                true);

        }

        private void mEVRStreamFilterSlider_Checked(object sender, RoutedEventArgs e)
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
                        
            if (mIEVRStreamControl != null)
                mIEVRStreamControl.setFilterParametr(
                    mEVROutputNode,
                    lindex,
                    0,
                    (bool)lCheckBox.IsChecked);

        }



        private void mEVRStreamOutputFeaturesSlider_ValueChanged(object sender, RoutedEventArgs e)
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

            if (mIEVRStreamControl != null)
                mIEVRStreamControl.setOutputFeatureParametr(
                    mEVROutputNode,
                    lindex,
                    lvalue);

        }

        private void ShowEVRStream(object sender, RoutedEventArgs e)
        {
            if (mShowBtn.Content.ToString() == "Show")
            {
                mEVRStreamParametrsTab.IsEnabled = true;

                mShowBtn.Content = "Hide";
                
                if (mIEVRStreamControl == null)
                    return;


                XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEVRStreamFiltersProvider"];

                if (lXmlDataProvider == null)
                    return;


                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

                string lxmldoc = "";

                mIEVRStreamControl.getCollectionOfFilters(
                    mEVROutputNode,
                    out lxmldoc);
                
                if (string.IsNullOrEmpty(lxmldoc))
                    return;

                doc.LoadXml(lxmldoc);

                lXmlDataProvider.Document = doc;



                lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEVRStreamOutputFeaturesProvider"];

                if (lXmlDataProvider == null)
                    return;


                doc = new System.Xml.XmlDocument();

                lxmldoc = "";

                mIEVRStreamControl.getCollectionOfOutputFeatures(
                    mEVROutputNode,
                    out lxmldoc);

                if (string.IsNullOrEmpty(lxmldoc))
                    return;

                doc.LoadXml(lxmldoc);

                lXmlDataProvider.Document = doc;

            }
            else if (mShowBtn.Content.ToString() == "Hide")
            {
                mEVRStreamParametrsTab.IsEnabled = false;

                mShowBtn.Content = "Show";
            }
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
            if (mISession != null)
            {
                var ltimer = new DispatcherTimer();

                ltimer.Interval = new TimeSpan(0, 0, 0, 1);

                ltimer.Tick += delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content.ToString() == "Stop")
                    {
                        if (mISession != null)
                        {
                            mISession.closeSession();
                        }

                        mLaunchButton.Content = "Launch";
                    }

                    mISession = null;

                    mEVROutputNode = null;

                    Close();

                    (sender1 as DispatcherTimer).Stop();
                };

                ltimer.Start();

                e.Cancel = true;
            }
        }

        private void mOpacitySlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (mIEVRStreamControl != null)
            //    mIEVRStreamControl.setFilterParametr(
            //        mEVROutputNode,
            //        lindex,
            //        lvalue,
            //        true);
        }
    }
}
