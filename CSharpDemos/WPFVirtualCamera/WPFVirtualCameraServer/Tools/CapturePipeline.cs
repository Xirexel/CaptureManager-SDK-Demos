using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using WPFVirtualCameraServer.UI.VideoPanel;

namespace WPFVirtualCameraServer.Tools
{
    [ComVisible(false)]
    public class CapturePipeline
    {
        public CaptureManager mCaptureManager = null;

        List<ISession> mISessions = new List<ISession>();

        object mEVROutputNode = null;

        IEVRStreamControl mIEVRStreamControl = null;

        public event Action RemoveDeviceEvent;

        public static CapturePipeline Instance { get; private set; } = new CapturePipeline();

        private CapturePipeline()
        {
            try
            {
                mCaptureManager = new CaptureManager();
            }
            catch (System.Exception)
            {

            }

            if (mCaptureManager == null)
                return;

            TargetTexture.Instance.init(VideoPanel.VideoWidth, VideoPanel.VideoHeight);

            IEVRMultiSinkFactory lSinkFactory;

            var lSinkControl = mCaptureManager.createSinkControl();

            lSinkControl.createSinkFactory(
            Guid.Empty,
            out lSinkFactory);

            List<object> lEVROutputNodes;

            lSinkFactory.createOutputNodes(
                    TargetTexture.Instance.TargetHandler,
                    1,
                    out lEVROutputNodes);

            if (lEVROutputNodes != null && lEVROutputNodes.Count > 0)
                mEVROutputNode = lEVROutputNodes[0];

            mIEVRStreamControl = mCaptureManager.createEVRStreamControl();
        }

        public void Start(string aSymbolicLink = "CaptureManager///Software///Sources///ScreenCapture///ScreenCapture")
        {           

            string lextendSymbolicLink = aSymbolicLink + " --options=" +
                "<?xml version='1.0' encoding='UTF-8'?>" +
                "<Options>" +
                    "<Option Type='Cursor' Visiblity='True'>" +
                        "<Option.Extensions>" +
                            "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" +
                        "</Option.Extensions>" +
                    "</Option>" +
                "</Options>";

            uint lStreamIndex = 0;

            uint lMediaTypeIndex = 4;

            string lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{10E52132-A73F-4A9E-A91B-FE18C91D6837}']");

            if (lSinkNode == null)
                return;

            var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

            if (lContainerNode == null)
                return;

            if (mEVROutputNode == null)
                return;

            var lSourceControl = mCaptureManager.createSourceControl();

            if (lSourceControl == null)
                return;



            object lPtrSourceNode;

            lSourceControl.createSourceNode(
                lextendSymbolicLink,
                lStreamIndex,
                lMediaTypeIndex,
                mEVROutputNode,
                out lPtrSourceNode);


            List<object> lSourceMediaNodeList = new List<object>();

            lSourceMediaNodeList.Add(lPtrSourceNode);

            var lSessionControl = mCaptureManager.createSessionControl();

            if (lSessionControl == null)
                return;

            ISession lISession = lSessionControl.createSession(
                lSourceMediaNodeList.ToArray());

            if (lISession == null)
                return;

            lISession.registerUpdateStateDelegate(UpdateStateDelegate);

            lISession.startSession(0, Guid.Empty);

            mISessions.Add(lISession);

            var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

            if (lEVRStreamControl != null)
            {
                lEVRStreamControl.setPosition(mEVROutputNode,
                    0.0f,
                    0.5f,
                    0.0f,
                    0.5f);
            }
        }

        void UpdateStateDelegate(uint aCallbackEventCode, uint aSessionDescriptor)
        {
            SessionCallbackEventCode k = (SessionCallbackEventCode)aCallbackEventCode;

            switch (k)
            {
                case SessionCallbackEventCode.Unknown:
                    break;
                case SessionCallbackEventCode.Error:
                    break;
                case SessionCallbackEventCode.Status_Error:
                    break;
                case SessionCallbackEventCode.Execution_Error:
                    break;
                case SessionCallbackEventCode.ItIsReadyToStart:
                    break;
                case SessionCallbackEventCode.ItIsStarted:
                    break;
                case SessionCallbackEventCode.ItIsPaused:
                    break;
                case SessionCallbackEventCode.ItIsStopped:
                    break;
                case SessionCallbackEventCode.ItIsEnded:
                    break;
                case SessionCallbackEventCode.ItIsClosed:
                    break;
                case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
                    {
                        System.Windows.Application.Current?.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(() => { 
                            Stop();

                            if (RemoveDeviceEvent != null)
                                RemoveDeviceEvent();
                        }));
                    }
                    break;
                default:
                    break;
            }
        }

        public void Stop()
        {
            foreach (var lISession in mISessions)
            {
                lISession.closeSession();
            }

            mISessions.Clear();
        }

        public void updateData()
        {
            SharedTexture.Instance.Update(TargetTexture.Instance);
        }

        public void SetPositionEvent(float arg1, float arg2, float arg3, float arg4)
        {
            if(mIEVRStreamControl != null && mEVROutputNode != null)
            mIEVRStreamControl.setPosition(
                mEVROutputNode,
                arg1,
                arg2,
                arg3,
                arg4);
        }
    }
}
