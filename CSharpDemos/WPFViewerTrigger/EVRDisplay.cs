using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFViewerTrigger
{
    internal class EVRDisplay : System.Windows.Controls.ContentControl
    {
        private System.Windows.Interop.D3DImage imageSource = null;

        private Interop.Direct3DSurface9 surface = null;

        public Interop.Direct3DSurface9 Surface { get { return surface; } }

        public EVRDisplay()
        {
            var lTuple = D3D9Image.createD3D9Image();

            if(lTuple != null)
            {
                this.imageSource = lTuple.Item1;

                this.surface = lTuple.Item2;
            }
            
            if (this.imageSource != null)
            {
                var image = new System.Windows.Controls.Image();
                image.Stretch = System.Windows.Media.Stretch.Uniform;
                image.Source = this.imageSource;
                this.AddChild(image);

                // To greatly reduce flickering we're only going to AddDirtyRect
                // when WPF is rendering.
                System.Windows.Media.CompositionTarget.Rendering += this.CompositionTargetRendering;
            }
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (this.imageSource != null && this.imageSource.IsFrontBufferAvailable)
            {
                this.imageSource.Lock();
                this.imageSource.AddDirtyRect(new Int32Rect(0, 0, this.imageSource.PixelWidth, this.imageSource.PixelHeight));
                this.imageSource.Unlock();
            }
        }
    }
}
