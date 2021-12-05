using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
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
using System.Windows.Threading;
using System.Xml;

namespace WPFImageViewerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static CaptureManager mCaptureManager = null;

        ISourceControlAsync mISourceControl = null;

        ISessionAsync mISession = null;

        public MainWindow()
        {
            InitializeComponent();
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

            mISourceControl = await mCaptureManager.createSourceControlAsync();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;
        }

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content.ToString() == "Stop")
            {
                if (mISession != null)
                {
                    await mISession.closeSessionAsync();

                    mLaunchButton.Content = "Launch";
                }

                mISession = null;

                return;
            }

            if (mISourceControl == null)
                return;

            var lICaptureProcessor = ImageCaptureProcessor.createCaptureProcessor();

            if (lICaptureProcessor == null)
                return;

            object lMediaSource = await mISourceControl.createSourceFromCaptureProcessorAsync(
                lICaptureProcessor);

            if (lMediaSource == null)
                return;


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
            
            var lSinkFactory = await lSinkControl.createEVRSinkFactoryAsync(
            Guid.Empty);

            object lEVROutputNode = await lSinkFactory.createOutputNodeAsync(
                mVideoPanel.Handle);

            if (lEVROutputNode == null)
                return;

            var lSourceControl = await mCaptureManager.createSourceControlAsync();

            if (lSourceControl == null)
                return;
            
            object lPtrSourceNode = await lSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnectionAsync(
                lMediaSource,
                0,
                0,
                lEVROutputNode);
            
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


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mISession != null)
            {
                var ltimer = new DispatcherTimer();

                ltimer.Interval = new TimeSpan(0, 0, 0, 1);

                ltimer.Tick += async delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content.ToString() == "Stop")
                    {
                        if (mISession != null)
                        {
                            await mISession.closeSessionAsync();
                        }

                        mLaunchButton.Content = "Launch";
                    }

                    mISession = null;

                    Close();

                    (sender1 as DispatcherTimer).Stop();
                };

                ltimer.Start();

                e.Cancel = true;
            }
        }
    }
}
