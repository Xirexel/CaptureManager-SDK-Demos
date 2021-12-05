using MediaFoundation;
using MediaFoundation.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WPFMediaFoundationPlayerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            mCaptureManagerLib.IsChecked = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            MFError throwonhr = MFExtern.MFStartup(0x10070, MFStartup.Full);

            if (throwonhr.Failed())
                return;

           await init();
        }

        private async void Library_Click(object sender, RoutedEventArgs e)
        {
            var lMenuItem = sender as MenuItem;

            if (lMenuItem == null || lMenuItem.Tag == null)
                return;

            var ltag = lMenuItem.Tag.ToString();

            int lIndex = 0;

            if (int.TryParse(ltag, out lIndex))
            {
                CaptureManagerVideoRendererMultiSinkFactory.getInstance().LibraryIndex = lIndex;

                mCaptureManagerLib.IsChecked = false;

                mCaptureManagerVideoRendererLib.IsChecked = false;

                lMenuItem.IsChecked = true;

                await init();
            }
        }

        private async Task init()
        {

            var lCaptureManagerEVRMultiSinkFactory = await CaptureManagerVideoRendererMultiSinkFactory.getInstance().getICaptureManagerEVRMultiSinkFactory();


            uint lMaxVideoRenderStreamCount = await lCaptureManagerEVRMultiSinkFactory.getMaxVideoRenderStreamCountAsync();

            if (lMaxVideoRenderStreamCount == 0)
                return;

            List<object> lOutputNodesList = await lCaptureManagerEVRMultiSinkFactory.createOutputNodesAsync(
                IntPtr.Zero,
                mPlayerControl.Surface.texture,
                lMaxVideoRenderStreamCount);

            if (lOutputNodesList.Count == 0)
                return;

            List<IMFTopologyNode> lEVRList = new List<IMFTopologyNode>();

            foreach (var item in lOutputNodesList)
            {
                var lRenderTopologyNode = (IMFTopologyNode)item;

                if (lRenderTopologyNode != null)
                {
                    lEVRList.Add(lRenderTopologyNode);
                }
            }

            var lIEVRStreamControl = await lCaptureManagerEVRMultiSinkFactory.getIEVRStreamControlAsync();

            mPlayerControl.setRenderList(
                lEVRList,
                lIEVRStreamControl,
                lMaxVideoRenderStreamCount);

        }
    }
}
