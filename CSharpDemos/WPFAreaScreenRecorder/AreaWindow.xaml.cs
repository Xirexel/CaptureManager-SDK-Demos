using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFAreaScreenRecorder
{
    /// <summary>
    /// Interaction logic for AreaWindow.xaml
    /// </summary>
    public partial class AreaWindow : Window
    {
        private Point startDrag;
        
        private Point startDragRect;

        public static Rect mSelectedRegion = Rect.Empty;

        public AreaWindow()
        {
            InitializeComponent();

            canvas.MouseDown += new MouseButtonEventHandler(canvas_MouseDown);
            canvas.MouseUp += new MouseButtonEventHandler(canvas_MouseUp);
            canvas.MouseMove += new MouseEventHandler(canvas_MouseMove);
            this.Loaded += Window_Loaded;
        }

        /*You can use this event for all the Windows*/
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var senderWindow = sender as Window;
            senderWindow.WindowState = WindowState.Maximized;
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mMoveRectangle)
                return;

            if (e.RightButton == MouseButtonState.Pressed)
                return;

            //Set the start point
            startDrag = e.GetPosition(canvas);
            //Move the selection marquee on top of all other objects in canvas
            Canvas.SetZIndex(rectangle, canvas.Children.Count);
            //Capture the mouse
            if (!canvas.IsMouseCaptured)
                canvas.CaptureMouse();
            canvas.Cursor = Cursors.Cross;
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mMoveRectangle)
                return;
            //Release the mouse
            if (canvas.IsMouseCaptured)
                canvas.ReleaseMouseCapture();
            canvas.Cursor = Cursors.Arrow;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mMoveRectangle)
                return;

            if (canvas.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(canvas);

                //Calculate the top left corner of the rectangle regardless of drag direction
                double x = startDrag.X < currentPoint.X ? startDrag.X : currentPoint.X;
                double y = startDrag.Y < currentPoint.Y ? startDrag.Y : currentPoint.Y;

                if (rectangle.Visibility == Visibility.Hidden)
                    rectangle.Visibility = Visibility.Visible;

                //Move the rectangle to proper place
                rectangle.RenderTransform = new TranslateTransform(x, y);
                //Set its size
                rectangle.Width = Math.Abs(e.GetPosition(canvas).X - startDrag.X);
                rectangle.Height = Math.Abs(e.GetPosition(canvas).Y - startDrag.Y);
            }
        }

        bool mMoveRectangle = false;

        private void rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mMoveRectangle = false;
        }

        private void rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mMoveRectangle = true;

            //Set the start point
            startDragRect = e.GetPosition(canvas);
        }

        private void rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if(mMoveRectangle)
            {
                var lTranslateTransform = rectangle.RenderTransform as TranslateTransform;

                if (lTranslateTransform == null)
                    return;

                Point currentPoint = e.GetPosition(canvas);

                lTranslateTransform.X += (currentPoint.X - startDragRect.X);

                lTranslateTransform.Y += (currentPoint.Y - startDragRect.Y);


                var lRightBotder = lTranslateTransform.X + rectangle.Width;

                if (lRightBotder > canvas.ActualWidth)
                    lTranslateTransform.X = canvas.ActualWidth - rectangle.Width;
                else
                    if (lTranslateTransform.X < 0)
                        lTranslateTransform.X = 0;




                var lBottonBotder = lTranslateTransform.Y + rectangle.Height;

                if (lBottonBotder > canvas.ActualHeight)
                    lTranslateTransform.Y = canvas.ActualHeight - rectangle.Height;
                else
                    if (lTranslateTransform.Y < 0)
                        lTranslateTransform.Y = 0;


                startDragRect = currentPoint;

            }
        }

        private void rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            mMoveRectangle = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            mSelectedRegion = Rect.Empty;

            var lTranslateTransform = rectangle.RenderTransform as TranslateTransform;

            if (lTranslateTransform != null)
            {
                mSelectedRegion = new Rect(lTranslateTransform.X, lTranslateTransform.Y, rectangle.Width, rectangle.Height);
            }

            Close();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            mSelectedRegion = Rect.Empty;

            Close();
        }
    }
}
