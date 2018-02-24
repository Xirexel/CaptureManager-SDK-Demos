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
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System.Windows.Threading;

namespace WPFScreenStreaming
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CaptureManager mCaptureManager = null;

        ISessionControl mISessionControl = null;

        ISession mISession = null;

        ISinkControl mSinkControl = null;

        ISourceControl mSourceControl = null;

        IEncoderControl mEncoderControl = null;

        NetworkStreamControl mNetworkStreamControl = null;

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
            {
                mCtrlBtn.Content = "Error";

                mCtrlBtn.IsEnabled = false;

                mState.Text = "Library CaptureManager.dll is not acessable";

                return;
            }

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            mSourceControl = mCaptureManager.createSourceControl();

            mEncoderControl = mCaptureManager.createEncoderControl();

            mSinkControl = mCaptureManager.createSinkControl();



            init();
        }

        private void init()
        {


            string lScreenCaptureSymbolicLink = "CaptureManager///Software///Sources///ScreenCapture///ScreenCapture";

            string lAudioLoopBack = "CaptureManager///Software///Sources///AudioEndpointCapture///AudioLoopBack";

            
            // Video Source
            uint lVideoSourceIndexStream = 0;

            uint lVideoSourceIndexMediaType = 2;

            object lVideoSourceOutputMediaType;

            mSourceControl.getSourceOutputMediaType(lScreenCaptureSymbolicLink, lVideoSourceIndexStream, lVideoSourceIndexMediaType, out lVideoSourceOutputMediaType);




            Guid lH264Encoder = new Guid("6ca50344-051a-4ded-9779-a43305165e35");

            IEncoderNodeFactory lIEncoderNodeFactory;

            mEncoderControl.createEncoderNodeFactory(lH264Encoder, out lIEncoderNodeFactory);

            if (lIEncoderNodeFactory == null)
            {
                mCtrlBtn.Content = "Error";

                mCtrlBtn.IsEnabled = false;

                mState.Text = "H264 Encoder is not acessable";

                return;
            }

            object lCompressedMediaType;

            lIEncoderNodeFactory.createCompressedMediaType(
                lVideoSourceOutputMediaType,
                new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55),
                75,
                0,
                out lCompressedMediaType);




            // Audio Source
            uint lAudioSourceIndexStream = 0;

            uint lAudioSourceIndexMediaType = 0;

            object lAudioSourceOutputMediaType;

            mSourceControl.getSourceOutputMediaType(lAudioLoopBack, lAudioSourceIndexStream, lAudioSourceIndexMediaType, out lAudioSourceOutputMediaType);




            Guid lAACEncoder = new Guid("93af0c51-2275-45d2-a35b-f2ba21caed00");

            IEncoderNodeFactory lAACIEncoderNodeFactory;

            mEncoderControl.createEncoderNodeFactory(lAACEncoder, out lAACIEncoderNodeFactory);

            if (lAACIEncoderNodeFactory == null)
            {
                mCtrlBtn.Content = "Error";

                mCtrlBtn.IsEnabled = false;

                mState.Text = "AAC Encoder is not acessable";

                return;
            }

            object lAACCompressedMediaType;

            lAACIEncoderNodeFactory.createCompressedMediaType(
                lAudioSourceOutputMediaType,
                new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55),
                0,
                0,
                out lAACCompressedMediaType);



            IByteStreamSinkFactory lSinkFactory;

            mSinkControl.createSinkFactory(new Guid("E80A6BFD-D9C2-4A1F-95DC-14685CACEF3E"), out lSinkFactory);

            if (lSinkFactory == null)
            {
                mCtrlBtn.Content = "Error";

                mCtrlBtn.IsEnabled = false;

                mState.Text = "MP4 ByteStream is not acessable";

                return;
            }

            mNetworkStreamControl = new NetworkStreamControl(lSinkFactory);

            List<object> lcompressesMediaTypes = new List<object>();

            lcompressesMediaTypes.Add(lAACCompressedMediaType);

            lcompressesMediaTypes.Add(lCompressedMediaType);

            var lOutputNode = mNetworkStreamControl.getOutputNode(
                lcompressesMediaTypes, 
                StartSession, 
                StopSession,
                WriteClientIP);

            object lVideoEncoderNode;

            lIEncoderNodeFactory.createEncoderNode(lVideoSourceOutputMediaType,
                new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55),
                75,
                0,
                lOutputNode[1],
                out lVideoEncoderNode);



            object lAudioEncoderNode;

            lAACIEncoderNodeFactory.createEncoderNode(
                lAudioSourceOutputMediaType,
                new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55),
                75,
                0,
                lOutputNode[0],
                out lAudioEncoderNode);


            object lVideoSourceSourceNode;

            mSourceControl.createSourceNode(
                lScreenCaptureSymbolicLink,
                lVideoSourceIndexStream,
                lVideoSourceIndexMediaType,
                lVideoEncoderNode,
                out lVideoSourceSourceNode);



            object lAudioSourceSourceNode;

            mSourceControl.createSourceNode(
                lAudioLoopBack,
                lAudioSourceIndexStream,
                lAudioSourceIndexMediaType,
                lAudioEncoderNode,
                out lAudioSourceSourceNode);


            mISessionControl = mCaptureManager.createSessionControl();

            object[] lSourceNodes = { lVideoSourceSourceNode, lAudioSourceSourceNode};

            mISession = mISessionControl.createSession(lSourceNodes);

            if (mISession != null)
                mState.Text = "Ready for connection";
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        void StartSession()
        {
            Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() => mISession.startSession(0, Guid.Empty)));

        }

        void StopSession()
        {
            Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        mISession.stopSession();

                        mISession.closeSession();

                        init();
                    }));

        }

        private void mCtrlBtn_Click(object sender, RoutedEventArgs e)
        {

            Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        mISession.stopSession();

                        mISession.closeSession();

                        Close();

                        mState.Text = "Ready for connection";
                    }));
        }

        private void WriteClientIP(string aIPstring)
        {

            Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        mState.Text = "Client IP" + aIPstring;
                    }));
        }
    }
}
