using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;
using System.Xml;

namespace WPFWebViewerEVRHandlerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        IEVRStreamControlAsync mIEVRStreamControl = null;

        ISessionAsync mISession = null;

        IWebCamControl mWebCamControl;

        object mEVROutputNode = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content == "Stop")
            {
                if (mISession != null)
                {
                    await mISession.closeSessionAsync();

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


            string lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{2F34AF87-D349-45AA-A5F1-E4104D5C458E}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            var lSinkControl = await mCaptureManager.createSinkControlAsync();

            var lSinkFactory = await lSinkControl.createEVRMultiSinkFactoryAsync(
            Guid.Empty);

            if (mEVROutputNode == null)
            {

                List<object> ltemp = await lSinkFactory.createOutputNodesAsync(
                    //IntPtr.Zero,
                    //mVideoPanel.SharedResource.texture,
                    mVideoPanel.Handle,
                    1);

                mEVROutputNode = ltemp[0];
            }

            if (mEVROutputNode == null)
                return;

            var lSourceControl = await mCaptureManager.createSourceControlAsync();

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

            object lPtrSourceNode = await lSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                lextendSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex,
                mEVROutputNode);



            List<object> lSourceMediaNodeList = new List<object>();

            lSourceMediaNodeList.Add(lPtrSourceNode);

            var lSessionControl = await mCaptureManager.createSessionControlAsync();

            if (lSessionControl == null)
                return;

            mISession = await lSessionControl.createSessionAsync(
                lSourceMediaNodeList.ToArray());

            if (mISession == null)
                return;
            
            await mISession.registerUpdateStateDelegateAsync(UpdateStateDelegate);

            await mISession.startSessionAsync(0, Guid.Empty);

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




        private async void mEVRStreamFilterSlider_ValueChanged(object sender, RoutedEventArgs e)
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
               await mIEVRStreamControl.setFilterParametrAsync(
                    mEVROutputNode,
                    lindex,
                    lvalue,
                    true);

        }

        private async void mEVRStreamFilterSlider_Checked(object sender, RoutedEventArgs e)
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
                await mIEVRStreamControl.setFilterParametrAsync(
                    mEVROutputNode,
                    lindex,
                    0,
                    (bool)lCheckBox.IsChecked);

        }



        private async void mEVRStreamOutputFeaturesSlider_ValueChanged(object sender, RoutedEventArgs e)
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
               await  mIEVRStreamControl.setOutputFeatureParametrAsync(
                    mEVROutputNode,
                    lindex,
                    lvalue);

        }

        private async void ShowEVRStream(object sender, RoutedEventArgs e)
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

                string lxmldoc = await mIEVRStreamControl.getCollectionOfFiltersAsync(
                    mEVROutputNode);

                if (string.IsNullOrEmpty(lxmldoc))
                    return;

                doc.LoadXml(lxmldoc);

                lXmlDataProvider.Document = doc;



                lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEVRStreamOutputFeaturesProvider"];

                if (lXmlDataProvider == null)
                    return;


                doc = new System.Xml.XmlDocument();

                lxmldoc = await mIEVRStreamControl.getCollectionOfOutputFeaturesAsync(
                    mEVROutputNode);

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

            System.Runtime.InteropServices.Marshal.AddRef(System.Runtime.InteropServices.Marshal.GetIUnknownForObject(mEVROutputNode));

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

                ltimer.Tick += async delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content == "Stop")
                    {
                        if (mISession != null)
                        {
                            await mISession.closeSessionAsync();
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

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

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

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




            mIEVRStreamControl = await mCaptureManager.createEVRStreamControlAsync();

        }
    }
}
