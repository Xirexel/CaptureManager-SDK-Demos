using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WPFAreaScreenRecorder
{
    /// <summary>
    /// Interaction logic for HSVPanel.xaml
    /// </summary>
    public partial class HSVPanel : UserControl
    {
        class HSVvalues
        {
            public double alpha;
            public double hue;
            public double saturation;
            public double value;
        }

        private HSVvalues lastValue = null;

        private Color initColor = Color.FromArgb(127, 127, 255, 127);

        private UIElement selectedElement;

        private Point prevPos;

        private Point lastmousePos;

        private bool isChangedByMe = false;

        private bool isNeedModify = false;

        private ColorContainer colorContainer;

        public HSVPanel()
        {
            InitializeComponent();

            this.DataContext = this;
        }



        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }

            set
            {
                initColor = value;

                setColor(value);
            }
        }

        public ColorContainer ColorContainer
        {
            private get { return colorContainer; }

            set
            {
                colorContainer = value;
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(HSVPanel), new PropertyMetadata(Color.FromRgb(255, 255, 255), onValueChanged));

        private static void onValueChanged(DependencyObject obj,
         DependencyPropertyChangedEventArgs e)
        {
            do
            {
                HSVPanel lHSVPanel = obj as HSVPanel;

                if (lHSVPanel == null)
                    break;

                if (!lHSVPanel.IsLoaded)
                    break;

                Color newColor = (Color)e.NewValue;

                if (newColor == null)
                    break;

                if (lHSVPanel.colorContainer != null)
                    lHSVPanel.colorContainer.update(newColor);

                if (lHSVPanel.isChangedByMe)
                {
                    break;
                }

                lHSVPanel.setColor(newColor);

            } while (false);
        }

        Color ColorFromMousePosition(Point mousePos, double radius)
        {
            // Position relative to center of canvas
            double xRel = mousePos.X - radius + 2.5;
            double yRel = mousePos.Y - radius + 3.25;

            // Hue is angle in deg, 0-360
            double angleRadians = Math.Atan2(yRel, xRel);
            double hue = angleRadians * (180 / Math.PI);
            if (hue < 0)
                hue = 360 + hue;

            double value = valueSlider.Value / 100.0;

            // Saturation is distance from center
            double saturation = Math.Min(Math.Sqrt(xRel * xRel + yRel * yRel) / radius, 1.0);

            byte r, g, b;
            ColorUtil.HsvToRgb(hue, saturation, value, out r, out g, out b);
            return Color.FromRgb(r, g, b);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            do
            {

                double minDuametr = hsImage.ActualWidth;

                if (hsImage.ActualWidth > hsImage.ActualHeight)
                {
                    minDuametr = hsImage.ActualHeight;
                }


                double radius = (minDuametr / 2);

                var mousePos = e.GetPosition(hsImage);

                // Position relative to center of canvas
                double xRel = mousePos.X - radius;

                double yRel = mousePos.Y - radius;

                if (Math.Sqrt(xRel * xRel + yRel * yRel) > radius)
                    break;

                lastmousePos = mousePos;

                SetValue(ColorProperty, ColorFromMousePosition(mousePos, radius));

            } while (false);
        }

        private void setColor(Color newColor)
        {

            double hue = 0.0;

            double saturation = 0.0;

            double value = 0.5;
            
            ColorUtil.RgbToHsv(newColor.R, newColor.G, newColor.B, out hue, out saturation, out value);
            
            lastValue = new HSVvalues() { alpha=newColor.A, hue = hue, saturation = saturation, value = value };

            setColorPicker(hue, saturation, value);
        }

        private void setColorPicker(double hue, double saturation, double value)
        {
            do
            {
                UIElement pick;

                if (backCanvas.Children.Count == 0)
                {

                    Ellipse rect = new Ellipse();

                    rect.Fill = Brushes.Black;

                    rect.Width = 5;

                    rect.Height = 5;

                    backCanvas.Children.Add(rect);

                    rect.RenderTransform = new TranslateTransform(-2.0, -2.0);
                }

                pick = backCanvas.Children[0];


                double minDuametr = hsImage.ActualWidth;

                if (hsImage.ActualWidth > hsImage.ActualHeight)
                {
                    minDuametr = hsImage.ActualHeight;
                }

                double radius = (minDuametr / 2);

                if (hue > 180.0)
                {
                    hue = hue - 360.0;
                }

                double angleRadians = hue / (180.0 / Math.PI);

                double ltang = Math.Tan(angleRadians);

                double yRel = Math.Sqrt((saturation * radius) * (saturation * radius) / (1 + ltang * ltang));

                double xRel = yRel * ltang;

                if (hue > -90.0 && hue < 90.0)
                {
                    xRel = -xRel;

                    yRel = -yRel;
                }

                lastValue = new HSVvalues() { alpha=lastValue.alpha, hue = hue, saturation = saturation, value = value };

                Canvas.SetTop(pick, -xRel + radius - 2.0);

                Canvas.SetLeft(pick, -yRel + radius - 1.0);

                if ((value * 100.0) > valueSlider.Minimum)
                    isNeedModify = true;

                valueSlider.Value = value * 100.0;

                valueSlider_ValueChanged(null, null);

                isNeedModify = false;


            } while (false);

        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (initColor != null)
            {

                isChangedByMe = true;

                SetValue(ColorProperty, initColor);

                isChangedByMe = false;

                setColor(initColor);
            }
        }

        private void backCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            do
            {
                if (sender != backCanvas)
                    break;

                if (selectedElement != null)
                    selectedElement = null;

                if (!checkPos(e))
                    break;

                selectedElement = e.Source as UIElement;

                if (selectedElement == null)
                {
                    selectedElement = null;

                    break;
                }

                if (selectedElement == backCanvas)
                {
                    if (backCanvas.Children.Count == 0)
                        break;

                    selectedElement = backCanvas.Children[0];
                }


                Image_MouseMove(sender, e);

                prevPos = e.GetPosition(backCanvas);

                Canvas.SetTop(selectedElement,
                    prevPos.Y);

                Canvas.SetLeft(selectedElement,
                    prevPos.X);


            } while (false);

        }

        private bool checkPos(MouseEventArgs e)
        {
            bool result = false;

            do
            {

                double minDuametr = hsImage.ActualWidth;

                if (hsImage.ActualWidth > hsImage.ActualHeight)
                {
                    minDuametr = hsImage.ActualHeight;
                }


                double radius = (minDuametr / 2);

                var mouseToImagePos = e.GetPosition(hsImage);

                // Position relative to center of canvas
                double xRel = mouseToImagePos.X - radius + 1.0;

                double yRel = mouseToImagePos.Y - radius + 2.0;

                if (Math.Sqrt(xRel * xRel + yRel * yRel) > radius * 0.95)
                {
                    selectedElement = null;

                    break;
                }

                result = true;

            } while (false);

            return result;
        }

        private void backCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            do
            {
                if (selectedElement == null)
                    break;

                var mousePos = e.GetPosition(backCanvas);

                if (!checkPos(e))
                    break;

                Image_MouseMove(sender, e);

                Canvas.SetTop(selectedElement,
                    mousePos.Y - prevPos.Y + Canvas.GetTop(selectedElement));

                Canvas.SetLeft(selectedElement,
                    mousePos.X - prevPos.X + Canvas.GetLeft(selectedElement));

                prevPos = mousePos;

            } while (false);
        }

        private void backCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            do
            {
                if (selectedElement == null)
                    break;

                selectedElement = null;

            } while (false);
        }

        private void valueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            do
            {
                if (lastValue == null)
                    break;

                if (backCanvas.Children.Count == 0)
                {
                    break;
                }

                if (isNeedModify)
                    break;

                UIElement pick = backCanvas.Children[0];

                isChangedByMe = true;

                byte r, g, b;

                ColorUtil.HsvToRgb(lastValue.hue, lastValue.saturation, valueSlider.Value / 100.0, out r, out g, out b);

                SetValue(ColorProperty, Color.FromArgb((byte)lastValue.alpha, r, g, b));

                isChangedByMe = false;


            } while (false);

        }
    }

    public static class ColorUtil
    {
        /// <summary>
        /// Convert HSV to RGB
        /// h is from 0-360
        /// s,v values are 0-1
        /// r,g,b values are 0-255
        /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
        /// </summary>
        public static void HsvToRgb(double h, double S, double V, out byte r, out byte g, out byte b)
        {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // C/C++ Macro HSV to RGB

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((byte)(R * 255.0));
            g = Clamp((byte)(G * 255.0));
            b = Clamp((byte)(B * 255.0));
        }

        public static void RgbToHsv(double r, double g, double b, out double hue, out double saturation, out double value)
        {


            double cHi;

            double cLo;

            if (g.CompareTo(b) >= 0)
            {
                cHi = g;

                cLo = b;
            }
            else
            {
                cHi = b;

                cLo = g;
            }


            if (r.CompareTo(cHi) >= 0)
            {
                cHi = r;
            }


            if (r.CompareTo(cLo) < 0)
            {
                cLo = r;
            }



            double cRng = cHi - cLo;

            value = cHi / 255.0;

            if (cHi > 0)
                saturation = cRng / cHi;
            else
                saturation = 0.0;

            if (cRng > 0)
            {
                double rr = (cHi - r) / cRng;

                double gg = (cHi - g) / cRng;

                double bb = (cHi - b) / cRng;

                double hh;

                if (r == cHi)
                    hh = bb - gg;
                else if (g == cHi)
                    hh = rr - bb + 2.0f;
                else
                    hh = gg - rr + 4.0f;
                if (hh < 0.0f)
                    hh = hh + 6.0f;

                hue = hh / 6.0f;
            }
            else
                hue = 0.0f;

            hue *= 360.0;
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        private static byte Clamp(byte i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }
}
