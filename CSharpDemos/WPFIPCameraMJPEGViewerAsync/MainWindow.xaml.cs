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

namespace WPFIPCameraMJPEGViewerAsync
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

        private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            mLaunchButton.IsEnabled = false;

            do
            {

                if (mLaunchButton.Content == "Stop")
                {
                    if (mISession != null)
                    {
                        await mISession.closeSessionAsync();

                        mLaunchButton.Content = "Launch";
                    }

                    mISession = null;

                    break;
                }

                if (mISourceControl == null)
                    break;

                ICaptureProcessor lICaptureProcessor = null;

                try
                {
                    lICaptureProcessor = IPCameraMJPEGCaptureProcessor.createCaptureProcessor();
                }
                catch (System.Exception exc)
                {
                    MessageBox.Show(exc.Message);

                    break;
                }

                if (lICaptureProcessor == null)
                    break;

                object lMediaSource = await mISourceControl.createSourceFromCaptureProcessorAsync(
                    lICaptureProcessor);

                if (lMediaSource == null)
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

                var lSinkFactory = await lSinkControl.createEVRSinkFactoryAsync(
                Guid.Empty);

                object lEVROutputNode = await lSinkFactory.createOutputNodeAsync(
                    mVideoPanel.Handle);

                if (lEVROutputNode == null)
                    break;

                var lSourceControl = await mCaptureManager.createSourceControlAsync();

                if (lSourceControl == null)
                    break;

                object lPtrSourceNode = await lSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnectionAsync(
                    lMediaSource,
                    0,
                    0,
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


                await mISession.registerUpdateStateDelegateAsync(UpdateStateDelegate);

                await mISession.startSessionAsync(0, Guid.Empty);

                mLaunchButton.Content = "Stop";

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

            mISourceControl = await mCaptureManager.createSourceControlAsync();

        }
    }
}
