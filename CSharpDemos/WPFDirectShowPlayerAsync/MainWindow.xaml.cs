using DirectShowLib;
using Microsoft.Win32;
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

namespace WPFDirectShowPlayerAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IGraphBuilder m_pGraph = (IGraphBuilder)new FilterGraph();

        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog lopenFileDialog = new OpenFileDialog();

            lopenFileDialog.AddExtension = true;

            var lresult = lopenFileDialog.ShowDialog();

            if (lresult != true)
                return;

            IBaseFilter lDSoundRender = new DSoundRender() as IBaseFilter;

            m_pGraph.AddFilter(lDSoundRender, "Audio Renderer");


            int k = 0;

            IPin[] lAudioRendererPins = new IPin[1];

            IEnumPins ppEnum;

            k = lDSoundRender.EnumPins(out ppEnum);

            k = ppEnum.Next(1, lAudioRendererPins, IntPtr.Zero);

            var lCaptureManagerEVRMultiSinkFactory = await CaptureManagerVideoRendererMultiSinkFactory.getInstance().getICaptureManagerEVRMultiSinkFactoryAsync();

            uint lMaxVideoRenderStreamCount = await lCaptureManagerEVRMultiSinkFactory.getMaxVideoRenderStreamCountAsync();

            if (lMaxVideoRenderStreamCount == 0)
                return;

            List<object> lOutputNodesList = await lCaptureManagerEVRMultiSinkFactory.createOutputNodesAsync(
                IntPtr.Zero,
                mEVRDisplay.Surface.texture,
                1);

            if (lOutputNodesList.Count == 0)
                return;

            IBaseFilter lVideoMixingRenderer9 = (IBaseFilter)lOutputNodesList[0];

            var h = m_pGraph.AddFilter(lVideoMixingRenderer9, "lVideoMixingRenderer9");


            IPin[] lVideoRendererPin = new IPin[1];


            k = lVideoMixingRenderer9.EnumPins(out ppEnum);

            k = ppEnum.Next(1, lVideoRendererPin, IntPtr.Zero);


            IBaseFilter m_SourceFilter = null;

            m_pGraph.AddSourceFilter(lopenFileDialog.FileName, null, out m_SourceFilter);

            IEnumPins lEnumPins = null;

            m_SourceFilter.EnumPins(out lEnumPins);

            IPin[] lPins = new IPin[1];

            while (lEnumPins.Next(1, lPins, IntPtr.Zero) == 0)
            {
                IEnumMediaTypes lIEnumMediaTypes;

                lPins[0].EnumMediaTypes(out lIEnumMediaTypes);

                AMMediaType[] ppMediaTypes = new AMMediaType[1];

                while (lIEnumMediaTypes.Next(1, ppMediaTypes, IntPtr.Zero) == 0)
                {
                    var gh = ppMediaTypes[0].subType;

                    if (ppMediaTypes[0].majorType == DirectShowLib.MediaType.Video)
                    {

                        k = m_pGraph.Connect(lPins[0], lVideoRendererPin[0]);

                    }
                }

                foreach (var item in lPins)
                {
                    k = m_pGraph.Render(item);

                }

            }

            IMediaControl lIMediaControl = m_pGraph as IMediaControl;

            k = lIMediaControl.Run();

        }
    }
}
