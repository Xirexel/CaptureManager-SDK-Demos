using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Xml;

namespace WPFMultiSourceRecorderAsync
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



        ISessionControlAsync mISessionControl = null;

        ISessionAsync mISession = null;

        ISinkControlAsync mSinkControl = null;

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;

        IStreamControlAsync mStreamControl = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;

        IEVRMultiSinkFactoryAsync mEVRMultiSinkFactory = null;

        bool mIsStarted = false;

        string mFilename = "";

        public MainWindow()
        {
            InitializeComponent();

            mChangeState += MainWindow_mChangeState;
        }

        void MainWindow_mChangeState()
        {
            if (mISources.Count > 0)
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

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            if (mCaptureManager == null)
                return;



            mSourceControl = await mCaptureManager.createSourceControlAsync();

            if (mSourceControl == null)
                return;

            mEncoderControl = await mCaptureManager.createEncoderControlAsync();

            if (mEncoderControl == null)
                return;

            mSinkControl = await mCaptureManager.createSinkControlAsync();

            if (mSinkControl == null)
                return;

            mISessionControl = await mCaptureManager.createSessionControlAsync();

            if (mISessionControl == null)
                return;

            mStreamControl = await mCaptureManager.createStreamControlAsync();

            if (mStreamControl == null)
                return;

            mSpreaderNodeFactory = await mStreamControl.createSpreaderNodeFactoryAsync();

            if (mSpreaderNodeFactory == null)
                return;

            mEVRMultiSinkFactory = await mSinkControl.createEVRMultiSinkFactoryAsync(Guid.Empty);

            if (mEVRMultiSinkFactory == null)
                return;


            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

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

        private async void mControlBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mIsStarted)
            {
                mIsStarted = false;

                if (mISession == null)
                    return;

                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

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

                var lFileSinkFactory = await mSinkControl.createFileSinkFactoryAsync(
                    Guid.Parse("A2A56DA1-EB84-460E-9F05-FEE51D8C81E3"));

                if (lFileSinkFactory == null)
                    return;

                List<object> lCompressedMediaTypeList = new List<object>();

                foreach (var item in mISources)
                {
                    var lCompressedMediaType = await item.getCompressedMediaType();

                    if (lCompressedMediaType != null)
                        lCompressedMediaTypeList.Add(lCompressedMediaType);
                }

                List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList, lFileSinkFactory);

                if (lOutputNodes == null || lOutputNodes.Count == 0)
                    return;

                List<object> lSourceNodes = new List<object>();

                for (int i = 0; i < lOutputNodes.Count; i++)
                {
                    var lSourceNode = await mISources[i].getSourceNode(lOutputNodes[i]);

                    if (lSourceNode != null)
                        lSourceNodes.Add(lSourceNode);
                }

                mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

                if (mISession == null)
                    return;

                if (await mISession.startSessionAsync(0, Guid.Empty))
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



        private async Task<List<object>> getOutputNodes(List<object> aCompressedMediaTypeList, IFileSinkFactoryAsync aFileSinkFactory)
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

                lresult = await aFileSinkFactory.createOutputNodesAsync(
                    aCompressedMediaTypeList,
                    mFilename);

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

                lsaveFileDialog.Filter = "Media file (*.asf)|*.asf";

                var lresult = lsaveFileDialog.ShowDialog();

                if (lresult != true)
                    break;

                mFilename = lsaveFileDialog.FileName;

                m_StartStopBtn.IsEnabled = true;

            } while (false);
        }
    }
}
