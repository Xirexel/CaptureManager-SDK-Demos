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

namespace WPFImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static CaptureManager mCaptureManager = null;

        ISourceControl mISourceControl = null;

        ISession mISession = null;
        
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

            mISourceControl = mCaptureManager.createSourceControl();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            if (string.IsNullOrEmpty(lxmldoc))
                return;
        }

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content == "Stop")
            {
                if (mISession != null)
                {
                    mISession.closeSession();

                    mLaunchButton.Content = "Launch";

                    GC.Collect();
                }

                mISession = null;

                return;
            }

            if (mISourceControl == null)
                return;

            var lICaptureProcessor = ImageCaptureProcessor.createCaptureProcessor();

            if (lICaptureProcessor == null)
                return;

            object lMediaSource = null;

            mISourceControl.createSourceFromCaptureProcessor(
                lICaptureProcessor,
                out lMediaSource);

            if (lMediaSource == null)
                return;

            
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



            lSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(
                lMediaSource,
                0,
                0,
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


            mISession.registerUpdateStateDelegate(UpdateStateDelegate);

            mISession.startSession(0, Guid.Empty);

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

                ltimer.Tick += delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content == "Stop")
                    {
                        if (mISession != null)
                        {
                            mISession.closeSession();
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

            GC.Collect();
        }
    }
}
