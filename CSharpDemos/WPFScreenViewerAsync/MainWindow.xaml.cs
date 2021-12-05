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

namespace WPFScreenViewerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        IEVRStreamControlAsync mIEVRStreamControl = null;

        ISessionAsync mISession = null;

        object mEVROutputNode = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private DispatcherTimer _ThumbnailTimer = null;
        public DispatcherTimer ThumbnailTimer
        {
            get
            {
                if (_ThumbnailTimer == null)
                {
                    _ThumbnailTimer = new DispatcherTimer(DispatcherPriority.Input);
                    _ThumbnailTimer.Interval = TimeSpan.FromSeconds(1);
                    _ThumbnailTimer.Tick += HandleTick;
                }
                return _ThumbnailTimer;
            }
        }
        private void HandleTick(object sender, EventArgs e)
        {
            //mLaunchButton_Click(null, null);
            //mLaunchButton_Click(null, null);
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            mLaunchButton.IsEnabled = false;

            do
            {

                if (mLaunchButton.Content.ToString() == "Stop")
                {
                    if (mISession != null)
                    {
                        await mISession.stopSessionAsync();

                        await mISession.closeSessionAsync();

                        mLaunchButton.Content = "Launch";
                    }

                    mISession = null;
                    
                    break;
                }

                var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    break;

                var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                uint lStreamIndex = 0;

                lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    break;

                lNode = lSourceNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    break;
                }


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

                var lSinkFactory = await lSinkControl.createEVRSinkFactoryAsync(
                Guid.Empty);

                if (mEVROutputNode == null)
                    mEVROutputNode = await lSinkFactory.createOutputNodeAsync(
                        mVideoPanel.Handle);

                if (mEVROutputNode == null)
                    break;

                var lSourceControl = await mCaptureManager.createSourceControlAsync();

                if (lSourceControl == null)
                    break;

                string lextendSymbolicLink = lSymbolicLink + " --options=" +
                    "<?xml version='1.0' encoding='UTF-8'?>" +
                    "<Options>" +
                        "<Option Type='Cursor' Visiblity='True'>" +
                            "<Option.Extensions>" +
                                "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' Shape='Ellipse'/>" +
                            "</Option.Extensions>" +
                        "</Option>" +
                    "</Options>";

                if ((bool)mNormalizeChkBtn.IsChecked)
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
                    break;

                mISession = await lSessionControl.createSessionAsync(
                    lSourceMediaNodeList.ToArray());

                if (mISession == null)
                    break;

                foreach (var item in lSourceMediaNodeList)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
                }

                await mISession.registerUpdateStateDelegateAsync(UpdateStateDelegate);

                await mISession.startSessionAsync(0, Guid.Empty);

                mLaunchButton.Content = "Stop";


                if (_ThumbnailTimer == null)
                {
                    ThumbnailTimer.Start();
                }

            } while (false);

            mLaunchButton.IsEnabled = true;            
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
                            await mISession.stopSessionAsync();

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

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

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;
                       
            mIEVRStreamControl = await mCaptureManager.createEVRStreamControlAsync();
        }
    }
}
