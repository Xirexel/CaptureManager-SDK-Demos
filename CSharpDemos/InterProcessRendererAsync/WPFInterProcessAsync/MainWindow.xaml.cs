using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using InterProcessRendererAsync.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace WPFInterProcessAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        private bool mIsStarted = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void closeOffScreenCapture()
        {
            if (mPipeProcessor != null && mOffScreenCaptureProcess != null)
            {
                mPipeProcessor.send("Close");
            }
        }

        private void mLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (mLaunchButton.Content == "Stop")
            {
                if (mIsStarted)
                {
                    closeOffScreenCapture();

                    HeartBeatTimer.Stop();

                    mLaunchButton.Content = "Launch";
                }

                mIsStarted = false;

                return;
            }

            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null)
                return;

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null)
                return;

            string lSymbolicLink = lNode.Value;

            lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

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

            lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

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

            string lextendSymbolicLink = lSymbolicLink + " --options=" +
                "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Options>" +
                    "<Option Type='Cursor' Visiblity='True'>" +
                        "<Option.Extensions>" +
                            "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
                        "</Option.Extensions>" +
                    "</Option>" +
                "</Options>";


            lextendSymbolicLink += " --normalize=Landscape";

            startOffScreenCapture(lextendSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex);

            mLaunchButton.Content = "Stop";

        }

        private Process mOffScreenCaptureProcess = null;

        private PipeProcessor mPipeProcessor;

        private void startOffScreenCapture(
            string aSymbolicLink,
            uint aStreamIndex,
            uint aMediaTypeIndex)
        {
            mOffScreenCaptureProcess = new Process();

            mOffScreenCaptureProcess.StartInfo.FileName = "InterProcessRendererAsync.exe";

            mOffScreenCaptureProcess.EnableRaisingEvents = true;

            mOffScreenCaptureProcess.Exited += mOffScreenCaptureProcess_Exited;

            mPipeProcessor = new PipeProcessor(
                "Server",
                "Client");

            mPipeProcessor.MessageDelegateEvent += lPipeProcessor_MessageDelegateEvent;

            mOffScreenCaptureProcess.StartInfo.Arguments =
                "SymbolicLink=" + aSymbolicLink + " " +
                "StreamIndex=" + aStreamIndex + " " +
                "MediaTypeIndex=" + aMediaTypeIndex + " " +
                "WindowHandler=" + mVideoPanel.Handle.ToInt64();

            mOffScreenCaptureProcess.StartInfo.UseShellExecute = false;

            try
            {
                HeartBeatTimer.Start();

                mIsStarted = mOffScreenCaptureProcess.Start();
            }
            catch (Exception)
            {
                HeartBeatTimer.Stop();

                mIsStarted = false;
            }
        }

        private System.Timers.Timer _Timer = null;

        private System.Timers.Timer HeartBeatTimer
        {
            get
            {
                if (_Timer == null)
                {
                    _Timer = new System.Timers.Timer(500);
                    _Timer.Elapsed += _Timer_Elapsed;
                }

                return _Timer;
            }
        }

        void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            mPipeProcessor.send("HeartBeat");
        }

        private void lPipeProcessor_MessageDelegateEvent(string aMessage)
        {

            switch (aMessage)
            {
                case "Initilized":
                    mPipeProcessor.send("Start");
                    break;

                default:
                    break;
            }

            if (aMessage.Contains("Get_EVRStreamFilters="))
            {
                Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() => {
                        XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEVRStreamFiltersProvider"];

                        if (lXmlDataProvider == null)
                            return;

                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

                        doc.LoadXml(aMessage.Replace("Get_EVRStreamFilters=", ""));

                        lXmlDataProvider.Document = doc;

                    }));

            }

            if (aMessage.Contains("Get_EVRStreamOutputFeatures="))
            {
                Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEVRStreamOutputFeaturesProvider"];

                        if (lXmlDataProvider == null)
                            return;

                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

                        doc.LoadXml(aMessage.Replace("Get_EVRStreamOutputFeatures=", ""));

                        lXmlDataProvider.Document = doc;

                    }));

            }
        }

        private void mOffScreenCaptureProcess_Exited(object sender, EventArgs e)
        {
            var lProcess = sender as Process;

            if (lProcess != null)
            {
                //lProcess.ExitCode
            }

            mOffScreenCaptureProcess = null;
        }


        private void mEVRStreamFilterSlider_ValueChanged(object sender, RoutedEventArgs e)
        {

            var lslider = e.OriginalSource as Slider;

            if (lslider == null)
                return;

            if (!lslider.IsFocused)
                return;

            var lParametrNode = lslider.Tag as XmlNode;

            if (lParametrNode == null)
                return;

            var lAttr = lParametrNode.Attributes["Index"];

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            int lvalue = (int)lslider.Value;

            mPipeProcessor.send("Set_EVRStreamFilters=" + lindex + "_" + lvalue + "_True");
        }

        private void mEVRStreamFilterSlider_Checked(object sender, RoutedEventArgs e)
        {

            var lCheckBox = e.OriginalSource as CheckBox;

            if (lCheckBox == null)
                return;

            if (!lCheckBox.IsFocused)
                return;

            var lAttr = lCheckBox.Tag as XmlAttribute;

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            mPipeProcessor.send("Set_EVRStreamFilters=" + lindex + "_0_" + (bool)lCheckBox.IsChecked);
        }



        private void mEVRStreamOutputFeaturesSlider_ValueChanged(object sender, RoutedEventArgs e)
        {

            var lslider = e.OriginalSource as Slider;

            if (lslider == null)
                return;

            if (!lslider.IsFocused)
                return;

            var lParametrNode = lslider.Tag as XmlNode;

            if (lParametrNode == null)
                return;

            var lAttr = lParametrNode.Attributes["Index"];

            if (lAttr == null)
                return;

            uint lindex = uint.Parse(lAttr.Value);

            int lvalue = (int)lslider.Value;

            mPipeProcessor.send("Set_EVRStreamOutputFeatures=" + lindex + "_" + lvalue);

        }

        private void ShowEVRStream(object sender, RoutedEventArgs e)
        {
            if (mShowBtn.Content.ToString() == "Show")
            {
                mEVRStreamParametrsTab.IsEnabled = true;

                mShowBtn.Content = "Hide";

                if (!mIsStarted)
                    return;

                mPipeProcessor.send("Get_EVRStreamFilters");

                mPipeProcessor.send("Get_EVRStreamOutputFeatures");                
            }
            else if (mShowBtn.Content.ToString() == "Hide")
            {
                mEVRStreamParametrsTab.IsEnabled = false;

                mShowBtn.Content = "Show";
            }
        }

        private void mShowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mShowBtn.Content.ToString() == "Show")
            {
                Canvas.SetBottom(mWebCamParametrsPanel, 0);

                mShowBtn.Content = "Hide";
            }
            else if (mShowBtn.Content.ToString() == "Hide")
            {
                Canvas.SetBottom(mWebCamParametrsPanel, -150);

                mShowBtn.Content = "Show";
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mIsStarted)
            {
                var ltimer = new DispatcherTimer();

                ltimer.Interval = new TimeSpan(0, 0, 0, 1);

                ltimer.Tick += delegate
                (object sender1, EventArgs e1)
                {
                    if (mLaunchButton.Content == "Stop")
                    {
                        if (mIsStarted)
                        {
                            closeOffScreenCapture();
                        }

                        mIsStarted = false;

                        mLaunchButton.Content = "Launch";
                    }

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

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;



            mEVRStreamFiltersTabItem.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_ValueChanged));

            mEVRStreamFiltersTabItem.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_Checked));

            mEVRStreamFiltersTabItem.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_Checked));

            mEVRStreamOutputFeaturesTabItem.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler(mEVRStreamOutputFeaturesSlider_ValueChanged));
        }
    }
}
