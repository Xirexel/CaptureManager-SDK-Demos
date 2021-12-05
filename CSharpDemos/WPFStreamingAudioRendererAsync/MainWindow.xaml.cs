/*
MIT License

Copyright(c) 2020 Evgeny Pereguda

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
using System.Xml;

namespace WPFStreamingAudioRendererAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CaptureManager mCaptureManager = null;

        ISessionControlAsync mISessionControl = null;

        ISessionAsync mISession = null;

        ISinkControlAsync mSinkControl = null;

        ISourceControlAsync mSourceControl = null;

        ISARSinkFactoryAsync mSARSinkFactory = null;

        ISARVolumeControlAsync mISARVolumeControl = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_WriteDelegateEvent(string aMessage)
        {
            MessageBox.Show(aMessage);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            mTitleTxtBlk.IsEnabled = false;

            do
            {
                if (mISession != null)
                {
                    await mISession.stopSessionAsync();

                    await mISession.closeSessionAsync();

                    mISession = null;

                    mTitleTxtBlk.Text = "Start playing";

                    break;
                }

                List<object> lSourceNodes = new List<object>();

                object lSARSinkOutputNode = await mSARSinkFactory.createOutputNodeAsync();

                if (lSARSinkOutputNode == null)
                    break;


                var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;

                if (l_AudioSourceXmlNode == null)
                    break;

                var lNode = l_AudioSourceXmlNode.SelectSingleNode(
    "Source.Attributes/Attribute" +
    "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
    "/SingleValue/@Value");

                if (lNode == null)
                    break;

                string lSymbolicLink = lNode.Value;

                object lSourceNode = await mSourceControl.createSourceNodeWithDownStreamConnectionAsync(
                    lSymbolicLink,
                    0,
                    0,
                    lSARSinkOutputNode);

                if (lSourceNode == null)
                    break;

                lSourceNodes.Add(lSourceNode);

                mISession = await mISessionControl.createSessionAsync(lSourceNodes.ToArray());

                if (mISession == null)
                    break;

                if (await mISession.startSessionAsync(0, Guid.Empty))
                {
                    mTitleTxtBlk.Text = "Stop playing";

                    uint lChannelCount = 0;

                    lChannelCount = await mISARVolumeControl.getChannelCountAsync(lSARSinkOutputNode);

                    if (lChannelCount > 0)
                    {
                        float lLevel = await mISARVolumeControl.getChannelVolumeAsync(lSARSinkOutputNode, 0);

                        mRVolume.ValueChanged += async (s, ev) => {
                            await mISARVolumeControl.setChannelVolumeAsync(lSARSinkOutputNode, 0, (float)ev.NewValue);
                        };

                        mRVolume.Value = lLevel;

                        if (lChannelCount > 1)
                        {
                            lLevel = await mISARVolumeControl.getChannelVolumeAsync(lSARSinkOutputNode, 1);

                            mLVolume.ValueChanged += async (s, ev) => {
                                await mISARVolumeControl.setChannelVolumeAsync(lSARSinkOutputNode, 1, (float)ev.NewValue);
                            };

                            mLVolume.Value = lLevel;
                        }
                    }
                }

            } while (false);

            mTitleTxtBlk.IsEnabled = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                mCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (Exception)
            {
                mCaptureManager = new CaptureManager();
            }

            LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

            if (mCaptureManager == null)
                return;

            mSourceControl = await mCaptureManager.createSourceControlAsync();

            if (mSourceControl == null)
                return;

            mSinkControl = await mCaptureManager.createSinkControlAsync();

            if (mSinkControl == null)
                return;

            mISessionControl = await mCaptureManager.createSessionControlAsync();

            if (mISessionControl == null)
                return;

            mISARVolumeControl = await mCaptureManager.createSARVolumeControlAsync();

            if (mISARVolumeControl == null)
                return;



            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlSources"];

            if (lXmlDataProvider == null)
                return;

            XmlDocument doc = new XmlDocument();

            string lxmldoc = await mCaptureManager.getCollectionOfSourcesAsync();

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;



            mSARSinkFactory = await mSinkControl.createSARSinkFactoryAsync(Guid.Empty);

        }
    }
}
