using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WPFAreaScreenRecorder
{
    public delegate void ColorUpdateDelegate(Color newColor);

    public class ColorContainer
    {
        public Color color;

        public event ColorUpdateDelegate colorUpdateEvent;

        public void update(Color newColor)
        {
            color = newColor;

            if (colorUpdateEvent != null)
                colorUpdateEvent(newColor);
        }

        public override string ToString()
        {
            return color.ToString();
        }
    }
}
