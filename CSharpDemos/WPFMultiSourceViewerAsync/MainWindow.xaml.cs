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

namespace WPFMultiSourceViewerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        IDictionary<int, ISessionAsync> mISessions = new Dictionary<int, ISessionAsync>();
        
        List<object> mEVROutputNodes = null;

        uint mStreams = 2;

        int mStreamCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
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
                    await mISessions[lSessionIndex].closeSessionAsync();

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

            string lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{10E52132-A73F-4A9E-A91B-FE18C91D6837}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            var lSinkControl = await mCaptureManager.createSinkControlAsync();

            var lSinkFactory = await lSinkControl.createEVRMultiSinkFactoryAsync(
            Guid.Empty);

            if (mEVROutputNodes == null)
                mEVROutputNodes = await lSinkFactory.createOutputNodesAsync(
                    mVideoPanel.Handle,
                    mStreams);

            if (mEVROutputNodes == null)
                return;

            if (mEVROutputNodes.Count == 0)
                return;

            var lSourceControl = await mCaptureManager.createSourceControlAsync();

            if (lSourceControl == null)
                return;



            object lPtrSourceNode = await lSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                lextendSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex,
                mEVROutputNodes[lSessionIndex]);


            List<object> lSourceMediaNodeList = new List<object>();

            lSourceMediaNodeList.Add(lPtrSourceNode);

            var lSessionControl = await mCaptureManager.createSessionControlAsync();

            if (lSessionControl == null)
                return;

            var lISession = await lSessionControl.createSessionAsync(
                lSourceMediaNodeList.ToArray());

            if (lISession == null)
                return;

            await lISession.registerUpdateStateDelegateAsync(UpdateStateDelegate);

            await lISession.startSessionAsync(0, Guid.Empty);

            mISessions.Add(lSessionIndex, lISession);

            var lEVRStreamControl = await mCaptureManager.createEVRStreamControlAsync();

            if (lEVRStreamControl != null)
            {
                await lEVRStreamControl.setPositionAsync(mEVROutputNodes[lSessionIndex],
                    0.5f * lSessionIndex,
                    0.5f + (0.5f * lSessionIndex),
                    0.5f * lSessionIndex,
                    0.5f + (0.5f * lSessionIndex));


                await lEVRStreamControl.setZOrderAsync(mEVROutputNodes[lSessionIndex],
                    1);
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
               
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mISessions.Count > 0)
            {
                var ltimer = new DispatcherTimer();

                ltimer.Interval = new TimeSpan(0, 0, 0, 1);

                ltimer.Tick += async delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content == "Stop")
                    {
                        foreach (var item in mISessions)
                        {
                            await item.Value.closeSessionAsync();
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
            if (mEVROutputNodes != null && mEVROutputNodes.Count > 0)
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

            string lxmldoc = await  mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

        }
    }
}
