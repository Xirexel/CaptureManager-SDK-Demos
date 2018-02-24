using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace WPFMultiSourceRecorder
{
    delegate void ChangeState();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static event ChangeState mChangeState;

        public static CaptureManager mCaptureManager = null;

        private static List<ISource> mISources = new List<ISource>();

        public static List<ISource> mISourceItems = new List<ISource>();

        

        ISessionControl mISessionControl = null;

        ISession mISession = null;

        ISinkControl mSinkControl = null;

        ISourceControl mSourceControl = null;

        IEncoderControl mEncoderControl = null;

        IStreamControl mStreamControl = null;

        ISpreaderNodeFactory mSpreaderNodeFactory = null;

        IEVRMultiSinkFactory mEVRMultiSinkFactory = null;

        bool mIsStarted = false;

        string mFilename = "";

        public MainWindow()
        {
            InitializeComponent();

            mChangeState += MainWindow_mChangeState;
        }

        void MainWindow_mChangeState()
        {
            if(mISources.Count > 0)
            {
                mSelectFileBtn.IsEnabled = true;
            }
            else
            {
                mSelectFileBtn.IsEnabled = false;

                m_StartStopBtn.IsEnabled = false;
            }
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            if (mCaptureManager == null)
                return;



            mSourceControl = mCaptureManager.createSourceControl();

            if (mSourceControl == null)
                return;

            mEncoderControl = mCaptureManager.createEncoderControl();

            if (mEncoderControl == null)
                return;

            mSinkControl = mCaptureManager.createSinkControl();

            if (mSinkControl == null)
                return;

            mISessionControl = mCaptureManager.createSessionControl();

            if (mISessionControl == null)
                return;

            mStreamControl = mCaptureManager.createStreamControl();

            if (mStreamControl == null)
                return;

            mStreamControl.createStreamControlNodeFactory(ref mSpreaderNodeFactory);

            if (mSpreaderNodeFactory == null)
                return;

            mSinkControl.createSinkFactory(Guid.Empty, out mEVRMultiSinkFactory);

            if (mEVRMultiSinkFactory == null)
                return;


            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            //lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

            //if (lXmlDataProvider == null)
            //    return;

        }

        public static void addSourceControl(ISource aISource)
        {
            mISources.Add(aISource);

            if (mChangeState != null)
                mChangeState();
        }

        public static void removeSourceControl(ISource aISource)
        {
            mISources.Remove(aISource);

            if (mChangeState != null)
                mChangeState();
        }

        private void mControlBtn_Click(object sender, RoutedEventArgs e)
        {
            if(mIsStarted)
            {
                mIsStarted = false;

                if (mISession == null)
                    return;

                mISession.stopSession();

                mISession.closeSession();

                mISession = null;

                m_StartStopBtn.Content = "Start";

                foreach (var item in mISourceItems)
                {
                    var lsourceitem = (ISource)item;

                    if (lsourceitem != null)
                        lsourceitem.access(true);
                }
                
                return;
            }
            else
            {

                IFileSinkFactory lFileSinkFactory = null;
                
                mSinkControl.createSinkFactory(
                    Guid.Parse("A2A56DA1-EB84-460E-9F05-FEE51D8C81E3"),
                    out lFileSinkFactory);

                if (lFileSinkFactory == null)
                    return;

                List<object> lCompressedMediaTypeList = new List<object>();

                foreach(var item in mISources)
                {
                    var lCompressedMediaType = item.getCompressedMediaType();

                    if (lCompressedMediaType != null)
                        lCompressedMediaTypeList.Add(lCompressedMediaType);
                }

                List<object> lOutputNodes = getOutputNodes(lCompressedMediaTypeList, lFileSinkFactory);
                
                if (lOutputNodes == null || lOutputNodes.Count == 0)
                    return;

                int lOutputIndex = 0;

                List<object> lSourceNodes = new List<object>();

                for (int i = 0; i < lOutputNodes.Count; i++)
                {
                    var lSourceNode = mISources[i].getSourceNode(lOutputNodes[i]);

                    if (lSourceNode != null)
                        lSourceNodes.Add(lSourceNode);
                }

                mISession = mISessionControl.createSession(lSourceNodes.ToArray());

                if (mISession == null)
                    return;

                if (mISession.startSession(0, Guid.Empty))
                {
                    m_StartStopBtn.Content = "Stop";
                }

                mIsStarted = true;

                foreach (var item in mISourceItems)
                {
                    var lsourceitem = (ISource)item;

                    if (lsourceitem != null)
                        lsourceitem.access(false);
                }

            }
        }



        private List<object> getOutputNodes(List<object> aCompressedMediaTypeList, IFileSinkFactory aFileSinkFactory)
        {
            List<object> lresult = new List<object>();

            do
            {
                if (aCompressedMediaTypeList == null)
                    break;

                if (aCompressedMediaTypeList.Count == 0)
                    break;

                if (aFileSinkFactory == null)
                    break;

                if (string.IsNullOrEmpty(mFilename))
                    break;

                aFileSinkFactory.createOutputNodes(
                    aCompressedMediaTypeList,
                    mFilename,
                    out lresult);

            } while (false);

            return lresult;
        }

        private void mSelectFileBtn_Click(object sender, RoutedEventArgs e)
        {

            do
            {
    
                String limageSourceDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                SaveFileDialog lsaveFileDialog = new SaveFileDialog();

                lsaveFileDialog.InitialDirectory = limageSourceDir;

                lsaveFileDialog.DefaultExt = ".asf";

                lsaveFileDialog.AddExtension = true;

                lsaveFileDialog.CheckFileExists = false;

                lsaveFileDialog.Filter = "Media file (*.asf)|*.asf" ;

                var lresult = lsaveFileDialog.ShowDialog();

                if (lresult != true)
                    break;

                mFilename = lsaveFileDialog.FileName;
                
                m_StartStopBtn.IsEnabled = true;

            } while (false);
        }
    }
}
