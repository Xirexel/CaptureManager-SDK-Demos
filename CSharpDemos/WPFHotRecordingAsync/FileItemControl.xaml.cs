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

namespace WPFHotRecordingAsync
{
    /// <summary>
    /// Interaction logic for FileItemControl.xaml
    /// </summary>
    public partial class FileItemControl : UserControl
    {
        public FileItemControl()
        {
            InitializeComponent();
        }

        public FileItemControl(string aTitle)
        {
            InitializeComponent();

            startAnimation();

            mTitleBlk.Text = aTitle;
        }

        private void startAnimation()
        {
            var lPlayBrush = Resources["rPlayBrush"] as LinearGradientBrush;

            mStatusTxtBlk.Foreground = lPlayBrush;

            int lCount = 0;

            foreach (var item in lPlayBrush.GradientStops)
            {
                this.RegisterName("GradientStop" + (++lCount).ToString(), item);
            }

            var lPlayAnim = Resources["rAnimatePlayBrush"] as System.Windows.Media.Animation.Storyboard;

            lPlayAnim.Begin(this);
        }

        public void stopAnimation()
        {
            var lPlayAnim = Resources["rAnimatePlayBrush"] as System.Windows.Media.Animation.Storyboard;

            lPlayAnim.Stop(this);

            mStatusTxtBlk.Foreground = Brushes.Gray;
        }
    }
}
