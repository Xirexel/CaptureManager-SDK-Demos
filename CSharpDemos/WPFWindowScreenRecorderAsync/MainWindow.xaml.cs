using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

namespace WPFWindowScreenRecorderAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static CaptureManager mCaptureManager = null;

        ISessionControlAsync mISessionControl = null;

        ISessionAsync mISession = null;

        ISinkControlAsync mSinkControl = null;

        ISourceControlAsync mSourceControl = null;

        IEncoderControlAsync mEncoderControl = null;

        IFileSinkFactoryAsync mFileSinkFactory = null;

        IStreamControlAsync mStreamControl = null;

        ISpreaderNodeFactoryAsync mSpreaderNodeFactory = null;

        IEVRMultiSinkFactoryAsync mEVRMultiSinkFactory = null;

        string mSymbolicLink = "CaptureManager///Software///Sources///ScreenCapture///ScreenCapture";

        enum State
        {
            Stopped, Started, Paused
        }

        State mState = State.Stopped;

        IntPtr m_CurrentHWND = IntPtr.Zero;

        string mFilename = null;

        public MainWindow()
        {
            InitializeComponent();

            SelectWindow.m_updateWindowNameEvent += setWindowName;

            SelectWindow.m_pressedKey += SelectWindow_m_pressedKey;
        }

        private string getSymbolicLink()
        {
            string lextendSymbolicLink = mSymbolicLink;

            if (mImageShapeComBx.SelectedIndex > 0 || mOptionType.SelectedIndex > 0)
            {
                string loptions = " --options=" +
                                    "<?xml version='1.0' encoding='UTF-8'?>" +
                                    "<Options>";

                if (mImageShapeComBx.SelectedIndex > 0)
                {
                    var litem = (ComboBoxItem)mImageShapeComBx.SelectedItem;

                    if (litem != null && litem.Content != null)
                    {
                        string lcursorOption =
                        "<Option Type='Cursor' Visiblity='True'>" +
                            "<Option.Extensions>" +
                                "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' Shape='Temp_Shape' />" +
                            "</Option.Extensions>" +
                        "</Option>";

                        loptions += lcursorOption.Replace("Temp_Shape", litem.Content.ToString());
                    }
                }

                if (mOptionType.SelectedIndex > 0)
                {

                    int lLeft = 0, lTop = 0, lWidth = 0, lHeight = 0;

                    int lValue = 0;

                    if (int.TryParse(mLeftTxtBx.Text, out lValue))
                    {
                        lLeft = (lValue >> 1) << 1;
                    }

                    if (int.TryParse(mTopTxtBx.Text, out lValue))
                    {
                        lTop = (lValue >> 1) << 1;
                    }

                    if (int.TryParse(mWidthTxtBx.Text, out lValue))
                    {
                        lWidth = (lValue >> 1) << 1;
                    }

                    if (int.TryParse(mHeightTxtBx.Text, out lValue))
                    {
                        lHeight = (lValue >> 1) << 1;
                    }

                    var litem = (ComboBoxItem)mOptionType.SelectedItem;

                    if (litem != null && litem.Content != null)
                    {
                        string lcursorOption =
                        "<Option Type='Temp_Type'>" +
                            "<Option.Extensions>" +
                                "<Extension Left='Temp_Left' Top='Temp_Top' Height='Temp_Height' Width='Temp_Width'/>" +
                            "</Option.Extensions>" +
                        "</Option>";

                        loptions += lcursorOption.Replace("Temp_Type", litem.Content.ToString())
                            .Replace("Temp_Left", lLeft.ToString())
                            .Replace("Temp_Top", lTop.ToString())
                            .Replace("Temp_Height", lHeight.ToString())
                            .Replace("Temp_Width", lWidth.ToString());
                    }
                }

                loptions += "</Options>";

                lextendSymbolicLink += loptions;
            }



            if (m_CurrentHWND != IntPtr.Zero)
            {
                lextendSymbolicLink += " --HWND=" + m_CurrentHWND.ToInt32().ToString();
            }

            return lextendSymbolicLink;
        }

        void SelectWindow_m_pressedKey(char a_KeyChar)
        {
            if (a_KeyChar == 'l')
            {
                Dispatcher.Invoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    SelectWindow.getInstance().uninstallMouseHook();

                    m_ClickTgglBtn.IsChecked = false;

                    m_ClickTgglBtn.Content = "Select by click";

                    fillmediaTypes(null, null);
                })
                );
            }
        }

        private void fillmediaTypes(object sender, RoutedEventArgs e)
        {
            if (m_CurrentHWND == IntPtr.Zero)
                return;

            m_VideoSourceMediaTypeComboBox.Items.Clear();

            m_VideoSourceMediaTypeComboBox.IsEnabled = true;

            NativeMethods.RECT l_Rect = new NativeMethods.RECT();

            IntPtr buf = Marshal.AllocHGlobal(
            Marshal.SizeOf(typeof(NativeMethods.RECT)));

            {
                var h = NativeMethods.GetClientRect(m_CurrentHWND, buf);

                l_Rect = (NativeMethods.RECT)Marshal.PtrToStructure(buf, typeof(NativeMethods.RECT));

                Marshal.FreeHGlobal(buf);

                for (int i = 0; i < 7; i++)
                {
                    var l_sizeString = String.Format("{0} x {1}, FPS {2}",
                        l_Rect.right - l_Rect.left,
                        l_Rect.bottom - l_Rect.top,
                        i == 0 ? 1 : i * 5);

                    m_VideoSourceMediaTypeComboBox.Items.Add(l_sizeString);
                }
            }

            m_VideoSourceMediaTypeComboBox.SelectedIndex = -1;
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        public void setWindowName(string a_Name, IntPtr a_HWND)
        {
            Dispatcher.Invoke(
            DispatcherPriority.Normal,
            new Action(() => {

                mWindowNameTxtBlk.Text = a_Name;

                m_CurrentHWND = a_HWND;
            })
            );

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

            string lxmldoc =  await mCaptureManager.getCollectionOfSourcesAsync();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            lxmldoc = await mCaptureManager.getCollectionOfEncodersAsync();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;




            lxmldoc = await mCaptureManager.getCollectionOfSinksAsync();


            lXmlDataProvider = (XmlDataProvider)this.Resources["XmlContainerTypeProvider"];

            if (lXmlDataProvider == null)
                return;

            doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;


            ProcessPriorityClass lpriority = Process.GetCurrentProcess().PriorityClass;

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

        }

        private async void m_VideoEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {
                if (mEncoderControl == null)
                    break;

                var lselectedNode = m_VideoEncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

                if (lCLSIDEncoderAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                    break;

                if (mSourceControl == null)
                    return;

                uint lStreamIndex = 0;

                if (m_VideoSourceMediaTypeComboBox.SelectedIndex < 0)
                    break;

                uint lMediaTypeIndex = (uint)m_VideoSourceMediaTypeComboBox.SelectedIndex;

                object lOutputMediaType;

                lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    getSymbolicLink(),
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lOutputMediaType == null)
                    break;

                string lMediaTypeCollection = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                    lOutputMediaType,
                    lCLSIDEncoder);

                if (string.IsNullOrWhiteSpace(lMediaTypeCollection))
                    break;



                XmlDataProvider lXmlEncoderModeDataProvider = (XmlDataProvider)this.Resources["XmlEncoderModeProvider"];

                if (lXmlEncoderModeDataProvider == null)
                    return;

                XmlDocument lEncoderModedoc = new XmlDocument();

                lEncoderModedoc.LoadXml(lMediaTypeCollection);

                lXmlEncoderModeDataProvider.Document = lEncoderModedoc;


            } while (false);
        }

        private async void m_AudioEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            do
            {
                if (mEncoderControl == null)
                    break;

                var lselectedNode = m_AudioEncodersComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

                if (lCLSIDEncoderAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder))
                    break;



                var lSourceNode = m_AudioSourceComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    return;

                var lNode = lSourceNode.SelectSingleNode(
            "Source.Attributes/Attribute" +
            "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
            "/SingleValue/@Value");

                if (lNode == null)
                    return;

                string lSymbolicLink = lNode.Value;

                lSourceNode = m_AudioStreamComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    return;

                lNode = lSourceNode.SelectSingleNode("@Index");

                if (lNode == null)
                    return;

                uint lStreamIndex = 0;

                if (!uint.TryParse(lNode.Value, out lStreamIndex))
                {
                    return;
                }

                lSourceNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;

                if (lSourceNode == null)
                    return;

                lNode = lSourceNode.SelectSingleNode("@Index");

                if (lNode == null)
                    return;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    return;
                }
                                             
                if (mSourceControl == null)
                    return;

                object lOutputMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lOutputMediaType == null)
                    return;

                string lMediaTypeCollection = await mEncoderControl.getMediaTypeCollectionOfEncoderAsync(
                    lOutputMediaType,
                    lCLSIDEncoder);

                if (string.IsNullOrWhiteSpace(lMediaTypeCollection))
                    break;



                XmlDataProvider lXmlEncoderModeDataProvider = (XmlDataProvider)this.Resources["XmlAudioEncoderModeProvider"];

                if (lXmlEncoderModeDataProvider == null)
                    return;

                XmlDocument lEncoderModedoc = new XmlDocument();

                lEncoderModedoc.LoadXml(lMediaTypeCollection);

                lXmlEncoderModeDataProvider.Document = lEncoderModedoc;


            } while (false);
        }

        private async void m_SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                var lselectedNode = m_FileFormatComboBox.SelectedItem as XmlNode;

                if (lselectedNode == null)
                    break;

                var lSelectedAttr = lselectedNode.Attributes["Value"];

                if (lSelectedAttr == null)
                    break;

                String limageSourceDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                SaveFileDialog lsaveFileDialog = new SaveFileDialog();

                lsaveFileDialog.InitialDirectory = limageSourceDir;

                lsaveFileDialog.DefaultExt = "." + lSelectedAttr.Value.ToLower();

                lsaveFileDialog.AddExtension = true;

                lsaveFileDialog.CheckFileExists = false;

                lsaveFileDialog.Filter = "Media file (*." + lSelectedAttr.Value.ToLower() + ")|*." + lSelectedAttr.Value.ToLower();

                var lresult = lsaveFileDialog.ShowDialog();

                if (lresult != true)
                    break;

                mFilename = lsaveFileDialog.FileName;

                lSelectedAttr = lselectedNode.Attributes["GUID"];

                if (lSelectedAttr == null)
                    break;

                mFileSinkFactory = await mSinkControl.createFileSinkFactoryAsync(Guid.Parse(lSelectedAttr.Value));

                m_StartStopBtn.IsEnabled = true;

            } while (false);

        }

        private async void m_StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mState == State.Paused && mISession != null)
            {
                await mISession.startSessionAsync(0, Guid.Empty);

                mState = State.Started;

                m_PauseBtn.IsEnabled = true;

                m_StopBtn.IsEnabled = true;

                m_StartStopBtn.IsEnabled = false;

                return;
            }


            List<object> lCompressedMediaTypeList = new List<object>();

            if ((bool)m_VideoStreamChkBtn.IsChecked)
            {
                object lCompressedMediaType = await getCompressedMediaType(
                    getSymbolicLink(),
                    0,
                    m_VideoSourceMediaTypeComboBox.SelectedIndex,
                    m_VideoEncodersComboBox.SelectedItem as XmlNode,
                    m_VideoEncodingModeComboBox.SelectedItem as XmlNode,
                    m_VideoCompressedMediaTypesComboBox.SelectedIndex);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }

            if ((bool)m_AudioStreamChkBtn.IsChecked)
            {
                object lCompressedMediaType = await getCompressedMediaType(
                    m_AudioSourceComboBox.SelectedItem as XmlNode,
                    m_AudioStreamComboBox.SelectedItem as XmlNode,
                    m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode,
                    m_AudioEncodersComboBox.SelectedItem as XmlNode,
                    m_AudioEncodingModeComboBox.SelectedItem as XmlNode,
                    m_AudioCompressedMediaTypesComboBox.SelectedIndex);

                if (lCompressedMediaType != null)
                    lCompressedMediaTypeList.Add(lCompressedMediaType);
            }

            List<object> lOutputNodes = await getOutputNodes(lCompressedMediaTypeList);

            if (lOutputNodes == null || lOutputNodes.Count == 0)
                return;

            int lOutputIndex = 0;

            List<object> lSourceNodes = new List<object>();

            if ((bool)m_VideoStreamChkBtn.IsChecked && m_VideoCompressedMediaTypesComboBox.SelectedIndex > -1)
            {
                object RenderNode = null;

                if ((bool)m_VideoStreamPreviewChkBtn.IsChecked)
                {
                    List<object> lRenderOutputNodesList = new List<object>();

                    if (mEVRMultiSinkFactory != null)
                        lRenderOutputNodesList = await mEVRMultiSinkFactory.createOutputNodesAsync(
                            IntPtr.Zero,
                            m_EVRDisplay.Surface.texture,
                            1);

                    if (lRenderOutputNodesList.Count == 1)
                    {
                        RenderNode = lRenderOutputNodesList[0];
                    }
                }



                object lSourceNode = await getSourceNode(
                    getSymbolicLink(),
                    0,
                    m_VideoSourceMediaTypeComboBox.SelectedIndex,
                    m_VideoEncodersComboBox.SelectedItem as XmlNode,
                    m_VideoEncodingModeComboBox.SelectedItem as XmlNode,
                    m_VideoCompressedMediaTypesComboBox.SelectedIndex,
                    RenderNode,
                    lOutputNodes[lOutputIndex++]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }

            if ((bool)m_AudioStreamChkBtn.IsChecked && m_AudioCompressedMediaTypesComboBox.SelectedIndex > -1)
            {
                object lSourceNode = await getSourceNode(
                    m_AudioSourceComboBox.SelectedItem as XmlNode,
                    m_AudioStreamComboBox.SelectedItem as XmlNode,
                    m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode,
                    m_AudioEncodersComboBox.SelectedItem as XmlNode,
                    m_AudioEncodingModeComboBox.SelectedItem as XmlNode,
                    m_AudioCompressedMediaTypesComboBox.SelectedIndex,
                    null,
                    lOutputNodes[lOutputIndex++]);

                if (lSourceNodes != null)
                    lSourceNodes.Add(lSourceNode);
            }

            mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

            if (mISession == null)
                return;

            if (await mISession.startSessionAsync(0, Guid.Empty))
            {
                mState = State.Started;

                m_PauseBtn.IsEnabled = true;

                m_StopBtn.IsEnabled = true;

                m_StartStopBtn.IsEnabled = false;
            }
        }

        private async Task<object> getCompressedMediaType(
            ICaptureProcessor a_ICaptureProcessor,
            int a_StreamIndex,
            int a_MediaTypeIndex,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;

                object lMediaSource = await mSourceControl.createSourceFromCaptureProcessorAsync(a_ICaptureProcessor);

                if (lMediaSource == null)
                    break;

                if (a_StreamIndex < 0)
                    break;

                if (a_MediaTypeIndex < 0)
                    break;

                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;

                uint lStreamIndex = (uint)a_StreamIndex;

                uint lMediaTypeIndex = (uint)a_MediaTypeIndex;

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeFromMediaSourceAsync(
                    lMediaSource,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                lresult = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);

            } while (false);

            return lresult;
        }



        private async Task<object> getCompressedMediaType(
            string a_SymbolicLink,
            int a_StreamIndex,
            int a_MediaTypeIndex,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;

                if (a_StreamIndex < 0)
                    break;

                if (a_MediaTypeIndex < 0)
                    break;

                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;

                uint lStreamIndex = (uint)a_StreamIndex;

                uint lMediaTypeIndex = (uint)a_MediaTypeIndex;

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    a_SymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                lresult = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);

            } while (false);

            return lresult;
        }

        private async Task<object> getCompressedMediaType(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;


                if (aSourceNode == null)
                    break;


                if (aStreamNode == null)
                    break;


                if (aMediaTypeNode == null)
                    break;


                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;



                if (aSourceNode == null)
                    break;

                var lNode = aSourceNode.SelectSingleNode(
            "Source.Attributes/Attribute" +
            "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
            "/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                if (aStreamNode == null)
                    break;

                lNode = aStreamNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lStreamIndex = 0;

                if (!uint.TryParse(lNode.Value, out lStreamIndex))
                {
                    break;
                }

                if (aMediaTypeNode == null)
                    break;

                lNode = aMediaTypeNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    break;
                }

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                lresult = await lEncoderNodeFactory.createCompressedMediaTypeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex);

            } while (false);

            return lresult;
        }

        private async Task<List<object>> getOutputNodes(List<object> aCompressedMediaTypeList)
        {
            List<object> lresult = new List<object>();

            do
            {
                if (aCompressedMediaTypeList == null)
                    break;

                if (aCompressedMediaTypeList.Count == 0)
                    break;

                if (mFileSinkFactory == null)
                    break;

                if (string.IsNullOrEmpty(mFilename))
                    break;

                lresult = await mFileSinkFactory.createOutputNodesAsync(
                    aCompressedMediaTypeList,
                    mFilename);

            } while (false);

            return lresult;
        }

        private async Task<object> getSourceNode(
            string a_SymbolicLink,
            int a_StreamIndex,
            int a_MediaTypeIndex,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex,
            object PreviewRenderNode,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;

                if (a_StreamIndex < 0)
                    break;


                if (a_MediaTypeIndex < 0)
                    break;


                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;

                uint lStreamIndex = (uint)a_StreamIndex;

                uint lMediaTypeIndex = (uint)a_MediaTypeIndex;

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    a_SymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode = await lEncoderNodeFactory.createEncoderNodeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex,
                    aOutputNode);

                if (lEncoderNode == null)
                    break;

                object SpreaderNode = lEncoderNode;

                if (PreviewRenderNode != null)
                {

                    List<object> lOutputNodeList = new List<object>();

                    lOutputNodeList.Add(PreviewRenderNode);

                    lOutputNodeList.Add(lEncoderNode);

                    SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                        lOutputNodeList);

                }

                lresult = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    a_SymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    SpreaderNode);

            } while (false);

            return lresult;
        }


        private async Task<object> getSourceNode(
            XmlNode aSourceNode,
            XmlNode aStreamNode,
            XmlNode aMediaTypeNode,
            XmlNode aEncoderNode,
            XmlNode aEncoderModeNode,
            int aCompressedMediaTypeIndex,
            object PreviewRenderNode,
            object aOutputNode)
        {
            object lresult = null;

            do
            {
                if (aCompressedMediaTypeIndex < 0)
                    break;


                if (aSourceNode == null)
                    break;


                if (aStreamNode == null)
                    break;


                if (aMediaTypeNode == null)
                    break;


                if (aEncoderNode == null)
                    break;


                if (aEncoderModeNode == null)
                    break;

                var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

                if (lEncoderGuidAttr == null)
                    break;

                Guid lCLSIDEncoder;

                if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder))
                    break;

                var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

                if (lEncoderModeGuidAttr == null)
                    break;

                Guid lCLSIDEncoderMode;

                if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode))
                    break;



                if (aSourceNode == null)
                    break;

                var lNode = aSourceNode.SelectSingleNode(
            "Source.Attributes/Attribute" +
            "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
            "/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                if (aStreamNode == null)
                    break;

                lNode = aStreamNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lStreamIndex = 0;

                if (!uint.TryParse(lNode.Value, out lStreamIndex))
                {
                    break;
                }

                if (aMediaTypeNode == null)
                    break;

                lNode = aMediaTypeNode.SelectSingleNode("@Index");

                if (lNode == null)
                    break;

                uint lMediaTypeIndex = 0;

                if (!uint.TryParse(lNode.Value, out lMediaTypeIndex))
                {
                    break;
                }

                object lSourceMediaType = await mSourceControl.getSourceOutputMediaTypeAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex);

                if (lSourceMediaType == null)
                    break;

                var lEncoderNodeFactory = await mEncoderControl.createEncoderNodeFactoryAsync(lCLSIDEncoder);

                if (lEncoderNodeFactory == null)
                    break;

                object lEncoderNode = await lEncoderNodeFactory.createEncoderNodeAsync(
                    lSourceMediaType,
                    lCLSIDEncoderMode,
                    50,
                    (uint)aCompressedMediaTypeIndex,
                    aOutputNode);

                if (lEncoderNode == null)
                    break;

                object SpreaderNode = lEncoderNode;

                if (PreviewRenderNode != null)
                {

                    List<object> lOutputNodeList = new List<object>();

                    lOutputNodeList.Add(PreviewRenderNode);

                    lOutputNodeList.Add(lEncoderNode);

                    SpreaderNode = await mSpreaderNodeFactory.createSpreaderNodeAsync(
                        lOutputNodeList);

                }

                lresult = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lSymbolicLink,
                    lStreamIndex,
                    lMediaTypeIndex,
                    SpreaderNode);

            } while (false);

            return lresult;
        }

        private async void m_PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mState == State.Started && mISession != null)
            {
                m_PauseBtn.IsEnabled = false;

                await mISession.pauseSessionAsync();

                m_StopBtn.IsEnabled = true;

                m_StartStopBtn.IsEnabled = true;

                mState = State.Paused;
            }


        }

        private async void m_StopBtn_Click(object sender, RoutedEventArgs e)
        {
            m_PauseBtn.IsEnabled = false;

            m_StopBtn.IsEnabled = false;

            if (mState != State.Stopped)
            {
                mState = State.Stopped;

                if (mISession == null)
                    return;

                await mISession.stopSessionAsync();

                await mISession.closeSessionAsync();

                mISession = null;

                m_StartStopBtn.IsEnabled = true;
            }
        }


        private void m_ClickTgglBtn_Checked(object sender, RoutedEventArgs e)
        {
            m_VideoSourceMediaTypeComboBox.IsEnabled = false;

            SelectWindow.getInstance().setupMouseHook();

            m_ClickTgglBtn.Content = "Press 'l' for catching";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
