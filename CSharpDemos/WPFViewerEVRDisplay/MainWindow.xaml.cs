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
using System.Xml;

namespace WPFViewerEVRDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        IDictionary<int, ISession> mISessions = new Dictionary<int, ISession>();
        
        List<object> mEVROutputNodes = null;

        uint mStreams = 1;

        public MainWindow()
        {
            InitializeComponent();

            m_Positioner.mCallback += m_Positioner_mCallback;

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


        }

        float mLeft = 0.0f;

        float mTop = 0.0f;

        void m_Positioner_mCallback(float aLeftNewValue, float aTopNewValue)
        {
            mLeft = aLeftNewValue;

            mTop = aTopNewValue;

            updatePos();
        }

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {

            //var lTuple = D3D9Image.createD3D9Image();

            //Interop.Direct3DSurface9 surface = null;

            //if (lTuple != null)
            //{
            //    this.Background = new ImageBrush(lTuple.Item1);

            //    this.surface = lTuple.Item2;
            //}


            var lButton = (Button)sender;

            if (lButton == null)
                return;
            
            int lSessionIndex = 0;

            if (lButton.Content == "Stop")
            {
                if (mISessions.ContainsKey(lSessionIndex))
                {
                    mISessions[lSessionIndex].closeSession();
                    
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
                    IntPtr.Zero,
                    m_EVRDisplay.Surface.texture,
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

            //if (lEVRStreamControl != null)
            //{
            //    lEVRStreamControl.setPosition(mEVROutputNodes[lSessionIndex],
            //        0.5f * lSessionIndex,
            //        0.5f + (0.5f * lSessionIndex),
            //        0.5f * lSessionIndex,
            //        0.5f + (0.5f * lSessionIndex));


            //    lEVRStreamControl.setZOrder(mEVROutputNodes[lSessionIndex],
            //        1);

            //    float lLeft;
            //    float lRight;
            //    float lTop;
            //    float lBottom;

            //    lEVRStreamControl.getPosition(mEVROutputNodes[lSessionIndex],
            //    out lLeft,
            //    out lRight,
            //    out lTop,
            //    out lBottom);

            //    uint lZOrder;

            //    lEVRStreamControl.getZOrder(mEVROutputNodes[lSessionIndex],
            //        out lZOrder);
            //}

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

        float mScale = 1.0f;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mScale = (float)e.NewValue;

            updatePos();
        }

        private void updatePos()
        {

            if (mEVROutputNodes != null && mEVROutputNodes.Count > 0)
            {
                var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

                if (lEVRStreamControl != null)
                {
                    lEVRStreamControl.setSrcPosition(mEVROutputNodes[0],
                    mLeft,
                    mLeft + mScale,
                    mTop,
                    mTop + mScale);
                }
            }

        }
    }
}
