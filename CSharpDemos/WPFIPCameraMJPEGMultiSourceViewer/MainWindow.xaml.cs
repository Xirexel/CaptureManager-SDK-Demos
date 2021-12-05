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

namespace WPFIPCameraMJPEGMultiSourceViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CaptureManager mCaptureManager = null;

        IDictionary<int, ISession> mISessions = new Dictionary<int, ISession>();

        List<object> mEVROutputNodes = null;

        ISourceControl mSourceControl = null;

        string[] URLs = {
                            //"http://144.139.80.151:8080/mjpg/video.mjpg", 
                            "http://mx.cafesydney.com:8888/mjpg/video.mjpg",
                            "http://mx.cafesydney.com:8888/mjpg/video.mjpg"
                        };

        uint mStreams = 1;

        int mColumnMax = 4;

        int mColumnCurrent = 0;

        int mRowCurrent = 0;

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

            mSourceControl = mCaptureManager.createSourceControl();

            if (mSourceControl == null)
                return;

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

            var lMaxPortCountAttr = lContainerNode.Attributes["MaxPortCount"];

            if (lMaxPortCountAttr == null || string.IsNullOrEmpty(lMaxPortCountAttr.Value))
                return;

            uint lValue = 0;

            if(uint.TryParse(lMaxPortCountAttr.Value, out lValue))
            {
                mStreams = lValue;
            }



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

            if (mEVROutputNodes == null || mEVROutputNodes.Count == 0)
                return;
            
            initEVRStreams();
        }

        private void initEVRStreams()
        {
            mRowCurrent = ((int)mStreams / mColumnMax) + 1;

            mColumnCurrent = mRowCurrent > 1 ? (int)mColumnMax : (int)mStreams;

            for (int i = 0; i < mRowCurrent; i++)
            {
                mControlGrid.RowDefinitions.Add(new RowDefinition());                
            }

            for (int i = 0; i < mColumnCurrent; i++)
            {
                mControlGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }


            var lEVRStreamControl = mCaptureManager.createEVRStreamControl();
                        
            for (int i = 0; i < mStreams; i++)
            {
                int lRow = i / mColumnCurrent;

                Border lBorder = new Border();

                lBorder.BorderBrush = Brushes.Red;

                lBorder.BorderThickness = new Thickness(2);

                mControlGrid.Children.Add(lBorder);

                var lButton = new Button();
                
                lButton.Width = 80;

                lButton.Height = 80;

                lButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                lButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                lButton.Click += lButton_Click;

                lButton.Tag = i;

                lButton.Content = "Start";

                lButton.FontSize = 30;

                mControlGrid.Children.Add(lButton);

                int lColomnIndex = i % (int)mColumnCurrent;

                Grid.SetColumn(lButton, lColomnIndex);

                Grid.SetRow(lButton, lRow);
                
                Grid.SetColumn(lBorder, lColomnIndex);

                Grid.SetRow(lBorder, lRow);


                if (lEVRStreamControl != null)
                {
                    lEVRStreamControl.setPosition(mEVROutputNodes[i],
                        (float)lColomnIndex / (float)mColumnCurrent,
                        (float)(lColomnIndex + 1) / (float)mColumnCurrent,
                        (float)lRow / (float)mRowCurrent,
                        (float)(lRow + 1) / (float)mRowCurrent);
                }
            }
        }

        private void lButton_Click(object sender, RoutedEventArgs e)
        {
            var lButton = (Button)sender;

            do
            {
                if (mSourceControl == null)
                    return;

                if(mISessions == null)
                    break;

                if(lButton.Tag == null)
                    break;

                int lIndex = (int)lButton.Tag;

                if (lButton.Content == "Stop")
                {
                    if(mISessions.ContainsKey(lIndex))
                    {
                        mISessions[lIndex].closeSession();
                        
                        mISessions.Remove(lIndex);

                        lButton.Content = "Start";
                    }
                    
                    break;
                }
                                                
                ICaptureProcessor lICaptureProcessor = null;

                try
                {
                    lICaptureProcessor = IPCameraMJPEGCaptureProcessor.createCaptureProcessor(URLs[lIndex % 2]);
                }
                catch (System.Exception exc)
                {
                    MessageBox.Show(exc.Message);

                    return;
                }

                if (lICaptureProcessor == null)
                    return;

                object lMediaSource = null;

                mSourceControl.createSourceFromCaptureProcessor(
                    lICaptureProcessor,
                    out lMediaSource);

                if (lMediaSource == null)
                    return;
                
               

                object lPtrSourceNode;

                var lSourceControl = mCaptureManager.createSourceControl();

                if (lSourceControl == null)
                    return;
                
                lSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(
                    lMediaSource,
                    0,
                    0,
                    mEVROutputNodes[lIndex],
                    out lPtrSourceNode);


                List<object> lSourceMediaNodeList = new List<object>();

                lSourceMediaNodeList.Add(lPtrSourceNode);

                var lSessionControl = mCaptureManager.createSessionControl();

                if (lSessionControl == null)
                    return;

               var lISession = lSessionControl.createSession(
                    lSourceMediaNodeList.ToArray());

               if (lISession == null)
                    return;


               lISession.registerUpdateStateDelegate(UpdateStateDelegate);

               lISession.startSession(0, Guid.Empty);

               mISessions[lIndex] = lISession;

               lButton.Content = "Stop";
                
            } while (false);
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
            foreach (var item in mISessions)
            {
                item.Value.closeSession();
            }
        }
    }
}
