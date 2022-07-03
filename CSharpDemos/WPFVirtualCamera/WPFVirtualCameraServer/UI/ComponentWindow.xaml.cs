using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using WPFVirtualCameraServer.Tools;

namespace WPFVirtualCameraServer.UI
{
    /// <summary>
    /// Interaction logic for ComponentWindow.xaml
    /// </summary>

    [System.Runtime.InteropServices.ComVisible(false)]
    public partial class ComponentWindow : Window
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hwnd, IntPtr hwndNewParent);

        private const int HWND_MESSAGE = -3;

        private IntPtr hwnd;
        private IntPtr oldParent;

        MenuItem m_prevMenuItem = null;


        DispatcherTimer mTimer = new DispatcherTimer();

        Action mSetCallback = null;

        public ComponentWindow()
        {
            InitializeComponent();
        }

        public ComponentWindow(Action aSetCallback)
        {
            mSetCallback = aSetCallback;

            InitializeComponent();

            HideWindow();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SharedTexture.Instance.init(m_VideoPanel.Handle);

            m_PositionFrame.SetPositionEvent += M_PositionFrame_SetPositionEvent;

            CapturePipeline.Instance.RemoveDeviceEvent += Instance_RemoveDeviceEvent;

            CapturePipeline.Instance.Start();

            if (mSetCallback != null)
                mSetCallback();

            mTaskbarIcon.ShowBalloonTip(Title, "Server is launched", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);

            m_PositionFrame.updatePosition();



            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            var doc = new XmlDocument();

            COMUtil.ExeCOMServer.Instance.SetLockQuit(true);

            string lxmldoc = await CapturePipeline.Instance.mCaptureManager.getCollectionOfSourcesAsync();

            COMUtil.ExeCOMServer.Instance.SetLockQuit(false);

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;

            mSourceContextMenu.Loaded += (s, e1) => {

                if (mSourceContextMenu.Tag == null)
                {

                    var l_MenuItem = FindLastChild<MenuItem>(mSourceContextMenu);

                    var l_RadioButton = VisualTreeHelper.GetChild(l_MenuItem, 0) as RadioButton;

                    if (l_RadioButton != null)
                        l_RadioButton.IsChecked = true;

                    mSourceContextMenu.Tag = true;
                }
            };

            mSourceContextMenu.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler((s, e1) =>
            {
                if (mSourceContextMenu.Tag != null && e1.OriginalSource is FrameworkElement)
                {
                    var lFrameworkElement = e1.OriginalSource as FrameworkElement;

                    var lSourceNode = lFrameworkElement.Tag as XmlNode;

                    restartCapture(lSourceNode.Value);
                }
            }
            ));
        }


        public static T FindLastChild<T>(DependencyObject parent) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindLastChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                }
            }

            return foundChild;
        }

        private void Instance_RemoveDeviceEvent()
        {
            if(m_prevMenuItem != null)
                m_prevMenuItem.IsChecked = false;

            m_prevMenuItem = null;
        }

        private void M_PositionFrame_SetPositionEvent(float arg1, float arg2, float arg3, float arg4)
        {
            CapturePipeline.Instance.SetPositionEvent(arg1, arg2, arg3, arg4);
        }

        private void mTaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            OpenWindow();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            HideWindow();
        }

        private void HideWindow()
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwnd = hwndSource.Handle;
                oldParent = SetParent(hwnd, (IntPtr)HWND_MESSAGE);
                Visibility = Visibility.Hidden;
            }
        }

        private void OpenWindow()
        {
            SetParent(hwnd, oldParent);
            Show();
            Activate();
            Height = 450;
            Width = 747;
            WindowStyle = WindowStyle.ToolWindow;

            double height = SystemParameters.WorkArea.Height;
            double width = SystemParameters.WorkArea.Width;
            this.Top = (height - this.Height) / 2;
            this.Left = (width - this.Width) / 2;
        }

        private void restartCapture(string aSymbolicLink)
        {
            if (string.IsNullOrWhiteSpace(aSymbolicLink))
                return;

            CapturePipeline.Instance.Stop();

            Thread.Sleep(500);

            CapturePipeline.Instance.Start(aSymbolicLink);

            m_PositionFrame.updatePosition();
        }
    }
}
